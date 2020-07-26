using System.Collections;
using ResourceScripts;
using UnityEngine;

namespace BuildingScripts
{
    public class Sawmill : Building
    {
        private bool _isChopping;

        public override void ToggleSelectionVisual(bool isVisible)
        {
            base.ToggleSelectionVisual(isVisible);
            if (!hasBeenBuilt) return;
            UiManager.Instance.ShowSawmill();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (currentHitPoints >= maxHitPoints && hasBeenBuilt && !_isChopping)
            {
                StartCoroutine(ChopWoodTick());
            }
        }

        private IEnumerator ChopWoodTick()
        {
            _isChopping = true;

            while (currentHitPoints >= maxHitPoints)
            {
                yield return new WaitForSeconds(3);
                SawmillGatherWood();
            }

            _isChopping = false;
        }

        private static void SawmillGatherWood()
        {
            TeamManager.Instance.AddResource(ResourceType.Wood, 5);
        }
    }
}