using System;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    public delegate void ViewerTitleSelectStateChangeHandler(string _title, bool _isSelected);

    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static int s_idIndex;

        private void OnValidate()
        {
            if (m_countDownProgress == null)
            {
                m_countDownProgress = FindFirstObjectByType<CountDownProgress>();
            }
        }

        private void Awake()
        {
            AwakeImageViewer();
            SetFGVisble(true);
        }

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy() { }


        private void OnSceneUnloaded(Scene _scene)
        {
            foreach (var notifyUI in m_notifyMap.Values)
            {
                if (notifyUI == null) continue;
                Destroy(notifyUI.gameObject);
            }
            m_notifyMap.Clear();
        }


#region Fade遮罩
        public CountDownProgress CountDownProgress => m_countDownProgress;
        [FoldoutGroup("Fade遮罩")]
        [SerializeField]
        private CountDownProgress m_countDownProgress;

        [FoldoutGroup("Fade遮罩")]
        [SerializeField]
        public Image m_overlayImage;

        /// <summary>
        /// 淡入遮罩(变黑)
        /// </summary>
        /// <param name="_duration">持续时间</param>
        /// <param name="_completeAction">完成回调</param>
        public void FadeInOverlay(float _duration, Action _completeAction = null)
        {
            m_overlayImage.gameObject.SetActive(true);
            m_overlayImage.color = new Color(m_overlayImage.color.r, m_overlayImage.color.g, m_overlayImage.color.b, 0);
            m_overlayImage.DOFade(1, _duration).OnComplete(() =>
            {
                _completeAction?.Invoke();
            });
        }

        /// <summary>
        /// 淡出遮罩(变透明)
        /// </summary>
        /// <param name="_duration">持续时间</param>
        /// <param name="_completeAction">完成回调</param>
        public void FadeOutOverlay(float _duration, Action _completeAction = null)
        {
            m_overlayImage.color = new Color(m_overlayImage.color.r, m_overlayImage.color.g, m_overlayImage.color.b, 1);
            m_overlayImage.DOFade(0, _duration)
            .OnComplete(() =>
                {
                    m_overlayImage.gameObject.SetActive(false);
                    _completeAction?.Invoke();
                });
        }

        /// <summary>
        /// 淡入淡出遮罩
        /// </summary>
        /// <param name="_fadeInDuration">淡入时间</param>
        /// <param name="_stayDuration">停留时间</param>
        /// <param name="_fadeOutDuration">淡出时间</param>
        /// <param name="_fadeInAction">淡入完成时回调</param>
        /// <param name="_completeAction">淡出完成时回调</param>
        public void FadeInOutOverlay(float _fadeInDuration, float _stayDuration, float _fadeOutDuration, Action _fadeInAction = null, Action _completeAction = null)
        {
            FadeInOverlay(_fadeInDuration, () =>
            {
                _fadeInAction?.Invoke();
                if (_stayDuration > 0)
                {
                    DOVirtual.DelayedCall(_stayDuration, () =>
                    {
                        FadeOutOverlay(_fadeOutDuration, _completeAction);
                    });
                }
                else
                {
                    FadeOutOverlay(_fadeOutDuration, _completeAction);
                }
            });
        }
#endregion


