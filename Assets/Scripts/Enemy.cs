using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Actor
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;

    [Header("Combat")]
    [SerializeField] private float comboStackDuration = 1f;
    [SerializeField] private float attackInterval = 0.3f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackKnockbackForce = 2f;
    [SerializeField] private float attackKnockbackDuration = 0.2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Animation")]
    [SerializeField] private Animation2D idleAnim;
    [SerializeField] private Animation2D hurtAnim;
    [SerializeField] private Animation2D moveAnim;
    [SerializeField] private Animation2D stunnedAnim;
    [SerializeField] private Animation2D attackAnim;

    public int ComboStacks { get; private set; }

    private bool canAttack = true;
    private Coroutine comboStackCoroutine;

    private Animator2D animator2D;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        animator2D = GetComponent<Animator2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();

        if (IsInputLocked())
        {
            if (State == ActorState.Hurt)
            {
                animator2D.Play(hurtAnim, true);
            }
            else if (State == ActorState.Stunned)
            {
                animator2D.Play(stunnedAnim, true);
            }
            return;
        }

        Vector2 playerOffset = GameManager.Player.transform.position - transform.position;
        Vector2 playerDir = Vector2.right * Mathf.Sign(playerOffset.x);
        float playerDist = Mathf.Abs(playerOffset.x);

        rigidbody2D.velocity = playerDir * moveSpeed + Vector2.up * rigidbody2D.velocity.y;

        animator2D.Play(playerDir.x != 0 ? moveAnim : idleAnim, true);

        if (playerDir.x != 0)
        {
            spriteRenderer.flipX = rigidbody2D.velocity.x < 0;
        }

        if (canAttack && playerDist <= attackRange)
        {
            StartCoroutine(_AttackCooldown(attackInterval));
            animator2D.Play(attackAnim, false, true);

            RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one, 0, playerDir, attackRange, playerLayer);
            if (hit && hit.collider.TryGetComponent<Player>(out Player player))
            {
                player.TakeDamage(attackDamage);
                player.Knockback(playerDir * attackKnockbackForce, attackKnockbackDuration);
            }
        }
    }

    protected override void SetState(ActorState state)
    {
        if (State == ActorState.Stunned && state != ActorState.Normal)
        {
            return;
        }
        
        base.SetState(state);

        if (state == ActorState.Hurt)
        {
            animator2D.Play(hurtAnim, false, true);
        }
        else if (state == ActorState.Stunned)
        {
            animator2D.Play(stunnedAnim, false, true);
        }
    }

    public void AddComboStacks(int stacks)
    {
        ComboStacks += stacks;

        if (comboStackCoroutine != null) StopCoroutine(comboStackCoroutine);
        comboStackCoroutine = StartCoroutine(_ComboStackDuration());
    }

    private IEnumerator _ComboStackDuration()
    {
        yield return new WaitForSeconds(comboStackDuration);
        ComboStacks = 0;
    }

    private IEnumerator _AttackCooldown(float duration)
    {
        canAttack = false;
        yield return new WaitForSeconds(duration);
        canAttack = true;
    }

    public override void Die()
    {
        base.Die();
        Destroy(gameObject);
    }
}
