using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Class containing player health logic.
/// </summary>
public class Health : MonoBehaviour
{
    /// <summary>
    /// Start player health.
    /// </summary>
    private float startingHealth;
    /// <summary>
    /// Max player health.
    /// </summary>
    [SerializeField] private float maxHealth;
    /// <summary>
    /// Current player health.
    /// </summary>
    public float currentHealth { get; private set;  }
    /// <summary>
    /// Number of hearts to display as GUI.
    /// </summary>
    public float maxbarHealth { get; private set; }

    /// <summary>
    /// Player Health initialization method.
    /// </summary>
    private void Awake()
    {
        startingHealth = PlayerPrefs.GetFloat("PlayerCurrentLives");
        currentHealth = startingHealth;
        maxbarHealth = maxHealth;
    }
    /// <summary>
    /// Method responsible for taking damage and decrementation player health, also checks if player is dead.
    /// </summary>
    /// <param name="damage">Damage taken by player.</param>
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        PlayerPrefs.SetFloat("PlayerCurrentLives", currentHealth);

        if (currentHealth > 0)
        {
            //hurt
        }
        else
        {
            SceneManager.LoadScene(4);
        }
    }
    /// <summary>
    /// Method responsible for adding health points - incrementation of players health.
    /// </summary>
    /// <param name="health"></param>
    public void AddHealth(float health)
    {
        currentHealth = Mathf.Clamp(currentHealth + health, 0, maxHealth);
        PlayerPrefs.SetFloat("PlayerCurrentLives", currentHealth);
    }
    /// <summary>
    /// Getter for players health.
    /// </summary>
    /// <returns>Player current health level.</returns>
    public float GetHealth()
    {
        return currentHealth;
    }
}
