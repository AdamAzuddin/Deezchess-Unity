using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Rook : Piece
{
    Bitboard myRooks;
    Bitboard friendlyPieces;
    Bitboard enemyPieces;
    King whiteKing;
    King blackKing;
    public override void Start()
    {
        King[] kings = FindObjectsOfType<King>();

        foreach (King king in kings)
        {
            // Check if the King is of color White
            if (king.pieceColor == PieceColor.White)
            {
                whiteKing = king;
            }
            else
            {
                blackKing = king;
            }
        }
        base.Start();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // get bitboard

        myRooks = boardManager.gameManager.isWhiteToMove ? boardManager.whiteRooks : boardManager.blackRooks;
        friendlyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.whitePieces : boardManager.blackPieces;
        enemyPieces = boardManager.gameManager.isWhiteToMove ? boardManager.blackPieces : boardManager.whitePieces;
        // get bitboard
        if (canDrag && originalSquare.occupiedPiece != null)
        {
            myRooks.RemoveAtIndex(originalSquare.index);
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
            myRooks.AddAtIndex(targetSquare.index);
            boardManager.UpdatePiecesBitboards();

            if (boardManager.gameManager.isWhiteToMove)
            {

                if (originalSquare.index == 7)
                {
                    whiteKing.canShortCastle = false;
                }
                if (originalSquare.index == 0)
                {
                    whiteKing.canLongCastle = false;
                }
            }
            else
            {
                if (originalSquare.index == 63)
                {
                    blackKing.canShortCastle = false;
                }
                if (originalSquare.index == 56)
                {
                    blackKing.canLongCastle = false;
                }
            }
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
        }
    }

    public void findLegalSquares()
    {
        Bitboard possibleMoves = new Bitboard();
        if (canDrag && originalSquare != null)
        {
            possibleMoves.SetBitboard(GenerateRookMovementMask() & ~friendlyPieces.GetBitboard());
            List<int> possibleSquaresIndices = possibleMoves.GetIndicesOfPieces();
            foreach (int index in possibleSquaresIndices)
            {
                Square possibleTargetSquare = boardManager.gameManager.FindSquareByIndex(index);
                boardManager.HighlightSquare(possibleTargetSquare);
            }


        }
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

            if ((northMask & myRooks.GetBitboard()) != 0)
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

            if ((southMask & myRooks.GetBitboard()) != 0)
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

            if ((eastMask & myRooks.GetBitboard()) != 0)
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

            if ((westMask & myRooks.GetBitboard()) != 0)
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
