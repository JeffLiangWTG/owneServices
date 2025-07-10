using CargoWise.RefDbRepo.USReferenceData.Business.DISCodes;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	sealed class DISCodeTest
	{
		[Test]
		public void TestDISCode()
		{
			var disCode = new DISCode();
			disCode.AgencyCode = "Test";
			disCode.AgencyCode = "AgencyCode";

			disCode.DocumentDescription = "Test";
			disCode.DocumentDescription = "DocumentDescription";

			disCode.DocumentType = "Test";
			disCode.DocumentType = "DocumentType";

			disCode.DocumentLabelCode = "Test";
			disCode.DocumentLabelCode = "DocumentLabelCode";

			disCode.DocCode = "Test";
			disCode.DocCode = "DocCode";

			disCode.Metadata = "Test";
			disCode.Metadata = "Metadata";

			Assert.AreEqual("TestAgencyCode", disCode.AgencyCode);
			Assert.AreEqual("TestDocumentDescription", disCode.DocumentDescription);
			Assert.AreEqual("TestDocumentType", disCode.DocumentType);
			Assert.AreEqual("TestDocumentLabelCode", disCode.DocumentLabelCode);
			Assert.AreEqual("TestDocCode", disCode.DocCode);
			Assert.AreEqual("TestMetadata", disCode.Metadata);
		}
	}
}
