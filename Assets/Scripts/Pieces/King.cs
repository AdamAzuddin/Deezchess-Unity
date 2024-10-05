using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class King : Piece
{
    Bitboard myKing;
    Bitboard friendlyPieces;
    Bitboard enemyPieces;
    public override void Start()
    {
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard

        myKing = boardManager.gameManager.isWhiteToMove ? boardManager.whiteKing : boardManager.blackKing;
        friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
        enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
        // get bitboard
        if (canDrag && originalSquare.occupiedPiece != null)
        {
            myKing.RemoveAtIndex(originalSquare.index);
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
            myKing.AddAtIndex(targetSquare.index);
            boardManager.UpdatePiecesBitboards();
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
        }
    }

    public void PrintBitboardInGrid(ulong bitboard)
    {
        for (int rank = 7; rank >= 0; rank--) // Start from rank 7 (top of the board) to rank 0
        {
            string line = "";
            for (int file = 0; file < 8; file++)
            {
                int square = rank * 8 + file; // Calculate square index
                line += ((bitboard & (1UL << square)) != 0) ? "1 " : "0 "; // Print 1 for set bits, 0 for unset bits
            }
            Debug.Log(line); // Print each rank
        }
    }

    public void PrintKingAttackTable()
    {
        for (int i = 0; i < boardManager.kingAttackTable.Length; i++)
        {
            Debug.Log($"King attack mask for square {i}:");
            PrintBitboardInGrid(boardManager.kingAttackTable[i]);
            Debug.Log("------------------------");
        }
    }


    public void findLegalSquares()
    {
        Bitboard possibleMoves = new Bitboard();
        if (canDrag && originalSquare != null)
        {
            PrintKingAttackTable();
            possibleMoves.SetBitboard(boardManager.kingAttackTable[originalSquare.index] & ~friendlyPieces.GetBitboard());
            List<int> possibleSquaresIndices = possibleMoves.GetIndicesOfPieces();
            foreach (int index in possibleSquaresIndices)
            {
                Square possibleTargetSquare = boardManager.gameManager.FindSquareByIndex(index);
                boardManager.HighlightSquare(possibleTargetSquare);
            }


        }
    }

}
