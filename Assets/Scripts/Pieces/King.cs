using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
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
        // check for castling move by checking the difference between target square and original square is >1
        if (canDrag)
        {
            string[] fenParts = boardManager.currentFen.Split(' ');
            string castlingRights = fenParts[2];
            int distanceBetweenTargetAndOriginalIndex = targetSquare.index - originalSquare.index;
            Piece rookToCastleWith;
            Square castledRookTargetSquare;
            if (Mathf.Abs(distanceBetweenTargetAndOriginalIndex) == 2)
            {
                if (pieceColor == PieceColor.White)
                {
                    if (distanceBetweenTargetAndOriginalIndex > 0)
                    {
                        // Kingside castling for White
                        if (castlingRights.Contains("K"))
                        {
                            rookToCastleWith = FindSquareByIndex(7).occupiedPiece;
                            castledRookTargetSquare = FindSquareByIndex(5);
                            rookToCastleWith.transform.position = castledRookTargetSquare.transform.position;
                            boardManager.MovePiece(7, 5, true);
                            castlingRights = castlingRights.Replace("Q", "");
                            castlingRights = castlingRights.Replace("K", "");
                        }
                    }
                    else if (distanceBetweenTargetAndOriginalIndex < 0)
                    {
                        // Queenside castling for White
                        if (castlingRights.Contains("Q"))
                        {
                            rookToCastleWith = FindSquareByIndex(0).occupiedPiece;
                            castledRookTargetSquare = FindSquareByIndex(3);
                            rookToCastleWith.transform.position = castledRookTargetSquare.transform.position;
                            boardManager.MovePiece(0, 3, true);
                            castlingRights = castlingRights.Replace("Q", "");
                            castlingRights = castlingRights.Replace("K", "");
                        }
                    }
                }
                else if (pieceColor == PieceColor.Black)
                {
                    if (distanceBetweenTargetAndOriginalIndex > 0)
                    {
                        // Kingside castling for Black
                        if (castlingRights.Contains("k"))
                        {
                            rookToCastleWith = FindSquareByIndex(63).occupiedPiece;
                            castledRookTargetSquare = FindSquareByIndex(61);
                            rookToCastleWith.transform.position = castledRookTargetSquare.transform.position;
                            boardManager.MovePiece(63, 61, true);
                            castlingRights = castlingRights.Replace("k", "");
                            castlingRights = castlingRights.Replace("q", "");

                        }
                    }
                    else if (distanceBetweenTargetAndOriginalIndex < 0)
                    {
                        // Queenside castling for Black
                        if (castlingRights.Contains("q"))
                        {
                            rookToCastleWith = FindSquareByIndex(56).occupiedPiece;
                            castledRookTargetSquare = FindSquareByIndex(59);
                            rookToCastleWith.transform.position = castledRookTargetSquare.transform.position;
                            boardManager.MovePiece(56, 59, true);
                            castlingRights = castlingRights.Replace("q", "");
                            castlingRights = castlingRights.Replace("k", "");
                        }
                    }
                }
            }
            else
            {
                if (pieceColor == PieceColor.White && (castlingRights.Contains("K") || castlingRights.Contains("Q")))
                {
                    castlingRights = castlingRights.Replace("K", "");
                    castlingRights = castlingRights.Replace("Q", "");
                }

                else if (pieceColor == PieceColor.Black && (castlingRights.Contains("k") || castlingRights.Contains("q")))
                {
                    castlingRights = castlingRights.Replace("k", "");
                    castlingRights = castlingRights.Replace("q", "");
                }
            }
            fenParts = boardManager.currentFen.Split(' ');
            if (castlingRights == "")
            {
                castlingRights = "-";
            }
            boardManager.currentFen = fenParts[0] + " " + fenParts[1] + " " + castlingRights + " " + fenParts[3] + " " + fenParts[4] + " " + fenParts[5];
            Debug.Log("Fen after castling: " + boardManager.currentFen);
        }
    }

}
