using UnityEngine;
using Photon.Pun;

public class GameInitializer : MonoBehaviourPunCallbacks
{
    public enum PlayerSymbol { X, O }

    public static PlayerSymbol LocalPlayerSymbol;

    void Start()
    {
        // Assign X to MasterClient, O to the other player
        LocalPlayerSymbol = PhotonNetwork.IsMasterClient ? PlayerSymbol.X : PlayerSymbol.O;
        Debug.Log("Assigned Player Role: " + LocalPlayerSymbol);
    }
}