using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Starscape.Simulation
{
    /// <summary>
    /// 指导步骤
    /// </summary>
    public class GuidanceStepBase : MonoBehaviour
    {
        public string Id => m_id;
        [TitleGroup("基础设置")]
        [SerializeField]
        private string m_id;

        [TitleGroup("基础设置")]
        [SerializeField]
        private string m_stepMessage;
        [TitleGroup("基础设置")]
        [SerializeField][ShowIf("@!string.IsNullOrEmpty(m_stepMessage)")]
        private Color m_stepMessageColor = Color.white;

        [TitleGroup("基础设置")]
        [SerializeField]
        private GameObject m_root;

        [TitleGroup("基础设置")]
        [SerializeField]
        private bool m_isSkippable;

        [TitleGroup("执行器")][InlineButton("CollectExecutors", SdfIconType.ArrowClockwise, "")]
        [SerializeField]
        private List<GuidanceExecutorBase> m_executors;

        [TitleGroup("事件")]
        [SerializeField]
        private UnityEvent m_onStepStart, m_onStepEnd, m_onFailure;

        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        public bool IsRunning => m_isRunning;
        private bool m_isRunning;

        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        public bool IsCompleted => m_isCompleted;
        private bool m_isCompleted;

        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        public int StepMessageId => m_stepMessageId;
        private int m_stepMessageId;

        public bool IsSkippable => m_isSkippable;
        private readonly HashSet<GuidanceExecutorBase> m_completedExecutors = new();
        private bool m_resultDispatched;

        protected virtual void OnValidate()
        {
            if (m_root == null) m_root = transform.GetChild(0).gameObject;
        }

        internal void Initialize()
        {
            OnInitialized();
        }

        public void StartStep()
        {
            if (m_isRunning)
            {
                return;
            }
            m_root.SetActive(true);

            m_isRunning = true;
            m_isCompleted = false;
            m_resultDispatched = false;
            m_completedExecutors.Clear();
            ExecutorsStart();
            m_onStepStart?.Invoke();
            OnStepStart();
            if (!string.IsNullOrEmpty(m_stepMessage))
            {
                m_stepMessageId = GameManager.Instance.UIManager.Notify(m_stepMessage, m_stepMessageColor, -1f);
            }
        }

        public void StepEnd()
        {
            if (!m_isRunning)
            {
                return;
            }
            m_root.SetActive(false);

            m_isRunning = false;
            m_isCompleted = true;
            m_resultDispatched = true;
            ExecutorEnd();
            m_onStepEnd?.Invoke();
            OnStepEnd();
            if (m_stepMessageId != 0)
            {
                GameManager.Instance.UIManager.HideNotify(m_stepMessageId);
                m_stepMessageId = 0;
            }
        }

        public void ResetStep()
        {
            m_root.SetActive(false);
            m_isRunning = false;
            m_isCompleted = false;
            m_resultDispatched = false;
            m_completedExecutors.Clear();
            ExecutorReset();
            OnStepReset();
        }

        public void SkipStep()
        {
            if (!IsSkippable)
            {
                return;
            }

            var manager = GameManager.Instance.GuidanceManager;
            manager.NextStep(manager.FindNextStep(this));
        }

        internal void HandleExecutorSuccess(GuidanceExecutorBase _executor)
        {
            if (m_resultDispatched)
            {
                return;
            }

            if (_executor != null)
            {
                m_completedExecutors.Add(_executor);
            }

            OnExecutorSuccess(_executor);

            if (AreRequiredExecutorsComplete())
            {
                m_resultDispatched = true;
                GameManager.Instance.GuidanceManager.OnStepSucceeded(this);
            }
        }

        private void ExecutorsStart()
        {
            if (m_executors == null)
            {
                return;
            }

            foreach (var executor in m_executors)
            {
                executor?.ExecutorStart();
            }
        }

        private void ExecutorEnd()
        {
            if (m_executors == null)
            {
                return;
            }

            foreach (var executor in m_executors)
            {
                executor?.ExecutorEnd();
            }
        }

        private void ExecutorReset()
        {
            if (m_executors == null)
            {
                return;
            }

            foreach (var executor in m_executors)
            {
                executor?.ExecutorReset();
            }
        }

        private bool AreRequiredExecutorsComplete()
        {
            if (m_executors == null || m_executors.Count == 0)
            {
                return true;
            }

            foreach (var executor in m_executors)
            {
                if (executor == null || !executor.IsRequired)
                {
                    continue;
                }

                if (!m_completedExecutors.Contains(executor))
                {
                    return false;
                }
            }

            return true;
        }

        private void CollectExecutors()
        {
            m_executors.Clear();
            m_executors.AddRange(GetComponentsInChildren<GuidanceExecutorBase>(true));
        }

        protected virtual void OnStepStart() { }
        protected virtual void OnStepEnd() { }
        protected virtual void OnStepReset() { }

        protected virtual void OnInitialized() { }
        protected virtual void OnExecutorSuccess(GuidanceExecutorBase _executor) { }
    }
}
