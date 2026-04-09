namespace Starscape.Simulation
{
    public class QuizBankUI : QuizUI
    {
        protected override bool IsAnswerCorrect()
        {
            return false;
        }

        protected override float CalculateScore()
        {
            return 0;
        }
    }
}
