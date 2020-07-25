using System;
using System.Collections;
using System.Collections.Generic;
using CameraSystem;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[SelectionBase]
public class Unit : MonoBehaviour
{
    private readonly List<Unit> _detectedTargets = new List<Unit>();

    private NavMeshAgent _agent;
    private float _currentAttackRadius;
    private float _currentHitPoints;
    private UnitState _currentState = UnitState.Idle;
    private Unit _enemyTarget;
    private ResourceNode _gatheringTarget;
    private bool _isAttacking;
    private bool _isGathering;
    private bool _isDead;
    private bool _isSelected;
    private float _nextActionTime;

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

    public ResourceNode GatheringTarget => _gatheringTarget;

    private void Start()
    {
        InstantiateHealthBar();
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
            _currentState = UnitState.Idle;

        if (_currentState == UnitState.Idle && _detectedTargets.Count > 0)
        {
            _enemyTarget = _detectedTargets[0];
            _currentState = UnitState.Combat;
        }

        if (_currentState == UnitState.Combat)
        {
            
            if (_enemyTarget)
            {
                if (_enemyTarget._isDead)
                {
                    _detectedTargets.Remove(_enemyTarget);
                    _enemyTarget = null;
                    _isAttacking = false;
                    return;
                }

                var dist = Vector3.Distance(_enemyTarget.transform.position, transform.position);
                if (dist <= attackRadius.GetValue())
                {
                    FaceTransform(_enemyTarget.transform);
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
            }
        }

        if (_currentState == UnitState.Gathering)
        {
            if (!_gatheringTarget)
            {
                _isGathering = false;
                inResourceRange = false;
                _currentState = UnitState.Idle;
            }

            // Physics.SphereCast(transform.position, _gatheringTarget.gatherRadius, out var hit, gameObject.layer);
            else if (inResourceRange)
            {
                if (!_isGathering && _agent.remainingDistance <= _gatheringTarget.gatherRadius)
                {
                    _gatheringTarget.AddGatherer(teamId);
                    _isGathering = true;
                }

                FaceTransform(_gatheringTarget.transform);
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
                    _enemyTarget = null;
                    _isAttacking = false;
                    break;
                case UnitState.Gathering:
                    inResourceRange = false;
                    _isGathering = false;
                    _gatheringTarget.RemoveGatherer(teamId);
                    _gatheringTarget = null;
                    break;
                case UnitState.Idle:
                case UnitState.Moving:
                    _currentState = UnitState.Moving;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _agent.stoppingDistance = 0;
        }

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
            _agent.SetDestination(_enemyTarget.transform.position);
        }

        else if (dist <= 20f)
        {
            if (!(currTime > _nextActionTime)) return;
            _nextActionTime += 0.5f;
            _nextActionTime = Time.time + 0f;
            _agent.SetDestination(_enemyTarget.transform.position);
        }

        else if (dist <= 50f)
        {
            if (!(currTime > _nextActionTime)) return;
            _nextActionTime = Time.time + 1.5f;
            _agent.SetDestination(_enemyTarget.transform.position);
        }
        else
        {
            if (!(currTime > _nextActionTime)) return;
            _nextActionTime = Time.time + 3f;
            _agent.SetDestination(_enemyTarget.transform.position);
        }
    }

    private void FaceTransform(Transform target)
    {
        var direction = (target.position - transform.position).normalized;
        var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }

    public void TakeDamage(float damage, Unit attacker)
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
        if (enemy.teamId == teamId) return;
        if (_currentState == UnitState.Gathering)
        {
            _gatheringTarget.RemoveGatherer(teamId);
            _isGathering = false;
            _gatheringTarget = null;
            inResourceRange = false;
        }

        _enemyTarget = enemy;
        _currentState = UnitState.Combat;
        _agent.stoppingDistance = attackRadius.GetValue();
    }

    public void GatherResource(ResourceNode resource)
    {
        if (_isDead) return;
        if (!canGather) return;
        if (resource.Equals(_gatheringTarget)) return;
        if (_gatheringTarget) _gatheringTarget.RemoveGatherer(teamId);
        _enemyTarget = null;
        _isGathering = false;
        inResourceRange = false;
        
        // Get a Random point around the resource within the gathering radius
        // var vector2 = Random.insideUnitCircle.normalized * resource.gatherRadius;
        // var destination =  resource.transform.position + new Vector3(vector2.x, 0, vector2.y);
        var resourcePosition = resource.transform.position;
        var randomPosition = Random.insideUnitCircle * resource.gatherRadius;
        var destination = resourcePosition + new Vector3(randomPosition.x, 0, randomPosition.y);
        
        NavMesh.SamplePosition(destination, out var hit, 3f, NavMesh.AllAreas);
        _gatheringTarget = resource;
        MoveTo(hit.position, false);
        _currentState = UnitState.Gathering;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;
        if (!other.CompareTag("Unit")) return;
        var targetTransform = other.transform;
        if (!targetTransform.TryGetComponent<Unit>(out var targetUnit)) return;
        if (targetUnit.teamId == teamId) return;
        _detectedTargets.Add(targetUnit);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isDead) return;
        if (!other.CompareTag("Unit")) return;
        var targetTransform = other.transform;
        if (!targetTransform.TryGetComponent<Unit>(out var targetUnit)) return;
        if (targetUnit.teamId == teamId) return;
        if (!_detectedTargets.Contains(targetUnit)) return;
        _detectedTargets.Remove(targetUnit);
        if (targetUnit != _enemyTarget) return;
        _enemyTarget = null;
    }

    private IEnumerator AttackTarget()
    {
        while (_isAttacking && !_isDead && _enemyTarget)
        {
            var value = attackSpeed.GetValue() > 0 ? attackSpeed.GetValue() : 0.5f;
            yield return new WaitForSeconds(value);
            if (!_enemyTarget) continue;
            var dist = Vector3.Distance(_enemyTarget.transform.position, transform.position);
            if (dist <= attackRadius.GetValue())
                _enemyTarget.TakeDamage(attackDamage.GetValue(), this);
        }
    }

    private IEnumerator Die()
    {
        _isDead = true;
        UnitEventManager.TriggerEvent("unitDeath", this);

        var unitTransform = transform;

        var startRotation = unitTransform.rotation;
        var endRotation = Quaternion.Euler(new Vector3(0, 0, 90)) * startRotation;

        var startPosition = unitTransform.position;
        var endPosition = startPosition - new Vector3(0, 3f, 0);

        const int rotateDuration = 1;
        const int sinkDuration = 4;

        for (float t = 0; t < sinkDuration; t += Time.deltaTime)
        {
            if (t < rotateDuration)
                unitTransform.rotation = Quaternion.Lerp(startRotation, endRotation, t / rotateDuration);
            else if (unitTransform.rotation != endRotation)
                unitTransform.rotation = endRotation;

            unitTransform.position = Vector3.Lerp(startPosition, endPosition, t / sinkDuration);
            yield return null;
        }

        unitTransform.position = endPosition;

        Destroy(gameObject);
    }

    public void ToggleSelectionVisual(bool isSelected)
    {
        _isSelected = isSelected;
        if (_unitHealthBar.gameObject.activeInHierarchy != isSelected)
            _unitHealthBar.gameObject.SetActive(isSelected);
    }
}