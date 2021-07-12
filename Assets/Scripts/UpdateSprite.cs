using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSprite : MonoBehaviour
{
    public bool faceUp = false;
    public Image cardDisplay;
    public GameManager gameManager;

    public Sprite cardFace;
    public Sprite cardBack;

    public GameObject greenHighlight, yellowHighlight;

    void Start()
    {
        List<string> deck = GameManager.GenerateDeck();
        gameManager = FindObjectOfType<GameManager>();

        int i = 0;
        foreach (string card in deck)
        {
            if (this.name == card)
            {
                cardFace = gameManager.cardFaces[i];
                break;
            }
            i++;
        }
    }

    void Update()
    {
        if(faceUp)
        {
            cardDisplay.sprite = cardFace;
        }

        else
        {
            cardDisplay.sprite = cardBack;
        }
    }

    public void Highlight()
    {
        if(faceUp)
        {
            yellowHighlight.SetActive(true);
        }

        else
        {
            greenHighlight.SetActive(true);
        }
    }

    public void NoHighlight()
    {
        yellowHighlight.SetActive(false);
        greenHighlight.SetActive(false);
    }
}
