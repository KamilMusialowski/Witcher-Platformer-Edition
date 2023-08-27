using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class containing finish-game logic.
/// </summary>
public class FinishGame : MonoBehaviour
{
    /// <summary>
    /// Field containing information if the game (last level) is completed. It prevents the endgame methods to be triggered more than once.
    /// </summary>
    private bool levelComplete = false;

    /// <summary>
    /// Methods that checks if it is the player who triggered the endgame event and if it is not already triggered.
    /// </summary>
    /// <param name="collision">Collision with endgame trigger object.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && levelComplete == false)
        {
            levelComplete = true;
            Invoke("CompleteLevel", 1f);
        }
    }

    /// <summary>
    /// Method that loads endgame screen.
    /// </summary>
    private void CompleteLevel()
    {
        SceneManager.LoadScene(5);
    }
}
