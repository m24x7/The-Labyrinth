using System.Collections.Generic;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    public class EnemyActions : MonoBehaviour
    {
        EnemyAgent agent;

        public bool pathFollowingInProgress = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            agent = GetComponent<EnemyAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            //if (pathFollowingInProgress)
            {
                PathFollowing(agent.Path, agent.DestinationTarget, agent.Movement, out Vector3 newTarget);
                agent.FloatingTarget = newTarget;
            }
        }

        public void Idle()
        {
            pathFollowingInProgress = false;
        }

        public void ChasePlayer()
        {
            if (agent.Perception.visibleTargets.Count != 0) agent.SetDestinationTarget(agent.Perception.visibleTargets[0].root.position);

            pathFollowingInProgress = true;
        }

        public void InvestigateNoise()
        {
            agent.SetDestinationTarget(agent.Perception.heardNoisePos);

            pathFollowingInProgress = true;
        }

        public void PathFollowing(List<PathNode> _path, Vector3 newTarget, EnemyMovement Movement, out Vector3 _floatingTarget)
        {
            // If we have a path, follow it
            if (_path != null && _path.Count > 0)
            {
                if (Vector3.Distance(transform.position, newTarget) <= Movement._LeavingPathDistance)
                {
                    _path = null;
                    _floatingTarget = newTarget;
                }
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
