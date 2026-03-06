using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;


[Serializable]
public struct AttackInfo
{
    public int Damage;
    public float Knockback;
    public float StaggerDuration;
    // TODO: somehow store info about the animations and collision boxes to play/enable whenever doing the attack

    public AttackInfo(int damage, float knockback, float staggerDur)
    {
        Damage = damage;
        Knockback = knockback;
        StaggerDuration = staggerDur;
    }
}

[Serializable]
public struct ComboInfo
{
    public List<PlayerAttackController.AttackDirection> Buffer;
    public AttackInfo Attack;

    public ComboInfo(List<PlayerAttackController.AttackDirection> buffer, AttackInfo attack)
    {
        Buffer = buffer;
        Attack = attack;
    }
}


[CreateAssetMenu(menuName = "Player Attack Stats")]
public class PlayerAttackStats : ScriptableObject
{
    public LayerMask CharacterLayer;

    [Header("Normal Attacks")]
    public AttackInfo NormalAttackLeft;
    public AttackInfo NormalAttackRight;
    public AttackInfo NormalAttackUp;
    public AttackInfo NormalAttackDown;

    [Header("Strong Attacks")]
    public AttackInfo StrongAttackLeft;
    public AttackInfo StrongAttackRight;
    public AttackInfo StrongAttackUp;
    public AttackInfo StrongAttackDown;
    public float TimeToExecuteStrongAttack = 2.5f;

    [Header("Special Attacks")]
    public List<ComboInfo> SpecialCombos = new();
    public float TimeBeforeBufferClear = 2f;
}

