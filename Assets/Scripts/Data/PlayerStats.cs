[System.Serializable]
public class PlayerStats
{
    public int MaxHP = 100;
    public int CurrentHP = 100;

    public int MaxMana = 100;
    public int CurrentMana = 100;

    public int Level = 1;
    public int Experience = 0;

    // Add skill cooldown states here if you want to track them persistently
}
