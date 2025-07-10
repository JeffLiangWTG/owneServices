using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteDockGatePivot))]
	public sealed class GteDockGatePivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new WhsTestHelperFunctionsEnv(factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var area = helper.CreateArea(warehouse, "AREA");
			var dock = factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = area.PK;
			factory.Save();

			var gate = Factory.NewWithValidTestData<GteGate>();

			var pivot = Factory.New<GteDockGatePivot>();
			pivot.GDP_WL_Dock = dock.PK;
			pivot.GDP_GTE_Gate = gate.PK;
			pivot.GDP_Direction = "BOTH";
			return pivot;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var area = helper.CreateArea(warehouse, "AREA");
			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = area.PK;
			Factory.Save();

			var gate = Factory.NewWithValidTestData<GteGate>();

			var pivot = Factory.New<GteDockGatePivot>();
			pivot.GDP_WL_Dock = dock.PK;
			pivot.GDP_GTE_Gate = gate.PK;
			pivot.GDP_Direction = "BOTH";
			return pivot;
		}
	}
}
