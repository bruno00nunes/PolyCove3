using BuildingScripts;
using Extras;
using ResourceScripts;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    private static UiManager _instance;
 
    public static UiManager Instance
    {
        get
        {
            if (_instance) return _instance;
            _instance = FindObjectOfType(typeof(UiManager)) as UiManager;

            if (_instance != null) return _instance;
            Debug.LogError("There needs to be one active UiManager script on a GameObject in your scene.");
            return null;
        }
    }
    
    public TextMeshProUGUI selectedUnitsTextNode;
    public TextMeshProUGUI foodResourceText;
    public TextMeshProUGUI stoneResourceText;
    public TextMeshProUGUI woodResourceText;
    public TextMeshProUGUI populationResourceText;
    public GameObject buildingMenu;
    public GameObject blacksmithUi;
    public GameObject houseUi;
    public GameObject sawmillUi;
    public GameObject farmUi;
    public GameObject helpUi;

    private void OnEnable()
    {
        EventManager.StartListening("selectedUnitsChanged", UpdateSelectedUnits);
        EventManager.StartListening("FoodResourceChanged", UpdateFoodResourceValues);
        EventManager.StartListening("StoneResourceChanged", UpdateStoneResourceValues);
        EventManager.StartListening("WoodResourceChanged", UpdateWoodResourceValues);
        EventManager.StartListening("PopulationResourceChanged", UpdatePopulationResourceValues);
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.H)) ToggleHelp();
        if (!Input.GetKey(KeyCode.Escape)) return;
        HideUiMenus();
    }

    private void HideUiMenus()
    {
        HideBlacksmith();
        HideFarm();
        HideHouse();
        HideSawmill();
        HideBuildingMenu();
    }

    private void ToggleHelp()
    {
        var setActive = !helpUi.activeInHierarchy;
        helpUi.SetActive(setActive);
        InputManager.Instance.FreezeControls(setActive);
    }

    private void UpdateSelectedUnits()
    {
        var selectedUnits = InputManager.Instance.SelectedUnits;

        selectedUnitsTextNode.SetText(selectedUnits.Length.ToString());
    }

    private void UpdateFoodResourceValues()
    {
        foodResourceText.SetText(TeamManager.Instance.GetResourceQuantity(ResourceType.Food).ToString());
    }
    private void UpdateStoneResourceValues()
    {
        stoneResourceText.SetText(TeamManager.Instance.GetResourceQuantity(ResourceType.Stone).ToString());
    }
    private void UpdateWoodResourceValues()
    {
        woodResourceText.SetText(TeamManager.Instance.GetResourceQuantity(ResourceType.Wood).ToString());
    }

    private void UpdatePopulationResourceValues()
    {
        populationResourceText.SetText(TeamManager.Instance.CurrentPopulation +
                                 "/" +
                                 TeamManager.Instance.MaxPopulation);
    }

    public void ShowBuildingMenu()
    {
        buildingMenu.SetActive(true);
    }

    public void HideBuildingMenu()
    {
        buildingMenu.SetActive(false);
    }

    public void ClickBuildingMenu(GameObject prefab)
    {
        HideBuildingMenu();
        TeamManager.Instance.TryPlacePrefab(prefab);
    }

    public void ShowBlacksmith(Blacksmith building)
    {
        blacksmithUi.SetActive(true);
        blacksmithUi.GetComponentInChildren<BlacksmithUnitInfo>().blacksmith = building;
    }

    public void HideBlacksmith()
    {
        blacksmithUi.GetComponentInChildren<BlacksmithUnitInfo>().blacksmith = null;
        blacksmithUi.SetActive(false);
    }

    public void ShowHouse(House building)
    {
        houseUi.SetActive(true);
        houseUi.GetComponentInChildren<HouseUnitInfo>().house = building;
    }

    public void HideHouse()
    {
        houseUi.GetComponentInChildren<HouseUnitInfo>().house = null;
        houseUi.SetActive(false);
    }

    public void ShowFarm()
    {
        farmUi.SetActive(true);
    }
    
    public void HideFarm()
    {
        farmUi.SetActive(false);
    }

    public void ShowSawmill()
    {
        sawmillUi.SetActive(true);
    }
    
    public void HideSawmill()
    {
        sawmillUi.SetActive(false);
    }
}
