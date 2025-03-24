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
        string castlingRights;
        base.OnEndDrag(eventData);
        if (canDrag && hasMoved)
        {
            // Check for castling move
            string[] fenParts = boardManager.currentFen.Split(' ');
            castlingRights = fenParts[2];
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
                rookToCastleWith = boardManager.FindSquareByIndex(rookOriginalIndex).occupiedPiece;
                castledRookTargetSquare = boardManager.FindSquareByIndex(rookCastledIndex);
                rookOriginalSquare = boardManager.FindSquareByIndex(rookOriginalIndex);
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
                boardManager.EngineMove(boardManager.currentFen, halfMoveCount, fullMoveCount);
            }
        }

        if (hasMoved)
        {
            // Remove castling rights if any
            string[] fenSplits = boardManager.currentFen.Split();
            castlingRights = fenSplits[2];
            if (pieceColor == PieceColor.White)
            {
                if (castlingRights.Contains('K') || castlingRights.Contains('Q'))
                {
                    castlingRights = castlingRights.Replace("K", "").Replace("Q", "");
                }
            }
            else
            {
                if (castlingRights.Contains('k') || castlingRights.Contains('q'))
                {
                    castlingRights = castlingRights.Replace("k", "").Replace("q", "");
                }
            }

            if (castlingRights == "")
            {
                castlingRights = "-";
            }

            boardManager.currentFen = fenSplits[0] + " " + fenSplits[1] + " " + castlingRights + " " + fenSplits[3] + " " + fenSplits[4] + " " + fenSplits[5];
        }
    }

}
