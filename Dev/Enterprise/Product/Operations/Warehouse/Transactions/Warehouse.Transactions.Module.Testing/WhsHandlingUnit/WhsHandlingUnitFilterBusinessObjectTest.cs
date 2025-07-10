using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsHandlingUnitFilterBusinessObject))]
	public class WhsHandlingUnitFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestOnlyLoadProductWarehouseHandlingUnit

		public void TestOnlyLoadProductWarehouseHandlingUnit()
		{
			var productHandlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			productHandlingUnit.KPU_JobContext = "3PL";
			var transitHandlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			transitHandlingUnit.KPU_JobContext = "TWH";
			var transportHandlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			transportHandlingUnit.KPU_JobContext = "LTR";
			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			Asserter.AddToScope(transitHandlingUnit, productHandlingUnit, transportHandlingUnit);
			Asserter.AssertMatches("Only return Product Warehouse Handling Unit Packages.", filters.Filter, productHandlingUnit);
		}

		#endregion

		#region TestWarehouseFilter

		public void TestWarehouseFilter()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1", "A", 1, 1);
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var warehouse2 = Helper.CreateWarehouse("WHS2", "A", 1, 1);
			warehouse2.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var warehouse3 = Helper.CreateWarehouse("WHS3", "A", 1, 1);
			warehouse3.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var handlingUnit1 = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit1.KPU_GB_Branch = warehouse1.WW_GB_RelatedCompanyBranch;
			handlingUnit1.KPU_JobContext = "3PL";
			var handlingUnit2 = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit2.KPU_GB_Branch = warehouse2.WW_GB_RelatedCompanyBranch;
			handlingUnit2.KPU_JobContext = "3PL";
			var handlingUnit3 = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit3.KPU_GB_Branch = warehouse3.WW_GB_RelatedCompanyBranch;
			handlingUnit3.KPU_JobContext = "3PL";
			var handlingUnit4 = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit4.KPU_GB_Branch = warehouse1.WW_GB_RelatedCompanyBranch;
			handlingUnit4.KPU_JobContext = "3PL";
			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			Asserter.AddToScope(handlingUnit1, handlingUnit2, handlingUnit3, handlingUnit4);

			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsHandlingUnitFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return Handling Unit for the given warehouses", filters.Filter, handlingUnit1, handlingUnit4);

			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return Handling Unit for the given warehouse", filters.Filter, handlingUnit2);

			filter.Property = warehouse3.PK;
			Asserter.AssertMatches("Do not return Handling Unit for warehouse which is not product warehouse", filters.Filter);
		}

		#endregion

		#region Implemetation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsHandlingUnitFilterBusinessObject();
		}

		protected FilterStripAsserter<PkgHandlingUnit> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<PkgHandlingUnit>(Factory, p => p.PK.ToString())); }
		}
		FilterStripAsserter<PkgHandlingUnit> asserter;

		#endregion

		#region Helper

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
