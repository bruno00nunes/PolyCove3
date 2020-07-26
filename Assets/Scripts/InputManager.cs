using System;
using System.Collections.Generic;
using System.Linq;
using BuildingScripts;
using ResourceScripts;
using UnitScripts;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
 
    public static InputManager Instance
    {
        get
        {
            if (_instance) return _instance;
            _instance = FindObjectOfType(typeof(InputManager)) as InputManager;

            if (_instance != null) return _instance;
            Debug.LogError("There needs to be one active InputManager script on a GameObject in your scene.");
            return null;
        }
    }
    
    private readonly LinkedList<Unit> _selectedUnits = new LinkedList<Unit>();
    private bool _firstMove;
    private int _formationIndex;
    private Transform _lastCameraDirection;
    private Vector3 _lastMoveCommand;
    private float _mouseHoldStart;
    private Vector2 _mouseClickStartPos;
    private Building _selectedBuilding;
    
    public Unit[] SelectedUnits => _selectedUnits.ToArray();
        
    [Header("GUI Settings")] public RectTransform selectionBox;

    public int unitSelectionLimit;
    public float mouseHoldInterval;
    private bool _freezeControls = true;

    public LayerMask interactiveLayer, terrainLayer;

    [SerializeField] private UnitFormation currentUnitFormation = UnitFormation.Circle;

    public Camera gameCamera;

    private void OnEnable()
    {
        UnitEventManager.StartListening("unitDeath", DeselectFriendlyUnit);
    }

    private void Update()
    {
        if (_freezeControls) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            _mouseHoldStart = Time.time;
            _mouseClickStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if (_mouseHoldStart + mouseHoldInterval < Time.time)
            {
                UpdateSelectionBox(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_mouseHoldStart + mouseHoldInterval >= Time.time)
                LeftClick();
            else
                HoldLeftClick();
        }

        else if (Input.GetMouseButton(1))
        {
            RightClick();
        }

        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectAllUnits();
        }

        else if (Input.GetKeyDown(KeyCode.V))
        {
            SwitchFormation(null);
        }
        
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (_selectedUnits.Count <= 0) return;
            gameCamera.GetComponent<CameraController>().Follow(_selectedUnits.First.Value.transform);
        }
        
        else if (Input.GetKeyDown(KeyCode.B))
        {
            OpenBuildingMenu();
        }
    }

    private void SwitchFormation(UnitFormation? formation)
    {
        if (formation.HasValue)
            currentUnitFormation = formation.Value;
        else
        {
            _formationIndex = (_formationIndex + 1) % 5 == 0 ? 0 : _formationIndex + 1;
            switch (_formationIndex)
            {
                case 0:
                    currentUnitFormation = UnitFormation.Circle;
                    break;
                case 1:
                    currentUnitFormation = UnitFormation.Line;
                    break;
                case 2:
                    currentUnitFormation = UnitFormation.Square;
                    break;
                case 3:
                    currentUnitFormation = UnitFormation.Wedge;
                    break;
                case 4:
                    currentUnitFormation = UnitFormation.Ranks;
                    break;
            }
        }

        if (_selectedUnits.Count > 0 && _firstMove)
            MoveUnits();
    }

    private void GatherResource(ResourceNode resource)
    {
        foreach (var selectedUnit in _selectedUnits)
        {
            selectedUnit.GatherResource(resource);
        }
    }

    private void RepairBuilding(Building building)
    {
        foreach (var selectedUnit in _selectedUnits)
        {
            selectedUnit.RepairBuilding(building);
        }
    }

    private void MoveUnits()
    {
        var positions = new Vector3[_selectedUnits.Count];
        var hitPosition = _lastMoveCommand;

        switch (currentUnitFormation)
        {
            case UnitFormation.Circle:
            {
                positions[0] = new Vector3();

                int count = 4, count2 = 0;
                for (var i = 1; i < _selectedUnits.Count; i++, count2++)
                {
                    if (count2 == count)
                    {
                        count2 = 0;
                        count *= 2;
                    }

                    float radius = count;

                    var angle = i * Mathf.PI * 2f / radius;
                    positions[i] = new Vector3(Mathf.Cos(angle + 1.58f) * radius / 3, 0,
                        Mathf.Sin(angle + 1.58f) * radius / 3);
                }

                break;
            }
            case UnitFormation.Line:
            {
                var invert = 1f;
                const float spacing = 2f;
                var count = 0;
                for (var i = 0; i < _selectedUnits.Count; i++)
                {
                    count = i % 2 == 1 ? count + 1 : count;

                    positions[i] = new Vector3(count * invert * spacing, 0, 0);

                    invert = invert < 0 ? 1f : -1f;
                }

                break;
            }
            case UnitFormation.Wedge:
            {
                const float verticalSpacing = 2f;
                const float horizontalSpacing = 2f;
                int count = 1, count2 = 0;

                for (var i = 0; i < _selectedUnits.Count; i++)
                {
                    var destination = new Vector3();

                    destination.z -= (count - 1) * verticalSpacing;

                    destination.x -= count2 * horizontalSpacing;
                    destination.x += (count - 1) * horizontalSpacing / 2;

                    positions[i] = destination;

                    count2++;

                    if (count2 % count != 0) continue;
                    count++;
                    count2 = 0;
                }

                break;
            }
            case UnitFormation.Square:
            {
                const float verticalSpacing = 2f;
                const float horizontalSpacing = 2f;
                int count = 0, count2 = 0;
                var unitsPerLine = Mathf.CeilToInt(Mathf.Sqrt(_selectedUnits.Count));

                var offset = new Vector3(unitsPerLine * horizontalSpacing / 2, 0,
                    _selectedUnits.Count / unitsPerLine * horizontalSpacing / 3);

                for (var i = 0; i < _selectedUnits.Count; i++)
                {
                    var destination = new Vector3(verticalSpacing * count, 0, horizontalSpacing * count2);
                    destination -= offset;

                    positions[i] = destination;

                    if (count == unitsPerLine - 1)
                    {
                        count = 0;
                        count2++;
                    }
                    else
                    {
                        count++;
                    }
                }

                break;
            }
            case UnitFormation.Ranks:
            {
                const float verticalSpacing = 2f;
                const float horizontalSpacing = 2f;
                int count = 0, count2 = 0;
                var unitsPerLine = Mathf.CeilToInt(_selectedUnits.Count / 2);
                unitsPerLine = Mathf.Clamp(unitsPerLine, 1, int.MaxValue);

                var offset = new Vector3(unitsPerLine * horizontalSpacing / 2, 0,
                    _selectedUnits.Count / unitsPerLine * horizontalSpacing / 3);

                for (var i = 0; i < _selectedUnits.Count; i++)
                {
                    var destination = new Vector3(verticalSpacing * count, 0, horizontalSpacing * count2);
                    destination -= offset;

                    positions[i] = destination;

                    if (i == unitsPerLine - 1)
                    {
                        count = 0;
                        count2++;
                    }
                    else
                    {
                        count++;
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        var positionCounter = 0;
        foreach (var selectedUnit in _selectedUnits)
        {
            var destination =
                RotatePointAroundPivot(positions[positionCounter++], new Vector3(), _lastCameraDirection.rotation.eulerAngles);
            selectedUnit.MoveTo(destination + hitPosition);
            selectedUnit.lastUnitFormation = currentUnitFormation;
        }
    }

    private void AttackEnemy(Unit targetUnit)
    {
        foreach (var selectedUnit in _selectedUnits)
        {
            selectedUnit.Attack(targetUnit);
        }
    }

    private void AttackEnemy(Building targetBuilding)
    {
        foreach (var selectedUnit in _selectedUnits)
        {
            selectedUnit.Attack(targetBuilding);
        }
    }

    private void LeftClick()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
            DeselectAllUnits();
        var raycastResults = new RaycastHit[10];

        var hits = Physics.RaycastNonAlloc(gameCamera.ScreenPointToRay(Input.mousePosition),
            raycastResults,
            120f,
            interactiveLayer);

        for (var i = 0; i < hits; i++)
        {
            var hit = raycastResults[i];
            var hitTransform = hit.transform;

            if (hitTransform.CompareTag("Unit"))
            {
                var selectedUnit = hitTransform.GetComponent<Unit>();

                if (!selectedUnit) continue;

                if (selectedUnit.teamId != TeamManager.Instance.teamId)
                {
                    SelectEnemyUnit(selectedUnit);
                    return;
                }

                ToggleUnitSelect(selectedUnit);
            }
            else if (hitTransform.CompareTag("Resource"))
            {
                SelectResourceNode(hitTransform.GetComponent<ResourceNode>());
            }
            else if (hitTransform.CompareTag("Building"))
            {
                SelectBuilding(hit.transform.GetComponent<Building>());
            }
        }
    }

    private void HoldLeftClick()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
            DeselectAllUnits();
        CloseBuildingMenu();
        selectionBox.gameObject.SetActive(false);

        var min = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2;
        var max = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2;

        foreach (var teamUnit in TeamManager.Instance.GetTeamUnits())
        {
            var screenPos = gameCamera.WorldToScreenPoint(teamUnit.transform.position);

            if (!(screenPos.x > min.x) || !(screenPos.x < max.x) || !(screenPos.y > min.y) ||
                !(screenPos.y < max.y)) continue;
            ToggleUnitSelect(teamUnit);
        }
    }

    private void RightClick()
    {
        CloseBuildingMenu();
        
        if (_selectedUnits.Count <= 0) return;
        if (_selectedUnits.Count == 1)
            if (_selectedUnits.First.Value.teamId != TeamManager.Instance.teamId)
                return;

        if (!Physics.Raycast(gameCamera.ScreenPointToRay(Input.mousePosition), out var hit, 120f)) return;
        if (1 << hit.collider.gameObject.layer != terrainLayer)
        {
            var targetHit = hit.transform;
            if (targetHit.CompareTag("Unit"))
            {
                var unit = targetHit.GetComponent<Unit>();
                if (unit.teamId == TeamManager.Instance.teamId) return;

                AttackEnemy(unit);
            }
            else if (targetHit.CompareTag("Resource"))
            {
                var targetResource = targetHit.GetComponent<ResourceNode>();

                if (!targetResource)
                    targetResource = targetHit.GetComponentInParent<ResourceNode>();

                if (!targetResource) return;
                
                GatherResource(targetResource);
            }
            else if (targetHit.CompareTag("Building"))
            {
                var building = targetHit.GetComponent<Building>();
                if (building.teamId == TeamManager.Instance.teamId)
                {
                    RepairBuilding(building);
                }

                AttackEnemy(building);
            }
        }
        else
        {
            _lastMoveCommand = hit.point;
            _lastCameraDirection = transform.parent;
            _firstMove = true;
            MoveUnits();
        }
    }

    private void SelectResourceNode(ResourceNode node)
    {
        DeselectAllUnits();
        // ShowNodeUI(node);
    }

    private void SelectEnemyUnit(Unit selectedUnit)
    {
        DeselectAllUnits();
        ToggleUnitSelect(selectedUnit);
        // selectedUnit.ToggleSelectionVisual(true);
    }

    private void SelectBuilding(Building selectedBuilding)
    {
        DeselectAllUnits();
        _selectedBuilding = selectedBuilding;
        selectedBuilding.ToggleSelectionVisual(true);
    }

    private void ToggleUnitSelect(Unit unit)
    {
        if (_selectedUnits.Contains(unit))
        {
            DeselectFriendlyUnit(unit);
        }
        else
        {
            if (_selectedUnits.Count >= unitSelectionLimit - 1) return;
            if(_selectedUnits.Count == 0)
                currentUnitFormation = unit.lastUnitFormation;
            
            _selectedUnits.AddLast(unit);
            unit.ToggleSelectionVisual(true);
        }

        EventManager.TriggerEvent("selectedUnitsChanged");
    }

    private void DeselectFriendlyUnit(Unit unit)
    {
        if (!_selectedUnits.Contains(unit)) return;
        
        unit.ToggleSelectionVisual(false);
        _selectedUnits.Remove(unit);
        EventManager.TriggerEvent("selectedUnitsChanged");
    }

    private void DeselectAllUnits()
    {
        if (_selectedBuilding)
        {
            _selectedBuilding.ToggleSelectionVisual(false);
            _selectedBuilding = null;
        }

        foreach (var selectedUnit in _selectedUnits) selectedUnit.ToggleSelectionVisual(false);
        _selectedUnits.Clear();
        _firstMove = false;
        EventManager.TriggerEvent("selectedUnitsChanged");
    }

    private void UpdateSelectionBox(Vector2 currentMousePos)
    {
        if (!selectionBox.gameObject.activeInHierarchy)
            selectionBox.gameObject.SetActive(true);

        var width = currentMousePos.x - _mouseClickStartPos.x;
        var height = currentMousePos.y - _mouseClickStartPos.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        selectionBox.anchoredPosition = _mouseClickStartPos + new Vector2(width / 2, height / 2);
    }

    private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        var dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    private void OpenBuildingMenu()
    {
        UiManager.Instance.ShowBuildingMenu();
    }

    private void CloseBuildingMenu()
    {
        UiManager.Instance.HideBuildingMenu();
    }

    public void FreezeControls(bool freeze)
    {
        gameCamera.GetComponent<CameraController>().freezeCamera = freeze;
        _freezeControls = freeze;
    }
}