using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class containing firetrap logic.
/// </summary>
public class Firetrap : MonoBehaviour
{
    /// <summary>
    /// Damage dealed by the trap.
    /// </summary>
    [Header("Firetrap Damage")]
    [SerializeField] private float damage;
    /// <summary>
    /// Time between triggering and activating the trap.
    /// </summary>
    [Header("Firetrap Timers")]
    [SerializeField] private float activationDelay;
    /// <summary>
    /// Time of trap being active.
    /// </summary>
    [SerializeField] private float activeTime;
    /// <summary>
    /// Trap animator.
    /// </summary>
    private Animator anim;
    /// <summary>
    /// Trap renderer.
    /// </summary>
    private SpriteRenderer spriteRenderer;
    /// <summary>
    /// Boolean field containing information if the trap is triggered.
    /// </summary>
    private bool triggered;
    /// <summary>
    /// Boolean field containing information if the trap is active.
    /// </summary>
    private bool active;
    /// <summary>
    /// Initializing method.
    /// </summary>
    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    /// <summary>
    /// Method to detect the collisions with player - first collision sets trap to trigger, after delay time trap sets to active for some time, when some next collision with player results with dealing the damage to him.
    /// </summary>
    /// <param name="collision">Collision event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if(!triggered)
            {
                StartCoroutine(ActivateFiretrap());
            }
            if(active)
            {
                collision.GetComponent<Health>().TakeDamage(damage);
            }
        }
    }
    /// <summary>
    /// Corutine responsible for delaying activation of the trap, and then diactivating it after proper time.
    /// </summary>
    /// <returns>Corutine pouse event.</returns>
    private IEnumerator ActivateFiretrap()
    {
        triggered = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(activationDelay);
        spriteRenderer.color = Color.white;
        active = true;
        anim.SetBool("activated", true);

        yield return new WaitForSeconds(activeTime);
        active = false;
        triggered = false;
        anim.SetBool("activated", false);
    }
}
