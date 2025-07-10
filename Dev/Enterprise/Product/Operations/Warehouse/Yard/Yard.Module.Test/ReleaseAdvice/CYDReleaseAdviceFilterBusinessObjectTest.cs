using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDReleaseAdviceFilterBusinessObject))]
	public class CYDReleaseAdviceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		CYDReleaseAdviceFilterBusinessObject Filter;
		CYDYardTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Filter = (CYDReleaseAdviceFilterBusinessObject)GetNewFilterStripBusinessObject();
			Helper = new CYDYardTestHelper(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDReleaseAdviceFilterBusinessObject();
		}

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateCYDWarehouse(warehouseCode: "WH1");
			var warehouse2 = Helper.CreateCYDWarehouse(warehouseCode: "WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;

			var releaseAdvice1 = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			releaseAdvice1.YRE_WW_Yard = warehouse1.PK;
			var releaseAdvice2 = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			releaseAdvice2.YRE_WW_Yard = warehouse2.PK;

			Factory.Save();

			var collection = new CYDReleaseAdviceCollection(Factory);
			AssertEquals("Should have 2 ReleaseAdvice", 2, collection.Count);

			collection.AdditionalFilter = Filter.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 ReleaseAdvice", 1, collection.Count);
				AssertCollectionNotContains("Should not have ReleaseAdvice1", releaseAdvice1, collection);
				AssertCollectionContains("Should have ReleaseAdvice2", releaseAdvice2, collection);
			});
		}
	}
}

