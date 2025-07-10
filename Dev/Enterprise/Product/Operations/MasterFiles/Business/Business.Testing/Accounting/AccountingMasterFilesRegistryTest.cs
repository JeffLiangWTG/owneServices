using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.MasterFiles.Business.Customs.XmlCredential.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesRegistry;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;
using RegistryOptions = Enterprise.Integration.RegistryOptions;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccountingMasterFilesRegistry))]
	public sealed class AccountingMasterFilesRegistryTest : RegistryItemSetTestCaseWithFactory<AccountingMasterFilesRegistry>
	{
		#region Reflection Based Tests

		public void TestCountrySpecificDefaultValueRegistryItemImpl_RegistriesMustAlsoSetCacheExpensiveDefaultValueOption()
			=> CountrySpecificDefaultValueRegistryItemImpl_RegistriesMustAlsoSetCacheExpensiveDefaultValueOption(AllItems);

		public static void CountrySpecificDefaultValueRegistryItemImpl_RegistriesMustAlsoSetCacheExpensiveDefaultValueOption(IReadOnlyCollection<IRegistryItem> allItems)
		{
			var countrySpecificType = typeof(CountrySpecificDefaultValueRegistryItemImpl<>);
			var incorrectRegistries = allItems
					.Select(item => new { item, innerType = (item as RegistryItemWrapper)?.Inner?.GetType() })
					.Where(x => x.innerType?.IsGenericType == true)
					.Select(x => new { x.item, innerGenericType = x.innerType.GetGenericTypeDefinition() })
					.Where(x => x.innerGenericType == countrySpecificType && !x.item.HasOption(RegistryOptions.CacheExpensiveDefaultValue))
					.Select(x => x.item)
					.ToList();
			if (incorrectRegistries.Any())
			{
				Fail($@"Any registry item which uses CountrySpecificDefaultValueRegistryItemImpl<T> must also set RegistryOption CacheExpensiveDefaultValue.
CountrySpecificDefaultValueRegistryItemImpl<T> does no caching of lookups from CompanyPK to CountryCode; it relies on CacheExpensiveDefaultValue.

Registries missing CacheExpensiveDefaultValue: {string.Join(", ", incorrectRegistries.Select(item => item.Name))}");
			}

			Assert("Passed", true);
		}

		#endregion

		#region Control Account Tests

		string GetGLHeaderAccountNum(Guid pk)
		{
			AccGLHeader obj = Factory.Load<AccGLHeader>(pk);
			return obj != null ? obj.AccountNum.ToString() : string.Empty;
		}

		// If test DB version is higher than 0, only some StmData (and Accounting) base data will get inserted and thus be correctly asserted.
		// (See $/Dev/Enterprise/Product/Core/DbUpgrader/Data/BaseData/StmData)
		bool HasDefaultValueFromBaseData(string name, string accountNum)
		{
			ZQuery query = new ZQuery(StmDataSchema.SD_Name, name);
			StmData obj = Factory.LoadTop1<StmData>(query);
			return obj != null && GetGLHeaderAccountNum(obj.SD_GuidValue.ToGuid()) == accountNum;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Not a file path")]
		public void TestPreventDeletionOfJobBillingExchangeRateAndCFXConfigurations()
		{
			AssertEquals("Name", "PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations", ItemSet.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Name);
			AssertEquals("Category", "Accounting/Temp.", ItemSet.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Category);
			AssertEquals("Caption", "Prevent deletion of Job Billing Exchange Rate and CFX Configurations (CargoWiseOne Support Only)", ItemSet.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Caption);
			AssertEquals("Hint", @"Recently, we had a number of reports from different customers saying that their Job Billing Exchange Rate Configurations (and CFX Configurations) go missing unexpectedly.
When this registry is enabled, the system will prevent deleting Job Billing Exchange Rate Configurations and CFX Configurations (at all levels).
This registry should be enabled only if the customer is experiencing unexplained deletions of Job Billing Exchange Rate and/or CFX Configurations.
The user will be able to delete the configurations only when the user is working on the actual configuration form, i.e. Job Billing Exchange Rate Configuration, Company maintenance, branch maintenance, debtor group, creditor group or organization maintenance.
However, the system will no longer be able to delete these configurations outside these forms. The customer should be informed that some functions may be impacted, such as organization import, merge or de-duplication features.", ItemSet.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Options);
			AssertEquals("Default value", false, ItemSet.PreventDeletionOfJobBillingExchangeRateAndCFXConfigurations.Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Not a file path")]
		public void TestReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations()
		{
			AssertEquals("Name", "ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations", ItemSet.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Name);
			AssertEquals("Category", "Accounting/Temp.", ItemSet.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Category);
			AssertEquals("Caption", "Report Error When Deleting Job Billing Exchange Rate and CFX Configurations (CargoWiseOne Support Only)", ItemSet.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Caption);
			AssertEquals("Hint", @"Recently, we had a number of reports from different customers saying that their Job Billing Exchange Rate Configurations (and CFX Configurations) go missing unexpectedly.
When this registry is enabled, the system will generate an error report to us when Job Billing Exchange Rate Configurations and CFX Configurations (at all levels) are deleted.
This registry should be enabled only if the customer is experiencing unexplained deletions of Job Billing Exchange Rate and/or CFX Configurations.", ItemSet.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Options);
			AssertEquals("Default value", false, ItemSet.ReportErrorWhenDeletingJobBillingExchangeRateAndCFXConfigurations.Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Not a file path")]
		public void TestAllowTriggersToSkipSaveEverythingBeforePostingValidation()
		{
			AssertEquals("Caption", "Allow Triggers to Skip 'Save Everything Before Posting' Validation (CargoWise Support Only)", ItemSet.AllowTriggersToSkipSaveEverythingBeforePostingValidation.Caption);
			AssertEquals("Category", "Accounting/Temp.", ItemSet.AllowTriggersToSkipSaveEverythingBeforePostingValidation.Category);
			AssertEquals("DefaultValue", false, ItemSet.AllowTriggersToSkipSaveEverythingBeforePostingValidation.DefaultValue);
			AssertEquals("Hint", "When this registry ticked, triggers are allowed to skip 'save everything before posting' validation.", ItemSet.AllowTriggersToSkipSaveEverythingBeforePostingValidation.Hint);
			AssertEquals("Name", "AllowTriggersToSkipSaveEverythingBeforePostingValidation", ItemSet.AllowTriggersToSkipSaveEverythingBeforePostingValidation.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AllowTriggersToSkipSaveEverythingBeforePostingValidation.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.AllowTriggersToSkipSaveEverythingBeforePostingValidation.Storage);
		}

		public void TestTemporaryProductionRuleEngineImportExportFeature()
		{
			AssertEquals("Caption", "Temporary Production Rule Engine Import Export Feature", ItemSet.TemporaryProductionRuleEngineImportExportFeature.Caption);
			AssertEquals("Category", "Accounting/Temp.", ItemSet.TemporaryProductionRuleEngineImportExportFeature.Category);
			AssertEquals("DefaultValue", false, ItemSet.TemporaryProductionRuleEngineImportExportFeature.DefaultValue);
			AssertEquals("Hint", @"This registry will enable the working-in-progress Production Rule Engine Import Export Feature.", ItemSet.TemporaryProductionRuleEngineImportExportFeature.Hint);
			AssertEquals("Name", "TemporaryProductionRuleEngineImportExportFeature", ItemSet.TemporaryProductionRuleEngineImportExportFeature.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.TemporaryProductionRuleEngineImportExportFeature.Options);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TemporaryProductionRuleEngineImportExportFeature.Storage);
		}

		public void TestTemporaryProductionRuleEngineCustomPropertyFeature()
		{
			AssertEquals("Caption", "Temporary Production Rule Engine Custom Field Feature", ItemSet.TemporaryProductionRuleEngineCustomFieldFeature.Caption);
			AssertEquals("Category", "Accounting/Temp.", ItemSet.TemporaryProductionRuleEngineCustomFieldFeature.Category);
			AssertEquals("DefaultValue", false, ItemSet.TemporaryProductionRuleEngineCustomFieldFeature.DefaultValue);
			AssertEquals("Hint", @"This registry will enable the working-in-progress Production Rule Engine Custom Field Feature.", ItemSet.TemporaryProductionRuleEngineCustomFieldFeature.Hint);
			AssertEquals("Name", "TemporaryProductionRuleEngineCustomFieldFeature", ItemSet.TemporaryProductionRuleEngineCustomFieldFeature.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.TemporaryProductionRuleEngineCustomFieldFeature.Options);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TemporaryProductionRuleEngineCustomFieldFeature.Storage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Not a file path")]
		public void TestIncludeRelatedJournalsInUniversalXMLTransaction()
		{
			AssertEquals("Caption", "Include Related Journals In Universal XML Transaction (CargoWise Support Only)", ItemSet.IncludeRelatedJournalsInUniversalXMLTransaction.Caption);
			AssertEquals("Category", "Accounting/Temp.", ItemSet.IncludeRelatedJournalsInUniversalXMLTransaction.Category);
			AssertEquals("DefaultValue", false, ItemSet.IncludeRelatedJournalsInUniversalXMLTransaction.DefaultValue);
			AssertEquals("Hint", @"This registry configures the inclusion of Apportionment and Installment Related Journals in Universal Transaction XML.
When overridden and set to yes Then
Apportionment Related Journals linked to the transactions exported via XUT are included in  < PostingJournalApportionamet > section
and
Installment related Journals linked to the transactions exported via XUT are included in < PostingJournalInstallment > section", ItemSet.IncludeRelatedJournalsInUniversalXMLTransaction.Hint);
			AssertEquals("Name", "IncludeRelatedJournalsInUniversalXMLTransaction", ItemSet.IncludeRelatedJournalsInUniversalXMLTransaction.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.IncludeRelatedJournalsInUniversalXMLTransaction.Options);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.IncludeRelatedJournalsInUniversalXMLTransaction.Storage);
		}

		public void TestEnableCFXUpliftStartAndExpiryDateColumns()
		{
			AssertEquals("Caption", "Enable CFX Uplift Start and Expiry Date Columns", ItemSet.EnableCFXUpliftStartAndExpiryDateColumns.Caption);
			AssertEquals("Category", "Accounting/Temp.", ItemSet.EnableCFXUpliftStartAndExpiryDateColumns.Category);
			AssertEquals("DefaultValue", false, ItemSet.EnableCFXUpliftStartAndExpiryDateColumns.DefaultValue);
			AssertEquals("Hint", @"This registry enables the new Start and Expiry Date Columns in all levels of CFX Uplift Configuration.", ItemSet.EnableCFXUpliftStartAndExpiryDateColumns.Hint);
			AssertEquals("Name", "EnableCFXUpliftStartAndExpiryDateColumns", ItemSet.EnableCFXUpliftStartAndExpiryDateColumns.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableCFXUpliftStartAndExpiryDateColumns.Options);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableCFXUpliftStartAndExpiryDateColumns.Storage);
		}

		public void TestTaxTransactionPrepaidAssetControlAccount()
		{
			AssertNotNull("TaxTransactionPrepaidAssetControlAccount", ItemSet.TaxTransactionPrepaidAssetControlAccount.Value);
			string defaultValue = "8210.00.00";

			if (HasDefaultValueFromBaseData(ItemSet.TaxTransactionPrepaidAssetControlAccount.Name, defaultValue))
			{
				AssertEquals("TaxTransactionPrepaidAssetControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.TaxTransactionPrepaidAssetControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.TaxTransactionPrepaidAssetControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("TaxTransactionPrepaidAssetControlAccount.Value", newGuid, ItemSet.TaxTransactionPrepaidAssetControlAccount.Value);
		}

		public void TestTaxTransactionPrepaidAssetControlAccountValues()
		{
			AssertEquals("Name", "GL_OTHER_TAXES_PREPAID_ASSET_CONTROL_ACCOUNT", ItemSet.TaxTransactionPrepaidAssetControlAccount.Name);
			AssertEquals("Caption", "Tax Transaction Realization Prepaid Asset Account", ItemSet.TaxTransactionPrepaidAssetControlAccount.Caption);
			AssertEquals("Hint", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

This registry defines a default 'Tax Realization' General Ledger account for Tax Configurations that will realize Tax Transactions as Prepaid Tax Assets.
This registry sets the 'Tax Realization' account of a New Tax Configuration when creating Payables Perceptions (PER), Payables Value Added Tax (VAT); Receivables Invoice Retention (RII) and Receivables Standard Payments Basis Withholding (SPR) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups.
Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", ItemSet.TaxTransactionPrepaidAssetControlAccount.Hint);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account/Tax Transaction", ItemSet.TaxTransactionPrepaidAssetControlAccount.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TaxTransactionPrepaidAssetControlAccount.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue, ItemSet.TaxTransactionPrepaidAssetControlAccount.Options);
			AssertEquals(RegistryFindBoxFilter.PandLOrBSHandNonControl, (ItemSet.TaxTransactionPrepaidAssetControlAccount.EditorInfo as GuidFindBoxRegistryEditorInfo).FindBoxFilter);
		}

		public void TestTaxTransactionRemittanceLiabilityControlAccount()
		{
			AssertNotNull("TaxTransactionRemittanceLiabilityControlAccount", ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Value);
			string defaultValue = "8210.00.00";

			if (HasDefaultValueFromBaseData(ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Name, defaultValue))
			{
				AssertEquals("TaxTransactionRemittanceLiabilityControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.TaxTransactionRemittanceLiabilityControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("TaxTransactionRemittanceLiabilityControlAccount.Value", newGuid, ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Value);
		}

		public void TestTaxTransactionRemittanceLiabilityControlAccountValues()
		{
			AssertEquals("Name", "GL_OTHER_TAXES_REMITTANCE_LIABILITY_CONTROL_ACCOUNT", ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Name);
			AssertEquals("Caption", "Tax Transaction Realization Liability Account", ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Caption);
			AssertEquals("Hint", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

This registry defines a default 'Tax Realization' GL Account for Tax Configurations that will realize Tax Transactions as Tax Remittance Liabilities payable to a Tax Authority.
This registry sets the 'Tax Realization' account of a New Tax Configuration when creating Receivables Perceptions (PER), Receivables Value Added Tax (VAT), Receivables Turnover Tax (TRX), Receivables Sales Tax (SLX); Payables Invoice Retention (RII) and Payables Standard Payments Basis Withholding (SPR) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups.
Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Hint);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account/Tax Transaction", ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue, ItemSet.TaxTransactionRemittanceLiabilityControlAccount.Options);
			AssertEquals(RegistryFindBoxFilter.PandLOrBSHandNonControl, (ItemSet.TaxTransactionRemittanceLiabilityControlAccount.EditorInfo as GuidFindBoxRegistryEditorInfo).FindBoxFilter);
		}

		public void TestPendingTaxTransactionPrepaidAssetControlAccount()
		{
			AssertNotNull("PendingTaxTransactionPrepaidAssetControlAccount", ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Value);
			string defaultValue = "8210.00.00";

			if (HasDefaultValueFromBaseData(ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Name, defaultValue))
			{
				AssertEquals("PendingTaxTransactionPrepaidAssetControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("PendingTaxTransactionPrepaidAssetControlAccount.Value", newGuid, ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Value);
		}

		public void TestPendingTaxTransactionPrepaidAssetControlAccountValues()
		{
			AssertEquals("Name", "GL_PENDING_OTHER_TAXES_PREPAID_ASSET_CONTROL_ACCOUNT", ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Name);
			AssertEquals("Caption", "Pending Realization Tax Transaction Prepaid Asset Account", ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Caption);
			AssertEquals("Hint", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Pending Realization Tax Transaction Prepaid Asset Account' registry defines a default 'Pending Tax Realization' GL account for 'Prepaid Tax Asset' Tax Configurations with 'Match Date' realization behavior.
This registry sets the 'Pending Tax Realization' account of a New Tax Configuration when creating Payables Perceptions (PER), Payables Value Added Tax (VAT) and Receivables Invoice Retention (RII) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups. Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Hint);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account/Tax Transaction", ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue, ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.Options);
			AssertEquals(RegistryFindBoxFilter.PandLOrBSHandNonControl, (ItemSet.PendingTaxTransactionPrepaidAssetControlAccount.EditorInfo as GuidFindBoxRegistryEditorInfo).FindBoxFilter);
		}

		public void TestPendingTaxTransactionRemittanceLiabilityControlAccount()
		{
			AssertNotNull("PendingTaxTransactionRemittanceLiabilityControlAccount", ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Value);
			string defaultValue = "8210.00.00";

			if (HasDefaultValueFromBaseData(ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Name, defaultValue))
			{
				AssertEquals("PendingTaxTransactionRemittanceLiabilityControlAccount.ChargeCode", defaultValue, GetGLHeaderAccountNum(ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Value));
			}

			Guid newGuid = Guid.NewGuid();
			ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("PendingTaxTransactionRemittanceLiabilityControlAccount.Value", newGuid, ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Value);
		}

		public void TestPendingTaxTransactionRemittanceLiabilityControlAccountValues()
		{
			AssertEquals("Name", "GL_PENDING_OTHER_TAXES_REMITTANCE_LIABILITY_CONTROL_ACCOUNT", ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Name);
			AssertEquals("Caption", "Pending Realization Tax Transaction Remittance Liability Account", ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Caption);
			AssertEquals("Hint", @"This registry is used when adding New Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Pending Realization Tax Transaction Remittance Liability Account' registry defines a default 'Pending Tax Realization' GL account for 'Tax Remittance Liability' Tax Configurations with 'Match Date' realization behavior.
This registry sets the 'Pending Tax Realization' account of a New Tax Configuration when creating Receivables Perceptions (PER), Value Added Tax (VAT), Turnover Tax (TRX) and Sales Tax (SLX); and Payables Invoice Retention (RII) Super Type Tax Configurations.
Changes to this registry do not change existing Tax Configuration setups. Changes to the GL account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue, ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.Options);
			AssertEquals(RegistryFindBoxFilter.PandLOrBSHandNonControl, (ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount.EditorInfo as GuidFindBoxRegistryEditorInfo).FindBoxFilter);
		}

		public void TestTaxTransactionExpenseAccount()
		{
			AssertEquals("Name", "OtherTaxesExpenseAccount", ItemSet.TaxTransactionExpenseAccount.Name);
			AssertEquals("Caption", "Tax Transaction Expense Account", ItemSet.TaxTransactionExpenseAccount.Caption);
			AssertEquals("Hint", @"This registry is used when adding new Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Tax Transaction Expense Account' registry defines a default 'Tax Expense' GL Account for Payables Sales Tax (SLX) Super Type Tax Configurations. The 'Tax Transaction Expense Account' is used to expense Sales Tax related to the posting of Costs.
Changes to this registry do not change existing Tax Configurations. Changes to the GL Account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", ItemSet.TaxTransactionExpenseAccount.Hint);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account/Tax Transaction", ItemSet.TaxTransactionExpenseAccount.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TaxTransactionExpenseAccount.Storage);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.PandLOrBSHandNonControl, ((GuidFindBoxRegistryEditorInfo)ItemSet.TaxTransactionExpenseAccount.EditorInfo).FindBoxFilter);

			AccGLHeader gLHeader = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8210.00.00");
			AssertNotNull(gLHeader);

			AssertEquals("Default value", Guid.Empty, ItemSet.TaxTransactionExpenseAccount.GetFallBackValueAtAllLevels(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

			ItemSet.TaxTransactionExpenseAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid());
			AssertEquals("Set value", gLHeader.PK.ToGuid(), ItemSet.TaxTransactionExpenseAccount.GetFallBackValueAtAllLevels(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestTaxTransactionNegativeRevenueAccount()
		{
			AssertEquals("Name", "OtherTaxesNegativeRevenueAccount", ItemSet.TaxTransactionNegativeRevenueAccount.Name);
			AssertEquals("Caption", "Tax Transaction Negative Revenue Account", ItemSet.TaxTransactionNegativeRevenueAccount.Caption);
			AssertEquals("Hint", @"This registry is used when adding new Tax Configurations to a Login Company.
The General Ledger Accounts recorded against Company and Branch Tax Configurations are used when Receivables and Payables Tax Transaction records are first created and posted.

The 'Tax Transaction Negative Revenue Account' registry defines a default 'Tax Expense' GL Account for Receivables Turnover Tax (TRX) Super Type Tax Configurations. 
The 'Tax Transaction Negative Revenue Account' is used is used by Receivables Turnover Tax Configurations when recording Turnover Tax records related to the posting of Revenue.
Changes to this registry do not change existing Tax Configurations. Changes to the GL Account setup of existing Company and Branch Tax Configurations are made by editing the relevant Company or Branch Tax Configuration itself.", ItemSet.TaxTransactionNegativeRevenueAccount.Hint);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Link Account/Tax Transaction", ItemSet.TaxTransactionNegativeRevenueAccount.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TaxTransactionNegativeRevenueAccount.Storage);
			AssertEquals("FindBoxFilter", RegistryFindBoxFilter.PandLOrBSHandNonControl, ((GuidFindBoxRegistryEditorInfo)ItemSet.TaxTransactionNegativeRevenueAccount.EditorInfo).FindBoxFilter);

			AccGLHeader gLHeader = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8210.00.00");
			AssertNotNull(gLHeader);

			AssertEquals("Default value", Guid.Empty, ItemSet.TaxTransactionNegativeRevenueAccount.GetFallBackValueAtAllLevels(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

			ItemSet.TaxTransactionNegativeRevenueAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid());
			AssertEquals("Set value", gLHeader.PK.ToGuid(), ItemSet.TaxTransactionNegativeRevenueAccount.GetFallBackValueAtAllLevels(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		#region GL Journal Exchange Rate Difference Account

		public void TestGLJournalExchangeRateDifferenceAccountDisplay()
		{
			TestGenericRegistryItem(ItemSet.GLJournalExchangeRateDifferenceAccount,
				"GLJournalExchangeRateDifferenceAccount",
				Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
				"GL Journal Exchange Rate Difference Account",
				"GL Journal Exchange Rate Difference Account",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueOptional);

			AssertType<GuidFindBoxRegistryEditorInfo>("Editor Info", ItemSet.GLJournalExchangeRateDifferenceAccount.EditorInfo);
			var editorInfo = (GuidFindBoxRegistryEditorInfo)ItemSet.GLJournalExchangeRateDifferenceAccount.EditorInfo;
			AssertEquals("Find Box Collection", RegistryFindBoxCollection.AccGLHeader, editorInfo.FindBoxCollection);
			AssertEquals("Find Box Collection Filter", RegistryFindBoxFilter.PandLOrBSHandNonControl, editorInfo.FindBoxFilter);

			var oldGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var newGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			oldGLHeader.AG_AccountNum = "Test.aa";
			newGLHeader.AG_AccountNum = "Test.bb";
			Factory.Save();

			var regItem = Instance.GLJournalExchangeRateDifferenceAccount.Inner;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, oldGLHeader.PK, newGLHeader.PK);
			var logReference = Instance.GLJournalExchangeRateDifferenceAccount.OnBuildLogReference(args);
			AssertEquals($"GL Account changed from [Test.aa] to [Test.bb].", logReference);
		}

		#endregion

		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			Assert(true);
		}

		#region Supply Type Classification Codes

		public void TestEnableSupplyTypeClassificationCodes()
		{
			AssertEquals("Caption", "Enable Supply Type Classification Codes", ItemSet.EnableSupplyTypeClassificationCodes.Caption);
			AssertEquals("Category", Categories.Accounting_TaxConfigurations_SupplyType, ItemSet.EnableSupplyTypeClassificationCodes.Category);
			AssertEquals("DefaultValue", false, ItemSet.EnableSupplyTypeClassificationCodes.DefaultValue);
			AssertEquals("Hint", @"Supply Type Classification Codes are additional, secondary item of information used to classify each revenue or cost charge line.

By default, this registry is set to 'No' and this feature is not enabled.
When this registry is set to 'Yes', users will be able to assign a supply type to revenue and cost charge line prior to posting.", ItemSet.EnableSupplyTypeClassificationCodes.Hint);
			AssertEquals("Name", "EnableSupplyTypeClassificationCodes", ItemSet.EnableSupplyTypeClassificationCodes.Name);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableSupplyTypeClassificationCodes.Storage);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EnableSupplyTypeClassificationCodes, oldValue, newValue);
			var logReference = ItemSet.EnableSupplyTypeClassificationCodes.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEnableSellComplianceDescription()
		{
			AssertEquals("Caption", "Enable Sell Compliance Description", ItemSet.EnableSellComplianceDescription.Caption);
			AssertEquals("Category", Categories.Accounting_TaxConfigurations_SupplyType, ItemSet.EnableSellComplianceDescription.Category);
			AssertEquals("DefaultValue", false, ItemSet.EnableSellComplianceDescription.DefaultValue);
			AssertEquals("Hint", @"Sell Compliance Description is an additional piece of information used to add meaningful descriptions to AR Invoice charge lines.
By default, this registry is set to 'No' and this feature is not enabled.

When overridden and set to 'Yes',
Sell Compliance Descriptions can be defined against each Charge Code,
and when posting Job Related Sell charges, the relevant Sell Compliance Description will be included as part of the Posted Charge Line's Description.

These features are relevant in companies where Receivables Invoice charge line descriptions must include information about the supply type of the posted charge.", ItemSet.EnableSellComplianceDescription.Hint);
			AssertEquals("Name", "EnableSellComplianceDescription", ItemSet.EnableSellComplianceDescription.Name);
			AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.EnableSellComplianceDescription.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableSellComplianceDescription.Storage);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EnableSellComplianceDescription, oldValue, newValue);
			var logReference = ItemSet.EnableSellComplianceDescription.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEnableSellComplianceDescriptionVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.EnableSellComplianceDescription.Options);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.Default, ItemSet.EnableSellComplianceDescription.Options);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.EnableSellComplianceDescription.Options);
		}

		public void TestSupplyTypeClassificationCodesIsMandatory()
		{
			AssertEquals("Caption", "Supply Type Classification Code is Mandatory", ItemSet.SupplyTypeClassificationCodesIsMandatory.Caption);
			AssertEquals("Category", Categories.Accounting_TaxConfigurations_SupplyType, ItemSet.SupplyTypeClassificationCodesIsMandatory.Category);
			AssertEquals("DefaultValue", false, ItemSet.SupplyTypeClassificationCodesIsMandatory.DefaultValue);
			AssertEquals("Hint", @"This registry allows you to enforce that a supply type must be assigned to each revenue and cost charge line.

By default, this registry is set to ‘No’ and the recording of supply type is optional.
When this registry is set to ‘Yes’, a validation error will be shown if supply type is not recorded when
1. posting receivables and payables transactions in operational modules.
2. creating and posting of receivables, payables and cash book transactions in the respective accounting modules.", ItemSet.SupplyTypeClassificationCodesIsMandatory.Hint);
			AssertEquals("Name", "SupplyTypeClassificationCodesIsMandatory", ItemSet.SupplyTypeClassificationCodesIsMandatory.Name);
			AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.SupplyTypeClassificationCodesIsMandatory.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.SupplyTypeClassificationCodesIsMandatory.Storage);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.SupplyTypeClassificationCodesIsMandatory, oldValue, newValue);
			var logReference = ItemSet.SupplyTypeClassificationCodesIsMandatory.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestSupplyTypeClassificationCodesIsMandatoryVisible()
		{
			ItemSet.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.SupplyTypeClassificationCodesIsMandatory.Options);
		}

		public void TestSupplyTypeClassificationCodesIsMandatoryVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.SupplyTypeClassificationCodesIsMandatory.Options);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.Default, ItemSet.SupplyTypeClassificationCodesIsMandatory.Options);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.SupplyTypeClassificationCodesIsMandatory.Options);
		}

		public void TestSupplyTypeClassificationCodesList()
		{
			AssertEquals("Caption", "Supply Type Classification Codes List", ItemSet.SupplyTypeClassificationCodesList.Caption);
			AssertEquals("Category", Categories.Accounting_TaxConfigurations_SupplyType, ItemSet.SupplyTypeClassificationCodesList.Category);
			AssertEquals("Hint", @"This list the valid supply type classification codes that can be used to assign to revenue and cost charge line.

You will not be allowed to add new code or delete existing code.
You will be allowed to mark code as inactive to disallow it from being used.", ItemSet.SupplyTypeClassificationCodesList.Hint);
			AssertEquals("Name", "SupplyTypeClassificationCodesList", ItemSet.SupplyTypeClassificationCodesList.Name);
			AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.SupplyTypeClassificationCodesList.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.SupplyTypeClassificationCodesList.Storage);

			var defaultValue = ItemSet.SupplyTypeClassificationCodesList.DefaultValue;
			var castedDefaultValue = ItemSet.SupplyTypeClassificationCodesList.DefaultValue.Cast<CodeDescriptionBool>();
			AssertEquals("Default value count", 7, defaultValue.Count);
			foreach (CodeDescriptionPair supplyType in SupplyTypeClassificationList)
			{
				Assert("All codes default as active", castedDefaultValue.Any(x => x.Code == supplyType.Code && x.Bool));
			}

			var oldValue = defaultValue;
			var newValue = new CodeDescriptionBoolDisallowNewCollection(SupplyTypeClassificationList);
			newValue.Cast<CodeDescriptionBool>().Where(x => x.Code != SupplyTypeClassificationCodes.LOA).ForEach(x => x.Bool = true);
			var expectedLogMessage = "LOA code has been marked as inactive.\r\n";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.SupplyTypeClassificationCodesList, oldValue, newValue);
			var logReference = ItemSet.SupplyTypeClassificationCodesList.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);

			expectedLogMessage = "LOA code has been marked as active.\r\n";
			args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.SupplyTypeClassificationCodesList, newValue, oldValue);
			logReference = ItemSet.SupplyTypeClassificationCodesList.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestSupplyTypeClassificationCodesListVisible()
		{
			ItemSet.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.SupplyTypeClassificationCodesList.Options);
		}

		public void TestSupplyTypeClassificationCodesListVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.SupplyTypeClassificationCodesList.Options);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.Default, ItemSet.SupplyTypeClassificationCodesList.Options);
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.SupplyTypeClassificationCodesList.Options);
		}

		#endregion

		#region Tax Framework Configuration

		public void TestTaxAuthorities()
		{
			TestGenericRegistryItem(ItemSet.TaxAuthorities,
				"TaxAuthorities",
				Categories.Accounting_TaxFrameworkConfiguration,
				"Tax Authorities (CargoWiseOne Support Only)",
				@"DO NOT OVERRIDE IN CUSTOMER DATABASES.
Supported Tax Framework Tax Authorities are shown in this registry.
Overriding this registry should never be required in customer systems.
Please discuss with the Accounting Compliance Product team if you think changes are needed to meet customer needs.
The registry is for Accounting Product development purposes only.
The registry is used by the Accounting Product team in WTG test systems when determining the set of Tax Authorities to be supported in our code base and deployed to all customer systems.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		[ExpectNoExceptions]
		public void TestTaxAuthorities_DefaultValuesMustBeValid()
		{
			ItemSet.TaxAuthorities.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.TaxAuthorities.DefaultValue);
		}

		public void TestSystems()
		{
			TestGenericRegistryItem(ItemSet.TaxSystems,
				"TaxSystems",
				Categories.Accounting_TaxFrameworkConfiguration,
				"Tax Systems (CargoWiseOne Support Only)",
				@"DO NOT OVERRIDE IN CUSTOMER DATABASES.
Supported Tax Framework Tax Systems are shown in this registry. 
Overriding this registry should never be required in customer systems.
Please discuss with the Accounting Compliance Product team if you think changes are needed to meet customer needs.
The registry is for Accounting Product development purposes only. 
The registry is used by the Accounting Product team in WTG test systems when determining the set of Tax Systems to be supported in our code base and deployed to all customer systems.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		[ExpectNoExceptions]
		public void TestTaxSystems_DefaultValuesMustBeValid()
		{
			ItemSet.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.TaxSystems.DefaultValue);
		}

		public void TestRevenueTaxExpenseRecoveryChargeCode()
		{
			AssertEquals("Name", "RevenueTaxExpenseRecoveryChargeCode", ItemSet.RevenueTaxExpenseRecoveryChargeCode.Name);
			AssertEquals("Category", "Accounting/Tax Configurations", ItemSet.RevenueTaxExpenseRecoveryChargeCode.Category);
			AssertEquals("Caption", "Revenue Tax Expense Recovery Charge Code", ItemSet.RevenueTaxExpenseRecoveryChargeCode.Caption);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.RevenueTaxExpenseRecoveryChargeCode.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, ItemSet.RevenueTaxExpenseRecoveryChargeCode.Options);
			AssertEquals("Default Value", Guid.Empty, ItemSet.RevenueTaxExpenseRecoveryChargeCode.DefaultValue);
			AssertEquals(typeof(TaxExpenseRecoveryChargeCodeGuidRegistryDataType), ItemSet.RevenueTaxExpenseRecoveryChargeCode.DataType.GetType());
		}

		#endregion

		#region Fixed Place of Supply Configuration

		public void TestFixedPlaceOfSupplyConfiguration()
		{
			TestGenericRegistryItem(ItemSet.FixedPlaceOfSupplyConfiguration,
				nameof(ItemSet.FixedPlaceOfSupplyConfiguration),
				Categories.Accounting_FixedPlaceOfSupplyConfiguration,
				"Fixed Place of Supply Type Configuration",
				"Configure the applicable types for Fixed Place of Supply for this company. The system will only allow users to select Fixed Place of Supply that belong to the types you select in this registry.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);

			AssertType<CodeDescriptionBoolRegistryEditorInfo>(ItemSet.FixedPlaceOfSupplyConfiguration.EditorInfo);
			var editor = (CodeDescriptionBoolRegistryEditorInfo)ItemSet.FixedPlaceOfSupplyConfiguration.EditorInfo;
			Assert("Checkbox should be visible", editor.IsBoolColumnVisible);
			Assert("Code and description should be read only", editor.IsOnlyBoolColumnEditable);

			Assert("All FPOS Types are unselected by default.", ItemSet.FixedPlaceOfSupplyConfiguration.DefaultValue.Cast<CodeDescriptionBool>().All(x => !x.Bool));
			var actualSequence = ItemSet.FixedPlaceOfSupplyConfiguration.DefaultValue.Cast<CodeDescriptionBool>().Select(x => x.Code.ToString()).ToArray();
			var expectedSequence = new[] { "STA", "TZN", "CON", "RUL" };
			AssertSequencesEqual("FPOS Types listed are in order of fallback, from highest priority to lowest.", expectedSequence, actualSequence);
		}

		public void TestEnableBranchLevelTaxOverrideRuleConfigurations()
		{
			AssertEquals("Caption", "Enable Branch Level Place of Supply and Tax Override Rule Configurations", ItemSet.EnableBranchLevelTaxOverrideRuleConfigurations.Caption);
			AssertEquals("Category", Categories.Accounting_FixedPlaceOfSupplyConfiguration, ItemSet.EnableBranchLevelTaxOverrideRuleConfigurations.Category);
			AssertEquals("DefaultValue", false, ItemSet.EnableBranchLevelTaxOverrideRuleConfigurations.DefaultValue);
			var expectedHint = @"Use this registry to extend Place of Supply and Tax Override Group configurations to allow Branch specific rules.

When set to 'No', Place of Supply and Tax Override Group rules are Country level configurations.

Most countries have a single, country-level tax system where all branches within a company record tax under a single, shared tax registration.

When set to 'Yes', Place of Supply and Tax Override Groups will permit configuration of 'Branch' specific rules.
Branch specific rules are only relevant when a Login Company is operating in a country where different branches within the same company have separate registrations, and where different tax defaulting rules, taxes and obligations apply in different parts of a country.

Note: When disabling this registry after it was overridden, Place of Supply and Tax Override Configuration that used Branch Level rules would become invalid. You would need to review your Place of Supply and Tax Override Configurations and remove any invalid rules.";
			AssertEquals("Hint", expectedHint, ItemSet.EnableBranchLevelTaxOverrideRuleConfigurations.Hint);
			AssertEquals("Name", "EnableBranchLevelTaxOverrideRuleConfigurations", ItemSet.EnableBranchLevelTaxOverrideRuleConfigurations.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnableBranchLevelTaxOverrideRuleConfigurations.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableBranchLevelTaxOverrideRuleConfigurations.Storage);
		}

		public void TestConfigureDefaultPlaceOfSupplyWhenNoMatchingRules()
		{
			TestGenericRegistryItem(ItemSet.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules,
				nameof(ItemSet.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules),
				Categories.Accounting_FixedPlaceOfSupplyConfiguration,
				"Configure default place of supply when no matching rules",
				"If you have enabled the Fixed Place of Supply Configuration in your login company, then use this registry to specify the default place of supply when there is no matching rule for the combination of the charge code/charge type /job type/direction.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode);

			var actualOptions = ((ComboBoxRegistryEditorInfo)ItemSet.ConfigureDefaultPlaceOfSupplyWhenNoMatchingRules.EditorInfo).LookUpList.GetAllCodes();
			var expectedOptions = new[] { ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode };
			AssertSequencesEqual(expectedOptions, actualOptions);
		}

		public void TestConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions()
		{
			AssertEquals("DEF", ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode);
			AssertEquals(ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Default.Code);
			AssertEquals("Bill to Party Location", ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Default.Description);

			AssertEquals("BLN", ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode);
			AssertEquals(ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode, ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Blank.Code);
			AssertEquals("Leave it empty", ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.Blank.Description);
		}

		public void TestConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation()
		{
			TestGenericRegistryItem(ItemSet.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation,
				nameof(ItemSet.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation),
				Categories.Accounting_FixedPlaceOfSupplyConfiguration,
				"Configure default place of supply configuration fallback",
				"If you have enabled the Fixed Place of Supply Configuration in your login company, then use this registry to specify the default place of supply when there is a matching rule for the combination of the charge code/charge type /job type/direction but there is missing location attribute for which the rule is looking for.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode);

			var actualOptions = ((ComboBoxRegistryEditorInfo)ItemSet.ConfigureDefaultPlaceOfSupplyWhenMatchingRulesButMissingLocation.EditorInfo).LookUpList.GetAllCodes();
			var expectedOptions = new[] { ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.DefaultCode, ConfigureDefaultPlaceOfSupplyWhenNoMatchingRulesOptions.BlankCode };
			AssertSequencesEqual(expectedOptions, actualOptions);
		}

		#endregion

		public void TestAmendmentReasonCodesList()
		{
			AssertEquals(ItemSet.AmendmentReasonCodesList.Value.CodesAsString, "IDE, IAM, TXT");

			CodeDescriptionPairList newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("AAA", "test aaa"));
			newlist1.Add(new CodeDescriptionPair("BBB", "test bbb"));
			ItemSet.AmendmentReasonCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertEquals(ItemSet.AmendmentReasonCodesList.Value.CodesAsString, "AAA, BBB");

			CodeDescriptionPairList newlist2 = new CodeDescriptionPairList();
			newlist2.Add(new CodeDescriptionPair("CCC", "test ccc"));
			newlist2.Add(new CodeDescriptionPair("DDD", "test ddd"));
			ItemSet.AmendmentReasonCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertEquals(ItemSet.AmendmentReasonCodesList.Value.CodesAsString, "CCC, DDD");

			AssertEquals(AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).CodesAsString, "AAA, BBB");
			AssertEquals(AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).CodesAsString, "CCC, DDD");

			GlbCompany argCompany = Factory.NewWithValidTestData<GlbCompany>();
			argCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			Factory.Save();

			var amendmentReasonCodeListForArgentina = ItemSet.AmendmentReasonCodesList.GetValueWithoutFallback(argCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals(4, amendmentReasonCodeListForArgentina.Count);
			AssertEquals(amendmentReasonCodeListForArgentina.CodesAsString, "IDE, IAM, TXT, MIR");
		}

		public void TestReversalReasonCodesList()
		{
			AssertEquals(ItemSet.ReversalReasonCodesList.Value.CodesAsString, "IDE, WOR, IAM, TXT");
			CodeDescriptionPairList newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("AAA", "test aaa"));
			newlist1.Add(new CodeDescriptionPair("BBB", "test bbb"));
			ItemSet.ReversalReasonCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertEquals(ItemSet.ReversalReasonCodesList.Value.CodesAsString, "AAA, BBB");

			CodeDescriptionPairList newlist2 = new CodeDescriptionPairList();
			newlist2.Add(new CodeDescriptionPair("CCC", "test ccc"));
			newlist2.Add(new CodeDescriptionPair("DDD", "test ddd"));
			ItemSet.ReversalReasonCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertEquals(ItemSet.ReversalReasonCodesList.Value.CodesAsString, "CCC, DDD");

			AssertEquals(AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).CodesAsString, "AAA, BBB");
			AssertEquals(AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).CodesAsString, "CCC, DDD");

			GlbCompany argCompany = Factory.NewWithValidTestData<GlbCompany>();
			argCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			Factory.Save();

			var reversalReasonCodeListForArgentina = ItemSet.ReversalReasonCodesList.GetValueWithoutFallback(argCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals(5, reversalReasonCodeListForArgentina.Count);
			AssertEquals(reversalReasonCodeListForArgentina.CodesAsString, "IDE, WOR, IAM, TXT, MIR");
		}

		public void TestComplianceDocumentSupportingReasonsReceivablesValue()
		{
			var newlist1 = new ComplianceDocumentSupportingReasonCollection();
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "AAA", EnglishDescription = "test aaa" });
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "BBB", EnglishDescription = "test bbb" });
			ItemSet.ComplianceDocumentSupportingReasonsReceivables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsReceivables.Value[0], "AAA", "test aaa");
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsReceivables.Value[1], "BBB", "test bbb");
			var newlist2 = new ComplianceDocumentSupportingReasonCollection();
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "CCC", EnglishDescription = "test ccc" });
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "DDD", EnglishDescription = "test ddd" });
			ItemSet.ComplianceDocumentSupportingReasonsReceivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsReceivables.Value[0], "CCC", "test ccc");
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsReceivables.Value[1], "DDD", "test ddd");

			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[0], "AAA", "test aaa");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[1], "BBB", "test bbb");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[0], "CCC", "test ccc");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[1], "DDD", "test ddd");
		}

		public void TestComplianceDocumentSupportingReasonsReceivables()
		{
			AssertEquals("Name", "ComplianceDocumentSupportingReasonsReceivables", ItemSet.ComplianceDocumentSupportingReasonsReceivables.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceDocumentSupportingReasonsReceivables.Category);
			AssertEquals("Caption", "Compliance Document Supporting Reasons - Receivables", ItemSet.ComplianceDocumentSupportingReasonsReceivables.Caption);
			AssertEquals("Hint", @"This registry enable users to configure a set of supporting reason codes for A/R Compliance Document. Where applicable, the user should select the applicable code against the compliance document record.

For example, a reason must be specified to support the issuance of zero rated compliance document in Taiwan. The single digit numeric value is included in the T02 text file submitted to the Tax Bureau.", ItemSet.ComplianceDocumentSupportingReasonsReceivables.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentSupportingReasonsReceivables.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentSupportingReasonsReceivables.Options);
			AssertEquals("Only show Taiwan", CountryFilterPKs.Taiwan, ItemSet.ComplianceDocumentSupportingReasonsReceivables.CountryFilterPKs);
		}

		public void TestComplianceDocumentSupportingReasonsPayables()
		{
			AssertEquals("Name", "ComplianceDocumentSupportingReasonsPayables", ItemSet.ComplianceDocumentSupportingReasonsPayables.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceDocumentSupportingReasonsPayables.Category);
			AssertEquals("Caption", "Compliance Document Supporting Reasons - Payables", ItemSet.ComplianceDocumentSupportingReasonsPayables.Caption);
			AssertEquals("Hint", @"This registry enable users to configure a set of supporting reason codes for Input Tax Claim. Where applicable, the user should select the applicable code against the compliance document record.

For example, the basis of input tax claim must be specified to A/P Compliance Document in Taiwan. The single digit numeric value is included in the TXT text file submitted to the Tax Bureau.", ItemSet.ComplianceDocumentSupportingReasonsPayables.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentSupportingReasonsPayables.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentSupportingReasonsPayables.Options);
			AssertEquals("Only show Taiwan", CountryFilterPKs.Taiwan, ItemSet.ComplianceDocumentSupportingReasonsPayables.CountryFilterPKs);
		}

		public void TestComplianceDocumentSupportingReasonsPayablesValue()
		{
			var newlist1 = new ComplianceDocumentSupportingReasonCollection();
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "AAA", EnglishDescription = "test aaa" });
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "BBB", EnglishDescription = "test bbb" });
			ItemSet.ComplianceDocumentSupportingReasonsPayables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsPayables.Value[0], "AAA", "test aaa");
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsPayables.Value[1], "BBB", "test bbb");
			var newlist2 = new ComplianceDocumentSupportingReasonCollection();
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "CCC", EnglishDescription = "test ccc" });
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "DDD", EnglishDescription = "test ddd" });
			ItemSet.ComplianceDocumentSupportingReasonsPayables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsPayables.Value[0], "CCC", "test ccc");
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingReasonsPayables.Value[1], "DDD", "test ddd");

			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsPayables.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[0], "AAA", "test aaa");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsPayables.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[1], "BBB", "test bbb");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsPayables.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[0], "CCC", "test ccc");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsPayables.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[1], "DDD", "test ddd");
		}

		public void TestComplianceDocumentSupportingDocumentType()
		{
			AssertEquals("Name", "ComplianceDocumentSupportingDocumentType", ItemSet.ComplianceDocumentSupportingDocumentType.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceDocumentSupportingDocumentType.Category);
			AssertEquals("Caption", "Compliance Document Supporting Document Type", ItemSet.ComplianceDocumentSupportingDocumentType.Caption);
			AssertEquals("Hint", @"This registry enable users to configure a set of document types and corresponding description that supports the tax filing where applicable.", ItemSet.ComplianceDocumentSupportingDocumentType.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentSupportingDocumentType.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentSupportingDocumentType.Options);
			AssertEquals("Only show Taiwan", CountryFilterPKs.Taiwan, ItemSet.ComplianceDocumentSupportingDocumentType.CountryFilterPKs);
		}

		public void TestComplianceDocumentSupportingDocumentTypeValue()
		{
			var newlist1 = new ComplianceDocumentSupportingReasonCollection();
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "AAA", EnglishDescription = "test aaa" });
			newlist1.Add(new ComplianceDocumentSupportingReason() { Code = "BBB", EnglishDescription = "test bbb" });
			ItemSet.ComplianceDocumentSupportingDocumentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingDocumentType.Value[0], "AAA", "test aaa");
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingDocumentType.Value[1], "BBB", "test bbb");

			var newlist2 = new ComplianceDocumentSupportingReasonCollection();
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "CCC", EnglishDescription = "test ccc" });
			newlist2.Add(new ComplianceDocumentSupportingReason() { Code = "DDD", EnglishDescription = "test ddd" });
			ItemSet.ComplianceDocumentSupportingDocumentType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingDocumentType.Value[0], "CCC", "test ccc");
			AssertComplianceDocumentSupporting(ItemSet.ComplianceDocumentSupportingDocumentType.Value[1], "DDD", "test ddd");

			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingDocumentType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[0], "AAA", "test aaa");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingDocumentType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[1], "BBB", "test bbb");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingDocumentType.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[0], "CCC", "test ccc");
			AssertComplianceDocumentSupporting(AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingDocumentType.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[1], "DDD", "test ddd");
		}

		void AssertComplianceDocumentSupporting(ComplianceDocumentSupportingReason reasonCode, string code, string englishDescription)
		{
			AssertEquals("code", code, reasonCode.Code);
			AssertEquals("englishDescription", englishDescription, reasonCode.EnglishDescription);
		}

		public void TestDocumentConfiguration()
		{
			AssertEquals("Name", "DocumentConfiguration", ItemSet.DocumentConfiguration.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.DocumentConfiguration.Category);
			AssertEquals("Caption", "Compliance Document Configuration", ItemSet.DocumentConfiguration.Caption);
			AssertEquals("Hint", @"This registry enables users to add an image and remark (optionally) against each compliance sub type where applicable.
When specified, you could include this image and remark in the relevant compliance document template printed via Receivables Compliance Documents > Print menu.", ItemSet.DocumentConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.DocumentConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.DocumentConfiguration.Options);
			AssertEquals("Only show Taiwan", CountryFilterPKs.Taiwan, ItemSet.DocumentConfiguration.CountryFilterPKs);
		}

		public void TestDocumentConfigurationValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var image = new Bitmap(1, 1);
				var image1 = new Bitmap(2, 2);

				var list = new ComplianceDocumentImageCollection();
				list.Add(new ComplianceDocumentImage() { Country = Core.Constants.CountryCodes.Taiwan, ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTC, Remark = "Remark", Image = image });
				list.Add(new ComplianceDocumentImage() { Country = Core.Constants.CountryCodes.Taiwan, ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, Remark = "Remark1", Image = image1 });
				ItemSet.DocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, list);
				AssertComplianceDocumentConfiguration(ItemSet.DocumentConfiguration.Value[0], "NTC", "Remark", image);
				AssertComplianceDocumentConfiguration(ItemSet.DocumentConfiguration.Value[1], "TXC", "Remark1", image1);

				AssertComplianceDocumentConfiguration(AccountingMasterFilesRegistry.Instance.DocumentConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[0], "NTC", "Remark", image);
				AssertComplianceDocumentConfiguration(AccountingMasterFilesRegistry.Instance.DocumentConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[1], "TXC", "Remark1", image1);
			}
		}

		void AssertComplianceDocumentConfiguration(ComplianceDocumentImage documentImage, ZString complianceSubType, ZString remark, Image image)
		{
			AssertEquals("Compliance Sub Type", complianceSubType, documentImage.ComplianceSubType);
			AssertEquals("Remark", remark, documentImage.Remark);
			Utilities.IsImageEqual(image, documentImage.Image);
		}

		public void TestComplianceDocumentRePrintRestriction()
		{
			AssertEquals("Name", "ComplianceDocumentRePrintRestriction", ItemSet.ComplianceDocumentRePrintRestriction.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceDocumentRePrintRestriction.Category);
			AssertEquals("Caption", "Compliance Document Re-Print Restriction", ItemSet.ComplianceDocumentRePrintRestriction.Caption);
			AssertEquals("Hint", @"Use this registry to impose re-print restriction with reference to the Organization Category (i.e. Organization > Details > Details > Organization Category and Compliance Document Menu (i.e. Compliance Invoice Book > Compliance Document Menu).

This is useful when you are only allowed to re-print x number of times for certain compliance document depending on the debtor's organization category.

By default, there will be no re-print restriction. Users will be allowed to re-print compliance document as many times as required.

Note: This registry is only relevant when 'Enable Compliance Document Module' registry has been set to 'Yes' .", ItemSet.ComplianceDocumentRePrintRestriction.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentRePrintRestriction.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentRePrintRestriction.Options);
			AssertEquals("Only show Taiwan", CountryFilterPKs.Taiwan, ItemSet.ComplianceDocumentRePrintRestriction.CountryFilterPKs);
		}

		public void TestComplianceDocumentRePrintRestrictionValue()
		{
			var newlist1 = new ComplianceDocumentRePrintRestrictionCollection();
			newlist1.Add(new ComplianceDocumentRePrintRestriction() { OrganizationCategory = "BUS", NumberOfReprintAllowed = 1, ComplianceDocumentMenu = "AAA" });
			newlist1.Add(new ComplianceDocumentRePrintRestriction() { OrganizationCategory = "GOV", NumberOfReprintAllowed = 2, ComplianceDocumentMenu = "BBB" });
			ItemSet.ComplianceDocumentRePrintRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertComplianceDocumentRePrintRestriction(ItemSet.ComplianceDocumentRePrintRestriction.Value[0], "BUS", 1, "AAA");
			AssertComplianceDocumentRePrintRestriction(ItemSet.ComplianceDocumentRePrintRestriction.Value[1], "GOV", 2, "BBB");

			var newlist2 = new ComplianceDocumentRePrintRestrictionCollection();
			newlist2.Add(new ComplianceDocumentRePrintRestriction() { OrganizationCategory = "NAT", NumberOfReprintAllowed = 3, ComplianceDocumentMenu = "CCC" });
			newlist2.Add(new ComplianceDocumentRePrintRestriction() { OrganizationCategory = "NGO", NumberOfReprintAllowed = 4, ComplianceDocumentMenu = "DDD" });
			ItemSet.ComplianceDocumentRePrintRestriction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertComplianceDocumentRePrintRestriction(ItemSet.ComplianceDocumentRePrintRestriction.Value[0], "NAT", 3, "CCC");
			AssertComplianceDocumentRePrintRestriction(ItemSet.ComplianceDocumentRePrintRestriction.Value[1], "NGO", 4, "DDD");

			AssertComplianceDocumentRePrintRestriction(AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[0], "BUS", 1, "AAA");
			AssertComplianceDocumentRePrintRestriction(AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)[1], "GOV", 2, "BBB");
			AssertComplianceDocumentRePrintRestriction(AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[0], "NAT", 3, "CCC");
			AssertComplianceDocumentRePrintRestriction(AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)[1], "NGO", 4, "DDD");
		}

		void AssertComplianceDocumentRePrintRestriction(ComplianceDocumentRePrintRestriction restriction, string organizationCategory, int numberOfReprintAllowed, string complianceDocumentMenu)
		{
			AssertEquals("OrganizationCategory", organizationCategory, restriction.OrganizationCategory);
			AssertEquals("NumberOfReprintAllowed", numberOfReprintAllowed, restriction.NumberOfReprintAllowed);
			AssertEquals("ComplianceDocumentMenu", complianceDocumentMenu, restriction.ComplianceDocumentMenu);
		}

		public void TestInvoiceCurrencyType()
		{
			AssertEquals("Name", "EnableInvoiceCurrencyType", ItemSet.EnableInvoiceCurrencyType.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.EnableInvoiceCurrencyType.Category);
			AssertEquals("Caption", "Enable Job Billing Exchange Rate Configuration by Invoice Currency Type", ItemSet.EnableInvoiceCurrencyType.Caption);
			AssertEquals("Hint", $"By default, this registry is set to 'NO'. Overriding this registry and setting it to 'YES' allows you to configure separate exchange rate preferences for invoices issued in foreign currency and invoices issued in local currency.{System.Environment.NewLine}{System.Environment.NewLine}When enabled, an additional 'Invoice Currency Type' column is available in Job Billing Exchange Rate Configuration and Job Exchange Rates grid. This allows you to select whether each configuration applies to foreign currency invoices only, local currency invoices only or whether it applies to both - foreign and local.", ItemSet.EnableInvoiceCurrencyType.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableInvoiceCurrencyType.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnableInvoiceCurrencyType.Options);
			AssertEquals("Default value", false, ItemSet.EnableInvoiceCurrencyType.DefaultValue);
		}

		public void TestEnableXUTImportAutoMapAccrualFeature()
		{
			AssertEquals("Name", "EnableXUTImportAutoMapAccrualFeature", ItemSet.EnableXUTImportAutoMapAccrualFeature.Name);
			AssertEquals("Category", Categories.Accounting_Temp, ItemSet.EnableXUTImportAutoMapAccrualFeature.Category);
			AssertEquals("Caption", "Enable XUT Import Auto Map Accrual Feature", ItemSet.EnableXUTImportAutoMapAccrualFeature.Caption);
			AssertEquals("Hint", "When this registry is enabled, enable the feature to automatically map the Accrual of imported XUT to charges.", ItemSet.EnableXUTImportAutoMapAccrualFeature.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableXUTImportAutoMapAccrualFeature.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableXUTImportAutoMapAccrualFeature.Options);
			AssertEquals("Default value", false, ItemSet.EnableXUTImportAutoMapAccrualFeature.DefaultValue);
		}

		public void TestCollectionOrderMinimumAmount()
		{
			var item = ItemSet.CollectionOrderMinimumAmount;
			AssertEquals("Name", "CollectionOrderMinimumAmount", item.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders, item.Category);
			AssertEquals("Caption", "Collection Order Minimum Amount", item.Caption);
			AssertEquals("Hint", "This registry enables you to define the minimum collection order amount where applicable. When an amount greater than zero is set, users not granted ‘Allow Order Below Minimum Amount’ will not be able to create collection order for amount less than the minimum stipulated amount in this registry", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertEquals("Default Value", 0, item.DefaultValue);
		}

		public void TestConsolidatedAccountingCategoryList()
		{
			AssertEquals("Name", "ConsolidatedAccountingCategory", ItemSet.ConsolidatedAccountingCategoryList.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Organizations_CodeLists, ItemSet.ConsolidatedAccountingCategoryList.Category);
			AssertEquals("Caption", "Consolidated Accounting Category List", ItemSet.ConsolidatedAccountingCategoryList.Caption);
			AssertEquals("Hint", "The list of valid entries for Consolidated Accounting Category", ItemSet.ConsolidatedAccountingCategoryList.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ConsolidatedAccountingCategoryList.Storage);
			Assert("Pre-condition: ProductivityWiseModeEnabled is false", !DataRegistry.Instance.ProductivityWiseModeEnabled);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ConsolidatedAccountingCategoryList.Options);

			AssertEquals("EditorInfo Type", typeof(CodeDescriptionWithGroupRegistryEditorInfo), ItemSet.ConsolidatedAccountingCategoryList.EditorInfo.GetType());
			AssertEquals("EditorInfo Caption", "Class", ((CodeDescriptionWithGroupRegistryEditorInfo)ItemSet.ConsolidatedAccountingCategoryList.EditorInfo).GroupColumnCaption);

			var expectedDefaultValue = new CodeDescriptionWithGroupCollection(new ConsolidatedAccountingCategoryClassList(), ConsolidatedAccountingCategoryClassList.Codes.Intercompany);
			expectedDefaultValue.Add(Constants.AccountsCategory.Unrelated, (NoResString)"Unrelated company", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			expectedDefaultValue.Add(Constants.AccountsCategory.WhollyOwned, (NoResString)"Wholly owned subsidiary");
			expectedDefaultValue.Add(Constants.AccountsCategory.MinorityWithReporting, (NoResString)"Minority Interest with reporting");
			expectedDefaultValue.Add(Constants.AccountsCategory.MinorityWithNoReporting, (NoResString)"Minority Interest with no reporting");
			expectedDefaultValue.Add(Constants.AccountsCategory.RelatedMinorityShareholder, (NoResString)"Related minority shareholder");
			expectedDefaultValue.Add(Constants.AccountsCategory.RelatedMajorityShareholder, (NoResString)"Related Majority Shareholder");
			expectedDefaultValue.Add(Constants.AccountsCategory.RelatedWhollyOwningShareholder, (NoResString)"Related Wholly Owning Shareholder");
			expectedDefaultValue.Add(Constants.AccountsCategory.GroupCompanyRelatedMinority, (NoResString)"Group Company Related Minority (no direct ownership)");
			expectedDefaultValue.Add(Constants.AccountsCategory.GroupCompanyRelatedMajority, (NoResString)"Group Company Related Majority (no direct ownership)");

			var defaultValue = ItemSet.ConsolidatedAccountingCategoryList.DefaultValue;
			foreach (ConsolidatedAccountingCategoryItem item in defaultValue)
			{
				AssertEquals(item.Code, item.OriginalCode);
			}

			AssertContainsExactElementsInAnyOrder(
				new CodeDescriptionWithGroupEqualityComparer(),
				expectedDefaultValue.Cast<CodeDescriptionWithGroup>(),
				defaultValue.Cast<CodeDescriptionWithGroup>()
			);
		}

		public void TestConsolidatedAccountingCategoryList_OnUpdateAction()
		{
			var collection = new ConsolidatedAccountingCategoryCollection();
			collection.Add(Constants.AccountsCategory.WhollyOwned, (NoResString)"Wholly owned subsidiary");
			collection.Add(Constants.AccountsCategory.MinorityWithReporting, (NoResString)"Minority Interest with reporting");
			AssertEquals("PreCondition", 2, collection.Count);
			foreach (ConsolidatedAccountingCategoryItem itemValue in collection)
			{
				AssertNullOrEmpty("PreCondition", itemValue.OriginalCode);
			}

			ItemSet.ConsolidatedAccountingCategoryList.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			foreach (ConsolidatedAccountingCategoryItem itemValue in collection)
			{
				AssertEquals(itemValue.Code, itemValue.OriginalCode);
			}
		}

		public void TestConsolidatedAccountingCategoryList_ItemType()
		{
			AssertType<AccountingCodeDescriptionWithGroupRegistryItem<ConsolidatedAccountingCategoryCollection, ConsolidatedAccountingCategoryItem>>(ItemSet.ConsolidatedAccountingCategoryList);
			AssertType<ConsolidatedAccountingCategoryCollection>(ItemSet.ConsolidatedAccountingCategoryList.DefaultValue);
			Assert(ItemSet.ConsolidatedAccountingCategoryList.DefaultValue.Any());
			AssertCollectionItemType(ItemSet.ConsolidatedAccountingCategoryList.DefaultValue);

			var overrideValue = ConsolidatedAccountingCategoryCollection.Converter(ItemSet.ConsolidatedAccountingCategoryList.DefaultValue);
			overrideValue.Add("AAA", (NoResString)"AAA Desc", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			ItemSet.ConsolidatedAccountingCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, overrideValue);
			AssertType<ConsolidatedAccountingCategoryCollection>(ItemSet.ConsolidatedAccountingCategoryList.Value);
			AssertCollectionItemType(ItemSet.ConsolidatedAccountingCategoryList.Value);

			void AssertCollectionItemType(CodeDescriptionWithGroupCollection collection)
			{
				foreach (var item in collection)
				{
					AssertType<ConsolidatedAccountingCategoryItem>(item);
				}
			}
		}

		public void TestConsolidatedAccountingCategoryList_ItemCanDelete()
		{
			const string testingCode = "AAA";
			const string testingCodeForChanging = "AAB";

			var newRegistryValue = ConsolidatedAccountingCategoryCollection.Converter(ItemSet.ConsolidatedAccountingCategoryList.DefaultValue);
			newRegistryValue.Add(testingCode, (NoResString)"AAA Desc", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			ItemSet.ConsolidatedAccountingCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistryValue);

			var item = ItemSet.ConsolidatedAccountingCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).FindByCode(testingCode) as ConsolidatedAccountingCategoryItem;
			AssertNotNull(item);
			AssertEquals("PreCondition", testingCode, item.Code);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.CompanyData.OB_ARConsolidatedAccountingCategory = testingCode;
			Factory.Save();

			item.Code = testingCodeForChanging;
			AssertEquals("PreCondition, OriginalCode will not be changed when Code is changed.", testingCode, item.OriginalCode);
			Assert(!item.CanDelete);
			AssertEquals("This code cannot be deleted as it is in use by at least one organization record in the system.", item.ReasonForNotAbleToDelete);

			item.Code = testingCode;
			Assert(!item.CanDelete);
			AssertEquals("This code cannot be deleted as it is in use by at least one organization record in the system.", item.ReasonForNotAbleToDelete);

			org.CompanyData.OB_ARConsolidatedAccountingCategory = string.Empty;
			Factory.Save();
			AssertEquals(true, item.CanDelete);
			AssertEquals("This is a system defined value and cannot be deleted.", item.ReasonForNotAbleToDelete);

			item.SystemDefined = true;
			AssertEquals(false, item.CanDelete);
			AssertEquals("This is a system defined value and cannot be deleted.", item.ReasonForNotAbleToDelete);
		}

		public void TestConsolidatedAccountingCategoryList_ItemCodeValidation()
		{
			const string testingCode = "AAA";
			const string testingCodeForChanging = "AAB";

			var registryValue = ConsolidatedAccountingCategoryCollection.Converter(ItemSet.ConsolidatedAccountingCategoryList.DefaultValue);
			registryValue.Add(testingCode, (NoResString)"AAA Desc", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			ItemSet.ConsolidatedAccountingCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var item = ItemSet.ConsolidatedAccountingCategoryList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).FindByCode(testingCode) as ConsolidatedAccountingCategoryItem;
			AssertNotNull(item);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.CompanyData.OB_ARConsolidatedAccountingCategory = testingCode;
			Factory.Save();
			AssertItemCodeValidation(true);

			org.CompanyData.OB_ARConsolidatedAccountingCategory = string.Empty;
			Factory.Save();
			AssertItemCodeValidation(false);

			item.SystemDefined = true;
			AssertItemCodeValidation(false);

			void AssertItemCodeValidation(bool expectedValidationError)
			{
				AssertEquals(false, item.CodeInfo.HasError("This code cannot be changed as it is in use by at least one organization record in the system."));

				item.Code = testingCodeForChanging;
				AssertEquals("PreCondition, OriginalCode will not be changed when Code is changed.", testingCode, item.OriginalCode);
				AssertEquals(expectedValidationError, item.CodeInfo.HasError("This code cannot be changed as it is in use by at least one organization record in the system."));

				item.Code = testingCode;
				AssertEquals(false, item.CodeInfo.HasError("This code cannot be changed as it is in use by at least one organization record in the system."));
			}
		}

		public void TestConsolidatedAccountingCategoryList_IsHiddenWhenProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertEquals("Should be Hidden when ProductivityWiseModeEnabled", RegistryOptions.IsHidden, ItemSet.ConsolidatedAccountingCategoryList.Options);
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
		}

		public void TestOnBuildLogReferenceForConsolidatedAccountingCategoryList()
		{
			var defaultCollection = ConsolidatedAccountingCategoryCollection.Converter(Instance.ConsolidatedAccountingCategoryList.DefaultValue);

			var collection_AddRow = ConsolidatedAccountingCategoryCollection.Converter(Instance.ConsolidatedAccountingCategoryList.DefaultValue);
			collection_AddRow.Add("AAA", (NoResString)"AAA Desc", "INT");

			Instance.ConsolidatedAccountingCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection_AddRow);
			var collection_AddRowInRegistryValue = Instance.ConsolidatedAccountingCategoryList.Value;

			var collection_ModifyRowDesc = ConsolidatedAccountingCategoryCollection.Converter(Instance.ConsolidatedAccountingCategoryList.DefaultValue);
			collection_ModifyRowDesc.Add("AAA", (NoResString)"AAA New Desc", "INT");

			var collection_ModifyRowClass = ConsolidatedAccountingCategoryCollection.Converter(Instance.ConsolidatedAccountingCategoryList.DefaultValue);
			collection_ModifyRowClass.Add("AAA", (NoResString)"AAA Desc", "TPY");

			var collection_NullDesc = ConsolidatedAccountingCategoryCollection.Converter(Instance.ConsolidatedAccountingCategoryList.DefaultValue);
			collection_NullDesc.Add("AAA", null, "INT");

			var collection_EmptyDesc = ConsolidatedAccountingCategoryCollection.Converter(Instance.ConsolidatedAccountingCategoryList.DefaultValue);
			collection_EmptyDesc.Add("AAA", (NoResString)string.Empty, "INT");

			AssertAddRow();
			AssertModifyDesc();
			AssertModifyClass();
			AssertRemoveRow();
			AssertValueNotChangedForReloadedValue();
			AssertValueNotChangedForNullAndEmpty();
			AssertModifyDescToNull();

			void AssertAddRow()
			{
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.ConsolidatedAccountingCategoryList, defaultCollection, collection_AddRow);
				var result = Instance.ConsolidatedAccountingCategoryList.OnBuildLogReference(args);
				AssertEquals(@"Added: Code=[AAA], Description=[AAA Desc], Class=[INT]
", result);
			}

			void AssertModifyDesc()
			{
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.ConsolidatedAccountingCategoryList, collection_AddRow, collection_ModifyRowDesc);
				var result = Instance.ConsolidatedAccountingCategoryList.OnBuildLogReference(args);
				AssertEquals(@"Deleted: Code=[AAA], Description=[AAA Desc], Class=[INT]
Added: Code=[AAA], Description=[AAA New Desc], Class=[INT]
", result);
			}

			void AssertModifyClass()
			{
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.ConsolidatedAccountingCategoryList, collection_AddRow, collection_ModifyRowClass);
				var result = Instance.ConsolidatedAccountingCategoryList.OnBuildLogReference(args);
				AssertEquals(@"Deleted: Code=[AAA], Description=[AAA Desc], Class=[INT]
Added: Code=[AAA], Description=[AAA Desc], Class=[TPY]
", result);
			}

			void AssertRemoveRow()
			{
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.ConsolidatedAccountingCategoryList, collection_AddRow, defaultCollection);
				var result = Instance.ConsolidatedAccountingCategoryList.OnBuildLogReference(args);
				AssertEquals(@"Deleted: Code=[AAA], Description=[AAA Desc], Class=[INT]
", result);
			}

			void AssertValueNotChangedForReloadedValue()
			{
				AssertType<NoResString>("PreCondition", collection_AddRow.FindByCode("AAA").Description);
				AssertType<ResourceString>("PreCondition", collection_AddRowInRegistryValue.FindByCode("AAA").Description);
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.ConsolidatedAccountingCategoryList, collection_AddRow, collection_AddRowInRegistryValue);
				var result = Instance.ConsolidatedAccountingCategoryList.OnBuildLogReference(args);
				AssertEquals(string.Empty, result);
			}

			void AssertValueNotChangedForNullAndEmpty()
			{
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.ConsolidatedAccountingCategoryList, collection_NullDesc, collection_EmptyDesc);
				var result = Instance.ConsolidatedAccountingCategoryList.OnBuildLogReference(args);
				AssertEquals(string.Empty, result);
			}

			void AssertModifyDescToNull()
			{
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.ConsolidatedAccountingCategoryList, collection_ModifyRowDesc, collection_NullDesc);
				var result = Instance.ConsolidatedAccountingCategoryList.OnBuildLogReference(args);
				AssertEquals(@"Deleted: Code=[AAA], Description=[AAA New Desc], Class=[INT]
Added: Code=[AAA], Description=[], Class=[INT]
", result);
			}
		}

		public void TestConsolidatedAccountingCategoryVisibility_WithoutCache()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			AssertEquals(RegistryOptions.Default, ItemSet.ConsolidatedAccountingCategoryList.Options);
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertEquals(RegistryOptions.IsHidden, ItemSet.ConsolidatedAccountingCategoryList.Options);
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			AssertEquals(RegistryOptions.Default, ItemSet.ConsolidatedAccountingCategoryList.Options);
		}

		public void TestUseCusClearPortAsHomeCntryForTaxOvrds()
		{
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Options);
			AssertEquals("Default value", true, AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Value);
		}

		public void TestUseBrokerageLocationsForBilling()
		{
			AssertEquals("Name", "UseBrokerageLocationsForBilling", ItemSet.UseBrokerageLocationsForBilling.Name);
			AssertEquals("Category", "Accounting", ItemSet.UseBrokerageLocationsForBilling.Category);
			AssertEquals("Caption", "Use Brokerage Locations For Auto Billing", ItemSet.UseBrokerageLocationsForBilling.Caption);
			AssertEquals("Hint", @"Use Brokerage Locations (Origin, Destination etc.) for auto-billing.", ItemSet.UseBrokerageLocationsForBilling.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.UseBrokerageLocationsForBilling.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.UseBrokerageLocationsForBilling.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.UseBrokerageLocationsForBilling.Value);
		}

		public void TestAllowRePrintingOfInvoicesAndCreditNotes()
		{
			AssertEquals("Name", "AllowRePrintingOfInvoicesAndCreditNotes", ItemSet.AllowRePrintingOfInvoicesAndCreditNotes.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.AllowRePrintingOfInvoicesAndCreditNotes.Category);
			AssertEquals("Caption", "Allow re-printing of invoices and credit notes", ItemSet.AllowRePrintingOfInvoicesAndCreditNotes.Caption);
			AssertEquals("Hint"
				, @"This registry controls the ability to re-print AR Invoices and Credit Notes that have already been printed / delivered.

Setting this registry to to 'No' will prevent all users in your login company from re-printing receivables invoices and credit notes and will redirect them to the eDocs tab of a transaction to open the saved version of the transaction and re-print it."
				, ItemSet.AllowRePrintingOfInvoicesAndCreditNotes.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.Storage);
			AssertEquals("Options", RegistryOptions.Default, AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.Options);
			AssertEquals("Default value", true, AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.Value);
		}

		public void TestValidateTaxIDApplicationForExporterExemptionItaly()
		{
			AssertEquals("Name", "ValidateTaxIDApplicationForExporterExemptionItaly", ItemSet.ValidateTaxIDApplicationForExporterExemptionItaly.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.ValidateTaxIDApplicationForExporterExemptionItaly.Category);
			AssertEquals("Caption", "Validate Tax ID Application for Exporter Exemption (Italy)", ItemSet.ValidateTaxIDApplicationForExporterExemptionItaly.Caption);
			AssertEquals("Hint", @"This registry controls whether a Debtor's Exporter Exemption Certificate and its Ceiling Limit prevents posting of transactions if they contain certain Tax IDs. This registry only applies for Italy login companies.

When enabled, the registry affects posting of new Invoice, Credit Note or Adjustment transactions and restricts the Tax IDs permitted in those transactions as described below.

The DICH.INT Tax ID is not permitted unless the Debtor has recorded a valid Exporter Exemption certificate and the cumulative DICH.INT charges are less than the certificate's Ceiling Limit.

Any Tax ID with a rate greater than zero is not permitted if the Debtor has recorded a valid Exporter Exemption certificate and the cumulative DICH.INT charges are less than the certificate's Ceiling Limit.

If these restrictions are not required, please set this registry to No. If the registry is disabled, the DICH.INT Tax ID can be used for any Debtor, and any Tax ID can be used for a Debtor with an Exporter Exemption certificate with remaining limit."
				, ItemSet.ValidateTaxIDApplicationForExporterExemptionItaly.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.Storage);
			AssertEquals("Default value", true, AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.Value);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertEquals("Expected default value", true, AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.Value);
			}
		}

		public void TestCollectionBatchTypes()
		{
			var item = ItemSet.CollectionBatchTypes;
			AssertEquals("Name", "CollectionBatchTypes", item.Name);
			AssertEquals("Category", Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders, item.Category);
			AssertEquals("Caption", "Collection Batch Types", item.Caption);
			AssertEquals("Hint", "This registry allows you to configure user-defined Collection Batch Types. Batch types can be used to classify Receivables Collection Batches. By default, all batches are recorded as STD - Standard Batch.This batch type cannot be removed.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.Company | RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.Default, item.Options);
			AssertEquals("STD Standard Batch Code is present", true, item.DefaultValue.ContainsCode("STD"));
		}

		public void TestTaxMessageGroupsManagementOnlyForSupport()
		{
			AssertEquals("Only for support", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.Options);
		}

		public void TestTaxMessageGroupsManagement()
		{
			var item = ItemSet.TaxMessageGroupsManagement;
			AssertEquals("Name", "TaxMessageGroupsManagement", item.Name);
			AssertEquals("Category", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, item.Category);
			AssertEquals("Caption", "Tax Message Groups Management (CargoWise Support Only)", item.Caption);
			AssertEquals("Hint", "This registry allows you to define a mapping between the Tax Message and a code used by Tax authorities for eInvoicing or reporting.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);

			AssertEquals("Pre-condition - Country is AU", Constants.CountryCodes.Australia, Env.CurrentCompany.Country.Code);
			AssertEquals("DefaultValue is empty", 0, item.DefaultValue.Count);
		}

		public void TestTaxMessageGroupsManagementDefaultValueForItalia()
		{
			var factory = new BusinessObjectFactory();
			var italyCompany1 = factory.NewWithValidTestData<GlbCompany>();
			italyCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Italy;

			var italyCompany2 = factory.NewWithValidTestData<GlbCompany>();
			italyCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Italy;

			var nonItalyCompany = factory.NewWithValidTestData<GlbCompany>();
			nonItalyCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(italyCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(italyCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements are same in ITAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonItalyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(italyCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements are same in ITAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			Assert("All elements are active", result.ToList<CodeDescriptionBoolRelatedItem>().All(x => x.Bool));
		}

		public void TestTaxMessageGroupsManagementDefaultValueForFiji()
		{
			var factory = new BusinessObjectFactory();
			var fijiCompany1 = factory.NewWithValidTestData<GlbCompany>();
			fijiCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Fiji;

			var fijiCompany2 = factory.NewWithValidTestData<GlbCompany>();
			fijiCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Fiji;

			var nonFijiCompany = factory.NewWithValidTestData<GlbCompany>();
			nonFijiCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(fijiCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(fijiCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements are same in FJAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonFijiCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(fijiCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements are same in ITAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", new ZString[] { FijiComplianceInfo.TaxMessageGroupCodes.A, FijiComplianceInfo.TaxMessageGroupCodes.B, FijiComplianceInfo.TaxMessageGroupCodes.C, FijiComplianceInfo.TaxMessageGroupCodes.D, FijiComplianceInfo.TaxMessageGroupCodes.N, FijiComplianceInfo.TaxMessageGroupCodes.NOT_REQ },
				resultList.Where(x => x.Bool).Select(y => y.Code));
			AssertContainsExactElementsInAnyOrder("inactive elements", new ZString[] { FijiComplianceInfo.TaxMessageGroupCodes.E, FijiComplianceInfo.TaxMessageGroupCodes.F, FijiComplianceInfo.TaxMessageGroupCodes.P },
				resultList.Where(x => !x.Bool).Select(y => y.Code));
		}

		public void TestTaxMessageGroupsManagementDefaultValueForSamoa()
		{
			var factory = new BusinessObjectFactory();
			var wsCompany1 = factory.NewWithValidTestData<GlbCompany>();
			wsCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.WesternSamoa;

			var wsCompany2 = factory.NewWithValidTestData<GlbCompany>();
			wsCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.WesternSamoa;

			var nonWSCompany = factory.NewWithValidTestData<GlbCompany>();
			nonWSCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(wsCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(wsCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements are same in WSAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonWSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(wsCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements are same in WSAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", new ZString[] { SamoaComplianceInfo.TaxMessageGroupCodes.A, SamoaComplianceInfo.TaxMessageGroupCodes.B, SamoaComplianceInfo.TaxMessageGroupCodes.C, SamoaComplianceInfo.TaxMessageGroupCodes.N, SamoaComplianceInfo.TaxMessageGroupCodes.NOT_REQ },
				resultList.Where(x => x.Bool).Select(y => y.Code));
			AssertContainsExactElementsInAnyOrder("inactive elements", new ZString[] { SamoaComplianceInfo.TaxMessageGroupCodes.E, SamoaComplianceInfo.TaxMessageGroupCodes.F, SamoaComplianceInfo.TaxMessageGroupCodes.P },
				resultList.Where(x => !x.Bool).Select(y => y.Code));
		}

		public void TestTaxMessageGroupsManagementDefaultValueForNorway()
		{
			var factory = new BusinessObjectFactory();
			var noCompany1 = factory.NewWithValidTestData<GlbCompany>();
			noCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Norway;

			var noCompany2 = factory.NewWithValidTestData<GlbCompany>();
			noCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Norway;

			var nonNOCompany = factory.NewWithValidTestData<GlbCompany>();
			nonNOCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(noCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(noCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements are same in NOAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonNOCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(noCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements are same in NOAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			Assert("All elements are active", result.ToList<CodeDescriptionBoolRelatedItem>().All(x => x.Bool));
		}

		public void TestPTBillingSoftwareCertificateNumber()
		{
			TestGenericRegistryItem(ItemSet.PTBillingSoftwareCertificateNumber,
				"PTBillingSoftwareCertificateNumber",
				Categories.Accounting_SystemCertifications_Portugal,
				$"Billing Software Certificate Number({Core.Constants.ProductName} Support Only)",
				$"This registry contains {Core.Constants.ProductName}'s software billing and accounting certification numbers issued by the relevant Local Authority in this country.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"0");
		}

		public void TestPTBillingSoftwareCertificateNumberMessage()
		{
			TestGenericRegistryItem(ItemSet.PTBillingSoftwareCertificateNumberMessage,
				"PTBillingSoftwareCertificateNumberMessage",
				Categories.Accounting_SystemCertifications_Portugal,
				$"Billing Software Certificate Number Message({Core.Constants.ProductName} Support Only)",
				@"This registry is used to configure the country's certification message to be printed in invoice documents.
Some countries require that all printed documents issued by a certified software to include both the certification number and a related certification description.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"-Processado por Programa Certificado no.");

			AssertEquals(typeof(StringRegistryItem), ItemSet.PTBillingSoftwareCertificateNumberMessage.GetType());
		}

		public void TestPTSoftwareName()
		{
			TestGenericRegistryItem(ItemSet.PTSoftwareName,
				"PTSoftwareName",
				Categories.Accounting_SystemCertifications_Portugal,
				$"Software Name({BrandingFactory.Instance.ProductName} Support Only)",
				@"This registry is used to store the name of the software as required by tax authorities for legal reporting purposes.
The software name must match the one registered with the tax authorities of each respective country.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				$"{BrandingFactory.Instance.ProductName}/Wisetech Global Limited");

			AssertEquals(typeof(StringRegistryItem), ItemSet.PTSoftwareName.GetType());
		}

		public void TestDEWTGsProducerIDForELSTER()
		{
			TestGenericRegistryItem(ItemSet.DEWTGsProducerIDForELSTER,
				"DEWTGsProducerIDForELSTER",
				Categories.Accounting_SystemCertifications_Germany,
				$"WTGs Producer ID For ELSTER ({BrandingFactory.Instance.ProductName} Support Only)",
				$"This registry contains WTG's software producer ID for {BrandingFactory.Instance.ProductName} for ELSTER reports issued by the German Fiscal Authorities.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"30624");
		}

		public void TestDEWTGsProducerIDForELSTERMatchesProductName()
		{
			var herstellerID = Instance.DEWTGsProducerIDForELSTER.Value;
			Assert("DE WTGs Producer ID for CargWise for ELSTER has changed.", herstellerID == "30624");
			AssertEquals("We must request a new DEWTGsProducerIDForELSTER because the product name has changed.", "CargoWise", BrandingFactory.Instance.ProductName);
		}

		public void TestILAccountingSoftwareNumber()
		{
			TestStringRegistryItem(ItemSet.ILAccountingSoftwareNumber,
				"ILAccountingSoftwareNumber",
				Categories.Accounting_SystemCertifications_Israel,
				"Accounting Software Number (CargoWiseOne Support only)",
				$"This registry contains {Core.Constants.ProductName}'s Software Accounting Certification Number issued by the relevant Local Authority in this country.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				string.Empty,
				CharacterCase.Normal,
				"12345678");
			AssertEquals(0, ((StringRegistryDataType)ItemSet.ILAccountingSoftwareNumber.DataType).MinLength);
			AssertEquals(8, ((StringRegistryDataType)ItemSet.ILAccountingSoftwareNumber.DataType).MaxLength);
		}

		public void TestILAccountingSoftwareVersion()
		{
			TestGenericRegistryItem(ItemSet.ILAccountingSoftwareVersion,
				"ILAccountingSoftwareVersion",
				Categories.Accounting_SystemCertifications_Israel,
				"Accounting Software Version (CargoWiseOne Support only)",
				$"This registry contains {Core.Constants.ProductName}'s Software Accounting Version.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				string.Empty);
			AssertEquals(0, ((StringRegistryDataType)ItemSet.ILAccountingSoftwareVersion.DataType).MinLength);
			AssertEquals(50, ((StringRegistryDataType)ItemSet.ILAccountingSoftwareVersion.DataType).MaxLength);
		}

		public void TestTaxMessageGroupsManagementDefaultValueForPortugal()
		{
			var factory = new BusinessObjectFactory();
			var portugalCompany1 = factory.NewWithValidTestData<GlbCompany>();
			portugalCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Portugal;

			var portugalCompany2 = factory.NewWithValidTestData<GlbCompany>();
			portugalCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Portugal;

			var nonPortugalCompany = factory.NewWithValidTestData<GlbCompany>();
			nonPortugalCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(portugalCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(portugalCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements are same in PTAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonPortugalCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(portugalCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements are same in PTAccInvMsgCodeList", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var activePortugalAccInvMsgCodes = new ZString[] { PortugalComplianceInfo.TaxMessageGroupCodes.M01,
				PortugalComplianceInfo.TaxMessageGroupCodes.M02, PortugalComplianceInfo.TaxMessageGroupCodes.M04,
				PortugalComplianceInfo.TaxMessageGroupCodes.M05, PortugalComplianceInfo.TaxMessageGroupCodes.M06,
				PortugalComplianceInfo.TaxMessageGroupCodes.M07, PortugalComplianceInfo.TaxMessageGroupCodes.M09,
				PortugalComplianceInfo.TaxMessageGroupCodes.M10, PortugalComplianceInfo.TaxMessageGroupCodes.M16,
				PortugalComplianceInfo.TaxMessageGroupCodes.M19,
				PortugalComplianceInfo.TaxMessageGroupCodes.M20, PortugalComplianceInfo.TaxMessageGroupCodes.M21,
				PortugalComplianceInfo.TaxMessageGroupCodes.M25, PortugalComplianceInfo.TaxMessageGroupCodes.M30,
				PortugalComplianceInfo.TaxMessageGroupCodes.M31, PortugalComplianceInfo.TaxMessageGroupCodes.M32,
				PortugalComplianceInfo.TaxMessageGroupCodes.M33, PortugalComplianceInfo.TaxMessageGroupCodes.M40,
				PortugalComplianceInfo.TaxMessageGroupCodes.M41, PortugalComplianceInfo.TaxMessageGroupCodes.M42,
				PortugalComplianceInfo.TaxMessageGroupCodes.M43, PortugalComplianceInfo.TaxMessageGroupCodes.M99 };

			var inactivePortugalAccInvMsgCodes = new ZString[] { PortugalComplianceInfo.TaxMessageGroupCodes.M00,
				PortugalComplianceInfo.TaxMessageGroupCodes.M03, PortugalComplianceInfo.TaxMessageGroupCodes.M08,
				PortugalComplianceInfo.TaxMessageGroupCodes.M11, PortugalComplianceInfo.TaxMessageGroupCodes.M12,
				PortugalComplianceInfo.TaxMessageGroupCodes.M13, PortugalComplianceInfo.TaxMessageGroupCodes.M14,
				PortugalComplianceInfo.TaxMessageGroupCodes.M15,  };

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", activePortugalAccInvMsgCodes, resultList.Where(x => x.Bool).Select(y => y.Code));
			AssertContainsExactElementsInAnyOrder("inactive elements", inactivePortugalAccInvMsgCodes, resultList.Where(x => !x.Bool).Select(y => y.Code));
		}

		public void TestTaxMessageGroupsManagementDefaultValueForArgentina()
		{
			var factory = new BusinessObjectFactory();
			var argentinaCompany1 = factory.NewWithValidTestData<GlbCompany>();
			argentinaCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Argentina;

			var argentinaCompany2 = factory.NewWithValidTestData<GlbCompany>();
			argentinaCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Argentina;

			var nonArgentinaCompany = factory.NewWithValidTestData<GlbCompany>();
			nonArgentinaCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(argentinaCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(argentinaCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements must be the same", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonArgentinaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(argentinaCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements must be the same", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var activeArgentinaAccInvMsgCodes = new ZString[] { ArgentinaComplianceInfo.TaxMessageGroupCodes.N1, ArgentinaComplianceInfo.TaxMessageGroupCodes.N2,
				ArgentinaComplianceInfo.TaxMessageGroupCodes.N3, ArgentinaComplianceInfo.TaxMessageGroupCodes.N4,
				ArgentinaComplianceInfo.TaxMessageGroupCodes.N5, ArgentinaComplianceInfo.TaxMessageGroupCodes.N6 };
			var inactiveArgentinaAccInvMsgCodes = new ZString[] { ArgentinaComplianceInfo.TaxMessageGroupCodes.N8, ArgentinaComplianceInfo.TaxMessageGroupCodes.N9 };

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", activeArgentinaAccInvMsgCodes, resultList.Where(x => x.Bool).Select(y => y.Code));
			AssertContainsExactElementsInAnyOrder("inactive elements", inactiveArgentinaAccInvMsgCodes, resultList.Where(x => !x.Bool).Select(y => y.Code));
		}

		public void TestTaxMessageGroupsManagementDefaultValueForHungary()
		{
			var factory = new BusinessObjectFactory();
			var hungaryCompany1 = factory.NewWithValidTestData<GlbCompany>();
			hungaryCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Hungary;

			var hungaryCompany2 = factory.NewWithValidTestData<GlbCompany>();
			hungaryCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Hungary;

			var nonHungaryCompany = factory.NewWithValidTestData<GlbCompany>();
			nonHungaryCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(hungaryCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(hungaryCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonHungaryCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(hungaryCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", new ZString[] {
																						HungaryComplianceInfo.TaxMessageGroupCodes.H01
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H02
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H03
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H05
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H06
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H21
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H22
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H23
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H24
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H25
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H26
																						, HungaryComplianceInfo.TaxMessageGroupCodes.H00
																					}
																	, resultList.Where(x => x.Bool).Select(y => y.Code));

			AssertContainsExactElementsInAnyOrder("inactive elements", new ZString[] {
																						HungaryComplianceInfo.TaxMessageGroupCodes.H04
																					 }
																	, resultList.Where(x => !x.Bool).Select(y => y.Code));
		}

		public void TestTaxMessageGroupsManagementDefaultValueForEgypt()
		{
			var factory = new BusinessObjectFactory();
			var egyptCompany1 = factory.NewWithValidTestData<GlbCompany>();
			egyptCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Egypt;

			var egyptCompany2 = factory.NewWithValidTestData<GlbCompany>();
			egyptCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Egypt;

			var nonEgyptCompany = factory.NewWithValidTestData<GlbCompany>();
			nonEgyptCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(egyptCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(egyptCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonEgyptCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(egyptCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", new ZString[] {
					EgyptComplianceInfo.TaxMessageGroupCodes.E00,
					EgyptComplianceInfo.TaxMessageGroupCodes.E01,
					EgyptComplianceInfo.TaxMessageGroupCodes.E02,
					EgyptComplianceInfo.TaxMessageGroupCodes.E03,
					EgyptComplianceInfo.TaxMessageGroupCodes.E04,
					EgyptComplianceInfo.TaxMessageGroupCodes.E05,
					EgyptComplianceInfo.TaxMessageGroupCodes.E06,
					EgyptComplianceInfo.TaxMessageGroupCodes.E07,
					EgyptComplianceInfo.TaxMessageGroupCodes.E08,
					EgyptComplianceInfo.TaxMessageGroupCodes.E09,
					EgyptComplianceInfo.TaxMessageGroupCodes.E10,
					EgyptComplianceInfo.TaxMessageGroupCodes.N01,
					EgyptComplianceInfo.TaxMessageGroupCodes.N02,
					EgyptComplianceInfo.TaxMessageGroupCodes.T2
				}
				, resultList.Where(x => x.Bool).Select(y => y.Code));

			AssertEquals("inactive elements", 0, resultList.Where(x => !x.Bool).Count());
		}

		public void TestTaxMessageGroupsManagementDefaultValueForSpain()
		{
			var factory = new BusinessObjectFactory();
			var spainCompany1 = factory.NewWithValidTestData<GlbCompany>();
			spainCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Spain;

			var spainCompany2 = factory.NewWithValidTestData<GlbCompany>();
			spainCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Spain;

			var nonSpainCompany = factory.NewWithValidTestData<GlbCompany>();
			nonSpainCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(spainCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(spainCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonSpainCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(spainCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", new ZString[] {
					SpainComplianceInfo.TaxMessageGroupCodes.A
					, SpainComplianceInfo.TaxMessageGroupCodes.B
					, SpainComplianceInfo.TaxMessageGroupCodes.C
					, SpainComplianceInfo.TaxMessageGroupCodes.D
					, SpainComplianceInfo.TaxMessageGroupCodes.E
					, SpainComplianceInfo.TaxMessageGroupCodes.F
					, SpainComplianceInfo.TaxMessageGroupCodes.G
					, SpainComplianceInfo.TaxMessageGroupCodes.H
					, SpainComplianceInfo.TaxMessageGroupCodes.I
					, SpainComplianceInfo.TaxMessageGroupCodes.J
					, SpainComplianceInfo.TaxMessageGroupCodes.K
					, SpainComplianceInfo.TaxMessageGroupCodes.L
					, SpainComplianceInfo.TaxMessageGroupCodes.M
					, SpainComplianceInfo.TaxMessageGroupCodes.X
				}
				, resultList.Where(x => x.Bool).Select(y => y.Code));

			AssertEquals("inactive elements", 0, resultList.Where(x => !x.Bool).Count());
		}

		public void TestTaxMessageGroupsManagementDefaultValueForTurkey()
		{
			var factory = new BusinessObjectFactory();
			var turkeyCompany1 = factory.NewWithValidTestData<GlbCompany>();
			turkeyCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Turkey;

			var turkeyCompany2 = factory.NewWithValidTestData<GlbCompany>();
			turkeyCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Turkey;

			var nonTurkeyCompany = factory.NewWithValidTestData<GlbCompany>();
			nonTurkeyCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Albania;

			factory.Save();

			var result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(turkeyCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(turkeyCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonTurkeyCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(turkeyCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", new ZString[] {
				TurkeyComplianceInfo.TaxMessageGroupCodes.Code201
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code202
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code204
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code205
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code206
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code207
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code208
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code209
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code211
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code212
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code213
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code214
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code215
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code216
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code217
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code218
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code219
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code220
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code221
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code223
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code225
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code226
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code227
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code228
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code229
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code230
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code231
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code232
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code234
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code235
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code236
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code237
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code238
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code239
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code240
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code241
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code242
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code250
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code301
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code302
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code303
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code304
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code305
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code306
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code307
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code308
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code309
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code310
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code311
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code312
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code313
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code314
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code315
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code316
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code317
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code318
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code319
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code320
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code321
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code322
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code323
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code324
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code325
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code326
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code327
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code328
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code330
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code331
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code332
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code333
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code334
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code335
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code336
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code337
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code338
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code339
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code340
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code341
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code350
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code351
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code601
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code602
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code603
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code604
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code605
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code606
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code607
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code608
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code609
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code610
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code611
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code612
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code613
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code614
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code615
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code616
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code617
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code618
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code619
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code620
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code621
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code622
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code623
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code624
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code625
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code626
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code627
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code801
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code802
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code803
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code804
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code805
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code806
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code807
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code808
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code809
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code810
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code811
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code812
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code813
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code814
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code815
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code816
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code817
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code818
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code819
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code820
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code821
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code822
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code823
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code824
				, TurkeyComplianceInfo.TaxMessageGroupCodes.Code825
				}
				, resultList.Where(x => x.Bool).Select(y => y.Code));

			AssertEquals("inactive elements", 0, resultList.Where(x => !x.Bool).Count());
		}

		public void TestTaxMessageGroupsManagementDefaultValueForSaudiArabia()
		{
			var factory = new BusinessObjectFactory();
			var saudiArabiaCompany1 = factory.NewWithValidTestData<GlbCompany>();
			saudiArabiaCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.SaudiArabia;

			var saudiArabiaCompany2 = factory.NewWithValidTestData<GlbCompany>();
			saudiArabiaCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.SaudiArabia;

			var nonSaudiArabiaCompany = factory.NewWithValidTestData<GlbCompany>();
			nonSaudiArabiaCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(saudiArabiaCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(saudiArabiaCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonSaudiArabiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(saudiArabiaCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Number of elements", expectedCollection.Count(), result.Count);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", new ZString[] {
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N29,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N297,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N30,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N32,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N33,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N341,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N342,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N343,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N344,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N345,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N35,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.N36,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.EDU,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.HEA,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.NON,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.OOS,
				SaudiArabiaComplianceInfo.TaxMessageGroupCodes.MIL,
				}
				, resultList.Where(x => x.Bool).Select(y => y.Code));

			AssertEquals("inactive elements", 0, resultList.Where(x => !x.Bool).Count());
		}

		public void TestTaxMessageGroupsManagementDefaultValueForPanama()
		{
			var factory = new BusinessObjectFactory();
			var panamaCompany1 = factory.NewWithValidTestData<GlbCompany>();
			panamaCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Panama;

			var panamaCompany2 = factory.NewWithValidTestData<GlbCompany>();
			panamaCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Panama;

			var nonPanamaCompany = factory.NewWithValidTestData<GlbCompany>();
			nonPanamaCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Algeria;

			factory.Save();

			var result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(panamaCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var expectedCollection = CountryComplianceFactory.GetITaxMessageGroupProvider(panamaCompany1.GC_RN_NKCountryCode)?.GetTaxMessageGroup().ToList<CodeDescriptionBoolRelatedItem>()
									.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description.ToString()));
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(nonPanamaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("Collection should be empty", 0, result.Count);

			result = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetValueWithoutFallback(panamaCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertContainsExactElementsInAnyOrder("Individual elements are same as well", expectedCollection, result);

			var activePanamaAccInvMsgCodes = new ZString[] { PanamaComplianceInfo.TaxMessageGroupCodes.N1, PanamaComplianceInfo.TaxMessageGroupCodes.N2,
				PanamaComplianceInfo.TaxMessageGroupCodes.N3 };
			var inactivePanamaAccInvMsgCodes = new ZString[] { PanamaComplianceInfo.TaxMessageGroupCodes.N4 };

			var resultList = result.ToList<CodeDescriptionBoolRelatedItem>();
			AssertContainsExactElementsInAnyOrder("active elements", activePanamaAccInvMsgCodes, resultList.Where(x => x.Bool).Select(y => y.Code));
			AssertContainsExactElementsInAnyOrder("inactive elements", inactivePanamaAccInvMsgCodes, resultList.Where(x => !x.Bool).Select(y => y.Code));
		}

		public void TestSystemDefinedMaximumAllowedTransactionAmount()
		{
			var registry = ItemSet.SystemDefinedMaximumAllowedTransactionAmount;
			AssertEquals("Name", "SystemDefinedMaximumAllowedTransactionAmount", registry.Name);
			AssertEquals("Category", "Accounting", registry.Category);
			AssertEquals("Caption", "System Defined Maximum Allowed Transaction Amount (CargoWise Support Only)", registry.Caption);
			AssertEquals("Hint", @"This registry define the maximum allowed transaction header and line amount acceptable by the system for accounting transactions posting.
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
- General Ledger Journals", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, registry.Options);
			AssertEquals("Default MaximumAllowedLineAmount", 100000000000M, registry.DefaultValue.MaximumAllowedLineAmount);
			AssertEquals("Default MaximumAllowedHeaderAmount", 1000000000000M, registry.DefaultValue.MaximumAllowedHeaderAmount);

			var oldValue = registry.DefaultValue;
			var newValue = new MaximumAllowedTransactionAmount();
			newValue.MaximumAllowedHeaderAmount = 12M;
			newValue.MaximumAllowedLineAmount = 10M;
			var expectedLogMessage = $"Maximum Allowed Header Amount has been changed to 12.\r\nMaximum Allowed Line Amount has been changed to 10.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestMaximumAllowedTransactionAmount()
		{
			var registry = ItemSet.MaximumAllowedTransactionAmount;
			AssertEquals("Name", "MaximumAllowedTransactionAmount", registry.Name);
			AssertEquals("Category", "Accounting", registry.Category);
			AssertEquals("Caption", "Maximum Allowed Transaction Amount", registry.Caption);
			AssertEquals("Hint", @"This registry enables you to define the maximum allowed transaction header and line amount acceptable in accordance to you business norms.
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
- General Ledger Journals", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.Default, registry.Options);
			AssertEquals("Default MaximumAllowedLineAmount", 100000000000M, registry.DefaultValue.MaximumAllowedLineAmount);
			AssertEquals("Default MaximumAllowedHeaderAmount", 1000000000000M, registry.DefaultValue.MaximumAllowedHeaderAmount);

			var oldValue = registry.DefaultValue;
			var newValue = new MaximumAllowedTransactionAmount();
			newValue.MaximumAllowedHeaderAmount = 12M;
			newValue.MaximumAllowedLineAmount = 10M;
			var expectedLogMessage = $"Maximum Allowed Header Amount has been changed to 12.\r\nMaximum Allowed Line Amount has been changed to 10.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestExcludeCurrencyFromCFXCalculation()
		{
			AssertEquals("No DefaultValue", 0, ItemSet.ExcludeCurrencyFromCFXCalculation.DefaultValue.Count);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.ExcludeCurrencyFromCFXCalculation.Storage);
			AssertEquals("Option", RegistryOptions.IsHidden, ItemSet.ExcludeCurrencyFromCFXCalculation.Options);
		}

		#region ARAP Default Tax Recognition

		public void TestARDefaultTaxRecognitionRule()
		{
			AssertEquals("Name", "ARDefaultTaxRecognitionRule", ItemSet.ARDefaultTaxRecognitionRule.Name);
			AssertEquals("Category", "Master Data/Organizations/Default Values", ItemSet.ARDefaultTaxRecognitionRule.Category);
			AssertEquals("Caption", "Receivables  Default Tax Recognition Rule", ItemSet.ARDefaultTaxRecognitionRule.Caption);
			AssertEquals("Hint", @"This registry allows you to specify the Receivables Organization Default Tax Recognition with reference to the current login company's country/region and the receivables organization's country/region.", ItemSet.ARDefaultTaxRecognitionRule.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ARDefaultTaxRecognitionRule.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ARDefaultTaxRecognitionRule.Options);
		}

		public void TestARDefaultTaxRecognitionRuleDefaultValue()
		{
			var collection = ItemSet.ARDefaultTaxRecognitionRule.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(4, collection.Count);
			AssertARAPDefaultTaxRecognitionRule(collection[0], "IEU", "IEU", "DEF");
			AssertARAPDefaultTaxRecognitionRule(collection[1], "IEU", "OEU", "DEF");
			AssertARAPDefaultTaxRecognitionRule(collection[2], "OEU", "SAL", "DEF");
			AssertARAPDefaultTaxRecognitionRule(collection[3], "OEU", "DTL", "DEF");
		}

		public void TestAPDefaultTaxRecognitionRule()
		{
			AssertEquals("Name", "APDefaultTaxRecognitionRule", ItemSet.APDefaultTaxRecognitionRule.Name);
			AssertEquals("Category", "Master Data/Organizations/Default Values", ItemSet.APDefaultTaxRecognitionRule.Category);
			AssertEquals("Caption", "Payables Default Tax Recognition Rule", ItemSet.APDefaultTaxRecognitionRule.Caption);
			AssertEquals("Hint", @"This registry allows you to specify the Payables Organization Default Tax Recognition with reference to the current login company's country/region and the payables organization's country/region.", ItemSet.APDefaultTaxRecognitionRule.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.APDefaultTaxRecognitionRule.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.APDefaultTaxRecognitionRule.Options);
		}

		public void TestAPDefaultTaxRecognitionRuleDefaultValue()
		{
			var collection = ItemSet.APDefaultTaxRecognitionRule.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(4, collection.Count);
			AssertARAPDefaultTaxRecognitionRule(collection[0], "IEU", "IEU", "DEF");
			AssertARAPDefaultTaxRecognitionRule(collection[1], "IEU", "OEU", "NON");
			AssertARAPDefaultTaxRecognitionRule(collection[2], "OEU", "SAL", "DEF");
			AssertARAPDefaultTaxRecognitionRule(collection[3], "OEU", "DTL", "NON");
		}

		void AssertARAPDefaultTaxRecognitionRule(ARAPDefaultTaxRecognitionRule rule, string expectedLoginCountry, string expectedOrgCountry, string expectedTaxRecognitionCode)
		{
			AssertEquals(expectedLoginCountry, rule.LoginCompanyCountryRuleCode);
			AssertEquals(expectedOrgCountry, rule.OrganizationCountryRuleCode);
			AssertEquals(expectedTaxRecognitionCode, rule.TaxRecognitionCode);
		}

		#endregion

		#region Cash Flow Related Configuration

		public void TestCashFlowActivityConfigurationDefaultValue()
		{
			CashFlowActivityConfigurationCollection collection = ItemSet.CashFlowActivityConfiguration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(23, collection.Count);
			AssertCashFlowActivityConfiguration(collection[0], "XXX", "X");
			AssertCashFlowActivityConfiguration(collection[1], "NON", "N");
			AssertCashFlowActivityConfiguration(collection[2], "CSH", "C");
			AssertCashFlowActivityConfiguration(collection[3], "EXX", "E");
			AssertCashFlowActivityConfiguration(collection[4], "O01", "O");
			AssertCashFlowActivityConfiguration(collection[5], "O02", "O");
			AssertCashFlowActivityConfiguration(collection[6], "O03", "O");
			AssertCashFlowActivityConfiguration(collection[7], "O04", "O");
			AssertCashFlowActivityConfiguration(collection[8], "O05", "O");
			AssertCashFlowActivityConfiguration(collection[9], "O06", "O");
			AssertCashFlowActivityConfiguration(collection[10], "I01", "I");
			AssertCashFlowActivityConfiguration(collection[11], "I02", "I");
			AssertCashFlowActivityConfiguration(collection[12], "I03", "I");
			AssertCashFlowActivityConfiguration(collection[13], "I04", "I");
			AssertCashFlowActivityConfiguration(collection[14], "I05", "I");
			AssertCashFlowActivityConfiguration(collection[15], "F01", "F");
			AssertCashFlowActivityConfiguration(collection[16], "F02", "F");
			AssertCashFlowActivityConfiguration(collection[17], "F03", "F");
			AssertCashFlowActivityConfiguration(collection[18], "F04", "F");
			AssertCashFlowActivityConfiguration(collection[19], "F05", "F");
			AssertCashFlowActivityConfiguration(collection[20], "F06", "F");
			AssertCashFlowActivityConfiguration(collection[21], "F07", "F");
			AssertCashFlowActivityConfiguration(collection[22], "F08", "F");
		}

		void AssertCashFlowActivityConfiguration(CashFlowActivityConfiguration rule, string expectedCode, string expectedActivityType)
		{
			AssertEquals(expectedCode, rule.Code);
			AssertEquals(expectedActivityType, rule.ActivityType);
		}

		public void TestSetValueForCashFlowActivityConfiguration()
		{
			CashFlowActivityConfigurationCollection collection = new CashFlowActivityConfigurationCollection();
			CashFlowActivityConfiguration item = collection.AddNew();
			item.Code = "ABC";
			item.EnglishDescription = "Test ABC";
			item.ActivityType = "O";
			ItemSet.CashFlowActivityConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals(item.Code, ItemSet.CashFlowActivityConfiguration.Value[0].Code);
			AssertEquals(item.Description, ItemSet.CashFlowActivityConfiguration.Value[0].Description);
			AssertEquals(item.ActivityType, ItemSet.CashFlowActivityConfiguration.Value[0].ActivityType);
		}

		public void TestCashFlowActivityConfiguration()
		{
			AssertEquals("Name", "CashFlowActivityConfiguration", ItemSet.CashFlowActivityConfiguration.Name);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Cash Flow", ItemSet.CashFlowActivityConfiguration.Category);
			AssertEquals("Caption", "Cash Flow Activity Configuration", ItemSet.CashFlowActivityConfiguration.Caption);
			AssertEquals("Hint", @"This registry enables you to configure cash flow type", ItemSet.CashFlowActivityConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.CashFlowActivityConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CashFlowActivityConfiguration.Options);
		}

		public void TestCashFlowCategoryBasedOnDebtorGroup()
		{
			AssertEquals("Name", "CashFlowCategoryBasedOnDebtorGroup", ItemSet.CashFlowCategoryBasedOnDebtorGroup.Name);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Cash Flow", ItemSet.CashFlowCategoryBasedOnDebtorGroup.Category);
			AssertEquals("Caption", "Cash Flow Based On Debtor Group", ItemSet.CashFlowCategoryBasedOnDebtorGroup.Caption);
			AssertEquals("Hint", @"This registry enables you to assign default cash flow categorization against each debtor group", ItemSet.CashFlowCategoryBasedOnDebtorGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.CashFlowCategoryBasedOnDebtorGroup.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CashFlowCategoryBasedOnDebtorGroup.Options);
		}

		public void TestCashFlowCategoryBasedOnCreditorGroup()
		{
			AssertEquals("Name", "CashFlowCategoryBasedOnCreditorGroup", ItemSet.CashFlowCategoryBasedOnCreditorGroup.Name);
			AssertEquals("Category", "Accounting/General Ledger Defaults/Cash Flow", ItemSet.CashFlowCategoryBasedOnCreditorGroup.Category);
			AssertEquals("Caption", "Cash Flow Based On Creditor Group", ItemSet.CashFlowCategoryBasedOnCreditorGroup.Caption);
			AssertEquals("Hint", @"This registry enables you to assign default cash flow categorization against each creditor group", ItemSet.CashFlowCategoryBasedOnCreditorGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.CashFlowCategoryBasedOnCreditorGroup.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CashFlowCategoryBasedOnCreditorGroup.Options);
		}

		public void TestOnBuildLogReferenceForCashFlowActivityConfiguration()
		{
			IRegistryItem regItem = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Inner;

			CashFlowActivityConfigurationCollection collection1 = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowActivityConfigurationCollection collection2 = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowActivityConfiguration item = collection2.AddNew();
			item.Code = "ABC";
			item.EnglishDescription = "Test ABC";
			item.ActivityType = "O";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			var result = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.OnBuildLogReference(args);
			AssertEquals("Added: Code [ABC] - Activity Type [O]\r\n", result);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection2, collection1);
			result = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.OnBuildLogReference(args);
			AssertEquals("Deleted: Code [ABC] - Activity Type [O]\r\n", result);

			((CashFlowActivityConfiguration)(collection2.FirstOrDefault())).ActivityType = "F";
			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			result = AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.OnBuildLogReference(args);
			AssertEquals("Added: Code [ABC] - Activity Type [O]\r\nUpdated: Code [XXX] - Activity Type from [X] to [F]\r\n", result);
		}

		public void TestOnBuildLogReferenceForCashFlowCategoryBasedOnCreditorGroup()
		{
			IRegistryItem regItem = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.Inner;

			CashFlowCategoryBasedOnCreditorGroupCollection collection1 = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowCategoryBasedOnCreditorGroupCollection collection2 = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowCategoryBasedOnCreditorGroupCollection collection3 = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowCategoryBasedOnCreditorGroup item = collection2.AddNew();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var orgGroup = factory.LoadTop1<OrgCreditorGroup>(new ZQuery(OrgCreditorGroupSchema.OG_Code, "ASC"));
			item.OrgGroupPK = orgGroup.PK;
			item.CashFlowCategory = "O02";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			var result = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.OnBuildLogReference(args);
			AssertEquals("Added: Creditor Group [ASSOCIATED COMPANY] - Cash Flow Category [O02]\r\n", result);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection2, collection1);
			result = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.OnBuildLogReference(args);
			AssertEquals("Deleted: Creditor Group [ASSOCIATED COMPANY] - Cash Flow Category [O02]\r\n", result);

			item = collection3.AddNew();
			item.OrgGroupPK = orgGroup.PK;
			item.CashFlowCategory = "O03";
			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection2, collection3);
			result = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.OnBuildLogReference(args);
			AssertEquals("Updated: Creditor Group [ASSOCIATED COMPANY] - Cash Flow Category changed from [O02] to [O03]\r\n", result);
		}

		public void TestOnBuildLogReferenceForCashFlowCategoryBasedOnDebtorGroup()
		{
			IRegistryItem regItem = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.Inner;

			CashFlowCategoryBasedOnDebtorGroupCollection collection1 = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowCategoryBasedOnDebtorGroupCollection collection2 = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowCategoryBasedOnDebtorGroupCollection collection3 = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			CashFlowCategoryBasedOnDebtorGroup item = collection2.AddNew();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var orgGroup = factory.LoadTop1<OrgDebtorGroup>(new ZQuery(OrgDebtorGroupSchema.OJ_Code, "ASC"));
			item.OrgGroupPK = orgGroup.PK;
			item.CashFlowCategory = "O02";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			var result = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.OnBuildLogReference(args);
			AssertEquals("Added: Debtor Group [ASSOCIATED COMPANY] - Cash Flow Category [O02]\r\n", result);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection2, collection1);
			result = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.OnBuildLogReference(args);
			AssertEquals("Deleted: Debtor Group [ASSOCIATED COMPANY] - Cash Flow Category [O02]\r\n", result);

			item = collection3.AddNew();
			item.OrgGroupPK = orgGroup.PK;
			item.CashFlowCategory = "O03";
			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection2, collection3);
			result = AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.OnBuildLogReference(args);
			AssertEquals("Updated: Debtor Group [ASSOCIATED COMPANY] - Cash Flow Category changed from [O02] to [O03]\r\n", result);
		}

		#endregion

		#region Gateway Consol Job Invoicing

		public void TestGatewayChargeDefaultDebtorConfiguration()
		{
			AssertEquals("Name", "Gateway Charge Default Debtor Configuration", ItemSet.GatewayChargeDefaultDebtorConfiguration.Name);
			AssertEquals("Category", "Accounting/Job Invoicing/Gateway Consol Job Invoicing", ItemSet.GatewayChargeDefaultDebtorConfiguration.Category);
			AssertEquals("Caption", "Gateway Charge Default Debtor Configuration", ItemSet.GatewayChargeDefaultDebtorConfiguration.Caption);
			AssertEquals("Hint", @"This registry allows you to configure debtor defaulting rules for Gateway Consol Billing Jobs.
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
PSA - When gateway job charge is related to a shipment.then you can set default debtor to the Sending Agent of the shipment's previous consol. Previous consol is a consol from the list of consols attached to the shipment, which is preceding the current gateway consol on the shipment's journey.", ItemSet.GatewayChargeDefaultDebtorConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.GatewayChargeDefaultDebtorConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.GatewayChargeDefaultDebtorConfiguration.Options);
		}

		public void TestGatewayChargeDefaultDebtorConfigurationDefaultValue()
		{
			var collection = ItemSet.GatewayChargeDefaultDebtorConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals(11, collection.Count);
			AssertGatewayChargeDefaultDebtorConfiguration(collection[0], "ALL", "PPD", "SGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[1], "ALL", "CCX", "RGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[2], "LOD", "ALL", "SGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[3], "ORG", "ALL", "SGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[4], "OBR", "ALL", "SGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[5], "OBO", "ALL", "SGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[6], "DST", "ALL", "RGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[7], "UNL", "ALL", "RGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[8], "BRK", "ALL", "RGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[9], "BON", "ALL", "RGT");
			AssertGatewayChargeDefaultDebtorConfiguration(collection[10], "CDS", "ALL", "RGT");
		}

		void AssertGatewayChargeDefaultDebtorConfiguration(GatewayChargeDefaultDebtorConfiguration configuration, ZString expectedChargeGroup, ZString expectedConsolPaymentTerm, ZString expectedDebtor)
		{
			AssertEquals("Charge Group", expectedChargeGroup, configuration.ChargeGroup);
			AssertEquals("Consol Payment Term", expectedConsolPaymentTerm, configuration.ConsolPaymentTerm);
			AssertEquals("Debtor", expectedDebtor, configuration.Debtor);
		}

		public void TestGatewayChargeDefaultInvoiceTargetJobConfiguration()
		{
			AssertEquals("Name", "Gateway Charge Default Invoice Target Job Configuration", ItemSet.GatewayChargeDefaultInvoiceTargetJobConfiguration.Name);
			AssertEquals("Category", "Accounting/Job Invoicing/Gateway Consol Job Invoicing", ItemSet.GatewayChargeDefaultInvoiceTargetJobConfiguration.Category);
			AssertEquals("Caption", "Intercompany Invoice Target Job Configuration", ItemSet.GatewayChargeDefaultInvoiceTargetJobConfiguration.Caption);
			AssertEquals("Hint", @"This registry allows you to configure defaulting rules for Intercompany Invoice Target Job on gateway billing charges.
Intercompany Invoice Target Job can be used when charge debtor is a sister company organization proxy and when Related Job Number is selected. During intercompany import of the gateway charge, the cost is created on a job recorded as 'Intercompany Invoice Target Job'.

By default, all intercompany charges 'target' the same job, i.e. the same consol.
Using this registry, you can set the target job to be:

REL - Related Shipment. Gateway charge will be imported as cost on the shipment.
SCL - Same Consol. Gateway charge will be imported as cost on the same consol, in the receiving company.
PCL - Previous Consol. Gateway charge will be imported as cost on the related shipment's previous consol. Previous consol is a consol in the list of consols attached to the shipment, which is preceding the current gateway consol on the shipment's journey.

You can add a separate defaulting rule, depending on the related shipment's Previous Sending Agent. Previous Sending Agent checks the Sending Agent of the shipment's previous consol. Previous Sending Agent can be a Gateway Agent (GTA), Gateway Agent with Tariff (GTT) or non-gateway agent (SGT). Previous consol is a consol from the list of consols attached to the shipment, which is preceding the current gateway consol on the shipment's journey.

For example, where Previous Sending Agent is a gateway agent GTA or GTT, you may set invoice target to be Previous Consol, so that the charge gets imported to the preceding gateway agent's gateway consol billing job. Otherwise, if previous sending agent is not a gateway agent, then you may wish to set invoice target to be the related Shipment itself.", ItemSet.GatewayChargeDefaultInvoiceTargetJobConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.GatewayChargeDefaultInvoiceTargetJobConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.GatewayChargeDefaultInvoiceTargetJobConfiguration.Options);
			Assert("Default value should be empty", !ItemSet.GatewayChargeDefaultInvoiceTargetJobConfiguration.DefaultValue.Any());
		}

		public void TestGetInternalJobConfigurationSetting()
		{
			AssertEquals("Name", "GetInternalJobConfigurationSetting", ItemSet.GetInternalJobConfigurationSetting.Name);
			AssertEquals("Category", "Accounting/Job Invoicing/Gateway Consol Job Invoicing", ItemSet.GetInternalJobConfigurationSetting.Category);
			AssertEquals("Caption", "Use Intercompany Invoice Target Rules to Set Internal Job for Internal Charges", ItemSet.GetInternalJobConfigurationSetting.Caption);
			AssertEquals("Hint", @"When this registry is set to Yes, if Auto Job Revenue Journals are enabled, and a gateway charge’s debtor is an organization proxy in the same login company, rather than defaulting Related Job (shipment) as the Internal Job, use the Intercompany Invoice Target Job Configuration registry to determine the Internal Job.", ItemSet.GetInternalJobConfigurationSetting.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.GetInternalJobConfigurationSetting.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.GetInternalJobConfigurationSetting.Options);
			AssertEquals("Default value should be true", true, ItemSet.GetInternalJobConfigurationSetting.DefaultValue);
		}

		#endregion

		#region compliance sub type
		public void TestComplianceSubTypeAttributionRuleConfigurationDefaultValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals(7, collection.Count);
				AssertComplianceSubTypeAttributionRule(collection[0], "PE", "TXI", "AR", "INV", "TID", "OTO", "ALL", "", "RUC");
				AssertComplianceSubTypeAttributionRule(collection[1], "PE", "TCR", "AR", "CRD", "TID", "ALL", "ALL", "", "RUC");
				AssertComplianceSubTypeAttributionRule(collection[2], "PE", "TCD", "AR", "INV", "TID", "ARO", "ALL", "", "RUC");
				AssertComplianceSubTypeAttributionRule(collection[3], "PE", "DSB", "AR", "INV", "EXL", "ALL", "ALL", "");
				AssertComplianceSubTypeAttributionRule(collection[4], "PE", "DSB", "AR", "CRD", "EXL", "ALL", "ALL", "");
				AssertComplianceSubTypeAttributionRule(collection[5], "PE", "TBO", "AR", "INV", "TID", "OTO", "ALL", "");
				AssertComplianceSubTypeAttributionRule(collection[6], "PE", "TBC", "AR", "CRD", "TID", "ALL", "ALL", "");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			{
				var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals(2, collection.Count);
				AssertComplianceSubTypeAttributionRule(collection[0], "VN", "TXI", "AR", "INV", "TXN", "OTO", "ALL", "VN");
				AssertComplianceSubTypeAttributionRule(collection[1], "VN", "TXI", "AR", "INV", "TXN", "OTO", "ALL", "");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals(0, collection.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ecuador))
			{
				var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals(7, collection.Count);
				AssertComplianceSubTypeAttributionRule(collection[0], "EC", "TXI", "AR", "INV", "TID", "OTO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[1], "EC", "TCR", "AR", "CRD", "TID", "OTO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[2], "EC", "TCR", "AR", "CRD", "TID", "ARO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[3], "EC", "TCD", "AR", "INV", "TID", "ARO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[4], "EC", "TXV", "AP", "INV", "RVS", "ALL", "ALL", "", "", "SBI", "", "");
				AssertComplianceSubTypeAttributionRule(collection[5], "EC", "XCL", "AR", "INV", "EXL", "ALL", "DSB", "");
				AssertComplianceSubTypeAttributionRule(collection[6], "EC", "XCL", "AR", "CRD", "EXL", "ALL", "DSB", "");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Guatemala))
			{
				var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals(7, collection.Count);
				AssertComplianceSubTypeAttributionRule(collection[0], "GT", "TCD", "AR", "INV", "TID", "ARO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[1], "GT", "TCR", "AR", "CRD", "TID", "OTO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[2], "GT", "TCR", "AR", "CRD", "TID", "ARO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[3], "GT", "TXI", "AR", "INV", "TID", "OTO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[4], "GT", "XCL", "AR", "INV", "EXL", "OTO", "DSB", "");
				AssertComplianceSubTypeAttributionRule(collection[5], "GT", "XCL", "AR", "INV", "EXL", "ARO", "DSB", "");
				AssertComplianceSubTypeAttributionRule(collection[6], "GT", "XCR", "AR", "CRD", "EXL", "ALL", "DSB", "");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Honduras))
			{
				var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals(6, collection.Count);
				AssertComplianceSubTypeAttributionRule(collection[0], "HN", "TCD", "AR", "INV", "TID", "ARO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[1], "HN", "TCR", "AR", "CRD", "TID", "OTO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[2], "HN", "TCR", "AR", "CRD", "TID", "ARO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[3], "HN", "TXI", "AR", "INV", "TID", "OTO", "NDB", "");
				AssertComplianceSubTypeAttributionRule(collection[4], "HN", "XCL", "AR", "INV", "EXL", "OTO", "DSB", "");
				AssertComplianceSubTypeAttributionRule(collection[5], "HN", "XCL", "AR", "CRD", "EXL", "ARO", "DSB", "");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.ElSalvador))
			{
				var expected = new[]
				{
					("SV", "TCD", "AR", "INV", "TID", "ARO", "ALL", "", "NRC", "", "", "", "NON"),
					("SV", "TCR", "AR", "CRD", "TID", "OTO", "ALL", "", "NRC", "", "", "", "NON"),
					("SV", "TCR", "AR", "CRD", "TID", "ARO", "ALL", "", "NRC", "", "", "", "NON"),
					("SV", "TXI", "AR", "INV", "TID", "OTO", "ALL", "", "NRC", "", "", "", "NON"),
					("SV", "TXN", "AR", "INV", "TID", "OTO", "ALL", "",  "" , "", "", "", "NON"),
					("SV", "TXE", "AR", "INV", "TID", "OTO", "ALL", "", "", "", "", "", "EXV"),
					("SV", "TCF", "AR", "CRD", "TID", "ARO", "ALL", "",  "" , "", "", "", "NON"),
					("SV", "TCF", "AR", "CRD", "TID", "ARO", "ALL", "",  "" , "", "", "", "EXV"),
				};

				AssertRules(expected);
			}
		}

		void AssertRules((string, string, string, string, string, string, string, string, string, string, string, string, string)[] expected)
		{
			var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var actual = collection
				.Cast<ComplianceSubTypeAttributionRuleConfiguration>()
				.Select(x => (x.Country, x.SubType, x.LedgerType, x.InvoiceType, x.TaxInvoiceRule, x.OriginalRule, x.DisbursementRule, x.OrganisationLocation, x.TaxRegistrationType, x.SelfBillingRule, x.TaxRegistrationLocationRule, x.VATGroupRule, x.ExporterExemption));

			AssertContainsExactElementsInAnyOrder(expected.Select(x => x.ToString()), actual.Select(x => x.ToString()));
		}

		void AssertComplianceSubTypeAttributionRule(ComplianceSubTypeAttributionRuleConfiguration rule, string expectedCountry, string expectedSubtype, string expectedLedgerType,
												string expectedInvoiceType, string expectedTaxInvoiceRule, string expectedOriginalRule, string expectedDisbursementRule, string expectedOrganisationLocation,
												string expectedTaxRegistrationType = "", string expectedSelfBillingRule = "", string expectedTaxRegistrationLocationRule = "", string expectedVATGroupRule = "")
		{
			AssertEquals("Country", expectedCountry, rule.Country);
			AssertEquals("Sub Type", expectedSubtype, rule.SubType);
			AssertEquals("Ledger Type", expectedLedgerType, rule.LedgerType);
			AssertEquals("Invoice Type", expectedInvoiceType, rule.InvoiceType);
			AssertEquals("Tax Invoice Rule", expectedTaxInvoiceRule, rule.TaxInvoiceRule);
			AssertEquals("Original/Amendment Rule", expectedOriginalRule, rule.OriginalRule);
			AssertEquals("Disbursement Transaction Rule", expectedDisbursementRule, rule.DisbursementRule);
			AssertEquals("Organisation Location Rule", expectedOrganisationLocation, rule.OrganisationLocation);
			AssertEquals("Tax Registration Type Rule", expectedTaxRegistrationType, rule.TaxRegistrationType);
			AssertEquals("Self Billing Rule", expectedSelfBillingRule, rule.SelfBillingRule);
			AssertEquals("Tax Registration Location Rule", expectedTaxRegistrationLocationRule, rule.TaxRegistrationLocationRule);
			AssertEquals("VAT Group Rule", expectedVATGroupRule, rule.VATGroupRule);
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration()
		{
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			ComplianceSubTypeAttributionRuleConfiguration item = collection.AddNew();
			item.Country = "PE";
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = "TID";
			item.DisbursementRule = "NDB";
			item.OriginalRule = "OTO";
			item.OrganisationLocation = "PE";
			item.TaxRegistrationType = "DNI";
			ItemSet.ComplianceSubTypeAttributionRuleConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals("Value", item.Country, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].Country);
			AssertEquals("Value", item.SubType, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].SubType);
			AssertEquals("Value", item.LedgerType, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].LedgerType);
			AssertEquals("Value", item.InvoiceType, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].InvoiceType);
			AssertEquals("Value", item.TaxInvoiceRule, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].TaxInvoiceRule);
			AssertEquals("Value", item.DisbursementRule, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].DisbursementRule);
			AssertEquals("Value", item.OriginalRule, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].OriginalRule);
			AssertEquals("Value", item.OrganisationLocation, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].OrganisationLocation);
			AssertEquals("Value", item.TaxRegistrationType, ItemSet.ComplianceSubTypeAttributionRuleConfiguration.Value[0].TaxRegistrationType);
		}

		public void TestComplianceReportConfiguration()
		{
			AssertEquals("Name", "ComplianceReportConfiguration", ItemSet.ComplianceReportConfiguration.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceReportConfiguration.Category);
			AssertEquals("Caption", "Compliance Report Configuration (CargoWiseOne Support Only)", ItemSet.ComplianceReportConfiguration.Caption);
			AssertEquals("Hint", @"This registry defines the Compliance Reporting Options available to a Login Company in the Compliance Reports Module.
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
The French ‘FEC’ report must have Code FEC.", ItemSet.ComplianceReportConfiguration.Hint);

			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceReportConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ComplianceReportConfiguration.Options);

			AssertEquals("Pre-condition - Country is AU", Constants.CountryCodes.Australia, Env.CurrentCompany.Country.Code);
			AssertEquals("DefaultValue is empty", 0, ItemSet.ComplianceReportConfiguration.DefaultValue.Count);

			AssertComplianceReportConfiguration(Constants.CountryCodes.UnitedKingdom, AssertComplianceReportConfigurationUnitedKingdom);
			AssertComplianceReportConfiguration(Constants.CountryCodes.Portugal, AssertComplianceReportConfigurationPortugal);
			AssertComplianceReportConfiguration(Constants.CountryCodes.Italy, AssertComplianceReportConfigurationItaly);
			AssertComplianceReportConfiguration(Constants.CountryCodes.Germany, AssertComplianceReportConfigurationGermany);
			AssertComplianceReportConfiguration(Constants.CountryCodes.France, AssertComplianceReportConfigurationFrance);
		}

		public void TestComplianceReportConfigurationRootFileExportPath()
		{
			AssertEquals("Name", "ComplianceReportConfigurationRootFileExportPath", ItemSet.ComplianceReportConfigurationRootFileExportPath.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document/Israel (IL)", ItemSet.ComplianceReportConfigurationRootFileExportPath.Category);
			AssertEquals("Caption", "Open Format - Root File Export Path (CargoWiseOne Support Only)", ItemSet.ComplianceReportConfigurationRootFileExportPath.Caption);
			AssertEquals("Hint", @"This setting defines the path for exporting the OP file structure (only relevant for Israel company).
With this setting, only the root is defined.

A basic folder with the name ‘OPENFRMT’ will be created on this path.
During the export process, a first subfolder (VAT.YY) will be created; VAT corresponds to the first 8 numbers of the VAT register; YY corresponds to the report's production year.
There will also be a subfolder (MMddHHmm) containing the files BKMVDATA.txt and INI.txt. ‘MMddHHmm’ is taken from the production date of the report.

Example, VAT = 512312115, production year 2022 at 05.01.2022 14:55 will be stored in \OPENFRM\51231211.22\01051455\", ItemSet.ComplianceReportConfigurationRootFileExportPath.Hint);

			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceReportConfigurationRootFileExportPath.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ComplianceReportConfigurationRootFileExportPath.Options);
			AssertEquals("CountryFilterPK", CountryFilterPKs.Israel, ItemSet.ComplianceReportConfigurationRootFileExportPath.CountryFilterPKs);
		}

		void AssertComplianceReportConfiguration(string countryCode, Action assertCountrySpecificComplianceReportConfiguration)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				assertCountrySpecificComplianceReportConfiguration();
			}
		}

		void AssertComplianceReportConfigurationUnitedKingdom()
		{
			var countryCode = Constants.CountryCodes.UnitedKingdom;
			var countryName = nameof(Constants.CountryCodes.UnitedKingdom);
			var defaultValue = ItemSet.ComplianceReportConfiguration.DefaultValue;
			AssertEquals($"{countryName} ({countryCode}) - incorrect number of reports set up", 1, defaultValue.Count);

			var reportConfig = defaultValue[0];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "MTD", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "UK VAT Return", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "VAT", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "TXR", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", true, reportConfig.IncludeQueuedForPreviousPeriod);

			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 6, reportConfig.Settings.Count);

			void AssertSetting(ComplianceReportConfigurationSetting setting, string ledger, string transactionType)
			{
				AssertEquals($"{countryName} ({countryCode}) - incorrect ComplianceSubType", string.Empty, setting.ComplianceSubType);
				AssertEquals($"{countryName} ({countryCode}) - incorrect LedgerType", ledger, setting.LedgerType);
				AssertEquals($"{countryName} ({countryCode}) - incorrect InvoiceType", transactionType, setting.InvoiceType);
				AssertEquals($"{countryName} ({countryCode}) - incorrect OriginalRule", "ALL", setting.OriginalRule);
				AssertEquals($"{countryName} ({countryCode}) - incorrect DisbursementRule", "ALL", setting.DisbursementRule);
				AssertEquals($"{countryName} ({countryCode}) - incorrect TaxInvoiceRule", "ALL", setting.TaxInvoiceRule);
			}

			AssertSetting(reportConfig.Settings[0], "AR", "INV");
			AssertSetting(reportConfig.Settings[1], "AR", "CRD");
			AssertSetting(reportConfig.Settings[2], "AP", "INV");
			AssertSetting(reportConfig.Settings[3], "AP", "CRD");
			AssertSetting(reportConfig.Settings[4], "CB", "DPY");
			AssertSetting(reportConfig.Settings[5], "CB", "DRC");
		}

		void AssertComplianceReportConfigurationPortugal()
		{
			var countryCode = Constants.CountryCodes.Portugal;
			var countryName = nameof(Constants.CountryCodes.Portugal);
			var defaultValue = ItemSet.ComplianceReportConfiguration.DefaultValue;
			AssertEquals($"{countryName} ({countryCode}) - incorrect number of reports set up", 2, defaultValue.Count);

			var reportConfig = defaultValue[0];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "SAF", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "SAFT (PT) File", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "IVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "PER", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "**", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "DBW", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 0, reportConfig.Settings.Count);
			AssertEquals($"{countryName} ({countryCode}) - incorrect default report type", false, reportConfig.IsDefaultReportType);

			reportConfig = defaultValue[1];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "SAT", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "SAFT (PT) File with transactions only", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "IVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "PER", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 14, reportConfig.Settings.Count);
			AssertEquals($"{countryName} ({countryCode}) - incorrect default report type", true, reportConfig.IsDefaultReportType);

			void AssertSetting(ComplianceReportConfigurationSetting setting, string ledger, string transactionType, string subType)
			{
				AssertEquals($"{countryName} ({countryCode}) - incorrect ComplianceSubType", subType, setting.ComplianceSubType);
				AssertEquals($"{countryName} ({countryCode}) - incorrect LedgerType", ledger, setting.LedgerType);
				AssertEquals($"{countryName} ({countryCode}) - incorrect InvoiceType", transactionType, setting.InvoiceType);
				AssertEquals($"{countryName} ({countryCode}) - incorrect OriginalRule", "ALL", setting.OriginalRule);
				AssertEquals($"{countryName} ({countryCode}) - incorrect DisbursementRule", "ALL", setting.DisbursementRule);
				AssertEquals($"{countryName} ({countryCode}) - incorrect TaxInvoiceRule", "ALL", setting.TaxInvoiceRule);
				AssertEquals($"{countryName} ({countryCode}) - incorrect ReportingDate", "INV", setting.ReportingDate);
			}

			AssertSetting(reportConfig.Settings[0], "AP", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI);
			AssertSetting(reportConfig.Settings[1], "AP", "CRD", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC);
			AssertSetting(reportConfig.Settings[2], "AP", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD);

			AssertSetting(reportConfig.Settings[3], "AR", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI);
			AssertSetting(reportConfig.Settings[4], "AR", "CRD", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR);
			AssertSetting(reportConfig.Settings[5], "AR", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD);
			AssertSetting(reportConfig.Settings[6], "AR", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL);
			AssertSetting(reportConfig.Settings[7], "AR", "CRD", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR);
			AssertSetting(reportConfig.Settings[8], "AR", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.TDM);
			AssertSetting(reportConfig.Settings[9], "AR", "CRD", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM);
			AssertSetting(reportConfig.Settings[10], "AR", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM);
			AssertSetting(reportConfig.Settings[11], "AR", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.LCD);
			AssertSetting(reportConfig.Settings[12], "AR", "CRD", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR);
			AssertSetting(reportConfig.Settings[13], "AR", "INV", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX);
		}

		void AssertComplianceReportConfigurationItaly()
		{
			var countryCode = Constants.CountryCodes.Italy;
			var countryName = nameof(Constants.CountryCodes.Italy);
			var defaultValue = ItemSet.ComplianceReportConfiguration.DefaultValue;
			AssertEquals($"{countryName} ({countryCode}) - incorrect number of reports set up", 5, defaultValue.Count);

			var reportConfig = defaultValue[0];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "EST", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Esterometro", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "IVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RepCountryRegistrationCode", "COD", reportConfig.RepCountryRegistrationCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "TXR", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);

			var repSettings = reportConfig.Settings.Cast<ComplianceReportConfigurationSetting>();
			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 2, repSettings.Count());

			void AssertSettingIT(string subType, string ledger = "", string invoiceType = "", string originalRule = "", string disbursementRule = "", string taxInvoice = "")
			{
				var setting = repSettings.FirstOrDefault(x => x.ComplianceSubType == subType && x.LedgerType == ledger && x.InvoiceType == invoiceType && x.OriginalRule == originalRule && x.DisbursementRule == disbursementRule && x.TaxInvoiceRule == taxInvoice);
				AssertNotNull($"{countryName} ({countryCode}) - incorrect Settings, must contain SubType = '{subType}' - LedgerType = '{ledger}' - InvoiceType = '{invoiceType}' - OriginalRule = '{originalRule}' - DisbursementRule = '{disbursementRule}' - TaxInvoiceRule = '{taxInvoice}'.", setting);

				AssertEquals($"{countryName} ({countryCode}) - TaxRegistrationLocationRule: {subType} - incorrect TaxRegistrationLocationRule", string.Empty, setting.TaxRegistrationLocationRule);
				AssertEquals($"{countryName} ({countryCode}) - OrganisationLocation: {subType} - incorrect OrganisationLocation", string.Empty, setting.OrganisationLocation);
			}

			AssertSettingIT("APS");
			AssertSettingIT("INT");

			reportConfig = defaultValue[1];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "ARC", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Registro IVA ARC", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "IVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RepCountryRegistrationCode", "COD", reportConfig.RepCountryRegistrationCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "TXR", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "CST", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);

			repSettings = reportConfig.Settings.Cast<ComplianceReportConfigurationSetting>();
			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 7, repSettings.Count());
			AssertSettingIT("INT");
			AssertSettingIT("INI");
			AssertSettingIT("ARS");
			AssertSettingIT("ARN");
			AssertSettingIT("ARI");
			AssertSettingIT("ARE");
			AssertSettingIT("APS");

			reportConfig = defaultValue[2];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "APC", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Registro IVA APC", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "IVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RepCountryRegistrationCode", "COD", reportConfig.RepCountryRegistrationCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "TXR", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "CST", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);

			repSettings = reportConfig.Settings.Cast<ComplianceReportConfigurationSetting>();
			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 5, repSettings.Count());
			AssertSettingIT("INT");
			AssertSettingIT("INI");
			AssertSettingIT("APV");
			AssertSettingIT("APS");
			AssertSettingIT("API");

			reportConfig = defaultValue[3];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "LBG", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Libro Giornale", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "IVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RepCountryRegistrationCode", "COD", reportConfig.RepCountryRegistrationCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "**", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "DAB", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "CST", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);

			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 0, reportConfig.Settings.Count);

			reportConfig = defaultValue[4];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "LIQ", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Liquidazione IVA", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "IVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RepCountryRegistrationCode", "COD", reportConfig.RepCountryRegistrationCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "TXR", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "LAS", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", true, reportConfig.IsDefaultReportType);

			repSettings = reportConfig.Settings.Cast<ComplianceReportConfigurationSetting>();
			AssertEquals($"{countryName} ({countryCode}) - incorrect Settings.Count", 6, repSettings.Count());

			AssertSettingIT("", "AR", "INV", "ALL", "ALL", "TID");
			AssertSettingIT("", "AR", "CRD", "ALL", "ALL", "TID");
			AssertSettingIT("", "AR", "ADJ", "ALL", "ALL", "TID");
			AssertSettingIT("", "AP", "INV", "ALL", "ALL", "TID");
			AssertSettingIT("", "AP", "CRD", "ALL", "ALL", "TID");
			AssertSettingIT("", "AP", "ADJ", "ALL", "ALL", "TID");
		}

		void AssertComplianceReportConfigurationGermany()
		{
			EnableAllGermanComplianceReports();
			var countryCode = Constants.CountryCodes.Germany;
			var countryName = nameof(Constants.CountryCodes.Germany);
			var defaultValue = ItemSet.ComplianceReportConfiguration.DefaultValue;
			AssertEquals($"{countryName} ({countryCode}) - incorrect number of reports set up", 4, defaultValue.Count);

			var reportConfig = defaultValue[0];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "UVA", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Umsatzsteuervoranmeldung", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "UST", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "HRS", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);

			reportConfig = defaultValue[1];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "U11", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Umsatzsteuer-Sondervorauszahlung", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "UST", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "RSH", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);

			reportConfig = defaultValue[2];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "ZMD", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Zusammenfassende Meldung", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "UST", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "MQY", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "AL", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", "HRS", reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);

			reportConfig = defaultValue[3];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "IDE", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "IDEA Tax Audit Export", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "UST", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "RNG", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "**", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineGrouping", ReportLineGroupingListCodes.DayBookWithPresentation, reportConfig.ReportLineGrouping);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IsDefaultReportType", false, reportConfig.IsDefaultReportType);
		}

		void AssertComplianceReportConfigurationFrance()
		{
			var countryCode = Constants.CountryCodes.France;
			var countryName = nameof(Constants.CountryCodes.France);
			var defaultValue = ItemSet.ComplianceReportConfiguration.DefaultValue;
			AssertEquals($"{countryName} ({countryCode}) - incorrect number of reports set up", 1, defaultValue.Count);

			var reportConfig = defaultValue[0];
			AssertEquals($"{countryName} ({countryCode}) - incorrect Country", countryCode, reportConfig.Country);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportCode", "FEC", reportConfig.ReportCode);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportTitle", "Fichier des Écritures Comptables", reportConfig.ReportTitle);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxRegistrationType", "TVA", reportConfig.TaxRegistrationType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportPeriodicity", "PRS", reportConfig.ReportPeriodicity);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportBaseTablePrefix", "**", reportConfig.ReportBaseTablePrefix);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportLineOrdering", "", reportConfig.ReportLineOrdering);
			AssertEquals($"{countryName} ({countryCode}) - incorrect GoodsServiceType", "", reportConfig.GoodsServiceType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingType", "", reportConfig.ReportAmountsRoundingType);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ReportAmountsRoundingTruncating", 0, reportConfig.ReportAmountsRoundingTruncating);
			AssertEquals($"{countryName} ({countryCode}) - incorrect AmountThresholdLevel", "", reportConfig.AmountThresholdLevel);
			AssertEquals($"{countryName} ({countryCode}) - incorrect ExTaxAmountThreshold", 0.0m, reportConfig.ExTaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect TaxAmountThreshold", 0.0m, reportConfig.TaxAmountThreshold);
			AssertEquals($"{countryName} ({countryCode}) - incorrect RecipientOrgPK", ZGuid.Empty, reportConfig.RecipientOrgPK);
			AssertEquals($"{countryName} ({countryCode}) - incorrect IncludeQueuedForPreviousPeriod", false, reportConfig.IncludeQueuedForPreviousPeriod);
		}

		public static void EnableAllGermanComplianceReports()
		{
			var reportTypeList = new CodeDescriptionBoolCollection()
				{
						{ "U11", (NoResString)"Umsatzsteuer-Sondervorauszahlung", true },
						{ "UVA", (NoResString)"Umsatzsteuervoranmeldung", true },
						{ "ZMD", (NoResString)"Zusammenfassende Meldung", true }
				};
			Instance.VisibleGermanComplianceReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportTypeList);
		}

		#region TestComplianceDocumentNumberAllocation_Receivables

		public void TestComplianceDocumentNumberAllocation_Receivables()
		{
			AssertEquals("Name", "ComplianceDocumentNumberAllocation", ItemSet.ComplianceDocumentNumberAllocation_Receivables.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceDocumentNumberAllocation_Receivables.Category);
			AssertEquals("Caption", "Compliance Document Number Allocation - Receivables", ItemSet.ComplianceDocumentNumberAllocation_Receivables.Caption);
			AssertEquals("Hint", @"This registry defines the default Compliance Number allocation behavior for all Branches in a login company. The method chosen determines when a Compliance Sub Type Transaction Number will be assigned to a Receivables Invoice (INV), Credit Note (CRD) or Adjustment Note (ADJ) transaction. Not all Receivables Invoice and Credit Note transactions necessarily require a Government Compliance document or Government Compliance Transaction Number.

Where MAN (Manually Assign Compliance Number after Posting Transaction) is selected, users will need to use the 'Allocate Compliance Number' Actions option to add the Compliance Number to the transactions.

Note: This registry is only relevant in login countries where the login company must assign additional and secondary Government Compliance Number allocation rules for certain types of Receivables INV, CRD or ADJ transactions.

Please note: If required, additional Branch specific and Compliance Sub Type specific number allocation behaviors can be defined in the 'Compliance Document Number Allocation Override - Receivables' registry. The Override registry is relevant when different Branches or Sub Types in the one login company need different number allocation behavior.", ItemSet.ComplianceDocumentNumberAllocation_Receivables.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentNumberAllocation_Receivables.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentNumberAllocation_Receivables.Options);
			AssertEquals("Default Value", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, ItemSet.ComplianceDocumentNumberAllocation_Receivables.DefaultValue);
		}

		public void TestComplianceDocumentNumberAllocation_Receivables_DefaultValue()
		{
			var alCompany = Factory.NewWithValidTestData<GlbCompany>();
			alCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Albania;

			var mzCompany = Factory.NewWithValidTestData<GlbCompany>();
			mzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Mozambique;

			var twCompany = Factory.NewWithValidTestData<GlbCompany>();
			twCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;

			var uzCompany = Factory.NewWithValidTestData<GlbCompany>();
			uzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Uzbekistan;

			var trCompany = Factory.NewWithValidTestData<GlbCompany>();
			trCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Turkey;

			var otherCountryCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCountryCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Somalia;

			Factory.Save();

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterDefaultProviderMock(mockComplianceFactory, Constants.CountryCodes.Albania, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate);
			CreateAndRegisterDefaultProviderMock(mockComplianceFactory, Constants.CountryCodes.Mozambique, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual);
			CreateAndRegisterDefaultProviderMock(mockComplianceFactory, Constants.CountryCodes.Taiwan, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print);
			CreateAndRegisterDefaultProviderMock(mockComplianceFactory, Constants.CountryCodes.Uzbekistan, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			CreateAndRegisterDefaultProviderMock(mockComplianceFactory, Constants.CountryCodes.Turkey, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual);

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				var alCompanyDefault = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(alCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("AL company > GOV (via CountryComplianceInfo)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, alCompanyDefault);

				var mzCompanyDefault = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(mzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("MZ company > MAN (via CountryComplianceInfo)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, mzCompanyDefault);

				var twCompanyDefault = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(twCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("TW company > PRN (via CountryComplianceInfo)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, twCompanyDefault);

				var uzCompanyDefault = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("UZ company > PST (via CountryComplianceInfo)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, uzCompanyDefault);

				var trCompanyDefault = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(trCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("TR company > MAN (via CountryComplianceInfo)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, trCompanyDefault);

				var otherCompanyDefault = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(otherCountryCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("Other company > PRN (default when no CountryComplianceInfo defined)", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, otherCompanyDefault);
			}
		}

		public void TestComplianceDocumentNumberAllocation_Receivables_Validation_WhenNoComplianceInfo()
		{
			var otherCountryCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCountryCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Somalia;

			Factory.Save();

			using (ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(otherCountryCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				var otherCompanyValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(otherCountryCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("When null CountryComplianceInfo, the value is set successfully", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, otherCompanyValue);
			}

			AssertExceptionThrown<RegistryValidationException>(
				"When null CountryComplianceInfo, default logic applies and setting GVT value throws",
				"'GVT' is not valid for country/region 'SO'.",
				() => ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(otherCountryCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate)
			);
			var otherCompanyDefaultValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(otherCountryCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("When validation error message, the value is not changed", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, otherCompanyDefaultValue);
		}

		public void TestComplianceDocumentNumberAllocation_Receivables_Validation_WhenNullFromComplianceInfo()
		{
			var alCompany = Factory.NewWithValidTestData<GlbCompany>();
			alCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Albania;

			var mzCompany = Factory.NewWithValidTestData<GlbCompany>();
			mzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Mozambique;

			Factory.Save();

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterRegistryDefaultProviderMockForARComplianceDocumentNumberValidation(mockComplianceFactory, Constants.CountryCodes.Albania, null);
			CreateAndRegisterRegistryDefaultProviderMockForARComplianceDocumentNumberValidation(mockComplianceFactory, Constants.CountryCodes.Mozambique, null);

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				using (ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(alCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
				{
					var alValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(alCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					AssertEquals("When null validation error message, the default validation applies: non-GVT value is set successfully", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, alValue);
				}

				AssertExceptionThrown<RegistryValidationException>(
					"When null validation error message, default validation rule applies and setting GVT value throws",
					"'GVT' is not valid for country/region 'MZ'.",
					() => ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(mzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate)
				);
				var otherCompanyDefaultValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(mzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("When validation error message, the value is not changed", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, otherCompanyDefaultValue);
			}
		}

		public void TestComplianceDocumentNumberAllocation_Receivables_Validation_WhenNoErrorFromComplianceInfo()
		{
			var mzCompany = Factory.NewWithValidTestData<GlbCompany>();
			mzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Mozambique;

			Factory.Save();

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterRegistryDefaultProviderMockForARComplianceDocumentNumberValidation(mockComplianceFactory, Constants.CountryCodes.Mozambique, string.Empty);

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				using (ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(mzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
				{
					var mzValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(mzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					AssertEquals("When empty validation error message, the value is set successfully", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, mzValue);
				}
			}
		}

		public void TestComplianceDocumentNumberAllocation_Receivables_Validation_WhenErrorReturnedByComplianceInfo()
		{
			var uzCompany = Factory.NewWithValidTestData<GlbCompany>();
			uzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Uzbekistan;

			Factory.Save();

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterRegistryDefaultProviderMockForARComplianceDocumentNumberValidation(mockComplianceFactory, Constants.CountryCodes.Uzbekistan, "A validation message for Uzbekistan");

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				AssertExceptionThrown<RegistryValidationException>(
					"When validation error message, setting value throws",
					"A validation message for Uzbekistan",
					() => ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual)
				);
				var uzCompanyValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("When validation error message, the value is not changed", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, uzCompanyValue);
			}
		}

		public void TestComplianceDocumentNumberAllocation_Receivables_Validation_WhenErrorReturnedByComplianceInfo_Turkey()
		{
			var trCompany = Factory.NewWithValidTestData<GlbCompany>();
			trCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Turkey;

			Factory.Save();

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			CreateAndRegisterDefaultProviderMock(mockComplianceFactory, Constants.CountryCodes.Turkey, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, "A validation message for Turkey");

			using (ObjectFactory.Substitute(mockComplianceFactory.Object))
			{
				AssertExceptionThrown<RegistryValidationException>(
					"When validation error message for Turkey, setting value throws",
					() => ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(trCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate)
				);
				var trCompanyValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(trCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("When validation error message, the value is not changed", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, trCompanyValue);

				AssertExceptionThrown<RegistryValidationException>(
					"When validation error message for Turkey, setting value throws",
					() => ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(trCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print)
				);
				trCompanyValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(trCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("When validation error message, the value is not changed", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, trCompanyValue);
			}
		}

		static void CreateAndRegisterDefaultProviderMock(Mock<ICountryComplianceFactory> mockComplianceFactory, string countryCode, string defaultValueToUse, string validationMessageToReturn = "")
		{
			var mock = new Mock<IComplianceRegistryDefaultProvider>();
			mock.Setup(x => x.GetDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry(It.IsAny<bool>())).Returns(defaultValueToUse);
			mock.Setup(x => x.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(It.IsAny<string>(), It.IsAny<bool>())).Returns(validationMessageToReturn);

			mockComplianceFactory.Setup((x) => x.GetIComplianceRegistryDefaultProvider(It.Is<ZString>(c => c == countryCode))).Returns(mock.Object);
		}

		public void TestComplianceDocumentNumberAllocation_Receivables_Validation_IntegrationWithEInvoicingRegistry()
		{
			var arCompany = Factory.NewWithValidTestData<GlbCompany>();
			arCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Argentina;
			Factory.Save();

			using (ItemSet.EnableEInvoicingFunctionality.SetTemporaryValue(arCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertExceptionThrown<RegistryValidationException>(
					"When validation error message for Argentina and E-Invoicing enabled, setting value throws",
					() => ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(arCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual)
				);
				var arCompanyValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(arCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("When validation error message, the value is not changed", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, arCompanyValue);
			}

			using (ItemSet.EnableEInvoicingFunctionality.SetTemporaryValue(arCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertExceptionThrown<RegistryValidationException>(
					"When validation error message for Argentina and E-Invoicing disabled, setting value throws",
					() => ItemSet.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(arCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate)
				);
				var arCompanyValue = ItemSet.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(arCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				AssertEquals("When validation error message, the value is not changed", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, arCompanyValue);
			}
		}

		static void CreateAndRegisterRegistryDefaultProviderMockForARComplianceDocumentNumberValidation(Mock<ICountryComplianceFactory> mockComplianceFactory, string countryCode, string validationMessageToReturn)
		{
			var mock = new Mock<IComplianceRegistryDefaultProvider>();
			mock.Setup(x => x.ValidateComplianceDocumentNumberAllocation_ReceivablesRegistry(It.IsAny<string>(), It.IsAny<bool>())).Returns(validationMessageToReturn);

			mockComplianceFactory.Setup((x) => x.GetIComplianceRegistryDefaultProvider(It.Is<ZString>(c => c == countryCode))).Returns(mock.Object);
		}

		#endregion

		public void TestComplianceDocumentNumberAllocationOverride_Receivables()
		{
			AssertEquals("Name", "ComplianceDocumentNumberAllocationOverride_Receivables", ItemSet.ComplianceDocumentNumberAllocationOverride_Receivables.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceDocumentNumberAllocationOverride_Receivables.Category);
			AssertEquals("Caption", "Compliance Document Number Allocation Override - Receivables", ItemSet.ComplianceDocumentNumberAllocationOverride_Receivables.Caption);
			AssertEquals("Hint", @"This registry allows you to override the Allocation method set in the 'Compliance Document Number Allocation - Receivables' Registry.
Use this registry to set additional Branch specific and Compliance Sub Type specific number allocation behaviors from the ones set in the 'Compliance Document Number Allocation - Receivables' registry.
This registry is relevant when different Branches or Sub Types in the one login company need different number allocation behavior.

You must always review both registries to ensure your Login Company has been setup correctly.", ItemSet.ComplianceDocumentNumberAllocationOverride_Receivables.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentNumberAllocationOverride_Receivables.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentNumberAllocationOverride_Receivables.Options);
			var defaultValue = ItemSet.ComplianceDocumentNumberAllocationOverride_Receivables.DefaultValue;
			AssertEquals("Default Value is empty", 0, defaultValue.Count);
		}

		public void TestComplianceDocumentNumberAllocation_Payables()
		{
			AssertEquals("Name", "ComplianceDocumentNumberAllocation_Payables", ItemSet.ComplianceDocumentNumberAllocation_Payables.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceDocumentNumberAllocation_Payables.Category);
			AssertEquals("Caption", "Compliance Document Number Allocation - Payables", ItemSet.ComplianceDocumentNumberAllocation_Payables.Caption);
			AssertEquals("Hint", @"This registry determines when a Compliance Sub Type Transaction Number will be assigned to a Payables Invoice (INV), Credit Note (CRD) or Adjustment Note (ADJ) transaction.  Not all Payables Invoice and Credit Note transactions necessarily require a Government Compliance document or Government Compliance Transaction Number.

This registry is only relevant to login countries/regions where businesses must assign and issue an additional and secondary Government Compliance Document for certain types of Payables INV, CRD and ADJ transactions.", ItemSet.ComplianceDocumentNumberAllocation_Payables.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentNumberAllocation_Payables.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentNumberAllocation_Payables.Options);
			AssertEquals("Default Value", AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, ItemSet.ComplianceDocumentNumberAllocation_Payables.DefaultValue);

			ItemSet.ComplianceDocumentNumberAllocation_Payables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print);
			AssertEquals("PRN", ItemSet.ComplianceDocumentNumberAllocation_Payables.Value);
		}

		public void TestComplianceSubTypeAttributionRuleSet()
		{
			AssertEquals("Name", "ComplianceSubTypeAttributionRuleSet", ItemSet.ComplianceSubTypeAttributionRuleSet.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceSubTypeAttributionRuleSet.Category);
			AssertEquals("Caption", "Compliance Sub Type Attribution Rule Set", ItemSet.ComplianceSubTypeAttributionRuleSet.Caption);
			AssertEquals("Hint", @"Use this registry to choose the Compliance Sub Type defaulting rule set to be applied to your Login Company. 

Note: This registry is only relevant to login countries where Government Compliance Sub Type and  Document features are enabled AND where more than one Compliance Sub Type Defaulting rule set is supported.

This registry determines the Compliance Sub Type Defaulting Rule set used by a Login Company as Invoice (INV) and Credit Note (CRD) transactions are posted.", ItemSet.ComplianceSubTypeAttributionRuleSet.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceSubTypeAttributionRuleSet.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceSubTypeAttributionRuleSet.Options);

			var registry = ItemSet.ComplianceSubTypeAttributionRuleSet;
			var factory = new BusinessObjectFactory();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var fallback = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var ruleSet1 = new ComplianceSubTypeAttributionRuleSet(fallback, factory, TaiwanComplianceInfo.RuleSetCodes.TXCTCR);
				var ruleSet3 = new ComplianceSubTypeAttributionRuleSet(fallback, factory, TaiwanComplianceInfo.RuleSetCodes.TXCTCRZNG);
				var expectedLogMessage = $"Rule Set changed from 1 to Rule Set 3.";
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, ruleSet1, ruleSet3);
				var logReference = ItemSet.ComplianceSubTypeAttributionRuleSet.OnBuildLogReference(args);
				AssertEquals(expectedLogMessage, logReference);
			}
		}

		public void TestComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch()
		{
			AssertEquals("Name", "ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch", ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.Category);
			AssertEquals("Caption", "Compliance Sub Type Attribution Rule Set – By Transaction Header Branch", ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.Caption);
			AssertEquals("Hint", @"Use this registry to choose the Compliance Sub Type defaulting rule set to be applied with reference to Transaction Header Branch. 

Note:
1. This registry is only relevant to login countries where Government Compliance Sub Type and Document features are enabled AND where more than one Compliance Sub Type Defaulting rule set is supported.
2. This registry currently applies to China login companies only.
3. If a rule set is not specified for a Branch here, the system will fall back to the rule set specified in ‘Compliance Sub Type Attribution Rule Set’ registry.", ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.Options);
			AssertEquals("CountryFilterPKs", true, ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.CountryFilterPKs.Contains(Constants.CountryGuids.China));
			AssertEquals("CountryFilterPKs", 1, ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.CountryFilterPKs.Count());

			var registry = ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch;
			var factory = new BusinessObjectFactory();
			var fallback = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var ruleSet1 = new ComplianceSubTypeAttributionRuleSet(fallback, factory, ChinaComplianceInfo.RuleSetCodes.TXATXB);
			var ruleSet3 = new ComplianceSubTypeAttributionRuleSet(fallback, factory, ChinaComplianceInfo.RuleSetCodes.TXATXBETB);
			var expectedLogMessage = $"Rule Set changed from 1 to Rule Set 3.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, ruleSet1, ruleSet3);
			var logReference = ItemSet.ComplianceSubTypeAttributionRuleSet.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch()
		{
			AssertEquals("Name", "ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch", ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.Category);
			AssertEquals("Caption", "Compliance Sub Type Attribution Rule Configuration – By Transaction Header Branch(CWSupport Only)", ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.Caption);
			AssertEquals("Hint", @"A Sub Type will be set against an Invoice, Credit Note or Adjustment Note transaction header when the transaction details match a Sub Type’s Attribution Rules.

The Attribution Rules of a Sub Type identify those transactions that will require an additional Government Compliance Document. Not all Invoices and Credit Note transactions necessarily require a Government Compliance document. 
 
Note:
1. This registry is only relevant to those login countries where businesses must assign and issue Government mandated documents and transactions number for certain types of INV, CRD, and ADJ transactions.
2. This registry currently applies to China login companies only.
3. If an attribution rule is not configured for a Branch here, the system will fall back to the attribution rule configured in ‘Compliance Sub Type Attribution Rule Configuration’ registry.", ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.Options);
			AssertEquals("CountryFilterPKs", true, ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.CountryFilterPKs.Contains(Constants.CountryGuids.China));
			AssertEquals("CountryFilterPKs count", 1, ItemSet.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.CountryFilterPKs.Count());
		}

		public void TestComplianceSubTypeAttributionRuleSet_ByTransactionHeaderDefaultByRuleSet()
		{
			var collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(0, collection.Count);

			var countryCode = Constants.CountryCodes.China;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty);
				Instance.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.SetValue(Guid.Empty, fallback.BranchPK, Guid.Empty, new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(fallback, Factory, ""));
				collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty);

				AssertEquals(0, collection.Count);

				Instance.ComplianceSubTypeAttributionRuleSet_ByTransactionHeaderBranch.SetValue(Guid.Empty, fallback.BranchPK, Guid.Empty, new ComplianceSubTypeAttributionRuleSetByTransactionHeaderBranch(fallback, Factory, ChinaComplianceInfo.RuleSetCodes.TXATXB));
				collection = ItemSet.ComplianceSubTypeAttributionRuleConfiguration_ByTransactionHeaderBranch.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty);
				AssertEquals(7, collection.Count);
				AssertComplianceSubTypeAttributionRule(collection[0], countryCode, ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, "", TaxRegistrationTypeCodes.Recoverable);
				AssertComplianceSubTypeAttributionRule(collection[1], countryCode, ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, "", TaxRegistrationTypeCodes.Recoverable);
				AssertComplianceSubTypeAttributionRule(collection[2], countryCode, ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, "", TaxRegistrationTypeCodes.NotRecoverable);
				AssertComplianceSubTypeAttributionRule(collection[3], countryCode, ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, "", TaxRegistrationTypeCodes.Individual);
				AssertComplianceSubTypeAttributionRule(collection[4], countryCode, ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, "", TaxRegistrationTypeCodes.Recoverable);
				AssertComplianceSubTypeAttributionRule(collection[5], countryCode, ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, "", TaxRegistrationTypeCodes.Recoverable);
				AssertComplianceSubTypeAttributionRule(collection[6], countryCode, ChinaComplianceInfo.ComplianceSubTypeCodes.TXB, LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT, OriginalRuleCodes.AllTransactions, DisbursementRuleCodes.AllTransactions, "", TaxRegistrationTypeCodes.NotRecoverable);
			}
		}

		public void TestLastUTCDateToDisableComplianceBookAfterDbRestored()
		{
			AssertEquals("Name", "LastUTCDateToDisableComplianceBookAfterDbRestored", ItemSet.LastUTCDateToDisableComplianceBookAfterDbRestored.Name);
			AssertEquals("Category", "Accounting", ItemSet.LastUTCDateToDisableComplianceBookAfterDbRestored.Category);
			AssertEquals("Caption", "Last UTC Date to Disable Compliance Book After Db Restored (Developer Only)", ItemSet.LastUTCDateToDisableComplianceBookAfterDbRestored.Caption);
			AssertEquals("Hint", @"Portugal Companies needs to receive a notification when database are restored.
Sensitive Accounting data (compliance sequence books for example) will be disabled.
This registry is to store last disabled UTC date.", ItemSet.LastUTCDateToDisableComplianceBookAfterDbRestored.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LastUTCDateToDisableComplianceBookAfterDbRestored.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForDevelopers, ItemSet.LastUTCDateToDisableComplianceBookAfterDbRestored.Options);
			AssertEquals("Default value", DateTime.MinValue, ItemSet.LastUTCDateToDisableComplianceBookAfterDbRestored.Value);
		}

		public void TestComplianceReportFileNextSequenceNumber()
		{
			AssertEquals("Name", "ComplianceReportFileNextSequenceNumber", ItemSet.ComplianceReportFileNextSequenceNumber.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceReportFileNextSequenceNumber.Category);
			AssertEquals("Caption", "Compliance Report File Next Number (CargoWiseOne Support Only)", ItemSet.ComplianceReportFileNextSequenceNumber.Caption);
			AssertEquals("Hint", @"This registry allows you to specify next sequential number to be used for generating Compliance Report File.
A sequential number is assigned to each file generated from a specific Report Type.
Currently, this registry is considered only when generating XML file for ‘EST - Esterometro’ report in Italy login companies.

Note: In Italy Esterometro XML, File Number (ProgressivoInvio) is a 5-character alphanumeric value. Therefore, the number in this registry is converted to base-36 text on the export file. As an example, numbers 1-9 will generate corresponding values 00001-00009 on the export file, but number 10 in this registry will generate the value 0000A on the export file.
To set this registry to the desired File Number, convert that base-36 value to a decimal first.
For example, to set next file number to ""10000"", set this registry to 1679616.", ItemSet.ComplianceReportFileNextSequenceNumber.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceReportFileNextSequenceNumber.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ComplianceReportFileNextSequenceNumber.Options);
			AssertEquals("Default Value", 1, ItemSet.ComplianceReportFileNextSequenceNumber.DefaultValue);
		}

		public void TestMaximumNumberofChargestoPrintperComplianceDocument()
		{
			AssertEquals("Name", "MaximumNumberofChargestoPrintperComplianceDocument", ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.Category);
			AssertEquals("Caption", "Default Maximum Number of Charges to Print per Compliance Document", ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.Caption);
			AssertEquals("Hint", @"This setting sets the default maximum number of charge lines to print per compliance document.
This setting is relevant when the local compliance document can only be a single page document.
This setting is used in the Compliance Sequences module when configuring a Compliance Invoice Book series.", ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.Options);
			AssertEquals("Default Value", 40, ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.DefaultValue);

			ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			AssertEquals(20, ItemSet.MaximumNumberOfChargesToPrintPerComplianceDocument.Value);
		}

		public void TestEnforceReceivablesComplianceDocumentDateandNumberSequencing()
		{
			AssertEquals("EnforceReceivablesComplianceDocumentDateandNumberSequencing", ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.Name);
			AssertEquals("Accounting/Government Compliance Invoice Document", ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.Category);
			AssertEquals("Enforce Receivables Compliance Document Date and Number Sequencing", ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.Caption);
			AssertEquals(@"When this registry is set to 'Yes', document number will not be allocate if the number and date sequence are out of sync for a given compliance invoice book.
Before running the allocation function, the document date will need to be adjusted.", ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.Hint);
			AssertEquals("Only show TW", CountryFilterPKs.Taiwan, ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.CountryFilterPKs);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.Storage);
			AssertEquals("Options: IsOnlyForSupport + CacheExpensiveDefaultValue",
				RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue,
				ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.Options
			);

			var twCompany = new FakeCompany(countryCode: Constants.CountryCodes.Taiwan);
			using (ObjectFactory.Substitute(twCompany.CreateMockForICompanyProvider().Object))
			{
				Assert(ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.DefaultValue);
			}

			var auCompany = new FakeCompany(countryCode: Constants.CountryCodes.Australia);
			using (ObjectFactory.Substitute(auCompany.CreateMockForICompanyProvider().Object))
			{
				Assert(!ItemSet.EnforceReceivablesComplianceDocumentDateandNumberSequencing.DefaultValue);
			}
		}

		public void TestEnableComplianceDocumentModule()
		{
			AssertEquals("EnableComplianceDocumentModule", ItemSet.EnableComplianceDocumentModule.Name);
			AssertEquals("Accounting/Government Compliance Invoice Document", ItemSet.EnableComplianceDocumentModule.Category);
			AssertEquals("Enable Compliance Document Module (CW1 Support Only)", ItemSet.EnableComplianceDocumentModule.Caption);
			AssertEquals(@"This registry should only be enabled for Taiwan login companies only.

When this module is enabled, the compliance sub type and number will be assigned to individual invoice line instead of the invoice header. 
Thus allowing multiple compliance documents to be issued for a billing invoice. 
Alternatively, a compliance document can be issued for multiple billing invoices. 

For more information, please refer to the ""Compliance Document Module"" update note on My Account.", ItemSet.EnableComplianceDocumentModule.Hint);
			AssertEquals("Only show TW", CountryFilterPKs.Taiwan, ItemSet.EnableComplianceDocumentModule.CountryFilterPKs);
			AssertEquals("Options: expect IsOnlyForSupport + CacheExpensiveDefaultValue",
				RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue,
				ItemSet.EnableComplianceDocumentModule.Options
			);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EnableComplianceDocumentModule.Storage);

			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				var company = new FakeCompany(countryCode: countryCode);
				using (ObjectFactory.Substitute(company.CreateMockForICompanyProvider().Object))
				{
					if (countryCode == Constants.CountryCodes.Taiwan)
					{
						Assert(ItemSet.EnableComplianceDocumentModule.DefaultValue);
					}
					else
					{
						Assert(!ItemSet.EnableComplianceDocumentModule.DefaultValue);
					}
				}
			}
		}

		public void TestExportMultipleDebtorOrganizationContactEmail()
		{
			AssertEquals("Name", "ExportMultipleDebtorOrganizationContactEmail", ItemSet.ExportMultipleDebtorOrganizationContactEmail.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations.ToString(), ItemSet.ExportMultipleDebtorOrganizationContactEmail.Category);
			AssertEquals("Caption", "Export Multiple Debtor Organization Contact Email", ItemSet.ExportMultipleDebtorOrganizationContactEmail.Caption);
			AssertEquals("Hint", @"This registry is relevant to Login Countries where the electronic invoicing Service Partner or Government Portal supports automatic mailing of Tax Invoice to multiple recipients.

By default, only one Debtor Organization Contact email address will be included during the electronic invoice transmission based on the existing defaulting and fallback rules(if any).
When enabled, multiple Debtor Organization Contact emails will be included during the electronic invoice transmission. Please refer to the respective country's e-Learning Materials for details of the transmission logic.

NOTE: Currently, this registry is only relevant to Vietnam and South Korea Login Companies for which the Receivables E-Reporting Functionality is enabled.", ItemSet.ExportMultipleDebtorOrganizationContactEmail.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ExportMultipleDebtorOrganizationContactEmail.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ExportMultipleDebtorOrganizationContactEmail.Options);
			AssertEquals("Only show Vietnam and KoreaSouth", true, ItemSet.ExportMultipleDebtorOrganizationContactEmail.CountryFilterPKs.Contains(Constants.CountryGuids.Vietnam) &&
				ItemSet.ExportMultipleDebtorOrganizationContactEmail.CountryFilterPKs.Contains(Constants.CountryGuids.KoreaRepublicof) &&
				ItemSet.ExportMultipleDebtorOrganizationContactEmail.CountryFilterPKs.Count() == 2);
			AssertEquals("DefaultValue", "DEF", Instance.ExportMultipleDebtorOrganizationContactEmail.DefaultValue);

			ItemSet.ExportMultipleDebtorOrganizationContactEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ExportMultipleDebtorOrganizationContactEmailCodes.DEF);
			AssertEquals("ExportMultipleDebtorOrganizationContactEmail value is DEF", "DEF", ItemSet.ExportMultipleDebtorOrganizationContactEmail.Value);

			ItemSet.ExportMultipleDebtorOrganizationContactEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ExportMultipleDebtorOrganizationContactEmailCodes.MAR);
			AssertEquals("ExportMultipleDebtorOrganizationContactEmail value is MAR", "MAR", ItemSet.ExportMultipleDebtorOrganizationContactEmail.Value);
		}

		public void TestIncludeContactEmailsOfLocalDebtorOrganizationOnly()
		{
			var registryItem = ItemSet.IncludeContactEmailsOfLocalDebtorOrganizationOnly;

			AssertEquals("Name", "IncludeContactEmailsOfLocalDebtorOrganizationOnly", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations.ToString(), registryItem.Category);
			AssertEquals("Caption", "Include Contact Emails of Local Debtor Organization Only", registryItem.Caption);
			AssertEquals("Hint", @"This registry is used in conjunction with the 'Export Multiple Debtor Organization Contact Email' registry to control whether the system exports the contact email address for local debtors only.
By default, the system exports the debtor's contact email address during electronic invoice transmission, regardless of the debtor’s location (local or foreign).
When this registry is enabled, the system will export the debtor’s contact email address only if the debtor is local.

Note: This registry is currently used for Vietnam login companies only.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);
			AssertEquals("Default value", false, registryItem.DefaultValue);
			AssertArrayEqualsByElements("Country filters", new [] { Constants.CountryGuids.Vietnam }, registryItem.CountryFilterPKs.ToArray());
		}

		public void TestEnableComplianceDocumentModule_ForThreadSafetyOfCountrySpecificDefault()
		{
			var allBranches = Factory.Load<GlbBranch>(new ZQuery());
			var branchPk = allBranches.First(x => x.PK != GlbBranch.CurrentBranch.PK).PK.ToGuid();

			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.DefaultValue);
			var countOfFactoryOnMainThread = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Count(x => x.NameForDebugging.StartsWith("AccountingMasterFilesRegistry Factory for Country Defaults"));
			AssertEquals("A factory should be created to get the default registry value", 1, countOfFactoryOnMainThread);

			var otherThread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (DisposableEnvironment.ForBranch(branchPk))
				{
					AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.DefaultValue);
				}
			});
			otherThread.Start();
			otherThread.Join();

			var countOfFactoryAfterOtherThread = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Count(x => x.NameForDebugging.StartsWith("AccountingMasterFilesRegistry Factory for Country Defaults"));
			AssertGreaterThan("Another factory should be created to get the default registry value on another thread", countOfFactoryAfterOtherThread, countOfFactoryOnMainThread);
		}

		public void TestEachComplianceReportConfigurationCollectionhasOnlyOneDefaultReportType()
		{
			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var defaultConfigs = ItemSet.ComplianceReportConfiguration.Value.Cast<ComplianceReportConfiguration>().Where(x => x.IsDefaultReportType);
					Assert($"A compliance report collection can have only one default config. Fix Country {countryCode}", defaultConfigs.Count() <= 1);
				}
			}
		}

		public void TestAllowNegativeComplianceDocumentLines()
		{
			AssertEquals("AllowNegativeComplianceDocumentLines", ItemSet.AllowNegativeComplianceDocumentLines.Name);
			AssertEquals("Accounting/Government Compliance Invoice Document", ItemSet.AllowNegativeComplianceDocumentLines.Category);
			AssertEquals("Allow Negative Compliance Document Lines", ItemSet.AllowNegativeComplianceDocumentLines.Caption);
			AssertEquals("Options: CacheExpensiveDefaultValue", RegistryOptions.CacheExpensiveDefaultValue, ItemSet.AllowNegativeComplianceDocumentLines.Options);
			AssertEquals("Only show Taiwan", CountryFilterPKs.Taiwan, ItemSet.AllowNegativeComplianceDocumentLines.CountryFilterPKs);
			AssertEquals(@"This registry is only relevant when the Compliance Document module is enabled.
By default, the system allows negative compliance document lines to be created for both AR and AP compliance documents.
You can override this registry to prevent users from creating compliance document containing negative compliance document lines.", ItemSet.AllowNegativeComplianceDocumentLines.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.AllowNegativeComplianceDocumentLines.Storage);

			using (Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
				{
					var company = new FakeCompany(countryCode: countryCode);
					using (ObjectFactory.Substitute(company.CreateMockForICompanyProvider().Object))
					{
						if (countryCode == Core.Constants.CountryCodes.Taiwan)
						{
							Assert(!ItemSet.AllowNegativeComplianceDocumentLines.DefaultValue);
						}
						else
						{
							Assert(ItemSet.AllowNegativeComplianceDocumentLines.DefaultValue);
						}
					}
				}
			}
		}

		public void TestAllowReActivationOfInactiveComplianceBook()
		{
			AssertEquals("AllowReActivationOfInactiveComplianceBook", ItemSet.AllowReActivationOfInactiveComplianceBook.Name);
			AssertEquals("Accounting/Government Compliance Invoice Document", ItemSet.AllowReActivationOfInactiveComplianceBook.Category);
			AssertEquals("Allow re-activation of inactive compliance books", ItemSet.AllowReActivationOfInactiveComplianceBook.Caption);
			AssertEquals(@"This registry controls the ability to reactivate compliance books after they have been flagged inactive.
When set to 'Yes', users will be able to flag compliance books as inactive and active without restrictions.
When set to 'No', when un-ticking the 'Is Active' box in a compliance book, the system will warn users that this action cannot be reversed and once inactive, the book cannot be flagged as active again.
By default, the value of this registry is 'Yes'.", ItemSet.AllowReActivationOfInactiveComplianceBook.Hint);
			AssertEquals("Default value", true, ItemSet.AllowReActivationOfInactiveComplianceBook.Value);
		}

		public void TestComplianceAllowPartialSequenceNumberAllocation()
		{
			AssertEquals("Name", "ComplianceAllowPartialSequenceNumberAllocation", ItemSet.ComplianceAllowPartialSequenceNumberAllocation.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceAllowPartialSequenceNumberAllocation.Category);
			AssertEquals("Caption", "Allow Partial Allocation of Compliance Sequence Number", ItemSet.ComplianceAllowPartialSequenceNumberAllocation.Caption);
			AssertEquals("Hint", @$"When allocating numbers to multiple transactions at once, this registry defines how {Core.Constants.ProductName} will behave when there are not enough numbers remaining in the relevant compliance invoice books to successfully assign each selected transaction an appropriate number.
By default, when there are not enough numbers available No Allocation to any transaction will be made.  The user will be shown a message and allowed to change the set of transactions selected for allocation.
Alternatively, when this registry is overridden and set to Yes, numbers will be assigned where possible until the relevant books have been exhausted.", ItemSet.ComplianceAllowPartialSequenceNumberAllocation.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceAllowPartialSequenceNumberAllocation.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceAllowPartialSequenceNumberAllocation.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.ComplianceAllowPartialSequenceNumberAllocation.Value);
		}

		public void TestSuppressShowComplianceBookHasNoTemplateWarning()
		{
			AssertEquals("Name", "SuppressShowComplianceBookHasNoTemplateWarning", ItemSet.SuppressShowComplianceBookHasNoTemplateWarning.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.SuppressShowComplianceBookHasNoTemplateWarning.Category);
			AssertEquals("Caption", "Suppress Warning When Compliance Book Has No Document Menu", ItemSet.SuppressShowComplianceBookHasNoTemplateWarning.Caption);
			AssertEquals("Hint", @$"By default {Core.Constants.ProductName} will warn a user when they attempt to assign and ‘Print’ Government Compliance Numbers using Compliance Invoice Books that do NOT have an assigned document menu.

When overridden and set to ‘Yes’ the warning message will not show.
Configure this registry to ‘Yes’ when no document is required and the Compliance Invoice Books will only be used to assign compliance numbers.",
			ItemSet.SuppressShowComplianceBookHasNoTemplateWarning.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.SuppressShowComplianceBookHasNoTemplateWarning.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.SuppressShowComplianceBookHasNoTemplateWarning.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.Value);
		}

		public void TestSuppressShowComplianceBookHasNoTemplateWarning_DefaultValueIsTrue_ForIndiaCompany()
		{
			var indiaCompany = Factory.NewWithValidTestData<GlbCompany>();
			indiaCompany.GC_RN_NKCountryCode = Constants.CountryCodes.India;
			Factory.Save();
			AssertEquals("Default Value should be true when company is in India", true, ItemSet.SuppressShowComplianceBookHasNoTemplateWarning.GetFallBackValueAtAllLevels(indiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestCreditNoteComplianceDocumentConfiguration()
		{
			ItemSet.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Name", "CreditNoteComplianceDocumentConfiguration", ItemSet.CreditNoteComplianceDocumentConfiguration.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.CreditNoteComplianceDocumentConfiguration.Category);
			AssertEquals("Caption", "Credit Note Compliance Document Configuration", ItemSet.CreditNoteComplianceDocumentConfiguration.Caption);
			AssertEquals("Options: IsOnlyForSupport + CacheExpensiveDefaultValue",
				RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue,
				ItemSet.CreditNoteComplianceDocumentConfiguration.Options
			);
			AssertEquals("Only show TW", CountryFilterPKs.Taiwan, ItemSet.CreditNoteComplianceDocumentConfiguration.CountryFilterPKs);
			AssertEquals("Hint", @"This registry is only relevant to those login countries where the Compliance Document Module has been enabled.
This registry applies to ‘Receivable Credit Note Compliance Document’ only.
By default, this registry is set to ‘No’ in which case the compliance sequence number will draw the number from respective invoice book based on compliance sub type and allocation level. Further, the change in behavior relating to setting this registry to ‘Yes’ will not be applied.

When this registry is set to ‘Yes’, the following change will be applied to the compliance document relating to Credit Note transactions.
	1.	Allocation of compliance sequence number will not be applied. The ‘Compliance Book’ field in Compliance Document screen will be left empty, read only and no validation will be applied.
	2.	The ‘Document Number’ field will be editable, and user will be able to manually enter a value if the field is empty.
	3.	When users create a Credit Note via ‘Amend with Credit Note’ function in Job / Consol > AR Invoices tab or when users create a Credit Note with Original Reference specified in the Receivables Transactions module, the system will behave as follows:
			•Users will not be allowed to amend an invoice with credit note if the selected invoice contains transaction lines with tax id and one or more of these lines does not have a compliance record with document number allocated.
			•Users will not be able to add new charge line nor edit charge code, tax id, job number of existing transaction lines. Deleting of existing transaction lines is allowed.
			•Compliance document record will be created roll up by charge code regardless of the debtor’s ‘Create Compliance Document Record on Posting’ setting.
			•Credit Note Compliance Document’s document number will derive from the respective invoice line’s compliance document.", ItemSet.CreditNoteComplianceDocumentConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.CreditNoteComplianceDocumentConfiguration.Storage);
			Assert("DefaultValue", !ItemSet.CreditNoteComplianceDocumentConfiguration.DefaultValue);

			var twCompany = new FakeCompany(countryCode: Constants.CountryCodes.Taiwan);
			using (ObjectFactory.Substitute(twCompany.CreateMockForICompanyProvider().Object))
			{
				ItemSet.EnableComplianceDocumentModule.SetValue(twCompany.PK, Guid.Empty, Guid.Empty, false);
				var itemValue = (bool)ItemSet.CreditNoteComplianceDocumentConfiguration.Inner.GetDefaultValue(twCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals("DefaultValue Taiwan should be false", false, itemValue);

				ItemSet.EnableComplianceDocumentModule.SetValue(twCompany.PK, Guid.Empty, Guid.Empty, true);
				itemValue = (bool)ItemSet.CreditNoteComplianceDocumentConfiguration.Inner.GetDefaultValue(twCompany.PK, Guid.Empty, Guid.Empty);
				AssertEquals("DefaultValue Taiwan should be true", true, itemValue);
			}
		}

		public void TestCreditNoteComplianceDocumentConfigurationDefaultValue()
		{
			ItemSet.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.CreditNoteComplianceDocumentConfiguration.DefaultValue);

			var twCompany = new FakeCompany(countryCode: Constants.CountryCodes.Taiwan);
			using (ObjectFactory.Substitute(twCompany.CreateMockForICompanyProvider().Object))
			{
				ItemSet.EnableComplianceDocumentModule.SetValue(twCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals(false, ItemSet.CreditNoteComplianceDocumentConfiguration.Inner.GetDefaultValue(twCompany.PK, Guid.Empty, Guid.Empty));

				ItemSet.EnableComplianceDocumentModule.SetValue(twCompany.PK, Guid.Empty, Guid.Empty, true);
				AssertEquals(true, ItemSet.CreditNoteComplianceDocumentConfiguration.Inner.GetDefaultValue(twCompany.PK, Guid.Empty, Guid.Empty));
			}
		}

		public void TestPromptToPrintComplianceDocumentOnCreation()
		{
			var item = ItemSet.PromptToPrintComplianceDocumentOnCreation;
			AssertEquals("Name", "PromptToPrintComplianceDocumentOnCreation", item.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, item.Category);
			AssertEquals("Caption", "Prompt to print AR compliance document on creation", item.Caption);
			AssertEquals("Hint", @"This registry is only relevant to system companies with the new Compliance Document enabled.
When this registry is set to 'Yes', the system will prompt the user whether to print the document on creation of the INV compliance document record.

Note: A document menu must be specified against the respective Compliance Sequence Book.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("DefaultValue", false, item.DefaultValue);

			AssertEquals("Options", RegistryOptions.Default, ItemSet.PromptToPrintComplianceDocumentOnCreation.Options);
			AssertEquals("Only show Taiwan", CountryFilterPKs.Taiwan, ItemSet.PromptToPrintComplianceDocumentOnCreation.CountryFilterPKs);

			using (ItemSet.PromptToPrintComplianceDocumentOnCreation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Assert(ItemSet.PromptToPrintComplianceDocumentOnCreation.Value);
			}

			using (ItemSet.PromptToPrintComplianceDocumentOnCreation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, ItemSet.PromptToPrintComplianceDocumentOnCreation.Value);
			}
		}

		public void TestPreventInvoiceDateGreaterThanPostDate()
		{
			AssertEquals("Caption", "Prevent posting Invoice Date greater than Post Date", ItemSet.PreventInvoiceDateGreaterThanPostDate.Caption);
			AssertEquals("Category", Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.PreventInvoiceDateGreaterThanPostDate.Category);
			AssertEquals("DefaultValue", false, ItemSet.PreventInvoiceDateGreaterThanPostDate.DefaultValue);
			AssertEquals("Hint", @"This registry can be used to prevent posting AP transactions with Invoice Date greater than Post Date. By default, the system allows you to select an Invoice Date which is greater than the post date. This can happen if you back date the Post Date of the transaction.

Set this registry to YES in order to prevent users from posting AP invoices, credit note and adjustments where invoice date is greater than the post date.", ItemSet.PreventInvoiceDateGreaterThanPostDate.Hint);
			AssertEquals("Name", "PreventInvoiceDateGreaterThanPostDate", ItemSet.PreventInvoiceDateGreaterThanPostDate.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.PreventInvoiceDateGreaterThanPostDate.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.PreventInvoiceDateGreaterThanPostDate.Storage);
		}

		#endregion

		#region Compliance Reports Setup.

		public void TestEnableReportSetup()
		{
			AssertEquals("Enable Report Setup Default value should be false", false, ItemSet.EnableReportSetup.DefaultValue);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.EnableReportSetup.Storage);
			AssertEquals("Enable Report Setup should for EDISupport only", RegistryOptions.IsOnlyForSupport, ItemSet.EnableReportSetup.Options);
		}

		public void TestComplianceReportsSetupsUserDefinedByEnableReportSetup()
		{
			ItemSet.EnableReportSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ComplianceReportsSetupsUserDefined should be show by EnableReportSetup", RegistryOptions.Default, ItemSet.ComplianceReportsSetupsUserDefined.Options);
		}

		public void TestComplianceReportsSetupsUserDefinedByChina()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;

			AssertEquals("ComplianceReportsSetupsUserDefined should be show by China.", RegistryOptions.Default, ItemSet.ComplianceReportsSetupsUserDefined.Options);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryCode;
		}

		public void TestComplianceReportsSetupsUserDefinedHiddenByNonChina()
		{
			AssertEquals("ComplianceReportsSetupsUserDefined should be hidden by non-China.",
				RegistryOptions.IsHidden,
				ItemSet.ComplianceReportsSetupsUserDefined.Options);
		}

		public void TestComplianceReportsSetupsUserDefinedVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.ComplianceReportsSetupsUserDefined.Options);
			AccountingMasterFilesRegistry.Instance.EnableReportSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.Default, ItemSet.ComplianceReportsSetupsUserDefined.Options);
			AccountingMasterFilesRegistry.Instance.EnableReportSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.ComplianceReportsSetupsUserDefined.Options);
		}

		public void TestMaximumTimeForCRQServiceTaskToRun()
		{
			var item = ItemSet.MaximumTimeForCRQServiceTaskToRun;
			AssertEquals("Name", "MaximumTimeForCRQServiceTaskToRun", item.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, item.Category);
			AssertEquals("Caption", "Maximum runtime for CRQ service task in seconds when generating and purging (CargoWise Support Only)", item.Caption);
			AssertEquals("Hint", @"CRQ generates large compliance reports like IDEA or FEC in the background.
Normal working cycle is: Queue - Generate - Purge. This cycle might take some time to complete.
To reduce waiting time, the working cycle is repeated until the configured timeout is reached or there is no more data to process.
The value of this registry is in seconds.
The default is 2700 (45 minutes). Maximum is 86400 (24 hours).
Set to zero to disable this behavior.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("DefaultValue", 2700, item.DefaultValue);
			AssertEquals("MinimumValue", 0d, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals("MaximumValue", 86400d, ((IntRegistryDataType)item.DataType).UpperBound);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
		}

		public void TestVisibleGermanComplianceReports()
		{
			AssertEquals("Caption", "Show/Hide German Compliance Reports", ItemSet.VisibleGermanComplianceReports.Caption);
			AssertEquals("Hint", "Hide German compliance reports already checked-in but not yet fully implemented", ItemSet.VisibleGermanComplianceReports.Hint);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, ItemSet.VisibleGermanComplianceReports.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.VisibleGermanComplianceReports.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.VisibleGermanComplianceReports.Options);
			AssertEquals("Number of reports", 3, ItemSet.VisibleGermanComplianceReports.DefaultValue.Count);
			AssertGermanReportStatus("U11", false);
			AssertGermanReportStatus("UVA", false);
			AssertGermanReportStatus("ZMD", false);

			void AssertGermanReportStatus(ZString reportCode, bool visible)
			{
				var reportStatus = ItemSet.VisibleGermanComplianceReports.DefaultValue.Cast<CodeDescriptionBool>().FirstOrDefault(v => v.Code == reportCode);
				AssertNotNull($"Report {reportCode} is defined in VisibleGermanComplianceReports.DefaultValue", reportStatus);
				AssertEquals($"Report {reportCode} is visible", visible, reportStatus.Bool);
			}
		}

		public void TestReportsSetupForBalanceSheet()
		{
			AssertEquals("Default Report Type count should be 0", 0, ItemSet.ComplianceReportsSetupsCN.Value.Count);

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			ComplianceReportTypeCollection list = new ComplianceReportTypeCollection(Constants.CountryCodes.Australia);
			ItemSet.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("Default Report Type count should be 0", 0, ItemSet.ComplianceReportsSetupsCN.Value.Count);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
			ItemSet.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("Default Report Type count should be 8", 8, ItemSet.ComplianceReportsSetupsCN.Value.Count);
			AssertEquals("Last Default Report Type", "TT0", ItemSet.ComplianceReportsSetupsCN.Value[7].ReportType);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryCode;
		}

		#endregion

		#region GatewayBillingChargeCodes

		public void TestGatewayBillingChargeCodes()
		{
			AssertEquals("Name", "GatewayBillingChargeCodes", ItemSet.GatewayBillingChargeCodes.Name);
			AssertEquals("Category", "Accounting/Job Invoicing/Gateway Consol Job Invoicing", ItemSet.GatewayBillingChargeCodes.Category);
			AssertEquals("Caption", "Gateway Billing Charge Codes", ItemSet.GatewayBillingChargeCodes.Caption);
			AssertEquals("Hint", @"This registry allows you to nominate charge codes that are related to gateway operations.
For gateway agents importing intercompany invoices, gateway related charge codes are imported as costs on gateway billing job, while other charges are imported as forwarding (non - gateway) costs.
For forwarding(non-gateway) agents who bill gateway agents, using gateway related charge codes allows you to post gateway agent AR invoice  separately from posting other overseas agent charges.
Note that when this registry is set to the Default value, ALL charge codes are treated as gateway-related. If you are using a designated set of charge codes for gateway operations, set this registry to Override.", ItemSet.GatewayBillingChargeCodes.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.GatewayBillingChargeCodes.Storage);
			AssertEquals("DefaultChargeCode", string.Empty, ItemSet.GatewayBillingChargeCodes.DefaultChargeCode);
			AssertEquals("DefaultFilter", RegistryFindBoxFilter.None, (ItemSet.GatewayBillingChargeCodes.EditorInfo as AccChargeCodeListRegistryEditorInfo).Filter);
			AssertEquals("IsValueMandatory", RegistryOptions.IsValueMandatory, AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.Options);
		}

		#endregion

		#region GL Local Number Format Registry Item.

		public void TestLocalNumberFormatRegistryItem()
		{
			AssertEquals("Default count should be 0", 0, ItemSet.LocalNumberFormats.Value.Count);

			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Constants.CountryCodes.China;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;
			ItemSet.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals("the count should be 1", 1, ItemSet.LocalNumberFormats.Value.Count);
		}

		#endregion

		public void TestEnableTransactionNumberCriticalValidation()
		{
			AssertEquals("Caption", "Enable Transaction Number Critical Validation (CargoWiseOne Support only)", ItemSet.EnableTransactionNumberCriticalValidation.Caption);
			AssertEquals("Category", "Accounting", ItemSet.EnableTransactionNumberCriticalValidation.Category);
			AssertEquals("DefaultValue", true, ItemSet.EnableTransactionNumberCriticalValidation.DefaultValue);
			AssertEquals("Hint", "This registry controls whether to enable Transaction Number Critical Validation.", ItemSet.EnableTransactionNumberCriticalValidation.Hint);
			AssertEquals("Name", "EnableTransactionNumberCriticalValidation", ItemSet.EnableTransactionNumberCriticalValidation.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableTransactionNumberCriticalValidation.Options);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableTransactionNumberCriticalValidation.Storage);
		}

		public void TestDefaultOSAgentFromPickupAgent()
		{
			AssertEquals("Category", Categories.Accounting_JobInvoicing, ItemSet.DefaultOSAgentFromPickupAgent.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.DefaultOSAgentFromPickupAgent.Storage);
			AssertEquals("Support Only", RegistryOptions.IsOnlyForSupport, ItemSet.DefaultOSAgentFromPickupAgent.Options);
			AssertEquals("Default value", true, ItemSet.DefaultOSAgentFromPickupAgent.Value);
		}

		public void TestJobChargeDataVersionAutoLogging()
		{
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.JobChargeDataVersionAutoLogging.Value);
		}

		public void TestCheckBookDataVersionAutoLogging()
		{
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.CheckBookDataVersionAutoLogging.Value);
		}

		public void TestOrganizationsEvaluatedForCreditControl()
		{
			OrgsEvaluatedForCreditControlCollection collection = AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.Value;
			AssertEquals("Default entry entry", 48, collection.Count);
			AssertEquals("Default entry entry", collection.Count, collection.Cast<OrgsEvaluatedForCreditControl>().Count(c => c.OrganizationType == OrgCodes.AllDebtors));
			AssertEquals("Name", "OrganizationsEvaluatedForControlledDocumentDelivery", ItemSet.OrganizationsEvaluatedForCreditControl.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.OrganizationsEvaluatedForCreditControl.Category);
			AssertEquals("Caption", "Organizations Excluded from Credit Control Evaluation", ItemSet.OrganizationsEvaluatedForCreditControl.Caption);
			var hint = @"This registry enables you to exclude one or more Organization Types from credit controlled document delivery restriction evaluations based on Job Type, Direction, Mode, INCO Term and Freight Payment Term. 

By default, 'ADB – ALL Debtors' organization type will be excluded from the credit evaluation for all job types. You can override the default setup and adjust the configuration in accordance with your needs.

Note: If the registry 'Accounting > Credit Controlled Documents Configuration > Enable Credit Control Evaluation for Controlling Customers and Controlling Agents' is set to 'Yes'. Both Controlling Customer and Controlling Agent will be taken into consideration in the credit control evaluation for 'SHP – Shipment', 'QSH – Quick Booking' and 'BRK – Declaration Job' job types and you will be able to exclude the credit evaluation of these organization types in this registry. ";
			AssertEquals("Hint", hint, ItemSet.OrganizationsEvaluatedForCreditControl.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.OrganizationsEvaluatedForCreditControl.Storage);

			AssertCollectionNotContains(JobInvoicingConsumerTypes.AgentBookingCode, collection);
			AssertCollectionNotContains(JobInvoicingConsumerTypes.TransportBookingCode, collection);
		}

		public void TestCreditLimitApprovalMode()
		{
			AssertEquals("Name", "CreditControlApprovalMode", ItemSet.CreditControlApprovalMode.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.CreditControlApprovalMode.Category);
			AssertEquals("Caption", "Credit Control Approval Mode", ItemSet.CreditControlApprovalMode.Caption);
			AssertEquals("Hint", @$"The default value for this registry is ‘CW1 – Approve requests in CW1’. When using this mode, {Core.Constants.ProductName} will evaluate all credit checks using data in its own database OR from synchronous external web service calls to external systems. Approvals of these requests are made within {Core.Constants.ProductName} always.

When this registry is set to ‘EXT’, {Core.Constants.ProductName} will submit credit control requests to an external system. Responses to these requests will be received by {Core.Constants.ProductName} and the requests will be updated accordingly.
The mechanism for this communication is outbound Universal Shipment messages and inbound Universal Event messages.
This method should only be used if your external systems can evaluate the credit status of all relevant parties on the shipment and can respond to {Core.Constants.ProductName} with an approval or rejection response.", ItemSet.CreditControlApprovalMode.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditControlApprovalMode.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CreditControlApprovalMode.Options);
			AssertEquals("Default value", AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveLocallyInCW1.Code, ItemSet.CreditControlApprovalMode.DefaultValue);
		}

		public void TestCreditLimitCacheExpiryPeriodRegistryItem()
		{
			AssertEquals("Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.Options);
			AssertEquals("Default value", 15, AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.Value);
		}

		public void TestCreditControllerOverrideThreshold()
		{
			AssertEquals("Name", "CreditControllerOverrideThreshold", ItemSet.CreditControllerOverrideThreshold.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration/Local Credit Limit", ItemSet.CreditControllerOverrideThreshold.Category);
			AssertEquals("Caption", "Credit Controller Override Threshold", ItemSet.CreditControllerOverrideThreshold.Caption);
			AssertEquals("Hint", @"Use this registry setting to set a percentage or amount threshold in which an authorized user can override credit controls based on the exceeded value of the client's credit limit.
The authorizing levels (up to 3) allows users with specific authorization levels to override credit controls that are set up in the security settings.", ItemSet.CreditControllerOverrideThreshold.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditControllerOverrideThreshold.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.CreditControllerOverrideThreshold.Options);
		}

		public void TestGlobalCreditControllerOverrideThreshold()
		{
			AssertEquals("Name", "GlobalCreditControllerOverrideThreshold", ItemSet.GlobalCreditControllerOverrideThreshold.Name);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration/Global Credit Limit", ItemSet.GlobalCreditControllerOverrideThreshold.Category);
			AssertEquals("Caption", "Global Credit Controller Override Threshold", ItemSet.GlobalCreditControllerOverrideThreshold.Caption);
			AssertEquals("Hint", @"Use this registry setting to set a percentage or amount threshold in which an authorized user can override credit controls based on the exceeded value of global credit group's credit limit.
The authorizing levels (up to 3) allows users with specific authorization levels to override credit controls that are set up in the security settings.", ItemSet.GlobalCreditControllerOverrideThreshold.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.GlobalCreditControllerOverrideThreshold.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.GlobalCreditControllerOverrideThreshold.Options);
		}

		public void TestDebtorGlobalCreditLimitNotifyGroup()
		{
			AssertEquals("Name", "DebtorGlobalCreditLimitNotifyGroup", ItemSet.DebtorGlobalCreditLimitNotifyGroup.Name);
			AssertEquals("Category", "Accounting/Email Notification", ItemSet.DebtorGlobalCreditLimitNotifyGroup.Category);
			AssertEquals("Caption", "Global AR Control Breach Notify Group", ItemSet.DebtorGlobalCreditLimitNotifyGroup.Caption);
			AssertEquals("Hint", "Notify Party when the Global Credit Limit of any Global Credit Control Group has been breached.", ItemSet.DebtorGlobalCreditLimitNotifyGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.DebtorGlobalCreditLimitNotifyGroup.Storage);

			var testGroup = Guid.NewGuid();
			ItemSet.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testGroup);
			AssertEquals("DebtorGlobalCreditLimitNotifyGroup", testGroup, ItemSet.DebtorGlobalCreditLimitNotifyGroup.GetFallBackValueAtAllLevels(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestEnableCreditControlEvaluationForControllingCustomersAndControllingAgents()
		{
			AssertEquals("Caption", "Enable Credit Control Evaluation for Controlling Customers and Controlling Agents", ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Caption);
			AssertEquals("Category", "Accounting/Credit Controlled Documents Configuration", ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Category);
			AssertEquals("DefaultValue", false, ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.DefaultValue);
			AssertEquals("Hint", @"By default, the credit status of the Controlling Customer and Controlling Agent will not be taken into consideration during the credit controlled document delivery restriction evaluation. 

When this registry is set to 'Yes', the system will include the Controlling Customer and Controlling Agent in the credit control evaluation for 'SHP – Shipment', 'QSH – Quick Booking' and 'BRK – Declaration Job' job types. Further, the 'CTP - Controlling Customer' and 'CAG - Controlling Agent' organization types will be available for configuration in the 'Accounting > Credit Controlled Documents Configuration > Organizations Excluded from Credit Control Evaluation' registry. 

Note: When this registry is set from 'Yes' to 'No', then any 'CTP - Controlling Customer' and 'CAG - Controlling Agent' rows configured in the 'Organizations Excluded from Credit Control Evaluation' registry will be removed. These changes will only be reflected when you have closed and re-open the registry. ", ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Hint);
			AssertEquals("Name", "EnableCreditControlEvaluationForControllingCustomersAndControllingAgents", ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.Storage);

			var registry = ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEnableCreditControlEvaluationForControllingCustomersAndControllingAgents_OnUpdateAction()
		{
			var registry = ItemSet.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents;
			registry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var collection = new OrgsEvaluatedForCreditControlCollection();
			var ctp = createOrgsEvaluatedForCreditControl();
			ctp.OrganizationType = OrgCodes.ControllingCustomer;
			var cag = createOrgsEvaluatedForCreditControl();
			cag.OrganizationType = OrgCodes.ControllingAgent;
			var adb = createOrgsEvaluatedForCreditControl();
			adb.OrganizationType = OrgCodes.AllDebtors;
			collection.Add(ctp);
			collection.Add(cag);
			collection.Add(adb);

			ItemSet.OrganizationsEvaluatedForCreditControl.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			AssertEquals(3, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).Count);
			registry.OnUpdateAction(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(1, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).Count);

			ItemSet.OrganizationsEvaluatedForCreditControl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ItemSet.OrganizationsEvaluatedForCreditControl.Inner.DeleteRecord(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			registry.OnUpdateAction(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(1, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).Count);
			AssertEquals(3, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Count);

			registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.OrganizationsEvaluatedForCreditControl.Inner.DeleteRecord(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			registry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(3, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).Count);
			AssertEquals(1, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Count);

			ItemSet.OrganizationsEvaluatedForCreditControl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertEquals(3, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Count);
			registry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(3, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).Count);
			AssertEquals(1, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Count);

			registry.Inner.DeleteRecord(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			registry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(1, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).Count);
			AssertEquals(1, ItemSet.OrganizationsEvaluatedForCreditControl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Count);

			OrgsEvaluatedForCreditControl createOrgsEvaluatedForCreditControl()
			{
				var orgsEvaluatedForCreditControl = new OrgsEvaluatedForCreditControl();
				orgsEvaluatedForCreditControl.Mode = Constants.TransportModes.All;
				orgsEvaluatedForCreditControl.INCOTerm = INCOTermCodes.All;
				orgsEvaluatedForCreditControl.FreightPaymentTerm = FreightPaymentTermCodes.All;
				orgsEvaluatedForCreditControl.DirectionCode = Constants.FreightShipmentDirection.Code.All;
				orgsEvaluatedForCreditControl.JobType = JobInvoicingConsumerTypes.ShipmentCode;
				return orgsEvaluatedForCreditControl;
			}
		}

		public void TestOrphanWIPorAcrDetection()
		{
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForDevelopers, AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.Value);
		}

		public void TestAutoRateDateByChargeGroupSetup()
		{
			var newValue = new AutoRateDateByChargeGroupConfiguration();
			var copy = newValue.AutoRateDateByChargeGroups.AddNew();
			copy.ChargeGroup = "ITC";
			var setting = copy.ChargeGroupSettings.AddNew();
			setting.JobType = "SHP";
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			setting.Mode = "AIR";
			setting.DateType = JobDateTypes.Codes.ArrivalDate;

			ItemSet.AutoRateDateByChargeGroupSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			var setup = ItemSet.AutoRateDateByChargeGroupSetup.Value.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().First(x => x.ChargeGroup == "ITC");
			AssertEquals(1, setup.ChargeGroupSettings.Count);
			AssertEquals("SHP", setup.ChargeGroupSettings[0].JobType);
			AssertEquals(Constants.FreightShipmentDirection.Code.Import, setup.ChargeGroupSettings[0].DirectionCode);
			AssertEquals("AIR", setup.ChargeGroupSettings[0].Mode);
			AssertEquals(JobDateTypes.Codes.ArrivalDate, setup.ChargeGroupSettings[0].DateType);
		}

		public void TestCollectionAndDemandRegistryItemDefaultValues()
		{
			AssertEquals("CollectionAndDemandFirstReminderDocumentName.DefaultValue", "REQUEST FOR IMMEDIATE PAYMENT", ItemSet.CollectionAndDemandFirstReminderDocumentName.DefaultValue);
			AssertEquals("CollectionAndDemandSecondReminderDocumentName.DefaultValue", "2ND REQUEST FOR IMMEDIATE PAYMENT", ItemSet.CollectionAndDemandSecondReminderDocumentName.DefaultValue);
			AssertEquals("CollectionLetterDocumentName.DefaultValue", "COLLECTION LETTER", ItemSet.CollectionLetterDocumentName.DefaultValue);
			AssertEquals("DemandLetterDocumentName.DefaultValue", "LETTER OF DEMAND", ItemSet.DemandLetterDocumentName.DefaultValue);
			AssertEquals("StatementDocumentName.DefaultValue", "STATEMENT OF ACCOUNT", ItemSet.StatementDocumentName.DefaultValue);

			string firstReminderOpeningText =
				"The transactions below have not yet been paid." + System.Environment.NewLine +
				"Under the credit arrangements agreed by you with our company, these transactions should have been settled by now." + System.Environment.NewLine +
				"Please pay promptly within the next SEVEN DAYS." + System.Environment.NewLine +
				"Please contact me directly if you require copies of any outstanding item." + System.Environment.NewLine +
				"Please disregard this notice if payment has been made in the past 7 days.";

			string secondReminderOpeningText =
				"By our records, the transactions below remain unpaid." + System.Environment.NewLine +
				"Please pay promptly within the next SEVEN DAYS." + System.Environment.NewLine +
				"Under the credit arrangements agreed by you with our company, these transactions should have been settled by now." + System.Environment.NewLine +
				"Please contact me directly if you require copies of any outstanding item." + System.Environment.NewLine +
				"If payment has been made, please contact me and advise the details of that payment.";

			string collectionOpeningText =
				"Please pay the transactions detailed below within the next SEVEN DAYS." + System.Environment.NewLine + System.Environment.NewLine +
				"Under the credit arrangements agreed by you with our company, these transactions require immediate payment." + System.Environment.NewLine +
				"Copies of each outstanding item can be provided on request." + System.Environment.NewLine +
				"If payment for any of the listed items has already been made, please contact me immediately and advise the details of that payment.";

			string demandOpeningText =
				"Please be advised that the transactions detailed below require immediate payment." + System.Environment.NewLine +
				"If payment is not received within SEVEN DAYS we will commence legal debt collection procedures." + System.Environment.NewLine + System.Environment.NewLine +
				"Under the credit arrangements agreed by you with our company, these transactions require IMMEDIATE PAYMENT." + System.Environment.NewLine + System.Environment.NewLine +
				"If payment for any of the listed items has already been made, please contact me immediately and advise the details of that payment.";

			AssertEquals("CollectionAndDemandFirstReminderOpeningText.DefaultValue", firstReminderOpeningText, ItemSet.CollectionAndDemandFirstReminderOpeningText.DefaultValue);
			AssertEquals("CollectionAndDemandSecondReminderOpeningText.DefaultValue", secondReminderOpeningText, ItemSet.CollectionAndDemandSecondReminderOpeningText.DefaultValue);
			AssertEquals("CollectionLetterOpeningText.DefaultValue", collectionOpeningText, ItemSet.CollectionLetterOpeningText.DefaultValue);
			AssertEquals("DemandLetterOpeningText.DefaultValue", demandOpeningText, ItemSet.DemandLetterOpeningText.DefaultValue);
		}

		public void TestSettingAndGettingCollectionAndDemandRegistryItems()
		{
			ItemSet.CollectionAndDemandFirstReminderDocumentName.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "First Reminder Doc");
			ItemSet.CollectionAndDemandSecondReminderDocumentName.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Second Reminder Doc");
			ItemSet.CollectionLetterDocumentName.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Collection Doc");
			ItemSet.DemandLetterDocumentName.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Demand Doc");
			ItemSet.StatementDocumentName.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Statement Doc");

			ItemSet.CollectionAndDemandFirstReminderOpeningText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "First Reminder");
			ItemSet.CollectionAndDemandSecondReminderOpeningText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Second Reminder");
			ItemSet.CollectionLetterOpeningText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Collection");
			ItemSet.DemandLetterOpeningText.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Demand");

			AssertEquals("CollectionAndDemandFirstReminderDocumentName.Value", "First Reminder Doc", ItemSet.CollectionAndDemandFirstReminderDocumentName.Value);
			AssertEquals("CollectionAndDemandSecondReminderDocumentName.Value", "Second Reminder Doc", ItemSet.CollectionAndDemandSecondReminderDocumentName.Value);
			AssertEquals("CollectionLetterDocumentName.Value", "Collection Doc", ItemSet.CollectionLetterDocumentName.Value);
			AssertEquals("DemandLetterDocumentName.Value", "Demand Doc", ItemSet.DemandLetterDocumentName.Value);
			AssertEquals("StatementDocumentName.Value", "Statement Doc", ItemSet.StatementDocumentName.Value);

			AssertEquals("CollectionAndDemandFirstReminderOpeningText.Value", "First Reminder", ItemSet.CollectionAndDemandFirstReminderOpeningText.Value);
			AssertEquals("CollectionAndDemandSecondReminderOpeningText.Value", "Second Reminder", ItemSet.CollectionAndDemandSecondReminderOpeningText.Value);
			AssertEquals("CollectionLetterOpeningText.Value", "Collection", ItemSet.CollectionLetterOpeningText.Value);
			AssertEquals("DemandLetterOpeningText.Value", "Demand", ItemSet.DemandLetterOpeningText.Value);
		}

		public void TestDocumentReceivedDateDefaultingLogic()
		{
			AssertEquals("Caption", "Document Received Date Defaulting Logic", ItemSet.DocumentReceivedDateDefaultingLogic.Caption);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.DocumentReceivedDateDefaultingLogic.Category);
			AssertEquals("DefaultValue", DocReceivedDateDefaultLogics.Code.Blank, ItemSet.DocumentReceivedDateDefaultingLogic.DefaultValue);
			AssertEquals("Hint", @"By default, the document received date will be set to blank.
If required, you can set the 'create date' or the 'invoice date' as the document received date.", ItemSet.DocumentReceivedDateDefaultingLogic.Hint);
			AssertEquals("Name", "DocumentReceivedDateDefaultingLogic", ItemSet.DocumentReceivedDateDefaultingLogic.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.DocumentReceivedDateDefaultingLogic.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.DocumentReceivedDateDefaultingLogic.Storage);

			var registry = ItemSet.DocumentReceivedDateDefaultingLogic;
			var oldValue = DocReceivedDateDefaultLogics.Code.Blank;
			var newValue = DocReceivedDateDefaultLogics.Code.InvoiceDate;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = ItemSet.DocumentReceivedDateDefaultingLogic.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestDocumentReceivedDateMustBeEntered()
		{
			AssertEquals("Caption", "Document Received Date Must Be Entered", ItemSet.DocumentReceivedDateMustBeEntered.Caption);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.DocumentReceivedDateMustBeEntered.Category);
			AssertEquals("DefaultValue", false, ItemSet.DocumentReceivedDateMustBeEntered.DefaultValue);
			AssertEquals("Hint", @"When overridden to ‘Yes’, users will be required to record a document received date against each Payable Invoice.
By default, the recording of document received date is optional.", ItemSet.DocumentReceivedDateMustBeEntered.Hint);
			AssertEquals("Name", "DocumentReceivedDateMustBeEntered", ItemSet.DocumentReceivedDateMustBeEntered.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.DocumentReceivedDateMustBeEntered.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.DocumentReceivedDateMustBeEntered.Storage);

			var registry = ItemSet.DocumentReceivedDateMustBeEntered;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = ItemSet.DocumentReceivedDateMustBeEntered.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestInvoiceDateDefaultValue()
		{
			AssertEquals("Caption", "Invoice Date Default Value", ItemSet.InvoiceDateDefaultValue.Caption);
			AssertEquals("Category", Categories.Accounting_PayableDefaults_DefaultSettings, ItemSet.InvoiceDateDefaultValue.Category);
			AssertEquals("DefaultValue", AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.CurrentDate, ItemSet.InvoiceDateDefaultValue.DefaultValue);
			AssertEquals("Hint", @"This registry defines the default Invoice Date value for Payable Invoice (INV), Credit Note (CRD) and Adjustment Note (ADJ) transactions.

By default, the registry value is ‘ADD – Invoice Add Date’ and the Invoice Date defaults to the date the invoice is added.

Set the registry to ‘BLK - Blank’ if you do not wish to default the invoice date. This option forces the user to enter the required Invoice Date.

Note: when this registry is set to ‘BLK’ this also has action when importing AP invoices via XUT, where the field 'Transaction Date' is not allowed to be empty.", ItemSet.InvoiceDateDefaultValue.Hint);
			AssertEquals("Name", "InvoiceDateDefaultValue", ItemSet.InvoiceDateDefaultValue.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.InvoiceDateDefaultValue.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.InvoiceDateDefaultValue.Storage);
		}

		public void TestAPInvoiceDueDateCalculationRule()
		{
			AssertEquals("Caption", "AP Invoice Due Date Calculation Rule", ItemSet.APInvoiceDueDateCalculationRule.Caption);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.APInvoiceDueDateCalculationRule.Category);
			AssertEquals("DefaultValue", false, ItemSet.APInvoiceDueDateCalculationRule.DefaultValue);
			AssertEquals("Hint", @"By default, the AP Invoice Due Date for 'INV', 'MTH', 'PER', 'COD' and 'PIA' payment terms will be calculated base on Invoice Date.
If you change the registry value to 'Yes', the calculation rule will be based on Document Received Date.
Note: The calculation rule of 'SHP' and 'CUS' are not affected by this registry setting.", ItemSet.APInvoiceDueDateCalculationRule.Hint);
			AssertEquals("Name", "APInvoiceDueDateCalculationRule", ItemSet.APInvoiceDueDateCalculationRule.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.APInvoiceDueDateCalculationRule.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.APInvoiceDueDateCalculationRule.Storage);

			var registry = ItemSet.APInvoiceDueDateCalculationRule;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = ItemSet.APInvoiceDueDateCalculationRule.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEnableReportingBooksFeature()
		{
			AssertEquals("Caption", "Enable Reporting Books Feature (CWSupport Only)", ItemSet.EnableReportingBooksFeature.Caption);
			AssertEquals("Category", "Accounting/Reporting Books", ItemSet.EnableReportingBooksFeature.Category);
			AssertEquals("DefaultValue", false, ItemSet.EnableReportingBooksFeature.DefaultValue);
			AssertEquals("Hint", @"This feature is currently under development,
This feature enables users to configure one or more reporting books to meet different reporting needs from a single source of truth.
With this feature, users will be able to:
1. Create one or more alternate reports with different layout and totalling rules with alpha-numeric alternate GL accounts.
2. Dissect single GL Account using one or more transaction's attributes to report account balance in more granular level
3. Group multiple GL Accounts to a single Alternate GL Accounts to report accounts balances at higher level
4. Translate the GL Accounts balances from your local currency to a foreign currency
5. Translate and report the GL Accounts balances using your head office period setting", ItemSet.EnableReportingBooksFeature.Hint);
			AssertEquals("Name", "EnableReportingBooksFeature", ItemSet.EnableReportingBooksFeature.Name);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableReportingBooksFeature.Options);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableReportingBooksFeature.Storage);

			var registry = ItemSet.EnableReportingBooksFeature;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = ItemSet.EnableReportingBooksFeature.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEnableReportingBooksFeature_DefaultValue()
		{
			AssertEquals("DefaultValue", false, ItemSet.EnableReportingBooksFeature.DefaultValue);

			ItemSet.RemoveItemFromCacheIfOlderThan("EnableReportingBooksFeature", TimeSpan.MinValue);

			var mockIFeatureData = new Mock<IFeatureData>();
			var reportingBookFeatureControlData = new ReportingBookFeatureControlData
			{
				EnableReportingBooksFeature = false,
			};
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);

			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingReportingBookFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals("DefaultValue will be false when feature control EnableReportingBooksFeature is false.", false, ItemSet.EnableReportingBooksFeature.DefaultValue);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("EnableReportingBooksFeature", TimeSpan.MinValue);

			reportingBookFeatureControlData.EnableReportingBooksFeature = true;
			mockIFeatureData = new Mock<IFeatureData>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);
			mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingReportingBookFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals("DefaultValue will be true when feature control EnableReportingBooksFeature is true.", true, ItemSet.EnableReportingBooksFeature.DefaultValue);
			}
		}

		public void TestStatementUsePrintStreaming()
		{
			AssertEquals("Caption", "Statement Use Print Streaming", ItemSet.StatementUsePrintStreaming.Caption);
			AssertEquals("Category", "Accounting/Receivable Defaults/Form Configurations/Collection & Demand Letters/Statement", ItemSet.StatementUsePrintStreaming.Category);
			AssertEquals("DefaultValue", true, ItemSet.StatementUsePrintStreaming.DefaultValue);
			AssertEquals("Hint", "Statements to use new Print Streaming for better memory utilization.", ItemSet.StatementUsePrintStreaming.Hint);
			AssertEquals("Name", "StatementUsePrintStreaming", ItemSet.StatementUsePrintStreaming.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.StatementUsePrintStreaming.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.StatementUsePrintStreaming.Storage);
		}

		public void TestInvoiceUsePrintStreaming()
		{
			AssertEquals("Caption", "Invoice Use Print Streaming", ItemSet.InvoiceUsePrintStreaming.Caption);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.InvoiceUsePrintStreaming.Category);
			AssertEquals("DefaultValue", true, ItemSet.InvoiceUsePrintStreaming.DefaultValue);
			AssertEquals("Hint", "Invoices to use new Print Streaming for better memory utilization.", ItemSet.InvoiceUsePrintStreaming.Hint);
			AssertEquals("Name", "InvoiceUsePrintStreaming", ItemSet.InvoiceUsePrintStreaming.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.InvoiceUsePrintStreaming.Options);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.InvoiceUsePrintStreaming.Storage);
		}

		public void TestEnableGovernmentChargeCode()
		{
			AssertEquals("Caption", "Enable Government Reporting Charge Code Behavior", ItemSet.EnableGovernmentChargeCode.Caption);
			AssertEquals("Category", "Accounting", ItemSet.EnableGovernmentChargeCode.Category);
			AssertEquals("DefaultValue", false, ItemSet.EnableGovernmentChargeCode.DefaultValue);
			AssertEquals("Hint", @"Government Reporting Charge Codes are an additional, secondary item of information used to classify each revenue or cost charge line using a Government defined code.

When this registry is set to 'Yes' charge line related Government Reporting Charge Code features are surfaced: Charge Codes can be assigned a default Government Code; that code defaults against Sell and Cost charges as they are entered; the government code recorded against a charge line is printed in the invoice and included in the invoice XML.

When set to ""No"" these features are disabled.

These features are relevant in countries when the charge line details of each Sale and Purchase Invoice must be classified and reported to government agencies using government specified reporting codes.", ItemSet.EnableGovernmentChargeCode.Hint);
			AssertEquals("Name", "EnableGovernmentChargeCode", ItemSet.EnableGovernmentChargeCode.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnableGovernmentChargeCode.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnableGovernmentChargeCode.Storage);
		}

		public void TestEnableGovernmentChargeCode_DefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertTestEnableGovernmentChargeCode_DafaultValues(Constants.CountryCodes.Australia, "AT1", "AB1", false);
				AssertTestEnableGovernmentChargeCode_DafaultValues(Constants.CountryCodes.India, "IT1", "IB1", true);
				AssertTestEnableGovernmentChargeCode_DafaultValues(Constants.CountryCodes.Brazil, "BT1", "BB1", false);
				AssertTestEnableGovernmentChargeCode_DafaultValues(Constants.CountryCodes.Mexico, "MT1", "MB1", true);
			});

			void AssertTestEnableGovernmentChargeCode_DafaultValues(ZString countryCode, ZString companyCode, ZString branchCode, ZBool expectedValue)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Code = companyCode;
				company.GC_RN_NKCountryCode = countryCode;

				var branch = company.Branches.AddNew();
				branch.GB_Code = branchCode;

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("DefaultValue", expectedValue, ItemSet.EnableGovernmentChargeCode.DefaultValue);
				}
			}
		}

		public void TestPaymentRejectionReasonList()
		{
			string expectedHint = @"The Reason Codes listed in this registry are used to classify the reason a Payment Request is being rejected.
A reason code is mandatory when rejecting AR and AP payment requests.
Reason codes are visible in the AR/AP Payment Processing module, on the Payment event logs, and are included in the Notification Email sent back to the requester.";

			var registryItem = ItemSet.PaymentRejectionReasonCodesList;
			AssertEquals("Caption", "Payment Request Rejection Reason Codes", registryItem.Caption);
			AssertEquals("Category", Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing, registryItem.Category);
			AssertEquals("Hint", expectedHint, registryItem.Hint);
			AssertEquals("Max Length", 3, registryItem.Value.MaxCodeLength);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, registryItem.Storage);

			AssertEquals(registryItem.Value.CodesAsString, "DIS, INS, INC, INA");

			var newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("AAA", "test aaa"));
			newlist1.Add(new CodeDescriptionPair("BBB", "test bbb"));
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertEquals(registryItem.Value.CodesAsString, "AAA, BBB");

			var newlist2 = new CodeDescriptionPairList();
			newlist2.Add(new CodeDescriptionPair("CCC", "test ccc"));
			newlist2.Add(new CodeDescriptionPair("DDD", "test ddd"));
			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertEquals(registryItem.Value.CodesAsString, "CCC, DDD");

			AssertEquals(AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).CodesAsString, "AAA, BBB");
			AssertEquals(AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).CodesAsString, "CCC, DDD");
		}

		public void TestCreditNoteReasonCodesList()
		{
			string expectedHint = @"The Reason Codes listed here are used to classify the reason an Accounts Receivable credit note is being raised.
A reason code is mandatory on credit note approval request.
Reason codes are visible in the Credit Notes Approval Module";

			AssertEquals("Caption", "Credit Note Approval Reason Codes", ItemSet.CreditNoteReasonCodesList.Caption);
			AssertEquals("Category", AccountingMasterFilesRegistry.Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting, ItemSet.CreditNoteReasonCodesList.Category);
			AssertEquals("Hint", expectedHint, ItemSet.CreditNoteReasonCodesList.Hint);
			AssertEquals("Max Length", 3, ItemSet.CreditNoteReasonCodesList.Value.MaxCodeLength);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.CreditNoteReasonCodesList.Storage);

			AssertEquals(ItemSet.CreditNoteReasonCodesList.Value.CodesAsString, "IOB, IRA, ICH, IJD, IDA, DAM, DSC, LDL");
			CodeDescriptionPairList newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("AAA", "test aaa"));
			newlist1.Add(new CodeDescriptionPair("BBB", "test bbb"));
			ItemSet.CreditNoteReasonCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertEquals(ItemSet.CreditNoteReasonCodesList.Value.CodesAsString, "AAA, BBB");

			CodeDescriptionPairList newlist2 = new CodeDescriptionPairList();
			newlist2.Add(new CodeDescriptionPair("CCC", "test ccc"));
			newlist2.Add(new CodeDescriptionPair("DDD", "test ddd"));
			ItemSet.CreditNoteReasonCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertEquals(ItemSet.CreditNoteReasonCodesList.Value.CodesAsString, "CCC, DDD");

			AssertEquals(AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).CodesAsString, "AAA, BBB");
			AssertEquals(AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).CodesAsString, "CCC, DDD");
		}

		public void TestSimulateGBOutOfEU()
		{
			AssertEquals("Caption", "Testing Only – GB no longer included in the EU VAT area", ItemSet.SimulateGBOutOfEU.Caption);
			AssertEquals("Category", AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ItemSet.SimulateGBOutOfEU.Category);
			AssertEquals("DefaultValue", false, ItemSet.SimulateGBOutOfEU.DefaultValue);
			var expectedHint = @"Only use in your TEST environment – do not use in your Production environment.

Nominate whether to effect behaviors to mimic GB (United Kingdom) having exited the EU (European Union).
While this field set as Off, your test system will operate with GB continuing as part of the EU for VAT purposes.

When set On, you can test data entry to verify the effect of GB not being part of the EU for VAT purposes.
This behavior can be checked for Receivables (AR) and Payables (AP)  INV/CRN/ADJ as well as Cashbook (DRC/DPY) data entry.

Once GB actually leaves the EU, the value of this registry will be ignored.";
			AssertEquals("Hint", expectedHint, ItemSet.SimulateGBOutOfEU.Hint);
			AssertEquals("Name", "SimulateGBOutOfEU", ItemSet.SimulateGBOutOfEU.Name);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.SimulateGBOutOfEU.Options);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.SimulateGBOutOfEU.Storage);
		}

		public void TestBranchManagementCodes()
		{
			var item = ItemSet.BranchManagementCodes;
			AssertEquals("Name", "BranchManagementCodes", item.Name);
			AssertEquals("Category", Enterprise.MasterFiles.Business.AccountingMasterFilesRegistry.Categories.Accounting, item.Category);
			AssertEquals("Caption", "Branch Management Codes", item.Caption);
			var expectedHint = "Branch Management Codes are used to group multiple branches together for management purposes." +
														"\r\n\r\n" +
														"Note: If any change is made to the 'Branch Management Codes' of the branches, please, close and reopen the registry form.";
			AssertEquals("Hint", expectedHint, item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Bool Caption", "Active", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("No Default Value", 0, ItemSet.BranchManagementCodes.DefaultValue.Count);
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "ComplianceReportsSetupsUserDefined";
				yield return "EnableComplianceDocumentModule";
				yield return "ComplianceDocumentRePrintRestriction";
				yield return "ComplianceDocumentSupportingReasonsReceivables";
				yield return "EnforceReceivablesComplianceDocumentDateandNumberSequencing";
				yield return "ComplianceDocumentSupportingReasonsPayables";
				yield return "ComplianceDocumentSupportingDocumentType";
				yield return "DocumentConfiguration";
				yield return "GatewayBillingChargeCodes";
				yield return "AllowNegativeComplianceDocumentLines";
				yield return "PromptToPrintComplianceDocumentOnCreation";
				yield return "TaxInvoiceStatusUpdateAutomatedRequestSchedule";
				yield return "APListAutomatedRequestSchedule";
				yield return "SupplyTypeClassificationCodesIsMandatory";
				yield return "SupplyTypeClassificationCodesList";
				yield return "EnableTaxBranchReporting";
				yield return "DefaultPaymentReference";
				yield return "GLJournalExchangeRateDifferenceAccount";
				yield return "TaxTypeToTaxInvoiceDocumentTypeCodeMapping";
				yield return "JournalEntriesLastProcessedDate";
				yield return "SAFTGroupingCategory";
				yield return "D365Credentials";
				yield return "D365WebserviceURL";
			}
		}

		public void TestMaxOccurrencesBeforeReportJHBranchIsNull()
		{
			AssertEquals("DefaultValue must be 5", 5, ItemSet.MaxOccurrencesBeforeReportJHBranchIsNull.DefaultValue);

			AssertEquals("Category must be Accounting", "Accounting", ItemSet.MaxOccurrencesBeforeReportJHBranchIsNull.Category);
			AssertEquals("Options must be IsOnlyForSupport", RegistryOptions.IsOnlyForSupport, ItemSet.MaxOccurrencesBeforeReportJHBranchIsNull.Options);
			AssertEquals("Storage must be System", RegistryStorageFlags.System, ItemSet.MaxOccurrencesBeforeReportJHBranchIsNull.Storage);

			ItemSet.MaxOccurrencesBeforeReportJHBranchIsNull.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals("Value must be 10", 10, ItemSet.MaxOccurrencesBeforeReportJHBranchIsNull.Value);
		}

		public void TestJHBranchIsNullOccurrenceCounter()
		{
			AssertEquals("DefaultValue must be 0", 0, ItemSet.JHBranchIsNullOccurrenceCounter.DefaultValue);
			AssertEquals("Category must be Accounting", "Accounting", ItemSet.JHBranchIsNullOccurrenceCounter.Category);
			AssertEquals("Options must be IsHidden", RegistryOptions.IsHidden, ItemSet.JHBranchIsNullOccurrenceCounter.Options);
			AssertEquals("Storage must be System", RegistryStorageFlags.System, ItemSet.JHBranchIsNullOccurrenceCounter.Storage);

			ItemSet.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals("Value must be 10", 10, ItemSet.JHBranchIsNullOccurrenceCounter.Value);

			ItemSet.JHBranchIsNullOccurrenceCounter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals("Value must be 0", 0, ItemSet.JHBranchIsNullOccurrenceCounter.Value);
		}

		#region E-Payment Registries

		public void TestOFXOAuthLoginWebURL_ProductionSystem() => AssertOFXOAuthLoginWebURL(true);

		public void TestOFXOAuthLoginWebURL_NonProductionSystem() => AssertOFXOAuthLoginWebURL(false);

		void AssertOFXOAuthLoginWebURL(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);

			var itemSet = GetNewItemSet();
			AssertEquals("OFXOAuthLoginWebURL", itemSet.OFXOAuthLoginWebURL.Name);
			AssertEquals("Accounting/E-Payment Configurations", itemSet.OFXOAuthLoginWebURL.Category);
			AssertEquals("OFX OAuth login web URL (CargoWiseOne Support Only)", itemSet.OFXOAuthLoginWebURL.Caption);
			AssertEquals(@"OFX OAuth login web URL.
IMPORTANT: Please do not change this registry value from default.", itemSet.OFXOAuthLoginWebURL.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.OFXOAuthLoginWebURL.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.OFXOAuthLoginWebURL.Options);

			if (isProductionSystem)
			{
				AssertEquals("https://live.api.ofx.com/v1/oauth/authorize", GetNewItemSet().OFXOAuthLoginWebURL.DefaultValue);
			}
			else
			{
				AssertEquals("https://sandbox.api.ofx.com/v1/oauth/authorize", itemSet.OFXOAuthLoginWebURL.DefaultValue);
			}
		}

		public void TestOFXOAuthWebURLEnvironment_ProductionSystem() => AssertOFXOAuthWebURLEnvironment(true);

		public void TestOFXOAuthWebURLEnvironment_NonProductionSystem() => AssertOFXOAuthWebURLEnvironment(false);

		void AssertOFXOAuthWebURLEnvironment(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);

			var itemSet = GetNewItemSet();
			AssertEquals("OFXOAuthWebURLEnvironment", itemSet.OFXOAuthWebURLEnvironment.Name);
			AssertEquals("Accounting/E-Payment Configurations", itemSet.OFXOAuthWebURLEnvironment.Category);
			AssertEquals("OFX OAuth Web URL Uses Production Environment(CargoWiseOne Support Only)", itemSet.OFXOAuthWebURLEnvironment.Caption);
			AssertEquals(@"Indicate OFX OAuth login web URL is for production environment or not. 'YES' means production, 'NO' means test.
IMPORTANT: Please do not change this registry value from default.", itemSet.OFXOAuthWebURLEnvironment.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.OFXOAuthWebURLEnvironment.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.OFXOAuthWebURLEnvironment.Options);

			if (isProductionSystem)
			{
				Assert(GetNewItemSet().OFXOAuthWebURLEnvironment.DefaultValue);
			}
			else
			{
				Assert(!GetNewItemSet().OFXOAuthWebURLEnvironment.DefaultValue);
			}
		}

		public void TestWTCCallbackSiteWebURL_ProductionSystem() => AssertWTCCallbackSiteWebURL(true);

		public void TestWTCCallbackSiteWebURL_NonProductionSystem() => AssertWTCCallbackSiteWebURL(false);

		void AssertWTCCallbackSiteWebURL(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);

			var itemSet = GetNewItemSet();
			AssertEquals("WTCCallbackSiteWebURL", itemSet.WTCCallbackSiteWebURL.Name);
			AssertEquals("Accounting/E-Payment Configurations", itemSet.WTCCallbackSiteWebURL.Category);
			AssertEquals("WTC Callback site web URL (CargoWiseOne Support Only)", itemSet.WTCCallbackSiteWebURL.Caption);
			AssertEquals(@"WTC Callback site web URL.", itemSet.WTCCallbackSiteWebURL.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.WTCCallbackSiteWebURL.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.WTCCallbackSiteWebURL.Options);

			if (isProductionSystem)
			{
				AssertEquals("https://ofxcallback.wisegrid.net", itemSet.WTCCallbackSiteWebURL.DefaultValue);
			}
			else
			{
				AssertEquals("https://ofxcallback-test.wisegrid.net", itemSet.WTCCallbackSiteWebURL.DefaultValue);
			}
		}

		public void TestEnableEPaymentFunctionality()
		{
			AssertEquals("EnableEPaymentFunctionality", ItemSet.EnableEPaymentFunctionality.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.EnableEPaymentFunctionality.Category);
			AssertEquals("Enable E-Payment Functionality", ItemSet.EnableEPaymentFunctionality.Caption);
			AssertEquals(@"This registry controls E-Payment functionality.E-Payments are global foreign payments processed through integrated third party service providers. 

CargoWise only relays messages and payment information, all foreign exchange quotes and payment transfers are provided and executed by the third party service providers.

Currently, CargoWise supports direct integration with OFX - OzForex Limited (www.ofx.com).

Unless overridden, the default value of the registry is set to Enabled for supported countries and set to Disabled for the countries not yet supported. For the login companies in the supported countries, this registry allows you to disable the integrated payments functionality, if you do not wish to use it.", ItemSet.EnableEPaymentFunctionality.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EnableEPaymentFunctionality.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.EnableEPaymentFunctionality.Options);

			AssertDefaultEnableEPaymentFunctionalityValue(Constants.CountryCodes.Australia, nameof(Constants.CountryCodes.Australia), true);
			AssertDefaultEnableEPaymentFunctionalityValue(Constants.CountryCodes.Afghanistan, nameof(Constants.CountryCodes.Afghanistan), false);
		}

		public void TestBeneficiarySearchResultHttpResponsePageSize()
		{
			AssertEquals("BeneficiarySearchResultHttpResponsePageSize", ItemSet.BeneficiarySearchResultHttpResponsePageSize.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.BeneficiarySearchResultHttpResponsePageSize.Category);
			AssertEquals("Beneficiary search result Http response page size", ItemSet.BeneficiarySearchResultHttpResponsePageSize.Caption);
			AssertEquals("This registry sets the maximum number of records returned when a beneficiary search request is sent to an electronic payment provider (e.g. OFX). This is achieved by setting this registry as the value of pagesize parameter while calling the API from XT.", ItemSet.BeneficiarySearchResultHttpResponsePageSize.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.BeneficiarySearchResultHttpResponsePageSize.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.BeneficiarySearchResultHttpResponsePageSize.Options);
			AssertEquals(100, ItemSet.BeneficiarySearchResultHttpResponsePageSize.DefaultValue);
		}

		public void TestMaximumNumberOfBeneficiaryInSearchResultXUE()
		{
			AssertEquals("MaximumNumberOfBeneficiaryInSearchResultXUE", ItemSet.MaximumNumberOfBeneficiaryInSearchResultXUE.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.MaximumNumberOfBeneficiaryInSearchResultXUE.Category);
			AssertEquals("Maximum Number of Beneficiary in a Beneficiary Search Result XUE", ItemSet.MaximumNumberOfBeneficiaryInSearchResultXUE.Caption);
			AssertEquals("This registry sets the maximum number of beneficiary details that a Beneficiary search result XUE can contain.", ItemSet.MaximumNumberOfBeneficiaryInSearchResultXUE.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.MaximumNumberOfBeneficiaryInSearchResultXUE.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.MaximumNumberOfBeneficiaryInSearchResultXUE.Options);
			AssertEquals(500, ItemSet.MaximumNumberOfBeneficiaryInSearchResultXUE.DefaultValue);
		}

		void AssertDefaultEnableEPaymentFunctionalityValue(string countryCode, string countryDescription, bool isOFXEPaymentEnabled)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var defaultValue = ItemSet.EnableEPaymentFunctionality.DefaultValue;
				AssertEquals("Only one default value exist per company.", 1, defaultValue.Count);
				AssertEquals(countryCode, defaultValue[0].CountryCode);
				AssertEquals(countryDescription, defaultValue[0].CountryDescription);
				AssertEquals(isOFXEPaymentEnabled, defaultValue[0].OFXEPaymentEnabled);
			}
		}

		public void TestOFXWebURL()
		{
			AssertEquals("OFXWebURL", ItemSet.OFXWebURL.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.OFXWebURL.Category);
			AssertEquals("OFX Web URL (CargoWiseOne Support Only)", ItemSet.OFXWebURL.Caption);
			AssertEquals("OFX Web URL.", ItemSet.OFXWebURL.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.OFXWebURL.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.OFXWebURL.Options);
			AssertEquals("https://www.ofx.com", ItemSet.OFXWebURL.DefaultValue);
		}

		public void TestOFXLogo()
		{
			AssertEquals("OFXLogo", ItemSet.OFXLogo.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.OFXLogo.Category);
			AssertEquals("OFX Logo (CargoWiseOne Support Only)", ItemSet.OFXLogo.Caption);
			AssertEquals("OFX Logo.", ItemSet.OFXLogo.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.OFXLogo.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.OFXLogo.Options);
			AssertNotNull(ItemSet.OFXLogo.DefaultValue);
		}

		public void TestEPaymentProductMarketingWebURL()
		{
			AssertEquals("EPaymentProductMarketingWebURL", ItemSet.EPaymentProductMarketingWebURL.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.EPaymentProductMarketingWebURL.Category);
			AssertEquals("E-Payment Product Marketing Web URL (CargoWiseOne Support Only)", ItemSet.EPaymentProductMarketingWebURL.Caption);
			AssertEquals("E-Payment Product Marketing Web URL.", ItemSet.EPaymentProductMarketingWebURL.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EPaymentProductMarketingWebURL.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EPaymentProductMarketingWebURL.Options);
			AssertEquals("https://cargowise.com/solutions/cargowise-enterprise/accounting/integrated-global-payments/?utm_source=cargowise&utm_medium=demo&utm_campaign=ofx", ItemSet.EPaymentProductMarketingWebURL.DefaultValue);
		}

		public void TestRebookExpiredQuotesBasedOnExRateTolerance()
		{
			AssertEquals("RebookExpiredQuotesBasedOnExRateTolerance", ItemSet.RebookExpiredQuotesBasedOnExRateTolerance.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.RebookExpiredQuotesBasedOnExRateTolerance.Category);
			AssertEquals("Re-book Expired Quotes Based on Ex Rate Tolerance (CargoWiseOne Support Only)", ItemSet.RebookExpiredQuotesBasedOnExRateTolerance.Caption);

			var expectedHint = @"Once a payment has been approved, by default, changes to exchange rate/local amount of the payment will reset the approval status back to ""Awaiting Approval"". If your business is comfortable to accept small fluctuations in exchange rates, without having to re-approve the payment, you can configure the ""Exchange Rate Tolerance"". When a payment is in Approved status, and the only thing that is changed on the payment is the exchange rate/local amount, the approval status will NOT be reset if the change is within the accepted tolerance. To configure Exchange Rate Tolerance, navigate to the registry Accounting > Payable Defaults > Default Settings > Payment Processing > Exchange Rate Tolerance.

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
* If the new quote is WITHIN the allowed tolerance, then the payment will be booked for processing. The new quote will be automatically accepted, and the payment details updated. The E-Payment Status will change to ACP – Accepted.";

			AssertEquals(expectedHint, ItemSet.RebookExpiredQuotesBasedOnExRateTolerance.Hint);

			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.RebookExpiredQuotesBasedOnExRateTolerance.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.RebookExpiredQuotesBasedOnExRateTolerance.Options);
			AssertEquals(false, ItemSet.RebookExpiredQuotesBasedOnExRateTolerance.DefaultValue);
		}

		public void TestDefaultPaymentReason()
		{
			AssertEquals("DefaultPaymentReason", ItemSet.DefaultPaymentReason.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.DefaultPaymentReason.Category);
			AssertEquals("Default Payment Reason", ItemSet.DefaultPaymentReason.Caption);
			AssertEquals("Select the Payment Reason to default as the reason for making foreign currency payments via FX integration.", ItemSet.DefaultPaymentReason.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.DefaultPaymentReason.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.DefaultPaymentReason.Options);
			var defaultValue = ItemSet.DefaultPaymentReason.DefaultValue;
			AssertEquals(1, defaultValue.Count);
			AssertNotNull(defaultValue.Cast<DefaultEPaymentReason>().FirstOrDefault(r => r.ProviderCode == EPaymentProviderCodes.Codes.OFX && r.ReasonCode == "SVT" && r.ReasonDescription == "Services trade"));
		}

		public void TestDefaultPaymentReference()
		{
			AssertEquals("DefaultPaymentReference", ItemSet.DefaultPaymentReference.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.DefaultPaymentReference.Category);
			AssertEquals("Default Payment Reference", ItemSet.DefaultPaymentReference.Caption);
			AssertEquals(@"Payment Reference will be included in the details sent to the recipients bank for display with the deposit when making payments via FX integration.
Payment Reference can be free text (e.g. your company name), invoice numbers being paid, or the Payment Reference Number.
Value entered in this registry will default to a Payables organization's Account Details when adding an E-Payment method, where it may be overridden if required.", ItemSet.DefaultPaymentReference.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.DefaultPaymentReference.Storage);
			AssertEquals(1, ItemSet.DefaultPaymentReference.DefaultValue.Count);
			AssertEquals(RegistryOptions.Default, ItemSet.DefaultPaymentReference.Options);
			var defaultValue = ItemSet.DefaultPaymentReference.DefaultValue;
			AssertEquals(1, defaultValue.Count);
			AssertEquals(defaultValue[0].ProviderCode, EPaymentProviderCodes.Codes.OFX);
			AssertEquals(defaultValue[0].ReferenceType, "INV");
		}

		public void TestPromptBankTransferOnPostingEPayment()
		{
			var item = ItemSet.PromptBankTransferOnPostingEPayment;
			AssertEquals("PromptBankTransferOnPostingEPayment", item.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, item.Category);
			AssertEquals("Prompt Bank Transfer on Posting E-Payment", item.Caption);
			AssertEquals("When this registry is set to Yes, on posting of an E-Payment, you will be presented with a Bank Transfer screen to create a bank transfer from the bank account that 'funded' the FX payment to the E-Payment bank account.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(true, item.DefaultValue);
		}

		public void TestEPaymentReasons()
		{
			AssertEquals("EPaymentReasons", ItemSet.PaymentReasons.Name);
			AssertEquals(Categories.Accounting_EPaymentConfigurations, ItemSet.PaymentReasons.Category);
			AssertEquals("Payment Reasons", ItemSet.PaymentReasons.Caption);
			AssertEquals("This registry is used to record accepted Payment Reasons by FX Provider.", ItemSet.PaymentReasons.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.PaymentReasons.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.PaymentReasons.Options);
			var defaultValue = ItemSet.PaymentReasons.DefaultValue.Cast<EPaymentReason>();
			AssertEquals(7, defaultValue.Count());
			var ofxReasons = defaultValue.Where(r => r.ProviderCode == EPaymentProviderCodes.Codes.OFX);
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "ACS" && r.ReasonDescription == "Accounting services"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "BCS" && r.ReasonDescription == "Business consultancy and PR Services"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "EPS" && r.ReasonDescription == "Employee payment, salary/wages"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "GPP" && r.ReasonDescription == "Goods payment, purchase"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "HCI" && r.ReasonDescription == "Hardware consultancy/implementation"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "SCI" && r.ReasonDescription == "Software consultancy/implementation"));
			AssertNotNull(ofxReasons.FirstOrDefault(r => r.ReasonCode == "SVT" && r.ReasonDescription == "Services trade"));
		}

		#endregion

		public void TestEnableAssetManagementFunctionality()
		{
			var item = ItemSet.EnableAssetManagementFunctionality;
			AssertNotNull(item);
			AssertEquals("EnableAssetManagementFunctionality", item.Name);
			AssertEquals("Accounting/Asset Management", item.Category);
			AssertEquals("Enable Asset Management Functionality", item.Caption);
			AssertEquals(@"This registry allows you to activate the new Asset Management module.
**PLEASE DO NOT ENABLE THIS REGISTRY FOR ANY CUSTOMER**", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(false, item.DefaultValue);
		}

		public void TestCanUserEditAssetCode()
		{
			var item = ItemSet.CanUserEditAssetCode;
			AssertNotNull(item);
			AssertEquals("CanUserEditAssetCode", item.Name);
			AssertEquals("Accounting/Asset Management", item.Category);
			AssertEquals("Asset Codes can be edited", item.Caption);
			AssertEquals("Can users edit asset codes?", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(false, item.DefaultValue);
		}

		#region E-Invoicing Registries

		[TestDate(2019, 8, 9)]
		public void TestEnableEInvoicingFunctionality()
		{
			var item = ItemSet.EnableEInvoicingFunctionality;
			AssertEquals("EnableEInvoicingFunctionality", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("Enable E-Reporting Functionality - Receivables", item.Caption);
			AssertEquals(@$"This registry is used to control the E-Reporting functionality for Receivables. When enabled, all new eligible transactions are automatically transmitted to the relevant Government authority in an approved electronic format.

Unless overridden, the default value of the registry will automatically switch from No to Yes for supported countries.

NOTE: For Taiwan, this feature is only relevant if you have enabled the compliance document module.

IMPORTANT:

It is strongly recommended that you DO NOT DISABLE E-Reporting functionality for supported countries.

This registry ensures that you are fully compliant with the local E-Reporting/E-Invoicing requirements without interrupting your normal existing processes. As you post transactions they are transferred directly from {Core.Constants.ProductName} to the government authority via the eHub platform, which provides the necessary encoding and secure exchange of messages. eHub processes government responses and sends updates back to {Core.Constants.ProductName}, so you can see the status of each transaction without leaving the system. As a result:
* You do not need to maintain mapping of {Core.Constants.ProductName} data via a third party.
* Mapped data is validated at source before being transmitted to the local government.
* Status of each transaction is reflected in {Core.Constants.ProductName} and you know which transactions must be corrected/re-sent.
* You do not need to spend time reconciling E-Reporting transactions between multiple systems.",
				item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(false, item.DefaultValue);

			var factory = new BusinessObjectFactory();
			var companyWithOverrideNoValueGuid = factory.NewWithValidTestData<GlbCompany>().PK.ToGuid();
			var companyWithOverrideYesValueGuid = factory.NewWithValidTestData<GlbCompany>().PK.ToGuid();
			var companyWithoutOverrideGuid = factory.NewWithValidTestData<GlbCompany>().PK.ToGuid();
			factory.Save();

			var currentDateForCountry = AccountingMasterFilesUtils.GetMaxLocationDateTimeByCountryCode(factory, Constants.CountryCodes.Australia);
			var itemDate = ItemSet.EReportingComplianceDate;

			using (item.SetTemporaryValue(companyWithOverrideNoValueGuid, Guid.Empty, Guid.Empty, false))
			using (item.SetTemporaryValue(companyWithOverrideYesValueGuid, Guid.Empty, Guid.Empty, true))
			{
				using (itemDate.SetTemporaryValue(companyWithOverrideNoValueGuid, Guid.Empty, Guid.Empty, currentDateForCountry.AddDays(-10).ToDateTime()))
				using (itemDate.SetTemporaryValue(companyWithOverrideYesValueGuid, Guid.Empty, Guid.Empty, currentDateForCountry.AddDays(-10).ToDateTime()))
				using (itemDate.SetTemporaryValue(companyWithoutOverrideGuid, Guid.Empty, Guid.Empty, currentDateForCountry.AddDays(-10).ToDateTime()))
				{
					Assert(!item.GetValueWithoutFallback(companyWithOverrideNoValueGuid, Guid.Empty, Guid.Empty));
					Assert(item.GetValueWithoutFallback(companyWithOverrideYesValueGuid, Guid.Empty, Guid.Empty));
					Assert(item.GetValueWithoutFallback(companyWithoutOverrideGuid, Guid.Empty, Guid.Empty));
				}

				using (itemDate.SetTemporaryValue(companyWithOverrideNoValueGuid, Guid.Empty, Guid.Empty, currentDateForCountry.AddDays(10).ToDateTime()))
				using (itemDate.SetTemporaryValue(companyWithOverrideYesValueGuid, Guid.Empty, Guid.Empty, currentDateForCountry.AddDays(10).ToDateTime()))
				using (itemDate.SetTemporaryValue(companyWithoutOverrideGuid, Guid.Empty, Guid.Empty, currentDateForCountry.AddDays(10).ToDateTime()))
				{
					Assert(!item.GetValueWithoutFallback(companyWithOverrideNoValueGuid, Guid.Empty, Guid.Empty));
					Assert(item.GetValueWithoutFallback(companyWithOverrideYesValueGuid, Guid.Empty, Guid.Empty));
					Assert(!item.GetValueWithoutFallback(companyWithoutOverrideGuid, Guid.Empty, Guid.Empty));
				}

				using (itemDate.SetTemporaryValue(companyWithOverrideNoValueGuid, Guid.Empty, Guid.Empty, currentDateForCountry.ToDateTime()))
				using (itemDate.SetTemporaryValue(companyWithOverrideYesValueGuid, Guid.Empty, Guid.Empty, currentDateForCountry.ToDateTime()))
				using (itemDate.SetTemporaryValue(companyWithoutOverrideGuid, Guid.Empty, Guid.Empty, currentDateForCountry.ToDateTime()))
				{
					Assert(!item.GetValueWithoutFallback(companyWithOverrideNoValueGuid, Guid.Empty, Guid.Empty));
					Assert(item.GetValueWithoutFallback(companyWithOverrideYesValueGuid, Guid.Empty, Guid.Empty));
					Assert(item.GetValueWithoutFallback(companyWithoutOverrideGuid, Guid.Empty, Guid.Empty));
				}
			}

			var supportedCountries = new List<string> {
				Constants.CountryCodes.Taiwan,
				Constants.CountryCodes.VietNam,
				Constants.CountryCodes.China,
				Constants.CountryCodes.KoreaSouth,
				Constants.CountryCodes.Romania,
				Constants.CountryCodes.Malaysia,
			};
			Guid currCompGuid;
			var today = ZDateTime.Today;
			var tomorrow = today.AddDays(1);
			foreach (var country in supportedCountries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					currCompGuid = GlbCompany.CurrentCompany.PK.ToGuid();
					using (item.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("compliance date is set today", today, itemDate.GetFallBackValueAtAllLevels(currCompGuid, Guid.Empty, Guid.Empty));
					}
				}

				currCompGuid = GlbCompany.CurrentCompany.PK.ToGuid();
				using (itemDate.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, tomorrow.ToDateTime()))
				using (item.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("compliance date is not set today", tomorrow, itemDate.GetFallBackValueAtAllLevels(currCompGuid, Guid.Empty, Guid.Empty));
				}
			}
		}

		public void TestDelayTimeForRequeueInvoices()
		{
			var item = ItemSet.DelayTimeForRequeueInvoices;
			AssertEquals("DelayTimeForRequeueInvoices", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("E-Reporting Delay Time For Re-queue Invoice With SNT Status (CW1 Support Only)", item.Caption);
			AssertEquals(@"By default, users will be able to re-queue invoices with SNT status at any time if they have the relevant security right for all countries except for China login companies.

For China login companies, the delay time is set to 30 mins by default to allow sufficient time for the system to process the initial transmission and update the status. Then, if the initial transmission is not successful after 30 minutes, authorised users will be able to re-queue the invoice with SNT status for transmission.
For all other countries, you may override the delay time from 0 to x mins if needed.

Note: This registry is only relevant to countries where E-Reporting has been enabled.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue, item.Options);

			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(item
				, x => x.EInvoicingRequeueDelayTime
				, ("1", 1)
				, ("2", 2)
				, "E-Reporting Delay Time For Re-queue Invoice With SNT Status (CW1 Support Only)");
		}

		[TestedType(typeof(DelayTimeForRequeueInvoicesType))]
		class DelayTimeForRequeueInvoicesTypeTest : RegistryDataTypeTestCase<DelayTimeForRequeueInvoicesType>
		{
			protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
			{
				return new[]
				{
					new ValidSampleAndBinaryValueInDB(1, Encoding.Unicode.GetBytes("1")),
					new ValidSampleAndBinaryValueInDB(1440, Encoding.Unicode.GetBytes("1440")),
				};
			}

			protected override object[] GetInvalidSamples()
			{
				return new object[] { -1, 1441 };
			}

			protected override DelayTimeForRequeueInvoicesType GetNewDataType()
			{
				return new DelayTimeForRequeueInvoicesType();
			}

			public void TestValidation()
			{
				var dataType = new DelayTimeForRequeueInvoicesType();
				var registryItem = new IntRegistryItem(new CountrySpecificDefaultValueRegistryItemImpl<int>("test", null, null, dataType, RegistryOptions.IsOnlyForSupport, new BusinessObjectFactory(), new DelayTimeForRequeueInvoices_RegistryDescriptor()));

				AssertNoExceptionThrown(() => dataType.Validate(registryItem, 0, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertNoExceptionThrown(() => dataType.Validate(registryItem, 1, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertNoExceptionThrown(() => dataType.Validate(registryItem, 1440, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertExceptionThrown(typeof(RegistryValidationException), "Delay time must between '0' and '1440'.", () => dataType.Validate(registryItem, -1, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertExceptionThrown(typeof(RegistryValidationException), "Delay time must between '0' and '1440'.", () => dataType.Validate(registryItem, 1441, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public void TestEnableGovernmentAllocatedNumberBehavior()
		{
			var item = ItemSet.EnableGovernmentAllocatedNumberBehavior;
			AssertEquals("EnableGovernmentAllocatedNumberBehavior", item.Name);
			AssertEquals(Categories.Accounting_PayableDefaults_DefaultSettings, item.Category);
			AssertEquals("Enable Government Allocated Number Behavior", item.Caption);
			AssertEquals(@"Government Allocated Number is the unique identifier assigned by the government through government channels when receiving and verifying electronic invoices.
When this registry is set to ""Yes"", manual entry of the Government Allocation number is allowed in the Payables Module during the posting of a New Invoice, Credit note, or Adjustment Note.
When it's set to ""No"" these features are disabled.

NOTE: When this registry is set to Yes, this also has an effect on the following area :
When importing AP invoices via XUT, if the 'GovernmentAllocatedID' field, which is used for the Government Allocation Number, is filled out, the value is imported into AH_GovernmentAllocatedID.

ALERT : This registry must be used in conjunction with the required validation for each country; currently, it is enabled only for Israel by default.
Please Consult the Accounting product team before enabling this registry.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue, item.Options);

			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(item
				, x => x.EnableGovernmentAllocatedNumberBehavior
				, ("False", false)
				, ("True", true)
				, "Enable Government Allocated Number Behavior");
		}

		public void TestUseVATRegistrationNumberAsOrganizationMatchingCriteria()
		{
			TestGenericRegistryItem(ItemSet.UseVATRegistrationNumberAsOrganizationMatchingCriteria,
				nameof(ItemSet.UseVATRegistrationNumberAsOrganizationMatchingCriteria),
				Categories.Accounting,
				"Use VAT Registration Number as Organization Matching Criteria",
				@"When any of the contexts are enabled, the organization's VAT registration number will be added as a matching criterion for enabled context(s) in the XML import logic when identifying the organization related to the imported transaction.
It will be the last level of fallback when Organization address or Organization code is not found.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default);

			AssertType<CodeDescriptionBoolRegistryEditorInfo>(ItemSet.UseVATRegistrationNumberAsOrganizationMatchingCriteria.EditorInfo);
			var editor = (CodeDescriptionBoolRegistryEditorInfo)ItemSet.UseVATRegistrationNumberAsOrganizationMatchingCriteria.EditorInfo;
			Assert("Checkbox should be visible", editor.IsBoolColumnVisible);
			Assert("Code and description should be read only", editor.IsOnlyBoolColumnEditable);

			Assert("All Context Types are unselected by default.", ItemSet.UseVATRegistrationNumberAsOrganizationMatchingCriteria.DefaultValue.Cast<CodeDescriptionBool>().All(x => !x.Bool));
			var actualSequence = ItemSet.UseVATRegistrationNumberAsOrganizationMatchingCriteria.DefaultValue.Cast<CodeDescriptionBool>().Select(x => x.Code.ToString()).ToArray();
			var expectedSequence = new[] { "PTR", "RTR" };
			AssertSequencesEqual(expectedSequence, actualSequence);
		}

		[TestDate(2022, 7, 2)]
		public void TestEnableEInvoicingFunctionalityForPayables()
		{
			var item = ItemSet.EnableEInvoicingFunctionalityForPayables;
			AssertEquals("EnableEInvoicingFunctionalityForPayables", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("Enable E-Reporting Functionality - Payables", item.Caption);
			AssertEquals(@"This registry is used to control the E-Reporting functionality for Payables.
When enabled, all new eligible transactions are automatically transmitted to the relevant Government authority in an approved electronic format.

Unless overridden, the default value of the registry will automatically switch from No to Yes for supported countries.

NOTE: E-Reporting for Payables is currently supported only in Italy login companies.

IMPORTANT:
It is strongly recommended that you DO NOT DISABLE E-Reporting for Payables functionality for supported countries.
This registry ensures that you are fully compliant with the local E-Reporting/E-Invoicing requirements without interrupting your normal existing processes.
See ""Enable E-Reporting Functionality"" item for further details.",
				item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(false, item.DefaultValue);

			var countries = Factory.Load<RefCountry>(new ZQuery());
			var currComp = GlbCompany.CurrentCompany;
			string[] automaticallyEnabled = { Constants.CountryCodes.Italy, Constants.CountryCodes.Spain };
			foreach (var country in countries)
			{
				if (Array.Exists(automaticallyEnabled, x => x == country.RN_Code))
				{
					using (currComp.TemporarilySetCountry(country.RN_Code))
					{
						var complianceInfo = CountryComplianceFactory.GetICountryComplianceEInvoice(country.RN_Code);
						if (complianceInfo != null)
						{
							AssertEquals($"Expected compliance date for {country.RN_Desc}. EInvoicing compliance date for payables was {complianceInfo.GetEInvoicingComplianceDateForPayables()}", true, item.Value);
						}
					}
				}
				else
				{
					using (currComp.TemporarilySetCountry(country.RN_Code))
					{
						AssertEquals($"Expected compliance date for {country.RN_Desc}.", false, item.Value);
					}
				}
			}
		}

		public void TestOnBuildLogReferenceForEnableEInvoicingFunctionality()
		{
			IRegistryItem regItem = Instance.EnableEInvoicingFunctionality.Inner;

			AssertOnBuildLogReferenceForEnableEInvoicingFunctionality(false, true);
			AssertOnBuildLogReferenceForEnableEInvoicingFunctionality(true, false);

			void AssertOnBuildLogReferenceForEnableEInvoicingFunctionality(bool oldValue, bool newValue)
			{
				var expectedLogMessage = $"E-Reporting Functionality = [{newValue}].";
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, oldValue, newValue);
				var logReference = Instance.EnableEInvoicingFunctionality.OnBuildLogReference(args);
				AssertEquals(expectedLogMessage, logReference);
			}
		}

		public void TestEInvoicingPendingTransactionsNotificationGroup()
		{
			var item = ItemSet.EInvoicingPendingTransactionsNotificationGroup;

			AssertEquals("EInvoicingPendingTransactionsNotificationGroup", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("Transactions Pending for E-Reporting Notification Group", item.Caption);
			AssertEquals(@"This registry only applies when the default Receivables E-Reporting Submit Pivot is set to 'PEN - Pending'.

The system will notify the party when transactions are pending for transmission based on the day calculation of the date criteria you have configured.
By default, no notification will be sent. If required, you can nominate a group, set the date criteria, and the days to be alerted of the pending transactions. The system will send the notification as per configuration.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company | RegistryStorageFlags.Branch, item.Storage);
		}

		public void TestOnBuildLogReferenceForEInvoicingPendingTransactionsNotificationGroup()
		{
			var group = Factory.Load<GlbGroup>(GlbGroup.BackupOperatorGroupPK);
			var expectedLogMessage = $"Set notification group [{group.GG_Code}], date option [PST] and [233] days.";

			var newValue = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = GlbGroup.BackupOperatorGroupPK,
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate,
				Days = 233
			};
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.EInvoicingPendingTransactionsNotificationGroup.Inner, null, newValue);
			var logReference = Instance.EInvoicingPendingTransactionsNotificationGroup.OnBuildLogReference(args);

			AssertEquals(expectedLogMessage, logReference);

			args = new RegistryItemWrapper.BuildLogReferenceArgs(Instance.EInvoicingPendingTransactionsNotificationGroup.Inner, newValue, new EInvoicingPendingTransactionsNotificationGroup());
			logReference = Instance.EInvoicingPendingTransactionsNotificationGroup.OnBuildLogReference(args);

			AssertEquals("Set notification group [], date option [] and [0] days.", logReference);
		}

		public void TestEReportingComplianceDate()
		{
			var item = ItemSet.EReportingComplianceDate;
			AssertEquals("EReportingComplianceDate", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("E-Reporting Compliance Date - Receivables (CargoWiseOne Support Only)", item.Caption);
			AssertEquals(@"This registry is used to control the date on which the E-Reporting functionality for Receivables gets enabled in the supported countries.

Where E-Reporting for Receivables is supported, the default state of the registry ""Enable E-Reporting Functionality - Receivables"" will change from No to Yes on the Compliance Date shown in this registry.

For countries where E-Reporting for Receivables functionality is not currently supported, the Compliance Date is blank.

NOTE: For Taiwan, this date will be set to today's date when 'Enable E-Reporting Functionality - Receivables' registry is set to 'Yes'.", item.Hint);
			var itemEditInfo = item.EditorInfo as DateTimeRegistryEditorInfo;
			AssertNotNull(itemEditInfo);
			AssertEquals(ZDateTimePickerFormat.Short, itemEditInfo.DateTimeFormat);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(DateTime.MinValue, item.DefaultValue);

			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(x => x.GetIComplianceInfoElectronicInvoicing(It.IsAny<ZString>())).Returns(() => null);
			ObjectFactory.Substitute(countryComplianceFactoryMock.Object);

			AssertEquals("Expected Compliance date when country has no implement CountryComplianceFactory", DateTime.MinValue, item.Value);

			var complianceInfoEInvoicingMock = new Mock<IComplianceInfoElectronicInvoicing>();
			complianceInfoEInvoicingMock.Setup(x => x.GetEInvoicingComplianceDate()).Returns(ZDate.Empty);
			countryComplianceFactoryMock.Setup(x => x.GetIComplianceInfoElectronicInvoicing(It.IsAny<ZString>())).Returns(complianceInfoEInvoicingMock.Object);

			AssertEquals("Expected Compliance date when country has no default value", DateTime.MinValue, item.Value);

			var expectedDate = new ZDate(2022, 07, 22);
			complianceInfoEInvoicingMock.Setup(x => x.GetEInvoicingComplianceDate()).Returns(expectedDate);

			AssertEquals("Expected Compliance date when country has default value", expectedDate, item.Value);
		}

		public void TestEReportingComplianceDateNewSchema()
		{
			var item = ItemSet.EReportingComplianceDateNewSchema;
			AssertEquals("EReportingComplianceDateNewSchema", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("E-Reporting Compliance Date - New Schema for Receivables", item.Caption);
			AssertEquals(@"This registry is used to control the transition to a new schema when you are logged into a country environment where the government allows a switch period between an old schema and a new one.

For countries where only one schema exists the date will be empty.", item.Hint);
			var itemEditInfo = item.EditorInfo as DateTimeRegistryEditorInfo;
			AssertNotNull(itemEditInfo);
			AssertEquals(ZDateTimePickerFormat.Short, itemEditInfo.DateTimeFormat);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(DateTime.MinValue, item.DefaultValue);

			var expectedDateByCountry = new Dictionary<ZString, DateTime>()
			{
				{ Constants.CountryCodes.Italy, new DateTime(2021, 1, 1) },
				{ Constants.CountryCodes.Hungary, new DateTime(2021, 4, 1) }
			};
			var countries = Factory.Load<RefCountry>(new ZQuery());
			var currComp = GlbCompany.CurrentCompany;
			foreach (var country in countries)
			{
				using (currComp.TemporarilySetCountry(country.RN_Code))
				{
					var expectedDate = DateTime.MinValue;
					expectedDateByCountry.TryGetValue(country.RN_Code, out expectedDate);
					AssertEquals($"Expected Compliance date for {country.RN_Desc}", expectedDate, item.Value);
				}
			}
		}

		public void TestEReportingComplianceDateForPayables()
		{
			var item = ItemSet.EReportingComplianceDateForPayables;
			AssertEquals("EReportingComplianceDateForPayables", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("E-Reporting Compliance Date - Payables (CargoWiseOne Support Only)", item.Caption);
			AssertEquals(@"This registry is used to control the date on which the E-Reporting functionality for Payables gets enabled in the supported countries.

The default Compliance Date is blank for all countries, and it can be overriden with Date From value only in countries where e-Reporting functionality is supported.

NOTE: E-Reporting for Payables is currently supported only in Italy login companies.", item.Hint);
			var itemEditInfo = item.EditorInfo as DateTimeRegistryEditorInfo;
			AssertNotNull(itemEditInfo);
			AssertEquals(ZDateTimePickerFormat.Short, itemEditInfo.DateTimeFormat);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(DateTime.MinValue, item.DefaultValue);

			var countries = Factory.Load<RefCountry>(new ZQuery());
			var currComp = GlbCompany.CurrentCompany;
			foreach (var country in countries)
			{
				using (currComp.TemporarilySetCountry(country.RN_Code))
				{
					AssertEquals(
						$"Expected Compliance date for {country.RN_Desc}",
						GetExpectedDate(country.RN_Code),
						item.Value);
				}
			}

			DateTime GetExpectedDate(ZString rnCode)
			{
				var expectedDateByCountry = new Dictionary<ZString, DateTime>()
				{
					{ Constants.CountryCodes.Italy, new DateTime(2022, 7, 1) },
					{ Constants.CountryCodes.Spain, new DateTime(2022, 4, 1) }, //Only for Non-Production
				};

				var expectedDate = DateTime.MinValue;
				expectedDateByCountry.TryGetValue(rnCode, out expectedDate);
				return expectedDate;
			}
		}

		public void TestTransactionAuthorizationNumberDate()
		{
			var item = ItemSet.TransactionAuthorizationNumberDate;
			AssertEquals("TransactionAuthorizationNumberDate", item.Name);
			AssertEquals(Categories.Accounting, item.Category);
			AssertEquals("Transaction Authorization Number Date (CargoWiseOne Support Only)", item.Caption);
			AssertEquals(@"This registry is used to control the date on wich the Transaction Authorization Number functionality gets enabled in the supported countries.
The default state of the registry will change from No to Yes on the Compliance Date shown in the registry.
For countries where the functionality is not currently supported, the Compliance Date is blank.

In Portugal, this feature is named ATCUD", item.Hint);
			var itemEditInfo = item.EditorInfo as DateTimeRegistryEditorInfo;
			AssertNotNull(itemEditInfo);
			AssertEquals(ZDateTimePickerFormat.Short, itemEditInfo.DateTimeFormat);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(DateTime.MaxValue, item.DefaultValue);

			var expectedDateByCountry = new Dictionary<ZString, DateTime>()
			{
				{ Constants.CountryCodes.Portugal, new DateTime(2023, 1, 1) }
			};
			var countries = Factory.Load<RefCountry>(new ZQuery());
			var currComp = GlbCompany.CurrentCompany;
			foreach (var country in countries)
			{
				using (currComp.TemporarilySetCountry(country.RN_Code))
				{
					DateTime expectedDate;
					if (!expectedDateByCountry.TryGetValue(country.RN_Code, out expectedDate))
					{
						expectedDate = DateTime.MaxValue;
					}
					AssertEquals($"Expected Compliance date for {country.RN_Desc}", expectedDate, item.Value);
				}
			}
		}

		public void TestEReportingSubmitPivotDefaultStatus()
		{
			AssertEquals("EReportingSubmitPivotDefaultStatus", ItemSet.EReportingSubmitPivotDefaultStatus.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.EReportingSubmitPivotDefaultStatus.Category);
			AssertEquals("E-Reporting Submit Pivot Default Status - Receivables (CargoWiseOne Support Only)", ItemSet.EReportingSubmitPivotDefaultStatus.Caption);
			AssertEquals(@"This registry is used to control the status of the Submit Pivot created or re-queued for eligible to E-Reporting Receivables transactions.

Where E-Reporting for Receivables is supported, the default status could be set to ""QUE"" or ""PEN"".

""QUE"" status indicates that transacttions are ready for sending.
""PEN"" status will need an extra action like digital signature to be changed to ""QUE"".

For countries where E-Reporting for Receivables functionality is not currently supported, the Default Status is blank.", ItemSet.EReportingSubmitPivotDefaultStatus.Hint);
			Assert(ItemSet.EReportingSubmitPivotDefaultStatus.EditorInfo is ComboBoxRegistryEditorInfo);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EReportingSubmitPivotDefaultStatus.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EReportingSubmitPivotDefaultStatus.Options);
			AssertEquals(string.Empty, ItemSet.EReportingSubmitPivotDefaultStatus.DefaultValue);
			var factory = new BusinessObjectFactory();
			var expectedStatusByCountry = new Dictionary<string, string>()
			{
				{ Constants.CountryCodes.Argentina, "QUE" },
				{ Constants.CountryCodes.Brazil, "QUE" },
				{ Constants.CountryCodes.Chile, "QUE" },
				{ Constants.CountryCodes.China, "QUE" },
				{ Constants.CountryCodes.Colombia, "QUE" },
				{ Constants.CountryCodes.CostaRica, "QUE" },
				{ Constants.CountryCodes.DominicanRepublic, "QUE" },
				{ Constants.CountryCodes.Egypt, "PEN" },
				{ Constants.CountryCodes.Fiji, "QUE" },
				{ Constants.CountryCodes.Germany, "QUE" },
				{ Constants.CountryCodes.Hungary, "QUE" },
				{ Constants.CountryCodes.India, "QUE" },
				{ Constants.CountryCodes.Israel, "QUE" },
				{ Constants.CountryCodes.Italy, "QUE" },
				{ Constants.CountryCodes.Jordan, "QUE" },
				{ Constants.CountryCodes.KoreaSouth, "QUE" },
				{ Constants.CountryCodes.Latvia, "QUE" },
				{ Constants.CountryCodes.Malaysia, "QUE" },
				{ Constants.CountryCodes.Mauritius, "QUE" },
				{ Constants.CountryCodes.Mexico, "QUE" },
				{ Constants.CountryCodes.Panama, "QUE" },
				{ Constants.CountryCodes.Philippines, "QUE" },
				{ Constants.CountryCodes.Poland, "QUE" },
				{ Constants.CountryCodes.Romania, "QUE" },
				{ Constants.CountryCodes.SaudiArabia, "QUE" },
				{ Constants.CountryCodes.Serbia, "QUE" },
				{ Constants.CountryCodes.Spain, "QUE" },
				{ Constants.CountryCodes.Turkey, "QUE" },
				{ Constants.CountryCodes.UnitedKingdom, "QUE" },
				{ Constants.CountryCodes.Uruguay, "QUE" },
				{ Constants.CountryCodes.VietNam, "QUE" },
				{ Constants.CountryCodes.WesternSamoa, "QUE" },
			};
			var countries = factory.Load<RefCountry>(new ZQuery());
			foreach (var country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.RN_Code))
				{
					((CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<string>)ItemSet.EReportingSubmitPivotDefaultStatus.Inner).ClearCache_ForTestOnly();
					expectedStatusByCountry.TryGetValue(country.RN_Code, out var expectedStatus);
					AssertEquals($"Expected Submit Pivot default status for {country.RN_Desc}", expectedStatus ?? string.Empty, ItemSet.EReportingSubmitPivotDefaultStatus.Value);
				}
			}
		}

		public void TestEReportingSubmitPivotDefaultStatusForPayables()
		{
			var registryItem = ItemSet.EReportingSubmitPivotDefaultStatusForPayables;
			AssertEquals("EReportingSubmitPivotDefaultStatusForPayables", registryItem.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, registryItem.Category);
			AssertEquals("E-Reporting Submit Pivot Default Status - Payables (CargoWiseOne Support Only)", registryItem.Caption);
			AssertEquals(@"This registry is used to control the status of the Submit Pivot created or re-queued for eligible E-Reporting Payables transactions.

Where E-Reporting for Payables is supported, the default status could be set to ""QUE"" or ""PEN"".

""QUE"" status indicates that a transaction is ready for sending.
""PEN"" status indicates that there is an extra action required before it can be queued.

For countries where E-Reporting for Payables functionality is not supported, the default status is ""Blank"".", registryItem.Hint);
			Assert(registryItem.EditorInfo is ComboBoxRegistryEditorInfo);
			AssertEquals(RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registryItem.Options);
			AssertEquals(string.Empty, registryItem.DefaultValue);
			var factory = new BusinessObjectFactory();
			var countries = factory.Load<RefCountry>(new ZQuery());
			var currComp = GlbCompany.CurrentCompany;
			foreach (var country in countries)
			{
				using (currComp.TemporarilySetCountry(country.RN_Code))
				{
					((EReportingSubmitPivotDefaultStatusForPayablesRegistryItemImpl)registryItem.Inner).ClearCache_ForTestOnly();
					var expectedStatus = country.RN_Code == Constants.CountryCodes.Italy ? "PEN" : string.Empty;
					AssertEquals($"Expected Submit Pivot default status for {country.RN_Desc}", expectedStatus, registryItem.Value);
				}
			}

			foreach (var country in countries)
			{
				using (currComp.TemporarilySetCountry(country.RN_Code))
				{
					((EReportingSubmitPivotDefaultStatusForPayablesRegistryItemImpl)registryItem.Inner).ClearCache_ForTestOnly();
					if (country.RN_Code == Constants.CountryCodes.Italy)
					{
						Instance.EReportingSubmitPivotDefaultStatusForPayablesOnlyItaly.SetTemporaryValue(currComp.EntityPK.ToGuid(), Guid.Empty, Guid.Empty, "QUE");
					}
					var expectedStatus = country.RN_Code == Constants.CountryCodes.Italy ? "QUE" : string.Empty;
					AssertEquals($"Expected Submit Pivot default status for {country.RN_Desc}", expectedStatus, registryItem.Value);
				}
			}
		}

		public void TestEReportingSubmitPivotDefaultStatusForPayablesOnlyItaly()
		{
			var registryItem = ItemSet.EReportingSubmitPivotDefaultStatusForPayablesOnlyItaly;
			AssertEquals("EReportingSubmitPivotDefaultStatusForPayablesOnlyItaly", registryItem.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Italy, registryItem.Category);
			AssertEquals("Default E-Reporting Status - Payables", registryItem.Caption);
			AssertEquals(@"This registry is used to control the default E-Reporting Status when E-reporting functionality for Payables is active.

Unless overridden, eligible transactions are posted in PEN - Pending status and require a user to review and queue the transaction for sending.

If you prefer to send the eligible transactions without requiring a manual action, change the value of this registry to QUE - Queued. Eligible transactions will be queued for sending as soon as posted.", registryItem.Hint);
			Assert(registryItem.EditorInfo is ComboBoxRegistryEditorInfo);
			AssertEquals(RegistryStorageFlags.Company, registryItem.Storage);
			AssertEquals(RegistryOptions.Default, registryItem.Options);
			AssertEquals(Constants.EInvoicingPivotState.Pending, registryItem.DefaultValue);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertEquals($"Expected Submit Pivot default status", "PEN", registryItem.Value);
			}
		}

		public void TestEReportingPivotPendingStatusDescription()
		{
			AssertEquals("EReportingPivotPendingStatusDescription", ItemSet.EReportingPivotPendingStatusDescription.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, ItemSet.EReportingPivotPendingStatusDescription.Category);
			AssertEquals("E-Reporting Pivot Pending Status Description (CargoWiseOne Support Only)", ItemSet.EReportingPivotPendingStatusDescription.Caption);
			AssertEquals(@"This registry is used to control description of the Pivot's Pending status.

Where E-Reporting is supported and default status of the Submit Pivot is set to ""PEN"" we will need to do some action to change it to ""QUE"".

Description of the ""PEN"" status code describes a pending extra action like digital signature which needs to be done to change status to ""QUE"".

For countries where E-Reporting functionality is not currently supported, the Pending Status Description is blank.", ItemSet.EReportingPivotPendingStatusDescription.Hint);
			Assert(ItemSet.EReportingPivotPendingStatusDescription.EditorInfo is TextRegistryEditorInfo);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EReportingPivotPendingStatusDescription.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EReportingPivotPendingStatusDescription.Options);
			AssertEquals(string.Empty, ItemSet.EReportingPivotPendingStatusDescription.DefaultValue);

			var factory = new BusinessObjectFactory();
			var expectedDescriptionByCountry = new Dictionary<string, string>()
			{
				{ Constants.CountryCodes.Argentina, "Pending User Action" },
				{ Constants.CountryCodes.Brazil, "Pending User Action" },
				{ Constants.CountryCodes.Chile, "Pending User Action" },
				{ Constants.CountryCodes.China, "Pending User Action" },
				{ Constants.CountryCodes.Colombia, "Pending User Action" },
				{ Constants.CountryCodes.CostaRica, "Pending User Action" },
				{ Constants.CountryCodes.DominicanRepublic, "Pending User Action" },
				{ Constants.CountryCodes.Egypt, "Pending Digital Signature" },
				{ Constants.CountryCodes.Fiji, "Pending User Action" },
				{ Constants.CountryCodes.Germany, "Pending User Action" },
				{ Constants.CountryCodes.Hungary, "Pending User Action" },
				{ Constants.CountryCodes.India, "Pending User Action" },
				{ Constants.CountryCodes.Israel, "Pending User Action" },
				{ Constants.CountryCodes.Italy, "Pending User Action" },
				{ Constants.CountryCodes.Jordan, "Pending User Action" },
				{ Constants.CountryCodes.KoreaSouth, "Pending User Action" },
				{ Constants.CountryCodes.Latvia, "Pending User Action" },
				{ Constants.CountryCodes.Malaysia, "Pending User Action" },
				{ Constants.CountryCodes.Mauritius, "Pending User Action" },
				{ Constants.CountryCodes.Mexico, "Pending User Action" },
				{ Constants.CountryCodes.Panama, "Pending User Action" },
				{ Constants.CountryCodes.Philippines, "Pending User Action" },
				{ Constants.CountryCodes.Poland, "Pending User Action" },
				{ Constants.CountryCodes.Romania, "Pending User Action" },
				{ Constants.CountryCodes.SaudiArabia, "Pending User Action" },
				{ Constants.CountryCodes.Serbia, "Pending User Action" },
				{ Constants.CountryCodes.Spain, "Pending User Action" },
				{ Constants.CountryCodes.Turkey, "Pending User Action" },
				{ Constants.CountryCodes.UnitedKingdom, "Pending User Action" },
				{ Constants.CountryCodes.Uruguay, "Pending User Action" },
				{ Constants.CountryCodes.VietNam, "Pending User Action" },
				{ Constants.CountryCodes.WesternSamoa, "Pending User Action" }
			};
			var countries = factory.Load<RefCountry>(new ZQuery());
			foreach (var country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.RN_Code))
				{
					expectedDescriptionByCountry.TryGetValue(country.RN_Code, out var expectedDescription);
					AssertEquals($"Expected Pivot Pending status description for {country.RN_Desc}", expectedDescription ?? string.Empty, ItemSet.EReportingPivotPendingStatusDescription.Value.ToString());
				}
			}
		}

		public void TestEReportingTransactionNumber()
		{
			var regItem = ItemSet.EReportingTransactionNumber;
			AssertEquals("EReportingTransactionNumber", regItem.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Italy, regItem.Category);
			AssertEquals("Transaction Number option for E-Invoicing", regItem.Caption);
			AssertEquals(@"This registry is used to decide which Reference Number should be sent via e-Reporting, according to the rules of each Login country.
When setting the value to 'COM', then the system uses the Compliance Number;
when setting the value to 'INV', then the system uses the Invoice (Transaction) Number;
when setting the value to 'CIN', then the system uses the Compliance Number if it is filled, else it uses the Transaction Number.

NOTE: when this registry is set to 'COM' and the Transaction has not been assigned the Compliance Number during posting, sending the e-Invoice may be prevented.
To assign the Compliance Number and enable sending you need to proceed in this way:
a) assign the Compliance Number using the actions 'Allocate Compliance Number' or 'Update Compliance Subtype and Number' in the Receivables Transactions module;
b) add the Transaction to the queue using the action 'Reset Status to Queued' in the Receivables Transactions module.", regItem.Hint);
			AssertEquals(RegistryStorageFlags.System, regItem.Storage);
			AssertEquals(RegistryOptions.Default, regItem.Options);
			AssertEquals(EReportingTransactionNumberOptions.ComplianceOrInvoiceNr.Code, regItem.DefaultValue);

			AssertCodePair(EReportingTransactionNumberOptions.ComplianceNr);
			AssertCodePair(EReportingTransactionNumberOptions.InvoiceNr);
			AssertCodePair(EReportingTransactionNumberOptions.ComplianceOrInvoiceNr);

			void AssertCodePair(CodeDescriptionPair codePair)
			{
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codePair.Code);
				AssertEquals(codePair.Code, regItem.Value);
			}
		}

		public void TestEReportingGEIMessageSystemType()
		{
			var regItem = ItemSet.EReportingGEIMessageSystemType;
			AssertEquals("EReportingGEIMessageSystemType", regItem.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, regItem.Category);
			AssertEquals("E-Reporting GEI Message System Type(CargoWise Support Only)", regItem.Caption);
			AssertEquals(@"This registry is used to define the GEI message system type of E-Reporting functionality.
By default, the system defines the GEI message system type based on the system license code.", regItem.Hint);
			AssertEquals(RegistryStorageFlags.Company, regItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, regItem.Options);
			AssertEquals(EReportingGEIMessageSystemTypeOptions.Default.Code, regItem.DefaultValue);

			AssertCodePair(EReportingGEIMessageSystemTypeOptions.Default);
			AssertCodePair(EReportingGEIMessageSystemTypeOptions.AlwaysProductionSystem);
			AssertCodePair(EReportingGEIMessageSystemTypeOptions.AlwaysTestSystem);

			void AssertCodePair(CodeDescriptionPair codePair)
			{
				var oldValue = regItem.Value;
				var newValue = codePair.Code;
				var expectedLogMessage = $"GEI Message System Type changed from [{oldValue}] to [{newValue}].";
				var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, oldValue, newValue);

				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codePair.Code);
				AssertEquals(codePair.Code, regItem.Value);

				var logReference = regItem.OnBuildLogReference(args);
				AssertEquals(expectedLogMessage, logReference);
			}
		}

		#region E-Reporting and E-Invoicing Configuration - Saudi Arabia

		public void TestSaudiArabiaEInvoicingCSIDAPIEndPoint_ProductionSystem() => AssertSaudiArabiaEInvoicingCSIDAPIEndPoint(true);

		public void TestSaudiArabiaEInvoicingCSIDAPIEndPoint_NonProductionSystem() => AssertSaudiArabiaEInvoicingCSIDAPIEndPoint(false);

		void AssertSaudiArabiaEInvoicingCSIDAPIEndPoint(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);

			var itemSet = GetNewItemSet();
			AssertEquals("SaudiArabiaEInvoicingCSIDAPIEndPoint", itemSet.SaudiArabiaEInvoicingCSIDAPIEndPoint.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_SaudiArabia, itemSet.SaudiArabiaEInvoicingCSIDAPIEndPoint.Category);
			AssertEquals("Registration CSID API Endpoint", itemSet.SaudiArabiaEInvoicingCSIDAPIEndPoint.Caption);
			AssertEquals("This registry defines the CSID API endpoint used to request Saudi Arabia e-Invoicing credentials.", itemSet.SaudiArabiaEInvoicingCSIDAPIEndPoint.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.SaudiArabiaEInvoicingCSIDAPIEndPoint.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.SaudiArabiaEInvoicingCSIDAPIEndPoint.Options);

			if (isProductionSystem)
			{
				AssertEquals("https://gw-fatoora.zatca.gov.sa/e-invoicing/core/compliance", GetNewItemSet().SaudiArabiaEInvoicingCSIDAPIEndPoint.DefaultValue);
			}
			else
			{
				AssertEquals("https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/compliance", itemSet.SaudiArabiaEInvoicingCSIDAPIEndPoint.DefaultValue);
			}
		}

		public void TestSaudiArabiaEInvoicingOnboardingAPIEndPoint_ProductionSystem() => AssertSaudiArabiaEInvoicingOnboardingAPIEndPoint(true);

		public void TestSaudiArabiaEInvoicingOnboardingAPIEndPoint_NonProductionSystem() => AssertSaudiArabiaEInvoicingOnboardingAPIEndPoint(false);

		void AssertSaudiArabiaEInvoicingOnboardingAPIEndPoint(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);

			var itemSet = GetNewItemSet();
			AssertEquals("SaudiArabiaEInvoicingOnboardingAPIEndPoint", itemSet.SaudiArabiaEInvoicingOnboardingAPIEndPoint.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_SaudiArabia, itemSet.SaudiArabiaEInvoicingOnboardingAPIEndPoint.Category);
			AssertEquals("Registration Onboarding/Renewal API Endpoint", itemSet.SaudiArabiaEInvoicingOnboardingAPIEndPoint.Caption);
			AssertEquals("This registry defines the Onboarding/Renewal API endpoint used to request Saudi Arabia e-Invoicing credentials.", itemSet.SaudiArabiaEInvoicingOnboardingAPIEndPoint.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.SaudiArabiaEInvoicingOnboardingAPIEndPoint.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.SaudiArabiaEInvoicingOnboardingAPIEndPoint.Options);

			if (isProductionSystem)
			{
				AssertEquals("https://gw-fatoora.zatca.gov.sa/e-invoicing/core/production/csids", GetNewItemSet().SaudiArabiaEInvoicingOnboardingAPIEndPoint.DefaultValue);
			}
			else
			{
				AssertEquals("https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/production/csids", itemSet.SaudiArabiaEInvoicingOnboardingAPIEndPoint.DefaultValue);
			}
		}

		#endregion

		#region E-Reporting and E-Invoicing Configuration - Israel

		public void TestIsraelEInvoicingITAOAuthWebURL_ProductionSystem()
			=> AssertIsraelEInvoicingITAOAuthWebURL(isProductionSystem: true, expectedValue: "");

		public void TestIsraelEInvoicingITAOAuthWebURL_NonProductionSystem()
			=> AssertIsraelEInvoicingITAOAuthWebURL(isProductionSystem: false, expectedValue: "https://openapi.taxes.gov.il/shaam/tsandbox/longtimetoken/oauth2/authorize");

		void AssertIsraelEInvoicingITAOAuthWebURL(bool isProductionSystem, string expectedValue)
		{
			var itemSet = SetupRegistryFor(isProductionSystem);

			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);
			AssertEquals("IsraelEInvoicingITAOAuthWebURL", itemSet.IsraelEInvoicingITAOAuthWebURL.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel, itemSet.IsraelEInvoicingITAOAuthWebURL.Category);
			AssertEquals("ITA OAuth Web URL (CargoWiseOne Support Only)", itemSet.IsraelEInvoicingITAOAuthWebURL.Caption);
			AssertEquals("ITA OAuth Web URL. Please enter the Web URL for ITA open authentication.", itemSet.IsraelEInvoicingITAOAuthWebURL.Hint);
			AssertEquals(expectedValue, itemSet.IsraelEInvoicingITAOAuthWebURL.DefaultValue);
			AssertEquals(RegistryStorageFlags.System, itemSet.IsraelEInvoicingITAOAuthWebURL.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.IsraelEInvoicingITAOAuthWebURL.Options);
			AssertNull(itemSet.IsraelEInvoicingITAOAuthWebURL.OnAllValuesSavedAction);
		}

		public void TestIsraelEInvoicingWTCCallbackSiteWebURL_ProductionSystem()
			=> AssertIsraelEInvoicingWTCCallbackSiteWebURL(isProductionSystem: true, expectedValue: "https://israeloauthcs.wisegrid.net/oauth/callback");

		public void TestIsraelEInvoicingWTCCallbackSiteWebURL_NonProductionSystem()
			=> AssertIsraelEInvoicingWTCCallbackSiteWebURL(isProductionSystem: false, expectedValue: "https://israeloauthcs-test.wisegrid.net/oauth/callback");

		void AssertIsraelEInvoicingWTCCallbackSiteWebURL(bool isProductionSystem, string expectedValue)
		{
			var itemSet = SetupRegistryFor(isProductionSystem);

			AssertEquals(isProductionSystem, Env.Instance.IsProductionSystem);
			AssertEquals("IsraelEInvoicingWTCCallbackSiteWebURL", itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel, itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.Category);
			AssertEquals("WTC Callback site Web URL (CargoWiseOne Support Only)", itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.Caption);
			AssertEquals("WTC Callback Site Web URL. Please enter the Web URL for the WTC Callback site.", itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.Options);
			AssertNull(itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.OnAllValuesSavedAction);
			AssertEquals(expectedValue, itemSet.IsraelEInvoicingWTCCallbackSiteWebURL.DefaultValue);
		}

		AccountingMasterFilesRegistry SetupRegistryFor(bool isProductionSystem)
		{
			var itemSet = ItemSet;
			if (isProductionSystem)
			{
				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
				itemSet = GetNewItemSet();
			}

			return itemSet;
		}

		public void TestIsraelEInvoicingCredentials_ProductionSystem()
			=> AssertIsraelEInvoicingCredentials(isProductionSystem: true, expectedClientId: "", expectedClientSecret: "");

		public void TestIsraelEInvoicingCredentials_NonProductionSystem()
			=> AssertIsraelEInvoicingCredentials(isProductionSystem: false, expectedClientId: "2ca90d5915db89e91f96570e881f15ed", expectedClientSecret: "94804c01302f5481ff13df0d0b22c886");

		public void AssertIsraelEInvoicingCredentials(bool isProductionSystem, string expectedClientId, string expectedClientSecret)
		{
			var itemSet = SetupRegistryFor(isProductionSystem);

			AssertEquals("IsraelEInvoicingCredentials", itemSet.IsraelEInvoicingCredentials.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel, itemSet.IsraelEInvoicingCredentials.Category);
			AssertEquals("E-Invoicing Credentials (CargoWiseOne Support Only)", itemSet.IsraelEInvoicingCredentials.Caption);
			AssertEquals("This registry defines the Client ID and Client Secret used to process Israel E-Invoices, which are allocated to registered software systems by the government.", itemSet.IsraelEInvoicingCredentials.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.IsraelEInvoicingCredentials.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.IsraelEInvoicingCredentials.Options);
			AssertEquals(expectedClientId, itemSet.IsraelEInvoicingCredentials.DefaultValue.ClientId);
			AssertEquals(expectedClientSecret, itemSet.IsraelEInvoicingCredentials.DefaultValue.ClientSecret);
			AssertNull(itemSet.IsraelEInvoicingCredentials.OnAllValuesSavedAction);
		}

		public void TestIsraelEncryptionKey()
		{
			AssertEquals(nameof(ItemSet.IsraelEncryptionKey), ItemSet.IsraelEncryptionKey.Name);
			AssertEquals(String.Empty, ItemSet.IsraelEncryptionKey.Category);
			AssertEquals(String.Empty, ItemSet.IsraelEncryptionKey.Caption);
			AssertEquals(String.Empty, ItemSet.IsraelEncryptionKey.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IsraelEncryptionKey.Storage);
			AssertEquals(RegistryOptions.IsHidden | RegistryOptions.IsReadOnly, ItemSet.IsraelEncryptionKey.Options);
			AssertEquals("ifbshpYlyGQlTUgq2j6vGKBN45B1Hpo3qJpfyACqoN4OxuSnfHkehUXXaBM5WM6HsCtApEsYV8mvBBBi7vMy8RlWDqur3AzRTzLNDYru6ab6NUCrrtPCT1FsuJinGwu7NYQ25dUd2lZdpAVgIoBOd55PJeToGcrkZX0WTGDjsJU=", ItemSet.IsraelEncryptionKey.DefaultValue);
		}

		#endregion

		#region E-Reporting and E-Invoicing Configuration - Romania

		public void TestRomaniaEInvoicingANAFOauthWebURL()
		{
			var itemSet = GetNewItemSet();
			AssertEquals("RomaniaEInvoicingANAFOauthWebURL", itemSet.RomaniaEInvoicingANAFOauthWebURL.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania, itemSet.RomaniaEInvoicingANAFOauthWebURL.Category);
			AssertEquals("ANAF OAuth web URL (CargoWiseOne Support Only)", itemSet.RomaniaEInvoicingANAFOauthWebURL.Caption);
			AssertEquals(@"ANAF OAuth web URL.", itemSet.RomaniaEInvoicingANAFOauthWebURL.Hint);
			AssertEquals(RegistryStorageFlags.System, itemSet.RomaniaEInvoicingANAFOauthWebURL.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, itemSet.RomaniaEInvoicingANAFOauthWebURL.Options);
			AssertEquals("https://logincert.anaf.ro/anaf-oauth2/v1/authorize", GetNewItemSet().RomaniaEInvoicingANAFOauthWebURL.DefaultValue);
		}

		public void TestRomaniaEInvoicingTestCredentials()
		{
			TestRomaniaEInvoicingCredentialsCore(ItemSet.RomaniaEInvoicingTestCredentials,
				isProductionSystem: false,
				clientId: "d6b898215d03e162feaa655d2eab02cbf0570045219ec465",
				clientSecret: "fae4c8aafb028a6582173b3e4bb76253c881890847ef02cbf0570045219ec465");
		}

		public void TestRomaniaEInvoicingProductionCredentials()
		{
			TestRomaniaEInvoicingCredentialsCore(ItemSet.RomaniaEInvoicingProductionCredentials,
				isProductionSystem: true,
				clientId: "42488c8525646a46b23ee87b5f307e8a7e3ee71df63ce065",
				clientSecret: "8ec973118bc3f2c219e770e690c766261f8c8f9646db7e8a7e3ee71df63ce065");
		}

		void TestRomaniaEInvoicingCredentialsCore(EInvoicingCredentialsRegistryItem registryItem, bool isProductionSystem, string clientId, string clientSecret)
		{
			SetUpIProductRegistration(isProductionSystem);

			var prefix = isProductionSystem ? "Production" : "Test";
			AssertEquals($"RomaniaEInvoicing{prefix}Credentials", registryItem.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania, registryItem.Category);
			AssertEquals($"{prefix} E-Invoicing Credentials (CargoWiseOne Support Only)", registryItem.Caption);
			AssertEquals($"This registry defines the Client ID and Client Secret used to process Romania E-Invoices for {prefix} Environment, which are allocated to Service Providers and could potentially also be allocated to Taxpayers.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registryItem.Options);
			AssertEquals(clientId, registryItem.DefaultValue.ClientId);
			AssertEquals(clientSecret, registryItem.DefaultValue.ClientSecret);
		}

		public void TestRomaniaEInvoicingTestWTCCallbackSiteWebURL()
		{
			TestRomaniaEInvoicingWTCCallbackSiteWebURLCore(ItemSet.RomaniaEInvoicingTestWTCCallbackWebSiteURL,
				isProductionSystem: false,
				callbackURL: "https://romaniaoauthcs-test.wisegrid.net/oauth/callback");
		}

		public void TestRomaniaEInvoicingProductionWTCCallbackSiteWebURL()
		{
			TestRomaniaEInvoicingWTCCallbackSiteWebURLCore(ItemSet.RomaniaEInvoicingProductionWTCCallbackWebSiteURL,
				isProductionSystem: true,
				callbackURL: "https://romaniaoauthcs.wisegrid.net/oauth/callback");
		}

		void TestRomaniaEInvoicingWTCCallbackSiteWebURLCore(StringRegistryItem registryItem, bool isProductionSystem, string callbackURL)
		{
			SetUpIProductRegistration(isProductionSystem);

			var prefix = isProductionSystem ? "Production" : "Test";
			AssertEquals($"RomaniaEInvoicing{prefix}WTCCallbackWebSiteURL", registryItem.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Romania, registryItem.Category);
			AssertEquals($"{prefix} WTC Callback web site URL (CargoWiseOne Support Only)", registryItem.Caption);
			AssertEquals($"{prefix} WTC Callback web site URL.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, registryItem.Options);
			AssertEquals(callbackURL, registryItem.DefaultValue);
		}

		void SetUpIProductRegistration(bool isProductionSystem)
		{
			var productRegistration = new Mock<IProductRegistration>();
			productRegistration
				.Setup(x => x.IsWiseTechGlobalInternalUATSystem())
				.Returns(!isProductionSystem);
			ObjectFactory.Substitute(productRegistration.Object);
		}

		public void TestRomaniaEncryptionKey()
		{
			AssertEquals(nameof(ItemSet.RomaniaEncryptionKey), ItemSet.RomaniaEncryptionKey.Name);
			AssertEquals(String.Empty, ItemSet.RomaniaEncryptionKey.Category);
			AssertEquals(String.Empty, ItemSet.RomaniaEncryptionKey.Caption);
			AssertEquals(String.Empty, ItemSet.RomaniaEncryptionKey.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.RomaniaEncryptionKey.Storage);
			AssertEquals(RegistryOptions.IsHidden | RegistryOptions.IsReadOnly, ItemSet.RomaniaEncryptionKey.Options);
			AssertEquals("H1xQW12y9MmG3xUbJ6c5gETFJmakmcslZbVEZ3kD2e6gf4AShkE19NSPIHeB66kI7iToqL46YIRZAUicIQ9nkmo8naKDwFHbJDaOwv0N07b6gBf6pWHkK6oHHIMXPCm3IltLTyVOdGOkrth299XHOH6X0awup3qR3naQtDt9gko=", ItemSet.RomaniaEncryptionKey.DefaultValue);
		}

		#endregion

		#region E-Reporting and E-Invoicing Configuration - China

		public void TestChinaEInvoicingCredentials()
		{
			AssertEquals("Name", "ChinaEInvoicingCredentials", ItemSet.ChinaEInvoicingCredentials.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_China, ItemSet.ChinaEInvoicingCredentials.Category);
			AssertEquals("Caption", "E-Invoicing Credentials", ItemSet.ChinaEInvoicingCredentials.Caption);
			AssertEquals("Hint", "This registry defines the Client Number assigned by e-Invoicing service partner system.", ItemSet.ChinaEInvoicingCredentials.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, ItemSet.ChinaEInvoicingCredentials.Storage);
			AssertEquals("Default Value", ZString.Empty, ItemSet.ChinaEInvoicingCredentials.DefaultValue);

			//event log
			var registry = Instance.ChinaEInvoicingCredentials;
			var oldValue = "oldValue";
			var newValue = "newValue";
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = Instance.ChinaEInvoicingCredentials.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestChinaTransmitAndIssueFapiao()
		{
			AssertEquals("Name", "ChinaTransmitAndIssueFapiao", ItemSet.ChinaTransmitAndIssueFapiao.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_China, ItemSet.ChinaTransmitAndIssueFapiao.Category);
			AssertEquals("Caption", "Transmit and Issue Fapiao", ItemSet.ChinaTransmitAndIssueFapiao.Caption);
			AssertEquals("Hint", "This registry is relevant to China Login Company only.\r\n\r\nBy default, the registry is set to 'No' and Fapiao will need to be manually issued via RongJin's Tax Easy web portal.\r\nIf required, you can change the registry value to 'Yes' and Fapiao will be issued on transmission to RongJin's Tax Easy web portal.", ItemSet.ChinaTransmitAndIssueFapiao.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, ItemSet.ChinaTransmitAndIssueFapiao.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.IsOnlyForSupport, ItemSet.ChinaTransmitAndIssueFapiao.Options);
			AssertEquals("Default Value", false, ItemSet.ChinaTransmitAndIssueFapiao.DefaultValue);
			AssertEquals("CountryFilterPKs Count", 1, ItemSet.ChinaTransmitAndIssueFapiao.CountryFilterPKs.Count());
			AssertEquals("CountryFilterPKs", true, ItemSet.ChinaTransmitAndIssueFapiao.CountryFilterPKs.Contains(Constants.CountryGuids.China));

			//event log
			var registry = Instance.ChinaTransmitAndIssueFapiao;
			var oldValue = true;
			var newValue = false;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = Instance.ChinaTransmitAndIssueFapiao.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestAlwaysTransmitNegativeChargesAsDiscount()
		{
			AssertEquals("Name", "AlwaysTransmitNegativeChargesAsDiscount", ItemSet.AlwaysTransmitNegativeChargesAsDiscount.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_China, ItemSet.AlwaysTransmitNegativeChargesAsDiscount.Category);
			AssertEquals("Caption", @"By default, the system will treat negative charges as discounts and you will not be allowed to merge negative charges with positive charges even if they are mapped to the same product or service code in RongJin's KPT system.

If you override this registry value to 'No', the system will not treat negative charges as discounts and you will be able to merge them with other charges with the same product or service code, if required.", ItemSet.AlwaysTransmitNegativeChargesAsDiscount.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.AlwaysTransmitNegativeChargesAsDiscount.Storage);
			AssertEquals("RegistryOptions", RegistryOptions.Default, ItemSet.AlwaysTransmitNegativeChargesAsDiscount.Options);
			AssertEquals("Default Value", true, ItemSet.AlwaysTransmitNegativeChargesAsDiscount.DefaultValue);
			AssertEquals("CountryFilterPKs Count", 1, ItemSet.AlwaysTransmitNegativeChargesAsDiscount.CountryFilterPKs.Count());
			AssertEquals("CountryFilterPKs", true, ItemSet.AlwaysTransmitNegativeChargesAsDiscount.CountryFilterPKs.Contains(Constants.CountryGuids.China));

			//event log
			var registry = Instance.AlwaysTransmitNegativeChargesAsDiscount;
			var oldValue = true;
			var newValue = false;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = Instance.AlwaysTransmitNegativeChargesAsDiscount.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		#endregion

		#region E-Reporting and E-Invoicing Configuration - India

		public void TestIndiaEInvoicingCredentials()
		{
			AssertEquals("IndiaEInvoicingCredentials", ItemSet.IndiaEInvoicingCredentials.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_India, ItemSet.IndiaEInvoicingCredentials.Category);
			AssertEquals("E-Invoicing Credentials (CargoWiseOne Support Only)", ItemSet.IndiaEInvoicingCredentials.Caption);
			AssertEquals("This registry defines the Client ID and Client Secret used to process India E-Invoices, which are allocated to Service Providers and could potentially also be allocated to Taxpayers.", ItemSet.IndiaEInvoicingCredentials.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IndiaEInvoicingCredentials.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IndiaEInvoicingCredentials.Options);
			AssertEquals(string.Empty, ItemSet.IndiaEInvoicingCredentials.DefaultValue.ClientId);
			AssertEquals(string.Empty, ItemSet.IndiaEInvoicingCredentials.DefaultValue.ClientSecret);
			AssertNull(ItemSet.IndiaEInvoicingCredentials.OnAllValuesSavedAction);

			AssertNull("Precondition: no credential message queued", Factory.GetLatestEHubConfigurationInterchange());
			using (ItemSet.IndiaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EInvoicingCredentials() { ClientId = "ID", ClientSecret = "Secret" }))
			{
				AssertNull("No credential message queued for new values", Factory.GetLatestEHubConfigurationInterchange());
				AssertEquals("ID", ItemSet.IndiaEInvoicingCredentials.Value.ClientId);
				AssertEquals("Secret", ItemSet.IndiaEInvoicingCredentials.Value.ClientSecret);

				using (ItemSet.IndiaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EInvoicingCredentials()))
				{
					AssertNull("No credential message queued for empty values", Factory.GetLatestEHubConfigurationInterchange());
					AssertEquals(string.Empty, ItemSet.IndiaEInvoicingCredentials.Value.ClientId);
					AssertEquals(string.Empty, ItemSet.IndiaEInvoicingCredentials.Value.ClientSecret);
				}
			}
		}

		#endregion

		public void TestIsraelInvoiceAmountBoundaries()
		{
			AssertEquals("IsraelInvoiceAmountBoundaries", ItemSet.IsraelInvoiceAmountBoundaries.Name);
			AssertEquals("Israel e-Invoicing Net Amount Eligibility Criteria (CargoWiseOne Support Only)", ItemSet.IsraelInvoiceAmountBoundaries.Caption);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Israel, ItemSet.IsraelInvoiceAmountBoundaries.Category);
			AssertEquals("Israel e-Invoicing Net Amount Eligibility Criteria. Invoices with equal or greater net amounts will be considered eligible for e-invoice based on the registry value. (NIS)",
				ItemSet.IsraelInvoiceAmountBoundaries.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.IsraelInvoiceAmountBoundaries.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.IsraelInvoiceAmountBoundaries.Options);
			Assert("There are default items in the collection", ItemSet.IsraelInvoiceAmountBoundaries.DefaultValue.Count != 0);
		}

		#endregion

		#region Currency Exchange Rate Types - Global

		public void TestCurrencyExchangeRateTypes()
		{
			AssertEquals("Name", "CurrencyExchangeRateTypes", ItemSet.CurrencyExchangeRateTypes.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Accounting, ItemSet.CurrencyExchangeRateTypes.Category);
			AssertEquals("Caption", "Currency Exchange Rate Types - Global", ItemSet.CurrencyExchangeRateTypes.Caption);
			AssertEquals("Hint", "This registry allows you to override the labels for currency exchange rate types. Custom rates can be used to record various sources of exchange rates.", ItemSet.CurrencyExchangeRateTypes.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, ItemSet.CurrencyExchangeRateTypes.Storage);

			var codeDescriptionCollection = ItemSet.CurrencyExchangeRateTypes.DefaultValue;
			AssertExchangeRateTypesDefaultValue(codeDescriptionCollection, false);
			AssertTogglingOfExRateTypesRegistryItem(codeDescriptionCollection, false);
			AssertModifyExRateTypesDescription(codeDescriptionCollection, false);
		}

		public void TestCurrencyExchangeRateTypesCompanySpecific()
		{
			AssertEquals("Name", "CurrencyExchangeRateTypesCompanySpecific", ItemSet.CurrencyExchangeRateTypesCompanySpecific.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Accounting, ItemSet.CurrencyExchangeRateTypesCompanySpecific.Category);
			AssertEquals("Caption", "Currency Exchange Rate Types - Local", ItemSet.CurrencyExchangeRateTypesCompanySpecific.Caption);
			AssertEquals("Hint", "This registry allows you to override the labels for company specific currency exchange rate types. Custom rates can be used to record various sources of exchange rates.", ItemSet.CurrencyExchangeRateTypesCompanySpecific.Hint);
			AssertEquals("Flags", RegistryStorageFlags.Company, ItemSet.CurrencyExchangeRateTypesCompanySpecific.Storage);

			var codeDescriptionCollection = ItemSet.CurrencyExchangeRateTypesCompanySpecific.DefaultValue;
			AssertExchangeRateTypesDefaultValue(codeDescriptionCollection, true);
			AssertTogglingOfExRateTypesRegistryItem(codeDescriptionCollection, true);
			AssertModifyExRateTypesDescription(codeDescriptionCollection, true);
		}

		public void TestCurrencyExchangeRateTypesDefaultValueSuspendValidation()
		{
			var hits = Db.Connection.ExecutedCommandCount;
			var newValue = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.DefaultValue;
			AssertEquals("No need to execute any commands for default value", hits, Db.Connection.ExecutedCommandCount);

			hits = Db.Connection.ExecutedCommandCount;
			newValue = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.DefaultValue;
			AssertEquals("No need to execute any commands for default value", hits, Db.Connection.ExecutedCommandCount);
		}

		public void TestFindModifiedCustomExchangeRates()
		{
			var description1 = ResString.GetMultilingualString("cc4c1fbd-b858-478e-a8f9-b7405040f768", "Description Changed");
			var description2 = ResString.GetMultilingualString("cc4c1fbd-b858-478e-a8f9-b7405040f768", "Bool and Description Changed");

			var newValue = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			newValue.ToList<CodeDescriptionBool>()
			.ForEach(x =>
			{
				if (x.Code.Equals("C01") || x.Code.Equals("C03"))
				{
					x.Bool = !x.Bool;
				}
				if (x.Code.Equals("C02"))
				{
					x.Description = description1;
				}
				if (x.Code.Equals("C05"))
				{
					x.Bool = !x.Bool;
					x.Description = description2;
				}
			});
			const string expected = "'C01' Enabled Status changed from 'N' to 'Y'. 'C02' Description changed from 'Custom Rate 02' to 'Description Changed'. 'C03' Enabled Status changed from 'N' to 'Y'. 'C05' Enabled Status changed from 'N' to 'Y'. 'C05' Description changed from 'Custom Rate 05' to 'Bool and Description Changed'. ";
			AssertChangeLog(newValue, expected, false);
		}

		public void TestFindModifiedCustomExchangeRates_CompanySpecific()
		{
			var description1 = ResString.GetMultilingualString("cc4c1fbd-b858-478e-a8f9-b7405040f768", "Description Changed");
			var description2 = ResString.GetMultilingualString("cc4c1fbd-b858-478e-a8f9-b7405040f768", "Bool and Description Changed");

			var newValue = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.Value;
			newValue.ToList<CodeDescriptionBool>()
			.ForEach(x =>
			{
				if (x.Code.Equals("L01") || x.Code.Equals("L03"))
				{
					x.Bool = !x.Bool;
				}
				if (x.Code.Equals("L02"))
				{
					x.Description = description1;
				}
				if (x.Code.Equals("L05"))
				{
					x.Bool = !x.Bool;
					x.Description = description2;
				}
			});
			const string expected = "'L01' Enabled Status changed from 'N' to 'Y'. 'L02' Description changed from 'Local Rate 02' to 'Description Changed'. 'L03' Enabled Status changed from 'N' to 'Y'. 'L05' Enabled Status changed from 'N' to 'Y'. 'L05' Description changed from 'Local Rate 05' to 'Bool and Description Changed'. ";
			AssertChangeLog(newValue, expected, true);
		}

		static void AssertChangeLog(CodeDescriptionBoolCollection newValue, string expected, bool isCompanySpecific)
		{
			var registry = isCompanySpecific ? AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific : AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes;
			var originalValue = registry.Value;
			var registryItem = registry.Inner;

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, originalValue, newValue);
			var result = registry.OnBuildLogReference(args);

			AssertEquals(expected, result);
		}

		void AssertTogglingOfExRateTypesRegistryItem(CodeDescriptionBoolCollection codeDescriptionCollection, bool isCompanySpecific)
		{
			var registry = isCompanySpecific ? ItemSet.CurrencyExchangeRateTypesCompanySpecific : ItemSet.CurrencyExchangeRateTypes;

			codeDescriptionCollection
				.ToList<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (!AccountingMasterFilesConstants.GetDefaultExchangeRateTypesList().ContainsCode(x.Code))
					{
						x.Bool = !x.Bool;
					}
				});
			registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionCollection);

			registry.Value
				.ToList<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (AccountingMasterFilesConstants.GetDefaultExchangeRateTypesList().ContainsCode(x.Code))
					{
						AssertEquals(x.Bool, registry.DefaultValue.Cast<CodeDescriptionBool>().FirstOrDefault(y => y.Code == x.Code).Bool);
					}
					else
					{
						AssertEquals(x.Bool, !registry.DefaultValue.Cast<CodeDescriptionBool>().FirstOrDefault(y => y.Code == x.Code).Bool);
					}
				});
		}

		void AssertModifyExRateTypesDescription(CodeDescriptionBoolCollection codeDescriptionCollection, bool isCompanySpecific)
		{
			var registry = isCompanySpecific ? ItemSet.CurrencyExchangeRateTypesCompanySpecific : ItemSet.CurrencyExchangeRateTypes;
			codeDescriptionCollection
				.ToList<CodeDescriptionBool>()
				.ForEach(x => x.Description = (NoResString)"Test Description");
			registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeDescriptionCollection);

			registry.Value
				.ToList<CodeDescriptionBool>()
				.ForEach(x =>
				{
					AssertEquals("Test Description", x.Description);
				});
		}

		static void AssertExchangeRateTypesDefaultValue(CodeDescriptionBoolCollection codeDescriptionCollection, bool isCompanySpecific)
		{
			var defaultExchangeRateTypesList = AccountingMasterFilesConstants.GetDefaultExchangeRateTypesList();
			codeDescriptionCollection
				.ToList<CodeDescriptionBool>()
				.ForEach(x =>
				{
					if (!isCompanySpecific)
					{
						var value = defaultExchangeRateTypesList.ContainsCode(x.Code);
						AssertEquals($"Default value for {x.Code}: {x.Description} should be {value}", value, x.Bool);
					}
					else
					{
						AssertEquals($"Default value for {x.Code}: {x.Description} should be false ", false, x.Bool);
					}
				});
		}

		#endregion

		#region TestEnablePaperStockOptionsToPrintComplianceDocuments

		public void TestEnablePaperStockOptionsToPrintComplianceDocuments()
		{
			AssertEquals("Name", "EnablePaperStockOptionsToPrintComplianceDocuments", ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.Category);
			AssertEquals("Caption", "Enable Paper Stock options to print compliance documents", ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.Caption);

			var expectedHint = @"This registry allows you to enable/disable paper printing options that can be configured in compliance books.

In some countries, compliance documents must be printed using a specific paper format.
Users can setup printing options in compliance books when required.

For countries where pre-printed or specific paper formats are not required, these options are not relevant and users can disable them, reducing the possibility of confusion and errors when setting up compliance books.

When set to 'Yes', users will be able to setup options in the 'Compliance Document' and 'Compliance Printing' sections of a compliance book.
When set to 'No', these sections will be disabled in a compliance book.

By default, the value of this registry is 'Yes'.";

			AssertEquals("Hint", expectedHint, ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.Options);
			AssertEquals("Default Value", true, ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.DefaultValue);
		}

		public void TestEnablePaperStockOptionsToPrintComplianceDocuments_DefaultValueIsFalse_InIndiaCompany()
		{
			var indiaCompany = Factory.NewWithValidTestData<GlbCompany>();
			indiaCompany.GC_RN_NKCountryCode = Constants.CountryCodes.India;
			Factory.Save();
			AssertEquals("Default Value should be false when company is in India", false, ItemSet.EnablePaperStockOptionsToPrintComplianceDocuments.GetFallBackValueAtAllLevels(indiaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		#endregion

		#region Compliance Number Allocation Date

		public void TestComplianceNumberAllocationDate()
		{
			AssertEquals("Name", "ComplianceNumberAllocationDate_AR", ItemSet.ComplianceNumberAllocationDate_AR.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceNumberAllocationDate_AR.Category);
			AssertEquals("Caption", "Compliance Number Allocation Rule - Receivables", ItemSet.ComplianceNumberAllocationDate_AR.Caption);
			var expectedHint = @"This registry allows you to define the relevant date that the system must use to allocate compliance numbers in Receivables Invoice (INV), Credit Note (CRD), or Adjustment Note (ADJ) transactions.

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

These validations ensure that the compliance numbers are assigned in the specified order.";
			AssertEquals("Hint", expectedHint, ItemSet.ComplianceNumberAllocationDate_AR.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceNumberAllocationDate_AR.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ComplianceNumberAllocationDate_AR.Options);
			AssertEquals("Default Value", ComplianceNumberAllocationDateOptions.NoControl.Code, ItemSet.ComplianceNumberAllocationDate_AR.DefaultValue);
		}

		public void TestComplianceNumberAllocationDate_AP()
		{
			AssertEquals("Name", "ComplianceNumberAllocationDate_AP", ItemSet.ComplianceNumberAllocationDate_AP.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.ComplianceNumberAllocationDate_AP.Category);
			AssertEquals("Caption", "Compliance Number Allocation Rule - Payables", ItemSet.ComplianceNumberAllocationDate_AP.Caption);
			var expectedHint = @"This registry allows you to define the relevant date that the system must use to allocate compliance numbers in Payables Invoice (INV), Credit Note (CRD), or Adjustment Note (ADJ) transactions.

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

These validations ensure that the compliance numbers are assigned in the specified order.";
			AssertEquals("Hint", expectedHint, ItemSet.ComplianceNumberAllocationDate_AP.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceNumberAllocationDate_AP.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ComplianceNumberAllocationDate_AP.Options);
			AssertEquals("Default Value", ComplianceNumberAllocationDateOptions.NoControl.Code, ItemSet.ComplianceNumberAllocationDate_AP.DefaultValue);
		}

		public void TestGetComplianceNumberAllocationDateRegistryValue()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var currentCompany = GlbCompany.CurrentCompany;
			var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, currentCompany.PK));

			ErrorReporter.Clear();
			AssertEquals(null, registry.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.None, ZGuid.Empty));
			AssertEquals("The ledger None is not supported by ComplianceNumberAllocationDate", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.InvoiceDate.Code))
			using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(nonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				AssertEquals(ComplianceNumberAllocationDateOptions.InvoiceDate.Code, registry.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.AR, currentCompany.PK));
				AssertEquals(ComplianceNumberAllocationDateOptions.NoControl.Code, registry.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.AP, currentCompany.PK));
				AssertEquals(ComplianceNumberAllocationDateOptions.InvoiceDate.Code, registry.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.AR, ZGuid.Empty));

				AssertEquals(ComplianceNumberAllocationDateOptions.PostDate.Code, registry.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.AP, nonCurrentCompany.PK));
				AssertEquals(ComplianceNumberAllocationDateOptions.NoControl.Code, registry.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.AR, nonCurrentCompany.PK));
				AssertEquals(ComplianceNumberAllocationDateOptions.NoControl.Code, registry.GetComplianceNumberAllocationDateRegistryValue(ComplianceNumberAllocationDateValidLedgerEnum.AP, ZGuid.Empty));
			}
		}

		public void TestComplianceAssignNumbersOrder_DefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertComplianceAssignNumbersOrderedByPostDat_DafaultValues(Constants.CountryCodes.Australia, "AT1", "AB1", ComplianceNumberAllocationDateOptions.NoControl.Code);
				AssertComplianceAssignNumbersOrderedByPostDat_DafaultValues(Constants.CountryCodes.Brazil, "BT1", "BB1", ComplianceNumberAllocationDateOptions.NoControl.Code);
				AssertComplianceAssignNumbersOrderedByPostDat_DafaultValues(Constants.CountryCodes.Portugal, "PT1", "PB1", ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
			});

			void AssertComplianceAssignNumbersOrderedByPostDat_DafaultValues(ZString countryCode, ZString companyCode, ZString branchCode, string expectedValue)
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Code = companyCode;
				company.GC_RN_NKCountryCode = countryCode;

				var branch = company.Branches.AddNew();
				branch.GB_Code = branchCode;

				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals("DefaultValue", expectedValue, ItemSet.ComplianceNumberAllocationDate_AR.DefaultValue);
				}
			}
		}

		#endregion

		#region Compliance Number Sequence Configuration

		public void TestComplianceNumberSequenceConfiguration()
		{
			var registry = ItemSet.ComplianceNumberSequenceConfiguration;
			AssertEquals("Name", "ComplianceNumberSequenceConfiguration", registry.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, registry.Category);
			AssertEquals("Caption", "Compliance Number Sequence Configuration", registry.Caption);
			var expectedHint = @"By default, when compliance sequence number is assigned to a transaction/compliance document record, the system will save the compliance book series prefix + sequence number.
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
c. Use the 'Order' column to specify the order of the element. E.g. 1, 2, 3, etc.";
			AssertEquals("Hint", expectedHint, registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.MustOverrideDefaultValue, registry.Options);
		}

		public void TestOnBuildLogReferenceForComplianceNumberSequenceConfiguration()
		{
			var regItem = Instance.ComplianceNumberSequenceConfiguration.Inner;

			var collection1 = Instance.ComplianceNumberSequenceConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var collection2 = Instance.ComplianceNumberSequenceConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var item = collection2.AddNew();
			item.Code = "ABC";
			item.Description = "Test ABC";
			var elements = item.Elements;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Include = true;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Order = 1;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Length = 2;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Include = true;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Order = 2;
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].DigitCode = "aa";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			var result = Instance.ComplianceNumberSequenceConfiguration.OnBuildLogReference(args);
			AssertEquals("Code 'ABC' Added : 1-Blank Space '2', 2-Custom Element 1 'aa', 50-Sequence Number\r\n", result);

			var item1 = collection1.AddNew();
			item1.Code = "ABC";
			item1.Description = "Test ABC Changed";
			var elements1 = item1.Elements;
			elements1[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Include = true;
			elements1[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Order = 1;
			elements1[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Length = 2;
			elements1[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Include = true;
			elements1[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Order = 2;
			elements1[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].DigitCode = "aa";

			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			result = Instance.ComplianceNumberSequenceConfiguration.OnBuildLogReference(args);
			AssertEquals("Code 'ABC' : Description changed from 'Test ABC Changed' to 'Test ABC'.\r\n", result);

			item1.Description = "Test ABC";
			elements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Include = false;
			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			result = Instance.ComplianceNumberSequenceConfiguration.OnBuildLogReference(args);
			AssertEquals("Code 'ABC' Changed : 1-Blank Space '2', 50-Sequence Number\r\n", result);

			collection2.Remove(item);
			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			result = Instance.ComplianceNumberSequenceConfiguration.OnBuildLogReference(args);
			AssertEquals("Code 'ABC' Deleted\r\n", result);
		}

		#endregion

		#region Invoice Remittance Configuration

		public void TestInvoiceRemittanceConfiguration()
		{
			AssertEquals("Name", "InvoiceRemittanceConfiguration", ItemSet.InvoiceRemittanceConfiguration.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.InvoiceRemittanceConfiguration.Category);
			AssertEquals("Caption", "Invoice Remittance Configuration", ItemSet.InvoiceRemittanceConfiguration.Caption);
			var expectedHint = @"This registry gives you the ability to define invoice remittance type and respective configuration using the available data elements and check digits.
You setup one or more invoice remittance type by location.

During the posting of Receivable Invoices, Credit Notes and Adjustment Notes, the invoice remittance type and the corresponding invoice remittance reference will be saved against the transaction record.

The Invoice Remittance Reference will be shown on the Invoice screen after posting.

The 'Invoice Remittance Reference' doc strip will be included during the printing of DocBuilder Invoice document if an Invoice Remittance Reference has been saved against the invoice record. You can customize this doc strip as required to meet your requirement.

This Invoice Remittance Reference will be included in the Universal Accounting Transaction XML export.

Note: The Invoice Remittance Type and Reference will only be saved against new transaction record posted after the registry has been configured. If you would like to saved the type and reference against transaction posted previously, you can run the 'Override Invoice Remittance Type' Action menu available in Receivables Transactions module.";
			AssertEquals("Hint", expectedHint, ItemSet.InvoiceRemittanceConfiguration.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.InvoiceRemittanceConfiguration.Storage);
			AssertEquals("Options", RegistryOptions.MustOverrideDefaultValue, ItemSet.InvoiceRemittanceConfiguration.Options);
		}

		public void TestOnBuildLogReferenceForPaymentReferenceCodeConfiguration()
		{
			var regItem = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.Inner;

			var collection1 = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var collection2 = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var item = collection2.AddNew();
			item.Code = "ABC";
			item.Description = "Test ABC";
			item.DebtorLocation = "ALL";

			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			var result = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.OnBuildLogReference(args);
			AssertEquals("New code <ABC> added.\r\n", result);

			var item1 = collection1.AddNew();
			item1.Code = "ABC";
			item1.Description = "Test ABC Changed";
			item1.DebtorLocation = "ALL";

			args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, collection1, collection2);
			result = AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.OnBuildLogReference(args);
			AssertEquals("Code <ABC> configuration edited.\r\n", result);
		}

		#endregion

		#region Record Invalid Job Creation By User

		public void TestRecordInvalidJobCreationByUser()
		{
			TestRegistryItem(ItemSet.RecordInvalidJobCreationByUser,
				"RecordInvalidJobCreationByUser",
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
				TextEditorType.Memo,
				string.Empty
			);
		}

		#endregion

		public void TestEnableReportCriticalValidationErrorsAfterDBSaving()
		{
			AssertEquals("Name", "EnableReportCriticalValidationErrorsAfterDBSaving", ItemSet.EnableReportCriticalValidationErrorsAfterDBSaving.Name);
			AssertEquals("Category", "Accounting/Critical Validation", ItemSet.EnableReportCriticalValidationErrorsAfterDBSaving.Category);
			AssertEquals("Caption", "Enable Report Critical Validation Errors After DBSaving", ItemSet.EnableReportCriticalValidationErrorsAfterDBSaving.Caption);
			var expectedHint = @"This Registry Item enables postponement of critical validation error reporting. The errors will be thrown if no DB error happened. It is set to Yes by default. Set it to No when we want to throw critical validation errors immediately.";
			AssertEquals("Hint", expectedHint, ItemSet.EnableReportCriticalValidationErrorsAfterDBSaving.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableReportCriticalValidationErrorsAfterDBSaving.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableReportCriticalValidationErrorsAfterDBSaving.Options);
		}

		public void TestAESEncryptionKey()
		{
			AssertEquals(typeof(AESEncryptionKey128RegistryItem), ItemSet.AESEncryptionKey.GetType());
			AssertEquals("Name", "AESEncryptionKey", ItemSet.AESEncryptionKey.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.AESEncryptionKey.Category);
			AssertEquals("Caption", "AES Encryption Key", ItemSet.AESEncryptionKey.Caption);
			AssertEquals("Hint", @"This registry is relevant to Taiwan Login Company only who is using the new Compliance Document Module.

In Taiwan, the AES Encryption Key will be used to encrypt 10 alpha-numeric compliance document number and 4 digits random number for added security stored against the compliance document record using Base64 encoding conversion. The encrypted details will be included in the data elements for the generation of QRCodes for TXE-Electronic GUI.

In Taiwan, CW1 user will receive a password from the Tax Bureau. CW1 user should then contact and provide this password to WTG Taiwan. WTG Taiwan will use this information to generate the AES Encryption Key using the tool provided by the Tax Bureau, then save this value into this registry. This usually happens during the initial system implementation only. ", ItemSet.AESEncryptionKey.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.AESEncryptionKey.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AESEncryptionKey.Options);
			AssertEquals("Default Value", string.Empty, ItemSet.AESEncryptionKey.DefaultValue);
		}

		#region Accounting_ReceivableDefaults_DefaultSettings

		public void TestAllowNegativeRevenueChargesOnJob()
		{
			TestRegistryItem(ItemSet.AllowNegativeRevenueChargesOnJob, "AllowNegativeRevenueChargesOnJob", Categories.Accounting_JobInvoicing, "Allow negative revenue charges on a job", "Setting this registry to No default will stop users being able to save jobs with negative charges.", RegistryStorageFlags.Company, true);
			AssertEquals("Default Value should be True", true, ItemSet.AllowNegativeRevenueChargesOnJob.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.AllowNegativeRevenueChargesOnJob.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value must be False", false, ItemSet.AllowNegativeRevenueChargesOnJob.Value);
		}

		#endregion

		#region Accounting_ReceivableDefaults_DefaultSettings

		public void TestReceivablePreventCreationOfCreditNotes()
		{
			AssertEquals("Name", "ReceivablePreventCreationOfCreditNotes", ItemSet.ReceivablePreventCreationOfCreditNotes.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.ReceivablePreventCreationOfCreditNotes.Category);
			AssertEquals("Caption", "Prevent Creation of Credit Notes", ItemSet.ReceivablePreventCreationOfCreditNotes.Caption);
			AssertEquals("Hint"
				, @"This registry controls the ability to create amending and stand alone AR CRD transactions.

Setting this registry to 'Yes' will prevent all users in your login company from performing the following tasks:

* Creating a stand alone credit note directly in the receivables module
* Triggering the 'Amend with Credit Note' option in a job
* Posting AR CRD transactions when the net total of the transaction is a negative amount
* Creating periodic invoice credit notes
* Importing receivables sister company credit notes
* Creating consol costing credit notes"
				, ItemSet.ReceivablePreventCreationOfCreditNotes.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.Value);
		}

		public void TestReceivablePreventCreationOfReversalTransactions()
		{
			AssertEquals("Name", "ReceivablePreventCreationOfReversalTransactions", ItemSet.ReceivablePreventCreationOfReversalTransactions.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.ReceivablePreventCreationOfReversalTransactions.Category);
			AssertEquals("Caption", "Prevent Reversal of Invoice Transactions", ItemSet.ReceivablePreventCreationOfReversalTransactions.Caption);
			AssertEquals("Hint"
				, @"This registry controls the ability to create reversal credit notes in Accounts Receivables.

Setting this registry to 'Yes' will prevent all users in your login company from reversing any document posted in Receivables."
				, ItemSet.ReceivablePreventCreationOfReversalTransactions.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.Value);
		}

		public void TestNegativeAmountAllowedOnAccountReceivableTransactions()
		{
			AssertEquals("Name", "NegativeAmountAllowedOnAccountReceivableTransactions", ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.Name);
			AssertEquals("Category", "Accounting/Receivable Defaults/Default Settings", ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.Category);
			AssertEquals("Caption", "Negative Charges on Accounts Receivable Transactions", ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.Caption);
			AssertEquals("Hint", @"By default, the system allows negative charge lines in all AR transactions.

Based on your own needs, you can override this registry either to prevent users from posting negative charges in AR Invoice, Credit Note, or Adjustment Note transactions, or to allow negative charges only when the Debtor is Not Applicable to Tax.

ALL - Negative Charges Allowed - Allows negative line amounts on all the AR transactions. 
NAL - Negative Charges Not Allowed - Stops users from posting negative line amounts on AR Invoice, Credit Note or Adjustment Note transactions. 
NOT - Negative Charges Allowed if the Debtor is Not Applicable to Tax

Note: This registry will be set to NAL option by default for Vietnam login companies with Receivables E-Reporting Functionality enabled as the negative charge lines will cause the E-reporting transmission error. You can choose to override this registry to NOT if there is a need to post Non-Taxable invoice with negative charges.", ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.Options);
			AssertEquals("Default value", TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.DefaultValue);
		}

		public void TestNegativeAmountAllowedOnAccountReceivableTransactions_VNCompany()
		{
			var vnCompany = Factory.NewWithValidTestData<GlbCompany>();
			vnCompany.GC_RN_NKCountryCode = Constants.CountryCodes.VietNam;
			Factory.Save();
			AssertEquals("Default Value should be ALL when company is in Vietnam but EnableEInvoicingFunctionality is false", TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.GetFallBackValueAtAllLevels(vnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			ItemSet.EnableEInvoicingFunctionality.SetValue(vnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			vnCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Chile;
			Factory.Save();

			AssertEquals("Default Value should be ALL when company is not in Vietnam", TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code, ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.GetFallBackValueAtAllLevels(vnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			vnCompany.GC_RN_NKCountryCode = Constants.CountryCodes.VietNam;
			Factory.Save();

			AssertEquals("Default Value should be NAL when company is in Vietnam and EnableEInvoicingFunctionality is true", TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Disallowed.Code, ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.GetFallBackValueAtAllLevels(vnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			AssertExceptionThrown<RegistryValidationException>(
				"You can override the registry to ALL option only when the Receivables E-Reporting Functionality is disabled.",
				() => ItemSet.NegativeAmountAllowedOnAccountReceivableTransactions.SetTemporaryValue(vnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TransactionLineNegativeAmountAllowedOnAccountReceivableModes.Allowed.Code)
			);
		}

		#endregion

		#region Accounting_PayableDefaults_DefaultSettings

		public void TestEnableRejectionERequestInTransactionPendingAllocationApproval()
		{
			AssertEquals("Name", "EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval", ItemSet.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.Category);
			AssertEquals("Caption", "Enable Rejection eInvoicing Request in Transaction Pending Allocation Approval", ItemSet.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.Caption);
			AssertEquals("Hint", "Enable Rejection eInvoicing Request in Transaction Pending Allocation Approval (For Support Only)", ItemSet.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.Options);
			AssertEquals("Default value", false, Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.Value);
		}

		public void TestPayablePreventCreationOfCreditNotes()
		{
			AssertEquals("Name", "PayablePreventCreationOfCreditNotes", ItemSet.PayablePreventCreationOfCreditNotes.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.PayablePreventCreationOfCreditNotes.Category);
			AssertEquals("Caption", "Prevent Creation of Credit Notes", ItemSet.PayablePreventCreationOfCreditNotes.Caption);
			AssertEquals("Hint"
				, @"This registry controls the ability to create credit notes in Accounts Payables.

Setting this registry to 'Yes' will prevent all users in your login company from performing the following tasks:


* Posting AP CRD transactions by entering negative cost amounts
* Creating a stand alone credit note directly in the payables module
* Creating incomplete Payables credit notes
* Importing AP Credit notes via Universal XML
* Accepting import of sister invoices with claim via XML"
				, ItemSet.PayablePreventCreationOfCreditNotes.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.Value);
		}

		public void TestPayablesPreventCreationOfReversalTransactions()
		{
			AssertEquals("Name", "PayablePreventCreationOfReversalTransactions", ItemSet.PayablePreventCreationOfReversalTransactions.Name);
			AssertEquals("Category", "Accounting/Payable Defaults/Default Settings", ItemSet.PayablePreventCreationOfReversalTransactions.Category);
			AssertEquals("Caption", "Prevent Reversal of Invoice Transactions", ItemSet.PayablePreventCreationOfReversalTransactions.Caption);
			AssertEquals("Hint"
				, @"This registry controls the ability to create reversal credit notes in Accounts Payables.

Setting this registry to 'Yes' will prevent all users in your login company from reversing any document posted in Payables."
				, ItemSet.PayablePreventCreationOfReversalTransactions.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.Options);
			AssertEquals("Default value", false, AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfReversalTransactions.Value);
		}

		public void TestAllowDuplicateInvoiceNumberDefaultingRule()
		{
			var regItem = ItemSet.AllowDuplicateInvoiceNumberDefaultingRule;
			AssertEquals("AllowDuplicateInvoiceNumberDefaultingRule", regItem.Name);
			AssertEquals(Categories.Accounting_PayableDefaults_DefaultSettings, regItem.Category);
			AssertEquals("Allow Duplicate Invoice Number Defaulting Rule", regItem.Caption);
			AssertEquals(
				@"This registry is to determine whether authorized users can post a duplicate invoice number for a creditor based on whether the last used invoice date is more than 12 months old, or whether it is in the previous calendar year.

When set to STD, an authorized user can post an invoice number previously used for the same creditor provided it is more than 12 months since the last invoice date.

When set to CAL, an authorized user can post an invoice number previously used for the same creditor provided it has not been used in the current calendar year.

This registry works in conjunction with the Manage > Payables Transactions > New transactions > Allow Duplicate AP invoice Number security setting. Without this security right granted, a user will not be able to re-use an invoice number regardless of the setting in this registry.

Note: This registry also has action when importing AP invoices via XUT.",
				regItem.Hint);
			AssertEquals(RegistryStorageFlags.Company, regItem.Storage);
			AssertEquals(RegistryOptions.Default, regItem.Options);
			AssertEquals(AllowDuplicateInvoiceNumberRules.STD.Code, regItem.DefaultValue);

			AssertCodePair(AllowDuplicateInvoiceNumberRules.STD);
			AssertCodePair(AllowDuplicateInvoiceNumberRules.CAL);

			void AssertCodePair(CodeDescriptionPair codePair)
			{
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codePair.Code);
				AssertEquals(codePair.Code, regItem.Value);
			}
		}

		#endregion

		public void TestEnableNewTurkeyARComplianceFeaturesFrom()
		{
			AssertEquals("EnableNewTurkeyARComplianceFeaturesFrom", ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey, ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.Category);
			AssertEquals("Enable New Turkey AR Compliance Features from", ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.Caption);
			AssertEquals(@"If the current date is greater than or equal to value in this registry, then enable the New Turkey AR Compliance Features.

If the current date is less than the value in this registry, the new features will not be available.

Use:
1. During Testing, WTG staff can change this registry to today and test the features
2. Once all the functionality is checked in and we are ready for the world to know what we did, we will remove the this registry Item and the features will be available as part of the standard installation.",
ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.Options);
			AssertEquals(Convert.ToDateTime(DateTime.MinValue, CultureInfo.CurrentCulture), ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.DefaultValue);
		}

		public void TestEnableNewTurkeyAPComplianceFeaturesFrom()
		{
			AssertEquals("EnableNewTurkeyAPComplianceFeaturesFrom", ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey, ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.Category);
			AssertEquals("Enable New Turkey AP Compliance Features from", ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.Caption);
			AssertEquals(@"If the current date is greater than or equal to value in this registry, then enable the New Turkey AP Compliance Features.

If the current date is less than the value in this registry, the new features will not be available.

Use:
1. During Testing, WTG staff can change this registry to today and test the features
2. Once all the functionality is checked in and we are ready for the world to know what we did, we will remove the this registry Item and the features will be available as part of the standard installation.",
ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.Options);
			AssertEquals(Convert.ToDateTime(DateTime.MinValue, CultureInfo.CurrentCulture), ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.DefaultValue);
		}

		public void TestEnableNewTurkeyARComplianceFeatures()
		{
			ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.DefaultValue);
			var testValue = new ZDateTime(ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.Value);
			AssertEquals("Default value, not overridden", DateTime.MinValue, testValue);
			AssertEquals("Must be false", false, ItemSet.EnableNewTurkeyARComplianceFeatures);

			testValue = ZDate.Today.AddDays(1);
			ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testValue.ToDateTime());
			AssertEquals("Overridden with future value", ZDate.Today.AddDays(1), testValue);
			AssertEquals("Must be false", false, ItemSet.EnableNewTurkeyARComplianceFeatures);

			testValue = ZDate.Today;
			ItemSet.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testValue.ToDateTime());
			AssertEquals("Overridden with now", ZDate.Today, testValue);
			AssertEquals("Must be true", true, ItemSet.EnableNewTurkeyARComplianceFeatures);
		}

		public void TestEnableNewTurkeyAPComplianceFeatures()
		{
			ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.DefaultValue);
			var testValue = new ZDateTime(ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.Value);
			AssertEquals("Default value, not overridden", DateTime.MinValue, testValue);
			AssertEquals("Must be false", false, ItemSet.EnableNewTurkeyAPComplianceFeatures);

			testValue = ZDate.Today.AddDays(1);
			ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testValue.ToDateTime());
			AssertEquals("Overridden with future value", ZDate.Today.AddDays(1), testValue);
			AssertEquals("Must be false", false, ItemSet.EnableNewTurkeyAPComplianceFeatures);

			testValue = ZDate.Today;
			ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testValue.ToDateTime());
			AssertEquals("Overridden with now", ZDate.Today, testValue);
			AssertEquals("Must be true", true, ItemSet.EnableNewTurkeyAPComplianceFeatures);
		}

		public void TestEnableTPARReportingForAustralia()
		{
			var registerItem = ItemSet.EnableComplianceReportsForAustralia;
			TestRegistryItem(registerItem,
				"EnableComplianceReportsForAustralia",
				AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
				"Enable Compliance Reports for Australia",
				@"This registry is referenced by Australian login companies only.

When enabled, CargoWise can generate the following reports:
- TPAR (Taxable Payments Annual Report) output, required to be lodged annually. 
- PTRS (Payment Times Reporting Scheme) output, required to be lodged every 6 months

Not all AU businesses are required to lodge either the TPAR return and/or the PTRS return.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestTaxInvoiceStatusUpdateAutomatedRequestScheduleRegistryItem()
		{
			AssertEquals("TaxInvoiceStatusUpdateAutomatedRequestSchedule", ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Name);
			AssertEquals(AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Category);
			AssertEquals("Tax Invoice Status Update Automated Request Schedule", ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Caption);
			AssertEquals(@"This registry item is referenced by Turkey login companies only.
When electronic invoicing is enabled for a Turkey login company, CW1 will automatically send status requests for eligible receivables tax invoices to Uyumsoft. This will check for any change to the current status of the electronic tax invoices. 
Only receivables tax invoices that do not have a final status of success or canceled will automatically have requests sent to Uyumsoft for the transaction status update.
This registry setting allows the login company to set the minimum time interval (in minutes) between automated requests being made to Uyumsoft for a status update on any receivables tax invoice.
In addition, where users require an even more recent status, they can also separately request a status update of a receivables tax invoice from the Receivables > Receivables Transactions module.", ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Storage);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Options);
			AssertEquals(60, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.DefaultValue);

			ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.DefaultValue);
			AssertEquals("Default value, not overridden", 60, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Value);

			ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 90);
			AssertEquals("Overridden with different value", 90, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Value);

			try
			{
				ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 59);
			}
			catch (Exception ex)
			{
				AssertEquals("Value must be greater than or equal to the minimum (60)", ex.Message);
			}

			try
			{
				ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1441);
			}
			catch (Exception ex)
			{
				AssertEquals("Value must be less than or equal to the maximum (1440)", ex.Message);
			}
		}

		public void TestTaxInvoiceStatusUpdateAutomatedRequestScheduleVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MaxValue);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Options);
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1));
			AssertEquals(RegistryOptions.Default, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Options);
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MaxValue);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.TaxInvoiceStatusUpdateAutomatedRequestSchedule.Options);
		}

		public void TestUseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport()
		{
			TestRegistryItem(ItemSet.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport,
				"UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport",
				Categories.Accounting_PayableDefaults_DefaultSettings,
				"Use Import Company Charge Code's Tax Overrides for Intercompany Invoice Import",
				 @"By default, the Tax ID will be set according to system defined tax defaulting logic.
When this system registry is enabled, the system will set the Tax ID according to your login company's charge code tax overrides setup before it falls back to system defined tax defaulting logic.

A new 'Transaction Context' column has been added to facilitate the defining of GST Tax Overrides for Intercompany Invoice Import in the following screens:
a. Maintain > Account > Charge Codes;
b. Maintain > Account > Tax Override Groups;

With GST Tax Overrides, you have the benefit of specifying the applicable Tax ID and Tax Message.

Note:
This registry must be set to 'Yes' before you can define the GST Tax Overrides for 'Intercompany Invoice Import' context.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);

			using (ItemSet.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				Assert(ItemSet.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			}

			using (ItemSet.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				Assert(!ItemSet.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			}
		}

		public void TestEnableTaxBranchFeature()
		{
			TestRegistryItem(ItemSet.EnableTaxBranchFeature,
				"EnableTaxBranchFeature",
				Categories.Accounting,
				"Enable Tax Branch Feature",
				@"We will be developing the Tax Branch Reporting in phases.
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
				false
			);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EnableTaxBranchFeature, oldValue, newValue);
			var logReference = ItemSet.EnableTaxBranchFeature.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestEnableTaxBranchReporting()
		{
			TestRegistryItem(ItemSet.EnableTaxBranchReporting,
				"EnableTaxBranchReporting",
				Categories.Accounting,
				"Enable Tax Branch Reporting",
				@"By default, this registry is set to 'No' and this feature is not enabled.

When enabled, a tax branch value must be recorded against revenue and cost if the current login company is Tax Registered and the debtor/creditor is tax applicable.

This value will be used for defaulting of tax id and allocation of compliance sequence number, etc.

This value will be available for export via Universal Shipment, Transaction and Transaction Batch XML.

This value will be used in 'Tax Transaction Analysis - Detail Report' and 'Tax Transaction Analysis - Summary Report'. This value will not be used in other reports.

Please do not enable this registry without consultation with the Accounting Product Team.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false
			);

			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EnableTaxBranchReporting, oldValue, newValue);
			var logReference = ItemSet.EnableTaxBranchReporting.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestAPListAutomatedRequestScheduleRegistryItem_WhenTurkeyAPComplianceFeaturesEnabled() =>
		AssertAPListAutomatedRequestScheduleRegistryItem(RegistryOptions.Default, true);

		public void TestAPListAutomatedRequestScheduleRegistryItem_WhenTurkeyAPComplianceFeaturesNotEnabled() =>
			AssertAPListAutomatedRequestScheduleRegistryItem(RegistryOptions.IsHidden, false);

		public void TestAPListAutomatedRequestScheduleVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MaxValue);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.APListAutomatedRequestSchedule.Options);
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1));
			AssertEquals(RegistryOptions.Default, ItemSet.APListAutomatedRequestSchedule.Options);
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MaxValue);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.APListAutomatedRequestSchedule.Options);
		}

		void AssertAPListAutomatedRequestScheduleRegistryItem(RegistryOptions visibilityOption, bool isFunctionalityEnabled)
		{
			var registryDate = isFunctionalityEnabled ? ZDateTime.Now.AddDays(-2) : ZDateTime.Now.AddDays(2);

			using (ItemSet.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDate.ToDateTime()))
			{
				AssertEquals("Turkey AP Functionality status", isFunctionalityEnabled, ItemSet.EnableNewTurkeyAPComplianceFeatures);
				AssertEquals("APListAutomatedRequestSchedule", ItemSet.APListAutomatedRequestSchedule.Name);
				AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey, ItemSet.APListAutomatedRequestSchedule.Category);
				AssertEquals("AP List Automated Request Schedule", ItemSet.APListAutomatedRequestSchedule.Caption);
				AssertEquals(@"This registry is currently only referenced by Turkey login companies.
When electronic invoicing is enabled for a Turkey login company, CW1 will automatically send requests for eligible payables tax invoices to Uyumsoft. This will check for any existing payables tax invoices.
This registry setting allows the login company to set the minimum time interval (in minutes) between automated requests being made to Uyumsoft for a list request on payables tax invoices.", ItemSet.APListAutomatedRequestSchedule.Hint);
				AssertEquals(RegistryStorageFlags.Company, ItemSet.APListAutomatedRequestSchedule.Storage);
				AssertEquals(visibilityOption, ItemSet.APListAutomatedRequestSchedule.Options);
				AssertEquals(60, ItemSet.APListAutomatedRequestSchedule.DefaultValue);

				ItemSet.APListAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItemSet.APListAutomatedRequestSchedule.DefaultValue);
				AssertEquals("Default value, not overridden", 60, ItemSet.APListAutomatedRequestSchedule.Value);

				ItemSet.APListAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 90);
				AssertEquals("Overridden with different value", 90, ItemSet.APListAutomatedRequestSchedule.Value);

				try
				{
					ItemSet.APListAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 59);
				}
				catch (Exception ex)
				{
					AssertEquals("Value must be greater than or equal to the minimum (60)", ex.Message);
				}

				try
				{
					ItemSet.APListAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1441);
				}
				catch (Exception ex)
				{
					AssertEquals("Value must be less than or equal to the maximum (1440)", ex.Message);
				}
			}
		}

		public void TestTaxIdAndTaxMessageCombinationRules()
		{
			var expectedHint = @"Tax Messages are used to explain the VAT/GST Tax ID treatment of Revenue and Cost charge lines.

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
DPY rules defined in this registry apply to charge lines in Cash Book Direct Payment transactions.";

			AssertEquals("Name", "TaxIdAndTaxMessageCombinationRules", ItemSet.TaxIdAndTaxMessageCombinationRules.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, ItemSet.TaxIdAndTaxMessageCombinationRules.Category);
			AssertEquals("Caption", "Tax ID and Tax Message Combination Rules", ItemSet.TaxIdAndTaxMessageCombinationRules.Caption);
			AssertEquals("Hint", expectedHint, ItemSet.TaxIdAndTaxMessageCombinationRules.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, Instance.TaxIdAndTaxMessageCombinationRules.Storage);
			AssertEquals("Options", RegistryOptions.Default, Instance.TaxIdAndTaxMessageCombinationRules.Options);
			AssertEquals("Default value", 0, ItemSet.TaxIdAndTaxMessageCombinationRules.Value.TaxIdAndTaxMessageCombinationRulesCollection.Count);
			AssertEquals("Default validationOption", AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage, GetNewItemSet().TaxIdAndTaxMessageCombinationRules.Value.ValidationOption);
		}

		public void TestOnBuildLogReferenceForTaxIdAndTaxMessageCombinationRules()
		{
			var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var taxRate1 = taxRateCollection.AddNew();
			taxRate1.AT_Code = "TGST";
			taxRate1.AT_Type = "RAT";
			taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate1.SetRateNumerator_ForTestOnly(10);
			var taxRate2 = taxRateCollection.AddNew();
			taxRate2.AT_Code = "TFREEGST";
			taxRate2.AT_Type = "RAT";
			taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate2.SetRateNumerator_ForTestOnly(10);

			var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var taxMessage1 = taxMessageCollection.AddNew();
			taxMessage1.A9_Code = "AAAAA";
			taxMessage1.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var taxMessage2 = taxMessageCollection.AddNew();
			taxMessage2.A9_Code = "BBBBB";
			taxMessage2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			IRegistryItem regItem = Instance.TaxIdAndTaxMessageCombinationRules.Inner;

			var configuration1 = Instance.TaxIdAndTaxMessageCombinationRules.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var configuration2 = Instance.TaxIdAndTaxMessageCombinationRules.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var item_configuration2 = configuration2.TaxIdAndTaxMessageCombinationRulesCollection.AddNew();
			using (item_configuration2.GetValidationSuspender())
			{
				item_configuration2.LineType = "CST";
				item_configuration2.TaxRate = taxRate1.PK;
				item_configuration2.TaxMessage = taxMessage1.PK;

				var configuration3 = Instance.TaxIdAndTaxMessageCombinationRules.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var item_configuration3 = configuration3.TaxIdAndTaxMessageCombinationRulesCollection.AddNew();
				using (item_configuration3.GetValidationSuspender())
				{
					item_configuration3.LineType = "DRC";
					item_configuration3.TaxRate = taxRate2.PK;
					item_configuration3.TaxMessage = taxMessage2.PK;

					var configuration4 = Instance.TaxIdAndTaxMessageCombinationRules.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					var item_configuration4 = configuration4.TaxIdAndTaxMessageCombinationRulesCollection.AddNew();
					using (item_configuration4.GetValidationSuspender())
					{
						item_configuration4.LineType = "DRC";
						item_configuration4.TaxRate = taxRate2.PK;
						item_configuration4.TaxMessage = taxMessage2.PK;
						configuration4.ValidationOption = AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID;

						var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, configuration1, configuration2);
						var result = Instance.TaxIdAndTaxMessageCombinationRules.OnBuildLogReference(args);
						AssertEquals(@"Rule Added: Line Type:CST, Tax ID:TGST, Tax Message:AAAAA
", result);

						args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, configuration2, configuration1);
						result = Instance.TaxIdAndTaxMessageCombinationRules.OnBuildLogReference(args);
						AssertEquals(@"Rule Deleted: Line Type:CST, Tax ID:TGST, Tax Message:AAAAA
", result);

						args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, configuration2, configuration3);
						result = Instance.TaxIdAndTaxMessageCombinationRules.OnBuildLogReference(args);
						AssertEquals(@"Rule Added: Line Type:DRC, Tax ID:TFREEGST, Tax Message:BBBBB
Rule Deleted: Line Type:CST, Tax ID:TGST, Tax Message:AAAAA
", result);

						args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, configuration3, configuration4);
						result = Instance.TaxIdAndTaxMessageCombinationRules.OnBuildLogReference(args);
						AssertEquals(@"Validation Option set to 'IDS - Limit permitted Tax IDs'.
", result);

						args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, configuration4, configuration3);
						result = Instance.TaxIdAndTaxMessageCombinationRules.OnBuildLogReference(args);
						AssertEquals(@"Validation Option set to 'MSG - Limit permitted Tax Message'.
", result);

						args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, configuration2, configuration4);
						result = Instance.TaxIdAndTaxMessageCombinationRules.OnBuildLogReference(args);
						AssertEquals(@"Rule Added: Line Type:DRC, Tax ID:TFREEGST, Tax Message:BBBBB
Rule Deleted: Line Type:CST, Tax ID:TGST, Tax Message:AAAAA
Validation Option set to 'IDS - Limit permitted Tax IDs'.
", result);
					}
				}
			}
		}

		public void TestComplianceDocumentNumberAllocationRule_Receivables()
		{
			AssertEquals("Name", "ComplianceDocumentNumberAllocationRule_Receivables", ItemSet.ComplianceDocumentNumberAllocationRuleReceivables.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, ItemSet.ComplianceDocumentNumberAllocationRuleReceivables.Category);
			AssertEquals("Caption", "Compliance Document Number Allocation Rule - Receivables", ItemSet.ComplianceDocumentNumberAllocationRuleReceivables.Caption);
			AssertEquals("Hint", @"This registry determines how the  compliance number will be allocated. 

By default, the system uses the compliance sequence book that belongs to the current login branch and department to allocate a compliance number.
If required, you can override this registry setting to use the compliance sequence book that belongs to the transaction header branch and department. 

Note: This registry is only relevant to login countries where compliance sequences module has been enabled for the allocation of compliance numbers to transactions.", ItemSet.ComplianceDocumentNumberAllocationRuleReceivables.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentNumberAllocationRuleReceivables.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentNumberAllocationRuleReceivables.Options);
			AssertEquals("Default Value", ComplianceDocumentNumberAllocationRuleTypes.LBD.Code, ItemSet.ComplianceDocumentNumberAllocationRuleReceivables.DefaultValue);

			var registry = ItemSet.ComplianceDocumentNumberAllocationRuleReceivables;
			var oldValue = ComplianceDocumentNumberAllocationRuleTypes.LBD.Code;
			var newValue = ComplianceDocumentNumberAllocationRuleTypes.HBD.Code;
			var expectedLogMessage = $"Registry has been overridden from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestComplianceDocumentNumberAllocationRule_Payables()
		{
			AssertEquals("Name", "ComplianceDocumentNumberAllocationRule_Payables", ItemSet.ComplianceDocumentNumberAllocationRulePayables.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, ItemSet.ComplianceDocumentNumberAllocationRulePayables.Category);
			AssertEquals("Caption", "Compliance Document Number Allocation Rule - Payables", ItemSet.ComplianceDocumentNumberAllocationRulePayables.Caption);
			AssertEquals("Hint", @"This registry determines how the  compliance number will be allocated. 

By default, the system uses the compliance sequence book that belongs to the current login branch and department to allocate a compliance number.
If required, you can override this registry setting to use the compliance sequence book that belongs to the transaction header branch and department. 

Note: This registry is only relevant to login countries where compliance sequences module has been enabled for the allocation of compliance numbers to transactions.", ItemSet.ComplianceDocumentNumberAllocationRulePayables.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ComplianceDocumentNumberAllocationRulePayables.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ComplianceDocumentNumberAllocationRulePayables.Options);
			AssertEquals("Default Value", ComplianceDocumentNumberAllocationRuleTypes.LBD.Code, ItemSet.ComplianceDocumentNumberAllocationRulePayables.DefaultValue);

			var registry = ItemSet.ComplianceDocumentNumberAllocationRulePayables;
			var oldValue = ComplianceDocumentNumberAllocationRuleTypes.LBD.Code;
			var newValue = ComplianceDocumentNumberAllocationRuleTypes.HBD.Code;
			var expectedLogMessage = $"Registry has been overridden from {oldValue} to {newValue}.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestOFXEncryptionKey()
		{
			AssertEquals("DefaultValue", "", ItemSet.OFXEncryptionKey.DefaultValue);
			AssertEquals("Name", "OFXEncryptionKey", ItemSet.OFXEncryptionKey.Name);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.OFXEncryptionKey.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden | RegistryOptions.IsReadOnly, ItemSet.OFXEncryptionKey.Options);
		}

		public void TestPrintGLVoucherBasedOnTransactionLineBranch()
		{
			AssertEquals("Name", "PrintGLVoucherBasedOnTransactionLineBranch", ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.Name);
			AssertEquals("Category", Categories.Accounting_GeneralLedgerDefaults, ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.Category);
			AssertEquals("Caption", "Print GL Voucher Based on Transaction Line Branch", ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.Caption);
			AssertEquals("Hint", @"By default, the system prints Accounting Voucher for WIP/ACR at company level.
You are allowed to print only one WIP and one ACR accounting voucher monthly for a company.

When the registry is set to 'Yes', WIP/ACR accounting voucher will be printed at branch level.
That means, WIP/ACR with different branches will be printed in separate accounting voucher monthly.", ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.Hint);
			AssertEquals("DefaultValue", false, Instance.PrintGLVoucherBasedOnTransactionLineBranch.DefaultValue);
			AssertEquals("Only show China and TaiWan", true,
				ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.CountryFilterPKs.Contains(Constants.CountryGuids.Taiwan)
				&& ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.CountryFilterPKs.Contains(Constants.CountryGuids.China)
				&& ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.CountryFilterPKs.Count() == 2);

			ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("PrintGLVoucherBasedOnTransactionLineBranch value is true", true, ItemSet.PrintGLVoucherBasedOnTransactionLineBranch.Value);
		}

		public void TestShowChinaGBTDataInterfaceMenus()
		{
			AssertEquals("Name", "ShowChinaGBTDataInterfaceMenus", ItemSet.ShowChinaGBTDataInterfaceMenus.Name);
			AssertEquals("Category", Categories.Accounting_GeneralLedgerDefaults, ItemSet.ShowChinaGBTDataInterfaceMenus.Category);
			AssertEquals("Caption", "Show China GB-T Data Interface Menus", ItemSet.ShowChinaGBTDataInterfaceMenus.Caption);
			AssertEquals("Hint", @"By default, this registry is set to 'No' in which case the following menus will be hidden in China.
1. GB-T 19851-2004 Data-interface
2. GB-T 24589.1 Data-interface

When this registry is set to 'Yes', these menus will be shown.", ItemSet.ShowChinaGBTDataInterfaceMenus.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.ShowChinaGBTDataInterfaceMenus.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ShowChinaGBTDataInterfaceMenus.Options);
			AssertEquals("DefaultValue", false, ItemSet.ShowChinaGBTDataInterfaceMenus.DefaultValue);
		}

		public void TestEnableNewOSOutstandingAmountFeature()
		{
			AssertEquals("Name", "EnableNewOSOutstandingAmountFeature", ItemSet.EnableNewOSOutstandingAmountFeature.Name);
			AssertEquals("Categroy", Categories.Accounting, ItemSet.EnableNewOSOutstandingAmountFeature.Category);
			AssertEquals("Caption", "Enable New OS Outstanding Amount Feature (CargoWiseOne Support only)", ItemSet.EnableNewOSOutstandingAmountFeature.Caption);
			AssertEquals("Hint", @"By default, this registry is set to 'No' and this feature is not enabled.
Once it's set to 'Yes', this registry cannot be turn off.", ItemSet.EnableNewOSOutstandingAmountFeature.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.EnableNewOSOutstandingAmountFeature.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableNewOSOutstandingAmountFeature.Options);
			AssertEquals("DefaultValue", false, ItemSet.EnableNewOSOutstandingAmountFeature.DefaultValue);
		}

		public void TestDisallowPostingInvoicesWithAFutureInvoiceDate()
		{
			AssertEquals("Name", "DisallowPostingInvoicesWithAFutureInvoiceDate", ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate.Name);
			AssertEquals("Categroy", Categories.Accounting_ReceivableDefaults_DefaultSettings, ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate.Category);
			AssertEquals("Caption", "Disallow Posting Invoices With A Future Invoice Date", ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate.Caption);
			AssertEquals("Hint", @"By default, this registry is set to 'No' and no restriction will be applied.

When the registry is set to 'Yes', the system will prevent users from posting AR Invoice, Credit Note and Adjustment Note with a future invoice date.", ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate.Options);
			AssertEquals("DefaultValue", false, ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate.DefaultValue);

			var registry = ItemSet.DisallowPostingInvoicesWithAFutureInvoiceDate;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry set to '{newValue}'.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestComplianceReportQueryTimeoutsInMinutes()
		{
			var registryItem = ItemSet.ComplianceReportQueryTimeoutInMinutes;

			AssertEquals("Name", "ComplianceReportQueryTimeoutInMinutes", registryItem.Name);
			AssertEquals("Categroy", Categories.Accounting_GovernmentComplianceInvoiceDocument, registryItem.Category);
			AssertEquals("Caption", "Compliance Report Query Timeout (minutes)", registryItem.Caption);
			AssertEquals("Hint", @"This registry sets the SQL timeout for Compliance Report queries in minutes.
If you experience query timeouts while working with Compliance Reports, you can increase this registry as a temporary workaround.
Please reset this registry to the default after your Compliance Report is completed."
				, registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals("Options", RegistryOptions.Default, registryItem.Options);
			AssertEquals("DefaultValue", 30, registryItem.DefaultValue);
			IntRegistryDataType dataType = (IntRegistryDataType)registryItem.DataType;
			AssertEquals("MinValue", 30, (int)dataType.LowerBound);
			AssertEquals("MaxValue", 20000, (int)dataType.UpperBound);
		}

		#region GenerateJournalEntriesForPostedAccountingTransactions

		public void TestGenerateJournalEntriesForPostedAccountingTransactions()
		{
			AssertEquals("Name", "GenerateJournalEntriesForPostedAccountingTransactions", ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.Name);
			AssertEquals("Category", Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.Category);
			AssertEquals("Caption", "Generate Journal Entries for Posted Accounting Transactions (CWSupport Only)", ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.Caption);
			AssertEquals("Hint", @"By default, this feature is disabled.

						When enabled, the system will commence to generate journal entries for all accounting transactions upon posting.
						If required, journal entries for transactions posted before enabling this feature can be generated via menu option in Period Management module.",
				ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.Options);
			AssertEquals("DefaultValue", false, ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.DefaultValue);

			var registry = ItemSet.GenerateJournalEntriesForPostedAccountingTransactions;
			var oldValue = false;
			var newValue = true;
			var expectedLogMessage = $"Registry set to '{newValue}'.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);

			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestGenerateJournalEntriesForPostedAccountingTransactions_DefaultValue()
		{
			AssertEquals("DefaultValue", false, ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.DefaultValue);

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateJournalEntriesForPostedAccountingTransactions", TimeSpan.MinValue);

			var mockIFeatureData = new Mock<IFeatureData>();
			var generalLedgerFeatureControlData = new GeneralLedgerDataFeatureControlModel() { EnableGenerateJournalEntriesForPostedAccountingTransactions = false };
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out generalLedgerFeatureControlData)).Returns(true);
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals("DefaultValue should be false if EnableGenerateJournalEntriesForPostedAccountingTransactions is false.", false, ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.DefaultValue);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateJournalEntriesForPostedAccountingTransactions", TimeSpan.MinValue);

			generalLedgerFeatureControlData.EnableGenerateJournalEntriesForPostedAccountingTransactions = true;
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out generalLedgerFeatureControlData)).Returns(true);
			using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
			{
				AssertEquals("DefaultValue should be true if EnableGenerateJournalEntriesForPostedAccountingTransactions is true.", true, ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.DefaultValue);
			}
		}

		public void TestGenerateJournalEntriesForPostedAccountingTransactions_OnUpdateAction_DefaultValue()
		{
			var currentRegistry = ItemSet.GenerateJournalEntriesForPostedAccountingTransactions;
			var targetRegistry = ItemSet.ConsolidatedAccountingCategoryList;
			targetRegistry.Options = RegistryOptions.NotCached;

			Assert("Should not have actual value", !HasActualValue(targetRegistry));
			AssertContainsExactElementsInAnyOrder(targetRegistry.DefaultValue, targetRegistry.Value);

			currentRegistry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("Should have actual value", HasActualValue(targetRegistry));
			AssertContainsExactElementsInAnyOrder(targetRegistry.DefaultValue, targetRegistry.Value);

			var stmData = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, "ConsolidatedAccountingCategory"));
			stmData.Delete();
			Factory.Save();

			targetRegistry.Options = RegistryOptions.NotCached;
			Assert("Should not have actual value", !HasActualValue(targetRegistry));
			currentRegistry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Should have actual value", HasActualValue(targetRegistry));
			AssertContainsExactElementsInAnyOrder(targetRegistry.DefaultValue, targetRegistry.Value);
		}

		public void TestGenerateJournalEntriesForPostedAccountingTransactions_OnUpdateAction_UpdatedValue()
		{
			var currentRegistry = ItemSet.GenerateJournalEntriesForPostedAccountingTransactions;
			var targetRegistry = ItemSet.ConsolidatedAccountingCategoryList;

			var updatedValue = targetRegistry.Value;
			updatedValue[0].Code = "AAA";
			targetRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, updatedValue);
			Assert("Should have actual value", HasActualValue(targetRegistry));
			currentRegistry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Should have actual value", HasActualValue(targetRegistry));
			AssertNotEquals(ValueToUse.DefaultValue,
				targetRegistry.Inner.GetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGenerateJournalEntriesForPostedAccountingTransactions_OnUpdateAction_PlAppropriationAccount()
		{
			var stmData = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, "GL_PL_APPROPRIATION_ACCOUNT"));
			stmData.Delete();
			Factory.Save();

			var currentRegistry = ItemSet.GenerateJournalEntriesForPostedAccountingTransactions;
			var targetRegistry = (RegistryItemImpl)ObjectFactory.Get<IAccounting>().PlAppropriationAccountRegistryItem;

			Assert("Should not have actual value", !HasActualValue(targetRegistry));
			currentRegistry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Should have actual value", HasActualValue(targetRegistry));
			AssertEquals(targetRegistry.DefaultValue, targetRegistry.Value);

			targetRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			currentRegistry.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertNotEquals(targetRegistry.DefaultValue, targetRegistry.Value);

			bool HasActualValue(RegistryItemImpl targetRegistry)
			{
				return ((IRegistryItemInternals)targetRegistry).HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		void AssertContainsExactElementsInAnyOrder(CodeDescriptionWithGroupCollection expected, CodeDescriptionWithGroupCollection actual)
		{
			AssertContainsExactElementsInAnyOrder(
				new CodeDescriptionWithGroupEqualityComparer(),
				expected.Cast<CodeDescriptionWithGroup>(),
				actual.Cast<CodeDescriptionWithGroup>()
			);
		}

		bool HasActualValue(RegistryItemWrapper targetRegistry)
		{
			return targetRegistry.Inner.HasActualValue(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#endregion

		public void TestGLJournalExchangeRateType()
		{
			var registry = ItemSet.GLJournalExchangeRateType;
			AssertEquals("Name", "GLJournalExchangeRateType", registry.Name);
			AssertEquals("Category", Categories.Accounting_GeneralLedgerDefaults, registry.Category);
			AssertEquals("Caption", "GL Journal Exchange Rate Type", registry.Caption);
			AssertEquals("Hint", "This registry defines which exchange rate type will be used to convert foreign currency journal value to local currency based on the GL Account Type during the creation of General Ledger Journal.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.Default, registry.Options);
			AssertEquals("Default BalanceSheetAccountTypeExchangeRateType", "PER", registry.DefaultValue.BalanceSheetAccountTypeExchangeRateType);
			AssertEquals("Default ProfitAndLossAccountTypeExchangeRateType", "PER", registry.DefaultValue.ProfitAndLossAccountTypeExchangeRateType);

			var oldValue = registry.DefaultValue;
			var newValue = new GLJournalExchangeRateType();
			newValue.BalanceSheetAccountTypeExchangeRateType = "COM";
			newValue.ProfitAndLossAccountTypeExchangeRateType = "COM";
			var expectedLogMessage = $"Balance Sheet Account Type Exchange Rate Type has been changed from PER to COM.\r\nProfit and Loss Account Type Exchange Rate Type has been changed from PER to COM.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = registry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		[TestDate(2023, 4, 27, 12, 12, 12)]
		public void TestGenerateJournalEntriesCDCStartDate()
		{
			TestGenericRegistryItem(ItemSet.GenerateJournalEntriesCDCStartDate, "GenerateJournalEntriesCDCStartDate", Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, "Generate Journal Entries - CDC Start Date (CWSupport Only)", @"By default, CDC is not enabled.
When a start date is specified and saved to the ""Generate Journal Entries - Start Date"" registry, the date (today's date) when the registry is set will be saved into this registry.
The system will commence to create CDC records for all journal entries created from this point.", RegistryStorageFlags.Company, AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly : RegistryOptions.IsHidden);

			AssertEquals("Default value is Empty", DateTime.MinValue, ItemSet.GenerateJournalEntriesCDCStartDate.DefaultValue);

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateJournalEntriesCDCStartDate", TimeSpan.MinValue);
			using (ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.GenerateJournalEntriesCDCStartDate.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("GenerateJournalEntriesCDCStartDate", TimeSpan.MinValue);
			using (ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is IsOnlyForSupport | IsReadOnly", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly, ItemSet.GenerateJournalEntriesCDCStartDate.Options);
			}

			var oldValue = ItemSet.GenerateJournalEntriesCDCStartDate.DefaultValue;
			var newValue = ZDateTime.Today.ToDateTime();
			var expectedLogMessage = $"CDC Start Date set to '27/04/2023 12:00:00 AM'.";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.GenerateJournalEntriesCDCStartDate, oldValue, newValue);
			AssertEquals(expectedLogMessage, ItemSet.GenerateJournalEntriesCDCStartDate.OnBuildLogReference(args));
		}

		public void TestGenerateJournalEntriesCDCStartDateVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.GenerateJournalEntriesCDCStartDate.Options);
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly, ItemSet.GenerateJournalEntriesCDCStartDate.Options);
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.GenerateJournalEntriesCDCStartDate.Options);
		}

		public void TestEnableCrossTradeDebtorDefaultingFunctionality()
		{
			AssertEquals("Name", "EnableCrossTradeDebtorDefaultingFunctionality", ItemSet.EnableCrossTradeDebtorDefaultingFunctionality.Name);
			AssertEquals("Category", "Accounting/Job Invoicing", ItemSet.EnableCrossTradeDebtorDefaultingFunctionality.Category);
			AssertEquals("Caption", "Enable Cross Trade Debtor Defaulting Functionality (CargoWiseOne Support Only)", ItemSet.EnableCrossTradeDebtorDefaultingFunctionality.Caption);
			var expectedHint = @"When this registry is set to Yes, on Cross Trade jobs, the Local Client field will read ‘Prepaid Bill-To Party’ and the Overseas Agent field will read 'Collect Bill-To Party’.
These fields will populate with the job’s Consignor’s IFT (falling back to Consignor) and Consignee’s IFT (falling back to Consignee) respectively.
The charge line debtor will default according to the Accounting > Job Invoicing > Cross Trade Debtor Defaulting Configuration registry. 
This applies to Forwarding Shipments, Quick Bookings and Bookings with Quotes.


These changes were implemented on PRJ00039869 and it is expected that this will be standard functionality and this registry will be removed.
IMPORTANT: Do not change this registry for any client. This registry will be removed in a future release and the behaviour will revert to the default. Should a client raise a complaint that the Cross Trade behaviour associated with this registry (as specified above) is not suitable, please escalate to the Accounting Product Team.";
			AssertEquals("Hint", expectedHint, ItemSet.EnableCrossTradeDebtorDefaultingFunctionality.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company | RegistryStorageFlags.System, ItemSet.EnableCrossTradeDebtorDefaultingFunctionality.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableCrossTradeDebtorDefaultingFunctionality.Options);
			AssertEquals("DefaultValue", true, ItemSet.EnableCrossTradeDebtorDefaultingFunctionality.DefaultValue);
		}

		[TestDate(2023, 4, 27, 0, 0, 0)]
		public void TestJournalEntriesLastQueuedDate()
		{
			var registryItem = ItemSet.JournalEntriesLastQueuedDate;
			AssertEquals("Name", "JournalEntriesLastQueuedDate", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, registryItem.Category);
			AssertEquals("Caption", "Journal Entries Last Queued Date (CWSupport Only)", registryItem.Caption);
			AssertEquals("Hint", @"When journal entries were queued by 'General Ledger Data Backlog Queue Service Task', the latest queued entry's post date will be saved into this registry.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesLastQueuedDate", TimeSpan.MinValue);
			using (ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.JournalEntriesLastQueuedDate.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesLastQueuedDate", TimeSpan.MinValue);
			using (ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is IsOnlyForSupport | IsReadOnly", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly, ItemSet.JournalEntriesLastQueuedDate.Options);
			}
		}

		public void TestJournalEntriesLastQueuedDateVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.JournalEntriesLastQueuedDate.Options);
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly, ItemSet.JournalEntriesLastQueuedDate.Options);
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.JournalEntriesLastQueuedDate.Options);
		}

		public void TestJournalEntriesLastProcessedDate()
		{
			var registryItem = ItemSet.JournalEntriesLastProcessedDate;
			AssertEquals("Name", "JournalEntriesLastProcessedDate", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries, registryItem.Category);
			AssertEquals("Caption", "Journal Entries Last Processed Date", registryItem.Caption);
			AssertEquals("Hint", @"When all journal entries queue records have been processed by 'GLP - General Ledger Data Backlog Process Service Task', the current 'Generate Journal Entries - Start Date' registry value will be saved into this registry.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesLastProcessedDate", TimeSpan.MinValue);
			using (ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Options is IsHidden", RegistryOptions.IsHidden, ItemSet.JournalEntriesLastProcessedDate.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("JournalEntriesLastProcessedDate", TimeSpan.MinValue);
			using (ItemSet.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Options is IsReadOnly", RegistryOptions.IsReadOnly, ItemSet.JournalEntriesLastProcessedDate.Options);
			}
		}

		public void TestJournalEntriesLastProcessedDateVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.JournalEntriesLastProcessedDate.Options);
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsReadOnly, ItemSet.JournalEntriesLastProcessedDate.Options);
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.JournalEntriesLastProcessedDate.Options);
		}

		public void TestTaxTypeToTaxInvoiceDocumentTypeCodeMapping()
		{
			var registryItem = ItemSet.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			AssertEquals("Name", "TaxTypeToTaxInvoiceDocumentTypeCodeMapping", registryItem.Name);
			AssertEquals("Category", Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea, registryItem.Category);
			AssertEquals("Caption", "Tax Type to Tax Invoice Document's Type Code Mapping (CargoWiseOne Support Only)", registryItem.Caption);
			AssertEquals("Hint", @"This is the mapping between Tax Type to Tax Invoice Document's Type Code Mapping. 
This registry specifies what Type Code should be used for different Tax Rate Types for Original/Amendment Transactions.", registryItem.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, registryItem.Storage);

			var edoitorInfo = registryItem.EditorInfo as CodeDescriptionWithGroupRegistryEditorInfo;
			AssertNotNull("EdoitorInfo.EditorType", edoitorInfo);
			AssertEquals("Tax Invoice Document/Type Code", edoitorInfo.GroupColumnCaption);
			AssertEquals(true, edoitorInfo.IsGroupColumnVisible);
			AssertEquals(true, edoitorInfo.IsOnlyGroupColumnEditable);

			var defaultValues = registryItem.DefaultValue.Cast<CodeDescriptionWithGroup>();
			AssertEquals(12, defaultValues.Count());
			AssertEquals(9, defaultValues.Count(x => x.Group == KoreaEInvoicingTypeCodeCategory.NotApplicable));
			AssertEquals(1, defaultValues.Count(x => x.Code == AccTaxRate.Types.Rated && x.Group == KoreaEInvoicingTypeCodeCategory.TaxInvoice));
			AssertEquals(1, defaultValues.Count(x => x.Code == AccTaxRate.Types.Exempt && x.Group == KoreaEInvoicingTypeCodeCategory.Invoice));
			AssertEquals(1, defaultValues.Count(x => x.Code == AccTaxRate.Types.CapitalRated && x.Group == KoreaEInvoicingTypeCodeCategory.TaxInvoice));

			var newCollection = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;

			newCollection.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == AccTaxRate.Types.Rated).Group = KoreaEInvoicingTypeCodeCategory.Invoice;
			var args1 = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, registryItem.DefaultValue, newCollection);
			var logReference1 = registryItem.OnBuildLogReference(args1);
			AssertMultilineASCIIEquals(@"Tax Type 'RAT', Tax Invoice Document/Type Code change from '01XX/02XX' to '03XX/04XX'.
", logReference1);

			newCollection.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == AccTaxRate.Types.Exempt).Group = KoreaEInvoicingTypeCodeCategory.TaxInvoice;
			var args2 = new RegistryItemWrapper.BuildLogReferenceArgs(registryItem, registryItem.DefaultValue, newCollection);
			var logReference2 = registryItem.OnBuildLogReference(args2);
			AssertMultilineASCIIEquals(@"Tax Type 'RAT', Tax Invoice Document/Type Code change from '01XX/02XX' to '03XX/04XX'.
Tax Type 'EXT', Tax Invoice Document/Type Code change from '03XX/04XX' to '01XX/02XX'.
", logReference2);
		}

		public void TestValidateTaxTypeToTaxInvoiceDocumentTypeCodeMapping()
		{
			var registryItem = ItemSet.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			var defaultValues = registryItem.DefaultValue.Cast<CodeDescriptionWithGroup>();
			var newCollection = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;
			var expectedMessage = "Tax Invoice Document/Type Code can not be '01XX/02XX' when Code is '<Null>'.";

			newCollection.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == AccountingMasterFilesConstants.NullTaxRateType.Code).Group = KoreaEInvoicingTypeCodeCategory.NotApplicable;
			AssertNoExceptionThrown(() => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCollection));

			newCollection.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == AccountingMasterFilesConstants.NullTaxRateType.Code).Group = KoreaEInvoicingTypeCodeCategory.Invoice;
			AssertNoExceptionThrown(() => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCollection));

			newCollection.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == AccountingMasterFilesConstants.NullTaxRateType.Code).Group = KoreaEInvoicingTypeCodeCategory.TaxInvoice;
			AssertExceptionThrown<RegistryValidationException>(expectedMessage, () => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCollection));
		}

		public void TestTaxTypeToTaxInvoiceDocumentTypeCodeMappingVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Options);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Options);
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Options);
		}

		public void TestAllowFrenchFECComplianceReportToUseStandardChartofAccounts()
		{
			AssertEquals("Name", "AllowFrenchFECComplianceReportToUseStandardChartofAccounts", ItemSet.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.Name);
			AssertEquals("Category", "Accounting/Government Compliance Invoice Document", ItemSet.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.Category);
			AssertEquals("Caption", "Allow French FEC compliance report to use the standard chart of accounts", ItemSet.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.Caption);
			AssertEquals("Hint", "This registry should only be activated when the French “Plan comptable general” is used as the standard chart of accounts in CW. When activated the accounts in the standard chart of accounts in CW will be used for the French FEC audit file and not the mapped accounts from the “GL Multi-Language Mapping” functionality.", ItemSet.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.Storage);
			AssertEquals("Default Value", false, ItemSet.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.DefaultValue);
			AssertEquals("CountryFilterPK", CountryFilterPKs.France, ItemSet.AllowFrenchFECComplianceReportToUseStandardChartofAccounts.CountryFilterPKs);
		}

		public void TestTaxTypeToTaxInvoiceDocumentTypCodeMappingIsVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Options);
		}

		public void TestTaxTypeToTaxInvoiceDocumentTypeCodeMappingIsNotVisible()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.TaxTypeToTaxInvoiceDocumentTypeCodeMapping.Options);
		}

		public void TestValidateGLAccountHasDissection()
		{
			var registriesWithValidateGLAccountHasDissection = new List<GuidRegistryItem>
			{
				ItemSet.GLJournalExchangeRateDifferenceAccount,
				ItemSet.TaxTransactionPrepaidAssetControlAccount,
				ItemSet.TaxTransactionRemittanceLiabilityControlAccount,
				ItemSet.PendingTaxTransactionPrepaidAssetControlAccount,
				ItemSet.PendingTaxTransactionRemittanceLiabilityControlAccount,
				ItemSet.TaxTransactionExpenseAccount,
				ItemSet.TaxTransactionNegativeRevenueAccount,
			};

			var factory = new BusinessObjectFactory();
			var testObjectCreator = new AccountingTestObjectCreator(factory);
			var glHeaderWithDissection = testObjectCreator.CreateAccGLHeader("GL1");
			var glHeaderWithoutDissection = testObjectCreator.CreateAccGLHeader("GL2");
			var chart = testObjectCreator.CreateAlternateChart("CH1");
			var dissection = glHeaderWithDissection.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = chart.PK;
			dissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			dissection.ADC_SeparateNumbering = true;
			factory.Save();

			foreach (var registryItem in registriesWithValidateGLAccountHasDissection)
			{
				AssertExceptionThrown<RegistryValidationException>("GL Accounts with Dissections cannot be selected", () => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeaderWithDissection.PK.ToGuid()));
				AssertNoExceptionThrown(() => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeaderWithoutDissection.PK.ToGuid()));
			}
		}

		public void TestValidateNotAllowedGLHeaderWhoseAlternateGLAccountIsMappedByMultipleGLAccountsSetToControlAccount()
		{
			var registriesWithValidateGLAccountHasDissection = AccountingMasterFilesUtils.NotAllowedForDissectionControlAccount;

			var factory = new BusinessObjectFactory();
			var testObjectCreator = new AccountingTestObjectCreator(factory);
			var glHeader = testObjectCreator.CreateAccGLHeader("GL1");
			var glHeader2 = testObjectCreator.CreateAccGLHeader("GL2");
			var chart = testObjectCreator.CreateAlternateChart("CH1");
			factory.Save();

			var alternateGLAccount = testObjectCreator.CreateAccAlternateGlAccount(chart.PK, "99");
			testObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, attribute: "");
			factory.Save();

			foreach (var registryItem in registriesWithValidateGLAccountHasDissection)
			{
				AssertNoExceptionThrown(() => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid()));
			}

			var attribute = testObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader2.PK, attribute: "");
			factory.Save();

			foreach (var registryItem in registriesWithValidateGLAccountHasDissection)
			{
				AssertExceptionThrown<RegistryValidationException>($"You cannot select this GL Account Number '{glHeader2.AG_AccountNum}' as it is mapped to an Alternate Account linked to multiple Parent Accounts.", () => registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader2.PK.ToGuid()));
			}
		}

		public void TestEnableFinalisedComplianceDocumentToBeSpecialVoided()
		{
			AssertEquals("EnableFinalisedComplianceDocumentToBeSpecialVoided", ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.Name);
			AssertEquals(Categories.Accounting_GovernmentComplianceInvoiceDocument, ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.Category);
			AssertEquals("Allow finalized compliance document records to be special voided", ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.Caption);
			AssertEquals(@"This registry is only relevant to those login companies where the Compliance Document Module has been enabled.

By default, 'Finalized' compliance document records are not allowed to be voided.
When this registry is set to 'Yes', users will be allowed to void such compliance records via 'Special Voiding'.

Note: No changes will be made to the finalized Compliance Report that contains Compliance Document Records have been Special Voiding.", ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.Options);
			AssertEquals(false, ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.DefaultValue);

			var oldValue = ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.DefaultValue;
			var expectedLogMessage = "Registry value changed from [False] to [True].";
			var newValue = true;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided, oldValue, newValue);
			var logReference = ItemSet.EnableFinalisedComplianceDocumentToBeSpecialVoided.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		public void TestGLAccountSelectionAndEntry()
		{
			Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertGLAccountSelectionAndEntry(true);

			Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertGLAccountSelectionAndEntry(false);

			var factory = new BusinessObjectFactory();
			var chart = factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "TST";
			factory.Save();

			var oldValue = ItemSet.GLAccountSelectionAndEntry.DefaultValue;
			var expectedLogMessage = "Alternate Chart changed from [] to [TST].";
			var newValue = chart.PK;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(ItemSet.GLAccountSelectionAndEntry, oldValue, newValue);
			var logReference = ItemSet.GLAccountSelectionAndEntry.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}

		void AssertGLAccountSelectionAndEntry(bool enableReportingBook)
		{
			AssertEquals("GLAccountSelectionAndEntry", ItemSet.GLAccountSelectionAndEntry.Name);
			AssertEquals(Categories.Accounting_ReportingBooks, ItemSet.GLAccountSelectionAndEntry.Category);
			AssertEquals("GL Account Selection and Entry", ItemSet.GLAccountSelectionAndEntry.Caption);
			AssertEquals(@"By default, this registry value is empty and the system will locate 'GL Account' based on the current logic:

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
And should no match is found, a validation error is shown.", ItemSet.GLAccountSelectionAndEntry.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.GLAccountSelectionAndEntry.Storage);
			AssertEquals(enableReportingBook ? RegistryOptions.IsValueOptional : RegistryOptions.IsHidden, ItemSet.GLAccountSelectionAndEntry.Options);
			AssertEquals(Guid.Empty, ItemSet.GLAccountSelectionAndEntry.DefaultValue);
		}

		public void TestReportingBookAccountingJournalPrintOption()
		{
			AssertEquals("ReportingBookAccountingJournalPrintOption", ItemSet.ReportingBookAccountingJournalPrintOption.Name);
			AssertEquals(Categories.Accounting_ReportingBooks, ItemSet.ReportingBookAccountingJournalPrintOption.Category);
			AssertEquals("Reporting Book Accounting Journal Print Option", ItemSet.ReportingBookAccountingJournalPrintOption.Caption);
			AssertEquals(@"The printing of Reporting Book Accounting Journal is not enabled by default. 

To enable this function, you have to add at least one Reporting Book record and mark it as 'Default'.
Further, for each Reporting Book, you can opt to include the printing of 'Parent Account' and 'Attribute Values', if required as these values are not printed by default.", ItemSet.ReportingBookAccountingJournalPrintOption.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.ReportingBookAccountingJournalPrintOption.Storage);

			ItemSet.RemoveItemFromCacheIfOlderThan("ReportingBookAccountingJournalPrintOption", TimeSpan.MinValue);
			using (Instance.EnableReportingBooksFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, ItemSet.ReportingBookAccountingJournalPrintOption.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("ReportingBookAccountingJournalPrintOption", TimeSpan.MinValue);
			using (Instance.EnableReportingBooksFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsValueOptional, ItemSet.ReportingBookAccountingJournalPrintOption.Options);
			}
		}

		public void TestOnBuildReportingBookAccountingJournalPrintOptionLogReference()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);
			var chart1 = testObjectCreator.CreateAlternateChart("1", "5", false, true);
			var chart2 = testObjectCreator.CreateAlternateChart("2", "5", false, false);
			var chart3 = testObjectCreator.CreateAlternateChart("3", "5", false, false);
			var chart4 = testObjectCreator.CreateAlternateChart("4", "5", false, false);
			Factory.Save();

			var reportingBook1 = testObjectCreator.CreateAccReportingBook("1", chart1.PK, GlbCompany.CurrentCompany.PK);
			var reportingBook2 = testObjectCreator.CreateAccReportingBook("2", chart2.PK, GlbCompany.CurrentCompany.PK);
			var reportingBook3 = testObjectCreator.CreateAccReportingBook("3", chart3.PK, GlbCompany.CurrentCompany.PK);
			var reportingBook4 = testObjectCreator.CreateAccReportingBook("4", chart4.PK, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var reportingBookAccountingJournalPrintOptionCollection1 = new ReportingBookAccountingJournalPrintOptionCollection();
			var reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection1.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook1.PK;
			reportingBookAccountingJournalPrintOption.Default = true;

			reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection1.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook2.PK;
			reportingBookAccountingJournalPrintOption.DisplayAttribute = true;

			reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection1.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook3.PK;
			reportingBookAccountingJournalPrintOption.DisplayParentAccount = true;

			var reportingBookAccountingJournalPrintOptionCollection2 = new ReportingBookAccountingJournalPrintOptionCollection();
			reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection2.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook1.PK;
			reportingBookAccountingJournalPrintOption.Default = true;

			reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection2.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook2.PK;
			reportingBookAccountingJournalPrintOption.DisplayAttribute = false;

			reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection2.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook4.PK;
			reportingBookAccountingJournalPrintOption.DisplayParentAccount = false;

			IRegistryItem regItem = Instance.ReportingBookAccountingJournalPrintOption.Inner;
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(regItem, reportingBookAccountingJournalPrintOptionCollection1, reportingBookAccountingJournalPrintOptionCollection2);
			var result = Instance.ReportingBookAccountingJournalPrintOption.OnBuildLogReference(args);
			AssertEquals(@"Record edited: Reporting Book = 2, Display Parent Account = N, Display Attribute = N, Default = N
Record added: Reporting Book = 4, Display Parent Account = N, Display Attribute = N, Default = N
Record deleted: Reporting Book = 3, Display Parent Account = Y, Display Attribute = N, Default = N
", result);
		}

		public void TestReportingBookAccountingJournalPrintOptionVisibility_WithoutCache()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.ReportingBookAccountingJournalPrintOption.Options);
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(RegistryOptions.IsValueOptional, ItemSet.ReportingBookAccountingJournalPrintOption.Options);
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.ReportingBookAccountingJournalPrintOption.Options);
		}

		public void TestAPIKeyGeneratorStrategyEncryptionKey()
		{
			AssertEquals(nameof(ItemSet.APIKeyGeneratorStrategyEncryptionKey), ItemSet.APIKeyGeneratorStrategyEncryptionKey.Name);
			AssertEquals(String.Empty, ItemSet.APIKeyGeneratorStrategyEncryptionKey.Category);
			AssertEquals(String.Empty, ItemSet.APIKeyGeneratorStrategyEncryptionKey.Caption);
			AssertEquals(String.Empty, ItemSet.APIKeyGeneratorStrategyEncryptionKey.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.APIKeyGeneratorStrategyEncryptionKey.Storage);
			AssertEquals(RegistryOptions.IsHidden | RegistryOptions.IsReadOnly, ItemSet.APIKeyGeneratorStrategyEncryptionKey.Options);
			AssertEquals("MPRc7lZ6YOOATOVS8iB6s6zWgi0jc8IrZOvxVxp9nDrS+vnW2R5u95/tdh+3W8giU/Fw6x4QMZZdtAcrjuIitxMFZmmx/2vEHvA6jG3mJmzl5zvJjZBAqoSaD2m1onBw6G9DHGpZou9ilggoPz7Dsq6GH0HJKMtqrwLB2xqVzRg=",
				ItemSet.APIKeyGeneratorStrategyEncryptionKey.DefaultValue);
		}

		public void TestSAFTGroupingCategoryForSAFTv1_10()
		{
			var item = ItemSet.SAFTGroupingCategory;
			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(item
				, x => x.SAFTGroupingCategory
				, ("RF-1167", "RF-1167")
				, (string.Empty, string.Empty)
				, "SAF-T Grouping Category");

			AssertEquals("Name", "SAFTGroupingCategory", item.Name);
			AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, item.Category);
			AssertEquals("Caption", "SAF-T Grouping Category", item.Caption);
			AssertEquals("Hint", @"The SAF-T grouping category code is exported in the SAF-T file as a part of the account section that contains the general ledger accounts.
It is used by the tax authorities to interpret and categorize the chart of accounts used in the SAF-T file and is therefore mandatory to include in the SAF-T report.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden | RegistryOptions.CacheExpensiveDefaultValue, item.Options);
		}

		public void TestSAFTGroupingCategoryForSAFTv1_30()
		{
			var mockFeatureManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			using (ObjectFactory.Substitute(mockFeatureManager.Object))
			{
				var item = ItemSet.SAFTGroupingCategory;
				AssertEquals("Name", "SAFTGroupingCategory", item.Name);
				AssertEquals("Category", Categories.Accounting_GovernmentComplianceInvoiceDocument, item.Category);
				AssertEquals("Caption", "SAF-T Grouping Category", item.Caption);
				AssertEquals("Hint", @"The SAF-T grouping category code is exported in the SAF-T file as a part of the account section that contains the general ledger accounts.
It is used by the tax authorities to interpret and categorize the chart of accounts used in the SAF-T file and is therefore mandatory to include in the SAF-T report.", item.Hint);
				AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
				AssertEquals("Options", RegistryOptions.Default | RegistryOptions.CacheExpensiveDefaultValue, item.Options);
			}
		}

		public void TestD365Credentials()
		{
			AssertEquals("Name", "D365Credentials", ItemSet.D365Credentials.Name);
			AssertEquals("Category", Categories.Accounting_ExternalAccountingSystemIntegration, ItemSet.D365Credentials.Category);
			AssertEquals("Caption", "D365 Credentials", ItemSet.D365Credentials.Caption);

			var expectedRegistryText = @"This registry setting allows seamless authentication between CargoWise and D365 F&O. 
Essential authentication details are configured as part of this setting, which help establish secure access between CargoWise and D365. 
This registry setting is only visible when the feature switch for enabling CW D365 Integration is turned on in the 'Feature Control' module, ensuring that authentication settings are managed efficiently and only when required.";
			AssertEquals("Registry Text", expectedRegistryText, ItemSet.D365Credentials.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.D365Credentials.Storage);
			AssertEquals(string.Empty, ItemSet.D365Credentials.DefaultValue.ClientID);
			AssertEquals(string.Empty, ItemSet.D365Credentials.DefaultValue.ClientSecret);
			AssertEquals(string.Empty, ItemSet.D365Credentials.DefaultValue.TenantID);

			var mockFeatureManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));

			ItemSet.RemoveItemFromCacheIfOlderThan("D365Credentials", TimeSpan.MinValue);
			using (ObjectFactory.Substitute(mockFeatureManager.Object))
			{
				AssertEquals("Options", RegistryOptions.Default, ItemSet.D365Credentials.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("D365Credentials", TimeSpan.MinValue);
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));
			using (ObjectFactory.Substitute(mockFeatureManager.Object))
			{
				AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.D365Credentials.Options);
			}
		}

		public void TestD365WebserviceURL()
		{
			AssertEquals("Name", "D365WebserviceURL", ItemSet.D365WebserviceURL.Name);
			AssertEquals("Category", Categories.Accounting_ExternalAccountingSystemIntegration, ItemSet.D365WebserviceURL.Category);
			AssertEquals("Caption", "D365 Web Service URL", ItemSet.D365WebserviceURL.Caption);

			var expectedRegistryText = @"This registry is to define the web service URL endpoint I should use for my D365 FO Integration.";

			AssertEquals("Registry Text", expectedRegistryText, ItemSet.D365WebserviceURL.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.D365WebserviceURL.Storage);
			AssertEquals(string.Empty, ItemSet.D365WebserviceURL.DefaultValue);

			var mockFeatureManager = new Mock<IFeatureControlManager>();
			var mockIFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			ItemSet.RemoveItemFromCacheIfOlderThan("D365WebserviceURL", TimeSpan.MinValue);

			using (ObjectFactory.Substitute(mockFeatureManager.Object))
			{
				AssertEquals("Options", RegistryOptions.Default, ItemSet.D365WebserviceURL.Options);
			}

			ItemSet.RemoveItemFromCacheIfOlderThan("D365WebserviceURL", TimeSpan.MinValue);
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CWD365IntegrationFeature, CancellationToken.None)).Returns(Task.FromResult((IFeatureData)null));
			
			using (ObjectFactory.Substitute(mockFeatureManager.Object))
			{
				AssertEquals("Options", RegistryOptions.IsHidden, ItemSet.D365WebserviceURL.Options);
			}		
		}

		public void TestEInvoicingReversalCodes()
		{
			var item = ItemSet.EInvoicingReversalCodes;
			AssertEquals("EInvoicingReversalCodes", item.Name);
			AssertEquals(Categories.Accounting_EReportingAndEInvoicingConfigurations, item.Category);
			AssertEquals("E-Invoicing Reversal Codes (CargoWise Support Only)", item.Caption);
			AssertEquals("This registry is used to configure the Reversal Codes according to the tax authorities of each country within the E-Invoicing module", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport | RegistryOptions.CacheExpensiveDefaultValue, item.Options);

			var emptyCodeDescriptionPairList = new ReadOnlyCodeDescriptionPairList();
			AssertEquals(emptyCodeDescriptionPairList, item.Value);
			AssertEquals(emptyCodeDescriptionPairList, item.DefaultValue);

			var mockOne = new CodeDescriptionPairList();
			mockOne.AddPair((NoResString)"INV", (NoResString)"Some temp string");
			var displayValOne = "INV - Some temp string";
			var mockTwo = new CodeDescriptionPairList();
			mockTwo.AddPair((NoResString)"MSC", (NoResString)"Some other temp string");
			mockTwo.AddPair((NoResString)"02", (NoResString)"Some third temp string");
			var displayValTwo = "MSC - Some other temp string; 02 - Some third temp string";
			CountrySpecificRegistryDefaultValueTestHelper.AssertCountrySpecificRegistryItemDefaultValues(
				item
				, x => x.EInvoicingReversalCodes
				, (displayValOne, mockOne)
				, (displayValTwo, mockTwo)
				, "E-Invoicing Reversal Codes (CargoWise Support Only)"
				, startingCallCount: 0);
		}
	}
}
