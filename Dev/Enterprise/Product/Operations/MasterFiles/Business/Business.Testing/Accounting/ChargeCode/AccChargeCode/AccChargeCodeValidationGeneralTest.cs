using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccChargeCodeValidationGeneralTest : TestCaseWithFactory
	{
		public void TestCheckAC_AX_TaxOverrideGroup()
		{
			var expectedError = "This is an invalid selection. Please select a valid ‘Tax Override Group’ from the list.";

			var company = GlbCompany.CurrentCompany;
			company.SetCountry(Core.Constants.CountryCodes.Argentina);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_GC = company.PK;
			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration(company, ledger);
			var taxOverrideGroupWithTaxConfiguration = AccountingTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration);

			var taxOverrideGroupWithOutTaxConfiguration = AccountingTestObjectCreator.CreateTaxOverrideGroup(company, null);

			Factory.Save();

			chargeCode.AC_AX_TaxOverrideGroup = taxOverrideGroupWithTaxConfiguration.PK;
			AssertHasError(chargeCode.AC_AX_TaxOverrideGroupInfo, expectedError);

			chargeCode.AC_AX_TaxOverrideGroup = taxOverrideGroupWithOutTaxConfiguration.PK;
			AssertNoErrors(chargeCode.AC_AX_TaxOverrideGroupInfo);
		}

		protected AccountingTestObjectCreator AccountingTestObjectCreator
		{
			get
			{
				return accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
			}
		}
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
