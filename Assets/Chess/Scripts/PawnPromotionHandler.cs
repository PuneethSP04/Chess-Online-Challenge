using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

// This script handles the UI and logic for pawn promotion. When a pawn reaches the end of the board,
// it presents the player with a choice of which piece to promote to.
public class PawnPromotionHandler : MonoBehaviour
{
    // A singleton instance for easy access from other scripts, like the ChessGameController.
    public static PawnPromotionHandler Instance { get; private set; }

    [Header("UI Windows")]
    // These are the UI panels for white and black's promotion choices, assigned in the Unity Editor.
    [SerializeField] private GameObject whitePromotionWindow;
    [SerializeField] private GameObject blackPromotionWindow;

    [Header("White Promotion Buttons")]
    // Buttons for the white player to select a new piece.
    [SerializeField] private Button whiteQueenButton;
    [SerializeField] private Button whiteRookButton;
    [SerializeField] private Button whiteBishopButton;
    [SerializeField] private Button whiteKnightButton;

    [Header("Black Promotion Buttons")]
    // Buttons for the black player to select a new piece.
    [SerializeField] private Button blackQueenButton;
    [SerializeField] private Button blackRookButton;
    [SerializeField] private Button blackBishopButton;
    [SerializeField] private Button blackKnightButton;

    // This holds a reference to the pawn that is ready to be promoted.
    private Pawn pawnToPromote;

    // An enum to represent the possible piece types for promotion. This makes the code cleaner than using magic numbers or strings.
    public enum PieceType
    {
        Queen,
        Rook,
        Bishop,
        Knight
    }

    // Standard singleton pattern implementation.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // In Start, we ensure the promotion windows are hidden and set up the listeners for all the promotion buttons.
    private void Start()
    {
        whitePromotionWindow.SetActive(false);
        blackPromotionWindow.SetActive(false);

        // When a button is clicked, it calls OnPromotionSelect with the corresponding piece type.
        whiteQueenButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Queen));
        whiteRookButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Rook));
        whiteBishopButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Bishop));
        whiteKnightButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Knight));

        blackQueenButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Queen));
        blackRookButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Rook));
        blackBishopButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Bishop));
        blackKnightButton.onClick.AddListener(() => OnPromotionSelect(PieceType.Knight));
    }

    // This function is called by the ChessGameController when a pawn reaches the promotion rank.
    public void ShowPromotionDialog(Pawn pawn)
    {
        // We store the pawn and show the correct promotion window based on the pawn's color.
        pawnToPromote = pawn;
        (pawn.isWhite ? whitePromotionWindow : blackPromotionWindow).SetActive(true);
    }

    // This function is triggered when the player clicks one of the promotion piece buttons.
    private void OnPromotionSelect(PieceType pieceType)
    {
        if (pawnToPromote == null) return;

        // Hide both promotion windows.
        whitePromotionWindow.SetActive(false);
        blackPromotionWindow.SetActive(false);

        // We use a Photon RPC (Remote Procedure Call) to tell all clients to perform the pawn promotion.
        // This ensures the game state remains synchronized across the network.
        ChessGameController.Instance.photonView.RPC("PromotePawnRPC", RpcTarget.All, pawnToPromote.row, pawnToPromote.column, (int)pieceType);
        pawnToPromote = null;
    }
}