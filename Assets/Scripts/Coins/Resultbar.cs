using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class responsible for displaying player result during the game.
/// </summary>
public class Resultbar : MonoBehaviour
{
    /// <summary>
    /// Field containig result as text to be displayed.
    /// </summary>
    [SerializeField] private Text ScoreText;
    /// <summary>
    /// Field for Result class object.
    /// </summary>
    [SerializeField] private Result PlayerResult;

    /// <summary>
    /// Initial display method.
    /// </summary>
    private void Start()
    {
        ScoreText.text = PlayerResult.currentScore.ToString();
    }
    /// <summary>
    /// Displayed result update method.
    /// </summary>
    private void Update()
    {
        ScoreText.text = PlayerResult.currentScore.ToString();
    }
}
