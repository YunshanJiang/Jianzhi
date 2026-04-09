using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 InputVector => m_inputVector;
    [SerializeField]
    private RectTransform m_joystickBackground;
    [SerializeField]
    private RectTransform m_joystickHandle;
    [SerializeField]
    private float m_moveRadius = 75f;

    private Vector2 m_startPosition;
    private Vector2 m_inputVector;

    // 在游戏开始时或脚本启用时执行
    void Start()
    {
        if (m_joystickBackground == null || m_joystickHandle == null)
        {
            Debug.LogError("请在 Unity 编辑器中指定摇杆背景和手柄!");
            return;
        }

        // 隐藏摇杆，直到触摸事件发生
        m_joystickBackground.gameObject.SetActive(false);
    }

    // 当手指第一次按下时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        // 激活摇杆背景，并将其移动到触摸位置
        m_joystickBackground.gameObject.SetActive(true);
        m_startPosition = eventData.position;
        m_joystickBackground.position = m_startPosition;

        // 调用 OnDrag 来立即更新手柄位置
        OnDrag(eventData);
    }

    // 当手指在屏幕上拖动时触发
    public void OnDrag(PointerEventData eventData)
    {
        // 计算从触摸点到摇杆中心的向量
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );

        // 如果向量长度超过了可移动半径，则将其限制在圆圈内
        if (position.magnitude > m_moveRadius)
        {
            position = position.normalized * m_moveRadius;
        }

        // 设置摇杆手柄的新位置
        m_joystickHandle.anchoredPosition = position;

        // 计算归一化的方向向量
        m_inputVector = position / m_moveRadius;
    }

    // 当手指离开屏幕时触发
    public void OnPointerUp(PointerEventData eventData)
    {
        // 将方向向量重置为 (0, 0)
        m_inputVector = Vector2.zero;

        // 隐藏摇杆
        m_joystickBackground.gameObject.SetActive(false);

        // 将手柄重置到中心
        m_joystickHandle.anchoredPosition = Vector2.zero;
    }
}
