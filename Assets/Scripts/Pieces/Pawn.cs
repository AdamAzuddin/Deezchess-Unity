using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pawn : Piece
{
    Bitboard myPawns;
    bool isEnPassant = false;
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

            // check if the square is in legal squares
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        myPawns.AddAtIndex(targetSquare.index);

    }


    public override List<Square> findLegalSquares()
    {
        base.findLegalSquares();
        Debug.Log("Finding legal squares to move to");
        int originalIndex = originalSquare.index;
        Square targetSquareByIndex;

        if (boardManager.gameManager.isWhiteToMove)
        {
            if (originalIndex > 7 && originalIndex < 16)
            {
                for (int i = originalIndex; i < 2; i++)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(i + 8);
                    boardManager.HighlightSquare(targetSquareByIndex);
                    Debug.Log(targetSquareByIndex.index);
                    legalSquares.Add(targetSquareByIndex);
                }

            } else{
                targetSquareByIndex =boardManager.gameManager.FindSquareByIndex(originalIndex + 8);
                boardManager.HighlightSquare(targetSquareByIndex);
                legalSquares.Add(targetSquareByIndex);
            }
        }
        return legalSquares;
    }
}
