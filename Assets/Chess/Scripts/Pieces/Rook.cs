using UnityEngine;
using System.Collections.Generic;

public class Rook : ChessPiece
{
    // This is where we figure out all the places a Rook can go.
    public override List<Vector2Int> GetPseudoLegalMoves()
    {
        var moves = new List<Vector2Int>();
        var controller = ChessGameController.Instance;

        // A rook moves in four straight lines. We'll check each one.
        // These numbers represent the four directions:
        // Up, Down, Left, and Right.
        int[] dr = { -1, 1, 0, 0 }; // Change in row
        int[] dc = { 0, 0, -1, 1 }; // Change in column

        // Let's look at each of the four directions.
        for (int i = 0; i < 4; i++)
        {
            // We'll keep moving one step at a time in the current direction.
            for (int step = 1; step < 8; step++)
            {
                int nextRow = row + dr[i] * step;
                int nextCol = column + dc[i] * step;

                // If we're about to go off the board, stop checking this direction.
                if (!controller.IsValidPosition(nextRow, nextCol)) break;

                // Let's see if there's a piece on the next square.
                ChessPiece occupyingPiece = controller.piecePositions[nextRow, nextCol];
                if (occupyingPiece != null)
                {
                    // If it's an enemy piece, we can capture it. So, that's a valid move.
                    if (occupyingPiece.isWhite != this.isWhite)
                    {
                        moves.Add(new Vector2Int(nextRow, nextCol));
                    }
                    // But we can't jump over any piece (friend or foe), so we stop looking further in this direction.
                    break;
                }

                // If the square is empty, it's a valid move. Let's add it.
                moves.Add(new Vector2Int(nextRow, nextCol));
            }
        }
        return moves;
    }
}