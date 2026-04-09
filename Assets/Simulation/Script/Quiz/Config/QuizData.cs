using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Starscape.Simulation
{
    public enum QuizType
    {
        [LabelText("单选题")]
        SingleOption,
        [LabelText("多选题")]
        MultiOption,
        [LabelText("排序题")]
        Sequence,
        [LabelText("判断题")]
        TrueFalse,
    }
    /// <summary>
    /// 题目
    /// </summary>
    [Serializable]
    public class QuizData
    {
        public string QuestionId;
        public QuizType Type;
        [MultiLineProperty(2)]
        public string Title;
        public bool RandomizeOptions = true;
        public int OptionScore = 1;
        public List<QuizOptionData> Options = new();

        [ShowIf("Type", QuizType.SingleOption)]
        [ValueDropdown("GetOptionIdDropdownItems")]
        public string CorrectOptionId;

        [ShowIf("Type", QuizType.MultiOption)]
        [ValueDropdown("GetOptionIdDropdownItems")]
        public List<string> CorrectOptionIds = new();

        [ShowIf("Type", QuizType.Sequence)]
        [ValueDropdown("GetOptionIdDropdownItems")]
        public List<string> CorrectOptionSequenceIds = new();

        [ShowIf("Type", QuizType.TrueFalse)]
        public bool CorrectTrueFalse;

        private IEnumerable<ValueDropdownItem<string>> GetOptionIdDropdownItems()
        {
            foreach (var option in Options)
            {
                if (option == null)
                    continue;
                var label = string.IsNullOrWhiteSpace(option.Text) ? option.OptionId : option.Text.Trim();
                yield return new ValueDropdownItem<string>($"{label}", option.OptionId);
            }
        }
    }
}
