using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlbStaffCredential))]
	sealed class GlbStaffCredentialTest : GlbExternalPasswordTest<GlbStaffCredential>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var staffCredential = Factory.New<GlbStaffCredential>();
			staffCredential.GP_GS = GlbStaff.CurrentUser.PK;
			return staffCredential;
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(PasswordTypesList.Codes.EBD, GlbExternalPassword.GP_PasswordType);
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
			AssertNotEquals(ZGuid.Empty, GlbExternalPassword.GP_GS);
			AssertEquals(PasswordStatusList.Codes.Valid, GlbExternalPassword.GP_PasswordStatus);
			AssertEquals(PasswordStatusList.Descriptions.Valid, GlbExternalPassword.PasswordStatus);
		}

		public void TestCurrentDecryptedCertificatePassphraseMaxLength()
		{
			AssertEquals(32, GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo.MaxLength);
		}

		public void TestLookupsAndValidationType()
		{
			Assert(GlbExternalPassword.Lookups is GlbStaffCredentialLookups);
			Assert(GlbExternalPassword.Validation is GlbStaffCredentialValidation);
		}
	}
}
