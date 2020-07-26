using BuildingScripts;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GroundPlacementController : MonoBehaviour
{
    private static GroundPlacementController _instance;
 
    public static GroundPlacementController Instance
    {
        get
        {
            if (_instance) return _instance;
            _instance = FindObjectOfType(typeof(GroundPlacementController)) as GroundPlacementController;

            if (_instance != null) return _instance;
            Debug.LogError("There needs to be one active GroundPlacementController script on a GameObject in your scene.");
            return null;
        }
    }
    
    private GameObject _currentPlaceableObject;
    private float _objectRotation;
    private bool _isPlacing;
    private bool _isPositionValid;

    private void Update()
    {
        if (!_isPlacing) return;
        if (!_currentPlaceableObject) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
        {
            _isPlacing = false;
            Destroy(_currentPlaceableObject);
            return;
        }
        MoveCurrentObjectToMouse();
        RotateObject();
        ReleaseIfClicked();
    }

    public void StartPlacingObject(GameObject objectToPlace)
    {
        _isPlacing = true;
        _currentPlaceableObject = objectToPlace;
        _currentPlaceableObject.transform.Rotate(-90f, 0, 0);
    }

    private void MoveCurrentObjectToMouse()
    {
        var ray = InputManager.Instance.gameCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hitInfo, int.MaxValue, InputManager.Instance.terrainLayer)) return;
        if (hitInfo.distance > 60f) SetPositionValid(false);
        CheckValidPosition(hitInfo);
        _currentPlaceableObject.transform.position = hitInfo.point;
        _currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hitInfo.normal);
    }

    private void CheckValidPosition(RaycastHit hitInfo)
    {
        
        var position = _currentPlaceableObject.transform.position;
        var angle = Vector3.Angle(hitInfo.normal, position); 
        if (angle < 75 || angle > 115)
        {
            SetPositionValid(false);
            return;
        }
        if (_currentPlaceableObject.GetComponent<Building>().Collisions > 0)
        {
            SetPositionValid(false);
            return;
        }
        SetPositionValid(true);
    }

    private void SetPositionValid(bool isValid)
    {
        _currentPlaceableObject.GetComponentInChildren<Renderer>().material.color = isValid ? Color.white : Color.red;
        _isPositionValid = isValid;
    }

    private void RotateObject()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            _objectRotation += 3f;
        }
        if (Input.GetKey(KeyCode.X))
        {
            _objectRotation -= 3f;
        }
        _currentPlaceableObject.transform.Rotate(Vector3.forward, _objectRotation);
    }

    private void ReleaseIfClicked()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!_isPositionValid) return;
        if (_currentPlaceableObject.TryGetComponent<Building>(out var building))
            building.SetBuildingStage(0);
        if (_currentPlaceableObject.TryGetComponent<NavMeshObstacle>(out var obstacle))
            obstacle.enabled = true;
        
        foreach (var buildingCost in building.buildingCosts)
             TeamManager.Instance.RemoveResource(buildingCost.resourceType, buildingCost.cost);

        building.preview = false;
        _currentPlaceableObject = null;
    }
}
