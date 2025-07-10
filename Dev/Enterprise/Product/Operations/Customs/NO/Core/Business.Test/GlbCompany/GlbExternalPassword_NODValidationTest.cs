using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(GlbExternalPassword_NODValidation))]
sealed class GlbExternalPassword_NODValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbExternalPassword_NOD, GlbExternalPassword_NODValidation>
{
	[TestDate(2023, 08, 10)]
	public void TestValidateCertificateExpiryDate()
	{
		AssertEquals("PRE-CONDITION", GlbExternalPassword.GP_ExpiryDate, ZDate.Empty);
		CombineAssertions(() =>
		{
			SetValidCertificate();
			var errorMessage = "The certificate has expired.";
			GlbExternalPassword.GP_ExpiryDate = new ZDateTime(2023, 12, 31, 1, 2, 3);
			AssertNoError(GlbExternalPassword.GP_ExpiryDateInfo, errorMessage);

			GlbExternalPassword.GP_ExpiryDate = new ZDateTime(2022, 12, 31, 1, 2, 3);
			AssertHasError(GlbExternalPassword.GP_ExpiryDateInfo, errorMessage);
		});
	}

	void SetValidCertificate()
	{
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
	}
}
