using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DeniedPartyGridColorHelperTest : TestCaseWithFactory
	{
		#region TestGetColorForResultStatus

		public void TestGetColorForResultStatus_ComplianceRiskParty()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = "CLR";
			var screeningPartyWrapper = new ScreeningPartyWrapper(new ScreeningParty(org, "party", org));
			AssertEquals(DeniedPartyConstants.GridColor.Clear, new DeniedPartyGridColorHelper().GetColorForResultStatus(screeningPartyWrapper));

			org.OH_ScreeningStatus = "UNK";
			screeningPartyWrapper = new ScreeningPartyWrapper(new ScreeningParty(org, "party", org));
			AssertEquals(DeniedPartyConstants.GridColor.Unknown, new DeniedPartyGridColorHelper().GetColorForResultStatus(screeningPartyWrapper));
		}

		public void TestGetColorForResultStatus_Release()
		{
			AssertEquals(DeniedPartyConstants.GridColor.Release, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.Release));
		}

		public void TestGetColorForResultStatus_Block()
		{
			AssertEquals(DeniedPartyConstants.GridColor.Block, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.Block));
		}

		public void TestGetColorForResultStatus_JobCleared()
		{
			AssertEquals(DeniedPartyConstants.GridColor.JobCleared, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.JobCleared));
		}

		public void TestGetColorForResultStatus_MatchedStatus()
		{
			AssertEquals(DeniedPartyConstants.GridColor.Matched, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.Matched));
		}

		public void TestGetColorForResultStatus_ClearStatus()
		{
			AssertEquals(DeniedPartyConstants.GridColor.Clear, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.Clear));
		}

		public void TestGetColorForResultStatus_UnknownStatus()
		{
			AssertEquals(DeniedPartyConstants.GridColor.Unknown, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.Unknown));
		}

		public void TestGetColorForResultStatus_PermanentClearStatus()
		{
			AssertEquals(DeniedPartyConstants.GridColor.PermanentClear, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.PermanentClear));
		}

		public void TestGetColorForResultStatus_NotScreenedStatus()
		{
			AssertEquals(DeniedPartyConstants.GridColor.NotScreened, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.NotScreened));
		}

		public void TestGetColorForResultStatus_JobClearedExternal()
		{
			AssertEquals(DeniedPartyConstants.GridColor.JobClearedExternal, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.JobClearedExternal));
		}

		public void TestGetColorForResultStatus_JobBlockedExternal()
		{
			AssertEquals(DeniedPartyConstants.GridColor.JobBlockedExternal, new DeniedPartyGridColorHelper().GetColorForResultStatus(ScreeningStatusesList.Codes.JobBlockedExternal));
		}

		#endregion

		#region ScreeningPartyWrapper

		public void TestGetColorForResultStatus_ScreeningPartyWrapper_BlockStatus()
		{
			AssertGetColorForResultStatus_ForScreeningPartyWrapper(ScreeningStatusesList.Codes.Block, DeniedPartyConstants.GridColor.Block);
		}

		public void TestGetColorForResultStatus_ScreeningPartyWrapper_ReleaseStatus()
		{
			AssertGetColorForResultStatus_ForScreeningPartyWrapper(ScreeningStatusesList.Codes.Release, DeniedPartyConstants.GridColor.Release);
		}

		public void TestGetColorForResultStatus_ScreeningPartyWrapper_MatchedStatus()
		{
			AssertGetColorForResultStatus_ForScreeningPartyWrapper(ScreeningStatusesList.Codes.Matched, DeniedPartyConstants.GridColor.Matched);
		}

		public void TestGetColorForResultStatus_ScreeningPartyWrapper_ClearStatus()
		{
			AssertGetColorForResultStatus_ForScreeningPartyWrapper(ScreeningStatusesList.Codes.Clear, DeniedPartyConstants.GridColor.Clear);
		}

		public void TestGetColorForResultStatus_ScreeningPartyWrapper_UnknownStatus()
		{
			AssertGetColorForResultStatus_ForScreeningPartyWrapper(ScreeningStatusesList.Codes.Unknown, DeniedPartyConstants.GridColor.Unknown);
		}

		public void TestGetColorForResultStatus_ScreeningPartyWrapper_PermanentClearStatus()
		{
			AssertGetColorForResultStatus_ForScreeningPartyWrapper(ScreeningStatusesList.Codes.PermanentClear, DeniedPartyConstants.GridColor.PermanentClear);
		}

		public void TestGetColorForResultStatus_ScreeningPartyWrapper_NotScreenedStatus()
		{
			AssertGetColorForResultStatus_ForScreeningPartyWrapper(ScreeningStatusesList.Codes.NotScreened, DeniedPartyConstants.GridColor.NotScreened);
		}

		void AssertGetColorForResultStatus_ForScreeningPartyWrapper(string screeningStatus, Color expectedColor)
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var screeningOrg = Factory.NewWithValidTestData<OrgHeader>();
			screeningOrg.OH_ScreeningStatus = screeningStatus;

			var screeningWrapper = new ScreeningPartyWrapper(new ScreeningParty(parentOrg, "", screeningOrg));
			AssertEquals(expectedColor, new DeniedPartyGridColorHelper().GetColorForResultStatus(screeningWrapper));
		}

		#endregion

		#region OrgScreeningStatus

		public void TestGetColorForResultStatus_OrgPartyScreeningStatus_BlockStatus()
		{
			AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(DeniedPartyConstants.LogsScreeningStatus.Block, DeniedPartyConstants.GridColor.Block);
		}

		public void TestGetColorForResultStatus_OrgPartyScreeningStatus_ReleaseStatus()
		{
			AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(DeniedPartyConstants.LogsScreeningStatus.Release, DeniedPartyConstants.GridColor.Release);
		}

		public void TestGetColorForResultStatus_OrgPartyScreeningStatus_MatchedStatus()
		{
			AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(DeniedPartyConstants.LogsScreeningStatus.MatchedDeniedParty, DeniedPartyConstants.GridColor.Matched);
		}

		public void TestGetColorForResultStatus_OrgPartyScreeningStatus_ClearStatus()
		{
			AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(DeniedPartyConstants.LogsScreeningStatus.ScreenedClear, DeniedPartyConstants.GridColor.Clear);
		}

		public void TestGetColorForResultStatus_OrgPartyScreeningStatus_UnknownStatus()
		{
			AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(DeniedPartyConstants.LogsScreeningStatus.PotentialMatchesFound, DeniedPartyConstants.GridColor.Unknown);
		}

		public void TestGetColorForResultStatus_OrgPartyScreeningStatus_PermanentClearStatus()
		{
			AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(DeniedPartyConstants.LogsScreeningStatus.ScreenedPermanentClear, DeniedPartyConstants.GridColor.PermanentClear);
		}

		public void TestGetColorForResultStatus_OrgPartyScreeningStatus_NotScreenedStatus()
		{
			AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(DeniedPartyConstants.LogsScreeningStatus.NoScreeningPerformed, DeniedPartyConstants.GridColor.NotScreened);
		}

		void AssertGetColorForResultStatus_ForOrgPartyScreeningStatus(string screeningStatus, Color expectedColor)
		{
			var orgScreeningStatus = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			orgScreeningStatus.PJ_Status = screeningStatus;
			AssertEquals(expectedColor, new DeniedPartyGridColorHelper().GetColorForResultStatus(orgScreeningStatus));
		}

		#endregion
	}
}
