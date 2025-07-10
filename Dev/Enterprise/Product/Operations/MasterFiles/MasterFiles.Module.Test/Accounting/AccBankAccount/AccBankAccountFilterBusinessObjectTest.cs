using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccBankAccountFilterBusinessObject))]
	public class AccBankAccountFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Text Filters

		public void TestBankCodeFilter()
		{
			AccBankAccount bankCode1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bankCode2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankCode1.AB_Code = "Indescr";
			bankCode2.AB_Code = "SomeDescr";

			Factory.Save();

			AccBankAccountFilterBusinessObject filter = new AccBankAccountFilterBusinessObject();
			((ModuleTextFilter)filter["Bank Code"]).Property = "Indescr";
			((ModuleTextFilter)filter["Bank Code"]).IsActive = true;

			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory, filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(bankCode1, bankAccounts);
			AssertCollectionNotContains(bankCode2, bankAccounts);
		}

		public void TestAccountNumberFilter()
		{
			AccBankAccount accountNumber1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount accountNumber2 = Factory.NewWithValidTestData<AccBankAccount>();
			accountNumber1.AB_AccountNum = "Indescr";
			accountNumber2.AB_AccountNum = "SomeDescr";

			Factory.Save();

			AccBankAccountFilterBusinessObject filter = new AccBankAccountFilterBusinessObject();
			((ModuleTextFilter)filter["Account Number"]).Property = "Indescr";
			((ModuleTextFilter)filter["Account Number"]).IsActive = true;

			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory, filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accountNumber1, bankAccounts);
			AssertCollectionNotContains(accountNumber2, bankAccounts);
		}

		public void TestDescriptionFilter()
		{
			AccBankAccount description1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount description2 = Factory.NewWithValidTestData<AccBankAccount>();
			description1.AB_Desc = "Indescr";
			description2.AB_Desc = "SomeDescr";

			Factory.Save();

			AccBankAccountFilterBusinessObject filter = new AccBankAccountFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Indescr";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory, filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(description1, bankAccounts);
			AssertCollectionNotContains(description2, bankAccounts);
		}

		public void TestBankNameFilter()
		{
			AccBankAccount bankName1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bankName2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankName1.AB_BankName = "Indescr";
			bankName2.AB_BankName = "SomeDescr";

			Factory.Save();

			AccBankAccountFilterBusinessObject filter = new AccBankAccountFilterBusinessObject();
			((ModuleTextFilter)filter["Bank Name"]).Property = "Indescr";
			((ModuleTextFilter)filter["Bank Name"]).IsActive = true;

			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory, filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(bankName1, bankAccounts);
			AssertCollectionNotContains(bankName2, bankAccounts);
		}

		#endregion

		#region RelatedItemFiltersTests

		public void TestBranchFilter()
		{
			GlbBranch glbBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch glbBranch2 = Factory.NewWithValidTestData<GlbBranch>();

			AccBankAccount accBankBranch0 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankBranch0.AB_GB = glbBranch1.PK;

			AccBankAccount accBankBranch1 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankBranch1.AB_GB = glbBranch2.PK;

			AccBankAccount accBankBranch2 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankBranch2.AB_GB = glbBranch2.PK;

			Factory.Save();

			AccBankAccountFilterBusinessObject glbBranch1Filter = new AccBankAccountFilterBusinessObject();
			((ModuleGuidFilter)glbBranch1Filter["Branch"]).Property = glbBranch1.PK;
			((ModuleGuidFilter)glbBranch1Filter["Branch"]).IsActive = true;

			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory, glbBranch1Filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accBankBranch0, bankAccounts);
			AssertCollectionNotContains(accBankBranch1, bankAccounts);
			AssertCollectionNotContains(accBankBranch2, bankAccounts);

			AccBankAccountFilterBusinessObject glbBranch2Filter = new AccBankAccountFilterBusinessObject();
			((ModuleGuidFilter)glbBranch2Filter["Branch"]).Property = glbBranch2.PK;
			((ModuleGuidFilter)glbBranch2Filter["Branch"]).IsActive = true;

			bankAccounts = new AccBankAccountCollection(Factory, glbBranch2Filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accBankBranch1, bankAccounts);
			AssertCollectionContains(accBankBranch2, bankAccounts);
			AssertCollectionNotContains(accBankBranch0, bankAccounts);

			AccBankAccountFilterBusinessObject emptyGlbBranchFilter = new AccBankAccountFilterBusinessObject();
			((ModuleGuidFilter)emptyGlbBranchFilter["Branch"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyGlbBranchFilter["Branch"]).IsActive = false;

			bankAccounts = new AccBankAccountCollection(Factory, emptyGlbBranchFilter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accBankBranch0, bankAccounts);
			AssertCollectionContains(accBankBranch1, bankAccounts);
			AssertCollectionContains(accBankBranch2, bankAccounts);
		}

		public void TestCurrencyFilter()
		{
			RefCurrency glbCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency glbCurrency2 = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount accBankCurrency0 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankCurrency0.AB_RX_NKAccountCurrency = glbCurrency1.RX_Code;

			AccBankAccount accBankCurrency1 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankCurrency1.AB_RX_NKAccountCurrency = glbCurrency2.RX_Code;

			AccBankAccount accBankCurrency2 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankCurrency2.AB_RX_NKAccountCurrency = glbCurrency2.RX_Code;

			Factory.Save();

			AccBankAccountFilterBusinessObject glbCurrency1Filter = new AccBankAccountFilterBusinessObject();
			((ModuleNkFilter)glbCurrency1Filter["Currency"]).Property = glbCurrency1.RX_Code;
			((ModuleNkFilter)glbCurrency1Filter["Currency"]).IsActive = true;

			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory, glbCurrency1Filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accBankCurrency0, bankAccounts);
			AssertCollectionNotContains(accBankCurrency1, bankAccounts);
			AssertCollectionNotContains(accBankCurrency2, bankAccounts);

			AccBankAccountFilterBusinessObject glbCurrency2Filter = new AccBankAccountFilterBusinessObject();
			((ModuleNkFilter)glbCurrency2Filter["Currency"]).Property = glbCurrency2.RX_Code;
			((ModuleNkFilter)glbCurrency2Filter["Currency"]).IsActive = true;

			bankAccounts = new AccBankAccountCollection(Factory, glbCurrency2Filter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accBankCurrency1, bankAccounts);
			AssertCollectionContains(accBankCurrency2, bankAccounts);
			AssertCollectionNotContains(accBankCurrency0, bankAccounts);

			AccBankAccountFilterBusinessObject emptyGlbCurrencyFilter = new AccBankAccountFilterBusinessObject();
			((ModuleNkFilter)emptyGlbCurrencyFilter["Currency"]).Property = ZString.Empty;
			((ModuleNkFilter)emptyGlbCurrencyFilter["Currency"]).IsActive = false;

			bankAccounts = new AccBankAccountCollection(Factory, emptyGlbCurrencyFilter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accBankCurrency0, bankAccounts);
			AssertCollectionContains(accBankCurrency1, bankAccounts);
			AssertCollectionContains(accBankCurrency2, bankAccounts);
		}

		#endregion

		#region HiddentFilterTest

		public void TestCompanyFilter()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			AccBankAccount accBankBranch0 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankBranch0.AB_GC = GlbCompany.CurrentCompany.PK;

			AccBankAccount accBankBranch1 = Factory.NewWithValidTestData<AccBankAccount>();
			accBankBranch1.AB_GC = company.PK;

			Factory.Save();

			AccBankAccountFilterBusinessObject glbCompanyFilter = new AccBankAccountFilterBusinessObject();
			AccBankAccountCollection bankAccounts = new AccBankAccountCollection(Factory, glbCompanyFilter.Filter);
			bankAccounts.Load();

			AssertCollectionContains(accBankBranch0, bankAccounts);
			AssertCollectionNotContains(accBankBranch1, bankAccounts);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccBankAccountFilterBusinessObject();
		}

		#endregion
	}
}
