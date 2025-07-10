using System;
using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public static class VoteExamSurveyAnswerOptionListHelper
	{
		public static ICollection GetAnswerOptionList(VoteExamSurveyQuestion question)
		{
			return GetAnswerOptionList(question, true);
		}

		public static ICollection GetAnswerOptionList(VoteExamSurveyQuestion question, bool insertEmptyOptionForNumericList)
		{
			if (question == null)
			{
				throw new ArgumentNullException(nameof(question));
			}

			return (question.IsVotingItem && question.Campaign != null)
				? ConstructNumericList(1, question.Campaign.VoteHeader.HY_Max, 1, insertEmptyOptionForNumericList)
				: GetAnswerOptionListForExamSurvey(question, insertEmptyOptionForNumericList);
		}

		static ICollection GetAnswerOptionListForExamSurvey(VoteExamSurveyQuestion question, bool insertEmptyOptionForNumericList)
		{
			ICollection result;

			switch (question.HY_AnswerType)
			{
				case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
					result = ConstructMultipleChoiceList(Res.GetString("6f58f7be-3f3b-46d2-aafa-f112fa210fbd", "Strongly Disagree"), Res.GetString("81a899a7-c731-4d4c-86e2-d8fb31bd5767", "Disagree"), Res.GetString("3d93fd47-dff9-42e9-81c8-a6aeb49f6af6", "Undecided"), Res.GetString("784822dd-a238-4d4b-ac36-fdac7241647f", "Agree"), Res.GetString("0ac3b20a-f261-4361-9b7e-863369bbedfa", "Strongly Agree"));
					break;

				case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
					result = question.SubQuestions;
					break;

				case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
					result = ConstructNumericList(question.HY_Min, question.HY_Max, 1, insertEmptyOptionForNumericList);
					break;

				case VoteExamSurveyAnswerTypeList.Codes.Percentage:
					result = ConstructNumericList(0, 100, 5, insertEmptyOptionForNumericList);
					break;

				case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
					result = ConstructMultipleChoiceList(Res.GetString("c2c64d4e-fa9a-47dd-9fcd-4a31cad56214", "True"), Res.GetString("c8ec8bf6-b4dc-410e-bc76-b02e57ff5e5b", "False"));
					break;

				case VoteExamSurveyAnswerTypeList.Codes.YesNo:
					result = ConstructMultipleChoiceList(Res.GetString("eb8168ee-4543-4257-9e8b-e55755b213ea", "Yes"), Res.GetString("d744f6d9-4ba1-49ce-a75e-1faaaf51ce9b", "No"));
					break;

				default:
					result = new CodeDescriptionPairList();
					break;
			}

			return result;
		}

		static CodeDescriptionPairList ConstructMultipleChoiceList(params string[] options)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			for (int i = 0; i < options.Length; i++)
			{
				string optionNo = (i + 1).ToString();
				result.AddPair(optionNo, options[i]);
			}
			return result;
		}

		static CodeDescriptionPairList ConstructNumericList(int minValue, int maxValue, int interval, bool insertEmptyOptionForNumericList)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (insertEmptyOptionForNumericList)
			{
				result.AddPair("");
			}

			for (int i = minValue; i <= maxValue; i += interval)
			{
				result.AddPair(i.ToString());
			}
			return result;
		}
	}
}
