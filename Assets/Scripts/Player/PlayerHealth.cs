// Assets/Scripts/Level/Player/PlayerHealth.cs
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    private int currentHP;
    
    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;

    public static UnityAction OnPlayerDamaged = () => { };
    public static UnityAction OnPlayerDied = () => { };
    
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
    }
    
    /// <summary>
    /// 受到傷害
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
        
        OnPlayerDamaged.Invoke();
        
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
        OnPlayerDied.Invoke();
        GameManager.Instance.GameOver();
    }
}
