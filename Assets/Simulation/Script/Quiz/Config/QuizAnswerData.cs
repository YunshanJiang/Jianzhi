using System;

namespace Starscape.Simulation
{
    /// <summary>
    /// 答题记录
    /// </summary>
    [Serializable]
    public struct QuizAnswerData
    {
        public string BankId;
        public string QuestionId;
        public string SelectedOptionId;
    }
}
