using UnityEngine;
using System.Collections.Generic;

public class King : ChessPiece
{
    // This is where we figure out all the places a King can go.
    public override List<Vector2Int> GetPseudoLegalMoves()
    {
        var moves = new List<Vector2Int>();
        var controller = ChessGameController.Instance;

        // A king can move one step in any of the 8 directions around it.
        // These numbers represent those eight directions.
        int[] dr = { -1, 1, 0, 0, 1, 1, -1, -1 }; // Change in row
        int[] dc = { 0, 0, -1, 1, 1, -1, 1, -1 }; // Change in column

        // Let's check each of the eight squares right next to the king.
        for (int i = 0; i < 8; i++)
        {
            int nextRow = row + dr[i];
            int nextCol = column + dc[i];

            // First, make sure the square is actually on the board.
            if (controller.IsValidPosition(nextRow, nextCol))
            {
                // Then, see if there's a piece on that square.
                ChessPiece occupyingPiece = controller.piecePositions[nextRow, nextCol];

                // A king can move to an empty square or a square with an enemy piece.
                // It just can't move to a square with one of its own pieces.
                if (occupyingPiece == null || occupyingPiece.isWhite != this.isWhite)
                {
                    moves.Add(new Vector2Int(nextRow, nextCol));
                }
            }
        }
        return moves;
    }
}