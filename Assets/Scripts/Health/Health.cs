using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Health : MonoBehaviour
{
    private float startingHealth;
    [SerializeField] private float maxHealth;
    public float currentHealth { get; private set;  }
    public float maxbarHealth { get; private set; }

    private void Awake()
    {
        startingHealth = PlayerPrefs.GetFloat("PlayerCurrentLives");
        currentHealth = startingHealth;
        maxbarHealth = maxHealth;
    }

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
    
    public void AddHealth(float health)
    {
        currentHealth = Mathf.Clamp(currentHealth + health, 0, maxHealth);
        PlayerPrefs.SetFloat("PlayerCurrentLives", currentHealth);
    }

    public float GetHealth()
    {
        return currentHealth;
    }
}
