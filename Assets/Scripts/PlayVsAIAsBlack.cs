using UnityEngine;
using UnityEngine.UI;

public class PlayVsAIAsBlack : MonoBehaviour
{

    public Button playVsAIAsWhiteButton;
    public Button playMultiplayerButton;
    public BoardManager boardManager;
    public GameObject boardObject;

    public void OnClick()
    {
        boardManager.gameManager.ShowPGNUploaderPopup();
        boardManager.gameManager.isVsAIAsBlack = true;
        boardManager.gameManager.isVsAIAsWhite = false;
    }
}
