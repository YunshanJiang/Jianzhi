using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    /// <summary>
    /// 指导执行器切换
    /// </summary>
    public class GuidanceExecutorToggle : GuidanceExecutorBase
    {
        [TitleGroup("Toggle")]
        [SerializeField][Required]
        private Toggle m_toggle;
        [TitleGroup("Toggle")]
        [SerializeField]
        private bool m_expectedValue;

        protected override void OnExecutorStart()
        {
            base.OnExecutorStart();
            m_toggle.onValueChanged.AddListener(HandleValueChanged);
        }

        protected override void OnExecutorEnd()
        {
            base.OnExecutorEnd();
            if (m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(HandleValueChanged);
            }
        }

        private void HandleValueChanged(bool _isOn)
        {
            if (!IsExecuting)
            {
                return;
            }

            if (_isOn == m_expectedValue)
            {
                ReportSuccess();
            }
        }
    }
}
