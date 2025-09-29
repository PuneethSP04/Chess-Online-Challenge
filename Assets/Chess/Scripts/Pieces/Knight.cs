using UnityEngine;
using System.Collections.Generic;

public class Knight : ChessPiece
{
    // This is where we figure out all the places a Knight can go.
    public override List<Vector2Int> GetPseudoLegalMoves()
    {
        var moves = new List<Vector2Int>();
        var controller = ChessGameController.Instance;

        // A knight moves in an "L" shape: two squares in one direction (straight),
        // and then one square to the side. There are 8 possible L-shapes.
        // These numbers represent those eight possible landing spots.
        int[] dr = { 2, 2, -2, -2, 1, 1, -1, -1 }; // Change in row
        int[] dc = { 1, -1, 1, -1, 2, -2, 2, -2 }; // Change in column

        // Let's check each of the eight possible L-moves.
        for (int i = 0; i < 8; i++)
        {
            int nextRow = row + dr[i];
            int nextCol = column + dc[i];

            // First, make sure the landing square is actually on the board.
            if (controller.IsValidPosition(nextRow, nextCol))
            {
                // Then, see if there's a piece on that square.
                ChessPiece occupyingPiece = controller.piecePositions[nextRow, nextCol];

                // A knight can land on an empty square or a square with an enemy piece.
                // It just can't land on a square with one of its own pieces.
                if (occupyingPiece == null || occupyingPiece.isWhite != this.isWhite)
                {
                    moves.Add(new Vector2Int(nextRow, nextCol));
                }
            }
        }
        return moves;
    }
}