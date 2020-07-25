using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UiManager : MonoBehaviour
{
    public TextMeshProUGUI selectedUnitsTextNode;
    public TextMeshProUGUI foodResourceText;
    public TextMeshProUGUI stoneResourceText;
    public TextMeshProUGUI woodResourceText;
    public GridLayoutGroup buildingMenu;
    public InputManager inputManager;
    public TeamManager teamManager;

    private void OnEnable()
    {
        EventManager.StartListening("selectedUnitsChanged", UpdateSelectedUnits);
        EventManager.StartListening("FoodResourceChanged", UpdateFoodResourceValues);
        EventManager.StartListening("StoneResourceChanged", UpdateStoneResourceValues);
        EventManager.StartListening("WoodResourceChanged", UpdateWoodResourceValues);
    }

    private void UpdateSelectedUnits()
    {
        var selectedUnits = inputManager.SelectedUnits;
        foreach (var selectedUnit in selectedUnits)
        {
            // Debug.Log("Unit " + selectedUnit.name);
        }

        selectedUnitsTextNode.SetText(selectedUnits.Length.ToString());
    }

    private void UpdateFoodResourceValues()
    {
        foodResourceText.SetText(teamManager.GetResourceQuantity(ResourceType.Food).ToString());
    }
    private void UpdateStoneResourceValues()
    {
        stoneResourceText.SetText(teamManager.GetResourceQuantity(ResourceType.Stone).ToString());
    }
    private void UpdateWoodResourceValues()
    {
        woodResourceText.SetText(teamManager.GetResourceQuantity(ResourceType.Wood).ToString());
    }

    public void ShowBuildingMenu()
    {
        // foreach (var buildingPrefab in teamManager.availableBuildings)
        // {
        //     
        //     var components = new[] {typeof(Button), typeof(Image)};
        //     var newButton = new GameObject(buildingPrefab.name, components);
        //     newButton.transform.Rotate(Vector3.forward * 90f);
        //     newButton.transform.SetParent(buildingMenu.transform);
        //
        //     var texture = GetBuildingPreview(buildingPrefab);
        //     var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //     
        //     newButton.GetComponent<Button>().onClick.AddListener(delegate {;
        //         CloseBuildingMenu();
        //     });
        //     newButton.GetComponent<Image>().sprite = sprite;
        //
        // }
        buildingMenu.gameObject.SetActive(true);
    }

    private void CloseBuildingMenu()
    {
        buildingMenu.gameObject.SetActive(false);
    }

    public void ClickBuildingMenu(GameObject prefab)
    {
        teamManager.TryPlacePrefab(prefab);
        CloseBuildingMenu();
    }
}
