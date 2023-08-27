using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coins logic class.
/// </summary>
public class Coin : MonoBehaviour
{
    /// <summary>
    /// Method responsible for incrementing player result.
    /// </summary>
    /// <param name="collision">The collision with coin event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<Result>().AddScore(1);
            gameObject.SetActive(false);
        }
    }
}
