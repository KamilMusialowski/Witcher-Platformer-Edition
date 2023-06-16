using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBandit : MonoBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;
    [SerializeField] private int damage;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private float standby;

    [Header("Player Layer")]
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask playerLayer;
    private float cdTimer = Mathf.Infinity;

    [Header("Sounds")]
    [SerializeField] private AudioClip attackSound;

    private Animator anim;
    private Health playerHealth;

    private EnemyPatrol enemyPatrol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
    }

    // Update is called once per frame
    void Update()
    {
        cdTimer += Time.deltaTime;

        //if (PlayerInView())
        //{
        //    anim.SetInteger("AnimState", 1);
        if (PlayerInRange())
        {
            if (cdTimer >= attackCooldown)
            {
                cdTimer = 0;
                anim.SetInteger("AnimState", 1);
                anim.SetTrigger("Attack");
                AudioSource.PlayClipAtPoint(attackSound, transform.position, 1);
                //anim.SetInteger("AnimState", 1);
            }
        }
        //}

        if (enemyPatrol != null)
            //enemyPatrol.enabled = !PlayerInView();
            enemyPatrol.enabled = !PlayerInRange();
    }

    private bool PlayerInRange()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, Vector2.left, 0, playerLayer);

        if (hit.collider != null)
            playerHealth = hit.transform.GetComponent<Health>();

        return hit.collider != null;
    }

    private bool PlayerInView()
    {
        RaycastHit2D ready = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance * standby,
            new Vector3(boxCollider.bounds.size.x * range + standby, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, Vector2.left, 0, playerLayer);

        return ready.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance * standby,
            new Vector3(boxCollider.bounds.size.x * range + standby, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }

    private void DamagePlayer()
    {
        if (PlayerInRange())
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
