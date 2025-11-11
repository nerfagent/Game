// Assets/Scripts/Level/TransitionController.cs
using UnityEngine;
using EasyTransition;  // Easy Transitions namespace

public class TransitionController : MonoBehaviour
{
    public static TransitionController Instance { get; private set; }

    [SerializeField] private TransitionSettings circleWipeTransition;  // Assign CircleWipePlayer in inspector
    
    private TransitionManager transitionManager;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        transitionManager = TransitionManager.Instance();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        
        if (circleWipeTransition == null)
        {
            Debug.LogError("TransitionController: CircleWipePlayer TransitionSettings not assigned in inspector!");
        }
    }

    /// <summary>
    /// 執行以玩家為中心的圓形擦拭過渡
    /// </summary>
    public void PlayCircleWipeTransition(float delay = 0f)
    {
        if (transitionManager == null || circleWipeTransition == null)
        {
            Debug.LogError("TransitionController: TransitionManager or TransitionSettings not available");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("TransitionController: Player transform not found");
            return;
        }

        // 將過渡位置設置到玩家位置
        // Easy Transitions 使用屏幕空間坐標，需要轉換
        Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(playerTransform.position);
        
        Debug.Log($"Playing circle wipe transition at player position: {playerScreenPos}");
        
        // 觸發過渡
        transitionManager.Transition(circleWipeTransition, delay);
    }
}
