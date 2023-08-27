using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing saw trap logic.
/// </summary>
public class Enemy_Sideways : MonoBehaviour
{
    /// <summary>
    /// Distance which saw can move in both vertical directions.
    /// </summary>
    [SerializeField] private float movementDistance;
    /// <summary>
    /// Movements speed.
    /// </summary>
    [SerializeField] private float movementSpeed;
    /// <summary>
    /// Dealing damage.
    /// </summary>
    [SerializeField] private float damage;
    /// <summary>
    /// Boolean value - true if saw is moving left.
    /// </summary>
    private bool movingLeft;
    /// <summary>
    /// Left edge of move area.
    /// </summary>
    private float leftEdge;
    /// <summary>
    /// Right edge of move area.
    /// </summary>
    private float rightEdge;
    /// <summary>
    /// Initialization method - sets the edges of move area.
    /// </summary>
    private void Awake()
    {
        leftEdge = transform.position.x - movementDistance;
        rightEdge = transform.position.x + movementDistance;
    }
    /// <summary>
    /// Method responsible for moving the saw - if reaches the move area edge - changes the direction.
    /// </summary>
    private void Update()
    {
        if (movingLeft)
        {
            if(transform.position.x > leftEdge)
            {
                transform.position = new Vector3(transform.position.x - movementSpeed * 
                    Time.deltaTime, transform.position.y, transform.position.z);
            }
            else
            {
                movingLeft = false;
            }
        }
        else
        {
            if (transform.position.x < rightEdge)
            {
                transform.position = new Vector3(transform.position.x + movementSpeed *
                    Time.deltaTime, transform.position.y, transform.position.z);
            }
            else
            {
                movingLeft = true;
            }
        }
    }

    /// <summary>
    /// Method responsible for detecting collision with player and dealing damage.
    /// </summary>
    /// <param name="collision">Collision event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<Health>().TakeDamage(damage);
        }
    }
}
