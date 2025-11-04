using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    public class EnemyMovement : MonoBehaviour
    {
        //// Start is called once before the first execution of Update after the MonoBehaviour is created
        //void Start()
        //{
        
        //}

        //// Update is called once per frame
        //void Update()
        //{
        
        //}

        //public void SetUp()
        //{
        //    // Setup movement parameters
        //}

        #region Path Following

        // Perform path following
        public void PathFollowing(List<PathNode> _path, out Vector3 _floatingTarget)
        {
            int closestNodeIndex = GetClosestNode(_path);
            int nextNodeIndex = closestNodeIndex + 1;

            PathNode targetNode = null;

            if (nextNodeIndex < _path.Count)
            {
                targetNode = _path[nextNodeIndex];
            }
            else
            {
                targetNode = _path[closestNodeIndex];
            }

            _floatingTarget = targetNode.transform.position;
        }

        // Public method to set the destination target
        public void SetDestinationTarget(List<PathNode> _path, Vector3 destination, float _MinimumPathDistance, LevelManager _levelManager,
            out Vector3 _destinationTarget, out Vector3 _floatingTarget, out List<PathNode> newPath)
        {
            Debug.Log("Destination: " + destination);

            _destinationTarget = destination;
            _floatingTarget = new Vector3();
            newPath = null;

            // If the straight line distance is greater than our minimum, Lets do pathfinding!
            if (Vector3.Distance(transform.position, _destinationTarget) > _MinimumPathDistance)
            {
                PathNode startNode = _levelManager.GetNode(transform.position);
                PathNode endNode = _levelManager.GetNode(destination);

                if (startNode == null || endNode == null)
                {
                    Debug.LogWarning("EnemyAgent: Unable to find start or end node for pathfinding.");
                    return;
                }

                newPath = Pathfinder.FindPath(startNode, endNode);

                StartCoroutine(DrawPathDebugLines(newPath));
            }

            // Otherwise, move directly to the target
            else
            {
                _floatingTarget = _destinationTarget;
            }
        }

        // Get the closest node to the player's current position
        private int GetClosestNode(List<PathNode> _path)
        {
            int closestNodeIndex = 0;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < _path.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, _path[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNodeIndex = i;
                }
            }
            return closestNodeIndex;
        }

        private IEnumerator DrawPathDebugLines(List<PathNode> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i].transform.position, path[i + 1].transform.position, Color.red, 3.5f);
                yield return new WaitForSeconds(0.1f);
            }
        }
        #endregion

        #region Move Agent
        public Vector3 GetNewAgentVelocity(Vector3 _floatingTarget, float _StoppingDistance, Vector3 curVelocity, float _MaxSpeed)
        {
            // If we have a floating target and we are not close enough to it, move towards it
            if (_floatingTarget != Vector3.zero && Vector3.Distance(transform.position, _floatingTarget) > _StoppingDistance)
            {
                // Calculate the direction to the target position
                Vector3 direction = (_floatingTarget - transform.position).normalized;

                // Calculate the movement vector
                return curVelocity = direction * _MaxSpeed;
            }

            // If we are close enough to the floating target, slow down
            else
            {
                return curVelocity *= .95f;
            }
        }

        public void RotateAgent(Vector3 _velocity, float _TurnRate)
        {
            // Calculate the desired rotation towards the movement vector
            if (_velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_velocity);

                // Smoothly rotate towards the target rotation based on the turn rate
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _TurnRate);
            }
        }
        #endregion
    }
}
