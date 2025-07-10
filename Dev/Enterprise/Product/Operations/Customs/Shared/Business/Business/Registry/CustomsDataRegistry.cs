using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.Customs.Business.ResString;

namespace Enterprise.Customs.DataRegistry.Business
{
	public sealed class CustomsDataRegistry : RegistryItemSet, Integration.Customs.Shared.ICustomsDataRegistry
	{
		#region Construction
		public static CustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new CustomsDataRegistry()); }
		}
		[ThreadStatic]
		static CustomsDataRegistry instance;

		CustomsDataRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_3rdPartyInterfaces => CombineCategories(Customs, ResString.GetMultilingualString("71B6DA99-4203-4452-B97D-5CC8562A3EF0", "3rd Party Interfaces"));
			public static MultilingualString Customs_AuditActions => CombineCategories(Customs, ResString.GetMultilingualString("8C6D656E-9664-431B-A48D-4BA351FC49C4", "Audit Actions"));
			public static MultilingualString Customs_Drawbacks => CombineCategories(Customs, ResString.GetMultilingualString("EA3AB766-B86D-4dfc-A696-121D49E615A5", "Drawbacks"));
			public static MultilingualString Customs_Integration_CustomsWare => CombineCategories(Customs_Integration, ResString.GetMultilingualString("f630c5d2-f24b-447c-af4c-7e1e7d758d25", "ABM"));
			public static MultilingualString Customs_Integration_CustomsWare_WebService => CombineCategories(Customs_Integration_CustomsWare, ResString.GetMultilingualString("c745084f-f2fc-4ca4-82bf-2e0811eb98db", "Web Service"));
			public static MultilingualString Customs_LateCargoReport => CombineCategories(Customs_Australia, ResString.GetMultilingualString("e037b62f-a4ee-4514-aa05-1d62f024d45d", "Late Cargo Report"));
			public static MultilingualString System_DataImportSettings_CustomsDeclarations => Enterprise.Registry.Business.SystemDataRegistry.Categories.System_DataImportSettings_CustomsDeclarations;
			public static MultilingualString System_DataExportSettings_CustomsDeclarations => CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("52d2da7b-0d11-4e9e-bf4d-f9effd500f62", "Customs Declarations"));
		}

		#endregion

		#region Integration

		#region Enable Accounting Integration

		public AccountingIntegrationOptionsRegistryItem EnableAccountingIntegration
		{
			get
			{
				return GetItem("EnableAccountingIntegration", delegate
				{
					return new AccountingIntegrationOptionsRegistryItem(
						"EnableAccountingIntegration",
						Categories.Customs_Integration,
						ResString.GetMultilingualString("560688aa-f020-46c9-b54c-1d9a5b49b7g0", "Enable Auto-Billing"),
						ResString.GetMultilingualString("18F7078D-AD93-48C8-8C85-FB668ADE843B", @"This function is available for AU, CA, DE, ES, FR, NZ, UK, US and ZA companies.
The options below govern how the system reacts on receipt of successful entries/declarations.
The ‘Post AR’ and ‘Post AP’ relate to the posting of Customs Disbursement charges.
If ticked, when a successful declaration message is received from Customs the customs disbursement charges and costs will be automatically posted.

US and CA companies also have an additional option, ‘Post AR & AP if job is approved’.
When ticked an additional option ‘Billing Job Ready for Posting’ is displayed when sending messages to Customs.
If Billing Job is indicated as ready for posting, non-disbursement charges will be posted.
If Billing Job is not indicated as ready for posting, no charges will be posted."),
						RegistryStorageFlags.Company)
					{
						CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia
							.Concat(RegistryItemSet.CountryFilterPKs.Canada)
							.Concat(RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments)
							.Concat(RegistryItemSet.CountryFilterPKs.NewZealand)
							.Concat(RegistryItemSet.CountryFilterPKs.Spain)
							.Concat(RegistryItemSet.CountryFilterPKs.UnitedKingdom)
							.Concat(Core.CountryGuids.CountriesUnderUSCustomsJurisdiction)
							.Concat(RegistryItemSet.CountryFilterPKs.SouthAfrica)
							.Concat(RegistryItemSet.CountryFilterPKs.Germany)
					};
				});
			}
		}

		public BooleanRegistryItem AutoBillingDueDateFromPaymentTerms
		{
			get
			{
				return GetItem("AutoBillingDueDateFromPaymentTerms", delegate
				{
					var result = new BooleanRegistryItem(
						"AutoBillingDueDateFromPaymentTerms",
						Categories.Customs_Integration,
						ResString.GetMultilingualString("12CDBE36-03AC-4FF3-A980-0AB6B2C16221", "Use Payment Terms to calculate Due Date"),
						ResString.GetMultilingualString("C5359EAD-6AAB-420C-9629-3DE093CEDA41", "This option allows to calculate the Due Date based on the Creditor’s Payment Terms when customs disbursements are populated after Auto Billing process."),
						RegistryStorageFlags.Company,
						false);
					return result;
				});
			}
		}

		public AutoBillingGroupNotificationRegistryItem AutoBillingEmailNotificationGroup
		{
			get
			{
				return GetItem("AutoBillingEmailNotificationGroup", delegate
				{
					var result = new AutoBillingGroupNotificationRegistryItem(
						"AutoBillingEmailNotificationGroup",
						Categories.Customs_Integration,
						ResString.GetMultilingualString("f800afae-85df-4484-82f1-f79569ce8a9c", "Auto-Billing Email Notification Group"),
						ResString.GetMultilingualString("b30aa243-7b14-4043-be7a-5c3c46a2bb08", "If there is abnormality that needs to be notified while auto-billing, an email will be sent to this group."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					return result;
				});
			}
		}

		public RoutingIntegrationOptionsRegistryItem RoutingIntegrationOptions
		{
			get
			{
				return GetItem("RoutingIntegrationOptions", delegate
				{
					var defaultValue = new RoutingIntegrationOptions();
					defaultValue.AlwaysLink = true;

					return new RoutingIntegrationOptionsRegistryItem(
						"RoutingIntegrationOptions",
						Categories.Customs_Integration,
						ResString.GetMultilingualString("60ca65cd-dd4f-45ea-9e55-235ee71aade0", "Routing Integration Options"),
						ResString.GetMultilingualString("23257f8a-95e8-4d91-a0da-7c60018bfc4h", @"This controls when system should create and link routing records for stand-alone Customs Declaration jobs. Key fields are defined as follows.

	SEA jobs : Vessel, Voyage and Carrier
	AIR jobs : Flight No and Departure Date
	RAIL jobs : Journey and Trip ID
	ROAD jobs : Reg. Number and Departure Date"),

						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						defaultValue);
				});
			}
		}

		#endregion

		public IntRegistryItem WebServiceTimeoutInSeconds
		{
			get
			{
				return GetItem("WebServiceTimeout", delegate
				{
					return new IntRegistryItem(
						"WebServiceTimeout",
						Categories.Customs_Integration,
						ResString.GetMultilingualString("5E33BF6B-2857-4173-A326-715B07CE480E", "Web Service Timeout"),
						ResString.GetMultilingualString("359F4504-7538-4AD1-97A9-E89D523B5AD1", "Web Service Timeout in Seconds"),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						60, 60, 300);
				});
			}
		}

		public LocalCountryCustomsInterfaceRegistryItem LocalCountryCustomsInterface
		{
			get
			{
				return GetItem("LocalCountryCustomsInterface", delegate
				{
					return new LocalCountryCustomsInterfaceRegistryItem(
						"LocalCountryCustomsInterface",
						Categories.Customs_Integration,
						ResString.GetMultilingualString("6B72E92C-E4C1-482F-8AD2-728020D4C973", "Local Country Customs Interface"),
						ResString.GetMultilingualString("E4181B49-EF59-48B8-93AF-EA464D5E8E18", "The fields below are used to setup interfacing into CW1 for a local country. The ID should be supplied to you by the provider of your local customs software. The \"Default Submission\" will control if entries can be submitted to Customs directly by CW1 or will be submitted via external Customs systems."),
						RegistryStorageFlags.Company);
				});
			}
		}

		ZString Integration.Customs.Shared.ICustomsDataRegistry.LocalCountryCustomsInterfaceSubmissionType
		{
			get => LocalCountryCustomsInterface.Value.SubmissionType;
#if DEBUG
			set
			{
				var localCountryCustomsInferface = new LocalCountryCustomsInterface();
				localCountryCustomsInferface.SubmissionType = value;
				LocalCountryCustomsInterface.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, localCountryCustomsInferface);
			}
#endif
		}

		#region ImportExportBilling

		public BooleanRegistryItem AllowBillingImportIntoDeclaration
		{
			get
			{
				return GetItem("AllowBillingImportIntoDeclaration", delegate
				{
					return new BooleanRegistryItem(
						"AllowBillingImportIntoDeclaration",
						Categories.System_DataImportSettings_CustomsDeclarations,
						ResString.GetMultilingualString("e5681978-8297-4604-8472-74a4fb9f1de9", "Import Billing Info From XML File"),
						ResString.GetMultilingualString("d5ad7345-c083-4069-bcf3-bdff37f39a98", "Enable this option to allow billing information to be imported into the customs declaration."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem IncludeBillingInformationInXMLFile
		{
			get
			{
				return GetItem("IncludeBillingInformationInXMLFile", delegate
				{
					return new BooleanRegistryItem(
						"IncludeBillingInformationInXMLFile",
						Categories.System_DataExportSettings_CustomsDeclarations,
						ResString.GetMultilingualString("f0bb5f4f-c5e8-4036-8ad8-54d679d51007", "Include Billing Information in XML File"),
						ResString.GetMultilingualString("352767ea-5f63-4c79-b0ee-96a4faffada0", "Enable this option to include billing information to export customs declaration XML file."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem IncludeARInvoicesInXMLFile
		{
			get
			{
				return GetItem("IncludeARInvoicesInXMLFile", delegate
				{
					return new BooleanRegistryItem(
						"IncludeARInvoicesInXMLFile",
						Categories.System_DataExportSettings_CustomsDeclarations,
						ExportARInvoicesCaption,
						ExportARInvoicesHint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}
		public static MultilingualString ExportARInvoicesCaption
		{
			get { return ResString.GetMultilingualString("f84169a1-3acc-49e6-9f3e-d503845335c6", "Include AR Invoices in XML File"); }
		}

		public static MultilingualString ExportARInvoicesHint
		{
			get { return ResString.GetMultilingualString("f613520e-f94a-481a-ace4-dc33709c149a", "Enable this option to include AR Invoices to export customs declaration XML file."); }
		}

		#endregion

		#region Web Service

		#region CustomWare

		public LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem CustomsWareCompany
		{
			get
			{
				return GetItem("CustomsWareCompany", delegate
				{
					LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem result = new LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem(
						"CustomsWareCompany",
						Categories.Customs_Integration_CustomsWare_WebService,
						ResString.GetMultilingualString("eaceb08a-9a62-47c2-8a98-50b7e22526a2", "Company"),
						ResString.GetMultilingualString("eaceb08a-9a62-47c2-8a98-50b7e22526a2", "Company"),
						RegistryStorageFlags.Company);

					return result;
				});
			}
		}

		#endregion

		#endregion

		public BooleanRegistryItem CreditCheckOnMessageSend
		{
			get
			{
				return GetItem("CreditCheckOnCustomsMessageSend", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"CreditCheckOnCustomsMessageSend",
						Categories.Customs,
						ResString.GetMultilingualString("f7f16872-a89f-4480-9881-bfda1f865833", "Check the credit limit and on hold status of related parties when sending messages"),
						ResString.GetMultilingualString("21f0847e-9b08-4f0d-a16c-efd480f116de", "Do you wish to check the credit limit and on hold status when sending messages? Messages may only be sent when the parties have sufficient credit."),
						RegistryStorageFlags.Company,
						true);
					result.EditorInfo = new BooleanRegistryEditorInfo();
					result.CountryFilterPKs = CreditCheckCountries;
					return result;
				});
			}
		}

		public static bool IsWHSUniversalXMLActive(ZDateTime registrationDate)
		{
			var date = Instance.WHSUniversalXMLChangeDate.Value;
			var result = date != DateTime.MinValue;
			if (result && registrationDate.IsValid)
			{
				result = registrationDate.ToDateTime() >= date;
			}
			return result;
		}

		public DateTimeRegistryItem WHSUniversalXMLChangeDate
		{
			get
			{
				return GetItem("DecWHSUniversalXMLChangeDate", delegate
				{
					return new DateTimeRegistryItem(
						"DecWHSUniversalXMLChangeDate",
						Categories.Customs,
						(NoResString)"Universal XML Integration Effective Date",
						(NoResString)"The date when the WHS Universal XML Integration is effective.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		public static IEnumerable<Guid> CreditCheckCountries // add more countries here as you implement them
		{
			get
			{
				return creditCheckCountries ?? (creditCheckCountries = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction.
					Concat(RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments).
					Concat([
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
					]));
			}
		}
		[ThreadStatic]
		static IEnumerable<Guid> creditCheckCountries;

		public BooleanRegistryItem EnableByProductFunctionality
		{
			get
			{
				return GetItem("EnableByProductFunctionality", delegate
				{
					return new BooleanRegistryItem(
						"EnableByProductFunctionality",
						Categories.Customs,
						ResString.GetMultilingualString("FE4F7666-E368-4472-AA12-6894CDA9A1F7", "Enable By Product Functionality"),
						ResString.GetMultilingualString("3E50CADA-B60B-4BC6-B1AB-67E354991FEF", "Enable this option to show By Product Functionality in supported countries."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		public BooleanRegistryItem EnableUCMTest
		{
			get
			{
				return GetItem("EnableUCMTest", delegate
				{
					return new BooleanRegistryItem(
						"EnableUCMTest",
						Categories.Customs,
						(NoResString)"Enable UCM Tests",
						(NoResString)"Enable Universal Customs Messaging testing using Application Codes define in 'UCM Test Application Codes'",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public IntRegistryItem UCMResourcePercentageUsage
		{
			get
			{
				return GetItem("UCMResourcePercentageUsage", delegate
				{
					return new IntRegistryItem(
						"UCMResourcePercentageUsage",
						Categories.Customs,
						(NoResString)"UCK/UCQ Resource Percentage",
						(NoResString)"A percentage of resources one application code can use to run UCK or UCQ service task",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						80);
				});
			}
		}

		public UCMEDIMessageTestTypeRegistryItem UCMTestApplicationCodes
		{
			get
			{
				return GetItem("UCMTestApplicationCodes", delegate
				{
					return new UCMEDIMessageTestTypeRegistryItem(
						"UCMTestApplicationCodes",
						Categories.Customs,
						(NoResString)"UCM Test Application Codes",
						(NoResString)"List of application codes to use in UCM testing.");
				});
			}
		}

		public IntRegistryItem UCKPreEnqueuerMaxBacklogSize
		{
			get
			{
				return GetItem("UCKPreEnqueuerMaxBacklogSize", delegate
				{
					return new IntRegistryItem(
						"UCKPreEnqueuerMaxBacklogSize",
						Categories.Customs,
						ResString.GetMultilingualString("C95D2DA3-7E0C-46AB-A5D4-3019B5B2E4AE", "{0} - Max Backlog Size", UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen),
						ResString.GetMultilingualString("93F7FB3A-139A-4205-B4DC-BCA6668CA375", "The maximum number of {0} rows added by the {1} service task before it stops calculating keys. If this capacity is reached and you have a large number of {1} service task runners, this value can be increased to ensure there are enough messages ready for {1} to process. This should be increased incrementally to achieve balance between keeping messages queued for {1} and managing a larger {0} table.", EDIMessageQueueStateSchema.Constants.TableName, UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 150000);
				});
			}
		}

		public IntRegistryItem UCKPreEnqueuerIncreasePerformanceLevelDurationInMillis
		{
			get
			{
				return GetItem("UCKPreEnqueuerIncreasePerformanceLevelDurationInMillis", delegate
				{
					return new IntRegistryItem(
						"UCKPreEnqueuerIncreasePerformanceLevelDurationInMillis",
						Categories.Customs,
						(NoResString)"UCK - Performance level increase duration",
						(NoResString)"How long getting a batch should take before we think it is broken.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						defaultValue: (int)TimeSpan.FromSeconds(20).TotalMilliseconds);
				});
			}
		}

		public IntRegistryItem UCKPreEnqueuerResetPerformanceLevelDurationInMillis
		{
			get
			{
				return GetItem("UCKPreEnqueuerResetPerformanceLevelDurationInMillis", delegate
				{
					return new IntRegistryItem(
						"UCKPreEnqueuerResetPerformanceLevelDurationInMillis",
						Categories.Customs,
						(NoResString)"UCK - Performance level increase duration",
						(NoResString)"How long getting a batch ought to take.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						defaultValue: (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
				});
			}
		}

		public IntRegistryItem UCKMessagesPerBatch
		{
			get
			{
				return GetItem("UCKMessagesPerBatch", delegate
				{
					return new IntRegistryItem(
						"UCKMessagesPerBatch",
						Categories.Customs,
						(NoResString)"UCK - Messages Per Batch",
						(NoResString)"The number of messages in each batch (per save) of UCK services task.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						50);
				});
			}
		}

		public IntRegistryItem UCKMessagesPerExecution
		{
			get
			{
				return GetItem("UCKMessagesPerExecution", delegate
				{
					return new IntRegistryItem(
						"UCKMessagesPerExecution",
						Categories.Customs,
						(NoResString)"UCK - Messages Per Execution",
						(NoResString)"The number of messages in each execution of the UCK service task per application code before it move on to the next application code.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						10000);
				});
			}
		}

		public IntRegistryItem UCQMessagesPerExecution
		{
			get
			{
				return GetItem("UCQMessagesPerExecution", delegate
				{
					return new IntRegistryItem(
						"UCQMessagesPerExecution",
						Categories.Customs,
						(NoResString)"UCQ - Messages Per Execution",
						(NoResString)"The number of messages in each execution of the UCQ service task per application code before it move on to the next application code.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						10000);
				});
			}
		}

		public IntRegistryItem ParallelUCMQueueHistoryInHours
		{
			get
			{
				return GetItem("ParallelUCMQueueHistoryInHours", delegate
				{
					return new IntRegistryItem(
						"ParallelUCMQueueHistoryInHours",
						Categories.Customs,
						(NoResString)"UCI - Parallel Queue History in Hours",
						(NoResString)"How many hours should EDIMessageQueueState rows with the status PRS be persisted.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 0);
				});
			}
		}

		public IntRegistryItem UCIMessagesPerBatch
		{
			get
			{
				return GetItem("UCIMessagesPerBatch", delegate
				{
					return new IntRegistryItem(
						"UCIMessagesPerBatch",
						Categories.Customs,
						(NoResString)"UCI - Messages Per Batch",
						(NoResString)"The number of messages in each batch (per save) of UCI services task.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						10000);
				});
			}
		}

		public IntRegistryItem UCIMessagesPerExecution
		{
			get
			{
				return GetItem("UCIMessagesPerExecution", delegate
				{
					return new IntRegistryItem(
						"UCIMessagesPerExecution",
						Categories.Customs,
						(NoResString)"UCI - Messages Per Execution",
						(NoResString)"The number of messages in each execution of the UCI service task per application code before it move on to the next application code.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						10000);
				});
			}
		}

		public IntRegistryItem UCMMessageQueueCapacity
		{
			get
			{
				return GetItem("UCMMessageQueueCapacity", delegate
				{
					return new IntRegistryItem(
						"UCMMessageQueueCapacity",
						Categories.Customs,
						ResString.GetMultilingualString("ADE62686-B927-4EB0-A7B9-6E2D84D54531", "Universal Customs Message Queue Max Backlog Size"),
						ResString.GetMultilingualString("878C4820-CBF7-4353-9583-38148C35359D", "The number of messages that can be queued in the {0} table. If you have a large number of {1} or {2} service task runners and message back logs, this can be increased to ensure each {1} or {2} runner has messages to process. This should be increased incrementally to achieve balance between keeping messages queued for {1} or {2} and managing a larger {0} table.", EDIMessageQueueStateSchema.Constants.TableName, UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen, UniversalCustomsMessagingConstants.ServiceTaskCodes.Master),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						100000);
				});
			}
		}

		public BooleanRegistryItem UCMSetChainIDLogging
		{
			get
			{
				return GetItem("UCMSetChainIDLogging", delegate
				{
					return new BooleanRegistryItem(
						"UCMSetChainIDLogging",
						Categories.Customs,
						(NoResString)"UCMP Log Chain ID settng",
						(NoResString)"Log when a Chain ID is set in UCMP service tasks",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem UCMExtendedUniversalLogging
		{
			get
			{
				return GetItem("UCMExtendedUniversalLogging", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem("UCMExtendedUniversalLogging",
						Categories.Customs,
						ResString.GetMultilingualString("E2E25B7F-D0E8-4840-A045-CBAF51448481", "Universal Customs Message Extended Logging Options"),
						ResString.GetMultilingualString("A5051E26-6689-41EE-A213-953D8B9002B9", "Additional log toggles for monitoring in the {0}, {1} and {2} service tasks.", UniversalCustomsMessagingConstants.ServiceTaskCodes.Master, UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker, UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("8BB976DF-AEA0-4099-B988-0E77BD1DB497", "Enable Logs"), true, true),
						new CodeDescriptionBoolDisallowNewCollection
						{
							{ ExtendedUniversalLoggingKeys.UCIFirstMessageLoad, ResString.GetMultilingualString("20BE98E0-FC9B-4E21-A7FC-2D12AC2AB7F9", "{0} - First message load", UniversalCustomsMessagingConstants.ServiceTaskCodes.Master), false },
							{ ExtendedUniversalLoggingKeys.UCIMessageAtFront, ResString.GetMultilingualString("667D29D8-BC50-4835-9AE6-EA48C0F8247D", "{0} - Messages at front of queue", UniversalCustomsMessagingConstants.ServiceTaskCodes.Master), false },
							{ ExtendedUniversalLoggingKeys.UCIChainStatistics, ResString.GetMultilingualString("30C6D338-77E6-41CC-AECA-DAF7EA9B7732", "{0} - Chain statistics", UniversalCustomsMessagingConstants.ServiceTaskCodes.Master), false },
							{ ExtendedUniversalLoggingKeys.UCIShowOldest, ResString.GetMultilingualString("7ADDE705-9CB8-4505-B8E1-5296E436133E", "{0} - Show oldest message", UniversalCustomsMessagingConstants.ServiceTaskCodes.Master), false },
							{ ExtendedUniversalLoggingKeys.UCIAllLoads, ResString.GetMultilingualString("99CA3276-D966-4CF5-9C61-F2A72AFA2BAC", "{0} - All message loads", UniversalCustomsMessagingConstants.ServiceTaskCodes.Master), false },
							{ ExtendedUniversalLoggingKeys.UCKAllLoads, ResString.GetMultilingualString("1FD3CA74-B835-4748-8DB1-2A3CB0F70B16", "{0} - All message loads", UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen), false },
							{ ExtendedUniversalLoggingKeys.UCKLocksTaken, ResString.GetMultilingualString("F0D256BD-8281-4A52-A373-362775A6E9A5", "{0} - Locks taken", UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen), false },
							{ ExtendedUniversalLoggingKeys.UCQAllLoads, ResString.GetMultilingualString("284AB393-8BF8-46A4-8230-C79D14565D41", "{0} - All message loads", UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker), false },
							{ ExtendedUniversalLoggingKeys.UCQLocksTaken, ResString.GetMultilingualString("6A7D1E87-4F55-4C61-9431-F193B13CABC9", "{0} - Locks taken", UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker), false },
						});
				});
			}
		}

		public static class ExtendedUniversalLoggingKeys
		{
			public const string UCIFirstMessageLoad = "IFM";
			public const string UCIMessageAtFront = "IMF";
			public const string UCIChainStatistics = "ICS";
			public const string UCIShowOldest = "ISO";
			public const string UCIAllLoads = "IAL";
			public const string UCKAllLoads = "KAL";
			public const string UCKLocksTaken = "KLT";
			public const string UCQAllLoads = "QAL";
			public const string UCQLocksTaken = "QLT";
		}

		public bool IsAUSeaCargoHouseEnabled =>
			ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.SeaCargoHouse, Core.Constants.CountryCodes.Australia, ZDateTime.Now) ||
			ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.PilotSeaCargoHouse, Core.Constants.CountryCodes.Australia, ZDateTime.Now, true);

		public BooleanRegistryItem EnableWarehouseInventory
		{
			get
			{
				return GetItem("EnableWarehouseInventory", delegate
				{
					return new BooleanRegistryItem(
						"EnableWarehouseInventory",
						Categories.Customs,
						(NoResString)"Enable Warehouse Inventory",
						(NoResString)"Enable Warehouse Inventory Functionality",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableInwardProcessing
		{
			get
			{
				return GetItem("EnableInwardProcessing", delegate
				{
					return new BooleanRegistryItem(
						"EnableInwardProcessing",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_InwardProcessing,
						ResString.GetMultilingualString("0A7E1E94-6C77-429E-BAC4-89D6A34BD809", "Enable Inward Processing Function"),
						ResString.GetMultilingualString("24734E64-E28A-4BF1-8725-42FE7895F96D", "When set to 'Yes', Inward Processing related functionalities will be enabled."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem SupportWarehouseOrderLines
		{
			get
			{
				return GetItem("SupportWarehouseOrderLines", delegate
				{
					return new BooleanRegistryItem(
						"SupportWarehouseOrderLines",
						RawDataRegistry.Categories.Customs,
						ResString.GetMultilingualString("7E3A7701-2820-4632-9020-6F9BA0E6B9E8", "Support Warehouse Order Lines"),
						ResString.GetMultilingualString("1DBEA1E9-483D-469F-8C1D-E71C58610F05", "Enable Support selection of warehouse order lines in invoice lines."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem CopyPreviousLineCustomsProcedureCode
		{
			get
			{
				return GetItem("CopyPreviousLineCustomsProcedureCode", delegate
				{
					return new BooleanRegistryItem(
						"CopyPreviousLineCustomsProcedureCode",
						RawDataRegistry.Categories.Customs,
						(NoResString)"Copy previous invoice line's customs procedure code",
						(NoResString)"Enable functionality to copy previous invoice line's customs procedure code",
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter,
						true
					);
				});
			}
		}

		public BooleanRegistryItem CopyCustomFieldsFromInvoiceLineToProduct
		{
			get
			{
				return GetItem("CopyCustomFieldsFromInvoiceLineToProduct", delegate
				{
					return new BooleanRegistryItem(
						"CopyCustomFieldsFromInvoiceLineToProduct",
						RawDataRegistry.Categories.Customs,
						ResString.GetMultilingualString("F52DA98B-5214-455C-825D-339C59A67F66", "Always copy matched customized field values from invoice line to product"),
						ResString.GetMultilingualString("8B7AA6EB-375B-424E-84C0-7971B1469503", "When this registry is set to “Yes”, matched customized field values will be copied from invoice line when a new product is created."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true);
				});
			}
		}

		public CodePairRegistryItem InvoiceChargesForExport
		{
			get
			{
				return GetItem("InvoiceChargesForExport", delegate
				{
					return new CodePairRegistryItem(
						"InvoiceChargesForExport",
						Categories.Customs,
						ResString.GetMultilingualString("A71B4C6D-A30E-461D-8B5F-526638130BEF", "Invoice Charges Export Editor"),
						ResString.GetMultilingualString("F4C09D1D-AB1B-4307-8147-A0752BBA2942", "Change default value of the Invoice Charges “Distribute By”-value in Export Declarations."),
						new CodeDescriptionPairListProvider(() => new ChargeDistributeByList()),
						RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter,
						ChargeDistributeByList.Codes.Weight);
				});
			}
		}

		public CodePairRegistryItem InvoiceChargesForImport
		{
			get
			{
				return GetItem("InvoiceChargesForImport", delegate
				{
					return new CodePairRegistryItem(
						"InvoiceChargesForImport",
						Categories.Customs,
						ResString.GetMultilingualString("31E2E4D9-89CD-4686-B781-F30EC3C5F689", "Invoice Charges Import Editor"),
						ResString.GetMultilingualString("95F56CAE-7C4E-4465-9948-5517546A2B4E", "Change default value of the Invoice Charges “Distribute By”-value in Import Declarations."),
						new CodeDescriptionPairListProvider(() => new ChargeDistributeByList()),
						RegistryStorageFlags.Company,
						RegistryOptions.CannotCallParameterlessValueGetter,
						ChargeDistributeByList.Codes.Value);
				});
			}
		}

		#region Enable Auto Tariff Description Population

		public AutomatedTariffDescriptionPopulationRegistryItem EnableAutoTariffDescriptionPopulation
		{
			get
			{
				return GetItem("EnableAutoTariffDescriptionPopulation", delegate
				{
					var result = new AutomatedTariffDescriptionPopulationRegistryItem(
						"EnableAutoTariffDescriptionPopulation",
						RawDataRegistry.Categories.Customs,
						ResString.GetMultilingualString("C00AE56E-043F-47AF-81C4-C4845319335C", "Enable Auto Population of Goods Description"),
						ResString.GetMultilingualString("C00AE56E-043F-47AF-81C4-C4845319335C", "Enable Auto Population of Goods Description"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						new AutomatedTariffDescriptionPopulation()
					);
					return result;
				});
			}
		}

		#endregion

		#region Universal Customs Message Processing

		public IntRegistryItem UCUInterchangesPerBatch
		{
			get
			{
				return GetItem("UCUInterchangesPerBatch", delegate
				{
					return new IntRegistryItem(
						"UCUInterchangesPerBatch",
						Categories.Customs,
						ResString.GetMultilingualString("FA03550A-67E5-495E-B428-FB3EB2DE7DD3", "Unpacking Interchanges Per Batch"),
						ResString.GetMultilingualString("BCBB806E-9EE6-49DD-86E4-47344AEC19C1", "The number of interchanges in each batch (per save) of Interchanges Unpacking services tasks(UCU)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						50);
				});
			}
		}

		public IntRegistryItem UCUInterchangeUnpackingMaxRetryCount
		{
			get
			{
				return GetItem("UCUInterchangeUnpackingMaxRetryCount", delegate
				{
					return new IntRegistryItem(
						"UCUInterchangeUnpackingMaxRetryCount",
						Categories.Customs,
						ResString.GetMultilingualString("6A16CF6E-BD94-4763-AE0D-3A965BB908D1", "Max Retry Count to Unpacking Interchange"),
						ResString.GetMultilingualString("4B789082-6E39-4CB3-9868-3533FED3F1E9", "The Max Retry Count to Unpacking Interchange in Interchanges Unpacking services tasks(UCU)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						3);
				});
			}
		}

		public IntRegistryItem UCPInterchangesPerBatch
		{
			get
			{
				return GetItem("UCPInterchangesPerBatch", delegate
				{
					return new IntRegistryItem(
						"UCPInterchangesPerBatch",
						Categories.Customs,
						ResString.GetMultilingualString("C9F89A3C-E34A-4D4F-9252-F048C45CA51F", "Packing Interchanges Per Batch"),
						ResString.GetMultilingualString("031F6EF7-7850-4E84-8E33-9CEFB745BF57", "The number of interchanges in each batch (per save) of Interchanges Packing services tasks(UCP)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						50);
				});
			}
		}

		public IntRegistryItem UCPInterchangePackingMaxRetryCount
		{
			get
			{
				return GetItem("UCPInterchangePackingMaxRetryCount", delegate
				{
					return new IntRegistryItem(
						"UCPInterchangePackingMaxRetryCount",
						Categories.Customs,
						ResString.GetMultilingualString("DC3C9C36-FE4F-460E-A751-64212E5EFCE9", "Max Retry Count for Packing Interchange"),
						ResString.GetMultilingualString("C90D846D-6E42-48FD-BACE-F1F174410FD9", "The Max Retry Count for Packing Interchange in Interchanges Packing services tasks(UCP)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						3);
				});
			}
		}

		#endregion

		public CodeDescriptionPairListWithDefaultCodeRegistryItem CustomsDeclarationAuditingCategories
		{
			get
			{
				return GetItem("CustomsDeclarationAuditingCategories", () =>
				{
					var defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("AEO", ResString.GetMultilingualString("00713FCC-5189-4911-A7A6-EF71ABEB9C22", "Authorized Economic Operator"));
					defaultList.AddPair("INT", ResString.GetMultilingualString("BCDAC382-2E44-46FE-BAB8-16C926DBD452", "Internal"));
					defaultList.AddPair("EXT", ResString.GetMultilingualString("5807E232-D2E9-4B57-8A64-071CABF1CDBC", "External"));

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"CustomsDeclarationAuditingCategories",
						Categories.Customs_DeclarationAuditing,
						ResString.GetMultilingualString("CE3513C1-6996-41B4-A4ED-5ADE215103F7", "Audit Types"),
						ResString.GetMultilingualString("469F0F25-B7E6-4F1C-935A-50568CE1965E", "Enter or override codes to categorize the types of audit you will undertake. These codes will be offered to users in the audit screen's 'Category' drop-down."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultList, true, false, 3, false);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem CustomsDeclarationAuditingResults
		{
			get
			{
				return GetItem("CustomsDeclarationAuditingResults", () =>
				{
					var defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("FAL", ResString.GetMultilingualString("C05D4886-8E27-4226-8294-287B21724221", "Failed"));
					defaultList.AddPair("PS1", ResString.GetMultilingualString("597DEC2F-63A8-4F69-8D13-F27B5C00732D", "Passed first time"));
					defaultList.AddPair("PS2", ResString.GetMultilingualString("50CE0ABD-C8FF-457E-8EC2-8D5FDA8349D5", "Passed on subsequent audit after previous failure"));

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"CustomsDeclarationAuditingResults",
						Categories.Customs_DeclarationAuditing,
						ResString.GetMultilingualString("0E59D420-1DC6-4F50-A81E-7DE8E5B2A186", "Result or outcome codes"),
						ResString.GetMultilingualString("978F5ECF-FE00-4B55-97D1-8B9C7C4B79FA", "Enter or override codes that user must select to complete their audit, to express the findings, results or conclusions of the process."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultList, true, false, 3, false);
				});
			}
		}

		public IntRegistryItem NoOfGuaranteesBGCServiceProcessPerBatch
		{
			get
			{
				return GetItem("NoOfGuaranteesBGCServiceProcessPerBatch", () =>
				{
					return new IntRegistryItem(
						"NoOfGuaranteesBGCServiceProcessPerBatch",
						Categories.Customs,
						ResString.GetMultilingualString("85B78285-1AAC-4F6C-945B-868ED90CFDDF", "Number of guarantee records the Guarantee Balance Calculation service task will process per batch"),
						ResString.GetMultilingualString("5B4A23BA-7A91-4753-90EC-E312B0A0DB6A", "Number of guarantee records the Guarantee Balance Calculation service task will process per batch. If guarantee usage exceeds this threshold, the unprocessed records will be processed in the next batch."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 50,
						minValue: 1,
						maxValue: int.MaxValue);
				});
			}
		}

		public DecimalRegistryItem DefaultInsuranceRate
		{
			get
			{
				return GetItem("DefaultInsuranceRate", () =>
				{
					var item = new DecimalRegistryItem(
						"DefaultInsuranceRate",
						Categories.Customs,
						ResString.GetMultilingualString("E659D8F6-7343-44FF-A3FB-C0C50DAAC31A", "Default Insurance Rate"),
						ResString.GetMultilingualString("5AE23210-C372-4E44-A430-C4C1E53E400C", "Override this value to default % of Line Price when 'ONS' charge code is added on Invoice Charges."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						0m);
					item.EditorInfo = new NumericRegistryEditorInfo(5);
					return item;
				});
			}
		}

		#region Declaration Lock For Edit

		public DeclarationLockConfigRegistryItem DeclarationLockForEdit
		{
			get
			{
				return GetItem("DeclarationLockForEdit", () =>
				{
					return new DeclarationLockConfigRegistryItem(
						"DeclarationLockForEdit",
						Categories.Customs,
						ResString.GetMultilingualString("fe118293-4cf1-4f6f-9e28-c69b2c58dd89", "Declaration Lock For Edit"),
						ResString.GetMultilingualString("299b2c37-ded3-4a87-a9e0-961bfb533f3a", "If declaration types are added, then the system will attempt to lock (for editing) the declaration of the specified type if an event that causes locking is logged. The tabs that become locked should also be specified. In the case that a declaration has multiple entry types, the declaration will become locked if a lock event is logged against all of the associated entries from the lock mode.\r\nNOTE: Functionality only applies to user edits of a declaration.\r\nNOTE: Applies only to Edits via Customs Declaration or Shipment form. Automated data imports and other automated functions are excluded."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#region Severity Level Of Usage Comment Validation

		public CodePairRegistryItem SeverityLevelOfUsageCommentValidation
		{
			get
			{
				return GetItem("SeverityLevelOfUsageCommentValidation", delegate
				{
					return new CodePairRegistryItem(
						"SeverityLevelOfUsageCommentValidation",
						Categories.Customs,
						ResString.GetMultilingualString("768D4CD3-7598-4691-8116-024BDB7960B5", "Severity Level Of Usage Comment Validation"),
						ResString.GetMultilingualString("CAFFAFB9-DBFE-4D5F-92A4-4ED3FD878B05", "Set the severity level for validation relating to the Usage Comment on an Invoice Line. The Usage Comment is defaulted to the Invoice Line from the Classification Line on the Product. Indicate the system behavior if a user does not indicate that they have read the Usage Comment on the Invoice Line."),
						new CodeDescriptionPairListProvider(() => new ProductAuditActions()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ProductAuditActions.Codes.NoAction);
				});
			}
		}
		#endregion

		#region Severity Level Of Total Weight Validation

		public CodePairRegistryItem SeverityLevelOfTotalWeightValidation
		{
			get
			{
				return GetItem("SeverityLevelOfTotalWeightValidation", delegate
				{
					var result = new CodePairRegistryItem(
						"SeverityLevelOfTotalWeightValidation",
						Categories.Customs,
						ResString.GetMultilingualString("C16D688D-E232-41D8-8185-B67B05999027", "Severity Level Of Total Weight Validation"),
						ResString.GetMultilingualString("04E66A25-A398-4341-AFC6-999C393CAD54", "Set the severity level for validation relating to Total Weight in Declaration. Indicate the system behavior of the validation between the total weight of the invoices lines and the value declared in the declaration. Not used in NZ."),
						new CodeDescriptionPairListProvider(() => new ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ProductAuditActions.Codes.AddWarningValidation);
					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingNewZealand;
					return result;
				});
			}
		}
		#endregion

		#region Severity Level Of Invoice Line Validation Against Existing Product Code Data

		public CodePairRegistryItem SeverityLevelOfInvoiceLineValidationAgainstProductData
		{
			get
			{
				return GetItem("SeverityLevelOfInvoiceLineValidationAgainstProductData", delegate
				{
					var result = new CodePairRegistryItem(
						"SeverityLevelOfInvoiceLineValidationAgainstProductData",
						Categories.Customs,
						ResString.GetMultilingualString("BB9813EC-2465-48CC-976B-7E46E166C940", "Severity Level for validating invoice line data against existing product code data"),
						ResString.GetMultilingualString("EE184296-C962-40D4-849A-2F576D8CBCAF", "Set the severity level for validating invoice line data against existing product code data. When an existing product code is used on an invoice line, the system will validate the data on the invoice line against existing data on the product code to confirm that the data matches."),
						new CodeDescriptionPairListProvider(() => new ProductAuditActions()),
						RegistryStorageFlags.Company,
						ProductAuditActions.Codes.AddWarningValidation);
					return result;
				});
			}
		}
		#endregion

		#region Refresh Options When Attaching an ASN type invoice to a shipment

		public ASNRefreshOptionsConfigRegistryItem ASNRefreshOptions
		{
			get
			{
				return GetItem("ASNRefreshOptions", delegate
				{
					return new ASNRefreshOptionsConfigRegistryItem(
						"ASNRefreshOptions",
						Categories.Customs,
						ResString.GetMultilingualString("41cf0467-4e32-4921-9dd9-07590926bcd8", "Refresh options when attaching an ASN type invoice to a declaration"),
						ResString.GetMultilingualString("9b33d38b-14ec-4047-9eaa-b8849c949689", "Select the fields you want to be refreshed from the Product file when a Commercial Invoice (Type = ASN-Advance Shipping Notice) is attached to a declaration. If the grid is blank ALL fields will be refreshed."),
						RegistryStorageFlags.Company,
						new ASNRefreshOptionsConfigCollection());
				});
			}
		}

		public ImmutableList<ZString> ASNRefreshOptionsFieldTypes(Guid companyPK)
		{
			var refreshOptions = ASNRefreshOptions.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			if (refreshOptions != null && refreshOptions.Count > 0)
			{
				return refreshOptions.Cast<ASNRefreshOptionsConfig>().Select(x => x.FieldType).ToImmutableList();
			}
			return ImmutableList<ZString>.Empty;
		}

		#endregion

		public BooleanRegistryItem EnableGenericMessageDeliveryWebService
		{
			get => GetItem("EnableGenericMessageDeliveryWebService", delegate
			{
				return new BooleanRegistryItem(
					"EnableGenericMessageDeliveryWebService",
					Categories.Customs,
					(NoResString)"Enable Generic Message Delivery Service",
					(NoResString)"Enables Generic Message Delivery Service",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
			});
		}

		#region Enable Query Interchange By EHub Portal Webservice

		public BooleanRegistryItem EnableQueryInterchangeByEHubPortalWebservice
		{
			get
			{
				return GetItem("EnableQueryInterchangeByEHubPortalWebservice", delegate
				{
					return new BooleanRegistryItem(
						"EnableQueryInterchangeByEHubPortalWebservice",
						Categories.Customs,
						ResString.GetMultilingualString("6E8125BC-CA29-4C32-9966-55C7E2DB7D27", "Enable Query Interchange By eHub Portal Web service"),
						ResString.GetMultilingualString("2F54C56C-97C3-4E86-A8B7-A1601BA83248", "If set to 'yes' the system will enable the query menu on Declaration - Messages, our clients can check the interchange on https://ehubadmin.wtg.zone/Message."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Default Auto Apportion Weight

		public BooleanRegistryItem EnableAutoApportionWeight
		{
			get
			{
				return GetItem("EnableAutoApportionWeight", delegate
				{
					return new BooleanRegistryItem(
						"EnableAutoApportionWeight",
						Categories.Customs,
						ResString.GetMultilingualString("888eafc8-ad7a-4bdf-8d96-9c7e20cdf4d6", "Default Auto Apportion Weight"),
						ResString.GetMultilingualString("2d36d564-f415-4f46-a907-65076ae4d463", "When the value of this registry item is set to 'Yes', the 'Auto Apportion Weight ' menu item under the Customs Declaration > Brokerage path will always be checked by default."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						RegistryOptions.CannotCallParameterlessValueGetter,
						false);
				});
			}
		}

		#endregion

		#region Always Copy from Previous Line

		public BooleanRegistryItem AlwaysCopyFromPreviousLine
		{
			get
			{
				return GetItem("AlwaysCopyFromPreviousLine", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysCopyFromPreviousLine",
						Categories.Customs,
						ResString.GetMultilingualString("0a816032-9c56-4c47-9f78-dd6865de2f1b", "Always Copy from Previous Line"),
						ResString.GetMultilingualString("359675cc-d7e1-4482-8143-3aaba8bef15d", "When the value of this registry item is set to 'Yes', the 'Copy from Previous Line' menu item will always be ticked."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						false);
				});
			}
		}

		#endregion

		#region Default Weight From Invoice Qty

		public BooleanRegistryItem DefaultWeightFromInvoiceQty
		{
			get
			{
				return GetItem("DefaultWeightFromInvoiceQty", delegate
				{
					return new BooleanRegistryItem(
						"DefaultWeightFromInvoiceQty",
						Categories.Customs,
						ResString.GetMultilingualString("1D677164-3AC9-410D-8C4D-86965C6D2DA0", "Default Gross Weight From Invoice Qty"),
						ResString.GetMultilingualString("731D3D53-AC9B-4E08-96F6-9493A8CD2345", "Set this to 'Yes' in order to automatically default the gross weight from an invoice quantity when the invoice unit of quantity is a weight unit."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Product Match

		public BooleanRegistryItem EnableExactMatchForProduct
		{
			get
			{
				return GetItem("EnableExactMatchForProduct", delegate
				{
					return new BooleanRegistryItem(
						"EnableExactMatchForProduct",
						Categories.Customs,
						ResString.GetMultilingualString("2ccb45cf-a948-46d0-9dfa-a66339084a51", "Enable Exact Match for Product from Declaration"),
						ResString.GetMultilingualString("5f343e4e-1753-4428-a967-4106f0f87555", @"If set to 'yes' the system will match and retrieve a product code entered in the Commercial Invoice lines with a product code in the product files, if that product code has:
The same importer and supplier matching the declaration details; OR
The same importer matching the declaration details, but there are no Suppliers listed on the organizations grid for the product code; OR
The same supplier matching the declaration details, but there are no Importers listed on the organizations grid for the product code."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		IRegistryItem Integration.Customs.Shared.ICustomsDataRegistry.EnableExactMatchForProduct => EnableExactMatchForProduct;

		#endregion

		#region Client Product

		public CodePairRegistryItem AssumeCreatedProductBelongsToClient
		{
			get
			{
				return GetItem("AssumeCreatedProductBelongsToClient", delegate
				{
					return new CodePairRegistryItem(
						"AssumeCreatedProductBelongsToClient",
						Categories.Customs,
						ResString.GetMultilingualString("24A3B4C5-CA1D-47A2-95E5-F19116BAD37E", "Assume Created Product belongs to Client"),
						ResString.GetMultilingualString("F82A8BB6-5151-4396-B1B6-0A0F83EC3A6A", @"This governs the default behavior when a product is created as a result of data entry on a Customs Declaration of a new or unrecognized product.
If ‘Yes’ then the system will create the product and add the Importer organization to the product for Imports and the Supplier organization for Exports. If ‘No’ then the system will create the product and add the Supplier organization to the product for Imports as well as for Exports.
‘Always’ operates in the same way as ‘Yes’, except the user will not be able to change the behavior at the time the product is created. Likewise, ‘Never’ is congruent with ‘No’, but does not allow user discretion."),
						new CodeDescriptionPairListProvider(() => new ClientProductCreationTypeList()),
						RegistryStorageFlags.Company,
						ClientProductCreationTypeList.Codes.Yes);
				});
			}
		}
		#endregion

		#region Default Owners Ref

		public BooleanRegistryItem PopulateOwnersRef
		{
			get
			{
				return GetItem("PopulateOwnersRef", delegate
				{
					return new BooleanRegistryItem(
						"PopulateOwnersRef",
						Categories.Customs,
						ResString.GetMultilingualString("1919ac15-40bf-4368-8dc6-9c18d0bc6247", "Populate Owner's Ref with Order Numbers"),
						ResString.GetMultilingualString("B6EDE708-8712-4E59-8F8F-899BD17350BB", "If set to 'Yes', the system will default Order Numbers into the Declaration Owner Reference field for both Shipments and Customs Declarations. Note: Any values defaulting into Owner Ref will be truncated to the valid allowable size."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Show Header Tariff Data

		public BooleanRegistryItem ShowHeaderTariffData
		{
			get
			{
				return GetItem("ShowHeaderTariffData", delegate
				{
					return new BooleanRegistryItem(
						"ShowHeaderTariffData",
						Categories.Customs,
						ResString.GetMultilingualString("{D8F64DF0-B0EC-4B1A-94BA-5C6E0038031C}", "Show Header Tariff Data"),
						ResString.GetMultilingualString("{B60A5359-BA32-4BA0-B75C-E1BE7D988F97}", "If set to 'Yes', the system will display top level data in the result of a Tariff Search."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true);
				});
			}
		}

		IRegistryItem Integration.Customs.Shared.ICustomsDataRegistry.ShowHeaderTariffData => ShowHeaderTariffData;

		#endregion

		#region 3rd Party Interfaces

		public GuidRegistryItem ThirdPartyCustomsResponseEmailNotificationGroup
		{
			get
			{
				return GetItem("CUSTOMS_THIRD_PARTY_CUSTOMS_RESPONSE_EMAIL_NOTIFICATION_GROUP", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CUSTOMS_THIRD_PARTY_CUSTOMS_RESPONSE_EMAIL_NOTIFICATION_GROUP",
						Categories.Customs_3rdPartyInterfaces,
						ResString.GetMultilingualString("54459405-15ad-464c-a9dd-547d36aff124", "Customs Response Email Notification Group"),
						ResString.GetMultilingualString("d0a83384-9fb5-4073-9382-45bbbe0dff00", "The group that will be sent notification emails for Customs responses."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public StringRegistryItem ThirdPartyInterfacesExportPath
		{
			get
			{
				return GetItem("CUSTOMS_THIRD_PARTY_INTERFACES_EXPORT_PATH", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CUSTOMS_THIRD_PARTY_INTERFACES_EXPORT_PATH",
						Categories.Customs_3rdPartyInterfaces,
						ResString.GetMultilingualString("0112194e-c5f1-4e8a-b72d-0b949b53b7bf", "Export Path"),
						ResString.GetMultilingualString("4c6a57c0-0501-4ae8-a21d-897524e48920", "Directory path for export of Customs information from {0} to an External Customs System.", Core.Constants.ProductName),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public StringRegistryItem ThirdPartyInterfacesImportPath
		{
			get
			{
				return GetItem("CUSTOMS_THIRD_PARTY_INTERFACES_IMPORT_PATH", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CUSTOMS_THIRD_PARTY_INTERFACES_IMPORT_PATH",
						Categories.Customs_3rdPartyInterfaces,
						ResString.GetMultilingualString("5501dd40-8edd-4a47-a546-afb4cec369c4", "Import Path"),
						ResString.GetMultilingualString("27abf6db-e15d-4792-af37-c3fa8fba04e7", "Directory path for Import of Customs information from an External Customs System to {0}.", Core.Constants.ProductName),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region Audit Actions

		public CodePairRegistryItem ImportProductAuditAction
		{
			get
			{
				return GetItem("ImportProductAuditAction", delegate
				{
					var result = new CodePairRegistryItem(
						"ImportProductAuditAction",
						Categories.Customs_AuditActions,
						ResString.GetMultilingualString("66c3dbba-0447-4182-8010-2d7439253391", "Import Product Audit Action"),
						ResString.GetMultilingualString("45169eea-0301-4945-990c-e14ccfe622d3", "Set the Product Audit Action for unaudited product classifications in Import Declarations."),
						new CodeDescriptionPairListProvider(() => new ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ProductAuditActions.Codes.NoAction);
					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingCanada;
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportProductAuditAction
		{
			get
			{
				return GetItem("ExportProductAuditAction", delegate
				{
					var result = new CodePairRegistryItem(
						"ExportProductAuditAction",
						Categories.Customs_AuditActions,
						ResString.GetMultilingualString("005e4e30-c95c-4dd7-9300-188a439af8ba", "Export Product Audit Action"),
						ResString.GetMultilingualString("be07db9e-461e-4edc-a062-f4a8d4ccc6bc", "Set the Product Audit Action for unaudited product classifications in Export Declarations."),
						new CodeDescriptionPairListProvider(() => new ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ProductAuditActions.Codes.NoAction);
					return result;
				});
			}
		}

		#endregion

		#region Drawbacks

		public IntRegistryItem MaximumAgeforRepresentativeShipmentCalculationMethod
		{
			get
			{
				return GetItem("MaximumAgeforRepresentativeShipmentCalculationMethod", delegate
				{
					return new IntRegistryItem(
						"MaximumAgeforRepresentativeShipmentCalculationMethod",
						Categories.Customs_Drawbacks,
						ResString.GetMultilingualString("72155b33-a540-4bbe-ab2e-cd712e25bb63", "Maximum Age for Representative Shipment Calculation Method"),
						ResString.GetMultilingualString("a288d84f-65de-495c-91a4-797cc28e7542", "The maximum age (in days) of import shipments to be considered when calculating claim amount using Representative Shipment method."),
						RegistryStorageFlags.Company,
						365);
				});
			}
		}

		public IntRegistryItem HistoryWindowForExceptionReportOfUnusualUnitValue
		{
			get
			{
				return GetItem("HistoryWindowForExceptionReportOfUnusualUnitValue", delegate
				{
					return new IntRegistryItem(
						"HistoryWindowForExceptionReportOfUnusualUnitValue",
						Categories.Customs_Drawbacks,
						ResString.GetMultilingualString("f3487fca-6446-4281-9339-e551fd6fb436", "History Window (Months) for Exception Report of Unusual Unit Value"),
						ResString.GetMultilingualString("2de7fef1-21d5-4af2-8e01-df09665a6d3e", "The number of past months that will be searched to determine if a unit value is unusual and so reported on the Drawback Exception Report. The variance percentage is specified in the registry key 'Variance Percentage Used For Exception Report of Unusual Unit Value'"),
						RegistryStorageFlags.Company,
						18);
				});
			}
		}

		public DecimalRegistryItem VariancePercentageUsedForExceptionReportOfUnusualUnitValue
		{
			get
			{
				return GetItem("VariancePercentageUsedForExceptionReportOfUnusualUnitValue", delegate
				{
					return new DecimalRegistryItem(
						"VariancePercentageUsedForExceptionReportOfUnusualUnitValue",
						Categories.Customs_Drawbacks,
						ResString.GetMultilingualString("adf0d049-5539-4130-9404-d768b82f04a1", "Variance Percentage Used For Exception Report of Unusual Unit Value"),
						ResString.GetMultilingualString("95395b53-339b-46a8-a01e-b0a8a63d6d92", "The percentage used when checking if a unit value is unusual and so reported on the Drawback Exception Report. The History Window used for this calculation is specified in the registry key 'History Window (Months) for Exception Report of Unusual Unit Value'"),
						RegistryStorageFlags.Company,
						20);
				});
			}
		}

		#endregion

		#region Always Assume Local Organisation Is Both Importer And Exporter

		public BooleanRegistryItem AlwaysAssumeLocalOrganisationIsBothImporterAndExporter
		{
			get
			{
				return GetItem("AlwaysAssumeLocalOrganisationIsBothImporterAndExporter", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysAssumeLocalOrganisationIsBothImporterAndExporter",
						Categories.Customs,
						ResString.GetMultilingualString("ed612745-8e98-46c3-94bf-932215cf0e74", "Always Assume the Local Organization is Both an Importer and an Exporter"),
						ResString.GetMultilingualString("7436c117-b0cc-4cbd-98af-449b5d284e00", "Always assume the Owner on an import declaration, and the Supplier on an export declaration, is both an Importer and Exporter of any part created during the declaration process, i.e. the 'Relationship' specified on the 'Related Organizations' tab of a part created from a declaration will be set to 'BTH'."),
						RegistryStorageFlags.All, RegistryOptions.PreserveTestValue, false);
				});
			}
		}

		IRegistryItem Integration.Customs.Shared.ICustomsDataRegistry.AlwaysAssumeLocalOrganisationIsBothImporterAndExporter => AlwaysAssumeLocalOrganisationIsBothImporterAndExporter;

		#endregion

		#region WarnUserWhenCommercialInvoiceHasBeenUsedBefore

		public BooleanRegistryItem WarnUserWhenCommercialInvoiceHasBeenUsedBefore
		{
			get
			{
				return GetItem("WarnUserWhenCommercialInvoiceHasBeenUsedBefore", delegate
				{
					return new BooleanRegistryItem(
						"WarnUserWhenCommercialInvoiceHasBeenUsedBefore",
						Categories.Customs,
						ResString.GetMultilingualString("fe4b8f9b-0ae7-4323-82b4-c52c65a91bb5", "Warn User When Commercial Invoice Has Been Used Before"),
						ResString.GetMultilingualString("a8262ebd-d241-48d7-98e6-5117f500c23b", "If this registry key is set to 'Yes' then a warning will be displayed, on the Invoice Header Tab of a declaration, if a Commercial Invoice Number is entered that has already been used on another declaration for this supplier."),
						RegistryStorageFlags.Company,
						Environment.Env.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Singapore);
				});
			}
		}

		#endregion

		#region GroupToSendLateCargoReportAndWarningsTo
		public GuidRegistryItem GroupToSendLateSeaCargoReportAndWarningsTo
		{
			get
			{
				return GetItem("GroupToSendLateSeaCargoReportAndWarningsTo", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
					"GroupToSendLateSeaCargoReportAndWarningsTo",
					Categories.Customs_LateCargoReport,
					ResString.GetMultilingualString("d454b88d-6257-462c-89c2-7b8b2bcd4713", "Group To Send Late Cargo Report and Warnings To for Sea Shipments"),
					ResString.GetMultilingualString("9c6a0f82-6813-44f5-9ba6-5c3835b7c943", "The Staff Group who are to receive the Late Cargo Report Exceptions Report for Sea Shipments"),
					RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
					return result;
				});
			}
		}
		public GuidRegistryItem GroupToSendLateAirCargoReportAndWarningsTo
		{
			get
			{
				return GetItem("GroupToSendLateAirCargoReportAndWarningsTo", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
					"GroupToSendLateAirCargoReportAndWarningsTo",
					Categories.Customs_LateCargoReport,
					ResString.GetMultilingualString("de863973-6781-40c9-8f4c-07839ef3055c", "Group To Send Late Cargo Report and Warnings To for Air Shipments"),
					ResString.GetMultilingualString("9c495742-3f5b-4799-b98a-a4907353942f", "The Staff Group who are to receive the Late Cargo Report Exceptions Report for Air Shipments"),
					RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public BooleanRegistryItem PublishLateCargoMilestone
		{
			get
			{
				return	GetItem("PublishLateCargoMilestone", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
					"PublishLateCargoMilestone",
					Categories.Customs_LateCargoReport,
					ResString.GetMultilingualString("83d45198-ec4c-4b46-8819-78eca6de814f", "Publish Late Cargo Milestone"),
					ResString.GetMultilingualString("7ebd3c6b-2015-404b-af53-c15f45ebfa9b", "If you override this setting and set to NO then the Cargo Report Accepted Milestone that is automatically generated to support the Late and Pending Cargo report feature will not be published in Web Tracker. If you leave this item at its default setting then this Milestone and its completed date will be published in Web Tracker."),
					RegistryStorageFlags.Company, true);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IntRegistryItem LateAndPendingCargoReportBatchSize
			=> GetItem(
				"LateAndPendingCargoReportBatchSize",
				() => new IntRegistryItem(
						"LateAndPendingCargoReportBatchSize",
						Categories.Customs_LateCargoReport,
						ResString.GetMultilingualString("13EC730D-5B2F-4F56-ABF6-8A995A5E09E7", "Batch Size for Late and Pending Cargo Report"),
						ResString.GetMultilingualString(
							"001B38AD-8878-4F25-BA83-5806359E7898",
							$"Specifies batch size when processing Late and Pending Cargo Report.\r\n" +
							$"If the processing time is too long, increase the value of this setting.\r\n" +
							$"If the process uses too much memory, reduce the value of this setting."
						),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 500, minValue: 1, maxValue: 5000
				)
			);

		#endregion

		#region CustomsIsRequiredForDomesticShipments

		public BooleanRegistryItem CustomsIsRequiredForDomesticShipments
		{
			get
			{
				return GetItem("CustomsIsRequiredForDomesticShipments", delegate
				{
					return new BooleanRegistryItem(
						"CustomsIsRequiredForDomesticShipments",
						Categories.Customs,
						ResString.GetMultilingualString("1b949f9e-189c-4f96-b911-0c2fc5d9e172", "Customs is required for domestic shipments"),
						ResString.GetMultilingualString("c1ba1bfa-69d7-47da-87c7-b8b3971d658c", "Set this to 'Yes' in order to force customs document checks on all shipments, even domestic ones. Set it to 'No' to enable customs document checks on non-domestic shipments only."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region DeclarationImportDescriptionCustomization

		public CodeDescriptionBoolDisallowNewRegistryItem DeclarationImportDescriptionCustomization
		{
			get
			{
				return GetItem("DeclarationImportDescriptionCustomization", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"DeclarationImportDescriptionCustomization",
						Categories.Customs,
						ResString.GetMultilingualString("988F773F-FC2B-475E-9306-76E0F2A45A3F", "Declaration (Import) Description Customization"),
						ResString.GetMultilingualString("8E256CCA-3478-4DC5-AD93-28E73FC930FE",
							"Select one or more of the following check boxes to define what reference information will be used to describe an import declaration when it appears in the Favorites or Recent Items areas on the main {0} form and the Recent Items on the Customs Declaration module.",
							Core.Constants.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("1B8DFC1D-A01F-45AD-9652-0C326700A68E", "Select the data you wish to use to describe an import declaration"), true, true),
						new CodeDescriptionBoolDisallowNewCollection() {
						{ "JNO", ResString.GetMultilingualString("0555F605-3289-4487-AF35-3BF46670CD78", "Job Number"), true },
						{ "MBL", ResString.GetMultilingualString("6BF6A8A6-ADD0-490A-981B-B46FF5137D31", "Master Bill"), true },
						{ "HBL", ResString.GetMultilingualString("E00A2C1D-41BC-42ED-8F93-9C5175CAAA8B", "House Bill"), true },
						{ "ENT", ResString.GetMultilingualString("48DBE12C-702C-4024-AD11-6DA0AEB8230F", "Entry Number"), false },
						{ "IMP", ResString.GetMultilingualString("98102F78-D860-4C94-AE8E-EC091016BF70", "Importer Code"), false },
						{ "EXP", ResString.GetMultilingualString("C6F778D7-7D3E-40BF-BDF4-F1C8EC27A6A6", "Exporter Code"), false },
						{ "ORF", ResString.GetMultilingualString("4EDAC5A5-A61C-4494-B4DA-88CC59516EC7", "Other Reference"), false }
						});
				});
			}
		}

		#endregion

		#region DeclarationExportDescriptionCustomization

		public CodeDescriptionBoolDisallowNewRegistryItem DeclarationExportDescriptionCustomization
		{
			get
			{
				return GetItem("DeclarationExportDescriptionCustomization", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"DeclarationExportDescriptionCustomization",
						Categories.Customs,
						ResString.GetMultilingualString("A4CC233A-4692-496F-99C6-18C55C9B5432", "Declaration (Export) Description Customization"),
						ResString.GetMultilingualString("5C6102B7-8380-40C1-9DDD-CEF5B2D906B3",
							"Select one or more of the following check boxes to define what reference information will be used to describe an export declaration when it appears in the Favorites or Recent Items areas on the main {0} form and the Recent Items on the Customs Declaration module.",
							Core.Constants.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("FA67E9D0-06B7-47C7-A86F-FA8AAA1E55E3", "Select the data you wish to use to describe an export declaration"), true, true),
						new CodeDescriptionBoolDisallowNewCollection() {
						{ "JNO", ResString.GetMultilingualString("167ECEDC-CDE3-4DE2-A6F9-4B1E28E22666", "Job Number"), true },
						{ "MBL", ResString.GetMultilingualString("C77E102F-664A-48BF-9AE5-883C92D76A5E", "Master Bill"), true },
						{ "HBL", ResString.GetMultilingualString("940D085E-872A-4655-AE8E-69CB2EB69B92", "House Bill"), true },
						{ "ENT", ResString.GetMultilingualString("88057358-4F33-4FF0-895F-A97A0548092F", "Entry Number"), false },
						{ "IMP", ResString.GetMultilingualString("F81E6FB5-6C7A-4D05-9ED2-C2EF52FAC89A", "Importer Code"), false },
						{ "EXP", ResString.GetMultilingualString("D944F9F0-1616-4A2C-B027-28F375443A02", "Exporter Code"), false },
						{ "ORF", ResString.GetMultilingualString("470F2275-39AF-471D-B8B9-507E8EB0F09B", "Other Reference"), false }
						});
				});
			}
		}

		#endregion

		#region Auto Allocate container to invoice lines

		public BooleanRegistryItem AutoAllocateContainerToInvoiceLines
		{
			get
			{
				return GetItem("AutoAllocateContainerToInvoiceLines", delegate
				{
					return new BooleanRegistryItem(
						"AutoAllocateContainerToInvoiceLines",
						Categories.Customs,
						ResString.GetMultilingualString("c555d2ba-c824-4e9f-babf-260b5c359590", "Auto Allocate Container to Invoice Lines"),
						ResString.GetMultilingualString("cdef664e-3aa8-4246-a720-5432e2d3c50a", "When set to 'Yes' a declaration that has a single container will see that container automatically allocated to its invoice lines (by placing a tick in the 'Is For Invoice' tick box)."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem AutoAllocatePackageToInvoiceLines
		{
			get
			{
				return GetItem("AutoAllocatePackageToInvoiceLines", delegate
				{
					return new BooleanRegistryItem(
						"AutoAllocatePackageToInvoiceLines",
						Categories.Customs,
						ResString.GetMultilingualString("11111111-c824-4e9f-babf-260b5c359590", "Auto Allocate Package to Invoice Lines"),
						ResString.GetMultilingualString("22222222-3aa8-4246-a720-5432e2d3c50a", "When set to 'Yes' a declaration that has a single package will see that package automatically allocated to its invoice lines (by placing a tick in the 'Is For Invoice' tick box). Applies only to some countries/regions."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region DeclarationNumberCustomisation

		public BillCustomisationRegistryItem DeclarationNumberCustomisation
		{
			get
			{
				return GetItem("DeclarationNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.GeneratedNumberName = ResString.GetMultilingualString("63A90B2A-3E87-4d8f-B700-032822B79C4F", "Declaration Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("9CC11730-DEDA-4B3F-A5D4-E0CA42F08139", "Declaration");
					dataType.MaxLength = Math.Min(JobDeclarationSchema.JE_DeclarationReference.MaxLength, JobHeaderSchema.JH_JobNum.MaxLength);

					return new BillCustomisationRegistryItem(
						"DeclarationNumberCustomisation",
						Categories.Customs,
						ResString.GetMultilingualString("ed8e470a-a5f2-4e8c-8202-dfddaad51742", "Declaration Number Customization"),
						ResString.GetMultilingualString("0b931584-cf78-4332-ac53-5f6fde15cfce", "Override this value to customize how declaration numbers are formatted"),
						RegistryStorageFlags.All,
						dataType);
				});
			}
		}

		IRegistryItem Integration.Customs.Shared.ICustomsDataRegistry.DeclarationNumberCustomisation => DeclarationNumberCustomisation;

		public BillCustomisationRegistryItem NctsLocalReferenceNumberCustomisation
		{
			get
			{
				return GetItem("NctsLocalReferenceNumberCustomisation", delegate
				{
					var dataType = new BillCustomisationRegistryDataType();
					dataType.GeneratedNumberName = ResString.GetMultilingualString("12345678-3E87-4d8f-B700-032822B79C4F", "NCTS Job Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("1C78D1D8-B7C6-45EC-ADD4-79677D66E488", "NCTS Job");
					dataType.MaxLength = Math.Min(CusInBondHeaderSchema.BH_JobReference.MaxLength, JobHeaderSchema.JH_JobNum.MaxLength);

					return new BillCustomisationRegistryItem(
						"NctsLocalReferenceNumberCustomisation",
						Categories.Customs,
						ResString.GetMultilingualString("12345678-a5f2-4e8c-8202-dfddaad51742", "NCTS Job Number Customization"),
						ResString.GetMultilingualString("12345678-cf78-4332-ac53-5f6fde15cfce", "Override this value to customize how NCTS job numbers are formatted"),
						RegistryStorageFlags.All,
						dataType);
				});
			}
		}

		public BillCustomisationRegistryItem EMCSLocalReferenceNumberCustomisation
		{
			get
			{
				return GetItem("EMCSLocalReferenceNumberCustomisation", delegate
				{
					var dataType = new BillCustomisationRegistryDataType();
					dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.EMCSDeclaration;
					dataType.GeneratedNumberName = ResString.GetMultilingualString("6EE18D0C-F847-4722-8DB2-C4A6D8B3AC3A", "EMCS Job Number");
					dataType.MaxLength = Math.Min(JobDeclarationSchema.JE_DeclarationReference.MaxLength, JobHeaderSchema.JH_JobNum.MaxLength);

					return new BillCustomisationRegistryItem(
						"EMCSLocalReferenceNumberCustomisation",
						Categories.Customs,
						ResString.GetMultilingualString("3848a4f0-4efd-4032-98f4-f2736543fdb2", "EMCS Job Number Customization"),
						ResString.GetMultilingualString("aa8f1e92-a944-46a5-8253-c4bf71bb6426", "Override this value to customize how EMCS job numbers are formatted"),
						RegistryStorageFlags.All,
						dataType);
				});
			}
		}

		public BillCustomisationRegistryItem ConsolidatedEntryNumberCustomisation
		{
			get
			{
				return GetItem("ConsolidatedEntryNumberCustomisation", delegate
				{
					var dataType = new BillCustomisationRegistryDataType();
					dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.ConsolidatedDeclaration;
					dataType.GeneratedNumberName = ResString.GetMultilingualString("CF4F6A5F-937A-4A45-9253-3CAC322AD1D2", "Consolidated Entry Job Number");
					dataType.MaxLength = Math.Min(JobDeclarationSchema.JE_DeclarationReference.MaxLength, JobHeaderSchema.JH_JobNum.MaxLength);

					var regOptions = IsConsolidatedEntriesEnableInAnyCompany() ? RegistryOptions.Default : RegistryOptions.IsHidden;

					var result = new BillCustomisationRegistryItem(
						"ConsolidatedEntryNumberCustomisation",
						Categories.Customs,
						ResString.GetMultilingualString("760434CF-DA6B-4181-805C-F724E3BBABFB", "Consolidated Entry Number Customization"),
						ResString.GetMultilingualString("50103653-95A4-4DCC-BE46-8CEDDBF3D0DD", "Override this value to customize how Consolidated Entry Job Numbers are formatted"),
						RegistryStorageFlags.All,
						regOptions,
						dataType);

					return result;
				});
			}
		}

		#endregion

		#region Attached Job Limits

		public IntRegistryItem OrdersPerDeclarationLimit
		{
			get
			{
				return GetItem("OrdersPerDeclarationLimit", () =>
					new IntRegistryItem(
					"OrdersPerDeclarationLimit",
					Categories.Customs,
					ResString.GetMultilingualString("BB14D31A-58D0-48AD-98C4-8A92BA680BE3", "Maximum number of Orders per Declaration"),
					ResString.GetMultilingualString("B1343FBA-E82D-49D6-8FD5-50A526042724", "This registry determines the maximum number of Orders that may be attached to a Declaration."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					100));
			}
		}

		IRegistryItem Integration.Customs.Shared.ICustomsDataRegistry.OrdersPerDeclarationLimit => OrdersPerDeclarationLimit;

		public DateTimeRegistryItem OrdersPerDeclarationLimitIntroductionTimeUTC
		{
			get
			{
				return GetItem("OrdersPerDeclarationLimitIntroductionTimeUTC", delegate
				{
					return new DateTimeRegistryItem(
						"OrdersPerDeclarationLimitIntroductionTimeUTC",
						Categories.Customs,
						ResString.GetMultilingualString("6128E0D5-6640-451D-B281-6E9CD1CC195C", "Maximum number of Orders per Declaration Active Time"),
						ResString.GetMultilingualString("FDB8E054-9B98-4BA1-A1A6-6D29D40380A3", "Time (in UTC) of introduction of the 'Maximum number of Orders per Declaration' registry."),
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden);
				});
			}
		}

		IRegistryItem Integration.Customs.Shared.ICustomsDataRegistry.OrdersPerDeclarationLimitIntroductionTimeUTC => OrdersPerDeclarationLimitIntroductionTimeUTC;

		#endregion

		#region Raise event DCE when saving error message

		public BooleanRegistryItem RaiseEventDCEWhenSavingErrorMSG
		{
			get
			{
				return GetItem("RaiseEventDCEWhenSavingErrorMSG", delegate
				{
					return new BooleanRegistryItem(
						"RaiseEventDCEWhenSavingErrorMSG",
						Categories.Customs,
						ResString.GetMultilingualString("f9395c5c-3da5-48f0-88be-1a8c6c8538de", "Raise Event DCE when saving a declaration with message errors"),
						ResString.GetMultilingualString("6bf7b534-193b-47a1-9048-d3d064b10416", "If set to 'Yes', an DCE event will be added when saving a declaration with message errors."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Enable Related Party defaulting

		public CodePairRegistryItem RelatedPartyDefaultingForwarder
		{
			get
			{
				return GetItem("RelatedPartyDefaultingForwarder", delegate
				{
					return new CodePairRegistryItem(
						"RelatedPartyDefaultingForwarder",
						Categories.Customs,
						ResString.GetMultilingualString("f7d46328-2e92-43e8-8448-0d6754aa14bf", "Enable Related Party defaulting for Forwarder (Receiving Agents)"),
						ResString.GetMultilingualString("e1006f7b-371a-498b-bb4c-cb8605d6f842", "Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs."),
						new CodeDescriptionPairListProvider(() => new RelatedPartyDefaultingTypeList()),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						RelatedPartyDefaultingTypeList.Codes.Disabled);
				});
			}
		}

		public CodePairRegistryItem RelatedPartyDefaultingCTO
		{
			get
			{
				return GetItem("RelatedPartyDefaultingCTO", delegate
				{
					return new CodePairRegistryItem(
						"RelatedPartyDefaultingCTO",
						Categories.Customs,
						ResString.GetMultilingualString("323c8ee0-066c-49c5-bb63-1554d3abbe1e", "Enable Related Party defaulting for CTO"),
						ResString.GetMultilingualString("ff763a61-d370-4568-8573-bdb967c88bf7", "Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs."),
						new CodeDescriptionPairListProvider(() => new RelatedPartyDefaultingTypeList()),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						RelatedPartyDefaultingTypeList.Codes.Disabled);
				});
			}
		}

		public CodePairRegistryItem RelatedPartyDefaultingDepot
		{
			get
			{
				return GetItem("RelatedPartyDefaultingDepot", delegate
				{
					return new CodePairRegistryItem(
						"RelatedPartyDefaultingDepot",
						Categories.Customs,
						ResString.GetMultilingualString("1f8d7216-91e3-4b76-a9a8-7b7f02ce9ca4", "Enable Related Party defaulting for Depot (CFS)"),
						ResString.GetMultilingualString("43ee1023-0fec-46e7-b43d-447c575679e6", "Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs."),
						new CodeDescriptionPairListProvider(() => new RelatedPartyDefaultingTypeList()),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						RelatedPartyDefaultingTypeList.Codes.Disabled);
				});
			}
		}

		public CodePairRegistryItem RelatedPartyDefaultingCarrier
		{
			get
			{
				return GetItem("RelatedPartyDefaultingCarrier", delegate
				{
					return new CodePairRegistryItem(
						"RelatedPartyDefaultingCarrier",
						Categories.Customs,
						ResString.GetMultilingualString("d5642b6d-8d09-4e39-a087-f0937a6f5b22", "Enable Related Party defaulting for Carrier"),
						ResString.GetMultilingualString("8fd311dd-a0ee-4afe-90dd-a8a3c5b1e144", "Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs."),
						new CodeDescriptionPairListProvider(() => new RelatedPartyDefaultingTypeList()),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						RelatedPartyDefaultingTypeList.Codes.Disabled);
				});
			}
		}

		public CodePairRegistryItem RelatedPartyDefaultingContainerYard
		{
			get
			{
				return GetItem("RelatedPartyDefaultingContainerYard", delegate
				{
					return new CodePairRegistryItem(
						"RelatedPartyDefaultingContainerYard",
						Categories.Customs,
						ResString.GetMultilingualString("29652886-5fd8-4cd0-810f-ba5be94ce137", "Enable Related Party defaulting for Container Yard"),
						ResString.GetMultilingualString("0cba4229-af9f-4f39-bc37-60d5e8299d90", "Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs."),
						new CodeDescriptionPairListProvider(() => new RelatedPartyDefaultingTypeList()),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						RelatedPartyDefaultingTypeList.Codes.Disabled);
				});
			}
		}

		public CodePairRegistryItem RelatedPartyDefaultingBondedWarehouse
		{
			get
			{
				return GetItem("RelatedPartyDefaultingBondedWarehouse", delegate
				{
					return new CodePairRegistryItem(
						"RelatedPartyDefaultingBondedWarehouse",
						Categories.Customs,
						ResString.GetMultilingualString("eb3cf666-a70d-4dd3-b793-55d714374289", "Enable Related Party defaulting for Bonded Warehouse"),
						ResString.GetMultilingualString("08bb2e39-a3fc-4fda-b5f0-66409320f525", "Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs."),
						new CodeDescriptionPairListProvider(() => new RelatedPartyDefaultingTypeList()),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						RelatedPartyDefaultingTypeList.Codes.Disabled);
				});
			}
		}

		public CodePairRegistryItem RelatedPartyDefaultingExternalBroker
		{
			get
			{
				return GetItem("RelatedPartyDefaultingExternalBroker", delegate
				{
					return new CodePairRegistryItem(
						"RelatedPartyDefaultingExternalBroker",
						Categories.Customs,
						ResString.GetMultilingualString("607f6fe6-e58c-4047-83ad-b2a7b7c53dc0", "Enable Related Party defaulting for External Broker"),
						ResString.GetMultilingualString("501ef3f6-5837-4d38-a18c-0c4a3ccace4e", "Enabling this registry will enable the defaulting logic used in Forwarding for Standalone Customs Declaration jobs."),
						new CodeDescriptionPairListProvider(() => new RelatedPartyDefaultingTypeList()),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						RelatedPartyDefaultingTypeList.Codes.Disabled);
				});
			}
		}

		#endregion

		#region Auto Refresh Product Data

		public BooleanRegistryItem EnableAutoRefreshProductData
		{
			get
			{
				return GetItem("EnableAutoRefreshProductData", delegate
				{
					return new BooleanRegistryItem(
						"EnableAutoRefreshProductData",
						Categories.Customs,
						ResString.GetMultilingualString("EDA1D262-9CE1-4858-AF31-81871AFA806A", "Auto Refresh Product Data"),
						ResString.GetMultilingualString("6926516F-7E13-476E-813E-3AD4AB89A561", "When the value of this registry item is set to 'Yes', product details will refresh automatically."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		public CargoWise.ComponentModel.INotificationType GetPowerOfAttorneyNotificationType()
		{
			return GetPowerOfAttorneyNotificationType(Enterprise.Environment.Env.CurrentCompany.PK);
		}

		public CargoWise.ComponentModel.INotificationType GetPowerOfAttorneyNotificationType(Guid companyPK)
		{
			switch (PowerOfAttorneyNotificationType.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty))
			{
				case PowerOfAttorneyNotificationTypeList.Codes.MessageError:
					return NotificationType.MessageError;
				default:
					return NotificationType.Warning;
			}
		}

		public CodePairRegistryItem PowerOfAttorneyNotificationType
		{
			get
			{
				return GetItem("PowerOfAttorneyNotificationType", delegate
				{
					return new CodePairRegistryItem(
						"PowerOfAttorneyNotificationType",
						Categories.Customs,
						ResString.GetMultilingualString("58A62563-2B52-40df-800C-55D0BC5ED12C", "Power Of Attorney Notification Type"),
						ResString.GetMultilingualString("18F4A578-A2AB-4201-9E7C-75CAA7CD7679", "The type of notification that the system will add if Power Of Attorney is missing or expired."),
						new CodeDescriptionPairListProvider(() => new PowerOfAttorneyNotificationTypeList()),
						RegistryStorageFlags.Company,
						PowerOfAttorneyNotificationTypeList.Codes.Warning);
				});
			}
		}

		public CargoWise.ComponentModel.INotificationType GetTariffAdditionalCodeNotificationType()
		{
			return GetTariffAdditionalCodeNotificationType(Enterprise.Environment.Env.CurrentBranch.PK);
		}

		public CargoWise.ComponentModel.INotificationType GetTariffAdditionalCodeNotificationType(Guid branchPK)
		{
			switch (TariffAdditionalCodeNotificationType.GetValueWithoutFallback(Guid.Empty, branchPK, Guid.Empty))
			{
				case TariffAdditionalCodeNotificationTypeList.Codes.MessageError:
					return NotificationType.MessageError;
				default:
					return NotificationType.Warning;
			}
		}

		public CodePairRegistryItem TariffAdditionalCodeNotificationType
		{
			get
			{
				return GetItem("TariffAdditionalCodeNotificationType", delegate
				{
					return new CodePairRegistryItem(
						"TariffAdditionalCodeNotificationType",
						Categories.Customs,
						ResString.GetMultilingualString("05F87CB3-594D-404B-93AA-6E92DAA51265", "Tariff Additional Code Notification Type"),
						ResString.GetMultilingualString("F2911303-3947-4E8C-AC8B-ADD27A69BD1B", "Severity of the invoice line validation when tariff additional codes (supplementary codes) exist but none is selected."),
						new CodeDescriptionPairListProvider(() => new TariffAdditionalCodeNotificationTypeList()),
						RegistryStorageFlags.Branch,
						TariffAdditionalCodeNotificationTypeList.Codes.Warning);
				});
			}
		}

		public IntRegistryItem MemoryThresholdForMessageProcessing
		{
			get
			{
				return GetItem("MemoryThresholdForMessageProcessing", delegate
				{
					return new IntRegistryItem(
						"MemoryThresholdForMessageProcessing",
						Categories.Customs,
						ResString.GetMultilingualString("82E77CE7-7CA2-4F06-B74C-5EF96125A2DB", "Memory Threshold for a Batch of Messages to Process Together (in bytes)"),
						ResString.GetMultilingualString("A3EE2990-71D0-4131-A5B0-DC4FC88E4699", "System will process a batch of messages within this memory threshold. If memory usage exceeds this threshold, the unprocessed messages will be processed in the next batch."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						250000000);
				});
			}
		}

		IRegistryItem Integration.Customs.Shared.ICustomsDataRegistry.MemoryThresholdForMessageProcessing => MemoryThresholdForMessageProcessing;

		public DateTimeRegistryItem CustomsTransactionalBillingTaskNextRunDate_EmailReport
		{
			get
			{
				return GetItem("CustomsTransactionalBillingTaskNextRunDate", delegate
				{
					return new DateTimeRegistryItem(
						 "CustomsTransactionalBillingTaskNextRunDate",
						 Categories.Customs,
						 ResString.GetMultilingualString("7546B37B-804C-4B40-8F45-E8F3FBB8564F", "Transactional Billing Task Next Run Date (Email)"),
						 ResString.GetMultilingualString("B3925619-D477-46A1-8652-C293A3B5BCBB", "Date/time on which the Transactional Billing Task is next due to run.  Hidden."),
						 RegistryStorageFlags.System,
						 RegistryOptions.IsHidden,
						 DateTime.MinValue);
				});
			}
		}

		public DateTimeRegistryItem CustomsTransactionalBillingTaskLastRunDate_BillingAPI
		{
			get
			{
				return GetItem("CustomsTransactionalBillingTaskLastRunDate_BillingAPI", delegate
				{
					return new DateTimeRegistryItem(
						 "CustomsTransactionalBillingTaskLastRunDate_BillingAPI",
						 Categories.Customs,
						 ResString.GetMultilingualString("CE65A477-CD3E-481A-BA05-791ADFCA42BA", "Transactional Billing Task Next Run Date (billing API)"),
						 ResString.GetMultilingualString("5DF02BAE-6E23-4574-8DC6-3AAC565659E2", "Date/time on which the Transactional Billing Task was last run.  Hidden."),
						 RegistryStorageFlags.System,
						 RegistryOptions.IsHidden,
						 DateTime.MinValue);
				});
			}
		}

		public DefaultCurrencyToLocalCurrencyRegistryItem DefaultCurrencyToLocalCurrency
		{
			get
			{
				return GetItem("DefaultCurrencyToLocalCurrency", delegate
				{
					return new DefaultCurrencyToLocalCurrencyRegistryItem(
						"DefaultCurrencyToLocalCurrency",
						Categories.Customs,
						ResString.GetMultilingualString("9C1461D2-F6AE-4553-9FC4-BBE44BC5A189", "Default Currency To Local Currency"),
						ResString.GetMultilingualString("F7CE5F1E-EC19-471A-85F7-6BD501DF07A4", "Default currency to local currency if no other defaults are available?"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
				});
			}
		}
		public BooleanRegistryItem EnablePromptToCreateProducts
		{
			get
			{
				return GetItem("EnablePromptToCreateProducts", delegate
				{
					return new BooleanRegistryItem(
						"EnablePromptToCreateProducts",
						Categories.Customs,
						ResString.GetMultilingualString("19875c4e-f7cb-4c28-ba0b-48367512c164", "Enable Prompt to Create Products"),
						ResString.GetMultilingualString("6a59095c-b33b-47bd-9e60-53f00b164f71", "If set to 'yes', the user will see the prompt to create products when saving customs declaration."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableNewStandaloneInvoiceData
		{
			get
			{
				return GetItem("EnableNewStandaloneInvoiceData", delegate
				{
					return new BooleanRegistryItem(
						"EnableNewStandaloneInvoiceData",
						Categories.Customs,
						(NoResString)"Enable New Standalone Invoice Data",
						(NoResString)"If set to 'yes', standalone commercial invoice should not create fake Declaration.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableInheritanceOfLinkedDeclarationsJobHeader
		{
			get
			{
				return GetItem("EnableInheritanceOfLinkedDeclarationsJobHeader", delegate
				{
					return new BooleanRegistryItem(
						"EnableInheritanceOfLinkedDeclarationsJobHeader",
						Categories.Customs,
						(NoResString)"Enable ParentJob inheritance for Job Header of linked declaration",
						(NoResString)"If set to 'yes', Job Header JH_JH_ParentJob should take value from ultimate parent Declaration's Job Header.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#region Implementation

		IEnumerable<Guid> ActiveCompaniesCountriesExcludingCanada
		{
			get
			{
				if (activeCompaniesCountriesExcludingCanada == null)
				{
					activeCompaniesCountriesExcludingCanada = GetActiveCompaniesCountriesExcluding(Core.Constants.CountryCodes.Canada);
				}
				return activeCompaniesCountriesExcludingCanada;
			}
		}
		IEnumerable<Guid> activeCompaniesCountriesExcludingCanada;

		IEnumerable<Guid> ActiveCompaniesCountriesExcludingNewZealand
		{
			get
			{
				if (activeCompaniesCountriesExcludingNewZealand == null)
				{
					activeCompaniesCountriesExcludingNewZealand = GetActiveCompaniesCountriesExcluding(Core.Constants.CountryCodes.NewZealand);
				}
				return activeCompaniesCountriesExcludingNewZealand;
			}
		}

		IEnumerable<Guid> activeCompaniesCountriesExcludingNewZealand;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		public IEnumerable<Guid> GetActiveCompaniesCountriesExcluding(ZString countryCode)
		{
			//SELECT DISTINCT RN_PK FROM dbo.RefCountry
			//INNER JOIN dbo.GlbCompany on RN_Code = GC_RN_NKCountryCode
			//WHERE GC_IsActive = 1 AND GC_RN_NKCountryCode<> 'CA'

			var countriesForActiveCompaniesExcludingCountrySql = FormattableString.Invariant($@"
SELECT DISTINCT {RefCountrySchema.Constants.PK} FROM {RefCountrySchema.Constants.SqlSchemaName}.{RefCountrySchema.Constants.TableName}
INNER JOIN {GlbCompanySchema.Constants.SqlSchemaName}.{GlbCompanySchema.Constants.TableName} ON {RefCountrySchema.Constants.RN_Code} = {GlbCompanySchema.Constants.GC_RN_NKCountryCode}
WHERE {GlbCompanySchema.Constants.GC_IsActive} = 1 AND {GlbCompanySchema.Constants.GC_RN_NKCountryCode} <> @code
");

			var activeCompanyCountryPksExcludingCountry = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			activeCompanyCountryPksExcludingCountry.Load(
				countriesForActiveCompaniesExcludingCountrySql,
				new ZSqlParameterCollection(ZSqlParameter.New("@code", countryCode, GlbCompanySchema.GC_RN_NKCountryCode)));
			return activeCompanyCountryPksExcludingCountry.Select(record => ((ZGuid)record[RefCountrySchema.Constants.PK]).ToGuid()).ToArray();
		}

		bool IsConsolidatedEntriesEnableInAnyCompany()
		{
			var activeCompaniesSql = FormattableString.Invariant($@"
			SELECT {GlbCompanySchema.Constants.PK}
			FROM {GlbCompanySchema.Constants.SqlSchemaName}.{GlbCompanySchema.Constants.TableName}
			WHERE {GlbCompanySchema.Constants.GC_IsActive} = 1
			");

			var activeCompanies = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			activeCompanies.Load(activeCompaniesSql);
			var regItem = RawDataRegistry.Instance.EnableConsolidatedEntries;
			return activeCompanies.Any(company => regItem.GetValueWithoutFallback(((ZGuid)company[GlbCompanySchema.Constants.PK]).ToGuid(), Guid.Empty, Guid.Empty));
		}

		#endregion
	}
}
