using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 指导-场景步骤
    /// </summary>
    public class GuidanceSceneStep : SceneStepBase
    {
        [TitleGroup("Guidance")]
        [ValueDropdown("GetGuidanceStepIds")]
        [SerializeField]
        private string m_guidanceId;

        [TitleGroup("Guidance")]
        [ValueDropdown("GetGuidanceEndStepIds")]
        [SerializeField]
        private string m_guidanceEndId;
        protected override void OnStepStart()
        {
            base.OnStepStart();
            GameManager.Instance.GuidanceManager.OnCompleted += OnCompleted;
            GameManager.Instance.GuidanceManager.StartStep(m_guidanceId, m_guidanceEndId);
            GameManager.Instance.UIManager.SetFGVisble(false);
        }

        protected override void OnStepEnd()
        {
            base.OnStepEnd();
            GameManager.Instance.GuidanceManager.OnCompleted -= OnCompleted;
            GameManager.Instance.GuidanceManager.StopStep();
            GameManager.Instance.UIManager.SetFGVisble(true);
        }
        protected override void OnStepReset()
        {
            base.OnStepReset();
            StepEnd();
        }
        private void OnCompleted(GuidanceStepBase _step)
        {
            StepEnd();
        }

        private IEnumerable<ValueDropdownItem<string>> GetGuidanceStepIds()
        {
            var guidanceManager = FindFirstObjectByType<GuidanceManager>();
            if (guidanceManager == null)
            {
                return Array.Empty<ValueDropdownItem<string>>();
            }
            return guidanceManager.GetStepIdDropdown();
        }

        private IEnumerable<ValueDropdownItem<string>> GetGuidanceEndStepIds()
        {
            var guidanceManager = FindFirstObjectByType<GuidanceManager>();
            if (guidanceManager == null)
            {
                return Array.Empty<ValueDropdownItem<string>>();
            }
            return guidanceManager.GetStepIdDropdown();
        }
    }
}
