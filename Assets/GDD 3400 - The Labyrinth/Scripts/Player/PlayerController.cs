using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace  GDD3400.Labyrinth
{
    /// <summary>
    /// This class handles player movement and actions
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private const float SneakSpeed = 2.0f;
        [SerializeField] private const float WalkSpeed = 4.0f;
        [SerializeField] private const float SprintSpeed = 6.0f;
        [SerializeField] private const float RotationSpeed = 1.0f;

        [Header("Connections")]
        [SerializeField] private Transform _GraphicsRoot;
        private Rigidbody _rigidbody;
        private CharacterController charController;
        private InputAction _moveAction;
        private InputAction sprintAction; //Left Shift
        private InputAction sneakAction; // C
        private InputAction useDistractionAction; // Left Mouse Button
        private Vector3 _moveVector;
        private Vector2 look;

        //private bool _performDash;
        //private bool _isDashing;

        // Movement Vars
        private bool isSprinting;
        private float curMoveSpeed;
        private bool isSneaking;
        public bool IsSneaking => isSneaking;

        //private bool canUseDistractionItem = true;
        [SerializeField] private GameObject distractionItemPrefab;

        #region Audio Variables
        [SerializeField] private GameObject SoundObjectPrefab;

        #region Footstep Audio Vars
        // Footstep audio clips
        [SerializeField] private AudioClip[] footstepClips = new AudioClip[1];

        // Footstep audio volume levels
        [SerializeField] private const float FootStepVolumeSneak = 0.25f;
        [SerializeField] private const float FootStepVolumeWalk = 0.5f;
        [SerializeField] private const float FootStepVolumeSprint = 0.60f;
        [SerializeField] private float footStepVolume;

        // Footstep audio intervals
        [SerializeField] private const float FootStepIntervalSneak = 0.75f;
        [SerializeField] private const float FootStepIntervalWalk = 0.5f;
        [SerializeField] private const float FootStepIntervalSprint = 0.25f;
        [SerializeField] private float footStepInterval;
        [SerializeField] private float curFootStepInterval = 0;

        // Footstep audio ranges
        [SerializeField] private const float FootStepRangeSneak = 2f;
        [SerializeField] private const float FootStepRangeWalk = 7f;
        [SerializeField] private const float FootStepRangeSprint = 10f;
        [SerializeField] private float footStepRange;
        #endregion

        //private Action soundPlayed;
        private Coroutine notifyAgentsOfSound;
        #endregion

        #region Camera
        [SerializeField] private GameObject camTarget; // Cinemachine camera target
        [SerializeField] private float TopClamp = 90.0f;
        [SerializeField] private float BottomClamp = -90.0f;
        [SerializeField] private float threshold = 0.01f; // to prevent too small input from affecting camera movement
        [SerializeField] private GameObject mainCamera;
        private float cinemachineTargetPitch;
        private float rotationVelocity;
        #endregion

        /// <summary>
        /// Awake is called when the script instance is being loaded
        /// </summary>
        private void Awake()
        {
            // Assign member variables
            _rigidbody = GetComponent<Rigidbody>();

            AssignInputVars();

            AssignMovementVarDefaults();

            AssignAudioVarDefaults();

            SetFootstepInterval();

            // Get the main camera in the scene
            if (mainCamera == null)
            {
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            // Get the Character Controller component
            if (charController == null)
            {
                charController = GetComponent<CharacterController>();
            }
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            // Lock cursor to center of screen
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // Store current input move vector, remap from X/Y to X/Z
            _moveVector.x = _moveAction.ReadValue<Vector2>().x;
            _moveVector.z = _moveAction.ReadValue<Vector2>().y;

            // Face the player toward the movement direction
            //if (_moveVector.magnitude > 0f) transform.forward = Vector3.Lerp(transform.forward, _moveVector, 0.2f);

            // If sprint is held down, enable sprinting
            if (sprintAction.IsPressed())
            {
                isSprinting = true;
            }
            else isSprinting = false;

            // If sneak is held down, enable sneaking
            if (sneakAction.IsPressed())
            {
                isSneaking = true;

                // Sneak has higher priority than sprint, so disable sprinting if sneaking
                if (isSprinting) isSprinting = false;
            }
            else isSneaking = false;

            // Update footstep audio settings based on movement flags
            SetFootstepInterval();
            SetFootstepAudioRange();
            SetFootstepAudioVolume();

            // Handle distraction item usage
            if (useDistractionAction.WasPressedThisFrame())
            {
                GameObject temp = Instantiate<GameObject>(distractionItemPrefab, camTarget.transform.position, transform.rotation);
                temp.GetComponent<ThrowItem>().InitializeThrownItem(mainCamera.transform.forward, 10f);
            }
        }

        /// <summary>
        /// FixedUpdate is called at a fixed interval and is independent of frame rate
        /// </summary>
        private void FixedUpdate()
        {
            // Reset angular velocity to prevent physics interference
            if (GetComponent<Rigidbody>() != null) GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            // If there is movement input
            if (_moveVector.magnitude != 0)
            {
                // Update current move speed based on sprinting/sneaking state
                SetMoveSpeed();

                // Apply the movement force
                //_rigidbody.AddForce((_moveVector.x * transform.right + _moveVector.z * transform.forward) * curMoveSpeed * 4f, ForceMode.Force);

                // Move the character controller
                charController.Move((_moveVector.x * transform.right + _moveVector.z * transform.forward).normalized * curMoveSpeed * Time.fixedDeltaTime);

                // Handle footstep audio playback
                if (curFootStepInterval <= 0f)
                {
                    //soundPlayed.Invoke();
                    //if (notifyAgentsOfSound == null) notifyAgentsOfSound = StartCoroutine(NotifyAgentsOfSound());
                    SoundObject temp = Instantiate<GameObject>(SoundObjectPrefab, transform.position, Quaternion.identity)
                        .GetComponent<SoundObject>();

                    // Set sound object variables
                    temp.SetSoundObjectVars(footStepRange, footStepVolume, footstepClips);

                    // Notify nearby agents of the sound
                    temp.notifAgents.Invoke();

                    //Debug.Log("Footstep sound played");

                    // Reset footstep interval timer
                    curFootStepInterval = footStepInterval;
                }
            }

            // Decrease footstep interval timer
            if (curFootStepInterval > 0f)
            {
                curFootStepInterval -= Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// LateUpdate is called after all Update functions have been called
        /// </summary>
        private void LateUpdate()
        {
            CameraRotation(); // Handle camera rotation
        }

        /// <summary>
        /// This method assigns input action variables
        /// </summary>
        private void AssignInputVars()
        {
            look = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();
            _moveAction = InputSystem.actions.FindAction("Move");
            sprintAction = InputSystem.actions.FindAction("Sprint");
            sneakAction = InputSystem.actions.FindAction("Sneak");
            useDistractionAction = InputSystem.actions.FindAction("UseDistraction");
        }

        /// <summary>
        /// OnLook is called when there is look input from the player
        /// </summary>
        /// <param name="value"></param>
        public void OnLook(InputValue value)
        {
            LookInput(value.Get<Vector2>());
        }

        /// <summary>
        /// LookInput processes look input from the player
        /// </summary>
        /// <param name="newLookDir"></param>
        public void LookInput(Vector2 newLookDir)
        {
            look = newLookDir;
        }

        /// <summary>
        /// This method assigns default values to movement-related variables
        /// </summary>
        private void AssignMovementVarDefaults()
        {
            // Set move vars to default vals
            isSprinting = false;
            isSneaking = false;
            curMoveSpeed = WalkSpeed;
        }

        /// <summary>
        /// Assigns default values to audio-related variables for footsteps.
        /// </summary>
        /// <remarks>This method sets the footstep volume, interval, and range to their default values 
        /// associated with walking. It is intended to initialize or reset these variables  to their standard
        /// defaults.</remarks>
        private void AssignAudioVarDefaults()
        {
            footStepVolume = FootStepVolumeWalk;
            footStepInterval = FootStepIntervalWalk;
            footStepRange = FootStepRangeWalk;
        }

        /// <summary>
        /// Sets the footstep interval based on the player's current movement state (sprinting, sneaking, or walking).
        /// </summary>
        private void SetFootstepInterval()
        {
            if (!isSprinting && !isSneaking)
            {
                footStepInterval = FootStepIntervalWalk;
                return;
            }

            if (isSprinting)
            {
                footStepInterval = FootStepIntervalSprint;
                return;
            }

            if (isSneaking)
            {
                footStepInterval = FootStepIntervalSneak;
                return;
            }
        }

        /// <summary>
        ///  Sets the footstep audio range based on the player's current movement state (sprinting, sneaking, or walking).
        /// </summary>
        private void SetFootstepAudioRange()
        {
            if (!isSprinting && !isSneaking)
            {
                footStepRange = FootStepRangeWalk;
                return;
            }

            if (isSprinting)
            {
                footStepRange = FootStepRangeSprint;
                return;
            }

            if (isSneaking)
            {
                footStepRange = FootStepRangeSneak;
                return;
            }
        }

        /// <summary>
        /// Sets the footstep audio volume based on the current movement state (sprinting, sneaking, or walking).
        /// </summary>
        private void SetFootstepAudioVolume()
        {
            if (!isSprinting && !isSneaking)
            {
                footStepVolume = FootStepVolumeWalk;
                return;
            }

            if (isSprinting)
            {
                footStepVolume = FootStepVolumeSprint;
                return;
            }

            if (isSneaking)
            {
                footStepVolume = FootStepVolumeSneak;
                return;
            }
        }

        /// <summary>
        /// Sets the current movement speed based on whether the player is sprinting, sneaking, or walking.
        /// </summary>
        private void SetMoveSpeed()
        {
            // Determine current move speed based on sprinting/sneaking state
            if (!isSprinting && !isSneaking)
            {
                curMoveSpeed = WalkSpeed;
                return;
            }
            else if (isSprinting)
            {
                curMoveSpeed = SprintSpeed;
                return;
            }
            else if (isSneaking)
            {
                curMoveSpeed = SneakSpeed;
                return;
            }
        }

        /// <summary>
        /// Sets the camera rotation based on player input.
        /// </summary>
        private void CameraRotation()
        {
            // if there is an input
            if (look.sqrMagnitude >= threshold)
            {
                cinemachineTargetPitch += look.y * RotationSpeed;
                rotationVelocity = look.x * RotationSpeed;

                // clamp our pitch rotation
                cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                camTarget.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * rotationVelocity);
            }
        }

        /// <summary>
        /// This method clamps an angle between a minimum and maximum value.
        /// </summary>
        /// <param name="lfAngle"></param>
        /// <param name="lfMin"></param>
        /// <param name="lfMax"></param>
        /// <returns></returns>
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
