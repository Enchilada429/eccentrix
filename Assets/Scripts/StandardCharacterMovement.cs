using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



/* FROM https://www.youtube.com/watch?v=zHSWG05byEc
on how to make a 2d platformer controller
*/
public class StandardCharacterMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Collider2D _bodyColl;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Rigidbody2D _rb;


    // movement variables
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    // jump variables
    public float VerticalVelocity {get; private set;}
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    // apex variables
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    // jump buffer variables
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    // coyote time variables
    private float _coyoteTimer;


    // collision checks
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;


    // input checks
    public Vector2 MoveInput => _playerInput.actions["move"].ReadValue<Vector2>();
    public bool JumpPressed => _playerInput.actions["jump"].WasPressedThisFrame();
    public bool JumpHeld => _playerInput.actions["jump"].IsPressed();
    public bool JumpReleased => _playerInput.actions["jump"].WasReleasedThisFrame();
    public bool RunHeld => _playerInput.actions["sprint"].IsPressed();



    void Awake()
    {
        _isFacingRight = true;
        if (!_rb) _rb = GetComponent<Rigidbody2D>();
        if (!_playerInput) _playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        CountTimers();
        JumpCheck();
    }

    void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        // if on ground then move based on ground acceleration and decceleration
        // otherwise move based on air
        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, MoveInput);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, MoveInput);
        }
    }


    private void Move(float accel, float decel, Vector2 inp)
    {
        // check if any movement is pressed down
        if (inp != Vector2.zero)
        {
            TurnCheck(inp);
            // determine if the final velocity should be run or walk based on if
            // the sprint key is held down or not
            Vector2 targetVel;
            if (RunHeld)
            {
                targetVel = new Vector2(inp.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVel = new Vector2(inp.x, 0f) * MoveStats.MaxWalkSpeed;
            }
            // lerp between the current move velocity so that over time it gets faster
            // until the max speed is reached, and alse set the velocity in the rigidbody
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVel, accel * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
        // if no movement then decelerate the character until stops
        else if (inp == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, decel * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
    }



    private void TurnCheck(Vector2 inp)
    {
        if (_isFacingRight && inp.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && inp.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool faceRight)
    {
        if (faceRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
    }



    // updates the _isGrounded variable by doing a boxcast below the feet collider
    private void IsGrounded()
    {
        // sets box origin to be the middle of the bottom edge
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    // updates the _bumpedHead variable by doing a box cast above the body
    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }


    private void JumpCheck()
    {
        // when the jump button is pressed
        if (JumpPressed){
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }
        // when the jump button is released
        if (JumpReleased)
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }
            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // initiate jump with buffer and coyote jump
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        // check if can do more jumps based on how many defined in MoveStats
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            InitiateJump(1);
            _isFastFalling = false;
        }

        // air jump AFTER coyote jump (use both jumps)
        else if (_jumpBufferTimer > 0f && _isFastFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        // if landed
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numJumps)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numJumps;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        // apply gravity when jumping
        if (_isJumping)
        {
            // check for head bump            
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            // gravity on ascending
            if (VerticalVelocity >= 0f)
            {
                // apex controls
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);
                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }
                    else if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                // gravity on acending below apex threshold
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }                    
                }
            }

            // gravity on descending
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }        

            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }            

        }

        // jump cut
        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, _fastFallTime / MoveStats.TimeForUpwardsCancel);
            }
            _fastFallTime += Time.fixedDeltaTime;
        }

        // normal gravity when falling
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        // clamp fall speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;
        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else{
            _coyoteTimer = MoveStats.JumpCoyoteTime;
        }
    }
}
