using System;
using System.Collections;
using System.Collections.Generic;
using Extras;
using ResourceScripts;
using UnitScripts;
using UnityEngine;

namespace BuildingScripts
{
    public class Building : MonoBehaviour
    {
    
        [Serializable]
        public struct BuildingCost {
            public ResourceType resourceType;
            public int cost;
    
            public BuildingCost(ResourceType resourceType, int cost)
            {
                this.resourceType = resourceType;
                this.cost = cost;
            }
    
            public bool Equals(BuildingCost other)
            {
                return resourceType == other.resourceType && cost == other.cost;
            }
    
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int) resourceType * 397) ^ cost;
                }
            }
        }

        public int teamId;
        public int maxHitPoints = 100;
        public Mesh[] buildingMeshes = new Mesh[3];
        public GameObject healthBarPrefab;
        public Vector3 healthBarOffset;
        public UnitHealthBar healthBar;
        public float currentHitPoints;
        public bool isSelected;
        public int populationCost;
        private int _totalBuilders;
        protected bool hasBeenBuilt;
        public List<BuildingCost> buildingCosts;
        private readonly HashSet<GameObject> _collisions = new HashSet<GameObject>();
        public int Collisions => _collisions.Count;
        public float buildingRadius;

        private MeshFilter _currentMeshFilter;
        public bool isRepairing;
        public bool startMaxHealth;
        public bool preview;
        public bool IsDead { get; private set; }

        protected void Start()
        {
            buildingRadius = GetComponent<SphereCollider>().radius;
            _currentMeshFilter = TryGetComponent<MeshFilter>(out var component) ? component : GetComponentInChildren<MeshFilter>();

            // _currentMeshFilter.mesh = buildingMeshes[0];
            InstantiateHealthBar();
            if (startMaxHealth) currentHitPoints = maxHitPoints;
        }
    
        private void Update()
        {
            if (preview)
            {
                healthBar.gameObject.SetActive(false);
                return;
            }

            healthBar.SetHealth(currentHitPoints);
            if (currentHitPoints < maxHitPoints)
            {
                if (!healthBar.gameObject.activeInHierarchy)
                {
                    healthBar.gameObject.SetActive(true);
                }
            }

            if (!hasBeenBuilt && currentHitPoints >= maxHitPoints)
                SetBuilt();
            else if (currentHitPoints >= maxHitPoints / 8 * 7)
            {
                SetBuildingStage(2);
            }
            else if (currentHitPoints >= maxHitPoints / 8 * 5)
            {
                SetBuildingStage(1);
            }
            else if (currentHitPoints <= maxHitPoints / 8 * 5)
            {
                SetBuildingStage(0);
            }
        
        
            if (!isSelected)
                healthBar.gameObject.SetActive(false);
        }

        protected virtual void SetBuilt()
        {
            hasBeenBuilt = true;
            TeamManager.Instance.AddCurrentPopulation(populationCost);
        }

        protected virtual void LateUpdate()
        {
            if (preview) return;
            if (!isRepairing && _totalBuilders > 0)
            {
                StartCoroutine(RepairTick());
            }
        }

        public void RemoveBuilder()
        {
            if (preview) return;
            _totalBuilders--;
            _totalBuilders = Mathf.Clamp(_totalBuilders, 0, int.MaxValue);
        }
    
        public void AddBuilder()
        {
            if (preview) return;
            _totalBuilders++;
        }

        private IEnumerator RepairTick()
        {
            isRepairing = true;
            while (_totalBuilders > 0)
            {
                yield return new WaitForSeconds(1);
                RepairBuilding();
            }

            isRepairing = false;
        }

        private void RepairBuilding()
        {
            if (preview) return;
            currentHitPoints += _totalBuilders * 5;

            if (currentHitPoints >= maxHitPoints) currentHitPoints = maxHitPoints;
        }

        private void InstantiateHealthBar()
        {
            var healthBarGameObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarGameObj.transform.SetParent(transform);
            healthBarGameObj.transform.localPosition = new Vector3() + healthBarOffset;
            var billboard = healthBarGameObj.GetComponent<Billboard>();

            var teamCamera = Camera.main;
            if (teamCamera == null) throw new Exception("No cameras available");

            billboard.Cam = teamCamera.transform;
            healthBar = healthBarGameObj.GetComponentInChildren<UnitHealthBar>();
            healthBar.SetMaxHealth(maxHitPoints);
        }

        protected virtual IEnumerator Die()
        {
            IsDead = true;
            var unitTransform = transform;

            var startPosition = unitTransform.position;
            var endPosition = startPosition - new Vector3(0, 3f, 0);

            const int sinkDuration = 4;

            for (float t = 0; t < sinkDuration; t += Time.deltaTime)
            {
                unitTransform.position = Vector3.Lerp(startPosition, endPosition, t / sinkDuration);
                yield return null;
            }

            unitTransform.position = endPosition;

            Destroy(gameObject);
        }

        public virtual void ToggleSelectionVisual(bool isVisible)
        {
            if (preview) return;
            isSelected = isVisible;
            if (healthBar.gameObject.activeInHierarchy != isVisible)
                healthBar.gameObject.SetActive(isVisible);
        }

        public void SetBuildingStage(int stage)
        {
            if (preview) return;
            if (stage >= buildingMeshes.Length) return;
            // if (stage == buildingMeshes.Length - 1) _hasBeenBuilt = true;
            _currentMeshFilter.mesh = buildingMeshes[stage];
        }

        public void TakeDamage(float damage)
        {
            if (preview) return;
            if (IsDead) return;

            currentHitPoints -= damage;
            healthBar.SetHealth(currentHitPoints);

            if (currentHitPoints <= 0)
                StartCoroutine(Die());
        }

        protected void OnCollisionEnter(Collision other)
        {
            var otherGameObject = other.gameObject;
            // if (otherGameObject.CompareTag("Building") || otherGameObject.CompareTag("Resource"))
            _collisions.Add(otherGameObject);
        }

        protected void OnCollisionExit(Collision other)
        {
            _collisions.Remove(other.gameObject);
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (preview) return;
            if (!other.CompareTag("Unit")) return;
            if (!other.TryGetComponent<Unit>(out var unit)) return;
            if (unit.BuildingTarget != this) return;
            unit.inBuildingRange = true;
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (preview) return;
            if (!other.CompareTag("Unit")) return;
            if (!other.TryGetComponent<Unit>(out var unit)) return;
            unit.inBuildingRange = false;
        }
    }
}
