using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GDD3400.Labyrinth
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAgent : MonoBehaviour
    {
        [SerializeField] private LevelManager _levelManager;

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        // Scripts
        [SerializeField] private EnemyMovement Movement;

        [SerializeField] public float _TurnRate = 10f;
        [SerializeField] public float _MaxSpeed = 5f;
        [SerializeField] public float _SightDistance = 25f;

        [SerializeField] public float _StoppingDistance = 1.5f;

        [Tooltip("The distance to the destination before we start leaving the path")]
        [SerializeField] public float _LeavingPathDistance = 2f; // This should not be less than 1

        [Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        [SerializeField] public float _MinimumPathDistance = 6f;

        private Vector3 _velocity;
        private Vector3 _floatingTarget;
        private Vector3 _destinationTarget;
        List<PathNode> _path;

        private Rigidbody _rb;

        //[SerializeField] private CapsuleCollider agentCollider;
        //public Collider AgentCollider => agentCollider;

        private LayerMask _wallLayer;

        private bool DEBUG_SHOW_PATH = true;


        public void Awake()
        {
            // Grab and store the rigidbody component
            _rb = GetComponent<Rigidbody>();

            Movement = GetComponent<EnemyMovement>();

            // Grab and store the wall layer
            _wallLayer = LayerMask.GetMask("Walls");

            //// If collider is not set, set it
            //if (agentCollider == null) agentCollider = transform.Find("Collider").GetComponent<CapsuleCollider>();
        }

        public void Start()
        {
            // If we didn't manually set the level manager, find it
            if (_levelManager == null) _levelManager = FindAnyObjectByType<LevelManager>();

            // If we still don't have a level manager, throw an error
            if (_levelManager == null) Debug.LogError("Unable To Find Level Manager");
        }

        public void Update()
        {
            if (!_isActive) return;

            Perception();
            DecisionMaking();
        }

        private void Perception()
        {
            // TODO: Implement perception            
        }

        private void DecisionMaking()
        {
            // If we have a path, follow it
            if (_path != null && _path.Count > 0)
            {
                if (Vector3.Distance(transform.position, _destinationTarget) <= _LeavingPathDistance)
                {
                    _path = null;
                    _floatingTarget = _destinationTarget;
                }
                else
                {
                    Movement.PathFollowing(_path, out Vector3 target);
                    _floatingTarget = target;
                }
            }
        }

        #region Action
        private void FixedUpdate()
        {
            if (!_isActive) return;


            Debug.DrawLine(this.transform.position, _floatingTarget, Color.green);

            _velocity = Movement.GetNewAgentVelocity(_floatingTarget, _StoppingDistance, _velocity,
                _MaxSpeed);

            Movement.RotateAgent(_velocity, _TurnRate);

            _rb.linearVelocity = _velocity;
        }
        #endregion



        public void SetDestinationTarget(Vector3 destination)
        {
            Movement.SetDestinationTarget(_path, destination, _MinimumPathDistance, _levelManager,
                out Vector3 destTarget, out Vector3 floatTarget, out List<PathNode> newPath);

            _destinationTarget = destTarget;

            if (floatTarget != new Vector3()) _floatingTarget = floatTarget;

            if (newPath != null) _path = newPath;
        }
    }
}
