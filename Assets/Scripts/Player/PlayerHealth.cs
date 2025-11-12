// Assets/Scripts/Level/Player/PlayerHealth.cs
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    private int currentHP;
    
    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    
    private void Start()
    {
        currentHP = maxHP;
    }
    
    /// <summary>
    /// 設置最大生命值
    /// </summary>
    public void SetMaxHP(int newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = Mathf.Min(currentHP, maxHP);
    }
    
    /// <summary>
    /// 恢復到滿血
    /// </summary>
    public void RestoreToFull()
    {
        currentHP = maxHP;
        Debug.Log($"玩家血量已恢復: {currentHP}/{maxHP}");
        EventManager.TriggerEvent("OnPlayerHealthRestored");
    }
    
    /// <summary>
    /// 受到傷害
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
        
        EventManager.TriggerEvent("OnPlayerDamaged");
        
        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        EventManager.TriggerEvent("OnPlayerDied");
        GameManager.Instance.GameOver();
    }
}
