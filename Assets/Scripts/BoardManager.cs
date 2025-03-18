using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using Rudz.Chess;
using Rudz.Chess.Factories;
using Rudz.Chess.Fen;
using Rudz.Chess.Types;

public class BoardManager : MonoBehaviour
{
    public GameObject board;
    public GameObject whitePawnPrefab;
    public GameObject whiteRookPrefab;
    public GameObject whiteKnightPrefab;
    public GameObject whiteBishopPrefab;
    public GameObject whiteQueenPrefab;
    public GameObject whiteKingPrefab;

    public GameObject blackPawnPrefab;
    public GameObject blackRookPrefab;
    public GameObject blackKnightPrefab;
    public GameObject blackBishopPrefab;
    public GameObject blackQueenPrefab;
    public GameObject blackKingPrefab;

    public Square[] squares = new Square[64];


    public List<Square> highlightedSquares = new List<Square>();
    public Piece pieceToMove = null;
    public Square pieceToMoveSquare = null;
    public bool isSelectPieceToMove = false;
    private Color selectedPieceSquareColor = new Color32(160, 200, 255, 255);

    public GameManager gameManager;

    public int numOfMovesWithoutCaptureOrCheck = 0;
    public int enPassantSquareIndex;
    public readonly int searchDepth = 10;
    public string currentFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public bool isWhitePlayedByHuman = true;
    public bool isBlackPlayedByHuman = true;

    public Dictionary<string, int> fenOccurences = new Dictionary<string, int>(50);


    public readonly StockfishEngine engine = new StockfishEngine();

    public enum BoardState
    {
        SelectingPiece,
        MovingPiece,
        Waiting
    }

    public BoardState currentState = BoardState.Waiting;

