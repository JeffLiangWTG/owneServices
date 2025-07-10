using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDTransportationUnitFilterBusinessObject))]
	public class CYDTransportationUnitFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		CYDTransportationUnitFilterBusinessObject Filter;
		CYDYardTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Filter = (CYDTransportationUnitFilterBusinessObject)GetNewFilterStripBusinessObject();
			Helper = new CYDYardTestHelper(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDTransportationUnitFilterBusinessObject();
		}

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateCYDWarehouse(warehouseCode: "WH1");
			var warehouse2 = Helper.CreateCYDWarehouse(warehouseCode: "WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var transportationUnit1 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			transportationUnit1.YTU_WW_Yard = warehouse1.PK;
			var transportationUnit2 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			transportationUnit2.YTU_WW_Yard = warehouse2.PK;

			Factory.Save();

			var collection = new CYDTransportationUnitCollection(Factory);
			AssertEquals("Should have 2 transportationUnit", 2, collection.Count);

			collection.AdditionalFilter = Filter.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 transportationUnit", 1, collection.Count);
				AssertCollectionNotContains("Should not have transportationUnit1", transportationUnit1, collection);
				AssertCollectionContains("Should have transportationUnit2", transportationUnit2, collection);
			});
		}
	}
}
