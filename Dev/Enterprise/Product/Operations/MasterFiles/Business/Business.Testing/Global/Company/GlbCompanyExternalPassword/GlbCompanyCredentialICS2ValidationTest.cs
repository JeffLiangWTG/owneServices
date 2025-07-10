using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialICS2Validation))]
	class GlbCompanyCredentialICS2ValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbCompanyCredentialICS2, GlbCompanyCredentialICS2Validation>
	{
		public void TestCheckGP_MailBoxID()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			GlbExternalPassword.GP_MailBoxID = ZString.Empty;
			var info = GlbExternalPassword.GP_MailBoxIDInfo;
			AssertNoErrors(info);
			GlbExternalPassword.GP_Certificate = ValidCertificate;
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ValidPassword;
			GlbExternalPassword.Validation.ValidateGP_MailBoxID();
			var mustEntereMessage = "Please enter a Reporter EORI.";
			var validCodeMessage = "Maximum Length of the EORI is 17 including the Country/Region code prefix.";
			AssertHasError(info, mustEntereMessage);
			AssertHasMessageError(info, validCodeMessage);

			GlbExternalPassword.GP_MailBoxID = "DE";
			AssertNoError(info, mustEntereMessage);
			AssertHasMessageError(info, validCodeMessage);

			GlbExternalPassword.GP_MailBoxID = "DE1";
			AssertNoError(info, mustEntereMessage);
			AssertNoMessageError(info, validCodeMessage);

			GlbExternalPassword.GP_MailBoxID = "DE1234567890123456";
			AssertNoError(info, mustEntereMessage);
			AssertHasMessageError(info, validCodeMessage);

			GlbExternalPassword.GP_MailBoxID = "AU123456789012345";
			AssertNoError(info, mustEntereMessage);
			AssertHasMessageError(info, validCodeMessage);

			GlbExternalPassword.GP_MailBoxID = "123456";
			AssertNoError(info, mustEntereMessage);
			AssertHasMessageError(info, validCodeMessage);

			GlbExternalPassword.GP_MailBoxID = "1234567890123456";
			AssertNoError(info, mustEntereMessage);
			AssertHasMessageError(info, validCodeMessage);
		}

		protected override bool IsCertificateMandatory => false;
	}
}
