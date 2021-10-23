using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStats : MonoBehaviour
{
    [SerializeField]
    public float lifeMax;
    public float lifeCurrent;
    public float staminaMax;
    public float staminaCurrent;
    public float power;
    public float intelligence;
    public float skill;
    public float speed;
    public float defense;
    public float resist;
    public string monsterName;

    public List<string> movesKnown = new List<string>();
}
