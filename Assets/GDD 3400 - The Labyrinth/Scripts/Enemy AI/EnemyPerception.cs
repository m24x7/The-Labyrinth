using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class handles enemy perception logic
    /// </summary>
    [RequireComponent(typeof(EnemyAgent))]
    public class EnemyPerception : MonoBehaviour
    {
        // Field of View parameters
        public float viewRadius = 10f;
        [Range(0,360)] public float viewAngle = 45f;

        public LayerMask foVTargetMask;
        public LayerMask foVObstacleMask;

        // Lists to hold visible targets and close obstacles
        public List<Transform> visibleTargets = new List<Transform>();
        public List<GameObject> closeObstacles = new List<GameObject>();

        // Hearing Vars
        public bool heardNoise = false;
        public Vector3 heardNoisePos;

        #region Field of View
        /// <summary>
        /// This coroutine periodically finds visible targets within the enemy's field of view
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public IEnumerator FindTargetsWithDelay(float delay)
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);
                FindVisibleTargets();
            }
        }
        /// <summary>
        /// This method finds all visible targets within the enemy's field of view
        /// </summary>
        private void FindVisibleTargets()
        {
            visibleTargets.Clear(); // Clear the list of visible targets

            // Find all targets within the view radius
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, foVTargetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                // Get the root transform of the target
                Transform target = targetsInViewRadius[i].transform.root;

                // Check if the target is within the field of view
                if (InFieldOfView(target))
                {
                    // Add the target to the list of visible targets
                    visibleTargets.Add(target);
                }
            }
        }

        /// <summary>
        /// This method checks if a target is within the enemy's field of view
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool InFieldOfView(Transform target)
        {
            // Get direction to target
            Vector3 dirToTarget = (target.position - transform.position);

            // Check if within view angle
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                // Get distance to target
                float distToTarget = Vector3.Distance(transform.position, target.position);

                // Check if the target is sneaking
                if (target.root.GetComponent<PlayerController>() != null)
                {
                    if (target.root.GetComponent<PlayerController>().IsSneaking)
                    {
                        // If sneaking, reduce detection distance by half
                        if (distToTarget >= viewRadius * 0.5f) return false;
                    }
                }

                // Check for obstacles between enemy and target
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, foVObstacleMask))
                {
                    return true; // Target is visible
                }
            }

            return false; // Target is not visible
        }

        /// <summary>
        /// This method finds all close obstacles within the enemy's view radius
        /// </summary>
        public void FindCloseObstacles()
        {
            closeObstacles.Clear(); // Clear the list of close obstacles

            // Find all obstacles within the view radius
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, foVObstacleMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                GameObject target = targetsInViewRadius[i].gameObject; // Get the obstacle game object
                closeObstacles.Add(target); // Add the obstacle to the list of close obstacles
            }
        }
        #endregion


        #region Hearing
        /// <summary>
        /// This method sets the heard noise position and flag
        /// </summary>
        /// <param name="noisePos"></param>
        public void SetHeardNoise(Vector3 noisePos)
        {
            heardNoise = true;
            heardNoisePos = noisePos;
        }
        #endregion
    }
}
