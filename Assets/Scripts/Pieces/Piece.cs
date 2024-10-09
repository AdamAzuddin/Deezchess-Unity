using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    public enum PieceType { Pawn, Rook, Knight, Bishop, Queen, King }
    public enum PieceColor { White, Black }

    public PieceType pieceType;
    public PieceColor pieceColor;

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
    protected Pawn[] allPawns;


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


        allPawns = FindObjectsOfType<Pawn>();
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
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
    public virtual void OnBeginDrag(PointerEventData eventData)
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
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (canDrag)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            mouseWorldPos.z = 0;
            transform.position = mouseWorldPos + offset;
        }
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
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
                        transform.position = dropOnSquare.transform.position;

                        dropOnSquare.occupiedPiece = this;
                        if (originalSquare != null)
                        {
                            originalSquare.occupiedPiece = null;
                        }

                        dropOnSquare.OnDrop(eventData);
                        targetSquare = dropOnSquare;
                        break;
                    }


                    else
                    {
                        transform.position = originalSquare.transform.position;
                        targetSquare = null;
                        break;
                    }
                }
                else if (result.gameObject.CompareTag("Piece") && result.gameObject != gameObject)
                {
                    Piece otherPiece = result.gameObject.GetComponent<Piece>();
                    if (otherPiece != null && ((boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.White) || (!boardManager.gameManager.isWhiteToMove && otherPiece.pieceColor != PieceColor.Black)))
                    {
                        Bitboard piecesToBeTakenBitboard = new Bitboard();

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

                                    switch (otherPiece.pieceType)
                                    {
                                        case PieceType.Pawn:
                                            piecesToBeTakenBitboard = boardManager.gameManager.isWhiteToMove ? boardManager.blackPawns : boardManager.whitePawns;
                                            break;
                                        case PieceType.Rook:
                                            piecesToBeTakenBitboard = boardManager.gameManager.isWhiteToMove ? boardManager.blackRooks : boardManager.whiteRooks;
                                            break;
                                        case PieceType.Knight:
                                            piecesToBeTakenBitboard = boardManager.gameManager.isWhiteToMove ? boardManager.blackKnights : boardManager.whiteKnights;
                                            break;
                                        case PieceType.Bishop:
                                            piecesToBeTakenBitboard = boardManager.gameManager.isWhiteToMove ? boardManager.blackBishops : boardManager.whiteBishops;
                                            break;
                                        case PieceType.Queen:
                                            piecesToBeTakenBitboard = boardManager.gameManager.isWhiteToMove ? boardManager.blackQueens : boardManager.whiteQueens;
                                            break;
                                        case PieceType.King:
                                            piecesToBeTakenBitboard = boardManager.gameManager.isWhiteToMove ? boardManager.blackKing : boardManager.whiteKing;
                                            break;
                                    }
                                    piecesToBeTakenBitboard.RemoveAtIndex(square.index);
                                    boardManager.UpdatePiecesBitboards();
                                    transform.position = square.transform.position;
                                    square.occupiedPiece = this;
                                    if (originalSquare != null)
                                    {
                                        originalSquare.occupiedPiece = null;
                                    }
                                    square.OnDrop(eventData);
                                    targetSquare = square;
                                    break;
                                }
                                else
                                {
                                    transform.position = originalSquare.transform.position;
                                    targetSquare = null;
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
            
            PieceColor myPieceColor = boardManager.gameManager.isWhiteToMove ? PieceColor.White : PieceColor.Black;
            
            // find all pawns of my color and make sure it cant en passant anymore
            Pawn[] myPawns = allPawns.Where(pawn => pawn.pieceColor == myPieceColor).ToArray();

            foreach (Pawn pawn in myPawns)
            {
                if (pawn.isEnPassantable)
                {
                    pawn.isEnPassantable = false;
                }
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Piece clicked");
    }

}
