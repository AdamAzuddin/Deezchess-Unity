using UnityEngine;
using UnityEngine.UI;

public class PlayVsAIAsBlack : MonoBehaviour
{

    public Button playVsAIAsWhiteButton;
    public Button playMultiplayerButton;
    public BoardManager boardManager;
    public GameObject boardObject;
    void Start()
    {

    }

    public void OnClick()
    {
        Vector3 originalPosition = boardObject.transform.position;
        playMultiplayerButton.interactable = false;
        playVsAIAsWhiteButton.interactable = false;
        boardObject.transform.Rotate(0, 0, 180);
        boardObject.transform.position = new Vector3(-originalPosition.x, -originalPosition.y, 0);
        boardManager.PlacePieces(false, true);
    }

    void Update()
    {

    }
}
