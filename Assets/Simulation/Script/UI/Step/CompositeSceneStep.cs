using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    public class CompositeSceneStep : SceneStepBase
    {
        private enum CompositeType
        {
            Sequential,
            Parallel,
            Random,
        }

        private enum CompleteCondition
        {
            All,
            Any,
        }

        [TitleGroup("Composite")]
        [SerializeField]
        private CompositeType m_compositeType = CompositeType.Sequential;
        [TitleGroup("Composite")]
        [SerializeField]
        private CompleteCondition m_completeCondition = CompleteCondition.All;
        [TitleGroup("Composite")]
        [SerializeField][InlineButton("CollectChildSteps", SdfIconType.ArrowClockwise, "")]
        private List<SceneStepBase> m_sceneStepSet;
        public IReadOnlyList<SceneStepBase> ChildSteps => m_sceneStepSet;
        private int m_completedStepCount = 0;
        private readonly List<SceneStepBase> m_runtimeSteps = new List<SceneStepBase>();
        private readonly Queue<SceneStepBase> m_waitingSteps = new Queue<SceneStepBase>();
        private readonly HashSet<SceneStepBase> m_activeSteps = new HashSet<SceneStepBase>();
        private bool m_isFinishing;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            PrepareSteps();

            if (m_runtimeSteps.Count == 0)
            {
                StepEnd();
                return;
            }

            switch (m_compositeType)
            {
                case CompositeType.Sequential:
                case CompositeType.Random:
                    StartNextQueuedStep();
                    break;
                case CompositeType.Parallel:
                    StartParallelSteps();
                    break;
            }
        }

        protected override void OnStepEnd()
        {
            base.OnStepEnd();
            //CancelRunningChildren();
            //CleanupChildren();
            CancelPendingWork();
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            CancelPendingWork();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CancelPendingWork();
        }

        private void PrepareSteps()
        {
            m_completedStepCount = 0;
            m_isFinishing = false;
            m_runtimeSteps.Clear();
            m_waitingSteps.Clear();
            m_activeSteps.Clear();

            if (m_sceneStepSet == null)
            {
                return;
            }

            foreach (var step in m_sceneStepSet)
            {
                if (step == null || step == this)
                {
                    continue;
                }
                setChildsZoneID(step);
                m_runtimeSteps.Add(step);
            }

            foreach (var step in m_runtimeSteps)
            {
                step.OnStepEndEvent -= HandleChildStepEnded;
                step.OnStepEndEvent += HandleChildStepEnded;
            }

            if (m_compositeType == CompositeType.Sequential || m_compositeType == CompositeType.Random)
            {
                if (m_compositeType == CompositeType.Random)
                {
                    Shuffle(m_runtimeSteps);
                }

                foreach (var step in m_runtimeSteps)
                {
                    m_waitingSteps.Enqueue(step);
                }
            }
        }

        public void setChildsZoneID(SceneStepBase step)
        {
            step.sceneZoneTypeId = sceneZoneTypeId;
        }
        private void StartParallelSteps()
        {
            foreach (var step in m_runtimeSteps)
            {
                StartChildStep(step);
            }
        }

        private void StartNextQueuedStep()
        {
            if (m_waitingSteps.Count == 0)
            {
                return;
            }

            StartChildStep(m_waitingSteps.Dequeue());
        }

        private void StartChildStep(SceneStepBase step)
        {
            if (step == null || m_activeSteps.Contains(step))
            {
                return;
            }

            m_activeSteps.Add(step);
            step.StepStart();
            Debug.Log($"{StepName}[{step.name}] 开始");
        }

        private void HandleChildStepEnded(SceneStepBase step)
        {
            if (!m_activeSteps.Remove(step))
            {
                return;
            }
            Debug.Log($"{StepName}[{step.name}] 结束");
            m_completedStepCount++;
            if (HasCompleted())
            {
                return;
            }

            if (m_compositeType == CompositeType.Sequential || m_compositeType == CompositeType.Random)
            {
                StartNextQueuedStep();
            }
        }

        private bool HasCompleted()
        {
            if (m_completeCondition == CompleteCondition.All)
            {
                if (m_completedStepCount < m_runtimeSteps.Count)
                {
                    return false;
                }
            }

            FinishComposite();
            return true;
        }

        private void FinishComposite()
        {
            if (m_isFinishing)
            {
                return;
            }

            m_isFinishing = true;
            CancelRunningChildren();
            StepEnd();
        }

        private void CancelRunningChildren()
        {
            while (m_activeSteps.Count > 0)
            {
                SceneStepBase step = null;
                var enumerator = m_activeSteps.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    step = enumerator.Current;
                }

                if (step == null)
                {
                    break;
                }

                m_activeSteps.Remove(step);
                // 必须始终 StepReset：子步骤在 StepEnd 开头会把 IsRunning 置 false，
                // 若仍留在 m_activeSteps 中，旧逻辑会跳过 StepReset，导致 Notify 无法 Hide。
                step.StepReset();
            }

            m_waitingSteps.Clear();
        }

        private void CleanupChildren()
        {
            foreach (var step in m_runtimeSteps)
            {
                if (step == null)
                {
                    continue;
                }

                step.OnStepEndEvent -= HandleChildStepEnded;
            }

            m_runtimeSteps.Clear();
        }

        private void CancelPendingWork()
        {
            CancelRunningChildren();
            foreach (var step in GetComponentsInChildren<SceneStepBase>(true))
            {
                if (step != null && step != this)
                {
                    step.ForceHideStepMessage();
                }
            }
            CleanupChildren();
            m_isFinishing = false;
            m_completedStepCount = 0;
        }

        private static void Shuffle(List<SceneStepBase> steps)
        {
            for (int i = steps.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                (steps[i], steps[swapIndex]) = (steps[swapIndex], steps[i]);
            }
        }

        private void CollectChildSteps()
        {
            if (m_sceneStepSet == null)
            {
                m_sceneStepSet = new List<SceneStepBase>();
            }
            else
            {
                m_sceneStepSet.Clear();
            }

            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf) continue;
                var step = child.GetComponent<SceneStepBase>();
                m_sceneStepSet.Add(step);
            }
        }
    }
}
