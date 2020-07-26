using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitScripts
{
    public class UnitEventManager : MonoBehaviour
    {
 
        private Dictionary<string, Action<Unit>> _eventDictionary;
 
        private static UnitEventManager _unitEventManager;
 
        public static UnitEventManager Instance
        {
            get
            {
                if (_unitEventManager) return _unitEventManager;
                _unitEventManager = FindObjectOfType(typeof(UnitEventManager)) as UnitEventManager;
 
                if (_unitEventManager == null)
                {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                    return null;
                }
                _unitEventManager.Init();
                return _unitEventManager;
            }
        }

        private void Init()
        {
            if (_eventDictionary == null)
            {
                _eventDictionary = new Dictionary<string, Action<Unit>>();
            }
        }
 
        public static void StartListening(string eventName, Action<Unit> listener)
        {
            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                //Add more event to the existing one
                thisEvent += listener;
 
                //Update the Dictionary
                Instance._eventDictionary[eventName] = thisEvent;
            }
            else
            {
                //Add event to the Dictionary for the first time
                thisEvent += listener;
                Instance._eventDictionary.Add(eventName, thisEvent);
            }
        }
 
        public static void StopListening(string eventName, Action<Unit> listener)
        {
            if (_unitEventManager == null) return;
            if (!Instance._eventDictionary.TryGetValue(eventName, out var thisEvent)) return;
            //Remove event from the existing one
            thisEvent -= listener;

            //Update the Dictionary
            Instance._eventDictionary[eventName] = thisEvent;
        }
 
        public static void TriggerEvent(string eventName, Unit unit)
        {
            if (Instance._eventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.Invoke(unit);
            }
        }
    }
}