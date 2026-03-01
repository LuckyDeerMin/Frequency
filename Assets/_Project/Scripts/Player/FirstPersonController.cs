using UnityEngine;

/// <summary>
/// 1인칭 플레이어 컨트롤러.
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
    [SerializeField] float mouseSensitivity = 2f;
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
        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        _cc.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _xRotation = Mathf.Clamp(_xRotation - mouseY, -maxLookAngle, maxLookAngle);
        _camTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(0f, mouseX, 0f);
    }

    void HandleCursorToggle()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

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
