using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Class containig player movement logic.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// Movement speed (vertical).
    /// </summary>
    [SerializeField] private float speedMovement;
    /// <summary>
    /// Movement speed (horizontal). Actually, the force used to make the jump.
    /// </summary>
    [SerializeField] private float speedJump;
    /// <summary>
    /// Boolean value if background should follow the player.
    /// </summary>
    [SerializeField] bool backgorundFollows;
    /// <summary>
    /// Body of the player, to collide with ground and objects.
    /// </summary>
    private Rigidbody2D body;
    /// <summary>
    /// Player animator.
    /// </summary>
    private Animator anim;
    /// <summary>
    /// Transform used for moving left.
    /// </summary>
    private Vector3 transformLeft = new Vector3(-4, 4, 1);
    /// <summary>
    /// Transform used for moving right.
    /// </summary>
    private Vector3 transformRight = new Vector3(4, 4, 1);
    /// <summary>
    /// State of colliding with the ground.
    /// </summary>
    private bool grounded;
    /// <summary>
    /// Field to detect horizontal controls.
    /// </summary>
    private float horizontalInput;
    /// <summary>
    /// Class initialization method.
    /// </summary>
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        

    }
    /// <summary>
    /// Method responsible for detecting controls and moving vertically and jumping, changing states of state machine and also for moving background graphics (if should follow the player).
    /// </summary>
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

        if (backgorundFollows && (SceneManager.GetActiveScene().buildIndex==1 || SceneManager.GetActiveScene().buildIndex == 3))
        {
            //background following the player. Like this it works, if I make playerPosition private etc it does not work.
            Vector3 playerPosition = body.position;
            playerPosition.y += 2;
            playerPosition.z = 0;
            GameObject.FindGameObjectWithTag("Background").transform.position = playerPosition;
        }

    }
    /// <summary>
    /// Method responsible for using force to make player jump and for changes in states machine.
    /// </summary>
    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, speedJump);
        anim.SetTrigger("jump");
        grounded = false;
    }
    /// <summary>
    /// Method responsible for detection of colliding with ground and changes in states machine.
    /// </summary>
    /// <param name="collision">Collision with ground event.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            grounded = true;
        }
    }
    /// <summary>
    /// Method to determine if player is able to attack.
    /// </summary>
    /// <returns>Boolean value: true if player is able to attack, false if not.</returns>
    public bool canAttack()
    {
        return horizontalInput==0 && grounded;
    }
}
