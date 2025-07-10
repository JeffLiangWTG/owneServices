using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseOrderValueSourceTest : WarehouseJobValueSourceTest
	{
		#region TestWarehouseOrderValueSource

		public void TestWarehouseOrderValueSource()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var docket = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			docket.WD_ExternalReference = "";

			var valueProviders1 = new NumberGeneratorValueProviderCollection();
			valueProviders1.AddRange(new WarehouseJobValueSource(docket));
			valueProviders1.AddRange(new WarehouseOrderValueSource(docket));

			AssertEquals("Sales Channel Code", "", valueProviders1[Keys.WarehouseSalesChannelCode].GetValue(Generator, "3"));

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			docket.WD_WSH_SalesChannel = salesChannel.PK;

			AssertEquals("Sales Channel Code", "ECO", valueProviders1[Keys.WarehouseSalesChannelCode].GetValue(Generator, "3"));
			AssertEquals("Sales Channel Code", "EC", valueProviders1[Keys.WarehouseSalesChannelCode].GetValue(Generator, "2"));
			AssertEquals("Sales Channel Code", "E", valueProviders1[Keys.WarehouseSalesChannelCode].GetValue(Generator, "1"));
		}

		protected override WhsDocket GetNewBusinessObject(OrgHeader org, WhsWarehouse whs)
		{
			return Helper.CreateWhsOrder(org, whs);
		}

		#endregion
	}
}
