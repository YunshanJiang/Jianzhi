using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("角色移动设置")]
    [SerializeField]
    private float m_moveSpeed = 5f;
    [SerializeField]
    private float m_gravity = -9.81f;

    [Header("摄像机设置")]
    public Transform CameraHolder => m_cameraHolder;
    [SerializeField]
    private Transform m_cameraHolder;
    [SerializeField]
    private float m_cameraRotationSpeed = 3f;

    private CharacterController m_characterController;
    private MobileController m_mobileController;
    private Vector3 m_moveDirection;
    private Vector3 m_playerVelocity;
    private float m_cameraRotationX;

    private Vector2 m_moveInput;
    private Vector2 m_rotateInput;
    [SerializeField]
    private bool m_isMovable = true;
    [SerializeField]
    private bool m_isRotate = true;
    [SerializeField]
    private bool m_lockCursor = true;

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_mobileController = GetComponent<MobileController>();
    }

    private void Start()
    {
        if (m_lockCursor && !m_mobileController.IsMobile)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        SyncCameraPitchFromHolder();
    }

    private void Update()
    {
        if (m_mobileController.IsMobile)
        {
            m_moveInput = m_mobileController.MobileJoystick.InputVector;
            m_rotateInput = m_mobileController.MobileCameraRotate.InputVector;
        }
        else
        {
            m_moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            m_rotateInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        HandleMovement();
        HandleCameraRotation();
    }

    private void HandleMovement()
    {
        if (!m_isMovable)
        {
            return;
        }
        // 如果角色在地面上，重置y轴速度
        if (m_characterController.isGrounded)
        {
            m_playerVelocity.y = 0f;
        }

        // 获取摄像机的前向方向和右侧方向
        Vector3 camForward = m_cameraHolder.forward;
        Vector3 camRight = m_cameraHolder.right;

        // 忽略y轴，确保角色只在水平面上移动
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 根据输入计算移动方向
        m_moveDirection = camForward * m_moveInput.y + camRight * m_moveInput.x;

        // 应用重力
        m_playerVelocity.y += m_gravity * Time.deltaTime;

        // 将移动方向和重力应用到角色控制器
        m_characterController.Move((m_moveDirection.normalized * m_moveSpeed + m_playerVelocity) * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        if (!m_isRotate || m_cameraHolder == null)
        {
            return;
        }
        // 摄像机上下旋转（绕X轴）
        m_cameraRotationX -= m_rotateInput.y * m_cameraRotationSpeed;
        // 限制上下旋转角度
        m_cameraRotationX = Mathf.Clamp(m_cameraRotationX, -80f, 80f);
        ApplyCameraPitchToHolder();

        // 角色左右旋转（绕Y轴）
        // 为了使摄像机左右旋转速度和角色左右旋转速度可以分开调整，我们使用 m_cameraRotationSpeed 来控制摄像机
        // 你也可以选择添加一个新的变量，例如 m_playerRotationSpeedY 来控制角色左右旋转
        transform.Rotate(Vector3.up * m_rotateInput.x * m_cameraRotationSpeed);
    }

    public void SetMovable(bool _isMovable)
    {
        m_isMovable = _isMovable;
    }

    public void SetRotate(bool _isRotate)
    {
        m_isRotate = _isRotate;
    }

    private void ApplyCameraPitchToHolder()
    {
        m_cameraHolder.localRotation = Quaternion.Euler(m_cameraRotationX, 0f, 0f);
    }

    private void SyncCameraPitchFromHolder()
    {
        if (m_cameraHolder == null)
        {
            return;
        }

        float currentX = m_cameraHolder.localEulerAngles.x;
        if (currentX > 180f)
        {
            currentX -= 360f;
        }

        m_cameraRotationX = Mathf.Clamp(currentX, -80f, 80f);
        ApplyCameraPitchToHolder();
    }

    /// <summary>
    /// 朝向目标位置
    /// </summary>
    /// <param name="_targetPosition"></param>
    public void FacingTarget(Transform _targetPosition)
    {
        Vector3 toTarget = _targetPosition.position - transform.position;
        if (toTarget.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 horizontal = new Vector3(toTarget.x, 0f, toTarget.z);
        if (horizontal.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(horizontal.normalized, Vector3.up);
        }

        if (m_cameraHolder == null)
        {
            return;
        }

        Vector3 holderToTarget = _targetPosition.position - m_cameraHolder.position;
        Vector2 planar = new Vector2(holderToTarget.x, holderToTarget.z);
        if (planar.sqrMagnitude < 0.0001f)
        {
            m_cameraRotationX = holderToTarget.y >= 0f ? -80f : 80f;
        }
        else
        {
            float pitch = Mathf.Atan2(holderToTarget.y, planar.magnitude) * Mathf.Rad2Deg;
            m_cameraRotationX = Mathf.Clamp(-pitch, -80f, 80f);
        }

        ApplyCameraPitchToHolder();
    }
}
