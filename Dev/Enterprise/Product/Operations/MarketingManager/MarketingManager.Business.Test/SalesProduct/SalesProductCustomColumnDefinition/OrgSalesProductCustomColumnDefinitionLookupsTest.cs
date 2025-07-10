using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgSalesProductCustomColumnDefinitionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			var types = Factory.New<OrgSalesProductCustomColumnDefinition>().Lookups.Types;
			Assert(!types.ContainsCode(AddOnColumnDataType.Codes.ComboBox));
		}
	}
}
