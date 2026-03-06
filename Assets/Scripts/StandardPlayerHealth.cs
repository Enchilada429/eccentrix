using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;


public class StandardPlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int MaxHealth = 100;
    private int _currentHealth = 100;


    [SerializeField] private UnityEvent onHit;
    [SerializeField] private UnityEvent onDeath;


    void Awake()
    {
        _currentHealth = MaxHealth;
    }

    public void TakeDamage(int dmg)
    {
        _currentHealth -= dmg;
        if (_currentHealth > 0)
        {
            onHit.Invoke();
        }
        else
        {
            onDeath.Invoke();
        }
    }
}