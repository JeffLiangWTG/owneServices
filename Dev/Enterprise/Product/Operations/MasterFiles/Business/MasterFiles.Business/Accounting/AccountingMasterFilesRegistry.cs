using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.MasterFiles.Business
{
	public sealed class AccountingMasterFilesRegistry : RegistryItemSet
	{
		#region Construction

		public static AccountingMasterFilesRegistry Instance
		{
			get { return instance ?? (instance = new AccountingMasterFilesRegistry()); }
		}

		[ThreadStatic]
		static AccountingMasterFilesRegistry instance;

		AccountingMasterFilesRegistry() { }

		BusinessObjectFactory FactoryForCountryDefaultValues
			=> factoryForCountryDefaultValues ?? (factoryForCountryDefaultValues = new BusinessObjectFactory() { NameForDebugging = "AccountingMasterFilesRegistry Factory for Country Defaults" });
		[ThreadStatic]
		static BusinessObjectFactory factoryForCountryDefaultValues;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Accounting_JobInvoicing_GatewayConsolJobInvoicing { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("BDE1007C-3524-4CC9-BAE1-9E5B276A1BA1", "Gateway Consol Job Invoicing")); } }
			public static MultilingualString Accounting_ReceivableDefaults { get { return CombineCategories(Accounting, ResString.GetMultilingualString("025348A3-F339-44B4-AAEF-46A866E1F2B9", "Receivable Defaults")); } }
			public static MultilingualString Accounting_ReceivableDefaults_DefaultSettings { get { return CombineCategories(Accounting_ReceivableDefaults, ResString.GetMultilingualString("6E7D5105-6D5B-4c76-B954-6F881BAB62AE", "Default Settings")); } }
			public static MultilingualString Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders { get { return CombineCategories(Accounting_ReceivableDefaults_DefaultSettings, ResString.GetMultilingualString("2F0D9F68-EDC6-4D1A-86FC-212F535F8282", "Collection Batches and Orders")); } }
			public static MultilingualString Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting { get { return CombineCategories(Accounting_ReceivableDefaults_DefaultSettings, ResString.GetMultilingualString("a689d0e4-bbea-4a24-8e50-d4e1dda1015d", "Credit Note And Invoice Reversal Authorization Settings")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations { get { return CombineCategories(Accounting_ReceivableDefaults, ResString.GetMultilingualString("EAC6A0B9-B9E8-43C0-AC62-C19F49535719", "Form Configurations")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_LocalInvoice { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations, ResString.GetMultilingualString("585A6E97-341A-4EE8-AB6B-0E29A61D7BD5", "Local Invoice")); } }
			public static MultilingualString Accounting_ReportsSetups { get { return CombineCategories(Accounting, ResString.GetMultilingualString("37CF825C-9D41-48ED-875A-827B0E5C8A45", "Reports Setups")); } }
			public static MultilingualString Accounting_GeneralLedgerDefaults { get { return AccountingDataRegistry.Categories.Accounting_GeneralLedgerDefaults; } }
			public static MultilingualString Accounting_GeneralLedgerDefaults_GenerateJournalEntries { get { return CombineCategories(Accounting_GeneralLedgerDefaults, ResString.GetMultilingualString("B278BF84-5778-459C-83BB-EC5F6575CA60", "Generate Journal Entries")); } }
			public static MultilingualString Accounting_GeneralLedgerDefaults_CashFlow { get { return CombineCategories(Accounting_GeneralLedgerDefaults, ResString.GetMultilingualString("8D2B1A5C-6433-4263-A5C6-1C84933D689F", "Cash Flow")); } }
			public static MultilingualString Accounting_PayableDefaults { get { return CombineCategories(Accounting, ResString.GetMultilingualString("27C8DF00-D08D-41FB-8823-BF8C280BF7DE", "Payable Defaults")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings { get { return CombineCategories(Accounting_PayableDefaults, ResString.GetMultilingualString("E845CE68-3325-451E-A02B-24B78D442F94", "Default Settings")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_BulkAPInvoicePosting { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("9a05db0c-0181-4336-aee9-9a90e74f7e33", "Bulk AP Invoice Posting")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_PaymentProcessing { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("eedc1d20-3aa5-4c9c-a396-ba4c23701fb7", "Payment Processing")); } }
			public static MultilingualString Accounting_CreditLimitCheck { get { return CombineCategories(Accounting, ResString.GetMultilingualString("19adc823-1787-410c-9b2f-7891d1d9052c", "Credit Controlled Documents Configuration")); } }
			public static MultilingualString Accounting_CreditLimitCheck_Local { get { return CombineCategories(Accounting_CreditLimitCheck, ResString.GetMultilingualString("c01ea8d6-182c-475f-b14f-dc5bd846b76b", "Local Credit Limit")); } }
			public static MultilingualString Accounting_CreditLimitCheck_Global { get { return CombineCategories(Accounting_CreditLimitCheck, ResString.GetMultilingualString("3a4ebc6d-fa0b-46e5-900e-22d9c026438d", "Global Credit Limit")); } }
			public static MultilingualString Accounting_GovernmentComplianceInvoiceDocument { get { return CombineCategories(Accounting, ResString.GetMultilingualString("E5A63727-00FC-479D-A92A-D40B5AE48983", "Government Compliance Invoice Document")); } }
			public static MultilingualString Accounting_GovernmentComplianceInvoiceDocument_Israel { get { return CombineCategories(Accounting_GovernmentComplianceInvoiceDocument, ResString.GetMultilingualString("E3792E46-C71E-43C1-8B82-4AE4B485A37A", "Israel (IL)")); } }
			public static MultilingualString Accounting_DataConsistencyCheck { get { return CombineCategories(Accounting, ResString.GetMultilingualString("Data Consistency Check", "Data Consistency Check")); } }
			public static MultilingualString Accounting_Web { get { return CombineCategories(Accounting, ResString.GetMultilingualString("NameOfAccountingRegistryItem", "Web")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations, ResString.GetMultilingualString("a2dd67f5-b74b-4919-bf15-9a9af49e95dc", "Collection & Demand Letters")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_1stReminder { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters, ResString.GetMultilingualString("c68b277f-0dee-4b99-b9f6-afa20e2204e9", "1st Reminder")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_2ndReminder { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters, ResString.GetMultilingualString("b8171271-aacc-4ce4-a378-b5c3222342a4", "2nd Reminder")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Collection { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters, ResString.GetMultilingualString("d34c72ad-f2fd-4769-9ced-9b4d4af62caf", "Collection")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Demand { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters, ResString.GetMultilingualString("8b28b3a0-965c-4f55-adac-390f4af99387", "Demand")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Statement { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters, ResString.GetMultilingualString("6ddc7ba2-bcc1-44bd-a267-62837d888ca9", "Statement")); } }
			public static MultilingualString Accounting_ReportingBooks { get { return CombineCategories(Accounting, ResString.GetMultilingualString("B51DD451-86CB-457B-9EAD-2C22D6E1D484", "Reporting Books")); } }
			public static MultilingualString Accounting_Netting { get { return CombineCategories(Accounting, ResString.GetMultilingualString("579333ee-25e1-4b99-be13-a57418c120ee", "Netting")); } }
			public static MultilingualString Accounting_EmailNotification { get { return CombineCategories(Accounting, ResString.GetMultilingualString("57e0ff6d-bad2-4488-980f-484024e00492", "Email Notification")); } }
			public static MultilingualString Accounting_CriticalValidation { get { return CombineCategories(Accounting, ResString.GetMultilingualString("Critical Validation", "Critical Validation")); } }
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations => CombineCategories(Accounting, ResString.GetMultilingualString("16a81456-e8a6-4e0b-acb2-65e0fb2a07c1", "E-Reporting and E-Invoicing Configurations"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_China => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("DE517760-3FAF-474D-A8C4-FB437F43975A", "China (CN)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_India => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("de18e6d3-06c4-428c-ac68-d9f173ee9ef6", "India (IN)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Italy => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("FA9CE01E-C075-4BC9-8F6D-D0E2469AED25", "Italy (IT)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Israel => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("BA819A10-C5EE-4667-B612-A5EB7A06A724", "Israel (IL)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Korea => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("1BDBAC32-3479-46EB-8B70-1FEBE9287AC4", "Korea (KR)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Vietnam => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("076d1bcd-a8c7-4ed4-b000-6b77c6ae7f92", "Vietnam (VN)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_SaudiArabia => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("945E128F-5978-4D98-A4FC-2E914A68AB47", "Saudi Arabia (SA)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Malaysia => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("DD7D210B-14B8-40C7-8C0F-F693D8EFD72B", "Malaysia (MY)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Mexico => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("6875b120-1976-4ef3-85b4-6859fd6d950a", "Mexico (MX)"));
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Romania => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("5af21bad-5eea-4788-8517-dcc56c190bba", "Romania (RO)"));
			public static MultilingualString Accounting_EPaymentConfigurations => CombineCategories(Accounting, ResString.GetMultilingualString("D7085F76-A7E9-4E99-AB94-78ACC348F362", "E-Payment Configurations"));
			public static MultilingualString Accounting_TaxFrameworkConfiguration { get { return CombineCategories(AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ResString.GetMultilingualString("17730EE1-696A-41A8-BB2C-938DD06074B2", "Tax Framework Configuration")); } }
			public static MultilingualString Accounting_GeneralLedgerDefaults_LinkAccount { get { return CombineCategories(Accounting_GeneralLedgerDefaults, ResString.GetMultilingualString("6ca00daf-713e-4998-8663-37f91ff4a565", "Link Account")); } }
			public static MultilingualString Accounting_GeneralLedgerDefaults_LinkAccount_TaxTransaction { get { return CombineCategories(Accounting_GeneralLedgerDefaults_LinkAccount, ResString.GetMultilingualString("3728ce7b-1753-471d-b008-4199470150e5", "Tax Transaction")); } }
			public static MultilingualString Accounting_GeneralLedgerDefaults_ControlAccount { get { return CombineCategories(Accounting_GeneralLedgerDefaults, ResString.GetMultilingualString("F18B89EC-93DE-415c-B24C-59CB7C323899", "Control Account")); } }
			public static MultilingualString Accounting_Temp { get { return CombineCategories(Accounting, ResString.GetMultilingualString("8e92e0c0-a5b3-403e-a352-e1a94fc62c72", "Temp.")); } }
			public static MultilingualString Accounting_SystemCertifications { get { return CombineCategories(Accounting, ResString.GetMultilingualString("5218b078-a3a0-4a46-97e1-2f85767ae8ce", "System Certifications")); } }
			public static MultilingualString Accounting_SystemCertifications_Portugal { get { return CombineCategories(Accounting_SystemCertifications, ResString.GetMultilingualString("e48b104c-0406-4bdd-907e-fc9098b04a78", "Portugal")); } }
			public static MultilingualString Accounting_SystemCertifications_Germany { get { return CombineCategories(Accounting_SystemCertifications, ResString.GetMultilingualString("BB0786B1-0022-4A0C-84B4-BE045970D622", "Germany")); } }
			public static MultilingualString Accounting_SystemCertifications_Israel { get { return CombineCategories(Accounting_SystemCertifications, ResString.GetMultilingualString("5152763D-6E16-4A6C-98A7-270278E138D8", "Israel")); } }
			public static MultilingualString Accounting_EReportingAndEInvoicingConfigurations_Turkey => CombineCategories(Accounting_EReportingAndEInvoicingConfigurations, ResString.GetMultilingualString("44ACF25C-A250-408E-9E25-1E4654A386CC", "Turkey (TR)"));
			public static MultilingualString Accounting_TaxConfigurations_SupplyType { get { return CombineCategories(AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ResString.GetMultilingualString("c277555a-84a6-4042-99ee-6c0e20bd2e3e", "Supply Type")); } }
			public static MultilingualString Accounting_FixedPlaceOfSupplyConfiguration { get { return CombineCategories(Accounting, ResString.GetMultilingualString("CF6EBA1C-AAC5-445F-AC0D-4CA2EB29AD35", "Fixed Place of Supply Configuration")); } }
			public static MultilingualString Accounting_AssetManagement => CombineCategories(Accounting, ResString.GetMultilingualString("075643B8-1AE2-482A-AF93-AA8C5D1841EF", "Asset Management"));
			public static MultilingualString Accounting_ExternalAccountingSystemIntegration => CombineCategories(Accounting, ResString.GetMultilingualString("50545f8c-20e5-4d15-af62-a747a74ce003", "External Accounting System Integration"));
		}

		#endregion

		#region PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations

		public BooleanRegistryItem PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations
		{
			get
			{
				return GetItem("PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations",
						delegate
						{
							return new BooleanRegistryItem("PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations",
								Categories.Accounting_Temp,
								(NoResString)"Prevent deletion of Job Billing Exchange Rate and CFX Configurations (CargoWiseOne Support Only)",
								(NoResString)@"Recently, we had a number of reports from different customers saying that their Job Billing Exchange Rate Configurations (and CFX Configurations) go missing unexpectedly.
When this registry is enabled, the system will prevent deleting Job Billing Exchange Rate Configurations and CFX Configurations (at all levels).
This registry should be enabled only if the customer is experiencing unexplained deletions of Job Billing Exchange Rate and/or CFX Configurations.
The user will be able to delete the configurations only when the user is working on the actual configuration form, i.e. Job Billing Exchange Rate Configuration, Company maintenance, branch maintenance, debtor group, creditor group or organization maintenance.
However, the system will no longer be able to delete these configurations outside these forms. The customer should be informed that some functions may be impacted, such as organization import, merge or de-duplication features.",
								RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
						});
			}
		}

		public BooleanRegistryItem ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations
		{
			get
			{
				return GetItem("ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations",
						delegate
						{
							return new BooleanRegistryItem("ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations",
								Categories.Accounting_Temp,
								(NoResString)"Report Error When Deleting Job Billing Exchange Rate and CFX Configurations (CargoWiseOne Support Only)",
								(NoResString)@"Recently, we had a number of reports from different customers saying that their Job Billing Exchange Rate Configurations (and CFX Configurations) go missing unexpectedly.
When this registry is enabled, the system will generate an error report to us when Job Billing Exchange Rate Configurations and CFX Configurations (at all levels) are deleted.
This registry should be enabled only if the customer is experiencing unexplained deletions of Job Billing Exchange Rate and/or CFX Configurations.",
								RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
						});
			}
		}

		#endregion

		public BooleanRegistryItem AllowTriggersToSkipSaveEverythingBeforePostingValidation
		{
			get
			{
				return GetItem("AllowTriggersToSkipSaveEverythingBeforePostingValidation", () =>
					new BooleanRegistryItem("AllowTriggersToSkipSaveEverythingBeforePostingValidation",
						Categories.Accounting_Temp,
						(NoResString)"Allow Triggers to Skip 'Save Everything Before Posting' Validation (CargoWise Support Only)",
						(NoResString)@"When this registry ticked, triggers are allowed to skip 'save everything before posting' validation.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem TemporaryProductionRuleEngineImportExportFeature
		{
			get
			{
				return GetItem("TemporaryProductionRuleEngineImportExportFeature", delegate
				{
					return new BooleanRegistryItem(
						"TemporaryProductionRuleEngineImportExportFeature",
						Categories.Accounting_Temp,
						(NoResString)"Temporary Production Rule Engine Import Export Feature",
						(NoResString)@"This registry will enable the working-in-progress Production Rule Engine Import Export Feature.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem TemporaryProductionRuleEngineCustomFieldFeature
		{
			get
			{
				return GetItem("TemporaryProductionRuleEngineCustomFieldFeature", delegate
				{
					return new BooleanRegistryItem(
						"TemporaryProductionRuleEngineCustomFieldFeature",
						Categories.Accounting_Temp,
						(NoResString)"Temporary Production Rule Engine Custom Field Feature",
						(NoResString)@"This registry will enable the working-in-progress Production Rule Engine Custom Field Feature.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableInvoiceCurrencyType
		{
			get
			{
				return GetItem("EnableInvoiceCurrencyType", () =>
					new BooleanRegistryItem(
						"EnableInvoiceCurrencyType",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("b8391711-5230-47a8-9f35-9030e1f92de0", "Enable Job Billing Exchange Rate Configuration by Invoice Currency Type"),
						ResString.GetMultilingualString("fd0f3e5b-c8e2-4521-a28c-f29bd0d350a6", @"By default, this registry is set to 'NO'. Overriding this registry and setting it to 'YES' allows you to configure separate exchange rate preferences for invoices issued in foreign currency and invoices issued in local currency.

When enabled, an additional 'Invoice Currency Type' column is available in Job Billing Exchange Rate Configuration and Job Exchange Rates grid. This allows you to select whether each configuration applies to foreign currency invoices only, local currency invoices only or whether it applies to both - foreign and local."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false));
			}
		}

		public BooleanRegistryItem EnableXUTImportAutoMapAccrualFeature
		{
			get
			{
				return GetItem("EnableXUTImportAutoMapAccrualFeature", () =>
					new BooleanRegistryItem(
						"EnableXUTImportAutoMapAccrualFeature",
						Categories.Accounting_Temp,
						(NoResString)"Enable XUT Import Auto Map Accrual Feature",
						(NoResString)"When this registry is enabled, enable the feature to automatically map the Accrual of imported XUT to charges.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem IncludeRelatedJournalsInUniversalXMLTransaction
		{
			get
			{
				return GetItem("IncludeRelatedJournalsInUniversalXMLTransaction", () =>
					new BooleanRegistryItem("IncludeRelatedJournalsInUniversalXMLTransaction",
						Categories.Accounting_Temp,
						(NoResString)"Include Related Journals In Universal XML Transaction (CargoWise Support Only)",
						(NoResString)@"This registry configures the inclusion of Apportionment and Installment Related Journals in Universal Transaction XML.
When overridden and set to yes Then
Apportionment Related Journals linked to the transactions exported via XUT are included in  < PostingJournalApportionamet > section
and
Installment related Journals linked to the transactions exported via XUT are included in < PostingJournalInstallment > section",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem EnableCFXUpliftStartAndExpiryDateColumns
		{
			get
			{
				return GetItem("EnableCFXUpliftStartAndExpiryDateColumns", () =>
					new BooleanRegistryItem("EnableCFXUpliftStartAndExpiryDateColumns",
						Categories.Accounting_Temp,
						(NoResString)"Enable CFX Uplift Start and Expiry Date Columns",
						(NoResString)@"This registry enables the new Start and Expiry Date Columns in all levels of CFX Uplift Configuration.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public MaximumAllowedTransactionAmountRegistryItem SystemDefinedMaximumAllowedTransactionAmount
		{
			get
			{
				return GetItem("SystemDefinedMaximumAllowedTransactionAmount",
					delegate
					{
						var item = new MaximumAllowedTransactionAmountRegistryItem(
									"SystemDefinedMaximumAllowedTransactionAmount",
									Categories.Accounting,
									(NoResString)"System Defined Maximum Allowed Transaction Amount (CargoWise Support Only)",
									(NoResString)@"This registry define the maximum allowed transaction header and line amount acceptable by the system for accounting transactions posting.
Users will be able to specified in a separate user controlled registry 'Maximum allowed transaction amount) on the acceptable amount with reference their company policy.
The values specified by the users  must not be more than the system defined values.

Note:
1. Please consult the accounting product team before overriding these values.
2. The maximum allowed transaction header amount will be used to validate against the header amounts of these transaction types in local currency.
- Receivables and Payables Invoice, Credit Note, Adjustment Note. Journal, Transfer, Contra, Receipt, Payment, Overpayment, Discount and Exchange Difference
- Cash Book Direct Receipt, Direct Payment and Exchange Difference
3. The maximum allowed transaction line amount will be used to validate against the line amounts of these transaction types in local currency.
- Receivables and Payables Invoice, Credit Note, Adjustment Note
- Cash Book Direct Receipt, Direct Payment
- Job Costing Accrual, WIP, Job Revenue Journal and CFX Journal
- General Ledger Journals",
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									RegistryOptions.IsOnlyForSupport,
									GetDefaultSystemDefinedMaximumAllowedTransactionAmount());

						item.OnBuildLogReference += (args) => BuildMaximumAllowedTransactionAmountLogReference(args);

						return item;
					});
			}
		}

		static MaximumAllowedTransactionAmount GetDefaultSystemDefinedMaximumAllowedTransactionAmount()
		{
			var defaultValue = new MaximumAllowedTransactionAmount();
			defaultValue.MaximumAllowedHeaderAmount = 1000000000000M;
			defaultValue.MaximumAllowedLineAmount = 100000000000M;

			return defaultValue;
		}

		public MaximumAllowedTransactionAmountRegistryItem MaximumAllowedTransactionAmount
		{
			get
			{
				return GetItem("MaximumAllowedTransactionAmount",
					delegate
					{
						var item = new MaximumAllowedTransactionAmountRegistryItem(
									"MaximumAllowedTransactionAmount",
									Categories.Accounting,
									ResString.GetMultilingualString("d8165cfb-1411-4222-8afd-4597959b51be", "Maximum Allowed Transaction Amount"),
									ResString.GetMultilingualString("305b03ab-52b0-4fd2-8da9-a5e0e634201e", @"This registry enables you to define the maximum allowed transaction header and line amount acceptable in accordance to you business norms.
This will help you to prevent users from posting invalid accounting transaction.
The maximum header amount must not exceed 1 trillion and the maximum line amount must not exceed 100 billions.

Note:
1. The maximum allowed transaction header amount will be used to validate against the header amounts of these transaction types in local currency.
- Receivables and Payables Invoice, Credit Note, Adjustment Note. Journal, Transfer, Contra, Receipt, Payment, Overpayment, Discount and Exchange Difference
- Cash Book Direct Receipt, Direct Payment and Exchange Difference
2. The maximum allowed transaction line amount will be used to validate against the line amounts of these transaction types in local currency.
- Receivables and Payables Invoice, Credit Note, Adjustment Note
- Cash Book Direct Receipt, Direct Payment
- Job Costing Accrual, WIP, Job Revenue Journal and CFX Journal
- General Ledger Journals"),
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									RegistryOptions.Default,
									true);

						item.OnBuildLogReference += (args) => BuildMaximumAllowedTransactionAmountLogReference(args);

						return item;
					});
			}
		}

		string BuildMaximumAllowedTransactionAmountLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var originalElements = (MaximumAllowedTransactionAmount)args.OriginalValue;
			var newElements = (MaximumAllowedTransactionAmount)args.NewValue;

			if (originalElements.MaximumAllowedHeaderAmount != newElements.MaximumAllowedHeaderAmount)
			{
				result += Res.GetString("e9e5b118-1bc5-42c1-acb5-23dc5a3b73f4", "Maximum Allowed Header Amount has been changed to {0}.", newElements.MaximumAllowedHeaderAmount) + "\r\n";
			}

			if (originalElements.MaximumAllowedLineAmount != newElements.MaximumAllowedLineAmount)
			{
				result += Res.GetString("185e0ca8-91d1-4ee6-9ade-af2141fdf99d", "Maximum Allowed Line Amount has been changed to {0}.", newElements.MaximumAllowedLineAmount);
			}

			return result;
		}

		public CodeDescriptionBoolRegistryItem ExcludeCurrencyFromCFXCalculation
		{
			get
			{
				return GetItem("ExcludeCurrencyFromCFXCalculation", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"ExcludeCurrencyFromCFXCalculation",
						Categories.Accounting,
						(NoResString)"Exclude Currency From CFX Calculation",
						(NoResString)"The stored currency code in this registry will be excluded from CFX calculation ",
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						new CodeDescriptionBoolRegistryEditorInfo(null, false),
						new CodeDescriptionBoolCollection());
				});
			}
		}

		public IntRegistryItem CollectionOrderMinimumAmount
		{
			get
			{
				return GetItem("CollectionOrderMinimumAmount", delegate
				{
					return new IntRegistryItem(
						"CollectionOrderMinimumAmount",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders,
						ResString.GetMultilingualString("14997d1c-dac2-4456-aef9-22b1eee5dd2c", "Collection Order Minimum Amount"),
						ResString.GetMultilingualString("51413d8d-24e7-461a-85cd-356f18578df9", "This registry enables you to define the minimum collection order amount where applicable. When an amount greater than zero is set, users not granted ‘Allow Order Below Minimum Amount’ will not be able to create collection order for amount less than the minimum stipulated amount in this registry"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						0);
				});
			}
		}

		public CodeDescriptionWithGroupRegistryItem ConsolidatedAccountingCategoryList
		{
			get
			{
				var item = GetItem("ConsolidatedAccountingCategory", delegate
				{
					var item = new AccountingCodeDescriptionWithGroupRegistryItem<ConsolidatedAccountingCategoryCollection, ConsolidatedAccountingCategoryItem>(
						"ConsolidatedAccountingCategory",
						RawDataRegistry.Categories.Organizations_CodeLists,
						ResString.GetMultilingualString("cd593642-c277-4c0b-a54e-564bde4abe30", "Consolidated Accounting Category List"),
						ResString.GetMultilingualString("7eb6d84d-0c96-4f3d-979c-8600645f9b4f", "The list of valid entries for Consolidated Accounting Category"),
						//If RegistryStorageFlags is extened to System & Company level, do not forget to also update ConsolidatedAccountingCategoryItem.IsCodeInUse() to do validation for each company.
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionWithGroupRegistryEditorInfo(ConsolidatedAccountingCategoryCollection.GetGroupColumnCaption()),
						ConsolidatedAccountingCategoryCollection.GetDefaultValue(),
						ConsolidatedAccountingCategoryCollection.Converter
					);

					item.OnBuildLogReference += BuildConsolidatedAccountingCategoryListLogReference;
					item.OnUpdateAction += ConsolidatedAccountingCategoryList_OnUpdateAction;

					return item;
				});
				item.Options = DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default;
				return item;
			}
		}

		void ConsolidatedAccountingCategoryList_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			((ConsolidatedAccountingCategoryCollection)newValue).OnUpdateAction();
		}

		string BuildConsolidatedAccountingCategoryListLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var originValue = ((CodeDescriptionWithGroupCollection)args.OriginalValue).Cast<CodeDescriptionWithGroup>();
			var newValue = ((CodeDescriptionWithGroupCollection)args.NewValue).Cast<CodeDescriptionWithGroup>();

			var comparer = new CodeDescriptionWithGroupEqualityComparer();
			var result = new StringBuilder();

			foreach (var element in originValue.Except(newValue, comparer))
			{
				result.AppendLine(Res.GetString("06B3911D-73A8-4B22-928E-E3EF88F27A51", "Deleted: Code=[{0}], Description=[{1}], Class=[{2}]", element.Code, element.Description, element.Group));
			}

			foreach (var element in newValue.Except(originValue, comparer))
			{
				result.AppendLine(Res.GetString("BDFF0D4C-7700-4247-B7FB-330F7AD44D85", "Added: Code=[{0}], Description=[{1}], Class=[{2}]", element.Code, element.Description, element.Group));
			}

			return result.ToString();
		}

		internal class CodeDescriptionWithGroupEqualityComparer : IEqualityComparer<CodeDescriptionWithGroup>
		{
			bool IEqualityComparer<CodeDescriptionWithGroup>.Equals(CodeDescriptionWithGroup x, CodeDescriptionWithGroup y) => x.Code == y.Code && x.Description?.ToString() == y.Description?.ToString() && x.Group == y.Group;
			int IEqualityComparer<CodeDescriptionWithGroup>.GetHashCode(CodeDescriptionWithGroup obj) => 0;
		}

		public BooleanRegistryItem UseCusClearPortAsHomeCntryForTaxOvrds
		{
			get
			{
				return GetItem("UseCusClearPortAsHomeCntryForTaxOvrds", () =>
					new BooleanRegistryItem("UseCusClearPortAsHomeCntryForTaxOvrds",
						Categories.Accounting,
						(NoResString)"Use Customs Port For Tax Overrides",
						(NoResString)"Use Customs Port Of Clearance In Place Of Debtor Or Creditor Home Country For Shipment and Declaration Jobs, for Tax Overrides (For Developers Only). This is a temporary hack until we can add Fixed Place Of Supply Rules properly.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						true));
			}
		}

		public BooleanRegistryItem UseBrokerageLocationsForBilling
		{
			get
			{
				return GetItem("UseBrokerageLocationsForBilling", () =>
					new BooleanRegistryItem("UseBrokerageLocationsForBilling",
						Categories.Accounting,
						(NoResString)"Use Brokerage Locations For Auto Billing",
						(NoResString)"Use Brokerage Locations (Origin, Destination etc.) for auto-billing.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem EnableNetting
		{
			get
			{
				return GetItem("EnableNetting", () => new BooleanRegistryItem(
					"EnableNetting",
					Categories.Accounting_Netting,
					(NoResString)"Enable Netting (CargoWiseOne Support only)",
					(NoResString)@"This registry controls the access of netting functionality.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public BooleanRegistryItem AllowRePrintingOfInvoicesAndCreditNotes
		{
			get
			{
				return GetItem("AllowRePrintingOfInvoicesAndCreditNotes", delegate
				{
					return new BooleanRegistryItem(
						"AllowRePrintingOfInvoicesAndCreditNotes",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("d5e27dda-2f55-48be-b296-befdaffec5e9", "Allow re-printing of invoices and credit notes"),
						ResString.GetMultilingualString("788a7aea-4ba5-4086-875c-e8a5daee61bf", @"This registry controls the ability to re-print AR Invoices and Credit Notes that have already been printed / delivered.

Setting this registry to to 'No' will prevent all users in your login company from re-printing receivables invoices and credit notes and will redirect them to the eDocs tab of a transaction to open the saved version of the transaction and re-print it."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem ValidateTaxIDApplicationForExporterExemptionItaly
		{
			get
			{
				return GetItem("ValidateTaxIDApplicationForExporterExemptionItaly",
					delegate
					{
						var item = new BooleanRegistryItem("ValidateTaxIDApplicationForExporterExemptionItaly",
							Categories.Accounting_ReceivableDefaults_DefaultSettings,
							ResString.GetMultilingualString("5C709245-FAED-468F-A4E1-66B0E11F2790", "Validate Tax ID Application for Exporter Exemption (Italy)"),
							ResString.GetMultilingualString("D67CD937-520C-40FF-B04C-36BC7FF759F0", @"This registry controls whether a Debtor's Exporter Exemption Certificate and its Ceiling Limit prevents posting of transactions if they contain certain Tax IDs. This registry only applies for Italy login companies.

When enabled, the registry affects posting of new Invoice, Credit Note or Adjustment transactions and restricts the Tax IDs permitted in those transactions as described below.

The DICH.INT Tax ID is not permitted unless the Debtor has recorded a valid Exporter Exemption certificate and the cumulative DICH.INT charges are less than the certificate's Ceiling Limit.

Any Tax ID with a rate greater than zero is not permitted if the Debtor has recorded a valid Exporter Exemption certificate and the cumulative DICH.INT charges are less than the certificate's Ceiling Limit.

If these restrictions are not required, please set this registry to No. If the registry is disabled, the DICH.INT Tax ID can be used for any Debtor, and any Tax ID can be used for a Debtor with an Exporter Exemption certificate with remaining limit."),
							RegistryStorageFlags.Company,
							true);
						item.OnBuildLogReference += (args) => Res.GetString("101ca9e1-1cf4-4f38-ac93-73c4cbbe28ef", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
						item.CountryFilterPKs = CountryFilterPKs.Italy;
						return item;
					});
			}
		}

		#region Reason Codes List

		public CodeDescriptionPairListRegistryItem PaymentRejectionReasonCodesList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(Constants.PaymentsRejectionReason.Codes.InvoiceChargesDisputed, Constants.PaymentsRejectionReason.Descriptions.InvoiceChargesDisputed);
				lookUpList.AddPair(Constants.PaymentsRejectionReason.Codes.InsufficientFunds, Constants.PaymentsRejectionReason.Descriptions.InsufficientFunds);
				lookUpList.AddPair(Constants.PaymentsRejectionReason.Codes.IncorrectPaymentDetails, Constants.PaymentsRejectionReason.Descriptions.IncorrectPaymentDetails);
				lookUpList.AddPair(Constants.PaymentsRejectionReason.Codes.IncorrectPaymentAllocation, Constants.PaymentsRejectionReason.Descriptions.IncorrectPaymentAllocation);

				var key = nameof(PaymentRejectionReasonCodesList);
				return GetItem(key, () =>
				{
					return new CodeDescriptionPairListRegistryItem(
						key,
						Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing,
						ResString.GetMultilingualString("1CB25228-79A0-4F42-B569-57E43E146AF1", "Payment Request Rejection Reason Codes"),
						ResString.GetMultilingualString("7CC2CD55-8895-4086-8CF8-FE7337C61D62", @"The Reason Codes listed in this registry are used to classify the reason a Payment Request is being rejected.
A reason code is mandatory when rejecting AR and AP payment requests.
Reason codes are visible in the AR/AP Payment Processing module, on the Payment event logs, and are included in the Notification Email sent back to the requester."),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						lookUpList
						);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem CreditNoteReasonCodesList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled, Constants.GenApprovalRequestReasonCode.Description.IncorrectOrganisationBilled);
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.IncorrectRating, Constants.GenApprovalRequestReasonCode.Description.IncorrectRating);
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.IncorrectCharges, Constants.GenApprovalRequestReasonCode.Description.IncorrectCharges);
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails, Constants.GenApprovalRequestReasonCode.Description.IncorrectJobDetails);
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.IncorrectDate, Constants.GenApprovalRequestReasonCode.Description.IncorrectDate);
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.DamagedGoods, Constants.GenApprovalRequestReasonCode.Description.DamagedGoods);
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.Discount, Constants.GenApprovalRequestReasonCode.Description.Discount);
				lookUpList.AddPair(Constants.GenApprovalRequestReasonCode.Code.LateDelivery, Constants.GenApprovalRequestReasonCode.Description.LateDelivery);

				return GetItem("ReasonCodesListForCreditNoteOfTransactions", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ReasonCodesListForCreditNoteOfTransactions",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting,
						ResString.GetMultilingualString("0a94b91c-4da6-40aa-8302-239d622ea7a9", "Credit Note Approval Reason Codes"),
						ResString.GetMultilingualString("8bdcd1c7-005a-47c0-8f97-9902251da257", @"The Reason Codes listed here are used to classify the reason an Accounts Receivable credit note is being raised.
A reason code is mandatory on credit note approval request.
Reason codes are visible in the Credit Notes Approval Module"),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						lookUpList
						);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem AmendmentReasonCodesList
		{
			get
			{
				CodeDescriptionPairList allDefaultValues = new CodeDescriptionPairList();
				allDefaultValues.AddPair("IDE", ResString.GetMultilingualString("f83a555d-08e1-48e6-9d56-f955e94fccaa", "Incorrect Data Entry"));
				allDefaultValues.AddPair("IAM", ResString.GetMultilingualString("9f63c034-8ae4-463c-b7e1-71ad31f25362", "Incorrect Amounts"));
				allDefaultValues.AddPair("TXT", ResString.GetMultilingualString("3b7e947a-2166-47b2-9e62-6974cf09f686", "Free Text"));
				allDefaultValues.AddPair("MIR", ResString.GetMultilingualString("D104EE7C-ACF1-4A1D-843D-0D116719B587", "{0} Original Invoice Rejected by the Buyer", "MiPyme"));

				MultilingualString[] categories = { Categories.Accounting_ReceivableDefaults_DefaultSettings };
				CodeDescriptionPairListRegistryItem item = GetItem("ReasonCodesListForAmendmentOfTransactions", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue("ReasonCodesListForAmendmentOfTransactions",
						categories,
						ResString.GetMultilingualString("3e0989f4-215d-4a3f-9759-30a3c0451d05", "Transaction Amendment Reason Codes"),
						ResString.GetMultilingualString("a850e138-adab-460a-81cf-0f69963e06f2", @"Transaction Amendment Reason Codes.
In Receivable Credit Note, only the reason description with code {0} will be editable.", ReasonFreeTextCode.Code),
						new CodeDescriptionPairListRegistryDataType(3),
						new CodeDescriptionPairListEditorInfo(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AmendmentReasonCodesDefaultValueGetter),
						true,
						allDefaultValues);
				});

				return item;
			}
		}

		static CodeDescriptionPairList AmendmentReasonCodesDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var countryCode = string.Empty;
			var company = (BusinessObject)new BusinessObjectFactory().Load<IGlbCompany>(companyPK);
			if (company != null)
			{
				countryCode = (ZString)company[GlbCompanySchema.GC_RN_NKCountryCode];
			}

			var list = new CodeDescriptionPairList();
			list.AddPair("IDE", ResString.GetMultilingualString("f83a555d-08e1-48e6-9d56-f955e94fccaa", "Incorrect Data Entry"));
			list.AddPair("IAM", ResString.GetMultilingualString("9f63c034-8ae4-463c-b7e1-71ad31f25362", "Incorrect Amounts"));
			list.AddPair(ReasonFreeTextCode.Code, ReasonFreeTextCode.Description);

			if (countryCode == Core.Constants.CountryCodes.Argentina)
			{
				list.AddPair("MIR", ResString.GetMultilingualString("D104EE7C-ACF1-4A1D-843D-0D116719B587", "{0} Original Invoice Rejected by the Buyer", "MiPyme"));
			}

			return list;
		}

		public CodeDescriptionPairListRegistryItem ReversalReasonCodesList
		{
			get
			{
				CodeDescriptionPairList allDefaultValues = new CodeDescriptionPairList();
				allDefaultValues.AddPair("IDE", ResString.GetMultilingualString("e8738cef-c501-4261-bb09-7d8524625108", "Incorrect Data Entry"));
				allDefaultValues.AddPair("WOR", ResString.GetMultilingualString("9b9b5e28-343f-4e9f-a20c-cd53c5bc8419", "Wrong Organization Code Used"));
				allDefaultValues.AddPair("IAM", ResString.GetMultilingualString("dbec9e73-65dd-41fe-9c83-264cbefe72c7", "Incorrect Amounts"));
				allDefaultValues.AddPair("TXT", ResString.GetMultilingualString("b7a6153c-e4ab-405f-aab0-316756799399", "Free Text"));
				allDefaultValues.AddPair("MIR", ResString.GetMultilingualString("D104EE7C-ACF1-4A1D-843D-0D116719B587", "{0} Original Invoice Rejected by the Buyer", "MiPyme"));

				MultilingualString[] categories = { Categories.Accounting };
				CodeDescriptionPairListRegistryItem item = GetItem("ReasonCodesListForReversalOfTransactions", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue("ReasonCodesListForReversalOfTransactions",
						categories,
						ResString.GetMultilingualString("bbf6651b-cd21-4059-a312-47aa6e3e217f", "Transaction Reversal Reason Codes"),
						ResString.GetMultilingualString("8a347b49-9b71-444d-b7b3-44d6195a14d0", @"Transaction Reversal Reason Codes."),
						new CodeDescriptionPairListRegistryDataType(3),
						new CodeDescriptionPairListEditorInfo(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ReversalReasonCodesDefaultValueGetter),
						true,
						allDefaultValues
					);
				});

				return item;
			}
		}

		static CodeDescriptionPairList ReversalReasonCodesDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var countryCode = string.Empty;
			var company = (BusinessObject)new BusinessObjectFactory().Load<IGlbCompany>(companyPK);
			if (company != null)
			{
				countryCode = (ZString)company[GlbCompanySchema.GC_RN_NKCountryCode];
			}

			var list = new CodeDescriptionPairList();
			list.AddPair("IDE", ResString.GetMultilingualString("e8738cef-c501-4261-bb09-7d8524625108", "Incorrect Data Entry"));
			list.AddPair("WOR", ResString.GetMultilingualString("9b9b5e28-343f-4e9f-a20c-cd53c5bc8419", "Wrong Organization Code Used"));
			list.AddPair("IAM", ResString.GetMultilingualString("dbec9e73-65dd-41fe-9c83-264cbefe72c7", "Incorrect Amounts"));
			list.AddPair(ReasonFreeTextCode.Code, ReasonFreeTextCode.Description);

			if (countryCode == Core.Constants.CountryCodes.Argentina)
			{
				list.AddPair("MIR", ResString.GetMultilingualString("D104EE7C-ACF1-4A1D-843D-0D116719B587", "{0} Original Invoice Rejected by the Buyer", "MiPyme"));
			}
			return list;
		}

		#endregion

		#region Tax Configurations

		#region Tax Framework Configuration

		public TaxAuthoritiesRegistryItem TaxAuthorities
		{
			get
			{
				return GetItem("TaxAuthorities",
					delegate
					{
						return new TaxAuthoritiesRegistryItem("TaxAuthorities",
							Categories.Accounting_TaxFrameworkConfiguration,
							(NoResString)"Tax Authorities (CargoWiseOne Support Only)",
							(NoResString)@"DO NOT OVERRIDE IN CUSTOMER DATABASES.
Supported Tax Framework Tax Authorities are shown in this registry.
Overriding this registry should never be required in customer systems.
Please discuss with the Accounting Compliance Product team if you think changes are needed to meet customer needs.
The registry is for Accounting Product development purposes only.
The registry is used by the Accounting Product team in WTG test systems when determining the set of Tax Authorities to be supported in our code base and deployed to all customer systems.",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							getDefaultValue());
					});

				TaxAuthoritiesConfigurationCollection getDefaultValue()
				{
					var result = new TaxAuthoritiesConfigurationCollection();
					var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();
					foreach (var country in AccountingMasterFilesTaxFrameworkConstants.CountriesWithDefaultTaxFrameworkConfiguration)
					{
						var taxAuthoritiesConfigurations = ((IInstanceProvider<ITaxFrameworkConfigurationDefaults>)globalAccountingCountryFactory.GetCountryFactory(country)).Get().GetTaxAuthorities();
						result.AddRange(taxAuthoritiesConfigurations);
					}

					return result;
				}
			}
		}

		public TaxSystemsRegistryItem TaxSystems
		{
			get
			{
				return GetItem("TaxSystems",
					delegate
					{
						return new TaxSystemsRegistryItem("TaxSystems",
							Categories.Accounting_TaxFrameworkConfiguration,
							(NoResString)"Tax Systems (CargoWiseOne Support Only)",
							(NoResString)@"DO NOT OVERRIDE IN CUSTOMER DATABASES.
Supported Tax Framework Tax Systems are shown in this registry. 
Overriding this registry should never be required in customer systems.
Please discuss with the Accounting Compliance Product team if you think changes are needed to meet customer needs.
The registry is for Accounting Product development purposes only. 
The registry is used by the Accounting Product team in WTG test systems when determining the set of Tax Systems to be supported in our code base and deployed to all customer systems.",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							getDefaultValue());
					});

				TaxSystemsConfigurationCollection getDefaultValue()
				{
					var result = new TaxSystemsConfigurationCollection();
					var globalAccountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>();
					foreach (var country in AccountingMasterFilesTaxFrameworkConstants.CountriesWithDefaultTaxFrameworkConfiguration)
					{
						var taxSystemsConfigurations = ((IInstanceProvider<ITaxFrameworkConfigurationDefaults>)globalAccountingCountryFactory.GetCountryFactory(country)).Get().GetTaxSystems();
						result.AddRange(taxSystemsConfigurations);
					}
					return result;
				}
			}
		}

		#endregion

		#region Supply Type

		public BooleanRegistryItem EnableSupplyTypeClassificationCodes
		{
			get
			{
				return GetItem("EnableSupplyTypeClassificationCodes",
					delegate
					{
						var item = new BooleanRegistryItem("EnableSupplyTypeClassificationCodes",
							Categories.Accounting_TaxConfigurations_SupplyType,
							ResString.GetMultilingualString("95289B30-71AC-48B1-B4D0-DD4F42391426", "Enable Supply Type Classification Codes"),
							ResString.GetMultilingualString("C7CC33BC-35C6-42B1-AD38-5C5F7E6891DB", @"Supply Type Classification Codes are additional, secondary item of information used to classify each revenue or cost charge line.

By default, this registry is set to 'No' and this feature is not enabled.
When this registry is set to 'Yes', users will be able to assign a supply type to revenue and cost charge line prior to posting."),
							RegistryStorageFlags.Company,
							false);
						item.OnBuildLogReference += (args) => Res.GetString("101ca9e1-1cf4-4f38-ac93-73c4cbbe28ef", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

						return item;
					});
			}
		}

		public BooleanRegistryItem EnableSellComplianceDescription
		{
			get
			{
				var item = GetItem("EnableSellComplianceDescription",
					delegate
					{
						var item = new BooleanRegistryItem("EnableSellComplianceDescription",
							Categories.Accounting_TaxConfigurations_SupplyType,
							(NoResString)"Enable Sell Compliance Description",
							(NoResString)@"Sell Compliance Description is an additional piece of information used to add meaningful descriptions to AR Invoice charge lines.
By default, this registry is set to 'No' and this feature is not enabled.

When overridden and set to 'Yes',
Sell Compliance Descriptions can be defined against each Charge Code,
and when posting Job Related Sell charges, the relevant Sell Compliance Description will be included as part of the Posted Charge Line's Description.

These features are relevant in companies where Receivables Invoice charge line descriptions must include information about the supply type of the posted charge.",
							RegistryStorageFlags.Company,
							RegistryOptions.IsHidden,
							false);
						item.OnBuildLogReference += (args) => Res.GetString("536A718F-BB53-4C8A-8FAC-625194CB18E4", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

						return item;
					});
				item.Options = EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		public BooleanRegistryItem SupplyTypeClassificationCodesIsMandatory
		{
			get
			{
				var item = GetItem("SupplyTypeClassificationCodesIsMandatory",
					delegate
					{
						var item = new BooleanRegistryItem("SupplyTypeClassificationCodesIsMandatory",
							Categories.Accounting_TaxConfigurations_SupplyType,
							ResString.GetMultilingualString("9b1bb12b-7651-4801-86f4-0694e4936991", "Supply Type Classification Code is Mandatory"),
							ResString.GetMultilingualString("25e3ec46-aa17-4853-b784-02a8c2394419", @"This registry allows you to enforce that a supply type must be assigned to each revenue and cost charge line.

By default, this registry is set to ‘No’ and the recording of supply type is optional.
When this registry is set to ‘Yes’, a validation error will be shown if supply type is not recorded when
1. posting receivables and payables transactions in operational modules.
2. creating and posting of receivables, payables and cash book transactions in the respective accounting modules."),
							RegistryStorageFlags.Company,
							RegistryOptions.IsHidden,
							false);
						item.OnBuildLogReference += (args) => Res.GetString("dce22170-154d-4e0e-b3c8-65a922415367", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

						return item;
					});
				item.Options = EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem SupplyTypeClassificationCodesList
		{
			get
			{
				var item = GetItem("SupplyTypeClassificationCodesList",
					delegate
					{
						var item = new CodeDescriptionBoolDisallowNewRegistryItem("SupplyTypeClassificationCodesList",
							Categories.Accounting_TaxConfigurations_SupplyType,
							ResString.GetMultilingualString("a921f421-4098-424e-a3f8-aa9183a630ff", "Supply Type Classification Codes List"),
							ResString.GetMultilingualString("32cb5936-88b6-4061-927d-d6c574245fd9", @"This list the valid supply type classification codes that can be used to assign to revenue and cost charge line.

You will not be allowed to add new code or delete existing code.
You will be allowed to mark code as inactive to disallow it from being used."),
							RegistryStorageFlags.Company,
							RegistryOptions.IsHidden,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("586b4dbf-2c93-434f-884a-a5133d22c534", "Active"), null, true, true, true),
							GetSupplyTypeClassificationCodesList()
							);
						item.OnBuildLogReference += (args) => BuildSupplyTypeClassificationCodesListLogReference(args);

						return item;
					});
				item.Options = EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		string BuildSupplyTypeClassificationCodesListLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var originalElements = ((CodeDescriptionBoolDisallowNewCollection)args.OriginalValue).Cast<CodeDescriptionBool>();
			var newElements = ((CodeDescriptionBoolDisallowNewCollection)args.NewValue).Cast<CodeDescriptionBool>();

			foreach (var originalElement in originalElements)
			{
				var updatedElement = newElements.FirstOrDefault(x => x.Code == originalElement.Code && x.Bool != originalElement.Bool);
				if (updatedElement != null)
				{
					result += Res.GetString("2abfac1a-db68-4e7a-a392-c15fab5270fa", "{0} code has been marked as {1}.", updatedElement.Code, updatedElement.Bool ? (NoResString)"active" : (NoResString)"inactive") + "\r\n";
				}
			}

			return result;
		}

		CodeDescriptionBoolDisallowNewCollection GetSupplyTypeClassificationCodesList()
		{
			var result = new CodeDescriptionBoolDisallowNewCollection(SupplyTypeClassificationList);
			result.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = true);

			return result;
		}

		#endregion

		public GuidRegistryItem RevenueTaxExpenseRecoveryChargeCode
		{
			get
			{
				return GetItem("RevenueTaxExpenseRecoveryChargeCode", () => new GuidRegistryItem(
					"RevenueTaxExpenseRecoveryChargeCode",
					AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
					ResString.GetMultilingualString("567E04DB-B859-4BA0-868F-40390BB87BEB", "Revenue Tax Expense Recovery Charge Code"),
					ResString.GetMultilingualString("C2D2CED8-7922-49CE-A6A8-0B555BE95EA7",
@"This registry is used when a Login Company has one or more Receivables Turnover Tax Expense Configurations and has a business practice of recovering Turnover Tax Expenses incurred from Receivables Organizations by adding additional Revenue charge lines to an invoice as they are posted. 
The charge code defined against this registry is used when a Turnover Tax Expense Recovery Revenue Charge Line is automatically added to Receivables Invoice and Credit Note transactions as they are posted. 
Note:  Only Charge Code with the Charge Type 'REV - Revenue' can be used with this registry."),
					new TaxExpenseRecoveryChargeCodeGuidRegistryDataType(),
					RegistryStorageFlags.Company,
					Guid.Empty)
				{
					EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.RevenueChargeCode)
				}
				);
			}
		}

		public DateTimeRegistryItem EnableNewTurkeyARComplianceFeaturesFrom
		{
			get
			{
				return GetItem("EnableNewTurkeyARComplianceFeaturesFrom", () => new DateTimeRegistryItem("EnableNewTurkeyARComplianceFeaturesFrom",
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey,
					(NoResString)"Enable New Turkey AR Compliance Features from",
					(NoResString)@"If the current date is greater than or equal to value in this registry, then enable the New Turkey AR Compliance Features.

If the current date is less than the value in this registry, the new features will not be available.

Use:
1. During Testing, WTG staff can change this registry to today and test the features
2. Once all the functionality is checked in and we are ready for the world to know what we did, we will remove the this registry Item and the features will be available as part of the standard installation.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					DateTime.MinValue));
			}
		}

		public bool EnableNewTurkeyARComplianceFeatures
		{
			get
			{
				var enableNewTurkeyARComplianceFeaturesFrom = new ZDateTime(EnableNewTurkeyARComplianceFeaturesFrom.Value);
				return !enableNewTurkeyARComplianceFeaturesFrom.IsEmpty && ZDateTime.Now >= enableNewTurkeyARComplianceFeaturesFrom;
			}
		}

		public DateTimeRegistryItem EnableNewTurkeyAPComplianceFeaturesFrom
		{
			get
			{
				return GetItem("EnableNewTurkeyAPComplianceFeaturesFrom", () => new DateTimeRegistryItem("EnableNewTurkeyAPComplianceFeaturesFrom",
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey,
					(NoResString)"Enable New Turkey AP Compliance Features from",
					(NoResString)@"If the current date is greater than or equal to value in this registry, then enable the New Turkey AP Compliance Features.

If the current date is less than the value in this registry, the new features will not be available.

Use:
1. During Testing, WTG staff can change this registry to today and test the features
2. Once all the functionality is checked in and we are ready for the world to know what we did, we will remove the this registry Item and the features will be available as part of the standard installation.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					DateTime.MinValue));
			}
		}

		public bool EnableNewTurkeyAPComplianceFeatures
		{
			get
			{
				var enableNewTurkeyAPComplianceFeaturesFrom = new ZDateTime(EnableNewTurkeyAPComplianceFeaturesFrom.Value);
				return !enableNewTurkeyAPComplianceFeaturesFrom.IsEmpty && ZDateTime.Now >= enableNewTurkeyAPComplianceFeaturesFrom;
			}
		}

		public IntRegistryItem TaxInvoiceStatusUpdateAutomatedRequestSchedule
		{
			get
			{
				var item = GetItem("TaxInvoiceStatusUpdateAutomatedRequestSchedule", delegate
				{
					var result = new IntRegistryItem(
						"TaxInvoiceStatusUpdateAutomatedRequestSchedule",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("14EAB016-F3CD-4946-8B64-14718D06FDCD", "Tax Invoice Status Update Automated Request Schedule"),
						ResString.GetMultilingualString("69314F3F-1819-4B1C-B770-A0943463B05D", @"This registry item is referenced by Turkey login companies only.
When electronic invoicing is enabled for a Turkey login company, CW1 will automatically send status requests for eligible receivables tax invoices to {0}. This will {1} for any change to the current status of the electronic tax invoices. 
Only receivables tax invoices that do not have a final status of success or canceled will automatically have requests sent to {0} for the transaction status update.
This registry setting allows the login company to set the minimum time interval (in minutes) between automated requests being made to {0} for a status update on any receivables tax invoice.
In addition, where users require an even more recent status, they can also separately request a status update of a receivables tax invoice from the Receivables > Receivables Transactions module.", "Uyumsoft", "check"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						60,
						60,
						1440);
					result.CountryFilterPKs = CountryFilterPKs.Turkey;
					return result;
				});
				item.Options = EnableNewTurkeyARComplianceFeatures ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		public BooleanRegistryItem SimulateGBOutOfEU
		{
			get
			{
				return GetItem("SimulateGBOutOfEU", () => new BooleanRegistryItem(
					"SimulateGBOutOfEU",
					AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
					ResString.GetMultilingualString("8084DCD6-4F49-49FC-B423-F1E73EEF1E91", "Testing Only – GB no longer included in the EU VAT area"),
					ResString.GetMultilingualString("DB92BCED-2303-4903-BF39-C7DEFE7D7575", @"Only use in your TEST environment – do not use in your Production environment.

Nominate whether to effect behaviors to mimic GB (United Kingdom) having exited the EU (European Union).
While this field set as Off, your test system will operate with GB continuing as part of the EU for VAT purposes.

When set On, you can test data entry to verify the effect of GB not being part of the EU for VAT purposes.
This behavior can be checked for Receivables (AR) and Payables (AP)  INV/CRN/ADJ as well as Cashbook (DRC/DPY) data entry.

Once GB actually leaves the EU, the value of this registry will be ignored."),
					RegistryStorageFlags.Company,
					false
					));
			}
		}

		public IntRegistryItem APListAutomatedRequestSchedule
		{
			get
			{
				var item = GetItem("APListAutomatedRequestSchedule", delegate
				{
					var result = new IntRegistryItem(
						"APListAutomatedRequestSchedule",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey,
						ResString.GetMultilingualString("F5E285C1-FF10-4FD1-82A9-CB476C56A390", "AP List Automated Request Schedule"),
						ResString.GetMultilingualString("141A0AC1-6236-46DB-8F94-64A6F79A6E50", @"This registry is currently only referenced by Turkey login companies.
When electronic invoicing is enabled for a Turkey login company, CW1 will automatically send requests for eligible payables tax invoices to {0}. This will {1} for any existing payables tax invoices.
This registry setting allows the login company to set the minimum time interval (in minutes) between automated requests being made to {0} for a list request on payables tax invoices.", "Uyumsoft", "check"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						60,
						60,
						1440);
					result.CountryFilterPKs = CountryFilterPKs.Turkey;
					return result;
				});
				item.Options = EnableNewTurkeyAPComplianceFeatures ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		#endregion

		#region Fixed Place of Supply Configuration

		public BooleanRegistryItem EnableBranchLevelTaxOverrideRuleConfigurations
		{
			get
			{
				return GetItem("EnableBranchLevelTaxOverrideRuleConfigurations", () => new BooleanRegistryItem(
					"EnableBranchLevelTaxOverrideRuleConfigurations",
					Categories.Accounting_FixedPlaceOfSupplyConfiguration,
					ResString.GetMultilingualString("4084101E-9530-40ED-8C02-7DA3763DB46F", "Enable Branch Level Place of Supply and Tax Override Rule Configurations"),
					ResString.GetMultilingualString("013D375A-CF7E-463B-85A6-F337F15EAE8A", @"Use this registry to extend Place of Supply and Tax Override Group configurations to allow Branch specific rules.

When set to 'No', Place of Supply and Tax Override Group rules are Country level configurations.

Most countries have a single, country-level tax system where all branches within a company record tax under a single, shared tax registration.

When set to 'Yes', Place of Supply and Tax Override Groups will permit configuration of 'Branch' specific rules.
Branch specific rules are only relevant when a Login Company is operating in a country where different branches within the same company have separate registrations, and where different tax defaulting rules, taxes and obligations apply in different parts of a country.

Note: When disabling this registry after it was overridden, Place of Supply and Tax Override Configuration that used Branch Level rules would become invalid. You would need to review your Place of Supply and Tax Override Configurations and remove any invalid rules."),
					RegistryStorageFlags.Company,
					false));
			}
		}

		public CodeDescriptionBoolRegistryItem FixedPlaceOfSupplyConfiguration
			=> GetItem(nameof(FixedPlaceOfSupplyConfiguration),
				() => new CodeDescriptionBoolRegistryItem(
						nameof(FixedPlaceOfSupplyConfiguration),
						Categories.Accounting_FixedPlaceOfSupplyConfiguration,
						ResString.GetMultilingualString("3a7e17df-b631-4c32-b0fd-0ab485f71829", "Fixed Place of Supply Type Configuration"),
						ResString.GetMultilingualString("30dc2663-7ffd-4788-8bdd-5a414d4f0450", "Configure the applicable types for Fixed Place of Supply for this company. The system will only allow users to select Fixed Place of Supply that belong to the types you select in this registry."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("9430166d-5e9f-41a9-8759-020b94388b25", "Enable Fixed Place of Supply Type"), true, true),
						FixedPlaceOfSupplyTypeList()
				)
			);

		static CodeDescriptionBoolCollection FixedPlaceOfSupplyTypeList()
			=> new CodeDescriptionBoolCollection()
			{
				{ PlaceOfSupplyTypes.State.Code,          ResString.GetMultilingualString("ec5d7abf-8970-4cac-bbf9-ddad91ef0d70", "A State/Province in Login Country/Region"), false },
				{ PlaceOfSupplyTypes.TaxZone.Code,        ResString.GetMultilingualString("23ce7bee-d764-44cf-becf-21da51e94be5", "An International Tax Zone"), false },
				{ PlaceOfSupplyTypes.Country.Code,        ResString.GetMultilingualString("746f9b7b-89a5-45a0-b8f7-1784d777b361", "A Country/Region"), false },
				{ PlaceOfSupplyTypes.PredefinedRule.Code, ResString.GetMultilingualString("8b78d1cd-2871-4e95-85d6-7e50d217416d", "A Predefined Rule"), false },
			};

		public CodePairRegistryItem ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules
		{
			get
			{
				return GetItem("ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules", delegate
				{
					return new CodePairRegistryItem(
						"ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules",
						Categories.Accounting_FixedPlaceOfSupplyConfiguration,
						ResString.GetMultilingualString("12F4C7BD-5A50-4E25-AB3D-81D7A786A5BE", "Configure default place of supply when no matching rules"),
						ResString.GetMultilingualString("11F0CC76-4BB3-4C12-83B9-A0853D7FA7D9", @"If you have enabled the Fixed Place of Supply Configuration in your login company, then use this registry to specify the default place of supply when there is no matching rule for the combination of the charge code/charge type /job type/direction."),
						GetOptionsListProvider(),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode);
				});

				ICodeDescriptionPairListProvider GetOptionsListProvider()
				{
					return new CodeDescriptionPairListProvider(() =>
					{
						var options = new CodeDescriptionPairList();
						options.Add(ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Default);
						options.Add(ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Blank);
						return options;
					});
				}
			}
		}

		public CodePairRegistryItem ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation
		{
			get
			{
				return GetItem("ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation", delegate
				{
					return new CodePairRegistryItem(
						"ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation",
						Categories.Accounting_FixedPlaceOfSupplyConfiguration,
						ResString.GetMultilingualString("c4e4895e-43f3-474f-97f3-3f436e89cac7", "Configure default place of supply configuration fallback"),
						ResString.GetMultilingualString("5ce8bc99-cc08-4fb1-a431-0ff38d501666", @"If you have enabled the Fixed Place of Supply Configuration in your login company, then use this registry to specify the default place of supply when there is a matching rule for the combination of the charge code/charge type /job type/direction but there is missing location attribute for which the rule is looking for."),
						GetOptionsListProvider(),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode);
				});

				ICodeDescriptionPairListProvider GetOptionsListProvider()
				{
					return new CodeDescriptionPairListProvider(() =>
					{
						var options = new CodeDescriptionPairList();
						options.Add(ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Default);
						options.Add(ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Blank);
						return options;
					});
				}
			}
		}

		public sealed class ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions
		{
			public static CodeDescriptionPair Default { get { return new CodeDescriptionPair(DefaultCode, ResString.GetMultilingualString("4AACC127-6282-4123-9D89-0D31B09A4D76", "Bill to Party Location")); } }
			public const string DefaultCode = "DEF";

			public static CodeDescriptionPair Blank { get { return new CodeDescriptionPair(BlankCode, ResString.GetMultilingualString("11E71785-533E-4F56-85C7-8581C088AB80", "Leave it empty")); } }
			public const string BlankCode = "BLN";
		}

		#endregion

		#region GeneralLedgerDefaults

		public BooleanRegistryItem PrintGLVoucherBasedOnTransactionLineBranch
		{
			get
			{
				return GetItem("PrintGLVoucherBasedOnTransactionLineBranch", () =>
				new BooleanRegistryItem(
					"PrintGLVoucherBasedOnTransactionLineBranch",
					Categories.Accounting_GeneralLedgerDefaults,
					ResString.GetMultilingualString("5B9128A0-1B80-4A8D-A1C8-5E66D54B44ED", "Print GL Voucher Based on Transaction Line Branch"),
					ResString.GetMultilingualString("AA1F2187-E715-423C-B684-03E49C865CB2", @"By default, the system prints Accounting Voucher for WIP/ACR at company level.
You are allowed to print only one WIP and one ACR accounting voucher monthly for a company.

When the registry is set to 'Yes', WIP/ACR accounting voucher will be printed at branch level.
That means, WIP/ACR with different branches will be printed in separate accounting voucher monthly."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					false
					)
				{ CountryFilterPKs = new[] { Constants.CountryGuids.Taiwan, Constants.CountryGuids.China } }
				);
			}
		}

		public BooleanRegistryItem ShowChinaGBTDataInterfaceMenus
			=> GetItem(nameof(ShowChinaGBTDataInterfaceMenus),
				() => new BooleanRegistryItem(
						nameof(ShowChinaGBTDataInterfaceMenus),
						Categories.Accounting_GeneralLedgerDefaults,
						(NoResString)"Show China GB-T Data Interface Menus",
						(NoResString)@"By default, this registry is set to 'No' in which case the following menus will be hidden in China.
1. GB-T 19851-2004 Data-interface
2. GB-T 24589.1 Data-interface

When this registry is set to 'Yes', these menus will be shown.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));

		public BooleanRegistryItem GenerateJournalEntriesForPostedAccountingTransactions
		{
			get
			{
				var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature);
				var defaultValue = false;

				if (featureData != null && featureData.TryDeserializeParameterAsJson<GeneralLedgerDataFeatureControlModel>(out var generalLedgerDataFeatureControlModel))
				{
					defaultValue = generalLedgerDataFeatureControlModel.EnableGenerateJournalEntriesForPostedAccountingTransactions;
				}

				var item = GetItem(nameof(GenerateJournalEntriesForPostedAccountingTransactions),
					() => new BooleanRegistryItem(
						nameof(GenerateJournalEntriesForPostedAccountingTransactions),
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						(NoResString)"Generate Journal Entries for Posted Accounting Transactions (CWSupport Only)",
						(NoResString)@"By default, this feature is disabled.

						When enabled, the system will commence to generate journal entries for all accounting transactions upon posting.
						If required, journal entries for transactions posted before enabling this feature can be generated via menu option in Period Management module.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue));
				item.OnBuildLogReference += (args) => Res.GetString("FE3CEBF3-7600-460E-89BB-81F7682044EF", "Registry set to '{0}'.", args.NewValue);
				item.OnUpdateAction += GenerateJournalEntries_OnUpdateAction;

				return item;
			}
		}

		void GenerateJournalEntries_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			if (!ConsolidatedAccountingCategoryList.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				ConsolidatedAccountingCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConsolidatedAccountingCategoryList.DefaultValue);
			}

			var pLAppropriationAccount = (RegistryItemImpl)ObjectFactory.Get<IAccounting>().PlAppropriationAccountRegistryItem;
			if (!((IRegistryItemInternals)pLAppropriationAccount).HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				pLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pLAppropriationAccount.DefaultValue);
			}
		}

		public DateTimeRegistryItem GenerateJournalEntriesCDCStartDate
		{
			get
			{
				var item = GetItem(nameof(GenerateJournalEntriesCDCStartDate),
					() => new DateTimeRegistryItem(
						nameof(GenerateJournalEntriesCDCStartDate),
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						(NoResString)"Generate Journal Entries - CDC Start Date (CWSupport Only)",
						(NoResString)@"By default, CDC is not enabled.
When a start date is specified and saved to the ""Generate Journal Entries - Start Date"" registry, the date (today's date) when the registry is set will be saved into this registry.
The system will commence to create CDC records for all journal entries created from this point.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden));
				item.OnBuildLogReference += (args) => Res.GetString("D41BEE03-8D01-461F-A3B9-DF517E5C877", "CDC Start Date set to '{0}'.", args.NewValue);
				item.Options = Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly : RegistryOptions.IsHidden;
				return item;
			}
		}

		public DateTimeRegistryItem JournalEntriesLastQueuedDate
		{
			get
			{
				var item = GetItem(nameof(JournalEntriesLastQueuedDate),
					() => new DateTimeRegistryItem(
						nameof(JournalEntriesLastQueuedDate),
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						(NoResString)"Journal Entries Last Queued Date (CWSupport Only)",
						(NoResString)@"When journal entries were queued by 'General Ledger Data Backlog Queue Service Task', the latest queued entry's post date will be saved into this registry.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden));
				item.Options = Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly : RegistryOptions.IsHidden;
				return item;
			}
		}

		public DateTimeRegistryItem JournalEntriesLastProcessedDate
		{
			get
			{
				var item = GetItem(nameof(JournalEntriesLastProcessedDate),
					() => new JournalEntriesLastProcessedDateItemImpl(
						nameof(JournalEntriesLastProcessedDate),
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						ResString.GetMultilingualString("01377068-96CE-4D19-94F3-A21A88DC8134", "Journal Entries Last Processed Date"),
						ResString.GetMultilingualString("91048CC8-B0AB-4810-BB52-5A27C056E399", @"When all journal entries queue records have been processed by 'GLP - General Ledger Data Backlog Process Service Task', the current 'Generate Journal Entries - Start Date' registry value will be saved into this registry."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden));
				item.Options = Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.IsReadOnly : RegistryOptions.IsHidden;
				return item;
			}
		}

		#region GeneralLedgerDefaults_LinkAccount_TaxTransaction

		public GuidRegistryItem TaxTransactionPrepaidAssetControlAccount
		{
			get
			{
				return GetItem("GL_OTHER_TAXES_PREPAID_ASSET_CONTROL_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("BE96AC4E-0430-4656-9F58-B8FC3269CFE3", "Tax Transaction Realization Prepaid Asset Account");
					MultilingualString hint = ResString.GetMultilingualString("D679C4DD-9DD6-441B-B616-6A03CC54FCC4", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

This registry defines a default 'Tax Realization' General Ledger account for Tax Configurations that will realize Tax Transactions as Prepaid Tax Assets.
This registry sets the 'Tax Realization' account of a New Tax Configuration when creating Payables Perceptions (PER), Payables Value Added Tax (VAT); Receivables Invoice Retention (RII) and Receivables Standard Payments Basis Withholding (SPR) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups.
Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_OTHER_TAXES_PREPAID_ASSET_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount_TaxTransaction,
						captionAndHint,
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					(result.DataType as GuidRegistryDataType).Validating += ValidateGLAccount;

					return result;
				});
			}
		}

		public GuidRegistryItem TaxTransactionRemittanceLiabilityControlAccount
		{
			get
			{
				return GetItem("GL_OTHER_TAXES_REMITTANCE_LIABILITY_CONTROL_ACCOUNT", delegate
				{
					MultilingualString caption = ResString.GetMultilingualString("551DF8FA-6EF4-482E-9144-AAE8E556F281", "Tax Transaction Realization Liability Account");
					MultilingualString hint = ResString.GetMultilingualString("DC67C7AC-5F30-441A-AB57-273DD6F8E2F3", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

This registry defines a default 'Tax Realization' GL Account for Tax Configurations that will realize Tax Transactions as Tax Remittance Liabilities payable to a Tax Authority.
This registry sets the 'Tax Realization' account of a New Tax Configuration when creating Receivables Perceptions (PER), Receivables Value Added Tax (VAT), Receivables Turnover Tax (TRX), Receivables Sales Tax (SLX); Payables Invoice Retention (RII) and Payables Standard Payments Basis Withholding (SPR) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups.
Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_OTHER_TAXES_REMITTANCE_LIABILITY_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount_TaxTransaction,
						caption,
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					(result.DataType as GuidRegistryDataType).Validating += ValidateGLAccount;

					return result;
				});
			}
		}

		public GuidRegistryItem PendingTaxTransactionPrepaidAssetControlAccount
		{
			get
			{
				return GetItem("GL_PENDING_OTHER_TAXES_PREPAID_ASSET_CONTROL_ACCOUNT", delegate
				{
					MultilingualString caption = ResString.GetMultilingualString("33F36257-24CB-4690-9566-2CCF0C03D510", "Pending Realization Tax Transaction Prepaid Asset Account");
					MultilingualString hint = ResString.GetMultilingualString("ADC19273-1ED2-4E52-AFBD-5CB638B97BE7", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Pending Realization Tax Transaction Prepaid Asset Account' registry defines a default 'Pending Tax Realization' GL account for 'Prepaid Tax Asset' Tax Configurations with 'Match Date' realization behavior.
This registry sets the 'Pending Tax Realization' account of a New Tax Configuration when creating Payables Perceptions (PER), Payables Value Added Tax (VAT) and Receivables Invoice Retention (RII) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups. Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_PENDING_OTHER_TAXES_PREPAID_ASSET_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount_TaxTransaction,
						caption,
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					(result.DataType as GuidRegistryDataType).Validating += ValidateGLAccount;

					return result;
				});
			}
		}

		public GuidRegistryItem PendingTaxTransactionRemittanceLiabilityControlAccount
		{
			get
			{
				return GetItem("GL_PENDING_OTHER_TAXES_REMITTANCE_LIABILITY_CONTROL_ACCOUNT", delegate
				{
					MultilingualString caption = ResString.GetMultilingualString("998F96BB-AE73-40BD-BB03-2E09420B65FD", "Pending Realization Tax Transaction Remittance Liability Account");
					MultilingualString hint = ResString.GetMultilingualString("FB499BAD-6102-4B6E-8CB4-1BC98BF9F837", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Pending Realization Tax Transaction Remittance Liability Account' registry defines a default 'Pending Tax Realization' GL account for 'Tax Remittance Liability' Tax Configurations with 'Match Date' realization behavior.
This registry sets the 'Pending Tax Realization' account of a New Tax Configuration when creating Receivables Perceptions (PER), Value Added Tax (VAT), Turnover Tax (TRX) and Sales Tax (SLX); and Payables Invoice Retention (RII) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups. Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_PENDING_OTHER_TAXES_REMITTANCE_LIABILITY_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount_TaxTransaction,
						caption,
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					(result.DataType as GuidRegistryDataType).Validating += ValidateGLAccount;

					return result;
				});
			}
		}

		public GuidRegistryItem TaxTransactionExpenseAccount
		{
			get
			{
				return GetItem("OtherTaxesExpenseAccount", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("OtherTaxesExpenseAccount",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount_TaxTransaction,
						ResString.GetMultilingualString("59519c54-2138-41e7-b423-9b27e2d927e4", "Tax Transaction Expense Account"),
						ResString.GetMultilingualString("67f00f22-2824-4d70-883d-32c575d716d9", @"This registry is used when adding new Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Tax Transaction Expense Account' registry defines a default 'Tax Expense' GL Account for Payables Sales Tax (SLX) Super Type Tax Configurations. The 'Tax Transaction Expense Account' is used to expense Sales Tax related to the posting of Costs.
Changes to this registry do not change existing Tax Configurations. Changes to the GL Account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl);

					(result.DataType as GuidRegistryDataType).Validating += ValidateGLAccount;

					return result;
				});
			}
		}

		public GuidRegistryItem TaxTransactionNegativeRevenueAccount
		{
			get
			{
				return GetItem("OtherTaxesNegativeRevenueAccount", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("OtherTaxesNegativeRevenueAccount",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount_TaxTransaction,
						ResString.GetMultilingualString("7cea3c1f-0f88-4e6f-a81f-bfbca4505c91", "Tax Transaction Negative Revenue Account"),
						ResString.GetMultilingualString("83eb9dbb-e0cb-417e-a444-cf0e068ecbe2", @"This registry is used when adding new Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Tax Transaction Negative Revenue Account' registry defines a default 'Tax Expense' GL Account for Receivables Turnover Tax (TRX) Super Type Tax Configurations. 
The 'Tax Transaction Negative Revenue Account' is used is used by Receivables Turnover Tax Configurations when recording Turnover Tax records related to the posting of Revenue.
Changes to this registry do not change existing Tax Configurations. Changes to the GL Account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl);

					(result.DataType as GuidRegistryDataType).Validating += ValidateGLAccount;

					return result;
				});
			}
		}

		#endregion

		public GuidRegistryItem GLJournalExchangeRateDifferenceAccount
		{
			get
			{
				var captionAndHint = ResString.GetMultilingualString("7ad5e017-5273-47ce-938e-46b8ae84593a", "GL Journal Exchange Rate Difference Account");
				return GetItem("GLJournalExchangeRateDifferenceAccount", delegate
				{
					var result = new GuidRegistryItem("GLJournalExchangeRateDifferenceAccount",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						captionAndHint,
						RegistryStorageFlags.System,
						RegistryOptions.IsValueOptional);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHandNonControl);
					result.OnBuildLogReference += (args) =>
					{
						var factory = new BusinessObjectFactory();
						var originalGLHeader = factory.Load<AccGLHeader>(new ZGuid(args.OriginalValue));
						var newGLHeader = factory.Load<AccGLHeader>(new ZGuid(args.NewValue));
						return Res.GetString("76deb4cd-e922-4074-8148-fbf560d313db", "GL Account changed from [{0}] to [{1}].", originalGLHeader?.AccountNum ?? ZString.Empty, newGLHeader?.AccountNum ?? ZString.Empty);
					};
					(result.DataType as GuidRegistryDataType).Validating += ValidateGLAccount;

					return result;
				});
			}
		}

		void ValidateGLAccount(object sender, RegistryDataTypeValidatingEventArgs<Guid> e)
		{
			var factory = new BusinessObjectFactory();
			var glHeader = factory.Load<AccGLHeader>(e.ProposedValue);
			if (glHeader != null)
			{
				if (glHeader.AlternateGLAccountDissections.Any())
				{
					throw new RegistryValidationException(
					Res.GetString("D0AD9CED-B020-49B9-9320-25E7DEBF359A", "GL Accounts with Dissections cannot be selected"));
				}

				var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccountAttribute));
				var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateGLAccountAttribute), AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount);
				subQuery.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, glHeader.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);
				query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, SQLComparisonOperator.NotEqual, glHeader.PK);
				var attributes = factory.Load<AccAlternateGLAccountAttribute>(query);
				if (attributes.Length > 0)
				{
					throw new RegistryValidationException(Res.GetString("6584D38C-026D-46E4-A4AD-0C47CEFDFF0D", @"You cannot select this GL Account Number '{0}' as it is mapped to an Alternate Account linked to multiple Parent Accounts.", glHeader.AG_AccountNum));
				}
			}
		}

		public GLJournalExchangeRateTypeRegistryItem GLJournalExchangeRateType
		{
			get
			{
				return GetItem("GLJournalExchangeRateType",
					delegate
					{
						var item = new GLJournalExchangeRateTypeRegistryItem(
									"GLJournalExchangeRateType",
									Categories.Accounting_GeneralLedgerDefaults,
									ResString.GetMultilingualString("64D77216-4A1A-4B53-B34E-D88B7B5E99F9", "GL Journal Exchange Rate Type"),
									ResString.GetMultilingualString("557C34FC-0FFB-44BC-9D9D-C3B5B3010357", "This registry defines which exchange rate type will be used to convert foreign currency journal value to local currency based on the GL Account Type during the creation of General Ledger Journal."),
									RegistryStorageFlags.Company,
									RegistryOptions.Default,
									GetDefaultGLJournalExchangeRateType());

						item.OnBuildLogReference += (args) => BuildGLJournalExchangeRateTypeLogReference(args);

						return item;
					});
			}
		}

		static GLJournalExchangeRateType GetDefaultGLJournalExchangeRateType()
		{
			var defaultValue = new GLJournalExchangeRateType();
			defaultValue.BalanceSheetAccountTypeExchangeRateType = Constants.ExchangeRateTypes.Code.PeriodEndRate;
			defaultValue.ProfitAndLossAccountTypeExchangeRateType = Constants.ExchangeRateTypes.Code.PeriodEndRate;

			return defaultValue;
		}

		string BuildGLJournalExchangeRateTypeLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var originalElements = (GLJournalExchangeRateType)args.OriginalValue;
			var newElements = (GLJournalExchangeRateType)args.NewValue;

			if (originalElements.BalanceSheetAccountTypeExchangeRateType != newElements.BalanceSheetAccountTypeExchangeRateType)
			{
				result += Res.GetString("729F4BCD-37E7-4869-B682-A34C073E269C", "Balance Sheet Account Type Exchange Rate Type has been changed from {0} to {1}.", originalElements.BalanceSheetAccountTypeExchangeRateType, newElements.BalanceSheetAccountTypeExchangeRateType) + "\r\n";
			}

			if (originalElements.ProfitAndLossAccountTypeExchangeRateType != newElements.ProfitAndLossAccountTypeExchangeRateType)
			{
				result += Res.GetString("6C127E80-BAA9-4597-B57C-EF55CC261230", "Profit and Loss Account Type Exchange Rate Type has been changed from {0} to {1}.", originalElements.ProfitAndLossAccountTypeExchangeRateType, newElements.ProfitAndLossAccountTypeExchangeRateType);
			}

			return result;
		}

		#endregion

		#region Compliance Reports Setup.

		#region SuppressResourceStringsCheckRegion

		public IntRegistryItem MaxOccurrencesBeforeReportJHBranchIsNull
		{
			get
			{
				return GetItem("MaxOccurrencesBeforeReportJHBranchIsNull", delegate
				{
					return new IntRegistryItem(
						"MaxOccurrencesBeforeReportJHBranchIsNull",
						Categories.Accounting,
						(NoResString)"Report 'JobHeader Branch/Department is null' error after number of occurrences(CargoWiseOne Support Only)",
							(NoResString)@"After the number of occurrences of 'Branch/Department is null' error, JobHeader constructor stack trace will be captured and will report if the error occurred again.

NOTE: the occurrence counter will be reset after the next occurrence reported",
							new NumericRegistryEditorInfo(0),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							5);
				});
			}
		}

		public IntRegistryItem JHBranchIsNullOccurrenceCounter
		{
			get
			{
				return GetItem("JHBranchIsNullOccurrenceCounter", delegate
				{
					return new IntRegistryItem(
						"JHBranchIsNullOccurrenceCounter",
						Categories.Accounting,
						(NoResString)"Internal Counter of Branch-Is-Null occurrences",
							(NoResString)"Internal Counter of Branch-Is-Null occurrences",
							new NumericRegistryEditorInfo(0),
							RegistryStorageFlags.System,
							RegistryOptions.IsHidden,
							0);
				});
			}
		}

		public ComplianceReportsSetupRegistryItem ComplianceReportsSetupsCN
		{
			get
			{
				return GetItem("ComplianceReportsSetupsCN", () => new ComplianceReportsSetupRegistryItem("ComplianceReportsSetupsCN",
																									Categories.Accounting_ReportsSetups,
																									(NoResString)"China Reports Setup(CargoWiseOne Support Only)",
																									(NoResString)@"This registry is used to define the ‘Report Codes’ and associated ‘Report Categories’.\r\nThis will be available as drop down options under Maintain > Accounts > GL Multi-Language Mapping > Report Setup.",
																									ComplianceReportTypeCollectionForCurrentCompany,
																									RegistryOptions.IsOnlyForSupport));
			}
		}

		ComplianceReportTypeCollection ComplianceReportTypeCollectionForCurrentCompany
		{
			get
			{
				return new ComplianceReportTypeCollection(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
		}

		public BooleanRegistryItem EnableReportSetup
		{
			get
			{
				return GetItem("EnableReportSetup", () =>
					new BooleanRegistryItem("EnableReportSetup",
						Categories.Accounting_ReportsSetups,
						(NoResString)"Enable Report Setup(CargoWise Support Only)",
						(NoResString)@"When this registry is enabled, the following functionalities will be made available:-
										a.	Access to Accounting > Reports Setups > User Define Reports Setup registry
										b.	Access to Maintain > Account > GL Multi-Language Mapping > Edit > Reports Setup tab.
										c.	Access to Report Setup Profile report in Accounts > General Ledger > Report and Admin > Account > Report module.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public IntRegistryItem MaximumTimeForCRQServiceTaskToRun
		{
			get
			{
				return GetItem("MaximumTimeForCRQServiceTaskToRun", delegate
				{
					return new IntRegistryItem(
						"MaximumTimeForCRQServiceTaskToRun",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Maximum runtime for CRQ service task in seconds when generating and purging (CargoWise Support Only)",
						(NoResString)@"CRQ generates large compliance reports like IDEA or FEC in the background.
Normal working cycle is: Queue - Generate - Purge. This cycle might take some time to complete.
To reduce waiting time, the working cycle is repeated until the configured timeout is reached or there is no more data to process.
The value of this registry is in seconds.
The default is 2700 (45 minutes). Maximum is 86400 (24 hours).
Set to zero to disable this behavior.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						2700,
						0,
						86400);
				});
			}
		}

		#endregion

		public ComplianceReportsSetupRegistryItem ComplianceReportsSetupsUserDefined
		{
			get
			{
				var item = GetItem("ComplianceReportsSetupsUserDefined", () =>
					new ComplianceReportsSetupRegistryItem("ComplianceReportsSetupsUserDefined",
						Categories.Accounting_ReportsSetups,
						ResString.GetMultilingualString("0B53C496-BC03-400C-BC77-20FA8177CB16", "User Define Reports Setup"),
						ResString.GetMultilingualString("BBD7FEE9-DD29-4B78-BA87-3B42E8AA626A", @"This registry is used to define the ‘Report Codes’ and associated ‘Report Categories’.
							This will be available as drop down options under Maintain > Accounts > GL Multi-Language Mapping > Report Setup."),
						new ComplianceReportTypeCollection(),
						RegistryOptions.IsHidden));
				item.Options = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China || Instance.EnableReportSetup.Value ? RegistryOptions.Default : RegistryOptions.IsHidden;
				return item;
			}
		}

		public CodeDescriptionBoolRegistryItem VisibleGermanComplianceReports
		{
			get
			{
				return GetItem("VisibleGermanComplianceReports", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"VisibleGermanComplianceReports",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Show/Hide German Compliance Reports",
						(NoResString)"Hide German compliance reports already checked-in but not yet fully implemented",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("240F9F35-793B-497B-9AA7-391CAE9779EC", "Is Enabled"), true, true),
						GetGermanComplianceReportCollection());
				});
			}
		}

		public IntRegistryItem ComplianceReportQueryTimeoutInMinutes
		{
			get
			{
				return GetItem("ComplianceReportQueryTimeoutInMinutes", () =>
				{
					return new IntRegistryItem(
						"ComplianceReportQueryTimeoutInMinutes",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("f0f51a7e-d618-4f9d-9bdb-3613c0ddb5b6", "Compliance Report Query Timeout (minutes)"),
						ResString.GetMultilingualString("2bb96b55-6e3c-4e4f-b331-9c3965514d68", @"This registry sets the SQL timeout for Compliance Report queries in minutes.
If you experience query timeouts while working with Compliance Reports, you can increase this registry as a temporary workaround.
Please reset this registry to the default after your Compliance Report is completed."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System, RegistryOptions.Default,
						30, 30, 20000);
				});
			}
		}

		static CodeDescriptionBoolCollection GetGermanComplianceReportCollection()
			=> new CodeDescriptionBoolCollection()
		{
				{ "U11", (NoResString)"Umsatzsteuer-Sondervorauszahlung", false },
				{ "UVA", (NoResString)"Umsatzsteuervoranmeldung", false },
				{ "ZMD", (NoResString)"Zusammenfassende Meldung", false }
		};

		#endregion

		#region GL Multi-Language Mapping Numbering Format

		public GLLocalNumberFormatRegistryItem LocalNumberFormats
		{
			get
			{
				return GetItem("LocalNumberFormats", () => new GLLocalNumberFormatRegistryItem("LocalNumberFormats",
																									Categories.Accounting_GeneralLedgerDefaults,
																									ResString.GetMultilingualString(
																										"3C320F6B-9A7E-4EAA-8C06-A6B89141FC9E",
																										"GL Multi-Language Mapping Numbering Format"),
																									ResString.GetMultilingualString(
																										"58BA33A7-5CBE-4770-AF2F-9AEF6D875692",
																										@"This registry enables you to define the format of “Local Account Number”  per Language and country entered  via Maintain > Account > GL Multi-Language Mapping module. 
By default, no validation is enforced. You can enter the “Local Account Number” in any format and  length without restriction. 

To enforce a validation, add a record for each Language, country,  format and length.
You can define the specific numbering format using:
 1. Numeric value (1 to 9) 
 2. “-“ as separator to identify the different levels. (Note: You do not have to include “-“ separator when entering the Local GL Account.)

There are two length option:
 1. F = Fixed Length
 2. V = Variable Length

Example 1: Format 4-2-2 , Variable length
 The accepted values are:
 1001, 100101 and 10010101

Example 2: Format 1-1-1-1-1, Variable length
 The accepted values are:
 1, 10, 101, 1010 and 10101

Example 3: Format 2-1-1-1, Fixed length
 The accepted values are:
 10101, 10102 and 10201"),
																									RegistryOptions.Default));
			}
		}

		#endregion

		#region JobInvoicingSubCategory

		public BooleanRegistryItem JobChargeDataVersionAutoLogging
		{
			get
			{
				return GetItem("JobChargeDataVersionAutoLogging", () =>
					new BooleanRegistryItem("JobChargeDataVersionAutoLogging",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("6999D386-88B8-4D82-80DF-74DD6ACB3533", "Job Charge Data Version Auto Logging"),
						ResString.GetMultilingualString("DED5003F-75FE-4FB3-A09A-655573AF5387", "Switching on/off Job Charge data version automatic logging."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem AllowNegativeRevenueChargesOnJob
		{
			get
			{
				return GetItem("AllowNegativeRevenueChargesOnJob", delegate
				{
					return new BooleanRegistryItem(
						"AllowNegativeRevenueChargesOnJob",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("c2303a0a-231c-4561-890c-2a1b4fadf71f", "Allow negative revenue charges on a job"),
						ResString.GetMultilingualString("34ad1ce1-9465-4982-a93a-95ad0fa8b142", "Setting this registry to No default will stop users being able to save jobs with negative charges."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem DefaultOSAgentFromPickupAgent
		{
			get
			{
				return GetItem("DefaultOSAgentFromPickupAgent", () =>
					new BooleanRegistryItem("DefaultOSAgentFromPickupAgent",
						Categories.Accounting_JobInvoicing,
						(NoResString)"Default OS Agent from Shipment Pickup Agent (CargoWise Support Only)",
						(NoResString)@"In WI00252641, we changed the logic that defaults the job billing OS Agent. It now includes the Shipment 'Pickup Agent' in its defaulting logic. 
We did not do an exhaustive review of customer data to confirm that this requirement fits the needs of many customers. 
The purpose of this registry item is to revert the behaviour of the OS Agent defaulting to exclude Pickup Agent, as it was before WI00252641.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true));
			}
		}

		#endregion

		#region Accounting_ReceivableDefaults_DefaultSettings

		public BooleanRegistryItem ReceivablePreventCreationOfCreditNotes
		{
			get
			{
				return GetItem("ReceivablePreventCreationOfCreditNotes", delegate
				{
					return new BooleanRegistryItem(
						"ReceivablePreventCreationOfCreditNotes",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						(NoResString)"Prevent Creation of Credit Notes",
						(NoResString)@"This registry controls the ability to create amending and stand alone AR CRD transactions.

Setting this registry to 'Yes' will prevent all users in your login company from performing the following tasks:

* Creating a stand alone credit note directly in the receivables module
* Triggering the 'Amend with Credit Note' option in a job
* Posting AR CRD transactions when the net total of the transaction is a negative amount
* Creating periodic invoice credit notes
* Importing receivables sister company credit notes
* Creating consol costing credit notes",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem ReceivablePreventCreationOfReversalTransactions
		{
			get
			{
				return GetItem("ReceivablePreventCreationOfReversalTransactions", delegate
				{
					return new BooleanRegistryItem(
						"ReceivablePreventCreationOfReversalTransactions",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						(NoResString)"Prevent Reversal of Invoice Transactions",
						(NoResString)@"This registry controls the ability to create reversal credit notes in Accounts Receivables.

Setting this registry to 'Yes' will prevent all users in your login company from reversing any document posted in Receivables.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public CodePairRegistryItem NegativeAmountAllowedOnAccountReceivableTransactions
		{
			get
			{
				return GetItem("NegativeAmountAllowedOnAccountReceivableTransactions", delegate
				{
					return new CodePairRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
						"NegativeAmountAllowedOnAccountReceivableTransactions",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("d24d3754-dac2-41cd-977e-fc3c92a68cd8", "Negative Charges on Accounts Receivable Transactions"),
						ResString.GetMultilingualString("0BAE68FD-DE43-45F1-B5F7-8E5C93184378", @"By default, the system allows negative charge lines in all AR transactions.

Based on your own needs, you can override this registry either to prevent users from posting negative charges in AR Invoice, Credit Note, or Adjustment Note transactions, or to allow negative charges only when the Debtor is Not Applicable to Tax.

ALL - Negative Charges Allowed - Allows negative line amounts on all the AR transactions. 
NAL - Negative Charges Not Allowed - Stops users from posting negative line amounts on AR Invoice, Credit Note or Adjustment Note transactions. 
NOT - Negative Charges Allowed if the Debtor is Not Applicable to Tax

Note: This registry will be set to NAL option by default for Vietnam login companies with Receivables E-Reporting Functionality enabled as the negative charge lines will cause the E-reporting transmission error. You can choose to override this registry to NOT if there is a need to post Non-Taxable invoice with negative charges."),
						new CodePairRegistryDataTypeWithAdditionalValidation(new CodeDescriptionPairListProvider(() => new TransactionLineNegativeAmountAllowedOnAccountReceivableModes()), false, true, ValidateNegativeAmountAllowedOnAccountReceivableTransactions),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						(companyPK, branchPK, departmentPK) => IsCountrySpecificAllowed(companyPK)
							? TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code
							: TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code
						)
					);
				});
			}
		}

		string ValidateNegativeAmountAllowedOnAccountReceivableTransactions(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var validationResult = string.Empty;

			if (proposedValue == TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code
				&& IsCountrySpecificAllowed(companyPK))
			{
				validationResult = Res.GetString("5A43BB03-2F21-4522-83F8-D018F71B24FC", "You can override the registry to ALL option only when the Receivables E-Reporting Functionality is disabled.");
			}

			return validationResult;
		}

		bool IsCountrySpecificAllowed(Guid companyPK)
		{
			if (EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty))
			{
				var countryCode = ObjectFactory.Get<ICompanyProvider>().WithFactory(FactoryForCountryDefaultValues).Get(companyPK).GetCountryCode().SomeOrDefault(string.Empty);
				return ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<ISupportNegativeAmountOnARTransactions>(countryCode)?.IsNegativeChargesAllowed ?? false;
			}
			
			return false;
		}	

		#endregion

		#region Accounting_PayableDefaults_DefaultSettings

		public BooleanRegistryItem EnableAPInvoiceApproval
		{
			get
			{
				return GetItem("EnableAPInvoiceApproval", () =>
					new BooleanRegistryItem("EnableAPInvoiceApproval",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)"Enable APInvoice Approval",
						(NoResString)"Enable APInvoice Approval (For Developers Only)",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true));
			}
		}

		public BooleanRegistryItem EnableTransactionPendingAllocationApproval
		{
			get
			{
				return GetItem("EnableTransactionPendingAllocationApproval", () =>
					new BooleanRegistryItem("EnableTransactionPendingAllocationApproval",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)"Enable Transaction Pending Allocation Approval",
						(NoResString)"Enable Transaction Pending Allocation Approval (For Developers Only)",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true));
			}
		}

		public BooleanRegistryItem EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval
		{
			get
			{
				return GetItem("EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval", () =>
					new BooleanRegistryItem("EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)"Enable Rejection eInvoicing Request in Transaction Pending Allocation Approval",
						(NoResString)"Enable Rejection eInvoicing Request in Transaction Pending Allocation Approval (For Support Only)",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem PayablePreventCreationOfCreditNotes
		{
			get
			{
				return GetItem("PayablePreventCreationOfCreditNotes", delegate
				{
					return new BooleanRegistryItem(
						"PayablePreventCreationOfCreditNotes",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)"Prevent Creation of Credit Notes",
						(NoResString)@"This registry controls the ability to create credit notes in Accounts Payables.

Setting this registry to 'Yes' will prevent all users in your login company from performing the following tasks:


* Posting AP CRD transactions by entering negative cost amounts
* Creating a stand alone credit note directly in the payables module
* Creating incomplete Payables credit notes
* Importing AP Credit notes via Universal XML
* Accepting import of sister invoices with claim via XML",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem PayablePreventCreationOfReversalTransactions
		{
			get
			{
				return GetItem("PayablePreventCreationOfReversalTransactions", delegate
				{
					return new BooleanRegistryItem(
						"PayablePreventCreationOfReversalTransactions",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)"Prevent Reversal of Invoice Transactions",
						(NoResString)@"This registry controls the ability to create reversal credit notes in Accounts Payables.

Setting this registry to 'Yes' will prevent all users in your login company from reversing any document posted in Payables.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public CodePairRegistryItem AllowDuplicateInvoiceNumberDefaultingRule
		{
			get
			{
				return GetItem("AllowDuplicateInvoiceNumberDefaultingRule", () =>
				{
					return new CodePairRegistryItem(
						"AllowDuplicateInvoiceNumberDefaultingRule",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("DB272A24-A7D6-4B95-B3DA-1ADCACB2A2A4",
							"Allow Duplicate Invoice Number Defaulting Rule"),
						ResString.GetMultilingualString("F515811E-B587-4D26-9969-29E79ED57C42",
							@"This registry is to determine whether authorized users can post a duplicate invoice number for a creditor based on whether the last used invoice date is more than 12 months old, or whether it is in the previous calendar year.

When set to STD, an authorized user can post an invoice number previously used for the same creditor provided it is more than 12 months since the last invoice date.

When set to CAL, an authorized user can post an invoice number previously used for the same creditor provided it has not been used in the current calendar year.

This registry works in conjunction with the Manage > Payables Transactions > New transactions > Allow Duplicate AP invoice Number security setting. Without this security right granted, a user will not be able to re-use an invoice number regardless of the setting in this registry.

Note: This registry also has action when importing AP invoices via XUT."),
						new CodeDescriptionPairListProvider(() => new AllowDuplicateInvoiceNumberRules()),
						RegistryStorageFlags.Company,
						AllowDuplicateInvoiceNumberRules.STD.Code);
				});
			}
		}

		public BooleanRegistryItem EnableGovernmentAllocatedNumberBehavior
		{
			get
			{
				return GetItem("EnableGovernmentAllocatedNumberBehavior", () =>
				new BooleanRegistryItem(
					new CountrySpecificDefaultValueRegistryItemImpl<bool>(
						"EnableGovernmentAllocatedNumberBehavior",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)@"Government Allocated Number is the unique identifier assigned by the government through government channels when receiving and verifying electronic invoices.
When this registry is set to ""Yes"", manual entry of the Government Allocation number is allowed in the Payables Module during the posting of a New Invoice, Credit note, or Adjustment Note.
When it's set to ""No"" these features are disabled.

NOTE: When this registry is set to Yes, this also has an effect on the following area :
When importing AP invoices via XUT, if the 'GovernmentAllocatedID' field, which is used for the Government Allocation Number, is filled out, the value is imported into AH_GovernmentAllocatedID.

ALERT : This registry must be used in conjunction with the required validation for each country; currently, it is enabled only for Israel by default.
Please Consult the Accounting product team before enabling this registry.",
						RegistryDataTypes.BoolType,
						RegistryOptions.CacheExpensiveDefaultValue | RegistryOptions.IsOnlyForSupport,
						FactoryForCountryDefaultValues,
						new EnableGovernmentAllocatedNumberBehavior_RegistryDescriptor())));
			}
		}

		#region Payment Processing Sub Category

		public BooleanRegistryItem CheckBookDataVersionAutoLogging
		{
			get
			{
				return GetItem("CheckBookDataVersionAutoLogging", () =>
					new BooleanRegistryItem("CheckBookDataVersionAutoLogging",
						Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing,
						ResString.GetMultilingualString("7e716af6-5559-439c-8180-9b8475c7da08", "Cheque Book Data Version Auto Logging"),
						ResString.GetMultilingualString("a47b458d-8fea-414d-bfbe-b6a53f67277c", "Switching on/off Cheque Book Data version automatic logging."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		#endregion

		#endregion

		#region Credit Controlled Documents Configuration

		#region Organizations Evaluated for Credit Control

		public OrgsEvaluatedForCreditControlRegistryItem OrganizationsEvaluatedForCreditControl
		{
			get
			{
#if DEBUG
				OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly++;
#endif
				return GetItem("OrganizationsEvaluatedForControlledDocumentDelivery", delegate
				{
					OrgsEvaluatedForCreditControlCollection defaultValues = GetOrganizationsEvaluatedForCreditControlDafaultValue();
					return new OrgsEvaluatedForCreditControlRegistryItem(
						"OrganizationsEvaluatedForControlledDocumentDelivery",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("21f68453-3ff7-4fbb-85c9-ebc89a90a3ae", "Organizations Excluded from Credit Control Evaluation"),
						ResString.GetMultilingualString("9688ef86-b337-41cf-9567-c835a6feaf14", @"This registry enables you to exclude one or more Organization Types from credit controlled document delivery restriction evaluations based on Job Type, Direction, Mode, INCO Term and Freight Payment Term. 

By default, 'ADB – ALL Debtors' organization type will be excluded from the credit evaluation for all job types. You can override the default setup and adjust the configuration in accordance with your needs.

Note: If the registry 'Accounting > Credit Controlled Documents Configuration > Enable Credit Control Evaluation for Controlling Customers and Controlling Agents' is set to 'Yes'. Both Controlling Customer and Controlling Agent will be taken into consideration in the credit control evaluation for 'SHP – Shipment', 'QSH – Quick Booking' and 'BRK – Declaration Job' job types and you will be able to exclude the credit evaluation of these organization types in this registry. "),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValues);
				});
			}
		}

		OrgsEvaluatedForCreditControlCollection GetOrganizationsEvaluatedForCreditControlDafaultValue()
		{
			var result = new OrgsEvaluatedForCreditControlCollection();

			foreach (CodeDescriptionPair jobType in OrgsEvaluatedForCreditControlLookups.GetJobTypeListWithoutTypeAll())
			{
				var defaultValue = new OrgsEvaluatedForCreditControl();
				using (defaultValue.GetValidationSuspender()) // To avoid access to Resource String when validating on JobType property set
				{
					defaultValue.JobType = jobType.Code;

					if (defaultValue.HasAllDebtors)
					{
						defaultValue.OrganizationType = OrgCodes.AllDebtors;

						if (!defaultValue.DirectionCode_ReadOnly)
						{
							defaultValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
						}
						if (!defaultValue.Mode_ReadOnly)
						{
							defaultValue.Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
						}
						if (!defaultValue.INCOTerm_ReadOnly)
						{
							defaultValue.INCOTerm = INCOTermCodes.All;
						}
						if (!defaultValue.FreightPaymentTerm_ReadOnly)
						{
							defaultValue.FreightPaymentTerm = FreightPaymentTermCodes.All;
						}

						result.Add(defaultValue);
					}
				}
			}

			return result;
		}

		public static string OrganizationsEvaluatedForCreditControlCacheKey(GlbCompany company = null)
			=> nameof(AccountingMasterFilesRegistry) + "."
			 + nameof(OrganizationsEvaluatedForCreditControl) + "."
			 + (company ?? GlbCompany.CurrentCompany).GC_Code;

#if DEBUG
		public int OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly;
#endif

		public CodePairRegistryItem CreditControlApprovalMode
		{
			get
			{
				return GetItem("CreditControlApprovalMode", () =>
				{
					return new CodePairRegistryItem(
						"CreditControlApprovalMode",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("BDCAF206-9971-47F6-8E0B-2D713C14395C", "Credit Control Approval Mode"), //caption
						ResString.GetMultilingualString("34234A49-B1E4-4365-8F75-E0E8D26F632C", @"The default value for this registry is ‘CW1 – Approve requests in CW1’. When using this mode, {0} will evaluate all credit checks using data in its own database OR from synchronous external web service calls to external systems. Approvals of these requests are made within {0} always.

When this registry is set to ‘EXT’, {0} will submit credit control requests to an external system. Responses to these requests will be received by {0} and the requests will be updated accordingly.
The mechanism for this communication is outbound Universal Shipment messages and inbound Universal Event messages.
This method should only be used if your external systems can evaluate the credit status of all relevant parties on the shipment and can respond to {0} with an approval or rejection response.", BrandingFactory.Instance.ProductName), //hint
						new CodeDescriptionPairListProvider(() => new CreditControlApprovalModes()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveLocallyInCW1.Code);
				});
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public IntRegistryItem CreditLimitCacheExpiryPeriod
		{
			get
			{
				return GetItem("CreditLimitCacheExpiryPeriod", () =>
					new IntRegistryItem("CreditLimitCacheExpiryPeriod",
						Categories.Accounting_CreditLimitCheck,
						(NoResString)"Credit Limit Cache Expiry Period",
						(NoResString)"Defines Credit Limit cache expiry period in minutes. Caching will improve performance, especially with using a third-party web service. Setting it to 0 will disable caching.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						15));
			}
		}

		#endregion

		public AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem CreditControllerOverrideThreshold
		{
			get
			{
				return GetItem("CreditControllerOverrideThreshold", delegate
				{
					return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem("CreditControllerOverrideThreshold",
						 Categories.Accounting_CreditLimitCheck_Local,
						 ResString.GetMultilingualString("5dd49831-390b-48b5-92c2-3bffada35309", "Credit Controller Override Threshold"),
						 ResString.GetMultilingualString("c0185dd2-6348-4eba-97a9-37bcd2708366", @"Use this registry setting to set a percentage or amount threshold in which an authorized user can override credit controls based on the exceeded value of the client's credit limit.
The authorizing levels (up to 3) allows users with specific authorization levels to override credit controls that are set up in the security settings."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem GlobalCreditControllerOverrideThreshold
		{
			get
			{
				return GetItem("GlobalCreditControllerOverrideThreshold", delegate
				{
					return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem("GlobalCreditControllerOverrideThreshold",
						Categories.Accounting_CreditLimitCheck_Global,
						ResString.GetMultilingualString("c41f3a65-793f-4d0f-a97b-95b60c9e44dc", "Global Credit Controller Override Threshold"),
						ResString.GetMultilingualString("4be1b27d-b209-4e17-a2ed-792986895e6c", @"Use this registry setting to set a percentage or amount threshold in which an authorized user can override credit controls based on the exceeded value of global credit group's credit limit.
The authorizing levels (up to 3) allows users with specific authorization levels to override credit controls that are set up in the security settings."),
						RegistryStorageFlags.System);
				});
			}
		}

		public GuidRegistryItem DebtorGlobalCreditLimitNotifyGroup
		{
			get
			{
				return GetItem("DebtorGlobalCreditLimitNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("DebtorGlobalCreditLimitNotifyGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("12837326-9cd7-4aa3-8d40-d719f5fd93cb", "Global AR Control Breach Notify Group"),
						ResString.GetMultilingualString("04244469-021c-4ca1-a450-fea1dbaa095a", "Notify Party when the Global Credit Limit of any Global Credit Control Group has been breached."),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableCreditControlEvaluationForControllingCustomersAndControllingAgents
		{
			get
			{
				return GetItem("EnableCreditControlEvaluationForControllingCustomersAndControllingAgents", delegate
				{
					var item = new BooleanRegistryItem(
						"EnableCreditControlEvaluationForControllingCustomersAndControllingAgents",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("28987990-6CF0-4A6C-BFF0-85FDC0ED76E0", "Enable Credit Control Evaluation for Controlling Customers and Controlling Agents"),
						ResString.GetMultilingualString("6DD5632E-1F38-4480-ACAF-A5E382772D69", @"By default, the credit status of the Controlling Customer and Controlling Agent will not be taken into consideration during the credit controlled document delivery restriction evaluation. 

When this registry is set to 'Yes', the system will include the Controlling Customer and Controlling Agent in the credit control evaluation for 'SHP – Shipment', 'QSH – Quick Booking' and 'BRK – Declaration Job' job types. Further, the 'CTP - Controlling Customer' and 'CAG - Controlling Agent' organization types will be available for configuration in the 'Accounting > Credit Controlled Documents Configuration > Organizations Excluded from Credit Control Evaluation' registry. 

Note: When this registry is set from 'Yes' to 'No', then any 'CTP - Controlling Customer' and 'CAG - Controlling Agent' rows configured in the 'Organizations Excluded from Credit Control Evaluation' registry will be removed. These changes will only be reflected when you have closed and re-open the registry. "),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
					item.OnBuildLogReference += (args) => Res.GetString("101ca9e1-1cf4-4f38-ac93-73c4cbbe28ef", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					item.OnUpdateAction = RemoveCTPCAGInOrganizationsEvaluatedForCreditControl_OnUpdateAction;
					return item;
				});
			}
		}

		void RemoveCTPCAGInOrganizationsEvaluatedForCreditControl_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			if (newValue is bool value && !value)
			{
				if (companyPk != Guid.Empty)
				{
					if (OrganizationsEvaluatedForCreditControl.Inner.HasActualValue(companyPk, branchPk, departmentPk))
					{
						RemoveCTPCAG(companyPk);
					}
					else
					{
						if (OrganizationsEvaluatedForCreditControl.Inner.HasActualValue(Guid.Empty, branchPk, departmentPk))
						{
							var systemLevelValue = OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Guid.Empty, branchPk, departmentPk);
							var excludeCTPCAGSystemValue = ExcludeCTPCAGInOrganizationsEvaluatedForCreditControl(systemLevelValue);
							if (excludeCTPCAGSystemValue.Count() != systemLevelValue.Count)
							{
								var result = ConvertToCollection(excludeCTPCAGSystemValue);
								OrganizationsEvaluatedForCreditControl.SetValue(companyPk, branchPk, departmentPk, result);
							}
						}
					}
				}
				else
				{
					var activeCompanies = GlbCompany.GetActiveCompanies();
					activeCompanies.ForEach(comp =>
					{
						var pk = comp.PK.ToGuid();
						var hasValueInEnableCAGCTP = EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Inner.HasActualValue(pk, branchPk, departmentPk);
						if (hasValueInEnableCAGCTP
						&& EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetValueWithoutFallback(pk, branchPk, departmentPk))
						{
							if (!OrganizationsEvaluatedForCreditControl.Inner.HasActualValue(pk, branchPk, departmentPk))
							{
								var systemCollection = OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(companyPk, branchPk, departmentPk);
								OrganizationsEvaluatedForCreditControl.SetValue(pk, branchPk, departmentPk, systemCollection);
							}
						}
						else if (!hasValueInEnableCAGCTP)
						{
							if (OrganizationsEvaluatedForCreditControl.Inner.HasActualValue(pk, branchPk, departmentPk))
							{
								RemoveCTPCAG(pk);
							}
						}
					});
					RemoveCTPCAG(companyPk);
				}
			}

			void RemoveCTPCAG(Guid pk)
			{
				var oldCollection = OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(pk, branchPk, departmentPk);
				var newEnumerable = ExcludeCTPCAGInOrganizationsEvaluatedForCreditControl(oldCollection);

				if (oldCollection.Count != newEnumerable.Count())
				{
					var result = ConvertToCollection(newEnumerable);
					OrganizationsEvaluatedForCreditControl.SetValue(pk, branchPk, departmentPk, result);
				}
			}

			IEnumerable<OrgsEvaluatedForCreditControl> ExcludeCTPCAGInOrganizationsEvaluatedForCreditControl(OrgsEvaluatedForCreditControlCollection collection)
			{
				return collection.Cast<OrgsEvaluatedForCreditControl>().Where(x => x.OrganizationType != OrgCodes.ControllingCustomer && x.OrganizationType != OrgCodes.ControllingAgent);
			}

			OrgsEvaluatedForCreditControlCollection ConvertToCollection(IEnumerable<OrgsEvaluatedForCreditControl> enumerable)
			{
				var result = new OrgsEvaluatedForCreditControlCollection();
				if (enumerable.Any())
				{
					enumerable.ForEach(x => result.Add(x));
				}
				return result;
			}
		}

		#endregion

		#region Gateway Consol Job Invoicing

		public GatewayChargeDefaultDebtorConfigurationRegistryItem GatewayChargeDefaultDebtorConfiguration
		{
			get
			{
				return GetItem((NoResString)"Gateway Charge Default Debtor Configuration", delegate
				{
					return new GatewayChargeDefaultDebtorConfigurationRegistryItem(
						(NoResString)"Gateway Charge Default Debtor Configuration",
						Categories.Accounting_JobInvoicing_GatewayConsolJobInvoicing,
						ResString.GetMultilingualString("FDC89925-2A4B-440E-B401-F4709D60CC1C", "Gateway Charge Default Debtor Configuration"),
						ResString.GetMultilingualString("4C4DAD27-B6DA-43AE-B9A6-C7140FC2B12C", @"This registry allows you to configure debtor defaulting rules for Gateway Consol Billing Jobs.
As you add charges to a gateway billing job, the system determines the charge debtor, based on the criteria specified in this registry. The system tries to find the most relevant configuration record, based on the charge and job parameters. If the specific record is not found, it then looks for a more generic record in the following order of priority:
1. Charge Group, recorded on the Charge Code.
2. Payment Term, recorded on the Consol's Details tab.
3. Related Job, recorded on the gateway billing job charge. Gateway job charge can be related to one of the shipments attached to the consol (SHP), or not related to any job (NON).
4. Previous Sending Agent - where gateway job charge is related to a shipment, then you can use this criteria to check the Sending Agent of that shipment's previous consol. Previous Sending Agent can be a Gateway Agent (GTA), Gateway Agent with Tariff (GTT) or non-gateway agent (SGT). Previous consol is a consol from the list of consols attached to the shipment, which is preceding the current gateway consol on the shipment's journey.

Using this registry, you can set the default debtor to be:
SGT - Sending Agent of the current gateway consol.
RGT - Receiving Agent of the current gateway consol.
SPA - When gateway job charge is related to a shipment, then you can set default debtor to the Pickup Agent, recorded on the Shipment's Pickup tab.
SDA - When gateway job charge is related to a shipment, then you can set default debtor to the Delivery Agent, recorded on the Shipment's Delivery tab.
PSA - When gateway job charge is related to a shipment.then you can set default debtor to the Sending Agent of the shipment's previous consol. Previous consol is a consol from the list of consols attached to the shipment, which is preceding the current gateway consol on the shipment's journey."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
			}
		}

		public GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItem GatewayChargeDefaultInvoiceTargetJobConfiguration
		{
			get
			{
				return GetItem((NoResString)"Gateway Charge Default Invoice Target Job Configuration", delegate
				{
					return new GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItem(
						(NoResString)"Gateway Charge Default Invoice Target Job Configuration",
						Categories.Accounting_JobInvoicing_GatewayConsolJobInvoicing,
						ResString.GetMultilingualString("B4108C9F-7CD6-47CF-B310-E50D4A59475C", "Intercompany Invoice Target Job Configuration"),
						ResString.GetMultilingualString("61C7DB4D-6718-48D4-93F6-3486E34BDA49", @"This registry allows you to configure defaulting rules for Intercompany Invoice Target Job on gateway billing charges.
Intercompany Invoice Target Job can be used when charge debtor is a sister company organization proxy and when Related Job Number is selected. During intercompany import of the gateway charge, the cost is created on a job recorded as 'Intercompany Invoice Target Job'.

By default, all intercompany charges 'target' the same job, i.e. the same consol.
Using this registry, you can set the target job to be:

REL - Related Shipment. Gateway charge will be imported as cost on the shipment.
SCL - Same Consol. Gateway charge will be imported as cost on the same consol, in the receiving company.
PCL - Previous Consol. Gateway charge will be imported as cost on the related shipment's previous consol. Previous consol is a consol in the list of consols attached to the shipment, which is preceding the current gateway consol on the shipment's journey.

You can add a separate defaulting rule, depending on the related shipment's Previous Sending Agent. Previous Sending Agent checks the Sending Agent of the shipment's previous consol. Previous Sending Agent can be a Gateway Agent (GTA), Gateway Agent with Tariff (GTT) or non-gateway agent (SGT). Previous consol is a consol from the list of consols attached to the shipment, which is preceding the current gateway consol on the shipment's journey.

For example, where Previous Sending Agent is a gateway agent GTA or GTT, you may set invoice target to be Previous Consol, so that the charge gets imported to the preceding gateway agent's gateway consol billing job. Otherwise, if previous sending agent is not a gateway agent, then you may wish to set invoice target to be the related Shipment itself."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
			}
		}

		public BooleanRegistryItem GetInternalJobConfigurationSetting
		{
			get
			{
				return GetItem("GetInternalJobConfigurationSetting", delegate
				{
					return new BooleanRegistryItem(
						"GetInternalJobConfigurationSetting",
						Categories.Accounting_JobInvoicing_GatewayConsolJobInvoicing,
						ResString.GetMultilingualString("05A4CB98-DB23-4090-9336-586C0403253D", "Use Intercompany Invoice Target Rules to Set Internal Job for Internal Charges"),
						ResString.GetMultilingualString("DA4A09F0-9A74-43AD-AB46-548BD9CBD888", @"When this registry is set to Yes, if Auto Job Revenue Journals are enabled, and a gateway charge’s debtor is an organization proxy in the same login company, rather than defaulting Related Job (shipment) as the Internal Job, use the Intercompany Invoice Target Job Configuration registry to determine the Internal Job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Compliance invoice document

		public BooleanRegistryItem EnablePaperStockOptionsToPrintComplianceDocuments
			=> GetItem("EnablePaperStockOptionsToPrintComplianceDocuments", () =>
				new BooleanRegistryItem(
					new CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<bool>(
						"EnablePaperStockOptionsToPrintComplianceDocuments",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("74DCDC25-0157-4C88-ADBC-FACF2E232D09", "Enable Paper Stock options to print compliance documents"),
						ResString.GetMultilingualString("5D5B88E2-F8FA-4297-8559-2E217E853F7B", @"This registry allows you to enable/disable paper printing options that can be configured in compliance books.

In some countries, compliance documents must be printed using a specific paper format.
Users can setup printing options in compliance books when required.

For countries where pre-printed or specific paper formats are not required, these options are not relevant and users can disable them, reducing the possibility of confusion and errors when setting up compliance books.

When set to 'Yes', users will be able to setup options in the 'Compliance Document' and 'Compliance Printing' sections of a compliance book.
When set to 'No', these sections will be disabled in a compliance book.

By default, the value of this registry is 'Yes'."),
						new BooleanRegistryDataType(),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						(complianceInfo) => (complianceInfo as IComplianceRegistryDefaultProvider)?.GetDefaultValueForEnablePaperStockOptionsToPrintComplianceDocumentsRegistry() ?? true,
						true
					)
				)
			);

		public IntRegistryItem MaximumNumberOfChargesToPrintPerComplianceDocument
		{
			get
			{
				return GetItem("MaximumNumberofChargestoPrintperComplianceDocument", delegate
				{
					return new IntRegistryItem(
	"MaximumNumberofChargestoPrintperComplianceDocument",
	Categories.Accounting_GovernmentComplianceInvoiceDocument,
	ResString.GetMultilingualString("840672DF-CA29-4754-996C-D0815C28D3EA", "Default Maximum Number of Charges to Print per Compliance Document"),
	ResString.GetMultilingualString("2FAF8351-A5EC-4389-AF3F-7871A742FC7A", @"This setting sets the default maximum number of charge lines to print per compliance document.
This setting is relevant when the local compliance document can only be a single page document.
This setting is used in the Compliance Sequences module when configuring a Compliance Invoice Book series."),
	new NumericRegistryEditorInfo(0),
	RegistryStorageFlags.Company,
	RegistryOptions.Default,
	40, 1, 40);
				});
			}
		}

		public BooleanRegistryItem EnableComplianceDocumentModule
		{
			get
			{
				return GetItem("EnableComplianceDocumentModule", () =>
					new BooleanRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
							"EnableComplianceDocumentModule",
							Categories.Accounting_GovernmentComplianceInvoiceDocument,
							ResString.GetMultilingualString("BBEA073E-4820-4C8A-A986-AE6209A77287", "Enable Compliance Document Module (CW1 Support Only)"),
							ResString.GetMultilingualString("B0CB8463-CD24-41B8-8B54-072B6E36509A", @"This registry should only be enabled for Taiwan login companies only.

When this module is enabled, the compliance sub type and number will be assigned to individual invoice line instead of the invoice header. 
Thus allowing multiple compliance documents to be issued for a billing invoice. 
Alternatively, a compliance document can be issued for multiple billing invoices. 

For more information, please refer to the ""Compliance Document Module"" update note on My Account."),
							RegistryDataTypes.BoolType,
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue,
							(companyPK, branchPK, departmentPK)
								=> ObjectFactory.Get<ICompanyProvider>().WithFactory(FactoryForCountryDefaultValues).Get(companyPK).GetCountryCode().SomeOrDefault(string.Empty) == Constants.CountryCodes.Taiwan
						)
					)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan }
				);
			}
		}

		public BooleanRegistryItem AllowNegativeComplianceDocumentLines
		{
			get
			{
				return GetItem("AllowNegativeComplianceDocumentLines", () =>
					new BooleanRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
							"AllowNegativeComplianceDocumentLines",
							Categories.Accounting_GovernmentComplianceInvoiceDocument,
							ResString.GetMultilingualString("8C86B562-EB19-406C-851A-49F80493A454", "Allow Negative Compliance Document Lines"),
							ResString.GetMultilingualString("6C82B95E-0412-4D2B-8C7B-F57DE270CE8F", @"This registry is only relevant when the Compliance Document module is enabled.
By default, the system allows negative compliance document lines to be created for both AR and AP compliance documents.
You can override this registry to prevent users from creating compliance document containing negative compliance document lines."),
							RegistryDataTypes.BoolType,
							RegistryStorageFlags.Company,
							RegistryOptions.CacheExpensiveDefaultValue,
							(companyPK, branchPK, departmentPK)
								=> ObjectFactory.Get<ICompanyProvider>().WithFactory(FactoryForCountryDefaultValues).Get(companyPK).GetCountryCode().SomeOrDefault(string.Empty) != Constants.CountryCodes.Taiwan
						)
					)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan }
				);
			}
		}

		public BooleanRegistryItem AllowReActivationOfInactiveComplianceBook
		{
			get
			{
				return GetItem("AllowReActivationOfInactiveComplianceBook", () =>
				new BooleanRegistryItem(
					"AllowReActivationOfInactiveComplianceBook",
					Categories.Accounting_GovernmentComplianceInvoiceDocument,
					ResString.GetMultilingualString("d2d77d98-fab8-4ec6-8bd6-4d355c1b847f", "Allow re-activation of inactive compliance books"),
					ResString.GetMultilingualString("2e58caa9-c06c-4e24-8a72-be3dc6f7dc5c", @"This registry controls the ability to reactivate compliance books after they have been flagged inactive.
When set to 'Yes', users will be able to flag compliance books as inactive and active without restrictions.
When set to 'No', when un-ticking the 'Is Active' box in a compliance book, the system will warn users that this action cannot be reversed and once inactive, the book cannot be flagged as active again.
By default, the value of this registry is 'Yes'."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					true));
			}
		}

		public BooleanRegistryItem EnforceReceivablesComplianceDocumentDateandNumberSequencing
		{
			get
			{
				return GetItem("EnforceReceivablesComplianceDocumentDateandNumberSequencing", () =>
					new BooleanRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue("EnforceReceivablesComplianceDocumentDateandNumberSequencing",
							Categories.Accounting_GovernmentComplianceInvoiceDocument,
							ResString.GetMultilingualString("4037BEF1-23DB-42BA-B362-8EE91A8ABCB5", "Enforce Receivables Compliance Document Date and Number Sequencing"),
							ResString.GetMultilingualString("B88BB4D5-5D10-42EF-825E-DA0970C93ECF", @"When this registry is set to 'Yes', document number will not be allocate if the number and date sequence are out of sync for a given compliance invoice book.
Before running the allocation function, the document date will need to be adjusted."),
							RegistryDataTypes.BoolType,
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue,
							(companyPK, branchPK, departmentPK)
								=> ObjectFactory.Get<ICompanyProvider>().WithFactory(FactoryForCountryDefaultValues).Get(companyPK).GetCountryCode().SomeOrDefault(string.Empty) == Constants.CountryCodes.Taiwan
						)
					)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan }
				);
			}
		}

		public ComplianceDocumentRePrintRestrictionRegistryItem ComplianceDocumentRePrintRestriction
		{
			get
			{
				return GetItem("ComplianceDocumentRePrintRestriction", delegate
				{
					return new ComplianceDocumentRePrintRestrictionRegistryItem(
						"ComplianceDocumentRePrintRestriction",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("7363E3E3-95F0-4f6d-88B1-E55DBAC1C707", "Compliance Document Re-Print Restriction"),
						ResString.GetMultilingualString("199A460D-9501-4d42-9A36-B23DD1A6C79B", @"Use this registry to impose re-print restriction with reference to the Organization Category (i.e. Organization > Details > Details > Organization Category and Compliance Document Menu (i.e. Compliance Invoice Book > Compliance Document Menu).

This is useful when you are only allowed to re-print x number of times for certain compliance document depending on the debtor's organization category.

By default, there will be no re-print restriction. Users will be allowed to re-print compliance document as many times as required.

Note: This registry is only relevant when 'Enable Compliance Document Module' registry has been set to 'Yes' ."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan };
				});
			}
		}

		public ComplianceDocumentSupportingReasonRegistryItem ComplianceDocumentSupportingReasonsReceivables
		{
			get
			{
				return GetItem("ComplianceDocumentSupportingReasonsReceivables", delegate
				{
					return new ComplianceDocumentSupportingReasonRegistryItem(
						"ComplianceDocumentSupportingReasonsReceivables",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("BA809A3C-131E-4587-977B-EC7631DE628E", "Compliance Document Supporting Reasons - Receivables"),
						ResString.GetMultilingualString("B1D9AEFD-9098-4e62-B5D1-993D890BC3DF", @"This registry enable users to configure a set of supporting reason codes for A/R Compliance Document. Where applicable, the user should select the applicable code against the compliance document record.

For example, a reason must be specified to support the issuance of zero rated compliance document in Taiwan. The single digit numeric value is included in the T02 text file submitted to the Tax Bureau."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan };
				});
			}
		}

		public ComplianceDocumentSupportingReasonRegistryItem ComplianceDocumentSupportingReasonsPayables
		{
			get
			{
				return GetItem("ComplianceDocumentSupportingReasonsPayables", delegate
				{
					return new ComplianceDocumentSupportingReasonRegistryItem(
						"ComplianceDocumentSupportingReasonsPayables",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("E018C560-FA9E-45c9-830B-9100D5646095", "Compliance Document Supporting Reasons - Payables"),
						ResString.GetMultilingualString("B2AE2CE6-0EA1-4532-9EBD-5ADE6C65B2CB", @"This registry enable users to configure a set of supporting reason codes for Input Tax Claim. Where applicable, the user should select the applicable code against the compliance document record.

For example, the basis of input tax claim must be specified to A/P Compliance Document in Taiwan. The single digit numeric value is included in the TXT text file submitted to the Tax Bureau."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan };
				});
			}
		}

		public ComplianceDocumentSupportingReasonRegistryItem ComplianceDocumentSupportingDocumentType
		{
			get
			{
				return GetItem("ComplianceDocumentSupportingDocumentType", delegate
				{
					return new ComplianceDocumentSupportingReasonRegistryItem(
						"ComplianceDocumentSupportingDocumentType",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("8EE4C5B3-3B4E-4c4d-9FB2-AB931C6859B9", "Compliance Document Supporting Document Type"),
						ResString.GetMultilingualString("B1FEA3C1-6D2D-4881-9FF6-C556EBB1DD5D", @"This registry enable users to configure a set of document types and corresponding description that supports the tax filing where applicable."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan };
				});
			}
		}

		public ComplianceDocumentImageCollectionRegistryItem DocumentConfiguration
		{
			get
			{
				return GetItem("DocumentConfiguration", delegate
				{
					return new ComplianceDocumentImageCollectionRegistryItem(
						"DocumentConfiguration",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("B05CF22A-5F0B-428F-B1F1-4B848DE39436", "Compliance Document Configuration"),
						ResString.GetMultilingualString("CCF3E749-7327-46B9-8AD4-A49FCF5A729B", @"This registry enables users to add an image and remark (optionally) against each compliance sub type where applicable.
When specified, you could include this image and remark in the relevant compliance document template printed via Receivables Compliance Documents > Print menu."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default
						)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan };
				});
			}
		}

		public BooleanRegistryItem PromptToPrintComplianceDocumentOnCreation
		{
			get
			{
				return GetItem("PromptToPrintComplianceDocumentOnCreation", () =>
					new BooleanRegistryItem("PromptToPrintComplianceDocumentOnCreation",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("03CF1751-5FE8-4F97-A0A1-7AFBC7E22C99", "Prompt to print AR compliance document on creation"),
						ResString.GetMultilingualString("A5135A10-0956-4287-993C-139AB63A06A3", @"This registry is only relevant to system companies with the new Compliance Document enabled.
When this registry is set to 'Yes', the system will prompt the user whether to print the document on creation of the INV compliance document record.

Note: A document menu must be specified against the respective Compliance Sequence Book."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan }
					);
			}
		}

		public ImageRegistryItem ComplianceDocumentReceiptImageContainer
		{
			get
			{
				return GetItem("COMPLIANCE_DOCUMENT_RECEIPT_IMAGE_CONTAINER", delegate
				{
					return new ImageRegistryItem(
						"COMPLIANCE_DOCUMENT_RECEIPT_IMAGE_CONTAINER",
						null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
				});
			}
		}

		public CodePairRegistryItem ComplianceDocumentNumberAllocation_Receivables
		{
			get
			{
				return GetItem("ComplianceDocumentNumberAllocation", delegate
				{
					return new CodePairRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
							"ComplianceDocumentNumberAllocation",
							Categories.Accounting_GovernmentComplianceInvoiceDocument,
							ResString.GetMultilingualString("2798cd1a-068b-406f-993e-6c777fdd8f21", "Compliance Document Number Allocation - Receivables"),
							ResString.GetMultilingualString("3CF332C0-AB66-4811-8E46-2D7A1C257F6D", @"This registry defines the default Compliance Number allocation behavior for all Branches in a login company. The method chosen determines when a Compliance Sub Type Transaction Number will be assigned to a Receivables Invoice (INV), Credit Note (CRD) or Adjustment Note (ADJ) transaction. Not all Receivables Invoice and Credit Note transactions necessarily require a Government Compliance document or Government Compliance Transaction Number.

Where MAN (Manually Assign Compliance Number after Posting Transaction) is selected, users will need to use the 'Allocate Compliance Number' Actions option to add the Compliance Number to the transactions.

Note: This registry is only relevant in login countries where the login company must assign additional and secondary Government Compliance Number allocation rules for certain types of Receivables INV, CRD or ADJ transactions.

Please note: If required, additional Branch specific and Compliance Sub Type specific number allocation behaviors can be defined in the 'Compliance Document Number Allocation Override - Receivables' registry. The Override registry is relevant when different Branches or Sub Types in the one login company need different number allocation behavior."),
							new CodePairRegistryDataTypeWithAdditionalValidation(new CodeDescriptionPairListProvider(() => AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList), false, true, ValidateComplianceDocumentNumberAllocation_Receivables),
							RegistryStorageFlags.Company,
							ComplianceDocumentNumberAllocation_ReceivablesDefaultValueGetter
						)
					);
				});

				#region Local Helpers

				object ComplianceDocumentNumberAllocation_ReceivablesDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
				{
					var (countryCode, isEInvoicingEnabled) = GetCountryCodeAndEInvoicingEnabled(companyPK);

					var defaultProvider = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIComplianceRegistryDefaultProvider(countryCode);
					var defaultValueFromCountryCompliance = defaultProvider?.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(isEInvoicingEnabled);

					return string.IsNullOrEmpty(defaultValueFromCountryCompliance)
						? ComplianceSubTypeAllocationOverrideConfiguration.DefaultAllocationMethod
						: defaultValueFromCountryCompliance;
				}

				string ValidateComplianceDocumentNumberAllocation_Receivables(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
				{
					var (countryCode, isEInvoicingEnabled) = GetCountryCodeAndEInvoicingEnabled(companyPK);

					var defaultProvider = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIComplianceRegistryDefaultProvider(countryCode);
					var validationResultFromCountryCompliance = defaultProvider?.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(proposedValue, isEInvoicingEnabled);
					var validationResult = validationResultFromCountryCompliance ?? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(proposedValue, countryCode);
					return validationResult;
				}

				(ZString countryCode, bool isEInvoicingEnabled) GetCountryCodeAndEInvoicingEnabled(Guid companyPK)
				{
					var company = companyPK == GlbCompany.CurrentCompany.PK ? GlbCompany.CurrentCompany : new BusinessObjectFactory().Load<GlbCompany>(companyPK);
					var countryCode = company?.GC_RN_NKCountryCode ?? ZString.Empty;
					var isEInvoicingEnabled = companyPK != Guid.Empty && EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
					return (countryCode, isEInvoicingEnabled);
				}

				#endregion
			}
		}

		public ComplianceSubTypeAllocationOverrideConfigurationRegistryItem ComplianceDocumentNumberAllocationOverride_Receivables
			=> GetItem("ComplianceDocumentNumberAllocationOverride_Receivables", () =>
				new ComplianceSubTypeAllocationOverrideConfigurationRegistryItem(
					"ComplianceDocumentNumberAllocationOverride_Receivables",
					Categories.Accounting_GovernmentComplianceInvoiceDocument,
					ResString.GetMultilingualString("dd427f71-bdcb-4409-952b-64dedb53150a", "Compliance Document Number Allocation Override - Receivables"),
					ResString.GetMultilingualString("7679c951-8cf5-452a-9d36-8a37f110c7c1", @"This registry allows you to override the Allocation method set in the 'Compliance Document Number Allocation - Receivables' Registry.
Use this registry to set additional Branch specific and Compliance Sub Type specific number allocation behaviors from the ones set in the 'Compliance Document Number Allocation - Receivables' registry.
This registry is relevant when different Branches or Sub Types in the one login company need different number allocation behavior.

You must always review both registries to ensure your Login Company has been setup correctly."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default
				)
			);

		#region System Certifications

		#region Portugal

		public StringRegistryItem PTBillingSoftwareCertificateNumber
		{
			get
			{
				return GetItem("PTBillingSoftwareCertificateNumber", delegate
				{
					return new StringRegistryItem(
						"PTBillingSoftwareCertificateNumber",
						Categories.Accounting_SystemCertifications_Portugal,
						(NoResString)FormattableString.Invariant($"Billing Software Certificate Number({BrandingFactory.Instance.ProductName} Support Only)"),
						(NoResString)FormattableString.Invariant($"This registry contains {BrandingFactory.Instance.ProductName}'s software billing and accounting certification numbers issued by the relevant Local Authority in this country."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"0");
				});
			}
		}

		public StringRegistryItem PTBillingSoftwareCertificateNumberMessage
		{
			get
			{
				return GetItem("PTBillingSoftwareCertificateNumberMessage", delegate
				{
					return new StringRegistryItem(
						"PTBillingSoftwareCertificateNumberMessage",
						Categories.Accounting_SystemCertifications_Portugal,
						(NoResString)FormattableString.Invariant($"Billing Software Certificate Number Message({BrandingFactory.Instance.ProductName} Support Only)"),
						(NoResString)@"This registry is used to configure the country's certification message to be printed in invoice documents.
Some countries require that all printed documents issued by a certified software to include both the certification number and a related certification description.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						(NoResString)"-Processado por Programa Certificado no.");
				});
			}
		}

		public StringRegistryItem PTSoftwareName
		{
			get
			{
				return GetItem("PTSoftwareName", delegate
				{
					return new StringRegistryItem(
						"PTSoftwareName",
						Categories.Accounting_SystemCertifications_Portugal,
						(NoResString)FormattableString.Invariant($"Software Name({BrandingFactory.Instance.ProductName} Support Only)"),
						(NoResString)@"This registry is used to store the name of the software as required by tax authorities for legal reporting purposes.
The software name must match the one registered with the tax authorities of each respective country.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						(NoResString)FormattableString.Invariant($"{BrandingFactory.Instance.ProductName}/Wisetech Global Limited"));
				});
			}
		}

		#endregion

		#region Germany
		public StringRegistryItem DEWTGsProducerIDForELSTER
		{
			get
			{
				return GetItem("DEWTGsProducerIDForELSTER", delegate
				{
					return new StringRegistryItem(
						"DEWTGsProducerIDForELSTER",
						Categories.Accounting_SystemCertifications_Germany,
						(NoResString)FormattableString.Invariant($"WTGs Producer ID For ELSTER ({BrandingFactory.Instance.ProductName} Support Only)"),
						(NoResString)FormattableString.Invariant($"This registry contains WTG's software producer ID for {BrandingFactory.Instance.ProductName} for ELSTER reports issued by the German Fiscal Authorities."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"30624");
				});
			}
		}

		#endregion

		#region Israel

		public StringRegistryItem ILAccountingSoftwareNumber
		{
			get
			{
				return GetItem("ILAccountingSoftwareNumber", delegate
				{
					return new StringRegistryItem(
						"ILAccountingSoftwareNumber",
						Categories.Accounting_SystemCertifications_Israel,
						(NoResString)"Accounting Software Number (CargoWiseOne Support only)",
						(NoResString)$"This registry contains {BrandingFactory.Instance.ProductName}'s Software Accounting Certification Number issued by the relevant Local Authority in this country.",
						new NumericOnlyStringRegistryDataType { MaxLength = 8, MinLength = 0 },
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem ILAccountingSoftwareVersion
		{
			get
			{
				return GetItem("ILAccountingSoftwareVersion", delegate
				{
					return new StringRegistryItem(
						"ILAccountingSoftwareVersion",
						Categories.Accounting_SystemCertifications_Israel,
						(NoResString)"Accounting Software Version (CargoWiseOne Support only)",
						(NoResString)$"This registry contains {BrandingFactory.Instance.ProductName}'s Software Accounting Version.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						string.Empty)
					{
						DataType = new StringRegistryDataType(0, 50)
					};
				});
			}
		}

		#endregion

		#endregion

		public CodePairRegistryItem ComplianceDocumentNumberAllocation_Payables
		{
			get
			{
				return GetItem("ComplianceDocumentNumberAllocation_Payables", delegate
				{
					return new CodePairRegistryItem(
						"ComplianceDocumentNumberAllocation_Payables",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("978488c1-5029-43ea-8562-f7518d4e9e72", "Compliance Document Number Allocation - Payables"),
						ResString.GetMultilingualString("47F85EA2-3C7D-42A8-BA55-0C5712D4702C", @"This registry determines when a Compliance Sub Type Transaction Number will be assigned to a Payables Invoice (INV), Credit Note (CRD) or Adjustment Note (ADJ) transaction.  Not all Payables Invoice and Credit Note transactions necessarily require a Government Compliance document or Government Compliance Transaction Number.

This registry is only relevant to login countries/regions where businesses must assign and issue an additional and secondary Government Compliance Document for certain types of Payables INV, CRD and ADJ transactions."),
						new CodeDescriptionPairListProvider(() => AccComplianceSequenceLookups.APComplianceDocumentNumberAllocationSettingList),
						RegistryStorageFlags.Company,
						AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print);
				});
			}
		}

		#region ComplianceNumberSequenceConfiguration

		public ComplianceNumberSequenceConfigurationRegistryItem ComplianceNumberSequenceConfiguration
		{
			get
			{
				var item = GetItem("ComplianceNumberSequenceConfiguration", delegate
				{
					var hint = ResString.GetMultilingualString("ac4dbf81-00a2-48b0-a9b1-d84adf49c866", @"By default, when compliance sequence number is assigned to a transaction/compliance document record, the system will save the compliance book series prefix + sequence number.
For instance, assuming a sequence number has been allocated from compliance invoice book as follows:
1. Series Prefix = AB
2. Allocated Sequence Number = 00001053

Then the compliance document number AB00001053 will be saved against the transaction/compliance document record.

This registry enables you to configure one or more compliance number format and assign the applicable format to the respective compliance sequence book.

Explanation (Top Grid):
The top grid enables you to define the format code and describe its usage.
You can configure one or more format and configure the elements to be included.
Note: The specify Code cannot be 'DEF' as this is reserved for the default format (Series Prefix + Sequence Number).

Explanation (Bottom Grid):
The bottom grid enables you to configure the elements that should be included for each of the format code.
a. It is mandatory to include the 'Compliance Book Sequence Number' so this has been ticked by default.
b. To include an element, tick the corresponding check box.
c. Use the 'Order' column to specify the order of the element. E.g. 1, 2, 3, etc.");
					return new ComplianceNumberSequenceConfigurationRegistryItem(
						"ComplianceNumberSequenceConfiguration",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("52dd6ad9-d00b-4908-a9f7-8c715f18698f", "Compliance Number Sequence Configuration"),
						hint,
						RegistryStorageFlags.Company,
						RegistryOptions.MustOverrideDefaultValue);
				});

				item.OnBuildLogReference += BuildComplianceNumberSequenceConfigurationChangeLogReference;
				return item;
			}
		}

		string BuildComplianceNumberSequenceConfigurationChangeLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var newConfigurationCollection = ((ComplianceNumberSequenceConfigurationCollection)args.NewValue).Cast<ComplianceNumberSequenceConfiguration>();
			var originalConfigurationCollection = ((ComplianceNumberSequenceConfigurationCollection)args.OriginalValue).Cast<ComplianceNumberSequenceConfiguration>();

			foreach (var configuration in originalConfigurationCollection)
			{
				if (!newConfigurationCollection.Any(x => x.Code == configuration.Code))
				{
					result += Res.GetString("7be7285d-f724-4f5f-9194-a0fd97503aee", "Code '{0}' Deleted", configuration.Code) + "\r\n";
				}
			}

			foreach (var newConfiguration in newConfigurationCollection)
			{
				if (!originalConfigurationCollection.Any(x => x.Code == newConfiguration.Code))
				{
					var elementsLog = newConfiguration.GetElementsChangedLog();
					result += Res.GetString("ad50efed-4003-42e9-af69-016f916f67e8", "Code '{0}' Added : {1}", newConfiguration.Code, elementsLog) + "\r\n";
				}
				else
				{
					var originalConfiguration = originalConfigurationCollection.First(x => x.Code == newConfiguration.Code);

					if (originalConfiguration.Description != newConfiguration.Description)
					{
						result += Res.GetString("80ecdcab-4910-4334-b7bf-6aee76a74319", "Code '{0}' : Description changed from '{1}' to '{2}'.", newConfiguration.Code, originalConfiguration.Description, newConfiguration.Description) + "\r\n";
					}

					var originalElements = originalConfiguration.Elements;
					var newElements = newConfiguration.Elements;
					var hasChanges = newElements.Cast<ComplianceNumberSequenceCustomisationElement>().Any(x => x.Order != originalElements[x.ElementName].Order
																											 || x.Include != originalElements[x.ElementName].Include
																											 || x.DigitCode != originalElements[x.ElementName].DigitCode
																											 || x.Length != originalElements[x.ElementName].Length);
					if (hasChanges)
					{
						var elementsLog = newConfiguration.GetElementsChangedLog();
						result += Res.GetString("82dc74dd-b454-4f0a-8785-07381673c811", "Code '{0}' Changed : {1}", newConfiguration.Code, elementsLog) + "\r\n";
					}
				}
			}

			return result;
		}

		#endregion

		public ComplianceSubTypeAttributionRuleSetRegistryItem ComplianceSubTypeAttributionRuleSet
		{
			get
			{
				return GetItem("ComplianceSubTypeAttributionRuleSet", () =>
				{
					var item = new ComplianceSubTypeAttributionRuleSetRegistryItem("ComplianceSubTypeAttributionRuleSet",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("F4DF7409-5103-4F58-90BE-371818AE906D", "Compliance Sub Type Attribution Rule Set"),
						ResString.GetMultilingualString("678E35D7-A3BA-4126-87C9-77F66CB9E551", @"Use this registry to choose the Compliance Sub Type defaulting rule set to be applied to your Login Company. 

Note: This registry is only relevant to login countries where Government Compliance Sub Type and  Document features are enabled AND where more than one Compliance Sub Type Defaulting rule set is supported.

This registry determines the Compliance Sub Type Defaulting Rule set used by a Login Company as Invoice (INV) and Credit Note (CRD) transactions are posted."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default);

					item.OnBuildLogReference += (args) => Res.GetString("CBCB2F46-F811-46B3-A3E3-0C6558D64B1C", "Rule Set changed from {0} to Rule Set {1}.", ((args.OriginalValue as ComplianceSubTypeAttributionRuleSet).RuleSetCode), (args.NewValue as ComplianceSubTypeAttributionRuleSet).RuleSetCode);

					return item;
				});
			}
		}

		public ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryItem ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch
		{
			get
			{
				return GetItem("ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch", () =>
				{
					var item = new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranchRegistryItem("ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("534EF2F0-662B-4824-950E-5BA19EF296FF", "Compliance Sub Type Attribution Rule Set – By Transaction Header Branch"),
						ResString.GetMultilingualString("2271E401-F407-49E8-9E8B-067DDDD87075", @"Use this registry to choose the Compliance Sub Type defaulting rule set to be applied with reference to Transaction Header Branch. 

Note:
1. This registry is only relevant to login countries where Government Compliance Sub Type and Document features are enabled AND where more than one Compliance Sub Type Defaulting rule set is supported.
2. This registry currently applies to China login companies only.
3. If a rule set is not specified for a Branch here, the system will fall back to the rule set specified in ‘Compliance Sub Type Attribution Rule Set’ registry."),
						RegistryStorageFlags.Branch,
						RegistryOptions.Default);

					item.OnBuildLogReference += (args) => Res.GetString("36AF1E0F-F69A-49C4-8B1F-4A3D6C40FE19", "Rule Set changed from {0} to Rule Set {1}.", ((args.OriginalValue as ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch).RuleSetCode), (args.NewValue as ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch).RuleSetCode);
					item.CountryFilterPKs = CountryFilterPKs.China;

					return item;
				});
			}
		}

		public ComplianceSubTypeAttributionRuleConfigurationRegistryItem ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch
		{
			get
			{
				return GetItem("ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch", () =>
				{
					var item = new ComplianceSubTypeAttributionRuleConfigurationRegistryItem("ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Compliance Sub Type Attribution Rule Configuration – By Transaction Header Branch(CWSupport Only)",
						(NoResString)@"A Sub Type will be set against an Invoice, Credit Note or Adjustment Note transaction header when the transaction details match a Sub Type’s Attribution Rules.

The Attribution Rules of a Sub Type identify those transactions that will require an additional Government Compliance Document. Not all Invoices and Credit Note transactions necessarily require a Government Compliance document. 
 
Note:
1. This registry is only relevant to those login countries where businesses must assign and issue Government mandated documents and transactions number for certain types of INV, CRD, and ADJ transactions.
2. This registry currently applies to China login companies only.
3. If an attribution rule is not configured for a Branch here, the system will fall back to the attribution rule configured in ‘Compliance Sub Type Attribution Rule Configuration’ registry.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport);

					item.CountryFilterPKs = CountryFilterPKs.China;

					return item;
				});
			}
		}

		public DateTimeRegistryItem LastUTCDateToDisableComplianceBookAfterDbRestored
		{
			get
			{
				return GetItem("LastUTCDateToDisableComplianceBookAfterDbRestored",
					() => new DateTimeRegistryItem(
						"LastUTCDateToDisableComplianceBookAfterDbRestored",
						Categories.Accounting,
						(NoResString)"Last UTC Date to Disable Compliance Book After Db Restored (Developer Only)",
						(NoResString)@"Portugal Companies needs to receive a notification when database are restored.
Sensitive Accounting data (compliance sequence books for example) will be disabled.
This registry is to store last disabled UTC date.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						DateTime.MinValue));
			}
		}

		#region SuppressResourceStringsCheckRegion

		public ComplianceSubTypeAttributionRuleConfigurationRegistryItem ComplianceSubTypeAttributionRuleConfiguration
		{
			get
			{
				return GetItem("ComplianceSubTypeAttributionRuleConfiguration", delegate
				{
					MultilingualString hint = (NoResString)@"A Sub Type will be set against an Invoice, Credit Note or Adjustment Note transaction header when the transaction details match a Sub Type’s Attribution Rules.

This registry is only relevant to those login countries where businesses must assign and issue government mandated documents and transaction numbers for certain types of INV, CRD and ADJ transactions.

The Attribution Rules of a Sub Type identify those transactions that will require an additional Government Compliance Document.
Not all Invoice and Credit Note transactions necessarily require a Government Compliance document.";
					return new ComplianceSubTypeAttributionRuleConfigurationRegistryItem(
						"ComplianceSubTypeAttributionRuleConfiguration",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Compliance Sub Type Attribution Rule Configuration (CargoWiseOne Support Only)",
						hint,
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public ComplianceSubTypeDependencyConfigurationRegistryItem ComplianceSubTypeDependencyConfiguration
		{
			get
			{
				return GetItem("ComplianceSubTypeDependencyConfiguration", delegate
				{
					MultilingualString hint = (NoResString)@"This registry is only relevant to those login countries where Compliance Sub Type behaviours have been enabled
AND a Compliance Sequence Book must be shared by more than one Sub Type. 

When configured, this registry will prevent creation of books for specific sub types
AND identify the Compliance Book Sub Type to be used when allocating compliance numbers.";
					return new ComplianceSubTypeDependencyConfigurationRegistryItem(
						"ComplianceSubTypeDependencyConfiguration",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Compliance Sub Types That Draw Numbers from Other SubType Books (CargoWiseOne Support Only)",
						hint,
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public ComplianceReportConfigurationRegistryItem ComplianceReportConfiguration
		{
			get
			{
				return GetItem("ComplianceReportConfiguration", delegate
				{
					MultilingualString hint = (NoResString)@"This registry defines the Compliance Reporting Options available to a Login Company in the Compliance Reports Module.
The reporting requirements and formats configured here will vary Login Country to Login Country.

Periodically, a Login Company can be required to periodically report to government agencies details about Sales and Purchase transactions. For example: Sales Invoice Listings, Reporting of Trade with Black List Countries, Purchase Invoice Listings.

When configured, this registry defines the distinct local reporting obligations of a Login Company.
The top grid defines a Compliance Reporting obligation,
The bottom grid defines the types of transactions that must be included in that report.

As Receivables and Payables transactions meeting the parameters defined against a Compliance Reporting Obligation are posted, those  transactions are added to the relevant Compliance Reports Module Reporting Queues.

The ‘Include previous queued records’ setting is used to determine if transactions from earlier periods should be included in the current compliance report or not.
This setting also changes the behaviour in which Compliance Reports can be created and finalised. When checked, Compliance Reports can only be generated after the previous reporting period has been finalised. If left unchecked, reports can be created in any sequence regardless of finalisation.

The Portuguese ‘SAFT’ report must have Code SAF. The Italian ‘Esterometro’ report must have code EST.
Required report codes for Germany: UVA - 'UstVA', U11 - 'Ust 1/11, ZMD - 'ZM'. Required group by codes for Germany: HRS - 'UstVA' and 'ZM', RSC - 'Ust 1/11.
The French ‘FEC’ report must have Code FEC.";

					return new ComplianceReportConfigurationRegistryItem(
						"ComplianceReportConfiguration",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Compliance Report Configuration (CargoWiseOne Support Only)",
						hint,
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem ComplianceReportConfigurationRootFileExportPath
		{
			get
			{
				return GetItem("ComplianceReportConfigurationRootFileExportPath", delegate
				{
					var result = new StringRegistryItem(
						"ComplianceReportConfigurationRootFileExportPath",
						Categories.Accounting_GovernmentComplianceInvoiceDocument_Israel,
						(NoResString)"Open Format - Root File Export Path (CargoWiseOne Support Only)",
						(NoResString)@"This setting defines the path for exporting the OP file structure (only relevant for Israel company).
With this setting, only the root is defined.

A basic folder with the name ‘OPENFRMT’ will be created on this path.
During the export process, a first subfolder (VAT.YY) will be created; VAT corresponds to the first 8 numbers of the VAT register; YY corresponds to the report's production year.
There will also be a subfolder (MMddHHmm) containing the files BKMVDATA.txt and INI.txt. ‘MMddHHmm’ is taken from the production date of the report.

Example, VAT = 512312115, production year 2022 at 05.01.2022 14:55 will be stored in \OPENFRM\51231211.22\01051455\",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
					result.CountryFilterPKs = CountryFilterPKs.Israel;
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableFinalisedComplianceDocumentToBeSpecialVoided
		{
			get
			{
				var item = GetItem("EnableFinalisedComplianceDocumentToBeSpecialVoided", () =>
					new BooleanRegistryItem("EnableFinalisedComplianceDocumentToBeSpecialVoided",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Allow finalized compliance document records to be special voided",
						(NoResString)@"This registry is only relevant to those login companies where the Compliance Document Module has been enabled.

By default, 'Finalized' compliance document records are not allowed to be voided.
When this registry is set to 'Yes', users will be allowed to void such compliance records via 'Special Voiding'.

Note: No changes will be made to the finalized Compliance Report that contains Compliance Document Records have been Special Voiding.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));

				item.OnBuildLogReference += (args) => Res.GetString("09A5F97F-3A01-4C76-A2BF-BE506B88590E", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

				return item;
			}
		}

		public AESEncryptionKey128RegistryItem AESEncryptionKey
		{
			get
			{
				return GetItem("AESEncryptionKey", () =>
					new AESEncryptionKey128RegistryItem("AESEncryptionKey",
					Categories.Accounting_GovernmentComplianceInvoiceDocument,
					(NoResString)"AES Encryption Key",
					(NoResString)@"This registry is relevant to Taiwan Login Company only who is using the new Compliance Document Module.

In Taiwan, the AES Encryption Key will be used to encrypt 10 alpha-numeric compliance document number and 4 digits random number for added security stored against the compliance document record using Base64 encoding conversion. The encrypted details will be included in the data elements for the generation of QRCodes for TXE-Electronic GUI.

In Taiwan, CW1 user will receive a password from the Tax Bureau. CW1 user should then contact and provide this password to WTG Taiwan. WTG Taiwan will use this information to generate the AES Encryption Key using the tool provided by the Tax Bureau, then save this value into this registry. This usually happens during the initial system implementation only. ",
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport));
			}
		}

		public BooleanRegistryItem OrphanWIPOrACRDetection
		{
			get
			{
				return GetItem("OrphanWIPOrACR", delegate
				{
					return new BooleanRegistryItem(
						"OrphanWIPOrACR",
						Categories.Accounting_DataConsistencyCheck,
						(NoResString)"Orphan WIP or ACR detection (Developer Only)",
						(NoResString)@"This registry is for indicative purpose. Please do not update the value.

This registry is automatically set to 'Yes'  by the service task (ADC) when there are WIPs and/or ACRs that have not been reversed and are not listed in Billing > Invoicing tab.  

A new filter 'Show Only Orphan Transactions' will be visible in Accounts > Job Costing > WIPs and Accruals module when this registry value has been set to 'Yes'.

With this filter, users can locate these orphan transactions and reverse them. 
When all orphan transactions have been reversed, the service task will indicate in the log that the orphan issue has been resolved and update the registry value to 'No'.",
						RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, false);
				});
			}
		}

		#endregion

		public BooleanRegistryItem ComplianceAllowPartialSequenceNumberAllocation
		{
			get
			{
				return GetItem("ComplianceAllowPartialSequenceNumberAllocation", () =>
					new BooleanRegistryItem("ComplianceAllowPartialSequenceNumberAllocation",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("884ba215-155d-48b1-a0b9-f1623b1b96fa", "Allow Partial Allocation of Compliance Sequence Number"),
						ResString.GetMultilingualString("a31fc19b-a679-4415-88b3-4eb86a48d60e", @"When allocating numbers to multiple transactions at once, this registry defines how {0} will behave when there are not enough numbers remaining in the relevant compliance invoice books to successfully assign each selected transaction an appropriate number.
By default, when there are not enough numbers available No Allocation to any transaction will be made.  The user will be shown a message and allowed to change the set of transactions selected for allocation.
Alternatively, when this registry is overridden and set to Yes, numbers will be assigned where possible until the relevant books have been exhausted.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company,
						false));
			}
		}

		#region Compliance Number Allocation Date

		public string GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum ledger, ZGuid companyPK)
				=> GetComplianceNumberAllocationDateRegistryItem(ledger)?.GetFallBackValueAtAllLevels(companyPK.IsValid ? companyPK.ToGuid() : GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		CodePairRegistryItem GetComplianceNumberAllocationDateRegistryItem(ComplianceNumberAllocationDateValidLedgerEnum ledger)
		{
			switch (ledger)
			{
				case ComplianceNumberAllocationDateValidLedgerEnum.AR:
					return ComplianceNumberAllocationDate_AR;

				case ComplianceNumberAllocationDateValidLedgerEnum.AP:
					return ComplianceNumberAllocationDate_AP;

				default:
					ErrorReporter.ReportOnce((NoResString)FormattableString.Invariant($"The ledger {Enum.GetName(typeof(ComplianceNumberAllocationDateValidLedgerEnum), ledger)} is not supported by ComplianceNumberAllocationDate"));
					return null;
			}
		}

		public CodePairRegistryItem ComplianceNumberAllocationDate_AR
		{
			get
			{
				return GetItem("ComplianceNumberAllocationDate_AR", delegate
				{
					return new CodePairRegistryItem(
						new CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<string>(
							"ComplianceNumberAllocationDate_AR",
							Categories.Accounting_GovernmentComplianceInvoiceDocument,
							(NoResString)"Compliance Number Allocation Rule - Receivables",
							(NoResString)@"This registry allows you to define the relevant date that the system must use to allocate compliance numbers in Receivables Invoice (INV), Credit Note (CRD), or Adjustment Note (ADJ) transactions.

There are three options for allocating the compliance number:
1  NOT - No Date Order Enforced: The system allocates the compliance number according to the order in which the allocation is performed.
2. INV - Invoice Date Order: The system allocates the compliance number in order by Invoice Date.
3. PST - Post Date Order: The system allocates the compliance number in order by Post Date.

This registry also defines the rules that the system applies before allocating the compliance number.

When the registry is set to 'NOT':
  - The system will perform only basic validations before allocating a compliance number:
  - The compliance subtype linked to the transaction must have a valid compliance sequence in Maintain > Account > Compliance Sequences.
  - The compliance number must be unique and within the allowed range, based on the Compliance Sequences linked to the transactions.

When the registry is set to 'PST' or 'INV':
  - The system will perform the following validations before allocating a compliance number based on the relevant date (Post Date or Invoice Date):
  - The compliance subtype linked to the transaction must have a valid compliance sequence in Maintain > Account > Compliance Sequences.
  - The compliance number must be unique and within the allowed range, based on the Compliance Sequences linked to the transaction.
  - All transactions with a relevant date earlier than the current transaction must already have a compliance number allocated.
  - The relevant date must be greater than or equal to the last transaction posted with the same compliance sequence.

If validations fail, the system will prevent the allocation of the compliance number.

Additionally, when this registry is set to 'PST' or 'INV', if the registry Accounting > Government Compliance Invoice Document > Compliance Document Number Allocation - Receivables is set to 'PST - Assign Compliance Number when Posting Transaction', then posting is prevented if any of the above validations fail.

These validations ensure that the compliance numbers are assigned in the specified order.",
							RegistryDataTypes.StringType,
							new ComboBoxRegistryEditorInfo(new CodeDescriptionPairListProvider(() => new ComplianceNumberAllocationDateOptions())),
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							(complianceInfo) => (complianceInfo as IComplianceRegistryDefaultProvider)?.GetDefaultValueForComplianceNumberAllocationDateRegistry() ?? ComplianceNumberAllocationDateOptions.NoControl.Code,
							ComplianceNumberAllocationDateOptions.NoControl.Code));
				});
			}
		}

		public CodePairRegistryItem ComplianceNumberAllocationDate_AP
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() =>
				{
					return new CodeDescriptionPairList() { ComplianceNumberAllocationDateOptions.NoControl, ComplianceNumberAllocationDateOptions.PostDate };
				});

				return GetItem("ComplianceNumberAllocationDate_AP", delegate
				{
					return new CodePairRegistryItem(
						"ComplianceNumberAllocationDate_AP",
				Categories.Accounting_GovernmentComplianceInvoiceDocument,
				(NoResString)"Compliance Number Allocation Rule - Payables",
				(NoResString)@"This registry allows you to define the relevant date that the system must use to allocate compliance numbers in Payables Invoice (INV), Credit Note (CRD), or Adjustment Note (ADJ) transactions.

There are three options for allocating the compliance number:
1  NOT - No Date Order Enforced: The system allocates the compliance number according to the order in which the allocation is performed.
2. PST - Post Date Order: The system allocates the compliance number in order by Post Date.


This registry also defines the rules that the system applies before allocating the compliance number.

When the registry is set to 'NOT':
  - The system will perform only basic validations before allocating a compliance number:
  - The compliance subtype linked to the transaction must have a valid compliance sequence in Maintain > Account > Compliance Sequences.
  - The compliance number must be unique and within the allowed range, based on the Compliance Sequences linked to the transactions.

When the registry is set to 'PST' :
  - The system will perform the following validations before allocating a compliance number based on the relevant date (Post Date or Invoice Date):
  - The compliance subtype linked to the transaction must have a valid compliance sequence in Maintain > Account > Compliance Sequences.
  - The compliance number must be unique and within the allowed range, based on the Compliance Sequences linked to the transaction.
  - All transactions with a relevant date earlier than the current transaction must already have a compliance number allocated.
  - The relevant date must be greater than or equal to the last transaction posted with the same compliance sequence.

If validations fail, the system will prevent the allocation of the compliance number.

Additionally, when this registry is set to 'PST' , if the registry Accounting > Government Compliance Invoice Document > Compliance Document Number Allocation - Payables is set to 'PST - Assign Compliance Number when Posting Transaction', then posting is prevented if any of the above validations fail.

These validations ensure that the compliance numbers are assigned in the specified order.",
				listProvider,
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				ComplianceNumberAllocationDateOptions.NoControl.Code);
				});
			}
		}

		#endregion

		public BooleanRegistryItem SuppressShowComplianceBookHasNoTemplateWarning
			=> GetItem("SuppressShowComplianceBookHasNoTemplateWarning", () =>
				new BooleanRegistryItem(
					new CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<bool>(
						"SuppressShowComplianceBookHasNoTemplateWarning",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("ac34fe0d-073e-4e45-b279-6bda6afa0bff", "Suppress Warning When Compliance Book Has No Document Menu"),
						ResString.GetMultilingualString("2e04c8cb-e682-47ab-bc5e-f628f646f769", @"By default {0} will warn a user when they attempt to assign and ‘Print’ Government Compliance Numbers using Compliance Invoice Books that do NOT have an assigned document menu.

When overridden and set to ‘Yes’ the warning message will not show.
Configure this registry to ‘Yes’ when no document is required and the Compliance Invoice Books will only be used to assign compliance numbers.", BrandingFactory.Instance.ProductName),
						RegistryDataTypes.BoolType,
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						(complianceInfo) => (complianceInfo as IComplianceRegistryDefaultProvider)?.GetDefaultValueForSuppressShowComplianceBookHasNoTemplateWarningRegistry() ?? false,
						false
					)
				)
			);

		public IntRegistryItem ComplianceReportFileNextSequenceNumber
		{
			get
			{
				return GetItem("ComplianceReportFileNextSequenceNumber", () =>
					new IntRegistryItem(name: "ComplianceReportFileNextSequenceNumber",
						category: Categories.Accounting_GovernmentComplianceInvoiceDocument,
						caption: (NoResString)"Compliance Report File Next Number (CargoWiseOne Support Only)",
						hint: (NoResString)@"This registry allows you to specify next sequential number to be used for generating Compliance Report File.
A sequential number is assigned to each file generated from a specific Report Type.
Currently, this registry is considered only when generating XML file for ‘EST - Esterometro’ report in Italy login companies.

Note: In Italy Esterometro XML, File Number (ProgressivoInvio) is a 5-character alphanumeric value. Therefore, the number in this registry is converted to base-36 text on the export file. As an example, numbers 1-9 will generate corresponding values 00001-00009 on the export file, but number 10 in this registry will generate the value 0000A on the export file.
To set this registry to the desired File Number, convert that base-36 value to a decimal first.
For example, to set next file number to ""10000"", set this registry to 1679616.",
						storage: RegistryStorageFlags.Company,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: 1,
						minValue: 0,
						maxValue: int.MaxValue
					)
				);
			}
		}

		public BooleanRegistryItem AllowFrenchFECComplianceReportToUseStandardChartofAccounts
		{
			get
			{
				return GetItem("AllowFrenchFECComplianceReportToUseStandardChartofAccounts", () =>
					new BooleanRegistryItem(name: "AllowFrenchFECComplianceReportToUseStandardChartofAccounts",
						category: Categories.Accounting_GovernmentComplianceInvoiceDocument,
						caption: ResString.GetMultilingualString("951F5E5E-E7B5-47ED-B20E-AFDE4FA6064A", "Allow French FEC compliance report to use the standard chart of accounts"),
						hint: ResString.GetMultilingualString("1F81D3D6-8261-4952-9295-17378004C124", "This registry should only be activated when the French “Plan comptable general” is used as the standard chart of accounts in CW. When activated the accounts in the standard chart of accounts in CW will be used for the French FEC audit file and not the mapped accounts from the “GL Multi-Language Mapping” functionality."),
						storage: RegistryStorageFlags.Company,
						defaultValue: false)
					{ CountryFilterPKs = CountryFilterPKs.France });
			}
		}

		#region CreditNoteComplianceDocumentConfiguration

		public BooleanRegistryItem CreditNoteComplianceDocumentConfiguration
		{
			get
			{
				return GetItem("CreditNoteComplianceDocumentConfiguration", () =>
					new BooleanRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue("CreditNoteComplianceDocumentConfiguration",
							Categories.Accounting_GovernmentComplianceInvoiceDocument,
							(NoResString)"Credit Note Compliance Document Configuration",
							(NoResString)@"This registry is only relevant to those login countries where the Compliance Document Module has been enabled.
This registry applies to ‘Receivable Credit Note Compliance Document’ only.
By default, this registry is set to ‘No’ in which case the compliance sequence number will draw the number from respective invoice book based on compliance sub type and allocation level. Further, the change in behavior relating to setting this registry to ‘Yes’ will not be applied.

When this registry is set to ‘Yes’, the following change will be applied to the compliance document relating to Credit Note transactions.
	1.	Allocation of compliance sequence number will not be applied. The ‘Compliance Book’ field in Compliance Document screen will be left empty, read only and no validation will be applied.
	2.	The ‘Document Number’ field will be editable, and user will be able to manually enter a value if the field is empty.
	3.	When users create a Credit Note via ‘Amend with Credit Note’ function in Job / Consol > AR Invoices tab or when users create a Credit Note with Original Reference specified in the Receivables Transactions module, the system will behave as follows:
			•Users will not be allowed to amend an invoice with credit note if the selected invoice contains transaction lines with tax id and one or more of these lines does not have a compliance record with document number allocated.
			•Users will not be able to add new charge line nor edit charge code, tax id, job number of existing transaction lines. Deleting of existing transaction lines is allowed.
			•Compliance document record will be created roll up by charge code regardless of the debtor’s ‘Create Compliance Document Record on Posting’ setting.
			•Credit Note Compliance Document’s document number will derive from the respective invoice line’s compliance document.",
							RegistryDataTypes.BoolType,
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue,
							(companyPK, branchPK, departmentPK)
								=> ObjectFactory.Get<ICompanyProvider>().WithFactory(FactoryForCountryDefaultValues).Get(companyPK).GetCountryCode().SomeOrDefault(string.Empty) == Constants.CountryCodes.Taiwan
								&& Instance.EnableComplianceDocumentModule.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty)
						)
					)
					{ CountryFilterPKs = CountryFilterPKs.Taiwan }
				);
			}
		}

		#endregion

		#endregion

		#region Gateway Consol Job Invoicing

		public ChargeCodeListRegistryItem GatewayBillingChargeCodes
		{
			get
			{
				return GetItem("GatewayBillingChargeCodes", delegate
				{
					return new ChargeCodeListRegistryItem(
						"GatewayBillingChargeCodes",
						Categories.Accounting_JobInvoicing_GatewayConsolJobInvoicing,
						ResString.GetMultilingualString("93F3C630-C8A6-4DD2-A5D2-50F7AEFE64AE", "Gateway Billing Charge Codes"),
						ResString.GetMultilingualString("81944BA9-A936-4D68-8A61-DCF8564A7BBC", @"This registry allows you to nominate charge codes that are related to gateway operations.
For gateway agents importing intercompany invoices, gateway related charge codes are imported as costs on gateway billing job, while other charges are imported as forwarding (non - gateway) costs.
For forwarding(non-gateway) agents who bill gateway agents, using gateway related charge codes allows you to post gateway agent AR invoice  separately from posting other overseas agent charges.
Note that when this registry is set to the Default value, ALL charge codes are treated as gateway-related. If you are using a designated set of charge codes for gateway operations, set this registry to Override."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory,
						string.Empty,
						RegistryFindBoxFilter.None);
				});
			}
		}

		#endregion

		#region AutoRating Date Filtering

		public AutoRateDateByChargeGroupRegistryItem AutoRateDateByChargeGroupSetup
		{
			get
			{
				return GetItem("AutoRatingDateFiltering", delegate
				{
					var defaultConfig = new AutoRateDateByChargeGroupConfiguration();
					defaultConfig.FilterType = Constants.RatingDateFilterTypes.Codes.Standard;
					return new AutoRateDateByChargeGroupRegistryItem(
						"AutoRatingDateFiltering",
						Categories.AutoRating_ChargeCodeGroups,
						ResString.GetMultilingualString("c398bf44-344f-4f8a-8dba-88f9117d7754", "AutoRating Date Filtering"),
						ResString.GetMultilingualString("be524e2b-fd36-4073-a050-f702cc6c114c", @"This setting allows you to nominate which job dates are used when finding rates for specific charge code groups.

Note: For Container Yard charges, behavior as below:

STD: Use Yard/Gate In Date for Handling/Gate In charges and Yard/Gate Out Date for Handling/ Gate Out charges.

DEP: Use Gate Out Date for Gate In and Out charges, and use Yard in Date for Handling In Charge and Yard Out Date for Handling Out Charges.

ARR: Use Gate In Date for Gate In and Out charges, and use Yard in Date for Handling In Charge and Yard Out Date for Handling Out Charges."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultConfig);
				});
			}
		}

		#endregion

		public ARDefaultTaxRecognitionRuleRegistryItem ARDefaultTaxRecognitionRule
		{
			get
			{
				ARDefaultTaxRecognitionRuleRegistryItem item = GetItem("ARDefaultTaxRecognitionRule", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("20f20da7-49e0-4431-a80c-4d8e9076af7f", "This registry allows you to specify the Receivables Organization Default Tax Recognition with reference to the current login company's country/region and the receivables organization's country/region.");
					return new ARDefaultTaxRecognitionRuleRegistryItem(
						"ARDefaultTaxRecognitionRule",
						OrganisationsDataRegistry.Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("bd09931a-356c-4e4c-a96c-e7698eec663d", "Receivables  Default Tax Recognition Rule"),
						hint,
						RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
				return item;
			}
		}

		public APDefaultTaxRecognitionRuleRegistryItem APDefaultTaxRecognitionRule
		{
			get
			{
				APDefaultTaxRecognitionRuleRegistryItem item = GetItem("APDefaultTaxRecognitionRule", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("68ddd12a-adc3-4a5b-9313-94348c661945", "This registry allows you to specify the Payables Organization Default Tax Recognition with reference to the current login company's country/region and the payables organization's country/region.");
					return new APDefaultTaxRecognitionRuleRegistryItem(
						"APDefaultTaxRecognitionRule",
						OrganisationsDataRegistry.Categories.Organizations_DefaultValues,
						ResString.GetMultilingualString("74c6d07f-b11a-4052-b4b7-d96232b8b5e7", "Payables Default Tax Recognition Rule"),
						hint,
						RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
				return item;
			}
		}

		public BooleanRegistryItem PreventInvoiceDateGreaterThanPostDate
		{
			get
			{
				return GetItem("PreventInvoiceDateGreaterThanPostDate", () =>
				new BooleanRegistryItem("PreventInvoiceDateGreaterThanPostDate",
				Categories.Accounting_PayableDefaults_DefaultSettings,
				ResString.GetMultilingualString("CC65A9A7-7BC4-4D2D-A80F-D8E82F7A4BC9", "Prevent posting Invoice Date greater than Post Date"),
				ResString.GetMultilingualString("B4C64458-62BD-47FC-91D0-82C104F0DD1B", @"This registry can be used to prevent posting AP transactions with Invoice Date greater than Post Date. By default, the system allows you to select an Invoice Date which is greater than the post date. This can happen if you back date the Post Date of the transaction.

Set this registry to YES in order to prevent users from posting AP invoices, credit note and adjustments where invoice date is greater than the post date."),
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false));
			}
		}

		public BooleanRegistryItem DocumentReceivedDateMustBeEntered
		{
			get
			{
				return GetItem("DocumentReceivedDateMustBeEntered", delegate
				{
					var item = new BooleanRegistryItem("DocumentReceivedDateMustBeEntered",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("36C0E4A6-EDB5-431A-A111-A991B946D7A5", "Document Received Date Must Be Entered"),
						ResString.GetMultilingualString("1EA5ADA1-00F0-454E-928F-FAFF470F5637", @"When overridden to ‘Yes’, users will be required to record a document received date against each Payable Invoice.
By default, the recording of document received date is optional."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);

					item.OnBuildLogReference += (args) => Res.GetString("ED0A1743-54E7-4E44-ABB9-ECF8FB5CE193", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		public CodePairRegistryItem InvoiceDateDefaultValue
		{
			get
			{
				return GetItem("InvoiceDateDefaultValue", delegate
				{
					return new CodePairRegistryItem(
						"InvoiceDateDefaultValue",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("8325B74D-7628-46A0-A299-D0E5F377E177", "Invoice Date Default Value"),
						ResString.GetMultilingualString("39D1F63E-3FFF-489E-9377-AC76E0D41173", @"This registry defines the default Invoice Date value for Payable Invoice (INV), Credit Note (CRD) and Adjustment Note (ADJ) transactions.

By default, the registry value is ‘ADD – Invoice Add Date’ and the Invoice Date defaults to the date the invoice is added.

Set the registry to ‘BLK - Blank’ if you do not wish to default the invoice date. This option forces the user to enter the required Invoice Date.

Note: when this registry is set to ‘BLK’ this also has action when importing AP invoices via XUT, where the field 'Transaction Date' is not allowed to be empty."),
						new CodeDescriptionPairListProvider(() => AccountingMasterFilesConstants.InvoiceDateDefaultValueTypes),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.CurrentDate);
				});
			}
		}

		public CodePairRegistryItem DocumentReceivedDateDefaultingLogic
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(DocReceivedDateDefaultLogics.Code.Blank, DocReceivedDateDefaultLogics.Descriptions.Blank);
					list.AddPair(DocReceivedDateDefaultLogics.Code.CreateDate, DocReceivedDateDefaultLogics.Descriptions.CreateDate);
					list.AddPair(DocReceivedDateDefaultLogics.Code.InvoiceDate, DocReceivedDateDefaultLogics.Descriptions.InvoiceDate);
					return list;
				});

				return GetItem("DocumentReceivedDateDefaultingLogic", delegate
				{
					var item = new CodePairRegistryItem(
						"DocumentReceivedDateDefaultingLogic",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("FBEF176F-A083-4BAF-9DAD-5034FC13AFE8", "Document Received Date Defaulting Logic"),
						ResString.GetMultilingualString("CF57557D-62B7-45C0-85C0-0315C6E6D6B2", @"By default, the document received date will be set to blank.
If required, you can set the 'create date' or the 'invoice date' as the document received date."),
						listProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DocReceivedDateDefaultLogics.Code.Blank);

					item.OnBuildLogReference += (args) => Res.GetString("1CC0A2D0-5E65-4A10-BFB9-945B5E4CB635", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		public BooleanRegistryItem APInvoiceDueDateCalculationRule
		{
			get
			{
				return GetItem("APInvoiceDueDateCalculationRule", delegate
				{
					var item = new BooleanRegistryItem(
						"APInvoiceDueDateCalculationRule",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("8FB739B4-0D62-4E8A-999F-A6B5C00A5850", "AP Invoice Due Date Calculation Rule"),
						ResString.GetMultilingualString("6886C7D0-8F8D-4919-9D50-4394C18498A8", @"By default, the AP Invoice Due Date for 'INV', 'MTH', 'PER', 'COD' and 'PIA' payment terms will be calculated base on Invoice Date.
If you change the registry value to 'Yes', the calculation rule will be based on Document Received Date.
Note: The calculation rule of 'SHP' and 'CUS' are not affected by this registry setting."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);

					item.OnBuildLogReference += (args) => Res.GetString("8E1624AE-9110-4993-9B0E-7AEF9AC9CAB1", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		public BooleanRegistryItem EnableReportingBooksFeature
		{
			get
			{
				return GetItem("EnableReportingBooksFeature", delegate
				{
					var item = new BooleanRegistryItem(
						"EnableReportingBooksFeature",
						Categories.Accounting_ReportingBooks,
						(NoResString)"Enable Reporting Books Feature (CWSupport Only)",
						(NoResString)@"This feature is currently under development,
This feature enables users to configure one or more reporting books to meet different reporting needs from a single source of truth.
With this feature, users will be able to:
1. Create one or more alternate reports with different layout and totalling rules with alpha-numeric alternate GL accounts.
2. Dissect single GL Account using one or more transaction's attributes to report account balance in more granular level
3. Group multiple GL Accounts to a single Alternate GL Accounts to report accounts balances at higher level
4. Translate the GL Accounts balances from your local currency to a foreign currency
5. Translate and report the GL Accounts balances using your head office period setting",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						GetEnableReportingBooksFeatureDefaultValue());

					item.OnBuildLogReference += (args) => Res.GetString("D5EF6724-0CE8-44C7-B288-3F62AE78B269", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		bool GetEnableReportingBooksFeatureDefaultValue()
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingReportingBookFeature);
			return featureData != null
				&& featureData.TryDeserializeParameterAsJson<ReportingBookFeatureControlData>(out var reportingBookFeatureControlData)
				&& reportingBookFeatureControlData.EnableReportingBooksFeature;
		}

		#region Cash Flow Related

		public CashFlowActivityConfigurationRegistryItem CashFlowActivityConfiguration
		{
			get
			{
				CashFlowActivityConfigurationRegistryItem item = GetItem("CashFlowActivityConfiguration", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("bd50c45a-0424-49ef-99a3-ea2feecef13b", "This registry enables you to configure cash flow type");
					return new CashFlowActivityConfigurationRegistryItem(
						"CashFlowActivityConfiguration",
						Categories.Accounting_GeneralLedgerDefaults_CashFlow,
						ResString.GetMultilingualString("02c4dedf-d39d-46ad-9991-4e206832a2b2", "Cash Flow Activity Configuration"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
				item.OnBuildLogReference += BuildCashFlowActivityConfigurationLogReference;
				return item;
			}
		}

		public CashFlowCategoryBasedOnDebtorGroupRegistryItem CashFlowCategoryBasedOnDebtorGroup
		{
			get
			{
				CashFlowCategoryBasedOnDebtorGroupRegistryItem item = GetItem("CashFlowCategoryBasedOnDebtorGroup", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("7dd28bb6-d478-4279-828f-9be198d4bec0", "This registry enables you to assign default cash flow categorization against each debtor group");
					return new CashFlowCategoryBasedOnDebtorGroupRegistryItem(
						"CashFlowCategoryBasedOnDebtorGroup",
						Categories.Accounting_GeneralLedgerDefaults_CashFlow,
						ResString.GetMultilingualString("664b9813-99ae-4864-9197-3defec51a27f", "Cash Flow Based On Debtor Group"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
				item.OnBuildLogReference += BuildCashFlowCategoryBasedOnDebtorGroupLogReference;
				return item;
			}
		}

		public CashFlowCategoryBasedOnCreditorGroupRegistryItem CashFlowCategoryBasedOnCreditorGroup
		{
			get
			{
				CashFlowCategoryBasedOnCreditorGroupRegistryItem item = GetItem("CashFlowCategoryBasedOnCreditorGroup", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("95801282-3fd7-4acb-8283-65ec1bdbda83", "This registry enables you to assign default cash flow categorization against each creditor group");
					return new CashFlowCategoryBasedOnCreditorGroupRegistryItem(
						"CashFlowCategoryBasedOnCreditorGroup",
						Categories.Accounting_GeneralLedgerDefaults_CashFlow,
						ResString.GetMultilingualString("64e05c22-b5ef-4e63-8f34-f8f51ef7f1fa", "Cash Flow Based On Creditor Group"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
				item.OnBuildLogReference += BuildCashFlowCategoryBasedOnCreditorGroupLogReference;
				return item;
			}
		}

		string BuildCashFlowActivityConfigurationLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var originalElement = (CashFlowActivityConfigurationCollection)args.OriginalValue;
			var newElement = (CashFlowActivityConfigurationCollection)args.NewValue;

			foreach (CashFlowActivityConfiguration element in newElement)
			{
				if (!originalElement.Any(x => ((CashFlowActivityConfiguration)x).Code == element.Code))
				{
					result += Res.GetString("439c17a5-c49a-4190-bb94-99af84ada017", "Added: Code [{0}] - Activity Type [{1}]", element.Code, element.ActivityType) + "\r\n";
				}
			}

			foreach (CashFlowActivityConfiguration element in originalElement)
			{
				CashFlowActivityConfiguration updatedElement =
					(CashFlowActivityConfiguration)newElement.FirstOrDefault(x => ((CashFlowActivityConfiguration)x).Code == element.Code
																		&& ((CashFlowActivityConfiguration)x).ActivityType != element.ActivityType);
				if (updatedElement != null)
				{
					result += Res.GetString("56346b31-455f-4f6b-a33d-3ab4a8596999", "Updated: Code [{0}] - Activity Type from [{1}] to [{2}]", element.Code, element.ActivityType, updatedElement.ActivityType) + "\r\n";
				}

				updatedElement =
					(CashFlowActivityConfiguration)newElement.FirstOrDefault(x => ((CashFlowActivityConfiguration)x).Code == element.Code
																		&& ((CashFlowActivityConfiguration)x).EnglishDescription != element.EnglishDescription);
				if (updatedElement != null)
				{
					result += Res.GetString("228968cd-2c0e-477c-b36f-dbda21b243e0", "Updated: Code [{0}] - Description from [{1}] to [{2}]", element.Code, element.EnglishDescription, updatedElement.EnglishDescription) + "\r\n";
				}
			}

			foreach (CashFlowActivityConfiguration element in originalElement)
			{
				if (!newElement.Any(x => ((CashFlowActivityConfiguration)x).Code == element.Code))
				{
					result += Res.GetString("03d01460-198d-4905-8174-173daaa20329", "Deleted: Code [{0}] - Activity Type [{1}]", element.Code, element.ActivityType) + "\r\n";
				}
			}

			return result;
		}

		string BuildCashFlowCategoryBasedOnCreditorGroupLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var originalElement = (CashFlowCategoryBasedOnCreditorGroupCollection)args.OriginalValue;
			var newElement = (CashFlowCategoryBasedOnCreditorGroupCollection)args.NewValue;

			foreach (CashFlowCategoryBasedOnCreditorGroup element in newElement)
			{
				if (!originalElement.Any(x => ((CashFlowCategoryBasedOnCreditorGroup)x).OrgGroupPK == element.OrgGroupPK))
				{
					result += Res.GetString("1450db1e-3bc6-4bcb-9906-1096b53d15aa", "Added: Creditor Group [{0}] - Cash Flow Category [{1}]", element.OrgGroupDescription, element.CashFlowCategory) + "\r\n";
				}
			}

			foreach (CashFlowCategoryBasedOnCreditorGroup element in originalElement)
			{
				CashFlowCategoryBasedOnCreditorGroup updatedElement =
					(CashFlowCategoryBasedOnCreditorGroup)newElement.FirstOrDefault(x => ((CashFlowCategoryBasedOnCreditorGroup)x).OrgGroupPK == element.OrgGroupPK
																		&& ((CashFlowCategoryBasedOnCreditorGroup)x).CashFlowCategory != element.CashFlowCategory);
				if (updatedElement != null)
				{
					result += Res.GetString("5032173a-7d7c-4d46-8be0-13703258583f", "Updated: Creditor Group [{0}] - Cash Flow Category changed from [{1}] to [{2}]", element.OrgGroupDescription, element.CashFlowCategory, updatedElement.CashFlowCategory) + "\r\n";
				}
			}

			foreach (CashFlowCategoryBasedOnCreditorGroup element in originalElement)
			{
				if (!newElement.Any(x => ((CashFlowCategoryBasedOnCreditorGroup)x).OrgGroupPK == element.OrgGroupPK))
				{
					result += Res.GetString("c5727cd1-9b48-489f-ac3b-878df47d859a", "Deleted: Creditor Group [{0}] - Cash Flow Category [{1}]", element.OrgGroupDescription, element.CashFlowCategory) + "\r\n";
				}
			}

			return result;
		}

		string BuildCashFlowCategoryBasedOnDebtorGroupLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var originalElement = (CashFlowCategoryBasedOnDebtorGroupCollection)args.OriginalValue;
			var newElement = (CashFlowCategoryBasedOnDebtorGroupCollection)args.NewValue;

			foreach (CashFlowCategoryBasedOnDebtorGroup element in newElement)
			{
				if (!originalElement.Any(x => ((CashFlowCategoryBasedOnDebtorGroup)x).OrgGroupPK == element.OrgGroupPK))
				{
					result += Res.GetString("39c40270-ee11-4c3f-8ad9-669b460a48f3", "Added: Debtor Group [{0}] - Cash Flow Category [{1}]", element.OrgGroupDescription, element.CashFlowCategory) + "\r\n";
				}
			}

			foreach (CashFlowCategoryBasedOnDebtorGroup element in originalElement)
			{
				CashFlowCategoryBasedOnDebtorGroup updatedElement =
					(CashFlowCategoryBasedOnDebtorGroup)newElement.FirstOrDefault(x => ((CashFlowCategoryBasedOnDebtorGroup)x).OrgGroupPK == element.OrgGroupPK
																		&& ((CashFlowCategoryBasedOnDebtorGroup)x).CashFlowCategory != element.CashFlowCategory);
				if (updatedElement != null)
				{
					result += Res.GetString("a3961f92-3e97-4953-af6e-555c5db1c3b8", "Updated: Debtor Group [{0}] - Cash Flow Category changed from [{1}] to [{2}]", element.OrgGroupDescription, element.CashFlowCategory, updatedElement.CashFlowCategory) + "\r\n";
				}
			}

			foreach (CashFlowCategoryBasedOnDebtorGroup element in originalElement)
			{
				if (!newElement.Any(x => ((CashFlowCategoryBasedOnDebtorGroup)x).OrgGroupPK == element.OrgGroupPK))
				{
					result += Res.GetString("c71026a0-b03a-4b9f-864c-d7f72a705f26", "Deleted: Debtor Group [{0}] - Cash Flow Category [{1}]", element.OrgGroupDescription, element.CashFlowCategory) + "\r\n";
				}
			}

			return result;
		}

		#endregion

		#region Branch Management Codes

		public BranchManagementCodeDescriptionBoolNewRegistryItem BranchManagementCodes
		{
			get
			{
				return GetItem("BranchManagementCodes", delegate
				{
					return new BranchManagementCodeDescriptionBoolNewRegistryItem(
						"BranchManagementCodes",
						Categories.Accounting,
						ResString.GetMultilingualString("f7d108fb-d392-4da5-b552-b3b95c09e3c5", "Branch Management Codes"),
						ResString.GetMultilingualString("010a1afe-35eb-482f-ab06-3fc65c2f9a6a", "Branch Management Codes are used to group multiple branches together for management purposes." +
																																							"\r\n\r\n" +
																																							"Note: If any change is made to the 'Branch Management Codes' of the branches, please, close and reopen the registry form."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("1c7a1071-e205-429c-9b15-865bafd9fd6b", "Active"), true),
						new BranchManagementCodeDescriptionBoolCollection());
				});
			}
		}

		#endregion

		#region Collection Batch Types

		public CodeDescriptionBoolRegistryItem CollectionBatchTypes
		{
			get
			{
				var defaultValue = new CodeDescriptionBoolCollection();
				defaultValue.AddSystemDefined("STD", ResString.GetMultilingualString("B6895212-FC28-4E7E-ACE7-66B9372D54B4", "Standard Batch"), true);

				return GetItem("CollectionBatchTypes", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"CollectionBatchTypes",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders,
						ResString.GetMultilingualString("D2963EDA-5228-42BA-9B7A-5F0C940F5FCE", "Collection Batch Types"),
						ResString.GetMultilingualString("E58E9987-EC71-45B0-8E3F-023C16B42062", "This registry allows you to configure user-defined Collection Batch Types. Batch types can be used to classify Receivables Collection Batches. By default, all batches are recorded as STD - Standard Batch.This batch type cannot be removed."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("478F1242-4A4A-449D-9524-5B733BF25AD2", "Include Transactions on Statements and Aged Reports"), true),
						defaultValue);
				});
			}
		}

		#endregion

		#region Tax Message Groups Management

		public CodeDescriptionBoolRelatedItemRegistryItem TaxMessageGroupsManagement
		{
			get
			{
				RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) =>
																						 TaxMessageGroupCodeHelper.GetDefaultValue(companyPK, branchPK, departmentPK);

				return GetItem("TaxMessageGroupsManagement", delegate
				{
					return new CodeDescriptionBoolRelatedItemRegistryItem(
						"TaxMessageGroupsManagement",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Tax Message Groups Management (CargoWise Support Only)",
						(NoResString)"This registry allows you to define a mapping between the Tax Message and a code used by Tax authorities for eInvoicing or reporting.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						10,
						new CodeDescriptionBoolRelatedItemRegistryEditorInfo(ResString.GetMultilingualString("BB8DC320-F118-4CFE-97C1-1AAAFF024C54", "Active"), true, (NoResString)"Govt. Code"),
						valueGetter,
						null);
				});
			}
		}

		static class TaxMessageGroupCodeHelper
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
			static internal object GetDefaultValue(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var result = new CodeDescriptionBoolRelatedItemCollection();

#if DEBUG
				defaultValuesPerCompanyPK = null;
#endif
				if (defaultValuesPerCompanyPK == null)
				{
					LoadDefaultValuesPerCompanyPK();
				}

				if (defaultValuesPerCompanyPK.ContainsKey(companyPK))
				{
					result = defaultValuesPerCompanyPK[companyPK];
				}

				return result;
			}

			static void LoadDefaultValuesPerCompanyPK()
			{
				var factory = new BusinessObjectFactory();
				defaultValuesPerCompanyPK = new Dictionary<Guid, CodeDescriptionBoolRelatedItemCollection>();

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Italy)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Italy)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Fiji)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Fiji)?.GetTaxMessageGroup());
				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.WesternSamoa)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.WesternSamoa)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Portugal)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Portugal)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Argentina)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Argentina)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Hungary)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Hungary)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Egypt)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Egypt)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Spain)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Spain)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.SaudiArabia)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.SaudiArabia)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Panama)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Panama)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Norway)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Norway)?.GetTaxMessageGroup());

				factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Turkey)).ForEach(
					x => defaultValuesPerCompanyPK[x.PK.ToGuid()] = CountryComplianceFactory.GetITaxMessageGroupProvider(Constants.CountryCodes.Turkey)?.GetTaxMessageGroup());
			}

			[ThreadStatic]
			static Dictionary<Guid, CodeDescriptionBoolRelatedItemCollection> defaultValuesPerCompanyPK;
		}

		#endregion

		#region Collection & Demand Letters

		#region 1st Reminder

		#region Document Name

		public MultilingualStringRegistryItem CollectionAndDemandFirstReminderDocumentName
		{
			get
			{
				return GetItem("CollectionAndDemandFirstReminderDocumentName", delegate
				{
					return new MultilingualStringRegistryItem(
						"CollectionAndDemandFirstReminderDocumentName",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_1stReminder,
						ResString.GetMultilingualString("916a81c7-1db3-4ac6-8038-fc0a6b5f6ca1", "Document Name"),
						ResString.GetMultilingualString("33be1ef3-0979-4829-8307-9f6c2bba39c0", "The document name that will appear on top of the Collection & Demand 1st Reminder document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("092bd7d7-866c-4819-a8f8-1c86cc13b611", "REQUEST FOR IMMEDIATE PAYMENT"));
				});
			}
		}

		#endregion

		#region Opening Text

		public MultilingualStringRegistryItem CollectionAndDemandFirstReminderOpeningText
		{
			get
			{
				return GetItem("CollectionAndDemandFirstReminderOpeningText", delegate
				{
					ResourceString defaultValue =
						ResString.GetMultilingualString("136000fa-da8e-4662-a7e3-8c89e1ae0d8b", "The transactions below have not yet been paid.\r\nUnder the credit arrangements agreed by you with our company, these transactions should have been settled by now.\r\nPlease pay promptly within the next SEVEN DAYS.\r\nPlease contact me directly if you require copies of any outstanding item.\r\nPlease disregard this notice if payment has been made in the past 7 days.");

					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"CollectionAndDemandFirstReminderOpeningText",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_1stReminder,
						ResString.GetMultilingualString("91d4a4d6-f861-47b3-aef4-3489a3a4fbb1", "Opening Text"),
						ResString.GetMultilingualString("7015483c-b250-4317-94be-ce199ebceb9f", "The opening text for the Collection & Demand 1st Reminder document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region 2nd Reminder

		#region Document Name

		public MultilingualStringRegistryItem CollectionAndDemandSecondReminderDocumentName
		{
			get
			{
				return GetItem("CollectionAndDemandSecondReminderDocumentName", delegate
				{
					return new MultilingualStringRegistryItem(
						"CollectionAndDemandSecondReminderDocumentName",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_2ndReminder,
						ResString.GetMultilingualString("916a81c7-1db3-4ac6-8038-fc0a6b5f6ca1", "Document Name"),
						ResString.GetMultilingualString("202ab31b-4f0f-4735-990e-376ac4f83462", "The document name that will appear on top of the Collection & Demand 2nd Reminder document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("49592bac-8b3a-4747-81dc-09d33ae95615", "2ND REQUEST FOR IMMEDIATE PAYMENT"));
				});
			}
		}

		#endregion

		#region Opening Text

		public MultilingualStringRegistryItem CollectionAndDemandSecondReminderOpeningText
		{
			get
			{
				return GetItem("CollectionAndDemandSecondReminderOpeningText", delegate
				{
					ResourceString defaultValue =
						ResString.GetMultilingualString("240a53f8-0b42-439b-a47d-52496f206d2f", "By our records, the transactions below remain unpaid.\r\nPlease pay promptly within the next SEVEN DAYS.\r\nUnder the credit arrangements agreed by you with our company, these transactions should have been settled by now.\r\nPlease contact me directly if you require copies of any outstanding item.\r\nIf payment has been made, please contact me and advise the details of that payment.");

					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"CollectionAndDemandSecondReminderOpeningText",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_2ndReminder,
						ResString.GetMultilingualString("91d4a4d6-f861-47b3-aef4-3489a3a4fbb1", "Opening Text"),
						ResString.GetMultilingualString("f8290d3d-4c2c-4c87-87c9-2c0a4b007a77", "The opening text for the Collection & Demand 2nd Reminder document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Collection

		#region Document Name

		public MultilingualStringRegistryItem CollectionLetterDocumentName
		{
			get
			{
				return GetItem("CollectionLetterDocumentName", delegate
				{
					return new MultilingualStringRegistryItem(
						"CollectionLetterDocumentName",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Collection,
						ResString.GetMultilingualString("916a81c7-1db3-4ac6-8038-fc0a6b5f6ca1", "Document Name"),
						ResString.GetMultilingualString("6af65fd3-e1bd-4810-b721-96f25e139396", "The document name that will appear on top of the Collection Letter document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("acc4805b-bd89-48f3-832e-00320b28ef6a", "COLLECTION LETTER"));
				});
			}
		}

		#endregion

		#region Opening Text

		public MultilingualStringRegistryItem CollectionLetterOpeningText
		{
			get
			{
				return GetItem("CollectionLetterOpeningText", delegate
				{
					ResourceString defaultValue =
						ResString.GetMultilingualString("04004448-f05a-481c-9451-01165f30b019", "Please pay the transactions detailed below within the next SEVEN DAYS.\r\n\r\nUnder the credit arrangements agreed by you with our company, these transactions require immediate payment.\r\nCopies of each outstanding item can be provided on request.\r\nIf payment for any of the listed items has already been made, please contact me immediately and advise the details of that payment.");

					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"CollectionLetterOpeningText",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Collection,
						ResString.GetMultilingualString("91d4a4d6-f861-47b3-aef4-3489a3a4fbb1", "Opening Text"),
						ResString.GetMultilingualString("d309197f-c2ac-4d89-8638-91eb1c3f8b94", "The opening text for the Collection Letter document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Demand

		#region Document Name

		public MultilingualStringRegistryItem DemandLetterDocumentName
		{
			get
			{
				return GetItem("DemandLetterDocumentName", delegate
				{
					return new MultilingualStringRegistryItem(
						"DemandLetterDocumentName",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Demand,
						ResString.GetMultilingualString("916a81c7-1db3-4ac6-8038-fc0a6b5f6ca1", "Document Name"),
						ResString.GetMultilingualString("3b57c83c-b2ec-48de-ad9c-608c3c02122a", "The document name that will appear on top of the Demand Letter document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("1665dc66-a00b-40f7-825e-e3eee1c4feca", "LETTER OF DEMAND"));
				});
			}
		}

		#endregion

		#region Opening Text

		public MultilingualStringRegistryItem DemandLetterOpeningText
		{
			get
			{
				return GetItem("DemandLetterOpeningText", delegate
				{
					ResourceString defaultValue =
						ResString.GetMultilingualString("2057ddb3-0e77-4102-a971-2d595561e0c1", "Please be advised that the transactions detailed below require immediate payment.\r\nIf payment is not received within SEVEN DAYS we will commence legal debt collection procedures.\r\n\r\nUnder the credit arrangements agreed by you with our company, these transactions require IMMEDIATE PAYMENT.\r\n\r\nIf payment for any of the listed items has already been made, please contact me immediately and advise the details of that payment.");

					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"DemandLetterOpeningText",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Demand,
						ResString.GetMultilingualString("91d4a4d6-f861-47b3-aef4-3489a3a4fbb1", "Opening Text"),
						ResString.GetMultilingualString("b2308219-69c1-45dd-b36b-a12dcfae3bf7", "The opening text for the Demand Letter document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Satement

		#region Document Name

		public MultilingualStringRegistryItem StatementDocumentName
		{
			get
			{
				return GetItem("StatementDocumentName", delegate
				{
					return new MultilingualStringRegistryItem(
						"StatementDocumentName",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Statement,
						ResString.GetMultilingualString("916a81c7-1db3-4ac6-8038-fc0a6b5f6ca1", "Document Name"),
						ResString.GetMultilingualString("62b2efc4-21e2-4760-baad-3a49ef3639bc", "The document name that will appear on top of the Statement document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("60714c24-bec9-43a8-80a2-6c217db12ca9", "STATEMENT OF ACCOUNT"));
				});
			}
		}

		public BooleanRegistryItem StatementUsePrintStreaming
		{
			get
			{
				return GetItem("StatementUsePrintStreaming", delegate
				{
					return new BooleanRegistryItem(
						"StatementUsePrintStreaming",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_CollectionDemandLetters_Statement,
						ResString.GetMultilingualString("5b9a36e0-fbb5-4aed-abc1-b3cff98d9dbb", "Statement Use Print Streaming"),
						ResString.GetMultilingualString("4c1a9829-ec03-466e-8a91-07291e48088f", "Statements to use new Print Streaming for better memory utilization."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem InvoiceUsePrintStreaming
		{
			get
			{
				return GetItem("InvoiceUsePrintStreaming", delegate
				{
					return new BooleanRegistryItem(
						"InvoiceUsePrintStreaming",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("486b1cb0-9b55-4d02-8b4d-2c2e32a6f454", "Invoice Use Print Streaming"),
						ResString.GetMultilingualString("c6d36a56-7844-4866-adf4-e6740964d4fc", "Invoices to use new Print Streaming for better memory utilization."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Government Charge Code
		public BooleanRegistryItem EnableGovernmentChargeCode
		{
			get
			{
				return GetItem("EnableGovernmentChargeCode", delegate
				{
					return new BooleanRegistryItem(
						new CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<bool>(
						"EnableGovernmentChargeCode",
						Categories.Accounting,
						ResString.GetMultilingualString("b978fb2d-d105-4c68-abff-a389d1d2f29d", "Enable Government Reporting Charge Code Behavior"),
						ResString.GetMultilingualString("5ee6b851-f2c6-4a1c-a73b-ed167daf762d", @"Government Reporting Charge Codes are an additional, secondary item of information used to classify each revenue or cost charge line using a Government defined code.

When this registry is set to 'Yes' charge line related Government Reporting Charge Code features are surfaced: Charge Codes can be assigned a default Government Code; that code defaults against Sell and Cost charges as they are entered; the government code recorded against a charge line is printed in the invoice and included in the invoice XML.

When set to ""No"" these features are disabled.

These features are relevant in countries when the charge line details of each Sale and Purchase Invoice must be classified and reported to government agencies using government specified reporting codes."),
						RegistryDataTypes.BoolType,
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						(complianceInfo) => (complianceInfo as IComplianceRegistryDefaultProvider)?.GetDefaultValueForEnableGovernmentChargeCodeRegistry() ?? false,
						false));
				});
			}
		}

		#endregion

		#region Job Costing Report - Hidden

		public BooleanRegistryItem NewCompanyPartitionKeyNeedsToBeCreated
		{
			get
			{
				return GetItem("NewCompanyPartitionKeyNeedsToBeCreated",
					() => new BooleanRegistryItem(
						"NewCompanyPartitionKeyNeedsToBeCreated",
						null,
						null,
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						false));
			}
		}

		#endregion

		#region Currency Exchange Rate Types - Global

		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryItem CurrencyExchangeRateTypes
		{
			get
			{
				const string key = "CurrencyExchangeRateTypes";
				var item = GetItem(key,
					() => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryItem(
						key,
						Categories.Accounting,
						ResString.GetMultilingualString("2eef5b24-6ec7-4da1-b0f2-5474367f8104", "Currency Exchange Rate Types - Global"),
						ResString.GetMultilingualString("34c4a43e-6d6b-4342-b617-3c5ff1030f79",
							"This registry allows you to override the labels for currency exchange rate types. Custom rates can be used to record various sources of exchange rates."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(
							ResString.GetMultilingualString("537d9e17-f3c7-4ef9-b9ce-efc4800141b8", "Is Enabled"), true, false),
						GetCurrencyExchangeRateTypesDefaultValue()));

				item.OnBuildLogReference += FindModifiedCustomExchangeRates;

				return item;
			}
		}

		static CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection GetCurrencyExchangeRateTypesDefaultValue()
		{
			var result = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();
			var defaultExchangeRateTypes = GetDefaultExchangeRateTypesList();

			foreach (CodeDescriptionPair rateType in GetExchangeRateTypesList_SystemLevel())
			{
				var isSystemDefined = defaultExchangeRateTypes.ContainsCode(rateType.Code);

				var item = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidation();

				using (item.GetValidationSuspender())
				{
					item.Code = rateType.Code;
					item.Description = rateType.MultilingualDescription;
					item.SystemDefined = isSystemDefined;
					item.Bool = isSystemDefined;
					item.IsCodeReadOnly = true;
				}

				result.Add(item);
			}
			return result;
		}

		static string FindModifiedCustomExchangeRates(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var originalValue = args.OriginalValue as CodeDescriptionBoolCollection;
			var newValue = args.NewValue as CodeDescriptionBoolCollection;

			var orignalCollection = originalValue.Cast<CodeDescriptionBool>().ToList();

			var sb = new StringBuilder();
			newValue.Cast<CodeDescriptionBool>()
				.ToList()
				.ForEach(x =>
				{
					var code = x.Code;
					var originalItem = orignalCollection.FirstOrDefault(y => y.Code == x.Code);
					if (originalItem != null)
					{
						if ((x.Bool != originalItem.Bool))
						{
							sb.Append(ResString.GetMultilingualString("df1a7a9b-d163-409a-a38f-becc00ebe985", "'{0}' Enabled Status changed from '{1}' to '{2}'. ", code, originalItem.Bool, x.Bool));
						}
						if (!(x.Description.Equals(originalItem.Description)))
						{
							sb.Append(ResString.GetMultilingualString("915bbf6a-db3f-446d-847a-4d93d0df6518", "'{0}' Description changed from '{1}' to '{2}'. ", code, originalItem.Description, x.Description));
						}
					}
				});

			return sb.ToString();
		}

		#endregion

		#region Currency Exchange Rate Types - Local

		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryItem CurrencyExchangeRateTypesCompanySpecific
		{
			get
			{
				const string key = "CurrencyExchangeRateTypesCompanySpecific";
				var item = GetItem(key,
					() => new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryItem(
						key,
						Categories.Accounting,
						ResString.GetMultilingualString("B544E7D5-A26C-4270-AB78-FF59756B215D", "Currency Exchange Rate Types - Local"),
						ResString.GetMultilingualString("8B4D0929-3142-4D93-91D7-317621DF9291",
							"This registry allows you to override the labels for company specific currency exchange rate types. Custom rates can be used to record various sources of exchange rates."),
						RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(
							ResString.GetMultilingualString("94932E2C-5911-4B6B-B31F-E323426C2007", "Is Enabled"), true, false),
						GetCurrencyExchangeRateTypesDefaultValue_CompanySpecific()));

				item.OnBuildLogReference += FindModifiedCustomExchangeRates;

				return item;
			}
		}

		static CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection GetCurrencyExchangeRateTypesDefaultValue_CompanySpecific()
		{
			var result = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection();
			foreach (CodeDescriptionPair rateType in GetExchangeRateTypesList_CompanyLevel())
			{
				var item = new CodeDescriptionBoolDisallowNewCodeReadOnlyValidation();
				using (item.GetValidationSuspender())
				{
					item.Code = rateType.Code;
					item.Description = rateType.MultilingualDescription;
					item.SystemDefined = false;
					item.Bool = false;
					item.IsCodeReadOnly = true;
				}

				result.Add(item);
			}
			return result;
		}

		#endregion

		#region Invoice Remittance Configuration

		public InvoiceRemittanceConfigurationRegistryItem InvoiceRemittanceConfiguration
		{
			get
			{
				var item = GetItem("InvoiceRemittanceConfiguration", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("C0EB08AF-CA62-44D8-9E0A-5DF7D6925AEE", @"This registry gives you the ability to define invoice remittance type and respective configuration using the available data elements and check digits.
You setup one or more invoice remittance type by location.

During the posting of Receivable Invoices, Credit Notes and Adjustment Notes, the invoice remittance type and the corresponding invoice remittance reference will be saved against the transaction record.

The Invoice Remittance Reference will be shown on the Invoice screen after posting.

The 'Invoice Remittance Reference' doc strip will be included during the printing of DocBuilder Invoice document if an Invoice Remittance Reference has been saved against the invoice record. You can customize this doc strip as required to meet your requirement.

This Invoice Remittance Reference will be included in the Universal Accounting Transaction XML export.

Note: The Invoice Remittance Type and Reference will only be saved against new transaction record posted after the registry has been configured. If you would like to saved the type and reference against transaction posted previously, you can run the 'Override Invoice Remittance Type' Action menu available in Receivables Transactions module.");
					return new InvoiceRemittanceConfigurationRegistryItem(
						"InvoiceRemittanceConfiguration",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("A45B521E-36CC-4EAA-B245-8C0A167A674D", "Invoice Remittance Configuration"),
						hint,
						RegistryStorageFlags.Company,
						RegistryOptions.MustOverrideDefaultValue);
				});

				item.OnBuildLogReference += BuildConfigurationChangeLogReference;
				return item;
			}
		}

		string BuildConfigurationChangeLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var originalConfigurationCollection = (InvoiceRemittanceConfigurationCollection)args.OriginalValue;
			var newConfigurationCollection = (InvoiceRemittanceConfigurationCollection)args.NewValue;

			foreach (InvoiceRemittanceConfiguration configuration in newConfigurationCollection)
			{
				var originalConfiguration = originalConfigurationCollection.Cast<InvoiceRemittanceConfiguration>().FirstOrDefault(x => x.Code == configuration.Code);
				if (originalConfiguration == null)
				{
					result += Res.GetString("C71A5578-F0F7-4FDA-B9C2-886D4B8EE4F1", "New code <{0}> added.", configuration.Code) + "\r\n";
				}
				else if (originalConfiguration.Description != configuration.Description ||
						originalConfiguration.BillerCode != configuration.BillerCode ||
						originalConfiguration.BillerAccountNumber != configuration.BillerAccountNumber ||
						originalConfiguration.MaxPossibleLength != configuration.MaxPossibleLength ||
						originalConfiguration.Message != configuration.Message ||
						originalConfiguration.DebtorLocation != configuration.DebtorLocation)
				{
					result += Res.GetString("DE25EF4F-4C81-4EFB-90AD-8D86F6F694FF", "Code <{0}> configuration edited.", configuration.Code) + "\r\n";
				}
			}

			return result;
		}

		#endregion

		public StringRegistryItem RecordInvalidJobCreationByUser
		{
			get
			{
				return GetItem("RecordInvalidJobCreationByUser", () =>
					new StringRegistryItem("RecordInvalidJobCreationByUser",
						Categories.Accounting_JobInvoicing,
						(NoResString)"Record invalid job creation by user(CargoWise Support Only)",
						(NoResString)@"Specify single/multiple User-Company-Branch-Department combinations for which you want to send job creation stack trace. While setting value(s) in this registry, please make sure that the specified user(s) cannot be supposed to create a job in the specified Company-Branch-Department. Otherwise system will report a valid job creation as an invalid one.
Value Format:
	[User code]-[Company Code]-[Branch Code]-[Department Code]
Example: 
	[ATL]-[YZP]-[TYU]-[BRN]  (report when the job is created by ATL with YZP company, TYU branch and BRN department)
	[ATL]-[YZP]-[TYU]    (report when the job is created by ATL with YZP company and TYU branch)
	[ATL]-[YZP]      (report when the job is created by ATL with YZP company)
	[ATL]        (report when the job is created by ATL)
Note: You must provide a user code to activate this registry.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						string.Empty)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo),
					});
			}
		}

		public BooleanRegistryItem EnableTransactionNumberCriticalValidation
		{
			get
			{
				return GetItem("EnableTransactionNumberCriticalValidation", () => new BooleanRegistryItem(
					"EnableTransactionNumberCriticalValidation",
					Categories.Accounting,
					(NoResString)"Enable Transaction Number Critical Validation (CargoWiseOne Support only)",
					(NoResString)@"This registry controls whether to enable Transaction Number Critical Validation.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));
			}
		}

		#region E-Payment Registries

		public StringRegistryItem OFXOAuthLoginWebURL
		{
			get
			{
				return GetItem(nameof(OFXOAuthLoginWebURL), () =>
				new StringRegistryItem(nameof(OFXOAuthLoginWebURL),
					Categories.Accounting_EPaymentConfigurations,
					(NoResString)"OFX OAuth login web URL (CargoWiseOne Support Only)",
					(NoResString)@"OFX OAuth login web URL.
IMPORTANT: Please do not change this registry value from default.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)(Env.Instance.IsProductionSystem ? OFXOAuthWebsiteURL.ProductionURL : OFXOAuthWebsiteURL.TestingURL)));
			}
		}

		public BooleanRegistryItem OFXOAuthWebURLEnvironment
		{
			get
			{
				return GetItem(nameof(OFXOAuthWebURLEnvironment), () =>
				new BooleanRegistryItem(nameof(OFXOAuthWebURLEnvironment),
					Categories.Accounting_EPaymentConfigurations,
					(NoResString)"OFX OAuth Web URL Uses Production Environment(CargoWiseOne Support Only)",
					(NoResString)@"Indicate OFX OAuth login web URL is for production environment or not. 'YES' means production, 'NO' means test.
IMPORTANT: Please do not change this registry value from default.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					Env.Instance.IsProductionSystem));
			}
		}

		public StringRegistryItem WTCCallbackSiteWebURL
		{
			get
			{
				return GetItem(nameof(WTCCallbackSiteWebURL), () =>
				new StringRegistryItem(nameof(WTCCallbackSiteWebURL),
					Categories.Accounting_EPaymentConfigurations,
					(NoResString)"WTC Callback site web URL (CargoWiseOne Support Only)",
					(NoResString)@"WTC Callback site web URL.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)(Env.Instance.IsProductionSystem ? "https://ofxcallback.wisegrid.net" : "https://ofxcallback-test.wisegrid.net")));
			}
		}

		public EPaymentConfigurationRegistryItem EnableEPaymentFunctionality
		{
			get
			{
				return GetItem("EnableEPaymentFunctionality",
						delegate
						{
							return new EPaymentConfigurationRegistryItem("EnableEPaymentFunctionality",
								Categories.Accounting_EPaymentConfigurations,
								ResString.GetMultilingualString("278cae53-a2cf-4664-86d9-3c2239d00035", "Enable E-Payment Functionality"),
								ResString.GetMultilingualString("927f1898-7f9a-4f23-96eb-3de0e24d8e46", @"This registry controls E-Payment functionality.E-Payments are global foreign payments processed through integrated third party service providers. 

CargoWise only relays messages and payment information, all foreign exchange quotes and payment transfers are provided and executed by the third party service providers.

Currently, CargoWise supports direct integration with {0}.

Unless overridden, the default value of the registry is set to Enabled for supported countries and set to Disabled for the countries not yet supported. For the login companies in the supported countries, this registry allows you to disable the integrated payments functionality, if you do not wish to use it.", "OFX - OzForex Limited (www.ofx.com)"),
								RegistryStorageFlags.Company, RegistryOptions.Default);
						});
			}
		}

		public StringRegistryItem OFXWebURL
		{
			get
			{
				return GetItem(nameof(OFXWebURL), () =>
				new StringRegistryItem(nameof(OFXWebURL),
					Categories.Accounting_EPaymentConfigurations,
					(NoResString)"OFX Web URL (CargoWiseOne Support Only)",
					(NoResString)"OFX Web URL.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)"https://www.ofx.com"));
			}
		}

		public ImageRegistryItem OFXLogo
		{
			get
			{
				return GetItem(nameof(OFXLogo), delegate
				{
					var logoStream = GetType().Assembly.GetManifestResourceStream("Enterprise.MasterFiles.Business.Accounting.EPayment.PaymentProviderLogos.OFXLogo.png");
					var ofxLogo = Image.FromStream(logoStream);

					return new ImageRegistryItem(
						nameof(OFXLogo),
						Categories.Accounting_EPaymentConfigurations,
						(NoResString)"OFX Logo (CargoWiseOne Support Only)",
						(NoResString)"OFX Logo.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						ofxLogo);
				});
			}
		}

		public BooleanRegistryItem RebookExpiredQuotesBasedOnExRateTolerance
		{
			get
			{
				return GetItem(nameof(RebookExpiredQuotesBasedOnExRateTolerance), () =>
				new BooleanRegistryItem(nameof(RebookExpiredQuotesBasedOnExRateTolerance),
					Categories.Accounting_EPaymentConfigurations,
					(NoResString)"Re-book Expired Quotes Based on Ex Rate Tolerance (CargoWiseOne Support Only)",
					(NoResString)@"Once a payment has been approved, by default, changes to exchange rate/local amount of the payment will reset the approval status back to ""Awaiting Approval"". If your business is comfortable to accept small fluctuations in exchange rates, without having to re-approve the payment, you can configure the ""Exchange Rate Tolerance"". When a payment is in Approved status, and the only thing that is changed on the payment is the exchange rate/local amount, the approval status will NOT be reset if the change is within the accepted tolerance. To configure Exchange Rate Tolerance, navigate to the registry Accounting > Payable Defaults > Default Settings > Payment Processing > Exchange Rate Tolerance.

When you submit an E-Payment for processing, a request is sent to the provider to book the E-Payment Deal with the same details as on the accepted quote. However, it is important to note each quote is valid only for several minutes. Unless you book the payment for processing shortly after a quote was received, the quote may no longer valid.

If the accepted quote is expired when booking the payment for processing, then CargoWise will automatically re-request a new quote and book the E-Payment deal on new quote if the Local Payment Amount is the same as the original expired quote.

BY DEFAULT, the following scenarios apply when booking an E-Payment Deal: 
* If the FX quote is still valid, the payment will be booked for processing. The E-Payment Status will change to ACP – Accepted. 
* If the FX quote is no longer valid, CargoWise will automatically request a new quote from the provider. The original accepted quote status will change to DCD – Discarded; and a new quote will be automatically requested. 
* If the new quote has the same Local Payment Amount as the expired quote, then the payment will be booked for processing. The new quote will be automatically accepted, and the payment details updated. The E-Payment Status will change to ACP – Accepted. 

With this feature enabled, the following scenarios apply when booking an E-Payment Deal:

* If the FX quote is still valid, the payment will be booked for processing. The E-Payment Status will change to ACP – Accepted.
* If the FX quote is no longer valid, CargoWise will automatically request a new quote from the provider. The original accepted quote status will change to DCD – Discarded; and a new quote will be available against the payment.
* If the new quote is OUTSIDE the allowed tolerance, then the payment will NOT be booked for processing. The new quote status show as ""RCV – Received"" and will require a review. The E-Payment Status will show as DEC – Declined.
* If the new quote is WITHIN the allowed tolerance, then the payment will be booked for processing. The new quote will be automatically accepted, and the payment details updated. The E-Payment Status will change to ACP – Accepted.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public IntRegistryItem BeneficiarySearchResultHttpResponsePageSize
		{
			get
			{
				return GetItem("BeneficiarySearchResultHttpResponsePageSize", delegate
				{
					return new IntRegistryItem(
						"BeneficiarySearchResultHttpResponsePageSize",
						Categories.Accounting_EPaymentConfigurations,
						(NoResString)"Beneficiary search result Http response page size",
						(NoResString)"This registry sets the maximum number of records returned when a beneficiary search request is sent to an electronic payment provider (e.g. OFX). This is achieved by setting this registry as the value of pagesize parameter while calling the API from XT.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						100);
				});
			}
		}

		public IntRegistryItem MaximumNumberOfBeneficiaryInSearchResultXUE
		{
			get
			{
				return GetItem("MaximumNumberOfBeneficiaryInSearchResultXUE", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfBeneficiaryInSearchResultXUE",
						Categories.Accounting_EPaymentConfigurations,
						(NoResString)"Maximum Number of Beneficiary in a Beneficiary Search Result XUE",
						(NoResString)"This registry sets the maximum number of beneficiary details that a Beneficiary search result XUE can contain.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						500);
				});
			}
		}

		public StringRegistryItem EPaymentProductMarketingWebURL
		{
			get
			{
				return GetItem(nameof(EPaymentProductMarketingWebURL), () =>
				new StringRegistryItem(nameof(EPaymentProductMarketingWebURL),
					Categories.Accounting_EPaymentConfigurations,
					(NoResString)"E-Payment Product Marketing Web URL (CargoWiseOne Support Only)",
					(NoResString)"E-Payment Product Marketing Web URL.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)"https://cargowise.com/solutions/cargowise-enterprise/accounting/integrated-global-payments/?utm_source=cargowise&utm_medium=demo&utm_campaign=ofx"));
			}
		}

		public DefaultEPaymentReasonRegistryItem DefaultPaymentReason
		{
			get
			{
				return GetItem(nameof(DefaultPaymentReason), () =>
				new DefaultEPaymentReasonRegistryItem(
					"DefaultPaymentReason",
					Categories.Accounting_EPaymentConfigurations,
					ResString.GetMultilingualString("90c16ff0-ad9a-4432-8171-39710553eb1e", "Default Payment Reason"),
					ResString.GetMultilingualString("e8ecd701-4fe6-4086-89db-63b89a22bf4e", "Select the Payment Reason to default as the reason for making foreign currency payments via FX integration."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default
				));
			}
		}

		public DefaultEPaymentReferenceRegistryItem DefaultPaymentReference
		{
			get
			{
				return GetItem(nameof(DefaultPaymentReference), () =>
				new DefaultEPaymentReferenceRegistryItem(
					"DefaultPaymentReference",
					Categories.Accounting_EPaymentConfigurations,
					ResString.GetMultilingualString("7D0F8E24-3FA1-4E09-AB6E-6D2A914E8631", "Default Payment Reference"),
					ResString.GetMultilingualString("270AAAC0-5914-4F29-B8EC-1FCA4B2BB97F", @"Payment Reference will be included in the details sent to the recipients bank for display with the deposit when making payments via FX integration.
Payment Reference can be free text (e.g. your company name), invoice numbers being paid, or the Payment Reference Number.
Value entered in this registry will default to a Payables organization's Account Details when adding an E-Payment method, where it may be overridden if required."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default
				));
			}
		}

		public BooleanRegistryItem PromptBankTransferOnPostingEPayment
		{
			get
			{
				return GetItem(nameof(PromptBankTransferOnPostingEPayment), () => new BooleanRegistryItem(
					"PromptBankTransferOnPostingEPayment",
					Categories.Accounting_EPaymentConfigurations,
					ResString.GetMultilingualString("25372701-9640-45c7-b662-7e8e7d4f6b03", "Prompt Bank Transfer on Posting E-Payment"),
					ResString.GetMultilingualString("ece089d6-1027-40d6-b2ac-72cde7670395", "When this registry is set to Yes, on posting of an E-Payment, you will be presented with a Bank Transfer screen to create a bank transfer from the bank account that 'funded' the FX payment to the E-Payment bank account."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					true));
			}
		}

		public EPaymentReasonRegistryItem PaymentReasons
		{
			get
			{
				return GetItem("EPaymentReasons",
						delegate
						{
							return new EPaymentReasonRegistryItem(
								"EPaymentReasons",
								Categories.Accounting_EPaymentConfigurations,
								(NoResString)"Payment Reasons",
								(NoResString)"This registry is used to record accepted Payment Reasons by FX Provider.",
								RegistryStorageFlags.System | RegistryStorageFlags.Company,
								RegistryOptions.IsOnlyForSupport);
						});
			}
		}

		#endregion

		public BooleanRegistryItem EnableAssetManagementFunctionality
			=> GetItem("EnableAssetManagementFunctionality", () => new BooleanRegistryItem(
					"EnableAssetManagementFunctionality",
					Categories.Accounting_AssetManagement,
					(NoResString)"Enable Asset Management Functionality",
					(NoResString)@"This registry allows you to activate the new Asset Management module.
**PLEASE DO NOT ENABLE THIS REGISTRY FOR ANY CUSTOMER**",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));

		public BooleanRegistryItem CanUserEditAssetCode
			=> GetItem("CanUserEditAssetCode", () => new BooleanRegistryItem(
					"CanUserEditAssetCode",
					Categories.Accounting_AssetManagement,
					(NoResString)"Asset Codes can be edited",
					(NoResString)"Can users edit asset codes?",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false));

		#region E-Invoicing Registries

		public BooleanRegistryItem EnableEInvoicingFunctionality
		{
			get
			{
				var item = GetItem("EnableEInvoicingFunctionality",
						delegate
						{
							return new BooleanRegistryItem(
								new EnableEInvoicingFunctionalityRegistryItemImpl(
								"EnableEInvoicingFunctionality",
								Categories.Accounting_EReportingAndEInvoicingConfigurations,
								ResString.GetMultilingualString("A30EBEEE-93D5-4313-907C-C9DF1BA651FD", "Enable E-Reporting Functionality - Receivables"),
								ResString.GetMultilingualString("EA922F4C-BFDE-4309-A248-374E9E101254", @"This registry is used to control the E-Reporting functionality for Receivables. When enabled, all new eligible transactions are automatically transmitted to the relevant Government authority in an approved electronic format.

Unless overridden, the default value of the registry will automatically switch from No to Yes for supported countries.

NOTE: For Taiwan, this feature is only relevant if you have enabled the compliance document module.

IMPORTANT:

It is strongly recommended that you DO NOT DISABLE E-Reporting functionality for supported countries.

This registry ensures that you are fully compliant with the local E-Reporting/E-Invoicing requirements without interrupting your normal existing processes. As you post transactions they are transferred directly from {0} to the government authority via the eHub platform, which provides the necessary encoding and secure exchange of messages. eHub processes government responses and sends updates back to {0}, so you can see the status of each transaction without leaving the system. As a result:
* You do not need to maintain mapping of {0} data via a third party.
* Mapped data is validated at source before being transmitted to the local government.
* Status of each transaction is reflected in {0} and you know which transactions must be corrected/re-sent.
* You do not need to spend time reconciling E-Reporting transactions between multiple systems.", BrandingFactory.Instance.ProductName),
								RegistryStorageFlags.Company,
								RegistryOptions.Default
								));
						});

				item.OnBuildLogReference += (args) => Res.GetString("37FF921D-16A7-49BB-B5B0-78717ECE7668", "E-Reporting Functionality = [{0}].", args.NewValue);

				return item;
			}
		}

		public IntRegistryItem DelayTimeForRequeueInvoices
		{
			get
			{
				var item = GetItem("DelayTimeForRequeueInvoices", delegate
				{
					return new IntRegistryItem(
						new CountrySpecificDefaultValueRegistryItemImpl<int>(
						"DelayTimeForRequeueInvoices",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						(NoResString)@"By default, users will be able to re-queue invoices with SNT status at any time if they have the relevant security right for all countries except for China login companies.

For China login companies, the delay time is set to 30 mins by default to allow sufficient time for the system to process the initial transmission and update the status. Then, if the initial transmission is not successful after 30 minutes, authorised users will be able to re-queue the invoice with SNT status for transmission.
For all other countries, you may override the delay time from 0 to x mins if needed.

Note: This registry is only relevant to countries where E-Reporting has been enabled.",
						new DelayTimeForRequeueInvoicesType(),
						RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue,
						FactoryForCountryDefaultValues,
						new DelayTimeForRequeueInvoices_RegistryDescriptor()));
				});

				item.OnBuildLogReference += (args) => Res.GetString("94e4a35f-0610-4367-ba46-94021143e0cd", "Registry value changed from '{0}' to '{1}'.", args.OriginalValue, args.NewValue);
				return item;
			}
		}

#if DEBUG
		internal
#endif
		class DelayTimeForRequeueInvoicesType : IntRegistryDataType
		{
			protected override void ValidateCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var minsOfOneDay = 1440;

				var defaultValue = registryItem is IntRegistryItem
					? (int)((IntRegistryItem)registryItem).Inner.GetDefaultValue(companyPK, branchPK, departmentPK)
					: (int)((RegistryItemImpl)registryItem).GetDefaultValue(companyPK, branchPK, departmentPK);

				if (proposedValue < defaultValue || proposedValue > minsOfOneDay)
				{
					throw new RegistryValidationException(Res.GetString("a6aab09c-4735-46a3-baa6-845d6d02be70", "Delay time must between '{0}' and '{1}'.", defaultValue, minsOfOneDay));
				}
			}
		}

		public CodeDescriptionBoolRegistryItem UseVATRegistrationNumberAsOrganizationMatchingCriteria
			=> GetItem(nameof(UseVATRegistrationNumberAsOrganizationMatchingCriteria),
				() => new CodeDescriptionBoolRegistryItem(
						nameof(UseVATRegistrationNumberAsOrganizationMatchingCriteria),
						Categories.Accounting,
						ResString.GetMultilingualString("C629C178-1D89-4721-B08E-EDED9B3F630B", "Use VAT Registration Number as Organization Matching Criteria"),
						ResString.GetMultilingualString("F39B0ED4-E9C0-4349-94A3-1DFAEBFD226D", @"When any of the contexts are enabled, the organization's VAT registration number will be added as a matching criterion for enabled context(s) in the XML import logic when identifying the organization related to the imported transaction.
It will be the last level of fallback when Organization address or Organization code is not found."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("5CED5E69-4CB0-48CF-9588-6023939960D2", "Enable Organization Matching by VAT Registration Number"), true, true),
						OrganizationMatcherVATRegistrationNumberContextTypeList()
				)
			);

		public static CodeDescriptionBoolCollection OrganizationMatcherVATRegistrationNumberContextTypeList()
			=> new CodeDescriptionBoolCollection()
			{
				{ OrganisationMatchingByVATRegistrationNumberContexts.Codes.Payables,    ResString.GetMultilingualString("EC1E8394-9C80-49A0-8663-E602C4A6F7D9", "Payables Transactions"), false },
				{ OrganisationMatchingByVATRegistrationNumberContexts.Codes.Receivables, ResString.GetMultilingualString("FAB5F5D2-AC8B-4925-8395-7704A6DC363E", "Receivables Transactions"), false },
			};

		public BooleanRegistryItem EnableEInvoicingFunctionalityForPayables
		{
			get
			{
				var item = GetItem("EnableEInvoicingFunctionalityForPayables",
						delegate
						{
							return new BooleanRegistryItem(
								new EnableEInvoicingFunctionalityRegistryItemImpl(
								"EnableEInvoicingFunctionalityForPayables",
					Categories.Accounting_EReportingAndEInvoicingConfigurations,
					ResString.GetMultilingualString("D366FA16-711E-4230-9C5B-C4B673AD39CD", "Enable E-Reporting Functionality - Payables"),
					ResString.GetMultilingualString("8321D7B3-8175-4F7D-B3E4-4166B24DB59C", @"This registry is used to control the E-Reporting functionality for Payables.
When enabled, all new eligible transactions are automatically transmitted to the relevant Government authority in an approved electronic format.

Unless overridden, the default value of the registry will automatically switch from No to Yes for supported countries.

NOTE: E-Reporting for Payables is currently supported only in Italy login companies.

IMPORTANT:
It is strongly recommended that you DO NOT DISABLE E-Reporting for Payables functionality for supported countries.
This registry ensures that you are fully compliant with the local E-Reporting/E-Invoicing requirements without interrupting your normal existing processes.
See ""Enable E-Reporting Functionality"" item for further details."),
								RegistryStorageFlags.Company,
								RegistryOptions.Default,
								true
								));
						});

				item.OnBuildLogReference += (args) => Res.GetString("EB8C0794-71EB-4E72-BE83-D66E2BC838FE", "E-Reporting Functionality - Payables = [{0}].", args.NewValue);

				return item;
			}
		}

		public EInvoicingPendingTransactionsNotificationGroupRegistryItem EInvoicingPendingTransactionsNotificationGroup
		{
			get
			{
				var item = GetItem(nameof(EInvoicingPendingTransactionsNotificationGroup), () =>
				new EInvoicingPendingTransactionsNotificationGroupRegistryItem(nameof(EInvoicingPendingTransactionsNotificationGroup),
					Categories.Accounting_EReportingAndEInvoicingConfigurations,
					ResString.GetMultilingualString("78E4E46B-859F-411A-8CEF-692499CD410E", "Transactions Pending for E-Reporting Notification Group"),
					ResString.GetMultilingualString("077B5B43-BFFF-4458-86BA-F7268730B2DC", @"This registry only applies when the default Receivables E-Reporting Submit Pivot is set to 'PEN - Pending'.

The system will notify the party when transactions are pending for transmission based on the day calculation of the date criteria you have configured.
By default, no notification will be sent. If required, you can nominate a group, set the date criteria, and the days to be alerted of the pending transactions. The system will send the notification as per configuration."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default));

				item.OnBuildLogReference += (args) =>
				{
					var newValue = (EInvoicingPendingTransactionsNotificationGroup)args.NewValue;
					return Res.GetString("9CE20EDE-4AC2-4DEF-A5AF-1B49A3B2DF25", "Set notification group [{0}], date option [{1}] and [{2}] days.", newValue.GlbGroup?.GG_Code, newValue.DateType, newValue.Days);
				};

				return item;
			}
		}

		#region E-Reporting and E-Invoicing Configuration - Saudi Arabia

		public StringRegistryItem SaudiArabiaEInvoicingCSIDAPIEndPoint
			=> GetItem(nameof(SaudiArabiaEInvoicingCSIDAPIEndPoint), () =>
				new StringRegistryItem(nameof(SaudiArabiaEInvoicingCSIDAPIEndPoint),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_SaudiArabia,
					(NoResString)"Registration CSID API Endpoint",
					(NoResString)"This registry defines the CSID API endpoint used to request Saudi Arabia e-Invoicing credentials.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)(Env.Instance.IsProductionSystem ? SAEInvoicingAPIEndPoints.Production_CSID_URL : SAEInvoicingAPIEndPoints.Testing_CSID_URL)));

		public StringRegistryItem SaudiArabiaEInvoicingOnboardingAPIEndPoint
			=> GetItem(nameof(SaudiArabiaEInvoicingOnboardingAPIEndPoint), () =>
				new StringRegistryItem(nameof(SaudiArabiaEInvoicingOnboardingAPIEndPoint),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_SaudiArabia,
					(NoResString)"Registration Onboarding/Renewal API Endpoint",
					(NoResString)"This registry defines the Onboarding/Renewal API endpoint used to request Saudi Arabia e-Invoicing credentials.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)(Env.Instance.IsProductionSystem ? SAEInvoicingAPIEndPoints.Production_Onboarding_URL : SAEInvoicingAPIEndPoints.Testing_Onboarding_URL)));

		#endregion

		#region E-Reporting and E-Invoicing Configuration - Romania

		public StringRegistryItem RomaniaEInvoicingANAFOauthWebURL
		{
			get
			{
				return GetItem(nameof(RomaniaEInvoicingANAFOauthWebURL), () =>
				new StringRegistryItem(nameof(RomaniaEInvoicingANAFOauthWebURL),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania,
					(NoResString)"ANAF OAuth web URL (CargoWiseOne Support Only)",
					(NoResString)"ANAF OAuth web URL.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)("https://logincert.anaf.ro/anaf-oauth2/v1/authorize")));
			}
		}

		public StringRegistryItem RomaniaEInvoicingTestWTCCallbackWebSiteURL
			=> GetItem(nameof(RomaniaEInvoicingTestWTCCallbackWebSiteURL), () =>
				new StringRegistryItem(nameof(RomaniaEInvoicingTestWTCCallbackWebSiteURL),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania,
					(NoResString)"Test WTC Callback web site URL (CargoWiseOne Support Only)",
					(NoResString)"Test WTC Callback web site URL.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)("https://romaniaoauthcs-test.wisegrid.net/oauth/callback")));

		public StringRegistryItem RomaniaEInvoicingProductionWTCCallbackWebSiteURL
			=> GetItem(nameof(RomaniaEInvoicingProductionWTCCallbackWebSiteURL), () =>
				new StringRegistryItem(nameof(RomaniaEInvoicingProductionWTCCallbackWebSiteURL),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania,
					(NoResString)"Production WTC Callback web site URL (CargoWiseOne Support Only)",
					(NoResString)"Production WTC Callback web site URL.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					(NoResString)("https://romaniaoauthcs.wisegrid.net/oauth/callback")));

		public EInvoicingCredentialsRegistryItem RomaniaEInvoicingTestCredentials
			=> GetItem(nameof(RomaniaEInvoicingTestCredentials), () =>
				new EInvoicingCredentialsRegistryItem(nameof(RomaniaEInvoicingTestCredentials),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania,
					(NoResString)"Test E-Invoicing Credentials (CargoWiseOne Support Only)",
					(NoResString)string.Format(RomaniaEInvoicingCredentialsRegistryMessage, "Test"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					defaultValue: GetRomaniaEInvoicingCredentialsDefaultValue(RomaniaEInvoicingTestClientId, RomaniaEInvoicingTestClientSecret)));

		public EInvoicingCredentialsRegistryItem RomaniaEInvoicingProductionCredentials
			=> GetItem(nameof(RomaniaEInvoicingProductionCredentials), () =>
				new EInvoicingCredentialsRegistryItem(nameof(RomaniaEInvoicingProductionCredentials),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania,
					(NoResString)"Production E-Invoicing Credentials (CargoWiseOne Support Only)",
					(NoResString)string.Format(RomaniaEInvoicingCredentialsRegistryMessage, "Production"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					defaultValue: GetRomaniaEInvoicingCredentialsDefaultValue(RomaniaEInvoicingProductionClientId, RomaniaEInvoicingProductionClientSecret)
					));

		EInvoicingCredentials GetRomaniaEInvoicingCredentialsDefaultValue(string clientId, string clientSecret)
		{
			var crypto = new AESCrypto(HashAlgorithmName.SHA256);
			var credential = new EInvoicingCredentials();
			credential.ClientId = crypto.DecryptStringAES(clientId, AccountingMasterFilesRegistry.Instance.RomaniaEncryptionKey.Value);
			credential.ClientSecret = crypto.DecryptStringAES(clientSecret, AccountingMasterFilesRegistry.Instance.RomaniaEncryptionKey.Value);
			return credential;
		}

		const string RomaniaEInvoicingProductionClientId = "EAAAADTLyDfLEIRMxarGdtnXe3/+8IJ+R76LZmmyKTWNLZFu91GNdmigofkzCMEol0AIVjNrZKTzP/2cLlmz5X6xhdsT8ZsQCvx/mVWQhYwCo76z";
		const string RomaniaEInvoicingProductionClientSecret = "EAAAAA6kREoJcp+loozwXcle1gkfcj87TynZV3Ee2sbqhmaj6aWs29AuUW3jtv/aBEguP5COTOIGQV/PzhSlASW+/b9kVrpIHf4vpZhNj4xGKJVcgQXgQErMGi11ONa9luXg4A==";
		const string RomaniaEInvoicingTestClientId = "EAAAAOs1SCy94qiWGEIun/uSysgw/qgOt/bVV/XWdkgBCVBgd7H8BfoGOzC0aNwkpZrrtfhlsLxcPuOuYNfPQuQHlmtCUO0za063hqkfcsddgMHB";
		const string RomaniaEInvoicingTestClientSecret = "EAAAAP9fwhODZCRtYwSoqajNvb9Ub/iCcX+XzHQDeJ0P7ei/ZXNXrw6co7iB8UuT5E1gOWj9R6lx0LKTYaW+C40hz+i1lTngvYZKAjlh+W+hgVGsx7P4bBaGDMmMJz9i0rhbYw==";
		string RomaniaEInvoicingCredentialsRegistryMessage => (NoResString)"This registry defines the Client ID and Client Secret used to process Romania E-Invoices for {0} Environment, which are allocated to Service Providers and could potentially also be allocated to Taxpayers.";

		public StringRegistryItem RomaniaEncryptionKey
		{
			get
			{
				return GetItem(nameof(RomaniaEncryptionKey), () =>
				{
					var item = new StringRegistryItem(nameof(RomaniaEncryptionKey),
						null, null, null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.IsReadOnly,
						"H1xQW12y9MmG3xUbJ6c5gETFJmakmcslZbVEZ3kD2e6gf4AShkE19NSPIHeB66kI7iToqL46YIRZAUicIQ9nkmo8naKDwFHbJDaOwv0N07b6gBf6pWHkK6oHHIMXPCm3IltLTyVOdGOkrth299XHOH6X0awup3qR3naQtDt9gko=");
					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		#endregion

		#region E-Invoicing Registries (China specific)

		public StringRegistryItem ChinaEInvoicingCredentials
		{
			get
			{
				var item = GetItem("ChinaEInvoicingCredentials", delegate
				{
					return new StringRegistryItem("ChinaEInvoicingCredentials",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
						ResString.GetMultilingualString("6B7B0E74-13BF-45DB-839E-3D5B1C940FD5", "E-Invoicing Credentials"),
						ResString.GetMultilingualString("1C7C0309-51F2-47EB-B263-F7E867FD7B03", "This registry defines the Client Number assigned by e-Invoicing service partner system."),
						RegistryStorageFlags.Branch,
						string.Empty)
					{
						DataType = new StringRegistryDataType(true)
					};
				});
				item.OnBuildLogReference += (args) => Res.GetString("8BFE6953-2687-42E6-BC57-E9CC8AC810BD", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		public BooleanRegistryItem ChinaTransmitAndIssueFapiao
		{
			get
			{
				var item = GetItem("ChinaTransmitAndIssueFapiao", delegate
				{
					return new BooleanRegistryItem("ChinaTransmitAndIssueFapiao",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
						(NoResString)"Transmit and Issue Fapiao",
						(NoResString)"This registry is relevant to China Login Company only.\r\n\r\nBy default, the registry is set to 'No' and Fapiao will need to be manually issued via RongJin's Tax Easy web portal.\r\nIf required, you can change the registry value to 'Yes' and Fapiao will be issued on transmission to RongJin's Tax Easy web portal.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						false)
					{ CountryFilterPKs = CountryFilterPKs.China };
				});
				item.OnBuildLogReference += (args) => Res.GetString("8BFE6953-2687-42E6-BC57-E9CC8AC810BD", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		public BooleanRegistryItem AlwaysTransmitNegativeChargesAsDiscount
		{
			get
			{
				var rongjin = "RongJin";

				var item = GetItem("AlwaysTransmitNegativeChargesAsDiscount", delegate
				{
					return new BooleanRegistryItem("AlwaysTransmitNegativeChargesAsDiscount",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
						ResString.GetMultilingualString("7F62AA48-2EC7-4E9C-945F-9F28997D3F67", "Always Transmit Negative Charges as Discount"),
						ResString.GetMultilingualString("0D54E374-786D-48F9-B13E-433ABA45D2F5", @"By default, the system will treat negative charges as discounts and you will not be allowed to merge negative charges with positive charges even if they are mapped to the same product or service code in {0}'s KPT system.

If you override this registry value to 'No', the system will not treat negative charges as discounts and you will be able to merge them with other charges with the same product or service code, if required.", rongjin),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						true)
					{ CountryFilterPKs = CountryFilterPKs.China };
				});
				item.OnBuildLogReference += (args) => Res.GetString("8BFE6953-2687-42E6-BC57-E9CC8AC810BD", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		#endregion

		#region E-Reporting and E-Invoicing Configuration - India

		public EInvoicingCredentialsRegistryItem IndiaEInvoicingCredentials
			=> GetItem(nameof(IndiaEInvoicingCredentials), () =>
				new EInvoicingCredentialsRegistryItem(nameof(IndiaEInvoicingCredentials),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_India,
					(NoResString)"E-Invoicing Credentials (CargoWiseOne Support Only)",
					(NoResString)"This registry defines the Client ID and Client Secret used to process India E-Invoices, which are allocated to Service Providers and could potentially also be allocated to Taxpayers.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport));

		#endregion

		#region E-Reporting and E-Invoicing Configuration - Israel

		public EInvoicingCredentialsRegistryItem IsraelEInvoicingCredentials
			=> GetItem(nameof(IsraelEInvoicingCredentials), () =>
				new EInvoicingCredentialsRegistryItem(nameof(IsraelEInvoicingCredentials),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel,
					(NoResString)"E-Invoicing Credentials (CargoWiseOne Support Only)",
					(NoResString)"This registry defines the Client ID and Client Secret used to process Israel E-Invoices, which are allocated to registered software systems by the government.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					defaultValue: !Env.Instance.IsProductionSystem
						? new EInvoicingCredentials() { ClientId = "2ca90d5915db89e91f96570e881f15ed", ClientSecret = "94804c01302f5481ff13df0d0b22c886" }
						: new EInvoicingCredentials()));

		public StringRegistryItem IsraelEInvoicingITAOAuthWebURL
			=> GetItem(nameof(IsraelEInvoicingITAOAuthWebURL), () =>
				new StringRegistryItem(nameof(IsraelEInvoicingITAOAuthWebURL),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel,
					(NoResString)"ITA OAuth Web URL (CargoWiseOne Support Only)",
					(NoResString)"ITA OAuth Web URL. Please enter the Web URL for ITA open authentication.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					defaultValue: !Env.Instance.IsProductionSystem
						? "https://openapi.taxes.gov.il/shaam/tsandbox/longtimetoken/oauth2/authorize"
						: ""));

		public StringRegistryItem IsraelEInvoicingWTCCallbackSiteWebURL
			=> GetItem(nameof(IsraelEInvoicingWTCCallbackSiteWebURL), () =>
				new StringRegistryItem(nameof(IsraelEInvoicingWTCCallbackSiteWebURL),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel,
					(NoResString)"WTC Callback site Web URL (CargoWiseOne Support Only)",
					(NoResString)"WTC Callback Site Web URL. Please enter the Web URL for the WTC Callback site.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					defaultValue: (NoResString)(Env.Instance.IsProductionSystem
						? "https://israeloauthcs.wisegrid.net/oauth/callback"
						: "https://israeloauthcs-test.wisegrid.net/oauth/callback")));

		public StringRegistryItem IsraelEncryptionKey
			=> GetItem(nameof(IsraelEncryptionKey), () =>
				{
					var encryptionKey = "ifbshpYlyGQlTUgq2j6vGKBN45B1Hpo3qJpfyACqoN4OxuSnfHkehUXXaBM5WM6HsCtApEsYV8mvBBBi7vMy8RlWDqur3AzRTzLNDYru6ab6NUCrrtPCT1FsuJinGwu7NYQ25dUd2lZdpAVgIoBOd55PJeToGcrkZX0WTGDjsJU=";
					var item = new StringRegistryItem(name: nameof(IsraelEncryptionKey),
						category: null,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden | RegistryOptions.IsReadOnly,
						defaultValue: encryptionKey)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};

					return item;
				});

		public InvoiceAmountBoundariesRegistryItem IsraelInvoiceAmountBoundaries
			=> GetItem(nameof(IsraelInvoiceAmountBoundaries), () =>
			{
				var defaultValue = new InvoiceAmountBoundaryCollection();
				defaultValue.AddDefaultValues();

				return new InvoiceAmountBoundariesRegistryItem(
					nameof(IsraelInvoiceAmountBoundaries),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel,
					(NoResString)"Israel e-Invoicing Net Amount Eligibility Criteria (CargoWiseOne Support Only)",
					(NoResString)"Israel e-Invoicing Net Amount Eligibility Criteria. Invoices with equal or greater net amounts will be considered eligible for e-invoice based on the registry value. (NIS)",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					defaultValue);
			});

		#endregion

		public StringRegistryItem APIKeyGeneratorStrategyEncryptionKey
			=> GetItem(nameof(APIKeyGeneratorStrategyEncryptionKey), () =>
			{
				var encryptionKey = (NoResString)"MPRc7lZ6YOOATOVS8iB6s6zWgi0jc8IrZOvxVxp9nDrS+vnW2R5u95/tdh+3W8giU/Fw6x4QMZZdtAcrjuIitxMFZmmx/2vEHvA6jG3mJmzl5zvJjZBAqoSaD2m1onBw6G9DHGpZou9ilggoPz7Dsq6GH0HJKMtqrwLB2xqVzRg=";
				var item = new StringRegistryItem(name: nameof(APIKeyGeneratorStrategyEncryptionKey),
					category: null,
					caption: null,
					hint: null,
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsHidden | RegistryOptions.IsReadOnly,
					defaultValue: encryptionKey)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				};

				return item;
			});

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public DateTimeRegistryItem EReportingComplianceDate =>
			GetItem("EReportingComplianceDate", () => new DateTimeRegistryItem(
				new EReportingComplianceDateRegistryItemImpl(
					"EReportingComplianceDate",
					Categories.Accounting_EReportingAndEInvoicingConfigurations,
					(NoResString)"E-Reporting Compliance Date - Receivables (CargoWiseOne Support Only)",
					(NoResString)@"This registry is used to control the date on which the E-Reporting functionality for Receivables gets enabled in the supported countries.

Where E-Reporting for Receivables is supported, the default state of the registry ""Enable E-Reporting Functionality - Receivables"" will change from No to Yes on the Compliance Date shown in this registry.

For countries where E-Reporting for Receivables functionality is not currently supported, the Compliance Date is blank.

NOTE: For Taiwan, this date will be set to today's date when 'Enable E-Reporting Functionality - Receivables' registry is set to 'Yes'."
			)));

		class EReportingComplianceDateRegistryItemImpl : RegistryItemImpl
		{
			public EReportingComplianceDateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, bool isForPayables = false)
				: base(name, category, caption, hint, RegistryDataTypes.DateTimeType, new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short), RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, DateTime.MinValue)
			{
				IsForAP = isForPayables;
			}

			Dictionary<Guid, DateTime> Cache;
			readonly bool IsForAP;

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				DateTime result;
#if DEBUG
				if (Cache != null && Globals.IsTest)
				{
					Cache = null;
				}
#endif
				if (Cache == null)
				{
					Cache = new Dictionary<Guid, DateTime>();
					UpdateCache();
				}

				if (!Cache.TryGetValue(companyPK, out result))
				{
					result = DateTime.MinValue;
				}

				return result;
			}

			void UpdateCache()
			{
				var factory = new BusinessObjectFactory();
				var companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Cache.Keys);
				var companies = factory.Load<GlbCompany>(companyQuery);

				foreach (GlbCompany company in companies)
				{
					var defaultValue = DateTime.MinValue;
					var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceInfoElectronicInvoicing(company.GC_RN_NKCountryCode);
					if (complianceInfo != null)
					{
						var complianceDate = IsForAP ? complianceInfo.GetEInvoicingComplianceDateForPayables() : complianceInfo.GetEInvoicingComplianceDate();
						if (!complianceDate.IsEmpty)
						{
							defaultValue = complianceDate.ToDateTime();
						}
					}
					Cache.Add(company.PK.ToGuid(), defaultValue);
				}
			}
		}

		public DateTimeRegistryItem EReportingComplianceDateNewSchema
		{
			get
			{
				return GetItem("EReportingComplianceDateNewSchema", delegate
				{
					return new DateTimeRegistryItem(
						new EReportingComplianceDateNewSchemaRegistryItemImpl(
						"EReportingComplianceDateNewSchema",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						ResString.GetMultilingualString("42468345-8BBD-4419-B9FB-16F0CD2A7664", "E-Reporting Compliance Date - New Schema for Receivables"),
						ResString.GetMultilingualString("E517AF97-58F6-4A7A-B8EF-58781430C6BD", @"This registry is used to control the transition to a new schema when you are logged into a country environment where the government allows a switch period between an old schema and a new one.

For countries where only one schema exists the date will be empty."),
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						DateTime.MinValue
						));
				});
			}
		}

		class EReportingComplianceDateNewSchemaRegistryItemImpl : RegistryItemImpl
		{
			public EReportingComplianceDateNewSchemaRegistryItemImpl(string name, MultilingualString category, MultilingualString caption,
				MultilingualString hint, DateTimeRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, DateTime defaultValue)
				: base(name, category, caption, hint, RegistryDataTypes.DateTimeType, editorInfo, storage, options, defaultValue)
			{
			}

			Dictionary<Guid, DateTime> Cache;

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				DateTime result;
#if DEBUG
				if (Cache != null && Globals.IsTest)
				{
					Cache = null;
				}
#endif
				if (Cache == null)
				{
					Cache = new Dictionary<Guid, DateTime>();
					UpdateCache();
				}

				if (!Cache.TryGetValue(companyPK, out result))
				{
					result = DateTime.MinValue;
				}

				return result;
			}

			void UpdateCache()
			{
				var factory = new BusinessObjectFactory();
				var companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Cache.Keys);
				var companies = factory.Load<GlbCompany>(companyQuery);
				var overriddenDefaultValueByCountry = OverriddenCountryDates();

				foreach (GlbCompany company in companies)
				{
					var defaultValue = DateTime.MinValue;
					overriddenDefaultValueByCountry.TryGetValue(company.GC_RN_NKCountryCode, out defaultValue);
					Cache.Add(company.PK.ToGuid(), defaultValue);
				}
			}

			Dictionary<ZString, DateTime> OverriddenCountryDates()
				=> new Dictionary<ZString, DateTime>()
				{
					{ Constants.CountryCodes.Italy,   new DateTime(2021, 1, 1) },
					{ Constants.CountryCodes.Hungary,   new DateTime(2021, 4, 1) },
				};
		}

		public DateTimeRegistryItem EReportingComplianceDateForPayables =>
			GetItem("EReportingComplianceDateForPayables", () => new DateTimeRegistryItem(
				new EReportingComplianceDateRegistryItemImpl(
					"EReportingComplianceDateForPayables",
					Categories.Accounting_EReportingAndEInvoicingConfigurations,
					(NoResString)"E-Reporting Compliance Date - Payables (CargoWiseOne Support Only)",
					(NoResString)@"This registry is used to control the date on which the E-Reporting functionality for Payables gets enabled in the supported countries.

The default Compliance Date is blank for all countries, and it can be overriden with Date From value only in countries where e-Reporting functionality is supported.

NOTE: E-Reporting for Payables is currently supported only in Italy login companies.",
					true
			)));

		class EnableEInvoicingFunctionalityRegistryItemImpl : RegistryItemImpl
		{
			public EnableEInvoicingFunctionalityRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, bool isForPayables = false)
				: base(name, category, caption, hint, new BooleanRegistryDataType(), storage, option)
			{
				IsForAP = isForPayables;
			}

			Dictionary<Guid, bool> Cache;
			Dictionary<ZString, ZDateTime> LocationDateTimeByCountryCode;
			readonly bool IsForAP;

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				bool result;
#if DEBUG
				if (Cache != null && Globals.IsTest)
				{
					Cache = null;
				}
#endif
				if (Cache == null)
				{
					Cache = new Dictionary<Guid, bool>();
				}

				if (!Cache.TryGetValue(companyPK, out result))
				{
					result = GetDefaultValueAndUpdateCache(companyPK);
				}

				return result;
			}

			protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
			{
				if ((bool)newValue)
				{
					var company = new BusinessObjectFactory().Load<GlbCompany>(companyOrOwnerPK);
					if (company != null && (ObjectFactory.Get<ICountryComplianceFactory>().GetIEInvoicingRegistryProvider(company.GC_RN_NKCountryCode)?.ShouldAutoSetEReportingComplianceDate ?? false))
					{
						var dateRegItem = IsForAP ? Instance.EReportingComplianceDateForPayables : Instance.EReportingComplianceDate;
						var complianceDate = dateRegItem.GetFallBackValueAtAllLevels(companyOrOwnerPK, branchPK, departmentPK);
						if (complianceDate == DateTime.MinValue)
						{
							dateRegItem.SetValue(companyOrOwnerPK, branchPK, departmentPK, ZDateTime.Today.ToDateTime());
						}
					}
				}
				base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
			}

			bool GetDefaultValueAndUpdateCache(Guid companyPK)
			{
				var factory = new BusinessObjectFactory();
				var company = factory.Load<GlbCompany>(companyPK);
				bool defaultValue = false;

				if (company != null)
				{
					var dateRegItem = IsForAP ? Instance.EReportingComplianceDateForPayables : Instance.EReportingComplianceDate;
					var complianceDateForCountry = dateRegItem.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (complianceDateForCountry != DateTime.MinValue)
					{
						if (LocationDateTimeByCountryCode == null)
						{
							LocationDateTimeByCountryCode = new Dictionary<ZString, ZDateTime>();
						}

						var countryCode = company.GC_RN_NKCountryCode;
						ZDateTime currentDateForCountry;
						if (!LocationDateTimeByCountryCode.TryGetValue(countryCode, out currentDateForCountry))
						{
							currentDateForCountry = AccountingMasterFilesUtils.GetMaxLocationDateTimeByCountryCode(factory, countryCode);
							LocationDateTimeByCountryCode[countryCode] = currentDateForCountry;
						}

						if (!currentDateForCountry.IsEmpty)
						{
							defaultValue = currentDateForCountry >= complianceDateForCountry;
						}
					}
					Cache.Add(company.PK.ToGuid(), defaultValue);
				}

				return defaultValue;
			}
		}

		public DateTimeRegistryItem TransactionAuthorizationNumberDate =>
			GetItem("TransactionAuthorizationNumberDate", () => new DateTimeRegistryItem(
				new TransactionAuthorizationNumberDateRegistryItemImpl(
					"TransactionAuthorizationNumberDate",
					Categories.Accounting,
					(NoResString)"Transaction Authorization Number Date (CargoWiseOne Support Only)",
					(NoResString)@"This registry is used to control the date on wich the Transaction Authorization Number functionality gets enabled in the supported countries.
The default state of the registry will change from No to Yes on the Compliance Date shown in the registry.
For countries where the functionality is not currently supported, the Compliance Date is blank.

In Portugal, this feature is named ATCUD"
			)));

		class TransactionAuthorizationNumberDateRegistryItemImpl : RegistryItemImpl
		{
			public TransactionAuthorizationNumberDateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
				: base(name, category, caption, hint, RegistryDataTypes.DateTimeType, new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short), RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, DateTime.MinValue)
			{
			}

			Dictionary<Guid, DateTime> Cache;

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				DateTime result;
#if DEBUG
				if (Cache != null && Globals.IsTest)
				{
					Cache = null;
				}
#endif
				if (Cache == null)
				{
					Cache = new Dictionary<Guid, DateTime>();
					UpdateCache();
				}

				if (!Cache.TryGetValue(companyPK, out result))
				{
					result = DateTime.MaxValue;
				}

				return result;
			}

			void UpdateCache()
			{
				var companies = new BusinessObjectFactory().Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Cache.Keys));

				foreach (GlbCompany company in companies)
				{
					var transactionAuthorizationNumberDate = ObjectFactory.Get<ICountryComplianceFactory>()?.GetITransactionAuthorizationNumber(company.GC_RN_NKCountryCode)?.GetDefaultTransactionAuthorizationNumberFeatureEnabledStartDate() ?? ZDate.Empty;
					var defaultValue = !transactionAuthorizationNumberDate.IsEmpty ? transactionAuthorizationNumberDate.ToDateTime() : DateTime.MaxValue;
					Cache.Add(company.PK.ToGuid(), defaultValue);
				}
			}
		}

		public CodePairRegistryItem EReportingTransactionNumber => GetItem("EReportingTransactionNumber",
			() => new CodePairRegistryItem(
				"EReportingTransactionNumber",
				Categories.Accounting_EReportingAndEInvoicingConfigurations_Italy,
				ResString.GetMultilingualString("961D2304-6D09-459B-A6AE-3399C3CB71F1", "Transaction Number option for E-Invoicing"),
				ResString.GetMultilingualString("101BA941-7A21-46CE-8E74-E7EF01B02E08", @"This registry is used to decide which Reference Number should be sent via e-Reporting, according to the rules of each Login country.
When setting the value to 'COM', then the system uses the Compliance Number;
when setting the value to 'INV', then the system uses the Invoice (Transaction) Number;
when setting the value to 'CIN', then the system uses the Compliance Number if it is filled, else it uses the Transaction Number.

NOTE: when this registry is set to 'COM' and the Transaction has not been assigned the Compliance Number during posting, sending the e-Invoice may be prevented.
To assign the Compliance Number and enable sending you need to proceed in this way:
a) assign the Compliance Number using the actions 'Allocate Compliance Number' or 'Update Compliance Subtype and Number' in the Receivables Transactions module;
b) add the Transaction to the queue using the action 'Reset Status to Queued' in the Receivables Transactions module."),
				new CodeDescriptionPairListProvider(() => new EReportingTransactionNumberOptions()),
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				EReportingTransactionNumberOptions.ComplianceOrInvoiceNr.Code)
			);

		public CodePairRegistryItem EReportingGEIMessageSystemType => GetItem("EReportingGEIMessageSystemType",
			() =>
			{
				var item = new CodePairRegistryItem(
						"EReportingGEIMessageSystemType",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						(NoResString)"E-Reporting GEI Message System Type(CargoWise Support Only)",
						(NoResString)@"This registry is used to define the GEI message system type of E-Reporting functionality.
By default, the system defines the GEI message system type based on the system license code.",
						new CodeDescriptionPairListProvider(() => new EReportingGEIMessageSystemTypeOptions()),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						EReportingGEIMessageSystemTypeOptions.Default.Code);

				item.OnBuildLogReference += (args) => Res.GetString("4181DDA9-56DC-46EF-9719-C9D250A623BD", "GEI Message System Type changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

				return item;
			}
		);

		public CodeDescriptionPairListRegistryItem EInvoicingReversalCodes
		{
			get
			{
				return GetItem("EInvoicingReversalCodes",
					() => new CodeDescriptionPairListRegistryItem(
						new CountrySpecificDefaultValueRegistryItemImpl<ReadOnlyCodeDescriptionPairList>(
							"EInvoicingReversalCodes",
							Categories.Accounting_EReportingAndEInvoicingConfigurations,
							(NoResString)"This registry is used to configure the Reversal Codes according to the tax authorities of each country within the E-Invoicing module",
							new CodeDescriptionPairListRegistryDataType(2),
							RegistryOptions.CacheExpensiveDefaultValue | RegistryOptions.IsOnlyForSupport,
							FactoryForCountryDefaultValues,
							new EInvoicingReversalCodes_RegistryDescriptor(),
							editorInfo: new CodeDescriptionPairListEditorInfo()),
						false,
						new CodeDescriptionPairList())
					);
			}
		}

		#endregion

		public CodePairRegistryItem EReportingSubmitPivotDefaultStatus
		{
			get
			{
				return GetItem("EReportingSubmitPivotDefaultStatus", delegate
				{
					return new CodePairRegistryItem(
						new CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<string>(
						"EReportingSubmitPivotDefaultStatus",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						(NoResString)"E-Reporting Submit Pivot Default Status - Receivables (CargoWiseOne Support Only)",
						(NoResString)@"This registry is used to control the status of the Submit Pivot created or re-queued for eligible to E-Reporting Receivables transactions.

Where E-Reporting for Receivables is supported, the default status could be set to ""QUE"" or ""PEN"".

""QUE"" status indicates that transacttions are ready for sending.
""PEN"" status will need an extra action like digital signature to be changed to ""QUE"".

For countries where E-Reporting for Receivables functionality is not currently supported, the Default Status is blank.",
						RegistryDataTypes.StringType,
						new ComboBoxRegistryEditorInfo(OLookUpEditType.EInvoicingPivotState),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						(complianceInfo) => (complianceInfo as IComplianceInfoElectronicInvoicing)?.GetDefaultEInvoicingSubmitPivotStatus() ?? string.Empty,
						string.Empty
						));
				});
			}
		}

		public CodePairRegistryItem EReportingSubmitPivotDefaultStatusForPayables
		{
			get
			{
				return GetItem("EReportingSubmitPivotDefaultStatusForPayables", delegate
				{
					return new CodePairRegistryItem(
						new EReportingSubmitPivotDefaultStatusForPayablesRegistryItemImpl(
						"EReportingSubmitPivotDefaultStatusForPayables",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						(NoResString)"E-Reporting Submit Pivot Default Status - Payables (CargoWiseOne Support Only)",
						(NoResString)@"This registry is used to control the status of the Submit Pivot created or re-queued for eligible E-Reporting Payables transactions.

Where E-Reporting for Payables is supported, the default status could be set to ""QUE"" or ""PEN"".

""QUE"" status indicates that a transaction is ready for sending.
""PEN"" status indicates that there is an extra action required before it can be queued.

For countries where E-Reporting for Payables functionality is not supported, the default status is ""Blank"".",
						RegistryDataTypes.StringType,
						new ComboBoxRegistryEditorInfo(OLookUpEditType.EInvoicingPivotState),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						(complianceInfo) => string.Empty,
						string.Empty
						));
				});
			}
		}

		public CodePairRegistryItem ExportMultipleDebtorOrganizationContactEmail
		{
			get
			{
				return GetItem("ExportMultipleDebtorOrganizationContactEmail", () =>
					 new CodePairRegistryItem(
						"ExportMultipleDebtorOrganizationContactEmail",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						ResString.GetMultilingualString("C7038CC2-DCBD-4909-B615-CF57F07B5309", "Export Multiple Debtor Organization Contact Email"),
						ResString.GetMultilingualString("9B378E19-8E8B-4885-ABFA-572ADD1A82BF", @"This registry is relevant to Login Countries where the electronic invoicing Service Partner or Government Portal supports automatic mailing of Tax Invoice to multiple recipients.

By default, only one Debtor Organization Contact email address will be included during the electronic invoice transmission based on the existing defaulting and fallback rules(if any).
When enabled, multiple Debtor Organization Contact emails will be included during the electronic invoice transmission. Please refer to the respective country's e-Learning Materials for details of the transmission logic.

NOTE: Currently, this registry is only relevant to Vietnam and South Korea Login Companies for which the Receivables E-Reporting Functionality is enabled."),
						new CodeDescriptionPairListProvider(() => ExportMultipleDebtorOrganizationContactEmailOptions),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ExportMultipleDebtorOrganizationContactEmailCodes.DEF)
					 { CountryFilterPKs = new[] { Constants.CountryGuids.Vietnam, Constants.CountryGuids.KoreaRepublicof } }
				);
			}
		}

		public BooleanRegistryItem IncludeContactEmailsOfLocalDebtorOrganizationOnly
		{
			get
			{
				return GetItem("IncludeContactEmailsOfLocalDebtorOrganizationOnly", () =>
					new BooleanRegistryItem(
						"IncludeContactEmailsOfLocalDebtorOrganizationOnly",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						ResString.GetMultilingualString("66D9105B-0235-404F-8E9E-2BF004653EBE", "Include Contact Emails of Local Debtor Organization Only"),
						ResString.GetMultilingualString("E3617A70-24A5-4540-B371-FA52C5AFBA70", @"This registry is used in conjunction with the 'Export Multiple Debtor Organization Contact Email' registry to control whether the system exports the contact email address for local debtors only.
By default, the system exports the debtor's contact email address during electronic invoice transmission, regardless of the debtor’s location (local or foreign).
When this registry is enabled, the system will export the debtor’s contact email address only if the debtor is local.

Note: This registry is currently used for Vietnam login companies only."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Vietnam }
				);
			}
		}

		public class EReportingSubmitPivotDefaultStatusForPayablesRegistryItemImpl : CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<string>
		{
			public EReportingSubmitPivotDefaultStatusForPayablesRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions option, Func<ICountryComplianceInfo, string> defaultValueGetter, string fallbackDefault)
				: base(name, category, caption, hint, RegistryDataTypes.StringType, editorInfo, storage, option, defaultValueGetter, fallbackDefault)
			{
			}

			protected override string GetDefaultValueByCompany(GlbCompany company)
			{
				var defaultValue = fallbackDefault;
				var complianceInfo = GetComplianceInfoFromCompany(company) as IComplianceInfoElectronicInvoicing;
				if (complianceInfo != null)
				{
					defaultValue = complianceInfo.GetDefaultEInvoicingSubmitPivotStatusForPayables(company);
				}
				return defaultValue;
			}
		}

		public CodePairRegistryItem EReportingSubmitPivotDefaultStatusForPayablesOnlyItaly => GetItem("EReportingSubmitPivotDefaultStatusForPayablesOnlyItaly",
			() => new CodePairRegistryItem(
				"EReportingSubmitPivotDefaultStatusForPayablesOnlyItaly",
				Categories.Accounting_EReportingAndEInvoicingConfigurations_Italy,
				ResString.GetMultilingualString("CBFEE774-7D88-40C1-B769-D0613CD032D0", "Default E-Reporting Status - Payables"),
				ResString.GetMultilingualString("6AB0952C-16C4-4D4F-83FD-5DE5A2929FE9", @"This registry is used to control the default E-Reporting Status when E-reporting functionality for Payables is active.

Unless overridden, eligible transactions are posted in PEN - Pending status and require a user to review and queue the transaction for sending.

If you prefer to send the eligible transactions without requiring a manual action, change the value of this registry to QUE - Queued. Eligible transactions will be queued for sending as soon as posted."),
				new CodeDescriptionPairListProvider(() => new EReportingSubmitPivotDefaultStatusForPayablesOnlyItalyOptions()),
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				EReportingSubmitPivotDefaultStatusForPayablesOnlyItalyOptions.Pending.Code)
			{ CountryFilterPKs = CountryFilterPKs.Italy });

		public MultilingualStringRegistryItem EReportingPivotPendingStatusDescription
		{
			get
			{
				return GetItem("EReportingPivotPendingStatusDescription", delegate
				{
					return new MultilingualStringRegistryItem(new EReportingPivotPendingStatusDescriptionRegistryItemImpl(
							"EReportingPivotPendingStatusDescription",
							Categories.Accounting_EReportingAndEInvoicingConfigurations,
							(NoResString)"E-Reporting Pivot Pending Status Description (CargoWiseOne Support Only)",
							(NoResString)@"This registry is used to control description of the Pivot's Pending status.

Where E-Reporting is supported and default status of the Submit Pivot is set to ""PEN"" we will need to do some action to change it to ""QUE"".

Description of the ""PEN"" status code describes a pending extra action like digital signature which needs to be done to change status to ""QUE"".

For countries where E-Reporting functionality is not currently supported, the Pending Status Description is blank.",
							new TextRegistryEditorInfo(TextEditorType.TextBox),
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							string.Empty));
				});
			}
		}

		class EReportingPivotPendingStatusDescriptionRegistryItemImpl : RegistryItemImpl
		{
			public EReportingPivotPendingStatusDescriptionRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
				: base(name, category, caption, hint, new StringRegistryDataType(), editorInfo, storage, options, defaultValue)
			{
			}

			Dictionary<Guid, MultilingualString> Cache;

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				MultilingualString result;
#if DEBUG
				if (Cache != null && Globals.IsTest)
				{
					Cache = null;
				}
#endif
				if (Cache == null)
				{
					Cache = new Dictionary<Guid, MultilingualString>();
					UpdateCache();
				}

				if (!Cache.TryGetValue(companyPK, out result))
				{
					result = (NoResString)string.Empty;
				}

				return result.ToString();
			}

			void UpdateCache()
			{
				var factory = new BusinessObjectFactory();
				var companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Cache.Keys);
				var companies = factory.Load<GlbCompany>(companyQuery);

				foreach (var company in companies)
				{
					var complianceInfo = CountryComplianceFactory.GetICountryComplianceEInvoice(company.GC_RN_NKCountryCode);
					var defaultValue = complianceInfo?.GetDefaultEInvoicingPivotPendingStatusDescription() ?? (NoResString)string.Empty;
					Cache.Add(company.PK.ToGuid(), defaultValue);
				}
			}
		}

		public BooleanRegistryItem EnableReportCriticalValidationErrorsAfterDBSaving
		{
			get
			{
				return GetItem("EnableReportCriticalValidationErrorsAfterDBSaving", delegate
				{
					return new BooleanRegistryItem(
						"EnableReportCriticalValidationErrorsAfterDBSaving",
						Categories.Accounting_CriticalValidation,
						(NoResString)"Enable Report Critical Validation Errors After DBSaving",
						(NoResString)"This Registry Item enables postponement of critical validation error reporting. The errors will be thrown if no DB error happened. It is set to Yes by default. Set it to No when we want to throw critical validation errors immediately.",
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, true);
				});
			}
		}

		public BooleanRegistryItem EnableCrossTradeDebtorDefaultingFunctionality
		{
			get
			{
				return GetItem("EnableCrossTradeDebtorDefaultingFunctionality", delegate
				{
					return new BooleanRegistryItem(
						"EnableCrossTradeDebtorDefaultingFunctionality",
						Categories.Accounting_JobInvoicing,
						(NoResString)"Enable Cross Trade Debtor Defaulting Functionality (CargoWiseOne Support Only)",
						(NoResString)@"When this registry is set to Yes, on Cross Trade jobs, the Local Client field will read ‘Prepaid Bill-To Party’ and the Overseas Agent field will read 'Collect Bill-To Party’.
These fields will populate with the job’s Consignor’s IFT (falling back to Consignor) and Consignee’s IFT (falling back to Consignee) respectively.
The charge line debtor will default according to the Accounting > Job Invoicing > Cross Trade Debtor Defaulting Configuration registry. 
This applies to Forwarding Shipments, Quick Bookings and Bookings with Quotes.


These changes were implemented on PRJ00039869 and it is expected that this will be standard functionality and this registry will be removed.
IMPORTANT: Do not change this registry for any client. This registry will be removed in a future release and the behaviour will revert to the default. Should a client raise a complaint that the Cross Trade behaviour associated with this registry (as specified above) is not suitable, please escalate to the Accounting Product Team.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableComplianceReportsForAustralia
			=> GetItem(nameof(EnableComplianceReportsForAustralia),
				() => new BooleanRegistryItem(nameof(EnableComplianceReportsForAustralia),
					AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
					ResString.GetMultilingualString("71E1C292-2089-42A3-B179-E9D39FC46E29", "Enable Compliance Reports for Australia"),
					ResString.GetMultilingualString("7DD714E8-B98A-4C1D-843F-647BC32A5BAB", @"This registry is referenced by Australian login companies only.

When enabled, CargoWise can generate the following reports:
- TPAR (Taxable Payments Annual Report) output, required to be lodged annually. 
- PTRS (Payment Times Reporting Scheme) output, required to be lodged every 6 months

Not all AU businesses are required to lodge either the TPAR return and/or the PTRS return."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					false)
			);

		public BooleanRegistryItem UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport
		{
			get
			{
				return GetItem("UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport",
					delegate
					{
						return new BooleanRegistryItem("UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport",
							Categories.Accounting_PayableDefaults_DefaultSettings,
							ResString.GetMultilingualString("FB885E55-B116-48DD-AE9C-91BCF36D4F9F", "Use Import Company Charge Code's Tax Overrides for Intercompany Invoice Import"),
							ResString.GetMultilingualString("3B9285E0-8B0D-492F-B421-FB1CC4E5BA42", @"By default, the Tax ID will be set according to system defined tax defaulting logic.
When this system registry is enabled, the system will set the Tax ID according to your login company's charge code tax overrides setup before it falls back to system defined tax defaulting logic.

A new 'Transaction Context' column has been added to facilitate the defining of GST Tax Overrides for Intercompany Invoice Import in the following screens:
a. Maintain > Account > Charge Codes;
b. Maintain > Account > Tax Override Groups;

With GST Tax Overrides, you have the benefit of specifying the applicable Tax ID and Tax Message.

Note:
This registry must be set to 'Yes' before you can define the GST Tax Overrides for 'Intercompany Invoice Import' context."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							false);
					});
			}
		}

		public BooleanRegistryItem EnableTaxBranchFeature
		{
			get
			{
				return GetItem("EnableTaxBranchFeature", () =>
				{
					var item = new BooleanRegistryItem("EnableTaxBranchFeature",
						Categories.Accounting,
						(NoResString)"Enable Tax Branch Feature",
						(NoResString)@"We will be developing the Tax Branch Reporting in phases.
In the initial phase, we will be building changes to support the recording of tax branch against revenue and cost charge line.
This value will then be used for recording of tax and allocation of compliance sequence number, etc.
This value will be available for export via Universal Shipment, Transaction and Transaction Batch XML.

In subsequent phases, we will build changes to support tax branch reporting in the sub ledger and general ledger.

By default, this registry is set to 'No' and this feature is not enabled.
When this registry is set to 'Yes', the system will export the following functions:
1. New security items relating to Tax Branch Reporting feature.
2. Enable Tax Branch Reporting registry.
3. Custom Job Tax Branch Defaulting Rules Engine Configuration.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);

					item.OnBuildLogReference += (args) => Res.GetString("c5609c27-abe1-4feb-9488-96f02f12fbe7", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		public BooleanRegistryItem EnableTaxBranchReporting
		{
			get
			{
				return GetItem("EnableTaxBranchReporting", () =>
				{
					var item = new BooleanRegistryItem("EnableTaxBranchReporting",
						Categories.Accounting,
						ResString.GetMultilingualString("592b4089-ed53-490e-81ea-d3e410743f76", "Enable Tax Branch Reporting"),
						ResString.GetMultilingualString("a8769526-236f-455f-abfe-fa52361d09af", @"By default, this registry is set to 'No' and this feature is not enabled.

When enabled, a tax branch value must be recorded against revenue and cost if the current login company is Tax Registered and the debtor/creditor is tax applicable.

This value will be used for defaulting of tax id and allocation of compliance sequence number, etc.

This value will be available for export via Universal Shipment, Transaction and Transaction Batch XML.

This value will be used in 'Tax Transaction Analysis - Detail Report' and 'Tax Transaction Analysis - Summary Report'. This value will not be used in other reports.

Please do not enable this registry without consultation with the Accounting Product Team."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);

					item.OnBuildLogReference += (args) => Res.GetString("71da3c5b-e801-4879-8c66-f302a5430c1f", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		#region Tax ID / Tax Message Mapping

		public TaxIdAndTaxMessageCombinationRulesRegistryItem TaxIdAndTaxMessageCombinationRules
		{
			get
			{
				return GetItem("TaxIdAndTaxMessageCombinationRules", () =>
				{
					var item = new TaxIdAndTaxMessageCombinationRulesRegistryItem("TaxIdAndTaxMessageCombinationRules",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("F6EF8F09-25E7-4EA6-BA48-F7C165F472B2", "Tax ID and Tax Message Combination Rules"),
						ResString.GetMultilingualString("17722EE5-F172-48D6-BE6A-A2CD65409FA9", @"Tax Messages are used to explain the VAT/GST Tax ID treatment of Revenue and Cost charge lines.

This registry can be used to define the Permitted set of Tax Messages or Tax IDs.

Option 1: Limit permitted Tax Messages

Rules added to this grid affect validation of Tax Messages when recording Revenue or Cost charges for the Tax ID and Line Type defined in the grid.
Rules do not prevent the use of a Tax ID in other contexts.

Example: 
Line Type   Tax ID   Tax Message
  CST         VAT        MSG1
  CST         VAT        MSG4
  REV         VAT        MSG4

The setup above limits the permitted Tax Messages for use with the 'VAT' Tax ID.
- REV charges recorded with the Tax ID 'VAT' will only permit Tax Message 'MSG4'.
- CST charges recorded with the Tax ID 'VAT' will permit either 'MSG1' or 'MSG4'.
- No restrictions apply to the Tax Messages permitted when recording 'VAT' Tax ID in Direct Receipt or Direct Payment transactions because no DRC or DPY rules exist for the 'VAT' Tax ID.
- No restrictions apply to the Tax Messages permitted when recording other Tax IDs (i.e. EXEMPT, NOTREPORT, etc.) against CST, REV, DPY or DRC charge lines because No rules exist.

Option 2: Limit permitted Tax IDs

Rules added to this grid restrict the permitted set of Tax IDs and Tax Messages that can be used when recording Revenue and/or Costs.

Example: 
Line Type   Tax ID   Tax Message
  CST         VAT        MSG1
  DPY         VAT        MSG1
  REV         VAT        MSG1
  DRC         VAT        MSG1
  CST         VATREV    MSG2
  DPY         VATREV    MSG2

The setup above limits the permitted Tax IDs to 'VAT' and 'VATREV', and only can be used against the specified Line Types and Tax Messages.
- CST, DPY, REV and DRC charges will be permitted for 'VAT' Tax ID with Tax Message 'MSG1'.
- CST, DPY, REV and DRC charges will NOT be permitted for 'VAT' Tax ID with Tax Message 'MSG2'.
- CST and DPY charges will be permitted for 'VATREV' Tax ID with Tax Message 'MSG2'.
- CST and DPY charges will NOT be permitted for 'VATREV' Tax ID with Tax Message 'MSG1'.
- REV and DRC charges will NOT be permitted for 'VATREV' Tax ID with Tax Message 'MSG2'.
- CST, DPY, REV and DRC charges will NOT be permitted for 'FREEVAT' Tax ID (as well as other Tax IDs not specified in the registry) with any Tax Message.

Note:
REV rules defined in this registry apply to Sell charges in Job Billing; and charge lines in Receivables Invoice, Credit Note and Adjustment Note transactions.
CST rules defined in this registry apply to Cost charges in Job Billing and Consol Costing; and charge lines in Payables Invoice, Credit Note and Adjustment Note transactions.
DRC rules defined in this registry apply to charge lines in Cash Book Direct Receipt transactions.
DPY rules defined in this registry apply to charge lines in Cash Book Direct Payment transactions."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default);

					item.OnBuildLogReference += BuildTaxIdAndTaxMessageCombinationRulesLogReference;
					return item;
				});
			}
		}

		string BuildTaxIdAndTaxMessageCombinationRulesLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var originValue = (TaxIdAndTaxMessageCombinationRulesConfiguration)args.OriginalValue;
			var newValue = (TaxIdAndTaxMessageCombinationRulesConfiguration)args.NewValue;

			var originalElements = originValue.TaxIdAndTaxMessageCombinationRulesCollection.OfType<TaxIdAndTaxMessageCombinationRules>().ToArray();
			var newElements = newValue.TaxIdAndTaxMessageCombinationRulesCollection.OfType<TaxIdAndTaxMessageCombinationRules>().ToArray();
			var comparer = new FuncComparer<TaxIdAndTaxMessageCombinationRules>(CompareMethod);
			var addedItems = newElements.Except(originalElements, comparer);
			var deletedItems = originalElements.Except(newElements, comparer);

			var factory = new BusinessObjectFactory();
			var usedTaxId = factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.PK, addedItems.Select(x => x.TaxRate).Union(deletedItems.Select(x => x.TaxRate)).Distinct().ToArray()))
				.ToDictionary(x => x.PK, x => x);
			var usedTaxMessage = factory.Load<AccInvMsg>(new ZQuery(AccInvMsgSchema.PK, addedItems.Select(x => x.TaxMessage).Union(deletedItems.Select(x => x.TaxMessage)).Distinct().ToArray()))
				.ToDictionary(x => x.PK, x => x);

			var result = new StringBuilder();

			foreach (var element in addedItems)
			{
				var taxRateCode = usedTaxId.TryGetValue(element.TaxRate, out var taxRate)
					? taxRate.AT_Code
					: ZString.Empty;

				var taxMessageCode = usedTaxMessage.TryGetValue(element.TaxMessage, out var taxMessage)
					? taxMessage.A9_Code
					: ZString.Empty;
				result.AppendLine(Res.GetString("F2F4E6FD-0141-4A0C-A2AE-54AF9C3B8711", "Rule Added: Line Type:{0}, Tax ID:{1}, Tax Message:{2}", element.LineType, taxRateCode, taxMessageCode));
			}

			foreach (var element in deletedItems)
			{
				var taxRateCode = usedTaxId.TryGetValue(element.TaxRate, out var taxRate)
					? taxRate.AT_Code
					: ZString.Empty;

				var taxMessageCode = usedTaxMessage.TryGetValue(element.TaxMessage, out var taxMessage)
					? taxMessage.A9_Code
					: ZString.Empty;
				result.AppendLine(Res.GetString("F4BEE515-BBF4-4C91-A2E4-67213D827E25", "Rule Deleted: Line Type:{0}, Tax ID:{1}, Tax Message:{2}", element.LineType, taxRateCode, taxMessageCode));
			}

			if (!originValue.ValidationOption.Equals(newValue.ValidationOption))
			{
				result.AppendLine($"Validation Option set to '{(newValue.ValidationOptionsList[newValue.ValidationOption] as CodeDescriptionPair)?.CodeAndDescription}'.");
			}

			return result.ToString();

			int CompareMethod(TaxIdAndTaxMessageCombinationRules a, TaxIdAndTaxMessageCombinationRules b)
			{
				var compareProps = new Func<int>[] {
					() => a.LineType.CompareTo(b.LineType),
					() => a.TaxRate.CompareTo(b.TaxRate),
					() => a.TaxMessage.CompareTo(b.TaxMessage)
				};

				foreach (var compareProp in compareProps)
				{
					var compareResult = compareProp();
					if (compareResult != 0)
					{
						return compareResult;
					}
				}

				return 0;
			}
		}

		#endregion

		#region Compliance Document Number Allocation Rule

		public CodePairRegistryItem ComplianceDocumentNumberAllocationRuleReceivables
		{
			get
			{
				var item = GetItem("ComplianceDocumentNumberAllocationRule_Receivables", delegate
				{
					return new CodePairRegistryItem(
						"ComplianceDocumentNumberAllocationRule_Receivables",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("8A5622F0-3A92-4A0E-88EC-BF9D79506280", "Compliance Document Number Allocation Rule - Receivables"),
						ResString.GetMultilingualString("4403734E-2C31-432E-9141-23A84F9E2FFB", @"This registry determines how the  compliance number will be allocated. 

By default, the system uses the compliance sequence book that belongs to the current login branch and department to allocate a compliance number.
If required, you can override this registry setting to use the compliance sequence book that belongs to the transaction header branch and department. 

Note: This registry is only relevant to login countries where compliance sequences module has been enabled for the allocation of compliance numbers to transactions."),
						new CodeDescriptionPairListProvider(() => new ComplianceDocumentNumberAllocationRuleTypes()),
						RegistryStorageFlags.Company,
						ComplianceDocumentNumberAllocationRuleTypes.LBD.Code);
				});

				item.OnBuildLogReference += (args) => Res.GetString("F2D7A384-C8BE-4DD8-8B1F-DBDBCF946E90", "Registry has been overridden from {0} to {1}.", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		public CodePairRegistryItem ComplianceDocumentNumberAllocationRulePayables
		{
			get
			{
				var item = GetItem("ComplianceDocumentNumberAllocationRule_Payables", delegate
				{
					return new CodePairRegistryItem(
						"ComplianceDocumentNumberAllocationRule_Payables",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("D1B55403-EC7A-4F20-B40B-C3C490F61862", "Compliance Document Number Allocation Rule - Payables"),
						ResString.GetMultilingualString("98384FEA-A6E8-4805-8EB7-4747B0C3E4F4", @"This registry determines how the  compliance number will be allocated. 

By default, the system uses the compliance sequence book that belongs to the current login branch and department to allocate a compliance number.
If required, you can override this registry setting to use the compliance sequence book that belongs to the transaction header branch and department. 

Note: This registry is only relevant to login countries where compliance sequences module has been enabled for the allocation of compliance numbers to transactions."),
						new CodeDescriptionPairListProvider(() => new ComplianceDocumentNumberAllocationRuleTypes()),
						RegistryStorageFlags.Company,
						ComplianceDocumentNumberAllocationRuleTypes.LBD.Code);
				});

				item.OnBuildLogReference += (args) => Res.GetString("3E79F680-B217-4078-9A50-F1922772B717", "Registry has been overridden from {0} to {1}.", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		#endregion

		#region OFX Encryption Key
		public StringRegistryItem OFXEncryptionKey
		{
			get
			{
				return GetItem("OFXEncryptionKey", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"OFXEncryptionKey",
						null, null, null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.IsReadOnly,
						"");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}
		#endregion
		public BooleanRegistryItem EnableNewOSOutstandingAmountFeature
		{
			get
			{
				return GetItem("EnableNewOSOutstandingAmountFeature", () => new BooleanRegistryItem(
					"EnableNewOSOutstandingAmountFeature",
					Categories.Accounting,
					(NoResString)"Enable New OS Outstanding Amount Feature (CargoWiseOne Support only)",
					(NoResString)@"By default, this registry is set to 'No' and this feature is not enabled.
Once it's set to 'Yes', this registry cannot be turn off.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false,
					new EnableNewOsOutstandingAmountDataType()));
			}
		}

		public BooleanRegistryItem DisallowPostingInvoicesWithAFutureInvoiceDate
		{
			get
			{
				var item = GetItem("DisallowPostingInvoicesWithAFutureInvoiceDate", () => new BooleanRegistryItem(
					"DisallowPostingInvoicesWithAFutureInvoiceDate",
					Categories.Accounting_ReceivableDefaults_DefaultSettings,
					ResString.GetMultilingualString("316BCEDD-B943-4B95-9B15-311143A24C0A", "Disallow Posting Invoices With A Future Invoice Date"),
					ResString.GetMultilingualString("8925451A-77A3-4AA5-BAA6-BD90227C7AE5", @"By default, this registry is set to 'No' and no restriction will be applied.

When the registry is set to 'Yes', the system will prevent users from posting AR Invoice, Credit Note and Adjustment Note with a future invoice date."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					false));

				item.OnBuildLogReference += (args) => Res.GetString("FE3CEBF3-7600-460E-89BB-81F7682043ED", "Registry set to '{0}'.", args.NewValue);
				return item;
			}
		}

		#region E-Invoicing Registries (Korea specific)

		#region TaxTypeToTaxInvoiceDocumentTypeCodeMapping

		public CodeDescriptionWithGroupRegistryItem TaxTypeToTaxInvoiceDocumentTypeCodeMapping
		{
			get
			{
				var item = GetItem("TaxTypeToTaxInvoiceDocumentTypeCodeMapping", delegate
				{
					var item = new CodeDescriptionWithGroupRegistryItem(
						"TaxTypeToTaxInvoiceDocumentTypeCodeMapping",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
						ResString.GetMultilingualString("73441C69-A7C1-4BAB-B44F-8135807B4F60", "Tax Type to Tax Invoice Document's Type Code Mapping (CargoWiseOne Support Only)"),
						ResString.GetMultilingualString("37A31ABA-BF2C-4210-8578-A84F520CBF77", @"This is the mapping between Tax Type to Tax Invoice Document's Type Code Mapping. 
This registry specifies what Type Code should be used for different Tax Rate Types for Original/Amendment Transactions."),
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						new CodeDescriptionWithGroupRegistryEditorInfo(ResString.GetMultilingualString("2F9BABD8-77CB-43FE-BD5C-C7C4A1DD6B22", "Tax Invoice Document/Type Code"), true, true),
					GetTaxRateTypeMappingListDefaultValue());

					item.OnBuildLogReference += BuildTaxTypeToTaxInvoiceDocumentTypeCodeMapping;
					(item.DataType as RegistryDataType<CodeDescriptionWithGroupCollection>).Validating += ValidateTaxTypeToTaxInvoiceDocumentTypeCodeMapping;

					return item;
				});
				item.Options = EnableEInvoicingFunctionality.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden;
				return item;
			}
		}

		void ValidateTaxTypeToTaxInvoiceDocumentTypeCodeMapping(object sender, RegistryDataTypeValidatingEventArgs<CodeDescriptionWithGroupCollection> e)
		{
			if (e.ProposedValue.GetGroupFromCode(AccountingMasterFilesConstants.NullTaxRateType.Code) == KoreaEInvoicingTypeCodeCategory.TaxInvoice)
			{
				throw new RegistryValidationException(Res.GetString("C18321F2-73B5-4B47-8139-0BF7F7F59F4D", "Tax Invoice Document/Type Code can not be '{0}' when Code is '{1}'.", KoreaEInvoicingTypeCodeCategory.TaxInvoice, AccountingMasterFilesConstants.NullTaxRateType.Code));
			}
		}

		string BuildTaxTypeToTaxInvoiceDocumentTypeCodeMapping(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var originalElements = ((CodeDescriptionWithGroupCollection)args.OriginalValue).Cast<CodeDescriptionWithGroup>();
			var newElements = ((CodeDescriptionWithGroupCollection)args.NewValue).Cast<CodeDescriptionWithGroup>();

			foreach (var originalElement in originalElements)
			{
				var updatedElement = newElements.FirstOrDefault(x => x.Code == originalElement.Code && x.Group != originalElement.Group);
				if (updatedElement != null)
				{
					result += Res.GetString("3045559A-6283-4374-A8B7-A4B3AE463197", "Tax Type '{0}', Tax Invoice Document/Type Code change from '{1}' to '{2}'.", originalElement.Code, originalElement.Group, updatedElement.Group) + "\r\n";
				}
			}

			return result;
		}

		CodeDescriptionWithGroupCollection GetTaxRateTypeMappingListDefaultValue()
		{
			var groupLookup = new CodeDescriptionPairList()
			{
				new CodeDescriptionPair(KoreaEInvoicingTypeCodeCategory.TaxInvoice, ResString.GetMultilingualString("E25829CB-01BB-4489-813C-1D14CD0AAE4A", "Tax Invoice / Amended Tax Invoice")),
				new CodeDescriptionPair(KoreaEInvoicingTypeCodeCategory.Invoice, ResString.GetMultilingualString("77D06C9D-A286-41D1-8604-27FF9D1C8593", "Invoice / Amended Invoice")),
				new CodeDescriptionPair(KoreaEInvoicingTypeCodeCategory.NotApplicable, ResString.GetMultilingualString("F5735B3C-92A4-4739-AE1C-D54852B13495", "Not applicable")),
			};

			var defaultValue = new CodeDescriptionWithGroupCollection(groupLookup, KoreaEInvoicingTypeCodeCategory.NotApplicable, 6);
			defaultValue.Add(NullTaxRateType.Code, NullTaxRateType.MultilingualDescription, KoreaEInvoicingTypeCodeCategory.NotApplicable);

			foreach (CodeDescriptionPair pair in AccountingMasterFilesConstants.TaxRateTypes)
			{
				if (pair.Code == AccTaxRate.Types.Rated || pair.Code == AccTaxRate.Types.CapitalRated)
				{
					defaultValue.Add(pair.Code, pair.MultilingualDescription, KoreaEInvoicingTypeCodeCategory.TaxInvoice);
				}
				else if (pair.Code == AccTaxRate.Types.Exempt)
				{
					defaultValue.Add(pair.Code, pair.MultilingualDescription, KoreaEInvoicingTypeCodeCategory.Invoice);
				}
				else
				{
					defaultValue.Add(pair.Code, pair.MultilingualDescription);
				}
			}

			return defaultValue;
		}

		#endregion

		#endregion

		public GuidRegistryItem GLAccountSelectionAndEntry
		{
			get
			{
				var item = GetItem("GLAccountSelectionAndEntry", () => new GuidRegistryItem(
					"GLAccountSelectionAndEntry",
					Categories.Accounting_ReportingBooks,
					(NoResString)"GL Account Selection and Entry",
					(NoResString)@"By default, this registry value is empty and the system will locate 'GL Account' based on the current logic:

The system will locate matching GL Mapping record based on the following values:
a. Local Account Number entered by the user
b. Current Login Company's Country 
c. Current Login User's Language 

If a match is found, the system will save the corresponding GL Account into the data entry screen.
If a match is not found, the system will continue to search for a matching GL Account.
And should no match is found, a validation error is shown.

If a registry value (Alternate Chart) is specified, the system will locate 'GL Account' based on the new logic:

The system will locate matching Alternate Account(s) record based on the Alternate Account Number entered by the user against the Alternate Chart specified in this registry.

If a match is found, the system will save the corresponding GL Account into the data entry screen.
If a match is not found, the system will continue to search for a matching GL Account.
And should no match is found, a validation error is shown.",
					new GLAccountSelectionAndEntryGuidRegistryDataType(),
					RegistryStorageFlags.Company,
					Guid.Empty)
				{
					EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccAlternateChart)
				}
				);

				item.Options = EnableReportingBooksFeature.Value ? RegistryOptions.IsValueOptional : RegistryOptions.IsHidden;
				item.OnBuildLogReference += (args) =>
				{
					var factory = new BusinessObjectFactory();
					var originalChart = factory.Load<AccAlternateChart>(new ZGuid(args.OriginalValue));
					var newChart = factory.Load<AccAlternateChart>(new ZGuid(args.NewValue));
					return Res.GetString("675D5A50-A55A-4200-B3D3-FD4A83AA2686", "Alternate Chart changed from [{0}] to [{1}].", originalChart?.AAC_Code ?? ZString.Empty, newChart?.AAC_Code ?? ZString.Empty);
				};

				return item;
			}
		}

		#region Reporting Book Accounting Journal Print Option

		public ReportingBookAccountingJournalPrintOptionRegistryItem ReportingBookAccountingJournalPrintOption
		{
			get
			{
				var item = GetItem("ReportingBookAccountingJournalPrintOption", delegate
				{
					var item = new ReportingBookAccountingJournalPrintOptionRegistryItem(
						"ReportingBookAccountingJournalPrintOption",
						Categories.Accounting_ReportingBooks,
						(NoResString)"Reporting Book Accounting Journal Print Option",
						(NoResString)@"The printing of Reporting Book Accounting Journal is not enabled by default. 

To enable this function, you have to add at least one Reporting Book record and mark it as 'Default'.
Further, for each Reporting Book, you can opt to include the printing of 'Parent Account' and 'Attribute Values', if required as these values are not printed by default.",
						RegistryStorageFlags.Company);
					item.OnBuildLogReference += OnBuildReportingBookAccountingJournalPrintOptionLogReference;

					return item;
				});
				item.Options = EnableReportingBooksFeature.Value ? RegistryOptions.IsValueOptional : RegistryOptions.IsHidden;
				return item;
			}
		}

		string OnBuildReportingBookAccountingJournalPrintOptionLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var originalElements = ((ReportingBookAccountingJournalPrintOptionCollection)args.OriginalValue).Cast<ReportingBookAccountingJournalPrintOption>();
			var newElements = ((ReportingBookAccountingJournalPrintOptionCollection)args.NewValue).Cast<ReportingBookAccountingJournalPrintOption>();

			var comparer = new FuncComparer<ReportingBookAccountingJournalPrintOption>((a, b) => a.ReportingBook.CompareTo(b.ReportingBook));
			var addedItems = newElements.Except(originalElements, comparer);
			var deletedItems = originalElements.Except(newElements, comparer);
			var editedReportingBookPKs = newElements.Select(x => x.ReportingBook).Intersect(originalElements.Select(x => x.ReportingBook));

			var result = new StringBuilder();

			foreach (var editedReportingBookPK in editedReportingBookPKs)
			{
				var originalElement = originalElements.First(x => x.ReportingBook == editedReportingBookPK);
				var newElement = newElements.First(x => x.ReportingBook == editedReportingBookPK);

				if (newElement.DisplayAttribute != originalElement.DisplayAttribute
					|| newElement.DisplayParentAccount != originalElement.DisplayParentAccount
					|| newElement.Default != originalElement.Default)
				{
					result.AppendLine(Res.GetString("BEC7E662-0838-4FFB-954B-503ED90441D9", "Record edited: Reporting Book = {0}, Display Parent Account = {1}, Display Attribute = {2}, Default = {3}", newElement.ReportingBookCode, GetBoolValue(newElement.DisplayParentAccount), GetBoolValue(newElement.DisplayAttribute), GetBoolValue(newElement.Default)));
				}
			}

			foreach (var element in addedItems)
			{
				result.AppendLine(Res.GetString("2531D134-C7DD-4D8B-9E07-C405B71BA8A9", "Record added: Reporting Book = {0}, Display Parent Account = {1}, Display Attribute = {2}, Default = {3}", element.ReportingBookCode, GetBoolValue(element.DisplayParentAccount), GetBoolValue(element.DisplayAttribute), GetBoolValue(element.Default)));
			}

			foreach (var element in deletedItems)
			{
				result.AppendLine(Res.GetString("4E2F71DA-AB28-40A9-9F67-DCA83DD8A37E", "Record deleted: Reporting Book = {0}, Display Parent Account = {1}, Display Attribute = {2}, Default = {3}", element.ReportingBookCode, GetBoolValue(element.DisplayParentAccount), GetBoolValue(element.DisplayAttribute), GetBoolValue(element.Default)));
			}

			return result.ToString();

			string GetBoolValue(bool value) => value ? "Y" : "N";
		}

		#endregion

		#region SAFT

		public StringRegistryItem SAFTGroupingCategory
		{
			get
			{
				var isSAFTv130FeatureEnabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report) != null;

				return GetItem("SAFTGroupingCategory", () =>
				new StringRegistryItem(
					new CountrySpecificDefaultValueRegistryItemImpl<string>(
						"SAFTGroupingCategory",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("B67F3904-0A40-4A95-8F69-98B9C27950DF", @"The SAF-T grouping category code is exported in the SAF-T file as a part of the account section that contains the general ledger accounts.
It is used by the tax authorities to interpret and categorize the chart of accounts used in the SAF-T file and is therefore mandatory to include in the SAF-T report."),
						RegistryDataTypes.StringType,
						RegistryOptions.CacheExpensiveDefaultValue | (isSAFTv130FeatureEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden),
						FactoryForCountryDefaultValues,
						new SAFTGroupingCategory_RegistryDescriptor())));
			}
		}

		#endregion

		#region External Accounting System Integration

		public D365CredentialsRegistryItem D365Credentials
		{
			get
			{
				var isCWD365IntegrationEnabled = FeatureControlHelper.IsCWD365IntegrationFeatureEnabled;

				return GetItem(nameof(D365Credentials), () =>
					new D365CredentialsRegistryItem(nameof(D365Credentials),
						Categories.Accounting_ExternalAccountingSystemIntegration,
						ResString.GetMultilingualString("90589ab4-579b-4e31-9fab-20e6e13e02d1", "D365 Credentials"),
						ResString.GetMultilingualString("38fac037-4795-409e-bfec-2b5781e7e59a", @"This registry setting allows seamless authentication between CargoWise and D365 F&O. 
Essential authentication details are configured as part of this setting, which help establish secure access between CargoWise and D365. 
This registry setting is only visible when the feature switch for enabling CW D365 Integration is turned on in the 'Feature Control' module, ensuring that authentication settings are managed efficiently and only when required."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, 
						 isCWD365IntegrationEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden));
			}
		}

		public StringRegistryItem D365WebserviceURL
		{
			get
			{
				var isCWD365IntegrationEnabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature) != null;
				return GetItem("D365WebserviceURL", delegate
				{
					return new StringRegistryItem("D365WebserviceURL",
						Categories.Accounting_ExternalAccountingSystemIntegration,
						ResString.GetMultilingualString("49e598c3-8f88-4230-a202-5af79984435b", "D365 Web Service URL"),
						ResString.GetMultilingualString("a406efc3-fef9-4c0e-a644-c4fcc288509c", "This registry is to define the web service URL endpoint I should use for my D365 FO Integration."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						isCWD365IntegrationEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden);
				});
			}
		}

		#endregion
	}
}
