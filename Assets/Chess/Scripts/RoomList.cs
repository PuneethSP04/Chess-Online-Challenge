using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UIElements;

// This script manages the display of available rooms in the lobby.
public class RoomList : MonoBehaviourPunCallbacks
{
    // A prefab for the UI element that represents a single room in the list.
    public GameObject RoomPrefab;
    // An array to keep track of the currently displayed room UI objects.
    public GameObject[] AllRooms;

    // This Photon callback is automatically called whenever the list of rooms on the server is updated.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // First, we clear the existing list of rooms to avoid duplicates.
        for (int i = 0; i < AllRooms.Length; i++)
        {
            if (AllRooms[i] != null)
            {
                Destroy(AllRooms[i]);
            }
        }
        // We resize our tracking array to match the new number of rooms.
        AllRooms = new GameObject[roomList.Count];

        // Now, we iterate through the updated list of rooms from Photon.
        for (int i = 0; i < roomList.Count; i++)
        {
            print(roomList[i].Name);

            // We only want to display rooms that are open, visible, and have at least one player.
            if (roomList[i].IsOpen && roomList[i].IsVisible && roomList[i].PlayerCount >= 1)
            {
                // For each valid room, we instantiate our RoomPrefab and parent it to the "Content" object in our scroll view.
                GameObject Room = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform);
                // We then set the name of the room on the UI element.
                Room.GetComponent<Room>().Name.text = roomList[i].Name;

                AllRooms[i] = Room;
            }
            
        }
    }
}
