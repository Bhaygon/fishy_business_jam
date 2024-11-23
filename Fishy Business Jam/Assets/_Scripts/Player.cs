using UnityEngine;

public class Player : MonoBehaviour
{
    public State Air, Ground, IdleGround, Interact, Run, Swim;
    [Space]
    public Rigidbody2D RB;
    public Collider2D Collider, ColliderSwim;
}
