using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Lumin;
using System.Runtime.InteropServices;
using System;


public class Pawn : Piece
{
    public override void Start()
    {
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (canDrag && originalSquare != null)
        {
            string[] fenParts = boardManager.currentFen.Split(' ');
            if (fenParts[3] != "-")
            {
                int enPassantSquareIndex = boardManager.UciSquareToBitboardIndex(fenParts[3]);
                Debug.Log(fenParts[2]);
                Square enPassantSquare = FindSquareByIndex(enPassantSquareIndex);
                int diffBetweenCurrentIndexAndenPassantIndex = Mathf.Abs(originalSquare.index - enPassantSquareIndex);
                if (diffBetweenCurrentIndexAndenPassantIndex > 6 && diffBetweenCurrentIndexAndenPassantIndex < 10 && enPassantSquare != null)
                {
                    boardManager.HighlightSquare(enPassantSquare);
                }
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        PieceColor enemyColor = pieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
        
        int direction = pieceColor == PieceColor.White ? -8 : 8;
        Square enPassantCapturedSquare = boardManager.gameManager.GetSquareByIndex(targetSquare.index + direction);

        // Capture en passant if the square contains an enemy pawn
        if (enPassantCapturedSquare.occupiedPiece != null &&
            enPassantCapturedSquare.occupiedPiece.pieceType == PieceType.Pawn &&
            enPassantCapturedSquare.occupiedPiece.pieceColor == enemyColor)
        {
            Destroy(enPassantCapturedSquare.occupiedPiece.gameObject);
            enPassantCapturedSquare.occupiedPiece = null;
        }

    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
    }
}
