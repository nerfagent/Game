using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class Hurtbox : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private HurtboxType hurtboxType = HurtboxType.Body;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private bool invincible = false;
    
    public event Action<DamageData> OnDamageReceived;
    
    private SAThirdPersonController owner;
    private Collider collider;
    
    public enum HurtboxType
    {
        Head,       // 2x damage
        Body,       // 1x damage
        Stomach,    // 1x damage
        Arms,       // 0.8x damage
        Legs,       // 0.8x damage
        WeakPoint   // 3x damage
    }
    
    void Awake()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = true;
        owner = GetComponentInParent<SAThirdPersonController>();
    }
    
    public void ReceiveDamage(DamageData damageData)
    {
        if (invincible || owner == damageData.owner) return;
        
        // Apply damage multiplier
        damageData.damage *= damageMultiplier;
        
        // Calculate final knockback
        Vector3 direction = damageData.owner.transform.position - transform.position;
        direction = new Vector3(direction.x, 0.5f, direction.z).normalized;
        damageData.knockbackDirection = direction;
        
        OnDamageReceived?.Invoke(damageData);
        
        // Visual feedback
        StartCoroutine(FlashHurtbox());
    }
    
    public void SetInvincible(bool state) => invincible = state;
    
    System.Collections.IEnumerator FlashHurtbox()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer)
        {
            Color original = meshRenderer.material.color;
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            meshRenderer.material.color = original;
        }
    }
}