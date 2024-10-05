using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject board;
    public GameObject whitePawnPrefab;
    public GameObject whiteRookPrefab;
    public GameObject whiteKnightPrefab;
    public GameObject whiteBishopPrefab;
    public GameObject whiteQueenPrefab;
    public GameObject whiteKingPrefab;

    // Prefabs for black pieces
    public GameObject blackPawnPrefab;
    public GameObject blackRookPrefab;
    public GameObject blackKnightPrefab;
    public GameObject blackBishopPrefab;
    public GameObject blackQueenPrefab;
    public GameObject blackKingPrefab;

    private Square[] squares = new Square[64];


    public List<Square> highlightedSquares = new List<Square>();
    public Piece pieceToMove = null;
    public Square pieceToMoveSquare = null;
    public bool isSelectPieceToMove = false;
    private Color selectedPieceSquareColor = new Color32(160, 200, 255, 255);

    public GameManager gameManager;

    public Bitboard whitePawns = new Bitboard();
    public Bitboard whiteRooks = new Bitboard();
    public Bitboard whiteKnights = new Bitboard();
    public Bitboard whiteBishops = new Bitboard();
    public Bitboard whiteQueens = new Bitboard();
    public Bitboard whiteKing = new Bitboard();

    public Bitboard blackPawns = new Bitboard();
    public Bitboard blackRooks = new Bitboard();
    public Bitboard blackKnights = new Bitboard();
    public Bitboard blackBishops = new Bitboard();
    public Bitboard blackQueens = new Bitboard();
    public Bitboard blackKing = new Bitboard();

    public Bitboard whiteControlledSquares = new Bitboard();
    public Bitboard blackControlledSquares = new Bitboard();
    public Bitboard occupiedSquares = new Bitboard();
    public Bitboard whitePieces = new Bitboard();
    public Bitboard blackPieces = new Bitboard();


    public ulong FILE_A_MASK;
    public ulong FILE_B_MASK;
    public ulong FILE_C_MASK;
    public ulong FILE_D_MASK;
    public ulong FILE_E_MASK;
    public ulong FILE_F_MASK;
    public ulong FILE_G_MASK;
    public ulong FILE_H_MASK;
    public ulong RANK_1_MASK;
    public ulong RANK_2_MASK;
    public ulong RANK_3_MASK;
    public ulong RANK_4_MASK;
    public ulong RANK_5_MASK;
    public ulong RANK_6_MASK;
    public ulong RANK_7_MASK;
    public ulong RANK_8_MASK;
    public ulong boardEdgeMask;
    public ulong[] kingAttackTable = new ulong[64];


    public int numOfMovesWithoutCaptureOrCheck = 0;
    public bool whiteCanShortCastle = true;
    public bool whiteCanLongCastle = true;
    public bool blackCanShortCastle = true;
    public bool blackCanLongCastle = true;
    public enum BoardState
    {
        SelectingPiece,
        MovingPiece,
        Waiting
    }

    public BoardState currentState = BoardState.Waiting;

    public static BoardManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeBoard();
        InitializeBitboards();
        initializeFilesAndRanks();
        PlacePieces();
        InitializeKingAttackMasks();
        gameManager = FindObjectOfType<GameManager>();
        gameManager.isWhiteToMove = true;
        if (gameManager == null)
        {
            Debug.LogError("BoardManager not found in the scene!");
        }
    }

    void InitializeBoard()
    {
        if (board == null)
        {
            Debug.LogError("Board GameObject is not assigned!");
            return;
        }

        int childCount = board.transform.childCount;
        if (childCount != 64)
        {
            Debug.LogError("The Board does not have 64 children!");
            return;
        }

        for (int index = 0; index < 64; index++)
        {
            Transform squareTransform = board.transform.GetChild(index);

            Square square = squareTransform.GetComponent<Square>();

            if (square != null)
            {
                square.index = index;

                squares[index] = square;
            }
            else
            {
                Debug.LogError($"Square component is missing on child at index {index}");
            }
        }
    }

    void InitializeBitboards()
    {
        // Initialize white pieces
        whitePawns.SetBitboard(0x000000000000FF00UL);
        whiteRooks.SetBitboard(0x0000000000000081UL);
        whiteKnights.SetBitboard(0x0000000000000042UL);
        whiteBishops.SetBitboard(0x0000000000000024UL);
        whiteQueens.SetBitboard(0x0000000000000008UL);
        whiteKing.SetBitboard(0x0000000000000010UL);

        // Initialize black pieces
        blackPawns.SetBitboard(0x00FF000000000000UL);
        blackRooks.SetBitboard(0x8100000000000000UL);
        blackKnights.SetBitboard(0x4200000000000000UL);
        blackBishops.SetBitboard(0x2400000000000000UL);
        blackQueens.SetBitboard(0x0800000000000000UL);
        blackKing.SetBitboard(0x1000000000000000UL);

        // Combine pieces into collections
        whitePieces.SetBitboard(
            whitePawns.GetBitboard() | whiteRooks.GetBitboard() |
            whiteKnights.GetBitboard() | whiteBishops.GetBitboard() |
            whiteQueens.GetBitboard() | whiteKing.GetBitboard());

        blackPieces.SetBitboard(
            blackPawns.GetBitboard() | blackRooks.GetBitboard() |
            blackKnights.GetBitboard() | blackBishops.GetBitboard() |
            blackQueens.GetBitboard() | blackKing.GetBitboard());

        occupiedSquares.SetBitboard(whitePieces.GetBitboard() | blackPieces.GetBitboard());

        // Initialize control squares
        whiteControlledSquares.SetBitboard(0);
        blackControlledSquares.SetBitboard(0);

        // Reset move count and castling rights
        numOfMovesWithoutCaptureOrCheck = 0;
        whiteCanShortCastle = true;
        whiteCanLongCastle = true;
        blackCanShortCastle = true;
        blackCanLongCastle = true;
    }

    public void initializeFilesAndRanks()
    {

        FILE_A_MASK = 0x0101010101010101UL;
        FILE_B_MASK = 0x0202020202020202UL;
        FILE_C_MASK = 0x0404040404040404UL;
        FILE_D_MASK = 0x0808080808080808UL;
        FILE_E_MASK = 0x1010101010101010UL;
        FILE_F_MASK = 0x2020202020202020UL;
        FILE_G_MASK = 0x4040404040404040UL;
        FILE_H_MASK = 0x8080808080808080UL;

        RANK_1_MASK = 0x00000000000000FFUL;
        RANK_2_MASK = 0x000000000000FF00UL;
        RANK_3_MASK = 0x0000000000FF0000UL;
        RANK_4_MASK = 0x00000000FF000000UL;
        RANK_5_MASK = 0x000000FF00000000UL;
        RANK_6_MASK = 0x0000FF0000000000UL;
        RANK_7_MASK = 0x00FF000000000000UL;
        RANK_8_MASK = 0xFF00000000000000UL;

        boardEdgeMask = FILE_A_MASK | RANK_8_MASK | FILE_H_MASK | RANK_1_MASK;
    }

    public void InitializeKingAttackMasks()
    {
        for (int square = 0; square < 64; square++)
        {
            ulong kingBitboard = 1UL << square;
            ulong attacks = 0;

            attacks |= (kingBitboard << 8);                // north
            attacks |= (kingBitboard >> 8);                // south
            attacks |= (kingBitboard & ~FILE_A_MASK) >> 1; // west
            attacks |= (kingBitboard & ~FILE_H_MASK) << 1; // east
            attacks |= (kingBitboard & ~FILE_A_MASK) << 7; // north-west
            attacks |= (kingBitboard & ~FILE_H_MASK) << 9; // north-east
            attacks |= (kingBitboard & ~FILE_A_MASK) >> 9; // south-west
            attacks |= (kingBitboard & ~FILE_H_MASK) >> 7; // south-east

            kingAttackTable[square] = attacks;
        }
    }


    // Method to place pieces on the board by index
    void PlacePieces()
    {
        for (int i = 8; i <= 15; i++)
        {
            PlacePiece(whitePawnPrefab, i, Piece.PieceColor.White, Piece.PieceType.Pawn);
        }

        for (int i = 48; i <= 55; i++)
        {
            PlacePiece(blackPawnPrefab, i, Piece.PieceColor.Black, Piece.PieceType.Pawn);
        }

        PlacePiece(whiteRookPrefab, 0, Piece.PieceColor.White, Piece.PieceType.Rook);
        PlacePiece(whiteRookPrefab, 7, Piece.PieceColor.White, Piece.PieceType.Rook);

        PlacePiece(blackRookPrefab, 56, Piece.PieceColor.Black, Piece.PieceType.Rook);
        PlacePiece(blackRookPrefab, 63, Piece.PieceColor.Black, Piece.PieceType.Rook);

        PlacePiece(whiteKnightPrefab, 1, Piece.PieceColor.White, Piece.PieceType.Knight);
        PlacePiece(whiteKnightPrefab, 6, Piece.PieceColor.White, Piece.PieceType.Knight);

        PlacePiece(blackKnightPrefab, 57, Piece.PieceColor.Black, Piece.PieceType.Knight);
        PlacePiece(blackKnightPrefab, 62, Piece.PieceColor.Black, Piece.PieceType.Knight);

        PlacePiece(whiteBishopPrefab, 2, Piece.PieceColor.White, Piece.PieceType.Bishop);
        PlacePiece(whiteBishopPrefab, 5, Piece.PieceColor.White, Piece.PieceType.Bishop);

        PlacePiece(blackBishopPrefab, 58, Piece.PieceColor.Black, Piece.PieceType.Bishop);
        PlacePiece(blackBishopPrefab, 61, Piece.PieceColor.Black, Piece.PieceType.Bishop);

        PlacePiece(whiteQueenPrefab, 3, Piece.PieceColor.White, Piece.PieceType.Queen);

        PlacePiece(blackQueenPrefab, 59, Piece.PieceColor.Black, Piece.PieceType.Queen);

        PlacePiece(whiteKingPrefab, 4, Piece.PieceColor.White, Piece.PieceType.King);

        PlacePiece(blackKingPrefab, 60, Piece.PieceColor.Black, Piece.PieceType.King);
    }


    // Method to place an individual piece on a specific square based on the single index
    void PlacePiece(GameObject piecePrefab, int squareIndex, Piece.PieceColor color, Piece.PieceType pieceType)
    {
        if (squareIndex < 0 || squareIndex >= 64)
        {
            Debug.LogError("Invalid square index!");
            return;
        }

        Square targetSquare = squares[squareIndex];

        if (targetSquare == null)
        {
            Debug.LogError("Square not found at index: " + squareIndex);
            return;
        }

        GameObject newPiece = Instantiate(piecePrefab, targetSquare.transform.position, Quaternion.identity);

        newPiece.transform.parent = transform;

        newPiece.transform.position = new Vector3(newPiece.transform.position.x, newPiece.transform.position.y, -1);

        SpriteRenderer spriteRenderer = newPiece.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10;
        }

        Piece pieceComponent = newPiece.GetComponent<Piece>();
        if (pieceComponent != null)
        {

            pieceComponent.pieceColor = color;
            pieceComponent.currentX = squareIndex % 8;
            pieceComponent.currentY = squareIndex / 8;
            pieceComponent.pieceType = pieceType;
            targetSquare.occupiedPiece = pieceComponent;

        }

    }

    // New method to select a square
    public void SelectPiece(Square square)
    {

        pieceToMove = square.occupiedPiece;
        pieceToMoveSquare = square;
        square.spriteRenderer.color = selectedPieceSquareColor;
        isSelectPieceToMove = true;
    }
    public void HighlightSquare(Square square)
    {
        highlightedSquares.Add(square);
        square.spriteRenderer.color = Color.red;
    }
    public void DeselectPiece()
    {
        pieceToMoveSquare.spriteRenderer.color = pieceToMoveSquare.color;
        foreach (Square sq in highlightedSquares)
        {
            sq.spriteRenderer.color = sq.color;
        }
        highlightedSquares.Clear();
        pieceToMoveSquare = null;
        pieceToMove = null;
        isSelectPieceToMove = false;
    }

    public void movePieceTo(GameObject pieceGameObj, int moveToIndex)
    {
        PlacePiece(pieceGameObj, moveToIndex, pieceToMove.pieceColor, pieceToMove.pieceType);
    }

    public void UpdatePiecesBitboards()
    {
        // Combine all white pieces
        whitePieces.SetBitboard(whitePawns.GetBitboard() | whiteRooks.GetBitboard() | whiteKnights.GetBitboard() | whiteBishops.GetBitboard() | whiteQueens.GetBitboard() | whiteKing.GetBitboard());

        // Combine all black pieces
        blackPieces.SetBitboard(blackPawns.GetBitboard() | blackRooks.GetBitboard() | blackKnights.GetBitboard() | blackBishops.GetBitboard() | blackQueens.GetBitboard() | blackKing.GetBitboard());

        // Combine all pieces to get occupied squares
        occupiedSquares.SetBitboard(whitePieces.GetBitboard() | blackPieces.GetBitboard());
    }

}
