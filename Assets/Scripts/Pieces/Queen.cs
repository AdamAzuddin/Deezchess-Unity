using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Queen : Piece
{
    Bitboard myQueens;
    public override void Start()
    {
        base.Start();
        myQueens = boardManager.gameManager.isWhiteToMove ? boardManager.whiteQueens : boardManager.blackQueens;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard
        if (canDrag && originalSquare != null)
        {
            myQueens.RemoveAtIndex(originalSquare.index);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        myQueens.AddAtIndex(targetSquare.index);
    }
}
