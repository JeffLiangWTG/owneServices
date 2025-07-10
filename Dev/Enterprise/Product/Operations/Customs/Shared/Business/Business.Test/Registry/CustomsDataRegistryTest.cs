using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(CustomsDataRegistry))]
	sealed class CustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<CustomsDataRegistry>
	{
		public void TestWebServiceTimeoutInSeconds()
		{
			TestRegistryItem(ItemSet.WebServiceTimeoutInSeconds,
				"WebServiceTimeout",
				CustomsDataRegistry.Categories.Customs_Integration,
				"Web Service Timeout",
				"Web Service Timeout in Seconds",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				60, 60, 300);
		}

		public void TestLocalCountryCustomsInterface()
		{
			TestGenericRegistryItem(ItemSet.LocalCountryCustomsInterface,
				"LocalCountryCustomsInterface",
				CustomsDataRegistry.Categories.Customs_Integration,
				"Local Country Customs Interface",
				"The fields below are used to setup interfacing into CW1 for a local country. The ID should be supplied to you by the provider of your local customs software. The \"Default Submission\" will control if entries can be submitted to Customs directly by CW1 or will be submitted via external Customs systems.",
				RegistryStorageFlags.Company);
		}

		public void TestEnableByProductFunctionality()
		{
			TestRegistryItem(ItemSet.EnableByProductFunctionality,
				"EnableByProductFunctionality",
				CustomsDataRegistry.Categories.Customs,
				"Enable By Product Functionality",
				"Enable this option to show By Product Functionality in supported countries.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestAllowBillingImportIntoDeclaration()
		{
			TestRegistryItem(ItemSet.AllowBillingImportIntoDeclaration,
				"AllowBillingImportIntoDeclaration",
				CustomsDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations,
				"Import Billing Info From XML File",
				"Enable this option to allow billing information to be imported into the customs declaration.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestWHSUniversalXMLChangeDate()
		{
			TestGenericRegistryItem(ItemSet.WHSUniversalXMLChangeDate,
					"DecWHSUniversalXMLChangeDate",
					CustomsDataRegistry.Categories.Customs,
					"Universal XML Integration Effective Date",
					"The date when the WHS Universal XML Integration is effective.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					DateTime.MinValue);
			var editInfo = (DateTimeRegistryEditorInfo)ItemSet.WHSUniversalXMLChangeDate.EditorInfo;
			AssertEquals(ZDateTimePickerFormat.Long, editInfo.DateTimeFormat);
		}

		public void TestIncludeBillingInformationInXMLFile()
		{
			TestRegistryItem(ItemSet.IncludeBillingInformationInXMLFile,
				"IncludeBillingInformationInXMLFile",
				CustomsDataRegistry.Categories.System_DataExportSettings_CustomsDeclarations,
				"Include Billing Information in XML File",
				"Enable this option to include billing information to export customs declaration XML file.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestEnableExperimentalAccountingIntegration()
		{
			AssertEquals("Disabled by default", false, ItemSet.EnableAccountingIntegration.Value.EnableAccountingIntegration);
			AssertEquals("Disabled by default", false, ItemSet.EnableAccountingIntegration.Value.APPostDSB);
			AssertEquals("Disabled by default", false, ItemSet.EnableAccountingIntegration.Value.ARPostDSB);

			AssertEquals("Disabled by default", false, ItemSet.EnableAccountingIntegration.Value.PreApprovalBillingJob);

			AssertEquals("Integration category", "Customs/Integration", ItemSet.EnableAccountingIntegration.Category);

			AssertContainsExactElementsInAnyOrder("Supported countries.", ItemSet.EnableAccountingIntegration.CountryFilterPKs,
				RegistryItemSet.CountryFilterPKs.Australia
				.Concat(RegistryItemSet.CountryFilterPKs.Canada)
				.Concat(RegistryItemSet.CountryFilterPKs.Spain)
				.Concat(RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments)
				.Concat(RegistryItemSet.CountryFilterPKs.NewZealand)
				.Concat(RegistryItemSet.CountryFilterPKs.UnitedKingdom)
				.Concat(Core.CountryGuids.CountriesUnderUSCustomsJurisdiction)
				.Concat(RegistryItemSet.CountryFilterPKs.SouthAfrica)
				.Concat(RegistryItemSet.CountryFilterPKs.Germany));
		}

		public void TestAutoBillingDueDateFromPaymentTerms()
		{
			TestRegistryItem(ItemSet.AutoBillingDueDateFromPaymentTerms,
				"AutoBillingDueDateFromPaymentTerms",
				CustomsDataRegistry.Categories.Customs_Integration,
				"Use Payment Terms to calculate Due Date",
				"This option allows to calculate the Due Date based on the Creditor’s Payment Terms when customs disbursements are populated after Auto Billing process.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestIncludeARInvoicesInXMLFile()
		{
			TestRegistryItem(ItemSet.IncludeARInvoicesInXMLFile,
				"IncludeARInvoicesInXMLFile",
				CustomsDataRegistry.Categories.System_DataExportSettings_CustomsDeclarations,
				CustomsDataRegistry.ExportARInvoicesCaption,
				CustomsDataRegistry.ExportARInvoicesHint,
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestCompany()
		{
			TestGenericRegistryItem(ItemSet.CustomsWareCompany,
				"CustomsWareCompany",
				CustomsDataRegistry.Categories.Customs_Integration_CustomsWare_WebService,
				"Company",
				"Company",
				RegistryStorageFlags.Company);
		}

		public void TestCreditCheckOnMessageSend()
		{
			TestRegistryItem(ItemSet.CreditCheckOnMessageSend,
				"CreditCheckOnCustomsMessageSend",
				CustomsDataRegistry.Categories.Customs,
				"Check the credit limit and on hold status of related parties when sending messages",
				"Do you wish to check the credit limit and on hold status when sending messages? Messages may only be sent when the parties have sufficient credit.",
				RegistryStorageFlags.Company,
				true);
			AssertEquals(ItemSet.CreditCheckOnMessageSend.CountryFilterPKs, CustomsDataRegistry.CreditCheckCountries);
		}

		public void TestCreditCheckCountries()
		{
			var expectedCountries = new List<Guid>
			{
				Enterprise.Core.Constants.CountryGuids.Canada,
				Enterprise.Core.Constants.CountryGuids.UnitedKingdom,
				Enterprise.Core.Constants.CountryGuids.SouthAfrica,
				Enterprise.Core.Constants.CountryGuids.Spain,
				Enterprise.Core.Constants.CountryGuids.Germany,
				Enterprise.Core.Constants.CountryGuids.Italy,
				Enterprise.Core.Constants.CountryGuids.Ireland,
				Enterprise.Core.Constants.CountryGuids.Netherlands,
				Enterprise.Core.Constants.CountryGuids.Switzerland,
				Enterprise.Core.Constants.CountryGuids.Norway,
			};

			foreach (var country in expectedCountries)
			{
				Assert($"Country {country} should be in the list", CreditCheckCountries.Contains(country));
			}
		}

		public void TestIsAUSeaCargoHouseEnabled()
		{
			var customsRegistry = CustomsDataRegistry.Instance;
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.SeaCargoHouse, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals("Setting FUNCS code should enable property", true, customsRegistry.IsAUSeaCargoHouseEnabled);
			}

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.SeaCargoHouse, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertEquals("Turning off FUNCS code should disable property", false, customsRegistry.IsAUSeaCargoHouseEnabled);
			}

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PilotSeaCargoHouse, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals("Setting PFUNC code should enable property", true, customsRegistry.IsAUSeaCargoHouseEnabled);
			}

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.PilotSeaCargoHouse, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertEquals("Turning off PFUNC code should disable property", false, customsRegistry.IsAUSeaCargoHouseEnabled);
			}
		}

		public void TestEnableWarehouseInventory()
		{
			TestRegistryItem(ItemSet.EnableWarehouseInventory,
				"EnableWarehouseInventory",
				RawDataRegistry.Categories.Customs,
				"Enable Warehouse Inventory",
				"Enable Warehouse Inventory Functionality",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestEnableInwardProcessing()
		{
			TestGenericRegistryItem(
				item: ItemSet.EnableInwardProcessing,
				expectedName: "EnableInwardProcessing",
				expectedCategory: RawDataRegistry.Categories.Customs_EuropeanUnionCommon_InwardProcessing,
				expectedCaption: "Enable Inward Processing Function",
				expectedHint: "When set to 'Yes', Inward Processing related functionalities will be enabled.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false);
		}

		public void TestSupportWarehouseOrderLines()
		{
			TestRegistryItem(ItemSet.SupportWarehouseOrderLines,
				"SupportWarehouseOrderLines",
				RawDataRegistry.Categories.Customs,
				"Support Warehouse Order Lines",
				"Enable Support selection of warehouse order lines in invoice lines.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestCopyPreviousLineCustomsProcedureCode()
		{
			TestRegistryItem(ItemSet.CopyPreviousLineCustomsProcedureCode,
				"CopyPreviousLineCustomsProcedureCode",
				RawDataRegistry.Categories.Customs,
				"Copy previous invoice line's customs procedure code",
				"Enable functionality to copy previous invoice line's customs procedure code",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				true);
		}

		public void TestCopyCustomFieldsFromInvoiceLineToProduct()
		{
			TestRegistryItem(ItemSet.CopyCustomFieldsFromInvoiceLineToProduct,
				"CopyCustomFieldsFromInvoiceLineToProduct",
				RawDataRegistry.Categories.Customs,
				"Always copy matched customized field values from invoice line to product",
				"When this registry is set to “Yes”, matched customized field values will be copied from invoice line when a new product is created.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				true);
		}

		public void TestInvoiceCharges_Export()
		{
			TestRegistryItem(ItemSet.InvoiceChargesForExport,
				"InvoiceChargesForExport",
				Categories.Customs,
				"Invoice Charges Export Editor",
				"Change default value of the Invoice Charges “Distribute By”-value in Export Declarations.",
				RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				new ChargeDistributeByList(),
				ChargeDistributeByList.Codes.Weight);
		}

		public void TestInvoiceCharges_Import()
		{
			TestRegistryItem(ItemSet.InvoiceChargesForImport,
				"InvoiceChargesForImport",
				Categories.Customs,
				"Invoice Charges Import Editor",
				"Change default value of the Invoice Charges “Distribute By”-value in Import Declarations.",
				RegistryStorageFlags.Company,
				RegistryOptions.CannotCallParameterlessValueGetter,
				new ChargeDistributeByList(),
				ChargeDistributeByList.Codes.Value);
		}

		#region Enable Auto Tariff Description Population

		public void TestEnableAutoTariffDescriptionPopulation()
		{
			TestGenericRegistryItem(
				ItemSet.EnableAutoTariffDescriptionPopulation,
				"EnableAutoTariffDescriptionPopulation",
				RawDataRegistry.Categories.Customs,
				"Enable Auto Population of Goods Description",
				"Enable Auto Population of Goods Description",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter
			);

			AssertEquals("Registry EnableAutoTariffDescriptionPopulation.EnableCommercialInvoice value should default to true.", ZBool.True, CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.DefaultValue.EnableCommercialInvoice);
			AssertEquals("Registry EnableAutoTariffDescriptionPopulation.EnableCustomsDeclaration value should default to true.", ZBool.True, CustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.DefaultValue.EnableCustomsDeclaration);
		}

		#endregion

		#region Universal Customs Message Processing

		public void TestEnableUCMTest()
		{
			TestRegistryItem(ItemSet.EnableUCMTest,
				"EnableUCMTest",
				RawDataRegistry.Categories.Customs,
				"Enable UCM Tests",
				"Enable Universal Customs Messaging testing using Application Codes define in 'UCM Test Application Codes'",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestUCMResourcePercentageUsage()
		{
			TestRegistryItem(ItemSet.UCMResourcePercentageUsage,
				"UCMResourcePercentageUsage",
				RawDataRegistry.Categories.Customs,
				"UCK/UCQ Resource Percentage",
				"A percentage of resources one application code can use to run UCK or UCQ service task",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				80);
		}

		public void TestUCMTestApplicationCodes()
		{
			TestGenericRegistryItem(ItemSet.UCMTestApplicationCodes,
				"UCMTestApplicationCodes",
				RawDataRegistry.Categories.Customs,
				"UCM Test Application Codes",
				"List of application codes to use in UCM testing.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers);
			AssertEquals("UCMTestApplicationCodes.Value.Count", 0, ItemSet.UCMTestApplicationCodes.Value.Count);
		}

		public void TestUCKPreEnqueuerMaxBacklogSize()
		{
			TestRegistryItem(ItemSet.UCKPreEnqueuerMaxBacklogSize,
				"UCKPreEnqueuerMaxBacklogSize",
				RawDataRegistry.Categories.Customs,
				"UCK - Max Backlog Size",
				"The maximum number of EDIMessageQueueState rows added by the UCK service task before it stops calculating keys. If this capacity is reached and you have a large number of UCK service task runners, this value can be increased to ensure there are enough messages ready for UCK to process. This should be increased incrementally to achieve balance between keeping messages queued for UCK and managing a larger EDIMessageQueueState table.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				150000);
		}

		public void TestUCKPreEnqueuerIncreasePerformanceLevelDurationInMillis()
		{
			TestRegistryItem(ItemSet.UCKPreEnqueuerIncreasePerformanceLevelDurationInMillis,
				"UCKPreEnqueuerIncreasePerformanceLevelDurationInMillis",
				RawDataRegistry.Categories.Customs,
				"UCK - Performance level increase duration",
				"How long getting a batch should take before we think it is broken.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden | RegistryOptions.NotCached,
				(int)TimeSpan.FromSeconds(20).TotalMilliseconds);
		}

		public void TestUCKPreEnqueuerResetPerformanceLevelDurationInMillis()
		{
			TestRegistryItem(ItemSet.UCKPreEnqueuerResetPerformanceLevelDurationInMillis,
				"UCKPreEnqueuerResetPerformanceLevelDurationInMillis",
				RawDataRegistry.Categories.Customs,
				"UCK - Performance level increase duration",
				"How long getting a batch ought to take.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden | RegistryOptions.NotCached,
				(int)TimeSpan.FromSeconds(1).TotalMilliseconds);
		}

		public void TestUCKMessagesPerBatch()
		{
			TestRegistryItem(ItemSet.UCKMessagesPerBatch,
				"UCKMessagesPerBatch",
				RawDataRegistry.Categories.Customs,
				"UCK - Messages Per Batch",
				"The number of messages in each batch (per save) of UCK services task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				50);
		}

		public void TestUCKMessagesPerExecution()
		{
			TestRegistryItem(ItemSet.UCKMessagesPerExecution,
				"UCKMessagesPerExecution",
				RawDataRegistry.Categories.Customs,
				"UCK - Messages Per Execution",
				"The number of messages in each execution of the UCK service task per application code before it move on to the next application code.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				10000);
		}

		public void TestUCQMessagesPerExecution()
		{
			TestRegistryItem(ItemSet.UCQMessagesPerExecution,
				"UCQMessagesPerExecution",
				RawDataRegistry.Categories.Customs,
				"UCQ - Messages Per Execution",
				"The number of messages in each execution of the UCQ service task per application code before it move on to the next application code.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				10000);
		}

		public void TestParallelUCMQueueHistoryInHours()
		{
			TestRegistryItem(ItemSet.ParallelUCMQueueHistoryInHours,
				"ParallelUCMQueueHistoryInHours",
				RawDataRegistry.Categories.Customs,
				"UCI - Parallel Queue History in Hours",
				"How many hours should EDIMessageQueueState rows with the status PRS be persisted.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				0);
		}

		public void TestUCIMessagesPerBatch()
		{
			TestRegistryItem(ItemSet.UCIMessagesPerBatch,
				"UCIMessagesPerBatch",
				RawDataRegistry.Categories.Customs,
				"UCI - Messages Per Batch",
				"The number of messages in each batch (per save) of UCI services task.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				10000);
		}

		public void TestUCIMessagesPerExecution()
		{
			TestRegistryItem(ItemSet.UCIMessagesPerExecution,
				"UCIMessagesPerExecution",
				RawDataRegistry.Categories.Customs,
				"UCI - Messages Per Execution",
				"The number of messages in each execution of the UCI service task per application code before it move on to the next application code.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				10000);
		}

		public void TestUCMMessageQueueCapacity()
		{
			TestRegistryItem(ItemSet.UCMMessageQueueCapacity,
				"UCMMessageQueueCapacity",
				RawDataRegistry.Categories.Customs,
				"Universal Customs Message Queue Max Backlog Size",
				"The number of messages that can be queued in the EDIMessageQueueState table. If you have a large number of UCK or UCI service task runners and message back logs, this can be increased to ensure each UCK or UCI runner has messages to process. This should be increased incrementally to achieve balance between keeping messages queued for UCK or UCI and managing a larger EDIMessageQueueState table.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				100000);
		}

		public void TestUCMSetChainIDLogging()
		{
			TestRegistryItem(ItemSet.UCMSetChainIDLogging,
				"UCMSetChainIDLogging",
				RawDataRegistry.Categories.Customs,
				"UCMP Log Chain ID settng",
				"Log when a Chain ID is set in UCMP service tasks",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestUCMExtendedUniversalLogging()
		{
			TestGenericRegistryItem(ItemSet.UCMExtendedUniversalLogging,
				"UCMExtendedUniversalLogging",
				RawDataRegistry.Categories.Customs,
				"Universal Customs Message Extended Logging Options",
				"Additional log toggles for monitoring in the UCI, UCQ and UCK service tasks.",
				RegistryStorageFlags.System);
			var editorInfo = (CodeDescriptionBoolRegistryEditorInfo)ItemSet.UCMExtendedUniversalLogging.EditorInfo;
			AssertEquals("BoolColumnCaption", "Enable Logs", editorInfo.BoolColumnCaption);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertEquals("IsOnlyBoolColumnEditable", true, editorInfo.IsOnlyBoolColumnEditable);
			var collection = ItemSet.UCMExtendedUniversalLogging.Value;
			AssertEquals("collection.Count", 9, collection.Count);

			Assert(collection[0], "0", ExtendedUniversalLoggingKeys.UCIFirstMessageLoad, "UCI - First message load", false);
			Assert(collection[1], "1", ExtendedUniversalLoggingKeys.UCIMessageAtFront, "UCI - Messages at front of queue", false);
			Assert(collection[2], "2", ExtendedUniversalLoggingKeys.UCIChainStatistics, "UCI - Chain statistics", false);
			Assert(collection[3], "3", ExtendedUniversalLoggingKeys.UCIShowOldest, "UCI - Show oldest message", false);
			Assert(collection[4], "4", ExtendedUniversalLoggingKeys.UCIAllLoads, "UCI - All message loads", false);
			Assert(collection[5], "5", ExtendedUniversalLoggingKeys.UCKAllLoads, "UCK - All message loads", false);
			Assert(collection[6], "6", ExtendedUniversalLoggingKeys.UCKLocksTaken, "UCK - Locks taken", false);
			Assert(collection[7], "7", ExtendedUniversalLoggingKeys.UCQAllLoads, "UCQ - All message loads", false);
			Assert(collection[8], "8", ExtendedUniversalLoggingKeys.UCQLocksTaken, "UCQ - Locks taken", false);
			void Assert(CodeDescriptionBool codeDescriptionBoolDisallowNew, string prefix, string code, string description, bool value)
			{
				AssertEquals(prefix + " - Code", code, codeDescriptionBoolDisallowNew.Code);
				AssertEquals(prefix + " - Description", description, codeDescriptionBoolDisallowNew.Description);
				AssertEquals(prefix + " - Bool", value, codeDescriptionBoolDisallowNew.Bool);
			}
		}

		public void TestUCUInterchangesPerBatch()
		{
			TestRegistryItem(ItemSet.UCUInterchangesPerBatch,
				"UCUInterchangesPerBatch",
				CustomsDataRegistry.Categories.Customs,
				"Unpacking Interchanges Per Batch",
				"The number of interchanges in each batch (per save) of Interchanges Unpacking services tasks(UCU).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				50);
		}

		public void TestUCUInterchangeUnpackingMaxRetryCount()
		{
			TestRegistryItem(ItemSet.UCUInterchangeUnpackingMaxRetryCount,
				"UCUInterchangeUnpackingMaxRetryCount",
				CustomsDataRegistry.Categories.Customs,
				"Max Retry Count to Unpacking Interchange",
				"The Max Retry Count to Unpacking Interchange in Interchanges Unpacking services tasks(UCU).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				3);
		}

		public void TestUCPInterchangesPerBatch()
		{
			TestRegistryItem(ItemSet.UCPInterchangesPerBatch,
				"UCPInterchangesPerBatch",
				CustomsDataRegistry.Categories.Customs,
				"Packing Interchanges Per Batch",
				"The number of interchanges in each batch (per save) of Interchanges Packing services tasks(UCP).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				50);
		}

		public void TestUCPInterchangePackingMaxRetryCount()
		{
			TestRegistryItem(ItemSet.UCPInterchangePackingMaxRetryCount,
				"UCPInterchangePackingMaxRetryCount",
				CustomsDataRegistry.Categories.Customs,
				"Max Retry Count for Packing Interchange",
				"The Max Retry Count for Packing Interchange in Interchanges Packing services tasks(UCP).",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				3);
		}

		#endregion

		public void TestCustomsDeclarationAuditingCategories()
		{
			TestRegistryItemWithDefaultCode(ItemSet.CustomsDeclarationAuditingCategories,
				"CustomsDeclarationAuditingCategories",
				Categories.Customs_DeclarationAuditing,
				"Audit Types",
				"Enter or override codes to categorize the types of audit you will undertake. These codes will be offered to users in the audit screen's 'Category' drop-down.",
				RegistryStorageFlags.Company,
				3,
				3);
		}

		public void TestCustomsDeclarationAuditingResults()
		{
			TestRegistryItemWithDefaultCode(ItemSet.CustomsDeclarationAuditingResults,
				"CustomsDeclarationAuditingResults",
				Categories.Customs_DeclarationAuditing,
				"Result or outcome codes",
				"Enter or override codes that user must select to complete their audit, to express the findings, results or conclusions of the process.",
				RegistryStorageFlags.Company,
				3,
				3);
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				var list = new List<string>(base.ConditionallyVisibleRegistryItems);
				list.Add("EnableQueryInterchangeByEHubPortalWebservice");
				list.Add("DefaultWeightFromInvoiceQty");
				list.Add("CustomsTransactionalBillingTaskNextRunDate");
				list.Add("CustomsTransactionalBillingTaskLastRunDate_BillingAPI");
				list.Add("OrdersPerDeclarationLimit");
				list.Add("OrdersPerDeclarationLimitIntroductionTimeUTC");
				list.Add("EnableRulesModule");
				list.Add("SeverityLevelForMissingUSMCACUSMACertificate");
				return list;
			}
		}

		public void TestDefaultInsuranceRate()
		{
			TestRegistryItem(ItemSet.DefaultInsuranceRate,
				"DefaultInsuranceRate",
				Categories.Customs,
				"Default Insurance Rate",
				"Override this value to default % of Line Price when 'ONS' charge code is added on Invoice Charges.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				0m);
		}

		public void TestNoOfGuaranteesBGCServiceProcessPerBatch()
		{
			TestRegistryItem(ItemSet.NoOfGuaranteesBGCServiceProcessPerBatch,
				"NoOfGuaranteesBGCServiceProcessPerBatch",
				Categories.Customs,
				"Number of guarantee records the Guarantee Balance Calculation service task will process per batch",
				"Number of guarantee records the Guarantee Balance Calculation service task will process per batch. If guarantee usage exceeds this threshold, the unprocessed records will be processed in the next batch.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				50,
				1,
				int.MaxValue);
		}

		public void TestSeverityLevelOfUsageCommentValidation()
		{
			TestRegistryItem(ItemSet.SeverityLevelOfUsageCommentValidation,
				"SeverityLevelOfUsageCommentValidation",
				Categories.Customs,
				"Severity Level Of Usage Comment Validation",
				"Set the severity level for validation relating to the Usage Comment on an Invoice Line. The Usage Comment is defaulted to the Invoice Line from the Classification Line on the Product. Indicate the system behavior if a user does not indicate that they have read the Usage Comment on the Invoice Line.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				new ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
		}

		public void TestASNRefreshOptions()
		{
			TestGenericRegistryItem(ItemSet.ASNRefreshOptions,
				"ASNRefreshOptions",
				Categories.Customs,
				"Refresh options when attaching an ASN type invoice to a declaration",
				"Select the fields you want to be refreshed from the Product file when a Commercial Invoice (Type = ASN-Advance Shipping Notice) is attached to a declaration. If the grid is blank ALL fields will be refreshed.",
				RegistryStorageFlags.Company);
		}

		public void TestASNRefreshOptions_SetValue()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);
			var config1 = collection.AddNew();
			config1.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;
			var config2 = collection.AddNew();
			config2.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.CountryOfOrigin;

			using (CustomsDataRegistry.Instance.ASNRefreshOptions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals("CLASS", ItemSet.ASNRefreshOptions.Value[0].FieldType);
				AssertEquals("COO", ItemSet.ASNRefreshOptions.Value[1].FieldType);
			}
		}

		public void TestDeclarationLockForEdit_Generic()
		{
			TestGenericRegistryItem(ItemSet.DeclarationLockForEdit,
				"DeclarationLockForEdit",
				RawDataRegistry.Categories.Customs,
				"Declaration Lock For Edit",
				"If declaration types are added, then the system will attempt to lock (for editing) the declaration of the specified type if an event that causes locking is logged. The tabs that become locked should also be specified. In the case that a declaration has multiple entry types, the declaration will become locked if a lock event is logged against all of the associated entries from the lock mode.\r\nNOTE: Functionality only applies to user edits of a declaration.\r\nNOTE: Applies only to Edits via Customs Declaration or Shipment form. Automated data imports and other automated functions are excluded.",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue);
		}

		public void TestDeclarationLockForEdit_SetValue()
		{
			var defaultValue = ItemSet.DeclarationLockForEdit.DefaultValue;
			AssertEquals("DefaultValue.Count", 0, defaultValue.Count);

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new DeclarationLockConfigCollection(fallbackLevel, Factory);

			var config = collection.AddNew();
			config.DeclarationType = "IMP";

			var eventLockInfo = config.EventInfos.AddNew();
			eventLockInfo.EventType = "ARV";
			eventLockInfo.EventReference = "REF=XXX";
			eventLockInfo.EventSource = "DEC";

			var tabLockInfo = config.TabInfos.AddNew();
			tabLockInfo.TabPage = "SVC";

			using (ItemSet.DeclarationLockForEdit.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals("Value[0].DeclarationType", "IMP", ItemSet.DeclarationLockForEdit.Value[0].DeclarationType);

				AssertEquals("Value[0].EventInfos[0].EventType", "ARV", ItemSet.DeclarationLockForEdit.Value[0].EventInfos[0].EventType);
				AssertEquals("Value[0].EventInfos[0].EventReference", "REF=XXX", ItemSet.DeclarationLockForEdit.Value[0].EventInfos[0].EventReference);
				AssertEquals("Value[0].EventInfos[0].EventSource", "DEC", ItemSet.DeclarationLockForEdit.Value[0].EventInfos[0].EventSource);
				AssertEquals("Value[0].TabInfos[0].TabPage", "SVC", ItemSet.DeclarationLockForEdit.Value[0].TabInfos[0].TabPage);
			}
		}

		public void TestAssumeCreatedProductBelongsToClient()
		{
			TestRegistryItem(ItemSet.AssumeCreatedProductBelongsToClient,
				"AssumeCreatedProductBelongsToClient",
				CustomsDataRegistry.Categories.Customs,
				"Assume Created Product belongs to Client",
				@"This governs the default behavior when a product is created as a result of data entry on a Customs Declaration of a new or unrecognized product.
If ‘Yes’ then the system will create the product and add the Importer organization to the product for Imports and the Supplier organization for Exports. If ‘No’ then the system will create the product and add the Supplier organization to the product for Imports as well as for Exports.
‘Always’ operates in the same way as ‘Yes’, except the user will not be able to change the behavior at the time the product is created. Likewise, ‘Never’ is congruent with ‘No’, but does not allow user discretion.",
				RegistryStorageFlags.Company,
				new ClientProductCreationTypeList(),
				ClientProductCreationTypeList.Codes.Yes);
		}

		public void TestThirdPartyInterfacesItems()
		{
			ItemSet.ThirdPartyInterfacesExportPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "x:\\x");
			ItemSet.ThirdPartyInterfacesImportPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "y:\\y");
			AssertEquals("ThirdPartyInterfacesExportPath.Value", "x:\\x", ItemSet.ThirdPartyInterfacesExportPath.Value);
			AssertEquals("ThirdPartyInterfacesImportPath.Value", "y:\\y", ItemSet.ThirdPartyInterfacesImportPath.Value);
		}

		public void TestPowerOfAttorneyNotificationType()
		{
			TestRegistryItem(ItemSet.PowerOfAttorneyNotificationType,
						"PowerOfAttorneyNotificationType",
						CustomsDataRegistry.Categories.Customs,
						"Power Of Attorney Notification Type",
						"The type of notification that the system will add if Power Of Attorney is missing or expired.",
						RegistryStorageFlags.Company,
						new PowerOfAttorneyNotificationTypeList(),
						PowerOfAttorneyNotificationTypeList.Codes.Warning);
		}

		public void TestGetPowerOfAttorneyNotificationType()
		{
			BusinessObject newCompany = (BusinessObject)Factory.New<IGlbCompany>();
			newCompany[GlbCompanySchema.GC_Name] = "Dummy Company";
			newCompany[GlbCompanySchema.GC_Code] = "Z@Z";
			newCompany[GlbCompanySchema.GC_OH_OrgProxy] = Env.CurrentCompany.OrganisationPK;

			BusinessObject branch1 = (BusinessObject)Factory.New<IGlbBranch>();
			branch1[GlbBranchSchema.GB_BranchName] = "Dummy Branch 1";
			branch1[GlbBranchSchema.GB_Code] = "Z@1";
			branch1[GlbBranchSchema.GB_GC] = newCompany.PK;
			branch1[GlbBranchSchema.GB_OH_OrgProxy] = Env.CurrentBranch.OrganisationPK;

			Factory.Save();

			var newCompanyPK = newCompany.PK.ToGuid();
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(newCompanyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			AssertEquals(NotificationType.MessageError, CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType());
			AssertEquals(NotificationType.Warning, CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType(newCompanyPK));

			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(newCompanyPK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			AssertEquals(NotificationType.Warning, CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType());
			AssertEquals(NotificationType.MessageError, CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType(newCompanyPK));

			((IRegistryItemInternals)CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals(NotificationType.Warning, CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType());
			AssertEquals(NotificationType.MessageError, CustomsDataRegistry.Instance.GetPowerOfAttorneyNotificationType(newCompanyPK));
		}

		public void TestDeclarationNumberCustomisation()
		{
			TestGenericRegistryItem(ItemSet.DeclarationNumberCustomisation, "DeclarationNumberCustomisation", CustomsDataRegistry.Categories.Customs, "Declaration Number Customization", "Override this value to customize how declaration numbers are formatted", RegistryStorageFlags.All);
			BillCustomisationRegistryDataType dataType = (BillCustomisationRegistryDataType)ItemSet.DeclarationNumberCustomisation.DataType;
			AssertEquals("GeneratedNumberName", "Declaration Number", dataType.GeneratedNumberName);
			AssertEquals("SequenceNumberName", "Declaration", dataType.SequenceNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
		}

		public void TestDeclarationNumberCustomisationForInterface()
		{
			AssertEquals(((Enterprise.Integration.Customs.Shared.ICustomsDataRegistry)ItemSet).DeclarationNumberCustomisation, ItemSet.DeclarationNumberCustomisation);
		}

		public void TestNctsLocalReferenceNumberCustomisation()
		{
			TestGenericRegistryItem(ItemSet.NctsLocalReferenceNumberCustomisation, "NctsLocalReferenceNumberCustomisation", CustomsDataRegistry.Categories.Customs, "NCTS Job Number Customization", "Override this value to customize how NCTS job numbers are formatted", RegistryStorageFlags.All);
			BillCustomisationRegistryDataType dataType = (BillCustomisationRegistryDataType)ItemSet.NctsLocalReferenceNumberCustomisation.DataType;
			AssertEquals("GeneratedNumberName", "NCTS Job Number", dataType.GeneratedNumberName);
			AssertEquals("SequenceNumberName", "NCTS Job", dataType.SequenceNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
		}

		public void TestEMCSJobNumberCustomisation()
		{
			TestGenericRegistryItem(ItemSet.EMCSLocalReferenceNumberCustomisation, "EMCSLocalReferenceNumberCustomisation", CustomsDataRegistry.Categories.Customs, "EMCS Job Number Customization", "Override this value to customize how EMCS job numbers are formatted", RegistryStorageFlags.All);
			BillCustomisationRegistryDataType dataType = (BillCustomisationRegistryDataType)ItemSet.EMCSLocalReferenceNumberCustomisation.DataType;
			AssertEquals("Categories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.EMCSDeclaration, dataType.Categories);
			AssertEquals("GeneratedNumberName", "EMCS Job Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
		}

		public void TestConsolidatedEntryNumberCustomisationIsVisible()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				TestGenericRegistryItem(ItemSet.ConsolidatedEntryNumberCustomisation, "ConsolidatedEntryNumberCustomisation", CustomsDataRegistry.Categories.Customs, "Consolidated Entry Number Customization", "Override this value to customize how Consolidated Entry Job Numbers are formatted", RegistryStorageFlags.All, RegistryOptions.Default);
			}
			BillCustomisationRegistryDataType dataType = (BillCustomisationRegistryDataType)ItemSet.ConsolidatedEntryNumberCustomisation.DataType;
			AssertEquals("Categories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.ConsolidatedDeclaration, dataType.Categories);
			AssertEquals("GeneratedNumberName", "Consolidated Entry Job Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
		}

		public void TestConsolidatedEntryNumberCustomisationIsHidden()
		{
			using (RawDataRegistry.Instance.EnableConsolidatedEntries.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				TestGenericRegistryItem(ItemSet.ConsolidatedEntryNumberCustomisation, "ConsolidatedEntryNumberCustomisation", CustomsDataRegistry.Categories.Customs, "Consolidated Entry Number Customization", "Override this value to customize how Consolidated Entry Job Numbers are formatted", RegistryStorageFlags.All, RegistryOptions.IsHidden);
			}
			BillCustomisationRegistryDataType dataType = (BillCustomisationRegistryDataType)ItemSet.ConsolidatedEntryNumberCustomisation.DataType;
			AssertEquals("Categories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.ConsolidatedDeclaration, dataType.Categories);
			AssertEquals("GeneratedNumberName", "Consolidated Entry Job Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
		}

		public void TestEnableGenericMessageDeliveryWebService()
		{
			TestRegistryItem(ItemSet.EnableGenericMessageDeliveryWebService, "EnableGenericMessageDeliveryWebService", CustomsDataRegistry.Categories.Customs, "Enable Generic Message Delivery Service", "Enables Generic Message Delivery Service", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableQueryInterchangeByEHubPortalWebservice()
		{
			TestRegistryItem(ItemSet.EnableQueryInterchangeByEHubPortalWebservice, "EnableQueryInterchangeByEHubPortalWebservice", CustomsDataRegistry.Categories.Customs, "Enable Query Interchange By eHub Portal Web service", "If set to 'yes' the system will enable the query menu on Declaration - Messages, our clients can check the interchange on https://ehubadmin.wtg.zone/Message.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestEnableAutoApportionWeight()
		{
			TestRegistryItem(ItemSet.EnableAutoApportionWeight, "EnableAutoApportionWeight", CustomsDataRegistry.Categories.Customs, "Default Auto Apportion Weight", "When the value of this registry item is set to 'Yes', the 'Auto Apportion Weight ' menu item under the Customs Declaration > Brokerage path will always be checked by default.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, RegistryOptions.CannotCallParameterlessValueGetter, false);
		}

		public void TestAlwaysCopyFromPreviousLine()
		{
			TestRegistryItem(ItemSet.AlwaysCopyFromPreviousLine, "AlwaysCopyFromPreviousLine", CustomsDataRegistry.Categories.Customs, "Always Copy from Previous Line", "When the value of this registry item is set to 'Yes', the 'Copy from Previous Line' menu item will always be ticked.", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter, false);
		}

		public void TestDefaultWeightFromInvoiceQty()
		{
			TestRegistryItem(ItemSet.DefaultWeightFromInvoiceQty, "DefaultWeightFromInvoiceQty", CustomsDataRegistry.Categories.Customs, "Default Gross Weight From Invoice Qty",

				"Set this to 'Yes' in order to automatically default the gross weight from an invoice quantity when the invoice unit of quantity is a weight unit.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.IsOnlyForSupport, false);
		}

		const string ExpectedCategory = "Customs";

		public void TestEnableExactMatchForProduct()
		{
			TestRegistryItem(
				ItemSet.EnableExactMatchForProduct,
				"EnableExactMatchForProduct",
				ExpectedCategory,
				"Enable Exact Match for Product from Declaration",
				@"If set to 'yes' the system will match and retrieve a product code entered in the Commercial Invoice lines with a product code in the product files, if that product code has:
The same importer and supplier matching the declaration details; OR
The same importer matching the declaration details, but there are no Suppliers listed on the organizations grid for the product code; OR
The same supplier matching the declaration details, but there are no Importers listed on the organizations grid for the product code.",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestPopulateOwnersRef()
		{
			TestRegistryItem(
				ItemSet.PopulateOwnersRef,
				"PopulateOwnersRef",
				ExpectedCategory,
				"Populate Owner's Ref with Order Numbers",
				"If set to 'Yes', the system will default Order Numbers into the Declaration Owner Reference field for both Shipments and Customs Declarations. Note: Any values defaulting into Owner Ref will be truncated to the valid allowable size.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestShowHeaderTariffData()
		{
			TestRegistryItem(
				ItemSet.ShowHeaderTariffData,
				"ShowHeaderTariffData",
				ExpectedCategory,
				"Show Header Tariff Data",
				"If set to 'Yes', the system will display top level data in the result of a Tariff Search.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				true);
		}

		public void TestAlwaysAssumeLocalOrganisationIsBothImporterAndExporter()
		{
			TestRegistryItem(
				ItemSet.AlwaysAssumeLocalOrganisationIsBothImporterAndExporter,
				"AlwaysAssumeLocalOrganisationIsBothImporterAndExporter",
				ExpectedCategory,
				"Always Assume the Local Organization is Both an Importer and an Exporter",
				"Always assume the Owner on an import declaration, and the Supplier on an export declaration, is both an Importer and Exporter of any part created during the declaration process, i.e. the 'Relationship' specified on the 'Related Organizations' tab of a part created from a declaration will be set to 'BTH'.",
				RegistryStorageFlags.All,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestEnableAutoRefreshProductData()
		{
			TestRegistryItem(
				ItemSet.EnableAutoRefreshProductData,
				"EnableAutoRefreshProductData",
				ExpectedCategory,
				"Auto Refresh Product Data",
				"When the value of this registry item is set to 'Yes', product details will refresh automatically.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				RegistryOptions.Default,
				true);
		}

		public void TestImportProductAuditAction()
		{
			TestRegistryItem(ItemSet.ImportProductAuditAction,
				"ImportProductAuditAction",
				CustomsDataRegistry.Categories.Customs_AuditActions,
				"Import Product Audit Action",
				"Set the Product Audit Action for unaudited product classifications in Import Declarations.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);

			var countryFilterPks = ItemSet.ImportProductAuditAction.CountryFilterPKs;
			AssertCollectionNotContains("Canada is excluded", Core.Constants.CountryGuids.Canada, countryFilterPks);
			AssertCollectionContains("Australia is included", Core.Constants.CountryGuids.Australia, countryFilterPks);
			AssertSame("Country PKs are cached", countryFilterPks, ItemSet.ImportProductAuditAction.CountryFilterPKs);
			AssertNotNull("Cache holds an evaluated Query", (Guid[])countryFilterPks);
		}

		public void TestExportProductAuditAction()
		{
			TestRegistryItem(ItemSet.ExportProductAuditAction,
				"ExportProductAuditAction",
				CustomsDataRegistry.Categories.Customs_AuditActions,
				"Export Product Audit Action",
				"Set the Product Audit Action for unaudited product classifications in Export Declarations.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
		}

		public void TestMaximumAgeforRepresentativeShipmentCalculationMethod()
		{
			TestRegistryItem(ItemSet.MaximumAgeforRepresentativeShipmentCalculationMethod,
				"MaximumAgeforRepresentativeShipmentCalculationMethod",
				"Customs/Drawbacks",
				"Maximum Age for Representative Shipment Calculation Method",
				"The maximum age (in days) of import shipments to be considered when calculating claim amount using Representative Shipment method.",
				RegistryStorageFlags.Company,
				365);
		}

		public void TestHistoryWindowForExceptionReportOfUnusualUnitValue()
		{
			TestRegistryItem(ItemSet.HistoryWindowForExceptionReportOfUnusualUnitValue,
				"HistoryWindowForExceptionReportOfUnusualUnitValue",
				"Customs/Drawbacks",
				"History Window (Months) for Exception Report of Unusual Unit Value",
				"The number of past months that will be searched to determine if a unit value is unusual and so reported on the Drawback Exception Report. The variance percentage is specified in the registry key 'Variance Percentage Used For Exception Report of Unusual Unit Value'",
				RegistryStorageFlags.Company,
				18);
		}

		public void TestVariancePercentageUsedForExceptionReportOfUnusualUnitValue()
		{
			TestRegistryItem(ItemSet.VariancePercentageUsedForExceptionReportOfUnusualUnitValue,
				"VariancePercentageUsedForExceptionReportOfUnusualUnitValue",
				"Customs/Drawbacks",
				"Variance Percentage Used For Exception Report of Unusual Unit Value",
				"The percentage used when checking if a unit value is unusual and so reported on the Drawback Exception Report. The History Window used for this calculation is specified in the registry key 'History Window (Months) for Exception Report of Unusual Unit Value'",
				RegistryStorageFlags.Company,
				20);
		}

		public void TestWarnUserWhenCommercialInvoiceHasBeenUsedBeforeForSigapore()
		{
			Guid departmentBRNPK = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "BRN");
			Guid singaporeBranchPK = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "SIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranchPK, departmentBRNPK))
			{
				TestRegistryItem(
					ItemSet.WarnUserWhenCommercialInvoiceHasBeenUsedBefore,
					"WarnUserWhenCommercialInvoiceHasBeenUsedBefore",
					ExpectedCategory,
					"Warn User When Commercial Invoice Has Been Used Before",
					"If this registry key is set to 'Yes' then a warning will be displayed, on the Invoice Header Tab of a declaration, if a Commercial Invoice Number is entered that has already been used on another declaration for this supplier.",
					RegistryStorageFlags.Company,
					false);
			}
		}

		public void TestWarnUserWhenCommercialInvoiceHasBeenUsedBeforeForOtherCountries()
		{
			TestRegistryItem(
				ItemSet.WarnUserWhenCommercialInvoiceHasBeenUsedBefore,
				"WarnUserWhenCommercialInvoiceHasBeenUsedBefore",
				ExpectedCategory,
				"Warn User When Commercial Invoice Has Been Used Before",
				"If this registry key is set to 'Yes' then a warning will be displayed, on the Invoice Header Tab of a declaration, if a Commercial Invoice Number is entered that has already been used on another declaration for this supplier.",
				RegistryStorageFlags.Company,
				true);
		}

		public void TestGroupToSendLateSeaCargoReportAndWarningsTo()
		{
			TestRegistryItem(ItemSet.GroupToSendLateSeaCargoReportAndWarningsTo,
				"GroupToSendLateSeaCargoReportAndWarningsTo",
				CustomsDataRegistry.Categories.Customs_LateCargoReport,
				"Group To Send Late Cargo Report and Warnings To for Sea Shipments",
				"The Staff Group who are to receive the Late Cargo Report Exceptions Report for Sea Shipments",
				RegistryStorageFlags.Company,
				RegistryOptions.IsValueMandatory,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestGroupToSendLateAirCargoReportAndWarningsTo()
		{
			TestRegistryItem(ItemSet.GroupToSendLateAirCargoReportAndWarningsTo,
				"GroupToSendLateAirCargoReportAndWarningsTo",
				CustomsDataRegistry.Categories.Customs_LateCargoReport,
				"Group To Send Late Cargo Report and Warnings To for Air Shipments",
				"The Staff Group who are to receive the Late Cargo Report Exceptions Report for Air Shipments",
				RegistryStorageFlags.Company,
				RegistryOptions.IsValueMandatory,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestPublishLateCargoMilestone()
		{
			TestRegistryItem(
				ItemSet.PublishLateCargoMilestone,
				"PublishLateCargoMilestone",
				CustomsDataRegistry.Categories.Customs_LateCargoReport,
				"Publish Late Cargo Milestone",
				"If you override this setting and set to NO then the Cargo Report Accepted Milestone that is automatically generated to support the Late and Pending Cargo report feature will not be published in Web Tracker. If you leave this item at its default setting then this Milestone and its completed date will be published in Web Tracker.",
				RegistryStorageFlags.Company,
				true);
		}

		public void TestLateAndPendingCargoReportBatchSize()
		{
			TestRegistryItem(
				ItemSet.LateAndPendingCargoReportBatchSize,
				"LateAndPendingCargoReportBatchSize",
				CustomsDataRegistry.Categories.Customs_LateCargoReport,
				"Batch Size for Late and Pending Cargo Report",
				"Specifies batch size when processing Late and Pending Cargo Report.\r\nIf the processing time is too long, increase the value of this setting.\r\nIf the process uses too much memory, reduce the value of this setting.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				expectedDefaultValue: 500, expectedMinValue: 1, expectedMaxValue: 5000
			);
		}

		public void TestAutoAllocateContainerToInvoiceLines()
		{
			TestRegistryItem(ItemSet.AutoAllocateContainerToInvoiceLines,
				"AutoAllocateContainerToInvoiceLines",
				CustomsDataRegistry.Categories.Customs,
				"Auto Allocate Container to Invoice Lines",
				"When set to 'Yes' a declaration that has a single container will see that container automatically allocated to its invoice lines (by placing a tick in the 'Is For Invoice' tick box).",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				true);
		}

		public void TestAutoAllocatePackageToInvoiceLines_AllThisTestDoesIsCheckMyAbilityToCopyAndPaste()
		{
			TestRegistryItem(ItemSet.AutoAllocatePackageToInvoiceLines,
				"AutoAllocatePackageToInvoiceLines",
				CustomsDataRegistry.Categories.Customs,
				"Auto Allocate Package to Invoice Lines",
				"When set to 'Yes' a declaration that has a single package will see that package automatically allocated to its invoice lines (by placing a tick in the 'Is For Invoice' tick box). Applies only to some countries/regions.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				true);
		}

		public void TestDefaultCurrencyToLocalCurrency()
		{
			TestGenericRegistryItem(ItemSet.DefaultCurrencyToLocalCurrency,
				"DefaultCurrencyToLocalCurrency",
				CustomsDataRegistry.Categories.Customs,
				"Default Currency To Local Currency",
				"Default currency to local currency if no other defaults are available?",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch);

			var companyUK = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			companyUK[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.UnitedKingdom;
			var companyUS = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			companyUS[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.UnitedStates;
			var companyNZ = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			companyNZ[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.NewZealand;
			var companySG = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompany)));
			companySG[GlbCompanySchema.GC_RN_NKCountryCode] = Core.Constants.CountryCodes.Singapore;
			Factory.Save();
			var registryItem = new DefaultCurrencyToLocalCurrencyRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company);
			Assert("GetDefaultValue for UK", !(bool)registryItem.GetDefaultValue(companyUK.PK.ToGuid(), Guid.Empty, Guid.Empty));
			Assert("GetDefaultValue for US", (bool)registryItem.GetDefaultValue(companyUS.PK.ToGuid(), Guid.Empty, Guid.Empty));
			Assert("GetDefaultValue for NZ", (bool)registryItem.GetDefaultValue(companyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty));
			Assert("GetDefaultValue for SG", (bool)registryItem.GetDefaultValue(companySG.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestRelatedPartyDefaultingForwarder()
		{
			TestRegistryItem(ItemSet.RelatedPartyDefaultingForwarder,
						"RelatedPartyDefaultingForwarder",
						CustomsDataRegistry.Categories.Customs,
						"Enable Related Party defaulting for Forwarder (Receiving Agents)",
						"Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						new RelatedPartyDefaultingTypeList(),
						RelatedPartyDefaultingTypeList.Codes.Disabled);
		}

		public void TestRelatedPartyDefaultingCTO()
		{
			TestRegistryItem(ItemSet.RelatedPartyDefaultingCTO,
						"RelatedPartyDefaultingCTO",
						CustomsDataRegistry.Categories.Customs,
						"Enable Related Party defaulting for CTO",
						"Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						new RelatedPartyDefaultingTypeList(),
						RelatedPartyDefaultingTypeList.Codes.Disabled);
		}

		public void TestRelatedPartyDefaultingDepot()
		{
			TestRegistryItem(ItemSet.RelatedPartyDefaultingDepot,
						"RelatedPartyDefaultingDepot",
						CustomsDataRegistry.Categories.Customs,
						"Enable Related Party defaulting for Depot (CFS)",
						"Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						new RelatedPartyDefaultingTypeList(),
						RelatedPartyDefaultingTypeList.Codes.Disabled);
		}

		public void TestRelatedPartyDefaultingCarrier()
		{
			TestRegistryItem(ItemSet.RelatedPartyDefaultingCarrier,
						"RelatedPartyDefaultingCarrier",
						CustomsDataRegistry.Categories.Customs,
						"Enable Related Party defaulting for Carrier",
						"Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						new RelatedPartyDefaultingTypeList(),
						RelatedPartyDefaultingTypeList.Codes.Disabled);
		}

		public void TestRelatedPartyDefaultingContainerYard()
		{
			TestRegistryItem(ItemSet.RelatedPartyDefaultingContainerYard,
						"RelatedPartyDefaultingContainerYard",
						CustomsDataRegistry.Categories.Customs,
						"Enable Related Party defaulting for Container Yard",
						"Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						new RelatedPartyDefaultingTypeList(),
						RelatedPartyDefaultingTypeList.Codes.Disabled);
		}

		public void TestRaiseEventDCEWhenSavingErrorMSG()
		{
			TestRegistryItem(ItemSet.RaiseEventDCEWhenSavingErrorMSG,
						"RaiseEventDCEWhenSavingErrorMSG",
						CustomsDataRegistry.Categories.Customs,
						"Raise Event DCE when saving a declaration with message errors",
						"If set to 'Yes', an DCE event will be added when saving a declaration with message errors.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
		}

		public void TestRelatedPartyDefaultingBondedWarehouse()
		{
			TestRegistryItem(ItemSet.RelatedPartyDefaultingBondedWarehouse,
						"RelatedPartyDefaultingBondedWarehouse",
						CustomsDataRegistry.Categories.Customs,
						"Enable Related Party defaulting for Bonded Warehouse",
						"Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						new RelatedPartyDefaultingTypeList(),
						RelatedPartyDefaultingTypeList.Codes.Disabled);
		}

		public void TestRelatedPartyDefaultingExternalBroker()
		{
			TestRegistryItem(ItemSet.RelatedPartyDefaultingExternalBroker,
						"RelatedPartyDefaultingExternalBroker",
						CustomsDataRegistry.Categories.Customs,
						"Enable Related Party defaulting for External Broker",
						"Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						new RelatedPartyDefaultingTypeList(),
						RelatedPartyDefaultingTypeList.Codes.Disabled);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestDeclarationDescriptionCustomization()
		{
			var importitem = ItemSet.DeclarationImportDescriptionCustomization;
			AssertEquals("Name", "DeclarationImportDescriptionCustomization", importitem.Name);
			AssertEquals("Category", CustomsDataRegistry.Categories.Customs, importitem.Category);
			AssertEquals("Caption", "Declaration (Import) Description Customization", importitem.Caption);
			AssertEquals("Hint", $"Select one or more of the following check boxes to define what reference information will be used to describe an import declaration when it appears in the Favorites or Recent Items areas on the main {Core.Constants.ProductName} form and the Recent Items on the Customs Declaration module.", importitem.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, importitem.Storage);
			AssertEquals("Bool Caption", "Select the data you wish to use to describe an import declaration", ((CodeDescriptionBoolRegistryEditorInfo)importitem.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 7, importitem.DefaultValue.Count);

			AssertCodeDescriptionBool("JNO", "Job Number", true, importitem.DefaultValue[0]);
			AssertCodeDescriptionBool("MBL", "Master Bill", true, importitem.DefaultValue[1]);
			AssertCodeDescriptionBool("HBL", "House Bill", true, importitem.DefaultValue[2]);
			AssertCodeDescriptionBool("ENT", "Entry Number", false, importitem.DefaultValue[3]);
			AssertCodeDescriptionBool("IMP", "Importer Code", false, importitem.DefaultValue[4]);
			AssertCodeDescriptionBool("EXP", "Exporter Code", false, importitem.DefaultValue[5]);
			AssertCodeDescriptionBool("ORF", "Other Reference", false, importitem.DefaultValue[6]);

			var exportitem = ItemSet.DeclarationExportDescriptionCustomization;
			AssertEquals("Name", "DeclarationExportDescriptionCustomization", exportitem.Name);
			AssertEquals("Category", CustomsDataRegistry.Categories.Customs, exportitem.Category);
			AssertEquals("Caption", "Declaration (Export) Description Customization", exportitem.Caption);
			AssertEquals("Hint", $"Select one or more of the following check boxes to define what reference information will be used to describe an export declaration when it appears in the Favorites or Recent Items areas on the main {Core.Constants.ProductName} form and the Recent Items on the Customs Declaration module.", exportitem.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, exportitem.Storage);
			AssertEquals("Bool Caption", "Select the data you wish to use to describe an export declaration", ((CodeDescriptionBoolRegistryEditorInfo)exportitem.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 7, exportitem.DefaultValue.Count);

			AssertCodeDescriptionBool("JNO", "Job Number", true, exportitem.DefaultValue[0]);
			AssertCodeDescriptionBool("MBL", "Master Bill", true, exportitem.DefaultValue[1]);
			AssertCodeDescriptionBool("HBL", "House Bill", true, exportitem.DefaultValue[2]);
			AssertCodeDescriptionBool("ENT", "Entry Number", false, exportitem.DefaultValue[3]);
			AssertCodeDescriptionBool("IMP", "Importer Code", false, exportitem.DefaultValue[4]);
			AssertCodeDescriptionBool("EXP", "Exporter Code", false, exportitem.DefaultValue[5]);
			AssertCodeDescriptionBool("ORF", "Other Reference", false, exportitem.DefaultValue[6]);
		}

		void AssertCodeDescriptionBool(string code, string description, bool sysdefined, CodeDescriptionBool codeDescriptionBool)
		{
			AssertEquals("Code", code, codeDescriptionBool.Code);
			AssertEquals("Description", description, codeDescriptionBool.Description);
			AssertEquals("Bool", sysdefined, codeDescriptionBool.Bool);
		}

		public void TestSeverityLevelOfTotalWeightValidation()
		{
			TestRegistryItem(ItemSet.SeverityLevelOfTotalWeightValidation,
				"SeverityLevelOfTotalWeightValidation",
				Categories.Customs,
				"Severity Level Of Total Weight Validation",
				"Set the severity level for validation relating to Total Weight in Declaration. Indicate the system behavior of the validation between the total weight of the invoices lines and the value declared in the declaration. Not used in NZ.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new ProductAuditActions(),
				ProductAuditActions.Codes.AddWarningValidation);

			var countryFilterPks = ItemSet.SeverityLevelOfTotalWeightValidation.CountryFilterPKs;
			AssertCollectionNotContains("New Zealand is excluded", Core.Constants.CountryGuids.NewZealand, countryFilterPks);
		}

		public void TestSeverityLevelOfInvoiceLineValidationAgainstProductData()
		{
			TestRegistryItem(ItemSet.SeverityLevelOfInvoiceLineValidationAgainstProductData,
				"SeverityLevelOfInvoiceLineValidationAgainstProductData",
				Categories.Customs,
				"Severity Level for validating invoice line data against existing product code data",
				"Set the severity level for validating invoice line data against existing product code data. When an existing product code is used on an invoice line, the system will validate the data on the invoice line against existing data on the product code to confirm that the data matches.",
				RegistryStorageFlags.Company,
				new ProductAuditActions(),
				ProductAuditActions.Codes.AddWarningValidation);
		}

		public void TestEnablePromptToCreateProducts()
		{
			TestRegistryItem(ItemSet.EnablePromptToCreateProducts,
				"EnablePromptToCreateProducts",
				CustomsDataRegistry.Categories.Customs,
				"Enable Prompt to Create Products",
				"If set to 'yes', the user will see the prompt to create products when saving customs declaration.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				true);
		}

		public void TestEnableNewStandaloneInvoiceData()
		{
			TestRegistryItem(ItemSet.EnableNewStandaloneInvoiceData,
				"EnableNewStandaloneInvoiceData",
				Categories.Customs,
				"Enable New Standalone Invoice Data",
				"If set to 'yes', standalone commercial invoice should not create fake Declaration.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestEnableInheritanceOfLinkedDeclarationsJobHeader()
		{
			TestRegistryItem(ItemSet.EnableInheritanceOfLinkedDeclarationsJobHeader,
				"EnableInheritanceOfLinkedDeclarationsJobHeader",
				Categories.Customs,
				"Enable ParentJob inheritance for Job Header of linked declaration",
				"If set to 'yes', Job Header JH_JH_ParentJob should take value from ultimate parent Declaration's Job Header.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestASNRefreshOptionsFieldTypes()
		{
			var registryOptions = new ASNRefreshOptionsConfigCollection()
			{
				new ASNRefreshOptionsConfig() { FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Preference },
				new ASNRefreshOptionsConfig() { FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification }
			};

			var expectedASNRefreshOptionsFieldTypes = new ZString[] { Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Preference, Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification };
			var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();

			using (Instance.ASNRefreshOptions.SetTemporaryValue(currentCompanyPk, Guid.Empty, Guid.Empty, registryOptions))
			{
				AssertArrayEqualsByElements(expectedASNRefreshOptionsFieldTypes, Instance.ASNRefreshOptionsFieldTypes(currentCompanyPk).ToArray());
			}
		}
	}
}
