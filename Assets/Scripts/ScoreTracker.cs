using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreTracker : MonoBehaviour
{
    public static ScoreTracker instance;
    public static int myScore, compScore;
    public static int numberOfRounds;
    public Text scoreDisplay;

    public static bool myTurn;

    public GameObject userTurnImg, compTurnImg;

    public GameManager gameManager;
    public GameObject confirmButton;
    public GameObject skipButton;

    public GameObject endScreen;
    public Text endText;

    public void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnSceneLoad()
    {
        gameManager = GameObject.Find("Canvas").GetComponent<GameManager>();
        confirmButton = GameObject.Find("Confirm");
        skipButton = GameObject.Find("SkipTurn");
        if (confirmButton)
        {
            confirmButton.SetActive(false);
        }
        endScreen = GameObject.Find("EndScreen");
        endText = GameObject.Find("EndText").GetComponent<Text>();
        if (endScreen)
        {
            endScreen.SetActive(false);
        }
    }

    private void Start()
    {
       if (instance != null)
       {
            Destroy(gameObject);
       }
       else
       {
            instance = this;
            DontDestroyOnLoad(this);
       }
       numberOfRounds = 1;
    }

    private void Update()
    {
        scoreDisplay.text = "You: " + myScore + "\n" + "Comp: " + compScore + "\n" + "Round: " + numberOfRounds;
        if(myTurn)
        {
            userTurnImg.SetActive(true);
            compTurnImg.SetActive(false);
            
            gameManager.debugText.text = "Your turn";
            if (confirmButton)
            {
                confirmButton.SetActive(false);
            }
        }

        else
        {
            if (gameManager)
            {
                if (GameManager.numberForCompToSelectFrom == gameManager.numberOfPlayerCardsToChooseFrom || GameManager.numberForCompToSelectFrom >= gameManager.playerCards.Count - 1)
                {
                    //Debug.Log("Confirm button enabled");
                    confirmButton.SetActive(true);
                }

                else
                {
                    if (confirmButton)
                    {
                        confirmButton.SetActive(false);
                    }
                }

                userTurnImg.SetActive(false);
                compTurnImg.SetActive(true);
                skipButton.GetComponent<Button>().interactable = false;
            }
        }

        if(numberOfRounds > 9)
        {
            endScreen.SetActive(true);

            if(myScore > compScore)
            {
                endText.text = "Congratulations!";
               
            }

            else
            {
                endText.text = "You Lose!";
            }

            numberOfRounds = 1;
            myScore = 0;
            compScore = 0;
        }

        else if (myScore >= 5)
        {
            endScreen.SetActive(true);

            endText.text = "Congratulations!";

            numberOfRounds = 1;
            myScore = 0;
            compScore = 0;
        }

        else if (compScore >= 5)
        {
            endScreen.SetActive(true);
            endText.text = "You Lose!";

            numberOfRounds = 1;
            myScore = 0;
            compScore = 0;
        }
    }
}
