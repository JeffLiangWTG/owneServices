using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public interface IVoteExamSurveySubmittedAnswer : IBusiness
	{
		ZString PersistedAnswer { get; }
		VoteExamSurveyQuestion Question { get; }
		IEnumerable<VoteExamSurveyQuestion> SelectedMultipleChoiceOptions { get; }
	}

	#region Extension Methods

	public static class IVoteExamSurveySubmittedAnswerExtensions
	{
		public static string GetAnswerFieldType(this IVoteExamSurveySubmittedAnswer submittedAnswer)
		{
			string result = nameof(FieldType.Text);

			VoteExamSurveyQuestion question = submittedAnswer.Question;
			if (question != null)
			{
				switch (question.HY_AnswerType)
				{
					case VoteExamSurveyAnswerTypeList.Codes.VotingItem:
						if (question.Campaign != null && question.Campaign.VoteHeader != null && question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote)
						{
							result = nameof(FieldType.Integer);
						}
						break;

					case VoteExamSurveyAnswerTypeList.Codes.FreeText:
					case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
						result = nameof(FieldType.TextMultiLine);
						break;

					case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
					case VoteExamSurveyAnswerTypeList.Codes.Percentage:
						result = nameof(FieldType.Integer);
						break;
				}
			}

			return result;
		}

		public static string GetAnswer(this IVoteExamSurveySubmittedAnswer submittedAnswer)
		{
			VoteExamSurveyQuestion question = submittedAnswer.Question;
			return (question != null && question.IsMultipleChoiceQuestion) ? GetAnswerAsStringFromSelectedOptions(submittedAnswer) : GetAnswerAsStringFromQuestion(submittedAnswer);
		}

		public static bool IsPopulated(this IVoteExamSurveySubmittedAnswer submittedAnswer)
		{
			return (submittedAnswer.Question.IsMultipleChoiceQuestion)
					? submittedAnswer.SelectedMultipleChoiceOptions.Any()
					: !submittedAnswer.PersistedAnswer.IsEmpty;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		static string GetAnswerAsStringFromSelectedOptions(IVoteExamSurveySubmittedAnswer submittedAnswer)
		{
			StringBuilder result = new StringBuilder();

			foreach (VoteExamSurveyQuestion multipleChoiceOption in submittedAnswer.SelectedMultipleChoiceOptions)
			{
				result.Append(multipleChoiceOption.HY_SubQuestionOrder);
				result.Append(") ");
				result.AppendLine(multipleChoiceOption.HY_Question);
			}

			return result.ToString();
		}

		static string GetAnswerAsStringFromQuestion(IVoteExamSurveySubmittedAnswer submittedAnswer)
		{
			string result = null;

			VoteExamSurveyQuestion question = submittedAnswer.Question;
			string persistedAnswer = submittedAnswer.PersistedAnswer;

			if (question != null && !string.IsNullOrEmpty(persistedAnswer))
			{
				switch (question.HY_AnswerType)
				{
					case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
					case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
					case VoteExamSurveyAnswerTypeList.Codes.YesNo:
						int persistedAnswerAsInt;
						int.TryParse(submittedAnswer.PersistedAnswer, out persistedAnswerAsInt);
						if (persistedAnswerAsInt > 0)
						{
							CodeDescriptionPairList codePairList = (CodeDescriptionPairList)VoteExamSurveyAnswerOptionListHelper.GetAnswerOptionList(question);
							ICodeDescription codePair = codePairList[persistedAnswerAsInt - 1];
							result = string.IsNullOrEmpty(codePair.Description) ? codePair.Code : codePair.Description;
						}
						break;

					default:
						result = submittedAnswer.PersistedAnswer;
						break;
				}
			}

			return result;
		}
	}

	#endregion
}
