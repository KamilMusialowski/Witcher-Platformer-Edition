using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing arrow trao logic.
/// </summary>
public class Arrowtrap : MonoBehaviour
{
    /// <summary>
    /// Time between shots.
    /// </summary>
    [SerializeField] private float attackCooldown;
    /// <summary>
    /// Point of spawning arrows.
    /// </summary>
    [SerializeField] private Transform firePoint;
    /// <summary>
    /// Array of arrows objects.
    /// </summary>
    [SerializeField] private GameObject[] arrows;
    /// <summary>
    /// Timer to count time between shots.
    /// </summary>
    private float cooldownTimer;

    /// <summary>
    /// Attack initialization method - resets timer and sets new arrows position to firepoints and activates it.
    /// </summary>
    private void Attack()
    {
        cooldownTimer = 0;

        arrows[FindArrow()].transform.position = firePoint.position;
        arrows[FindArrow()].GetComponent<EnemyProjectile>().ActivateProjectile();
    }
    /// <summary>
    /// Method to find active arrows.
    /// </summary>
    /// <returns>Index of active arrow.</returns>
    private int FindArrow()
    {
        for(int i = 0; i < arrows.Length; i++)
        {
            if (!arrows[i].activeInHierarchy) return i;
        }
        return 0;
    }
    /// <summary>
    /// Method counting time between shots - cooldown time passed, triggers attack script.
    /// </summary>
    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if(cooldownTimer >= attackCooldown)
        {
            Attack();
        }
    }
}
