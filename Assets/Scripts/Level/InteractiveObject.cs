// Assets/Scripts/Level/InteractiveObject.cs
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public abstract class InteractiveObject : MonoBehaviour
{
    [SerializeField] protected string stateKey;  // 唯一狀態鍵
    [SerializeField] protected Collider objectCollider;
    protected PersistentStateManager persistentState;

    // 為每個物件實例存儲其特定的事件
    public UnityAction onObjectActivated;
    public UnityAction onObjectDeactivated;

    protected virtual void Awake()
    {
        // 初始化 UnityAction
        onObjectActivated = new UnityAction(() => { });
        onObjectDeactivated = new UnityAction(() => { });
    }

    protected virtual void Start()
    {
        persistentState = PersistentStateManager.Instance;
        if (objectCollider == null)
            objectCollider = GetComponent<Collider>();

        // 初始化時恢復狀態
        RestoreState();
    }

    /// <summary>
    /// 啟用此物件（改變永久狀態）
    /// </summary>
    public virtual void Activate()
    {
        persistentState.SetBoolState(stateKey, true);
        OnStateChanged(true);
        onObjectActivated.Invoke();
        Debug.Log($"{gameObject.name} 已啟用");
    }

    /// <summary>
    /// 停用此物件
    /// </summary>
    public virtual void Deactivate()
    {
        persistentState.SetBoolState(stateKey, false);
        OnStateChanged(false);
        onObjectDeactivated.Invoke();
    }

    /// <summary>
    /// 恢復此物件的永久狀態
    /// </summary>
    public virtual void RestoreState()
    {
        bool isActive = persistentState.GetBoolState(stateKey, false);
        OnStateChanged(isActive);
    }

    /// <summary>
    /// 狀態改變時調用。派生類實作具體行為。
    /// </summary>
    protected abstract void OnStateChanged(bool isActive);
}
