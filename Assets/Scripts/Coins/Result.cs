using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result : MonoBehaviour
{
    public int currentScore { get; private set; }

    private void Awake()
    {
        currentScore = PlayerPrefs.GetInt("PlayerCurrentScore"); ;
    }

    public void AddScore(int score)
    {
        currentScore++;
        PlayerPrefs.SetInt("PlayerCurrentScore", currentScore);
    }
}
