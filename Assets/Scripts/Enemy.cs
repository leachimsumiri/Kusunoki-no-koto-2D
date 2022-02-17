using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float speed = 3.0f, attackRange = 0.5f, attackSpeed = 0.3f;
    [SerializeField] Transform attackPoint;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] GameObject healthItems;

    private GameObject player;
    private Transform playerTransform;
    private Rigidbody2D body;
    private Animator animator;
    private bool combatIdle = false, isDead = false;
    private float initialXScale;
    private int health = 3, defaultDamage = 1;

    private void Awake()
    {
        player = GameObject.Find("/Player");
        playerTransform = player.transform;
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        initialXScale = transform.localScale.x;
    }

    private void Update()
    {
        if (isDead)
        {
            animator.SetTrigger("Death");
            return;
        }
        else if (player.GetComponent<Player>().isDead)
        {
            animator.SetInteger("AnimState", 0);
            return;
        }

        float playerXPosDiff = getPlayerXPosDiff();

        if (!isAttacking() && !isClose())
        {
            body.velocity = new Vector2(playerXPosDiff * speed, body.velocity.y);

            if (playerXPosDiff < 0)
                transform.localScale = new Vector3(initialXScale, transform.localScale.y, transform.localScale.z);
            else if (playerXPosDiff > 0)
                transform.localScale = new Vector3(-1 * initialXScale, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            //stop moving while attacking
            body.velocity = new Vector2(0f, body.velocity.y);
        }

        //Run
        if (!isClose())
            animator.SetInteger("AnimState", 2);

        //Combat Idle
        else if (combatIdle)
            animator.SetInteger("AnimState", 1);

        //Attack
        else if (!isAttacking())
            StartCoroutine(Attack());

        //Idle
        else if (!isDead)
            animator.SetInteger("AnimState", 0);
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(attackSpeed);
        if (!UnderAttack() && !isDead)
        {
            animator.SetTrigger("Attack");
        }
    }

    private bool isAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }

    private bool UnderAttack()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }

    private float getPlayerXPosDiff()
    {
        return body.position.x < playerTransform.position.x ? 1f : -1f;
    }

    private bool isClose()
    {
        float diff = body.position.x - playerTransform.position.x;
        return diff < 1.0f && diff > -1.0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void TriggerHit()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Player>().takeDamage();
        }
    }

    public void takeDamage()
    {
        removeHeart();

        animator.SetTrigger("Hurt");
        animator.SetInteger("AnimState", 0);
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
