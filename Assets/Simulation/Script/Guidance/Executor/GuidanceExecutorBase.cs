using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Starscape.Simulation
{
    /// <summary>
    /// 指导执行器基类
    /// </summary>
    public class GuidanceExecutorBase : MonoBehaviour
    {
        public bool IsRequired => m_isRequired;
        [TitleGroup("基础设置")]
        [SerializeField]
        private bool m_isRequired = true;

        protected GuidanceStepBase Step => m_step;
        [TitleGroup("基础设置")]
        [SerializeField]
        private GuidanceStepBase m_step;

        [TitleGroup("事件")]
        [SerializeField]
        private UnityEvent m_onComplete;

        protected bool IsExecuting => m_isExecuting;
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private bool m_isExecuting;

        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private bool m_isCompleted;

        protected virtual void OnValidate()
        {
            if (m_step == null) m_step = GetComponentInParent<GuidanceStepBase>(true);
        }

        protected virtual void Awake()
        {
            if (m_step == null) m_step = GetComponentInParent<GuidanceStepBase>(true);
        }

        internal void ExecutorStart()
        {
            if (m_isExecuting)
            {
                return;
            }

            m_isExecuting = true;
            OnExecutorStart();
        }

        internal void ExecutorEnd()
        {
            if (!m_isExecuting)
            {
                return;
            }

            m_isExecuting = false;
            OnExecutorEnd();
        }

        internal void ExecutorReset()
        {
            m_isExecuting = false;
            OnExecutorReset();
            m_isCompleted = false;
        }

        protected void ReportSuccess()
        {
            Step.HandleExecutorSuccess(this);
            m_onComplete?.Invoke();
            m_isCompleted = true;
        }

        protected virtual void OnExecutorStart() { }
        protected virtual void OnExecutorEnd() { }
        protected virtual void OnExecutorReset() { }
    }
}
