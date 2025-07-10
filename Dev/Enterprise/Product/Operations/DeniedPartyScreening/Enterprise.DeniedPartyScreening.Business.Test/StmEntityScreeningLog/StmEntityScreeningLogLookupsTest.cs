using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class StmEntityScreeningLogLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			StmEntityScreeningLog status = Factory.New<StmEntityScreeningLog>();
			Assert(status.Lookups.Statuses.Count == 17);

			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedClear));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.UpdateRelatedJobs));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.JobCleared));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.AddPartyScreen));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.RemovePartyScreen));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.UserDecisionsMarkAsSanctioned));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedMarkAsSanctioned));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.ScreeningProcessError));
			AssertEquals("Contains code", true, status.Lookups.Statuses.ContainsCode(DeniedPartyConstants.LogsScreeningStatus.ComplianceRiskSnapshot));
		}

		public void TestCodeDescriptionList()
		{
			StmEntityScreeningLog status = Factory.New<StmEntityScreeningLog>();

			AssertEquals("No Screening Performed", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed));
			AssertEquals("Screening Status Clear", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedClear));
			AssertEquals("Update of Related Jobs", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.UpdateRelatedJobs));
			AssertEquals("Requires Review - Potential Matches Found", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound));
			AssertEquals("Matched To Denied Party", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty));
			AssertEquals("Needs Re-screening - Invalidated by Local Data Changes", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges));
			AssertEquals("Needs Re-screening - Invalidated by Content Update", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate));
			AssertEquals("Screening Result Not Accepted", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedCanceled));
			AssertEquals("Screening Status Permanently Clear", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear));
			AssertEquals("Mark As Job Clear", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.JobCleared));
			AssertEquals("Party Added", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.AddPartyScreen));
			AssertEquals("Party Removed", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.RemovePartyScreen));
			AssertEquals("Marked as Sanctioned", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.UserDecisionsMarkAsSanctioned));
			AssertEquals("Sanctions Removed", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions));
			AssertEquals("Screened and Marked as Sanctioned", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.ScreenedMarkAsSanctioned));
			AssertEquals("Screening Process Error", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.ScreeningProcessError));
			AssertEquals("Compliance Risk Snapshot", status.Lookups.Statuses.GetMultilingualDescriptionFromCode(DeniedPartyConstants.LogsScreeningStatus.ComplianceRiskSnapshot));
		}
	}
}
