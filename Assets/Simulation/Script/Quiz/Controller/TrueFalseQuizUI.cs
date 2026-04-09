using System.Collections.Generic;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 判断题UI
    /// </summary>
    public class TrueFalseQuizUI : QuizUI
    {
        private string m_trueOptionId;
        private string m_falseOptionId;

        protected override void OnQuestionRendered(QuizData _quizData, IReadOnlyList<QuizOptionUI> _options)
        {
            base.OnQuestionRendered(_quizData, _options);
            if (_quizData?.Type != QuizType.TrueFalse)
                return;

            if (_quizData.Options.Count < 2)
            {
                Debug.LogError($"{QuizBankData.DisplayName} {_quizData.QuestionId} 判断题缺少选项，至少需要两个选项表示对/错！");
                m_trueOptionId = null;
                m_falseOptionId = null;
                return;
            }

            m_trueOptionId = _quizData.Options[0]?.OptionId;
            m_falseOptionId = _quizData.Options.Count > 1 ? _quizData.Options[1]?.OptionId : null;

            if (string.IsNullOrEmpty(m_trueOptionId) || string.IsNullOrEmpty(m_falseOptionId))
            {
                Debug.LogError($"{QuizBankData.DisplayName} {_quizData.QuestionId} 判断题选项缺少OptionId，无法判断对错！");
            }

            if (_options.Count != 2)
            {
                Debug.LogWarning($"{QuizBankData.DisplayName} {_quizData.QuestionId} 判断题应当只生成两个选项，当前数量：{_options.Count}");
            }
        }

        protected override void OnOptionSelected(QuizOptionUI _optionUI)
        {
            base.OnOptionSelected(_optionUI);
            if (CurrentQuizData?.Type != QuizType.TrueFalse)
                return;

            SelectedOptions.Clear();
            SelectedOptions.Add(_optionUI);
            foreach (var option in SpawnedOptions)
            {
                option.SetSelected(option == _optionUI);
            }

            if (m_isTipWrongOption && !IsAnswerCorrect())
            {
                TipWrongOption(_optionUI);
            }

            var canSubmit = m_isAllCorrectShowSubmit ? IsAnswerCorrect() : SelectedOptions.Count > 0;
            EnableSubmitWhenReady(canSubmit);
        }

        protected override bool IsAnswerCorrect()
        {
            if (CurrentQuizData?.Type != QuizType.TrueFalse)
                return false;
            if (SelectedOptions.Count == 0)
                return false;

            if (!TryGetOptionValue(SelectedOptions[0], out var selectedValue))
                return false;

            return selectedValue == CurrentQuizData.CorrectTrueFalse;
        }

        protected override float CalculateScore()
        {
            return IsAnswerCorrect() ? CurrentQuizData.OptionScore : 0;
        }

        private bool TryGetOptionValue(QuizOptionUI _optionUI, out bool _value)
        {
            _value = false;
            if (_optionUI == null || string.IsNullOrEmpty(_optionUI.OptionId))
                return false;

            if (!string.IsNullOrEmpty(m_trueOptionId) && _optionUI.OptionId == m_trueOptionId)
            {
                _value = true;
                return true;
            }

            if (!string.IsNullOrEmpty(m_falseOptionId) && _optionUI.OptionId == m_falseOptionId)
            {
                _value = false;
                return true;
            }

            return false;
        }
    }
}
