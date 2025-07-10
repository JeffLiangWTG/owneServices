using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyEInvoicingCertificateCredential))]
	internal sealed class GlbCompanyEInvoicingCredentialTest : EInvoicingCredentialTest<GlbCompanyEInvoicingCertificateCredential>
	{
		public override void TestIsCorrectlySetupForTypeDecider()
		{
			Assert("No longer returned by the type decider, placeholder until class is deleted", true);
		}

		#region Implementation

		protected override GlbCompanyEInvoicingCertificateCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var certificateCredential = base.CreateNewGlbExternalPassword(factory);
			certificateCredential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			return certificateCredential;
		}

		#endregion
	}
}
