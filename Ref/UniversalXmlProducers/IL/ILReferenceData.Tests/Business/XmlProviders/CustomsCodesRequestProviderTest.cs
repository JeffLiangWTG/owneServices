using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	sealed class CustomsCodesRequestProviderTest
	{
		[Test]
		public void TestConstructor()
		{
			Assert.Catch<ArgumentNullException>(() => new CustomsCodesRequestProvider(null), "ArgumentNullException raise when systemTableRequestWrapper is null ");
		}

		[Test]
		public void TestGetSystemTableRequest_ByTableName()
		{
			var expectedXml = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableRequest_2012.xml");
			var customsCodesRequest = new CustomsCodesRequestProvider(new SYSTBL_NG_9000_MSG_SystemTableRequestWrapper("2012", new DateTime(2024, 6, 2)));
			var request = customsCodesRequest.GetSystemTableRequest();
			Assert.IsNotNull(request, "request is not null");
			Assert.AreEqual(request, expectedXml);
		}
	}
}
