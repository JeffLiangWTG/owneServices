using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchEInvoicingCertificateCredential))]
	sealed class GlbBranchEInvoicingCertificateCredentialTest : EInvoicingCredentialTest<GlbBranchEInvoicingCertificateCredential>
	{
		#region Implementation

		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();

			var credential = Factory.New<GlbBranchEInvoicingCertificateCredential>();
			AssertEquals(nameof(credential.GP_GB), GlbBranch.CurrentBranch.PK, credential.GP_GB);
		}

		protected override GlbBranchEInvoicingCertificateCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var certificateCredential = base.CreateNewGlbExternalPassword(factory);
			certificateCredential.GP_GB = GlbBranch.CurrentBranch.PK;
			certificateCredential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			return certificateCredential;
		}

		#endregion

		public override void TestIsCorrectlySetupForTypeDecider()
		{
			Assert("No longer returned by the type decider, placeholder until class is deleted", true);
		}
	}
}
