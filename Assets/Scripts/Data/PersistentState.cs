using System.Collections.Generic;

[System.Serializable]
public class PersistentState
{
    // Example using keys
    public List<string> completedQuests = new List<string>();
    public List<string> openedDoors = new List<string>();
    public List<string> defeatedBosses = new List<string>();

    // Define methods to add/query states as needed
}
