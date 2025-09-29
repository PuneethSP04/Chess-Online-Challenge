using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// This script manages game-wide scene transitions and UI panels, like confirming to exit a match.
public class GameSceneManager : MonoBehaviour
{
    // A singleton instance for easy access from other parts of the game.
    public static GameSceneManager Instance { get; private set; }

    // This is the UI panel that pops up to ask the player if they're sure they want to leave the game.
    public GameObject confirmExitPanel;

    private void Awake()
    {
        // We're using a singleton pattern here to ensure there's only one GameSceneManager.
        // If another one already exists, this new one destroys itself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // At the start of the game scene, we make sure the exit confirmation panel is hidden.
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }

    // This function is called when the player clicks the 'Home' or 'Exit' button during a game.
    public void OnHomeButtonClicked()
    {
        // Instead of leaving the game immediately, we show the confirmation panel to prevent accidental exits.
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Confirm Exit Panel is not assigned in the GameSceneManager inspector.");
        }
    }

    // This is called when the player confirms they want to exit by clicking 'Yes' on the panel.
    public void ConfirmExitGame()
    {
        if (PhotonNetwork.InRoom)
        {
            // If the player is in a Photon room, we call `LeaveRoom`. This triggers the `OnLeftRoom` callback for this client
            // and the `OnPlayerLeftRoom` callback for the opponent, allowing for graceful disconnection.
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            // If for some reason the player is not in a room, we just load the Lobby scene directly.
            SceneManager.LoadScene("Lobby");
        }
    }

    // This is called if the player decides not to exit, by clicking 'No' or 'Cancel' on the panel.
    public void CancelExitGame()
    {
        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }

    // This function is used for post-game scenarios (like after a checkmate or opponent disconnect).
    // It returns the player to the lobby without needing a confirmation popup.
    public void ReturnToHome()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}
