// PlayerMovement.cs
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 30f;           // 玩家移動速度
    public float gravity = 160f;            // 重力加速度
    public float rotationSpeed = 10f;       // 玩家旋轉速度

    [Header("Ledge Detection")]
    public float ledgeDetectHeight = 10f;   // 檢測邊緣掉落的高度
    public float ledgeDetectDistance = 0.05f; // 向前檢測邊緣的距離

    [Header("Component References")]
    private CharacterController controller;  // 控制角色移動的組件
    private PlayerState playerState;         // 玩家狀態控制元件
    private PlayerCombat playerCombat;       // 玩家戰鬥功能，用於判斷鎖定狀態
    private Transform cameraTransform;       // 主攝影機變換參考，用於相機相對移動

    private Vector3 moveDirection;           // 當前的移動方向與速度向量

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerState = GetComponent<PlayerState>();
        playerCombat = GetComponent<PlayerCombat>();
        cameraTransform = Camera.main.transform;

        // 檢查是否缺少必要元件
        if (controller == null || playerState == null || playerCombat == null || cameraTransform == null)
        {
            Debug.LogError("PlayerMovement missing required components.");
            enabled = false; // 關閉腳本避免錯誤
            return;
        }
    }

    void Update()
    {
        // 若玩家被動作鎖定（滑步、施法等），僅套用重力不處理輸入
        if (playerState.IsActionLocked)
        {
            ApplyGravity(); 
            controller.Move(moveDirection * Time.deltaTime);
            return;
        }

        // 取得相機相對輸入方向
        float horizontalInput = InputManager.GetHorizontalInput();
        float verticalInput = InputManager.GetVerticalInput();

        // 根據相機方向計算移動向量
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 計算相機相對的水平移動
        Vector3 desiredHorizontalMovement = (cameraForward * verticalInput) + (cameraRight * horizontalInput);

        // 若未鎖定敵人則允許旋轉角色朝移動方向
        if (!playerCombat.IsLockedOn)
        {
            if (desiredHorizontalMovement.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredHorizontalMovement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // 應用移動速度並切換玩家狀態
        if (desiredHorizontalMovement.magnitude > 0.1f)
        {
            desiredHorizontalMovement.Normalize();
            desiredHorizontalMovement *= moveSpeed;
            playerState.SetState(PlayerState.State.Moving);
        }
        else
        {
            desiredHorizontalMovement = Vector3.zero;
            if (playerState.CurrentState == PlayerState.State.Moving)
            {
                playerState.ResetToIdle(); // 回到待機狀態
            }
        }

        // 邊緣偵測邏輯，防止玩家走出平台
        bool canMove = true;
        if (desiredHorizontalMovement.magnitude > 0.1f)
        {
            Vector3 raycastOrigin = transform.position + (desiredHorizontalMovement.normalized * ledgeDetectDistance) + (Vector3.up * 0.5f);
            Debug.DrawRay(raycastOrigin, Vector3.down * ledgeDetectHeight, Color.red);

            // 若射線未擊中地面，代表前方是懸崖，禁止往前
            if (!Physics.Raycast(raycastOrigin, Vector3.down, ledgeDetectHeight))
            {
                canMove = false;
            }
        }

        // 套用垂直方向（重力）
        float verticalVelocity = moveDirection.y;
        moveDirection = new Vector3(desiredHorizontalMovement.x, verticalVelocity, desiredHorizontalMovement.z);

        // 套用重力效果
        ApplyGravity();

        // 若前方是懸崖，停止水平移動
        if (!canMove)
        {
            moveDirection.x = 0f;
            moveDirection.z = 0f;
        }

        // 最終移動指令
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        // 若玩家未接觸地面則套用重力加速度
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -0.5f; // 輕微的向下力保持貼地
        }
    }

    public void ResetVerticalVelocity()
    {
        moveDirection.y = -0.5f; // 重設垂直速度以避免落地後跳動
    }
}
