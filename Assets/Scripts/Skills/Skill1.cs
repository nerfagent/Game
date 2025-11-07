// Skill1.cs
using UnityEngine;

public class Skill1 : BaseSkill
{
    public Skill1()
    {
        skillName = "Fireball";
        description = "Launch a fireball at enemies";
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
        
        Debug.Log("Skill1 Effect Applied!");
        // Add your skill effect logic here
    }
}
