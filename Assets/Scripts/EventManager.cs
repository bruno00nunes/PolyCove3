using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private Dictionary <string, UnityEvent> _eventDictionary;

    private static EventManager _eventManager;

    private static EventManager Instance
    {
        get
        {
            if (_eventManager) return _eventManager;
            _eventManager = FindObjectOfType (typeof (EventManager)) as EventManager;

            if (!_eventManager)
            {
                Debug.LogError ("There needs to be one active EventManger script on a GameObject in your scene.");
            }
            else if (_eventManager != null) _eventManager.Init();

            return _eventManager;
        }
    }

    private void Init ()
    {
        if (_eventDictionary == null)
        {
            _eventDictionary = new Dictionary<string, UnityEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction listener)
    {
        if (Instance._eventDictionary.TryGetValue (eventName, out var thisEvent))
        {
            thisEvent.AddListener (listener);
        } 
        else
        {
            thisEvent = new UnityEvent ();
            thisEvent.AddListener (listener);
            Instance._eventDictionary.Add (eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction listener)
    {
        if (_eventManager == null) return;
        if (Instance._eventDictionary.TryGetValue (eventName, out var thisEvent))
        {
            thisEvent.RemoveListener (listener);
        }
    }

    public static void TriggerEvent(string eventName)
    {
        if (Instance._eventDictionary.TryGetValue (eventName, out var thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}