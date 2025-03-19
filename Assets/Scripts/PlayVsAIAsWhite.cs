using UnityEngine;
using UnityEngine.UI;

public class PlayVsAIAsWhite : MonoBehaviour
{
    public Button playMultiplayerButton;
    public Button playVsAIAsBlackButton;
    public BoardManager boardManager;
    void Start()
    {


    }

    public void OnClick()
    {
        boardManager.gameManager.ShowPGNUploaderPopup();
        /*
        playVsAIAsBlackButton.interactable = false;
        playMultiplayerButton.interactable = false;
        boardManager.PlacePieces(true, false);*/
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
