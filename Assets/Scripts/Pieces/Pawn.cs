using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Lumin;

public class Pawn : Piece
{
    Bitboard myPawns;
    Bitboard enemyPieces;
    Bitboard friendlyPieces;
    public override void Start()
    {
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        myPawns = boardManager.gameManager.isWhiteToMove ? boardManager.whitePawns : boardManager.blackPawns;
        enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
        friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
        // get bitboard
        if (canDrag && originalSquare != null)
        {
            myPawns.RemoveAtIndex(originalSquare.index);
            boardManager.UpdatePiecesBitboards();
            findLegalSquares();

            // check if the square is in legal squares
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (canDrag && targetSquare != null)
        {
            myPawns.AddAtIndex(targetSquare.index);
            myPawns.PrintBitboardInRowsAndColumns();
            boardManager.UpdatePiecesBitboards();
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
            myPawns = boardManager.gameManager.isWhiteToMove ? boardManager.whitePawns : boardManager.blackPawns;
            enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
            friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
            //findLegalSquares();
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
        ulong possibleCaptureNorthEast = 0;
        ulong possibleCaptureNorthWest = 0;
        if (canDrag && originalSquare != null)
        {
            int originalIndex = originalSquare.index;
            Square targetSquareByIndex;

            if (boardManager.gameManager.isWhiteToMove)
            {
                targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 8);

                if ((friendlyPieces.GetBitboard() & 1UL << (originalIndex + 8)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex + 8)) == 0)
                {
                    boardManager.HighlightSquare(targetSquareByIndex);
                }

                if (originalIndex > 7 && originalIndex < 16 && (friendlyPieces.GetBitboard() & 1UL << (originalIndex + 16)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex + 16)) == 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 16);
                    boardManager.HighlightSquare(targetSquareByIndex);

                }
                // check for captures;
                possibleCaptureNorthEast = 1UL << (originalIndex + 9);
                possibleCaptureNorthWest = 1UL << (originalIndex + 7);

                if ((enemyPieces.GetBitboard() & possibleCaptureNorthEast) != 0 && ((originalIndex+1) % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 9);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }
                if ((enemyPieces.GetBitboard() & possibleCaptureNorthWest) != 0 && (originalIndex % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex + 7);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }

            }
            else
            {
                targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 8);
                if ((friendlyPieces.GetBitboard() & 1UL << (originalIndex - 8)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex - 8)) == 0)
                {
                    boardManager.HighlightSquare(targetSquareByIndex);
                }

                if (originalIndex > 47 && originalIndex < 56 && (friendlyPieces.GetBitboard() & 1UL << (originalIndex - 16)) == 0 && (enemyPieces.GetBitboard() & 1UL << (originalIndex - 16)) == 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 16);
                    boardManager.HighlightSquare(targetSquareByIndex);

                }
                // check for captures;
                possibleCaptureNorthEast = 1UL << (originalIndex - 9);
                possibleCaptureNorthWest = 1UL << (originalIndex - 7);

                if ((enemyPieces.GetBitboard() & possibleCaptureNorthEast) != 0 && (originalIndex % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 9);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }
                if ((enemyPieces.GetBitboard() & possibleCaptureNorthWest) != 0 && ((originalIndex+1) % 8) != 0)
                {
                    targetSquareByIndex = boardManager.gameManager.FindSquareByIndex(originalIndex - 7);
                    boardManager.HighlightSquare(targetSquareByIndex);
                }

            }
        }
    }
}
