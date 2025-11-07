using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class represents an enemy agent in the game, acting as the brain.
    /// </summary>
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

        // Audio Alert Chase
        [SerializeField] private AudioClip[] alertClip = new AudioClip[1];
        [SerializeField] private float alertTimerMax = 5f;
        [SerializeField] private float alertTimerMin = 2f;
        [SerializeField] private float alertTimer = 0f;

        // Audio Alert Investigate
        [SerializeField] private AudioClip[] investigateClip = new AudioClip[1];
        [SerializeField] private float InvestigateTimerMax = 5f;
        [SerializeField] private float InvestigateTimerMin = 2f;
        [SerializeField] private float InvestigateTimer = 0f;

        private bool DEBUG_SHOW_PATH = true;


        /// <summary>
        /// Awake is called when the script instance is being loaded
        /// </summary>
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

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
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

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        public void Update()
        {
            if (!_isActive) return;

            DecisionMaking(); // Handle Decision Making

            // Play Chase Alert Sound if in Chase State
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

            // Play Investigate Sound if in Investigate State
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

        /// <summary>
        /// This method handles the decision-making process of the enemy agent
        /// </summary>
        private void DecisionMaking()
        {
            Decision.DetermineEnemyState(Perception.visibleTargets, Perception.heardNoisePos, _destinationTarget);

            Decision.DetermineAction();
        }

        #region Action
        /// <summary>
        /// FixedUpdate is called at a fixed interval and is independent of frame rate
        /// </summary>
        private void FixedUpdate()
        {
            if (!_isActive) return;

            
            Debug.DrawLine(transform.position, _floatingTarget, Color.green);

            // Get new velocity based on floating target
            _velocity = Movement.GetNewAgentVelocity(_floatingTarget, _velocity);

            //Debug.Log($"Agent Velocity: {_velocity}");
            //if (_velocity != Vector3.zero) Movement.RotateAgent(_velocity);

            // Rotate to face floating target
            Movement.FaceTowards(_floatingTarget);
            //_rb.MovePosition(Movement.WallAvoidanceBehavior());

            // Apply velocity to rigidbody
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
            // Use the Movement module to set the destination target
            Movement.SetDestinationTarget(_path, destination, _levelManager,
                out Vector3 destTarget, out Vector3 floatTarget, out List<PathNode> newPath);

            _destinationTarget = destTarget; // Update destination target

            if (floatTarget != new Vector3()) _floatingTarget = floatTarget; // Update floating target

            if (newPath != null) _path = newPath; // Update path
        }
        #endregion

        /// <summary>
        /// OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            // If we collide with the player, restart the level
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
