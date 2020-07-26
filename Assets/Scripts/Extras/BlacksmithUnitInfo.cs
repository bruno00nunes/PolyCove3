using System;
using BuildingScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Extras
{
    public class BlacksmithUnitInfo : MonoBehaviour
    {
        public Blacksmith blacksmith;
        
        public TextMeshProUGUI foodCostText;
        public TextMeshProUGUI stoneCostText;
        public TextMeshProUGUI woodCostText;
        public TextMeshProUGUI attackDamageText;
        public TextMeshProUGUI attackRangeText;
        public TextMeshProUGUI attackSpeedText;
        public TextMeshProUGUI movementSpeedText;
        public TextMeshProUGUI armorText;
        public TextMeshProUGUI hitpointsText;

        public void ShowSoldierStats()
        {
            foodCostText.SetText("20");
            stoneCostText.SetText("40");
            woodCostText.SetText("5");
            attackDamageText.SetText("20");
            attackRangeText.SetText("2");
            attackSpeedText.SetText("3s/att");
            movementSpeedText.SetText("2");
            armorText.SetText("3");
            hitpointsText.SetText("120");
        }

        public void ShowKnightStats()
        {
            foodCostText.SetText("40");
            stoneCostText.SetText("80");
            woodCostText.SetText("20");
            attackDamageText.SetText("20");
            attackRangeText.SetText("2");
            attackSpeedText.SetText("2s/att");
            movementSpeedText.SetText("3");
            armorText.SetText("4");
            hitpointsText.SetText("100");
        }

        public void ShowArcherStats()
        {
            foodCostText.SetText("20");
            stoneCostText.SetText("5");
            woodCostText.SetText("40");
            attackDamageText.SetText("15");
            attackRangeText.SetText("10");
            attackSpeedText.SetText("1.3s/att");
            movementSpeedText.SetText("4");
            armorText.SetText("1");
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

        public void SpawnSoldier()
        {
            blacksmith.SpawnSoldier();
            blacksmith = null;
            transform.parent.gameObject.SetActive(false);
        }

        public void SpawnKnight()
        {
            blacksmith.SpawnKnight();
            blacksmith = null;
            transform.parent.gameObject.SetActive(false);
        }

        public void SpawnArcher()
        {
            blacksmith.SpawnArcher();
            blacksmith = null;
            transform.parent.gameObject.SetActive(false);
        }
    }
}
