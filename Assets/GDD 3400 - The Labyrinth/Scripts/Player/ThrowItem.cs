using UnityEngine;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class handles the logic for throwing items/objects.
    /// </summary>
    public class ThrowItem : MonoBehaviour
    {
        [SerializeField] private GameObject SoundObjectPrefab;
        [SerializeField] private AudioClip[] throwSoundClips = new AudioClip[1];
        [SerializeField] private float audioVolume = 1.0f;
        [SerializeField] private float audioRange = 5.0f;

        [SerializeField] private float despawnTimer = 10.0f;
        public void InitializeThrownItem(Vector3 throwDirection, float throwForce)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }
        }

        private void Update()
        {
            despawnTimer -= Time.deltaTime;
            if (despawnTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player"))
            {
                // Ignore collisions with enemies and the player
                return;
            }
            // Create a sound object at the point of impact
            GameObject soundObjectInstance = Instantiate(SoundObjectPrefab, transform.position, Quaternion.identity);
            SoundObject soundObject = soundObjectInstance.GetComponent<SoundObject>();
            if (soundObject != null)
            {
                soundObject.SetSoundObjectVars(audioRange, audioVolume, throwSoundClips);
                soundObject.notifAgents?.Invoke();
            }
        }
    }
}
