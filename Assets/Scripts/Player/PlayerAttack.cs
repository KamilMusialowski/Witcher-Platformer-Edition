//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerAttack : MonoBehaviour
//{
//    [SerializeField] private float attackCooldown;
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private GameObject[] ignis;

//    private Animator anim;
//    private PlayerMovement playerMovement;
//    private float cooldownTimer = Mathf.Infinity;

//    private void Awake()
//    {
//        anim = GetComponent<Animator>();
//        playerMovement = GetComponent<PlayerMovement>();
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.H) && cooldownTimer > attackCooldown && playerMovement.canAttack())
//            Attack();

//        cooldownTimer += Time.deltaTime;
//    }

//    private void Attack()
//    {
//        anim.SetTrigger("attack");
//        cooldownTimer = 0;

//        ignis[FindIgni()].transform.position = firePoint.position;
//        ignis[FindIgni()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
//    }

//    private int FindIgni()
//    {
//        for (int i = 0; i < ignis.Length; i++)
//        {
//            if (!ignis[i].activeInHierarchy)
//                return i;
//        }
//        return 0;
//    }

//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] ignis;

    private Animator anim;
    private BoxCollider2D boxCollider;
    private float cooldownTimer = Mathf.Infinity;

    private float swordRange = 0.5f;
    public LayerMask enemyLayer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

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

    private void Attack()
    {
        anim.SetTrigger("attack");
        cooldownTimer = 0;

        GameObject newIgni = Instantiate(ignis[FindIgni()], firePoint.position, Quaternion.identity);
        Projectile projectile = newIgni.GetComponent<Projectile>();

        // Ustalanie kierunku na podstawie skali lokalnej gracza
        float direction = Mathf.Sign(transform.localScale.x);
        projectile.SetDirection(direction);
    }

    private int FindIgni()
    {
        for (int i = 0; i < ignis.Length; i++)
        {
            if (!ignis[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (this.swordCollision && collision.tag == "Enemy")
    //        collision.GetComponent<MyBandit>().TakeDamage(1);
    //}


}

