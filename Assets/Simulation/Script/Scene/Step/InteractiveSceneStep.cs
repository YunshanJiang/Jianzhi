using Sirenix.OdinInspector;
using Starscape.Common;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 交互场景步骤
    /// </summary>
    public class InteractiveSceneStep : SceneStepBase
    {
        [TitleGroup("Interactive")]
        public InteractiveBase Interactive => m_interactive;
        [SerializeField]
        private InteractiveBase m_interactive;
        [TitleGroup("Interactive")]
        [SerializeField]
        private string m_interactiveContent;
        [TitleGroup("Interactive")]
        [SerializeField]
        private bool m_stepEndDisableInteractive = true;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            m_interactive.gameObject.SetActive(true);
            m_interactive.OnInteractEvent += OnInteract;
            if (!string.IsNullOrEmpty(m_interactiveContent))
            {
                m_interactive.SetContent(m_interactiveContent);
            }
        }

        protected override void OnStepEnd()
        {
            
            base.OnStepEnd();
            m_interactive.gameObject.SetActive(false);
            m_interactive.OnInteractEvent -= OnInteract;
        }

        private void OnInteract(InteractiveBase _interactiveBase)
        {
            if (m_stepEndDisableInteractive)
            {
                m_interactive.gameObject.SetActive(false);
            }
            StepEnd();
        }
        protected override void OnStepReset()
        {
            base.OnStepReset();
            m_interactive.gameObject.SetActive(false);
            m_interactive.OnInteractEvent -= OnInteract;
        }
        public void ShakeStepMessage()
        {
            if (StepMessageId != 0 && GameManager.Instance.UIManager.TryGetNotify(StepMessageId, out var notifyUI))
            {
                notifyUI.StrongTipAnimation();
            }
        }
    }
}
