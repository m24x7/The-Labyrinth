using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

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
        [SerializeField] private EnemyMovement Movement;
        public EnemyMovement GetMovement => Movement;
        [SerializeField] private EnemyPerception Perception;
        public EnemyPerception GetPerception => Perception;
        [SerializeField] private EnemyDecisionMaking Decision;
        public EnemyDecisionMaking GetDecision => Decision;
        [SerializeField] private EnemyActions Actions;
        public EnemyActions GetActions => Actions;
        #endregion

        //#region Movement Settings
        //[SerializeField] public float _TurnRate = 10f;
        //[SerializeField] public float _MaxSpeed = 5f;
        //[SerializeField] public float _SightDistance = 25f;

        //[SerializeField] public float _StoppingDistance = 1.5f;

        //[Tooltip("The distance to the destination before we start leaving the path")]
        //[SerializeField] public float _LeavingPathDistance = 2f; // This should not be less than 1

        //[Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        //[SerializeField] public float _MinimumPathDistance = 6f;
        //#endregion

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

        [SerializeField] private AudioClip[] alertClip = new AudioClip[1];
        [SerializeField] private float alertTimerMax = 5f;
        [SerializeField] private float alertTimerMin = 2f;
        [SerializeField] private float alertTimer = 0f;

        [SerializeField] private AudioClip[] investigateClip = new AudioClip[1];
        [SerializeField] private float InvestigateTimerMax = 5f;
        [SerializeField] private float InvestigateTimerMin = 2f;
        [SerializeField] private float InvestigateTimer = 0f;

        private bool DEBUG_SHOW_PATH = true;


        public void Awake()
        {
            // Grab and store the rigidbody component
            _rb = GetComponent<Rigidbody>();

            // Grab and store agent modules
            Movement = GetComponent<EnemyMovement>();
            Perception = GetComponent<EnemyPerception>();
            Decision = GetComponent<EnemyDecisionMaking>();
            Actions = GetComponent<EnemyActions>();

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

            if (Decision.enemyState == EnemyState.Chase)
            {
                if (alertTimer <= 0f)
                {
                    AudioSource.PlayClipAtPoint(alertClip[Random.Range(0, alertClip.Length)], transform.position);
                    alertTimer = Random.Range(alertTimerMin, alertTimerMax);
                }
                else
                {
                    alertTimer -= Time.deltaTime;
                }
            }

            if (Decision.enemyState == EnemyState.Investigate)
            {
                if (InvestigateTimer <= 0f)
                {
                    AudioSource.PlayClipAtPoint(investigateClip[Random.Range(0, investigateClip.Length)], transform.position);
                    InvestigateTimer = Random.Range(InvestigateTimerMin, InvestigateTimerMax);
                }
                else
                {
                    InvestigateTimer -= Time.deltaTime;
                }
            }
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


            Debug.DrawLine(transform.position, _floatingTarget, Color.green);

            _velocity = Movement.GetNewAgentVelocity(_floatingTarget, _velocity);

            //Debug.Log($"Agent Velocity: {_velocity}");
            //if (_velocity != Vector3.zero) Movement.RotateAgent(_velocity);
            Movement.FaceTowards(_floatingTarget);
            //_rb.MovePosition(Movement.WallAvoidanceBehavior());

            _rb.linearVelocity = _velocity;

            // Reset angular velocity to prevent physics interference
            if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        #endregion


        #region Movement Utils
        /// <summary>
        /// This method sets the destination target for the enemy agent
        /// </summary>
        /// <param name="destination"></param>
        public void SetDestinationTarget(Vector3 destination)
        {
            Movement.SetDestinationTarget(_path, destination, _levelManager,
                out Vector3 destTarget, out Vector3 floatTarget, out List<PathNode> newPath);

            _destinationTarget = destTarget;

            if (floatTarget != new Vector3()) _floatingTarget = floatTarget;

            if (newPath != null) _path = newPath;
        }
        #endregion

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
