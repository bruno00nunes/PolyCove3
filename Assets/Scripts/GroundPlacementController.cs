using UnityEngine;
using UnityEngine.AI;

public class GroundPlacementController : MonoBehaviour
{
    private GameObject _currentPlaceableObject;
    private float _objectRotation;
    private bool _isPlacing;
    private InputManager _inputManager;
    private bool _isPositionValid;

    private void Start()
    {
        _inputManager = GetComponent<InputManager>();
    }

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
        _currentPlaceableObject = Instantiate(objectToPlace);
        _currentPlaceableObject.transform.Rotate(-90f, 0, 0);
    }

    private void MoveCurrentObjectToMouse()
    {
        var ray = _inputManager.gameCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hitInfo, int.MaxValue, _inputManager.terrainLayer)) return;
        if (hitInfo.distance > 60f) SetPositionValid(false);
        CheckValidPosition();
        _currentPlaceableObject.transform.position = hitInfo.point;
        _currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hitInfo.normal);
    }

    private void CheckValidPosition()
    {
        var angles = _currentPlaceableObject.transform.rotation.eulerAngles;
        
        if (!((angles.x <= 270 + 40 && angles.x >= 270 - 40) && (angles.z <= 40 || angles.z >= 320)))
        {
            SetPositionValid(false);
            return;
        }
        SetPositionValid(true);
    }

    private void SetPositionValid(bool isValid)
    {
        _currentPlaceableObject.GetComponent<Renderer>().material.color = isValid ? Color.white : Color.red;
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
        _currentPlaceableObject = null;
    }
}
