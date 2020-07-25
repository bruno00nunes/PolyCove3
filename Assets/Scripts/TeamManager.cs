using System;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public int teamId;
    [SerializeField] private List<Unit> teamUnits = new List<Unit>();
    [SerializeField] private Dictionary<ResourceType, int> teamResources = new Dictionary<ResourceType, int>();
    public List<GameObject> availableBuildings = new List<GameObject>();
    private GroundPlacementController _groundPlacementController;

    private void Start()
    {
        _groundPlacementController = GetComponent<GroundPlacementController>();
        foreach (var type in (ResourceType[]) Enum.GetValues(typeof(ResourceType)))
        {
            teamResources.Add(type, 0);
        }
    }

    private void OnEnable()
    {
        UnitEventManager.StartListening("unitSpawn", AddUnit);
        UnitEventManager.StartListening("unitDeath", RemoveUnit);
    }
    
    public IEnumerable<Unit> GetTeamUnits()
    {
        return new List<Unit>(teamUnits);
    }

    public int GetResourceQuantity(ResourceType type)
    {
        return teamResources.TryGetValue(type, out var quantity) ? quantity : 0;
    }

    private void AddUnit(Unit newUnit)
    {
        if (newUnit.teamId == teamId)
            teamUnits.Add(newUnit);
    }

    private void RemoveUnit(Unit deadUnit)
    {
        teamUnits.Remove(deadUnit);
    }

    public bool CheckResources(ResourceType type, int quantity)
    {
        return teamResources[type] >= quantity;
    }

    public void AddResource(ResourceType resourceType, int quantity)
    {
        if (teamResources.ContainsKey(resourceType))
            teamResources[resourceType] += quantity;
        else
            teamResources[resourceType] = quantity;
        EventManager.TriggerEvent(resourceType+"ResourceChanged");
    }

    public void RemoveResource(ResourceType resourceType, int quantity)
    {
        if (teamResources.ContainsKey(resourceType))
            teamResources[resourceType] += quantity;
        else
            teamResources[resourceType] = quantity;
        EventManager.TriggerEvent(resourceType+"ResourceChanged");
    }

    public void TryPlacePrefab(GameObject prefab)
    {
        _groundPlacementController.StartPlacingObject(prefab);
    }
}
