using System.Collections;
using ResourceScripts;
using UnityEngine;

namespace BuildingScripts
{
    public class Farm : Building
    {
        private bool _isFarming;

        public override void ToggleSelectionVisual(bool isVisible)
        {
            base.ToggleSelectionVisual(isVisible);
            if (!hasBeenBuilt) return;
            UiManager.Instance.ShowFarm();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (currentHitPoints >= maxHitPoints && hasBeenBuilt && !_isFarming)
            {
                StartCoroutine(FarmFoodTick());
            }
        }

        private IEnumerator FarmFoodTick()
        {
            _isFarming = true;

            while (currentHitPoints >= maxHitPoints)
            {
                yield return new WaitForSeconds(3);
                FarmGatherFood();
            }

            _isFarming = false;
        }

        private static void FarmGatherFood()
        {
            TeamManager.Instance.AddResource(ResourceType.Food, 5);
        }
    }
}