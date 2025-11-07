// PlayerCombat.cs
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Component References")]
    private CharacterController controller; // 控制角色移動
    private PlayerMovement playerMovement;  // 用於呼叫重設垂直速度等
    private PlayerState playerState;        // 用於設定狀態（滑步中、閒置等）

    [Header("Lock-On Settings")]
    public GameObject lockOnIndicatorPrefab; // 鎖定指示圖示的預製物
    public float lockOnRange = 25f;          // 鎖定敵人的最遠距離
    public LayerMask enemyLayer;             // 敵人所在的圖層

    private GameObject lockedEnemy;          // 目前被鎖定的敵人
    private GameObject currentLockOnIndicator; // 實例化後的鎖定指示器

    [Header("Dash Settings")]
    public float dashDistance = 5f;          // 滑步距離
    public float dashSpeed = 20f;            // 滑步速度
    public float dashDuration = 0.2f;        // 滑步持續時間
    public float dashStopDistance = 1f;      // 與敵人距離低於此值則停止滑步

    private Vector3 dashDirection;           // 滑步方向
    private float dashTimer;                 // 滑步計時器

    [Header("VFX Settings")]
    public GameObject slashVFXPrefab;        // 攻擊特效預製物

    public bool IsLockedOn => lockedEnemy != null; // 是否目前鎖定敵人

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerState = GetComponent<PlayerState>();

        // 檢查必要元件
        if (controller == null || playerMovement == null || playerState == null)
        {
            Debug.LogError("PlayerCombat missing required components.");
            enabled = false;
            return;
        }

        // 初始化鎖定指示器
        if (lockOnIndicatorPrefab != null)
        {
            currentLockOnIndicator = Instantiate(lockOnIndicatorPrefab);
            currentLockOnIndicator.SetActive(false);
        }

        // 警告未設定敵人圖層
        if (enemyLayer.value == 0)
        {
            Debug.LogWarning("Enemy LayerMask for Lock-On is not set.");
        }
    }

    void Update()
    {
        // 處理鎖定輸入
        if (InputManager.GetLockOnInput())
        {
            FindAndLockEnemy();
        }

        // 更新鎖定指示器位置與顯示
        UpdateLockOnIndicator();

        // 若輸入滑步且有鎖定敵人，並且目前未在滑步中，則啟動滑步
        if (InputManager.GetDashInput() && lockedEnemy != null && !playerState.IsDashing)
        {
            DashToLockedEnemy();
        }

        // 若目前處於滑步狀態則更新位置
        if (playerState.IsDashing)
        {
            UpdateDash();
        }
    }

    private void UpdateLockOnIndicator()
    {
        if (lockedEnemy != null)
        {
            // 若敵人仍存在且位於螢幕視野內，則顯示指示器並面向敵人
            if (lockedEnemy.activeInHierarchy && IsTargetOnScreen(lockedEnemy.transform))
            {
                if (currentLockOnIndicator != null)
                {
                    currentLockOnIndicator.SetActive(true);
                    currentLockOnIndicator.transform.position = lockedEnemy.transform.position + Vector3.up * 0.5f;
                }
                FaceLockedEnemy(); // 面向敵人
            }
            else
            {
                ClearLock(); // 若敵人不在視線內則解除鎖定
            }
        }
        else
        {
            if (currentLockOnIndicator != null)
            {
                currentLockOnIndicator.SetActive(false);
            }
        }
    }

    private void UpdateDash()
    {
        // 滑步移動
        controller.Move(dashDirection * dashSpeed * Time.deltaTime);
        dashTimer -= Time.deltaTime;

        // 若時間結束或接近敵人則結束滑步
        if (dashTimer <= 0f || (lockedEnemy != null && Vector3.Distance(transform.position, lockedEnemy.transform.position) < dashStopDistance))
        {
            EndDash();
        }
    }

    private void FaceLockedEnemy()
    {
        if (lockedEnemy == null) return;

        // 計算朝向敵人的水平方向
        Vector3 directionToTarget = lockedEnemy.transform.position - transform.position;
        directionToTarget.y = 0;

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = targetRotation;
        }
    }

    private void FindAndLockEnemy()
    {
        // 在指定範圍內搜尋敵人
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);
        GameObject closestEnemy = null;
        float minDistance = Mathf.Infinity;

        // 挑選最近的在螢幕可見敵人
        foreach (var hitCollider in hitColliders)
        {
            GameObject enemy = hitCollider.gameObject;

            if (enemy.activeInHierarchy && IsTargetOnScreen(enemy.transform))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        // 若有找到敵人，進行鎖定或解除
        if (closestEnemy != null)
        {
            if (lockedEnemy == closestEnemy)
            {
                ClearLock(); // 若重複鎖定相同敵人則取消鎖定
            }
            else
            {
                lockedEnemy = closestEnemy;
                EventManager.TriggerEvent("OnEnemyLocked");
                Debug.Log("Locked onto: " + lockedEnemy.name);
            }
        }
        else
        {
            ClearLock();
            Debug.Log("No enemy found or visible to lock onto.");
        }
    }

    private void ClearLock()
    {
        // 解除鎖定敵人
        if (lockedEnemy != null)
        {
            Debug.Log("Lock cleared from: " + lockedEnemy.name);
            lockedEnemy = null;
            EventManager.TriggerEvent("OnLockCleared");
        }
        if (currentLockOnIndicator != null)
        {
            currentLockOnIndicator.SetActive(false);
        }
    }

    private void DashToLockedEnemy()
    {
        if (lockedEnemy == null) return;

        // 設定滑步方向並面向敵人
        dashDirection = (lockedEnemy.transform.position - transform.position).normalized;

        Vector3 lookAtTarget = lockedEnemy.transform.position;
        lookAtTarget.y = transform.position.y;
        transform.LookAt(lookAtTarget);

        // 切換狀態為滑步中
        playerState.SetState(PlayerState.State.Dashing);
        dashTimer = dashDuration;
        EventManager.TriggerEvent("OnDashStarted");
        Debug.Log("Dashing towards: " + lockedEnemy.name);
    }

    private void EndDash()
    {
        // 滑步結束，重設狀態與垂直速度
        playerState.ResetToIdle();
        playerMovement.ResetVerticalVelocity();

        // 生成斬擊特效
        if (slashVFXPrefab != null)
        {
            GameObject spawnedVFX = Instantiate(slashVFXPrefab, transform.position, transform.rotation);
        }

        EventManager.TriggerEvent("OnDashEnded");
    }

    private bool IsTargetOnScreen(Transform target)
    {
        // 檢查攝影機是否存在
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found.");
            return false;
        }

        // 判斷目標是否在螢幕可見範圍內
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(target.position);
        return viewportPoint.x > 0 && viewportPoint.x < 1 &&
               viewportPoint.y > 0 && viewportPoint.y < 1 &&
               viewportPoint.z > 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lockOnRange); // 在編輯器中繪出鎖定範圍
    }
}