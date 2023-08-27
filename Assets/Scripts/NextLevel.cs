using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class responsible for activating next level after completing the previous one.
/// </summary>
public class NextLevel : MonoBehaviour
{
    /// <summary>
    /// Field containing information if the level is completed, to prevent triggering endlevel scripts more than once.
    /// </summary>
    private bool levelComplete = false;

    /// <summary>
    /// Method that checks if it was the player who triggered the script and if it is not already triggered.
    /// </summary>
    /// <param name="collision">The collision event that triggered the script.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && levelComplete == false)
        {
            levelComplete = true;
            Invoke("CompleteLevel", 1f);
        }
    }

    /// <summary>
    /// Method that loads the next level.
    /// </summary>
    private void CompleteLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