    public static BoardManager Instance;
    public ChessAPI chessAPI;



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
        GetLegalMoves();
        InitializeBoard();
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("BoardManager not found in the scene!");
        }
        chessAPI = FindObjectOfType<ChessAPI>();
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


    public void GetLegalMoves()
    {
        Game game = new Game(new Position());
        game.SetFen(new FenData(currentFen));
        MoveList moves = MoveFactory.GenerateMoves(game.Position);
        Debug.Log("Legal moves from ChessLib: ");
        foreach (Move move in moves){
            StringBuilder stringBuilder = new StringBuilder();
            game.MoveToString(move, stringBuilder);
            Debug.Log(stringBuilder.ToString());
        }
        
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

    public void UpdateFenAfterPromotion(int promotionSquareIndex, Piece.PieceColor color, Piece.PieceType type)
    {
        List<char> piecesList = GetCurrentPiecesFromFen();

        // Convert square index to piecesList index
        int promotionIndex = SquareIndexToPieceListIndex(promotionSquareIndex);

        // Determine promoted piece character
        char promotedPiece = type switch
        {
            Piece.PieceType.Queen => 'q',
            Piece.PieceType.Rook => 'r',
            Piece.PieceType.Bishop => 'b',
            Piece.PieceType.Knight => 'n',
            _ => 'q' // Default to queen
        };

        // Keep capitalization for color
        if (color == Piece.PieceColor.White)
            promotedPiece = char.ToUpper(promotedPiece);

        // Replace the pawn with the promoted piece
        piecesList[promotionIndex] = promotedPiece;

        // Preserve all FEN parts
        string[] fenParts = currentFen.Split(' ');

        // Maintain the correct turn (preserve instead of switching)
        string currentTurn = fenParts[1];

        // Convert back to FEN while keeping the correct turn
        currentFen = ConvertPiecesListToFen(piecesList, "-", false);

        // Restore the correct turn in the updated FEN
        string[] updatedFenParts = currentFen.Split(' ');
        updatedFenParts[1] = currentTurn;

        gameManager.isWhiteToMove = (currentTurn == "w") ? true : false;

        // Join the parts back into a full FEN string
        currentFen = string.Join(" ", updatedFenParts);
    }


    public GameObject GetPrefab(Piece.PieceType type, Piece.PieceColor color)
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
    public void PlacePiece(GameObject piecePrefab, int squareIndex, Piece.PieceColor color, Piece.PieceType pieceType, bool whitePiecesIsDraggable, bool blackPiecesIsDraggable)
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
    public void GetNumberOfLegalMoves(string fen, System.Action<int> onResult)
    {
        StartCoroutine(chessAPI.GetTotalLegalMovesCount(fen, onResult));
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
    public string RemovePieceFromFen(string fen, int indexToRemove)
    {
        string[] parts = fen.Split(' ');
        string boardPart = parts[0];

        // Convert FEN to 64-char board representation
        List<char> boardList = new List<char>();

        foreach (char c in boardPart)
        {
            if (char.IsDigit(c))
            {
                int emptySquares = c - '0';
                for (int i = 0; i < emptySquares; i++)
                    boardList.Add('1'); // Use '1' to represent empty for internal processing
            }
            else if (c == '/')
            {
                // Marker to track new rank
                boardList.Add('/');
            }
            else
            {
                boardList.Add(c); // Piece character
            }
        }

        // Map bitboard index to correct index in boardList
        int rank = 7 - (indexToRemove / 8); // FEN starts from rank 8
        int file = indexToRemove % 8;

        int currentRank = 0, currentFile = 0;

        for (int i = 0; i < boardList.Count; i++)
        {
            if (boardList[i] == '/')
            {
                currentRank++;
                currentFile = 0;
                continue;
            }

            if (currentRank == rank && currentFile == file)
            {
                boardList[i] = '1'; // remove the piece
                break;
            }

            currentFile++;
        }

        // Rebuild FEN piece placement from boardList
        List<string> finalRanks = new List<string>();
        List<char> currentRankList = new List<char>();

        foreach (char c in boardList)
        {
            if (c == '/')
            {
                finalRanks.Add(CompressRank(currentRankList));
                currentRankList.Clear();
            }
            else
            {
                currentRankList.Add(c);
            }
        }

        finalRanks.Add(CompressRank(currentRankList)); // last rank

        string newBoardPart = string.Join("/", finalRanks);

        // Return new FEN with updated piece placement + original other info
        return newBoardPart + " " + string.Join(" ", parts.Skip(1));
    }

    private static string CompressRank(List<char> rank)
    {
        string result = "";
        int emptyCount = 0;

        foreach (char c in rank)
        {
            if (c == '1')
            {
                emptyCount++;
            }
            else
            {
                if (emptyCount > 0)
                {
                    result += emptyCount.ToString();
                    emptyCount = 0;
                }
                result += c;
            }
        }

        if (emptyCount > 0)
            result += emptyCount.ToString();

        return result;
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

    public (int, int) UciMoveToBitboardIndices(string uciMove)
    {

        // (0,0) = short castle white
        // (1,1) = long castle white
        // (2,2) = short castle black
        // (3,3) = long castle black

        if (uciMove.Length != 4)
            return (-1, -1);

        string fromSquare = uciMove.Substring(0, 2);
        string toSquare = uciMove.Substring(2, 2);
        string castlingRights = currentFen.Split(" ")[2];

        int fromIndex = UciSquareToBitboardIndex(fromSquare);
        int toIndex = UciSquareToBitboardIndex(toSquare);

        // Castling logic

        Square whiteKingSquare = FindSquareByIndex(4);
        Square blackKingSquare = FindSquareByIndex(60);

        if (whiteKingSquare.occupiedPiece != null && whiteKingSquare.occupiedPiece.pieceType == Piece.PieceType.King && whiteKingSquare.occupiedPiece.pieceColor == Piece.PieceColor.White && gameManager.isWhiteToMove)
        {
            if (uciMove == "e1g1" && castlingRights.Contains("K"))
            {
                return (0, 0);
            }
            else if (uciMove == "e1c1" && castlingRights.Contains("Q"))
            {
                return (1, 1);
            }
        }
        else if (blackKingSquare.occupiedPiece != null && blackKingSquare.occupiedPiece.pieceType == Piece.PieceType.King && blackKingSquare.occupiedPiece.pieceColor == Piece.PieceColor.Black && !gameManager.isWhiteToMove)
        {
            if (uciMove == "e8g8" && castlingRights.Contains("k"))
            {
                return (2, 2);
            }
            else if (uciMove == "e8c8" && castlingRights.Contains("q"))
            {
                return (3, 3);
            }
        }

        return (fromIndex, toIndex);
    }

    public int UciSquareToBitboardIndex(string square)
    {
        // Example logic: Convert "e2" to bitboard index
        if (square.Length != 2)
            return -1;

        char file = square[0]; // 'a' to 'h'
        char rank = square[1]; // '1' to '8'

        if (file < 'a' || file > 'h' || rank < '1' || rank > '8')
            return -1;

        return (rank - '1') * 8 + (file - 'a'); // Convert to 0-63 index
    }

    public Square FindSquareByIndex(int targetIndex)
    {
        return gameManager.GetSquareByIndex(targetIndex);
    }

    public void EngineMove(string fen, int depth, int halfMoveCount, int fullMoveCount)
    {
        // find the best move using stockfish
        string bestMoveUci = engine.GetBestMove(fen, depth);
        bool isPromotionMove = false;
        char pieceToPromoteWith = ' ';
        // check if its a promotion!
        if (bestMoveUci.Length == 5)
        {
            isPromotionMove = true;
            pieceToPromoteWith = bestMoveUci[^1]; // Get the last character as string
            bestMoveUci = bestMoveUci.Remove(bestMoveUci.Length - 1);
        }
        Debug.Log("BestMoveUCI length: " + bestMoveUci.Length);
        var bestMove = UciMoveToBitboardIndices(bestMoveUci);
        if (bestMove.Item1 == -1)
        {
            gameManager.ShowGameOver("Stockfish gave invalid Move");
        }

        bool isCastling = false;
        if (bestMove.Item2 == bestMove.Item1 && bestMove.Item1 != -1)
        {
            isCastling = true;
        }
        if (!isCastling)
        {
            Debug.Log($"Stockfish move from: {bestMove.Item1}, To: {bestMove.Item2}");
            Square initialSquare = FindSquareByIndex(bestMove.Item1);
            Square targetSquare = FindSquareByIndex(bestMove.Item2);

            // actually move the piece
            Piece pieceToMove = initialSquare.occupiedPiece;
            if (targetSquare.occupiedPiece != null)
            {
                Destroy(targetSquare.occupiedPiece.gameObject);
            }
            pieceToMove.transform.position = targetSquare.transform.position;
            pieceToMove.currentX = targetSquare.index % 8;
            pieceToMove.currentY = targetSquare.index / 8;
            if (pieceToMove.pieceType == Piece.PieceType.Pawn || targetSquare.occupiedPiece != null)
            {
                halfMoveCount = 0;
            }
            else
            {
                halfMoveCount++;
            }
            if (pieceToMove.pieceColor == Piece.PieceColor.Black)
            {
                fullMoveCount++;
            }
            targetSquare.occupiedPiece = pieceToMove;
            initialSquare.occupiedPiece = null;
            bool isCastlingMove = false;
            if (initialSquare.index == targetSquare.index && initialSquare.index != -1)
                isCastlingMove = true;

            MovePiece(initialSquare.index, targetSquare.index, isCastlingMove); // move in fen
            string[] fenParts = currentFen.Split(' ');
            currentFen = fenParts[0] + " " + fenParts[1] + " " + fenParts[2] + " " + fenParts[3] + " " + halfMoveCount.ToString() + " " + fullMoveCount.ToString();

            if (isPromotionMove)
            {
                if (targetSquare.occupiedPiece != null)
                {
                    // change occupied piece from pawn to the selected piece
                    Destroy(targetSquare.occupiedPiece.gameObject);
                    targetSquare.occupiedPiece = null;
                    Debug.Log("Stockfish want to promote to " + pieceToPromoteWith);
                    Debug.Log("Current color to move: " + currentFen.Split()[1]);
                    Piece.PieceColor pieceToPromoteWithColor = (currentFen.Split()[1] == "b") ? Piece.PieceColor.White : Piece.PieceColor.Black;
                    Piece.PieceType pieceToPromoteWithType = char.ToLower(pieceToPromoteWith) switch // Convert to lowercase to handle case-insensitivity
                    {
                        'q' => Piece.PieceType.Queen,
                        'r' => Piece.PieceType.Rook,
                        'b' => Piece.PieceType.Bishop,
                        'n' => Piece.PieceType.Knight,
                        _ => Piece.PieceType.Queen,// Default to Queen if input is invalid
                    };
                    GameObject promotionPiecePrefab = GetPrefab(pieceToPromoteWithType, pieceToPromoteWithColor);
                    PlacePiece(promotionPiecePrefab, targetSquare.index, pieceToPromoteWithColor, pieceToPromoteWithType, isWhitePlayedByHuman, isBlackPlayedByHuman);
                    UpdateFenAfterPromotion(targetSquare.index, pieceToPromoteWithColor, pieceToPromoteWithType);
                }
            }
        }

        // castling move
        else
        {
            int kingTargetSquareIndex = 0;
            int rookTargetSquareIndex = 0;
            int originalRookIndex = 0;

            bool isWhiteCastling = true;

            Piece whiteKing = FindSquareByIndex(4).occupiedPiece;
            Piece blackKing = FindSquareByIndex(60).occupiedPiece;
            switch (bestMove.Item1)
            {
                case 0:
                    kingTargetSquareIndex = 6;
                    rookTargetSquareIndex = 5;
                    originalRookIndex = 7;
                    break;
                case 1:
                    kingTargetSquareIndex = 2;
                    rookTargetSquareIndex = 3;
                    originalRookIndex = 0;
                    break;
                case 2:
                    kingTargetSquareIndex = 62;
                    rookTargetSquareIndex = 61;
                    originalRookIndex = 63;
                    isWhiteCastling = false;
                    break;
                case 3:
                    kingTargetSquareIndex = 58;
                    rookTargetSquareIndex = 59;
                    originalRookIndex = 56;
                    isWhiteCastling = false;
                    break;
            }
            string[] fenParts = currentFen.Split(' ');
            string castlingRights = fenParts[2];
            if (isWhiteCastling)
            {
                whiteKing.transform.position = FindSquareByIndex(kingTargetSquareIndex).transform.position;
                FindSquareByIndex(kingTargetSquareIndex).occupiedPiece = whiteKing;
                FindSquareByIndex(4).occupiedPiece = null;
                MovePiece(4, kingTargetSquareIndex, true);
                castlingRights = castlingRights.Replace("K", "").Replace("Q", "");
            }
            else
            {
                blackKing.transform.position = FindSquareByIndex(kingTargetSquareIndex).transform.position;
                FindSquareByIndex(kingTargetSquareIndex).occupiedPiece = blackKing;
                FindSquareByIndex(60).occupiedPiece = null;
                MovePiece(60, kingTargetSquareIndex, true);
                castlingRights = castlingRights.Replace("q", "").Replace("q", "");
            }
            if (castlingRights == "")
            {
                castlingRights = "-";
            }
            FindSquareByIndex(originalRookIndex).occupiedPiece.transform.position = FindSquareByIndex(rookTargetSquareIndex).transform.position;
            MovePiece(originalRookIndex, rookTargetSquareIndex, true);
            FindSquareByIndex(rookTargetSquareIndex).occupiedPiece = FindSquareByIndex(originalRookIndex).occupiedPiece;
            FindSquareByIndex(originalRookIndex).occupiedPiece = null;

            fenParts[0] = currentFen.Split(' ')[0];
            string sideToMove = fenParts[1];

            if (sideToMove == "w")
            {
                sideToMove = "b";
            }
            else
            {
                sideToMove = "w";
            }


            gameManager.isWhiteToMove = !gameManager.isWhiteToMove;
            currentFen = fenParts[0] + " " + sideToMove + " " + castlingRights + " " + fenParts[3] + " " + fenParts[4] + " " + fenParts[5];
            Debug.Log("Stockfish castled! Current fen: " + currentFen);
        }
    }

    public Piece.PieceType SelectPromotionPiece()
    {
        return Piece.PieceType.Queen;
    }

}
