using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SupportingDocument>
	{
		public void TestHasAnyWithCode()
		{
			var collection = SupportingDocuments;
			CombineAssertions(() =>
			{
				AssertEquals("Without any document, has M2?", expected: false, collection.HasAnyWithCode("M2"));
				AssertEquals("Without any document, has ZZ?", expected: false, collection.HasAnyWithCode("ZZ"));

				var supportingDocument = collection.AddNew();
				supportingDocument.CSI_Code = "M2";
				AssertEquals("With M2 document, has M2?", expected: true, collection.HasAnyWithCode("M2"));
				AssertEquals("With M2 document, has ZZ?", expected: false, collection.HasAnyWithCode("ZZ"));

				supportingDocument.CSI_Code = "ZZ";
				AssertEquals("With MZ document, has M2?", expected: false, collection.HasAnyWithCode("M2"));
				AssertEquals("With MZ document, has ZZ?", expected: true, collection.HasAnyWithCode("ZZ"));
			});
		}

		protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new SupportingDocumentCollection(declaration);
		}

		SupportingDocumentCollection SupportingDocuments => GetCusSupportingInfoCollection() as SupportingDocumentCollection;
	}
}
