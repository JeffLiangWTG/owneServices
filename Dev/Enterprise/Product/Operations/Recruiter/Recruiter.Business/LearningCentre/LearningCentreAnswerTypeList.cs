using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreAnswerTypeList : VoteExamSurveyAnswerTypeList
	{
		public new class Codes : VoteExamSurveyAnswerTypeList.Codes
		{
			public const string ScaleRange = "SCR";
		}

		public LearningCentreAnswerTypeList(VoteExamSurveyQuestion question)
			: base(question)
		{
		}

		public LearningCentreAnswerTypeList(LearningCentreCampaign campaign)
			: base(campaign)
		{
		}

		protected override void ConstructList(VoteExamSurveyQuestion question)
		{
			if (question.HY_AnswerType == Codes.ScaleRange)
			{
				AddPair(Codes.ScaleRange);
			}
			else
			{
				base.ConstructList(question);
			}
		}

		protected override void ConstructList(GlbCompanyCampaign campaign, bool isSubQuestion)
		{
			if (!isSubQuestion)
			{
				AddPair(VoteExamSurveyAnswerTypeList.Codes.Header, Descriptions.Header);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, Descriptions.MultipleChoice);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.YesNo, Descriptions.YesNo);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.TrueFalse, Descriptions.TrueFalse);

				if (campaign != null && campaign.G0_Type == LearningCentreTestTypes.Codes.Exam)
				{
					AddPair(VoteExamSurveyAnswerTypeList.Codes.NumericScale, Descriptions.NumericScale);
				}
				else
				{
					AddPair(VoteExamSurveyAnswerTypeList.Codes.LikertScale, Descriptions.LikertScale);
				}
			}
			else
			{
				AddPair(VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption);
			}
		}
	}
}
