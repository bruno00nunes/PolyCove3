using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class ResourceNode : MonoBehaviour
{
    public ResourceType resourceType;

    public float harvestTime;
    public int availableQuantity;
    private int _totalGatherers;
    public float gatherRadius;
    private readonly Dictionary<int, int> _gatheringUnits = new Dictionary<int, int>();
    private readonly List<TeamManager> _teamManagers = new List<TeamManager>();

    [SerializeField] private bool isTicking;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = gatherRadius;
        foreach (var teamManager in FindObjectsOfType<TeamManager>())
        {
            _teamManagers.Add(teamManager);
        }
    }

    private void LateUpdate()
    {
        if (availableQuantity == 0)
        {
            Destroy(gameObject);
        }

        if (!isTicking && _totalGatherers > 0)
        {
            StartCoroutine(ResourceTick());
        }
    }

    public void ResourceGather()
    {
        foreach (var teamManager in _teamManagers)
        {
            var quantityGathered = _gatheringUnits.ContainsKey(teamManager.teamId)
                ? _gatheringUnits[teamManager.teamId]
                : 0;
            
            if (quantityGathered >= availableQuantity)
                quantityGathered = availableQuantity;

            availableQuantity -= quantityGathered;
            teamManager.AddResource(resourceType, quantityGathered);

            if (availableQuantity == 0) break;
        }
    }

    public void AddGatherer(int teamId)
    {
        if (!_gatheringUnits.ContainsKey(teamId))
        {
            _gatheringUnits[teamId] = 0;
        }
        
        _gatheringUnits[teamId]++;
        _totalGatherers++;
    }

    public void RemoveGatherer(int teamId)
    {
        if (!_gatheringUnits.ContainsKey(teamId)) return;
        
        _gatheringUnits[teamId]--;
        _totalGatherers--;
        _gatheringUnits[teamId] = Mathf.Clamp(_gatheringUnits[teamId], 0, int.MaxValue);
        _totalGatherers = Mathf.Clamp(_totalGatherers, 0, int.MaxValue);
    }

    private IEnumerator ResourceTick()
    {
        isTicking = true;
        while (_totalGatherers > 0)
        {
            yield return new WaitForSeconds(harvestTime);
            ResourceGather();
        }

        isTicking = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Unit")) return;
        if (!other.TryGetComponent<Unit>(out var unit)) return;
        if (unit.GatheringTarget != this) return;
        // AddGatherer(unit.teamId);
        unit.inResourceRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Unit")) return;
        if (!other.TryGetComponent<Unit>(out var unit)) return;
        // if (unit.GatheringTarget != this) return;
        // RemoveGatherer(unit.teamId);
        unit.inResourceRange = false;
    }
}
