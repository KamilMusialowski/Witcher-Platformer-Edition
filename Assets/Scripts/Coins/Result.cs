using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containig player result logic.
/// </summary>
public class Result : MonoBehaviour
{
    /// <summary>
    /// Player result field.
    /// </summary>
    public int currentScore { get; private set; }

    /// <summary>
    /// Method that sets the player result to value given by gamestart script.
    /// </summary>
    private void Awake()
    {
        currentScore = PlayerPrefs.GetInt("PlayerCurrentScore"); ;
    }

    /// <summary>
    /// Method responsible for incrementing player result.
    /// </summary>
    /// <param name="score">Parameter needed if the result should be incremented by certain value in the future.</param>
    public void AddScore(int score)
    {
        currentScore++;
        PlayerPrefs.SetInt("PlayerCurrentScore", currentScore);
    }
}
