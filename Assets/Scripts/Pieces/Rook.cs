using UnityEngine.EventSystems;
using UnityEngine;

public class Rook : Piece
{
    public override void Start()
    {
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        string[] fenParts = boardManager.currentFen.Split(' ');
        string castlingRights = fenParts[2];
        if (canDrag)
        {
            if (pieceColor == PieceColor.White)
            {
                if (originalSquare.index == 7 && castlingRights.Contains("K"))
                {
                    castlingRights = castlingRights.Replace("K", "");
                }
                else if (originalSquare.index == 0 && castlingRights.Contains("Q"))
                {
                    castlingRights = castlingRights.Replace("Q", "");
                }
            }
            else if (pieceColor == PieceColor.Black)
            {
                if (originalSquare.index == 63 && castlingRights.Contains("k"))
                {
                    castlingRights = castlingRights.Replace("k", "");
                }
                else if (originalSquare.index == 56 && castlingRights.Contains("q"))
                {
                    castlingRights = castlingRights.Replace("q", "");
                }
            }

            fenParts = boardManager.currentFen.Split(' ');
            if (castlingRights == "")
            {
                castlingRights = "-";
            }
            boardManager.currentFen = fenParts[0] + " " + fenParts[1] + " " + castlingRights + " " + fenParts[3] + " " + fenParts[4] + " " + fenParts[5];
            Debug.Log("Fen after moving rook that can be used for castling: " + boardManager.currentFen);
        }
    }
}
