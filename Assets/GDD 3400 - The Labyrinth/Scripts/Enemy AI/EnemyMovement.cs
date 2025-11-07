using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class handles enemy movement logic
    /// </summary>
    [RequireComponent(typeof(EnemyAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        private void FixedUpdate()
        {
            // Reset angular velocity to prevent physics interference
            if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        #region Path Following

        /// <summary>
        /// This method performs path following
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_floatingTarget"></param>
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

        /// <summary>
        /// This public method sets the destination target
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="destination"></param>
        /// <param name="_MinimumPathDistance"></param>
        /// <param name="_levelManager"></param>
        /// <param name="_destinationTarget"></param>
        /// <param name="_floatingTarget"></param>
        /// <param name="newPath"></param>
        public void SetDestinationTarget(List<PathNode> _path, Vector3 destination, float _MinimumPathDistance, LevelManager _levelManager,
            out Vector3 _destinationTarget, out Vector3 _floatingTarget, out List<PathNode> newPath)
        {
            //Debug.Log("Destination: " + destination);

            _destinationTarget = destination;
            _floatingTarget = new Vector3();
            newPath = null;

            // If the straight line distance is greater than our minimum, Lets do pathfinding!
            if (Vector3.Distance(transform.position, _destinationTarget) >= _MinimumPathDistance)
            {
                PathNode startNode = _levelManager.GetNode(transform.position);
                PathNode endNode = _levelManager.GetNode(destination);

                if (startNode == null || endNode == null)
                {
                    Debug.LogWarning("EnemyAgent: Unable to find start or end node for pathfinding.");
                    return;
                }

                newPath = Pathfinder.FindPath(startNode, endNode);

                newPath = SmoothPath(newPath);

                StartCoroutine(DrawPathDebugLines(newPath));
            }

            // Otherwise, move directly to the target
            else
            {
                _floatingTarget = _destinationTarget;
            }
        }

        /// <summary>
        /// This method smooths the given path by removing unnecessary nodes
        /// </summary>
        /// <param name="curPath"></param>
        /// <returns></returns>
        public List<PathNode> SmoothPath(List<PathNode> curPath)
        {
            if (curPath == null || curPath.Count < 3) return curPath;

            List<PathNode> smoothedPath = new List<PathNode>();
            smoothedPath.Add(curPath[0]);

            int currentIndex = 2;

            while (currentIndex < curPath.Count)
            {
                PathNode lastAddedNode = smoothedPath[smoothedPath.Count - 1];
                PathNode currentNode = curPath[currentIndex];

                if (!Physics.Linecast(lastAddedNode.transform.position + Vector3.up,
                    currentNode.transform.position + Vector3.up,
                    LayerMask.NameToLayer("Walls"),
                    QueryTriggerInteraction.Ignore))
                {
                    smoothedPath.Add(curPath[currentIndex - 1]);
                }
                currentIndex++;
            }

            smoothedPath.Add(curPath[curPath.Count - 1]);
            return smoothedPath;
        }

        /// <summary>
        /// This method gets the closest node to the player's current position
        /// </summary>
        /// <param name="_path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This coroutine draws debug lines between the nodes in the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
        /// <summary>
        /// This method calculates a new velocity for the agent to move towards a floating target.
        /// </summary>
        /// <param name="_floatingTarget"></param>
        /// <param name="_StoppingDistance"></param>
        /// <param name="curVelocity"></param>
        /// <param name="_MaxSpeed"></param>
        /// <returns></returns>
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

            // If we are close enough to the floating target, slow down or stop
            else
            {
                // Stop if within a small threshold
                if (curVelocity.magnitude < 0.1f)
                {
                    return Vector3.zero;
                }

                // Gradually reduce the velocity to simulate slowing down
                return curVelocity *= .95f;
            }
        }

        /// <summary>
        /// This method rotates the agent to face the direction of its velocity.
        /// </summary>
        /// <param name="_velocity"></param>
        /// <param name="_TurnRate"></param>
        public void RotateAgent(Vector3 _velocity, float _TurnRate)
        {
            //Debug.Log($"Rotating Agent with Velocity: {_velocity}");

            // Only rotate if we have a significant velocity
            if (_velocity == Vector3.zero || _velocity.magnitude < 0.1f) return;
            else
            {
                // Calculate the desired rotation towards the movement vector
                Quaternion targetRotation = Quaternion.LookRotation(_velocity);
                //Debug.Log($"Target Rotation: {targetRotation.eulerAngles}");

                // Smoothly rotate towards the target rotation based on the turn rate
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _TurnRate);
            }
        }
        #endregion
    }
}
