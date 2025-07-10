using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.ServiceTasks.Testing;

sealed class PLServiceTaskHelperTest : TestCaseWithFactory
{
	public void TestCheckCertificate()
	{
		const string message = "There is no Token Credential / Certificate configured in Poland.";

		var company = Factory.New<GlbCompany>();
		company.GC_Code = "DPL";
		company.GC_RN_NKCountryCode = CountryCodes.Poland;

		CombineAssertions(() =>
		{
			AssertEquals("No certificate", message, PLServiceTaskHelper.CheckCertificate());

			var certificate = Factory.New<GlbExternalPassword>();
			certificate.GP_GC = company.PK;
			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			certificate.GP_PasswordType = PasswordTypesList.Codes.PLB;
			certificate.GP_UserID = certificate.PK.ToString();
			Factory.Save();
			CertificateRequirementChecker.ResetForTesting();
			AssertNotEquals(message, PLServiceTaskHelper.CheckCertificate());

			certificate.GP_PasswordType = PasswordTypesList.Codes.IEM;
			Factory.Save();
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(message, PLServiceTaskHelper.CheckCertificate());

			certificate.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			certificate.GP_PasswordType = PasswordTypesList.Codes.PLB;
			Factory.Save();
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(message, PLServiceTaskHelper.CheckCertificate());
		});
	}
}
