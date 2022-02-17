using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] float speed = 6.0f, jumpForce = 6.0f, attackRange = 0.5f;
    [SerializeField] Transform attackPoint;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] GameObject healthItems;

    private Rigidbody2D body;
    private Animator animator;

    private bool grounded = false, groundedAfterAttack = true, combatIdle = false;
    public bool isDead = false;
    private float initialXScale;
    private int health = 3, defaultDamage = 1;

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

            if (horizontalInput > 0)
                transform.localScale = new Vector3(initialXScale, transform.localScale.y, transform.localScale.z);
            else if (horizontalInput < 0)
                transform.localScale = new Vector3(-1 * initialXScale, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            //stop moving while attacking
            body.velocity = new Vector2(0f, body.velocity.y);
        }

        //Attack
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

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

    private void TriggerHit()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().takeDamage();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
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

    public void takeDamage()
    {
        removeHeart();

        animator.SetTrigger("Hurt");
        health -= defaultDamage;

        if (health <= 0)
        {
            StartCoroutine(die());
        }
    }

    private void removeHeart()
    {
        Transform lastChild = healthItems.transform.GetChild(healthItems.transform.childCount - 1);
        lastChild.gameObject.SetActive(false);
        lastChild.parent = null;
    }

    private IEnumerator die()
    {
        yield return new WaitForSeconds(0.3f);
        animator.SetTrigger("Death");
        isDead = true;
        this.gameObject.layer = LayerMask.NameToLayer("Dead");
        this.enabled = false;
    }
}
