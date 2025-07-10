using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseReceiveValueSourceTest : WarehouseJobValueSourceTest
	{
		#region TestWarehouseReceiveValueSource

		public void TestWarehouseReceiveValueSource()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var valueProviders1 = new NumberGeneratorValueProviderCollection();
			valueProviders1.AddRange(new WarehouseJobValueSource(docket));
			valueProviders1.AddRange(new WarehouseReceiveValueSource(docket));

			AssertEquals("Supplier Code", "", valueProviders1[Keys.WarehouseSupplierCode].GetValue(Generator, "9"));
			AssertEquals("Receive Category Code", "", valueProviders1[Keys.WarehouseReceiveCategoryCode].GetValue(Generator, "3"));

			var supplier = Helper.CreateClient("ORGTEST1");
			docket.SupplierDocAddress.OrganisationPK = supplier.PK;

			docket.WD_ReceiveCategory = "RC1";

			AssertEquals("Supplier Code", "ORGTEST1", valueProviders1[Keys.WarehouseSupplierCode].GetValue(Generator, "9"));
			AssertEquals("Supplier Code", "ORGTE", valueProviders1[Keys.WarehouseSupplierCode].GetValue(Generator, "5"));
			AssertEquals("Supplier Code", "O", valueProviders1[Keys.WarehouseSupplierCode].GetValue(Generator, "1"));
			AssertEquals("Receive Category Code", "RC1", valueProviders1[Keys.WarehouseReceiveCategoryCode].GetValue(Generator, "3"));
			AssertEquals("Receive Category Code", "RC", valueProviders1[Keys.WarehouseReceiveCategoryCode].GetValue(Generator, "2"));
			AssertEquals("Receive Category Code", "R", valueProviders1[Keys.WarehouseReceiveCategoryCode].GetValue(Generator, "1"));
		}

		protected override WhsDocket GetNewBusinessObject(OrgHeader org, WhsWarehouse whs)
		{
			return Helper.CreateWhsReceive(org, whs);
		}

		#endregion
	}
}
