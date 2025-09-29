using UnityEngine;

// This component is attached to each tile of the chessboard.
public class Tile : MonoBehaviour
{
    // These public variables store the tile's position in the 8x8 grid.
    public int row;
    public int col;

    // This Unity message is called automatically when the tile's collider is clicked.
    private void OnMouseDown()
    {
        // When a tile is clicked, it notifies the main game controller, passing its own row and column.
        ChessGameController.Instance.OnTileClicked(row, col);
    }
}