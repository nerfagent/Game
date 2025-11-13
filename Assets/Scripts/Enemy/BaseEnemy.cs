// Assets/Scripts/Enemy/BaseEnemy.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("基礎屬性")]
    [SerializeField] protected float maxHP = 50f;     // 最大生命值
    protected float currentHP;                         // 當前生命值

    [Header("ID 與重生")]
    [Tooltip("敵人在遊戲中的唯一標識符。手動或自動生成以確保唯一性。")]
    [SerializeField] protected string uniqueID;
    [SerializeField] protected bool shouldRespawn = true;      // true = 普通敵人, false = Boss

    [Header("移動")]
    [SerializeField] protected float moveSpeed = 5f;           // 移動速度
    [SerializeField] protected float stoppingDistance = 2f;    // 停止距離（攻擊範圍）

    [Header("偵測")]
    [SerializeField] protected float sightRange = 20f;         // 視野範圍
    [SerializeField] protected float fieldOfViewAngle = 90f;   // 視野角度
    [SerializeField] protected LayerMask playerLayer;          // 玩家所在的圖層
    [SerializeField] protected LayerMask wallLayer;            // 普通牆壁圖層
    [SerializeField] protected LayerMask invisibleWallLayer;   // 隱形牆壁圖層

    [Header("走位逃亡")]
    [SerializeField] protected float evadeRangeMin = 10f;      // 最小走位距離
    [SerializeField] protected float evadeRangeMax = 20f;      // 最大走位距離
    [SerializeField] protected float evadeDurationMin = 2f;    // N: 走位時長最小值
    [SerializeField] protected float evadeDurationMax = 5f;    // M: 走位時長最大值
    [SerializeField] protected float directionChangeInterval = 1f;  // 每秒檢查一次改變方向的概率
    [SerializeField] protected float initialDirectionChangeProbability = 0.1f;  // 初始改變方向概率（初期低）
    [SerializeField] protected float maxDirectionChangeProbability = 0.8f;      // 最大改變方向概率（後期高）

    [Header("元件參考")]
    protected CharacterController characterController;  // 角色控制器
    protected Transform playerTransform;                // 玩家位置參考

    // 狀態機相關
    protected enum EnemyState { Idle, Attacking, Evading, Dead }  // 敵人狀態：待機、攻擊、走位、死亡
    protected EnemyState currentState = EnemyState.Idle;           // 當前狀態
    protected bool playerInSight = false;                          // 玩家是否在視野內

    // 垂直速度與重力
    protected Vector3 velocity = Vector3.zero;  // 當前速度向量
    protected float gravity = 9.81f;            // 重力加速度

    // 敵人 ID（用於事件系統）
    protected int enemyID;

    // 走位相關變數
    protected Vector3 currentEvadeDirection = Vector3.zero;        // 當前走位方向
    protected float evadeDuration = 0f;                            // 本次走位的總時長
    protected float evadeElapsedTime = 0f;                         // 走位已經過時間
    protected float timeSinceLastDirectionChange = 0f;             // 上次改變方向後經過的時間
    protected float currentDirectionChangeProbability = 0.1f;      // 當前改變方向的概率（隨時間增長）

    // 屬性訪問器
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public bool IsDead => currentState == EnemyState.Dead;
    public bool ShouldRespawn => shouldRespawn;
    public string UniqueID => uniqueID;

    protected virtual void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError($"BaseEnemy: {gameObject.name} 需要 CharacterController 元件。");
            enabled = false;
            return;
        }

        currentHP = maxHP;                   // 初始化生命值為滿血
    }

    protected virtual void Start()
    {
        // 尋找場景中的玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("BaseEnemy: 找不到玩家。敵人將保持待機。");
        }
    }

    /// <summary>
    /// 由 EnemySpawner 分配唯一 ID
    /// </summary>
    public void AssignUniqueID(string id)
    {
        this.uniqueID = id;
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead)
            return;  // 死亡狀態下不執行任何邏輯

        CheckForPlayer();    // 檢查玩家是否在視野內
        UpdateState();       // 更新狀態機
        ApplyGravity();      // 套用重力

        // 移動角色控制器
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 主狀態機更新。根據當前狀態執行不同邏輯。
    /// </summary>
    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                // 待機狀態：當看到玩家時進入攻擊狀態
                if (playerInSight && playerTransform != null)
                {
                    currentState = EnemyState.Attacking;
                    OnStateChange(EnemyState.Attacking);
                }
                break;

            case EnemyState.Attacking:
                // 攻擊狀態：執行符卡邏輯
                if (!playerInSight || playerTransform == null)
                {
                    // 玩家逃出視線，回到待機狀態
                    currentState = EnemyState.Idle;
                    velocity.x = 0;
                    velocity.z = 0;
                    OnAttackEnd();  // 通知派生類攻擊結束（重置內部狀態）
                    OnStateChange(EnemyState.Idle);
                }
                else
                {
                    // 執行符卡邏輯（派生類實作具體彈幕發射）
                    ExecuteSpellCard();

                    // 派生類決定符卡是否結束
                    if (IsSpellCardFinished())
                    {
                        // 符卡完成，進入走位狀態
                        PrepareEvadePhase();
                        currentState = EnemyState.Evading;
                        OnAttackEnd();  // 通知派生類攻擊結束
                        OnStateChange(EnemyState.Evading);
                    }
                }
                break;

            case EnemyState.Evading:
                // 走位狀態：遠離玩家並隨機移動
                if (!playerInSight || playerTransform == null)
                {
                    // 玩家逃出視線，回到待機
                    currentState = EnemyState.Idle;
                    velocity.x = 0;
                    velocity.z = 0;
                    OnStateChange(EnemyState.Idle);
                }
                else
                {
                    // 更新走位邏輯
                    UpdateEvadePhase();

                    // 走位時長已到，回到攻擊狀態
                    if (evadeElapsedTime >= evadeDuration)
                    {
                        currentState = EnemyState.Attacking;
                        velocity.x = 0;
                        velocity.z = 0;
                        OnStateChange(EnemyState.Attacking);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 檢查玩家是否在視野範圍內。
    /// 使用距離、角度與射線檢測綜合判斷。
    /// </summary>
    protected virtual void CheckForPlayer()
    {
        playerInSight = false;

        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 超出視野範圍則不可見
        if (distanceToPlayer > sightRange)
            return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // 檢查玩家是否在視野角度內
        if (angleToPlayer < fieldOfViewAngle / 2f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            // 檢查普通牆或隱形牆是否阻擋視線
            LayerMask obstructionMask = wallLayer | invisibleWallLayer;
            if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, sightRange, playerLayer | obstructionMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    playerInSight = true;
                }
            }
        }
    }

    /// <summary>
    /// 執行符卡邏輯。派生類在此實作具體的彈幕發射模式。
    /// </summary>
    protected virtual void ExecuteSpellCard()
    {
        // 派生類覆寫此方法以實現攻擊邏輯
    }

    /// <summary>
    /// 判斷符卡是否已結束。派生類決定什麼代表「結束」（例如發射次數達標）。
    /// </summary>
    protected virtual bool IsSpellCardFinished()
    {
        // 派生類覆寫此方法以決定符卡結束條件
        return false;
    }

    /// <summary>
    /// 攻擊結束時調用。派生類可用此重置內部狀態（如計數器、計時器）。
    /// </summary>
    protected virtual void OnAttackEnd()
    {
        // 派生類可覆寫以重置符卡相關狀態
    }

    /// <summary>
    /// 準備走位階段。
    /// 隨機挑選一個 N~M 秒的走位時長，並選擇初始逃亡方向。
    /// </summary>
    protected virtual void PrepareEvadePhase()
    {
        // 隨機決定本次走位的總時長
        evadeDuration = Random.Range(evadeDurationMin, evadeDurationMax);
        evadeElapsedTime = 0f;
        timeSinceLastDirectionChange = 0f;
        currentDirectionChangeProbability = initialDirectionChangeProbability;

        // 選擇初始逃亡方向（遠離玩家）
        ChooseNewEvadeDirection();
    }

    /// <summary>
    /// 選擇一個新的遠離玩家的方向。
    /// 以隨機角度偏移遠離向量，增加不可預測性。
    /// </summary>
    protected virtual void ChooseNewEvadeDirection()
    {
        Vector3 dirFromPlayer = (transform.position - playerTransform.position).normalized;
        float randomAngle = Random.Range(0f, 360f);

        currentEvadeDirection = Quaternion.Euler(0, randomAngle, 0) * dirFromPlayer;
        currentEvadeDirection.y = 0;
        currentEvadeDirection = currentEvadeDirection.normalized;

        timeSinceLastDirectionChange = 0f;
    }

    /// <summary>
    /// 更新走位邏輯。
    /// 每秒根據逐漸升高的概率改變方向，遇到普通牆或隱形牆時立即改變方向。
    /// </summary>
    protected virtual void UpdateEvadePhase()
    {
        evadeElapsedTime += Time.deltaTime;
        timeSinceLastDirectionChange += Time.deltaTime;

        // 更新改變方向的概率（從低到高線性增長）
        float progressRatio = evadeElapsedTime / evadeDuration;  // 0 ~ 1
        currentDirectionChangeProbability = Mathf.Lerp(initialDirectionChangeProbability, maxDirectionChangeProbability, progressRatio);

        // 每 directionChangeInterval 秒檢查一次是否改變方向
        if (timeSinceLastDirectionChange >= directionChangeInterval)
        {
            if (Random.value < currentDirectionChangeProbability)
            {
                ChooseNewEvadeDirection();
            }
            timeSinceLastDirectionChange = 0f;
        }

        // 移動敵人
        Vector3 moveDirection = currentEvadeDirection;
        moveDirection.y = 0;
        moveDirection = moveDirection.normalized;

        // 檢查前方是否有普通牆或隱形牆（射線偵測）
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        LayerMask obstacleMask = wallLayer | invisibleWallLayer;
        if (Physics.Raycast(rayOrigin, moveDirection, out RaycastHit hit, 1f, obstacleMask))
        {
            // 遇到障礙，立即選擇新方向
            ChooseNewEvadeDirection();
            moveDirection = currentEvadeDirection.normalized;
        }

        // 旋轉敵人面向移動方向
        transform.rotation = Quaternion.LookRotation(moveDirection);

        // 設置速度（僅水平移動）
        velocity.x = moveDirection.x * moveSpeed;
        velocity.z = moveDirection.z * moveSpeed;
    }

    /// <summary>
    /// 敵人受傷。扣除生命值並檢查是否死亡。
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        EventManager.TriggerEvent($"OnEnemy{enemyID}Damaged");

        Debug.Log($"{gameObject.name} 受到 {damage} 點傷害。血量: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 敵人死亡。
    /// </summary>
    protected virtual void Die()
    {
        if (currentState == EnemyState.Dead) return;

        currentState = EnemyState.Dead;
        velocity = Vector3.zero;

        // *** 核心修改：區分普通敵人和 Boss 的死亡記錄方式 ***
        if (shouldRespawn)
        {
            // 普通敵人：通知 EnemyManager 在當前 Session 記錄死亡
            EnemyManager.Instance.RecordSessionEnemyDeath(uniqueID);
        }
        else
        {
            // Boss：在持久化管理器中記錄永久性死亡
            string persistentKey = "BossDefeated_" + uniqueID;
            PersistentStateManager.Instance.SetBoolState(persistentKey, true);
            
            // 同時記錄到 EnemyManager 的列表中以供存檔
            EnemyManager.Instance.RecordBossDefeated(uniqueID);
        }

        EventManager.TriggerEvent($"OnEnemy{GetInstanceID()}Died");
        EventManager.TriggerEvent("OnEnemyDefeated");

        Debug.Log($"{gameObject.name} ({uniqueID}) 已被擊敗！狀態已記錄。");
        
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 狀態改變時調用。派生類可覆寫以處理動畫、音效等。
    /// </summary>
    protected virtual void OnStateChange(EnemyState newState)
    {
        // 派生類可覆寫
    }

    /// <summary>
    /// 應用重力。確保敵人接觸地面或自由落體。
    /// </summary>
    protected virtual void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            velocity.y = -0.5f;  // 輕微向下力保持貼地
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 重置敵人狀態（檢查點重生時調用）。
    /// 恢復滿血並回到待機狀態。
    /// </summary>
    public virtual void ResetEnemy()
    {
        gameObject.SetActive(true);
        currentHP = maxHP;
        currentState = EnemyState.Idle;
        playerInSight = false;
        velocity = Vector3.zero;
        currentEvadeDirection = Vector3.zero;
        evadeElapsedTime = 0f;
        timeSinceLastDirectionChange = 0f;
        currentDirectionChangeProbability = initialDirectionChangeProbability;
        OnAttackEnd();  // 重置符卡內部狀態
    }
}
