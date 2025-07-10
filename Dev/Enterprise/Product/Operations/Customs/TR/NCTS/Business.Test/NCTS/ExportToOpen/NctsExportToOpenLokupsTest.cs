using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsExportToOpenLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclarationTypeList()
		{
			var document = Factory.New<NctsExportToOpen>();

			AssertSame(Factory.GetCachedValue<DeclarationTypeList>(), document.Lookups.DeclarationTypeList);
		}
	}
}
