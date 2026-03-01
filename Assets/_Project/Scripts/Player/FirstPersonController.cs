using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 1인칭 플레이어 컨트롤러 — New Input System 기반.
/// WASD 이동 / Left Shift 달리기 / Left Ctrl 앉기 / Space 점프 / 마우스 시점 / ESC 커서 토글
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] float walkSpeed   = 5f;
    [SerializeField] float sprintSpeed = 9f;
    [SerializeField] float crouchSpeed = 2.5f;

    [Header("점프 / 중력")]
    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity    = -20f;

    [Header("앉기")]
    [SerializeField] float standHeight        = 2f;
    [SerializeField] float crouchHeight       = 1f;
    [SerializeField] float crouchTransition   = 12f; // lerp 속도

    [Header("시점")]
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float maxLookAngle     = 80f;

    // ── 내부 상태 ──────────────────────────────────────────────
    CharacterController _cc;
    Transform           _camTransform;
    Vector3             _velocity;
    float               _xRotation;

    bool    _isCrouching;
    float   _camStandLocalY;   // Awake 시점의 카메라 로컬 Y

    // ── 초기화 ────────────────────────────────────────────────
    void Awake()
    {
        _cc           = GetComponent<CharacterController>();
        _camTransform = GetComponentInChildren<Camera>().transform;
        _camStandLocalY = _camTransform.localPosition.y;

        _cc.height = standHeight;
        _cc.center = Vector3.zero;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // ── 업데이트 ──────────────────────────────────────────────
    void Update()
    {
        HandleCrouch();      // 높이 변경을 먼저 처리
        HandleMovement();
        HandleLook();
        HandleCursorToggle();
    }

    // ── 이동 ──────────────────────────────────────────────────
    void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        // WASD
        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        // 속도 선택: 앉기 > 달리기 > 걷기
        float speed = _isCrouching              ? crouchSpeed
                    : kb.leftShiftKey.isPressed ? sprintSpeed
                    : walkSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize(); // 대각선 정규화
        _cc.Move(move * speed * Time.deltaTime);

        // 점프 (앉은 상태에서는 불가)
        if (kb.spaceKey.wasPressedThisFrame && grounded && !_isCrouching)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    // ── 앉기 ──────────────────────────────────────────────────
    void HandleCrouch()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // 토글: Ctrl 눌릴 때마다 상태 전환
        if (kb.leftCtrlKey.wasPressedThisFrame)
        {
            // 일어서려 할 때 머리 위 공간 체크
            if (_isCrouching && !CanStandUp()) { /* 공간 없으면 무시 */ }
            else _isCrouching = !_isCrouching;
        }
        float targetH = _isCrouching ? crouchHeight : standHeight;

        // CharacterController 높이 / 중심 보간 (발이 바닥에 고정되도록 center 조정)
        float newH = Mathf.Lerp(_cc.height, targetH, Time.deltaTime * crouchTransition);
        _cc.center = new Vector3(0f, (newH - standHeight) * 0.5f, 0f);
        _cc.height = newH;

        // 카메라 Y 보간 (높이 변화량만큼 카메라도 이동)
        float targetCamY = _camStandLocalY + (newH - standHeight) * 0.5f;
        Vector3 camPos   = _camTransform.localPosition;
        camPos.y         = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchTransition);
        _camTransform.localPosition = camPos;
    }

    /// <summary>앉은 상태에서 일어설 공간이 있는지 SphereCast로 체크</summary>
    bool CanStandUp()
    {
        float checkDist = standHeight - _cc.height - 0.01f;
        if (checkDist <= 0f) return true;

        Vector3 origin = transform.position + _cc.center + Vector3.up * (_cc.height * 0.5f);
        return !Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.up, out _, checkDist);
    }

    // ── 시점 ──────────────────────────────────────────────────
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

    // ── 커서 토글 ────────────────────────────────────────────
    void HandleCursorToggle()
    {
        var kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }
}
