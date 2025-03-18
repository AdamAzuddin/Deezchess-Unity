using System;
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

                    if (square != null && square.occupiedPiece == this &&
    ((square.occupiedPiece.pieceColor == PieceColor.White && boardManager.gameManager.isWhiteToMove) ||
    (square.occupiedPiece.pieceColor == PieceColor.Black && !boardManager.gameManager.isWhiteToMove)))
                    {
                        originalSquare = square;
                        int[] possibleLegalMoveIndices = boardManager.GetLegalMovesFromIndex(originalSquare.index, boardManager.currentFen);
                        foreach (int index in possibleLegalMoveIndices)
                        {
                            Debug.Log(index);
                            Square squareToHighlight = boardManager.FindSquareByIndex(index);
                            if (squareToHighlight != null)
                            {
                                boardManager.HighlightSquare(squareToHighlight);
                            }
                        }
                        canDrag = true;
                        /*
                        Debug.Log("Sending request to the API...");
                        StartCoroutine(boardManager.chessAPI.GetLegalMovesFromIndex(square.index, boardManager.currentFen, (possibleLegalMoveIndices) =>
                        {
                            Debug.Log("Current piece index: " + originalSquare.index);
                            Debug.Log("Possible legal moves indices:\n");

                            foreach (int index in possibleLegalMoveIndices)
                            {
                                Debug.Log(index);
                                Square squareToHighlight = boardManager.FindSquareByIndex(index);
                                if (squareToHighlight != null)
                                {
                                    boardManager.HighlightSquare(squareToHighlight);
                                }
                            }

                            canDrag = true;
                        }));*/
                    }

                    else
                    {
                        if (square == null)
                        {
                            Debug.Log("Square is null");
                        }
                        if (square.occupiedPiece != this)
                        {
                            Debug.Log("Mismatch between occupied piece and this piece");
                        }
                        if (square.occupiedPiece.pieceColor != PieceColor.White && boardManager.gameManager.isWhiteToMove || square.occupiedPiece.pieceColor != PieceColor.Black && !boardManager.gameManager.isWhiteToMove)
                        {

                            Debug.Log("This colour is not suppose to move now");
                            if (boardManager.gameManager.isWhiteToMove)
                            {
                                Debug.Log("White to move!");
                            }
                            else
                            {
                                Debug.Log("Black to move");
                            }
                        }
                        Debug.Log("Occupied piece type: " + square.occupiedPiece.pieceType);
                        Debug.Log("Occupied piece color: " + square.occupiedPiece.pieceColor);
                        canDrag = false;
                    }
                    originalSquare = square;
                    break;
                }
            }
        }
        else
        {
            Debug.Log("This piece is not draggable");
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
                            if (dropOnSquare.occupiedPiece != null && (dropOnSquare.color == Color.red || dropOnSquare.spriteRenderer.color == Color.red) && dropOnSquare.occupiedPiece != this)
                            {
                                Destroy(dropOnSquare.occupiedPiece.gameObject);
                            }
                            transform.position = dropOnSquare.transform.position;
                            currentX = dropOnSquare.index % 8;
                            currentY = dropOnSquare.index / 8;
                            dropOnSquare.occupiedPiece = this;
                            dropOnSquare.OnDrop(eventData);
                            targetSquare = dropOnSquare;
                            boardManager.MovePiece(originalSquare.index, targetSquare.index, false);
                            if (originalSquare != null)
                            {
                                originalSquare.occupiedPiece = null;
                            }
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
                        if (otherPiece != null && otherPiece != this && ((boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.White) || (!boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.Black)))
                        {
                            int idx = GetSquareIndex(otherPiece.currentX, otherPiece.currentY);
                            Square otherPieceSquare = boardManager.FindSquareByIndex(idx);

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

                                        square.OnDrop(eventData);
                                        targetSquare = square;
                                        boardManager.MovePiece(originalSquare.index, targetSquare.index, false);
                                        if (originalSquare != null)
                                        {
                                            originalSquare.occupiedPiece = null;
                                        }
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
                int moveCount = boardManager.GetNumberOfLegalMoves(boardManager.currentFen);
                if (moveCount == 0)
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
                    boardManager.EngineMove(boardManager.currentFen, boardManager.searchDepth, halfMoveCount, fullMoveCount);
                    Debug.Log("Fen after stockfish move: " + boardManager.currentFen);
                    //Check if its game over
                    if (halfMoveCount == 50)
                    {
                        boardManager.gameManager.ShowGameOver("It's a tie by 50 move rule");
                    }
                    int engineMoveCount = boardManager.GetNumberOfLegalMoves(boardManager.currentFen);
                    if (engineMoveCount == 0)
                    {
                        boardManager.gameManager.ShowGameOver("You Lose!");
                    }
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


    public int GetSquareIndex(int currentX, int currentY)
    {
        return (currentY * 8) + currentX;
    }



}
