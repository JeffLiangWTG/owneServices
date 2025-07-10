using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccBankAccountCollection))]
	sealed class AccBankAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			return new AccBankAccountCollection(Factory, branch, new ZQuery());
		}

		public void TestAccBankAccountCollectionForCompany()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TS1";
			company1.GC_StartDate = Env.Time.CurrentLocalDate;
			company1.GC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			company1.GC_RX_NKLocalCurrency = Env.CurrentCompany.LocalCurrency.Code;

			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "TB1";
			branch1.GB_GC = company1.PK;

			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "TB2";
			branch2.GB_GC = company1.PK;

			BusinessObject[] headers = Factory.Load(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "TTL"));

			AccBankAccount account1 = Factory.New<AccBankAccount>();
			account1.AB_Code = "111";
			account1.AB_AccountNum = "111";
			account1.AB_GC = company1.PK;
			account1.AB_GB = branch1.PK;
			account1.AB_AG = headers[0].PK;
			account1.AB_RX_NKAccountCurrency = Env.CurrentCompany.LocalCurrency.Code;

			AccBankAccount account2 = Factory.New<AccBankAccount>();
			account2.AB_Code = "222";
			account2.AB_AccountNum = "222";
			account2.AB_GC = company1.PK;
			account2.AB_GB = branch2.PK;
			account2.AB_AG = headers[1].PK;
			account2.AB_RX_NKAccountCurrency = Env.CurrentCompany.LocalCurrency.Code;

			Factory.Save();

			AccBankAccountCollection collection = new AccBankAccountCollection(Factory, company1);
			collection.Load();

			AssertEquals("Should be 2 bank accounts in collection", 2, collection.Count);

			collection = new AccBankAccountCollection(Factory, company1, new ZQuery(AccBankAccountSchema.AB_Code, account2.AB_Code));
			collection.Load();

			AssertEquals("Should be 1 bank accounts in collection", 1, collection.Count);
			AssertEquals("Bank Account Code should be filtered", account2.AB_Code, collection[0].AB_Code);

			collection = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany, new ZQuery(AccBankAccountSchema.AB_Code, account2.AB_Code));
			collection.Load();

			AssertEquals("Should be no bank accounts in collection", 0, collection.Count);
		}
	}
}
