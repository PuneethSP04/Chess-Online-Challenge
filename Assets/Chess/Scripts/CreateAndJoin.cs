using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// This script handles the player's interaction with the UI for creating and joining rooms.
public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    // Input field for the player to type the name of the room they want to create.
    public TMP_InputField input_Create;
    // Input field for the player to type the name of the room they want to join.
    public TMP_InputField input_Join;

    // This function is called when the "Create Room" button is clicked.
    public void CreateRoom()
    {
        // We check if the player has entered a room name.
        if (string.IsNullOrEmpty(input_Create.text))
        {
            Debug.LogWarning("Room name for creation is empty. Cannot create room.");
            // It's a good idea to also show a message to the user in the UI here.
            return;
        }
        // We set the room options, like the maximum number of players. For chess, it's 2.
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        // We call Photon to create a new room with the given name and options.
        PhotonNetwork.CreateRoom(input_Create.text, roomOptions);
    }

    // This function is called when the "Join Room" button is clicked.
    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(input_Join.text))
        {
            Debug.LogWarning("Room name for joining is empty. Cannot join room.");
            return;
        }
        // We call Photon to join the room with the specified name.
        PhotonNetwork.JoinRoom(input_Join.text);
    }

    // This function is called from the `Room` script when a player clicks on a room in the list.
    public void JoinRoomInList(string RoomName)
    {
        PhotonNetwork.JoinRoom(RoomName);
    }

    // This Photon callback is triggered when the local player successfully joins a room.
    public override void OnJoinedRoom()
    {
        print(PhotonNetwork.CountOfPlayersInRooms);
        // Once in a room, we load the main "Chess" game scene.
        PhotonNetwork.LoadLevel("Chess");
    }
}
