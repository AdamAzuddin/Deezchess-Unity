using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Lumin;

public class Pawn : Piece
{
    Bitboard myPawns;
    Bitboard enemyPawns;
    Bitboard enemyPieces;
    Bitboard friendlyPieces;
    PieceColor enemyColor;

    public bool isEnPassantable = false;
    private Pawn enemyPawn;
    public override void Start()
    {
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        myPawns = boardManager.gameManager.isWhiteToMove ? boardManager.whitePawns : boardManager.blackPawns;
        enemyPawns = boardManager.gameManager.isWhiteToMove ? boardManager.blackPawns : boardManager.whitePawns;
        enemyColor = boardManager.gameManager.isWhiteToMove ? PieceColor.Black : PieceColor.White;
        enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
        friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
        // get bitboard
        if (canDrag && originalSquare != null)
        {
            myPawns.RemoveAtIndex(originalSquare.index);
            boardManager.UpdatePiecesBitboards();
            findLegalSquares();
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        int originalIndex = originalSquare.index;
        if (canDrag && targetSquare != null)
        {
            int targetIndex = targetSquare.index;
            myPawns.AddAtIndex(targetSquare.index);
            boardManager.UpdatePiecesBitboards();
            Bitboard enemyPawns = boardManager.gameManager.isWhiteToMove ? boardManager.blackPawns : boardManager.whitePawns;
            Pawn enemyPawnObject;
            if (boardManager.gameManager.isWhiteToMove)
            {
                if (originalIndex > 7 && originalIndex < 16 && targetIndex == (originalIndex + 16))
                {
                    isEnPassantable = true;
                    Debug.Log("Your pawn can be captured en passantly");
                }

                enemyPawnObject = boardManager.gameManager.FindSquareByIndex(targetSquare.index - 8).occupiedPiece as Pawn;
                if (enemyPawnObject != null && enemyPawnObject.pieceColor != PieceColor.White)
                {
                    enemyPawns.RemoveAtIndex(targetSquare.index - 8);
                    Destroy(enemyPawnObject.gameObject);
                }
            }
            else
            {
                if (originalIndex > 47 && originalIndex < 56 && targetIndex == (originalIndex - 16))
                {
                    isEnPassantable = true;
                    Debug.Log("Your pawn can be captured en passantly");
                }

                enemyPawnObject = boardManager.gameManager.FindSquareByIndex(targetSquare.index + 8).occupiedPiece as Pawn;
                if (enemyPawnObject != null && enemyPawnObject.pieceColor != PieceColor.Black)
                {
                    // we are capturing en passantly
                    enemyPawns.RemoveAtIndex(targetSquare.index - 8);
                    Destroy(enemyPawnObject.gameObject);
                }
            }
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;

            // can be captured by en passant if move two squares

        }

    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (canDrag)
        {
            findLegalSquares();
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (canDrag)
        {
            findLegalSquares();
        }
    }


    public void findLegalSquares()
    {
        if (canDrag && originalSquare != null)
        {
            int originalIndex = originalSquare.index;
            Square targetSquareByIndex;

            if (boardManager.gameManager.isWhiteToMove)
            {
                targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 8);

                // no enemy nor friendly piece on front
                if ((friendlyPieces.GetBitboard() & 1UL << (originalIndex + 8)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex + 8)) == 0)
                {
                    boardManager.HighlightSquare(targetSquareByIndex);
                    if (originalIndex > 7 && originalIndex < 16 && (friendlyPieces.GetBitboard() & 1UL << (originalIndex + 16)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex + 16)) == 0)
                    {
                        targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 16);
                        boardManager.HighlightSquare(targetSquareByIndex);
                    }
                }
                // check for captures
                ulong possibleCaptureNorthEast = 1UL << (originalIndex + 9);
                ulong possibleCaptureNorthWest = 1UL << (originalIndex + 7);

                if ((enemyPieces.GetBitboard() & possibleCaptureNorthEast) != 0 && ((originalIndex + 1) % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 9);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }

                // capture if en passantable
                if ((enemyPieces.GetBitboard() & possibleCaptureNorthEast) == 0 && (enemyPawns.GetBitboard() & 1UL << (originalIndex + 1)) != 0 && boardManager.gameManager.FindSquareByIndex(originalIndex + 1).occupiedPiece.pieceType == PieceType.Pawn && boardManager.gameManager.FindSquareByIndex(originalIndex + 1).occupiedPiece.pieceColor == enemyColor && ((originalIndex + 1) % 8) != 0)
                {
                    enemyPawn = boardManager.gameManager.FindSquareByIndex(originalIndex + 1).occupiedPiece as Pawn;
                    if (enemyPawn.pieceColor != pieceColor && enemyPawn.isEnPassantable)
                    {
                        targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 9);
                        boardManager.HighlightSquare(targetSquareByIndex);
                    }
                    else
                    {
                        Debug.Log("You can't en passant a pawn");
                    }
                }

