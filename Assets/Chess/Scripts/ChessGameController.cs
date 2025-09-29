using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// This is the main controller for the chess game. It manages game state, player turns, piece movements, and UI updates.
public class ChessGameController : MonoBehaviourPunCallbacks
{
    // A singleton instance, making it the central point of control and easily accessible from other scripts.
    public static ChessGameController Instance { get; private set; }

    // A boolean to keep track of whose turn it is. `true` for White, `false` for Black.
    public bool isWhiteTurn = true;

    [Header("Game Over UI")]
    // UI elements for displaying game over states like checkmate or when a player leaves.
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Game State UI")]
    // UI elements related to the current game state.
    public GameObject checkHighlightPrefab;
    public TextMeshProUGUI turnStatusText;
    // A reference to the highlight object shown when a king is in check.
    private GameObject _activeCheckHighlight;

    [Header("Player Color Info UI")]
    // UI elements to inform the player which color they are playing as.
    public GameObject playerColorInfoPanel;
    public TextMeshProUGUI playerColorInfoText;

    [Header("Captured Pieces UI")]
    // UI Text elements to display the count of captured black pieces.
    public TextMeshProUGUI capturedBlackPawnText;
    public TextMeshProUGUI capturedBlackRookText;
    public TextMeshProUGUI capturedBlackKnightText;
    public TextMeshProUGUI capturedBlackBishopText;
    public TextMeshProUGUI capturedBlackQueenText;

    // A spacer for better organization in the Unity Inspector.
    [Space]
    // UI Text elements to display the count of captured white pieces.
    public TextMeshProUGUI capturedWhitePawnText;
    public TextMeshProUGUI capturedWhiteRookText;
    public TextMeshProUGUI capturedWhiteKnightText;
    public TextMeshProUGUI capturedWhiteBishopText;
    public TextMeshProUGUI capturedWhiteQueenText;

    // Dictionaries to keep track of the count of captured pieces for each color and type.
    private Dictionary<System.Type, int> _capturedWhitePiecesCount = new Dictionary<System.Type, int>();
    private Dictionary<System.Type, int> _capturedBlackPiecesCount = new Dictionary<System.Type, int>();

    // This holds a reference to the piece that the player has currently selected.
    private ChessPiece _selectedPiece;

    // This 2D array is the logical representation of the chessboard, storing a reference to the `ChessPiece` at each position.
    public ChessPiece[,] piecePositions = new ChessPiece[8, 8];
    // A flag to indicate if the game has ended.
    private bool isGameOver = false;

