using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Building : MonoBehaviour
{
    // [Serializable]
    // public class AvailableBuildings
    // {
    //     public GameObject buildingPrefab;
    //     public List<BuildingCost> buildingCosts;
    //
    //     public Texture2D GetBuildingPreview()
    //     {
    //         return AssetPreview.GetAssetPreview(buildingPrefab);
    //     } 
    // }
    
    [Serializable]
    public struct BuildingCost {
        public ResourceType resourceType;
        public int cost;
    
        public BuildingCost(ResourceType resourceType, int cost)
        {
            this.resourceType = resourceType;
            this.cost = cost;
        }
    
        public bool Equals(BuildingCost other)
        {
            return resourceType == other.resourceType && cost == other.cost;
        }
    
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) resourceType * 397) ^ cost;
            }
        }
    }
    
    public int teamId;
    public int maxHitPoints = 100;
    public int buildingSpeed; // Hitpoints gained per second
    public Mesh[] buildingMeshes = new Mesh[3];
    public GameObject healthBarPrefab;
    public Vector3 healthBarOffset;
    private UnitHealthBar _healthBar;
    private float _currentHitPoints;
    private bool _isSelected;
    private int _numBuilders;
    private bool _hasBeenBuilt;
    public List<BuildingCost> buildingCosts;

    private MeshFilter _currentMeshFilter;

    private void Start()
    {
        _currentMeshFilter = GetComponent<MeshFilter>();
        // _currentMeshFilter.mesh = buildingMeshes[0];
        InstantiateHealthBar();
        _currentHitPoints = maxHitPoints / 100f;
    }
    
    private void Update()
    {
        if (_currentHitPoints < maxHitPoints)
        {
            if (!_healthBar.gameObject.activeInHierarchy)
            {
                _healthBar.gameObject.SetActive(true);
            }
        }

        if (!_hasBeenBuilt && _currentHitPoints >= maxHitPoints / 2)
        {
            SetBuildingStage(1);
        }
        if (!_hasBeenBuilt && _currentHitPoints >= maxHitPoints / 3 * 2)
        {
            SetBuildingStage(2);
        }
        if (!_isSelected)
            _healthBar.gameObject.SetActive(false);
    }

    private void InstantiateHealthBar()
    {
        var healthBarGameObj = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);
        healthBarGameObj.transform.SetParent(transform);
        var billboard = healthBarGameObj.GetComponent<Billboard>();

        var teamCamera = Camera.main;
        if (teamCamera == null) throw new Exception("No cameras available");

        billboard.Cam = teamCamera.transform;
        _healthBar = healthBarGameObj.GetComponentInChildren<UnitHealthBar>();
        _healthBar.SetMaxHealth(maxHitPoints);
    }

    private IEnumerator Die()
    {
        var unitTransform = transform;

        var startRotation = unitTransform.rotation;
        var endRotation = Quaternion.Euler(new Vector3(0, 0, 90)) * startRotation;

        var startPosition = unitTransform.position;
        var endPosition = startPosition - new Vector3(0, 3f, 0);

        const int rotateDuration = 1;
        const int sinkDuration = 4;

        for (float t = 0; t < sinkDuration; t += Time.deltaTime)
        {
            if (t < rotateDuration)
                unitTransform.rotation = Quaternion.Lerp(startRotation, endRotation, t / rotateDuration);
            else if (unitTransform.rotation != endRotation)
                unitTransform.rotation = endRotation;

            unitTransform.position = Vector3.Lerp(startPosition, endPosition, t / sinkDuration);
            yield return null;
        }

        unitTransform.position = endPosition;

        Destroy(gameObject);
    }

    public void ToggleSelectionVisual(bool isSelected)
    {
        _isSelected = isSelected;
        if (_healthBar.gameObject.activeInHierarchy != isSelected)
            _healthBar.gameObject.SetActive(isSelected);
    }

    public void SetBuildingStage(int stage)
    {
        if (stage >= buildingMeshes.Length) return;
        if (stage == buildingMeshes.Length - 1) _hasBeenBuilt = true;
        _currentMeshFilter.mesh = buildingMeshes[stage];
    }
}
