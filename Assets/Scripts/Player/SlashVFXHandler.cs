using UnityEngine;

public class SlashVFXHandler : MonoBehaviour
{
    [Tooltip("Layer containing enemy GameObjects for collision detection.")]
    public LayerMask enemyLayer;

    [Header("攻擊設定")]
    [Tooltip("此斬擊造成的傷害值")]
    public int damage = 25;

    [Header("Impact VFX Settings")]
    [Tooltip("Prefab for the impact effect to spawn when an enemy is hit.")]
    public GameObject impactVFXPrefab;

    private ParticleSystem mainSlashParticleSystem;
    private Collider attackCollider;

    void Awake()
    {
        mainSlashParticleSystem = GetComponentInChildren<ParticleSystem>();
        attackCollider = GetComponent<Collider>();

        if (mainSlashParticleSystem == null)
        {
            Debug.LogWarning("SlashVFXHandler: No ParticleSystem found. Main VFX will not play.", this);
            enabled = false;
            return;
        }
    }

    void Start()
    {
        if (mainSlashParticleSystem != null)
        {
            mainSlashParticleSystem.Play();
            float totalDuration = mainSlashParticleSystem.main.duration + mainSlashParticleSystem.main.startLifetime.constantMax;
            Destroy(gameObject, totalDuration);
        }
        else
        {
            Destroy(gameObject);
        }

        if (enemyLayer.value == 0)
        {
            Debug.LogWarning("Enemy LayerMask for SlashVFXHandler is not set. Please set it in the prefab inspector.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer.value) != 0)
        {
            // 嘗試獲取敵人的BaseEnemy組件並造成傷害
            if (other.TryGetComponent<BaseEnemy>(out var enemy) && !enemy.IsDead)
            {
                Debug.Log($"Slash VFX hit {other.gameObject.name}. Applying {damage} damage.");
                enemy.TakeDamage(damage);

                // 在敵人位置生成衝擊特效
                if (impactVFXPrefab != null)
                {
                    Instantiate(impactVFXPrefab, other.transform.position, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("Impact VFX Prefab is not assigned in SlashVFXHandler.");
                }

                // 為了防止一次斬擊對同一個敵人造成多次傷害，可以禁用碰撞器
                if (attackCollider != null)
                {
                    attackCollider.enabled = false;
                }
            }
        }
    }
}
