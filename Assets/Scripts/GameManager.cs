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
    public GameObject resultTextGameObject;
    private Text resultText;

    // Start is called before the first frame update
    private Dictionary<int, Square> squareDictionary;

    void Start()
    {
        // Assuming you have a way to access all squares
        GameObject[] allSquareObjects = GameObject.FindGameObjectsWithTag("Square");
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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        HideGameOver();
    }

}
