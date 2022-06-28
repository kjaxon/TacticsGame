/*
 * Credit for the base code: Kryzarel
 * https://forum.unity.com/threads/tutorial-character-stats-aka-attributes-system.504095/
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.ProtoTiles.Examples.Scripts
{
    public enum StatModType
    {
        Flat,
        Percent,
    }
    public class StatModifier
    {
        public readonly float Value;
        public readonly StatModType Type;
        public readonly int Order;
    
        public StatModifier(float value, StatModType type, int order)
        {
            Value = value;
            Type = type;
            Order = order;
        }
        // Add a new constructor that automatically sets a default Order, in case the user doesn't want to manually define it
        public StatModifier(float value, StatModType type) : this(value, type, (int)type) { }
    }
}