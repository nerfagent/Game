// Assets/Scripts/Level/PersistentStateManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PersistentStateManager : MonoBehaviour
{
    public static PersistentStateManager Instance { get; private set; }

    // 永久狀態字典：key = 狀態識別碼, value = 布林值或資料
    private Dictionary<string, bool> booleanStates = new Dictionary<string, bool>();
    private Dictionary<string, int> integerStates = new Dictionary<string, int>();
    private Dictionary<string, float> floatStates = new Dictionary<string, float>();
    private Dictionary<string, string> stringStates = new Dictionary<string, string>();

    private void Awake()
    {
        // 單例模式：確保只有一個實例
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

    /// <summary>
    /// 設定布林狀態（例如：門是否已開啟）
    /// </summary>
    public void SetBoolState(string stateKey, bool value)
    {
        booleanStates[stateKey] = value;
        EventManager.TriggerEvent($"OnStateChanged_{stateKey}");
        Debug.Log($"State '{stateKey}' set to {value}");
    }

    /// <summary>
    /// 取得布林狀態
    /// </summary>
    public bool GetBoolState(string stateKey, bool defaultValue = false)
    {
        return booleanStates.TryGetValue(stateKey, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// 設定整數狀態（例如：擊敗的敵人數量）
    /// </summary>
    public void SetIntState(string stateKey, int value)
    {
        integerStates[stateKey] = value;
        EventManager.TriggerEvent($"OnStateChanged_{stateKey}");
    }

    /// <summary>
    /// 取得整數狀態
    /// </summary>
    public int GetIntState(string stateKey, int defaultValue = 0)
    {
        return integerStates.TryGetValue(stateKey, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// 增加整數狀態
    /// </summary>
    public void IncrementIntState(string stateKey, int increment = 1)
    {
        if (!integerStates.ContainsKey(stateKey))
            integerStates[stateKey] = 0;
        
        integerStates[stateKey] += increment;
        EventManager.TriggerEvent($"OnStateChanged_{stateKey}");
    }

    /// <summary>
    /// 設定浮點數狀態
    /// </summary>
    public void SetFloatState(string stateKey, float value)
    {
        floatStates[stateKey] = value;
        EventManager.TriggerEvent($"OnStateChanged_{stateKey}");
    }

    /// <summary>
    /// 取得浮點數狀態
    /// </summary>
    public float GetFloatState(string stateKey, float defaultValue = 0f)
    {
        return floatStates.TryGetValue(stateKey, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// 設定字串狀態
    /// </summary>
    public void SetStringState(string stateKey, string value)
    {
        stringStates[stateKey] = value;
        EventManager.TriggerEvent($"OnStateChanged_{stateKey}");
    }

    /// <summary>
    /// 取得字串狀態
    /// </summary>
    public string GetStringState(string stateKey, string defaultValue = "")
    {
        return stringStates.TryGetValue(stateKey, out var value) ? value : defaultValue;
    }
    
    /// <summary>
    /// 獲取所有布林狀態
    /// </summary>
    public Dictionary<string, bool> GetAllBoolStates()
    {
        return new Dictionary<string, bool>(booleanStates);
    }

    /// <summary>
    /// 獲取所有整數狀態
    /// </summary>
    public Dictionary<string, int> GetAllIntStates()
    {
        return new Dictionary<string, int>(integerStates);
    }

    /// <summary>
    /// 獲取所有浮點數狀態
    /// </summary>
    public Dictionary<string, float> GetAllFloatStates()
    {
        return new Dictionary<string, float>(floatStates);
    }

    /// <summary>
    /// 獲取所有字串狀態
    /// </summary>
    public Dictionary<string, string> GetAllStringStates()
    {
        return new Dictionary<string, string>(stringStates);
    }

    /// <summary>
    /// 從存檔加載狀態
    /// </summary>
    public void LoadStates(
        Dictionary<string, bool> boolStates,
        Dictionary<string, int> intStates,
        Dictionary<string, float> floatStates,
        Dictionary<string, string> strStates)
    {
        booleanStates = new Dictionary<string, bool>(boolStates);
        integerStates = new Dictionary<string, int>(intStates);
        this.floatStates = new Dictionary<string, float>(floatStates);
        stringStates = new Dictionary<string, string>(strStates);
        
        Debug.Log("持久化狀態已從存檔加載");
    }

    /// <summary>
    /// 檢查狀態是否存在
    /// </summary>
    public bool HasState(string stateKey)
    {
        return booleanStates.ContainsKey(stateKey) ||
               integerStates.ContainsKey(stateKey) ||
               floatStates.ContainsKey(stateKey) ||
               stringStates.ContainsKey(stateKey);
    }

    /// <summary>
    /// 清除所有狀態
    /// </summary>
    public void ClearAllStates()
    {
        booleanStates.Clear();
        integerStates.Clear();
        floatStates.Clear();
        stringStates.Clear();
        Debug.Log("All persistent states cleared");
    }
}
