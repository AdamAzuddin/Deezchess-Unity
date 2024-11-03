using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isWhiteToMove = true;
    public bool whiteCanShortCastle = true;
    public bool whiteCanLongCastle = true;
    public bool blackCanShortCastle = true;
    public bool blackCanLongCastle = true;
    
    // Start is called before the first frame update
    private Dictionary<int, Square> squareDictionary;

    void Start()
    {
        // Assuming you have a way to access all squares
        GameObject[] allSquareObjects = GameObject.FindGameObjectsWithTag("Square");
        squareDictionary = new Dictionary<int, Square>();

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
}
