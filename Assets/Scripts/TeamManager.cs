using System;
using System.Collections.Generic;
using System.Linq;
using BuildingScripts;
using ResourceScripts;
using UnitScripts;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private static TeamManager _instance;
 
    public static TeamManager Instance
    {
        get
        {
            if (_instance) return _instance;
            _instance = FindObjectOfType(typeof(TeamManager)) as TeamManager;

            if (_instance != null) return _instance;
            Debug.LogError("There needs to be one active TeamManager script on a GameObject in your scene.");
            return null;
        }
    }

    public int teamId;
    public int startingResources;
    [SerializeField] private List<Unit> teamUnits = new List<Unit>();
    [SerializeField] private Dictionary<ResourceType, int> teamResources = new Dictionary<ResourceType, int>();

    public int CurrentPopulation { get; private set; }
    public int MaxPopulation { get; private set; }

    // public List<GameObject> availableBuildings = new List<GameObject>();

    private void Start()
    {
        foreach (var type in (ResourceType[]) Enum.GetValues(typeof(ResourceType)))
        {
            AddResource(type, startingResources);
        }

        AddCurrentPopulation(2);
        AddMaxPopulation(2);
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
        CurrentPopulation -= deadUnit.populationCost;
        CurrentPopulation = Mathf.Clamp(CurrentPopulation, 0, int.MaxValue);
        teamUnits.Remove(deadUnit);
    }

    public bool CheckResources(ResourceType type, int quantity)
    {
        Debug.Log(type);
        return teamResources.TryGetValue(type, out var available) && available >= quantity;
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
            teamResources[resourceType] -= quantity;
        else
            teamResources[resourceType] = quantity;

        EventManager.TriggerEvent(resourceType+"ResourceChanged");
    }

    public void TryPlacePrefab(GameObject prefab)
    {
        var buildingObject = Instantiate(prefab);
        if (!buildingObject.TryGetComponent<Building>(out var building))
        {
            Destroy(buildingObject);
            return;
        }

        if (building.buildingCosts.Any(buildingCost => !CheckResources(buildingCost.resourceType, buildingCost.cost)))
        {
            Destroy(buildingObject);
            return;
        }

        if (building.populationCost + Instance.CurrentPopulation > Instance.MaxPopulation)
        {
            Destroy(buildingObject);
            return;
        }

        GroundPlacementController.Instance.StartPlacingObject(buildingObject);
    }

    public void AddCurrentPopulation(int i)
    {
        i = Mathf.Abs(i);
        CurrentPopulation += i;
        EventManager.TriggerEvent("PopulationResourceChanged");
    }

    public void RemoveCurrentPopulation(int i)
    {
        i = Mathf.Abs(i);
        CurrentPopulation -= i;
        CurrentPopulation = Mathf.Clamp(CurrentPopulation, 0, int.MaxValue);
        EventManager.TriggerEvent("PopulationResourceChanged");
    }

    public void AddMaxPopulation(int i)
    {
        i = Mathf.Abs(i);
        MaxPopulation += i;
        EventManager.TriggerEvent("PopulationResourceChanged");
    }

    public void RemoveMaxPopulation(int i)
    {
        i = Mathf.Abs(i);
        MaxPopulation -= i;
        MaxPopulation = Mathf.Clamp(MaxPopulation, 0, int.MaxValue);
        EventManager.TriggerEvent("PopulationResourceChanged");
    }
}
