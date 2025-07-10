using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class ExportSupportingDocumentLookupsTest : SupportingDocumentLookupsAbstractTest
	{
		protected override string MessageType => JobMessageTypeList.Codes.Export;

		public void TestList()
		{
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();

			CombineAssertions(() =>
			{
				AssertEquals(1, collection.Count);
				var lookupRow = collection[0];
				AssertEquals(lookupRow.ZZD_Code, ValidExportCode);
			});
		}

		public void TestEmptyListWhenMissingDeclaration()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = supportingDocument.Lookups.CodeList;

			AssertEquals(0, collection.Count);
		}
	}
}
