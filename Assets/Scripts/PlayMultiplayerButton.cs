using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMultiplayerButton : MonoBehaviour
{
    public Button playVsAIAsWhiteButton;
    public Button playVsAIAsBlackButton;
    public Button playMultiplayerButton;
    public BoardManager boardManager;
    public void OnClick()
    {
        playVsAIAsBlackButton.interactable = false;
        playVsAIAsWhiteButton.interactable = false;
        playMultiplayerButton.interactable = false;
        boardManager.PlacePieces();
    }
}