#region 通知
        [FoldoutGroup("通知")]
        [SerializeField]
        private Transform m_notifyPanel;
        [FoldoutGroup("通知")]
        [SerializeField]
        private GameObject m_notifyPrefab;
        [FoldoutGroup("通知")]
        [SerializeField]
        private Transform m_notifyCenter;
        [FoldoutGroup("通知/运行时")]
        [ShowInInspector][ReadOnly]
        private Dictionary<int, NotifyUI> m_notifyMap = new();

        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="_message">内容</param>
        public int Notify(string _message)
        {
            return Notify(_message, Color.white);
        }

        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="_message">内容</param>
        /// <param name="_color">颜色</param>
        /// <param name="_duration">持续时间</param>
        public int Notify(string _message, Color _color, float _duration = 2f)
        {
            var notifyObj = Instantiate(m_notifyPrefab, m_notifyPanel);
            var notifyUI = notifyObj.GetComponent<NotifyUI>();
            notifyUI.SetData(_message, _color, _duration, NotifyType.Normal);
            notifyUI.transform.SetAsFirstSibling();
            s_idIndex++;
            m_notifyMap.Add(s_idIndex, notifyUI);
            return s_idIndex;
        }

        /// <summary>
        /// 居中通知, 同时只能显示一个
        /// </summary>
        /// <param name="_message">内容</param>
        public int NotifyCenter(string _message)
        {
            return NotifyCenter(_message, Color.white);
        }

        /// <summary>
        /// 居中通知, 同时只能显示一个
        /// </summary>
        /// <param name="_message">内容</param>
        /// <param name="_color">颜色</param>
        /// <param name="_duration">持续时间</param>
        public int NotifyCenter(string _message, Color _color, float _duration = 3f)
        {
            if (m_notifyCenter.childCount > 0)
            {
                var oldGo = m_notifyCenter.GetChild(0).gameObject;
                var oldUi = oldGo.GetComponent<NotifyUI>();
                RemoveNotifyMapEntriesFor(oldUi);
                Destroy(oldGo);
            }
            var notifyObj = Instantiate(m_notifyPrefab, m_notifyCenter);
            var notifyUI = notifyObj.GetComponent<NotifyUI>();
            notifyUI.SetData(_message, _color, _duration, NotifyType.Error);
            s_idIndex++;
            m_notifyMap.Add(s_idIndex, notifyUI);
            return s_idIndex;
        }

        /// <summary>
        /// 隐藏通知
        /// </summary>
        /// <param name="_id">ID</param>
        public void HideNotify(int _id)
        {
            if (m_notifyMap.TryGetValue(_id, out var notifyUI))
            {
                if (notifyUI != null)
                {
                    notifyUI.Hide();
                }

                m_notifyMap.Remove(_id);
            }
        }

        /// <summary>
        /// 从映射中移除指向指定实例的条目（例如居中通知被新通知替换时已 Destroy 但 id 仍留在表中）。
        /// </summary>
        private void RemoveNotifyMapEntriesFor(NotifyUI _ui)
        {
            if (_ui == null)
            {
                return;
            }

            var toRemove = new List<int>();
            foreach (var kv in m_notifyMap)
            {
                if (kv.Value == _ui)
                {
                    toRemove.Add(kv.Key);
                }
            }

            for (var i = 0; i < toRemove.Count; i++)
            {
                m_notifyMap.Remove(toRemove[i]);
            }
        }

        /// <summary>
        /// 获取通知UI
        /// </summary>
        /// <param name="_id">ID</param>
        /// <param name="_notifyUI"></param>
        /// <returns></returns>
        public bool TryGetNotify(int _id, out NotifyUI _notifyUI)
        {
            return m_notifyMap.TryGetValue(_id, out _notifyUI);
        }
#endregion


