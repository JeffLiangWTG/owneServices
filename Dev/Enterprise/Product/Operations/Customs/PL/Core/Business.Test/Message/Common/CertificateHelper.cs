using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

public static class CertificateHelper
{
	public static void SetUpTestCertificate()
	{
		var staffPassword = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		staffPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		staffPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
	}
}
