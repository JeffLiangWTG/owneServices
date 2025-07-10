using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyLicensingCredential))]
	sealed class GlbCompanyLicensingCredentialTest : GlbExternalPasswordBase_TWTest<GlbCompanyLicensingCredential>
	{
		public new void TestGP_MailBoxID()
		{
			var password = Factory.NewWithValidTestData<GlbCompanyLicensingCredential>();
			AssertEquals(35, password.GP_MailBoxIDInfo.MaxLength);
			AssertExceptionThrown<MaxLengthExceededException>(() => password.GP_MailBoxID = new ZString('X', 36));
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		protected override string ValidMailBox => "CBK0123";
		protected override string ValidPasswordType => PasswordTypesList.Codes.NXM;
	}
}
