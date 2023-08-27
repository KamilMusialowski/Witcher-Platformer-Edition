using UnityEngine;
/// <summary>
/// Class containing Spikehead trap logic.
/// </summary>
public class Spikehead : EnemyDamage
{
    /// <summary>
    /// Number of attacks trap can perform.
    /// </summary>
    [SerializeField] private int numberOfAttacksMax;
    /// <summary>
    /// Movement speed.
    /// </summary>
    [SerializeField] private float speed;
    /// <summary>
    /// Attack range.
    /// </summary>
    [SerializeField] private float range;
    /// <summary>
    /// Time of delay between attacks.
    /// </summary>
    [SerializeField] private float checkDelay;
    /// <summary>
    /// Player layer.
    /// </summary>
    [SerializeField] private LayerMask playerLayer;
    /// <summary>
    /// Timer to count the delay time.
    /// </summary>
    private float checkTimer;
    /// <summary>
    /// Destination of attack.
    /// </summary>
    private Vector3 destination;
    /// <summary>
    /// Direstions: Up, Right, Down and Left.
    /// </summary>
    private Vector3[] directions = new Vector3[4];
    /// <summary>
    /// Material field for vanishing animation.
    /// </summary>
    private Material material;
    /// <summary>
    /// Initial trap fade set to 1.0 - trap is fully visible.
    /// </summary>
    private float fade = 1.0f;
    /// <summary>
    /// Boolean field containing information if the trap is attacking.
    /// </summary>
    private bool attacking;
    /// <summary>
    /// Initializing method.
    /// </summary>
    private void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
    }


    /// <summary>
    /// Method triggering trap to stop.
    /// </summary>
    private void OnEnable()
    {
        Stop();
    }
    /// <summary>
    /// Updating method - if the trap is attacking, it calculates its position. If not, it counts time of delay before next attack and then starts checking for the player. If all possible attacks were performed, it sets trap fade to 0 and diactivates the trap object.
    /// </summary>
    private void Update()
    {
        if (attacking)
            transform.Translate(destination * Time.deltaTime * speed);
        else
        {
            checkTimer += Time.deltaTime;
            if(checkTimer > checkDelay && numberOfAttacksMax>0)
            {
                CheckForPlayer();
            }
        }
        if(numberOfAttacksMax <= 0)
        {

            fade -= Time.deltaTime/2;
            if(fade <= 0 )
            {
                fade = 0;
                gameObject.SetActive(false);
            }
            material.SetFloat("_Fade", fade);
        }
    }
    /// <summary>
    /// Checking for player method: it check for the player in 4 directions. If detected, if triggers the next attack.
    /// </summary>
    private void CheckForPlayer()
    {
        CalculateDirections();

        for(int i = 0; i < directions.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directions[i], range, playerLayer);

            if(hit.collider != null && !attacking)
            {
                attacking = true;
                destination = directions[i];
                checkTimer = 0;
                numberOfAttacksMax--;
            }
        }
    }
    /// <summary>
    /// Method to initialize the array of directions.
    /// </summary>
    private void CalculateDirections()
    {
        directions[0] = transform.right * range;
        directions[1] = -transform.right * range;
        directions[2] = transform.up * range;
        directions[3] = -transform.up * range;
    }
    /// <summary>
    /// Method to stop the trap movement.
    /// </summary>
    private void Stop()
    {
        destination = transform.position;
        attacking = false;
    }
    /// <summary>
    /// Method that deals damage to the player nad stops attack for some time after.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        Stop();
    }
}
