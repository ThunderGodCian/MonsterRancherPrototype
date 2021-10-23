using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TechContent
{
    public string techName;


    public enum techRanges
    {
        CLOSE,
        MID,
        LONG,
        EXTREME
    }
    public techRanges techRange = new techRanges();

    public enum techTypes
    {
        ATTACK,
        MOVEMENT,
        BUFF,
        DEBUFF
    }
    public techTypes techType = new techTypes();

    public enum techDamages
    {
        POW,
        INT
    }
    public techDamages techDamage = new techDamages();

    public float cost;
    public float lifeDamage;
    public float hit;
    public float staminaDamage;
}
