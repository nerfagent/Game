using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    [SerializeField] private DamageData damageData;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private float hitboxDuration = 0.2f;

    [Header("AOE Settings")]
    [SerializeField] private bool allowMultiHit = true;  // Can hit multiple enemies
    [SerializeField] private bool singleHitPerTarget = true; // Each enemy hit only once
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
    
    private Collider hitboxCollider;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // Track by GameObject
    private HashSet<SAThirdPersonController> hitCharacters = new HashSet<SAThirdPersonController>(); // Alternative: track by Character
    
    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        DisableHitbox();
    }
    
    public void ActivateHitbox(GameObject owner)
    {
        damageData.owner = owner;
        hitTargets.Clear();  // Reset tracking each activation
        hitCharacters.Clear();
        hitboxCollider.enabled = true;
        
        if (hitboxDuration > 0)
            Invoke(nameof(DisableHitbox), hitboxDuration);
    }
    
    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
        hitTargets.Clear();  // Reset tracking each activation
        hitCharacters.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
            return;
        
        // Find Character component
        SAThirdPersonController targetCharacter = other.GetComponentInParent<SAThirdPersonController>();
        if (targetCharacter == null)
            return;
        
        // Check if we already hit this character
        if (singleHitPerTarget && hitCharacters.Contains(targetCharacter))
            return;
        
        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null)
        {
            hurtbox.ReceiveDamage(damageData);
            hitCharacters.Add(targetCharacter);  // Mark character as hit

            // Spawn hit effect
            SpawnHitEffect(other.ClosestPoint(transform.position));
        }
    }
    
    void SpawnHitEffect(Vector3 position)
    {
        // Instantiate hit particles/effects
    }
    
    void OnDrawGizmos()
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
    }
}