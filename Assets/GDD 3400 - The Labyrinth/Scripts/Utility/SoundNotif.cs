using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class is used to notify agents of created sounds.
    /// </summary>
    public class SoundNotif
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        public bool CollectColliders(Vector3 soundCenter, float soundRadius, LayerMask hitMask, out Collider[] collectedColliders)
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

        public bool CollectGameObjects(Collider[] colliders, out GameObject[] collectedObjects)
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
