// Assets/Scripts/Core/InputManager.cs
using UnityEngine;

public static class InputManager
{
    // Movement input
    public static float GetHorizontalInput() => Input.GetAxisRaw("Horizontal");
    public static float GetVerticalInput() => Input.GetAxisRaw("Vertical");

    // Combat input
    public static bool GetLockOnInput() => Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
    public static bool GetDashInput() => Input.GetKeyDown(KeyCode.Z);

    // Skill input (for Phase 3)
    public static bool GetSkill1Input() => Input.GetKeyDown(KeyCode.Q);
    public static bool GetSkill2Input() => Input.GetKeyDown(KeyCode.W);
    public static bool GetSkill3Input() => Input.GetKeyDown(KeyCode.E);
    public static bool GetSkill4Input() => Input.GetKeyDown(KeyCode.R);

    // Pause input
    public static bool GetPauseInput() => Input.GetKeyDown(KeyCode.Escape);
}
