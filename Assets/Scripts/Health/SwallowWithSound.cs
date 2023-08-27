using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class responsible for sound effect fo colliding with Swallow.
/// </summary>
public class SwallowWithSound : MonoBehaviour
{
    /// <summary>
    /// Value to be added as player collides with Swallow.
    /// </summary>
    [SerializeField] private float healthValue;
    /// <summary>
    /// Sound for succesful gaining health.
    /// </summary>
    [SerializeField] private AudioClip drinkSuccess;
    /// <summary>
    /// Sound for not gaining health.
    /// </summary>
    [SerializeField] private AudioClip drinkFail;

    /// <summary>
    /// Method to detect Player collision with Swallow and to trigger adding health points.
    /// </summary>
    /// <param name="collision">Collision event with player.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (collision.GetComponent<Health>().currentHealth < collision.GetComponent<Health>().maxbarHealth)
            {
                collision.GetComponent<Health>().AddHealth(healthValue);
                AudioSource.PlayClipAtPoint(drinkSuccess, transform.position, 2);
            }
            else
                AudioSource.PlayClipAtPoint(drinkFail, transform.position, 2);
            
            gameObject.SetActive(false);
        }
    }
}
