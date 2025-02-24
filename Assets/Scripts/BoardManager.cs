using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;

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

    public int numOfMovesWithoutCaptureOrCheck = 0;
    public int enPassantSquareIndex;
    public string currentFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public bool isWhitePlayedByHuman = true;
    public bool isBlackPlayedByHuman = true;
    
    public Dictionary<string, int> fenOccurences = new Dictionary<string, int>(50);


    [StructLayout(LayoutKind.Sequential)]
    public struct MoveIndices
    {
        public int from;
        public int to;
    }

    [DllImport("ChessLogic", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr getLegalMoves(string fen, out int moveCount);

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

    public int UciSquareToBitboardIndex(string square)
    {
        if (square.Length != 2)
            return -1;

        char file = square[0];
        char rank = square[1];

        int fileIndex = file - 'a';
        int rankIndex = rank - '1';

        return rankIndex * 8 + fileIndex;
    }

    string BitboardIndexToUci(int index)
    {
        if (index < 0 || index > 63)
            return null; // Return null if the index is out of bounds

        int fileIndex = index % 8; // Get the file (column) by taking modulo 8
        int rankIndex = index / 8; // Get the rank (row) by integer division by 8

        char file = (char)('a' + fileIndex); // Convert file index to letter ('a' to 'h')
        char rank = (char)('1' + rankIndex); // Convert rank index to digit ('1' to '8')

        return $"{file}{rank}";
    }


    public void PlacePieces(bool whitePiecesIsDraggable = true, bool blackPiecesIsDraggable = true)
    {
        isWhitePlayedByHuman = whitePiecesIsDraggable;
        isBlackPlayedByHuman = blackPiecesIsDraggable;
        string[] fenParts = currentFen.Split(' ');
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
            enPassantSquareIndex = UciSquareToBitboardIndex(enPassantSquare);
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
                PlacePieceFromFen(c, squareIndex, whitePiecesIsDraggable, blackPiecesIsDraggable);
                squareIndex++;
            }
        }
    }
    void PlacePieceFromFen(char fenChar, int squareIndex, bool whitePiecesIsDraggable, bool blackPiecesIsDraggable)
    {
        Piece.PieceColor color = char.IsUpper(fenChar) ? Piece.PieceColor.White : Piece.PieceColor.Black;
        Piece.PieceType type;

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
        PlacePiece(prefab, squareIndex, color, type, whitePiecesIsDraggable, blackPiecesIsDraggable);
    }

    GameObject GetPrefab(Piece.PieceType type, Piece.PieceColor color)
    {
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
    void PlacePiece(GameObject piecePrefab, int squareIndex, Piece.PieceColor color, Piece.PieceType pieceType, bool whitePiecesIsDraggable, bool blackPiecesIsDraggable)
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
            pieceComponent.isDraggable = (color == Piece.PieceColor.White) ? whitePiecesIsDraggable : blackPiecesIsDraggable;

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
    public List<char> GetPiecesForBitboard(string fen)
    {
        // Initialize a list of 64 characters for the chessboard
        List<char> pieces = new List<char>(new char[64]);

        // Extract the piece placement part from the FEN string
        string piecePlacement = fen.Split(' ')[0];

        int index = 0; // Index for the bitboard

        foreach (char c in piecePlacement)
        {
            if (c == '/')
            {
                continue; // Skip row separators
            }
            else if (char.IsDigit(c))
            {
                // Convert digit to an integer and increase the index
                int emptySquares = c - '0';
                index += emptySquares; // Skip empty squares
            }
            else
            {
                pieces[index] = c; // Place the piece at the correct index
                index++; // Move to the next index
            }
        }

        return pieces;
    }


    public List<int> GetLegalMovesFromIndex(int currentIndex)
    {
        List<int> legalMovesTo = new List<int>();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        IntPtr movesPtr = getLegalMoves(currentFen, out int moveCount);

        // Convert the pointer to an array of MoveIndices structs
        MoveIndices[] moves = new MoveIndices[moveCount];
        for (int i = 0; i < moveCount; i++)
        {
            IntPtr movePtr = IntPtr.Add(movesPtr, i * Marshal.SizeOf(typeof(MoveIndices)));
            moves[i] = Marshal.PtrToStructure<MoveIndices>(movePtr);
        }

        // Filter moves where move.from equals currentIndex
        foreach (var move in moves)
        {
            if (move.from == currentIndex)
            {
                legalMovesTo.Add(move.to);
            }
        }
#else
        Debug.Log("DLL not supported on this platform.");
#endif

        return legalMovesTo;
    }

    public int GetNumberOfLegalMoves(string fen)
    {
        int moveCount = 0;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Call the getLegalMoves function from the DLL
        IntPtr movesPtr = getLegalMoves(fen, out moveCount);
#else
    Debug.Log("DLL not supported on this platform.");
#endif

        return moveCount; // Return the total number of legal moves
    }


    public List<char> GetCurrentPiecesFromFen()
    {
        string[] fenParts = currentFen.Split(' ');
        string piecePlacement = fenParts[0]; // Get the piece placement part of the FEN
        List<char> piecesList = new List<char>(64); // Create a list for 64 squares

        // Initialize the list with empty spaces
        for (int i = 0; i < 64; i++)
        {
            piecesList.Add(' '); // Use ' ' to represent empty squares
        }

        int squareIndex = 0; // Start from the top-left of the board (A8)

        foreach (char c in piecePlacement)
        {
            if (c == '/')
            {
                continue;
            }
            else if (char.IsDigit(c))
            {
                squareIndex += int.Parse(c.ToString()); // Skip empty squares by moving the index
            }
            else // Piece
            {
                piecesList[squareIndex] = c; // Place the piece in the correct index
                squareIndex++; // Move to the next square
            }
        }

        return piecesList; // Return the list of pieces
    }

    public int SquareIndexToPieceListIndex(int squareIndex)
    {
        int row = squareIndex / 8; // Get the row number in 0-based indexing from A1 upwards
        int col = squareIndex % 8; // Get the column number (0 for A, 7 for H)

        // Calculate the corresponding piece list index by reversing the row order
        int pieceListIndex = (7 - row) * 8 + col;

        return pieceListIndex;
    }

    public void UpdateFenAfterMove(int originalSquareIndex, int targetSquareIndex, bool isCastlingMove)
    {
        List<char> piecesList = GetCurrentPiecesFromFen();

        // Convert square indices to piecesList indices
        int originalIndex = SquareIndexToPieceListIndex(originalSquareIndex);
        int targetIndex = SquareIndexToPieceListIndex(targetSquareIndex);

        // Find the piece at the original index
        char movingPiece = piecesList[originalIndex];

        // Check if the moving piece is valid (not empty)
        if (movingPiece == ' ')
        {
            Debug.LogError("No piece found at the index: " + originalSquareIndex);
            return; // Exit if no piece is found
        }

        // Set the original index to an empty space
        piecesList[originalIndex] = ' ';

        // Place the moving piece at the target index
        piecesList[targetIndex] = movingPiece;

        //Debug.Log("Current pieces on the board after moving a piece: [" + string.Join(", ", piecesList.Select(c => $"'{c}'")) + "]");

        // Convert the pieces list back to a FEN string


        if (char.ToLower(movingPiece) == 'p' && Mathf.Abs(originalSquareIndex - targetSquareIndex) == 16)
        {
            // Update en passant square logic here
            string enPassantSquare;
            if (char.IsUpper(movingPiece))
            {
                enPassantSquare = BitboardIndexToUci(targetSquareIndex - 8);
            }
            else
            {
                enPassantSquare = BitboardIndexToUci(targetSquareIndex + 8);
            }
            currentFen = ConvertPiecesListToFen(piecesList, enPassantSquare, isCastlingMove);
        }
        else
        {
            currentFen = ConvertPiecesListToFen(piecesList, "-", isCastlingMove);
        }
        Debug.Log(currentFen);
    }


    private string ConvertPiecesListToFen(List<char> piecesList, string enPassantSquareUci, bool isCastlingMove)
    {
        StringBuilder fenBuilder = new StringBuilder();
        int emptyCount = 0;

        for (int i = 0; i < piecesList.Count; i++)
        {
            char piece = piecesList[i];

            if (piece == ' ')
            {
                emptyCount++;
            }
            else
            {
                if (emptyCount > 0)
                {
                    fenBuilder.Append(emptyCount); // Append number of empty squares
                    emptyCount = 0;
                }
                fenBuilder.Append(piece); // Append the piece
            }

            // End of rank: Add '/' and reset count
            if ((i + 1) % 8 == 0)
            {
                if (emptyCount > 0)
                {
                    fenBuilder.Append(emptyCount); // Append any remaining empty squares
                    emptyCount = 0;
                }
                if (i < 63) fenBuilder.Append('/'); // Add '/' except for the last rank
            }
        }

        // Return the generated piece placement FEN string, keeping the turn, castling rights, and en passant
        string[] fenParts = currentFen.Split(' ');
        string colorToMove = fenParts[1];
        if (!isCastlingMove)
        {
            if (colorToMove == "w")
            {
                gameManager.isWhiteToMove = false;
                colorToMove = "b";
            }
            else
            {
                gameManager.isWhiteToMove = true;
                colorToMove = "w";
            }
        }
        return fenBuilder.ToString() + " " + colorToMove + " " + fenParts[2] + " " + enPassantSquareUci + " " + fenParts[4] + " " + fenParts[5];
    }

    // Call this method when a piece is moved
    public void MovePiece(int originalIndex, int targetIndex, bool isCastlingMove)
    {
        UpdateFenAfterMove(originalIndex, targetIndex, isCastlingMove);
    }

    public void AddFen(Dictionary<string, int> fenOccurrences, string fen)
    {
        if (fenOccurrences.ContainsKey(fen))
        {
            fenOccurrences[fen]++;

            // Check for threefold repetition
            if (fenOccurrences[fen] == 3)
            {
                gameManager.ShowGameOver("Draw by threefold repetition");
            }

            if (fenOccurrences.Count >= 50)
            {
                fenOccurrences.Clear();
                fenOccurrences[fen] = 1;
            }
        }
        else
        {
            fenOccurrences[fen] = 1;

            if (fenOccurrences.Count >= 50)
            {
                fenOccurrences.Clear();
                fenOccurrences[fen] = 1;
            }
        }
    }

}
