using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredential))]
	sealed class GlbCompanyCredentialTest : GlbExternalPasswordBase_TWTest<GlbCompanyCredential>
	{
		protected override string ValidMailBox => "CBK0123";
		protected override string ValidPasswordType => PasswordTypesList.Codes.TVF;

		public void TestLookupsType()
		{
			var password = Factory.New<GlbCompanyCredential>();
			AssertType<GlbCompanyCredentialLookups>(password.Lookups);
		}

		public void TestValidationType()
		{
			var password = Factory.New<GlbCompanyCredential>();
			AssertType<GlbCompanyCredentialValidation>(password.Validation);
		}

		public void TestIsEmpty()
		{
			var password = Factory.New<GlbCompanyCredential>();
			Assert(password.IsEmpty);

			password.GP_MailBoxID = "123";
			Assert(!password.IsEmpty);
			password.GP_MailBoxID = "";

			password.CurrentDecryptedPassword = "123";
			Assert(!password.IsEmpty);
			password.CurrentDecryptedPassword = "";

			password.GP_UserID = "123";
			Assert(!password.IsEmpty);
			password.GP_UserID = "";

			password.CurrentDecryptedCertificatePassphrase = "123";
			Assert(!password.IsEmpty);
			password.CurrentDecryptedCertificatePassphrase = "";

			password.GP_Certificate = ZBlob.FromUTF8("123");
			Assert(!password.IsEmpty);
		}

		public new void TestGP_MailBoxID()
		{
			var password = Factory.NewWithValidTestData<GlbCompanyCredential>();
			AssertEquals(35, password.GP_MailBoxIDInfo.MaxLength);
			AssertExceptionThrown<MaxLengthExceededException>(() => password.GP_MailBoxID = new ZString('X', 36));
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}
	}
}
