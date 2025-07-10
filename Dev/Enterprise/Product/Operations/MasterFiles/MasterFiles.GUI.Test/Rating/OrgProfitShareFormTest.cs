using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class OrgProfitShareFormTest : TestCaseWithFactory
	{
		public void TestControlsIncludeOrgOverride()
		{
			using (OrgProfitShareForm form = new OrgProfitShareForm(Factory.New<OrgAgentRelationship>()))
			{
				var control = form.Controls.Find("ProfitShareControl", true)[0];
				var profitShareControl = control as ProfitShareAgreementControl;
				AssertNotNull(profitShareControl);
				Assert("profitShareControl should not include OrgOverride", !profitShareControl.IncludeOrgOverrideColumn);

				var agreementControl = form.Controls.Find("ClientSpecificProfitShareControl", true)[0];
				var profitShareAgreementControl = agreementControl as ProfitShareAgreementControl;
				AssertNotNull(profitShareAgreementControl);
				Assert("profitShareAgreementControl should include OrgOverride", profitShareAgreementControl.IncludeOrgOverrideColumn);
			}
		}

		public void TestProfitShareGridColumnsAvailabilitiesByProfitShareType_Standard()
		{
			AssertProfitShareGridColumnsAvailabilitiesByProfitShareType(
				profitShareType: OrgAgentRelationship.ProfitShareTypes.Standard,
				isApportionmentMethodUnavailable: true);
		}

		public void TestProfitShareGridColumnsAvailabilitiesByProfitShareType_AgencyProfile()
		{
			AssertProfitShareGridColumnsAvailabilitiesByProfitShareType(
				profitShareType: OrgAgentRelationship.ProfitShareTypes.AgencyProfile,
				isApportionmentMethodUnavailable: false);
		}

		void AssertProfitShareGridColumnsAvailabilitiesByProfitShareType(string profitShareType, bool isApportionmentMethodUnavailable)
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = profitShareType;

			using (var form = new OrgProfitShareForm(agentRelationship))
			{
				var profitShareGrids = form.Controls.Find("ProfitShareGrid", true).OfType<ZGrid>().ToArray();
				AssertEquals("There should be 2 grids from 2 ProfitShareAgreementControls in the form", 2, profitShareGrids.Length);

				var message = $"{profitShareType} Profit Share Type should support Gateway Consol Profit Share and show 2 columns: JobType and GatewayAgentType";
				Assert(message, profitShareGrids.All(x => !x.GetColumnStyle(AutoOrgProfitShareDetails.Schema.O4_JobType).IsUnavailable));
				Assert(message, profitShareGrids.All(x => !x.GetColumnStyle(AutoOrgProfitShareDetails.Schema.O4_GatewayAgentType).IsUnavailable));

				message = $"Gateway Profit Apportionment Method column in {profitShareType} Profit Share should be {(isApportionmentMethodUnavailable ? "unavailable" : "available")}";
				Assert(message, profitShareGrids.All(x => x.GetColumnStyle(OrgProfitShareDetails.Schema.O4_GatewayProfitApportionmentMethod).IsUnavailable == isApportionmentMethodUnavailable));
			}
		}

		[RequiresSTA]
		public void TestGatewayConsolProfitRedistributionTabPageVisibility_Standard() =>
			AssertGatewayConsolProfitRedistributionTabPageVisibility(
				profitShareType: OrgAgentRelationship.ProfitShareTypes.Standard,
				"",
				expectedVisible: false);

		public void TestGatewayConsolProfitRedistributionTabPageVisibility_AgencyProfile_ApportionmentMethod() =>
			AssertGatewayConsolProfitRedistributionTabPageVisibility(
				profitShareType: OrgAgentRelationship.ProfitShareTypes.AgencyProfile,
				GatewayProfitApportionmentMethodList.Codes.SHP,
				expectedVisible: true);

		public void TestGatewayConsolProfitRedistributionTabPageVisibility_AgencyProfile_EmptyApportionmentMenthod() =>
			AssertGatewayConsolProfitRedistributionTabPageVisibility(
				profitShareType: OrgAgentRelationship.ProfitShareTypes.AgencyProfile,
				"",
				expectedVisible: false);

		void AssertGatewayConsolProfitRedistributionTabPageVisibility(string profitShareType, string apportionmentMethod, bool expectedVisible)
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = profitShareType;
			var profitShareDetails = agentRelationship.ProfitShareDetails.AddNew();
			profitShareDetails.O4_JobType = JobTypesList.Codes.GCN;
			profitShareDetails.O4_GatewayProfitApportionmentMethod = apportionmentMethod;

			using (var form = new OrgProfitShareForm(agentRelationship))
			{
				form.Show();
				var testTabPages = form.Controls.Find("gatewayConsolProfitRedistributionTabPage", true).OfType<ZTabPage>().ToArray();
				var expectedNumberOfTabPages = expectedVisible ? 1 : 0;
				AssertEquals("Number of tab pages from 2 ProfitShareAgreementControls in the form", expectedNumberOfTabPages, testTabPages.Length);
			}
		}
	}
}
