using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WarehouseJobValueSourceTest : WhsTestCaseWithFactory
	{
		#region TestWarehouseJobValueSource

		public void TestWarehouseJobValueSource()
		{
			var org = Helper.CreateClient("ORGTEST1");
			var whs = Helper.CreateWarehouse("WHS");

			var docket = GetNewBusinessObject(org, whs);

			var valueProviders1 = new NumberGeneratorValueProviderCollection();
			valueProviders1.AddRange(new WarehouseJobValueSource(docket));

			AssertEquals("Warehouse Code", "WHS", valueProviders1[Keys.WarehouseCode].GetValue(Generator, "3"));
			AssertEquals("Warehouse Code", "WH", valueProviders1[Keys.WarehouseCode].GetValue(Generator, "2"));
			AssertEquals("Warehouse Code", "W", valueProviders1[Keys.WarehouseCode].GetValue(Generator, "1"));
			AssertEquals("Client Code", "ORGTEST1", valueProviders1[Keys.WarehouseClientCode].GetValue(Generator, "9"));
			AssertEquals("Client Code", "ORGTE", valueProviders1[Keys.WarehouseClientCode].GetValue(Generator, "5"));
			AssertEquals("Client Code", "O", valueProviders1[Keys.WarehouseClientCode].GetValue(Generator, "1"));
			AssertEquals("Sub Type", docket.WD_DocketSubType, valueProviders1[Keys.WarehouseSubType].GetValue(Generator, "3"));
		}

		protected NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}

		NumberGenerator generator;

		protected abstract WhsDocket GetNewBusinessObject(OrgHeader org, WhsWarehouse whs);

		#endregion
	}
}
