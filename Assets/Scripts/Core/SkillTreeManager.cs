using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillTreeManager : MonoBehaviour 
{
    public static UnityAction<SkillNodeUI> OnSkillUpgrade; 
    [SerializeField]
    private List<SkillNodeUI> parentNodes;

    private void Awake()
    {
        OnSkillUpgrade += UpdateSkill;
    }

    private void UpdateSkill(SkillNodeUI nodeUI)
    {
        int i = (int)nodeUI.SkillNode.SkillType;
        BaseSkill skill = CooldownSystem.Instance.GetSkill(i);
        SkillUpgradeManager.Instance.ApplyUpgrade(skill,nodeUI.SkillNode.UpgradeType,nodeUI.SkillNode.Value);
        SkillUpgradeManager.Instance.RecordUpgrade(i, nodeUI.SkillNode.UpgradeType);
    }

}
