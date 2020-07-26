using TMPro;
using UnityEngine;

namespace Extras
{
    public class BuildingMenuInfo : MonoBehaviour
    {
        public TextMeshProUGUI foodCostText;
        public TextMeshProUGUI stoneCostText;
        public TextMeshProUGUI woodCostText;
        public TextMeshProUGUI populationCostText;

        public void ShowBlacksmithCost()
        {
            foodCostText.SetText("150");
            stoneCostText.SetText("250");
            woodCostText.SetText("100");
            populationCostText.SetText("2");
        }

        public void ShowFarmCost()
        {
            foodCostText.SetText("200");
            stoneCostText.SetText("100");
            woodCostText.SetText("100");
            populationCostText.SetText("3");
        }

        public void ShowSawmillCost()
        {
            foodCostText.SetText("100");
            stoneCostText.SetText("100");
            woodCostText.SetText("200");
            populationCostText.SetText("2");
        }

        public void ShowHouseCost()
        {
            foodCostText.SetText("50");
            stoneCostText.SetText("50");
            woodCostText.SetText("30");
            populationCostText.SetText("0");
        }

        public void HideBuildingCost()
        {
            foodCostText.SetText("0");
            stoneCostText.SetText("0");
            woodCostText.SetText("0");
            populationCostText.SetText("0");
        }
    }
}
