using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    public class TextViewerSceneStep : ViewerBaseSceneStep
    {
        [TitleGroup("Text Viewer")]
        [SerializeField][MultiLineProperty(5)]
        private string m_content;
        [TitleGroup("Text Viewer")][PropertyTooltip("true: 开始时显示完成按钮，false：滚动条完成显示按钮")]
        [SerializeField]
        private bool m_isCompleteButtonActive;
        public override bool IsNeedFlipButtons => false;
        protected override int ViewerCount => 1;

        private bool m_isReachedScrollEnd;

        protected override void OnStepStart()
        {
            base.OnStepStart();
            RestoreInitialState();
            CurrentIndex = 0;
            if (!HasGroup())
            {
                GameManager.Instance.UIManager.GetViewerContentFlipButtonsVisible += OnGetViewerContentFlipButtonsVisible;
            }
            GameManager.Instance.UIManager.GetTextViewerContent += OnGetTextViewerContent;
            GameManager.Instance.UIManager.ViewerDisplay(GetViewerTitle(), this, GetViewerTitleSet());
            GameManager.Instance.UIManager.TextViewerScrollRect.verticalNormalizedPosition = 1f;
            GameManager.Instance.UIManager.TextViewerScrollRect.onValueChanged.AddListener(OnTextViewerScrollRectValueChanged);
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            RestoreInitialState();
            CurrentIndex = 0;
            GameManager.Instance.UIManager.ViewerDisplay(GetViewerTitle(), this, GetViewerTitleSet());
            GameManager.Instance.UIManager.TextViewerScrollRect.verticalNormalizedPosition = 1f;
        }

        protected override void OnStepResume()
        {
            base.OnStepResume();
            RestoreInitialState();
            if (CurrentIndex == -1)
            {
                CurrentIndex = 0;
            }
            if (!HasGroup())
            {
                GameManager.Instance.UIManager.GetViewerContentFlipButtonsVisible += OnGetViewerContentFlipButtonsVisible;
            }
            GameManager.Instance.UIManager.GetTextViewerContent += OnGetTextViewerContent;
            GameManager.Instance.UIManager.ViewerDisplay(GetViewerTitle(), this, GetViewerTitleSet());
            GameManager.Instance.UIManager.TextViewerScrollRect.onValueChanged.AddListener(OnTextViewerScrollRectValueChanged);
        }

        protected override void OnStepEnd()
        {
            base.OnStepEnd();
            if (!HasGroup())
            {
                GameManager.Instance.UIManager.GetViewerContentFlipButtonsVisible -= OnGetViewerContentFlipButtonsVisible;
            }
            GameManager.Instance.UIManager.GetTextViewerContent -= OnGetTextViewerContent;
            GameManager.Instance.UIManager.TextViewerScrollRect.onValueChanged.RemoveListener(OnTextViewerScrollRectValueChanged);
        }

        protected override void OnClose()
        {
            base.OnClose();
            StepEnd();
        }

        private bool OnGetViewerContentFlipButtonsVisible()
        {
            return false;
        }

        private (string Content, string Title) OnGetTextViewerContent()
        {
            return (m_content, Title);
        }

        private void OnTextViewerScrollRectValueChanged(Vector2 _position)
        {
            if (_position.y <= 0.01f)
            {
                m_isReachedScrollEnd = true;
                GameManager.Instance.UIManager.UpdateViewerButtonVisible();
            }
        }

        protected override bool OnGetViewerContentCompleteButtonVisible()
        {
            if (!base.OnGetViewerContentCompleteButtonVisible())
            {
                return false;
            }
            if (m_isCompleteButtonActive)
            {
                return true;
            }
            return m_isReachedScrollEnd;
        }

        private void RestoreInitialState()
        {
            m_isCompleteButtonActive = false;
            m_isReachedScrollEnd = false;
        }
    }
}
