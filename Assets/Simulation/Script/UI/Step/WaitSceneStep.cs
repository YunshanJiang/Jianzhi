using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 等待-场景步骤
    /// </summary>
    public class WaitSceneStep : SceneStepBase
    {
        [TitleGroup("Wait")]
        [LabelText("等待时长（秒）")]
        [SerializeField]
        private float m_duration;
        [TitleGroup("Wait")]
        [SerializeField]
        private bool m_displayProgressUI;
        [TitleGroup("Wait")][ShowIf("m_displayProgressUI")]
        [SerializeField]
        private string m_content;


        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private float m_timer;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            m_timer = 0;
            if (m_displayProgressUI)
            {
                GameManager.Instance.UIManager.CountDownProgress.Display(m_content);
            }
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            StepEnd();
            GameManager.Instance.UIManager.CountDownProgress.Hide();
            m_timer = 0;
        }

        protected override void Update()
        {
            base.Update();
            if (IsRunning)
            {
                m_timer += Time.deltaTime;
                if (m_displayProgressUI)
                {
                    GameManager.Instance.UIManager.CountDownProgress.UpdateProgress(m_timer, m_duration);
                }
                if (m_timer >= m_duration)
                {
                    if (m_displayProgressUI)
                    {
                        GameManager.Instance.UIManager.CountDownProgress.Hide();
                    }
                    StepEnd();
                }
            }
        }
    }
}
