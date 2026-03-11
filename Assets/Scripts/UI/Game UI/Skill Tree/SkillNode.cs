using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillNode
{
    public string name;
    public string UpgradeType; //This needs to be changed in the future.
    public SkillType SkillType = SkillType.FIREBALL;
    [SerializeField]private float value; 
    [SerializeField]private bool locked = true;
    [SerializeField]private bool unlockable = false;
    [SerializeField]private List<SkillNodeUI> children;

    public bool IsLocked => locked;
    public bool Unlockable => unlockable;
    public float Value => value;
    public void ApplyUpgrade()
    {
        if (!locked) Debug.Log("Skill has already been upgraded");
        locked = false;
        if (unlockable && !locked)
        {
            foreach (SkillNodeUI nodeUI in children)
            {
                nodeUI.SkillNode.SetUnlockable(true);
                nodeUI.UpdateUpgrade();
            }
        }
    }

    private void SetUnlockable(bool unlockable)
    {
        this.unlockable = unlockable;
    }

}

public enum SkillType { 
    FIREBALL,
    SKILL2,
    SKILL3,
    SKILL4
}
