using UnityEngine;

public class AirState : State
{
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;

    [Header("Jump cut")] [Range(0.02f, 0.3f)]
    public float TimeForUpwardCancel = 0.027f;

    [Header("Jump apex")] [Range(0.5f, 1f)]
    public float ApexThreshold = 0.97f;

    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("Jump buffer")] [Range(0f, 1f)]
    public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote time")] [Range(0, 1f)]
    public float JumpCoyoteTime = 0.1f;

    [Header("Jump visualization tool")] public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    public override void StateStart()
    {
    }

    public override State StateUpdate()
    {
        return null;
    }

    public override State StateFixedUpdate()
    {
        return null;
    }

    public override void StateExit()
    {
    }
}