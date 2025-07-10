using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCusAccount.Loader))]
	sealed class OrgCusAccountLoaderTest : LoaderTestCase
	{
		public void TestLoadByCodeAndCountry_InvalidOrgPk()
		{
			AssertEquals(0, OrgCusAccount.Loader.LoadByCodeAndCountry(Factory, ZGuid.Invalid, CodeForTest, Core.Constants.CountryCodes.Eritrea).Length);
		}

		public void TestLoadByCodeAndCountry_EmptyCode()
		{
			AssertEquals(0, OrgCusAccount.Loader.LoadByCodeAndCountry(Factory, organisation.PK, ZString.Empty, Core.Constants.CountryCodes.Eritrea).Length);
		}

		public void TestLoadByCodeAndCountry_EmptyCountry()
		{
			AssertEquals(0, OrgCusAccount.Loader.LoadByCodeAndCountry(Factory, organisation.PK, CodeForTest, ZString.Empty).Length);
		}

		public void TestLoadTop1ByCodeAndCountry()
		{
			var account2 = CreateOrgCusAccount();
			var firstAccountByPK = new OrgCusAccount[] { account, account2 }.OrderBy(x => x.PK).First();
			AssertEquals(firstAccountByPK, OrgCusAccount.Loader.LoadTop1ByCodeAndCountry(Factory, organisation.PK, CodeForTest, Core.Constants.CountryCodes.Eritrea));
		}

		public void TestLoadByCodeAndCountry()
		{
			var account2 = CreateOrgCusAccount();
			var orgCusAccounts = OrgCusAccount.Loader.LoadByCodeAndCountry(Factory, organisation.PK, CodeForTest, Core.Constants.CountryCodes.Eritrea);
			CombineAssertions(() =>
			{
				AssertEquals("Two accounts", 2, orgCusAccounts.Length);
				AssertCollectionContains("First Account", account, orgCusAccounts);
				AssertCollectionContains("Second Account", account2, orgCusAccounts);
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new OrgCusAccount.Loader(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			account = CreateOrgCusAccount();
		}
		OrgHeader organisation;
		OrgCusAccount account;

		OrgCusAccount CreateOrgCusAccount()
		{
			var account = Factory.New<OrgCusAccount>();
			account.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			account.CZ_OH = organisation.PK;
			account.CZ_Code = CodeForTest;
			return account;
		}

		const string CodeForTest = "ZZZ";
	}
}
