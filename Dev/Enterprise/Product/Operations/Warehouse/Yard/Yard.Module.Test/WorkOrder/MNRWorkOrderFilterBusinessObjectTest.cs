using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRWorkOrderFilterBusinessObject))]
	public class MNRWorkOrderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		MNRWorkOrderFilterBusinessObject Filter;
		CYDYardTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Filter = (MNRWorkOrderFilterBusinessObject)GetNewFilterStripBusinessObject();
			Helper = new CYDYardTestHelper(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new MNRWorkOrderFilterBusinessObject();
		}

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateCYDWarehouse(warehouseCode: "WH1");
			var warehouse2 = Helper.CreateCYDWarehouse(warehouseCode: "WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var workOrder1 = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			workOrder1.MWO_WW_Facility = warehouse1.PK;
			var workOrder2 = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			workOrder2.MWO_WW_Facility = warehouse2.PK;

			Factory.Save();

			var collection = new MNRWorkOrderHeaderCollection(Factory);
			AssertEquals("Should have 2 workOrder", 2, collection.Count);

			collection.AdditionalFilter = Filter.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 workOrder", 1, collection.Count);
				AssertCollectionNotContains("Should not have workOrder1", workOrder1, collection);
				AssertCollectionContains("Should have workOrder2", workOrder2, collection);
			});
		}
	}
}
