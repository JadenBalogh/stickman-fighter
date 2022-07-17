using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Actor : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10f;

    public ActorState State { get; private set; }
    public float Health { get; private set; }

    public UnityEvent OnDeath { get; private set; }

    protected new Rigidbody2D rigidbody2D;

    protected virtual void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        Health = maxHealth;
        OnDeath = new UnityEvent();
    }

    protected virtual void Update()
    {
        if (transform.position.y < -5f)
        {
            Die();
        }
    }

    public virtual void TakeDamage(float damage)
    {
        Health = Mathf.Max(Health - damage, 0);
        if (Health <= 0)
        {
            Die();
        }
    }

    public virtual void Knockback(Vector2 pushForce, float duration)
    {
        rigidbody2D.AddForce(pushForce, ForceMode2D.Impulse);
        StartCoroutine(_ApplyState(ActorState.Hurt, duration));
    }

    public virtual void Stun(float duration)
    {
        StartCoroutine(_ApplyState(ActorState.Stunned, duration));
    }

    protected virtual IEnumerator _ApplyState(ActorState state, float duration)
    {
        SetState(state);
        yield return new WaitForSeconds(duration);
        SetState(ActorState.Normal);
    }

    protected virtual void SetState(ActorState state)
    {
        State = state;
    }

    protected bool IsInputLocked()
    {
        return State == ActorState.Hurt || State == ActorState.Stunned;
    }

    public virtual void Die()
    {
        OnDeath.Invoke();
    }

    public enum ActorState
    {
        Normal, Hurt, Stunned
    }
}
