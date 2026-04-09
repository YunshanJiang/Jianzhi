using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 旋转物体
    /// </summary>
    public class RotateObject : MonoBehaviour
    {
        [SerializeField]
        private float m_rotateSpeed = 10;
        [SerializeField]
        private Vector3 m_axis = Vector3.up;
        [SerializeField]
        private bool m_isValidRotate = true;

        private void Update()
        {
            if (!m_isValidRotate) return;
            transform.Rotate(m_axis, m_rotateSpeed * Time.deltaTime);
        }

        public void SetRotateValid(bool _isValid)
        {
            m_isValidRotate = _isValid;
        }
    }
}
