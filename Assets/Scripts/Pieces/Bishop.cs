using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bishop : Piece
{
    Bitboard myBishops;
    public override void Start()
    {
        base.Start();
        myBishops = boardManager.gameManager.isWhiteToMove ? boardManager.whiteBishops : boardManager.blackBishops;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard
        if (canDrag && originalSquare != null)
        {
            myBishops.RemoveAtIndex(originalSquare.index);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        myBishops.AddAtIndex(targetSquare.index);
    }
}
