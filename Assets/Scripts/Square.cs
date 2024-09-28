using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Square : MonoBehaviour, IDropHandler
{
    public SpriteRenderer spriteRenderer;

    public Color color;

    public Color onSelectColor;

    public Piece occupiedPiece;

    public int index;

    private BoardManager boardManager;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("On square drop");
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boardManager = FindObjectOfType<BoardManager>();

        if (boardManager == null)
        {
            Debug.LogError("BoardManager not found in the scene!");
        }
    }

    // This method is called when the square is clicked
    /* void OnMouseDown()
    {
        Debug.Log($"isWhiteToMove: {boardManager.gameManager.isWhiteToMove}");
        if (occupiedPiece && !boardManager.isSelectPieceToMove && ((occupiedPiece.pieceColor == Piece.PieceColor.White && boardManager.gameManager.isWhiteToMove) ||
         (occupiedPiece.pieceColor == Piece.PieceColor.Black && !boardManager.gameManager.isWhiteToMove)))
        {
            boardManager.SelectPiece(this);
            boardManager.currentState = BoardManager.BoardState.SelectingPiece;
            // show legal moves
        }
        else if (occupiedPiece && occupiedPiece == boardManager.pieceToMove)
        {
            boardManager.pieceToMoveSquare.spriteRenderer.color = color;
            boardManager.DeselectPiece();
            boardManager.currentState = BoardManager.BoardState.Waiting;
        }
        else if (occupiedPiece && occupiedPiece != boardManager.pieceToMove)
        {
            boardManager.pieceToMoveSquare.spriteRenderer.color = color;
            boardManager.DeselectPiece();
            boardManager.SelectPiece(this);
            boardManager.currentState = BoardManager.BoardState.Waiting;
        }
        else if (!occupiedPiece && boardManager.isSelectPieceToMove)
        {
            GameObject pieceGameObject = boardManager.pieceToMove.gameObject;
            boardManager.movePieceTo(pieceGameObject, index);
            boardManager.DeselectPiece();
            Destroy(pieceGameObject);
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
            boardManager.currentState = BoardManager.BoardState.Waiting;
        }

    } */
}
