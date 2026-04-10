using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Starscape.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Starscape.Simulation
{
    public delegate void SceneStepHandler(SceneStepBase _step);

    /// <summary>
    /// 场景管理基类
    /// </summary>
    public abstract class SceneManagerBase : MonoBehaviour
    {
        protected virtual void OnValidate()
        {
            CheckSteps();
        }

        protected virtual void Awake() { }

        protected virtual void Start()
        {
            if (m_playOnStart)
            {
                Invoke(nameof(AutoStartStep), 0.5f);
            }
        }
        protected virtual void OnDestroy() { }

        private void AutoStartStep()
        {
            StepStart(GetStepById(m_firstStepId));
        }


#region 步骤
        protected enum GetNextStepResult
        {
            Success,
            NoNextStep,
            CurrentStepNotFound,
            Finish,
        }

        public List<SceneStepBase> SceneStepSet => m_sceneStepSet;
        [BoxGroup("步骤")]
        [BoxGroup("步骤/基础配置")]
        [SerializeField]
        [InlineButton("CollectChildSteps", SdfIconType.ArrowClockwise, "")]
        protected List<SceneStepBase> m_sceneStepSet;
        [BoxGroup("步骤/基础配置")]
        [SerializeField]
        private bool m_playOnStart = true;
        [BoxGroup("步骤/基础配置")]
        [ShowIf("m_playOnStart")][ValueDropdown("GetStepIndexDropdown")]
        [SerializeField]
        private string m_firstStepId;
        [BoxGroup("步骤/基础配置")]
        [SerializeField]
        private bool m_isAutoNextStep = true;


        [BoxGroup("步骤/运行时")][HideInEditorMode]
        [ShowInInspector][ReadOnly]
        public SceneStepBase CurrentStep { get; private set; }

        public event SceneStepHandler OnStepStartEvent;
        public event SceneStepHandler OnStepEndEvent;

        public Transform safeZoneSpawnPoint;
        public Transform prepareZoneSpawnPoint;
        //当前step的zoneID的值
        public int currentStepZoneID;
        public PhysicsColliderInteractive door;
        /// <summary>
        /// 开始步骤
        /// </summary>
        /// <param name="_stepId"></param>
        public void StepStart([CanBeNull] string _stepId = null)
        {
            if (string.IsNullOrEmpty(_stepId))
            {
                if (m_sceneStepSet == null || m_sceneStepSet.Count == 0)
                {
                    Debug.LogWarning("SceneManagerBase: No steps available to start.");
                    return;
                }
                _stepId = m_sceneStepSet[0].name;
            }
            var step = GetStepById(_stepId);
           
            StartStep(step);
        }

        /// <summary>
        /// 开始步骤
        /// </summary>
        /// <param name="_step"></param>
        public void StepStart([CanBeNull] SceneStepBase _step)
        {
            if (_step == null)
            {
                if (m_sceneStepSet == null || m_sceneStepSet.Count == 0)
                {
                    Debug.LogWarning("SceneManagerBase: No steps available to start.");
                    return;
                }
                _step = m_sceneStepSet[0];
            }
            if (_step == null)
            {
                Debug.LogError("SceneManagerBase: Cannot start a null step.");
                return;
            }
            StartStep(_step);
        }

        /// <summary>
        /// 结束步骤
        /// </summary>
        public void StopStep()
        {
            if (CurrentStep == null)
            {
                Debug.LogError("当前没有运行中的步骤，无法结束");
                return;
            }
            StepStop();
        }

        /// <summary>
        /// 下个步骤
        /// </summary>
        /// <param name="_isForce">true: 强制结束当前未完成的步骤</param>
        /// <returns></returns>
        public bool NextStep([CanBeNull] string _stepId = null, bool _isForce = false)
        {
            if (CurrentStep == null)
            {
                StepStart(_stepId);
                return true;
            }
            GetNextStepResult nextStepResult;
            SceneStepBase nextStep;
            if (string.IsNullOrEmpty(_stepId))
            {
                nextStepResult = GetNextStep(CurrentStep, out nextStep);
            }
            else
            {
                nextStep = GetStepById(_stepId);
                nextStepResult = nextStep == null ? GetNextStepResult.NoNextStep : GetNextStepResult.Success;
            }
            if (nextStepResult == GetNextStepResult.CurrentStepNotFound || nextStepResult == GetNextStepResult.NoNextStep)
            {
                Debug.LogError($"找不到下一个步骤, 当前步骤: {(CurrentStep != null ? CurrentStep.name : "EMPTY")}");
                return false;
            }
            if (nextStepResult == GetNextStepResult.Finish)
            {
                Debug.Log("场景步骤已全部完成");
                return false;
            }
            if (CurrentStep != null)
            {
                if (_isForce)
                {
                    if (CurrentStep.IsRunning)
                    {
                        StepStop();
                    }
                }
                else
                {
                    if (CurrentStep.IsRunning)
                    {
                        Debug.LogWarning($"当前步骤未完成，无法切换到下一个步骤: {CurrentStep.StepId}");
                        return false;
                    }
                }
            }
            StartStep(nextStep);
            return true;
        }

        /// <summary>
        /// 跳转到指定步骤
        /// </summary>
        /// <param name="_step">目标步骤</param>
        /// <param name="_isForce">true: 强制结束当前运行中的步骤</param>
        /// <returns>true: 跳转成功</returns>
        public bool JumpToStep([CanBeNull] SceneStepBase _step, bool _isForce = false)
        {
            if (_step == null)
            {
                Debug.LogWarning("目标步骤为空，无法跳转。");
                return false;
            }

            if (CurrentStep == _step && CurrentStep != null && CurrentStep.IsRunning)
            {
                return true;
            }

            if (CurrentStep != null)
            {
                if (CurrentStep.IsRunning && !_isForce)
                {
                    Debug.LogWarning($"当前步骤未完成，无法跳转到步骤: {_step.StepId}");
                    return false;
                }

                // 先解绑，避免 StepEnd 触发自动 NextStep 干扰跳转。
                CurrentStep.OnStepStartEvent -= OStepStart;
                CurrentStep.OnStepEndEvent -= OnStepEnd;

                if (CurrentStep.IsRunning)
                {
                    Debug.Log($"结束步骤[{CurrentStep.StepName}]: {CurrentStep.StepId}");
                    CurrentStep.StepEnd();
                }

                CurrentStep = null;
            }

            ApplyJumpStepActiveState(_step);

            StartStep(_step);
            return true;
        }

        /// <summary>
        /// 跳转到指定步骤
        /// </summary>
        /// <param name="_stepId">目标步骤ID</param>
        /// <param name="_isForce">true: 强制结束当前运行中的步骤</param>
        /// <returns>true: 跳转成功</returns>
        public bool JumpToStep([CanBeNull] string _stepId, bool _isForce = false)
        {
            var step = GetStepById(_stepId);
            if (step == null)
            {
                Debug.LogWarning($"找不到目标步骤: {_stepId}");
                return false;
            }
            return JumpToStep(step, _isForce);
        }

        protected void StartStep(SceneStepBase _step)
        {
            if (_step == null)
            {
                Debug.LogWarning("SceneManagerBase: Cannot start a null step.");
                return;
            }
            Debug.Log($"开始步骤[{_step.StepName}]: {_step.StepId}");
            _step.StepStart();

            //door在不同zone之间切换时才开门
            if (currentStepZoneID != _step.sceneZoneTypeId)
            {
                door.PlayDoorOpen();
            }
            CurrentStep = _step;
            //设置当前步骤的zoneID，后续可以根据这个ID来判断玩家是否进入了新的区域
            currentStepZoneID = CurrentStep.sceneZoneTypeId;
            CurrentStep.OnStepStartEvent += OStepStart;
            CurrentStep.OnStepEndEvent += OnStepEnd;
        }

        protected void StepStop()
        {
            Debug.Log($"结束步骤[{CurrentStep.StepName}]: {CurrentStep.StepId}");
            CurrentStep.StepEnd();
            CurrentStep.OnStepStartEvent -= OStepStart;
            CurrentStep.OnStepEndEvent -= OnStepEnd;
            CurrentStep = null;
        }

        protected virtual GetNextStepResult GetNextStep(SceneStepBase _currentStep, out SceneStepBase _nextStep)
        {
            _nextStep = null;
            if (_currentStep == null)
            {
                if (m_sceneStepSet != null && m_sceneStepSet.Count > 0)
                {
                    _nextStep = m_sceneStepSet[0];
                    return GetNextStepResult.Success;
                }
                return GetNextStepResult.NoNextStep;
            }
            var currentIndex = m_sceneStepSet.FindIndex(_item => _item.StepId == _currentStep.StepId);
            if (currentIndex == -1)
            {
                Debug.LogError($"当前步骤未在步骤列表中找到: {_currentStep.StepId}");
                return GetNextStepResult.CurrentStepNotFound;
            }
            var nextIndex = currentIndex + 1;
            if (nextIndex >= m_sceneStepSet.Count)
            {
                return GetNextStepResult.Finish;
            }
            _nextStep = m_sceneStepSet[nextIndex];
            return GetNextStepResult.Success;
        }

        private void ApplyJumpStepActiveState(SceneStepBase _targetStep)
        {
            if(CurrentStep == _targetStep)
            {
                return;
            }
            if (_targetStep == null || m_sceneStepSet == null || m_sceneStepSet.Count == 0)
            {
                return;
            }

            var targetIndex = m_sceneStepSet.FindIndex(_item => _item == _targetStep);
            if (targetIndex == -1)
            {
                targetIndex = m_sceneStepSet.FindIndex(_item => _item != null && _item.StepId == _targetStep.StepId);
            }
            if (targetIndex == -1)
            {
                Debug.LogWarning($"目标步骤未在步骤列表中找到，跳转状态同步已跳过: {_targetStep.StepId}");
                return;
            }

            for (var i = 0; i < m_sceneStepSet.Count; i++)
            {
                var step = m_sceneStepSet[i];
                if (step == null )
                {
                    continue;
                }

                if (i < targetIndex)
                {
                    step.ApplyStepEndActiveSet();
                }
                else
                {
                    step.ApplyBeforeStepStartActiveSet();
                }
            }
        }

        /// <summary>
        /// true: 当前步骤正在运行
        /// </summary>
        /// <returns></returns>
        public bool IsStepRunning()
        {
            if (CurrentStep == null)
            {
                return false;
            }
            return CurrentStep.IsRunning;
        }

        /// <summary>
        /// true: 指定步骤正在运行
        /// </summary>
        /// <param name="_stepId"></param>
        /// <returns></returns>
        public bool IsStepRunning(string _stepId)
        {
            var step = GetStepById(_stepId);
            if (step == null)
            {
                Debug.LogWarning($"SceneManagerBase: Step with ID {_stepId} not found.");
                return false;
            }
            return step.IsRunning;
        }

        /// <summary>
        /// true: 当前步骤已完成
        /// </summary>
        /// <returns></returns>
        public bool IsStepFinished()
        {
            if (CurrentStep == null)
            {
                return false;
            }
            return CurrentStep.IsFinished;
        }

        /// <summary>
        /// true: 指定步骤已完成
        /// </summary>
        /// <param name="_stepId"></param>
        /// <returns></returns>
        public bool IsStepFinished(string _stepId)
        {
            var step = GetStepById(_stepId);
            if (step == null)
            {
                Debug.LogWarning($"SceneManagerBase: Step with ID {_stepId} not found.");
                return false;
            }
            return step.IsFinished;
        }

        /// <summary>
        /// 获取指定步骤
        /// </summary>
        /// <param name="_stepId"></param>
        /// <returns></returns>
        public SceneStepBase GetStepById([CanBeNull] string _stepId)
        {
            if (string.IsNullOrEmpty(_stepId)) return null;
            return m_sceneStepSet.Find(_item => _item != null && _item.StepId == _stepId);
        }

        protected virtual void OStepStart(SceneStepBase _step) { }

        protected virtual void OnStepEnd(SceneStepBase _step)
        {
            if (m_isAutoNextStep)
            {
                NextStep();
            }
        }

        private IEnumerable<ValueDropdownItem<string>> GetStepIndexDropdown()
        {
            if (m_sceneStepSet == null)
            {
                yield break;
            }
            foreach (var step in m_sceneStepSet)
            {
                if (step != null)
                {
                    var stepId = string.IsNullOrEmpty(step.StepId) ? $"EMPTY_ID" : step.StepId;
                    yield return new ValueDropdownItem<string>($"{stepId}: {step.StepName}", step.StepId);
                }
            }
        }

        private void CollectChildSteps()
        {
            m_sceneStepSet.Clear();
            foreach (Transform child in transform)
            {
                var step = child.GetComponent<SceneStepBase>();
                if (step != null)
                {
                    if (!step.gameObject.activeSelf) continue;
                    m_sceneStepSet.Add(step);
                }
            }
            CheckSteps();
        }

        private void CheckSteps()
        {
            var stepIdSet = new HashSet<string>();
            foreach (var step in m_sceneStepSet)
            {
                if (step == null) continue;
                if (!stepIdSet.Add(step.StepId))
                {
                    Debug.LogError($"SceneManagerBase: 重复的StepID: {step.StepId} in step {step.name}");
                }
            }
        }

        internal void RaiseOnStepStartEvent(SceneStepBase _step)
        {
            OnStepStartEvent?.Invoke(_step);
        }

        internal void RaiseOnStepEndEvent(SceneStepBase _step)
        {
            OnStepEndEvent?.Invoke(_step);
        }

#endregion


#region 通知
        [System.Serializable]
        private struct NotifyData
        {
            public enum Type
            {
                Notify,
                Warning,
            }

            public Type NotifyType;
            public string Id;
            public string Message;
            public float Duration;
        }

        [BoxGroup("通知")]
        [SerializeField]
        private List<NotifyData> m_notifyDataSet;

        /// <summary>配置项 Id → <see cref="UIManager.Notify"/> 返回的运行时 id（用于按 Id 关闭）</summary>
        private readonly Dictionary<string, int> m_notifyRuntimeIdByDataId = new();

        public void Notify(string _id)
        {
            var index = m_notifyDataSet.FindIndex(_item => _item.Id == _id);
            if (index == -1)
            {
                return;
            }
            var data = m_notifyDataSet[index];
            if (data.NotifyType == NotifyData.Type.Warning)
            {
                GameManager.Instance.UIManager.Warning(data.Message, data.Duration);
            }
            else if (data.NotifyType == NotifyData.Type.Notify)
            {
                if (m_notifyRuntimeIdByDataId.TryGetValue(_id, out var oldRuntimeId))
                {
                    GameManager.Instance.UIManager.HideNotify(oldRuntimeId);
                }
                Debug.Log(_id);
                var runtimeId = GameManager.Instance.UIManager.Notify(data.Message, Color.white, data.Duration);
                m_notifyRuntimeIdByDataId[_id] = runtimeId;
            }
        }

        /// <summary>
        /// 按配置项 Id 关闭对应通知（普通 Notify 用运行时 id；Warning 则关闭警告条）
        /// </summary>
        public void HideNotify(string _id)
        {
            if (string.IsNullOrEmpty(_id))
            {
                return;
            }
            var index = m_notifyDataSet.FindIndex(_item => _item.Id == _id);
            if (index == -1)
            {
                return;
            }
            var data = m_notifyDataSet[index];
            if (data.NotifyType == NotifyData.Type.Warning)
            {
                GameManager.Instance.UIManager.HideWarning();
            }
            else if (data.NotifyType == NotifyData.Type.Notify)
            {
                if (m_notifyRuntimeIdByDataId.TryGetValue(_id, out var runtimeId))
                {
                    
                    GameManager.Instance.UIManager.HideNotify(runtimeId);
                    m_notifyRuntimeIdByDataId.Remove(_id);
                }
            }
        }
#endregion
    }
}
