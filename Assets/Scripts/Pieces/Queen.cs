using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Queen : Piece
{
    Bitboard myQueens;
    Bitboard friendlyPieces;
    Bitboard enemyPieces;
    public override void Start()
    {
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        myQueens = boardManager.gameManager.isWhiteToMove ? boardManager.whiteQueens : boardManager.blackQueens;
        friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
        enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
        // get bitboard
        if (canDrag && originalSquare.occupiedPiece != null)
        {
            myQueens.RemoveAtIndex(originalSquare.index);
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
            myQueens.AddAtIndex(targetSquare.index);
            boardManager.UpdatePiecesBitboards();
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
        }
    }

    public void findLegalSquares()
    {
        Bitboard possibleMoves = new Bitboard();
        if (canDrag && originalSquare != null)
        {
            possibleMoves.SetBitboard(GenerateBishopMovementMask() | GenerateRookMovementMask() & ~friendlyPieces.GetBitboard());
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

    private ulong GenerateRookMovementMask()
    {
        Bitboard fileMask = new Bitboard();
        fileMask.SetBitboard(0);

        Bitboard rankMask = new Bitboard();
        rankMask.SetBitboard(0);

        ulong northMask, southMask, westMask, eastMask;

        int currentIndex = originalSquare.index;

        // Move north
        while (true)
        {
            currentIndex += 8;
            if (currentIndex > 63)
                break;

            northMask = 1UL << currentIndex;

            if ((northMask & myQueens.GetBitboard()) != 0)
            {
                fileMask.SetBitboard(fileMask.GetBitboard() | northMask);
                break;
            }

            if ((northMask & friendlyPieces.GetBitboard()) != 0)
                break;

            if ((northMask & enemyPieces.GetBitboard()) != 0)
            {
                fileMask.SetBitboard(fileMask.GetBitboard() | northMask);
                break;
            }

            fileMask.SetBitboard(fileMask.GetBitboard() | northMask);
        }

        currentIndex = originalSquare.index;

        // Move south
        while (true)
        {
            currentIndex -= 8;
            if (currentIndex < 0)
                break;

            southMask = 1UL << currentIndex;

            if ((southMask & myQueens.GetBitboard()) != 0)
            {
                fileMask.SetBitboard(fileMask.GetBitboard() | southMask);
                break;
            }

            if ((southMask & enemyPieces.GetBitboard()) != 0)
            {
                fileMask.SetBitboard(fileMask.GetBitboard() | southMask);
                break;
            }

            if ((southMask & friendlyPieces.GetBitboard()) != 0)
                break;

            fileMask.SetBitboard(fileMask.GetBitboard() | southMask);
        }

        currentIndex = originalSquare.index;

        // Move east
        while (true)
        {
            currentIndex++;
            if (currentIndex > 63)
                break;

            eastMask = 1UL << currentIndex;

            if ((eastMask & myQueens.GetBitboard()) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | eastMask);
                break;
            }

            if ((eastMask & boardManager.FILE_H_MASK) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | eastMask);
                break;
            }

            if ((eastMask & friendlyPieces.GetBitboard()) != 0)
                break;

            if ((eastMask & enemyPieces.GetBitboard()) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | eastMask);
                break;
            }

            rankMask.SetBitboard(rankMask.GetBitboard() | eastMask);
        }

        currentIndex = originalSquare.index;

        // Move west
        while (true)
        {
            currentIndex--;
            if (currentIndex < 0)
                break;

            westMask = 1UL << currentIndex;

            if ((westMask & myQueens.GetBitboard()) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | westMask);
                break;
            }

            if ((westMask & boardManager.FILE_A_MASK) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | westMask);
                break;
            }

            if ((westMask & friendlyPieces.GetBitboard()) != 0)
                break;

            if ((westMask & enemyPieces.GetBitboard()) != 0)
            {
                rankMask.SetBitboard(rankMask.GetBitboard() | westMask);
                break;
            }

            rankMask.SetBitboard(rankMask.GetBitboard() | westMask);
        }

        return fileMask.GetBitboard() | rankMask.GetBitboard();
    }
}
