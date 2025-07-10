using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CreditOnHoldCheckerTest : TestCaseWithFactory
	{
		OrgCompanyData CompanyData;
		OrgHeader Org;
		OrgMiscServ MiscServData;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
			CompanyData = Org.CompanyData;
			MiscServData = Org.MiscServ;
		}

		[TestDate]
		public void TestGetLastUserSettingCreditOnHold()
		{
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			AssertEquals("Precondition", false, CompanyData.OB_AROnCreditHold);

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData, authorizingUser, 2);

			var actualResult = CreditOnHoldChecker.GetLastUserSettingLocalCreditOnHold(CompanyData.Logs);

			AssertEquals(authorizingUser.GS_LoginName, actualResult.GS_LoginName);
			AssertEquals(authorizingUser.GS_FullName, actualResult.GS_FullName);
		}

		[TestDate]
		public void TestGetLastUserSettingGlobalCreditOnHold()
		{
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "ABC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			Factory.Save();

			AssertEquals("Precondition", false, CompanyData.Header.MiscServ.OM_ARGlobalOnCreditHold);

			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(CompanyData.Header.MiscServ, authorizingUser, 1, true);

			var actualResult = CreditOnHoldChecker.GetLastUserSettingGlobalCreditHold(MiscServData.Logs);
			AssertEquals(authorizingUser.GS_LoginName, actualResult.GS_LoginName);
			AssertEquals(authorizingUser.GS_FullName, actualResult.GS_FullName);
		}
	}
}
