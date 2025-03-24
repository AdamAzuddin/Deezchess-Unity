using UnityEngine;
using UnityEngine.UI;

public class PlayVsAIAsWhite : MonoBehaviour
{
    public Button playMultiplayerButton;
    public Button playVsAIAsBlackButton;
    public BoardManager boardManager;
    public void OnClick()
    {
        boardManager.gameManager.ShowPGNUploaderPopup();
        boardManager.gameManager.isVsAIAsWhite = true;
        boardManager.gameManager.isVsAIAsBlack = false;
    }
}
