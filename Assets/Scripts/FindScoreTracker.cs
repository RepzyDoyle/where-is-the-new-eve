using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindScoreTracker : MonoBehaviour
{
    public ScoreTracker scoreTracker;

    public void Start()
    {
        scoreTracker = GameObject.Find("ScoreTracker").GetComponent<ScoreTracker>();
    }
    public void LoadMainSceneOnScoreTracker()
    {
        scoreTracker.LoadMainScene();
    }
}
