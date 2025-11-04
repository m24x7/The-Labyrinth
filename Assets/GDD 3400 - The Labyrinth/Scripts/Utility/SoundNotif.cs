using Unity.VisualScripting;
using UnityEditor;
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

            if (hitColliders.Length == 0)
            {
                collectedColliders = null;
                return false;
            }

            collectedColliders = hitColliders;
            return true;
        }

        /// <summary>
        /// This method converts an array of colliders to an array of game objects
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="collectedObjects"></param>
        /// <returns></returns>
        public static bool TryCollidersToGameObjects(Collider[] colliders, out GameObject[] collectedObjects)
        {
            collectedObjects = null;
            if (colliders == null || colliders.Length == 0)
            {
                return false;
            }


            for (int i = 0; i < colliders.Length; i++)
            {
                //ArrayUtility.AddRange(ref collectedObjects, colliders[i].gameObject);
                //collectedObjects.add
            }


            
            return false;
        }
    }
}
