using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Starscape.Simulation
{
    public delegate void StepStartHandler(SceneStepBase _step);
    public delegate void StepEndHandler(SceneStepBase _step);

    public abstract class SceneStepBase : MonoBehaviour
    {
        public event StepStartHandler OnStepStartEvent;
        public event StepEndHandler OnStepEndEvent;


        public string StepId => m_stepId;
        [TitleGroup("基础设置")][ShowIf("NoParent")]
        [SerializeField]
        protected string m_stepId;

        public string StepName => m_stepName;
        [TitleGroup("基础设置")][ShowIf("NoParent")]
        [SerializeField]
        protected string m_stepName;

        public bool IsDisplayStep => m_isDisplayStep;
        [TitleGroup("基础设置")][ShowIf("NoParent")]
        [SerializeField][PropertyTooltip("是否显示在步骤列表中")]
        private bool m_isDisplayStep = true;

        [TitleGroup("基础设置")]
        public int sceneZoneTypeId = 0;

        [TitleGroup("基础设置")][ShowIf("NoChild")]
        [SerializeField][PropertyTooltip("步骤开始时的提示信息, 步骤结束时不显示")]
        private string m_stepMessage;
        [TitleGroup("基础设置")]
        [SerializeField][ShowIf("@!string.IsNullOrEmpty(m_stepMessage)")]
        private Color m_stepMessageColor = Color.white;

        [TitleGroup("基础设置")]
        [SerializeField]
        private GameObject[] m_stepStartActiveSet;

        [TitleGroup("基础设置")][ShowIf("@m_stepStartActiveSet.Length > 0")]
        [SerializeField][PropertyTooltip("步骤结束时是否将步骤开始时激活的对象设为不激活")]
        private bool m_stepEndInactive = true;


        [TitleGroup("运行时")][HideInEditorMode]
        [ShowInInspector][ReadOnly]
        public bool IsRunning { get; private set; }
        [TitleGroup("运行时")][HideInEditorMode]
        [ShowInInspector][ReadOnly]
        public bool IsFinished => m_isStarted && !IsRunning;
        [TitleGroup("运行时")][HideInEditorMode]
        [ShowInInspector][ReadOnly]
        private bool m_isStarted;
        protected int StepMessageId => m_stepMessageId;
        [TitleGroup("运行时")][HideInEditorMode]
        [ShowInInspector][ReadOnly]
        private int m_stepMessageId;


        [TitleGroup("事件")]
        [SerializeField]
        private UnityEvent m_onStepStart, m_onStepEnd;

       

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void OnDestroy() { }
        protected virtual void Update() { }


        public void StepStart()
        {
            IsRunning = true;
            m_isStarted = true;
            foreach (var obj in m_stepStartActiveSet)
            {
                obj.SetActive(true);
            }
            OnStepStart();
            m_onStepStart?.Invoke();
            OnStepStartEvent?.Invoke(this);
            GameManager.Instance.SceneManager.RaiseOnStepStartEvent(this);
            if (!string.IsNullOrEmpty(m_stepMessage))
            {
                m_stepMessageId = GameManager.Instance.UIManager.Notify(m_stepMessage, m_stepMessageColor, -1f);
            }
        }

        public void StepEnd()
        {
            IsRunning = false;
            if (m_stepEndInactive)
            {
                foreach (var obj in m_stepStartActiveSet)
                {
                    obj.SetActive(false);
                }
            }
            OnStepEnd();
            m_onStepEnd?.Invoke();
            OnStepEndEvent?.Invoke(this);
            GameManager.Instance.SceneManager.RaiseOnStepEndEvent(this);
            if (m_stepMessageId != 0)
            {
                GameManager.Instance.UIManager.HideNotify(m_stepMessageId);
                m_stepMessageId = 0;
            }
        }

        public void Run_m_onStepEnd()
        {
            m_onStepEnd?.Invoke();
        }

        public void StepReset()
        {
            IsRunning = false;
            m_isStarted = false;
            if (m_stepMessageId != 0 && GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                GameManager.Instance.UIManager.HideNotify(m_stepMessageId);
                m_stepMessageId = 0;
            }
            OnStepReset();
        }

        public void StepResume()
        {
            IsRunning = true;
            m_isStarted = true;
            foreach (var obj in m_stepStartActiveSet)
            {
                obj.SetActive(true);
            }
            OnStepResume();
        }

        protected virtual void OnStepStart()
        {
        }

        protected virtual void OnStepEnd()
        {
        }

        protected virtual void OnStepReset()
        {
        }

        protected virtual void OnStepResume()
        {

        }

        protected bool NoParent()
        {
            // Editor 会使用, 需要每次都获取
            if (transform.parent == null)
            {
                return true;
            }
            return transform.parent.GetComponent<SceneStepBase>() == null;
        }

        protected bool NoChild()
        {
            // Editor 会使用, 需要每次都获取
            if (transform.childCount == 0)
            {
                return true;
            }
            return transform.GetComponentsInChildren<SceneStepBase>().Length == 0;
        }
    }
}
