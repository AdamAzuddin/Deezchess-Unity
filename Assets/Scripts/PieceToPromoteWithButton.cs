
using Unity.VisualScripting;
using UnityEngine;

public class PieceToPromoteWithButton : MonoBehaviour
{
    public BoardManager boardManager;
    public Piece.PieceType pieceType;
    public Piece.PieceColor pieceColor;

    public void OnClick()
    {
        Square promotionSquare = boardManager.gameManager.GetSquareByIndex(boardManager.gameManager.pawnPromotionSquareIndex);
        Debug.Log("You're trying to promote a pawn at index " + promotionSquare + " to " + pieceColor + " " + pieceType);
        if (promotionSquare.occupiedPiece != null)
        {
            // change occupied piece from pawn to the selected piece
            Debug.Log(promotionSquare.occupiedPiece);
            Destroy(promotionSquare.occupiedPiece.gameObject);
            promotionSquare.occupiedPiece = null;
            GameObject promotionPiecePrefab = boardManager.GetPrefab(pieceType, pieceColor);
            boardManager.PlacePiece(promotionPiecePrefab, promotionSquare.index, pieceColor, pieceType, boardManager.isWhitePlayedByHuman, boardManager.isBlackPlayedByHuman);
            boardManager.UpdateFenAfterPromotion(promotionSquare.index, pieceColor, pieceType);
            //boardManager.gameManager.isWhiteToMove = !boardManager.gameManager.isWhiteToMove;
        }
        boardManager.gameManager.HidePawnPromotionPopup();
    }

}
