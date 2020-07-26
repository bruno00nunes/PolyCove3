using BuildingScripts;
using TMPro;
using UnityEngine;

namespace Extras
{
    public class HouseUnitInfo : MonoBehaviour
    {
        public House house;
    
        public TextMeshProUGUI foodCostText;
        public TextMeshProUGUI stoneCostText;
        public TextMeshProUGUI woodCostText;
        public TextMeshProUGUI attackDamageText;
        public TextMeshProUGUI attackRangeText;
        public TextMeshProUGUI attackSpeedText;
        public TextMeshProUGUI movementSpeedText;
        public TextMeshProUGUI armorText;
        public TextMeshProUGUI hitpointsText;

        public void ShowPeasantStats()
        {
            foodCostText.SetText("10");
            stoneCostText.SetText("10");
            woodCostText.SetText("10");
            attackDamageText.SetText("1");
            attackRangeText.SetText("2");
            attackSpeedText.SetText("1");
            movementSpeedText.SetText("3");
            armorText.SetText("0");
            hitpointsText.SetText("100");
        }

        public void HideStats()
        {
            foodCostText.SetText("0");
            stoneCostText.SetText("0");
            woodCostText.SetText("0");
            attackDamageText.SetText("0");
            attackRangeText.SetText("0");
            attackSpeedText.SetText("0");
            movementSpeedText.SetText("0");
            armorText.SetText("0");
            hitpointsText.SetText("0");
        }

        public void SpawnPeasant()
        {
            house.SpawnPeasant();
            house = null;
            transform.parent.gameObject.SetActive(false);
        }
    }
}
