using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.VersionControl;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This enum represents the different states an enemy can be in.
    /// </summary>
    public enum EnemyState
    {
        Idle,
        Investigate,
        Chase,
    }

    /// <summary>
    /// This class handles the decision-making logic for enemy AI.
    /// </summary>
    public class EnemyDecisionMaking : MonoBehaviour
    {
        // Flags
        private bool canSeePlayer;
        private bool heardNoise;
        private bool visitedHeardNoisePosition;
        private bool isAtTargetPosition;

        // Flag getters and setters
        public bool CanSeePlayer
        {
            get { return canSeePlayer; }
            set { canSeePlayer = value; }
        }

        public bool HeardNoise
        {
            get { return heardNoise; }
            set { heardNoise = value; }
        }

        public bool VisitedHeardNoisePosition
        {
            get { return visitedHeardNoisePosition; }
            set { visitedHeardNoisePosition = value; }
        }

        public bool IsAtTargetPosition
        {
            get { return isAtTargetPosition; }
            set { isAtTargetPosition = value; }
        }

        public EnemyState enemyState = EnemyState.Idle;

        private EnemyAgent enemyAgent;

        private Action idle;
        private Action chase;
        private Action investigate;

        private float chaseTimer = 0f;
        private const float maxChaseTime = 10f;
        public GameObject chaseTarget;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            enemyAgent = GetComponent<EnemyAgent>();

            idle += () => enemyAgent.GetActions.Idle();

            chase += () => enemyAgent.GetActions.ChasePlayer();

            investigate = () => enemyAgent.GetActions.InvestigateNoise();
        }

        // Update is called once per frame
        void Update()
        {
            if (chaseTimer > 0)
            {
                chaseTimer -= Time.deltaTime;
            }
            else chaseTarget = null;
        }

        /// <summary>
        /// This method determines the current state of the enemy based on perception flags.
        /// </summary>
        /// <returns></returns>
        public void DetermineEnemyState(List<Transform> visibleTargets, Vector3 heardNoisePos, Vector3 targetPos)
        {
            CheckVisibleTargets(visibleTargets);
            CheckHeardNoise(enemyAgent.GetPerception.heardNoise);
            CheckAtTargetPosition(targetPos);
            VisitedNoisePosition(heardNoisePos);

            // Priority 1: If the enemy can see the player, switch to Chase state
            if (canSeePlayer || chaseTimer > 0f) 
            {
                if (canSeePlayer) chaseTimer = maxChaseTime;
                if (chaseTarget == null) chaseTarget = visibleTargets[0].gameObject;
                //Debug.Log("Can See Player - Switching to Chase State");
                enemyState = EnemyState.Chase;
                return;
            }

            // Priority 2: If the enemy heard a noise, switch to Investigate state
            if (visitedHeardNoisePosition)
            {
                heardNoise = false;
                enemyAgent.GetPerception.heardNoise = false;
                visitedHeardNoisePosition = false;
            }
            if (heardNoise && Vector3.Distance(heardNoisePos, transform.position) > 1f)
            {
                //Debug.Log("Heard Noise - Switching to Investigate State");
                enemyState = EnemyState.Investigate;
                return;
            }
            
            // Priority 3: If the enemy is at the last known target position, switch to Idle state
            if (isAtTargetPosition)
            {
                //Debug.Log("At Last Known Position - Switching to Idle State");
                enemyState = EnemyState.Idle;
                return;
            }
            
            

            // Default: Switch to Idle state
            //Debug.Log("No Stimuli - Switching to Idle State");
            enemyState = EnemyState.Idle;
        }

        public void DetermineAction()
        {
            switch (enemyState)
            {
                case EnemyState.Idle:
                    // Right Now, do nothing
                    idle.Invoke();
                    break;
                case EnemyState.Investigate:
                    // Pathfind to last known noise position
                    investigate.Invoke();
                    break;
                case EnemyState.Chase:
                    // Chase the player
                    chase.Invoke();
                    break;
            }
        }

        public void CheckVisibleTargets(List<Transform> targets)
        {
            if (targets.Count > 0)
            {
                canSeePlayer = true;
            }
            else
            {
                canSeePlayer = false;
            }
        }

        public void CheckHeardNoise(bool _heardNoise)
        {
            heardNoise = _heardNoise;
        }

        public void VisitedNoisePosition(Vector3 noisePosition, float threshold = 0.5f)
        {
            if (Vector3.Distance(transform.position, noisePosition) <= threshold)
            {
                visitedHeardNoisePosition = true;
            }
        }

        public void CheckAtTargetPosition(Vector3 targetPosition, float threshold = 0.5f)
        {
            if (Vector3.Distance(transform.position, targetPosition) <= threshold)
            {
                isAtTargetPosition = true;
            }
            else
            {
                isAtTargetPosition = false;
            }
        }
    }
}
