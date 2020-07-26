using System;
using System.Collections;
using System.Collections.Generic;
using BuildingScripts;
using Extras;
using ResourceScripts;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitScripts
{
    [SelectionBase]
    public class Unit : MonoBehaviour
    {
        private readonly List<Unit> _detectedEnemyUnits = new List<Unit>();
        private readonly List<Building> _detectecEnemyBuildings = new List<Building>();

        private Animator _animator;

        private NavMeshAgent _agent;
        private float _currentAttackRadius;
        private float _currentHitPoints;
        private UnitState _currentState = UnitState.Idle;
        private Unit _enemyUnit;
        private Building _enemyBuilding;
        private bool _isAttacking;
        private bool _isGathering;
        private bool _isDead;
        private bool _isSelected;
        private float _nextActionTime;
        private bool _isBuilding;

        public UnitFormation lastUnitFormation;
        public GameObject unitHealthBarPrefab;
        public Vector3 healthBarOffset;
        private UnitHealthBar _unitHealthBar;

        [Header("Unit Stats")]
        public int teamId;
        public int maxHitPoints = 100;
        public Stat moveSpeed;
        public Stat armor;
        public Stat attackRadius;
        public Stat attackDamage;
        public Stat attackSpeed;
        public bool canGather;
        public bool inResourceRange;
        public bool inBuildingRange;
        public int populationCost;
        private static readonly int Dead = Animator.StringToHash("Dead");

        public ResourceNode GatheringTarget { get; private set; }

        public Building BuildingTarget { get; private set; }

        private void Start()
        {
            InstantiateHealthBar();
            _animator = GetComponent<Animator>();
            _currentHitPoints = maxHitPoints;
            _agent = GetComponent<NavMeshAgent>();
            UpdateAttackRange();
            UnitEventManager.TriggerEvent("unitSpawn", this);
        }

        private void InstantiateHealthBar()
        {
            var healthBarGameObj = Instantiate(unitHealthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);
            healthBarGameObj.transform.SetParent(transform);
            var billboard = healthBarGameObj.GetComponent<Billboard>();

            var teamCamera = Camera.main;
            if (teamCamera == null) throw new Exception("No cameras available");

            billboard.Cam = teamCamera.transform;
            _unitHealthBar = healthBarGameObj.GetComponentInChildren<UnitHealthBar>();
            _unitHealthBar.SetMaxHealth(maxHitPoints);
        }

        private void Update()
        {
            if (_isDead) return;
        
            if (Math.Abs(_currentAttackRadius - attackRadius.GetValue()) > 0)
            {
                _currentAttackRadius = attackRadius.GetValue();
                UpdateAttackRange();
            }

            if (_currentState == UnitState.Moving && _agent.remainingDistance <= 0)
            {
                _currentState = UnitState.Idle;
                _animator.SetInteger("unitState", 0);
            }

            if (_currentState == UnitState.Moving || _currentState == UnitState.Idle)
            {
                if (_detectedEnemyUnits.Count > 0)
                {
                    _enemyUnit = _detectedEnemyUnits[0];
                    _currentState = UnitState.Combat;
                }
                else if (_detectecEnemyBuildings.Count > 0)
                {
                    _enemyBuilding = _detectecEnemyBuildings[0];
                    _currentState = UnitState.Combat;
                }
            }

            if (_currentState == UnitState.Combat)
            {
                if (_agent.remainingDistance >= attackRadius.GetValue())
                {
                    _animator.SetInteger("unitState", 1);
                }
                if (_enemyUnit)
                {
                    if (_enemyUnit._isDead)
                    {
                        _detectedEnemyUnits.Remove(_enemyUnit);
                        _enemyUnit = null;
                        _isAttacking = false;
                        return;
                    }

                    var dist = Vector3.Distance(_enemyUnit.transform.position, transform.position);
                    if (dist <= attackRadius.GetValue())
                    {
                        FaceTransform(_enemyUnit.transform);
                        if (!_isAttacking)
                        {
                            _isAttacking = true;
                            StartCoroutine(AttackTarget());
                        }
                    }
                    else
                    {
                        GoToTarget(dist);
                    }
                }
                else if (_enemyBuilding)
                {
                    if (_enemyBuilding.IsDead)
                    {
                        _detectecEnemyBuildings.Remove(_enemyBuilding);
                        _enemyBuilding = null;
                        _isAttacking = false;
                        return;
                    }

                    var dist = Vector3.Distance(_enemyBuilding.transform.position, transform.position);
                    if (dist <= attackRadius.GetValue())
                    {
                        FaceTransform(_enemyBuilding.transform);
                        if (!_isAttacking)
                        {
                            _isAttacking = true;
                            StartCoroutine(AttackTarget());
                        }
                    }
                    else
                    {
                        GoToTarget(dist);
                    }
                }
                else
                {
                    _currentState = UnitState.Idle;
                    _animator.SetInteger("unitState", 0);
                }
            }

            if (_currentState == UnitState.Gathering)
            {
                if (!GatheringTarget)
                {
                    _isGathering = false;
                    inResourceRange = false;
                    _currentState = UnitState.Idle;
                    _animator.SetInteger("unitState", 0);
                }

                // Physics.SphereCast(transform.position, _gatheringTarget.gatherRadius, out var hit, gameObject.layer);
                else if (inResourceRange)
                {
                    if (!_isGathering && _agent.remainingDistance <= GatheringTarget.gatherRadius)
                    {
                        _animator.SetInteger("unitState", 2);
                        GatheringTarget.AddGatherer(teamId);
                        _isGathering = true;
                    }

                    FaceTransform(GatheringTarget.transform);
                }
            }

            if (_currentState == UnitState.Building)
            {
                if (!BuildingTarget)
                {
                    _isBuilding = false;
                    inBuildingRange = false;
                    _currentState = UnitState.Idle;
                    _animator.SetInteger("unitState", 0);
                }

                else if (inBuildingRange)
                {
                    if (!_isBuilding && _agent.remainingDistance <= BuildingTarget.buildingRadius)
                    {
                        _animator.SetInteger("unitState", 2);
                        BuildingTarget.AddBuilder();
                        _isBuilding = true;
                    }

                    FaceTransform(BuildingTarget.transform);
                }
            }
        
            if (_currentHitPoints < maxHitPoints)
            {
                if (!_unitHealthBar.gameObject.activeInHierarchy)
                {
                    _unitHealthBar.gameObject.SetActive(true);
                }
            }
            else if (_currentState == UnitState.Idle && !_isSelected)
                _unitHealthBar.gameObject.SetActive(false);

        }

        private void UpdateAttackRange()
        {
            var radius = attackRadius.GetValue();
            radius = Mathf.Clamp(radius, 1, int.MaxValue);
            GetComponentInChildren<SphereCollider>().radius = radius;
        }

        public void MoveTo(Vector3 destination, bool forceChangeState = true)
        {
            if (_isDead) return;

            if (forceChangeState)
            {
                switch (_currentState)
                {
                    case UnitState.Combat:
                        _enemyUnit = null;
                        _isAttacking = false;
                        break;
                    case UnitState.Gathering:
                        inResourceRange = false;
                        _isGathering = false;
                        GatheringTarget.RemoveGatherer(teamId);
                        GatheringTarget = null;
                        break;
                    case UnitState.Idle:
                    case UnitState.Moving:
                        _currentState = UnitState.Moving;
                        _animator.SetInteger("unitState", 1);
                        break;
                    case UnitState.Building:
                        inBuildingRange = false;
                        _isBuilding = false;
                        BuildingTarget.RemoveBuilder();
                        BuildingTarget = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _agent.stoppingDistance = 0;
            }

            _animator.SetInteger("unitState", 1);
            _agent.speed = moveSpeed.GetValue();
            _agent.SetDestination(destination);
        }

        private void GoToTarget(float dist)
        {
            var currTime = Time.time;
            if (dist <= 10f)
            {
                if (!(currTime > _nextActionTime)) return;
                _nextActionTime = Time.time + 0f;
                if (_enemyBuilding)
                    _agent.SetDestination(_enemyBuilding.transform.position);
                else if (_enemyUnit)
                    _agent.SetDestination(_enemyUnit.transform.position);
            }

            else if (dist <= 20f)
            {
                if (!(currTime > _nextActionTime)) return;
                _nextActionTime += 0.5f;
                _nextActionTime = Time.time + 0f;
                if (_enemyBuilding)
                    _agent.SetDestination(_enemyBuilding.transform.position);
                else if (_enemyUnit)
                    _agent.SetDestination(_enemyUnit.transform.position);
            }

            else if (dist <= 50f)
            {
                if (!(currTime > _nextActionTime)) return;
                _nextActionTime = Time.time + 1.5f;
                if (_enemyBuilding)
                    _agent.SetDestination(_enemyBuilding.transform.position);
                else if (_enemyUnit)
                    _agent.SetDestination(_enemyUnit.transform.position);
            }
            else
            {
                if (!(currTime > _nextActionTime)) return;
                _nextActionTime = Time.time + 3f;
                if (_enemyBuilding)
                    _agent.SetDestination(_enemyBuilding.transform.position);
                else if (_enemyUnit)
                    _agent.SetDestination(_enemyUnit.transform.position);
            }
        }

        private void FaceTransform(Transform target)
        {
            var direction = (target.position - transform.position).normalized;
            var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
        }

        private void TakeDamage(float damage, Unit attacker)
        {
            if (_isDead) return;

            if (_currentState != UnitState.Combat)
                Attack(attacker);

            damage -= armor.GetValue();
            damage = Mathf.Clamp(damage, 0, int.MaxValue);

            _currentHitPoints -= damage;
            _unitHealthBar.SetHealth(_currentHitPoints);

            if (_currentHitPoints <= 0)
                StartCoroutine(Die());
        }

        public void Attack(Unit enemy)
        {
            if (_isAttacking) return;
            if (enemy.teamId == teamId) return;
            switch (_currentState)
            {
                case UnitState.Gathering:
                    GatheringTarget.RemoveGatherer(teamId);
                    _isGathering = false;
                    GatheringTarget = null;
                    inResourceRange = false;
                    break;
                case UnitState.Building:
                    BuildingTarget.RemoveBuilder();
                    _isBuilding = false;
                    BuildingTarget = null;
                    inBuildingRange = false;
                    break;
                case UnitState.Idle:
                    break;
                case UnitState.Moving:
                    break;
                case UnitState.Combat:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _enemyBuilding = null;
            _enemyUnit = enemy;
            _currentState = UnitState.Combat;
            _agent.stoppingDistance = attackRadius.GetValue();
        }

        public void Attack(Building enemy)
        {
            if (_isAttacking) return;
            if (enemy.teamId == teamId) return;
            switch (_currentState)
            {
                case UnitState.Gathering:
                    GatheringTarget.RemoveGatherer(teamId);
                    _isGathering = false;
                    GatheringTarget = null;
                    inResourceRange = false;
                    break;
                case UnitState.Building:
                    BuildingTarget.RemoveBuilder();
                    _isBuilding = false;
                    BuildingTarget = null;
                    inBuildingRange = false;
                    break;
                case UnitState.Idle:
                    break;
                case UnitState.Moving:
                    break;
                case UnitState.Combat:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _enemyUnit = null;
            _enemyBuilding = enemy;
            _currentState = UnitState.Combat;
            _agent.stoppingDistance = attackRadius.GetValue();
        }

        public void RepairBuilding(Building building)
        {
            if (_isDead) return;
            if (!canGather) return;
            if (building.Equals(BuildingTarget)) return;
            if (BuildingTarget) BuildingTarget.AddBuilder();
            _enemyUnit = null;
            _isBuilding = false;
            inBuildingRange = false;
        
            // Get a Random point around the resource within the gathering radius
            // var vector2 = Random.insideUnitCircle.normalized * resource.gatherRadius;
            // var destination =  resource.transform.position + new Vector3(vector2.x, 0, vector2.y);
            var transform1 = building.transform;
            var resourcePosition = transform1.position;
            var randomPosition = Random.insideUnitCircle * (building.buildingRadius * transform1.localScale);
            var destination = resourcePosition + new Vector3(randomPosition.x, 0, randomPosition.y);
        
            NavMesh.SamplePosition(destination, out var hit, 3f, NavMesh.AllAreas);
            BuildingTarget = building;
            MoveTo(hit.position, false);
            _currentState = UnitState.Building;
        }

        public void GatherResource(ResourceNode resource)
        {
            if (_isDead) return;
            if (!canGather) return;
            if (resource.Equals(GatheringTarget)) return;
            if (GatheringTarget) GatheringTarget.RemoveGatherer(teamId);
            _enemyUnit = null;
            _isGathering = false;
            inResourceRange = false;
        
            // Get a Random point around the resource within the gathering radius
            // var vector2 = Random.insideUnitCircle.normalized * resource.gatherRadius;
            // var destination =  resource.transform.position + new Vector3(vector2.x, 0, vector2.y);
            var resourcePosition = resource.transform.position;
            var randomPosition = Random.insideUnitCircle * resource.gatherRadius;
            var destination = resourcePosition + new Vector3(randomPosition.x, 0, randomPosition.y);
        
            NavMesh.SamplePosition(destination, out var hit, 3f, NavMesh.AllAreas);
            GatheringTarget = resource;
            MoveTo(hit.position, false);
            _currentState = UnitState.Gathering;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isDead) return;
            if (!other.CompareTag("Unit")) return;
            var targetTransform = other.transform;
            
            if (other.CompareTag("Unit"))
            {
                if (!targetTransform.TryGetComponent<Unit>(out var targetUnit)) return;
                if (targetUnit.teamId == teamId) return;
                _detectedEnemyUnits.Add(targetUnit);
            }
            else if (other.CompareTag("Building"))
            {
                if (!targetTransform.TryGetComponent<Building>(out var building)) return;
                if (building.teamId == teamId) return;
                _detectecEnemyBuildings.Add(building);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_isDead) return;
            if (!other.CompareTag("Unit") || !other.CompareTag("Building")) return;
            var targetTransform = other.transform;
            if (other.CompareTag("Unit"))
            {
                if (!targetTransform.TryGetComponent<Unit>(out var targetUnit)) return;
                if (targetUnit.teamId == teamId) return;
                if (!_detectedEnemyUnits.Contains(targetUnit)) return;
                _detectedEnemyUnits.Remove(targetUnit);
                if (targetUnit != _enemyUnit) return;
                _enemyUnit = null;
            }
            else if (other.CompareTag("Building"))
            {
                if (!targetTransform.TryGetComponent<Building>(out var targetBuilding)) return;
                if (targetBuilding.teamId == teamId) return;
                if (!_detectecEnemyBuildings.Contains(targetBuilding)) return;
                _detectecEnemyBuildings.Remove(targetBuilding);
                if (targetBuilding != _enemyBuilding) return;
                _enemyBuilding = null;
            }
        }

        private IEnumerator AttackTarget()
        {
            while (_isAttacking && !_isDead && (_enemyUnit || _enemyBuilding))
            {
                var value = attackSpeed.GetValue() > 0 ? attackSpeed.GetValue() : 0.5f;
                yield return new WaitForSeconds(value);
                if (_enemyUnit)
                {
                    var dist = Vector3.Distance(_enemyUnit.transform.position, transform.position);
                    if (!(dist <= attackRadius.GetValue())) continue;
                    _animator.SetInteger("unitState", 2);
                    _enemyUnit.TakeDamage(attackDamage.GetValue(), this);
                }
                else if (_enemyBuilding)
                {
                    var dist = Vector3.Distance(_enemyBuilding.transform.position, transform.position);
                    if (!(dist <= attackRadius.GetValue())) continue;
                    _animator.SetInteger("unitState", 2);
                    _enemyBuilding.TakeDamage(attackDamage.GetValue());
                }
            }
        }

        private IEnumerator Die()
        {
            _isDead = true;
            UnitEventManager.TriggerEvent("unitDeath", this);
            _animator.SetTrigger(Dead);

            var unitTransform = transform;

            // var startRotation = unitTransform.rotation;
            // var endRotation = Quaternion.Euler(new Vector3(0, 0, 90)) * startRotation;

            var startPosition = unitTransform.position;
            var endPosition = startPosition - new Vector3(0, 3f, 0);

            // const int rotateDuration = 1;
            // const int sinkDuration = 4;

            // for (float t = 0; t < sinkDuration; t += Time.deltaTime)
            // {
            //     // if (t < rotateDuration)
            //     //     unitTransform.rotation = Quaternion.Lerp(startRotation, endRotation, t / rotateDuration);
            //     // else if (unitTransform.rotation != endRotation)
            //     //     unitTransform.rotation = endRotation;
            //
            //     unitTransform.position = Vector3.Lerp(startPosition, endPosition, t / sinkDuration);
            //     yield return null;
            // }

            // unitTransform.position = endPosition;
            
            yield return new WaitForSeconds(1.7f);

            Destroy(gameObject);
        }

        public void ToggleSelectionVisual(bool isSelected)
        {
            _isSelected = isSelected;
            if (_unitHealthBar.gameObject.activeInHierarchy != isSelected)
                _unitHealthBar.gameObject.SetActive(isSelected);
        }
    }
}