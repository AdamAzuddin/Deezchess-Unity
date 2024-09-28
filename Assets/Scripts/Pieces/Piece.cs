using System.Collections;
using System.Collections.Generic;
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

    public virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find the BoardManager by its tag
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

    // Update is called once per frame
    void Update()
    {
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // Capture the offset between the piece's position and the mouse position
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;
        offset = transform.position - mouseWorldPos;

        // Perform a raycast to determine the square where the piece is currently located
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
                    Debug.Log("Original square recorded: " + originalSquare.name);
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


    // Called when dragging starts
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (canDrag)
        {

            Debug.Log("Drag started on");
            canvasGroup.alpha = .8f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.Log("It's not your time");
        }
    }

    // Called during dragging
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (canDrag)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            mouseWorldPos.z = 0;
            transform.position = mouseWorldPos + offset;
        }
    }

    // Called when dragging ends
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (canDrag)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // Check where the piece was dropped
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            Square dropOnSquare = null; // Ensure it is defined

            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.CompareTag("Square"))
                {
                    dropOnSquare = result.gameObject.GetComponent<Square>();
                    if (dropOnSquare != null)
                    {

                        // Check if there is already a piece on the target square
                        if (dropOnSquare.occupiedPiece != null && dropOnSquare.occupiedPiece != this)
                        {
                            Debug.Log("Destroying piece currently in this square");
                            Destroy(dropOnSquare.occupiedPiece.gameObject);
                        }

                        // Snap to the target square's position
                        transform.position = dropOnSquare.transform.position;

                        // Update occupied pieces
                        dropOnSquare.occupiedPiece = this;
                        if (originalSquare != null)
                        {
                            originalSquare.occupiedPiece = null;
                        }

                        dropOnSquare.OnDrop(eventData);
                        targetSquare = dropOnSquare;
                        originalSquare = null;
                        break;
                    }
                }
                else if (result.gameObject.CompareTag("Piece") && result.gameObject != gameObject)
                {
                    Piece otherPiece = result.gameObject.GetComponent<Piece>();
                    if (otherPiece != null)
                    {
                        Debug.Log("Another piece found: " + otherPiece.pieceType);
                        Destroy(otherPiece.gameObject);

                        List<RaycastResult> raycastSquare = new List<RaycastResult>();
                        EventSystem.current.RaycastAll(pointerData, raycastSquare);

                        foreach (RaycastResult squareResult in raycastSquare)
                        {
                            if (squareResult.gameObject.CompareTag("Square"))
                            {
                                Square square = squareResult.gameObject.GetComponent<Square>();
                                if (square != null)
                                {
                                    Debug.Log("Snapping piece to square: " + square.name);

                                    // Snap the piece to the center of this square
                                    transform.position = square.transform.position;

                                    square.occupiedPiece = this;
                                    if (originalSquare != null)
                                    {
                                        originalSquare.occupiedPiece = null; // Avoid accessing null
                                    }
                                    square.OnDrop(eventData);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
            foreach (Square sq in boardManager.highlightedSquares)
            {
                sq.spriteRenderer.color = sq.color;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Piece clicked");
        // original square already  stored in on mouse down event
    }

}
