using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

// This script manages the initial connection to the Photon server infrastructure.
public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // The `Start` method is called when the script is first enabled.
    void Start()
    {
        // This line initiates the connection to the Photon Master Server using the settings defined in the PhotonServerSettings file.
        PhotonNetwork.ConnectUsingSettings();
    }

    // This callback is triggered by Photon once the client has successfully connected to the Master Server.
    public override void OnConnectedToMaster()
    {
        // After connecting to the Master Server, we join a lobby. Lobbies are used for matchmaking and listing available rooms.
        PhotonNetwork.JoinLobby();
    }

    // This callback is triggered after successfully joining a lobby.
    public override void OnJoinedLobby()
    {
        // Once in the lobby, we load the "Lobby" scene where the player can see a list of rooms, create a new one, or join an existing game.
        SceneManager.LoadScene("Lobby");
    }
}
