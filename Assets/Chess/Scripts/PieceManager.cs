using UnityEngine;
using Photon.Pun;

public class PieceManager : MonoBehaviour
{
    // A singleton instance to allow easy access from other scripts.
    public static PieceManager Instance { get; private set; }

    // These are the prefabs for the white chess pieces, which are assigned in the Unity Editor.
    [Header("White Piece Prefabs")]
    public GameObject whitePawnPrefab;
    public GameObject whiteRookPrefab;
    public GameObject whiteKnightPrefab;
    public GameObject whiteBishopPrefab;
    public GameObject whiteQueenPrefab;
    public GameObject whiteKingPrefab;

    // These are the prefabs for the black chess pieces, also assigned in the Unity Editor.
    public GameObject blackPawnPrefab;
    public GameObject blackRookPrefab;
    public GameObject blackKnightPrefab;
    public GameObject blackBishopPrefab;
    public GameObject blackQueenPrefab;
    public GameObject blackKingPrefab;
    
    // Sets up the singleton instance when the script is loaded.
    private void Awake()
    {
        Instance = this;
    }

    // This runs when the game scene starts.
    private void Start()
    {
        // In a Photon game, only one client should be responsible for setting up the initial state.
        // Here, we designate the Master Client to set up the board. The pieces are created using
        // `PhotonNetwork.Instantiate`, which ensures they appear for all players in the room.
        if (PhotonNetwork.IsMasterClient)
        {
            SetupBoard();
        }
    }

    // This function clears all pieces from the board. It's useful for resetting the game.
    private void ClearBoard()
    {
        var controller = ChessGameController.Instance;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (controller.piecePositions[row, col] != null)
                {
                    // For objects instantiated over the network, we must use `PhotonNetwork.Destroy` to remove them correctly on all clients.
                    PhotonNetwork.Destroy(controller.piecePositions[row, col].gameObject);
                    controller.piecePositions[row, col] = null;
                }
            }
        }
        ChessBoardPlacementHandler.Instance.ClearHighlights();
    }

    // This is the main function to set up the board for a new game.
    public void SetupBoard()
    {
        ClearBoard();
        SetupStandard();
    }

    // This function places all the pieces in their standard starting positions.
    private void SetupStandard()
    {
        // --- Create White Pieces ---
        // Pawns
        for (int i = 0; i < 8; i++)
        {
            CreatePiece(whitePawnPrefab, 1, i, true);
        }

        // Back Rank
        CreatePiece(whiteRookPrefab, 0, 0, true);
        CreatePiece(whiteKnightPrefab, 0, 1, true);
        CreatePiece(whiteBishopPrefab, 0, 2, true);
        CreatePiece(whiteQueenPrefab, 0, 3, true);
        CreatePiece(whiteKingPrefab, 0, 4, true);
        CreatePiece(whiteBishopPrefab, 0, 5, true);
        CreatePiece(whiteKnightPrefab, 0, 6, true);
        CreatePiece(whiteRookPrefab, 0, 7, true);

        // --- Create Black Pieces ---
        // Pawns
        for (int i = 0; i < 8; i++)
        {
            CreatePiece(blackPawnPrefab, 6, i, false);
        }

        // Back Rank
        CreatePiece(blackRookPrefab, 7, 0, false);
        CreatePiece(blackKnightPrefab, 7, 1, false);
        CreatePiece(blackBishopPrefab, 7, 2, false);
        CreatePiece(blackQueenPrefab, 7, 3, false);
        CreatePiece(blackKingPrefab, 7, 4, false);
        CreatePiece(blackBishopPrefab, 7, 5, false);
        CreatePiece(blackKnightPrefab, 7, 6, false);
        CreatePiece(blackRookPrefab, 7, 7, false);
    }

    // This helper function creates a single piece on the board.
    private void CreatePiece(GameObject prefab, int row, int col, bool isWhite)
    {
        if (prefab == null)
        {
            Debug.LogError($"Prefab for piece at ({row}, {col}) is not assigned in PieceManager. Aborting piece creation.");
            return;
        }

        // We package up the initial data for the piece (color, row, and column) to be sent over the network.
        // This data is received by the `OnPhotonInstantiate` method in the `ChessPiece` script on all clients.
        object[] instantiationData = { isWhite, row, col };

        var tile = ChessBoardPlacementHandler.Instance.GetTile(row, col);
        if (tile == null)
        {
            Debug.LogError($"Could not find tile at ({row}, {col}) to create a piece. Aborting piece creation for prefab '{prefab.name}'.");
            return;
        }

        // `PhotonNetwork.Instantiate` is the key Photon function for creating networked objects.
        // It takes the prefab name (which must be in a "Resources" folder), position, rotation, and the instantiation data.
        // This ensures the piece is created on all connected clients.
        PhotonNetwork.Instantiate(prefab.name, tile.transform.position, Quaternion.identity, 0, instantiationData);

        // The rest of the piece's setup, like placing it on the board and adding it to the `piecePositions` array,
        // is handled within the `ChessPiece.OnPhotonInstantiate` method, which is executed on every client.
    }
}