using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

[Serializable]
public struct AttackInfo
{
    public int Damage;
    public float Knockback;
    public float StaggerDurationSeconds;
    // TODO: somehow store info about the animations and collision boxes to play/enable whenever doing the attack
}

[Serializable]
public struct ComboInfo
{
    public List<CharAction> ConsecutiveActions;
    public AttackInfo Attack;
}


[CreateAssetMenu(menuName = "Player Attack Stats")]
public class PlayerAttackStats : ScriptableObject
{
    public LayerMask CharacterLayer;


    [Header("Normal Ground Attacks")]
    public AttackInfo NormalGroundAttackLeft;
    public AttackInfo NormalGroundAttackRight;
    public AttackInfo NormalGroundAttackUp;


    [Header("Normal Air Attacks")]
    public AttackInfo NormalAirAttackLeft;
    public AttackInfo NormalAirAttackRight;
    public AttackInfo NormalAirAttackUp;
    public AttackInfo NormalAirAttackDown;


    [Header("Strong Ground Attacks")]
    [Range(1f, 4f)] public float TimeHeldForStrongAttack;
    public AttackInfo StrongGroundAttackLeft;
    public AttackInfo StrongGroundAttackRight;
    public AttackInfo StrongGroundAttackUp;


    [Header("Strong Air Attacks")]
    public AttackInfo StrongAirAttackLeft;
    public AttackInfo StrongAirAttackRight;
    public AttackInfo StrongAirAttackUp;
    public AttackInfo StrongAirAttackDown;


    [Header("Special Attacks")]
    public List<ComboInfo> Combos;
    public int ComboBufferClearFrames = 120; // how long before an action is cleared from the buffer, assumed 60fps
}

