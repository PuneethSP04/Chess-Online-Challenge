using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

// This abstract class serves as the foundation for all chess pieces in the game (Pawn, Rook, King, etc.).
// It handles common properties like position and color, and defines the core logic for movement and network instantiation.
public abstract class ChessPiece : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    // These public variables store the piece's current position on the board grid.
    public int row;
    public int column;
    // `isWhite` determines the piece's color, which is crucial for turn management and identifying friend from foe.
    public bool isWhite;
    // `hasMoved` is a flag used for special chess rules like the pawn's first two-square move and castling for the King and Rook.
    public bool hasMoved = false;

    // A reference to the SmoothLerp component, which provides smooth visual movement between tiles.
    private SmoothLerp _smoothLerp;

    // This Photon callback is executed on all clients when a new piece is instantiated over the network.
    // It's responsible for setting up the piece's initial state (color, position) based on data sent by the Master Client.
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // The `InstantiationData` is an array of objects sent by the client that created this piece.
        // For a standard piece, it contains: [isWhite, row, col]. For a promoted pawn, it includes an extra boolean.
        object[] data = info.photonView.InstantiationData;
        if (data != null && data.Length >= 3)
        {
            // We ensure a SmoothLerp component is attached to handle the piece's movement animation.
            _smoothLerp = GetComponent<SmoothLerp>() ?? gameObject.AddComponent<SmoothLerp>();

            this.isWhite = (bool)data[0];
            int initialRow = (int)data[1];
            int initialCol = (int)data[2];

            // All clients (including the one who instantiated it) run this code to place the piece on the board
            // and update the central game controller's record of piece positions. This keeps everyone's game state in sync.
            PlaceOnBoard(initialRow, initialCol);
            ChessGameController.Instance.piecePositions[initialRow, initialCol] = this;

            // If this piece was created as part of a pawn promotion, the game flow was paused.
            // Now that the new piece exists on all clients, we signal the game controller to continue.
            if (data.Length == 4 && (bool)data[3])
            {
                ChessGameController.Instance.ContinueGame();
            }
        }
    }

    // This function handles the visual placement of the piece on a specific tile.
    public void PlaceOnBoard(int newRow, int newColumn)
    {
        // We update the piece's internal row and column, then find the world position of the target tile.
        row = newRow;
        column = newColumn;
        Vector3 tilePosition = ChessBoardPlacementHandler.Instance.GetTile(row, column).transform.position;

        // The SmoothLerp component is used to animate the piece's movement to the new tile, making it look natural.
        _smoothLerp.StartMove(tilePosition, 0.2f); // 0.2 seconds for a quick but smooth move.
    }

    // Each piece type (Pawn, Rook, etc.) must implement this method to define its unique movement rules.
    // This returns a list of "pseudo-legal" moves, which are all possible moves without considering if they put the king in check.
    public abstract List<Vector2Int> GetPseudoLegalMoves();

    // This method filters the pseudo-legal moves to determine the truly legal ones.
    // A move is only legal if it does not result in the player's own king being in check.
    public List<Vector2Int> GetPossibleMoves()
    {
        var legalMoves = new List<Vector2Int>();
        var pseudoLegalMoves = GetPseudoLegalMoves();
        var controller = ChessGameController.Instance;

        // For each potential move, we simulate it on a temporary board state.
        foreach (var move in pseudoLegalMoves)
        {
            // We save the original state of the board at the start and end positions.
            int originalRow = row;
            int originalCol = column;
            ChessPiece pieceAtTarget = controller.piecePositions[move.x, move.y];

            // Here, we perform the move on the logical board representation.
            controller.piecePositions[move.x, move.y] = this;
            controller.piecePositions[originalRow, originalCol] = null;
            row = move.x;
            column = move.y;

            // After simulating the move, we check if the current player's king is in check.
            // If it's not, the move is legal and added to our list.
            if (!controller.IsInCheck(isWhite))
            {
                legalMoves.Add(move);
            }

            // Finally, we revert the board state back to how it was before the simulation.
            row = originalRow;
            column = originalCol;
            controller.piecePositions[originalRow, originalCol] = this;
            controller.piecePositions[move.x, move.y] = pieceAtTarget;
        }
        return legalMoves;
    }

    // This Unity message is called automatically when the GameObject's collider is clicked by the mouse.
    private void OnMouseDown()
    {
        // When a piece is clicked, we notify the main game controller to handle the selection logic.
        ChessGameController.Instance.OnPieceSelected(this);
    }
}