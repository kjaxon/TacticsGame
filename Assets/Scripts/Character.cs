using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles.Examples.Scripts;
using UnityEngine.Serialization;

public class Character : MonoBehaviour
{
    [SerializeField] private List<UNIT_STAT_TYPE> statTypes;
    [SerializeField] private List<float> statStartingValues;
    [SerializeField] private int experience;
    [SerializeField] private int expToNextLvl;

    private bool addingExp;
    public enum UNIT_STAT_TYPE {
        HEALTH,
        STRENGTH,
        ATTACK
    }

    private Dictionary<UNIT_STAT_TYPE, UnitStat> unitStats = new Dictionary<UNIT_STAT_TYPE, UnitStat>();

    private void Awake(){
        for (int i = 0; i < statTypes.Count; i++) {
            unitStats.Add(statTypes[i], new UnitStat(statStartingValues[i]));
        }
        experience = 0;
        expToNextLvl = 5;
        Attack(this);
    }

    /// <summary>
    /// Calculates result of attack on other unit
    /// </summary>
    /// <param name="other">other character unit</param>
    /// <returns>A dictionary of new stats for the other unit</returns>
    public Dictionary<UNIT_STAT_TYPE, float> Attack(Character other) {
        float newOtherHealth = other.unitStats[UNIT_STAT_TYPE.HEALTH].Value;
        return new Dictionary<UNIT_STAT_TYPE, float>{ {UNIT_STAT_TYPE.HEALTH, newOtherHealth} };
    }

    public Dictionary<UNIT_STAT_TYPE, float> LevelUp()
    {
        return new Dictionary<UNIT_STAT_TYPE, float>();
    }

    public void AddExperience(int gainedExp)
    {
        experience += gainedExp;
        if (experience < expToNextLvl) return;
        LevelUp();
        expToNextLvl += 5;
        //TODO Exp scaling
        //TODO Exp Bar
    }
}
