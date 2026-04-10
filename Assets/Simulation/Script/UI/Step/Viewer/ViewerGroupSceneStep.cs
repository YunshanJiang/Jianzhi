using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 图片查看器组
    /// </summary>
    public class ViewerGroupSceneStep : SceneStepBase
    {
        public string ViewerTitle => m_viewerTitle;
        [TitleGroup("Image Viewer Group")]
        [SerializeField]
        private string m_viewerTitle;

        [TitleGroup("Image Viewer Group")]
        [InlineButton("CollectChildSteps", SdfIconType.ArrowClockwise, "")]
        [SerializeField]
        private List<ViewerBaseSceneStep> m_viewerSteps;

        [TitleGroup("Image Viewer Group")]
        [SerializeField][ValueDropdown("GetTitleDropdownList")]
        private string m_defaultStepTitle;


        [TitleGroup("Image Viewer Group/运行时")]
        [ShowInInspector][ReadOnly]
        private string m_currentStepTitle;

        public List<string> TitleSet => m_titleSet;
        private List<string> m_titleSet;

        protected override void Awake()
        {
            base.Awake();
            m_titleSet = new List<string>(m_viewerSteps.Select(_item => _item.Title));
        }

        protected override void OnStepStart()
        {
            base.OnStepStart();
            GameManager.Instance.UIManager.OnViewerTitleSelectStateChange += OnViewerTitleSelectStateChange;
            GameManager.Instance.UIManager.GetViewerContentFlipButtonsVisible += GetViewerContentFlipButtonsVisible;
            GameManager.Instance.UIManager.GetViewerCloseButtonVisible += GetViewerCloseButtonVisible;
            GameManager.Instance.UIManager.OnViewerCloseClicked += OnViewerCloseClicked;

            m_currentStepTitle = m_defaultStepTitle;
            var step = m_viewerSteps.Find(_item => _item.Title == m_defaultStepTitle);
            step.StepStart();
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            var previousStep = m_viewerSteps.Find(_item => _item.Title == m_currentStepTitle);
            previousStep?.StepEnd();

            m_currentStepTitle = m_defaultStepTitle;
            var step = m_viewerSteps.Find(_item => _item.Title == m_defaultStepTitle);
            step.StepReset();
        }

        protected override void OnStepResume()
        {
            base.OnStepResume();
            GameManager.Instance.UIManager.OnViewerTitleSelectStateChange += OnViewerTitleSelectStateChange;
            GameManager.Instance.UIManager.GetViewerContentFlipButtonsVisible += GetViewerContentFlipButtonsVisible;
            GameManager.Instance.UIManager.GetViewerCloseButtonVisible += GetViewerCloseButtonVisible;
            GameManager.Instance.UIManager.OnViewerCloseClicked += OnViewerCloseClicked;

            ChangeViewer(m_currentStepTitle);
        }

        protected override void OnStepEnd()
        {
            base.OnStepEnd();
            m_currentStepTitle = null;
            GameManager.Instance.UIManager.OnViewerTitleSelectStateChange -= OnViewerTitleSelectStateChange;
            GameManager.Instance.UIManager.GetViewerContentFlipButtonsVisible -= GetViewerContentFlipButtonsVisible;
            GameManager.Instance.UIManager.GetViewerCloseButtonVisible -= GetViewerCloseButtonVisible;
            GameManager.Instance.UIManager.OnViewerCloseClicked -= OnViewerCloseClicked;
        }

        private void OnViewerTitleSelectStateChange(string _title, bool _isSelected)
        {
            if (_isSelected)
            {
                if (!m_titleSet.Contains(_title))
                {
                    Debug.LogError($"ViewerGroupSceneStep: {_title} 不在标题列表中");
                    return;
                }
                if (m_currentStepTitle == _title) return;
                ChangeViewer(_title);
            }
        }

        private bool GetViewerContentFlipButtonsVisible()
        {
            if (string.IsNullOrEmpty(m_currentStepTitle)) return false;
            var step = m_viewerSteps.Find(_item => _item.Title == m_currentStepTitle);
            if (step == null) return false;
            return step.IsNeedFlipButtons;
        }

        private bool GetViewerCloseButtonVisible()
        {
            return true;
        }

        private void OnViewerCloseClicked()
        {
            StepEnd();
        }

        private IEnumerable<ValueDropdownItem<string>> GetTitleDropdownList()
        {
            return m_viewerSteps.Select(_item => new ValueDropdownItem<string>(_item.Title, _item.Title));
        }

        private void ChangeViewer(string _title)
        {
            var previousStep = m_viewerSteps.Find(_item => _item.Title == m_currentStepTitle);
            previousStep?.StepEnd();

            m_currentStepTitle = _title;
            var step = m_viewerSteps.Find(_item => _item.Title == _title);
            step.StepResume();
        }

        private void CollectChildSteps()
        {
            if (m_viewerSteps == null)
            {
                m_viewerSteps = new List<ViewerBaseSceneStep>();
            }
            else
            {
                m_viewerSteps.Clear();
            }

            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf) continue;
                var step = child.GetComponent<ViewerBaseSceneStep>();
                m_viewerSteps.Add(step);
            }
        }
    }
}
