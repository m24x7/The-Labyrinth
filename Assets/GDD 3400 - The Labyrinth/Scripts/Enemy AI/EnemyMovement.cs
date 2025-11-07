using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class handles enemy movement logic
    /// </summary>
    [RequireComponent(typeof(EnemyAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        #region Movement Settings
        [SerializeField] public float _TurnRate = 10f;
        [SerializeField] public float _MaxSpeed = 5f;
        [SerializeField] public float _SightDistance = 25f;

        [SerializeField] public float _StoppingDistance = 1f;

        [Tooltip("The distance to the destination before we start leaving the path")]
        [SerializeField] public float _LeavingPathDistance = 2f; // This should not be less than 1

        [Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        [SerializeField] public float _MinimumPathDistance = 4f;

        [SerializeField] private Collider agentCollider;
        #endregion

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
            // Find the closest node to the agent's current position
            int closestNodeIndex = GetClosestNode(_path);

            // Get the next node index
            int nextNodeIndex = closestNodeIndex + 1;

            // Determine the target node
            PathNode targetNode = null;
            if (nextNodeIndex < _path.Count)
            {
                targetNode = _path[nextNodeIndex];
            }
            else
            {
                targetNode = _path[closestNodeIndex];
            }

            // Set the floating target to the target node's position
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
        public void SetDestinationTarget(List<PathNode> _path, Vector3 destination, LevelManager _levelManager,
            out Vector3 _destinationTarget, out Vector3 _floatingTarget, out List<PathNode> newPath)
        {
            //Debug.Log("Destination: " + destination);

            // Set the destination target
            _destinationTarget = destination;

            // Reset the floating target and new path
            _floatingTarget = new Vector3();
            newPath = null;

            // If the straight line distance is greater than our minimum, Lets do pathfinding!
            if (Vector3.Distance(transform.position, _destinationTarget) >= _MinimumPathDistance)
            {
                // Get the start and end nodes for pathfinding
                PathNode startNode = _levelManager.GetNode(transform.position);
                PathNode endNode = _levelManager.GetNode(destination);

                // If we can't find either node, exit
                if (startNode == null || endNode == null)
                {
                    Debug.LogWarning("EnemyAgent: Unable to find start or end node for pathfinding.");
                    return;
                }

                // Find a new path using the Pathfinder
                newPath = Pathfinder.FindPath(startNode, endNode);

                // Smooth the path to remove unnecessary nodes
                newPath = SmoothPath(newPath);

                // Draw debug lines for the new path
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
            if (curPath == null || curPath.Count < 3) return curPath; // If the path is null or has less than 3 nodes, return it as is

            List<PathNode> smoothedPath = new List<PathNode>(); // Create a new list to hold the smoothed path

            smoothedPath.Add(curPath[0]); // Always add the first node

            int currentIndex = 2; // Start checking from the third node

            // Loop through the path and remove unnecessary nodes
            while (currentIndex < curPath.Count)
            {
                // Get the last added node to the smoothed path and the current node of the original path
                PathNode lastAddedNode = smoothedPath[smoothedPath.Count - 1];
                PathNode currentNode = curPath[currentIndex];

                // Check if there is a direct line of sight between the last added node and the current node
                if (!Physics.Linecast(lastAddedNode.transform.position + Vector3.up,
                    currentNode.transform.position + Vector3.up,
                    LayerMask.GetMask("Walls"),
                    QueryTriggerInteraction.Ignore))
                {
                    smoothedPath.Add(curPath[currentIndex - 1]);
                }
                currentIndex++;
            }

            smoothedPath.Add(curPath[curPath.Count - 1]); // Always add the last node

            return smoothedPath; // Return the smoothed path
        }

        /// <summary>
        /// This method gets the closest node to the player's current position
        /// </summary>
        /// <param name="_path"></param>
        /// <returns></returns>
        private int GetClosestNode(List<PathNode> _path)
        {
            int closestNodeIndex = 0; // Initialize to first index
            float closestDistance = float.MaxValue; // Start with a large distance

            // Loop through the path to find the closest node
            for (int i = 0; i < _path.Count; i++)
            {
                // Calculate the distance from the agent to the current node
                float distance = Vector3.Distance(transform.position, _path[i].transform.position);

                // If this distance is less than the closest distance found so far, update the closest node index and distance
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNodeIndex = i;
                }
            }

            // Return the index of the closest node
            return closestNodeIndex;
        }

        /// <summary>
        /// This coroutine draws debug lines between the nodes in the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerator DrawPathDebugLines(List<PathNode> path)
        {
            // Draw lines between each node in the path
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
        public Vector3 GetNewAgentVelocity(Vector3 _floatingTarget, Vector3 curVelocity)
        {
            // If we have a floating target and we are not close enough to it, move towards it
            if (_floatingTarget != Vector3.zero && Vector3.Distance(transform.position, _floatingTarget) > _StoppingDistance)
            {
                // Calculate the direction to the target position
                Vector3 direction = (_floatingTarget - transform.position).normalized;

                // Calculate the movement vector
                curVelocity = (direction + WallAvoidanceBehavior()).normalized * _MaxSpeed;

                return curVelocity;
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
        /// This method calculates a wall avoidance offset for the agent.
        /// </summary>
        /// <param name="minWallDist"></param>
        /// <returns></returns>
        public Vector3 WallAvoidanceBehavior(float minWallDist = 1f)
        {
            var center = agentCollider.bounds.center; // Get the center of the agent's collider

            // Find nearby wall colliders within the specified distance
            var nearby = Physics.OverlapSphere(center,
                minWallDist, LayerMask.NameToLayer("Walls"),
                QueryTriggerInteraction.Ignore);

            if (nearby.Length == 0) return Vector3.zero; // No nearby walls, no avoidance needed

            Vector3 totalOffset = Vector3.zero; // Initialize the total offset vector

            // Calculate the avoidance offset based on nearby walls
            foreach (var col in nearby)
            {
                // Find the closest point on the wall collider to the agent's center
                Vector3 closestPoint = col.ClosestPoint(center);

                // Calculate the vector away from the wall and its distance
                Vector3 awayFromWall = center - closestPoint;
                float dist = awayFromWall.magnitude;

                // If the agent is within the minimum wall distance, calculate the avoidance offset
                if (dist < minWallDist)
                {
                    float gain = (1f - (dist / minWallDist)); // Gain increases as we get closer to the wall
                    totalOffset += awayFromWall.normalized * (gain * (minWallDist - dist)); // Scale the offset by the gain and distance
                }
            }

            //if (totalOffset.sqrMagnitude > 0f) totalOffset = Vector3.ClampMagnitude(totalOffset, 0.25f);

            return totalOffset;
        }

        /// <summary>
        /// This method rotates the agent to face the direction of its velocity.
        /// </summary>
        /// <param name="_velocity"></param>
        /// <param name="_TurnRate"></param>
        public void RotateAgent(Vector3 _velocity)
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

        /// <summary>
        /// This method makes the agent face towards a specific target position.
        /// </summary>
        /// <param name="targetPos"></param>
        public void FaceTowards(Vector3 targetPos)
        {
            // Calculate the direction to the target position
            Vector3 direction = targetPos - transform.position;
            direction.y = 0; // Keep only the horizontal direction

            if (direction.sqrMagnitude < 0.01f) return; // Avoid small direction vector

            // Calculate the desired rotation towards the target position
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Smoothly rotate towards the target rotation based on the turn rate
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _TurnRate);
        }
        #endregion
    }
}
