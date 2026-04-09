using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCameraRotate : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 InputVector { get; private set; }
    [SerializeField]
    private float m_scale = 0.1f;
    [SerializeField]
    private float m_deadZone = 0.1f;

    private Vector2 m_startPosition;
    public void OnPointerDown(PointerEventData eventData)
    {
        m_startPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var direction = eventData.position - m_startPosition;
        var distance = direction.magnitude;
        if (distance < m_deadZone) return;
        InputVector = direction * m_scale;
        m_startPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;
    }

    private void LateUpdate()
    {
        InputVector = Vector2.zero;
    }
}
