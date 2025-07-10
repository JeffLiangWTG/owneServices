using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTaxConfigurationExtensionHelperTest : TransactionedTestCase
	{
		public void TestIsAPWithholdTaxEnabled_CompanyLevel()
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());

			var testCases = new[]
			{
				new { HasSPRTaxConfigured = false, ExpectedResult = false },
				new { HasSPRTaxConfigured = true, ExpectedResult = true },
			};

			var taxConfigured = false;
			foreach (var testCase in testCases)
			{
				if (testCase.HasSPRTaxConfigured && !taxConfigured)
				{
					taxConfigured = true;
					taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);
				}
				AssertEquals(testCase.ExpectedResult, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			}

			var taxConfig = GlbCompany.CurrentCompany.AccTaxConfigurations[0];
			taxConfig.ETC_IsActive = false;
			taxConfig.Factory.Save();
			AssertEquals(false, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
		}

		public void TestIsAPWithholdTaxEnabled_BranchLevel()
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());

			var testCases = new[]
			{
				new { HasSPRTaxConfigured = false, ExpectedResult = false },
				new { HasSPRTaxConfigured = true, ExpectedResult = true },
			};

			var taxConfigured = false;
			foreach (var testCase in testCases)
			{
				if (testCase.HasSPRTaxConfigured && !taxConfigured)
				{
					taxConfigured = true;
					taxTestHelper.ConfigureTaxFrameworkAtBranchLevel(GlbBranch.CurrentBranch, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);
				}
				AssertEquals(testCase.ExpectedResult, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
			}
		}

		public void TestIsAPWithholdTaxEnabled_NonSPR()
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.SalesTax.Code);
			AssertEquals(false, GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled());
		}
	}
}
