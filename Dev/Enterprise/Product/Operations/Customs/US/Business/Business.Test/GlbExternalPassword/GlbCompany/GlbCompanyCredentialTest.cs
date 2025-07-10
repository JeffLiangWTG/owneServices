using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredential))]
	sealed class GlbCompanyCredentialTest : GlbExternalPasswordTest<GlbCompanyCredential>
	{
		protected override GlbCompanyCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.New<GlbCompanyCredential>();
		}

		public void TestSetDefaultValues()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(PasswordTypesList.Codes.EBD, credential.GP_PasswordType);
			AssertEquals(ZGuid.Empty, credential.GP_GS);
			AssertNotEquals(ZGuid.Empty, credential.GP_GC);
			AssertEquals(PasswordStatusList.Codes.Valid, credential.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Valid, credential.PasswordStatus);
		}

		public void TestCurrentDecryptedCertificatePassphraseMaxLength()
		{
			var credential = Factory.New<GlbCompanyCredential>();

			AssertEquals(32, credential.CurrentDecryptedCertificatePassphraseInfo.MaxLength);
		}

		public void TestLookupsAndValidationType()
		{
			Assert(GlbExternalPassword.Lookups is GlbCompanyCredentialLookups);
			Assert(GlbExternalPassword.Validation is GlbCompanyCredentialValidation);
		}
	}
}
