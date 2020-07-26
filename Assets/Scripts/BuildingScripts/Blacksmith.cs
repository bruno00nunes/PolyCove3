using ResourceScripts;
using UnityEngine;

namespace BuildingScripts
{
    public class Blacksmith : Building
    {
        public GameObject soldierPrefab;
        public GameObject knightPrefab;
        public GameObject archerPrefab;
        
        public override void ToggleSelectionVisual(bool isVisible)
        {
            base.ToggleSelectionVisual(isVisible);
            if (!hasBeenBuilt) return;
            UiManager.Instance.ShowBlacksmith(this);
        }

        public void SpawnSoldier()
        {
            var teamManager = TeamManager.Instance;
            
            if (!teamManager.CheckResources(ResourceType.Food, 20)) return;
            if (!teamManager.CheckResources(ResourceType.Stone, 40)) return;
            if (!teamManager.CheckResources(ResourceType.Wood, 5)) return;
            if (!(teamManager.CurrentPopulation + 1 <= teamManager.MaxPopulation)) return;
            
            teamManager.RemoveResource(ResourceType.Food, 20);
            teamManager.RemoveResource(ResourceType.Stone, 40);
            teamManager.RemoveResource(ResourceType.Wood, 5);
            teamManager.AddCurrentPopulation(1);
            var transform1 = transform;
            Instantiate(soldierPrefab, transform1.position + (transform1.forward * 3), Quaternion.identity);
        }

        public void SpawnKnight()
        {
            var teamManager = TeamManager.Instance;
            
            if (!teamManager.CheckResources(ResourceType.Food, 40)) return;
            if (!teamManager.CheckResources(ResourceType.Stone, 80)) return;
            if (!teamManager.CheckResources(ResourceType.Wood, 20)) return;
            if (!(teamManager.CurrentPopulation + 2 <= teamManager.MaxPopulation)) return;
            
            teamManager.RemoveResource(ResourceType.Food, 40);
            teamManager.RemoveResource(ResourceType.Stone, 80);
            teamManager.RemoveResource(ResourceType.Wood, 20);
            teamManager.AddCurrentPopulation(2);
            var transform1 = transform;
            Instantiate(knightPrefab, transform1.position + (transform1.forward * 3), Quaternion.identity);
        }

        public void SpawnArcher()
        {
            var teamManager = TeamManager.Instance;
            
            if (!teamManager.CheckResources(ResourceType.Food, 20)) return;
            if (!teamManager.CheckResources(ResourceType.Stone, 5)) return;
            if (!teamManager.CheckResources(ResourceType.Wood, 40)) return;
            if (!(teamManager.CurrentPopulation + 1 <= teamManager.MaxPopulation)) return;
            
            teamManager.RemoveResource(ResourceType.Food, 20);
            teamManager.RemoveResource(ResourceType.Stone, 5);
            teamManager.RemoveResource(ResourceType.Wood, 40);
            teamManager.AddCurrentPopulation(1);
            var transform1 = transform;
            Instantiate(archerPrefab, transform1.position + (transform1.forward * 3), Quaternion.identity);
        }
    }
}
