using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    public GameManager gameManager;
    public UpdateSprite updateSprite;
    public bool isSelected;
   
    public void Start()
    {
        gameManager = GameObject.Find("Canvas").GetComponent<GameManager>();
    }

    public void Clicked()
    {
        if (gameObject.tag == "Top")
        {
            //this is in the computers cards
            updateSprite.faceUp = true;
            foreach (GameObject card in gameManager.cardsToEnable)
            {
                card.GetComponent<Button>().interactable = false;
            }

            if (gameObject.name.Contains("joker"))
            {
                gameManager.displayBetweenRounds.SetActive(true);
                gameManager.displayBetweenRoundsText.text = "You won Round " + ScoreTracker.numberOfRounds;
                ScoreTracker.myScore += 1;
                ScoreTracker.numberOfRounds += 1;
                gameManager.jessicaCard.SetActive(true);
            }

            else if (!gameObject.name.Contains("joker"))
            {
                //move selected card to users hand and play immidiately if possible
                gameManager.DidntPickJoker(gameObject.name);
                gameManager.jessicaCard.SetActive(false);
            }

        }

        //if this is the deck and I haven't already pulled from it this turn
        else if (gameObject.tag == "Deck" && !gameManager.revealedDeck && ScoreTracker.myTurn)
        {
            //this is the deck
            gameManager.DealFromDeck();
            gameManager.revealedDeck = true;

            //clear the display text
            gameManager.debugHowManyToPresent.text = "Select cards to play";
            gameManager.debugText.text = "Your turn";
            foreach (string otherPlayerCards in gameManager.playerCards)
            {
                GameObject cards = GameObject.Find(otherPlayerCards);
                cards.GetComponent<Button>().interactable = true;
            }

            gameManager.skipButton.GetComponent<Button>().interactable = true;
        }

        //this is the players cards
        else
        {
            if (gameObject.name.Contains("joker"))
            {
                //can't select the joker
                return;
            }

            //this is a clickable card 
            else
            {
                //if it's my turn and I have checked the deck
                if (ScoreTracker.myTurn && gameManager.revealedDeck)
                {
                    //if there is less than 2 cards selected 
                    if (!isSelected && GameManager.numberSelected < 2)
                    {
                        //this is not a face card
                        if (!gameObject.name.Contains("K") && !gameObject.name.Contains("Q") && !gameObject.name.Contains("J"))
                        {
                            Selected();

                            //if the user chooses to play the top card from the deck 
                            if (gameObject.tag == "DeckTop")
                            {
                                //remove it from the deck on display list
                                gameManager.deckOnDisplay.Remove(gameObject.name);
                                gameManager.skipButton.GetComponent<Button>().interactable = false;
                            }
                        }

                        
                    }

                    //if there is less than 3 cards selected
                    else if (!isSelected && GameManager.numberSelected < 3)
                    {
                        //this is a face card
                        if (gameObject.name.Contains("K") || gameObject.name.Contains("Q") || gameObject.name.Contains("J"))
                        {
                            Selected();
                        }

                        //if the user chooses to play the top card from the deck 
                        if (gameObject.tag == "DeckTop")
                        {
                            //remove it from the deck on display list

                            gameManager.deckOnDisplay.Remove(gameObject.name);
                            gameManager.skipButton.GetComponent<Button>().interactable = false;

                        }

                       
                    }

                    else if (isSelected)
                    {
                        //if the user chooses to deselect the top card from the deck 
                        if (gameObject.tag == "DeckTop")
                        {
                            //add it back to the deck on display list
                            gameManager.deckOnDisplay.Add(gameObject.name);
                            gameManager.skipButton.GetComponent<Button>().interactable = true;
                        }
                       
                        CancelSelection();
                    }

                    else
                    {
                        return;
                    }
                }

                else
                {
                    //select when computer is playing
                    if (GameManager.numberForCompToSelectFrom < gameManager.numberOfPlayerCardsToChooseFrom)
                    {
                        if (gameObject.tag != "Deck")
                        {
                            GameManager.numberForCompToSelectFrom += 1;
                            gameManager.computerPickFromList.Add(gameObject);

                            //dont allow double selection
                            gameObject.GetComponent<Button>().interactable = false;
                            gameObject.GetComponent<Image>().color = Color.yellow;
                        }
                    }
                }
            }
        }
    }

    private void Selected()
    {
        gameManager.CardSelected(gameObject.name);

    }

    //use this on click or when too many cards have been selected
    public void CancelSelection()
    {
        //GameManager.numberSelected -= 1;
        gameManager.CardDeselected(gameObject.name);
    }


}
