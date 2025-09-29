using UnityEngine;
using TMPro;

// This script is attached to the UI prefab that represents a single room in the lobby's room list.
public class Room : MonoBehaviour
{
    // A reference to the TextMeshPro UI element that displays the room's name.
    public TextMeshProUGUI Name;

    // This function is called when the player clicks on this room's UI element in the lobby.
    public void JoinRoom()
    {
        // It finds the "CreateAndJoin" manager in the scene and calls its `JoinRoomInList` method, passing the room's name.
        GameObject.Find("CreateAndJoin").GetComponent<CreateAndJoin>().JoinRoomInList(Name.text);
    }
}
