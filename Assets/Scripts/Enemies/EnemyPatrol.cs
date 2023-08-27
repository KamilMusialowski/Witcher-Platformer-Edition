using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containig enemies patrolling logic.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    /// <summary>
    /// Left edge of patrolling area field.
    /// </summary>
    [Header("Patrol Points")]
    [SerializeField] private Transform leftEdge;
    /// <summary>
    /// Right edge of patrolling area field.
    /// </summary>
    [SerializeField] private Transform rightEdge;

    /// <summary>
    /// Current transform (position etc.) of enemy.
    /// </summary>
    [Header("Enemy")]
    [SerializeField] private Transform enemy;

    /// <summary>
    /// Speed of patroll movement field.
    /// </summary>
    [Header("Movement parameters")]
    [SerializeField] private float speed;
    /// <summary>
    /// Scale of enemy ingame.
    /// </summary>
    private Vector3 initScale;
    /// <summary>
    /// Field containing information about enemys movement direction.
    /// </summary>
    private bool movingLeft;
    /// <summary>
    /// Field for idle animation.
    /// </summary>
    [Header("Idle Behaviour")]
    [SerializeField] private float idleDuration;
    /// <summary>
    /// Field for idle animation duration.
    /// </summary>
    private float idleTimer;
    /// <summary>
    /// Enemy Animator object field.
    /// </summary>
    [Header("Enemy Animator")]
    [SerializeField] private Animator anim;

    /// <summary>
    /// Initailization of enemy.
    /// </summary>
    private void Awake()
    {
        initScale = enemy.localScale;
    }
    /// <summary>
    /// Method that reacts to disabling the enemy.
    /// </summary>
    private void OnDisable()
    {
        anim.SetInteger("AnimState", 0);
    }
    /// <summary>
    /// Method responsible for enemy movement - the direction and movement and standing itself.
    /// </summary>
    private void Update()
    {
        if (movingLeft)
        {
            if (enemy.position.x >= leftEdge.position.x)
                MoveInDirection(-1);
            else
                DirectionChange();
        }
        else
        {
            if (enemy.position.x <= rightEdge.position.x)
                MoveInDirection(1);
            else
                DirectionChange();
        }
    }
    /// <summary>
    /// Method that changes the patrolling direction and flips the enemy.
    /// </summary>
    private void DirectionChange()
    {
        anim.SetInteger("AnimState", 0);

        idleTimer += Time.deltaTime;

        if(idleTimer > idleDuration)
            movingLeft = !movingLeft;
    }
    /// <summary>
    /// Method responsible for moving the enemy.
    /// </summary>
    /// <param name="direction">Direction movement.</param>
    private void MoveInDirection(int direction)
    {
        idleTimer = 0;
        anim.SetInteger("AnimState", 2);

        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * direction, initScale.y, initScale.z);
        enemy.position = new Vector3(enemy.position.x + Time.deltaTime * direction * speed,
            enemy.position.y, enemy.position.z);
    }


}
