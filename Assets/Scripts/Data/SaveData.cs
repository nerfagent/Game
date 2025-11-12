// Assets/Scripts/Data/SaveData.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // 檢查點信息
    public string lastCheckpointID = "";
    public string lastCheckpointSceneName = "";
    public Vector3 lastCheckpointPosition = Vector3.zero;
    
    // 玩家數值
    public int maxHP = 100;
    
    // 技能升級（每個技能的升級列表）
    public List<string> skill0Upgrades = new List<string>();
    public List<string> skill1Upgrades = new List<string>();
    public List<string> skill2Upgrades = new List<string>();
    public List<string> skill3Upgrades = new List<string>();
    
    // 持久化狀態
    public Dictionary<string, bool> booleanStates = new Dictionary<string, bool>();
    public Dictionary<string, int> integerStates = new Dictionary<string, int>();
    public Dictionary<string, float> floatStates = new Dictionary<string, float>();
    public Dictionary<string, string> stringStates = new Dictionary<string, string>();
    
    // 已擊敗的Boss
    public List<string> defeatedBosses = new List<string>();
    
    // 時間戳
    public string saveTimestamp = "";
}
