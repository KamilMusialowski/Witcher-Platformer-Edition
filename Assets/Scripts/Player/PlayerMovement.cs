using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedMovement;
    [SerializeField] private float speedJump;
    [SerializeField] bool backgorundFollows;
    private Rigidbody2D body;
    private Animator anim;
    private Vector3 transformLeft = new Vector3(-4, 4, 1);
    private Vector3 transformRight = new Vector3(4, 4, 1);
    private bool grounded;
    private float horizontalInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        

    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        body.velocity = new Vector3(horizontalInput*speedMovement, body.velocity.y);

        //Flipping right and left
        if(horizontalInput > 0.01f)
        {
            transform.localScale = transformRight;
        } else if (horizontalInput < -0.01f)
        {
            transform.localScale = transformLeft;
        }


        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }

        //Set animator params
        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", grounded);

        if (backgorundFollows && SceneManager.GetActiveScene().buildIndex==1)
        {
            //background following the player. Like this it works, if I make playerPosition private etc it does not work.
            Vector3 playerPosition = body.position;
            playerPosition.y += 2;
            playerPosition.z = 0;
            GameObject.FindGameObjectWithTag("Background").transform.position = playerPosition;
        }

    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, speedJump);
        anim.SetTrigger("jump");
        grounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            grounded = true;
        }
    }

    public bool canAttack()
    {
        return horizontalInput==0 && grounded;
    }
}
