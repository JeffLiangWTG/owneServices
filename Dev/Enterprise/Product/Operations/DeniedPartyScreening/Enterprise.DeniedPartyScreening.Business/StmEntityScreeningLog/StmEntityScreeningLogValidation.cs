//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmEntityScreeningLogValidation
//
//    This class should be used for overriding validation in AutoStmEntityScreeningLogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DeniedPartyScreening.Business
{
	using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

	public class StmEntityScreeningLogValidation : AutoStmEntityScreeningLogValidation
	{
		public StmEntityScreeningLogValidation(AutoStmEntityScreeningLog parent) : base(parent)
		{
		}

		protected override void CheckPJ_HighConfidenceResults()
		{
			if (ShouldCheck && Parent.PJ_HighConfidenceResults.IsEmpty)
			{
				Parent.PJ_HighConfidenceResultsInfo.AddError(Res.GetString("153A1AA1-D3EE-40F9-9FF1-C82B0AFBEB7D", "Suggested High Confidence Matches can't be empty"));
			}
		}

		protected override void CheckPJ_MediumConfidenceResults()
		{
			if (ShouldCheck && Parent.PJ_MediumConfidenceResults.IsEmpty)
			{
				Parent.PJ_MediumConfidenceResultsInfo.AddError(Res.GetString("4C36A766-5C4A-4596-BF7E-6FF652E1EB22", "Suggested Medium Confidence Matches can't be empty"));
			}
		}

		protected override void CheckPJ_ExcludedLists()
		{
			if (ShouldCheck && Parent.PJ_ExcludedLists.IsEmpty)
			{
				Parent.PJ_ExcludedListsInfo.AddError(Res.GetString("44C403A6-CF63-41A5-ACAC-BBBC2126F1DB", "Excluded Lists can't be empty"));
			}
		}

		protected override void CheckPJ_IncludedLists()
		{
			if (ShouldCheck && Parent.PJ_IncludedLists.IsEmpty)
			{
				Parent.PJ_IncludedListsInfo.AddError(Res.GetString("5C6F9131-5DFF-4C84-8F4D-ECF7558B88B2", "Included Lists can't be empty"));
			}
		}

		bool ShouldCheck => Parent.PJ_Status == LogsScreeningStatus.ScreenedClear || Parent.PJ_Status == LogsScreeningStatus.MatchedDeniedParty || Parent.PJ_Status == LogsScreeningStatus.ScreenedCanceled || Parent.PJ_Status == LogsScreeningStatus.PotentialMatchesFound;
	}
}
