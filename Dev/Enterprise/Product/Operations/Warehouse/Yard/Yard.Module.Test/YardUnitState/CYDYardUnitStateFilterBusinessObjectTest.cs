using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDYardUnitStateFilterBusinessObject))]
	public class CYDYardUnitStateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		CYDYardUnitStateFilterBusinessObject Filter;
		CYDYardTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Filter = (CYDYardUnitStateFilterBusinessObject)GetNewFilterStripBusinessObject();
			Helper = new CYDYardTestHelper(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDYardUnitStateFilterBusinessObject();
		}

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateCYDWarehouse(warehouseCode: "WH1");
			var warehouse2 = Helper.CreateCYDWarehouse(warehouseCode: "WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var yardUnitState1 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState1.YUS_WW_CurrentYard = warehouse1.PK;
			var yardUnitState2 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState2.YUS_WW_CurrentYard = warehouse2.PK;

			Factory.Save();

			var collection = new CYDYardUnitStateCollection(Factory);
			AssertEquals("Should have 2 yardUnitState", 2, collection.Count);

			collection.AdditionalFilter = Filter.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 yardUnitState", 1, collection.Count);
				AssertCollectionNotContains("Should not have yardUnitState1", yardUnitState1, collection);
				AssertCollectionContains("Should have yardUnitState2", yardUnitState2, collection);
			});
		}
	}
}
