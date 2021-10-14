using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] float speed = 6.0f;
    [SerializeField] float jumpForce = 6.0f;

    private Rigidbody2D body;
    private Animator animator;
    private bool grounded = false;
    private bool groundedAfterAttack = true;
    private bool combatIdle = false;
    private bool isDead = false;
    private float initialXScale;
 
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        initialXScale = transform.localScale.x;
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (!isAttacking() && groundedAfterAttack)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
            animator.SetFloat("AirSpeed", body.velocity.y);

            if (horizontalInput < 0)
                transform.localScale = new Vector3(initialXScale, transform.localScale.y, transform.localScale.z);
            else if (horizontalInput > 0)
                transform.localScale = new Vector3(-1 * initialXScale, transform.localScale.y, transform.localScale.z);
        } else
        {
            //stop moving while attacking
            body.velocity = new Vector2(0f, body.velocity.y);
        }

        // -- Handle Animations --
        //Death
        if (Input.GetKeyDown("e"))
        {
            if (!isDead)
                animator.SetTrigger("Death");
            else
                animator.SetTrigger("Recover");

            isDead = !isDead;
        }

        //Hurt
        else if (Input.GetKeyDown("q"))
            animator.SetTrigger("Hurt");

        //Attack
        else if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        //Change between idle and combat idle
        else if (Input.GetKeyDown("f"))
            combatIdle = !combatIdle;

        //Jump
        else if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }

        //Run
        else if (Mathf.Abs(horizontalInput) > Mathf.Epsilon)
            animator.SetInteger("AnimState", 2);

        //Combat Idle
        else if (combatIdle)
            animator.SetInteger("AnimState", 1);

        //Idle
        else
            animator.SetInteger("AnimState", 0);
    }

    private void Jump()
    {
        animator.SetTrigger("Jump");
        grounded = false;
        animator.SetBool("Grounded", grounded);
        body.velocity = new Vector2(body.velocity.x, jumpForce);
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
        if (!grounded)
        {
            groundedAfterAttack = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            grounded = true;
            groundedAfterAttack = true;
            animator.SetBool("Grounded", grounded);
        }
    }

    private bool isAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }
}
