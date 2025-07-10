using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	sealed class HelpersTest
	{
		[Test]
		public void TestCreateSoapEnvelopeWithRawBody()
		{
			var systemTableRequest_2012 = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableRequest_2012.xml");
			var expectedSoapEnvelopeWithRawBody = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableRequest_2012SoapEnvelope.xml");
			var actualSoapEnvelopeWithRawBody = Helpers.CreateSoapEnvelopeWithRawBody(systemTableRequest_2012, "901", "560038416", "1CF1A4D0-3EDB-4A83-92A4-AC9389348221");
			Assert.AreEqual(expectedSoapEnvelopeWithRawBody, actualSoapEnvelopeWithRawBody);
		}
	}
}
