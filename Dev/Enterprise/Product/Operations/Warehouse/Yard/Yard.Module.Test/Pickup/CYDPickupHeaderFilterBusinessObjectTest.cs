using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDPickupHeaderFilterBusinessObject))]
	public class CYDPickupHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		CYDPickupHeaderFilterBusinessObject Filter;
		CYDYardTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Filter = (CYDPickupHeaderFilterBusinessObject)GetNewFilterStripBusinessObject();
			Helper = new CYDYardTestHelper(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDPickupHeaderFilterBusinessObject();
		}

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateCYDWarehouse(warehouseCode: "WH1");
			var warehouse2 = Helper.CreateCYDWarehouse(warehouseCode: "WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var pickupHeader1 = Factory.NewWithValidTestData<CYDPickupHeader>();
			pickupHeader1.YPH_WW_Yard = warehouse1.PK;
			var pickupHeader2 = Factory.NewWithValidTestData<CYDPickupHeader>();
			pickupHeader2.YPH_WW_Yard = warehouse2.PK;
			pickupHeader2.YPH_IsBulkRun = true;
			pickupHeader2.YPH_FromDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(-1);
			pickupHeader2.YPH_ToDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(1);

			Factory.Save();

			var collection = new CYDPickupHeaderCollection(Factory);
			AssertEquals("Should have 2 pickups", 2, collection.Count);

			collection.AdditionalFilter = Filter.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 pickup", 1, collection.Count);
				AssertCollectionNotContains("Should not have pickup1", pickupHeader1, collection);
				AssertCollectionContains("Should have pickup2", pickupHeader2, collection);
			});
		}
	}
}
