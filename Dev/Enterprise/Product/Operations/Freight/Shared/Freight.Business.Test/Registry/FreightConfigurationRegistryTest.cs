using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(FreightConfigurationRegistry))]
	sealed class FreightConfigurationRegistryTest : RegistryItemSetTestCaseWithFactory<FreightConfigurationRegistry>
	{
		public void TestReplaceExistingCarrierContractNumbersWithNewNumbers()
		{
			TestGenericRegistryItem(ItemSet.IgnoreAndReplaceCarrierContractNumbersDuringAutorating,
				"FreightIgnoreAndReplaceContractNumbersDuringAutorating",
				"Freight/Consolidations/Carrier Contract Numbers",
				"Ignore and replace Consol Contract Number/s during Autorating",
				"This registry setting controls whether the Contract Number/s of the rates applied to the job during Autorating Costs will populate to Consol > Reference Number with the type 'Carrier Contract Number' regardless if Carrier Contract Number/s were previously registered on a Consol or not. If this registry is set to “Yes”, any previously entered or auto-populated Contract Numbers will not be used as rate differentiators, but instead they will be deleted and new Contract Numbers if found from matching costs, will be populated to the Consol. Do you want to override the default value?",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestPopulateForwardingConsolContractNumbersFromCostingRates()
		{
			TestGenericRegistryItem(ItemSet.PopulateForwardingConsolContractNumbersIfBlankDuringAutorating,
				"PopulateForwardingConsolContractNumbersIfBlankDuringAutorating",
				"Freight/Consolidations/Carrier Contract Numbers",
				"Auto populate carrier contract numbers if blank during Autorating",
				"This registry setting controls whether Contract Number/s of the rates applied to the job during Autorating to be populated to the Consol Reference Number with type 'Carrier Contract Number' if no Carrier Contract Number has been added before. Override the registry if Carrier Contract Number/s to be populated to Consols during Autorating Costs.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestReplaceExistingClientContractNumberWithNewNumbers()
		{
			TestGenericRegistryItem(ItemSet.IgnoreAndReplaceClientContractNumbersDuringAutorating,
				"FreightIgnoreAndReplaceClientContractNumbersDuringAutorating",
				"Freight/Shipment/Client Contract Number",
				"Ignore and replace Client Contract Number(s) during Autorating",
				"This registry setting controls whether the Client Contract Number/s of the rates applied to the job during Autorating Revenue will populate to Shipment Reference Number with the type 'Client Contract Number' regardless if Client Contract Number/s were previously registered on a Shipment or not. If this registry is set to “Yes”, any previously entered or auto-populated Client Contract Numbers will not be used as rate differentiators, but instead they will be deleted and new Client Contract Numbers if found from matching revenue, will be populated to the Shipment. Do you want to override the default value?",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestPopulateClientContractNumbersFromClientRates()
		{
			TestGenericRegistryItem(ItemSet.PopulateClientContractNumbersIfBlankDuringAutorating,
				"PopulateClientContractNumbersIfBlankDuringAutorating",
				"Freight/Shipment/Client Contract Number",
				"Auto populate Client Contract Numbers if blank during Autorating",
				"This registry setting controls whether Client Contract Number/s of the rates applied to the job during Autorating are to be populated to the Reference Number grid with type 'CLC - Client Contract Number', if no Client Contract Number has been added previously. Override the registry to 'Yes' if Client Contract Number/s are to be populated to Shipments during Autorating Revenue.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestClientContractNumberFormat()
		{
			void assertions(BillCustomisationRegistryDataType item)
			{
				CombineAssertions(() =>
				{
					AssertEquals("CLA", item.FountainPrefix);
					AssertEquals("Client Contract Number Format", item.GeneratedNumberName);
					AssertEquals(RatingContractSchema.RCT_ContractNumber.MaxLength, item.MaxLength);
					AssertEquals(item.DefaultValue.Categories, NumberCustomisationElementCategories.ClientContract);
					AssertEquals("NON", item.DefaultValue.CheckDigitAlgorithm);
				});
			}

			TestRegistryItem(ItemSet.ClientContractNumberFormat,
				"ClientContractNumberFormat",
				"Freight/Shipment/Client Contract Number",
				"Client Contract Number Format",
				"Override this value to customize how Client Contract numbers are formatted",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				assertions);
		}

		public void TestConsolNumberCustomisation()
		{
			AssertEquals("ConsolNumberCustomisation", ItemSet.ConsolNumberCustomisation.Name);
			AssertEquals("Override this value to customize how consolidations are formatted for ALL Transport Modes", ItemSet.ConsolNumberCustomisation.Hint);
			AssertEquals("Freight/Consolidations", ItemSet.ConsolNumberCustomisation.Category);
			AssertEquals("Consol Number", ItemSet.ConsolNumberCustomisation.Caption);
			AssertEquals(RegistryStorageFlags.All, ItemSet.ConsolNumberCustomisation.Storage);
			AssertEquals(NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.Consol, ItemSet.ConsolNumberCustomisation.DefaultValue.Categories);
		}

		public void TestRoadConsolMasterBillNumber()
		{
			AssertEquals("RoadConsolMasterBillNumber", ItemSet.RoadConsolMasterBillNumber.Name);
			AssertEquals("Override this value to customize how Master Bills are formatted for Transport Mode ROAD.", ItemSet.RoadConsolMasterBillNumber.Hint);
			AssertEquals("Freight/Consolidations", ItemSet.RoadConsolMasterBillNumber.Category);
			AssertEquals("Road Consol Master Bill Number", ItemSet.RoadConsolMasterBillNumber.Caption);
			AssertEquals(RegistryStorageFlags.All, ItemSet.RoadConsolMasterBillNumber.Storage);
			AssertEquals(NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.Consol, ItemSet.RoadConsolMasterBillNumber.DefaultValue.Categories);
		}

		public void TestAWBIssueDate()
		{
			AssertEquals("AWBIssueDate", ItemSet.AWBIssueDate.Name);
			AssertEquals("Specify which date should be used for AWB issue date.", ItemSet.AWBIssueDate.Hint);
			AssertEquals("Freight/AWB", ItemSet.AWBIssueDate.Category);
			AssertEquals("AWB Issue Date", ItemSet.AWBIssueDate.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.AWBIssueDate.Storage);
			AssertEquals("TYE", ItemSet.AWBIssueDate.DefaultValue);
		}

		public void TestDefaultShipmentIssueDateFromHAWB()
		{
			AssertEquals("DefaultShipmentIssueDateFromHAWB", ItemSet.DefaultShipmentIssueDateFromHAWB.Name);
			AssertEquals("Specify whether the Shipment Issue Date, if blank, should defaults from the Executed Date on the HAWB once it has been printed. This setting only applies to Export Air Shipments.", ItemSet.DefaultShipmentIssueDateFromHAWB.Hint);
			AssertEquals("Freight/AWB/HAWB", ItemSet.DefaultShipmentIssueDateFromHAWB.Category);
			AssertEquals("Default Shipment Issue Date from HAWB Executed Date ", ItemSet.DefaultShipmentIssueDateFromHAWB.Caption);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.DefaultShipmentIssueDateFromHAWB.Storage);
			AssertEquals(false, ItemSet.DefaultShipmentIssueDateFromHAWB.DefaultValue);
		}

		public void TestMandatoryINCO()
		{
			AssertEquals("MandatoryIncoTerm", ItemSet.MandatoryIncoTerm.Name);
			AssertEquals("Specify whether the shipment Incoterm should be a mandatory field. This setting only applies to Export Shipments.", ItemSet.MandatoryIncoTerm.Hint);
			AssertEquals("Freight/Shipment", ItemSet.MandatoryIncoTerm.Category);
			AssertEquals("Incoterm Mandatory", ItemSet.MandatoryIncoTerm.Caption);
			AssertEquals(RegistryStorageFlags.BranchDepartment, ItemSet.MandatoryIncoTerm.Storage);
			AssertEquals(false, ItemSet.MandatoryIncoTerm.DefaultValue);
		}

		public void TestMandatoryBroker()
		{
			AssertEquals("MandatoryImportBroker", ItemSet.MandatoryImportBroker.Name);
			AssertEquals("Specify whether the shipment Import Broker should be a mandatory field. This setting only applies to Import Shipments.", ItemSet.MandatoryImportBroker.Hint);
			AssertEquals("Freight/Shipment", ItemSet.MandatoryImportBroker.Category);
			AssertEquals("Import Broker Mandatory", ItemSet.MandatoryImportBroker.Caption);
			AssertEquals(RegistryStorageFlags.BranchDepartment, ItemSet.MandatoryImportBroker.Storage);
			AssertEquals(false, ItemSet.MandatoryImportBroker.DefaultValue);
		}

		public void TestConsolCutOffDate()
		{
			AssertEquals("ConsolCutOffDate", ItemSet.ConsolCutOffDate.Name);
			AssertEquals("Specify whether the Consol cut off date for the first routing leg should be mandatory. This setting only applies to Export Consols.", ItemSet.ConsolCutOffDate.Hint);
			AssertEquals("Freight/Consolidations", ItemSet.ConsolCutOffDate.Category);
			AssertEquals("Consol Cut Off Mandatory", ItemSet.ConsolCutOffDate.Caption);
			AssertEquals(RegistryStorageFlags.BranchDepartment, ItemSet.ConsolCutOffDate.Storage);
			AssertEquals(false, ItemSet.ConsolCutOffDate.DefaultValue);
		}

		public void TestAutoPackFreightContainers()
		{
			TestRegistryItem(ItemSet.AutoPackFreightContainers,
				"AutoPackFreightContainers",
				"Freight",
				"Auto-pack Containers",
				"Select No if you do not want your shipments to be automatically packed to Containers on a Consol",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestAllowInterCompanyHyperlinksToBeOpenedInReceivingCompany()
		{
			TestRegistryItem(ItemSet.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany,
				"AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany",
				"Freight",
				"Allow inter-company hyperlinks to be opened in receiving company",
				"Allow inter-company Hyperlinks to be opened and create an operational job in the logged-in company",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				false);
		}

		public void TestAutoDeliverDelayAlertDocuments()
		{
			TestGenericRegistryItem(ItemSet.AutoDeliverDelayAlertDocuments,
				"AutoDeliverDelayAlertDocuments",
				"Freight/Notifications",
				"Delay alert auto-send",
				"When the sailing schedule changes, this registry determines which jobs should have a delay alert delivered.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue);

			DelayAlertDeliveryRuleCollection collection = ItemSet.AutoDeliverDelayAlertDocuments.DefaultValue;

			AssertEquals("should be empty", 0, collection.Count);
		}

		public void TestAutoCreateLooseConfirmations()
		{
			TestRegistryItem(ItemSet.AutoCreateLooseConfirmations,
				"AutoCreateLooseConfirmations",
				"Freight/Shipment",
				"Automatically Create Confirmations for Loose Cargo",
				"Setting the registry to 'No' will disable automatic creation of Confirmations for every loose cargo shipment. With the registry set to 'No', the users will still be able to create Confirmations by manually clicking on Pickup/Delivery > Confirmations tab.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestSIDefaultForwarderForNonDirectConsols()
		{
			TestRegistryItem(ItemSet.SIDefaultForwarderForNonDirectConsols,
				"SIDefaultForwarderForNonDirectConsols",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"SI Default Forwarder for Non-Direct Consols",
				@"Carrier Shipping Instruction messages include optional Forwarder organization. Forwarder is defaulted from Consol > Sending Agent field. Override this registry to leave Forwarder organization blank for a non-direct Consolidation.

Note. Forwarder organization can still be overridden per a carrier messaging form prior to sending.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		public void TestSendContactCompanyIdInSIBillClauses()
		{
			TestRegistryItem(ItemSet.SendContactCompanyIdInSIBillClauses,
				"SendContactCompanyIdInSIBillClauses",
				"Freight/Consolidations/Ocean Carrier Messaging",
				"Send Contact/Company Id in SI Bill Clauses",
				@"Override this setting if you would like to include Contact details and Government Company ID Registration details of shipper, consignee and main notify party to ‘Other BL Clauses’ section of the Shipping Instruction message to carriers.

Please note, these details are already available within Shipper, Consignee and Notify Sections but might not be printed by some of the carriers on Bills of Lading.  Once this registry is enabled, some carriers may print details in both places.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		#region Schedules / Sailing Schedule / Allocations

		public void TestVoyageRecyclingPeriod()
		{
			string hint = "Use this registry to specify system default value for voyage recycling period. Vessel-Voyage-Carrier combinations will automatically be flagged as 'Archived' after a period of time specified here.\r\n" +
				"The default value is \"None\", i.e. no automatic archiving.\r\n" +
				"Default voyage recycling period can also be specified at a carrier organization.";

			TestGenericRegistryItem(ItemSet.VoyageRecyclingPeriod,
				"VoyageRecyclingPeriod", FreightConfigurationRegistry.Categories.Schedules_SailingSchedule,
				"Voyage Recycling Period", hint,
				RegistryStorageFlags.System);
		}

		public void TestDefaultAllocationMethods()
		{
			string hint = "This registry option allows you to specify the default allocation method to use for each country/region.";

			TestGenericRegistryItem(ItemSet.DefaultAllocationMethods,
				"SchedulesDefaultAllocationMethods", FreightConfigurationRegistry.Categories.Schedules_SailingSchedule_Allocations,
				"Allocation Method Defaulting", hint,
				RegistryStorageFlags.System);
		}

		public void TestDefaultOverAllocationPercent()
		{
			string hint = "The default percentage by which an allocation may be exceeded.\r\n" +
				"If set to 0%, allocations may not be exceeded.\r\n" +
				"If set to 10%, allocations may exceed allocations by up to 10%.\r\n\r\n" +
				"Warning: no checking will be done to ensure existing bookings remain within there " +
				"respective allocations after reducing this value";

			TestGenericRegistryItem(ItemSet.DefaultOverAllocationPercent,
				"DefaultOverAllocationPercent", FreightConfigurationRegistry.Categories.Schedules_SailingSchedule_Allocations,
				"Default Over Allocation Percent", hint,
				RegistryStorageFlags.System, 0m);
		}

		public void TestUseGlobalAllocations()
		{
			TestGenericRegistryItem(
				ItemSet.UseGlobalAllocations,
				"UseGlobalAllocations", FreightConfigurationRegistry.Categories.Schedules_SailingSchedule_Allocations,
				"Use Global Allocations", "If enabled, then allocations for all countries/regions will be visible and enforced, not just those for the country/region of the current logged in company.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestGlowGSS()
		{
			AssertEquals("GlowGSS", ItemSet.GlowGSS.Name);
			AssertEquals("Override this registry to enable the new web-based global sailing schedules.", ItemSet.GlowGSS.Hint);
			AssertEquals("Schedules/Sailing Schedule", ItemSet.GlowGSS.Category);
			AssertEquals("GLOW GSS", ItemSet.GlowGSS.Caption);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.GlowGSS.Storage);
			AssertEquals(false, ItemSet.GlowGSS.DefaultValue);
		}

		#endregion // Schedules / Sailing Schedule / Allocations

		#region Default Number of Decimal Places

		public void TestDefaultNumberOfDecimalPlaces()
		{
			AssertEquals("DefaultNumberOfDecimalPlacesForFreight", ItemSet.DefaultNumberOfDecimalPlaces.Name);
			AssertEquals("Freight/Shipment/Packages", ItemSet.DefaultNumberOfDecimalPlaces.Category);
			AssertEquals("Default Number of Decimal Places", ItemSet.DefaultNumberOfDecimalPlaces.Caption);
			AssertEquals(@"This registry item allows you to configure default number of decimal places for different units of measure (and different rounding rule when data is calculated or imported electronically) per specific transport mode.

If not configured, 3 decimal places are used for weight, volume and length units of measure by default.", ItemSet.DefaultNumberOfDecimalPlaces.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.DefaultNumberOfDecimalPlaces.Storage);

			var collection = ItemSet.DefaultNumberOfDecimalPlaces.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("RegistryItem contains no rows.", 0, collection.Count);
		}

		public void TestDuplicateRegistryEntry()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);

			var defaultNumberOfDecimals = collection.AddNew();
			defaultNumberOfDecimals.UnitOfMeasure = "KG";
			defaultNumberOfDecimals.TransportMode = "AIR";
			defaultNumberOfDecimals.NumberOfDecimals = 1;
			defaultNumberOfDecimals.RoundingMode = RoundingModes.Up;

			AssertNoExceptionThrown(() => FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection));
			collection.RunPreSaveValidation();
			Assert(!defaultNumberOfDecimals.HasErrors);

			var duplicateDefaultNumberOfDecimals = collection.AddNew();
			duplicateDefaultNumberOfDecimals.UnitOfMeasure = "KG";
			duplicateDefaultNumberOfDecimals.TransportMode = "AIR";
			duplicateDefaultNumberOfDecimals.NumberOfDecimals = 2;
			duplicateDefaultNumberOfDecimals.RoundingMode = RoundingModes.Down;

			AssertExceptionThrown(typeof(RegistryValidationException), () => FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection));
			collection.RunPreSaveValidation();
			Assert(defaultNumberOfDecimals.HasErrors);
			Assert(duplicateDefaultNumberOfDecimals.HasErrors);
		}

		public void TestNumberOfDecimalPlacesForWeightAndVolume()
		{
			TestRegistryItem(ItemSet.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume,
				"UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume",
				FreightDataRegistry.Categories.Freight_AWB,
				"Number of Decimal Places for Weight and Volume",
				@"Set the value to Yes if you want to synchronize number of decimal places for weight and volume on MAWB and HAWB with the number of decimal places specified in the registry Freight > Shipment > Packages > Default Number of Decimal Places for transport mode Air.

If the registry is off, the weight and volume will be rounded for AWB to 1 decimal place for weight and 2 decimal places for volume.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		public void TestDefaultContainerModes()
		{
			var testRegistryValues = new DefaultContainerModesCollection();
			AssertEquals("Registry type:", testRegistryValues.GetType(), ItemSet.DefaultContainerModes.DefaultValue.GetType());
		}

		public void TestIsConsolDangerousGoodsValidationEnabled()
		{
			AssertEquals("Should be false by default", false, ItemSet.IsConsolDangerousGoodsValidationEnabled.DefaultValue);
			ItemSet.IsConsolDangerousGoodsValidationEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.IsConsolDangerousGoodsValidationEnabled.Value);
		}

		public void TestEnableTransitWarehouseIntegration()
		{
			TestRegistryItem(ItemSet.EnableTransitWarehouseIntegration,
				"DisplayUniquePacklineID",
				"Freight/Pack Line",
				"Enable Transit Warehouse Integration",
				"Enable the integration between Forwarding and Transit Warehouse",
				RegistryStorageFlags.System,
				false);
		}

		public void TestEnableTWPackageLinking()
		{
			TestRegistryItem(ItemSet.EnableTWPackageLinking,
				"EnableTWPackageLinking",
				"Freight/Pack Line",
				"Enable TW Package Linking",
				"Enable TW Package Linking",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableOverpacksAndSealsProjectFeatures()
		{
			AssertEquals("Should be false by default", false, ItemSet.EnableOverpacksAndSealsProjectFeatures.DefaultValue);
			ItemSet.EnableOverpacksAndSealsProjectFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.EnableOverpacksAndSealsProjectFeatures.Value);

			AssertEquals(ItemSet.EnableOverpacksAndSealsProjectFeatures.Category, RawDataRegistry.Categories.Freight);

			TestRegistryItem(ItemSet.EnableOverpacksAndSealsProjectFeatures,
				"EnableOverpacksAndSealsProjectFeatures",
				"Freight",
				"Enable Overpacks & Seals project features",
				"Enable all features developed in the overpacks and seals project. This Registry must be deleted upon release of these features to the whole client base.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableOverpacksGroupingToNewPackLines()
		{
			AssertEquals("Should be false by default", false, ItemSet.EnableOverpacksGroupingToNewPackLines.Value);
			ItemSet.EnableOverpacksGroupingToNewPackLines.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should set the registry item", true, ItemSet.EnableOverpacksGroupingToNewPackLines.Value);

			AssertEquals(ItemSet.EnableOverpacksGroupingToNewPackLines.Category, RawDataRegistry.Categories.Freight_PackLine);

			TestRegistryItem(ItemSet.EnableOverpacksGroupingToNewPackLines,
				"EnableOverpacksGroupingToNewPackLines",
				"Freight/Pack Line",
				"Enable Overpacks Grouping To New Pack Lines",
				"Enable this setting in order to allow the grouping of overpacks sent from TWH integration onto new pack lines. Original pack lines containing the inner packages will be removed from the Shipment in this case. \r\n\r\nNote that Integration with Transit Warehouse must be enabled (Registry > Freight > Pack Line > Enable Transit Warehouse Integration).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableCarrierContractAllocations()
		{
			TestRegistryItem(ItemSet.EnableCarrierContractAllocations,
				"EnableCarrierContractAllocationsCWNext",
				"Freight/Contracts",
				"Enable Carrier Contract Allocations",
				@"Set this registry to 'Yes' to enable Allocation Routes under Carrier Contract & Allocations module for production of clients who have subscribed to these functions.
Set this registry to 'Yes' to enable Allocation Routes under Carrier Contract & Allocations module for trial or testing in non-Production environment.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableCarrierContractTariffsAndRates()
		{
			TestRegistryItem(ItemSet.EnableCarrierContractTariffsAndRates,
				"EnableCarrierContractTariffsAndRatesCWNext",
				"Freight/Contracts",
				"Enable Carrier Contract Tariffs And Rates",
				(NoResString)@"Set this registry to 'Yes' to enable Tariffs & Rates configuration under Carrier Contract & Allocations module for clients who have subscribed to these functions.
Set this registry to 'Yes' to enable Tariffs & Rates configuration under Carrier Contract & Allocations module for trial or testing in non-Production environment.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableContainerWeightLimitSupportOnAllocationRoutes()
		{
			TestRegistryItem(ItemSet.EnableContainerWeightLimitSupportOnAllocationRoutes,
				"EnableContainerWeightLimitSupportOnAllocationRoutes",
				"Freight/Contracts",
				"Enable Container Weight Limit Support On Allocation Routes",
				@"If set to ‘Yes’, Container Weight Limit Support will be activated for Allocation Routes. Note: This is temporary and will be removed in WI00881108.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes()
		{
			TestRegistryItem(ItemSet.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes,
				"EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes",
				"Freight/Contracts",
				"Enable Place of Receipt and Place of Delivery Support On Allocation Routes",
				@"Set this registry to 'Yes' to enable Place of Receipt and Place of Delivery on Allocation Routes. Note: This is temporary and will be removed in WI00894799.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnablePrioritySupportOnAllocationRoutes()
		{
			TestRegistryItem(ItemSet.EnablePrioritySupportOnAllocationRoutes,
				"EnablePrioritySupportOnAllocationRoutes",
				"Freight/Contracts",
				"Enable Priority Support On Allocation Routes",
				@"If set to 'Yes', Priority will be enabled for Allocation Routes",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableAllocatedEventGenerationOnConsolidations()
		{
			TestRegistryItem(ItemSet.EnableAllocatedEventGenerationOnConsolidations,
				"EnableAllocatedEventGenerationOnConsolidations",
				"Freight/Contracts",
				"Enable Allocated Event Generation on Consolidations",
				@"If set to 'Yes', Allocated Event Generation will be enabled for Consolidations",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableDangerousGoodsPortal()
		{
			TestRegistryItem(ItemSet.EnableDangerousGoodsPortal,
				"EnableDangerousGoodsPortal",
				"Freight",
				"Enable Dangerous Goods Portal",
				"If set to ‘Yes’, Dangerous Goods Portal will be activated",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableFreightSpotRateOnAllocationRoutes()
		{
			TestRegistryItem(ItemSet.EnableFreightSpotRateOnAllocationRoutes,
				"EnableFreightSpotRateOnAllocationRoutes",
				"Freight/Contracts",
				"Enable Freight Spot Rate On Allocation Routes",
				"If set to 'Yes', Freight Spot Rate will be enabled for Allocation Routes",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableSubAllocationRouteSupport()
		{
			TestRegistryItem(ItemSet.EnableSubAllocationForAllocationRoutes,
				"EnableSubAllocationForAllocationRoutes",
				"Freight/Contracts",
				"Enable Sub Allocation For Allocation Routes",
				"If set to 'Yes', Sub Allocations will be enabled for Allocation Routes",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableBlankQuantityForSubAllocations()
		{
			TestRegistryItem(ItemSet.EnableBlankQuantityForSubAllocations,
				"EnableBlankQuantityForSubAllocations",
				"Freight/Contracts",
				"Enable Blank Quantity For Sub Allocation Routes",
				"If set to 'Yes', Blank Quantity will be enabled for Sub Allocation Routes",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableDemandQuantityOnAllocationRoutes()
		{
			TestRegistryItem(ItemSet.EnableDemandQuantityOnAllocationRoutes,
				"EnableDemandQuantityOnAllocationRoutes",
				"Freight/Contracts",
				"Enable Demand Quantity On Allocation Routes",
				"If set to 'Yes', Demand Quantity will be enabled for Allocation Routes",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestMaximumHierarchyDepthOfAMGRelatedParties()
		{
			TestRegistryItem(
				ItemSet.MaximumHierarchyDepthOfAMGRelatedParties,
				"MaximumHierarchyDepthOfAMGRelatedParties",
				"Freight/Organizations",
				"Maximum Hierarchy Depth of AMG Related Parties",
				@"Maximum allowable hierarchy depth for configuring 'AMG - Allocations Management Group' type Related Parties between organizations'.
Note: Setting this registry value above 5 may lead to performance issues when searching for carrier contracts with allocations assigned to non-exclusive customers.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				5,
				2,
				int.MaxValue
			);
		}

		public void TestEnableRelatedOrganizationForCustomerSpecificAllocations()
		{
			TestRegistryItem(ItemSet.EnableRelatedOrganizationForCustomerSpecificAllocations,
				"EnableRelatedOrganizationForCustomerSpecificAllocations",
				"Freight/Contracts",
				"Enable Related Organization for Customer Specific Allocations",
				"Set this registry to 'Yes' to enable Related Organization for Customer Specific Allocations.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region Implementation

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "EnableConsolTypesForSuperManifest";
				yield return "AllowImportAdditionalHVLVLines";
				yield return "DisplayUniquePacklineID";
				yield return "EnableTransitDispatchExperiment";
				yield return "SupplierBookingNumberFormat";
				yield return "ContainerLoadListNumberFormat";
				yield return "EnableTWPackageLinking";
			}
		}

		#endregion
	}
}
