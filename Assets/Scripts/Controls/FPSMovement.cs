using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSMovement : MonoBehaviour
{
    [Range(0f, 5f)]
    [SerializeField]
    private float sensitivity = 1f;
    [Range(1f, 20f)]
    [SerializeField]
    private float _moveSpeed = 10f;
    [Range(5f, 20f)]
    [SerializeField]
    private float _gravity = 9.81f;
    private Vector3 _velocity;
    [Range(1f, 10f)]
    [SerializeField]
    private float jumpHeight = 2f;

    private CharacterController _ctrl;
    private Camera _cam;
    private InputMaster _input;
    private CapsuleCollider _collider;

    [SerializeField]
    private Transform _groundCheck;
    [SerializeField]
    private float groundDistance = 0.4f;
    [SerializeField]
    private LayerMask groundMask;

    private Vector2 _moveDirection;
    private bool _isGrounded;

    private float _stepHeight; 

    private void Awake() {
        _input = new InputMaster();

        _input.Player.Movement.performed += OnMovement;
        _input.Player.Look.performed += OnLook;
        _input.Player.Jump.performed += OnJump;
    }

    private void OnEnable() {
        _input.Enable();    
    }

    private void OnDestroy() {
        _input.Disable();
    }

    void Start()
    {
        _ctrl = GetComponent<CharacterController>();
        _cam = Camera.main;
        _collider = GetComponent<CapsuleCollider>();

        _stepHeight = _ctrl.stepOffset;
    }

    // Update is called once per frame
    void Update() {
        updateGrounded();
        procGravity();
        Move();

        // Get current mouse+keyboard
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        // Lock/Free Mouse
        if (mouse.leftButton.wasPressedThisFrame) {
            Cursor.lockState = CursorLockMode.Locked;
        } else if (keyboard.escapeKey.wasPressedThisFrame) {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnDrawGizmos() {
        float colliderHeight = _collider != null ? _collider.height : 2f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_groundCheck.position, groundDistance);
    }

    private void updateGrounded() {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, groundDistance, groundMask);
        if (!_isGrounded && _ctrl.stepOffset > 0f) {
            _ctrl.stepOffset = 0f;
        } else if(_isGrounded && _ctrl.stepOffset == 0f) {
            _ctrl.stepOffset = _stepHeight;
        }
    }

    private void procGravity() {
        if (!_isGrounded) {
            _velocity.y -= _gravity * Time.deltaTime;
        } else if (_velocity.y < 0f) {
            _velocity.y = -2f;
        }
    }

    public void OnMovement(InputAction.CallbackContext context) {
        Vector2 dir = context.ReadValue<Vector2>();
        _moveDirection = dir.normalized;
    }

    public void Move() {
        Debug.Log($"{_moveDirection} * {_moveSpeed} * {Time.deltaTime}");
        Vector3 worldMove = transform.right * _moveDirection.x + transform.forward * _moveDirection.y;

        _ctrl.Move(worldMove * _moveSpeed * Time.deltaTime);

        _ctrl.Move(_velocity * Time.deltaTime);
    }

    public void OnLook(InputAction.CallbackContext context) {
        if (Cursor.lockState == CursorLockMode.None) {
            return;
        }

        // Y Look
        Vector2 lookDir = context.ReadValue<Vector2>();
        transform.Rotate(new Vector3(0f, lookDir.x * sensitivity * Time.deltaTime, 0f));

        // X look
        Vector3 camAngle = _cam.transform.rotation.eulerAngles;
        camAngle.y = Mathf.Clamp(camAngle.y, -90f, 90f);
        _cam.transform.Rotate(new Vector3(-lookDir.y * sensitivity * Time.deltaTime, 0f, 0f));
    }

    public void OnJump(InputAction.CallbackContext context) {
        Debug.Log("I JUMPED!");
        if (_isGrounded) {
            _velocity.y = Mathf.Sqrt(jumpHeight * 2 * _gravity);
        }
    }
}
