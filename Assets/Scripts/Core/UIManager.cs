// Assets/Scripts/Core/UIManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    List<SkillUI> skillUIList;
    private void Start()
    {
        if(CooldownSystem.Instance != null)
        {
            int skillCount = CooldownSystem.Instance.GetSkillCount();
            if (skillUIList.Count > skillCount)
            {
                Debug.LogError("SkillUI and skill size mismatch");
            }
            else
            {
                for (int i = 0; i < skillCount; i++)
                {
                    skillUIList[i].AssignSkill(CooldownSystem.Instance.GetSkill(i));
                }
            }
        }
        else
        {
            Debug.LogError("CooldownSystem Instance is null");
        }
    }


}
