using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitStat : MonoBehaviour
{
    public float BaseValue;
    private bool isDirty = true;
    private float _value;
 
    private readonly List<StatModifier> statModifiers;
 
    public UnitStat(float baseValue)
    {
        BaseValue = baseValue;
        statModifiers = new List<StatModifier>();
    }

        // Add these variables

     
    // Change the Value property to this
    public float Value {
        get {
            if(isDirty) {
                _value = CalculateFinalValue();
                isDirty = false;
            }
            return _value;
        }
    }
     
    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
        statModifiers.Sort(CompareModifierOrder);
    }
     
    public bool RemoveModifier(StatModifier mod)
    {
        isDirty = true;
        return statModifiers.Remove(mod);
    }
     
    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
 
        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];
 
            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModType.Percent)
            {
                finalValue *= 1 + mod.Value;
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
