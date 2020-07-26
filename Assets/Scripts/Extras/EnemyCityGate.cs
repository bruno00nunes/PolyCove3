using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnitScripts;
using UnityEngine;
using UnityEngine.AI;

namespace Extras
{
    public class EnemyCityGate : MonoBehaviour
    {
        public List<Unit> killsRequiredToOpenGate = new List<Unit>();
        private static readonly int GateOpen = Animator.StringToHash("GateOpen");
        private bool _scanning;
        public NavMeshObstacle[] gateObstacles = new NavMeshObstacle[3];

        private void Start()
        {
            UnitEventManager.StartListening("unitDeath", CheckGate);
        }

        private void CheckGate(Unit deadUnit)
        {
            if (killsRequiredToOpenGate.Contains(deadUnit))
            {
                killsRequiredToOpenGate.Remove(deadUnit);
            }

            if (killsRequiredToOpenGate.Count > 0) return;
            GetComponent<Animator>().SetTrigger(GateOpen);
            foreach (var navMeshObstacle in gateObstacles) 
                navMeshObstacle.enabled = false;
        }
    }
}
