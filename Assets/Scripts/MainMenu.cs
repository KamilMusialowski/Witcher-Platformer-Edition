using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public float playerLives = 3;

    public void StartGame()
    {
        PlayerPrefs.SetFloat("PlayerCurrentLives", playerLives);
        PlayerPrefs.SetInt("PlayerCurrentScore", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
