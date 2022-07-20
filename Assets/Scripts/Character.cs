using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedBjorn.ProtoTiles.Examples.Scripts;

public class Character : MonoBehaviour
{
    [SerializeField] private List<UNIT_STAT_TYPE> statTypes;
    [SerializeField] private List<float> statStartingValues;
    public enum UNIT_STAT_TYPE {
        HEALTH,
        STRENGTH,
        ATTACK
    }

    private Dictionary<UNIT_STAT_TYPE, UnitStat> unitStats = new Dictionary<UNIT_STAT_TYPE, UnitStat>();

    private void Awake(){
        for(int i = 0; i < statTypes.Count; i++){
            unitStats.Add(statTypes[i], new UnitStat(statStartingValues[i]));
        }
    }
}
