using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationLookupsTest :
		BusinessObjectLookupsTestCase
	{
		public void TestLoginRoleList()
		{
			AssertEquals(2, Lookups.LoginRoleList.Count);
			AssertEquals(true, Lookups.LoginRoleList.ContainsCode(GatewayPickupAgentStatus.Code.PickupAgent));
			AssertEquals(true, Lookups.LoginRoleList.ContainsCode(GatewayPickupAgentStatus.Code.NotPickupAgent));
		}

		public void TestLoginAgentRoleList()
		{
			AssertEquals(4, Lookups.LoginAgentRoleList.Count);
			AssertEquals(true, Lookups.LoginAgentRoleList.ContainsCode(GatewayLoginAgentRole.Code.NotGateway));
			AssertEquals(true, Lookups.LoginAgentRoleList.ContainsCode(GatewayLoginAgentRole.Code.Gateway));
			AssertEquals(true, Lookups.LoginAgentRoleList.ContainsCode(AgentStatusList.Codes.GatewayAgent));
			AssertEquals(true, Lookups.LoginAgentRoleList.ContainsCode(AgentStatusList.Codes.GatewayAgentWithTariff));
		}

		public void TestShipmentDirectionList()
		{
			AssertEquals(5, Lookups.ShipmentDirectionList.Count);
			AssertEquals(true, Lookups.ShipmentDirectionList.ContainsCode(FreightShipmentDirection.Code.All));
			AssertEquals(true, Lookups.ShipmentDirectionList.ContainsCode(FreightShipmentDirection.Code.Import));
			AssertEquals(true, Lookups.ShipmentDirectionList.ContainsCode(FreightShipmentDirection.Code.Export));
			AssertEquals(true, Lookups.ShipmentDirectionList.ContainsCode(FreightShipmentDirection.Code.Domestic));
			AssertEquals(true, Lookups.ShipmentDirectionList.ContainsCode(FreightShipmentDirection.Code.Other));
		}

		public void TestAutoratingJobList()
		{
			AssertEquals(2, Lookups.AutoratingJobList.Count);
			AssertEquals(true, Lookups.AutoratingJobList.ContainsCode(JobInvoicingConsumerTypes.ShipmentCode));
			AssertEquals(true, Lookups.AutoratingJobList.ContainsCode(JobInvoicingConsumerTypes.ForwardingConsolCode));
		}

		public void TestAutoratingRuleList()
		{
			AssertEquals(4, Lookups.AutoratingRuleList.Count);
			AssertEquals(true, Lookups.AutoratingRuleList.ContainsCode(GatewayAutoratingRule.Code.AutoratingCost));
			AssertEquals(true, Lookups.AutoratingRuleList.ContainsCode(GatewayAutoratingRule.Code.AutoratingRevenue));
			AssertEquals(true, Lookups.AutoratingRuleList.ContainsCode(GatewayAutoratingRule.Code.StopAutoratingCost));
			AssertEquals(true, Lookups.AutoratingRuleList.ContainsCode(GatewayAutoratingRule.Code.StopAutoratingCostFromICT));
		}

		public void TestICTServiceProviderList()
		{
			AssertEquals(4, Lookups.ICTServiceProviderList.Count);
			AssertEquals(true, Lookups.ICTServiceProviderList.ContainsCode(GatewayICTServiceProvider.Code.First));
			AssertEquals(true, Lookups.ICTServiceProviderList.ContainsCode(GatewayICTServiceProvider.Code.CurrentGatewayOrganization));
			AssertEquals(true, Lookups.ICTServiceProviderList.ContainsCode(GatewayICTServiceProvider.Code.NextGatewayOrganization));
			AssertEquals(true, Lookups.ICTServiceProviderList.ContainsCode(GatewayICTServiceProvider.Code.NotAutorate));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new AutoratingIntercompanyTariffsForGatewayJobConfiguration();
			Lookups = new AutoratingIntercompanyTariffsForGatewayJobConfigurationLookups(configuration);
		}
		AutoratingIntercompanyTariffsForGatewayJobConfigurationLookups Lookups;
		AutoratingIntercompanyTariffsForGatewayJobConfiguration configuration;

		#endregion
	}
}
