using System.Collections.Generic;
using UnityEngine;

namespace Starscape.Simulation
{
    /// <summary>
    /// 题库
    /// </summary>
    [CreateAssetMenu(menuName = "StarscapeCulture/Simulation/Quiz Question Bank", fileName = "QuizQuestionBank")]
    public class QuizBankData : ScriptableObject
    {
        public string BankId;
        public string DisplayName;
        public List<QuizData> Questions = new();
    }
}
