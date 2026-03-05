using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum CharAction
{
    IDLE, MOVE_LEFT, MOVE_RIGHT, CROUCH, JUMP, DOUBLE_JUMP,
    NORM_ATK_GND_LEFT, NORM_ATK_GND_RIGHT, NORM_ATK_GND_UP,
    NORM_ATK_AIR_LEFT, NORM_ATK_AIR_RIGHT, NORM_ATK_AIR_UP, NORM_ATK_AIR_DOWN,
    STR_ATK_GND_LEFT, STR_ATK_GND_RIGHT, STR_ATK_GND_UP,
    STR_ATK_AIR_LEFT, STR_ATK_AIR_RIGHT, STR_ATK_AIR_UP, STR_ATK_AIR_DOWN,
    SPECIAL
}

public class CharActionComparer : IComparer<Tuple<CharAction, int>>
{
    // sorts based on the most RECENT actions, so if the frame > other then return <0 to make it sort before it
    int IComparer<Tuple<CharAction, int>>.Compare(Tuple<CharAction, int> x, Tuple<CharAction, int> y)
    {
        if (x.Item2 > y.Item2)
        {
            return -1;
        }
        else if (x.Item2 < y.Item2)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}


public class StandardCharacterAttack : MonoBehaviour
{
    // components
    public PlayerMovementStats MoveStats;
    public PlayerAttackStats AttackStats;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Collider2D _bodyColl;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Rigidbody2D _rb;    


    // attack variables


    // move variables
    private bool _isGrounded = false;
    private bool _bumpedHead = false;


    // timers
    private float _timeUntilStrongAttack = 0f;


    // buffers
    private List<Tuple<CharAction, int>> _prevActionsPerformed = new();


    // raycasts
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;




    // input checks
    public Vector2 MoveInput => _playerInput.actions["move"].ReadValue<Vector2>();
    public bool JumpPressed => _playerInput.actions["jump"].WasPressedThisFrame();
    public bool JumpHeld => _playerInput.actions["jump"].IsPressed();
    public bool JumpReleased => _playerInput.actions["jump"].WasReleasedThisFrame();
    public bool RunHeld => _playerInput.actions["sprint"].IsPressed();    
    // the look vector is from 0,0 bottom left to 1,1, top right so normalise to be between -1,-1 bottom left and 0.5,0.5 top right
    public Vector2 LookInput => Camera.main.ScreenToViewportPoint(_playerInput.actions["look"].ReadValue<Vector2>());
    public bool AttackPressed => _playerInput.actions["attack"].WasPressedThisFrame();
    public bool AttackHeld => _playerInput.actions["attack"].IsPressed();
    public bool AttackReleased => _playerInput.actions["attack"].WasReleasedThisFrame();


    void Awake()
    {
        if (!_playerInput) _playerInput = GetComponent<PlayerInput>();
        if (!_rb) _rb = GetComponent<Rigidbody2D>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AttackChecks();
        CountTimers();
        UpdateActionBuffer();
    }

    void FixedUpdate()
    {
        CollisionChecks();
    }


    private ComboInfo? ComboPerformed()
    {
        // loop through all of the combos and see if the actions match any of the combos defined
        foreach (ComboInfo combo in AttackStats.Combos)
        {
            // get the n recent actions from the action buffer
            List<CharAction> recActs = GetRecentActions(combo.ConsecutiveActions.Count);
            if (recActs == combo.ConsecutiveActions)
            {
                return combo;
            }
        }

        return null;
    }

    private List<CharAction> GetRecentActions(int n)
    {
        // sort the list and then return the n recent
        List<CharAction> acts = new();
        List<Tuple<CharAction, int>> recentActionsSorted = new(_prevActionsPerformed);
        recentActionsSorted.Sort(new CharActionComparer());

        for (int i = 0; i < Mathf.Min(n, recentActionsSorted.Count); i++)
        {
            acts.Add(recentActionsSorted[i].Item1);
        }
        return acts;
    }


    private void AttackChecks()
    {
        print(LookInput);

        // check if combo executed
        ComboInfo? combo = ComboPerformed();
        if (combo != null)
        {
            // TODO: perform combo here

            #if UNITY_EDITOR
            Debug.Log($"Combo performed {combo}");
            #endif
        }
        else if (AttackReleased && _timeUntilStrongAttack <= 0f)
        {
            // TODO: perform strong attack here

            #if UNITY_EDITOR
            Debug.Log("Strong Attack performed");
            #endif
        }
        else if (AttackPressed)
        {
            // TODO: perform normal attack here

            #if UNITY_EDITOR
            Debug.Log("Normal Attack Performed");
            #endif
        }
    }


    private void CountTimers()
    {
        if (AttackHeld)
        {
            _timeUntilStrongAttack -= Time.deltaTime;
        }
        else
        {
            _timeUntilStrongAttack = AttackStats.TimeHeldForStrongAttack;
        }
    }

    private void UpdateActionBuffer()
    {
        int currentFrame = Time.frameCount;
        List<int> _indicesToRemove = new();

        for (int i = 0; i < _prevActionsPerformed.Count; i++)
        {
            Tuple<CharAction, int> act = _prevActionsPerformed[i];
            if (currentFrame - act.Item2 >= AttackStats.ComboBufferClearFrames)
            {
                _indicesToRemove.Add(i);
            }
        }

        foreach (int index in _indicesToRemove)
        {
            _prevActionsPerformed.RemoveAt(index);
        }

        // read what actions have been performed this frame and add it to the actions performed
        _prevActionsPerformed.Add(new (
            MoveInput.x == 1 ? CharAction.MOVE_RIGHT
            : MoveInput.x == -1 ? CharAction.MOVE_LEFT
            : MoveInput.y == -1 ? CharAction.CROUCH
            : !_isGrounded && JumpPressed ? CharAction.DOUBLE_JUMP
            : JumpPressed ? CharAction.JUMP : CharAction.IDLE,
            currentFrame));
        _prevActionsPerformed.Add(new (
            AttackPressed ? LookInput.x == 1 ? _isGrounded ? CharAction.NORM_ATK_GND_RIGHT : CharAction.NORM_ATK_AIR_RIGHT
                : LookInput.x == -1 ? _isGrounded ? CharAction.NORM_ATK_GND_LEFT : CharAction.NORM_ATK_AIR_LEFT
                : LookInput.y == 1 ? _isGrounded ? CharAction.NORM_ATK_GND_UP: CharAction.NORM_ATK_AIR_UP
                : LookInput.y == -1 && !_isGrounded ? CharAction.NORM_ATK_AIR_DOWN : CharAction.CROUCH
            : AttackReleased && _timeUntilStrongAttack <= 0f
                ? LookInput.x == 1 ? _isGrounded ? CharAction.STR_ATK_GND_RIGHT : CharAction.STR_ATK_AIR_RIGHT
                : LookInput.x == -1 ? _isGrounded ? CharAction.STR_ATK_GND_LEFT : CharAction.STR_ATK_AIR_LEFT
                : LookInput.y == 1 ? _isGrounded ? CharAction.STR_ATK_GND_UP : CharAction.STR_ATK_AIR_UP
                : LookInput.y == -1 ? _isGrounded ? CharAction.CROUCH : CharAction.STR_ATK_AIR_DOWN
            : CharAction.SPECIAL : CharAction.IDLE,
        currentFrame));
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
}
