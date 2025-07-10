using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignItemValidation : AutoGlbCompanyCampaignItemValidation
	{
		public GlbCompanyCampaignItemValidation(AutoGlbCompanyCampaignItem parent) : base(parent)
		{
		}

		#region CheckG8_ClosedDate

		protected override void CheckG8_ClosedDateUtc()
		{
			base.CheckG8_ClosedDateUtc();
			CheckVotingNominations();
		}

		void CheckVotingNominations()
		{
			if (Globals.IsWeb && Parent.CompanyCampaign != null && Parent.CompanyCampaign.IsVoteCampaign && Parent.CompanyCampaign.VoteHeader != null && Parent.G8_ClosedDateUtc.IsValid)
			{
				var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Parent.Factory, Parent);
				var votedItems = voteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(Parent);
				if (Parent.CompanyCampaign.VoteHeader.HY_Min > votedItems.Count() || votedItems.Count() > Parent.CompanyCampaign.VoteHeader.HY_Max)
				{
					string errorMessage = (Parent.CompanyCampaign.VoteHeader.HY_Min != Parent.CompanyCampaign.VoteHeader.HY_Max)
						? Res.GetString("055088a6-5fa5-4513-9da6-9b2fce5c8a56", "You need to nominate at least {0} and at most {1} votes.", Parent.CompanyCampaign.VoteHeader.HY_Min, Parent.CompanyCampaign.VoteHeader.HY_Max)
						: Res.GetString("dbb0958c-d112-4213-a827-4289c278a3c3", "You need to nominate {0} votes.", Parent.CompanyCampaign.VoteHeader.HY_Min);
					errorMessage += " " + Res.GetString("783533dd-1928-498f-8ebd-b422c135358b", "You have nominated {0} so far", votedItems.Count());
					Parent.G8_ClosedDateUtcInfo.AddError(errorMessage);
				}
				else if (Parent.CompanyCampaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote)
				{
					CheckVotingNominationOrder(votedItems);
				}
			}
		}

		void CheckVotingNominationOrder(IEnumerable<VoteExamSurveyAnswer> votedItems)
		{
			bool rankNotFound = false;
			for (int i = 1; i <= votedItems.Count(); i++)
			{
				if (!votedItems.Any(v => v.AnswerAsInt == i))
				{
					rankNotFound = true;
					break;
				}
			}

			if (rankNotFound)
			{
				string errorMessage = Res.GetString("8e116c19-4edb-4ac9-8628-16b5c4d049c3", "You have nominated {0} votes. Please rank them with value between 1 to {0}", votedItems.Count());
				Parent.G8_ClosedDateUtcInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckG8_GS_NKFollowedUpBy

		protected override void CheckG8_GS_NKFollowedUpBy()
		{
			base.CheckG8_GS_NKFollowedUpBy();
			ListValidation.ErrorIfInvalidCode(Parent.G8_GS_NKFollowedUpByInfo, Parent.Lookups.FollowedUpBys);
		}

		#endregion

		#region CheckG8_GS_NKSender

		protected override void CheckG8_GS_NKSender()
		{
			base.CheckG8_GS_NKSender();
			ListValidation.ErrorIfInvalidCode(Parent.G8_GS_NKSenderInfo, Parent.Lookups.Senders);
		}

		#endregion

		#region CheckG8_SenderEmailAddress

		static MultilingualString InvalidEmailMessage { get; } = ResString.GetMultilingualString("cb85b495-cf2d-43f3-bd04-f5d048647af4", "The entered email address is not valid.");

		protected override void CheckG8_SenderEmailAddress()
		{
			base.CheckG8_SenderEmailAddress();
			if (!Parent.G8_SenderEmailAddress.IsEmpty && !EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.G8_SenderEmailAddress))
			{
				Parent.G8_SenderEmailAddressInfo.AddError(InvalidEmailMessage);
			}
		}

		#endregion

		protected new GlbCompanyCampaignItem Parent
		{
			get { return (GlbCompanyCampaignItem)base.Parent; }
		}
	}
}
