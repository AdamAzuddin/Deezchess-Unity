using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isWhiteToMove = true;
    public GameObject[] allSquares;

    
    // Start is called before the first frame update
    void Start()
    {
        allSquares  = GameObject.FindGameObjectsWithTag("Square");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Square FindSquareByIndex(int targetIndex)
    {

        // Iterate through all squares to find the one with the matching index
        foreach (GameObject squareObject in allSquares)
        {
            Square squareComponent = squareObject.GetComponent<Square>();

            // Check if the squareComponent is valid and has the desired index
            if (squareComponent != null && squareComponent.index == targetIndex) 
            {
                return squareComponent;
            }
        }

        // Return null if no square with the specified index was found
        Debug.LogError("Square with index " + targetIndex + " not found!");
        return null;
    }
}
