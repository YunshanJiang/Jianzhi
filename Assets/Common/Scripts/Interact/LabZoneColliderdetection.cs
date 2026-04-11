using Sirenix.OdinInspector;
using Starscape.Simulation;
using UnityEngine;

namespace Starscape.Common
{
    /// <summary>
    /// 实验室区域碰撞检测：目标Tag在区域内为true，否则为false。
    /// </summary>
    public class LabZoneColliderdetection : MonoBehaviour
    {
        [TitleGroup("PhysicsCollider")]
        [SerializeField]
        [ValueDropdown("@Starscape.Common.Utils.GetAllTags()")]
        private string m_targetTag;

        private void SetPlayerInLab(bool _isInLab)
        {
            if (GameManager.Instance == null || GameManager.Instance.Player == null)
            {
                return;
            }

            GameManager.Instance.Player.SetIsInLab(_isInLab);
        }

        ///------------------------------------- 3D部分 -------------------------------------///
        private void OnTriggerStay(Collider _other) => ColliderStay(_other);
        private void OnTriggerEnter(Collider _other) => ColliderEnter(_other);
        private void OnTriggerExit(Collider _other) => ColliderExit(_other);
        private void OnCollisionStay(Collision _other) => ColliderStay(_other.collider);
        private void OnCollisionExit(Collision _other) => ColliderExit(_other.collider);


        private void ColliderStay(Collider _collider)
        {
           // SetPlayerInLab(_collider.CompareTag(m_targetTag));
        }
        private void ColliderEnter(Collider _collider)
        {
            SetPlayerInLab(_collider.CompareTag(m_targetTag));
        }
        private void ColliderExit(Collider _collider)
        {
            if (_collider.CompareTag(m_targetTag))
            {
                SetPlayerInLab(false);
            }
        }

        ///------------------------------------- 2D部分 -------------------------------------///
        private void OnTriggerStay2D(Collider2D _other) => Collider2DStay(_other);
        private void OnTriggerExit2D(Collider2D _other) => Collider2DExit(_other);
        private void OnCollisionStay2D(Collision2D _other) => Collider2DStay(_other.collider);
        private void OnCollisionExit2D(Collision2D _other) => Collider2DExit(_other.collider);

        private void Collider2DStay(Collider2D _collider)
        {
           // SetPlayerInLab(_collider.CompareTag(m_targetTag));
        }

        private void Collider2DExit(Collider2D _collider)
        {
            //if (_collider.CompareTag(m_targetTag))
          //  {
          //      SetPlayerInLab(false);
          //  }
        }
    }
}
