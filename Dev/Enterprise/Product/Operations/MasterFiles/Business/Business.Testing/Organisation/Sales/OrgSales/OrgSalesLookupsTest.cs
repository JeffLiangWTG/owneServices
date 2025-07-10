using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLocations_ShouldOnlyIncludeLocationTypesAllowedOnSalesProduct()
		{
			var shpProduct = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = shpProduct.Identifier;
			AssertEquals(shpProduct.AllowedLocationTypes, sales.Lookups.Locations.LocationsTypesToInclude);
		}
	}
}
