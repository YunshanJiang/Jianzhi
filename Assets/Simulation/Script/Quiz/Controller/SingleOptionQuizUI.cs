using System.Collections.Generic;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 单选题界面
    /// </summary>
    public class SingleOptionQuizUI : QuizUI
    {
        protected override void OnQuestionRendered(QuizData _quizData, IReadOnlyList<QuizOptionUI> _options)
        {
            base.OnQuestionRendered(_quizData, _options);
            var isContainCorrectOption = CurrentQuizData.Options.Exists(_item => _item.OptionId == CurrentQuizData.CorrectOptionId);
            if (!isContainCorrectOption)
            {
                Debug.LogError($"{QuizBankData.DisplayName} {CurrentQuizData.QuestionId} 缺少正确答案选项，无法进行单选题出题！");
            }
            isContainCorrectOption = SpawnedOptions.Exists(_item => _item.OptionId == CurrentQuizData.CorrectOptionId);
            if (!isContainCorrectOption)
            {
                Debug.LogError($"{QuizBankData.DisplayName} {CurrentQuizData.QuestionId} 已生成选项中缺少正确答案选项，无法进行单选题出题！");
            }
        }

        protected override void OnOptionSelected(QuizOptionUI _optionUI)
        {
            base.OnOptionSelected(_optionUI);
            if (CurrentQuizData?.Type != QuizType.SingleOption)
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
            if (SelectedOptions == null || SelectedOptions.Count <= 0 || string.IsNullOrEmpty(CurrentQuizData.CorrectOptionId))
                return false;
            var selectedOption = SelectedOptions[0];
            return selectedOption.OptionId == CurrentQuizData.CorrectOptionId;
        }

        protected override float CalculateScore()
        {
            return IsAnswerCorrect() ? CurrentQuizData.OptionScore : 0;
        }
    }
}
