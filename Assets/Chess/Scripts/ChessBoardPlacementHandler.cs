using System;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;

// This script handles the visual setup of the chessboard and manages highlighting for piece movements.
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class ChessBoardPlacementHandler : MonoBehaviour {
    // These are the rows of the chessboard, linked from the Unity Editor. Each GameObject in the array represents a row of tiles.
    [SerializeField] private GameObject[] _rowsArray;
    // A prefab for the green marker that shows valid move locations for a selected piece.
    [SerializeField] private GameObject _highlightPrefab;
    // A prefab for the red marker that indicates an enemy piece can be captured on that tile.
    [SerializeField] private GameObject _highlightEnemyPrefab;
    // A prefab for the marker that highlights the currently selected piece.
    [SerializeField] private GameObject _highlightSelectedPrefab;
    // This 2D array represents our 8x8 chessboard, storing a reference to each tile's GameObject for easy access.
    private GameObject[,] _chessBoard;

    // A static reference to this instance, making it easily accessible from anywhere in the game. This is the Singleton pattern.
    internal static ChessBoardPlacementHandler Instance;

    // When the script instance is being loaded, this function sets up the singleton instance and prepares the board array.
    private void Awake() {
        Instance = this;
        GenerateArray();
    }

    // This function populates the 8x8 `_chessBoard` grid by iterating through the `_rowsArray` linked in the editor.
    // It also ensures each tile has a collider and a `Tile` component, making them clickable and aware of their position.
    private void GenerateArray() {
        _chessBoard = new GameObject[8, 8];
        for (var i = 0; i < 8; i++) {
            for (var j = 0; j < 8; j++) {
                _chessBoard[i, j] = _rowsArray[i].transform.GetChild(j).gameObject;
                GameObject tileObject = _chessBoard[i, j];

                // A collider is necessary for the `OnMouseDown` event to be triggered when a tile is clicked.
                if (tileObject.GetComponent<Collider>() == null)
                {
                    tileObject.AddComponent<BoxCollider>();
                }

                // The `Tile` component holds the row and column information for each tile, making it easy to identify which tile was clicked.
                Tile tile = tileObject.GetComponent<Tile>() ?? tileObject.AddComponent<Tile>();
                tile.row = i;
                tile.col = j;
            }
        }
    }

    // Retrieves a specific tile's GameObject from our `_chessBoard` grid using its row and column.
    internal GameObject GetTile(int i, int j) {
        try {
            // A try-catch block provides a safety net in case we request a tile that is outside the board's 8x8 bounds.
            return _chessBoard[i, j];
        } catch (Exception) {
            Debug.LogError("Invalid row or column.");
            return null;
        }
    }

    // A helper function to create and place a highlight marker on a specific tile.
    private void CreateHighlight(GameObject prefab, int row, int col) {
        var tile = GetTile(row, col);
        if (tile == null) {
            // If the tile doesn't exist, `GetTile` will log an error, so we just exit here.
            return;
        }

        // We instantiate the highlight prefab at the tile's position and make it a child of the tile.
        // This keeps the scene hierarchy organized. The visual layering is handled by Unity's Sorting Layers.
        Instantiate(prefab, tile.transform.position, Quaternion.identity, tile.transform);
    }

    // Places a green highlight on a tile, indicating a valid move for the selected piece.
    internal void Highlight(int row, int col) {
        CreateHighlight(_highlightPrefab, row, col);
    }

    // Places a red highlight on a tile, indicating an enemy piece can be captured there.
    internal void HighlightEnemy(int row, int col) {
        CreateHighlight(_highlightEnemyPrefab, row, col);
    }

    // Places a special highlight on a tile to show which piece is currently selected by the player.
    internal void HighlightSelected(int row, int col) {
        CreateHighlight(_highlightSelectedPrefab, row, col);
    }


    // Clears all highlights from the board, preparing it for the next player's turn or a new selection.
    internal void ClearHighlights() {
        for (var i = 0; i < 8; i++) {
            for (var j = 0; j < 8; j++) {
                var tile = GetTile(i, j);
                if (tile.transform.childCount <= 0) continue;
                // We iterate backward when removing items from a collection to avoid issues with shifting indices.
                for (int k = tile.transform.childCount - 1; k >= 0; k--) {
                    var child = tile.transform.GetChild(k);
                    // We only destroy GameObjects that are tagged as "Highlight" to avoid accidentally removing pieces or other objects.
                    if (child.CompareTag("Highlight"))
                        Destroy(child.gameObject);
                }
            }
        }
    }

}