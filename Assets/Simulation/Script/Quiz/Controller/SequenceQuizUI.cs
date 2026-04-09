using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 序列题界面
    /// </summary>
    public class SequenceQuizUI : QuizUI
    {
        protected override void OnQuestionRendered(QuizData _quizData, IReadOnlyList<QuizOptionUI> _options)
        {
            base.OnQuestionRendered(_quizData, _options);
            foreach (var option in _options)
            {
                option.SetIndexText(string.Empty);
            }

            if (CurrentQuizData.Options.Count != CurrentQuizData.CorrectOptionSequenceIds.Count)
            {
                Debug.LogError($"{QuizBankData.DisplayName} {CurrentQuizData.QuestionId} 选项数量与答案数量不匹配！");
            }
            if (CurrentQuizData.Options.Count != SpawnedOptions.Count)
            {
                Debug.LogError($"{QuizBankData.DisplayName} {CurrentQuizData.QuestionId} 选项数量与已生成选项数量不匹配！");
            }
        }

        protected override void OnOptionSelected(QuizOptionUI _optionUI)
        {
            base.OnOptionSelected(_optionUI);
            if (CurrentQuizData?.Type != QuizType.Sequence)
                return;

            if (SelectedOptions.Contains(_optionUI))
            {
                SelectedOptions.Remove(_optionUI);
                _optionUI.SetSelected(false);
                _optionUI.SetIndexText(string.Empty);
            }
            else
            {
                SelectedOptions.Add(_optionUI);
                _optionUI.SetSelected(true);
            }

            RefreshSelectionIndices();
            if (SelectedOptions.Count == CurrentQuizData.Options.Count)
            {
                if (m_isAllCorrectShowSubmit)
                {
                    EnableSubmitWhenReady(IsAnswerCorrect());
                }
                else
                {
                    EnableSubmitWhenReady(true);
                }
            }
            else
            {
                EnableSubmitWhenReady(false);
            }
        }

        private void RefreshSelectionIndices()
        {
            for (var i = 0; i < SelectedOptions.Count; i++)
            {
                var selectOption = SelectedOptions[i];
                selectOption.SetIndexText((i + 1).ToString());
                selectOption.SetSelected(true);
                if (m_isTipWrongOption)
                {
                    var correctOptionIds = CurrentQuizData.CorrectOptionSequenceIds;
                    var isCorrect = i < correctOptionIds.Count && selectOption.OptionId == correctOptionIds[i];
                    if (!isCorrect)
                    {
                        TipWrongOption(selectOption);
                    }
                }
            }
        }

        protected override bool IsAnswerCorrect()
        {
            if (CurrentQuizData?.CorrectOptionSequenceIds == null)
                return false;

            if (SelectedOptions.Count != CurrentQuizData.CorrectOptionSequenceIds.Count)
                return false;

            for (var i = 0; i < SelectedOptions.Count; i++)
            {
                var expectedId = CurrentQuizData.CorrectOptionSequenceIds[i];
                if (SelectedOptions[i].OptionId != expectedId)
                {
                    return false;
                }
            }

            return true;
        }

        protected override float CalculateScore()
        {
            if (CurrentQuizData?.Type != QuizType.Sequence) return 0;
            var expected = CurrentQuizData.CorrectOptionSequenceIds;
            var score = 0;
            var maxSteps = Mathf.Min(expected.Count, SelectedOptions.Count);
            for (var i = 0; i < maxSteps; i++)
            {
                if (SelectedOptions[i].OptionId == expected[i])
                {
                    score += CurrentQuizData.OptionScore;
                }
            }

            return score;
        }
    }
}
