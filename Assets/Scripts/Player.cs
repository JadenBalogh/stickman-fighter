using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Actor
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;

    [Header("Combat")]
    [SerializeField] private PlayerAttack[] attacks;
    [SerializeField] private float attackDist = 1f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation")]
    [SerializeField] private Animation2D idleAnim;
    [SerializeField] private Animation2D moveAnim;
    [SerializeField] private Animation2D hurtAnim;
    [SerializeField] private Animation2D stunnedAnim;

    private bool canAttack = true;
    private Vector2 facingDir = Vector2.right;

    private Animator2D animator2D;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        animator2D = GetComponent<Animator2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameManager.SetPlayer(this);
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

        float inputX = Input.GetAxis("Horizontal");
        rigidbody2D.velocity = Vector2.right * inputX * moveSpeed + Vector2.up * rigidbody2D.velocity.y;

        animator2D.Play(inputX != 0 ? moveAnim : idleAnim, true);

        if (inputX != 0)
        {
            facingDir = Vector2.right * Mathf.Sign(inputX);
            spriteRenderer.flipX = rigidbody2D.velocity.x < 0;
        }

        foreach (PlayerAttack attack in attacks)
        {
            if (canAttack && Input.GetButtonDown(attack.inputButton))
            {
                StartCoroutine(_AttackCooldown(attack.duration));
                animator2D.Play(attack.anim, false, true);
                CameraController.ShakeScreen(attack.screenShakeAmount);

                RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one, 0, facingDir, attackDist, enemyLayer);
                if (hit && hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
                {
                    int numStacks = enemy.ComboStacks;
                    float damage = attack.damage * (1 + attack.damageComboMult * numStacks);
                    float knockbackForce = attack.knockbackForce * (1 + attack.knockbackForceComboMult * numStacks);
                    float knockbackDuration = attack.knockbackDuration * (1 + attack.knockbackDurationComboMult * numStacks);

                    if (numStacks >= attack.stunStackThreshold)
                    {
                        float stunDuration = attack.stunDuration * (1 + attack.stunDurationComboMult * numStacks);
                        enemy.Stun(stunDuration);
                    }

                    enemy.TakeDamage(damage);
                    enemy.Knockback(facingDir * knockbackForce, knockbackDuration);
                    enemy.AddComboStacks(attack.appliedComboStacks);
                }
            }
        }
    }

    protected override void SetState(ActorState state)
    {
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

    private IEnumerator _AttackCooldown(float duration)
    {
        canAttack = false;
        yield return new WaitForSeconds(duration);
        canAttack = true;
    }

    public override void Die()
    {
        base.Die();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [System.Serializable]
    public class PlayerAttack
    {
        public string inputButton = "Fire1";
        public float duration = 0.3f;
        public float damage = 1f;
        public float damageComboMult = 0f;
        public float knockbackForce = 3f;
        public float knockbackForceComboMult = 0f;
        public float knockbackDuration = 0.5f;
        public float knockbackDurationComboMult = 0f;
        public int stunStackThreshold = 5;
        public float stunDuration = 1f;
        public float stunDurationComboMult = 0f;
        public float screenShakeAmount = 1f;
        public int appliedComboStacks = 1;
        public Animation2D anim;
    }
}
