using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using static Enterprise.MasterFiles.Business.AccTaxOverrideGroup;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaxFrameworkAccTaxOverrideGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCommentTypeChargeCodesShouldNotBePresent()
		{
			var taxFrameworkAccTaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxFrameworkAccTaxOverrideGroup.SetContext(BusinessContext.TaxFramework);
			var chargeCode1 = taxFrameworkAccTaxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration.AddNew();
			var chargeCode2 = taxFrameworkAccTaxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration.AddNew();
			var chargeCode3 = taxFrameworkAccTaxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration.AddNew();
			var chargeCode4 = taxFrameworkAccTaxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration.AddNew();

			chargeCode1.AC_Code = "CMT111";
			chargeCode1.AC_ChargeType = Core.Constants.ChargeType.Comment;

			chargeCode2.AC_Code = "REV123";
			chargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			chargeCode3.AC_Code = "CMT444";
			chargeCode3.AC_ChargeType = Core.Constants.ChargeType.Comment;

			chargeCode4.AC_Code = "REV666";
			chargeCode4.AC_ChargeType = Core.Constants.ChargeType.Comment;

			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				AssertHasRowError(chargeCode1, "Comment type Charge Codes are not permitted on Tax Configuration Override Groups. Please detach any Comment Charge Codes from this Override Group.");
				AssertNoRowErrors(chargeCode2);
				AssertHasRowError(chargeCode3, "Comment type Charge Codes are not permitted on Tax Configuration Override Groups. Please detach any Comment Charge Codes from this Override Group.");
				AssertHasRowError(chargeCode4, "Comment type Charge Codes are not permitted on Tax Configuration Override Groups. Please detach any Comment Charge Codes from this Override Group.");
			});
		}

		public void TestValidateRecoveryChargeCodeNotAllowedForARLedgerTypeTaxOverrideGroup()
		{
			var taxSystem1 = new AccountingTestObjectCreator(Factory).CreateTaxSystem("TAXSYS1", includeInInvoiceTotal: false);
			var mockITaxFrameworkConfigurationHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.WithGetTaxSystems(taxSystem1);

			var taxConfigList = new AccTaxConfigurationCollection(Factory);

			var taxConfiguration = taxConfigList.AddNew();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_RN_NKCountry = taxSystem1.Country;
			taxConfiguration.ETC_TaxSystemCode = taxSystem1.Code;
			mockITaxFrameworkConfigurationHelper.WithGetCompanyTaxConfigurations(taxConfigList, Factory, GlbCompany.CurrentCompany,new CargoWise.EntityFramework.ZQuery());

			var taxOverrideGroup = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup.SetContext(BusinessContext.TaxFramework);

			var taxOverrideGroupTaxConfigurationPivot = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;

			var nonRecTypeChargeCode = Factory.New<AccChargeCode>();
			nonRecTypeChargeCode.AC_Code = "NONRECOV";
			nonRecTypeChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			taxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration.Add(nonRecTypeChargeCode);

			var recTypeChargeCode = Factory.New<AccChargeCode>();
			recTypeChargeCode.AC_Code = "TAXRECOV";
			recTypeChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			taxOverrideGroup.ChargeCodesLinkedToTaxFrameworkConfiguration.Add(recTypeChargeCode);

			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			taxConfiguration.ETC_Ledger = LedgerTypes.AccountsPayable;
			AssertForDifferentChargeCodes(expectErrorForRecoveryTypeChargeCode: false);

			taxConfiguration.ETC_Ledger = LedgerTypes.AccountsReceivable;
			AssertForDifferentChargeCodes(expectErrorForRecoveryTypeChargeCode: false);

			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, recTypeChargeCode.PK.ToGuid());
			taxConfiguration.ETC_Ledger = LedgerTypes.AccountsPayable;
			AssertForDifferentChargeCodes(expectErrorForRecoveryTypeChargeCode: false);

			taxConfiguration.ETC_Ledger = LedgerTypes.AccountsReceivable;
			AssertForDifferentChargeCodes(expectErrorForRecoveryTypeChargeCode: true);

			void AssertForDifferentChargeCodes(bool expectErrorForRecoveryTypeChargeCode)
			{
				taxOverrideGroup.Validation.ValidateAll();
				AssertNoRowErrors(nonRecTypeChargeCode);

				taxOverrideGroup.Validation.ValidateAll();
				if (expectErrorForRecoveryTypeChargeCode)
				{
					AssertHasRowError(recTypeChargeCode, @"You cannot have Revenue Tax Expense Recovery Charge Code attached to a Tax Override Group with AR (Receivables Ledger) Tax Framework Configurations.
Please detach the Revenue Tax Expense Recovery Charge Code before saving.");
				}
				else
				{
					AssertNoRowErrors(recTypeChargeCode);
				}
			}
		}

		public void TestValidateTaxOverrideGroupShouldHaveTaxConfiguration()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_TaxSystemCode = "TAXSYS";

			var taxFrameworkAccTaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxFrameworkAccTaxOverrideGroup.SetContext(BusinessContext.TaxFramework);
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertHasRowError(taxFrameworkAccTaxOverrideGroup, "Please add at least one row in the Tax Configuration grid.");

			var taxOverrideGroupTaxConfigurationPivot = taxFrameworkAccTaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxFrameworkAccTaxOverrideGroup);
		}

		public void TestValidateDuplicateTaxConfigurations()
		{
			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			var taxConfiguration2 = Factory.NewWithValidTestData<AccTaxConfiguration>();

			taxConfiguration1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration2.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			taxConfiguration1.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration2.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			taxConfiguration1.ETC_TaxSystemCode = "TAXSYS1";
			taxConfiguration2.ETC_TaxSystemCode = "TAXSYS2";

			var taxFrameworkAccTaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxFrameworkAccTaxOverrideGroup.SetContext(BusinessContext.TaxFramework);

			var taxOverrideGroupTaxConfigurationPivot1 = taxFrameworkAccTaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverrideGroupTaxConfigurationPivot1);

			var taxOverrideGroupTaxConfigurationPivot2 = taxFrameworkAccTaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertHasRowError(taxOverrideGroupTaxConfigurationPivot1, "You cannot have identical Tax Configurations.");
			AssertHasRowError(taxOverrideGroupTaxConfigurationPivot2, "You cannot have identical Tax Configurations.");

			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration2.PK;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverrideGroupTaxConfigurationPivot1);
			AssertNoRowErrors(taxOverrideGroupTaxConfigurationPivot2);
		}

		public void TestValidateTaxConfigurationsShouldHaveSameLedger()
		{
			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			var taxConfiguration2 = Factory.NewWithValidTestData<AccTaxConfiguration>();

			taxConfiguration1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration2.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			taxConfiguration1.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration2.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			taxConfiguration1.ETC_TaxSystemCode = "TAXSYS1";
			taxConfiguration2.ETC_TaxSystemCode = "TAXSYS2";

			taxConfiguration1.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfiguration2.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;

			var taxFrameworkAccTaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxFrameworkAccTaxOverrideGroup.SetContext(BusinessContext.TaxFramework);

			var taxOverrideGroupTaxConfigurationPivot1 = taxFrameworkAccTaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverrideGroupTaxConfigurationPivot1);

			var taxOverrideGroupTaxConfigurationPivot2 = taxFrameworkAccTaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration2.PK;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertHasRowError(taxOverrideGroupTaxConfigurationPivot2, "A mix of Tax Configurations with different Ledgers is not permitted.");

			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = ZGuid.Empty;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverrideGroupTaxConfigurationPivot2);

			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration2.PK;
			taxConfiguration2.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxFrameworkAccTaxOverrideGroup.Validation.ValidateAll();
			AssertNoRowErrors(taxOverrideGroupTaxConfigurationPivot2);
		}
	}
}
