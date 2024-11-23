using UnityEngine;

public class IdleGroundState : State
{
    public override void StateStart()
    {
    }

    public override State StateUpdate()
    {
        return null;
    }

    public override State StateFixedUpdate()
    {
        Player.CollisionChecks();

        if (Player.IsGrounded)
        {
            Player.MoveOnGround(Player.GroundAcceleration, Player.GroundDeceleration, new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
        }
        else
        {
            Player.MoveOnGround(Player.AirAcceleration, Player.AirDeceleration, new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
        }
        
        return null;
    }

    public override void StateExit()
    {
    }
}