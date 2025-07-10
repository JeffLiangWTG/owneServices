using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class CusSupportingInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcedureList()
		{
			var header = Factory.New<RelatedDeclarationForExport>();
			AssertEquals(new ProcedureList(), header.Lookups.ProcedureList);
		}

		public void TestSubTypeList()
		{
			var header = Factory.New<RelatedDeclarationForExport>();
			AssertEquals(new SubTypeList(), header.Lookups.SubTypeList);
		}
	}
}
