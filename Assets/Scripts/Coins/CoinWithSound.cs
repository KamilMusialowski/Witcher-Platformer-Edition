using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinWithSound : MonoBehaviour
{
    public AudioClip sound;

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
