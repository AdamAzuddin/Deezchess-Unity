using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class King : Piece
{
    Bitboard myKing;
    public override void Start()
    {
        base.Start();
        myKing = boardManager.gameManager.isWhiteToMove ? boardManager.whiteKing : boardManager.blackKing;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard
        if (canDrag && originalSquare != null)
        {
            myKing.RemoveAtIndex(originalSquare.index);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        myKing.AddAtIndex(targetSquare.index);
    }
}
