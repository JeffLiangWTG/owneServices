using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public interface ILearningCentreSubmittedAnswer : IVoteExamSurveySubmittedAnswer
	{
		ZBool IsAnsweredCorrectly { get; }
		ZBool IsAnsweredIncorrectly { get; }
		ZBool IsPopulated { get; }
		ZInt QuestionNumber { get; }
		IEnumerable<ZGuid> OrderedMultipleChoiceOptionPKs { get; }
	}

	public static class ILearningCentreSubmittedAnswerExtensions
	{
		public static string GetResultAsText(this ILearningCentreSubmittedAnswer submittedAnswer)
		{
			if (submittedAnswer.IsAnsweredCorrectly)
			{
				return Res.GetString("150f53ef-fe72-4bab-a85f-fd4b26187c4b", "Correct");
			}
			else if (submittedAnswer.IsPopulated)
			{
				return Res.GetString("6745d944-0c9f-4c61-a4d6-628d7e200e35", "Incorrect");
			}
			else
			{
				return Res.GetString("df78d758-0e07-4ea3-b0d6-ccc0dafd20f0", "Unanswered");
			}
		}
	}
}
