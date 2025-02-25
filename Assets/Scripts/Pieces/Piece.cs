using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    private readonly int depth = 10;

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
                            if (dropOnSquare.occupiedPiece != null)
                            {
                                Destroy(dropOnSquare.occupiedPiece.gameObject);
                            }
                            transform.position = dropOnSquare.transform.position;

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
                            transform.position = originalSquare.transform.position;
                            targetSquare = null;
                            hasMoved = false;
                            break;
                        }
                    }
                    else if (result.gameObject.CompareTag("Piece") && result.gameObject != gameObject)
                    {
                        Piece otherPiece = result.gameObject.GetComponent<Piece>();
                        if (otherPiece != null && ((boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.White) || (!boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.Black)))
                        {
                            Destroy(otherPiece.gameObject);
                            Debug.Log("Other piece deleted");

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
                                        transform.position = originalSquare.transform.position;
                                        targetSquare = null;
                                        hasMoved = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            transform.position = originalSquare.transform.position;
                            targetSquare = null;
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

                // check if next move piece color is played by computer or human
                if (boardManager.gameManager.isWhiteToMove && !boardManager.isWhitePlayedByHuman || !boardManager.gameManager.isWhiteToMove && !boardManager.isBlackPlayedByHuman)
                {
                    // find the best move using stockfish
                    var bestMove = engine.GetBestMove(boardManager.currentFen, depth);

                    Debug.Log($"From: {bestMove.Item1}, To: {bestMove.Item2}");
                    Square initialSquare = FindSquareByIndex(bestMove.Item1);
                    Square targetSquare = FindSquareByIndex(bestMove.Item2);
                    // actually move the piece
                    Piece pieceToMove = initialSquare.occupiedPiece;
                    if (targetSquare.occupiedPiece != null)
                    {
                        Destroy(targetSquare.occupiedPiece.gameObject);
                    }
                    pieceToMove.transform.position = targetSquare.transform.position;
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
                    fenParts = boardManager.currentFen.Split(' ');
                    boardManager.currentFen = fenParts[0] + " " + fenParts[1] + " " + fenParts[2] + " " + fenParts[3] + " " + halfMoveCount.ToString() + " " + fullMoveCount.ToString();

                    // reset half move if its a capture 
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

            }
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
}
