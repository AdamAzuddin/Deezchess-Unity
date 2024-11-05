using UnityEngine;

public class Board : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public GameObject square;
    public Color lightColor;
    public Color darkColor;
    void Start()
    {
        //initializeGame();
    }

    void Update()
    {

    }


    void initializeGame()
    {
        int index = 0;
        for (int col = 1; col <= 8; col++)
        {
            for (int row = 1; row <= 8; row++)
            {
                GameObject newSquare = Instantiate(square, new Vector3(row, col, 0), transform.rotation);
                // Access the SpriteRenderer of the instantiated square
                spriteRenderer = newSquare.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = newSquare.AddComponent<SpriteRenderer>();
                    spriteRenderer.sortingOrder = 1;
                }


                // Set the color based on the row and column values
                if ((row + col) % 2 == 0)
                {
                    spriteRenderer.color = lightColor;
                }
                else
                {
                    spriteRenderer.color = darkColor;
                }
                newSquare.transform.parent = transform;

                newSquare.name = $"Square_{col}_{row}";

                Square squareComponent = newSquare.GetComponent<Square>();
                if (squareComponent == null)
                {
                    squareComponent = newSquare.AddComponent<Square>();
                }
                squareComponent.index = index;
                squareComponent.color = spriteRenderer.color;
                squareComponent.occupiedPiece = null;

                index++;
            }
        }
    }
}
