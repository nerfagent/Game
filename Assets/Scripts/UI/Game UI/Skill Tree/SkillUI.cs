using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField]
    private Image _skillHolder;
    [SerializeField]
    private Image _skillTimer;

    private BaseSkill _skill;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if(_skill == null)
        {
            Debug.Log("Skill is NULL");
            return;
        }
        float time = _skill.CurrentCooldownPool;
        float totalDuration = _skill.MaxCooldownPool;
        _skillTimer.fillAmount = time/totalDuration;
        if (!_skill.IsReady) _skillHolder.color = new Color(_skillHolder.color.r,_skillHolder.color.g,_skillHolder.color.b,0.5f);
        else _skillHolder.color = new Color(_skillHolder.color.r, _skillHolder.color.g, _skillHolder.color.b, 1f);
    }

    public void AssignSkill(BaseSkill skill)
    {
        _skill = skill;
    }
}
