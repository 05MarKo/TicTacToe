using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    private PhotonView photonView;

    public Sprite xSprite;
    public Sprite oSprite;

    public GameObject xPlayerRoot;
    public GameObject oPlayerRoot;

    public TextMeshProUGUI xScoreText;
    public TextMeshProUGUI oScoreText;

    public GameObject restartButton;
    public CanvasGroup restartButtonGroup;
    public GameObject mainMenuButton;
    public CanvasGroup mainMenuButtonGroup;

    public GameObject[] buttons;
    private bool isXTurn = true;
    private bool gameEnded = false;

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

        isXTurn = true;
        gameEnded = false;
        UpdateTurnVisuals();
        UIFader.Instance.FadeCanvasGroup(restartButtonGroup, 0f, 0.2f, false);
        UIFader.Instance.FadeCanvasGroup(mainMenuButtonGroup, 0f, 0.2f, false);
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
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_RestartGame", RpcTarget.AllBuffered);
        }
    }

    public void GoToMainMenu()
    {
        PhotonNetwork.LeaveRoom(); // optional: leave Photon room if using networking
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    [PunRPC]
    void RPC_RestartGame()
    {
        ResetBoardState();
    }
}
