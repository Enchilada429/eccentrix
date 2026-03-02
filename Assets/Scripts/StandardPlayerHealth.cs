using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;


public class StandardPlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int MaxHealth = 100;
    [SerializeField, Min(1)] private int CurrentHealth = 100;


    [SerializeField] private UnityEvent onHit;
    [SerializeField] private UnityEvent onDeath;



    public void TakeDamage(int dmg)
    {
        CurrentHealth -= dmg;
        if (CurrentHealth > 0)
        {
            onHit.Invoke();
        }
        else
        {
            onDeath.Invoke();
        }
    }
}