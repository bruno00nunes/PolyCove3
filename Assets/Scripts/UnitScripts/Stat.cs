using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitScripts
{
    [System.Serializable]
    public class Stat    
    {
        [SerializeField]
        private float baseValue;
    
        private List<float> _modifiers = new List<float>();

        public float GetValue()
        {
            return baseValue + _modifiers.Sum();
        }

        public void AddModifier(float modifier)
        {
            if(modifier > 0)
                _modifiers.Add(modifier);
        }

        public void RemoveModifier(float modifier)
        {
            if(modifier > 0)
                _modifiers.Remove(modifier);
        }
    }
}
