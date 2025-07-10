using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(DataRegistryRating))]
	sealed class DataRegistryRatingTest : RegistryItemSetTestCaseWithFactory<DataRegistryRating>
	{
		public void TestCargoSphereUrl()
		{
			TestGenericRegistryItem(
				ItemSet.CargoSphereSUDSUrl,
				"CargoSphereSUDSUrl",
				"AutoRating/Rates Service/Third Party Rate Providers/CargoSphere",
				"SUDS Url",
				@"The URL of CargoSphere SUDS page.

Important Note:
Overriding this Registry Setting could effect user access to CargoSphere whilst subscription charges still apply.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"");
		}

		public void TestCargoGuideRateSearchUrl()
		{
			TestGenericRegistryItem(
				ItemSet.CargoguideRateSearchUrl,
				"CargoguideRateSearchUrl",
				"AutoRating/Rates Service/Third Party Rate Providers/Cargoguide",
				"Rate Search Url",
				@"The URL of Cargoguide rate search service.

Important Note:
Overriding this Registry Setting could effect user access to Cargoguide whilst subscription charges still apply.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"");
		}

		public void TestCargoSphereRateSearchUrl()
		{
			TestGenericRegistryItem(
				ItemSet.CargoSphereRateSearchUrl,
				"CargoSphereRateSearchUrl",
				"AutoRating/Rates Service/Third Party Rate Providers/CargoSphere",
				"Rate Search Url",
				@"The URL of CargoSphere rate search service.

Important Note:
Overriding this Registry Setting could effect user access to CargoSphere whilst subscription charges still apply.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"");
		}

		public void TestCargoSphereIntegrationEnabled()
		{
			TestGenericRegistryItem(
				ItemSet.CargoSphereIntegrationEnabled,
				nameof(DataRegistryRating.CargoSphereIntegrationEnabled),
				"AutoRating/Rates Service/Third Party Rate Providers/CargoSphere",
				"Enable Ocean Rate Management Integration",
				@"CargoSphere provides Ocean Rates and Rate Management Services to Rates Service. Overriding this Registry enables the integration and allows all CW1 users to subscribe to CargoSphere’s rate management services.

CargoSphere subscriptions may be administered on a per user basis in Maintain > User Admin > Staff and Resources > Staff Details and Security Rights

Important Note:
Changing this Registry Setting from 'Yes' to 'No' disables the entire CargoSphere integration.  
This means that no user will be able to access CargoSphere Rate Management Services regardless of individual user subscription status.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestCargoGuideIntegrationEnabled()
		{
			TestGenericRegistryItem(
				ItemSet.CargoguideIntegrationEnabled,
				nameof(DataRegistryRating.CargoguideIntegrationEnabled),
				"AutoRating/Rates Service/Third Party Rate Providers/Cargoguide",
				"Enable Air Rate Management Integration",
				@"Cargoguide provides Air Rates and Rate Management Services to Rates Service. Overriding this Registry enables the integration and allow all CW1 users to subscribe to Cargoguide’s rate management services.

Cargoguide subscriptions may be administered on a per user basis in Maintain > User Admin > Staff and Resources > Staff Details and Security Rights

Important Note:
Changing this Registry Setting from 'Yes' to 'No' disables the entire Cargoguide integration.  
This means that no user will be able to access Cargoguide Rate Management Services regardless of individual user subscription status.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestQuoteRequireInternalApproval()
		{
			TestGenericRegistryItem(
				ItemSet.QuoteRequireInternalApproval,
				"QuoteRequireInternalApproval",
				"AutoRating/Quotations",
				"Require Internal Approval",
				"Specifies whether Quotations need to have internal approval.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestPreserveQuoteRevenueRatingBehaviour()
		{
			TestGenericRegistryItem(
				ItemSet.PreserveQuoteRevenueRatingBehaviour,
				"PreserveQuoteRevenueRatingBehaviour",
				"AutoRating/Quotations/Spot Quotes",
				"Preserve Quote Charges Revenue Rating Behavior",
				"When this registry is set to ‘Yes’, Revenue Rating Behavior of Charges is preserved as is when Booking with Quote is converted to Shipment or One Off Quote is consolidated/linked.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestDiagnosticSettingsEnableOnODPL()
		{
			TestGenericRegistryItem(
				ItemSet.DiagnosticSettingsEnableOnODPL,
				nameof(DataRegistryRating.DiagnosticSettingsEnableOnODPL),
				"AutoRating/Rates Service/Diagnostic Settings",
				"Enable Rates Service for non-STL - Testing or Trial Only",
				"Set this registry to \"Yes\" for trial or testing of Rates Service on non-STL CW1. This registry should NEVER be set to \"Yes\" for non-STL production CW1.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestRatesServiceRateSearchRequestTimeout()
		{
			TestGenericRegistryItem(
				ItemSet.RatesServiceRateSearchRequestTimeout,
				"WiseRatesRateSearchRequestTimeout",
				"AutoRating/Rates Service",
				"Rates Service Search Request Timeout",
				"Support Only Registry. A Timeout in seconds after which Rates Service Search request will be discarded to continue with autorating.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				100);
		}

		public void TestCGSPAPIUrl()
		{
			TestGenericRegistryItem(
				ItemSet.CGSPApiURLOverride,
				nameof(DataRegistryRating.CGSPApiURLOverride),
				"AutoRating/Rates Service/Third Party Rate Providers/CargoSphere",
				"API URL Override",
				@"An override URL to the CargoSphere Rate Search API.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"");

			AssertType<UriRegistryDataType>(ItemSet.CGSPApiURLOverride.DataType);

			var dataType = (UriRegistryDataType)ItemSet.CGSPApiURLOverride.DataType;
			AssertEquals(false, dataType.AllowAutoProtocolPrefixing);
		}

		public void TestCGGDAPIUrl()
		{
			TestGenericRegistryItem(
				ItemSet.CGGDApiSettings,
				nameof(DataRegistryRating.CGGDApiSettings),
				"AutoRating/Rates Service/Third Party Rate Providers/Cargoguide",
				"API Settings",
				@"An override URL and the version for the Cargoguide API.

Important Note:
Overriding this Registry Setting could effect user access to Cargoguide whilst subscription charges still apply.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestSpotQuoteRequireInternalApproval()
		{
			TestGenericRegistryItem(
				ItemSet.SpotQuoteRequireInternalApproval,
				"SpotQuoteRequireInternalApproval",
				"AutoRating/Quotations/Spot Quotes",
				"Require Internal Approval",
				"Specifies whether Spot quotations need to have internal approval.",
				RegistryStorageFlags.System,
				true);
		}

		public void TestSpotQuoteApprovalSettings()
		{
			TestGenericRegistryItem(
				ItemSet.SpotQuoteApprovalSettings,
				"SpotQuoteApprovalSettings",
				"AutoRating/Quotations/Spot Quotes",
				"Spot Quote Approval Settings",
				@"This registry item allows you to specify the authorization required to approve an unapproved spot quote.

If Require Internal Approval registry is set to Yes (internal approval required) you can set up the authorization required based on the local value of the total quotation revenue amount; when no value is set, any quoted amount will require an approval.

The system will allow you to specify required authorization for different ranges. You must specify at least one ""Up to"" line and only one ""Above"" line.",
				RegistryStorageFlags.Company);
		}

		public void TestProposeSimilarRates()
		{
			TestGenericRegistryItem(
				ItemSet.ProposeSimilarRates,
				"ProposeSimilarRates",
				"AutoRating/Calculation",
				"Propose Similar Rates",
				"Propose Similar Rates if exact match is not found.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestAllowSavingOfAutoRatingLogNote()
		{
			TestGenericRegistryItem(
				ItemSet.AllowSavingOfAutoRatingLogNote,
				"AllowSavingOfAutoRatingLogNote",
				"AutoRating/Calculation",
				"Allow Saving of AutoRating Log Note",
				"The AutoRating Log Note is automatically generated by the AutoRating process. Unless this registry is enabled, they will be deleted upon saving the job. They are always company specific but can be viewed by users from other companies with appropriate security rights.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.BranchDepartment,
				false);
		}

		public void TestOneOffQuoteKPISettings()
		{
			TestGenericRegistryItem(
				ItemSet.OneOffQuoteKPISettings,
				"OneOffQuoteKPISettings",
				"AutoRating/Quotations/Spot Quotes",
				"Quote KPI configuration",
				"Provides the ability to specify settings for One Off Quote KPI.",
				RegistryStorageFlags.System,
				new CodeDescriptionPairList());

			var quoteKPIList = (CodeDescriptionPairList)ItemSet.OneOffQuoteKPISettings.DefaultValue;
			quoteKPIList.AddPair("WIN", "win the quotation");
			ItemSet.OneOffQuoteKPISettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, quoteKPIList);

			var getValue = ItemSet.OneOffQuoteKPISettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("GetValue.Count", 1, getValue.Count);

			quoteKPIList.AddPair("WIN", "Code should not be duplicate");
			string error = ItemSet.OneOffQuoteKPISettings.GetValidationErrorMessage(quoteKPIList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has duplicate error", "The code 'WIN' has been duplicated. Please enter a unique code.", error);

			var emptyValueList = new CodeDescriptionPairList();
			emptyValueList.AddPair("", "Code should not be empty");
			error = ItemSet.OneOffQuoteKPISettings.GetValidationErrorMessage(emptyValueList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has error", "You cannot enter an item with no Code.", error);
		}

		public void TestOneOffQuoteSourceSettings()
		{
			TestGenericRegistryItem(
				ItemSet.OneOffQuoteSourceSettings,
				"OneOffQuoteSourceSettings",
				"AutoRating/Quotations/Spot Quotes",
				"Quote Source configuration",
				"Provides the ability to specify settings for One Off Quote Source.",
				RegistryStorageFlags.System,
				new CodeDescriptionPairList());

			var quoteSourceList = (CodeDescriptionPairList)ItemSet.OneOffQuoteKPISettings.DefaultValue;
			quoteSourceList.AddPair("WIN", "win the quotation");
			ItemSet.OneOffQuoteKPISettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, quoteSourceList);

			var getValue = ItemSet.OneOffQuoteKPISettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("GetValue.Count", 1, getValue.Count);

			quoteSourceList.AddPair("WIN", "Code should not be duplicate");
			string error = ItemSet.OneOffQuoteKPISettings.GetValidationErrorMessage(quoteSourceList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has duplicate error", "The code 'WIN' has been duplicated. Please enter a unique code.", error);

			var emptyValueList = new CodeDescriptionPairList();
			emptyValueList.AddPair("", "Code should not be empty");
			error = ItemSet.OneOffQuoteKPISettings.GetValidationErrorMessage(emptyValueList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has error", "You cannot enter an item with no Code.", error);
		}

		public void TestOneOffQuoteRevisionReasonSettings()
		{
			TestGenericRegistryItem(
				ItemSet.OneOffQuoteRevisionReasonSettings,
				"OneOffQuoteRevisionReasonSettings",
				"AutoRating/Quotations/Spot Quotes",
				"Quote Revision Reason configuration",
				"Provides the ability to specify settings for One Off Quote Revision Reason.",
				RegistryStorageFlags.System,
				new CodeDescriptionPairList());

			var quoteRevisionReasonList = (CodeDescriptionPairList)ItemSet.OneOffQuoteKPISettings.DefaultValue;
			quoteRevisionReasonList.AddPair("WIN", "win the quotation");
			ItemSet.OneOffQuoteKPISettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, quoteRevisionReasonList);

			var getValue = ItemSet.OneOffQuoteKPISettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("GetValue.Count", 1, getValue.Count);

			quoteRevisionReasonList.AddPair("WIN", "Code should not be duplicate");
			string error = ItemSet.OneOffQuoteKPISettings.GetValidationErrorMessage(quoteRevisionReasonList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has duplicate error", "The code 'WIN' has been duplicated. Please enter a unique code.", error);

			var emptyValueList = new CodeDescriptionPairList();
			emptyValueList.AddPair("", "Code should not be empty");
			error = ItemSet.OneOffQuoteKPISettings.GetValidationErrorMessage(emptyValueList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has error", "You cannot enter an item with no Code.", error);
		}

		public void TestAllowOverrideCompanyTariffLevel()
		{
			TestGenericRegistryItem(
				ItemSet.AllowOverrideCompanyTariffLevel,
				"AllowOverrideCompanyTariffLevel",
				"AutoRating/Calculation",
				"Allow Company Tariff Level Override in Forwarding Jobs",
				"When registry is set to ‘Yes’, Company Tariff Level can be overridden in Forwarding Shipment, Booking, Booking with Quote and One Off Quote.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestAutoratingIntercompanyTariffsForGatewayJobConfiguration()
		{
			AssertEquals("Name", "AutoratingIntercompanyTariffsForGatewayJobConfiguration", ItemSet.AutoratingIntercompanyTariffsForGatewayJobConfiguration.Name);
			AssertEquals("Category", DataRegistryRating.Categories.AutoRating_GatewayBilling, ItemSet.AutoratingIntercompanyTariffsForGatewayJobConfiguration.Category);
			AssertEquals("Caption", "Autorating Intercompany Tariffs for Gateway Billing Job Configuration", ItemSet.AutoratingIntercompanyTariffsForGatewayJobConfiguration.Caption);
			AssertEquals("Hint", @"This registry allows you to configure certain logic of Autorating Intercompany Tariffs as Costs/Revenues for Forwarding Gateway Consols/Shipments.", ItemSet.AutoratingIntercompanyTariffsForGatewayJobConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutoratingIntercompanyTariffsForGatewayJobConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AutoratingIntercompanyTariffsForGatewayJobConfiguration.Options);
		}

		public void TestAutoratingIntercompanyTariffsForGatewayJobConfigurationDefaultValue()
		{
			var collection = ItemSet.AutoratingIntercompanyTariffsForGatewayJobConfiguration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(18, collection.Count);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[0], string.Empty, GatewayLoginAgentRole.Code.NotGateway, FreightShipmentDirection.Code.Export, JobInvoicingConsumerTypes.ShipmentCode, GatewayAutoratingRule.Code.AutoratingCost, GatewayICTServiceProvider.Code.NextGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[1], string.Empty, AgentStatusList.Codes.GatewayAgent, FreightShipmentDirection.Code.Export, JobInvoicingConsumerTypes.ShipmentCode, GatewayAutoratingRule.Code.AutoratingCost, GatewayICTServiceProvider.Code.NextGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[2], string.Empty, AgentStatusList.Codes.GatewayAgentWithTariff, FreightShipmentDirection.Code.Export, JobInvoicingConsumerTypes.ShipmentCode, GatewayAutoratingRule.Code.AutoratingCost, GatewayICTServiceProvider.Code.CurrentGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(
				collection[3],
				loginRole: GatewayPickupAgentStatus.Code.PickupAgent,
				loginAgentRole: GatewayLoginAgentRole.Code.NotGateway,
				shipmentDirection: FreightShipmentDirection.Code.Other,
				autoratingJob: JobInvoicingConsumerTypes.ShipmentCode,
				autoratingRule: GatewayAutoratingRule.Code.AutoratingCost,
				serviceProvider: GatewayICTServiceProvider.Code.NextGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[4], string.Empty, AgentStatusList.Codes.GatewayAgent, FreightShipmentDirection.Code.Other, JobInvoicingConsumerTypes.ShipmentCode, GatewayAutoratingRule.Code.AutoratingCost, GatewayICTServiceProvider.Code.NextGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[5], string.Empty, AgentStatusList.Codes.GatewayAgentWithTariff, FreightShipmentDirection.Code.Other, JobInvoicingConsumerTypes.ShipmentCode, GatewayAutoratingRule.Code.StopAutoratingCost, GatewayICTServiceProvider.Code.NotAutorate);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[6], string.Empty, AgentStatusList.Codes.GatewayAgentWithTariff, FreightShipmentDirection.Code.Export, JobInvoicingConsumerTypes.ForwardingConsolCode, GatewayAutoratingRule.Code.AutoratingRevenue, GatewayICTServiceProvider.Code.CurrentGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[7], string.Empty, AgentStatusList.Codes.GatewayAgentWithTariff, FreightShipmentDirection.Code.Other, JobInvoicingConsumerTypes.ForwardingConsolCode, GatewayAutoratingRule.Code.AutoratingRevenue, GatewayICTServiceProvider.Code.CurrentGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[8], string.Empty, AgentStatusList.Codes.GatewayAgentWithTariff, FreightShipmentDirection.Code.Import, JobInvoicingConsumerTypes.ForwardingConsolCode, GatewayAutoratingRule.Code.AutoratingRevenue, GatewayICTServiceProvider.Code.CurrentGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[9], string.Empty, AgentStatusList.Codes.GatewayAgent, FreightShipmentDirection.Code.Other, JobInvoicingConsumerTypes.ForwardingConsolCode, GatewayAutoratingRule.Code.AutoratingRevenue, GatewayICTServiceProvider.Code.CurrentGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[10], string.Empty, AgentStatusList.Codes.GatewayAgent, FreightShipmentDirection.Code.Import, JobInvoicingConsumerTypes.ForwardingConsolCode, GatewayAutoratingRule.Code.AutoratingRevenue, GatewayICTServiceProvider.Code.CurrentGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[11], string.Empty, AgentStatusList.Codes.GatewayAgentWithTariff, FreightShipmentDirection.Code.Domestic, JobInvoicingConsumerTypes.ForwardingConsolCode, GatewayAutoratingRule.Code.AutoratingRevenue, GatewayICTServiceProvider.Code.CurrentGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(collection[12], string.Empty, GatewayLoginAgentRole.Code.Gateway, FreightShipmentDirection.Code.Export, JobInvoicingConsumerTypes.ShipmentCode, GatewayAutoratingRule.Code.AutoratingCost, GatewayICTServiceProvider.Code.NextGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(
				collection[13],
				loginRole: GatewayPickupAgentStatus.Code.PickupAgent,
				loginAgentRole: GatewayLoginAgentRole.Code.Gateway,
				shipmentDirection: FreightShipmentDirection.Code.Other,
				autoratingJob: JobInvoicingConsumerTypes.ShipmentCode,
				autoratingRule: GatewayAutoratingRule.Code.AutoratingCost,
				serviceProvider: GatewayICTServiceProvider.Code.NextGatewayOrganization);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(
				collection[14],
				loginRole: GatewayPickupAgentStatus.Code.NotPickupAgent,
				loginAgentRole: AgentStatusList.Codes.GatewayAgentWithTariff,
				shipmentDirection: FreightShipmentDirection.Code.Export,
				autoratingJob: JobInvoicingConsumerTypes.ShipmentCode,
				autoratingRule: GatewayAutoratingRule.Code.StopAutoratingCostFromICT,
				serviceProvider: GatewayICTServiceProvider.Code.NotAutorate);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(
				collection[15],
				loginRole: GatewayPickupAgentStatus.Code.NotPickupAgent,
				loginAgentRole: GatewayLoginAgentRole.Code.NotGateway,
				shipmentDirection: FreightShipmentDirection.Code.Export,
				autoratingJob: JobInvoicingConsumerTypes.ShipmentCode,
				autoratingRule: GatewayAutoratingRule.Code.StopAutoratingCostFromICT,
				serviceProvider: GatewayICTServiceProvider.Code.NotAutorate);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(
				collection[16],
				loginRole: GatewayPickupAgentStatus.Code.NotPickupAgent,
				loginAgentRole: GatewayLoginAgentRole.Code.Gateway,
				shipmentDirection: FreightShipmentDirection.Code.Export,
				autoratingJob: JobInvoicingConsumerTypes.ShipmentCode,
				autoratingRule: GatewayAutoratingRule.Code.StopAutoratingCostFromICT,
				serviceProvider: GatewayICTServiceProvider.Code.NotAutorate);
			AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(
				collection[17],
				loginRole: GatewayPickupAgentStatus.Code.PickupAgent,
				loginAgentRole: AgentStatusList.Codes.GatewayAgentWithTariff,
				shipmentDirection: FreightShipmentDirection.Code.Other,
				autoratingJob: JobInvoicingConsumerTypes.ShipmentCode,
				autoratingRule: GatewayAutoratingRule.Code.AutoratingCost,
				serviceProvider: GatewayICTServiceProvider.Code.First);
		}

		void AssertAutoratingIntercompanyTariffsForGatewayJobConfiguration(AutoratingIntercompanyTariffsForGatewayJobConfiguration configuration, ZString loginRole, ZString loginAgentRole, ZString shipmentDirection, ZString autoratingJob, ZString autoratingRule, ZString serviceProvider)
		{
			AssertEquals("Login Role", loginRole, configuration.LoginRole);
			AssertEquals("Login Agent Role", loginAgentRole, configuration.LoginAgentRole);
			AssertEquals("Shipment Direction", shipmentDirection, configuration.ShipmentDirection);
			AssertEquals("Autorating Job", autoratingJob, configuration.AutoratingJob);
			AssertEquals("Autorating Rule", autoratingRule, configuration.AutoratingRule);
			AssertEquals("ICT Service Provider", serviceProvider, configuration.ICTServiceProvider);
		}
	}
}
