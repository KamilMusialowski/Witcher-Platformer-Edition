using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Distance attack projectiles class.
/// </summary>
public class Projectile : MonoBehaviour
{
    /// <summary>
    /// Projectiles speed.
    /// </summary>
    [SerializeField] private float speed;
    /// <summary>
    /// Attack direction.
    /// </summary>
    private float direction;
    /// <summary>
    /// Boolean field to determine if projectile hit.
    /// </summary>
    private bool hit;
    /// <summary>
    /// Time of projectile being active.
    /// </summary>
    private float lifetime;
    /// <summary>
    /// Projectile animator.
    /// </summary>
    private Animator anim;
    /// <summary>
    /// Projectile collider.
    /// </summary>
    private BoxCollider2D boxCollider;
    /// <summary>
    /// Projectile initialization method.
    /// </summary>
    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    /// <summary>
    /// Method to check if the projectile is active (moving).
    /// </summary>
    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0);

        lifetime += Time.deltaTime;
        if (lifetime > 5) gameObject.SetActive(false);
    }
    /// <summary>
    /// Method that detects the collision with object; if it is enemy deals the damage.
    /// </summary>
    /// <param name="collision">Collision event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        boxCollider.enabled = false;
        anim.SetTrigger("explode");
        if(collision.tag == "Enemy")
        {
            collision.GetComponent<MyBandit>().TakeDamage(1);
        }
    }
    /// <summary>
    /// Method that determines attack direction.
    /// </summary>
    /// <param name="_direction">Value that represents if player is attacking left or right.</param>
    public void SetDirection(float _direction)
    {
        lifetime = 0;
        direction = _direction;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;

        transform.localScale = new Vector3(_direction, transform.localScale.y, transform.localScale.z);
    }
    /// <summary>
    /// Method that deactivates the projectile.
    /// </summary>
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}

