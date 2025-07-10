using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class ImportSupportingDocumentLookupsTest : SupportingDocumentLookupsAbstractTest
	{
		protected override string MessageType => JobMessageTypeList.Codes.Import;

		public void TestList()
		{
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();

			CombineAssertions(() =>
			{
				AssertEquals(1, collection.Count);
				var lookupRow = collection[0];
				AssertEquals(lookupRow.ZZD_Code, ValidImportCode);
			});
		}
	}
}
