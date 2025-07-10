using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefDocOrgCusCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentTypeList()
		{
			var list = RefDocOrgCusCodeLookups.GetDocumentTypeList(Factory);

			AssertEquals("DocumentTypeList should have 4 types", 4, list.Count);
			Assert("DocumentTypeList should contain 'AWB' type", list.ContainsCode("AWB"));
			Assert("DocumentTypeList should contain 'HAW' type", list.ContainsCode("HAW"));
			Assert("DocumentTypeList should contain 'ESI' type", list.ContainsCode("ESI"));
			Assert("DocumentTypeList should contain 'HBL' type", list.ContainsCode("HBL"));
		}

		public void TestDirectionList()
		{
			var list = RefDocOrgCusCodeLookups.GetDirectionList(Factory);

			AssertEquals("DirectionList should have 3 types", 3, list.Count);
			Assert("DirectionList should contain 'BTH' type", list.ContainsCode("BTH"));
			Assert("DirectionList should contain 'IMP' type", list.ContainsCode("IMP"));
			Assert("DirectionList should contain 'EXP' type", list.ContainsCode("EXP"));
		}
	}
}
