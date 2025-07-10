using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Integration.Testing
{
	sealed class SystemDefinedSalesProductListTest : TestCaseWithFactory
	{
		public void TestAddedToBaseData()
		{
			var list = new SystemDefinedSalesProductList();
			foreach (ICodeDescription item in list)
			{
				var salesProduct = Factory.LoadTop1<IOrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, item.Code));
				AssertNotNull(string.Format("Make sure Sales Product '{0}' is added to the base data.", item.Code), salesProduct);
				AssertEquals(string.Format("Sales Product '{0}' should be system defined", item.Code), true, salesProduct.MP_IsSystemDefined);
				AssertEquals(string.Format("The default name in base data for Sales Product '{0}' should be '{1}'", item.Code, item.Description), item.Description, salesProduct.MP_Name);
			}
		}
	}
}
