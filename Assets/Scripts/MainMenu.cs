using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class containing main menu logic.
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Number of lifes given to player at the start of the game, due to assumption, that player does not have full-life at the beginning.
    /// </summary>
    public float playerLives = 3;

    /// <summary>
    /// Method responsible for starting the game, when triggered. It sets players life points, sets players score to 0 and loads the first level.
    /// </summary>
    public void StartGame()
    {
        PlayerPrefs.SetFloat("PlayerCurrentLives", playerLives);
        PlayerPrefs.SetInt("PlayerCurrentScore", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
