using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameController : MonoBehaviourPunCallbacks
{
    private PhotonView photonView;

    [Header("Player UI References")]
    public GameObject xPlayerRoot;
    public GameObject oPlayerRoot;

    [Header("Score UI")]
    public TextMeshProUGUI xScoreText;
    public TextMeshProUGUI oScoreText;

    [Header("UI Buttons")]
    public GameObject restartButton;
    public CanvasGroup restartButtonGroup;
    public GameObject mainMenuButton;
    public CanvasGroup mainMenuButtonGroup;

    [Header("Gameplay")]
    public Sprite xSprite;
    public Sprite oSprite;
    public Sprite sushiXSprite;
    public Sprite sushiOSprite;
    public Sprite cupcakeXSprite;
    public Sprite cupcakeOSprite;
    public GameObject[] buttons;

    public TextMeshProUGUI playerLeftText;

    private bool isXTurn = true;
    private bool gameEnded = false;
    private bool isXStarting = true;
    private bool localPlayerVotedRestart = false;
    private bool remotePlayerVotedRestart = false;

    private int xScore = 0;
    private int oScore = 0;


    private enum CellState { Empty, X, O }
    private CellState[] board = new CellState[9];

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        UpdateTurnVisuals();
        ResetBoardState();
        restartButtonGroup.alpha = 0f;
        restartButtonGroup.interactable = false;
        restartButtonGroup.blocksRaycasts = false;

        if (ThemeManager.Instance != null)
        {
            var selectedTheme = ThemeManager.Instance.GetCurrentTheme();

            if (selectedTheme == ThemeManager.ThemeType.Sushi)
            {
                xSprite = sushiXSprite; // Add a serialized field for this
                oSprite = sushiOSprite;
            }
            else
            {
                xSprite = cupcakeXSprite;
                oSprite = cupcakeOSprite;
            }
        }
    }

    public void OnCellClicked(GameObject buttonObj)
    {
        if (gameEnded) return;

        // Allow only the correct player to click
        bool isMyTurn = (isXTurn && GameInitializer.LocalPlayerSymbol == GameInitializer.PlayerSymbol.X) ||
                        (!isXTurn && GameInitializer.LocalPlayerSymbol == GameInitializer.PlayerSymbol.O);

        if (!isMyTurn) return;

        int index = GetButtonIndex(buttonObj);
        if (index == -1 || board[index] != CellState.Empty) return;

        // Broadcast move to all clients
        photonView.RPC("RPC_MakeMove", RpcTarget.AllBuffered, index, isXTurn ? (int)CellState.X : (int)CellState.O);
    }

    [PunRPC]
    void RPC_MakeMove(int index, int symbol)
    {
        if (board[index] != CellState.Empty || gameEnded) return;

        CellState playerSymbol = (CellState)symbol;
        board[index] = playerSymbol;

        Transform markTransform = buttons[index].transform.Find("MarkImage");
        if (markTransform == null) return;

        Image markImage = markTransform.GetComponent<Image>();
        markImage.sprite = playerSymbol == CellState.X ? xSprite : oSprite;
        markImage.gameObject.SetActive(true);

        if (CheckWin(playerSymbol))
        {
            gameEnded = true;

            if (playerSymbol == CellState.X)
            {
                xScore++;
                xScoreText.text = xScore.ToString();
            }
            else
            {
                oScore++;
                oScoreText.text = oScore.ToString();
            }

            UIFader.Instance.FadeCanvasGroup(restartButtonGroup, 1f, 0.5f);
            UIFader.Instance.FadeCanvasGroup(mainMenuButtonGroup, 1f, 0.5f);
            return;
        }

        bool isBoardFull = true;
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == CellState.Empty)
            {
                isBoardFull = false;
                break;
            }
        }

        if (isBoardFull)
        {
            gameEnded = true;
            UIFader.Instance.FadeCanvasGroup(restartButtonGroup, 1f, 0.5f);
            UIFader.Instance.FadeCanvasGroup(mainMenuButtonGroup, 1f, 0.5f);
            return;
        }

        isXTurn = !isXTurn;
        UpdateTurnVisuals();
    }

    void UpdateTurnVisuals()
    {
        SetAlpha(xPlayerRoot, isXTurn ? 1f : 0.2f);
        SetAlpha(oPlayerRoot, isXTurn ? 0.2f : 1f);
    }

    void SetAlpha(GameObject root, float alpha)
    {
        Image icon = root.GetComponentInChildren<Image>();
        TextMeshProUGUI tmp = root.GetComponentInChildren<TextMeshProUGUI>();

        if (icon != null)
            UIFader.Instance.FadeImage(icon, alpha, 0.4f);

        if (tmp != null)
            UIFader.Instance.FadeText(tmp, alpha, 0.4f);
    }

    void ResetBoardState()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = CellState.Empty;

            Transform markTransform = buttons[i].transform.Find("MarkImage");
            if (markTransform != null)
            {
                markTransform.gameObject.SetActive(false);
            }
        }

        isXTurn = isXStarting;
        gameEnded = false;
        UpdateTurnVisuals();
        UIFader.Instance.FadeCanvasGroup(restartButtonGroup, 0f, 0.2f, false);
        UIFader.Instance.FadeCanvasGroup(mainMenuButtonGroup, 0f, 0.2f, false);

        if (playerLeftText != null)
            playerLeftText.gameObject.SetActive(false);
    }

    int GetButtonIndex(GameObject buttonObj)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == buttonObj)
                return i;
        }
        return -1;
    }

    bool CheckWin(CellState player)
    {
        int[,] winConditions = new int[,]
        {
            {0,1,2}, {3,4,5}, {6,7,8},
            {0,3,6}, {1,4,7}, {2,5,8},
            {0,4,8}, {2,4,6}
        };

        for (int i = 0; i < 8; i++)
        {
            int a = winConditions[i, 0];
            int b = winConditions[i, 1];
            int c = winConditions[i, 2];

            if (board[a] == player && board[b] == player && board[c] == player)
                return true;
        }

        return false;
    }

    public void RestartGame()
    {
        if (!localPlayerVotedRestart)
        {
            localPlayerVotedRestart = true;

            // Inform the other player that this one is ready
            photonView.RPC("RPC_PlayerVotedRestart", RpcTarget.Others);

            CheckRestartReadiness();
        }
    }

    public void GoToMainMenu()
    {
        PhotonNetwork.LeaveRoom(); // optional: leave Photon room if using networking
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    void CheckRestartReadiness()
    {
        if (localPlayerVotedRestart && remotePlayerVotedRestart)
        {
            // Only the MasterClient actually sends the restart signal
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_RestartGame", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    void RPC_PlayerVotedRestart()
    {
        remotePlayerVotedRestart = true;
        CheckRestartReadiness();
    }

    [PunRPC]
    void RPC_RestartGame()
    {
        isXStarting = !isXStarting;
        localPlayerVotedRestart = false;
        remotePlayerVotedRestart = false;
        ResetBoardState();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ShowPlayerLeftMessage();
    }
    void ShowPlayerLeftMessage()
    {
        if (playerLeftText != null)
        {
            playerLeftText.gameObject.SetActive(true);
            playerLeftText.text = "The other player left the game.";
        }

        // Show main menu button
        UIFader.Instance.FadeCanvasGroup(mainMenuButtonGroup, 1f, 0.5f);

        // Hide restart button
        UIFader.Instance.FadeCanvasGroup(restartButtonGroup, 0f, 0.2f, false);

        // End the game to block further interaction
        gameEnded = true;
    }
}