#region 警告
        [FoldoutGroup("警告")]
        [SerializeField]
        private CanvasGroup m_warningCanvasGroup;
        [FoldoutGroup("警告")]
        [SerializeField]
        private TextMeshProUGUI m_warningText;
        private Tween m_warningTween;

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="_message"></param>
        [FoldoutGroup("警告")]
        [Button]
        public void Warning(string _message, float _duration = 5f)
        {
            m_warningText.text = _message;
            m_warningCanvasGroup.gameObject.SetActive(true);
            m_warningCanvasGroup.alpha = 0;
            m_warningCanvasGroup.DOFade(1, 0.5f);
            if (m_warningTween != null && m_warningTween.IsActive() && m_warningTween.IsPlaying())
            {
                m_warningTween.Kill();
            }
            m_warningTween = m_warningCanvasGroup.DOFade(0, 1f).SetDelay(_duration).OnComplete(() =>
            {
                m_warningCanvasGroup.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// 立即关闭警告条（与 <see cref="Warning"/> 配对使用）
        /// </summary>
        public void HideWarning()
        {
            if (m_warningTween != null && m_warningTween.IsActive())
            {
                m_warningTween.Kill();
            }
            m_warningCanvasGroup.alpha = 0;
            m_warningCanvasGroup.gameObject.SetActive(false);
        }
#endregion


#region 测试题UI
        /// <summary>
        /// 题目根节点
        /// </summary>
        [FoldoutGroup("测试题UI")]
        [SerializeField]
        private Transform m_quizRoot;
        /// <summary>
        /// 题目预制体
        /// </summary>
        [FoldoutGroup("测试题UI")]
        [SerializeField]
        private GameObject m_quizBankPrefab, m_quizPrefabSingle, m_quizPrefabMultiple, m_quizPrefabSequence, m_quizPrefabTrueFalse;
        public bool IsAutoNextQuestion => m_isAutoNextQuestion;
        [FoldoutGroup("测试题UI")]
        [SerializeField]
        private bool m_isAutoNextQuestion;

        /// <summary>
        /// 当前题目UI
        /// </summary>
        public QuizUI CurrentQuizUI { get; private set; }

        /// <summary>
        /// true: 当前题目已完成
        /// </summary>
        public bool CurrentQuizIsFinish => CurrentQuizUI?.IsFinish ?? false;

        /// <summary>
        /// 显示题目
        /// </summary>
        /// <param name="_quizBankData">题库</param>
        /// <exception cref="Exception"></exception>
        public QuizUI DisplayQuiz(QuizBankData _quizBankData)
        {
            Debug.Log($"Bank[{_quizBankData.DisplayName}] displaying quiz.");
            var quizUIObj = Instantiate(m_quizBankPrefab, m_quizRoot);
            var quizUI = quizUIObj.GetComponent<QuizUI>();
            quizUI.SetData(_quizBankData);
            CurrentQuizUI = quizUI;
            return quizUI;
        }

        /// <summary>
        /// 显示题目
        /// </summary>
        /// <param name="_quizBankData">题库</param>
        /// <param name="_questionId">题目Id</param>
        /// <exception cref="Exception"></exception>
        public QuizUI DisplayQuiz(QuizBankData _quizBankData, string _questionId)
        {
            Debug.Log($"Bank[{_quizBankData.DisplayName}] displaying quiz with ID: {_questionId}");
            var quizData = _quizBankData.Questions.Find(_item => _item.QuestionId == _questionId);
            if (quizData == null)
            {
                Debug.Log($"Quiz with ID {_questionId} not found.");
                return null;
            }
            var prefab = quizData.Type switch
            {
                QuizType.SingleOption => m_quizPrefabSingle,
                QuizType.MultiOption => m_quizPrefabMultiple,
                QuizType.Sequence => m_quizPrefabSequence,
                QuizType.TrueFalse => m_quizPrefabTrueFalse,
                _ => throw new Exception($"未定义的类型: {quizData.Type}"),
            };
            var quizUIObj = Instantiate(prefab, m_quizRoot);
            var quizUI = quizUIObj.GetComponent<QuizUI>();
            quizUI.SetData(_quizBankData);
            quizUI.Display(_questionId);
            CurrentQuizUI = quizUI;
            return quizUI;
        }

        /// <summary>
        /// 隐藏题目
        /// </summary>
        public void HideQuiz()
        {
            foreach (Transform child in m_quizRoot)
            {
                var quizUI = child.GetComponent<QuizUI>();
                if (quizUI == null) continue;
                quizUI.Hide();
                Destroy(child.gameObject);
            }
        }
#endregion


#region 浏览器
        [FoldoutGroup("浏览器")]
        [SerializeField]
        private GameObject m_viewerPanel;
        [FoldoutGroup("浏览器")]
        [SerializeField]
        private TextMeshProUGUI m_viewerTitle;
        [FoldoutGroup("浏览器")]
        [SerializeField]
        private Button m_viewerCloseButton;
        [FoldoutGroup("浏览器")]
        [ShowInInspector][ReadOnly]
        private ViewerBaseSceneStep m_currentViewerBaseSceneStep;
        public event Action OnViewerCloseClicked;
        public event Func<bool> GetViewerCloseButtonVisible;


        [FoldoutGroup("浏览器/标题")]
        [SerializeField]
        private Transform m_viewerTitlePanel;
        [FoldoutGroup("浏览器/标题")]
        [SerializeField]
        private Transform m_viewerTitleParent;
        [FoldoutGroup("浏览器/标题")]
        [SerializeField]
        private GameObject m_viewerTitlePrefab;
        [FoldoutGroup("浏览器/标题")]
        [SerializeField]
        private Button m_viewerTitleUpButton, m_viewerTitleDownButton;
        [FoldoutGroup("浏览器/标题")]
        [SerializeField]
        private ScrollRect m_viewerTitleScrollRect;
        public event ViewerTitleSelectStateChangeHandler OnViewerTitleSelectStateChange;


        [FoldoutGroup("浏览器/内容")]
        [SerializeField]
        private Button m_viewerContentPreviousButton;
        [FoldoutGroup("浏览器/内容")]
        [SerializeField]
        private Button m_viewerContentNextButton;
        [FoldoutGroup("浏览器/内容")]
        [SerializeField]
        private TextMeshProUGUI m_viewerContentTitleText;
        [FoldoutGroup("浏览器/内容")]
        [SerializeField]
        private GameObject m_viewerContentFlipButtonsRoot;
        [FoldoutGroup("浏览器/内容")]
        [SerializeField]
        private Button m_viewerContentCompleteButton;
        public event Func<bool> GetViewerContentPreviousButtonInteractable;
        public event Func<bool> GetViewerContentNextButtonInteractable;
        public event Func<bool> GetViewerContentFlipButtonsVisible;
        public event Func<bool> GetViewerContentCompleteButtonVisible;

        [FoldoutGroup("浏览器/内容/Image Viewer")]
        [SerializeField]
        private Image m_imageViewerImage;
        public event Func<(Sprite Sprite, string Title)> GetImageViewerSprite;

        [FoldoutGroup("浏览器/内容/Text Viewer")]
        [SerializeField]
        private GameObject m_textViewerRoot;
        public ScrollRect TextViewerScrollRect => m_textViewerScrollRect;
        [FoldoutGroup("浏览器/内容/Text Viewer")]
        [SerializeField]
        private ScrollRect m_textViewerScrollRect;
        [FoldoutGroup("浏览器/内容/Text Viewer")]
        [SerializeField]
        private TextMeshProUGUI m_textViewerText;
        public event Func<(string Content, string Title)> GetTextViewerContent;

        private void AwakeImageViewer()
        {
            m_viewerPanel.SetActive(false);
            m_viewerCloseButton.onClick.AddListener(OnViewerCloseButtonClicked);

            m_viewerTitleUpButton.onClick.AddListener(OnViewerTitleUpButtonClicked);
            m_viewerTitleDownButton.onClick.AddListener(OnViewerTitleDownButtonClicked);

            m_viewerContentPreviousButton.onClick.AddListener(OnViewerContentPreviousButtonClicked);
            m_viewerContentNextButton.onClick.AddListener(OnViewerContentNextButtonClicked);
            m_viewerContentCompleteButton.onClick.AddListener(OnViewerContentCompleteButtonClicked);
        }

        /// <summary>
        /// 显示图片
        /// </summary>
        /// <param name="_viewerTitle">浏览器标题</param>
        /// <param name="_viewerBaseSceneStep">浏览器步骤</param>
        /// <param name="_viewerTitleSet">标题集合</param>
        public void ViewerDisplay(string _viewerTitle, ViewerBaseSceneStep _viewerBaseSceneStep, [CanBeNull] List<string> _viewerTitleSet = null)
        {
            m_currentViewerBaseSceneStep = _viewerBaseSceneStep;
            m_viewerTitle.text = _viewerTitle;
            m_viewerPanel.SetActive(true);
            m_viewerCloseButton.gameObject.SetActive(GetViewerCloseButtonVisible?.Invoke() ?? false);
            m_imageViewerImage.gameObject.SetActive(GetImageViewerSprite != null);
            m_textViewerRoot.SetActive(GetTextViewerContent != null);

            foreach (Transform child in m_viewerTitleParent)
            {
                Destroy(child.gameObject);
            }

            if (_viewerTitleSet != null && _viewerTitleSet.Count > 0)
            {
                m_viewerTitlePanel.gameObject.SetActive(true);
                var titleUISet = new List<ViewerTitleUI>();
                foreach (var title in _viewerTitleSet)
                {
                    var viewerTitleObject = Instantiate(m_viewerTitlePrefab, m_viewerTitleParent);
                    var viewerTitleUI = viewerTitleObject.GetComponent<ViewerTitleUI>();
                    viewerTitleUI.SetData(title);
                    viewerTitleUI.OnToggleStateChangeEvent += _isOn =>
                    {
                        OnViewerTitleSelectStateChange?.Invoke(title, _isOn);
                    };
                    titleUISet.Add(viewerTitleUI);
                }
                // 设置默认选中
                var defaultTitle = titleUISet.Find(_item => _item.Title == _viewerBaseSceneStep.Title);
                defaultTitle?.SetToggleState(true);
                if (defaultTitle == null)
                {
                    Debug.Log($"当前选择的标题[{_viewerBaseSceneStep.Title}]不在标题列表中.");
                }
            }
            else
            {
                m_viewerTitlePanel.gameObject.SetActive(false);
            }

            UpdateViewer();
        }

        /// <summary>
        /// 隐藏图片
        /// </summary>
        public void ViewerHide()
        {
            m_viewerPanel.SetActive(false);
            m_viewerCloseButton.gameObject.SetActive(false);
            GetImageViewerSprite = null;
            GetTextViewerContent = null;
            OnViewerTitleSelectStateChange = null;
            foreach (Transform child in m_viewerTitleParent)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnViewerCloseButtonClicked()
        {
            ViewerHide();
            OnViewerCloseClicked?.Invoke();
        }

        private void OnViewerTitleUpButtonClicked()
        {
            ScrollViewerTitles(-1f);
        }

        private void OnViewerTitleDownButtonClicked()
        {
            ScrollViewerTitles(1f);
        }

        private void ScrollViewerTitles(float _direction)
        {
            if (m_viewerTitleScrollRect == null || m_viewerTitleScrollRect.content == null) return;
            var content = m_viewerTitleScrollRect.content;
            var viewport = m_viewerTitleScrollRect.viewport != null ? m_viewerTitleScrollRect.viewport : (RectTransform)m_viewerTitleScrollRect.transform;
            var viewportHeight = viewport.rect.height;
            var contentHeight = content.rect.height;
            if (contentHeight <= viewportHeight || viewportHeight <= 0f) return;
            var maxOffset = contentHeight - viewportHeight;
            var targetY = Mathf.Clamp(content.anchoredPosition.y + _direction * viewportHeight, 0f, maxOffset);
            content.DOAnchorPosY(targetY, 0.3f).SetEase(Ease.OutCubic);
        }

        private void OnViewerContentPreviousButtonClicked()
        {
            m_currentViewerBaseSceneStep.Previous();
            UpdateViewer();
        }

        private void OnViewerContentNextButtonClicked()
        {
            m_currentViewerBaseSceneStep.Next();
            UpdateViewer();
        }

        private void OnViewerContentCompleteButtonClicked()
        {
            m_viewerPanel.SetActive(false);
            m_currentViewerBaseSceneStep.Close();
        }

        private void UpdateViewer()
        {
            UpdateImageViewerSprite();
            UpdateTextViewerContent();
            UpdateViewerButtonVisible();
        }

        public void UpdateViewerButtonVisible()
        {
            m_viewerCloseButton.gameObject.SetActive(GetViewerCloseButtonVisible?.Invoke() ?? false);

            m_viewerContentFlipButtonsRoot.SetActive(GetViewerContentFlipButtonsVisible?.Invoke() ?? true);
            m_viewerContentPreviousButton.interactable = GetViewerContentPreviousButtonInteractable?.Invoke() ?? false;
            m_viewerContentNextButton.interactable = GetViewerContentNextButtonInteractable?.Invoke() ?? false;
            m_viewerContentCompleteButton.gameObject.SetActive(GetViewerContentCompleteButtonVisible?.Invoke() ?? false);
        }

        private void UpdateImageViewerSprite()
        {
            if (GetImageViewerSprite == null) return;
            m_imageViewerImage.gameObject.SetActive(true);
            var (sprite, title) = GetImageViewerSprite();
            m_imageViewerImage.sprite = sprite;
            m_viewerContentTitleText.text = title;
        }

        private void UpdateTextViewerContent()
        {
            if (GetTextViewerContent == null) return;
            m_textViewerRoot.SetActive(true);
            var (content, title) = GetTextViewerContent();
            m_textViewerText.text = content;
            m_viewerContentTitleText.text = title;
        }
#endregion


#region FG

        [FoldoutGroup("FG")]
        [SerializeField]
        private GameObject m_fgRoot;
        public void SetFGVisble(bool _isVisible)
        {
            m_fgRoot.SetActive(_isVisible);
        }

#endregion

        /// <summary>
        /// 切换步骤前，重置与步骤强关联的临时UI。
        /// </summary>
        public void ResetStepRelatedUI()
        {
            HideQuiz();
            ViewerHide();
        }
    }
}
