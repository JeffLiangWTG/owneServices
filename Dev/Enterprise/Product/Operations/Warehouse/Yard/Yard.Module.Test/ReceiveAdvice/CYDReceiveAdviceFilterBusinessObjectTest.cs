using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDReceiveAdviceFilterBusinessObject))]
	public class CYDReceiveAdviceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		CYDReceiveAdviceFilterBusinessObject Filter;
		CYDYardTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Filter = (CYDReceiveAdviceFilterBusinessObject)GetNewFilterStripBusinessObject();
			Helper = new CYDYardTestHelper(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDReceiveAdviceFilterBusinessObject();
		}

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateCYDWarehouse(warehouseCode: "WH1");
			var warehouse2 = Helper.CreateCYDWarehouse(warehouseCode: "WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var receiveAdvice1 = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice1.YRA_WW_Yard = warehouse1.PK;
			var receiveAdvice2 = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice2.YRA_WW_Yard = warehouse2.PK;

			Factory.Save();

			var collection = new CYDReceiveAdviceCollection(Factory);
			AssertEquals("Should have 2 ReceiveAdvice", 2, collection.Count);

			collection.AdditionalFilter = Filter.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 receiveAdvice", 1, collection.Count);
				AssertCollectionNotContains("Should not have receiveAdvice1", receiveAdvice1, collection);
				AssertCollectionContains("Should have receiveAdvice2", receiveAdvice2, collection);
			});
		}
	}
}
