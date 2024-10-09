using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class King : Piece
{
    Bitboard myKing;
    Bitboard friendlyPieces;
    Bitboard enemyPieces;


    public bool canShortCastle = true;
    public bool canLongCastle = true;
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
            canShortCastle = false;
            canLongCastle = false;
            // check if the king is castling

            if (boardManager.gameManager.isWhiteToMove)
            {
                if (targetSquare.index == originalSquare.index + 2)
                {

                    Rook myShortCastleRook = boardManager.gameManager.FindSquareByIndex(7).occupiedPiece as Rook;
                    Square shortCastleRookSquare = boardManager.gameManager.FindSquareByIndex(5);
                    Bitboard myRooks = boardManager.gameManager.isWhiteToMove ? boardManager.whiteRooks : boardManager.blackRooks;
                    myShortCastleRook.transform.position = shortCastleRookSquare.transform.position;
                    shortCastleRookSquare.occupiedPiece = myShortCastleRook;
                    boardManager.gameManager.FindSquareByIndex(7).occupiedPiece = null;
                    myRooks.RemoveAtIndex(7);
                    myRooks.AddAtIndex(5);
                }
                else if (targetSquare.index == originalSquare.index - 2)
                {

                    Rook myLongCastleRook = boardManager.gameManager.FindSquareByIndex(0).occupiedPiece as Rook;
                    Square longCastleRookSquare = boardManager.gameManager.FindSquareByIndex(3);
                    Bitboard myRooks = boardManager.gameManager.isWhiteToMove ? boardManager.whiteRooks : boardManager.blackRooks;
                    myLongCastleRook.transform.position = longCastleRookSquare.transform.position;
                    longCastleRookSquare.occupiedPiece = myLongCastleRook;
                    boardManager.gameManager.FindSquareByIndex(0).occupiedPiece = null;
                    myRooks.RemoveAtIndex(0);
                    myRooks.AddAtIndex(3);
                }

            }
            else
            {
                if (targetSquare.index == originalSquare.index + 2)
                {

                    Rook myShortCastleRook = boardManager.gameManager.FindSquareByIndex(63).occupiedPiece as Rook;
                    Square shortCastleRookSquare = boardManager.gameManager.FindSquareByIndex(61);
                    Bitboard myRooks = boardManager.gameManager.isWhiteToMove ? boardManager.whiteRooks : boardManager.blackRooks;
                    myShortCastleRook.transform.position = shortCastleRookSquare.transform.position;
                    shortCastleRookSquare.occupiedPiece = myShortCastleRook;
                    boardManager.gameManager.FindSquareByIndex(63).occupiedPiece = null;
                    myRooks.RemoveAtIndex(63);
                    myRooks.AddAtIndex(61);
                }
                else if (targetSquare.index == originalSquare.index - 2)
                {
                    Rook myLongCastleRook = boardManager.gameManager.FindSquareByIndex(56).occupiedPiece as Rook;
                    Square longCastleRookSquare = boardManager.gameManager.FindSquareByIndex(59);
                    Bitboard myRooks = boardManager.gameManager.isWhiteToMove ? boardManager.whiteRooks : boardManager.blackRooks;
                    myLongCastleRook.transform.position = longCastleRookSquare.transform.position;
                    longCastleRookSquare.occupiedPiece = myLongCastleRook;
                    boardManager.gameManager.FindSquareByIndex(56).occupiedPiece = null;
                    myRooks.RemoveAtIndex(56);
                    myRooks.AddAtIndex(59);
                }

            }

            boardManager.UpdatePiecesBitboards();
            boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
        }
    }
    public void findLegalSquares()
    {
        Bitboard possibleMoves = new Bitboard();
        if (canDrag && originalSquare != null)
        {
            possibleMoves.SetBitboard(boardManager.kingAttackTable[originalSquare.index] & ~friendlyPieces.GetBitboard());
            List<int> possibleSquaresIndices = possibleMoves.GetIndicesOfPieces();
            foreach (int index in possibleSquaresIndices)
            {
                Square possibleTargetSquare = boardManager.gameManager.FindSquareByIndex(index);
                boardManager.HighlightSquare(possibleTargetSquare);
            }

            // find castling square
            Square shortCastleKingSquare;
            Square longCastleKingSquare;
            Square shortCastleRookSquare;
            Square longCastleRookSquare;
            if (boardManager.gameManager.isWhiteToMove && originalSquare.index == 4)
            {
                shortCastleKingSquare = boardManager.gameManager.FindSquareByIndex(6);
                longCastleKingSquare = boardManager.gameManager.FindSquareByIndex(2);
                shortCastleRookSquare = boardManager.gameManager.FindSquareByIndex(5);
                longCastleRookSquare = boardManager.gameManager.FindSquareByIndex(3);

                if (canShortCastle && shortCastleKingSquare.occupiedPiece == null && shortCastleRookSquare.occupiedPiece == null)
                {
                    boardManager.HighlightSquare(shortCastleKingSquare);
                }

                if (canLongCastle && longCastleKingSquare.occupiedPiece == null && longCastleRookSquare.occupiedPiece == null)
                {
                    boardManager.HighlightSquare(longCastleKingSquare);
                }

            }
            else if (!boardManager.gameManager.isWhiteToMove && originalSquare.index == 60)
            {
                shortCastleKingSquare = boardManager.gameManager.FindSquareByIndex(62);

                longCastleKingSquare = boardManager.gameManager.FindSquareByIndex(58);

                shortCastleRookSquare = boardManager.gameManager.FindSquareByIndex(61);

                longCastleRookSquare = boardManager.gameManager.FindSquareByIndex(59);

                if (canShortCastle && shortCastleKingSquare.occupiedPiece == null && shortCastleRookSquare.occupiedPiece == null)
                {
                    boardManager.HighlightSquare(shortCastleKingSquare);
                }

                if (canLongCastle && longCastleKingSquare.occupiedPiece == null && longCastleRookSquare.occupiedPiece == null)
                {
                    boardManager.HighlightSquare(longCastleKingSquare);
                }
            }


        }
    }

}
