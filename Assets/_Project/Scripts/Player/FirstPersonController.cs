using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 1인칭 플레이어 컨트롤러 — New Input System 기반.
/// WASD 이동 / Space 점프 / 마우스 시점 / ESC 커서 토글
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -20f;

    [Header("시점")]
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float maxLookAngle = 80f;

    CharacterController _cc;
    Transform _camTransform;
    Vector3 _velocity;
    float _xRotation;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _camTransform = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleCursorToggle();
    }

    void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        // WASD 입력
        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize(); // 대각선 이동 속도 정규화
        _cc.Move(move * moveSpeed * Time.deltaTime);

        // 점프
        if (kb.spaceKey.wasPressedThisFrame && grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;

        _xRotation = Mathf.Clamp(_xRotation - delta.y, -maxLookAngle, maxLookAngle);
        _camTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(0f, delta.x, 0f);
    }

    void HandleCursorToggle()
    {
        var kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
