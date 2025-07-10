using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyAnswerValidation : AutoVoteExamSurveyAnswerValidation
	{
		public VoteExamSurveyAnswerValidation(AutoVoteExamSurveyAnswer parent) : base(parent)
		{
		}

		protected override void CheckHZ_Answer()
		{
			base.CheckHZ_Answer();

			if (Globals.IsWeb && !Parent.HZ_Answer.IsEmpty && ShouldValidateRanking)
			{
				CompareValidation.CheckWithinRange(Parent.AnswerAsIntInfo, 1, VoteHeader.HY_Max);
				var otherVotes = Parent.GetOtherVotesWithRanking(Parent.HZ_Answer);
				if (otherVotes.Any())
				{
					Parent.HZ_AnswerInfo.AddError(Res.GetString("bce664d1-a27c-4653-b851-7a4e5e55670c", "Rank number {0} is nominated to more than one voting item", Parent.HZ_Answer));
					foreach (VoteExamSurveyAnswer otherVote in otherVotes)
					{
						otherVote.Validation.ValidateHZ_Answer();
					}
				}
			}
		}

		bool ShouldValidateRanking
		{
			get
			{
				return
					Parent.Question != null &&
					Parent.Question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.VotingItem &&
					Campaign != null &&
					Campaign.G0_BroadcastVoteSurveyExam == CampaignTypeList.Codes.Voting &&
					VoteHeader != null &&
					VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			}
		}

		GlbCompanyCampaign Campaign
		{
			get { return Parent.Question != null ? Parent.Question.Campaign : null; }
		}

		VoteExamSurveyQuestion VoteHeader
		{
			get { return (Campaign != null) ? Campaign.VoteHeader : null; }
		}

		protected new VoteExamSurveyAnswer Parent
		{
			get { return (VoteExamSurveyAnswer)base.Parent; }
		}
	}
}
