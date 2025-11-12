// Assets/Scripts/Level/Checkpoint.cs
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("檢查點設定")]
    [SerializeField] private string checkpointID;  // 唯一識別碼
    [SerializeField] private string checkpointSceneName;  // 檢查點所在場景名稱
    [SerializeField] private Vector3 playerSpawnPosition;  // 玩家重生位置
    
    [Header("視覺效果")]
    [SerializeField] private GameObject activatedEffect;  // 激活特效
    [SerializeField] private bool showDebugInfo = true;
    private Collider triggerCollider;
    
    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
        }
        
        // 設置默認玩家生成位置為檢查點位置
        if (playerSpawnPosition == Vector3.zero)
        {
            playerSpawnPosition = transform.position;
        }
        
        // 如果場景名稱為空，自動獲取當前場景名稱
        if (string.IsNullOrEmpty(checkpointSceneName))
        {
            checkpointSceneName = gameObject.scene.name;
        }
        
        if (activatedEffect != null)
        {
            activatedEffect.SetActive(false);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && InputManager.GetInteractInput())
        {
            ActivateCheckpoint();
        }
    }
    
    /// <summary>
    /// 激活檢查點
    /// </summary>
    private void ActivateCheckpoint()
    {
        // 顯示激活特效
        if (activatedEffect != null)
        {
            activatedEffect.SetActive(true);
        }
        
        // 自動保存遊戲
        if (SaveLoadManager.Instance != null)
        {
            // 補滿玩家血量
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.RestoreToFull();
                }
            }
            
            // 重置技能冷卻
            if (CooldownSystem.Instance != null)
            {
                CooldownSystem.Instance.ResetAllCooldowns();
            }
            
            // 保存遊戲（傳入完整的檢查點信息）
            SaveLoadManager.Instance.SaveGame(checkpointID, checkpointSceneName, playerSpawnPosition);
        }
        
        // 觸發事件
        EventManager.TriggerEvent("OnCheckpointActivated");
        EventManager.TriggerEvent("OnCheckpointRest");  // 重生敵人事件
        
        if (showDebugInfo)
        {
            Debug.Log($"檢查點已激活: {checkpointID} (場景: {checkpointSceneName}, 位置: {playerSpawnPosition})");
        }
    }
    
    /// <summary>
    /// 獲取檢查點ID
    /// </summary>
    public string GetCheckpointID()
    {
        return checkpointID;
    }
    
    /// <summary>
    /// 獲取玩家重生位置
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        return playerSpawnPosition;
    }
    
    /// <summary>
    /// 獲取場景名稱
    /// </summary>
    public string GetSceneName()
    {
        return checkpointSceneName;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // 繪製玩家重生位置
        Vector3 spawnPos = playerSpawnPosition == Vector3.zero ? transform.position : playerSpawnPosition;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnPos, Vector3.one * 0.5f);
        Gizmos.DrawLine(transform.position, spawnPos);
    }
}
