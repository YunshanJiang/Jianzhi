using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Starscape.Simulation
{
    public delegate void QuizStartHandler(QuizUI _quizUI);
    public delegate void QuizEndHandler(QuizUI _quizUI);

    /// <summary>
    /// 题目UI
    /// </summary>
    public abstract class QuizUI : MonoBehaviour
    {
        [TitleGroup("配置")]
        [LabelText("UI根节点")]
        [SerializeField]
        protected GameObject m_root;
        [TitleGroup("配置")]
        [LabelText("选项预制体")][PropertyTooltip("根节点必须包含Button组件")]
        [SerializeField]
        protected GameObject m_optionPrefab;
        [TitleGroup("配置")]
        [LabelText("选项容器")]
        [SerializeField]
        protected Transform m_optionParent;
        [TitleGroup("配置")]
        [LabelText("循环出题")][PropertyTooltip("true: 题目答完后从头开始出题；false: 题目答完后不再出题") ]
        [SerializeField]
        protected bool m_isLoopQuiz;
        [TitleGroup("配置")]
        [LabelText("全部正确时才显示提交按钮")]
        [SerializeField]
        protected bool m_isAllCorrectShowSubmit;
        [TitleGroup("配置")]
        [LabelText("答题完成自动隐藏")]
        [SerializeField]
        protected bool m_quizCompleteAutoHide = true;
        [TitleGroup("配置")]
        [LabelText("提示错误选项")]
        [SerializeField]
        protected bool m_isTipWrongOption = true;

        [TitleGroup("UI")]
        [SerializeField]
        protected TextMeshProUGUI m_questionTitleText;
        [TitleGroup("UI")]
        [SerializeField]
        protected Button m_submitButton;

        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        protected QuizBankData QuizBankData { get; private set; }
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        protected int CurrentQuestionIndex { get; private set; }
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        protected QuizData CurrentQuizData { get; private set; }
        /// <summary>
        /// true: 已完成所有题目
        /// </summary>
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        public bool IsFinish { get; private set; }
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        protected readonly List<QuizOptionUI> SelectedOptions = new();
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        protected readonly List<QuizOptionUI> SpawnedOptions = new();
        [TitleGroup("运行时")]
        [ShowInInspector][ReadOnly]
        private readonly List<int> m_optionIndexBuffer = new();

        [TitleGroup("Events")]
        public event QuizStartHandler OnQuizStartEvent, OnQuizEndEvent;

        protected virtual void Awake()
        {
            if (m_submitButton != null)
            {
                m_submitButton.onClick.AddListener(OnSubmitClicked);
            }
        }

        private void OnValidate()
        {
            if (m_optionPrefab != null && !m_optionPrefab.TryGetComponent<QuizOptionUI>(out _))
            {
                Debug.LogError("选项预制体必须包含QuizOptionUI组件！");
            }
        }

        /// <summary>
        /// 设置题库数据
        /// </summary>
        /// <param name="_quizBankData"></param>
        public void SetData(QuizBankData _quizBankData)
        {
            QuizBankData = _quizBankData;
        }

        /// <summary>
        /// 显示题目UI
        /// </summary>
        /// <param name="_questionIndex"></param>
        public void Display(string _questionId)
        {
            m_root.SetActive(true);
            OnQuizStartEvent?.Invoke(this);
            CurrentQuestionIndex = QuizBankData.Questions.FindIndex(_item => _item.QuestionId == _questionId);
            PrepareSubmitButton(false);
            UpdateQuestion();
        }

        /// <summary>
        /// 隐藏题目UI
        /// </summary>
        public void Hide()
        {
            m_root.SetActive(false);
        }

        /// <summary>
        /// 上一题
        /// </summary>
        public void PreviousQuestion()
        {
            CurrentQuestionIndex--;
            UpdateQuestion();
        }

        /// <summary>
        /// 下一题
        /// </summary>
        public void NextQuestion()
        {
            CurrentQuestionIndex++;
            if (CurrentQuestionIndex >= QuizBankData.Questions.Count)
            {
                if (m_isLoopQuiz)
                {
                    CurrentQuestionIndex = 0;
                }
                else
                {
                    QuizComplete();
                    return;
                }
            }
            PrepareSubmitButton(false);
            UpdateQuestion();
        }

        private void QuizComplete()
        {
            OnQuizEndEvent?.Invoke(this);
            if (m_quizCompleteAutoHide)
            {
                Hide();
            }
            IsFinish = true;
        }

        private void PrepareSubmitButton(bool _isActive)
        {
            if (m_submitButton == null)
                return;
            m_submitButton.gameObject.SetActive(_isActive);
            m_submitButton.interactable = _isActive;
        }

        private void UpdateQuestion()
        {
            ClearOptions();
            CurrentQuizData = QuizBankData.Questions[CurrentQuestionIndex];
            m_questionTitleText.text = CurrentQuizData.Title;
            OnQuestionWillRender(CurrentQuizData);
            var index = 0;
            var optionOrder = BuildOptionOrder(CurrentQuizData);
            foreach (var optionIndex in optionOrder)
            {
                var optionData = CurrentQuizData.Options[optionIndex];
                var optionGameObject = Instantiate(m_optionPrefab, m_optionParent);
                var optionUI = optionGameObject.GetComponent<QuizOptionUI>();
                optionUI.SetData(index, optionData, OnOptionSelected);
                SpawnedOptions.Add(optionUI);
                index++;
            }
            OnQuestionRendered(CurrentQuizData, SpawnedOptions);
        }

        private List<int> BuildOptionOrder(QuizData _quizData)
        {
            m_optionIndexBuffer.Clear();
            for (var i = 0; i < _quizData.Options.Count; i++)
            {
                m_optionIndexBuffer.Add(i);
            }

            if (_quizData.RandomizeOptions)
            {
                for (var i = m_optionIndexBuffer.Count - 1; i > 0; i--)
                {
                    var swapIndex = Random.Range(0, i + 1);
                    (m_optionIndexBuffer[i], m_optionIndexBuffer[swapIndex]) = (m_optionIndexBuffer[swapIndex], m_optionIndexBuffer[i]);
                }
            }

            return m_optionIndexBuffer;
        }

        protected virtual void OnQuestionWillRender(QuizData _quizData)
        {
        }

        protected virtual void OnQuestionRendered(QuizData _quizData, IReadOnlyList<QuizOptionUI> _options)
        {
            ClearSelectedOptions();
            foreach (var option in _options)
            {
                option.ResetVisual();
                option.SetInteractable(true);
            }
            EnableSubmitWhenReady(false);
        }

        protected void ClearOptions()
        {
            foreach (Transform child in m_optionParent)
            {
                Destroy(child.gameObject);
            }
            SpawnedOptions.Clear();
        }

        protected void ClearSelectedOptions()
        {
            SelectedOptions.Clear();
        }

        protected virtual void OnOptionSelected(QuizOptionUI _optionUI)
        {
            Debug.Log($"OnOptionSelected: {_optionUI.OptionId} {_optionUI.OptionData.Text}");
        }

        protected void EnableSubmitWhenReady(bool _isReady)
        {
            PrepareSubmitButton(_isReady);
        }

        protected abstract bool IsAnswerCorrect();

        protected abstract float CalculateScore();

        private void OnSubmitClicked()
        {
            var score = CalculateScore();
            OnAnswerSubmitted(score);
            PrepareSubmitButton(false);
            if (GameManager.Instance.UIManager.IsAutoNextQuestion)
            {
                NextQuestion();
            }
            else
            {
                QuizComplete();
            }
        }

        protected virtual void OnAnswerSubmitted(float _score)
        {
            Debug.Log($"AnswerSubmitted: Score = {_score}");
            // Override to record score or analytics
        }

        protected void TipWrongOption(QuizOptionUI _optionUI)
        {
            if (!m_isTipWrongOption) return;
            _optionUI.ShowWrongSelection();
        }

        protected QuizOptionUI FindOptionById(string _optionId)
        {
            if (string.IsNullOrEmpty(_optionId))
                return null;
            foreach (var option in SpawnedOptions)
            {
                if (option.OptionId == _optionId)
                    return option;
            }
            return null;
        }
    }
}

