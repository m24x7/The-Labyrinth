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
        public float viewRadius = 10f;
        [Range(0,360)] public float viewAngle = 45f;

        public LayerMask foVTargetMask;
        public LayerMask foVObstacleMask;

        public List<Transform> visibleTargets = new List<Transform>();
        public List<GameObject> closeObstacles = new List<GameObject>();

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
            // Clear the list of visible targets
            visibleTargets.Clear();

            // Find all targets within the view radius
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, foVTargetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform target = targetsInViewRadius[i].transform.root;
                
                if (InFieldOfView(target))
                {
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
            Vector3 dirToTarget = (target.position - transform.position);
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, foVObstacleMask))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method finds all close obstacles within the enemy's view radius
        /// </summary>
        public void FindCloseObstacles()
        {
            // Clear the list of close obstacles
            closeObstacles.Clear();

            // Find all obstacles within the view radius
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, foVObstacleMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                GameObject target = targetsInViewRadius[i].gameObject;
                closeObstacles.Add(target);
            }
        }
        #endregion


        #region Hearing

        #endregion
    }
}
