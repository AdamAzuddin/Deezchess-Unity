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
    public int enPassantSquareIndex;

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

        // Set each white piece as white
        whitePawns.SetWhitePiece(true);
        whiteRooks.SetWhitePiece(true);
        whiteKnights.SetWhitePiece(true);
        whiteBishops.SetWhitePiece(true);
        whiteQueens.SetWhitePiece(true);
        whiteKing.SetWhitePiece(true);

        // Initialize black pieces
        blackPawns.SetBitboard(0x00FF000000000000UL);
        blackRooks.SetBitboard(0x8100000000000000UL);
        blackKnights.SetBitboard(0x4200000000000000UL);
        blackBishops.SetBitboard(0x2400000000000000UL);
        blackQueens.SetBitboard(0x0800000000000000UL);
        blackKing.SetBitboard(0x1000000000000000UL);

        // Set each black piece as black
        blackPawns.SetWhitePiece(false);
        blackRooks.SetWhitePiece(false);
        blackKnights.SetWhitePiece(false);
        blackBishops.SetWhitePiece(false);
        blackQueens.SetWhitePiece(false);
        blackKing.SetWhitePiece(false);
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

    int uciSquareToBitboardIndex(string square)
    {
        if (square.Length != 2)
            return -1;

        char file = square[0];
        char rank = square[1];

        int fileIndex = file - 'a';
        int rankIndex = rank - '1';

        return rankIndex * 8 + fileIndex;
    }


    // Method to place pieces on the board by index
    void PlacePieces()
    {
        string fen = "rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 1";
        string[] fenParts = fen.Split(' ');
        string piecePlacement = fenParts[0];
        gameManager.isWhiteToMove = fenParts[1] == "w";
        string castlingRights = fenParts[2];
        gameManager.whiteCanShortCastle = castlingRights.Contains("K");
        gameManager.whiteCanLongCastle = castlingRights.Contains("Q");
        gameManager.blackCanShortCastle = castlingRights.Contains("k");
        gameManager.blackCanLongCastle = castlingRights.Contains("q");
        string enPassantSquare = fenParts[3];
        if (enPassantSquare != "-")
        {
            enPassantSquareIndex = uciSquareToBitboardIndex(enPassantSquare);
        }
        int squareIndex = 56; // Start from the bottom-left of the board (A1)

        foreach (char c in piecePlacement)
        {
            if (c == '/')
            {
                squareIndex -= 16;
            }
            else if (char.IsDigit(c)) // Empty squares
            {
                squareIndex += int.Parse(c.ToString());
            }
            else // Piece
            {
                PlacePieceFromFen(c, squareIndex);
                squareIndex++;
            }
        }
    }

    // Helper method to place a piece based on the FEN character and square index
    void PlacePieceFromFen(char fenChar, int squareIndex)
    {
        Piece.PieceColor color = char.IsUpper(fenChar) ? Piece.PieceColor.White : Piece.PieceColor.Black;
        Piece.PieceType type;

        // Determine the piece type based on the FEN character
        switch (char.ToLower(fenChar))
        {
            case 'p': type = Piece.PieceType.Pawn; break;
            case 'r': type = Piece.PieceType.Rook; break;
            case 'n': type = Piece.PieceType.Knight; break;
            case 'b': type = Piece.PieceType.Bishop; break;
            case 'q': type = Piece.PieceType.Queen; break;
            case 'k': type = Piece.PieceType.King; break;
            default: return; // Unknown piece type
        }

        GameObject prefab = GetPrefab(type, color);
        PlacePiece(prefab, squareIndex, color, type);
    }

    // Method to retrieve the prefab for the specific piece type and color
    GameObject GetPrefab(Piece.PieceType type, Piece.PieceColor color)
    {
        // Replace with actual references to your prefabs
        switch (type)
        {
            case Piece.PieceType.Pawn: return color == Piece.PieceColor.White ? whitePawnPrefab : blackPawnPrefab;
            case Piece.PieceType.Rook: return color == Piece.PieceColor.White ? whiteRookPrefab : blackRookPrefab;
            case Piece.PieceType.Knight: return color == Piece.PieceColor.White ? whiteKnightPrefab : blackKnightPrefab;
            case Piece.PieceType.Bishop: return color == Piece.PieceColor.White ? whiteBishopPrefab : blackBishopPrefab;
            case Piece.PieceType.Queen: return color == Piece.PieceColor.White ? whiteQueenPrefab : blackQueenPrefab;
            case Piece.PieceType.King: return color == Piece.PieceColor.White ? whiteKingPrefab : blackKingPrefab;
            default: return null;
        }
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
        UpdateWhiteControlledSquares();
        UpdateBlackControlledSquares();
    }



    public void UpdateWhiteControlledSquares()
    {
        whiteControlledSquares.SetBitboard(0UL);

        // Calculate control for each white piece type and update the controlled squares
        whiteControlledSquares.SetBitboard(
            whiteControlledSquares.GetBitboard() | CalculatePawnAttacks(whitePawns));

        whiteControlledSquares.SetBitboard(
            whiteControlledSquares.GetBitboard() | CalculateRookAttacks(whiteRooks));

        whiteControlledSquares.SetBitboard(
            whiteControlledSquares.GetBitboard() | CalculateBishopAttacks(whiteBishops));

        whiteControlledSquares.SetBitboard(
            whiteControlledSquares.GetBitboard() | CalculateQueenAttacks(whiteQueens));

        whiteControlledSquares.SetBitboard(
            whiteControlledSquares.GetBitboard() | CalculateKingAttacks(whiteKing));

        whiteControlledSquares.SetBitboard(
            whiteControlledSquares.GetBitboard() | CalculateKnightAttacks(whiteKnights));
    }

    public void UpdateBlackControlledSquares()
    {
        blackControlledSquares.SetBitboard(0UL);

        blackControlledSquares.SetBitboard(
            blackControlledSquares.GetBitboard() | CalculatePawnAttacks(blackPawns));

        blackControlledSquares.SetBitboard(
            blackControlledSquares.GetBitboard() | CalculateRookAttacks(blackRooks));

        blackControlledSquares.SetBitboard(
            blackControlledSquares.GetBitboard() | CalculateBishopAttacks(blackBishops));

        blackControlledSquares.SetBitboard(
            blackControlledSquares.GetBitboard() | CalculateQueenAttacks(blackQueens));

        blackControlledSquares.SetBitboard(
            blackControlledSquares.GetBitboard() | CalculateKingAttacks(blackKing));

        blackControlledSquares.SetBitboard(
            blackControlledSquares.GetBitboard() | CalculateKnightAttacks(blackKnights));
    }

    // Placeholder methods for calculating attacks
    public ulong CalculatePawnAttacks(Bitboard pawns)
    {
        Bitboard pawnControlledSquare = new Bitboard();
        pawnControlledSquare.SetBitboard(0UL);

        var friendlyPieces = pawns.IsWhitePiece() ? whitePieces : blackPieces;
        ulong northEastAttackMask = 0UL;
        ulong northWestAttackMask = 0UL;

        // Get all pawn positions (indices)
        List<int> pawnIndices = pawns.GetIndicesOfPieces();

        for (int i = 0; i < pawnIndices.Count; i++)
        {
            int pawnIndex = pawnIndices[i];

            // Reset attack masks for each pawn
            northEastAttackMask = 0UL;
            northWestAttackMask = 0UL;

            // Check if the pawn is white or black for attack directions
            if (pawns.IsWhitePiece())
            {
                // White pawns attack to the northeast and northwest
                if (pawnIndex % 8 != 0)  // Not on A-file
                {
                    northWestAttackMask = 1UL << (pawnIndex + 7);
                }
                if ((pawnIndex + 1) % 8 != 0)  // Not on H-file
                {
                    northEastAttackMask = 1UL << (pawnIndex + 9);
                }
            }
            else
            {
                // Black pawns attack to the southeast and southwest
                if (pawnIndex % 8 != 0)  // Not on A-file
                {
                    northEastAttackMask = 1UL << (pawnIndex - 7);
                }
                if ((pawnIndex + 1) % 8 != 0)  // Not on H-file
                {
                    northWestAttackMask = 1UL << (pawnIndex - 9);
                }
            }

            // Combine attack masks for this pawn
            pawnControlledSquare.SetBitboard(
                pawnControlledSquare.GetBitboard() | northEastAttackMask | northWestAttackMask);
        }

        // Remove any attacks that fall on friendly pieces
        ulong attackMask = pawnControlledSquare.GetBitboard();
        ulong friendlyPiecesMask = friendlyPieces.GetBitboard();
        pawnControlledSquare.SetBitboard(attackMask & ~friendlyPiecesMask);

        return pawnControlledSquare.GetBitboard();
    }


    public ulong CalculateRookAttacks(Bitboard rooks)
    {
        Bitboard rookAttackMask = new Bitboard();
        rookAttackMask.SetBitboard(0);

        List<int> rookIndices = rooks.GetIndicesOfPieces();

        for (int i = 0; i < rookIndices.Count; i++)
        {
            ulong attackMask = GenerateRookAttackMask(rookIndices[i], rooks.IsWhitePiece()) & ~(1UL << rookIndices[i]);
            rookAttackMask.SetBitboard(rookAttackMask.GetBitboard() | attackMask);
        }

        rookAttackMask.SetBitboard(rookAttackMask.GetBitboard() & ~rooks.GetBitboard());

        return rookAttackMask.GetBitboard();
    }

    public ulong CalculateBishopAttacks(Bitboard bishops)
    {
        Bitboard bishopAttackMask = new Bitboard();
        bishopAttackMask.SetBitboard(0);

        List<int> bishopIndices = bishops.GetIndicesOfPieces();

        for (int i = 0; i < bishopIndices.Count; i++)
        {
            bishopAttackMask.SetBitboard(bishopAttackMask.GetBitboard() | GenerateBishopAttackMask(bishopIndices[i], bishops.IsWhitePiece()));
        }

        return bishopAttackMask.GetBitboard();
    }

    public ulong CalculateQueenAttacks(Bitboard queens)
    {
        Bitboard queenAttackMask = new Bitboard();
        queenAttackMask.SetBitboard(0);

        List<int> queenIndices = queens.GetIndicesOfPieces();

        for (int i = 0; i < queenIndices.Count; i++)
        {
            ulong rookMask = GenerateRookAttackMask(queenIndices[i], queens.IsWhitePiece());
            ulong bishopMask = GenerateBishopAttackMask(queenIndices[i], queens.IsWhitePiece());
            queenAttackMask.SetBitboard(queenAttackMask.GetBitboard() | rookMask | bishopMask);
        }

        queenAttackMask.SetBitboard(queenAttackMask.GetBitboard() & ~queens.GetBitboard());

        return queenAttackMask.GetBitboard();
    }

    public ulong CalculateKingAttacks(Bitboard king)
    {
        var friendlyPieces = king.IsWhitePiece() ? whitePieces : blackPieces;
        ulong kingBitboard = king.GetBitboard();
        int kingIndex = FindFirstSetBit(kingBitboard); // Helper method to find index of first set bit (king)

        Bitboard attacks = new Bitboard();
        attacks.SetBitboard(kingAttackTable[kingIndex] & ~friendlyPieces.GetBitboard());

        return attacks.GetBitboard();
    }

    public ulong CalculateKnightAttacks(Bitboard knights)
    {
        Bitboard knightAttackMask = new Bitboard();
        knightAttackMask.SetBitboard(0);

        var friendlyPieces = knights.IsWhitePiece() ? whitePieces : blackPieces;
        List<int> knightIndices = knights.GetIndicesOfPieces();

        for (int i = 0; i < knightIndices.Count; i++)
        {
            knightAttackMask.SetBitboard(knightAttackMask.GetBitboard() | GenerateKnightAttackMask(knightIndices[i]));
        }

        return knightAttackMask.GetBitboard() & ~friendlyPieces.GetBitboard();
    }

    // Helper method to find the first set bit
    private int FindFirstSetBit(ulong bitboard)
    {
        int index = 0;
        while (bitboard != 0)
        {
            if ((bitboard & 1UL) != 0)
                return index;
            bitboard >>= 1;
            index++;
        }
        return -1;
    }

    // Placeholder methods for attack generation
    public ulong GenerateRookAttackMask(int rookIndex, bool isWhiteRook)
    {
        Bitboard fileMask = new Bitboard();
        fileMask.SetBitboard(0);

        Bitboard rankMask = new Bitboard();
        rankMask.SetBitboard(0);

        ulong rookMask = 1UL << rookIndex;
        ulong currentMask = 1UL << rookIndex;
        ulong northMask, southMask, westMask, eastMask = 1UL << rookIndex;
        ulong FILE_AH_MASK = FILE_A_MASK | FILE_H_MASK;

        ulong friendlyPieces = isWhiteRook ? whitePieces.GetBitboard() : blackPieces.GetBitboard();
        ulong enemyPieces = isWhiteRook ? blackPieces.GetBitboard() : whitePieces.GetBitboard();

        int currentIndex = rookIndex;

        while (true)
        {
            currentIndex += 8;
            if (currentIndex > 63) break;

            northMask = 1UL << currentIndex;
            if ((northMask & friendlyPieces) != 0) break;

            if ((northMask & enemyPieces) != 0)
            {
                fileMask.SetBitboard(fileMask.GetBitboard() | northMask);
                break;
            }
            fileMask.SetBitboard(fileMask.GetBitboard() | northMask);
        }

        currentIndex = rookIndex;

        while (true)
        {
            currentIndex -= 8;
            if (currentIndex < 0) break;

            southMask = 1UL << currentIndex;
            if ((southMask & enemyPieces) != 0)
            {
                fileMask.SetBitboard(fileMask.GetBitboard() | southMask);
                break;
            }

            if ((southMask & friendlyPieces) != 0) break;
            fileMask.SetBitboard(fileMask.GetBitboard() | southMask);
        }

        currentIndex = rookIndex;

        while (true)
        {
            currentIndex++;
            if (currentIndex > 63) break;
            eastMask = 1UL << currentIndex;

            if ((eastMask & FILE_H_MASK) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | eastMask);
                break;
            }

            if ((eastMask & friendlyPieces) != 0) break;

            if ((eastMask & enemyPieces) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | eastMask);
                break;
            }
            rankMask.SetBitboard(rankMask.GetBitboard() | eastMask);
        }

        currentIndex = rookIndex;

        while (true)
        {
            currentIndex--;
            if (currentIndex < 0) break;
            westMask = 1UL << currentIndex;

            if ((westMask & FILE_A_MASK) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | westMask);
                break;
            }

            if ((westMask & friendlyPieces) != 0) break;

            if ((westMask & enemyPieces) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | westMask);
                break;
            }
            rankMask.SetBitboard(rankMask.GetBitboard() | westMask);
        }

        return fileMask.GetBitboard() | rankMask.GetBitboard();
    }

    public ulong GenerateTopHalfDiagonal(int index, int shift, bool isWhiteBishop)
    {
        ulong currentMask;
        int currentIndex = index;
        Bitboard result = new Bitboard();
        ulong friendlyPieces = isWhiteBishop ? whitePieces.GetBitboard() : blackPieces.GetBitboard();
        ulong enemyPieces = isWhiteBishop ? blackPieces.GetBitboard() : whitePieces.GetBitboard();
        result.SetBitboard(0);

        ulong leftRightTopEdgeMask = FILE_A_MASK | FILE_H_MASK | RANK_8_MASK;

        currentMask = 1UL << index;

        while (true)
        {
            currentIndex += shift;
            currentMask = 1UL << currentIndex;
            if ((currentMask & friendlyPieces) != 0 || currentIndex > 63) break;

            if ((currentMask & leftRightTopEdgeMask) != 0 || (currentMask & enemyPieces) != 0)
            {
                result.SetBitboard(result.GetBitboard() | currentMask);
                break;
            }

            if (currentIndex < 0) break;

            result.SetBitboard(result.GetBitboard() | currentMask);
        }

        return result.GetBitboard();
    }
    public ulong GenerateBottomHalfDiagonal(int index, int shift, bool isWhiteBishop)
    {
        ulong currentMask;
        int currentIndex = index;
        Bitboard result = new Bitboard();
        ulong friendlyPieces = isWhiteBishop ? whitePieces.GetBitboard() : blackPieces.GetBitboard();
        ulong enemyPieces = isWhiteBishop ? blackPieces.GetBitboard() : whitePieces.GetBitboard();
        result.SetBitboard(0);

        ulong leftRightBottomEdgeMask = FILE_A_MASK | FILE_H_MASK | RANK_1_MASK;

        currentMask = 1UL << index;

        while (true)
        {
            currentIndex -= shift;
            currentMask = 1UL << currentIndex;
            if ((currentMask & friendlyPieces) != 0 || currentIndex < 0) break;

            if ((currentMask & leftRightBottomEdgeMask) != 0 || (currentMask & enemyPieces) != 0)
            {
                result.SetBitboard(result.GetBitboard() | currentMask);
                break;
            }

            result.SetBitboard(result.GetBitboard() | currentMask);
        }

        return result.GetBitboard();
    }

    public ulong GenerateBishopAttackMask(int bishopIndex, bool isWhiteBishop)
    {
        Bitboard attacks = new Bitboard();

        // Create mask for diagonals
        attacks.SetBitboard(GenerateBottomHalfDiagonal(bishopIndex, 7, isWhiteBishop));
        attacks.SetBitboard(attacks.GetBitboard() | GenerateBottomHalfDiagonal(bishopIndex, 9, isWhiteBishop));
        attacks.SetBitboard(attacks.GetBitboard() | GenerateTopHalfDiagonal(bishopIndex, 9, isWhiteBishop));
        attacks.SetBitboard(attacks.GetBitboard() | GenerateTopHalfDiagonal(bishopIndex, 7, isWhiteBishop));

        return attacks.GetBitboard();
    }

    public ulong GenerateKnightAttackMask(int knightIndex)
    {
        Bitboard attacks = new Bitboard();
        ulong knightMask = 1UL << knightIndex;
        ulong FILE_AB_MASK = FILE_A_MASK | FILE_B_MASK;
        ulong FILE_GH_MASK = FILE_G_MASK | FILE_H_MASK;

        ulong attackNorthNorthWest = (knightMask & ~FILE_A_MASK) << 15;  // 2 up, 1 left
        ulong attackNorthNorthEast = (knightMask & ~FILE_H_MASK) << 17;  // 2 up, 1 right
        ulong attackSouthSouthhEast = (knightMask & ~FILE_H_MASK) >> 15; // 2 down, 1 right
        ulong attackSouthSouthhWest = (knightMask & ~FILE_A_MASK) >> 17; // 2 down, 1 left

        ulong attackWestWestNorth = (knightMask & ~FILE_AB_MASK) << 6;   // 2 left, 1 up
        ulong attackWestWestSouth = (knightMask & ~FILE_AB_MASK) >> 10;  // 2 left, 1 down
        ulong attackEastEastNorth = (knightMask & ~FILE_GH_MASK) << 10;  // 2 right, 1 up
        ulong attackEastEastSouth = (knightMask & ~FILE_GH_MASK) >> 6;   // 2 right, 1 down

        attacks.SetBitboard(attackNorthNorthWest | attackNorthNorthEast | attackSouthSouthhEast | attackSouthSouthhWest |
                            attackWestWestNorth | attackWestWestSouth | attackEastEastNorth | attackEastEastSouth);

        return attacks.GetBitboard();
    }


}
