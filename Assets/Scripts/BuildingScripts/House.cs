using System.Collections;
using ResourceScripts;
using UnityEngine;

namespace BuildingScripts
{
    public class House : Building
    {
        public GameObject peasantPrefab;
    
        public override void ToggleSelectionVisual(bool isVisible)
        {
            base.ToggleSelectionVisual(isVisible);
            if (!hasBeenBuilt) return;
            UiManager.Instance.ShowHouse(this);
        }

        public void SpawnPeasant()
        {
            var teamManager = TeamManager.Instance;
        
            if (!teamManager.CheckResources(ResourceType.Food, 10)) return;
            if (!teamManager.CheckResources(ResourceType.Stone, 10)) return;
            if (!teamManager.CheckResources(ResourceType.Wood, 10)) return;
            if (!(teamManager.CurrentPopulation + 1 <= teamManager.MaxPopulation)) return;
            
            teamManager.RemoveResource(ResourceType.Food, 10);
            teamManager.RemoveResource(ResourceType.Stone, 10);
            teamManager.RemoveResource(ResourceType.Wood, 10);
            teamManager.AddCurrentPopulation(1);
            var transform1 = transform;
            Instantiate(peasantPrefab, transform1.position + (transform1.forward * 3), Quaternion.identity);
        }
    
        protected override void SetBuilt()
        {
            TeamManager.Instance.AddMaxPopulation(3);
            base.SetBuilt();
        }

        protected override IEnumerator Die()
        {
            TeamManager.Instance.RemoveMaxPopulation(3);

            return base.Die();
        }
    }
}
