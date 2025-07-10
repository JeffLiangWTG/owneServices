using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GenRegCertAccredMaintList.Loader))]
	sealed class GlbCertificateLoaderTest : LoaderTestCase
	{
		public void TestGetByCertificateType()
		{
			GlbStaff betty = Factory.New<GlbStaff>();

			AssertEquals("Blank by default", "", new GenRegCertAccredMaintList.Loader(Factory).GetByCertificateType(betty, "DTA"));

			GenRegCertAccredMaintList cert1 = betty.Certificates.AddNew();
			cert1.XZ_Type = "DTA";
			cert1.XZ_RefNumber = "111-222-333";

			GenRegCertAccredMaintList cert2 = betty.Certificates.AddNew();
			cert2.XZ_Type = "ZZZ";
			cert2.XZ_RefNumber = "3-4-5";

			AssertEquals("Correct number", cert1.XZ_RefNumber, new GenRegCertAccredMaintList.Loader(Factory).GetByCertificateType(betty, "DTA"));

			cert1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-10);
			AssertEquals("Blank, as expired", ZString.Empty, new GenRegCertAccredMaintList.Loader(Factory).GetByCertificateType(betty, "DTA"));

			cert1.XZ_ExpiryOrDueDate = ZDateTime.Empty;
			AssertEquals("Correct number - as no expiry specified", cert1.XZ_RefNumber, new GenRegCertAccredMaintList.Loader(Factory).GetByCertificateType(betty, "DTA"));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new GenRegCertAccredMaintList.Loader(Factory);
		}

		public void TestDeleteAllRegCodePatterns()
		{
			var certificate = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var patterns = PatternMatchingTestHelper.CreateCompleteSetOfPatternMatchingObjects(Factory, certificate);

			Factory.Save();
			certificate.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Certificate should be deleted", true, certificate.IsDeleted);
				AssertContainsExactElementsInAnyOrder("Certificate patterns should all be deleted", System.Array.Empty<BusinessObject>(), patterns.Where(p => !p.IsDeleted));
			});
		}
	}
}