                if ((enemyPieces.GetBitboard() & possibleCaptureNorthWest) != 0 && (originalIndex % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 7);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }

                if ((enemyPieces.GetBitboard() & possibleCaptureNorthWest) == 0 && (enemyPawns.GetBitboard() & 1UL << (originalIndex - 1)) != 0 && boardManager.gameManager.FindSquareByIndex(originalIndex - 1).occupiedPiece.pieceType == PieceType.Pawn && boardManager.gameManager.FindSquareByIndex(originalIndex - 1).occupiedPiece.pieceColor == enemyColor && (originalIndex % 8) != 0)
                {
                    enemyPawn = boardManager.gameManager.FindSquareByIndex(originalIndex - 1).occupiedPiece as Pawn;
                    if (enemyPawn.pieceColor != pieceColor && enemyPawn.isEnPassantable)
                    {
                        targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 7);
                        boardManager.HighlightSquare(targetSquareByIndex);
                    }
                    else
                    {
                        Debug.Log("You can't en passant a pawn");
                    }
                }

            }
            else
            {
                targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 8);
                if ((friendlyPieces.GetBitboard() & 1UL << (originalIndex - 8)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex - 8)) == 0)
                {
                    boardManager.HighlightSquare(targetSquareByIndex);
                    if (originalIndex > 47 && originalIndex < 56 && (friendlyPieces.GetBitboard() & 1UL << (originalIndex - 16)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex - 16)) == 0)
                    {
                        targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 16);
                        boardManager.HighlightSquare(targetSquareByIndex);
                        isEnPassantable = true;

                    }
                }


                // check for captures;
                ulong possibleCaptureSouthWest = 1UL << (originalIndex - 9);
                ulong possibleCaptureSouthEast = 1UL << (originalIndex - 7);

                if ((enemyPieces.GetBitboard() & possibleCaptureSouthWest) != 0 && (originalIndex % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 9);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }

                if ((enemyPieces.GetBitboard() & possibleCaptureSouthWest) == 0 && (enemyPawns.GetBitboard() & 1UL << (originalIndex - 1)) != 0 && boardManager.gameManager.FindSquareByIndex(originalIndex - 1).occupiedPiece.pieceType == PieceType.Pawn && boardManager.gameManager.FindSquareByIndex(originalIndex - 1).occupiedPiece.pieceColor == enemyColor && (originalIndex % 8) != 0)
                {
                    enemyPawn = boardManager.gameManager.FindSquareByIndex(originalIndex - 1).occupiedPiece as Pawn;
                    if (enemyPawn.pieceColor != pieceColor && enemyPawn.isEnPassantable)
                    {
                        targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 9);
                        boardManager.HighlightSquare(targetSquareByIndex);
                    }
                    else
                    {
                        Debug.Log("You can't en passant a pawn");
                    }
                }

                if ((enemyPieces.GetBitboard() & possibleCaptureSouthEast) != 0 && ((originalIndex + 1) % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 7);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }
                if ((enemyPieces.GetBitboard() & possibleCaptureSouthEast) == 0 && (enemyPawns.GetBitboard() & 1UL << (originalIndex + 1)) != 0 && boardManager.gameManager.FindSquareByIndex(originalIndex + 1).occupiedPiece.pieceType == PieceType.Pawn && boardManager.gameManager.FindSquareByIndex(originalIndex + 1).occupiedPiece.pieceColor == enemyColor && ((originalIndex + 1) % 8) != 0)
                {
                    enemyPawn = boardManager.gameManager.FindSquareByIndex(originalIndex + 1).occupiedPiece as Pawn;
                    if (enemyPawn.pieceColor != pieceColor && enemyPawn.isEnPassantable)
                    {
                        targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 7);
                        boardManager.HighlightSquare(targetSquareByIndex);
                    }
                    else
                    {
                        Debug.Log("You can't en passant a pawn");
                    }
                }

            }
        }
    }
}
