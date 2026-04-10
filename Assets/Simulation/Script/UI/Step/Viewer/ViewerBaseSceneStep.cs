using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 查看器基础
    /// </summary>
    public abstract class ViewerBaseSceneStep : SceneStepBase
    {
        public string Title => m_title;
        [TitleGroup("Viewer Base")]
        [SerializeField]
        private string m_title;

        [TitleGroup("Viewer Base")]
        [SerializeField]
        private bool m_isLoop = true;

        [TitleGroup("Viewer Base/运行时")]
        [ShowInInspector][ReadOnly]
        protected abstract int ViewerCount { get; }

        [TitleGroup("Viewer Base/运行时")]
        [ShowInInspector][ReadOnly]
        public int CurrentIndex { get; protected set; } = -1;

        [TitleGroup("Viewer Base/运行时")]
        [ShowInInspector][ReadOnly]
        public abstract bool IsNeedFlipButtons { get; }

        private bool m_isReachedEnd;
        private ViewerGroupSceneStep m_groupSceneStep;

        protected override void Awake()
        {
            base.Awake();
            m_groupSceneStep = GetComponentInParent<ViewerGroupSceneStep>();
        }

        public void Previous()
        {
            if (CurrentIndex == 0)
            {
                CurrentIndex = ViewerCount - 1;
            }
            else
            {
                CurrentIndex = (CurrentIndex - 1) % ViewerCount;
            }
            OnPrevious();
        }

        public void Next()
        {
            CurrentIndex = (CurrentIndex + 1) % ViewerCount;
            OnNext();
        }

        public void Close()
        {
            OnClose();
        }

        protected virtual void OnPrevious() { }

        protected virtual void OnNext() { }

        protected virtual void OnClose() { }

        protected override void OnStepStart()
        {
            base.OnStepStart();
            GameManager.Instance.UIManager.GetViewerContentPreviousButtonInteractable += OnGetViewerContentPreviousButtonInteractable;
            GameManager.Instance.UIManager.GetViewerContentNextButtonInteractable += OnGetViewerContentNextButtonInteractable;
            GameManager.Instance.UIManager.GetViewerContentCompleteButtonVisible += OnGetViewerContentCompleteButtonVisible;
        }

        protected override void OnStepReset()
        {
            base.OnStepReset();
            m_isReachedEnd = false;
        }

        protected override void OnStepEnd()
        {
            base.OnStepEnd();
            GameManager.Instance.UIManager.GetViewerContentPreviousButtonInteractable -= OnGetViewerContentPreviousButtonInteractable;
            GameManager.Instance.UIManager.GetViewerContentNextButtonInteractable -= OnGetViewerContentNextButtonInteractable;
            GameManager.Instance.UIManager.GetViewerContentCompleteButtonVisible -= OnGetViewerContentCompleteButtonVisible;
        }

        protected override void OnStepResume()
        {
            base.OnStepResume();
            GameManager.Instance.UIManager.GetViewerContentPreviousButtonInteractable += OnGetViewerContentPreviousButtonInteractable;
            GameManager.Instance.UIManager.GetViewerContentNextButtonInteractable += OnGetViewerContentNextButtonInteractable;
            GameManager.Instance.UIManager.GetViewerContentCompleteButtonVisible += OnGetViewerContentCompleteButtonVisible;
        }

        private bool OnGetViewerContentPreviousButtonInteractable()
        {
            if (m_isLoop)
            {
                return true;
            }
            if (CurrentIndex > 0)
            {
                return true;
            }
            return false;
        }

        private bool OnGetViewerContentNextButtonInteractable()
        {
            if (CurrentIndex == ViewerCount - 1)
            {
                m_isReachedEnd = true;
            }
            if (m_isLoop)
            {
                return true;
            }
            if (CurrentIndex < ViewerCount - 1)
            {
                return true;
            }
            return false;
        }

        protected virtual bool OnGetViewerContentCompleteButtonVisible()
        {
            if (HasGroup()) return false;
            return m_isReachedEnd;
        }

        protected bool HasGroup()
        {
            // Editor 会使用, 需要每次都获取
            if (transform.parent == null)
            {
                return false;
            }
            return transform.parent.GetComponent<ViewerGroupSceneStep>() != null;
        }

        protected string GetViewerTitle()
        {
            if (m_groupSceneStep != null)
            {
                return m_groupSceneStep.ViewerTitle;
            }
            return m_title;
            // return string.Empty;
        }

        protected List<string> GetViewerTitleSet()
        {
            if (m_groupSceneStep != null)
            {
                return m_groupSceneStep.TitleSet;
            }
            return new List<string>();
        }
    }
}
