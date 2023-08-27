using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class containing enemy projectiles logic.
/// </summary>
public class EnemyProjectile : EnemyDamage
{
    /// <summary>
    /// Projectiles speed.
    /// </summary>
    [SerializeField] private float speed;
    /// <summary>
    /// Time after which projectile sets to inactive.
    /// </summary>
    [SerializeField] private float resetTime;
    /// <summary>
    /// Field used to measure time of projectile being active.
    /// </summary>
    private float lifetime;

    /// <summary>
    /// Method that initializes new projectile.
    /// </summary>
    public void ActivateProjectile()
    {
        lifetime = 0;
        gameObject.SetActive(true);
    }
    /// <summary>
    /// Method responsible for calculating projectiles position and lifetime, also, in proper moment diactivates it.
    /// </summary>
    private void Update()
    {
        float movementSpeed = speed * Time.deltaTime;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if(lifetime > resetTime) 
        {
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Method containing collision logic of projectile.
    /// </summary>
    /// <param name="collision">Collision event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        gameObject.SetActive(false);
    }
}
