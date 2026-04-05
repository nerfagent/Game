using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[Serializable]
public class DamageMultiplier
{
    public string Name;
    public float Value;
}
[Serializable]
public class Hurtbox : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] public HurtboxType hurtboxType = HurtboxType.Body;
    
    // 0.25: huge res, 0.5: res, 1: neutral, 1.5: weak, 2: huge weak
    public List<DamageMultiplier> damageMultiplier = new List<DamageMultiplier>
    {
        // Inherit from Elden Ring
        new DamageMultiplier { Name = "Standard", Value = 1f },
        new DamageMultiplier { Name = "Strike", Value = 1f },
        new DamageMultiplier { Name = "Slash", Value = 1f },
        new DamageMultiplier { Name = "Pierce", Value = 1f },
        new DamageMultiplier { Name = "Magic", Value = 1f },
        new DamageMultiplier { Name = "Fire", Value = 1f },
        new DamageMultiplier { Name = "Lightning", Value = 1f },
        new DamageMultiplier { Name = "Holy", Value = 1f },
        // Custom types can be added as needed
        new DamageMultiplier { Name = "Dark", Value = 1f }
    };
    // 1: neutral, 2: weak
    public List<DamageMultiplier> specialMultiplier = new List<DamageMultiplier>
    {
        new DamageMultiplier { Name = "FALSE", Value = 1f },
        new DamageMultiplier { Name = "GunAnti", Value = 1f },
        new DamageMultiplier { Name = "AntiAirS", Value = 1f },
        new DamageMultiplier { Name = "AntiAirB", Value = 1f },
        new DamageMultiplier { Name = "HorSlashAnti", Value = 1f },
        new DamageMultiplier { Name = "GroundAttackAnti", Value = 1f }
    };
    public bool invincible = false;
    
    public event Action<DamageData> OnDamageReceived;
    
    // owner setting
    public GameObject owner;
    public bool isPlayer = true;
    public bool isNPC = false;
    public bool isEnemy = false;
    public Collider collider;
    
    public enum HurtboxType
    {
        Head,       // 1x damage
        Body,       // 1x damage
        Stomach,    // 1x damage
        Arms,       // 1x damage
        Legs,       // 1x damage
        WeakPoint,   // 3x damage
        Special     // Custom multiplier
        // execute // 3x damage
    }
    
    void Awake()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = false;
        collider.enabled = true;
    }
    
    public void ReceiveDamage(DamageData damageData)
    {
        // Apply damage multiplier
        for (int i = 0; i < damageData.damageTypes.Count; i++)
        {
            damageData.damageTypes[i].Value *= damageMultiplier[i].Value;
            damageData.poiseDamageTypes[i].Value *= damageMultiplier[i].Value;

        }
        for (int i = 0; i < damageData.specialWeaknesses.Count; i++)
        {
            damageData.specialWeaknesses[i].Value *= specialMultiplier[i].Value;
        }
        
        // Calculate final knockback
        Vector3 normalizedKnockback = damageData.owner.transform.TransformDirection(damageData.knockbackDirection).normalized;
        // Vector3 normalizedKnockback = damageData.knockbackDirection.normalized;
        
        // Define the discrete directions
        Vector3 forward = owner.transform.forward;    // Forward direction
        Vector3 backward = -owner.transform.forward;   // Backward direction
        Vector3 left = -owner.transform.right;        // Left direction
        Vector3 right = owner.transform.right;         // Right direction

        // Determine the direction that is closest to the knockback vector
        float forwardAngle = Vector3.Angle(normalizedKnockback, forward);
        float backwardAngle = Vector3.Angle(normalizedKnockback, backward);
        float leftAngle = Vector3.Angle(normalizedKnockback, left);
        float rightAngle = Vector3.Angle(normalizedKnockback, right);

        // Find the minimum angle and return the corresponding direction
        float minAngle = Mathf.Min(forwardAngle, backwardAngle, leftAngle, rightAngle);
        
        if (minAngle == forwardAngle) damageData.knockbackindex = 0; // KnockbackDirectionType.Forward
        if (minAngle == backwardAngle) damageData.knockbackindex = 1; // KnockbackDirectionType.Backward
        if (minAngle == leftAngle) damageData.knockbackindex = 2;
        if (minAngle == rightAngle) damageData.knockbackindex = 3;
        damageData.knockbackDirection = normalizedKnockback;
        
        // Spawn hit effect
        // SpawnHitEffect(other.ClosestPoint(transform.position));
        
        // Handle in damage receiver's script
        OnDamageReceived?.Invoke(damageData);
        
        // Visual feedback
        // StartCoroutine(FlashHurtbox());
    }

    public bool CanHurt(DamageData damageData)
    {
        if (!damageData.selfAttack && owner == damageData.owner) return false;
        if (!damageData.themAttack && owner != damageData.owner) return false;
        if (
            (damageData.playerAttack && isPlayer)   || 
            (damageData.npcAttack && isNPC)         || 
            (damageData.enemyAttack && isEnemy))   return false;

        if (invincible) return false;

        return true;
    }

    // void SpawnHitEffect(Vector3 position)
    // {
    //     // Instantiate hit particles/effects
    // }
    
    // System.Collections.IEnumerator FlashHurtbox()
    // {
    //     var meshRenderer = GetComponent<MeshRenderer>();
    //     if (meshRenderer)
    //     {
    //         Color original = meshRenderer.material.color;
    //         meshRenderer.material.color = Color.red;
    //         yield return new WaitForSeconds(0.1f);
    //         meshRenderer.material.color = original;
    //     }
    // }
}