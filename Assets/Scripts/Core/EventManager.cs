//EventManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    // 儲存事件名稱與對應的委派（Action）
    private static Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    // 註冊（監聽）事件
    public static void StartListening(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent += listener; // 將新的監聽者加入既有事件
            eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener; // 建立新事件並加入監聽者
            eventDictionary.Add(eventName, thisEvent);
        }
    }

    // 取消事件監聽
    public static void StopListening(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent -= listener; // 移除監聽者
            eventDictionary[eventName] = thisEvent;
        }
    }

    // 觸發事件（通知所有監聽者）
    public static void TriggerEvent(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent?.Invoke(); // 執行所有已登錄的委派方法
        }
    }
}
