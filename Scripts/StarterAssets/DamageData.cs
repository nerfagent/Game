using UnityEngine;
using System;

[Serializable]
public class DamageData
{
    public float damage = 400f;
    public Vector3 knockbackDirection = Vector3.forward;
    public float knockbackForce = 5f;
    public float poiseDamage = 10f;
    public float stanceDamage = 10f;
    public AttackType attackType = AttackType.Normal;
    public float poiseStunDuration = 0.3f;
    public float stanceStunDuration = 3f;
    public GameObject owner; // Who dealt damage
}

public enum AttackType
{
    Normal,
    Fall,
    Projectile,
    Special
}