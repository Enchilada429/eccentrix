using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class StandardCharacterAttack : MonoBehaviour
{
    public enum AttackState
    {
        IDLE, PENDING, NORMAL, STRONG, SPECIAL
    };
    public enum AttackDirection
    {
        LEFT, RIGHT, UP, DOWN
    }

    // components
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private PlayerAttackStats _attackStats;


    // attack variables
    private AttackState _atkState = AttackState.IDLE;
    private List<AttackDirection> _attackBuffer = new();
    private float _timeUntilStrongAttack = 0f;
    private float _timeUntilBufferClear = 0f;
    private float _timeUntilAttackFinished = 0f;

    private bool _isFacingRight = true;


    // inputs
    public bool AttackPressed => _playerInput.actions["attack"].WasPressedThisFrame();
    public bool AttackHeld => _playerInput.actions["attack"].IsPressed();
    public bool AttackReleased => _playerInput.actions["attack"].WasReleasedThisFrame();
    public Vector2 MoveInput => _playerInput.actions["move"].ReadValue<Vector2>().normalized;
    public AttackDirection AttackDir =>
        MoveInput.x < 0 ? AttackDirection.LEFT
        : MoveInput.x > 0 ? AttackDirection.RIGHT
        : MoveInput.y < 0 ? AttackDirection.DOWN
        : MoveInput.y > 0 ? AttackDirection.UP
        : _isFacingRight ? AttackDirection.RIGHT : AttackDirection.LEFT;


    private bool HasPerformedCombo()
    {
        // loop through all the combos and if any match the attack buffer then return true
        foreach (ComboInfo combo in _attackStats.SpecialCombos)
        {
            // need to check from the recent dir backwards since we dont care about actions before the possible combo
            if (_attackBuffer.Count < combo.Buffer.Count) continue;
            else if (_attackBuffer.Last() != combo.Buffer.Last()) continue;
            else
            {
                for (int i = 0; i < combo.Buffer.Count; i++)
                {
                    if (_attackBuffer[_attackBuffer.Count-1-i] != combo.Buffer[combo.Buffer.Count-1-i]) return false;
                }
                return true;
            }
        }
        return false;
    }

    
    private void TransitionState()
    {
        // checks the current state and other variables, then changes the current enum state based on it
        switch (_atkState)
        {
            case AttackState.IDLE:
                if (AttackPressed)
                {
                    _atkState = AttackState.PENDING;
                    _attackBuffer.Add(AttackDir);
                    _timeUntilStrongAttack = _attackStats.TimeToExecuteStrongAttack;
                }
                else if (_timeUntilBufferClear > 0f)
                {
                    _timeUntilBufferClear -= Time.deltaTime;
                }
                else if (_timeUntilBufferClear <= 0f)
                {
                    _attackBuffer.Clear();
                }
                break;

            case AttackState.PENDING:
                if (HasPerformedCombo())
                {
                    _atkState = AttackState.SPECIAL;
                    _timeUntilAttackFinished = 3f; // TODO: replace with animator length
                }
                else if (AttackHeld)
                {
                    _timeUntilStrongAttack -= Time.deltaTime;
                }
                else if (AttackReleased && _timeUntilStrongAttack <= 0f)
                {
                    _atkState = AttackState.STRONG;
                    _timeUntilAttackFinished = 2f; // TODO: replace with animator length
                }
                else if (AttackReleased && _timeUntilStrongAttack > 0f)
                {
                    _atkState = AttackState.NORMAL;
                    _timeUntilAttackFinished = 1f; // TODO: replace with animator length
                }
                break;

            case AttackState.NORMAL:
                print("normal attack");
                if (_timeUntilAttackFinished > 0f)
                {
                    _timeUntilAttackFinished -= Time.deltaTime;
                }
                else
                {
                    _atkState = AttackState.IDLE;
                    _timeUntilBufferClear = _attackStats.TimeBeforeBufferClear;
                }
                break;

            case AttackState.STRONG:
                print("strong attack");
                if (_timeUntilAttackFinished > 0f)
                {
                    _timeUntilAttackFinished -= Time.deltaTime;
                }
                else
                {
                    _atkState = AttackState.IDLE;
                    _timeUntilBufferClear = _attackStats.TimeBeforeBufferClear;
                }
                break;

            case AttackState.SPECIAL:
                print("special attack");
                if (_timeUntilAttackFinished > 0f)
                {
                    _timeUntilAttackFinished -= Time.deltaTime;
                }
                else
                {
                    _atkState = AttackState.IDLE;
                    // immediately clear buffer whenever specials done otherwise checking for combos might repeat recent ones done
                    _timeUntilBufferClear = 0f;
                }
                break;
        }
    }

    private void TurnCheck()
    {
        if (_isFacingRight && MoveInput.x < 0)
        {
            _isFacingRight = false;
        }
        else if (!_isFacingRight && MoveInput.x > 0)
        {
            _isFacingRight = true;
        }
    }

    void Start()
    {
        
    }


    void Update()
    {
        TurnCheck();
        TransitionState();
    }
}