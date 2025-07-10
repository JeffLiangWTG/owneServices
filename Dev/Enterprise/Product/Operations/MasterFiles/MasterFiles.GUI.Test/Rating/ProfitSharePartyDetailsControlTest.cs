using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class ProfitSharePartyDetailsControlTest : TestCaseWithFactory
	{
		[GuiTest]
		[RequiresSTA]
		public void TestSetGatewayConsolProfitRedistributionTabPageVisibility()
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;

			using (var form = new ZForm())
			{
				var control = new ProfitSharePartyDetailsControl();
				form.Controls.Add(control);
				form.Show();

				var generalTab = control.Controls
					.Find("generalTabPage", true)
					.OfType<ZTabPage>().SingleOrDefault();
				generalTab.Should().NotBeNull();

				var gatewayConsolProfitRedistributionTab = control.Controls
					.Find("gatewayConsolProfitRedistributionTabPage", true)
					.OfType<ZTabPage>().SingleOrDefault();
				gatewayConsolProfitRedistributionTab.Should().NotBeNull();

				control.SetGatewayConsolProfitRedistributionTabPageVisibility(false);
				control.Controls
					.Find("gatewayConsolProfitRedistributionTabPage", true)
					.OfType<ZTabPage>().Should().BeEmpty("The redistribution tab should be removed");
				generalTab = control.Controls
					.Find("generalTabPage", true)
					.OfType<ZTabPage>().SingleOrDefault();
				generalTab.Should().NotBeNull("General tab should always stay");

				control.SetGatewayConsolProfitRedistributionTabPageVisibility(true);
				control.Controls
					.Find("gatewayConsolProfitRedistributionTabPage", true)
					.OfType<ZTabPage>().Should().NotBeEmpty("The redistribution tab should be added back");
				generalTab = control.Controls
					.Find("generalTabPage", true)
					.OfType<ZTabPage>().SingleOrDefault();
				generalTab.Should().NotBeNull("General tab should always stay");

				Assert("This test is using FluentAssertions", true);
			}
		}
	}
}