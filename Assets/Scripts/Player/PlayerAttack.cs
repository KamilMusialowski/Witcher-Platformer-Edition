using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing player attacks logic.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    /// <summary>
    /// Time between possible player attacks.
    /// </summary>
    [SerializeField] private float attackCooldown;
    /// <summary>
    /// Point of attack (spawning distance attack or detecting enemy in close combat attack).
    /// </summary>
    [SerializeField] private Transform firePoint;
    /// <summary>
    /// Array of distance attack projectiles.
    /// </summary>
    [SerializeField] private GameObject[] ignis;
    /// <summary>
    /// Player animator.
    /// </summary>
    private Animator anim;
    /// <summary>
    /// Player collider.
    /// </summary>
    private BoxCollider2D boxCollider;
    /// <summary>
    /// Attack cooldown timer.
    /// </summary>
    private float cooldownTimer = Mathf.Infinity;
    /// <summary>
    /// Range of sword attack.
    /// </summary>
    private float swordRange = 0.5f;
    /// <summary>
    /// Enemies layer.
    /// </summary>
    public LayerMask enemyLayer;
    /// <summary>
    /// Class initialization method.
    /// </summary>
    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    /// <summary>
    /// Method to detects attacks buttons being pressed and triggering attack scripts.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && cooldownTimer > attackCooldown)
        {
            Attack();
            
        }
        cooldownTimer += Time.deltaTime;


        if (Input.GetKeyDown(KeyCode.U))
            StartCoroutine(SwordAttack());

        
    }
    /// <summary>
    /// Sword attack method, implemented as corutine to delay dealing damage to enemy (to synchronize it with animation).
    /// </summary>
    /// <returns>Corutine delay event.</returns>
    private IEnumerator SwordAttack()
    {
        anim.SetTrigger("swordAttack");
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, swordRange, enemyLayer);
        yield return new WaitForSeconds(0.5f);
        foreach (Collider2D enemy in  hitEnemies)
        {
            enemy.GetComponent<MyBandit>().TakeDamage(2);
        }
    }
    /// <summary>
    /// Method responsible for distance attack. Harder part was to determine the attack direction once and not to change it as player changes his direction.
    /// </summary>
    private void Attack()
    {
        anim.SetTrigger("attack");
        cooldownTimer = 0;

        GameObject newIgni = Instantiate(ignis[FindIgni()], firePoint.position, Quaternion.identity);
        Projectile projectile = newIgni.GetComponent<Projectile>();

        float direction = Mathf.Sign(transform.localScale.x);
        projectile.SetDirection(direction);
    }

    /// <summary>
    /// Method to determine if the projectile is active in game.
    /// </summary>
    /// <returns>If exists and attacking, the index of projectile. If not, 0.</returns>
    private int FindIgni()
    {
        for (int i = 0; i < ignis.Length; i++)
        {
            if (!ignis[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}

