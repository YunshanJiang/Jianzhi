using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Starscape.Simulation
{
    public delegate void GuidanceStepHandler(GuidanceStepBase _step);

    /// <summary>
    /// 指导管理器
    /// </summary>
    public class GuidanceManager : MonoBehaviour
    {
        [TitleGroup("通用")]
        [SerializeField][InlineButton("CollectSteps", SdfIconType.ArrowClockwise, "")]
        private List<GuidanceStepBase> m_steps = new();
        [TitleGroup("通用")]
        [SerializeField]
        private Transform m_stepsParent;

        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        public GuidanceStepBase CurrentStep { get; private set; }
        public GuidanceStepBase EndStep { get; private set; }
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private bool m_hasStarted;


        public event GuidanceStepHandler OnStarted;
        public event GuidanceStepHandler OnCompleted;
        public event GuidanceStepHandler OnStepChanged;

        private readonly Dictionary<string, GuidanceStepBase> m_stepLookup = new();

        private void Awake()
        {
            BuildLookup();
            InitializeSteps();
        }

        private void BuildLookup()
        {
            m_stepLookup.Clear();
            foreach (var step in m_steps)
            {
                if (step == null || string.IsNullOrEmpty(step.Id))
                {
                    continue;
                }

                if (!m_stepLookup.ContainsKey(step.Id))
                {
                    m_stepLookup.Add(step.Id, step);
                }
            }
        }

        private void InitializeSteps()
        {
            foreach (var step in m_steps)
            {
                step?.Initialize();
            }
        }

        public GuidanceStepBase FindStepById(string _id)
        {
            if (string.IsNullOrEmpty(_id))
            {
                return null;
            }

            m_stepLookup.TryGetValue(_id, out var step);
            return step;
        }

        public void StartStep(string _id, string _endId = null)
        {
            var step = FindStepById(_id);
            var endStep = FindStepById(_endId);
            if (step == null)
            {
                return;
            }
            m_stepsParent.gameObject.SetActive(true);

            m_hasStarted = true;
            OnStarted?.Invoke(step);
            SetCurrentStep(step);
            SetEndStep(endStep);
        }

        public GuidanceStepBase FindNextStep(GuidanceStepBase _step)
        {
            if (_step == null)
            {
                return null;
            }

            var index = m_steps.IndexOf(_step);
            if (index < 0)
            {
                return null;
            }

            var nextIndex = index + 1;
            if (nextIndex >= m_steps.Count)
            {
                return null;
            }

            return m_steps[nextIndex];
        }

        public void NextStep(GuidanceStepBase _nextStep)
        {
            if (_nextStep == null || _nextStep == EndStep)
            {
                CompleteSequence();
                return;
            }

            SetCurrentStep(_nextStep);
        }

        public void StopStep()
        {
            if (!m_hasStarted) return;
            m_stepsParent.gameObject.SetActive(false);
            CurrentStep?.StepEnd();
            m_hasStarted = false;
            CurrentStep = null;
        }

        public void RestartCurrentStep()
        {
            CurrentStep?.ResetStep();
            CurrentStep?.StartStep();
        }

        public void SkipCurrentStep()
        {
            CurrentStep?.SkipStep();
        }

        internal void OnStepSucceeded(GuidanceStepBase _step)
        {
            if (_step != CurrentStep)
            {
                return;
            }

            _step.StepEnd();
            NextStep(FindNextStep(_step));
        }

        private void SetCurrentStep(GuidanceStepBase _step)
        {
            if (CurrentStep == _step)
            {
                return;
            }

            CurrentStep?.StepEnd();
            CurrentStep = _step;
            CurrentStep?.StartStep();
            OnStepChanged?.Invoke(CurrentStep);
        }
        private void SetEndStep(GuidanceStepBase _step)
        {
            EndStep = _step;

        }

        private void CompleteSequence()
        {
            if (!m_hasStarted)
            {
                return;
            }
            Debug.Log("step end");
            m_stepsParent.gameObject.SetActive(false);
            m_hasStarted = false;
            CurrentStep = null;
            OnCompleted?.Invoke(null);
        }

        private void CollectSteps()
        {
            m_steps.Clear();
            if (m_stepsParent == null)
            {
                return;
            }

            foreach (Transform child in m_stepsParent)
            {
                var step = child.GetComponent<GuidanceStepBase>();
                if (step != null)
                {
                    m_steps.Add(step);
                }
            }
        }

        public IEnumerable<ValueDropdownItem<string>> GetStepIdDropdown()
        {
            foreach (var step in m_steps)
            {
                if (step == null || string.IsNullOrEmpty(step.Id)) continue;
                yield return new ValueDropdownItem<string>(step.Id, step.Id);
            }
        }
    }
}
