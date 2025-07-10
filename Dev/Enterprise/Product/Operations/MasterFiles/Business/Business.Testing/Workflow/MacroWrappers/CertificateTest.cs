using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class CertificateTest : TestCaseWithFactory
	{
		public void TestCertificate()
		{
			var today = ZDateTime.Today;

			var certificateBO = Factory.New<GenRegCertAccredMaintList>();
			certificateBO.MasterParent = GlbStaff.CurrentUser;
			certificateBO.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.APP;
			certificateBO.XZ_ExpiryOrDueDate = today;

			var certificate = new Certificate(certificateBO);

			CombineAssertions(() =>
			{
				AssertEquals("Type.Code", "APP", certificate.Type.Code);
				AssertEquals("Type.Description", "Airport Pass", certificate.Type.Description);
				AssertEquals("ExpiryDate", today, certificate.ExpiryDate);
			});
		}

		public void TestNullCertificate()
		{
			var certificate = new Certificate(null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Type.Code", certificate.Type.Code);
				AssertNullOrEmpty("Type.Description", certificate.Type.Description);
				AssertEquals("ExpiryDate", ZDateTime.Empty, certificate.ExpiryDate);
			});
		}
	}
}
