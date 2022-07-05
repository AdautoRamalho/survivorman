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

        [SerializeField]
        private float moveSpeed = 0.02f;

        [SerializeField]
        private float movementTolerance = 0.01f;

        [SerializeField]
        private Vector2 startPosition = new (0,0);

        [SerializeField]
        private NetworkVariable<Vector2> movement = new();

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Move"];
        }

        private void OnEnable()
        {
            _playerInput.actions.Enable();

            _moveAction.performed += MoveActionOnPerformed;
        }

        private void OnDisable()
        {
            _playerInput.actions.Disable();

            _moveAction.performed -= MoveActionOnPerformed;
        }

        private void Start()
        {
            transform.position = startPosition;
        }

        private void Update()
        {
            if (IsServer)
                UpdateServer();
        }

        private void UpdateServer()
        {
            var localTransform = transform;
            var position = localTransform.position;

            var newPosition = new Vector2(
                position.x + movement.Value.x,
                position.z + movement.Value.y);

            if (Math.Abs(position.x - newPosition.x) < movementTolerance && Math.Abs(position.z - newPosition.y) < movementTolerance)
                return;

            localTransform.position = new Vector3(newPosition.x, 0, newPosition.y);
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
    }
}
