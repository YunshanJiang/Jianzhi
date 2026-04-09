using Sirenix.OdinInspector;
using Starscape.Simulation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Starscape.Common
{
    public delegate void InteractHandler(InteractiveBase _object);

    /// <summary>
    /// 交互基类
    /// </summary>
    public abstract class InteractiveBase : MonoBehaviour
    {
        public bool CanInteract => m_canInteract;
        [TitleGroup("通用")]
        [SerializeField]
        private bool m_canInteract = true;

        public string Content => m_content;
        [TitleGroup("通用")]
        [SerializeField]
        private string m_content;

        public InputActionReference InteractAction => m_interactAction;
        [TitleGroup("通用")]
        [SerializeField]
        private InputActionReference m_interactAction;

        public Transform PromptPosition => m_promptPosition;
        [TitleGroup("通用")]
        [SerializeField]
        private Transform m_promptPosition;

        [TitleGroup("通用")]
        [SerializeField]
        private bool m_locationTip = true;

        public float IndicatorDistance => m_indicatorDistance;
        [TitleGroup("位置提示")]
        [SerializeField]
        private float m_indicatorDistance = 100f;

        public event InteractHandler OnInteractEvent;
        [TitleGroup("事件")]
        [SerializeField]
        private UnityEvent m_onInteract;

        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        public bool IsValidInteract => m_isValidInteract;
        private bool m_isValidInteract;
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private InteractiveUI m_currentUI;

        protected virtual void Awake()
        {
            m_interactAction.action.performed += OnPerformed;
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable()
        {
            if (m_currentUI != null)
            {
                GameManager.Instance.InteractiveManager.HideInteractPrompt(m_currentUI);
                m_currentUI = null;
            }
        }

        protected virtual void Update()
        {
            if (GameManager.Instance.Player == null) return;
            Vector3 targetPosition = m_promptPosition.position;
            var playerPosition = GameManager.Instance.Player.transform.position;
            float distance = Vector3.Distance(targetPosition, playerPosition);
            bool shouldShow = m_isValidInteract || (m_locationTip && distance < m_indicatorDistance);

            if (shouldShow)
            {
                if (m_currentUI == null)
                {
                    m_currentUI = GameManager.Instance.InteractiveManager.ShowInteractPrompt(this);
                }
                m_currentUI.SetInteractState(m_isValidInteract);
            }
            else
            {
                if (m_currentUI != null)
                {
                    GameManager.Instance.InteractiveManager.HideInteractPrompt(m_currentUI);
                    m_currentUI = null;
                }
            }
        }

        /// <summary>
        /// 设置提示可见性
        /// </summary>
        /// <param name="_visible"></param>
        public virtual void SetPromptVisible(bool _visible)
        {
            m_isValidInteract = _visible;
        }

        /// <summary>
        /// 尝试交互
        /// </summary>
        public virtual void TryInteract()
        {
            if (!m_canInteract) return;
            if (!m_isValidInteract) return;
            OnInteract();
            OnInteractEvent?.Invoke(this);
            // Debug.Log("Interacted with " + name);
            m_isValidInteract = false;
        }

        /// <summary>
        /// 设置可交互状态
        /// </summary>
        /// <param name="_value"></param>
        public void SetCanInteract(bool _value) => m_canInteract = _value;

        private void OnPerformed(InputAction.CallbackContext _obj)
        {
            TryInteract();
        }

        protected virtual void OnInteract()
        {

        }

        public void SetContent(string _content)
        {
            m_content = _content;
        }

        private void OnDrawGizmos()
        {
            if (m_promptPosition != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_promptPosition.position, 0.02f);
            }
        }

        /// <summary>
        /// 设置位置提示距离
        /// </summary>
        /// <param name="_indicatorDistance"></param>
        public void SetIndicatorDistance(float _indicatorDistance)
        {
            m_indicatorDistance = _indicatorDistance;
        }
    }
}
