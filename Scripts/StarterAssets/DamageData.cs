using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class DamageData
{
    public string attackName = "Basic Attack";
    public float damage = 500f;
    public Vector3 knockbackDirection = Vector3.left;
    public float poiseDamage = 10f;
    public float stanceDamage = 10f;
    public float poiseStunDuration = 0.3f;
    public Dictionary<string, int> damageTypes = new Dictionary<string, int>
        {
            // Inherit from Elden Ring
            { "Standard", 0 },
            { "Strike", 0 },
            { "Slash", 50 },
            { "Pierce", 0 },
            { "Magic", 50 },
            { "Fire", 0 },
            { "Lightning", 0 },
            { "Holy", 0 },
            // Custom types can be added as needed
            { "Dark", 0 }
        };
    public Dictionary<string, int> statusEffects = new Dictionary<string, int>
        {
            // Inherit from Elden Ring
            { "Poison", 0 },
            { "Scarlet Rot", 0 },
            { "Blood Loss", 55 },
            { "Frostbite", 0 },
            { "Sleep", 0 },
            { "Madness", 0 },
            { "Death Blight", 0 },
            // Inherit from Sekiro
            { "OnFire", 0 },
            { "ElectricShock", 0 },
            { "Year", 0 },
            // Custom types can be added as needed
            { "Corruption", 0 }
        };
    public GameObject owner; // Who dealt damage
}