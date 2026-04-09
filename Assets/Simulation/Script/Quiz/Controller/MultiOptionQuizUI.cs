using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 多选题界面
    /// </summary>
    public class MultiOptionQuizUI : QuizUI
    {
        protected override void OnQuestionRendered(QuizData _quizData, IReadOnlyList<QuizOptionUI> _options)
        {
            base.OnQuestionRendered(_quizData, _options);

            var correctOptionIds = new HashSet<string>(CurrentQuizData.CorrectOptionIds);
            var options = CurrentQuizData.Options.Select(_item => _item.OptionId).ToList();
            if (!correctOptionIds.SetEquals(options))
            {
                Debug.LogError($"{QuizBankData.DisplayName} {CurrentQuizData.QuestionId} 选项数量与答案不匹配！");
            }
            options = SpawnedOptions.Select(_item => _item.OptionId).ToList();
            if (!correctOptionIds.SetEquals(options))
            {
                Debug.LogError($"{QuizBankData.DisplayName} {CurrentQuizData.QuestionId} 已生成选项与答案不匹配！");
            }
        }

        protected override void OnOptionSelected(QuizOptionUI _optionUI)
        {
            base.OnOptionSelected(_optionUI);
            if (CurrentQuizData?.Type != QuizType.MultiOption)
                return;
            ToggleMultiSelection(_optionUI);
        }

        private void ToggleMultiSelection(QuizOptionUI _optionUI)
        {
            if (string.IsNullOrEmpty(_optionUI.OptionId))
                return;

            if (SelectedOptions.Contains(_optionUI))
            {
                SelectedOptions.Remove(_optionUI);
                _optionUI.SetSelected(false);
            }
            else
            {
                SelectedOptions.Add(_optionUI);
                _optionUI.SetSelected(true);

                var isCorrectOption = CurrentQuizData.CorrectOptionIds.Contains(_optionUI.OptionId);
                if (m_isTipWrongOption && !isCorrectOption)
                {
                    TipWrongOption(_optionUI);
                }
            }

            var hasSelection = SelectedOptions.Count > 0;
            var canSubmit = m_isAllCorrectShowSubmit ? hasSelection && IsAnswerCorrect() : hasSelection;
            EnableSubmitWhenReady(canSubmit);
        }

        protected override bool IsAnswerCorrect()
        {
            if (CurrentQuizData?.Type != QuizType.MultiOption)
                return false;

            var target = new HashSet<string>(CurrentQuizData.CorrectOptionIds);
            var options= SelectedOptions.Select(_item => _item.OptionId).ToList();
            return target.SetEquals(options);
        }

        protected override float CalculateScore()
        {
            if (CurrentQuizData?.Type != QuizType.MultiOption) return 0;
            var target = new HashSet<string>(CurrentQuizData.CorrectOptionIds);
            var options = SelectedOptions.Select(_item => _item.OptionId).ToList();
            var intersection = options.Intersect(target).Count();
            return CurrentQuizData.OptionScore * intersection;
        }
    }
}
