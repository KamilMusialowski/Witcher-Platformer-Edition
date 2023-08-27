using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing bandits logic.
/// </summary>
public class MyBandit : MonoBehaviour
{
    /// <summary>
    /// Bandits max health level.
    /// </summary>
    [Header("Bandit Health")]
    [SerializeField] private float maxHealth;
    /// <summary>
    /// Bandit health field.
    /// </summary>
    private float currentHealth;

    /// <summary>
    /// Attack cooldown time.
    /// </summary>
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    /// <summary>
    /// Attack range.
    /// </summary>
    [SerializeField] private float range;
    /// <summary>
    /// Attack damage.
    /// </summary>
    [SerializeField] private int damage;
    /// <summary>
    /// Detecting player parameters.
    /// </summary>
    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    /// <summary>
    /// Multiplier to extend the detection range of the bandit's line of sight for detecting the player. 
    /// </summary>
    [SerializeField] private float standby;
    /// <summary>
    /// Bandit collider.
    /// </summary>
    [Header("Player Layer")]
    [SerializeField] private BoxCollider2D boxCollider;
    /// <summary>
    /// Player layer for detecting collisions and attacking.
    /// </summary>
    [SerializeField] private LayerMask playerLayer;
    /// <summary>
    /// Attacks timer.
    /// </summary>
    private float cdTimer = Mathf.Infinity;
    /// <summary>
    /// Attack sounds files field.
    /// </summary>
    [Header("Sounds")]
    [SerializeField] private AudioClip attackSound;
    /// <summary>
    /// Bandit animator.
    /// </summary>
    private Animator anim;
    /// <summary>
    /// Player health class object field.
    /// </summary>
    private Health playerHealth;
    /// <summary>
    /// EnemyPatrol class object field.
    /// </summary>
    private EnemyPatrol enemyPatrol;
    /// <summary>
    /// Bandit initialization method.
    /// </summary>
    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        this.currentHealth = this.maxHealth;
    }

    /// <summary>
    /// Method responsible for player detection reactions and diactivating the bandit if dead.
    /// </summary>
    void Update()
    {
        cdTimer += Time.deltaTime;

        if (PlayerInRange())
        {
            if (cdTimer >= attackCooldown)
            {
                cdTimer = 0;
                anim.SetInteger("AnimState", 1);
                anim.SetTrigger("Attack");
                AudioSource.PlayClipAtPoint(attackSound, transform.position, 1);
            }
        }

        if (enemyPatrol != null)
            enemyPatrol.enabled = !PlayerInRange();

        if(this.currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Method responsible for taking damage.
    /// </summary>
    /// <param name="damage">Damage to dicreament bandit health.</param>
    public void TakeDamage(float damage)
    {
        this.currentHealth -= damage;
        if (this.currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Method to check if player can be attacked.
    /// </summary>
    /// <returns>If player can be attacked, returns true.</returns>
    private bool PlayerInRange()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, Vector2.left, 0, playerLayer);

        if (hit.collider != null)
            playerHealth = hit.transform.GetComponent<Health>();

        return hit.collider != null;
    }
    /// <summary>
    /// Method to check if bandit can see the player.
    /// </summary>
    /// <returns>If bandit can see the player, returns true.</returns>
    private bool PlayerInView()
    {
        RaycastHit2D ready = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance * standby,
            new Vector3(boxCollider.bounds.size.x * range + standby, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, Vector2.left, 0, playerLayer);

        return ready.collider != null;
    }
    /// <summary>
    /// Help method to draw sight and attack range lines.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance * standby,
            new Vector3(boxCollider.bounds.size.x * range + standby, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }
    /// <summary>
    /// Method responsible for dealing the damage to player.
    /// </summary>
    private void DamagePlayer()
    {
        if (PlayerInRange())
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
