using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    /// <summary>
    /// 场景步骤执行流程UI控制
    /// </summary>
    public class SceneStepFlowManager : MonoBehaviour
    {
        private class StepEntry
        {
            public SceneStepBase Step { get; }
            public SceneStepFlowItem View { get; }

            public StepEntry(SceneStepBase step, SceneStepFlowItem view)
            {
                Step = step;
                View = view;
            }
        }

        [TitleGroup("组件引用")]
        [SerializeField]
        private GameObject m_sceneStepFlowRoot;
        private CanvasGroup m_sceneStepFlowRootCanvasGroup;
        [TitleGroup("组件引用")]
        [SerializeField]
        private ScrollRect m_scrollRect;
        [TitleGroup("组件引用")]
        [SerializeField]
        private RectTransform m_contentRoot;
        [TitleGroup("组件引用")]
        [SerializeField]
        private SceneStepFlowItem m_stepItemPrefab;
        [TitleGroup("组件引用")]
        [SerializeField]
        private Button m_displayButton, m_hideButton;
        [TitleGroup("组件引用")]
        [SerializeField]
        private Transform m_hideButtonArrow;


        [TitleGroup("配置")]
        [SerializeField]
        private bool m_isDefaultDisplay;
        [TitleGroup("配置")]
        [SerializeField]
        private float m_displayWidth = 159f;
        [TitleGroup("配置")]
        [SerializeField]
        private float m_hideAnchorX = 30f;
        [TitleGroup("配置")]
        [SerializeField]
        private float m_animDuration = 0.25f;
        [TitleGroup("配置")]
        [SerializeField]
        private float m_itemOffsetSize;
        [TitleGroup("配置")]
        [SerializeField][PropertyTooltip("点击步骤时是否强制结束当前运行步骤并跳转")]
        private bool m_forceJumpFromUI = true;

        private bool m_isDisplay = true;
        private RectTransform m_hideButtonRect;
        private RectTransform m_displayButtonRect;
        private Tween m_toggleSequence;
        private List<StepEntry> m_stepEntries;

        private void Awake()
        {
            m_stepEntries = new();

            if (m_displayButton != null)
            {
                var targetRect = m_displayButton.transform.parent != null
                    ? m_displayButton.transform.parent.GetComponent<RectTransform>()
                    : null;
                m_displayButtonRect = targetRect != null ? targetRect : m_displayButton.GetComponent<RectTransform>();
                m_displayButton.onClick.AddListener(OnDisplayButtonClicked);
            }

            if (m_hideButton != null)
            {
                m_hideButtonRect = m_hideButton.GetComponent<RectTransform>();
                m_hideButton.onClick.AddListener(OnHideButtonClicked);
            }

            if (m_sceneStepFlowRoot != null)
            {
                if (!m_sceneStepFlowRoot.TryGetComponent(out m_sceneStepFlowRootCanvasGroup))
                {
                    m_sceneStepFlowRootCanvasGroup = m_sceneStepFlowRoot.AddComponent<CanvasGroup>();
                }
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            m_isDisplay = m_isDefaultDisplay;
            ApplyStateInstant(m_isDisplay);
        }

        private void OnDisplayButtonClicked()
        {
            if (m_isDisplay)
            {
                return;
            }

            m_isDisplay = true;
            PlayDisplayAnimation();
        }
        private void OnHideButtonClicked()
        {
            if (!m_isDisplay)
            {
                return;
            }

            m_isDisplay = false;
            PlayHideAnimation();
        }

        private void OnEnable()
        {
            SubscribeStepEvents();
        }

        private void OnDisable()
        {
            UnsubscribeStepEvents();
            KillTweens();
        }

        private IEnumerator DelayedRebuild()
        {
            yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.SceneManager != null);
            GameManager.Instance.SceneManager.OnStepStartEvent += HandleStepStart;
            GameManager.Instance.SceneManager.OnStepEndEvent += HandleStepEnd;
            Rebuild();
        }

        [Button]
        public void Rebuild()
        {
            SceneManagerBase sceneManager = null;
            if (Application.isPlaying)
            {
                sceneManager = GameManager.Instance.SceneManager;
            }
            else
            {
                var sceneManagerBaseSet = FindObjectsByType<SceneManagerBase>(FindObjectsSortMode.None);
                if (sceneManagerBaseSet.Length > 0)
                {
                    sceneManager = sceneManagerBaseSet[0];
                }
            }
            if (sceneManager == null)
            {
                Debug.LogWarning("无法重建场景步骤执行流程UI，缺少 SceneManagerBase 对象！");
                return;
            }

            ClearItems();

            foreach (var step in sceneManager.SceneStepSet)
            {
                AppendEntries(step);
            }

            RefreshVisuals();
        }

        private void AppendEntries(SceneStepBase step)
        {
            if (step == null)
            {
                return;
            }

            if (!step.IsDisplayStep)
            {
                return;
            }

            var view = Instantiate(m_stepItemPrefab, m_contentRoot);
            view.Initialize(step.StepName);
            view.OnClicked += () => OnStepItemClicked(step);

            var entry = new StepEntry(step, view);
            m_stepEntries.Add(entry);
        }

        private void OnStepItemClicked(SceneStepBase _targetStep)
        {
            if (_targetStep == null)
            {
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.SceneManager == null)
            {
                return;
            }

            GameManager.Instance.UIManager?.ResetStepRelatedUI();

            var isSuccess = GameManager.Instance.SceneManager.JumpToStep(_targetStep, m_forceJumpFromUI);
            if (!isSuccess)
            {
                return;
            }

            RefreshVisuals();
            ScrollToStep(_targetStep);
        }

        private void HandleStepStart(SceneStepBase step)
        {
            RefreshVisuals();
            ScrollToStep(step);
        }

        private void HandleStepEnd(SceneStepBase step)
        {
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            var sceneManager = GameManager.Instance != null ? GameManager.Instance.SceneManager : null;
            if (sceneManager == null)
            {
                return;
            }

            var currentStep = sceneManager.CurrentStep;
            var currentEntryIndex = m_stepEntries.FindIndex(entry => entry != null && entry.Step == currentStep);

            for (int i = 0; i < m_stepEntries.Count; i++)
            {
                var entry = m_stepEntries[i];
                if (entry == null || entry.View == null || entry.Step == null)
                {
                    continue;
                }

                var isCurrent = sceneManager.IsStepRunning(entry.Step.StepId);
                var isFinished = entry.Step.IsFinished;
                if (currentEntryIndex >= 0)
                {
                    isFinished = i < currentEntryIndex;
                }

                entry.View.SetHighlight(isCurrent);
                entry.View.SetCompleted(isFinished);
            }
        }

        private void SubscribeStepEvents()
        {
            foreach (var entry in m_stepEntries)
            {
                if (entry?.Step == null)
                {
                    continue;
                }

                entry.Step.OnStepStartEvent -= HandleStepStart;
                entry.Step.OnStepStartEvent += HandleStepStart;
                entry.Step.OnStepEndEvent -= HandleStepEnd;
                entry.Step.OnStepEndEvent += HandleStepEnd;
            }
        }

        private void UnsubscribeStepEvents()
        {
            foreach (var entry in m_stepEntries)
            {
                if (entry?.Step == null)
                {
                    continue;
                }

                entry.Step.OnStepStartEvent -= HandleStepStart;
                entry.Step.OnStepEndEvent -= HandleStepEnd;
            }
        }

        private void ClearItems()
        {
            UnsubscribeStepEvents();

            foreach (Transform child in m_contentRoot)
            {
                Destroy(child.gameObject);
            }

            m_stepEntries.Clear();
        }

        private void ApplyStateInstant(bool _isDisplay)
        {
            KillTweens();

            if (m_hideButtonRect != null)
            {
                var targetWidth = _isDisplay ? m_displayWidth : 0f;
                m_hideButtonRect.sizeDelta = new Vector2(targetWidth, m_hideButtonRect.sizeDelta.y);
            }

            if (m_displayButtonRect != null)
            {
                var targetX = _isDisplay ? m_hideAnchorX : 0f;
                m_displayButtonRect.anchoredPosition = new Vector2(targetX, m_displayButtonRect.anchoredPosition.y);
            }

            if (m_hideButtonArrow != null)
            {
                var targetZ = _isDisplay ? 0f : 180f;
                var euler = m_hideButtonArrow.localEulerAngles;
                m_hideButtonArrow.localEulerAngles = new Vector3(euler.x, euler.y, targetZ);
            }

            if (m_sceneStepFlowRootCanvasGroup != null)
            {
                m_sceneStepFlowRootCanvasGroup.alpha = _isDisplay ? 1f : 0f;
            }

            if (m_sceneStepFlowRoot != null)
            {
                m_sceneStepFlowRoot.SetActive(_isDisplay);
            }

            SetDisplayButtonInteractable(!_isDisplay);
            SetHideButtonInteractable(_isDisplay);
        }

        private void PlayDisplayAnimation()
        {
            KillTweens();
            SetDisplayButtonInteractable(false);
            SetHideButtonInteractable(false);

            if (m_sceneStepFlowRoot != null)
            {
                m_sceneStepFlowRoot.SetActive(true);
            }

            if (m_sceneStepFlowRootCanvasGroup != null)
            {
                m_sceneStepFlowRootCanvasGroup.alpha = 0f;
            }

            var sequence = DOTween.Sequence();

            if (m_hideButtonRect != null)
            {
                sequence.Join(m_hideButtonRect.DOSizeDelta(new Vector2(m_displayWidth, m_hideButtonRect.sizeDelta.y), m_animDuration));
            }

            if (m_displayButtonRect != null)
            {
                sequence.Join(m_displayButtonRect.DOAnchorPosX(m_hideAnchorX, m_animDuration));
            }

            if (m_hideButtonArrow != null)
            {
                sequence.Join(m_hideButtonArrow.DOLocalRotate(new Vector3(0f, 0f, 0f), m_animDuration));
            }

            if (m_sceneStepFlowRootCanvasGroup != null)
            {
                sequence.Join(m_sceneStepFlowRootCanvasGroup.DOFade(1f, m_animDuration));
            }

            sequence.OnComplete(() =>
            {
                SetHideButtonInteractable(true);
                m_toggleSequence = null;
            });

            m_toggleSequence = sequence;
        }

        private void PlayHideAnimation()
        {
            KillTweens();
            SetDisplayButtonInteractable(false);
            SetHideButtonInteractable(false);

            if (m_sceneStepFlowRoot != null && !m_sceneStepFlowRoot.activeSelf)
            {
                m_sceneStepFlowRoot.SetActive(true);
            }

            if (m_sceneStepFlowRootCanvasGroup != null)
            {
                m_sceneStepFlowRootCanvasGroup.alpha = 1f;
            }

            var sequence = DOTween.Sequence();

            if (m_hideButtonRect != null)
            {
                sequence.Join(m_hideButtonRect.DOSizeDelta(new Vector2(0f, m_hideButtonRect.sizeDelta.y), m_animDuration));
            }

            if (m_displayButtonRect != null)
            {
                sequence.Join(m_displayButtonRect.DOAnchorPosX(0f, m_animDuration));
            }

            if (m_hideButtonArrow != null)
            {
                sequence.Join(m_hideButtonArrow.DOLocalRotate(new Vector3(0f, 0f, 180f), m_animDuration));
            }

            if (m_sceneStepFlowRootCanvasGroup != null)
            {
                sequence.Join(m_sceneStepFlowRootCanvasGroup.DOFade(0f, m_animDuration));
            }

            sequence.OnComplete(() =>
            {
                if (m_sceneStepFlowRoot != null)
                {
                    m_sceneStepFlowRoot.SetActive(false);
                }

                SetDisplayButtonInteractable(true);
                m_toggleSequence = null;
            });

            m_toggleSequence = sequence;
        }

        private void KillTweens()
        {
            if (m_toggleSequence == null)
            {
                return;
            }

            m_toggleSequence.Kill();
            m_toggleSequence = null;
        }

        // [Button]
        // private void TestScrollToStep()
        // {
        //     ScrollToStep(GameManager.Instance.SceneManager.CurrentStep);
        // }

        private void ScrollToStep(SceneStepBase step)
        {
            if (m_scrollRect == null || m_contentRoot == null || step == null)
            {
                return;
            }

            var entry = m_stepEntries.Find(e => e != null && e.Step == step);
            if (entry?.View == null)
            {
                return;
            }

            var targetRect = entry.View.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_contentRoot);

            var viewport = m_scrollRect.viewport != null ? m_scrollRect.viewport : m_scrollRect.GetComponent<RectTransform>();
            if (viewport == null)
            {
                return;
            }

            var contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_contentRoot);
            var targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_contentRoot, targetRect);
            var contentHeight = Mathf.Abs(contentBounds.size.y);
            var viewportHeight = Mathf.Abs(viewport.rect.height);
            if (viewportHeight <= Mathf.Epsilon)
            {
                return;
            }
            var scrollableHeight = contentHeight - viewportHeight;
            if (scrollableHeight <= Mathf.Epsilon)
            {
                m_scrollRect.verticalNormalizedPosition = 1f;
                return;
            }

            var targetTopFromContentTop = contentBounds.max.y - targetBounds.max.y + m_itemOffsetSize;
            var clampedTopOffset = Mathf.Clamp(targetTopFromContentTop, 0f, scrollableHeight);
            var normalized = 1f - (clampedTopOffset / scrollableHeight);
            m_scrollRect.verticalNormalizedPosition = normalized;
        }

        private void SetDisplayButtonInteractable(bool isInteractable)
        {
            if (m_displayButton == null)
            {
                return;
            }

            m_displayButton.image.raycastTarget = isInteractable;
            m_displayButton.interactable = isInteractable;
        }

        private void SetHideButtonInteractable(bool isInteractable)
        {
            if (m_hideButton == null)
            {
                return;
            }

            m_hideButton.image.raycastTarget = isInteractable;
            m_hideButton.interactable = isInteractable;
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
        {
            StartCoroutine(nameof(DelayedRebuild));
        }
    }
}
