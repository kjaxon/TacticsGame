using System;
using UnityEngine;

namespace RedBjorn.ProtoTiles.Examples.Scripts
{
    public class MainCharacter : MonoBehaviour
    {
        private UnitStat Health = new UnitStat(5);
        private UnitStat Strength  = new UnitStat(9);
        private UnitStat Attack = new UnitStat(7);

        private void Init()
        {
            print("HEALTH: " + Health.baseValue);
            print("STRENGTH: " + Strength.baseValue);
            print("ATTACK: " + Attack.baseValue);
        }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Health.AddModifier(new StatModifier(5.0f, StatModType.Flat));
                
                print("HEALTH: " + Health.Value);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                Attack.AddModifier(new StatModifier(1f, StatModType.Flat));
                Attack.AddModifier(new StatModifier(.10f, StatModType.Percent));
                print("ATTACK: " + Attack.Value);
            }
        }
    }
}
