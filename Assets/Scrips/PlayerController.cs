using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrips
{
    public class PlayerController : NetworkBehaviour
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _swingAction;

        [SerializeField]
        private float moveSpeed = 0.02f;
        //
        // [SerializeField]
        // private float movementTolerance = 0.01f;

        [SerializeField]
        private Vector2 startPosition = new (0,0);

        [SerializeField]
        private NetworkVariable<Vector2> movement = new();

        [SerializeField]
        private NetworkVariable<float> rotation = new();

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Move"];
            _swingAction = _playerInput.actions["Swing"];
        }

        private void OnEnable()
        {
            _playerInput.actions.Enable();

            _moveAction.performed += MoveActionOnPerformed;
            _swingAction.performed += SwingActionOnPerformed;
        }

        private void OnDisable()
        {
            _playerInput.actions.Disable();

            _moveAction.performed -= MoveActionOnPerformed;
            _swingAction.performed -= SwingActionOnPerformed;
        }

        private void Start()
        {
            transform.position = new Vector3(startPosition.x, 0, startPosition.y);
        }

        private void Update()
        {
            if (IsServer)
                UpdateServer();
        }

        private void UpdateServer()
        {
            // var localTransform = transform;
            // var position = localTransform.position;
            // var newPosition = new Vector2(
            //     position.x + movement.Value.x,
            //     position.z + movement.Value.y);
            //
            // if (Math.Abs(position.x - newPosition.x) < movementTolerance && Math.Abs(position.z - newPosition.y) < movementTolerance)
            //     return;
            //
            // localTransform.position = new Vector3(newPosition.x, 0, newPosition.y);

            var body = GetComponent<Rigidbody>();
            body.AddForce(new Vector3(movement.Value.x * Time.deltaTime, 0, movement.Value.y * Time.deltaTime), ForceMode.VelocityChange);
            body.AddTorque(new Vector3(0, rotation.Value * Time.deltaTime, 0), ForceMode.VelocityChange);
        }

        private void MoveActionOnPerformed(InputAction.CallbackContext context)
        {
            if (!(IsClient && IsOwner))
                return;

            var newMovement = context.ReadValue<Vector2>();

            UpdateClientPositionServerRpc(newMovement * moveSpeed);
        }

        [ServerRpc]
        private void UpdateClientPositionServerRpc(Vector2 newMovement)
        {
            movement.Value = newMovement;
        }

        private void SwingActionOnPerformed(InputAction.CallbackContext context)
        {
            if (!(IsClient && IsOwner))
                return;

            SwingServerRpc(context.ReadValueAsButton());
        }

        [ServerRpc]
        private void SwingServerRpc(bool isPressing)
        {
            rotation.Value = isPressing ? 50 : 0;
        }

    }
}
