using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Rook : Piece
{
    Bitboard myRooks;
    public override void Start()
    {
        base.Start();
        myRooks = boardManager.gameManager.isWhiteToMove ? boardManager.whiteRooks : boardManager.blackRooks;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (canDrag && originalSquare != null)
        {
            myRooks.RemoveAtIndex(originalSquare.index);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        myRooks.AddAtIndex(targetSquare.index);
    }
}
