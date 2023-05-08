using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set;  }

    private void Awake()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, startingHealth);

        if(currentHealth > 0)
        {
            //hurt
        }
        else
        {
            //dead
        }
    }
    
    public void AddHealth(float health)
    {
        currentHealth = Mathf.Clamp(currentHealth + health, 0, startingHealth);
    }
}
