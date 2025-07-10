using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PhoneNumberPropertyHelperTest : TestCaseWithFactory
	{
		ZString DefaultCountryCodeForTest => "XX";

		public void TestGetPhoneNumberInLocalIfLoggedInSameCountryDoesNotThrowNullReferenceException()
		{
			var branchWithNoHomePort = Factory.NewWithValidTestData<GlbBranch>();
			branchWithNoHomePort.GB_RL_NKHomePort = ZString.Empty;
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			branchWithNoHomePort.GB_GC = newCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, branchWithNoHomePort.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			{
				AssertNull("Precondition", GlbBranch.CurrentBranch.Country);

				var helper = new PhoneNumberPropertyHelper(() => DefaultCountryCodeForTest);
				var dummyPhoneNumberInfo = Factory.New<DummyBusinessObject>().Z0_NVarCharInfo;

				AssertNoExceptionThrown(() => helper.GetPhoneNumberInLocalIfLoggedInSameCountry(dummyPhoneNumberInfo));
				AssertEquals(ZString.Empty, helper.GetPhoneNumberInLocalIfLoggedInSameCountry(dummyPhoneNumberInfo));
			}
		}
	}
}
