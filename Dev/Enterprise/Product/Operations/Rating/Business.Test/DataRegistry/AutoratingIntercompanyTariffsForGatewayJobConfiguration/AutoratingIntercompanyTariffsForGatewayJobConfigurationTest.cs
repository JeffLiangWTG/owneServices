using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(AutoratingIntercompanyTariffsForGatewayJobConfiguration))]
	public class AutoratingIntercompanyTariffsForGatewayJobConfigurationTest :
		RegistryBusinessObjectTemplateTestCase
	{
		public void TestCheckIdenticalConfigurationExists()
		{
			var newConfiguration = new AutoratingIntercompanyTariffsForGatewayJobConfiguration();
			configurations.Add(newConfiguration);

			newConfiguration.LoginRole = string.Empty;
			newConfiguration.LoginAgentRole = AgentStatusList.Codes.GatewayAgentWithTariff;
			newConfiguration.ShipmentDirection = FreightShipmentDirection.Code.Export;
			newConfiguration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			newConfiguration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			newConfiguration.ICTServiceProvider = GatewayICTServiceProvider.Code.NextGatewayOrganization;
			AssertHasRowError(newConfiguration, AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);

			newConfiguration.LoginRole = string.Empty;
			newConfiguration.LoginAgentRole = AgentStatusList.Codes.GatewayAgent;
			newConfiguration.ShipmentDirection = FreightShipmentDirection.Code.Export;
			newConfiguration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			newConfiguration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			newConfiguration.ICTServiceProvider = GatewayICTServiceProvider.Code.CurrentGatewayOrganization;
			AssertNoRowError(newConfiguration, AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);

			newConfiguration.LoginAgentRole = AgentStatusList.Codes.GatewayAgentWithTariff;
			AssertHasRowError(newConfiguration, AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
		}

		public void TestFixIdenticalConfigurationCrossProperty()
		{
			var newConfiguration = new AutoratingIntercompanyTariffsForGatewayJobConfiguration();
			configurations.Add(newConfiguration);

			newConfiguration.LoginRole = string.Empty;
			newConfiguration.LoginAgentRole = AgentStatusList.Codes.GatewayAgentWithTariff;
			newConfiguration.ShipmentDirection = FreightShipmentDirection.Code.Export;
			newConfiguration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			newConfiguration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			newConfiguration.ICTServiceProvider = GatewayICTServiceProvider.Code.NextGatewayOrganization;
			AssertHasRowError(
				"Autorating Intercompany Tariff For Gateway Billing Configuration with identical criteria already exists",
				newConfiguration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
			AssertNoErrors(newConfiguration.LoginRoleInfo);
			AssertNoErrors(newConfiguration.LoginAgentRoleInfo);
			AssertNoErrors(newConfiguration.ShipmentDirectionInfo);
			AssertNoErrors(newConfiguration.AutoratingJobInfo);
			AssertNoErrors(newConfiguration.AutoratingRuleInfo);
			AssertNoErrors(newConfiguration.ICTServiceProviderInfo);

			newConfiguration.AutoratingJob = JobInvoicingConsumerTypes.ForwardingConsolCode;
			newConfiguration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingRevenue;
			AssertNoRowError(
				configuration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
			AssertNoRowError(
				newConfiguration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
			AssertNoErrors(configuration.LoginRoleInfo);
			AssertNoErrors(configuration.LoginAgentRoleInfo);
			AssertNoErrors(newConfiguration.LoginRoleInfo);
			AssertNoErrors(newConfiguration.LoginAgentRoleInfo);
			AssertNoErrors(newConfiguration.ShipmentDirectionInfo);
			AssertNoErrors(newConfiguration.AutoratingJobInfo);
			AssertNoErrors(newConfiguration.AutoratingRuleInfo);
			AssertNoErrors(newConfiguration.ICTServiceProviderInfo);

			newConfiguration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			newConfiguration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			AssertHasRowError(
				"Autorating Intercompany Tariff For Gateway Billing Configuration with identical criteria already exists",
				newConfiguration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);

			newConfiguration.ShipmentDirection = FreightShipmentDirection.Code.Import;
			AssertNoRowError(
				configuration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
			AssertNoRowError(
				newConfiguration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
			AssertNoErrors(configuration.LoginRoleInfo);
			AssertNoErrors(configuration.LoginAgentRoleInfo);
			AssertNoErrors(newConfiguration.LoginRoleInfo);
			AssertNoErrors(newConfiguration.LoginAgentRoleInfo);
			AssertNoErrors(newConfiguration.ShipmentDirectionInfo);
			AssertNoErrors(newConfiguration.AutoratingJobInfo);
			AssertNoErrors(newConfiguration.AutoratingRuleInfo);
			AssertNoErrors(newConfiguration.ICTServiceProviderInfo);

			newConfiguration.ShipmentDirection = FreightShipmentDirection.Code.Export;
			AssertHasRowError(
				"Autorating Intercompany Tariff For Gateway Billing Configuration with identical criteria already exists",
				newConfiguration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);

			newConfiguration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingRevenue;
			newConfiguration.AutoratingJob = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertNoRowError(
				configuration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
			AssertNoRowError(
				newConfiguration,
				AutoratingIntercompanyTariffsForGatewayJobConfiguration.IdenticalGatewayBillingConfigConfigurationExists);
			AssertNoErrors(configuration.LoginRoleInfo);
			AssertNoErrors(configuration.LoginAgentRoleInfo);
			AssertNoErrors(newConfiguration.LoginRoleInfo);
			AssertNoErrors(newConfiguration.LoginAgentRoleInfo);
			AssertNoErrors(newConfiguration.ShipmentDirectionInfo);
			AssertNoErrors(newConfiguration.AutoratingJobInfo);
			AssertNoErrors(newConfiguration.AutoratingRuleInfo);
			AssertNoErrors(newConfiguration.ICTServiceProviderInfo);
		}

		public void TestValidateLoginRole()
		{
			AssertNoErrors(configuration.LoginRoleInfo);

			configuration.LoginRole = string.Empty;
			AssertNoErrors("LoginRole can be empty", configuration.LoginRoleInfo);

			configuration.LoginRole = "AAA";
			AssertHasErrors("LoginRole has invalid code", configuration.LoginRoleInfo);

			configuration.LoginRole = GatewayPickupAgentStatus.Code.PickupAgent;
			AssertNoErrors(configuration.LoginAgentRoleInfo);
		}

		public void TestValidateLoginAgentRole()
		{
			AssertNoErrors(configuration.LoginAgentRoleInfo);

			configuration.LoginAgentRole = string.Empty;
			AssertHasErrors("LoginAgentRole cannot be blank", configuration.LoginAgentRoleInfo);

			configuration.LoginAgentRole = "AAA";
			AssertHasErrors("LoginAgentRole has invalid code", configuration.LoginAgentRoleInfo);

			configuration.LoginAgentRole = AgentStatusList.Codes.GatewayAgentWithTariff;
			AssertNoErrors(configuration.LoginAgentRoleInfo);
		}

		public void TestValidateShipmentDirection()
		{
			AssertNoErrors(configuration.ShipmentDirectionInfo);

			configuration.ShipmentDirection = string.Empty;
			AssertHasErrors("ShipmentDirection cannot be blank", configuration.ShipmentDirectionInfo);

			configuration.ShipmentDirection = "AAA";
			AssertHasErrors("ShipmentDirection has invalid code", configuration.ShipmentDirectionInfo);

			configuration.ShipmentDirection = FreightShipmentDirection.Code.Import;
			AssertNoErrors(configuration.ShipmentDirectionInfo);
		}

		public void TestValidateAutoratingJob()
		{
			AssertNoErrors(configuration.AutoratingJobInfo);

			configuration.AutoratingJob = string.Empty;
			AssertHasErrors("AutoratingJob cannot be blank", configuration.AutoratingJobInfo);

			configuration.AutoratingJob = "AAA";
			AssertHasErrors("AutoratingJob has invalid code", configuration.AutoratingJobInfo);

			configuration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNoErrors(configuration.AutoratingJobInfo);
		}

		public void TestValidateAutoratingRule()
		{
			AssertNoErrors(configuration.AutoratingRuleInfo);

			configuration.AutoratingRule = string.Empty;
			AssertHasErrors("AutoratingRule cannot be blank", configuration.AutoratingRuleInfo);

			configuration.AutoratingRule = "AAA";
			AssertHasErrors("AutoratingRule has invalid code", configuration.AutoratingRuleInfo);

			configuration.AutoratingRule = GatewayAutoratingRule.Code.StopAutoratingCost;
			AssertNoErrors(configuration.AutoratingRuleInfo);
		}

		public void TestValidateICTServiceProvider()
		{
			AssertNoErrors(configuration.ICTServiceProviderInfo);

			configuration.ICTServiceProvider = string.Empty;
			AssertHasErrors("ICTServiceProvider cannot be blank", configuration.ICTServiceProviderInfo);

			configuration.ICTServiceProvider = "AAA";
			AssertHasErrors("ICTServiceProvider has invalid code", configuration.ICTServiceProviderInfo);

			configuration.ICTServiceProvider = GatewayICTServiceProvider.Code.NextGatewayOrganization;
			AssertNoErrors(configuration.ICTServiceProviderInfo);
		}

		public void TestICTProviderCode_ShouldSetToNotAndBecomeReadOnly_WhenAutoratingRuleIsStopAutoratingCost()
		{
			configuration.AutoratingRule = GatewayAutoratingRule.Code.StopAutoratingCost;
			AssertEquals(GatewayICTServiceProvider.Code.NotAutorate, configuration.ICTServiceProvider);
			AssertEquals(true, configuration.ICTServiceProviderIsReadOnly);
			configuration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			AssertEquals(false, configuration.ICTServiceProviderIsReadOnly);
		}

		public void TestICTProviderCode_ShouldSetToNotAndBecomeReadOnly_WhenAutoratingRuleIsStopAutoratingCostFromICT()
		{
			configuration.AutoratingRule = GatewayAutoratingRule.Code.StopAutoratingCostFromICT;
			AssertEquals(GatewayICTServiceProvider.Code.NotAutorate, configuration.ICTServiceProvider);
			AssertEquals(true, configuration.ICTServiceProviderIsReadOnly);
			configuration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			AssertEquals(false, configuration.ICTServiceProviderIsReadOnly);
		}

		public void TestShipmentDirection_ShouldHasError_WhenItSetToAll()
		{
			configuration.ShipmentDirection = FreightShipmentDirection.Code.All;
			AssertHasError(configuration.ShipmentDirectionInfo, "ALL is not supported for Shipment direction");
		}

		public void TestAutoratingRuleShouldHasError_WhenAutoratingRuleSetToICRAndAutoratingJobIsShipment()
		{
			configuration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			configuration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingRevenue;
			AssertHasError(configuration.AutoratingRuleInfo, "Cannot select Autorate Revenue for Gateway Billing of Shipment");
		}

		public void TestAutoratingRuleShouldHasError_WhenAutoratingRuleSetToICCAndAutoratingJobIsForwardingConsolidation()
		{
			configuration.AutoratingJob = JobInvoicingConsumerTypes.ForwardingConsolCode;
			configuration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			AssertHasError(configuration.AutoratingRuleInfo, "Cannot select Autorate Cost for Gateway Billing of Forwarding Consolidation");
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new AutoratingIntercompanyTariffsForGatewayJobConfiguration();
			configuration.LoginRole = string.Empty;
			configuration.LoginAgentRole = AgentStatusList.Codes.GatewayAgentWithTariff;
			configuration.ShipmentDirection = FreightShipmentDirection.Code.Export;
			configuration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			configuration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			configuration.ICTServiceProvider = GatewayICTServiceProvider.Code.CurrentGatewayOrganization;
			configurations = new AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection();
			configurations.Add(configuration);
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;
		protected new AutoratingIntercompanyTariffsForGatewayJobConfiguration BizObj => (AutoratingIntercompanyTariffsForGatewayJobConfiguration)base.BizObj;

		AutoratingIntercompanyTariffsForGatewayJobConfiguration configuration;
		AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection configurations;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var configuration = new AutoratingIntercompanyTariffsForGatewayJobConfiguration();
			configuration.LoginRole = string.Empty;
			configuration.LoginAgentRole = AgentStatusList.Codes.GatewayAgentWithTariff;
			configuration.ShipmentDirection = FreightShipmentDirection.Code.Export;
			configuration.AutoratingJob = JobInvoicingConsumerTypes.ShipmentCode;
			configuration.AutoratingRule = GatewayAutoratingRule.Code.AutoratingCost;
			configuration.ICTServiceProvider = GatewayICTServiceProvider.Code.CurrentGatewayOrganization;
			return configuration;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
