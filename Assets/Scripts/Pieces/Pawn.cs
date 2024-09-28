using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pawn : Piece
{
    Bitboard myPawns;
    public override void Start()
    {
        base.Start();
        myPawns = boardManager.gameManager.isWhiteToMove ? boardManager.whitePawns : boardManager.blackPawns;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard
        if (canDrag && originalSquare != null)
        {
            myPawns.RemoveAtIndex(originalSquare.index);
            findLegalSquares();

            // check if the square is in legal squares
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (canDrag)
        {
            myPawns.AddAtIndex(targetSquare.index);
            findLegalSquares();
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (canDrag)
        {
            findLegalSquares();
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (canDrag)
        {
            findLegalSquares();
        }
    }


    public void findLegalSquares()
    {
        if (canDrag && originalSquare != null)
        {
            int originalIndex = originalSquare.index;
            Square targetSquareByIndex;

            if (boardManager.gameManager.isWhiteToMove)
            {
                targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 8);
                boardManager.HighlightSquare(targetSquareByIndex);
                if (originalIndex > 7 && originalIndex < 16)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 16);
                    boardManager.HighlightSquare(targetSquareByIndex);

                }
            }
            else
            {
                targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 8);
                boardManager.HighlightSquare(targetSquareByIndex);
                if (originalIndex > 47 && originalIndex < 56)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 16);
                    boardManager.HighlightSquare(targetSquareByIndex);

                }
            }
        }
    }
}
