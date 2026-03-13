using UnityEngine;

/* FROM https://www.youtube.com/watch?v=zHSWG05byEc
on how to make a 2d platformer controller
*/
[CreateAssetMenu(menuName = "Player Move Stats")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;


    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;


    [Header("Grounded Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.1f;
    public float HeadDetectionRayLength = 0.1f;
    [Range(0f, 1f)] public float HeadWidth = 0.5f;

    [Header("Jump")]
    public float jumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    [Header("Debug")]
    public bool DebugShowIsGroundedBox = false;
    public bool DebugShowHeadBumpBox = false;

    [Header("Jump Visualisation Tool")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualisationSteps = 90;

    public float Gravity { get; private set;}
    public float InitialJumpVelocity {get; private set;}
    public float AdjustedJumpHeight {get; private set;}

    private void CalculateValues()
    {
        AdjustedJumpHeight = jumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }


}