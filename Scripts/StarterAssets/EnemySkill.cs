using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemySkill : MonoBehaviour
{
    public string Name;
    public int index;
    public float staminaCost = 25f;

    public float defaultPosibility = 100f; // can be used for random skill selection in enemy AI
    public float posibility = 100f; // 100f as default, can be used for random skill selection in enemy AI
    public float minPosibility = 25f;
    public float maxPosibility = 400f;

    public float minDistance = -10f; // can be used for distance based skill selection in enemy AI
    public float maxDistance = 3f; // can be used for distance based skill selection in enemy AI
}