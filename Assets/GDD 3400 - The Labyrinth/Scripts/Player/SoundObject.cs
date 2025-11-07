using System;
using System.Collections.Generic;
using UnityEngine;

namespace GDD3400.Labyrinth
{
    public class SoundObject : MonoBehaviour
    {
        [SerializeField] private float soundRange;
        [SerializeField] private float volume;
        [SerializeField] private AudioClip[] soundClips;

        public Action notifAgents;
        public void SetSoundObjectVars(float _soundRange = 5f, float _volume = 1f, AudioClip[] _soundClips = null)
        {
            soundRange = _soundRange;
            volume = _volume;
            soundClips = _soundClips;

            notifAgents += NotifyAgentsOfSound;
            
            //Debug.Log("SoundObject variables set.");
        }

        public void NotifyAgentsOfSound()
        {
            //Debug.Log("Notifying agents of sound...");
            AudioSource.PlayClipAtPoint(soundClips[UnityEngine.Random.Range(0, soundClips.Length)],
                transform.position,
                volume);

            bool collectedColliders = false;
            collectedColliders = SoundNotif.TryCollectColliders(transform.position,
                soundRange,
                LayerMask.GetMask("Enemy"),
                out Collider[] colliders);
            if (!collectedColliders) return;

            bool collectedObjects = false;
            collectedObjects = SoundNotif.TryCollidersToGameObjects(colliders,
                    out List<GameObject> objects);
            if (!collectedObjects) return;

            SoundNotif.TryNotifyAgentsOfSound(objects, transform);

            //Debug.Log("Agents notified of sound.");

            Destroy(gameObject);
        }
    }
}
