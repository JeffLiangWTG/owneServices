using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNHTSADocumentAddInfo))]
	sealed class USNHTSADocumentAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocument()
		{
			var document = Factory.New<NHTSADocument>();
			var addInfo = new USNHTSADocumentAddInfo(document.B7_AddInfoDataInfo);
			AssertEquals(document.PK, addInfo.Document.PK);
		}

		public void TestHumanReadableNames()
		{
			var document = Factory.New<NHTSADocument>();
			AssertEquals("Document Type", document.US_NHTDocumentTypeInfo.HumanReadableName);
			AssertEquals("Who has a copy?", document.US_NHTDocumentOwnerInfo.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var document = Factory.New<NHTSADocument>();
			return new USNHTSADocumentAddInfo(document.B7_AddInfoDataInfo);
		}
	}
}
