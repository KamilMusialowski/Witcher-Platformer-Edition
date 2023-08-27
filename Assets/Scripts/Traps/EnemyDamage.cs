using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing dealing damage to player logic.
/// </summary>
public class EnemyDamage : MonoBehaviour
{
    /// <summary>
    /// Value of damage.
    /// </summary>
    [SerializeField] protected float damage;

    /// <summary>
    /// Method responsible for detecting collisions with player and dealing damage.
    /// </summary>
    /// <param name="collision">Collision event.</param>
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<Health>().TakeDamage(damage);
        }
    }
}
