using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsVASOrderLineLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestProducts

		public void TestProducts()
		{
			var client = Factory.New<OrgHeader>();
			var vasOrder = Factory.New<WhsVASOrder>();
			var line1 = vasOrder.Lines.AddNew();
			AssertEquals(typeof(WhsOrgSupplierPartCollection), line1.Lookups.Products.GetType());
			AssertEquals(false, line1.Lookups.Products.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));

			vasOrder.WVO_OH_Client = client.PK;
			var line2 = vasOrder.Lines.AddNew();
			AssertEquals(typeof(WhsOrgSupplierPartCollection), line2.Lookups.Products.GetType());
			AssertEquals(true, line2.Lookups.Products.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
		}

		#endregion
	}
}
