using UnityEngine;
using UnityEngine.EventSystems;

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
                Square enPassantSquare = boardManager.FindSquareByIndex(enPassantSquareIndex);
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
        if (hasMoved)
        {
            PieceColor enemyColor = pieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            string[] fenParts = boardManager.currentFen.Split(' ');
            int direction = pieceColor == PieceColor.White ? -8 : 8;
            Square enPassantCapturedSquare = boardManager.gameManager.GetSquareByIndex(targetSquare.index + direction);

            // Capture en passant if the square contains an enemy pawn
            if (enPassantCapturedSquare.occupiedPiece != null &&
                enPassantCapturedSquare.occupiedPiece.pieceType == PieceType.Pawn &&
                enPassantCapturedSquare.occupiedPiece.pieceColor == enemyColor && fenParts[3] != "-")
            {
                Destroy(enPassantCapturedSquare.occupiedPiece.gameObject);
                Debug.Log("Captured en passantly");
                enPassantCapturedSquare.occupiedPiece = null;
            }

            boardManager.currentFen = fenParts[0] + " " + fenParts[1] + " " + fenParts[2] + " " + fenParts[3] + " 0 " + fenParts[5];
            Debug.Log("Fen string after resetted half move: " + boardManager.currentFen);
        }
        else
        {
            transform.position = originalSquare.transform.position;
        }

        if (pieceColor == PieceColor.White && (originalSquare.index - 1) / 8 == 6)
        {
            boardManager.gameManager.pawnPromotionSquareIndex = targetSquare.index;
            boardManager.gameManager.ShowPawnPromotionPopup(PieceColor.White);
        }
        else if (pieceColor == PieceColor.Black && (originalSquare.index - 1) / 8 == 1)
        {
            boardManager.gameManager.pawnPromotionSquareIndex = targetSquare.index;
            boardManager.gameManager.ShowPawnPromotionPopup(PieceColor.Black);
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
