using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class MessageHostedServiceRequirementTest : TestCaseWithFactory
	{
		public void TestCheckUYCompanyHasCertificate_True()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Uruguay;

			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(company).GlbExternalPassword;
			credential.GP_UserID = "ADMIN13";
			credential.GP_CurrentPassword = "9974567890";

			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_PasswordType = PasswordTypesList.Codes.UTB;

			GlbCompany.CurrentCompany.Factory.Save();
			Factory.Save();
			CertificateRequirementChecker.ResetForTesting();

			AssertEquals(ZString.Empty, MessageHostedServiceRequirement.CheckUYCompanyHasCertificate());
		}

		public void TestCheckUYCompanyHasCertificate_False()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Uruguay;

			Factory.Save();

			AssertEquals("There is no Valid Certificate in Uruguay.", MessageHostedServiceRequirement.CheckUYCompanyHasCertificate());
		}
	}
}
