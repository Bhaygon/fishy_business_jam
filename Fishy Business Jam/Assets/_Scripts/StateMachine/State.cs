using UnityEngine;

public abstract class State : MonoBehaviour
{
    public bool isComplete { get; protected set; }
    protected float startTime;
    public float time => Time.time - startTime;
    
    public Animator animator;
    public Rigidbody2D body;
    
    public virtual void StateStart()
    {
    }

    public virtual State StateUpdate()
    {
        return null;
    }

    public virtual State StateFixedUpdate()
    {
        return null;
    }

    public virtual void StateExit()
    {
    }
}