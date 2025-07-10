using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ProfitShareAgreementControlTest : TestCaseWithFactory
	{
		public void TestIncludeOrgOverrideColumn()
		{
			using (ProfitShareAgreementControl control = new ProfitShareAgreementControl())
			{
				control.IncludeOrgOverrideColumn = true;
				var grid = control.Controls.Find("ProfitShareGrid", true);
				ZGrid partyDetailsGrid = grid[0] as ZGrid;
				Assert("should include O4_OrgOverrideType", !partyDetailsGrid.GetColumnStyle(OrgProfitShareDetails.Schema.O4_OrgOverrideType).IsUnavailable);
				Assert("should include O4_OH_OrgOverride", !partyDetailsGrid.GetColumnStyle(OrgProfitShareDetails.Schema.O4_OH_OrgOverride).IsUnavailable);
			}

			using (ProfitShareAgreementControl control = new ProfitShareAgreementControl())
			{
				control.IncludeOrgOverrideColumn = false;
				var grid = control.Controls.Find("ProfitShareGrid", true);
				ZGrid partyDetailsGrid = grid[0] as ZGrid;
				Assert("should not include O4_OrgOverrideType", partyDetailsGrid.GetColumnStyle(OrgProfitShareDetails.Schema.O4_OrgOverrideType).IsUnavailable);
				Assert("should not include O4_OH_OrgOverride", partyDetailsGrid.GetColumnStyle(OrgProfitShareDetails.Schema.O4_OH_OrgOverride).IsUnavailable);
			}
		}

		public void TestGatewayProfitShareColumns_Types()
		{
			using (var control = new ProfitShareAgreementControl())
			{
				var grid = control.Controls.Find("ProfitShareGrid", true);
				var partyDetailsGrid = grid[0] as ZGrid;

				var jobTypeColumnInfo = partyDetailsGrid.GetColumnStyle(OrgProfitShareDetails.Schema.O4_JobType);
				AssertType<ZDropEditColumnStyleInfo>(jobTypeColumnInfo);

				var gatewayAgentTypeInfo = partyDetailsGrid.GetColumnStyle(OrgProfitShareDetails.Schema.O4_GatewayAgentType);
				AssertType<ZDropEditColumnStyleInfo>(gatewayAgentTypeInfo);
			}
		}
	}
}
