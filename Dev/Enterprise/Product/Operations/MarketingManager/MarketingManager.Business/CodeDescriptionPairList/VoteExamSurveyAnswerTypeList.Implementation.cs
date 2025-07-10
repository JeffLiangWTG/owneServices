namespace Enterprise.MarketingManager.Business
{
	public partial class VoteExamSurveyAnswerTypeList
	{
		public VoteExamSurveyAnswerTypeList(VoteExamSurveyQuestion question)
		{
			ConstructList(question);
		}

		public VoteExamSurveyAnswerTypeList(GlbCompanyCampaign campaign)
		{
			ConstructList(campaign, false);
		}

		#region Implementation

		protected virtual void ConstructList(VoteExamSurveyQuestion question)
		{
			ConstructList(question.Campaign, question.IsSubQuestion);
		}

		protected virtual void ConstructList(GlbCompanyCampaign campaign, bool isSubQuestion)
		{
			if (campaign != null)
			{
				if (campaign.IsSurveyCampaign)
				{
					ConstructListForSurveyCampaign(isSubQuestion);
				}
				else if (campaign.IsVoteCampaign)
				{
					ConstructListForVoteCampaign(isSubQuestion);
				}
			}
		}

		void ConstructListForSurveyCampaign(bool isSubQuestion)
		{
			if (isSubQuestion)
			{
				AddPair(VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption, VoteExamSurveyAnswerTypeList.Descriptions.MultipleChoiceOption);
			}
			else
			{
				AddPair(VoteExamSurveyAnswerTypeList.Codes.NumericScale, VoteExamSurveyAnswerTypeList.Descriptions.NumericScale);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, VoteExamSurveyAnswerTypeList.Descriptions.MultipleChoice);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.Percentage, VoteExamSurveyAnswerTypeList.Descriptions.Percentage);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.TrueFalse, VoteExamSurveyAnswerTypeList.Descriptions.TrueFalse);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.Header, VoteExamSurveyAnswerTypeList.Descriptions.Header);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.YesNo, VoteExamSurveyAnswerTypeList.Descriptions.YesNo);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.LikertScale, VoteExamSurveyAnswerTypeList.Descriptions.LikertScale);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.FreeText, VoteExamSurveyAnswerTypeList.Descriptions.FreeText);
			}
		}

		void ConstructListForVoteCampaign(bool isSubQuestion)
		{
			if (isSubQuestion)
			{
				AddPair(VoteExamSurveyAnswerTypeList.Codes.VotingItem, VoteExamSurveyAnswerTypeList.Descriptions.VotingItem);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.Header, VoteExamSurveyAnswerTypeList.Descriptions.Header);
			}
			else
			{
				AddPair(VoteExamSurveyAnswerTypeList.Codes.RankedVote, VoteExamSurveyAnswerTypeList.Descriptions.RankedVote);
				AddPair(VoteExamSurveyAnswerTypeList.Codes.UnrankedVote, VoteExamSurveyAnswerTypeList.Descriptions.UnrankedVote);
			}
		}

		#endregion
	}
}
