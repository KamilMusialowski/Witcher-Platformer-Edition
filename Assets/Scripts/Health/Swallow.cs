using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Swallow (Player Health items) class.
/// </summary>
public class Swallow : MonoBehaviour
{
    /// <summary>
    /// Value to be added as player collides with Swallow.
    /// </summary>
    [SerializeField]private float healthValue;

    /// <summary>
    /// Method to detect Player collision with Swallow and to trigger adding health points.
    /// </summary>
    /// <param name="collision">Collision event with player.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<Health>().AddHealth(healthValue);
            gameObject.SetActive(false);
        }
    }
}
