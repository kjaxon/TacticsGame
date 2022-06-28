/*
 * Credit for the base code: Kryzarel
 * https://forum.unity.com/threads/tutorial-character-stats-aka-attributes-system.504095/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.ProtoTiles.Examples.Scripts
{
    public class UnitStat
    {
        public readonly float baseValue;
        private bool _isDirty = true;
        private float _value;
 
        private readonly List<StatModifier> _statModifiers;
 
        public UnitStat(float baseValue)
        {
            this.baseValue = baseValue;
            _statModifiers = new List<StatModifier>();
        }
        public float Value {
            get {
                if(_isDirty) {
                    _value = CalculateFinalValue();
                    _isDirty = false;
                }
                return _value;
            }
        }
     
        public void AddModifier(StatModifier mod)
        {
            _isDirty = true;
            _statModifiers.Add(mod);
            _statModifiers.Sort(CompareModifierOrder);
        }
     
        public bool RemoveModifier(StatModifier mod)
        {
            _isDirty = true;
            return _statModifiers.Remove(mod);
        }
     
        private float CalculateFinalValue()
        {
            float finalValue = baseValue;
 
            foreach (var mod in _statModifiers)
            {
                switch (mod.Type)
                {
                    case StatModType.Flat:
                        finalValue += mod.Value;
                        break;
                    case StatModType.Percent:
                        finalValue *= 1 + mod.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            // Rounding gets around dumb float calculation errors (like getting 12.0001f, instead of 12f)
            // 4 significant digits is usually precise enough, but feel free to change this to fit your needs
            return (float)Mathf.Round(finalValue);
        }
        private int CompareModifierOrder(StatModifier a, StatModifier b)
        {
            if (a.Order < b.Order)
                return -1;
            else if (a.Order > b.Order)
                return 1;
            return 0; // if (a.Order == b.Order)
        }
    }
}
