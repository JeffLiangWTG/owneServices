using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CaseNumberLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var caseNumber = Factory.New<CaseNumber>();
			AssertEquals(true, object.ReferenceEquals(caseNumber.Lookups.CY_CodeList, Factory.GetCachedValue<CaseNumberTypeList>()));
		}

		public void TestDocumentStatusCodeList()
		{
			var caseNumber = Factory.New<CaseNumber>();
			AssertEquals(true, object.ReferenceEquals(caseNumber.Lookups.DocumentStatusCodeList, Factory.GetCachedValue<DocumentStatusCodes>()));
		}
	}
}
