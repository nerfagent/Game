using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [SerializeField] private DamageData weaponData;
    [SerializeField] private DamageData weaponBuffData;
    [SerializeField] private DamageData skillData;
    [SerializeField] private DamageData ownerBuffData;
    [SerializeField] private DamageData summedData;

    [SerializeField] private LayerMask hitLayers;
    // [SerializeField] private float hitboxDuration = 0.2f;

    [Header("AOE Settings")]
    [SerializeField] private bool allowMultiHit = true;  // Can hit multiple enemies
    [SerializeField] private bool singleHitPerTarget = true; // Each enemy hit only once
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
    
    private Collider hitboxCollider;
    private HashSet<Sprite_A0> hitCharacters = new HashSet<Sprite_A0>(); // Alternative: track by Character

    public readonly object _rewardLock = new object();
    public List<DamageData> currentPendingRewardList = new List<DamageData> { };
    
    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        DisableHitbox();
    }

    public void UpdateDamageData(DamageData damageData, int j)
    {
        switch (j)
        {
            case 0:
                weaponData = damageData;
                break;
            case 1:
                weaponBuffData = damageData;
                break;
            case 2:
                skillData = damageData;
                break;
            case 3:
                ownerBuffData = damageData;
                break;
            case 4:
                summedData = weaponData;
                for (int i = 0; i < damageData.damageTypes.Count; i++)
                {
                    // after weapon buff
                    summedData.damageTypes[i].Value += weaponBuffData.damageTypes[i].Value;
                    summedData.poiseDamageTypes[i].Value += weaponBuffData.poiseDamageTypes[i].Value;

                    // after skill's damage multiplier
                    summedData.damageTypes[i].Value *= skillData.damageTypes[i].Value;
                    summedData.poiseDamageTypes[i].Value *= skillData.poiseDamageTypes[i].Value;

                    // no owner buff for now // AFTER_FYP
                }
                for (int i = 0; i < damageData.specialWeaknesses.Count; i++)
                {
                    // anti opposives' weakness
                    summedData.specialWeaknesses[i].Value *= weaponBuffData.specialWeaknesses[i].Value;
                    summedData.specialWeaknesses[i].Value *= skillData.specialWeaknesses[i].Value;
                }
                // after skill's special buff
                summedData.attackName = skillData.attackName;
                summedData.knockbackDirection = skillData.knockbackDirection;
                summedData.knockbackindex = skillData.knockbackindex;
                summedData.damageLevel = skillData.damageLevel;
                summedData.forceDizzy = skillData.forceDizzy;
                break;
        }
    }
    
    public void ActivateHitbox()
    {
        // hitTargets.Clear();  // Reset tracking each activation
        hitCharacters.Clear();
        hitboxCollider.enabled = true;
        
        // if (hitboxDuration > 0)
        //     Invoke(nameof(DisableHitbox), hitboxDuration);
    }
    
    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
        // hitTargets.Clear();  // Reset tracking each activation
        hitCharacters.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        // if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
        //     return;
        
        // Find Character component
        Sprite_A0 targetCharacter = other.GetComponentInParent<Sprite_A0>();
        if (targetCharacter == null)
            return;
        
        // Check if we already hit this character // if it is invincible wont count as hitted
        if (singleHitPerTarget && hitCharacters.Contains(targetCharacter))
            return;
        
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null && hitboxCollider.enabled == true && hurtbox.CanHurt(summedData))
        {
            hurtbox.ReceiveDamage(summedData);
            hitCharacters.Add(targetCharacter); // Mark character as hit
            lock (_rewardLock)
            {
                currentPendingRewardList.Add(summedData); // Reward for player, can be used in other scripts like PlayerMain_A0
            }
        }
    }

    void OnDrawGizmos() // Can be ignore for now
    {
        if (!showDebug || !hitboxCollider) return;
        
        Gizmos.color = debugColor;
        if (hitboxCollider is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (hitboxCollider is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
        }
        else if (hitboxCollider is CapsuleCollider capsule)
        {
            // Set the gizmo matrix to the localToWorldMatrix of the GameObject
            Gizmos.matrix = transform.localToWorldMatrix;

            // Calculate the position for the top hemisphere
            Vector3 topSpherePosition = capsule.center + Vector3.up * (capsule.height / 2 - capsule.radius);
            // Calculate the position for the bottom hemisphere
            Vector3 bottomSpherePosition = capsule.center + Vector3.down * (capsule.height / 2 - capsule.radius);

            // Draw the cylindrical part of the capsule
            Gizmos.DrawCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height - capsule.radius * 2, capsule.radius * 2));

            // Draw the top and bottom hemispheres
            Gizmos.DrawSphere(topSpherePosition, capsule.radius);
            Gizmos.DrawSphere(bottomSpherePosition, capsule.radius);
        }
    }
}