    // Sets up the singleton instance when the script is loaded.
    private void Awake()
    {
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        // This game is designed for multiplayer, so we must be in a Photon room to play.
        if (!PhotonNetwork.InRoom)
        {
            // If we're not in a room, we return to the lobby. This is a safety measure.
            Debug.Log("Not in a room, returning to Lobby.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }

    private void Start()
    {
        // At the start of the game, we hide all the conditional UI panels.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (playerColorInfoPanel != null) playerColorInfoPanel.SetActive(false);

        // The game setup only proceeds if the client is successfully in a Photon room.
        if (PhotonNetwork.InRoom)
        {
            InitializeCapturedPieceCounters();
            UpdateTurnStatusText();
            ShowPlayerColorInfo();
        }
        else
        {
            Debug.LogWarning("ChessGameController.Start() - Not in a room, game setup is paused.");
        }
    }

    // This method is called from the `ChessPiece` script when a piece is clicked.
    public void OnPieceSelected(ChessPiece piece)
    {
        if (isGameOver) return;

        // In this game, the Master Client always plays as White. This line determines the local player's color.
        bool isPlayerWhite = PhotonNetwork.IsMasterClient;

        // We check if it's the correct color's turn to move.
        if (piece.isWhite != isWhiteTurn)
        {
            return;
        }

        // We check if the selected piece belongs to the current player.
        if (piece.isWhite != isPlayerWhite)
        {
            return;
        }

        // Before showing new highlights, we clear any existing ones from the board.
        ChessBoardPlacementHandler.Instance.ClearHighlights();

        // We store a reference to the selected piece.
        _selectedPiece = piece;

        // Highlight the selected piece's tile.
        ChessBoardPlacementHandler.Instance.HighlightSelected(piece.row, piece.column);

        // Ask the piece where it can go. Each piece knows its own rules!
        List<Vector2Int> possibleMoves = _selectedPiece.GetPossibleMoves();

        // Now, let's light up all the possible spots.
        foreach (var move in possibleMoves)
        {
            // We check if there's another piece on the potential move's tile.
            if (piecePositions[move.x, move.y] != null)
            {
                // If it's an enemy piece, we highlight it with the "enemy" marker (usually red).
                if (piecePositions[move.x, move.y].isWhite != _selectedPiece.isWhite)
                {
                    ChessBoardPlacementHandler.Instance.HighlightEnemy(move.x, move.y);
                }
            }
            else // If the spot is empty, we'll just mark it green for a normal move.
            {
                ChessBoardPlacementHandler.Instance.Highlight(move.x, move.y);
            }
        }
    }

    // This method is called from the `Tile` script when a board tile is clicked.
    public void OnTileClicked(int row, int col)
    {
        if (isGameOver) return;

        if (_selectedPiece != null)
        {
            // If a piece is currently selected, this click is interpreted as a move attempt.
            Vector2Int targetMove = new Vector2Int(row, col);
            if (_selectedPiece.GetPossibleMoves().Contains(targetMove))
            {
                // If the clicked tile is a valid move, we use an RPC to execute the move on all clients' machines.
                photonView.RPC("MovePieceRPC", RpcTarget.All, _selectedPiece.row, _selectedPiece.column, row, col);
            }

            // After the click, regardless of whether it was a valid move, we clear the selection and highlights.
            ChessBoardPlacementHandler.Instance.ClearHighlights();
            _selectedPiece = null;
        }
    }

    [PunRPC]
    private void MovePieceRPC(int fromRow, int fromCol, int toRow, int toCol, PhotonMessageInfo info)
    {
        ChessPiece pieceToMove = piecePositions[fromRow, fromCol];
        if (pieceToMove == null) return;

        // If there's a piece at the destination, it's a capture.
        ChessPiece pieceAtTarget = piecePositions[toRow, toCol];
        if (pieceAtTarget != null)
        {
            // We identify the type of the captured piece to update the UI counter.
            System.Type pieceType = pieceAtTarget.GetType();

            if (pieceType != typeof(King))
            {
                if (pieceAtTarget.isWhite)
                {
                    if (_capturedWhitePiecesCount.ContainsKey(pieceType))
                    {
                        _capturedWhitePiecesCount[pieceType]++;
                    }
                }
                else
                {
                    if (_capturedBlackPiecesCount.ContainsKey(pieceType))
                    {
                        _capturedBlackPiecesCount[pieceType]++;
                    }
                }
            }

            UpdateCapturedPiecesUI();

            // To maintain consistency, only the Master Client is allowed to destroy networked objects.
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(pieceAtTarget.gameObject);
            }
        }

        // We update the logical board state by moving the piece to its new position.
        piecePositions[fromRow, fromCol] = null;
        pieceToMove.PlaceOnBoard(toRow, toCol);
        piecePositions[toRow, toCol] = pieceToMove;
        pieceToMove.hasMoved = true;

        // After the move, we check if a pawn has reached the opposite end of the board for promotion.
        if (pieceToMove is Pawn)
        {
            bool isWhitePawn = pieceToMove.isWhite;
            if ((isWhitePawn && toRow == 7) || (!isWhitePawn && toRow == 0))
            {
                // If promotion conditions are met, we show the promotion dialog only for the player who made the move.
                // The game pauses here, waiting for the player's choice.
                if (info.Sender.IsLocal)
                {
                    PawnPromotionHandler.Instance.ShowPromotionDialog(pieceToMove as Pawn);
                }
                // Wait for the player to choose a piece. Do not continue the game yet.
                return;
            }
        }

        // If there's no promotion, we proceed with the rest of the game turn.
        ContinueGame();
    }

    // This method handles the logic to continue the game after a move is completed (or after a promotion is chosen).
    public void ContinueGame()
    {
        // We clear any highlight that might have been on the king from a previous check.
        if (_activeCheckHighlight != null)
        {
            Destroy(_activeCheckHighlight);
            _activeCheckHighlight = null;
        }

        // It's the next player's turn.
        isWhiteTurn = !isWhiteTurn;
        UpdateTurnStatusText();

        // We check if the current player's king is in check.
        if (IsInCheck(isWhiteTurn))
        {
            // If it is, we highlight the king to alert the player.
            HighlightKingInCheck();

            if (IsCheckmate(isWhiteTurn))
            {
                string winner = isWhiteTurn ? "Black" : "White";
                Debug.Log($"Checkmate! {winner} wins.");
                // If it's also checkmate, the game ends. The player whose turn it is has lost.
                EndGame(isWhiteTurn);
            }
            else
            {
                Debug.Log(isWhiteTurn ? "White is in check." : "Black is in check.");
            }
        }
    }

    // This function is called when the game ends due to checkmate.
    private void EndGame(bool losingPlayerIsWhite)
    {
        isGameOver = true;
        bool localPlayerIsWhite = PhotonNetwork.IsMasterClient;
        bool localPlayerLoses = (losingPlayerIsWhite == localPlayerIsWhite);

        if (localPlayerLoses)
        {
            if (losePanel != null) losePanel.SetActive(true);
            Debug.Log("Checkmate! You lose.");
        }
        else
        {
            if (winPanel != null) winPanel.SetActive(true);
            Debug.Log("Checkmate! You win.");
        }

        // The `isGameOver` flag will now prevent any further moves.
    }

    // This Photon callback is triggered on the client that is leaving the room.
    public override void OnLeftRoom()
    {
        // We simply load the "Lobby" scene to return to the main menu.
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

    // This Photon callback is triggered for the remaining player when their opponent leaves the room.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        isGameOver = true;
        // The remaining player is declared the winner.
        Debug.Log($"Player {otherPlayer.NickName} has left the room. You win!");
        if (gameOverPanel != null && gameOverText != null)
        {
            gameOverText.text = "You Win!\nOpponent Left The Game.";
            gameOverPanel.SetActive(true);
        }
    }

    private void HighlightKingInCheck()
    {
        King kingInCheck = FindKing(isWhiteTurn);
        if (kingInCheck != null && checkHighlightPrefab != null)
        {
            // We get the tile GameObject to ensure the highlight is placed on the board itself, not floating with the piece.
            GameObject tile = ChessBoardPlacementHandler.Instance.GetTile(kingInCheck.row, kingInCheck.column);
            // We instantiate the highlight and parent it to the tile for correct positioning and scaling.
            _activeCheckHighlight = Instantiate(checkHighlightPrefab, tile.transform.position, Quaternion.identity, tile.transform);
        }
    }

    // This RPC is called on all clients to handle the pawn promotion logic.
    [PunRPC]
    private void PromotePawnRPC(int row, int col, int pieceType)
    {
        ChessPiece pawnToDestroy = piecePositions[row, col];
        if (pawnToDestroy == null || !(pawnToDestroy is Pawn)) return;

        // We get the color of the pawn and remove it from the logical board on all clients.
        bool isWhite = pawnToDestroy.isWhite;
        piecePositions[row, col] = null;

        // Only the Master Client has the authority to destroy the old pawn and instantiate the new piece.
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(pawnToDestroy.gameObject);

            // Get the correct prefab for the new piece
            GameObject prefabToCreate = GetPromotionPrefab(isWhite, (PawnPromotionHandler.PieceType)pieceType);

            if (prefabToCreate != null)
            {
                // We create the new piece using PhotonNetwork.Instantiate.
                // An extra boolean is added to the instantiation data to signal that this is a promotion,
                // which allows the `ContinueGame` logic to be correctly triggered on all clients.
                object[] instantiationData = { isWhite, row, col, true };
                PhotonNetwork.Instantiate(prefabToCreate.name, ChessBoardPlacementHandler.Instance.GetTile(row, col).transform.position, Quaternion.identity, 0, instantiationData);
            }
        }
    }

    // A helper function to get the correct piece prefab for promotion based on color and selected piece type.
    private GameObject GetPromotionPrefab(bool isWhite, PawnPromotionHandler.PieceType pieceType)
    {
        PieceManager pm = PieceManager.Instance;
        return isWhite
            ? pieceType switch
            {
                PawnPromotionHandler.PieceType.Queen => pm.whiteQueenPrefab,
                PawnPromotionHandler.PieceType.Rook => pm.whiteRookPrefab,
                PawnPromotionHandler.PieceType.Bishop => pm.whiteBishopPrefab,
                PawnPromotionHandler.PieceType.Knight => pm.whiteKnightPrefab,
                _ => null
            }
            : pieceType switch
            {
                PawnPromotionHandler.PieceType.Queen => pm.blackQueenPrefab,
                PawnPromotionHandler.PieceType.Rook => pm.blackRookPrefab,
                PawnPromotionHandler.PieceType.Bishop => pm.blackBishopPrefab,
                PawnPromotionHandler.PieceType.Knight => pm.blackKnightPrefab,
                _ => null
            };
    }

    // Initializes the dictionaries that count captured pieces at the start of the game.
    public void InitializeCapturedPieceCounters()
    {
        _capturedWhitePiecesCount = new Dictionary<System.Type, int>
        {
            { typeof(Pawn), 0 },
            { typeof(Rook), 0 },
            { typeof(Knight), 0 },
            { typeof(Bishop), 0 },
            { typeof(Queen), 0 }
        };

        _capturedBlackPiecesCount = new Dictionary<System.Type, int>
        {
            { typeof(Pawn), 0 },
            { typeof(Rook), 0 },
            { typeof(Knight), 0 },
            { typeof(Bishop), 0 },
            { typeof(Queen), 0 }
        };

        UpdateCapturedPiecesUI();
    }

    // Updates the UI text elements to show the current count of captured pieces.
    private void UpdateCapturedPiecesUI()
    {
        // Update UI for captured black pieces (captured by white player)
        if (capturedBlackPawnText != null) capturedBlackPawnText.text = "x " + _capturedBlackPiecesCount[typeof(Pawn)];
        if (capturedBlackRookText != null) capturedBlackRookText.text = "x " + _capturedBlackPiecesCount[typeof(Rook)];
        if (capturedBlackKnightText != null) capturedBlackKnightText.text = "x " + _capturedBlackPiecesCount[typeof(Knight)];
        if (capturedBlackBishopText != null) capturedBlackBishopText.text = "x " + _capturedBlackPiecesCount[typeof(Bishop)];
        if (capturedBlackQueenText != null) capturedBlackQueenText.text = "x " + _capturedBlackPiecesCount[typeof(Queen)];

        // Update UI for captured white pieces (captured by black player)
        if (capturedWhitePawnText != null) capturedWhitePawnText.text = "x " + _capturedWhitePiecesCount[typeof(Pawn)];
        if (capturedWhiteRookText != null) capturedWhiteRookText.text = "x " + _capturedWhitePiecesCount[typeof(Rook)];
        if (capturedWhiteKnightText != null) capturedWhiteKnightText.text = "x " + _capturedWhitePiecesCount[typeof(Knight)];
        if (capturedWhiteBishopText != null) capturedWhiteBishopText.text = "x " + _capturedWhitePiecesCount[typeof(Bishop)];
        if (capturedWhiteQueenText != null) capturedWhiteQueenText.text = "x " + _capturedWhitePiecesCount[typeof(Queen)];
    }

    // A utility function to find the king of a specific color on the board.
    private King FindKing(bool isWhite)
    {
        foreach (ChessPiece piece in piecePositions)
        {
            if (piece != null && piece is King && piece.isWhite == isWhite)
            {
                return (King)piece;
            }
        }
        return null;
    }

    // This function determines if a king of a given color is currently in check.
    public bool IsInCheck(bool isWhiteKing)
    {
        King king = FindKing(isWhiteKing);
        if (king == null) return false; // This should not happen in a standard game.

        Vector2Int kingPos = new Vector2Int(king.row, king.column);

        // It iterates through all enemy pieces and checks if any of their pseudo-legal moves include the king's position.
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                ChessPiece piece = piecePositions[r, c];
                if (piece != null && piece.isWhite != isWhiteKing)
                {
                    if (piece.GetPseudoLegalMoves().Contains(kingPos))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // This function determines if a king is in checkmate.
    public bool IsCheckmate(bool isWhiteKing)
    {
        if (!IsInCheck(isWhiteKing)) return false;

        // It checks if the player whose king is in check has any legal moves available for any of their pieces.
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                ChessPiece piece = piecePositions[r, c];
                if (piece != null && piece.isWhite == isWhiteKing && piece.GetPossibleMoves().Count > 0)
                {
                    return false; // If even one piece has a legal move, it's not checkmate.
                }
            }
        }

        return true; // If the king is in check and no piece can make a legal move, it's checkmate.
    }

    // A simple utility to check if a given row and column are within the board's 8x8 boundaries.
    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }

    private void UpdateTurnStatusText()
    {
        if (turnStatusText != null)
        {
            bool isPlayerWhite = PhotonNetwork.IsMasterClient;
            if (isWhiteTurn)
            {
                turnStatusText.text = isPlayerWhite ? "Your turn (White)" : "Opponent's turn (White)";
            }
            else
            {
                turnStatusText.text = !isPlayerWhite ? "Your turn (Black)" : "Opponent's turn (Black)";
            }
        }
    }

    // This function shows a temporary UI panel to inform the player of their assigned color.
    private void ShowPlayerColorInfo()
    {
        if (playerColorInfoPanel != null && playerColorInfoText != null)
        {
            bool isPlayerWhite = PhotonNetwork.IsMasterClient;
            string color = isPlayerWhite ? "White" : "Black";
            playerColorInfoText.text = "You play with " + color + " set";
            playerColorInfoPanel.SetActive(true);
            StartCoroutine(HidePlayerColorInfoAfterDelay(3.0f));
        }
    }

    // A coroutine to hide the player color info panel after a short delay.
    private IEnumerator HidePlayerColorInfoAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerColorInfoPanel != null) playerColorInfoPanel.SetActive(false);
    }
}