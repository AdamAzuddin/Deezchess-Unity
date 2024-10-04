using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bishop : Piece
{
    Bitboard myBishops;
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

        myBishops = boardManager.gameManager.isWhiteToMove ? boardManager.whiteBishops : boardManager.blackBishops;
        friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
        enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
        // get bitboard
        if (canDrag && originalSquare.occupiedPiece != null)
        {
            myBishops.RemoveAtIndex(originalSquare.index);
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
            myBishops.AddAtIndex(targetSquare.index);
            boardManager.UpdatePiecesBitboards();
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
        }
    }

    public void findLegalSquares()
    {
        Bitboard possibleMoves = new Bitboard();
        if (canDrag && originalSquare != null)
        {
            possibleMoves.SetBitboard(GenerateBishopMovementMask() & ~friendlyPieces.GetBitboard());
            List<int> possibleSquaresIndices = possibleMoves.GetIndicesOfPieces();
            foreach (int index in possibleSquaresIndices)
            {
                targetSquare = boardManager.gameManager.FindSquareByIndex(index);
                boardManager.HighlightSquare(targetSquare);
            }


        }
    }

    private ulong GenerateTopHalfDiagonal(int index, int shift)
    {
        ulong currentMask;
        int currentIndex = index;
        Bitboard result = new Bitboard();
        result.SetBitboard(0);

        ulong leftRightTopEdgeMask = boardManager.FILE_A_MASK | boardManager.FILE_H_MASK | boardManager.RANK_8_MASK;

        while (true)
        {
            currentIndex += shift;
            currentMask = 1UL << currentIndex;

            if ((currentMask & friendlyPieces.GetBitboard()) != 0 || currentIndex > 63)
            {
                break;
            }

            if ((currentMask & leftRightTopEdgeMask) != 0 || (currentMask & enemyPieces.GetBitboard()) != 0)
            {
                result.SetBitboard(result.GetBitboard() | currentMask);
                break;
            }

            if (currentIndex < 0)
                break;

            result.SetBitboard(result.GetBitboard() | currentMask);
        }

        return result.GetBitboard();
    }

    private ulong GenerateBottomHalfDiagonal(int index, int shift)
    {
        ulong currentMask;
        int currentIndex = index;
        Bitboard result = new Bitboard();
        result.SetBitboard(0);

        ulong leftRightBottomEdgeMask = boardManager.FILE_A_MASK | boardManager.FILE_H_MASK | boardManager.RANK_1_MASK;

        while (true)
        {
            currentIndex -= shift;
            currentMask = 1UL << currentIndex;

            if ((currentMask & friendlyPieces.GetBitboard()) != 0 || currentIndex < 0)
            {
                break;
            }

            if ((currentMask & leftRightBottomEdgeMask) != 0 || (currentMask & enemyPieces.GetBitboard()) != 0)
            {
                result.SetBitboard(result.GetBitboard() | currentMask);
                break;
            }

            if (currentIndex < 0)
                break;

            result.SetBitboard(result.GetBitboard() | currentMask);
        }

        return result.GetBitboard();
    }

    private ulong GenerateBishopMovementMask()
    {
        Bitboard movements = new Bitboard();

        // Create mask for positive gradient diagonal
        movements.SetBitboard(GenerateBottomHalfDiagonal(originalSquare.index, 7));
        movements.SetBitboard(movements.GetBitboard() | GenerateBottomHalfDiagonal(originalSquare.index, 9));
        movements.SetBitboard(movements.GetBitboard() | GenerateTopHalfDiagonal(originalSquare.index, 9));
        movements.SetBitboard(movements.GetBitboard() | GenerateTopHalfDiagonal(originalSquare.index, 7));

        // Return the movement mask
        return movements.GetBitboard();
    }

}
