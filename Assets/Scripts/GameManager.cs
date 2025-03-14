using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
    public GameObject NoConnectionPopup;
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
        // ✅ Check if API server is connected
        StartCoroutine(CheckServerAvailability(isConnected =>
        {
            if (!isConnected)
            {
                ShowNoConnectionPopup();
                return;
            }

            // ✅ Server is connected — continue initializing
            GameObject[] allSquareObjects = GameObject.FindGameObjectsWithTag("Square");
            piecesLayer = LayerMask.NameToLayer("Pieces");
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
                    squareDictionary[square.index] = square;
                }
            }

            Debug.Log("Game initialized successfully.");
        }));
    }

    public void ShowNoConnectionPopup()
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
        NoConnectionPopup.SetActive(true);
    }

    public void RetryConnection()
    {
        Debug.Log("Retrying connection...");
        StartCoroutine(CheckServerAvailability(isConnected =>
        {
            if (!isConnected)
            {
                Debug.Log("Still no connection.");
            }
            else
            {
                Debug.Log("Server connected after retry. Restarting...");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }));
    }


    IEnumerator CheckServerAvailability(System.Action<bool> onResult)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:8000/health");
        request.timeout = 5;

        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.Log("Server not reachable: " + request.error);
            onResult?.Invoke(false);
        }
        else
        {
            Debug.Log("Server is reachable!");
            onResult?.Invoke(true);
        }
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
        NoConnectionPopup.SetActive(false);
        HidePawnPromotionPopup();
        HideGameOver();
    }

}
