using System.Collections.Generic;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class defines the actions that an enemy can take.
    /// </summary>
    public class EnemyActions : MonoBehaviour
    {
        EnemyAgent agent;

        public bool pathFollowingInProgress = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // Get reference to the EnemyAgent component
            agent = GetComponent<EnemyAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            // If we are following a path, continue to do so
            if (pathFollowingInProgress)
            {
                PathFollowing(agent.Path, agent.DestinationTarget, agent.GetMovement, out Vector3 newTarget);
                agent.FloatingTarget = newTarget;
            }
        }

        /// <summary>
        /// This action makes the enemy idle in place.
        /// </summary>
        public void Idle()
        {
            pathFollowingInProgress = false;
        }

        /// <summary>
        /// This action makes the enemy chase the player.
        /// </summary>
        public void ChasePlayer()
        {
            if (agent.GetPerception.visibleTargets.Count != 0) agent.SetDestinationTarget(agent.GetDecision.chaseTarget.transform.root.position);

            pathFollowingInProgress = true;
        }

        /// <summary>
        /// This action makes the enemy investigate a noise.
        /// </summary>
        public void InvestigateNoise()
        {
            agent.SetDestinationTarget(agent.GetPerception.heardNoisePos);

            pathFollowingInProgress = true;
        }

        /// <summary>
        /// This method performs path following
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="newTarget"></param>
        /// <param name="Movement"></param>
        /// <param name="_floatingTarget"></param>
        public void PathFollowing(List<PathNode> _path, Vector3 newTarget, EnemyMovement Movement, out Vector3 _floatingTarget)
        {
            // If we have a path, follow it
            if (_path != null && _path.Count > 0)
            {
                // If we are close enough to the target, stop following the path
                if (Vector3.Distance(transform.position, newTarget) <= Movement._LeavingPathDistance)
                {
                    _path = null;
                    _floatingTarget = newTarget;
                }
                // Otherwise, continue following the path
                else
                {
                    Movement.PathFollowing(_path, out Vector3 target);
                    _floatingTarget = target;
                }
                return;
            }

            _floatingTarget = new Vector3();
        }
    }
}
