using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RelatedDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			RelatedDocument relatedDocument = Factory.New<RelatedDocument>();
			AssertEquals(typeof(RelatedDocumentIdentifierList), relatedDocument.Lookups.CY_CodeList.GetType());
		}
	}
}
