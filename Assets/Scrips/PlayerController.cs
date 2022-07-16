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
        private NetworkVariable<bool> attacking = new();

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
            var body = GetComponent<Rigidbody>();
            if (attacking.Value)
                body.AddForce(new Vector3(movement.Value.x * 5 * Time.deltaTime, 0, movement.Value.y * 5 * Time.deltaTime), ForceMode.VelocityChange);
            else
                body.AddForce(new Vector3(movement.Value.x * Time.deltaTime, 0, movement.Value.y * Time.deltaTime), ForceMode.VelocityChange);
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
            attacking.Value = isPressing;
        }

    }
}
