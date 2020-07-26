using System.Collections;
using System.Collections.Generic;
using UnitScripts;
using UnityEngine;
using UnityEngine.AI;

namespace ResourceScripts
{
    [SelectionBase]
    public class ResourceNode : MonoBehaviour
    {
        public ResourceType resourceType;

        public float harvestTime;
        private int _availableQuantity;
        private int _totalGatherers;
        public float gatherRadius;
        private readonly Dictionary<int, int> _gatheringUnits = new Dictionary<int, int>();

        [SerializeField] private bool isTicking;

        private void Start()
        {
            GetComponent<SphereCollider>().radius = gatherRadius;
            
            var transform1 = transform;
            var childTransform = transform1.GetChild(0).transform;
            
            transform1.position = childTransform.position;
            childTransform.position = new Vector3() + transform1.position;

            if (transform.localScale.x < 3)
            {
                var randomSize = Random.Range(0, 1.5f) + 0.5f;
                var transform2 = transform;
                transform2.localScale = new Vector3(randomSize, randomSize, randomSize);
            }

            _availableQuantity = Mathf.CeilToInt(Random.Range(60, 120) * transform.localScale.x);
        }

        private void LateUpdate()
        {
            // GetComponent<NavMeshObstacle>().size = transform.localScale;
            if (_availableQuantity == 0)
            {
                Destroy(gameObject);
            }

            if (!isTicking && _totalGatherers > 0)
            {
                StartCoroutine(ResourceTick());
            }
        }

        private void ResourceGather()
        {
            var quantityGathered = _gatheringUnits.ContainsKey(TeamManager.Instance.teamId)
                ? _gatheringUnits[TeamManager.Instance.teamId]
                : 0;
        
            if (quantityGathered >= _availableQuantity)
                quantityGathered = _availableQuantity;

            _availableQuantity -= quantityGathered;
            TeamManager.Instance.AddResource(resourceType, quantityGathered);
        }

        public void AddGatherer(int teamId)
        {
            if (!_gatheringUnits.ContainsKey(teamId))
            {
                _gatheringUnits[teamId] = 0;
            }
        
            _gatheringUnits[teamId]++;
            _totalGatherers++;
        }

        public void RemoveGatherer(int teamId)
        {
            if (!_gatheringUnits.ContainsKey(teamId)) return;
        
            _gatheringUnits[teamId]--;
            _totalGatherers--;
            _gatheringUnits[teamId] = Mathf.Clamp(_gatheringUnits[teamId], 0, int.MaxValue);
            _totalGatherers = Mathf.Clamp(_totalGatherers, 0, int.MaxValue);
        }

        private IEnumerator ResourceTick()
        {
            isTicking = true;
            while (_totalGatherers > 0 && _availableQuantity > 0)
            {
                yield return new WaitForSeconds(harvestTime);
                ResourceGather();
            }

            isTicking = false;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Unit")) return;
            if (!other.TryGetComponent<Unit>(out var unit)) return;
            if (unit.GatheringTarget != this) return;
            // AddGatherer(unit.teamId);
            unit.inResourceRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Unit")) return;
            if (!other.TryGetComponent<Unit>(out var unit)) return;
            // if (unit.GatheringTarget != this) return;
            // RemoveGatherer(unit.teamId);
            unit.inResourceRange = false;
        }
    }
}
