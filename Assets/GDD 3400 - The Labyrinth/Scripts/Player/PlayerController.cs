using System;
using System.Collections;
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
        [SerializeField] private float BaseMoveSpeed = 10;
        [SerializeField] private float _DashDistance = 2.5f;
        [SerializeField] private float _DashCooldown = 1.5f;

        [Header("Connections")]
        [SerializeField] private Transform _GraphicsRoot;
        private Rigidbody _rigidbody;
        private InputAction _moveAction;
        //private InputAction _dashAction;
        private InputAction sprintAction;
        private InputAction sneakAction;
        private InputAction useDistractionAction;
        private Vector3 _moveVector;

        //private bool _performDash;
        //private bool _isDashing;

        private bool isSprinting;
        private float curMoveSpeed;

        private bool isSneaking;

        private void Awake()
        {
            // Assign member variables
            _rigidbody = GetComponent<Rigidbody>();

            AssignInputVars();

            AssignMovementVarDefaults();
        }

        private void Update()
        {
            // Store current input move vector, remap from X/Y to X/Z
            _moveVector.x = _moveAction.ReadValue<Vector2>().x;
            _moveVector.z = _moveAction.ReadValue<Vector2>().y;

            // Face the player toward the movement direction
            if (_moveVector.magnitude > 0f) transform.forward = Vector3.Lerp(transform.forward, _moveVector, 0.2f);

            // If the dash is available and pressed this frame, perform the dash
            //if (!_isDashing && _dashAction.WasPressedThisFrame()) PerformDash();

            // If sprint is held down, enable sprinting
            if (sprintAction.IsPressed()) isSprinting = true;
            else isSprinting = false;

            // If sneak is held down, enable sneaking
            if (sneakAction.IsPressed())
            {
                isSneaking = true;

                // Sneak has higher priority than sprint, so disable sprinting if sneaking
                if (isSprinting) isSprinting = false;
            }
            else isSneaking = false;
        }

        private void FixedUpdate()
        {
            // Determine current move speed based on sprinting/sneaking state
            if (!isSprinting && !isSneaking) curMoveSpeed = BaseMoveSpeed;
            else if (isSprinting) curMoveSpeed = BaseMoveSpeed * 1.5f;
            else if (isSneaking) curMoveSpeed = BaseMoveSpeed * 0.5f;

            // Apply the movement force
            _rigidbody.AddForce(_moveVector * curMoveSpeed * 4f, ForceMode.Force);
        }

        /// <summary>
        /// This method assigns input action variables
        /// </summary>
        private void AssignInputVars()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            sprintAction = InputSystem.actions.FindAction("Sprint");
            sneakAction = InputSystem.actions.FindAction("Sneak");
            useDistractionAction = InputSystem.actions.FindAction("UseDistraction");
        }

        /// <summary>
        /// This method assigns default values to movement-related variables
        /// </summary>
        private void AssignMovementVarDefaults()
        {
            // Set move vars to default vals
            isSprinting = false;
            isSneaking = false;
            curMoveSpeed = BaseMoveSpeed;
        }
    }
}
