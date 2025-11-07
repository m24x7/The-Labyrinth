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

        #region Scripts
        [SerializeField] public EnemyMovement Movement;
        [SerializeField] public EnemyPerception Perception;
        [SerializeField] private EnemyDecisionMaking Decision;
        [SerializeField] private EnemyActions Actions;
        #endregion

        #region Movement Settings
        [SerializeField] public float _TurnRate = 10f;
        [SerializeField] public float _MaxSpeed = 5f;
        [SerializeField] public float _SightDistance = 25f;

        [SerializeField] public float _StoppingDistance = 1.5f;

        [Tooltip("The distance to the destination before we start leaving the path")]
        [SerializeField] public float _LeavingPathDistance = 2f; // This should not be less than 1

        [Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        [SerializeField] public float _MinimumPathDistance = 6f;
        #endregion

        #region Movement Vars
        [SerializeField] private Vector3 _velocity;
        public Vector3 Velocity => _velocity;
        [SerializeField] private Vector3 _floatingTarget;
        public Vector3 FloatingTarget { get => _floatingTarget; set => _floatingTarget = value; }
        [SerializeField] private Vector3 _destinationTarget;
        public Vector3 DestinationTarget => _destinationTarget;
        List<PathNode> _path;
        public List<PathNode> Path => _path;
        #endregion

        private Rigidbody _rb;

        [SerializeField] private CapsuleCollider agentCollider;
        public Collider AgentCollider => agentCollider;

        private LayerMask _wallLayer;

        private bool DEBUG_SHOW_PATH = true;


        public void Awake()
        {
            // Grab and store the rigidbody component
            _rb = GetComponent<Rigidbody>();

            // Grab and store agent modules
            Movement = GetComponent<EnemyMovement>();
            Perception = GetComponent<EnemyPerception>();
            Decision = GetComponent<EnemyDecisionMaking>();

            // Grab and store the wall layer
            _wallLayer = LayerMask.GetMask("Walls");

            // If collider is not set, set it
            if (agentCollider == null) agentCollider = transform.Find("Collider").GetComponent<CapsuleCollider>();
        }

        public void Start()
        {
            // If we didn't manually set the level manager, find it
            if (_levelManager == null) _levelManager = FindAnyObjectByType<LevelManager>();

            // If we still don't have a level manager, throw an error
            if (_levelManager == null) Debug.LogError("Unable To Find Level Manager");

            // Start Enemy Line of Sight Routine
            StartCoroutine(Perception.FindTargetsWithDelay(0.2f));

            // Initialize destination & target position to starting position
            _destinationTarget = transform.position;
            _floatingTarget = transform.position;
        }

        public void Update()
        {
            if (!_isActive) return;

            DecisionMaking();
        }

        private void DecisionMaking()
        {
            Decision.DetermineEnemyState(Perception.visibleTargets, Perception.heardNoisePos, _destinationTarget);

            Decision.DetermineAction();
        }

        #region Action
        private void FixedUpdate()
        {
            if (!_isActive) return;


            Debug.DrawLine(this.transform.position, _floatingTarget, Color.green);

            _velocity = Movement.GetNewAgentVelocity(_floatingTarget, _StoppingDistance, _velocity,
                _MaxSpeed);
            //Debug.Log($"Agent Velocity: {_velocity}");
            if (_velocity != Vector3.zero) Movement.RotateAgent(_velocity, _TurnRate);

            _rb.linearVelocity = _velocity;
        }
        #endregion


        #region Movement Utils
        /// <summary>
        /// This method sets the destination target for the enemy agent
        /// </summary>
        /// <param name="destination"></param>
        public void SetDestinationTarget(Vector3 destination)
        {
            Movement.SetDestinationTarget(_path, destination, _MinimumPathDistance, _levelManager,
                out Vector3 destTarget, out Vector3 floatTarget, out List<PathNode> newPath);

            _destinationTarget = destTarget;

            if (floatTarget != new Vector3()) _floatingTarget = floatTarget;

            if (newPath != null) _path = newPath;
        }
        #endregion
    }
}
