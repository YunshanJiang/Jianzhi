using UnityEngine;

namespace Starscape.Simulation
{
    [ExecuteAlways]
    public class FixedViewControl : MonoBehaviour
    {
        [SerializeField]
        private Transform m_fixedPosition;
        [SerializeField]
        private Transform m_fixedLookAt;
        [SerializeField]
        private Camera m_camera;

        private void LateUpdate()
        {
            if (m_camera != null && m_fixedPosition != null && m_fixedLookAt != null)
            {
                m_camera.transform.position = m_fixedPosition.position;
                m_camera.transform.rotation = Quaternion.LookRotation(m_fixedLookAt.position - m_fixedPosition.position);
            }
        }

        private void OnDrawGizmos()
        {
            if (m_fixedPosition != null && m_fixedLookAt != null)
            {
                Debug.DrawLine(m_fixedPosition.position, m_fixedLookAt.position, Color.red);
            }
        }
    }
}
