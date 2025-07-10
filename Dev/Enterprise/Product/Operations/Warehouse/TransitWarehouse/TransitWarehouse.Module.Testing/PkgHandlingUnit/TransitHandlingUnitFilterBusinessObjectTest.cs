using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(TransitHandlingUnitFilterBusinessObject))]
	public class TransitHandlingUnitFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestIsHandlingUnit

		public void TestOnlyLoadTransitHandlingUnit()
		{
			Helper.CreateTransitWarehouseInCurrentBranch();

			var transitHandlingUnit = Helper.CreatePackageHandlingUnit();
			var productHandlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			productHandlingUnit.KPU_JobContext = "3PL";
			var transportHandlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			transportHandlingUnit.KPU_JobContext = "LTR";
			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			Asserter.AddToScope(transitHandlingUnit, productHandlingUnit, transportHandlingUnit);
			Asserter.AssertMatches("Only return transit Handling Unit Packages.", filters.Filter, transitHandlingUnit);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var hu1 = Helper.CreatePackageHandlingUnit();
			hu1.KPU_GB_Branch = warehouse1.WW_GB_RelatedCompanyBranch;

			var hu2 = Helper.CreatePackageHandlingUnit();
			hu2.KPU_GB_Branch = warehouse1.WW_GB_RelatedCompanyBranch;

			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var hu3 = Helper.CreatePackageHandlingUnit();
			hu3.KPU_GB_Branch = warehouse2.WW_GB_RelatedCompanyBranch;

			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(hu1, hu2, hu3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[TransitHandlingUnitFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter, hu1, hu2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter, hu3);
			filter.Property = warehouse3.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter);
		}

		#endregion

		#region Implemetation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TransitHandlingUnitFilterBusinessObject();
		}

		protected FilterStripAsserter<PkgHandlingUnit> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<PkgHandlingUnit>(Factory, p => p.PK.ToString())); }
		}
		FilterStripAsserter<PkgHandlingUnit> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
