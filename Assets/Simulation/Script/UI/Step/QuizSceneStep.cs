using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 问答
    /// </summary>
    public class QuizSceneStep : SceneStepBase
    {
        [TitleGroup("Quiz")]
        [SerializeField]
        private QuizBankData m_quizBankData;
        [TitleGroup("Quiz")]
        [SerializeField]
        private string m_questionId;

        private QuizUI m_quizUI;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            m_quizUI = GameManager.Instance.UIManager.DisplayQuiz(m_quizBankData, m_questionId);
            m_quizUI.OnQuizEndEvent += OnQuizEndCallback;
        }

        protected override void OnStepEnd()
        {
            base.OnStepEnd();
            GameManager.Instance.UIManager.HideQuiz();
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            // 清除之前的UI事件绑定
            if (m_quizUI != null)
            {
                Run_m_onStepEnd();
                m_quizUI.OnQuizEndEvent -= OnQuizEndCallback;
                GameManager.Instance.UIManager.HideQuiz();
                return;
            }
           

            m_quizUI.OnQuizEndEvent += OnQuizEndCallback;

            m_quizUI = GameManager.Instance.UIManager.DisplayQuiz(m_quizBankData, m_questionId);
        }

        private void OnQuizEndCallback(QuizUI _quizUI)
        {
            StepEnd();
        }
    }
}
