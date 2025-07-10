using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgDebtorGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPayToBankAccountsAreCompanySpecific()
		{
			AccBankAccount bankAccountCurrentCompany = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bankAccountNonCurrentCompany = Factory.NewWithValidTestData<AccBankAccount>();

			bankAccountCurrentCompany.AB_GC = GlbCompany.CurrentCompany.PK;
			bankAccountNonCurrentCompany.AB_GC = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			OrgDebtorGroup group = Factory.New<OrgDebtorGroup>();

			Factory.Save();
			AccBankAccountCollection loadedBankAccounts = group.Lookups.PayToBankAccounts;
			loadedBankAccounts.Load();
			AssertEquals("Should only be 1 item in bank accounts lookup", 1, loadedBankAccounts.Count);
		}
	}
}
