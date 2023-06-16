using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwallowWithSound : MonoBehaviour
{
    [SerializeField] private float healthValue;
    [SerializeField] private AudioClip drinkSuccess;
    [SerializeField] private AudioClip drinkFail;

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
