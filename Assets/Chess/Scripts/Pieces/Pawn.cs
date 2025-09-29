using UnityEngine;
using System.Collections.Generic;

public class Pawn : ChessPiece
{
    // This is where we figure out all the places a Pawn can go.
    public override List<Vector2Int> GetPseudoLegalMoves()
    {
        var moves = new List<Vector2Int>();
        var controller = ChessGameController.Instance;

        // Pawns move in different directions depending on their color.
        // White pawns move "up" the board (increasing row number), black pawns move "down".
        int direction = isWhite ? 1 : -1;

        // --- Let's check for moves straight ahead ---

        // First, find the square one step in front of the pawn.
        int oneStepForwardRow = row + direction;

        // We can only move forward if the square in front is on the board and is empty.
        if (controller.IsValidPosition(oneStepForwardRow, column) && controller.piecePositions[oneStepForwardRow, column] == null)
        {
            // If it's clear, that's a valid move.
            moves.Add(new Vector2Int(oneStepForwardRow, column));

            // Now, let's check for the special two-step first move.
            // This is only possible if the pawn is on its starting row.
            bool isStartingRank = (isWhite && row == 1) || (!isWhite && row == 6);
            int twoStepsForwardRow = row + 2 * direction;

            // To make the two-step move, the pawn must be on its starting row,
            // and the square two steps ahead must also be on the board and empty.
            if (isStartingRank && controller.IsValidPosition(twoStepsForwardRow, column) && controller.piecePositions[twoStepsForwardRow, column] == null)
            {
                moves.Add(new Vector2Int(twoStepsForwardRow, column));
            }
        }

        // --- Now, let's check for diagonal captures ---

        // A pawn can capture on the two diagonal squares in front of it.
        int[] captureCols = { column - 1, column + 1 };
        foreach (int captureCol in captureCols)
        {
            // Make sure the diagonal square is on the board.
            if (controller.IsValidPosition(oneStepForwardRow, captureCol))
            {
                // See if there's a piece on that diagonal square.
                ChessPiece pieceToCapture = controller.piecePositions[oneStepForwardRow, captureCol];

                // We can only capture if there's an enemy piece there.
                if (pieceToCapture != null && pieceToCapture.isWhite != this.isWhite)
                {
                    moves.Add(new Vector2Int(oneStepForwardRow, captureCol));
                }
            }
        }

        return moves;
    }
}