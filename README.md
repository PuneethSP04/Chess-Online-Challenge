# Chess Online Challenge

This is a multiplayer online chess game built with the Unity Engine and Photon PUN 2. It allows two players to connect over the internet and play a full game of chess.

## Features

*   **Real-time Multiplayer**: Play against another person online using Photon PUN 2 for networking.
*   **Lobby System**: Browse, create, and join game rooms.
*   **Complete Chess Logic**: Implements all standard chess rules, including:
    *   Piece movements (Pawn, Rook, Knight, Bishop, Queen, King).
    *   Capturing pieces.
    *   Pawn promotion.
    *   Check and Checkmate detection.
*   **Interactive UI**:
    *   Visual feedback for valid moves, selected pieces, and check status.
    *   Displays whose turn it is.
    *   Tracks and displays captured pieces for both players.
    *   Game over screens for win, lose, and opponent disconnect scenarios.

## Built With

*   **Unity Engine**: `6000.0.57f1`
*   **Photon PUN 2**: For networking and multiplayer functionality.

## Getting Started

To get a local copy up and running, follow these simple steps.

### Prerequisites

*   **Unity Hub**
*   **Unity Editor version `6000.0.57f1`** (or a compatible version).
*   A **Photon Engine AppId**. You can get one for free from the Photon Engine website.

### Installation

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/PuneethSP04/Chess-Online-Challenge.git
    ```
2.  **Open the project in Unity Hub:**
    *   Open Unity Hub.
    *   Click "Add" and navigate to the cloned project's root folder (`.../ChessTemplate/`).
    *   Open the project with the correct Unity Editor version.
3.  **Configure Photon:**
    *   Once the project is open, find the `PhotonServerSettings` asset in `Assets/Photon/PhotonUnityNetworking/Resources/`.
    *   In the Inspector, enter your Photon AppId in the "App Id Realtime" field.

## How to Play

1.  **Build and Run**: Build the project for your desired platform (e.g., Windows, WebGL) or run it directly in the Unity Editor.
2.  **Connect**: The game will automatically connect to the Photon master server.
3.  **Lobby**:
    *   In the lobby, you can create a new room by entering a room name and clicking "Create Room".
    *   Alternatively, you can join an existing room from the list.
4.  **Gameplay**:
    *   The game starts once two players are in the same room.
    *   The player who creates the room (the Master Client) will play as **White**. The second player will play as **Black**.
    *   Click on a piece to see its possible moves, then click on a highlighted tile to move it.
    *   The game ends when one player checkmates the other, or if a player disconnects.