using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for soung when coin collected.
/// </summary>
public class CoinWithSound : MonoBehaviour
{
    /// <summary>
    /// Collecting sound field.
    /// </summary>
    public AudioClip sound;

    /// <summary>
    /// Method responsible for playing the sound when coin is collected.
    /// </summary>
    /// <param name="collision">Collision with coin event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            AudioSource.PlayClipAtPoint(sound, transform.position, 1);
            collision.GetComponent<Result>().AddScore(1);
            gameObject.SetActive(false);
        }
    }
}
