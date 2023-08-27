using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Class responsible for death-screen events. 
/// </summary>
public class Death : MonoBehaviour
{
    /// <summary>
    /// Method responsible for loading the main menu screen, when triggered.
    /// </summary>
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
}
