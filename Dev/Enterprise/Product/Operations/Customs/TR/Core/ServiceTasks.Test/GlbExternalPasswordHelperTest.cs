using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.ServiceTasks.Testing
{
	public class GlbExternalPasswordHelperTest : TestCaseWithFactory
	{
		public void TestAnyStaffHasTRKCertificate()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "AB2";
			newStaff.GS_RN_NKCountryCode = CountryCodes.Austria;
			newStaff.GS_LoginName = "Test";
			var wrapper = TRGlbStaffWrapper.Get(newStaff);
			Factory.Save();

			CertificateRequirementChecker.ResetForTesting();
			AssertEquals("There is no Certificate configured in Turkey.", GlbExternalPasswordHelper.CheckAnyStaffHasCertificate());

			SetupGlbExternalPassword_TRK(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, GlbExternalPasswordHelper.CheckAnyStaffHasCertificate());
		}

		public static void SetupGlbExternalPassword_TRK(BusinessObjectFactory factory)
		{
			var newStaff = factory.New<GlbStaff>();
			newStaff.GS_Code = "ABC";
			newStaff.GS_RN_NKCountryCode = CountryCodes.Turkey;
			newStaff.GS_LoginName = "Test2";
			var wrapper = TRGlbStaffWrapper.Get(newStaff);
			var password = wrapper.TRBPassword;
			password.GP_PasswordType = PasswordTypesList.Codes.TRK;
			password.GP_UserID = "1234";
			password.CurrentDecryptedPassword = "xxx";
			password.GP_CertificateAuthority = "TÜBİTAK";
			password.TR_Chipset = "ABC";
			password.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			factory.Save();
		}
	}
}
