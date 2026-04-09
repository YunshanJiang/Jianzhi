using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Starscape.Common
{
    /// <summary>
    /// 物理碰撞交互组件
    /// </summary>
    public class PhysicsColliderInteractive : InteractiveBase
    {
        [TitleGroup("PhysicsCollider")]
        [SerializeField][ValueDropdown("@Starscape.Common.Utils.GetAllTags()")]
        private string m_targetTag;
        [TitleGroup("事件")]
        [SerializeField]
        private UnityEvent m_doorClose, m_doorOpen;
        [TitleGroup("Door")]
        [SerializeField]
        private bool m_isDoorOpen;

        protected override void OnInteract()
        {
            m_isDoorOpen = !m_isDoorOpen;
            // 按需求映射：门开时触发Close事件，门关时触发Open事件。
            if (m_isDoorOpen)
            {
                m_doorClose?.Invoke();
            }
            else
            {
                m_doorOpen?.Invoke();
            }
            //SetPromptVisible(true);
        }
        public void PlayDoorOpen()
        {
            if(!m_isDoorOpen)
            {
                m_doorOpen?.Invoke();
            }
            m_isDoorOpen = true;
            
        }
        ///------------------------------------- 3D部分 -------------------------------------///
        private void OnTriggerEnter(Collider _other) => ColliderEnter(_other);
        private void OnTriggerExit(Collider _other) => ColliderExit(_other);
        private void OnCollisionEnter(Collision _other) => ColliderEnter(_other.collider);
        private void OnCollisionExit(Collision _other) => ColliderExit(_other.collider);

        private void ColliderEnter(Collider _collider)
        {
            if (_collider.CompareTag(m_targetTag))
            {
                SetPromptVisible(true);
            }
        }

        private void ColliderExit(Collider _collider)
        {
            if (_collider.CompareTag(m_targetTag))
            {
                SetPromptVisible(false);
            }
        }


        ///------------------------------------- 2D部分 -------------------------------------///
        private void OnTriggerEnter2D(Collider2D _other) => Collider2DEnter(_other);
        private void OnTriggerExit2D(Collider2D _other) => Collider2DExit(_other);
        private void OnCollisionEnter2D(Collision2D _other) => Collider2DEnter(_other.collider);
        private void OnCollisionExit2D(Collision2D _other) => Collider2DExit(_other.collider);

        private void Collider2DEnter(Collider2D _collider)
        {
            if (_collider.CompareTag(m_targetTag))
            {
                SetPromptVisible(true);
            }
        }

        private void Collider2DExit(Collider2D _collider)
        {
            if (_collider.CompareTag(m_targetTag))
            {
                SetPromptVisible(false);
            }
        }
    }
}
