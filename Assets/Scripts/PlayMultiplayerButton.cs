using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMultiplayerButton : MonoBehaviour
{
    public Button playVsAIAsWhiteButton;
    public Button playVsAIAsBlackButton;
    public BoardManager boardManager;
    void Start()
    {


    }

    public void OnClick()
    {
        playVsAIAsBlackButton.interactable = false;
        playVsAIAsWhiteButton.interactable = false;
        boardManager.PlacePieces();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
