using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class DamageType
{
    public string Name;
    public float Value;
}
[Serializable]
public class StatusEffect
{
    public string Name;
    public float Value;
}
[Serializable]
public class SpecialWeakness
{
    public string Name;
    public float Value;
}
[Serializable]
public class DamageData : MonoBehaviour
{
    public string attackName = "Basic Attack";
    public bool selfAttack = false;
    public bool themAttack = true;
    public bool playerAttack = false;
    public bool npcAttack = false;
    public bool enemyAttack = true;
    public Vector3 knockbackDirection = Vector3.left;
    public int knockbackindex = 0; // 0: F, 1: B, 2: L, 3: R, 4: U, 5: D
    public int damageLevel = 1; // 0: cant knock // 1: small knock // 2: big knock // 3: knock up // 4: knock down // 5: status effect
    public bool forceDizzy = false;

    public List<DamageType> damageTypes = new List<DamageType>
    {
        // Inherit from Elden Ring
        new DamageType { Name = "Standard", Value = 0f },
        new DamageType { Name = "Strike", Value = 0f },
        new DamageType { Name = "Slash", Value = 300f },
        new DamageType { Name = "Pierce", Value = 0f },
        new DamageType { Name = "Magic", Value = 0f },
        new DamageType { Name = "Fire", Value = 0f },
        new DamageType { Name = "Lightning", Value = 0f },
        new DamageType { Name = "Holy", Value = 0f },
        // Custom types can be added as needed
        new DamageType { Name = "Dark", Value = 0f }
    };
    public List<DamageType> poiseDamageTypes = new List<DamageType>
    {
        // Inherit from Elden Ring
        new DamageType { Name = "Standard", Value = 0f },
        new DamageType { Name = "Strike", Value = 0f },
        new DamageType { Name = "Slash", Value = 10f },
        new DamageType { Name = "Pierce", Value = 0f },
        new DamageType { Name = "Magic", Value = 0f },
        new DamageType { Name = "Fire", Value = 0f },
        new DamageType { Name = "Lightning", Value = 0f },
        new DamageType { Name = "Holy", Value = 0f },
        // Custom types can be added as needed
        new DamageType { Name = "Dark", Value = 0f }
    };
    public List<StatusEffect> statusEffects = new List<StatusEffect> // AFTER_FYP
    {
        // Inherit from Elden Ring
        new StatusEffect { Name = "Poison", Value = 0f },
        new StatusEffect { Name = "Scarlet Rot", Value = 0f },
        new StatusEffect { Name = "Blood Loss", Value = 55f },
        new StatusEffect { Name = "Frostbite", Value = 0f },
        new StatusEffect { Name = "Sleep", Value = 0f },
        new StatusEffect { Name = "Madness", Value = 0f },
        new StatusEffect { Name = "Death Blight", Value = 0f },
        // Inherit from Sekiro
        new StatusEffect { Name = "OnFire", Value = 0f },
        new StatusEffect { Name = "ElectricShock", Value = 0f },
        new StatusEffect { Name = "Year", Value = 0f },
        // Custom types can be added as needed
        new StatusEffect { Name = "Corruption", Value = 0f }
    };
    public List<SpecialWeakness> specialWeaknesses = new List<SpecialWeakness>
    {
        new SpecialWeakness { Name = "FALSE", Value = 1f },
        new SpecialWeakness { Name = "GunAnti", Value = 1f },
        new SpecialWeakness { Name = "AntiAirS", Value = 1f },
        new SpecialWeakness { Name = "AntiAirB", Value = 1f },
        new SpecialWeakness { Name = "HorSlashAnti", Value = 1f },
        new SpecialWeakness { Name = "GroundAttackAnti", Value = 1f }
    };
    public GameObject owner; // Who dealt damage
}