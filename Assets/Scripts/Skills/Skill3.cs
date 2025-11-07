// Skill3.cs
using UnityEngine;

public class Skill3 : BaseSkill
{
    public Skill3()
    {
        skillName = "Shoot";
        description = "Shoot";
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
        
        Debug.Log("Skill3");
    }
}