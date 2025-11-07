using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class is used to notify agents of created sounds.
    /// </summary>
    public static class SoundNotif
    {
        /// <summary>
        /// This method collects all colliders within a sound radius
        /// </summary>
        /// <param name="soundCenter"></param>
        /// <param name="soundRadius"></param>
        /// <param name="hitMask"></param>
        /// <param name="collectedColliders"></param>
        /// <returns></returns>
        public static bool TryCollectColliders(Vector3 soundCenter, float soundRadius, LayerMask hitMask, out Collider[] collectedColliders)
        {
            Collider[] hitColliders = Physics.OverlapSphere(soundCenter, soundRadius, hitMask);

            collectedColliders = hitColliders;
            return collectedColliders.Length > 0;
        }

        /// <summary>
        /// This method converts an array of colliders to an array of game objects
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="collectedObjects"></param>
        /// <returns></returns>
        public static bool TryCollidersToGameObjects(Collider[] colliders, out List<GameObject> collectedObjects)
        {
            collectedObjects = new List<GameObject>();
            if (colliders == null || colliders.Length == 0)
            {
                return false;
            }


            foreach(Collider col in colliders)
            {
                if (col != null)
                {
                    collectedObjects.Add(col.transform.root.gameObject);
                }
            }



            return collectedObjects.Count > 0;
        }

        public static bool TryNotifyAgentsOfSound(List<GameObject> objects, Transform soundPosition)
        {
            bool notifiedAtLeastOne = false;
            foreach (GameObject obj in objects)
            {
                EnemyAgent agent = obj.GetComponent<EnemyAgent>();
                if (agent != null)
                {
                    agent.Perception.SetHeardNoise(soundPosition.position);
                    notifiedAtLeastOne = true;
                }
            }
            return notifiedAtLeastOne;
        }
    }
}
