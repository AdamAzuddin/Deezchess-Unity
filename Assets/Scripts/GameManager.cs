using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool isWhiteToMove = true;
    public bool whiteCanShortCastle = true;
    public bool whiteCanLongCastle = true;
    public bool blackCanShortCastle = true;
    public bool blackCanLongCastle = true;
    public GameObject gameOverPopup;
    public GameObject pawnPromotionPopup;
    public GameObject resultTextGameObject;

    public GameObject promotionRook;
    public GameObject promotionKnight;
    public GameObject promotionBishop;
    public GameObject promotionQueen;
    public Sprite wR, wN, wB, wQ;
    public Sprite bR, bN, bB, bQ;

    public Camera mainCamera;

    private Text resultText;

    private Dictionary<int, Square> squareDictionary;
    private int piecesLayer;

    public int pawnPromotionSquareIndex;

    void Start()
    {
        GameObject[] allSquareObjects = GameObject.FindGameObjectsWithTag("Square");
        piecesLayer  = LayerMask.NameToLayer("Pieces");
        squareDictionary = new Dictionary<int, Square>();
        if (resultTextGameObject != null)
        {
            resultText = resultTextGameObject.GetComponent<Text>();
        }

        foreach (GameObject squareObject in allSquareObjects)
        {
            Square square = squareObject.GetComponent<Square>();
            if (square != null)
            {
                squareDictionary[square.index] = square; // Use square index as the key
            }
        }
    }

    void Update()
    {

    }

    public Square GetSquareByIndex(int targetIndex)
    {
        if (squareDictionary.TryGetValue(targetIndex, out Square square))
        {
            return square;
        }

        Debug.LogError("Square with index " + targetIndex + " not found!");
        return null;
    }

    public void ShowGameOver(string text)
    {
        Piece[] pieces = FindObjectsOfType<Piece>();
        foreach (Piece piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        Square[] squares = FindObjectsOfType<Square>();
        foreach (Square square in squares)
        {
            Destroy(square.gameObject);
        }
        if (resultText != null)
        {
            resultText.text = text;
        }
        gameOverPopup.SetActive(true);

    }



    public void HideGameOver()
    {
        gameOverPopup.SetActive(false);
    }

    public void ShowPawnPromotionPopup(Piece.PieceColor color)
    {

        if (color == Piece.PieceColor.White)
        {
            promotionRook.GetComponent<Button>().image.sprite = wR;
            promotionKnight.GetComponent<Button>().image.sprite = wN;
            promotionBishop.GetComponent<Button>().image.sprite = wB;
            promotionQueen.GetComponent<Button>().image.sprite = wQ;
        }
        else
        {
            promotionRook.GetComponent<Button>().image.sprite = bR;
            promotionRook.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
            promotionKnight.GetComponent<Button>().image.sprite = bN;
            promotionKnight.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
            promotionBishop.GetComponent<Button>().image.sprite = bB;
            promotionBishop.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
            promotionQueen.GetComponent<Button>().image.sprite = bQ;
            promotionQueen.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
        }

        // Show the popup
        pawnPromotionPopup.SetActive(true);

        // Hide the "Pieces" layer from the main camera
        mainCamera.cullingMask &= ~(1 << piecesLayer);
    }

    public void HidePawnPromotionPopup()
    {
        // Show the "Pieces" layer again in the main camera
        mainCamera.cullingMask |= 1 << piecesLayer;
        pawnPromotionPopup.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        HideGameOver();
    }

}
