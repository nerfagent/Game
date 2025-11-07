// Skill2.cs
using UnityEngine;

public class Skill2 : BaseSkill
{
    public Skill2()
    {
        skillName = "Teleport";
        description = "Teleport";
        baseCooldown = 40f;
        castTime = 1f;
        baseDamage = 10f;
    }

    public override void Cast(Transform casterTransform)
    {
        base.Cast(casterTransform);
    }

    protected override void OnCastComplete()
    {
        base.OnCastComplete();
        
        Debug.Log("Skill2");
    }
}