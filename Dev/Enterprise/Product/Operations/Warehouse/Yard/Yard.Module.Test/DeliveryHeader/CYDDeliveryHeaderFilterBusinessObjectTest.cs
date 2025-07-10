using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDDeliveryHeaderFilterBusinessObject))]
	public class CYDDeliveryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		CYDDeliveryHeaderFilterBusinessObject Filter;
		CYDYardTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Filter = (CYDDeliveryHeaderFilterBusinessObject)GetNewFilterStripBusinessObject();
			Helper = new CYDYardTestHelper(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDDeliveryHeaderFilterBusinessObject();
		}

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateCYDWarehouse(warehouseCode: "WH1");
			var warehouse2 = Helper.CreateCYDWarehouse(warehouseCode: "WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var deliveryHeader1 = Factory.NewWithValidTestData<CYDDeliveryHeader>();
			deliveryHeader1.YDH_WW_Yard = warehouse1.PK;
			var deliveryHeader2 = Factory.NewWithValidTestData<CYDDeliveryHeader>();
			deliveryHeader2.YDH_WW_Yard = warehouse2.PK;
			deliveryHeader2.YDH_IsBulkRun = true;
			deliveryHeader2.YDH_FromDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(-1);
			deliveryHeader2.YDH_ToDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(1);

			Factory.Save();

			var collection = new CYDDeliveryHeaderCollection(Factory);
			AssertEquals("Should have 2 deliveryHeader", 2, collection.Count);

			collection.AdditionalFilter = Filter.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 deliveryHeader", 1, collection.Count);
				AssertCollectionNotContains("Should not have deliveryHeader1", deliveryHeader1, collection);
				AssertCollectionContains("Should have deliveryHeader2", deliveryHeader2, collection);
			});
		}
	}
}
