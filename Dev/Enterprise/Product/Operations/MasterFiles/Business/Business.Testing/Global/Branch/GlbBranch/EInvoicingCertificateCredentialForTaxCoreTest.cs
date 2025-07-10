using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingCertificateCredentialForTaxCore))]
	internal sealed class EInvoicingCertificateCredentialForTaxCoreTest : EInvoicingCredentialTest<EInvoicingCertificateCredentialForTaxCore>
	{
		protected override string ExpectedPasswordType => PasswordTypesList.Codes.FPC;
		protected override string SubjectNameSample => "CN=Child;SERIALNUMBER = PTY7TTHW";

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			var credential = CreateNewGlbExternalPassword(Factory);
			AssertEquals(nameof(credential.GP_GB), GlbBranch.CurrentBranch.PK, credential.GP_GB);
		}

		public void TestCompanyBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Fiji;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();

			AssertNotNull(nameof(branch.CertificateCredentialsTaxCore), branch.CertificateCredentialsTaxCore);

			var branchCredential = branch.CertificateCredentialsTaxCore.AddNew();
			AssertEquals(nameof(branchCredential.GP_GB), branch.PK, branchCredential.GP_GB);
		}

		public override void TestLookups()
		{
			Assert("EInvoicingCredential.Lookups is EInvoicingCredentialLookups", GlbExternalPassword.Lookups is EInvoicingCredentialLookups);
			AssertEquals("EInvoicingCredential.Lookups.PasswordTypeList.Count", 1, GlbExternalPassword.Lookups.PasswordTypeList.Count);
			AssertEquals("EInvoicingCredential.Lookups.PasswordTypeList[0].Code", ExpectedPasswordType, GlbExternalPassword.Lookups.PasswordTypeList[0].Code);
		}

		public override void TestValidationType()
		{
			Assert(GlbExternalPassword.Validation is EInvoicingCertificateCredentialForTaxCoreValidation);
		}

		protected override EInvoicingCertificateCredentialForTaxCore CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<EInvoicingCertificateCredentialForTaxCore>();
		}
	}
}
