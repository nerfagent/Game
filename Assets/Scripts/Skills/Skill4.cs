// Skill4.cs
using UnityEngine;

public class Skill4 : BaseSkill
{
    public Skill4()
    {
        skillName = "ASD";
        description = "ASD";
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
        
        Debug.Log("Skill4");
    }
}