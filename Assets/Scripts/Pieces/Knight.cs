using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.EventSystems;

public class Knight : Piece
{
    Bitboard myKnights;
    Bitboard enemyPieces;
    Bitboard friendlyPieces;
    public override void Start()
    {
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard

        myKnights = boardManager.gameManager.isWhiteToMove ? boardManager.whiteKnights : boardManager.blackKnights;
        enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
        friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
        // get bitboard
        if (canDrag && originalSquare.occupiedPiece != null)
        {
            myKnights.RemoveAtIndex(originalSquare.index);
            boardManager.UpdatePiecesBitboards();
            findLegalSquares();
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

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (canDrag && targetSquare != null)
        {
            myKnights.AddAtIndex(targetSquare.index);
            boardManager.UpdatePiecesBitboards();
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
        }
    }

    public void findLegalSquares()
    {
        Bitboard possibleMoves = new Bitboard();
        if (canDrag && originalSquare != null)
        {
            possibleMoves.SetBitboard(generateKnightMovementMask() & ~friendlyPieces.GetBitboard());
            List<int> possibleSquaresIndices = possibleMoves.GetIndicesOfPieces();
            foreach (int index in possibleSquaresIndices)
            {
                targetSquare = boardManager.gameManager.FindSquareByIndex(index);
                boardManager.HighlightSquare(targetSquare);
            }


        }
    }

    public ulong generateKnightMovementMask()
    {
        int knightIndex = originalSquare.index;
        ulong knightMask = 1UL << knightIndex;
        ulong FILE_AB_MASK = boardManager.FILE_A_MASK | boardManager.FILE_B_MASK;
        ulong FILE_GH_MASK = boardManager.FILE_G_MASK | boardManager.FILE_H_MASK;

        // movement of knight for horizonatally with mask over at the edge of the board

        ulong attackNorthNorthWest = (knightMask & ~boardManager.FILE_A_MASK) << 15;  // 2 up, 1 left
        ulong attackNorthNorthEast = (knightMask & ~boardManager.FILE_H_MASK) << 17;  // 2 up 1 right
        ulong attackSouthSouthhEast = (knightMask & ~boardManager.FILE_H_MASK) >> 15; // 2 down, 1 right
        ulong attackSouthSouthhWest = (knightMask & ~boardManager.FILE_A_MASK) >> 17; // 2 down, 1 left

        // movement of knight vertically with mask over at the edge of the board

        ulong attackWestWestNorth = (knightMask & ~FILE_AB_MASK) << 6;  // 2 left, 1 up
        ulong attackWestWestSouth = (knightMask & ~FILE_AB_MASK) >> 10; // 2 left, 1 down
        ulong attackEastEastNorth = (knightMask & ~FILE_GH_MASK) << 10; // 2 right, 1 up
        ulong attackEastEastSouth = (knightMask & ~FILE_GH_MASK) >> 6;  // 2 right, 1 down

        return attackNorthNorthWest | attackNorthNorthEast | attackSouthSouthhEast | attackSouthSouthhWest | attackWestWestNorth | attackWestWestSouth | attackEastEastNorth | attackEastEastSouth;
    }
}
