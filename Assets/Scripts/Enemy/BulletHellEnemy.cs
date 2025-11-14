// Assets/Scripts/Enemy/BulletHellEnemy.cs
using UnityEngine;

/// <summary>
/// 彈幕地獄敵人範例。實作環繞旋轉彈幕攻擊模式。
/// </summary>
public class BulletHellEnemy : BaseEnemy
{
    private float nextBulletTime = 0f;  // 下次發射時間
    private int bulletsFired = 0;       // 已發射次數
    
    /// <summary>
    /// 執行符卡邏輯：定時發射圓形旋轉陣型。
    /// </summary>
    protected override void ExecuteSpellCard()
    {
        if (Time.time >= nextBulletTime)
        {
            FireCircleFormationWithVirtualCenter();
            bulletsFired++;
            nextBulletTime = Time.time + 0.6f;  // 每 0.6 秒發射一次圓形陣型
        }
    }
    
    /// <summary>
    /// 符卡結束條件：發射 5 次後結束。
    /// </summary>
    protected override bool IsSpellCardFinished()
    {
        return bulletsFired >= 5;
    }
    
    /// <summary>
    /// 攻擊結束時重置計數器與計時器。
    /// </summary>
    protected override void OnAttackEnd()
    {
        base.OnAttackEnd();
        bulletsFired = 0;
        nextBulletTime = Time.time;
    }
    
    /// <summary>
    /// 發射圓形旋轉陣型。
    /// 生成 6 顆子彈，以虛擬中心點為基礎環繞旋轉並向玩家移動。
    /// </summary>
    private void FireCircleFormationWithVirtualCenter()
    {
        if (playerTransform == null)
            return;
        
        // 計算朝向玩家的方向
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 centerVelocity = directionToPlayer * 25f;
        Vector3 centerStartPos = transform.position + Vector3.up * 0.5f;
        
        int bulletCount = 5;
        float angleStep = 360f / bulletCount;
        
        // 生成圓形陣型
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            
            BulletManager.Instance.SpawnBullet(
                centerStartPos,  // 所有彈幕從中心點創建
                new VirtualOrbitBehavior(
                    startCenterPos: centerStartPos,
                    centerMoveVelocity: centerVelocity,
                    initialRadius: 1f,       // 起始半徑
                    radiusGrowth: 12f,       // 半徑擴張速度
                    rotSpeed: 50f,          // 旋轉速度
                    startAngle: angle        // 起始角度（均勻分布）
                )
            );
        }
    }
}
