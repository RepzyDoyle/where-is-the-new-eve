using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static int numberSelected;
    public static int numberForCompToSelectFrom;
    public Sprite[] cardFaces;

    public GameObject cardPrefab;
    public GameObject deckButton;
    public GameObject[] playerPos;
    public GameObject[] computerPos;

    public GameObject inPlay1, inPlay2, inPlaySpecial;
    public GameObject compInPlay1, compInPlay2, compInPlaySpecial;

    public static string[] suits = new string[] { "C", "D", "H", "S" };
    public static string[] values = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "J", "K", "Q" };

    public List<string> playerCards;
    public List<string> computerCards;
    public List<string> deckOnDisplay = new List<string>();
    public List<List<string>> deckTrips = new List<List<string>>();

    public List<string> deck;
    private List<string> myCards;

    public List<string> discardPile = new List<string>();
    int deckLocation;

    public int trips;
    public int tripsRemainder;
    public Text debugText;
    public Text debugHowManyToPresent;
    public Button playSelectedButton;
    public Button shuffleCardsButton;

    public List<string> currentlySelectedCards = new List<string>();
    public List<string> cardsToChooseFrom = new List<string>();

    //the computer card gameobjects to enable for user to pick from
    public List<GameObject> cardsToEnable;

    //all the gameobjects of users current cards
    public List<GameObject> currentUserCards;

    //for calculating hands
    public int value1 = 0;
    public int value2 = 0;
    public int numberOfCardsToChooseFrom;

    bool jackPlayed; bool queenPlayed; bool kingPlayed;
    public bool jackPlayedComp, queenPlayedComp, kingPlayedComp;
    public bool mustPlay, cascadeAllowed, revealedDeck;
    public int turn, compTurn;
    public int numberOfPlayerCardsToChooseFrom;

    public List<string> playableComputerCards;
    public List<GameObject> computerPickFromList = new List<GameObject>();

    public GameObject displayBetweenRounds;
    public Text displayBetweenRoundsText;

    public GameObject kingCanvas;
    public ScoreTracker scoreTracker;

    public int faceCardToPlay;
    public string computerSpecialCard;

    public List<string> compFaceCards = new List<string>();

    public Text displayNoOfCards;

    public GameObject skipButton;

    public GameObject jessicaCard;

    public void Update()
    {
        displayNoOfCards.text = numberSelected.ToString();
    }

    public static List<string> GenerateDeck()
    {
        List<string> newDeck = new List<string>();

        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                newDeck.Add(s + v);
            }
        }

        newDeck.Add("red_joker");
        newDeck.Add("black_joker");

        return newDeck;
    }

    public List<string> GenerateMyCardsList()
    {
        List<string> myCurrentCards = new List<string>();


        foreach (string card in playerCards)
        {
            myCurrentCards.Add(card);
        }

        return myCurrentCards;

    }

    public List<string> GenerateCompCardsList()
    {
        List<string> compCurrentCards = new List<string>();


        foreach (string card in computerCards)
        {
            compCurrentCards.Add(card);
        }


        return compCurrentCards;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene("MainScene");
        Start();
    }

    public void Start()
    {
        Debug.Log("StartCalled");
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        turn = 1;
        compTurn = 1;
        scoreTracker = GameObject.Find("ScoreTracker").GetComponent<ScoreTracker>();
        if (scoreTracker)
        {
            scoreTracker.OnSceneLoad();
        }
        Debug.Log("game started");
        yield return new WaitForSeconds(1.0f);
        Restart();
    }
    public void Restart()
    {
        //if it's an odd round user starts, if it's an even round computer starts
        if (ScoreTracker.numberOfRounds % 2 == 0)
        {
            ScoreTracker.myTurn = false;
            debugText.text = "Computer turn";
            compTurn = 1;
        }

        else if (ScoreTracker.numberOfRounds % 2 == 1)
        {
            ScoreTracker.myTurn = true;
            debugText.text = "Your turn";
            debugHowManyToPresent.text = "Draw from deck";
            turn = 1;
        }

        foreach(string card in playerCards)
        {
            GameObject cardToDelete = GameObject.Find(card);
            Destroy(cardToDelete);
        }

        playerCards.Clear();

        foreach (string card in computerCards)
        {
            GameObject cardToDelete = GameObject.Find(card);
            Destroy(cardToDelete);
        }

        computerCards.Clear();

        numberSelected = 0;
        playSelectedButton.interactable = false;
        playerCards = new List<string>();
        computerCards = new List<string>();

        //delete the cards that have been played from the screen
        if (inPlay1.transform.childCount > 0)
        {
            Destroy(inPlay1.transform.GetChild(0).gameObject);
        }
        if (inPlay2.transform.childCount > 0)
        {
            Destroy(inPlay2.transform.GetChild(0).gameObject);
        }
        if (inPlaySpecial.transform.childCount > 0)
        {
            Destroy(inPlaySpecial.transform.GetChild(0).gameObject);
        }

        if (compInPlay1.transform.childCount > 0)
        {
            Destroy(compInPlay1.transform.GetChild(0).gameObject);
        }
        if (compInPlay2.transform.childCount > 0)
        {
            Destroy(compInPlay2.transform.GetChild(0).gameObject);
        }
        if (compInPlaySpecial.transform.childCount > 0)
        {
            Destroy(compInPlaySpecial.transform.GetChild(0).gameObject);
        }

        revealedDeck = false;
        numberForCompToSelectFrom = 0;
        computerPickFromList.Clear();
        numberOfCardsToChooseFrom = 50;
        numberOfPlayerCardsToChooseFrom = 0;
        PlayCards();
        StartCoroutine(DelayCheckTurn());
    }

    IEnumerator DelayCheckTurn()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(CheckWhosTurn());
    }

    public void PlayCards()
    {
       
        deck = GenerateDeck();
        Shuffle(deck);
        SolitareSort();
        ShuffleMyCards();
        StartCoroutine(ComputerDeal());        
        SortDeckIntoTrips();
    }

    public void ShuffleMyCards()
    {
        Shuffle(playerCards);

        for (int i = 0; i < 10; i++)
        {
            foreach (Transform child in playerPos[i].transform)
            {               
                Destroy(child.gameObject);
            }
        }
        StartCoroutine(PlayerDeal());
    }

    public void ShuffleComputerCards()
    {

        Shuffle(computerCards);


        for (int i = 0; i < 10; i++)
        {
            foreach (Transform child in computerPos[i].transform)
            {
                Destroy(child.gameObject);
            }
        }
        StartCoroutine(ComputerDeal());
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    IEnumerator PlayerDeal()
    {
        //deal player cards
        for (int i = 0; i < playerCards.Count; i++)
        {
            float yOffset = 0f;
            float zOffset = 0.03f;

            //deal the player cards
            Debug.Log("dealPlayer");
            yield return new WaitForSeconds(0.01f);
            GameObject newCard = Instantiate(cardPrefab, new Vector3(playerPos[i].transform.position.x, playerPos[i].transform.position.y - yOffset, playerPos[i].transform.position.z - zOffset), Quaternion.identity, playerPos[i].transform);
            newCard.name = playerCards[i];
            newCard.tag = newCard.transform.parent.gameObject.tag;
            newCard.GetComponent<UpdateSprite>().faceUp = true;

            yOffset += 30f;
            zOffset += 1f;
            discardPile.Add(playerCards[i]);
        }

        //remove card to avoid duplicates
        foreach (string card in discardPile)
        {
            if (deck.Contains(card))
            {
                deck.Remove(card);
            }
        }

        discardPile.Clear();
    }

    IEnumerator ComputerDeal()
    {
        //deal computer cards
        for (int j = 0; j < computerCards.Count; j++)
        {
            float yOffset = 0f;
            float zOffset = 0.03f;


            Debug.Log("dealComputer");
            yield return new WaitForSeconds(0.01f);
            GameObject newCard = Instantiate(cardPrefab, new Vector3(computerPos[j].transform.position.x, computerPos[j].transform.position.y - yOffset, computerPos[j].transform.position.z - zOffset), Quaternion.identity, computerPos[j].transform);
            newCard.name = computerCards[j];
            newCard.tag = newCard.transform.parent.gameObject.tag;
            newCard.GetComponent<UpdateSprite>().faceUp = false;
            newCard.GetComponent<Button>().interactable = false;

            yOffset += 30f;
            zOffset += 1f;
            discardPile.Add(computerCards[j]);
        }

        foreach (string card in discardPile)
        {
            if (deck.Contains(card))
            {
                deck.Remove(card);
            }
        }

        discardPile.Clear();
    }

    void SolitareSort()
    {
        int jokerRed = deck.IndexOf("red_joker");
        playerCards.Add(deck[jokerRed]);
        deck.RemoveAt(jokerRed);

        int jokerBlack = deck.IndexOf("black_joker");
        computerCards.Add(deck[jokerBlack]);
        deck.RemoveAt(jokerBlack);

        for (int i = 0; i < 9; i++)
        {
            playerCards.Add(deck.Last<string>());
            deck.RemoveAt(deck.Count - 1);
        }

        for (int j = 0; j < 9; j++)
        {
            computerCards.Add(deck.Last<string>());
            deck.RemoveAt(deck.Count - 1);
        }
    }

    public void SortDeckIntoTrips()
    {
        trips = deck.Count / 1;
        tripsRemainder = deck.Count % 1;
        deckTrips.Clear();

        //this will run as long as remaining cards is greater than 1
        int modifier = 0;
        for (int i = 0; i < trips; i++)
        {
            List<string> myTrips = new List<string>();
            for (int j = 0; j < 1; j++)
            {
                myTrips.Add(deck[j + modifier]);
            }

            deckTrips.Add(myTrips);
            modifier = modifier + 1;
        }

        if (tripsRemainder != 0)
        {
            List<string> myRemainders = new List<string>();
            modifier = 0;
            for (int k = 0; k < tripsRemainder; k++)
            {
                myRemainders.Add(deck[deck.Count - tripsRemainder + modifier]);
                modifier++;
            }
            deckTrips.Add(myRemainders);
            trips++;
        }

        deckLocation = 0;
    }

    public void DealFromDeck()
    {
        //add remaining cards to the discard pile
        foreach (Transform child in deckButton.transform)
        {
            if (child.CompareTag("Card"))
            {
                deck.Remove(child.name);
                discardPile.Add(child.name);
                Destroy(child.gameObject);
            }
        }

        //if there's still enough cards in the deck
        if (deckLocation < trips)
        {
            //draw 1 new cards
            float xOffset = 130f;

            foreach (string card in deckTrips[deckLocation])
            {
                GameObject newTopCard = Instantiate(cardPrefab, new Vector3(deckButton.transform.position.x + xOffset, deckButton.transform.position.y, deckButton.transform.position.z), Quaternion.identity, deckButton.transform);
                newTopCard.tag = "DeckTop";
                xOffset = xOffset + 120f;
                newTopCard.name = card;
                deckOnDisplay.Add(card);
                newTopCard.GetComponent<UpdateSprite>().faceUp = true;

                //don't let the player interact unless it's their turn
                if(!ScoreTracker.myTurn)
                {
                    newTopCard.GetComponent<Button>().interactable = false;
                }
            } 

            deckLocation++;
        }

        //else restack the deck
        else
        {
            RestackTopDeck();
        }
    }

    void RestackTopDeck()
    {
        foreach (string card in discardPile)
        {
            deck.Add(card);
        }

        discardPile.Clear();
        SortDeckIntoTrips();
    }

    public void CardSelected(string cardName)
    {
        numberSelected += 1;
        GameObject selectedCard = GameObject.Find(cardName);

        Debug.Log(cardName + "selected");
        selectedCard.GetComponent<UpdateSprite>().Highlight();

        if (numberSelected < 3)
        {
            //must be a number card
            if (!cardName.Contains("Q") && !cardName.Contains("K") && !cardName.Contains("J"))
            {
                //if this is the first card selected
                if (numberSelected < 2)
                {
                    selectedCard.GetComponent<UserInput>().isSelected = true;
                    currentlySelectedCards.Add(cardName);
                    if (currentlySelectedCards[0].Contains("9"))
                    {
                        //allow to play on its own, or with a face card
                        Debug.Log("9 selected");
                        value1 = 9;
                        numberSelected = 2;
                        playSelectedButton.interactable = true;
                    }

                    else
                    {
                        for (int i = 0; i <= 8; i++)
                        {
                            if (currentlySelectedCards[0].Contains(i.ToString()))
                            {
                                //value of the first selected card

                                value1 = i;
                                playSelectedButton.interactable = false;
                            }
                        }
                    }
                }

                else if (numberSelected == 2)
                {
                    selectedCard.GetComponent<UserInput>().isSelected = true;
                    currentlySelectedCards.Add(cardName);
                    for (int i = 0; i <= 8; i++)
                    {
                        if (currentlySelectedCards[1].Contains(i.ToString()))
                        {
                            //value of the second selected card
                            value2 = i;
                        }
                    }

                    //if value1 and 2 don't add up to 9, disallow the selection
                    if (value1 + value2 == 9)
                    {
                        //allow play cards
                        playSelectedButton.interactable = true;
                    }

                    else
                    {
                        selectedCard.GetComponent<UserInput>().CancelSelection();
                        //debugText.text = "doesn't add up to 9";
                        return;
                    }
                }
            }

            else
            {
                selectedCard.GetComponent<UserInput>().CancelSelection();
                Debug.Log("is face card");
                return;
            }
        }

        else if (numberSelected == 3)
        {

            //check if this is a face card and not a jack 
            if (cardName.Contains("Q") || cardName.Contains("K"))
            {
                selectedCard.GetComponent<UserInput>().isSelected = true;
                currentlySelectedCards.Add(cardName);
            }

            //check if it's a jack and 4 and 5 are not selected
            else if (cardName.Contains("J") && value1 != 4 && value1 != 5)
            {
                selectedCard.GetComponent<UserInput>().isSelected = true;
                currentlySelectedCards.Add(cardName);
            
            }

            else
            {
                selectedCard.GetComponent<UserInput>().CancelSelection();
                return;
            }
        }

        else
        {

            GameObject.Find(cardName).GetComponent<UserInput>().CancelSelection();
            return;
        }

        Debug.Log("Number of cards selected: " + numberSelected);
    }

    public void CardDeselected(string cardName)
    {
            numberSelected -= 1;
            GameObject selectedCard = GameObject.Find(cardName);
            selectedCard.GetComponent<UpdateSprite>().NoHighlight();
            selectedCard.GetComponent<Image>().color = Color.white;
            selectedCard.GetComponent<UserInput>().isSelected = false;

            if (numberSelected == 2)
            {
                //if the deselected card is not a face card all face cards must be deselected too
                if (!cardName.Contains("Q") && !cardName.Contains("K") && !cardName.Contains("J"))
                {
                    //remove the last card
                    string cardToRemove = currentlySelectedCards[2];
                    GameObject.Find(cardToRemove).GetComponent<UserInput>().isSelected = false;
                    GameObject.Find(cardToRemove).GetComponent<Image>().color = Color.white;
                    currentlySelectedCards.RemoveAt(currentlySelectedCards.Count - 1);
                    playSelectedButton.interactable = false;
                    numberSelected -= 1;
                }
            }

            if(cardName.Contains("9"))
            {
                numberSelected -= 1;
            }   

            if(!cardName.Contains("9"))
            {
                if (!cardName.Contains("Q") && !cardName.Contains("K") && !cardName.Contains("J"))
                {
                playSelectedButton.interactable = false;
                }
            }
            currentlySelectedCards.Remove(cardName);
        
        Debug.Log("number of cards selected: " + numberSelected);
    }

    public void PlaySelected()
    {
        //force positive
        skipButton.GetComponent<Button>().interactable = false;
        numberOfCardsToChooseFrom = System.Math.Abs(value1 - value2) + 1;
       
        GameObject card1 = GameObject.Find(currentlySelectedCards[0]);

        card1.transform.position = inPlay1.transform.position;
        card1.GetComponent<Button>().interactable = false;

        card1.transform.parent = inPlay1.transform;

        if (value1 == 9 && numberSelected > 2)
        {
                GameObject card2 = GameObject.Find(currentlySelectedCards[1]);               

                card2.transform.position = inPlaySpecial.transform.position;
                card2.transform.rotation = inPlaySpecial.transform.rotation;
                card2.transform.parent = inPlaySpecial.transform;
                card2.GetComponent<Button>().interactable = false;
                Debug.Log("Discard[1]");
                discardPile.Add(currentlySelectedCards[1]);
                playerCards.Remove(currentlySelectedCards[1]);

                if (currentlySelectedCards[1].Contains("J"))
                {
                    //do jack power
                    jackPlayed = true;
                    Debug.Log("J selected");
                }

                else if (currentlySelectedCards[1].Contains("Q"))
                {
                    //do queen power
                    queenPlayed = true;
                    Debug.Log("Q selected");
                }

                else if (currentlySelectedCards[1].Contains("K"))
                {
                    //do king power
                    kingPlayed = true;
                    Debug.Log("K selected");
                }
            
        }

        if (value1 != 9)
        {
            GameObject card2 = GameObject.Find(currentlySelectedCards[1]);

            card2.transform.position = inPlay2.transform.position;
            card2.transform.rotation = inPlay2.transform.rotation;
            card2.transform.parent = inPlay2.transform;
            card2.GetComponent<Button>().interactable = false;

            discardPile.Add(currentlySelectedCards[1]);
            playerCards.Remove(currentlySelectedCards[1]);


            if (numberSelected > 2)
            {
                GameObject card3 = GameObject.Find(currentlySelectedCards[2]);

                card3.transform.position = inPlaySpecial.transform.position;
                card3.transform.rotation = inPlaySpecial.transform.rotation;
                card3.transform.parent = inPlaySpecial.transform;
                card3.GetComponent<Button>().interactable = false;

                if (currentlySelectedCards[2].Contains("J"))
                {
                    //do jack power
                    jackPlayed = true;
                    Debug.Log("J selected");
                }

                else if (currentlySelectedCards[2].Contains("Q"))
                {
                    //do queen power
                    queenPlayed = true;
                    Debug.Log("Q selected");
                }

                else if (currentlySelectedCards[2].Contains("K"))
                {
                    //do king power
                    kingPlayed = true;
                    Debug.Log("K selected");
                }

                discardPile.Add(currentlySelectedCards[2]);
                playerCards.Remove(currentlySelectedCards[2]);
            }
        }

        shuffleCardsButton.interactable = false;

        discardPile.Add(currentlySelectedCards[0]);
        playerCards.Remove(currentlySelectedCards[0]);
        Debug.Log(playerCards);

        if (jackPlayed)
        {
            float newNumber;
            newNumber = (float)numberOfCardsToChooseFrom;
            newNumber = Mathf.Ceil(newNumber / 2);
            numberOfCardsToChooseFrom = (int)newNumber + 1;
            jackPlayed = false;
        }

        playSelectedButton.interactable = false;
        StartCoroutine(ComputerChooseCardsToEnable());
    }

    public IEnumerator ComputerChooseCardsToEnable()
    {
        yield return new WaitForSeconds(1f);

        //find the joker and add it to the list
        foreach (string card in computerCards)
        {
            if (card.Contains("joker"))
            {
                //add the joker to the selection
                cardsToChooseFrom.Add(card);
            }
        }

        //make sure you're not trying to choose from more cards than are in the computers hand
        if(numberOfCardsToChooseFrom > computerCards.Count)
        {
            numberOfCardsToChooseFrom = computerCards.Count;
        }

        //add the other cards
        for (int j = 0; j < numberOfCardsToChooseFrom; j++)
        {
            if (numberOfCardsToChooseFrom <= computerCards.Count)
            {
                if (!computerCards[j].Contains("joker"))
                { 
                    cardsToChooseFrom.Add(computerCards[j]);
                }
            }
        }

        //then shuffle the computer cards and present the selectable cards to the player
        ShuffleComputerCards();
        yield return new WaitForSeconds(1f);
        

        for (int k = 0; k < numberOfCardsToChooseFrom; k++)
        {
            if (numberOfCardsToChooseFrom <= computerCards.Count)
            {
                cardsToEnable.Add(GameObject.Find(cardsToChooseFrom[k]));
            }
        }

        foreach (GameObject card in cardsToEnable)
        {
            card.GetComponent<Button>().interactable = true;
        }

        if (kingPlayed)
        {
            foreach (GameObject card in cardsToEnable)
            {
                card.GetComponent<UpdateSprite>().faceUp = true;
            }
            StartCoroutine(DoCardMonty(cardsToEnable));
            kingPlayed = false;
        }

        else if (queenPlayed)
        {
            kingCanvas.SetActive(true);
            queenPlayed = false;
        }

        debugHowManyToPresent.text = "Choose from computer's hand";
    }

    IEnumerator DoCardMonty(List<GameObject> cardsToMonty)
    {
        yield return new WaitForSeconds(0.1f);
        foreach (GameObject card in cardsToMonty)
        {
            card.GetComponent<UpdateSprite>().faceUp = false;           
        }
    }

    public void RemoveSuitFromPresentedCards(string suit)
    {
        List<GameObject> cardList;
        if (ScoreTracker.myTurn)
        {
            cardList = cardsToEnable;
        }

        else
        {
            cardList = computerPickFromList;
        }

        foreach (GameObject card in cardList)
        {
            if (!card.name.Contains("joker"))
            {
                if (card.name.Contains(suit))
                {
                    Debug.Log("Card(s) to remove: " + card.name);
                    card.GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    //see if user can play the card that they picked
    public void DidntPickJoker(string cardName)
    {
        numberSelected = 0;

        if (turn == 1)
        {
            turn = 0;
            for (int i = 0; i <= 9; i++)
            {
                if (cardName.Contains(i.ToString()))
                {
                    //value of the selected card
                    value1 = i;
                    Debug.Log(value1);

                    if (value1 == 9)
                    {
                        //play immidiately
                        Debug.Log("Must Play");
                        debugHowManyToPresent.text = "Must play 9";
                        mustPlay = true;
                    }

                    // if it's another number
                    else
                    {
                        Debug.Log("It's another number");
                        foreach (string card in playerCards)
                        {
                            for (int k = 0; k <= playerCards.Count; k++)
                            {
                                if (card.Contains(k.ToString()))
                                {
                                    if (value1 + k == 9)
                                    {
                                        //play immidiately
                                        Debug.Log("Must Play" + k);
                                        debugHowManyToPresent.text = "Must create 9 pair";
                                        mustPlay = true;
                                    }

                                    else
                                    {
                                        //dont play, take the card in to your hand
                                        Debug.Log("Cant play" + k);                                     
                                    }
                                }

                                else
                                {
                                    Debug.Log("No number cards");
                                }
                            }
                        }
                    }
                }

                else
                {
                    Debug.Log("Its a face card");
                }
            }
        }

        playerCards.Add(cardName);
        computerCards.Remove(cardName);
        GameObject cardToAdd = GameObject.Find(cardName);

        if (playerCards.Count <= 10)
        {
            foreach(GameObject slot in playerPos)
            {
                if (slot.transform.childCount == 0)
                {
                    cardToAdd.transform.position = slot.transform.position;
                    cardToAdd.transform.parent = slot.transform;
                    cardToAdd.tag = cardToAdd.transform.parent.gameObject.tag;                  
                }
            }
        }

        //if there's space, add the card from the top of the deck to the users hand
        foreach (GameObject slot in playerPos)
        {
            if (slot.transform.childCount == 0)
            {
                if (deckOnDisplay.Count > 0)
                {
                    //end of this hand, add the deck card to the users hand
                    GameObject deckCard = GameObject.Find(deckOnDisplay.Last<string>());

                    if (deckCard)
                    {
                        deckOnDisplay.Remove(deckCard.name);
                    }
                    playerCards.Add(deckCard.name);
                    deckCard.transform.position = slot.transform.position;
                    deckCard.transform.parent = slot.transform;
                    deckCard.tag = deckCard.transform.parent.gameObject.tag;
                }
            }
        }

        currentlySelectedCards.Clear();
        cardsToChooseFrom.Clear();
        cardsToEnable.Clear();

        if (mustPlay)
        {
            CardSelected(cardName);

            //allow the other player cards to be enabled 
            foreach (string otherPlayerCards in playerCards)
            {
                if (!otherPlayerCards.Contains(cardName))
                {
                    GameObject cards = GameObject.Find(otherPlayerCards);
                    cards.GetComponent<Button>().interactable = true;
                }
            }

            mustPlay = false;
        }

        else
        {
            if (playerCards.Count > 10)
            {
                playerCards.Remove(cardName);
                Destroy(cardToAdd);
            }

            debugHowManyToPresent.text = "Not possible to play, turn over";

            //reset deck click
            revealedDeck = false;
            ScoreTracker.myTurn = false;

            //reset players cascade ability 
            turn = 1;
        }

        //delete the cards that have been played from the screen
        if (inPlay1.transform.childCount > 0)
        {
            Destroy(inPlay1.transform.GetChild(0).gameObject);
        }
        if (inPlay2.transform.childCount > 0)
        {
            Destroy(inPlay2.transform.GetChild(0).gameObject);
        }
        if (inPlaySpecial.transform.childCount > 0)
        {
            Destroy(inPlaySpecial.transform.GetChild(0).gameObject);
        }

        StartCoroutine(DelayCheckTurn());        
    }

    public IEnumerator CheckWhosTurn()
    {
        if (ScoreTracker.myTurn)
        {           
            //make sure all player cards are white
            if (playerCards.Count > 0)
            {
                foreach (string card in playerCards)
                {
                    GameObject playerCard = GameObject.Find(card);
                    playerCard.GetComponent<Image>().color = Color.white;
                }
            }
            yield break;
        }

        //if it's the computers turn
        else
        {
            //allow for cascade again
            debugText.text = "Computer's turn";

            compFaceCards = new List<string>();
            List<string> numberCompCards = new List<string>();
            List<int> numberCardsIntList = new List<int>();

            //reveal the top card of the deck and allow the computer to choose from that
            DealFromDeck();

            yield return new WaitForSeconds(2f);
            
            computerCards.Add(deckOnDisplay.Last<string>());
           

            foreach (string card in computerCards)
            {
                //add all the number cards to a new list 
                for (int i = 0; i < 11; i++)
                {
                    if (card.Contains(i.ToString()))
                    {
                        numberCompCards.Add(card);
                        numberCardsIntList.Add(i);
                    }

                    //else its a face card
                    else if(card.Contains("J") || card.Contains("Q") || card.Contains("K"))
                    {
                        compFaceCards.Add(card);
                    }
                }
            }

            //if there is a face card in the hand, randomly select and play it if the hand adds up to 9
            if (compFaceCards.Count > 0)
            {
                faceCardToPlay = Random.Range(0, compFaceCards.Count);
                //store the name of the card so it can be removed from the computer's hand later
                computerSpecialCard = compFaceCards[faceCardToPlay];

                if (compFaceCards[faceCardToPlay].Contains("J"))
                {
                    jackPlayedComp = true;
                }

                else if (compFaceCards[faceCardToPlay].Contains("Q"))
                {
                    queenPlayedComp = true;
                }

                else if (compFaceCards[faceCardToPlay].Contains("K"))
                {
                    kingPlayedComp = true;
                }
            }

            //check if two of the cards add up to 9
            for (int i = 0; i < numberCompCards.Count; i++)
            {
                for (int j = 0; j < numberCompCards.Count; j++)
                {
                    if (i != j)
                    {
                        int sum = numberCardsIntList[i] + numberCardsIntList[j];
                        if (sum == 9)
                        {
 
                            //if its a 4 and a 5 play first
                            if (numberCardsIntList[i] == 4 || numberCardsIntList[i] == 5)
                            {
                                numberOfPlayerCardsToChooseFrom = System.Math.Abs(5 - 4);
                                ComputerPlayBestHand(numberCompCards[i], numberCompCards[j]);
                                
                                ScoreTracker.myTurn = false;
                                yield break;
                            }

                            //then play 6 and 3
                            else if (numberCardsIntList[i] == 3 || numberCardsIntList[i] == 6)
                            {
                                numberOfPlayerCardsToChooseFrom = System.Math.Abs(6 - 3);
                                ComputerPlayBestHand(numberCompCards[i], numberCompCards[j]);

                                ScoreTracker.myTurn = false;
                                yield break;
                            }

                            //then play 2 and 7
                            else if (numberCardsIntList[i] == 2 || numberCardsIntList[i] == 7)
                            {
                                numberOfPlayerCardsToChooseFrom = System.Math.Abs(7 - 2);
                                ComputerPlayBestHand(numberCompCards[i], numberCompCards[j]);

                                ScoreTracker.myTurn = false;
                                yield break;
                            }

                            //then play 8 and 1
                            else if (numberCardsIntList[i] == 8 || numberCardsIntList[i] == 1)
                            {
                                numberOfPlayerCardsToChooseFrom = System.Math.Abs(8 - 1);
                                ComputerPlayBestHand(numberCompCards[i], numberCompCards[j]);

                                ScoreTracker.myTurn = false;
                                yield break;
                            }
                        }
                    }

                    else if (numberCardsIntList[i] == 9 || numberCardsIntList[j] == 9)
                    {
                        //9 is last option
                        if (numberCompCards[i].Contains("9"))
                        {
                            ComputerPlayBestHand(numberCompCards[i], "NA");
                        }

                        else if (numberCompCards[j].Contains("9"))
                        {
                            ComputerPlayBestHand("NA", numberCompCards[j]);
                        }
                        
                        ScoreTracker.myTurn = false;
                        yield break;
                    }

                    else
                    {
                        //Doesn't add to 9, comp must skip
                        debugHowManyToPresent.text = "Computer can't play, your turn";
                        revealedDeck = false;
                        ScoreTracker.myTurn = true;

                        if (deckOnDisplay.Count > 0)
                        {
                            bool slotFound = false;
                            GameObject deckCard = GameObject.Find(deckOnDisplay.Last<string>());
                            Debug.Log("The deck card is:" + deckCard);
                            foreach (GameObject slot in computerPos)
                            {
                                if (slot.transform.childCount == 0)
                                {
                                    deckCard.transform.position = slot.transform.position;
                                    deckCard.transform.parent = slot.transform;
                                    deckCard.tag = deckCard.transform.parent.tag;
                                    deckCard.GetComponent<UpdateSprite>().faceUp = false;
                                    deckCard.GetComponent<Button>().interactable = false;
                                    if (!computerCards.Contains(deckCard.name))
                                    {
                                        //if the deck card isn't already in the list, add it
                                        computerCards.Add(deckCard.name);
                                    }
                                    //remove it from the deck pile                       
                                    deckOnDisplay.Remove(deckCard.name);
                                    slotFound = true;
                                }
                            }

                            if (!slotFound)
                            {
                                //if the deck card can't be added to the hand, remove it from the list
                                if (computerCards.Count > 0)
                                {
                                    computerCards.Remove(deckOnDisplay.Last<string>());
                                }
                            }
                        }

                        StartCoroutine(CheckWhosTurn());
                        numberForCompToSelectFrom = 0;
                    }
                }
            }
           
        }
    }

    public void ComputerPlaysFaceCard(string faceCard)
    {
        if (compInPlaySpecial.transform.childCount > 0)
        {
            Destroy(compInPlaySpecial.transform.GetChild(0).gameObject);
        }

        if (compFaceCards.Count >= 0)
        {
            GameObject compCard3 = GameObject.Find(faceCard);
            compCard3.transform.position = compInPlaySpecial.transform.position;
            compCard3.transform.rotation = compInPlaySpecial.transform.rotation;
            compCard3.transform.parent = compInPlaySpecial.transform;
            compCard3.GetComponent<UpdateSprite>().faceUp = true;
        }
    }

    public void ComputerPlayBestHand(string card1, string card2)
    {
        if (deckOnDisplay.Count > 0)
        {
            // add the deck card object to the hand
            GameObject deckCard = GameObject.Find(deckOnDisplay.Last<string>());
            deckOnDisplay.Remove(deckOnDisplay.Last<string>());
            bool slotFound = false;
            foreach (GameObject slot in computerPos)
            {
                if (slot.transform.childCount == 0)
                {
                    deckCard.transform.position = slot.transform.position;
                    deckCard.transform.parent = slot.transform;
                    deckCard.tag = deckCard.transform.parent.tag;
                    deckCard.GetComponent<UpdateSprite>().faceUp = false;
                    deckCard.GetComponent<Button>().interactable = false;
                    if (!computerCards.Contains(deckCard.name))
                    {
                        //if the deck card isn't already in the list, add it
                        computerCards.Add(deckCard.name);
                    }

                    //remove it from the deck pile                       
                    deckOnDisplay.Remove(deckCard.name);
                    slotFound = true;
                }
            }

            if (!slotFound)
            {
                //if the deck card can't be added to the hand, remove it from the list
                if (computerCards.Count > 0)
                {
                    computerCards.Remove(deckCard.name);
                }
            }
                
            
        }

        numberForCompToSelectFrom = 0;
        //clear cards already on display
        if (compInPlay1.transform.childCount > 0)
        {
            Destroy(compInPlay1.transform.GetChild(0).gameObject);
        }

        if (compInPlay2.transform.childCount > 0)
        {
            Destroy(compInPlay2.transform.GetChild(0).gameObject);
        }



        //if it's not 9
        if (!card1.Contains("9") && !card2.Contains("9"))
        {
            if (jackPlayedComp)
            {
                if (!card1.Contains("5") && !card1.Contains("4"))
                {
                    float newNumber;
                    newNumber = (float)numberOfPlayerCardsToChooseFrom;
                    newNumber = Mathf.Ceil(newNumber / 2);
                    numberOfPlayerCardsToChooseFrom = (int)newNumber;
                    computerCards.Remove(computerSpecialCard);
                }

                else
                {
                    compFaceCards.Clear();
                }

                jackPlayedComp = false;
            }

            debugHowManyToPresent.text = "Present " + numberOfPlayerCardsToChooseFrom + " cards";

            //remove these cards from the players hand, then play the cards
            computerCards.Remove(card1);
            computerCards.Remove(card2);
            Debug.Log("Best hand is: " + card1 + "+" + card2);
            GameObject compCard1 = GameObject.Find(card1);
            GameObject compCard2 = GameObject.Find(card2);

            compCard1.transform.position = compInPlay1.transform.position;
            compCard1.transform.rotation = compInPlay1.transform.rotation;
            compCard1.transform.parent = compInPlay1.transform;
            compCard1.GetComponent<UpdateSprite>().faceUp = true;

            compCard2.transform.position = compInPlay2.transform.position;
            compCard2.transform.rotation = compInPlay2.transform.rotation;
            compCard2.transform.parent = compInPlay2.transform;
            compCard2.GetComponent<UpdateSprite>().faceUp = true;

            discardPile.Add(card1);
            discardPile.Add(card2);
        }

        else if (card1.Contains("9"))
        {
            numberOfPlayerCardsToChooseFrom = 9;

            if (jackPlayedComp)
            {
                float newNumber;
                newNumber = (float)numberOfPlayerCardsToChooseFrom;
                newNumber = Mathf.Ceil(newNumber / 2);
                numberOfPlayerCardsToChooseFrom = (int)newNumber;
                computerCards.Remove(computerSpecialCard);
                jackPlayedComp = false;
            }

            debugHowManyToPresent.text = "Present " + numberOfPlayerCardsToChooseFrom + " cards";

            //remove these cards from the players hand, then play the cards
            computerCards.Remove(card1);
            GameObject compCard1 = GameObject.Find(card1);

            compCard1.transform.position = compInPlay1.transform.position;
            compCard1.transform.rotation = compInPlay1.transform.rotation;
            compCard1.transform.parent = compInPlay1.transform;
            compCard1.GetComponent<UpdateSprite>().faceUp = true;
            discardPile.Add(card1);
        }

        else if (card2.Contains("9"))
        {
            numberOfPlayerCardsToChooseFrom = 9;

            if (jackPlayedComp)
            {
                float newNumber;
                newNumber = (float)numberOfPlayerCardsToChooseFrom;
                newNumber = Mathf.Ceil(newNumber / 2);
                numberOfPlayerCardsToChooseFrom = (int)newNumber;
                computerCards.Remove(computerSpecialCard);
                jackPlayedComp = false;
            }

            debugHowManyToPresent.text = "Present " + numberOfPlayerCardsToChooseFrom + " cards";
            //remove these cards from the players hand, then play the cards
            computerCards.Remove(card2);
            GameObject compCard2 = GameObject.Find(card2);

            compCard2.transform.position = compInPlay1.transform.position;
            compCard2.transform.rotation = compInPlay1.transform.rotation;
            compCard2.transform.parent = compInPlay1.transform;
            compCard2.GetComponent<UpdateSprite>().faceUp = true;
            discardPile.Add(card2);
        }

        foreach (GameObject card in computerPickFromList)
        {
            if (!card.name.Contains("joker"))
            {
                card.GetComponent<Image>().color = Color.white;
            }
        }
        computerPickFromList.Clear();

        if (compFaceCards.Count > 0)
        {
            ComputerPlaysFaceCard(compFaceCards[faceCardToPlay]);
        }

        UserChooseCardsToPresent(numberOfPlayerCardsToChooseFrom);
    }

    public void UserChooseCardsToPresent(int howMany)
    {
        //enable all the users cards
        foreach (string card in playerCards)
        {
            GameObject eachCard = GameObject.Find(card);
            eachCard.GetComponent<Button>().interactable = true;
        }

        GameObject joker = GameObject.Find("red_joker");
        computerPickFromList.Add(joker);
        joker.GetComponent<Image>().color = Color.yellow;
    }

    public void ComputerChoosesCard()
    {
        
        if (compInPlaySpecial.transform.childCount > 0)
        {
            Destroy(compInPlaySpecial.transform.GetChild(0).gameObject);
        }

        if (kingPlayedComp)
        {
            foreach (GameObject card in computerPickFromList)
            {
                card.GetComponent<UpdateSprite>().faceUp = true;
            }

            StartCoroutine(DoCardMonty(computerPickFromList));
            computerCards.Remove(computerSpecialCard);
            kingPlayedComp = false;
        }

        else if (queenPlayedComp)
        {
            //randomly remove a suit from the computerpickfrom list
            int suitToChoose = Random.Range(0, suits.Length);
            RemoveSuitFromPresentedCards(suits[suitToChoose]);
            computerCards.Remove(computerSpecialCard);
            queenPlayedComp = false;
        }

        if (numberForCompToSelectFrom == numberOfPlayerCardsToChooseFrom || numberForCompToSelectFrom >= playerCards.Count - 1)
        {
            int cardToChoose = Random.Range(0, computerPickFromList.Count);
            computerPickFromList[cardToChoose].GetComponent<Image>().color = Color.red;
            computerPickFromList[cardToChoose].GetComponent<Button>().interactable = false;
            playerCards.Remove(computerPickFromList[cardToChoose].name);
            int chosenCardValue = 0;

            //computer picked joker
            if (computerPickFromList[cardToChoose].name.Contains("joker"))
            {
                displayBetweenRounds.SetActive(true);
                displayBetweenRoundsText.text = "You lost Round " + ScoreTracker.numberOfRounds;
                ScoreTracker.compScore += 1;
                ScoreTracker.numberOfRounds += 1;
            }

            //computer didn't pick joker
            else
            {
                //if the computer is allowed to cascade
                if (compTurn == 1)
                {
                    compTurn = 0;
                    compFaceCards.Clear();
                    compFaceCards = new List<string>();
                    List<string> numberCompCards = new List<string>();
                    List<int> numberCardsIntList = new List<int>();

                    //find the value of the chosen card
                    for (int i = 0; i < 10; i++)
                    {
                        if (computerPickFromList[cardToChoose].name.Contains(i.ToString()))
                        {
                            Debug.Log(i);
                            chosenCardValue = i;
                        }
                    }

                    foreach (string card in computerCards)
                    {
                        //add all the number cards to a new list 
                        for (int i = 0; i < 10; i++)
                        {
                            if (card.Contains(i.ToString()))
                            {
                                numberCompCards.Add(card);
                                numberCardsIntList.Add(i);
                            }
                        }
                    }

                    if (chosenCardValue == 9)
                    {
                        //if its 9
                        numberOfPlayerCardsToChooseFrom = 9;
                        ComputerPlayBestHand(computerPickFromList[cardToChoose].name, "9");
                        return;
                    }

                    if (chosenCardValue != 0)
                    {
                        for (int j = 0; j < numberCompCards.Count; j++)
                        {
                            int sum = chosenCardValue + numberCardsIntList[j];
                            Debug.Log(chosenCardValue + "+" + numberCardsIntList[j]);
                            if (sum == 9)
                            {

                                //if its a 4 and a 5 play first
                                if (chosenCardValue == 4 || chosenCardValue == 5)
                                {
                                    numberOfPlayerCardsToChooseFrom = System.Math.Abs(5 - 4);

                                    ComputerPlayBestHand(computerPickFromList[cardToChoose].name, numberCompCards[j]);
                                    return;
                                }

                                //then play 6 and 3
                                else if (chosenCardValue == 3 || chosenCardValue == 6)
                                {
                                    numberOfPlayerCardsToChooseFrom = System.Math.Abs(6 - 3);

                                    ComputerPlayBestHand(computerPickFromList[cardToChoose].name, numberCompCards[j]);
                                    return;
                                }

                                //then play 2 and 7
                                else if (chosenCardValue == 2 || chosenCardValue == 7)
                                {
                                    numberOfPlayerCardsToChooseFrom = System.Math.Abs(7 - 2);

                                    ComputerPlayBestHand(computerPickFromList[cardToChoose].name, numberCompCards[j]);
                                    return;
                                }

                                //then play 8 and 1
                                else if (chosenCardValue == 8 || chosenCardValue == 1)
                                {
                                    numberOfPlayerCardsToChooseFrom = System.Math.Abs(8 - 1);

                                    ComputerPlayBestHand(computerPickFromList[cardToChoose].name, numberCompCards[j]);
                                    return;
                                }
                            }
                        }

                        //if nothing is returned from this count it can't be played and must be added to their hand. Computer's turn is ended
                        StartCoroutine(ComputerCantPlayCard(cardToChoose));
                    }

                    else
                    {
                        ////it can't be played and must be added to their hand. Computer's turn is ended
                        StartCoroutine(ComputerCantPlayCard(cardToChoose));

                    }
                }

                //computer is not allowed to cascade
                else
                {
                    //it can't be played and must be added to their hand. Computer's turn is ended
                    StartCoroutine(ComputerCantPlayCard(cardToChoose));
                }

            }
        }
        numberOfPlayerCardsToChooseFrom = 0;
        computerPickFromList.Clear();        
    }

    public IEnumerator ComputerCantPlayCard(int theCardToChoose)
    {
        computerCards.Add(computerPickFromList[theCardToChoose].name);
        GameObject cardSelected = GameObject.Find(computerPickFromList[theCardToChoose].name);
        if (computerCards.Count <= 10)
        {
            foreach(GameObject slot in computerPos)
            {
                if(slot.transform.childCount == 0)
                {
                    cardSelected.transform.position = slot.transform.position;
                    cardSelected.transform.parent = slot.transform;
                    cardSelected.GetComponent<UpdateSprite>().faceUp = false;
                    cardSelected.tag = cardSelected.transform.parent.gameObject.tag;
                }
            }
        }

        if (computerCards.Count >= 11)
        {
            discardPile.Add(computerPickFromList[theCardToChoose].name);
            //remove the deck card from the computer hand
            computerCards.Remove(deckOnDisplay.Last<string>());
            revealedDeck = false;
            Destroy(cardSelected);
        }

        if (compInPlay1.transform.childCount > 0)
        {
            Destroy(compInPlay1.transform.GetChild(0).gameObject);
        }

        if (compInPlay2.transform.childCount > 0)
        {
            Destroy(compInPlay2.transform.GetChild(0).gameObject);
        }

        if (compInPlaySpecial.transform.childCount > 0)
        {
            Destroy(compInPlaySpecial.transform.GetChild(0).gameObject);
        }

        ScoreTracker.myTurn = true;
        computerPickFromList.Clear();
        numberForCompToSelectFrom = 0;
        debugHowManyToPresent.text = "Computer can't play, your turn";
        
        if (deckOnDisplay.Count > 0)
        {
            bool slotFound = false;
            GameObject deckCard = GameObject.Find(deckOnDisplay.Last<string>());
            foreach (GameObject slot in computerPos)
            {
                if (slot.transform.childCount == 0)
                {
                    deckCard.transform.position = slot.transform.position;
                    deckCard.transform.parent = slot.transform;
                    deckCard.tag = deckCard.transform.parent.tag;
                    deckCard.GetComponent<Button>().interactable = false;
                    deckCard.GetComponent<UpdateSprite>().faceUp = false;
                    if (!computerCards.Contains(deckCard.name))
                    {
                        //if the deck card isn't already in the list, add it
                        computerCards.Add(deckCard.name);
                    }

                    //remove it from the deck pile                       
                    deckOnDisplay.Remove(deckCard.name);
                    slotFound = true;
                    //yield break;
                }
            }
            if(!slotFound)
            {
                //if the deck card can't be added to the hand, remove it from the list
                if (computerCards.Count > 0)
                {
                    computerCards.Remove(deckOnDisplay.Last<string>());
                }
            }
        }

        yield return new WaitForSeconds(2f);
        debugHowManyToPresent.text = "Draw from deck";

        //reset computer turn
        compTurn = 1;
        StartCoroutine(CheckWhosTurn());
    }

    public void SkipMyGo()
    {
        //if add the last revealed deck card to the users hand if there's space
        if (deckOnDisplay.Count > 0 && playerCards.Count < 10)
        {
            playerCards.Add(deckOnDisplay.Last<string>());
            GameObject deckCard = GameObject.Find(deckOnDisplay.Last<string>());

            foreach(GameObject slot in playerPos)
            {
                if(slot.transform.childCount == 0)
                {
                    deckCard.transform.position = slot.transform.position;
                    deckCard.transform.parent = slot.transform;
                    deckCard.tag = deckCard.transform.parent.tag;
                    deckCard.GetComponent<UpdateSprite>().faceUp = true;
                }
            }

            deckOnDisplay.Remove(deckCard.name);
        }

        currentlySelectedCards.Clear();
        cardsToChooseFrom.Clear();
        cardsToEnable.Clear();

        debugHowManyToPresent.text = "Turn skipped";
        //reset deck click
        revealedDeck = false;
        ScoreTracker.myTurn = false;
        

        //delete the cards that have been played from the screen
        if (inPlay1.transform.childCount > 0)
        {
            Destroy(inPlay1.transform.GetChild(0).gameObject);
        }
        if (inPlay2.transform.childCount > 0)
        {
            Destroy(inPlay2.transform.GetChild(0).gameObject);
        }
        if (inPlaySpecial.transform.childCount > 0)
        {
            Destroy(inPlaySpecial.transform.GetChild(0).gameObject);
        }

        foreach(string card in computerCards)
        {
            GameObject cardToFind = GameObject.Find(card);
            cardToFind.GetComponent<Image>().color = Color.white;
        }

        StartCoroutine(DelayCheckTurn());
    }

    public void RestartWholeGame()
    {
        SceneManager.LoadScene("StartScene");
    }
}
