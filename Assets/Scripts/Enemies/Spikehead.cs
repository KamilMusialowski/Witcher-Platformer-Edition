using UnityEngine;

public class Spikehead : EnemyDamage
{
    [SerializeField] private int numberOfAttacksMax;
    [SerializeField] private float speed;
    [SerializeField] private float range;
    [SerializeField] private float checkDelay;
    [SerializeField] private LayerMask playerLayer;
    private float checkTimer;
    private Vector3 destination;
    private Vector3[] directions = new Vector3[4];
    private Material material;
    private float fade = 1.0f;

    private bool attacking;

    private void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
    }



    private void OnEnable()
    {
        Stop();
    }

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

    private void CheckForPlayer()
    {
        CalculateDirections();

        for(int i = 0; i < directions.Length; i++)
        {
            //Debug.DrawRay(transform.position, directions[i], Color.red);
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

    private void CalculateDirections()
    {
        directions[0] = transform.right * range;
        directions[1] = -transform.right * range;
        directions[2] = transform.up * range;
        directions[3] = -transform.up * range;
    }

    private void Stop()
    {
        destination = transform.position;
        attacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        Stop();
    }
}
