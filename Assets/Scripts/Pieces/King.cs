using UnityEngine;
using UnityEngine.EventSystems;

public class King : Piece
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
        if (canDrag)
        {
            string[] fenParts = boardManager.currentFen.Split(' ');
            string castlingRights = fenParts[2];
            int distanceBetweenTargetAndOriginalIndex = targetSquare.index - originalSquare.index;
            int rookOriginalIndex = 0;
            int rookCastledIndex = 0;
            Piece rookToCastleWith;
            Square castledRookTargetSquare;
            Square rookOriginalSquare;
            if (Mathf.Abs(distanceBetweenTargetAndOriginalIndex) == 2)
            {
                if (pieceColor == PieceColor.White)
                {
                    if (distanceBetweenTargetAndOriginalIndex > 0)
                    {
                        // Kingside castling for White
                        if (castlingRights.Contains("K"))
                        {
                            rookOriginalIndex = 7;
                            rookCastledIndex = 5;
                        }
                    }
                    else
                    {
                        // Queenside castling for White
                        if (castlingRights.Contains("Q"))
                        {
                            rookOriginalIndex = 0;
                            rookCastledIndex = 3;
                        }
                    }
                    castlingRights = castlingRights.Replace("Q", "");
                    castlingRights = castlingRights.Replace("K", "");
                }
                else if (pieceColor == PieceColor.Black)
                {
                    if (distanceBetweenTargetAndOriginalIndex > 0)
                    {
                        // Kingside castling for Black
                        if (castlingRights.Contains("k"))
                        {
                            rookOriginalIndex = 63;
                            rookCastledIndex = 61;
                        }
                    }
                    else
                    {
                        // Queenside castling for Black
                        if (castlingRights.Contains("q"))
                        {
                            rookOriginalIndex = 56;
                            rookCastledIndex = 59;
                        }
                    }
                    castlingRights = castlingRights.Replace("k", "");
                    castlingRights = castlingRights.Replace("q", "");
                }
                rookToCastleWith = FindSquareByIndex(rookOriginalIndex).occupiedPiece;
                castledRookTargetSquare = FindSquareByIndex(rookCastledIndex);
                rookOriginalSquare = FindSquareByIndex(rookOriginalIndex);
                rookToCastleWith.transform.position = castledRookTargetSquare.transform.position;
                castledRookTargetSquare.occupiedPiece = rookToCastleWith;
                rookOriginalSquare.occupiedPiece = null;
                boardManager.MovePiece(rookOriginalIndex, rookCastledIndex, true);
            }

            fenParts = boardManager.currentFen.Split(' ');
            if (castlingRights == "")
            {
                castlingRights = "-";
            }

            int halfMoveCount = int.Parse(fenParts[4]);
            int fullMoveCount = int.Parse(fenParts[5]);
            boardManager.currentFen = fenParts[0] + " " + fenParts[1] + " " + castlingRights + " " + fenParts[3] + " " + fenParts[4] + " " + fenParts[5];
            Debug.Log("Fen after castling: " + boardManager.currentFen);
            if (boardManager.gameManager.isWhiteToMove && !boardManager.isWhitePlayedByHuman || !boardManager.gameManager.isWhiteToMove && !boardManager.isBlackPlayedByHuman)
            {
                EngineMove(boardManager.currentFen, depth, halfMoveCount, fullMoveCount);
            }
        }
    }

}
