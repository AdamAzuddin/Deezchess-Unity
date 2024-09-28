using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Knight : Piece
{
    Bitboard myKnights;
    public override void Start()
    {
        base.Start();
        myKnights = boardManager.gameManager.isWhiteToMove ? boardManager.whiteKnights : boardManager.blackKnights;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard
        if (canDrag && originalSquare != null)
        {
            myKnights.RemoveAtIndex(originalSquare.index);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        myKnights.AddAtIndex(targetSquare.index);
    }
}
