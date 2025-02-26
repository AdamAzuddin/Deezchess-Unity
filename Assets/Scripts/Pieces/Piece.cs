using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.XR.Interaction;

public class Piece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    public enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King }
    public enum PieceColor { White, Black }

    public PieceType pieceType;
    public PieceColor pieceColor;
    public bool isDraggable;
    protected SpriteRenderer spriteRenderer;
    protected Vector3 offset;
    public Vector2Int currentPos;
    protected Camera mainCamera;

    public int currentX;
    public int currentY;
    protected BoardManager boardManager;
    protected CanvasGroup canvasGroup;
    protected Square originalSquare;
    protected Square targetSquare;
    protected bool canDrag;
    readonly StockfishEngine engine = new StockfishEngine();
    protected readonly int depth = 10;

    public bool hasMoved = false;

    public virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject boardManagerObject = GameObject.FindGameObjectWithTag("BoardManager");

        if (boardManagerObject != null)
        {
            boardManager = boardManagerObject.GetComponent<BoardManager>();

            if (boardManager == null)
            {
                Debug.Log("The BoardManager script was not found on the object with the 'BoardManager' tag.");
            }
        }
        else
        {
            Debug.Log("No GameObject found with the 'BoardManager' tag!");
        }
        mainCamera = Camera.main;
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (isDraggable)
        {

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            mouseWorldPos.z = 0;
            offset = transform.position - mouseWorldPos;

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.CompareTag("Square"))
                {
                    Square square = result.gameObject.GetComponent<Square>();
                    if (square != null && square.occupiedPiece == this && (square.occupiedPiece.pieceColor == PieceColor.White && boardManager.gameManager.isWhiteToMove || square.occupiedPiece.pieceColor == PieceColor.Black && !boardManager.gameManager.isWhiteToMove))
                    {
                        originalSquare = square;
                        List<int> possibleLegalMoveIndices = boardManager.GetLegalMovesFromIndex(square.index);

                        foreach (int index in possibleLegalMoveIndices)
                        {
                            Square squareToHighlight = FindSquareByIndex(index);
                            if (squareToHighlight != null)
                            {
                                boardManager.HighlightSquare(squareToHighlight);
                            }
                        }

                        canDrag = true;
                    }
                    else
                    {
                        canDrag = false;
                    }
                    break;
                }
            }
        }
    }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (isDraggable)
        {

            if (canDrag)
            {
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                Debug.Log("It's not your time");
            }
        }
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (isDraggable)
        {
            if (canDrag)
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
                mouseWorldPos.z = 0;
                transform.position = mouseWorldPos + offset;
            }
        }
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (isDraggable)
        {

            string[] fenParts = boardManager.currentFen.Split(' ');

            int halfMoveCount = int.Parse(fenParts[4]);
            int fullMoveCount = int.Parse(fenParts[5]);
            if (canDrag)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Input.mousePosition;

                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, raycastResults);

                Square dropOnSquare = null;

                foreach (RaycastResult result in raycastResults)
                {
                    if (result.gameObject.CompareTag("Square"))
                    {
                        dropOnSquare = result.gameObject.GetComponent<Square>();

                        if (dropOnSquare != null && dropOnSquare.color != dropOnSquare.spriteRenderer.color)
                        {
                            if (dropOnSquare.occupiedPiece != null && (dropOnSquare.color == Color.red || dropOnSquare.spriteRenderer.color == Color.red))
                            {
                                Destroy(dropOnSquare.occupiedPiece.gameObject);
                            }
                            transform.position = dropOnSquare.transform.position;
                            currentX = dropOnSquare.index % 8;
                            currentY = dropOnSquare.index / 8;

                            dropOnSquare.occupiedPiece = this;
                            if (originalSquare != null)
                            {
                                originalSquare.occupiedPiece = null;
                            }

                            dropOnSquare.OnDrop(eventData);
                            targetSquare = dropOnSquare;
                            boardManager.MovePiece(originalSquare.index, targetSquare.index, false);
                            if (pieceColor == PieceColor.Black)
                            {
                                fullMoveCount++;
                            }
                            hasMoved = true;
                            halfMoveCount++;
                            break;
                        }

                        else
                        {
                            hasMoved = false;
                            break;
                        }
                    }
                    else if (result.gameObject.CompareTag("Piece") && result.gameObject != gameObject)
                    {
                        Piece otherPiece = result.gameObject.GetComponent<Piece>();
                        if (otherPiece != null && ((boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.White) || (!boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.Black)))
                        {
                            int idx = GetSquareIndex(otherPiece.currentX, otherPiece.currentY);
                            Square otherPieceSquare = FindSquareByIndex(idx);

                            for (int i = 0; i < boardManager.highlightedSquares.Count; i++)
                            {
                                if (boardManager.highlightedSquares[i].index == idx)
                                {
                                    Destroy(otherPiece.gameObject);
                                    Debug.Log("Deleted " + otherPiece.pieceColor + " " + otherPiece.pieceType + " at index " + idx);
                                }

                            }
                            List<RaycastResult> raycastSquare = new List<RaycastResult>();
                            EventSystem.current.RaycastAll(pointerData, raycastSquare);

                            foreach (RaycastResult squareResult in raycastSquare)
                            {
                                if (squareResult.gameObject.CompareTag("Square"))
                                {
                                    Square square = squareResult.gameObject.GetComponent<Square>();
                                    if (square != null && square.color != square.spriteRenderer.color)
                                    {
                                        transform.position = square.transform.position;
                                        currentX = square.index % 8;
                                        currentY = square.index / 8;
                                        square.occupiedPiece = this;
                                        if (originalSquare != null)
                                        {
                                            originalSquare.occupiedPiece = null;
                                        }
                                        square.OnDrop(eventData);
                                        targetSquare = square;
                                        boardManager.MovePiece(originalSquare.index, targetSquare.index, false);
                                        if (pieceColor == PieceColor.Black)
                                        {
                                            fullMoveCount++;
                                        }
                                        hasMoved = true;
                                        halfMoveCount = 0;
                                        break;
                                    }
                                    else
                                    {
                                        hasMoved = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            hasMoved = false;
                            break;
                        }

                    }

                }
                foreach (Square sq in boardManager.highlightedSquares)
                {
                    sq.spriteRenderer.color = sq.color;
                }
                boardManager.highlightedSquares.Clear();
                fenParts = boardManager.currentFen.Split(' ');
                boardManager.currentFen = fenParts[0] + " " + fenParts[1] + " " + fenParts[2] + " " + fenParts[3] + " " + halfMoveCount.ToString() + " " + fullMoveCount.ToString();
                if (pieceType != PieceType.Pawn)
                {
                    Debug.Log("Fen string after  updated full and half move: " + boardManager.currentFen);
                }
                if (hasMoved)
                {
                    boardManager.AddFen(boardManager.fenOccurences, fenParts[0] + fenParts[2]);
                }

                
                if (halfMoveCount == 50)
                {
                    boardManager.gameManager.ShowGameOver("It's a tie by 50 move rule");
                }
                if (boardManager.GetNumberOfLegalMoves(boardManager.currentFen) == 0)
                {
                    if (fenParts[1] == "w")
                    {
                        boardManager.gameManager.ShowGameOver("Black win");
                    }
                    else if (fenParts[1] == "b")
                    {
                        boardManager.gameManager.ShowGameOver("White win");
                    }
                }

                // check if next move piece color is played by computer or human
                if ((boardManager.gameManager.isWhiteToMove && !boardManager.isWhitePlayedByHuman || !boardManager.gameManager.isWhiteToMove && !boardManager.isBlackPlayedByHuman) && pieceType != PieceType.King && Math.Abs(originalSquare.index - targetSquare.index) != 2)
                {
                    EngineMove(boardManager.currentFen, depth, halfMoveCount, fullMoveCount);

                    Debug.Log("Fen after stockfish move: " + boardManager.currentFen);
                    //Check if its game over
                    if (halfMoveCount == 50)
                    {
                        boardManager.gameManager.ShowGameOver("It's a tie by 50 move rule");
                    }
                    if (boardManager.GetNumberOfLegalMoves(boardManager.currentFen) == 0)
                    {
                        boardManager.gameManager.ShowGameOver("You Lose!");
                    }
                    // reset half move if its a capture 
                }

            }
        }
        if (!hasMoved)
        {
            transform.position = originalSquare.transform.position;
            currentX = originalSquare.index % 8;
            currentY = originalSquare.index / 8;
            targetSquare = null;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Piece clicked");
    }

    public Square FindSquareByIndex(int targetIndex)
    {
        return boardManager.gameManager.GetSquareByIndex(targetIndex);
    }

    public int GetSquareIndex(int currentX, int currentY)
    {
        return (currentY * 8) + currentX;
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
        string castlingRights = boardManager.currentFen.Split(" ")[2];

        int fromIndex = UciSquareToBitboardIndex(fromSquare);
        int toIndex = UciSquareToBitboardIndex(toSquare);

        // Castling logic

        Square whiteKingSquare = FindSquareByIndex(4);
        Square blackKingSquare = FindSquareByIndex(60);

        if (whiteKingSquare.occupiedPiece != null && whiteKingSquare.occupiedPiece.pieceType == PieceType.King && whiteKingSquare.occupiedPiece.pieceColor == PieceColor.White && boardManager.gameManager.isWhiteToMove)
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
        else if (blackKingSquare.occupiedPiece != null && blackKingSquare.occupiedPiece.pieceType == PieceType.King && blackKingSquare.occupiedPiece.pieceColor == PieceColor.Black && !boardManager.gameManager.isWhiteToMove)
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

    public static int UciSquareToBitboardIndex(string square)
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

    public void EngineMove(string fen, int depth, int halfMoveCount, int fullMoveCount)
    {
        // find the best move using stockfish
        var bestMove = UciMoveToBitboardIndices(engine.GetBestMove(fen, depth));

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
            hasMoved = true;
            if (pieceToMove.pieceType == PieceType.Pawn || targetSquare.occupiedPiece != null)
            {
                halfMoveCount = 0;
            }
            else
            {
                halfMoveCount++;
            }
            if (pieceToMove.pieceColor == PieceColor.Black)
            {
                fullMoveCount++;
            }
            targetSquare.occupiedPiece = pieceToMove;
            initialSquare.occupiedPiece = null;
            bool isCastlingMove = false;
            if (initialSquare.index == targetSquare.index && initialSquare.index != -1)
                isCastlingMove = true;

            boardManager.MovePiece(initialSquare.index, targetSquare.index, isCastlingMove); // move in fen
            string[] fenParts = boardManager.currentFen.Split(' ');
            boardManager.currentFen = fenParts[0] + " " + fenParts[1] + " " + fenParts[2] + " " + fenParts[3] + " " + halfMoveCount.ToString() + " " + fullMoveCount.ToString();
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
            string[] fenParts = boardManager.currentFen.Split(' ');
            string castlingRights = fenParts[2];
            if (isWhiteCastling)
            {
                whiteKing.transform.position = FindSquareByIndex(kingTargetSquareIndex).transform.position;
                FindSquareByIndex(kingTargetSquareIndex).occupiedPiece = whiteKing;
                FindSquareByIndex(4).occupiedPiece = null;
                boardManager.MovePiece(4, kingTargetSquareIndex, true);
                castlingRights = castlingRights.Replace("K", "").Replace("Q", "");
            }
            else
            {
                blackKing.transform.position = FindSquareByIndex(kingTargetSquareIndex).transform.position;
                FindSquareByIndex(kingTargetSquareIndex).occupiedPiece = blackKing;
                FindSquareByIndex(60).occupiedPiece = null;
                boardManager.MovePiece(60, kingTargetSquareIndex, true);
                castlingRights = castlingRights.Replace("q", "").Replace("q", "");
            }
            if (castlingRights == "")
            {
                castlingRights = "-";
            }
            FindSquareByIndex(originalRookIndex).occupiedPiece.transform.position = FindSquareByIndex(rookTargetSquareIndex).transform.position;
            boardManager.MovePiece(originalRookIndex, rookTargetSquareIndex, true);
            FindSquareByIndex(rookTargetSquareIndex).occupiedPiece = FindSquareByIndex(originalRookIndex).occupiedPiece;
            FindSquareByIndex(originalRookIndex).occupiedPiece = null;

            fenParts[0] = boardManager.currentFen.Split(' ')[0];
            string sideToMove = fenParts[1];

            if (sideToMove == "w")
            {
                sideToMove = "b";
            }
            else
            {
                sideToMove = "w";
            }


            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
            boardManager.currentFen = fenParts[0] + " " + sideToMove + " " + castlingRights + " " + fenParts[3] + " " + fenParts[4] + " " + fenParts[5];
            Debug.Log("Stockfish castled! Current fen: " + boardManager.currentFen);

        }
    }

}
