using System;
using Sirenix.OdinInspector;

namespace Starscape.Simulation
{
    /// <summary>
    /// 题目选项
    /// </summary>
    [Serializable]
    public class QuizOptionData
    {
        public string OptionId;
        [MultiLineProperty]
        public string Text;
        public int Score;
    }
}
