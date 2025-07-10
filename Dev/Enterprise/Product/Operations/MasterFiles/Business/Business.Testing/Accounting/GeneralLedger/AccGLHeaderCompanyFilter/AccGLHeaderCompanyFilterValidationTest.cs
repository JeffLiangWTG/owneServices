using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccGLHeaderCompanyFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDuplicateCompanyFilter()
		{
			var header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_IsGlobal = false;
			var filter1 = header.CompanyFilters.AddNew();
			filter1.ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertNoErrors(filter1.ACF_GC_CompanyInfo);
			var filter2 = header.CompanyFilters.AddNew();
			filter2.ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertHasError(filter2.ACF_GC_CompanyInfo, "This company filter is already entered");
		}

		public void TestCompanyFiltersMissing()
		{
			var header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_IsGlobal = true;
			var bankAccountCompany = Factory.NewWithValidTestData<GlbCompany>();
			bankAccountCompany.GC_Code = "BAC";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AG = header.PK;
			bankAccount.AB_GC = bankAccountCompany.PK;
			var chargeCodeCompany = Factory.NewWithValidTestData<GlbCompany>();
			chargeCodeCompany.GC_Code = "CHC";
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AG_AccrualAccount = header.PK;
			chargeCode.AC_GC = chargeCodeCompany.PK;
			Factory.Save();

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			header.AG_IsGlobal = false;
			var filter = header.CompanyFilters.AddNew();
			filter.ACF_GC_Company = newCompany.PK;
			AssertHasError(filter.ACF_GC_CompanyInfo, "This GL Account has been used in Charge Codes configuration and/or Bank Accounts configuration in the following companies: BAC, CHC");
		}
	}
}
