using UnityEngine;

public class MovingState : State
{
    public override void StateStart()
    {
    }

    public override State StateUpdate()
    {
        Player.CountTimers();
        Player.JumpChecks();

        return null;
    }

    public override State StateFixedUpdate()
    {
        var rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Player.CollisionChecks();
        Player.Jump();

        if (Player.IsGrounded())
        {
            Player.Move(Player.GroundAcceleration, Player.GroundDeceleration, rawInput);
        }
        else
        {
            Player.Move(Player.AirAcceleration, Player.AirDeceleration, rawInput);
        }

        return null;
    }

    public override void StateExit()
    {
    }
}