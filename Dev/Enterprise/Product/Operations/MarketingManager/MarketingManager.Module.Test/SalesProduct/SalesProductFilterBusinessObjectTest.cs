using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(SalesProductFilterBusinessObject))]
	public class SalesProductFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Module Filters

		public void TestCodeFilter()
		{
			var salesProduct1 = Factory.NewWithValidTestData<OrgSalesProduct>();
			var salesProduct2 = Factory.NewWithValidTestData<OrgSalesProduct>();
			var salesProduct3 = Factory.NewWithValidTestData<OrgSalesProduct>();

			salesProduct1.MP_Code = "PRD1";
			salesProduct2.MP_Code = "PRD2";
			salesProduct3.MP_Code = "PRD3";

			Factory.Save();

			var filterBizObj = new SalesProductFilterBusinessObject();
			var codeFilter = (ModuleTextFilter)filterBizObj[SalesProductFilterBusinessObject.FilterDescription.Code];
			AssertNotNull(codeFilter);
			AssertEquals("Code", codeFilter.MultilingualDescription);
			codeFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProduct>.PKOnlyComparer,
				new[] { salesProduct1, salesProduct2, salesProduct3 },
				Factory.Load<OrgSalesProduct>(GetFilterIgnoringSystemDefined(filterBizObj.Filter)));

			codeFilter.Property = "PRD1";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProduct>.PKOnlyComparer,
				new[] { salesProduct1 },
				Factory.Load<OrgSalesProduct>(GetFilterIgnoringSystemDefined(filterBizObj.Filter)));

			codeFilter.Property = "PRD2";
			codeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProduct>.PKOnlyComparer,
				new[] { salesProduct1, salesProduct3 },
				Factory.Load<OrgSalesProduct>(GetFilterIgnoringSystemDefined(filterBizObj.Filter)));
		}

		public void TestNameFilter()
		{
			var salesProduct1 = Factory.NewWithValidTestData<OrgSalesProduct>();
			var salesProduct2 = Factory.NewWithValidTestData<OrgSalesProduct>();
			var salesProduct3 = Factory.NewWithValidTestData<OrgSalesProduct>();

			salesProduct1.MP_Name = "Product 1";
			salesProduct2.MP_Name = "Product 2";
			salesProduct3.MP_Name = "Product 3";

			Factory.Save();

			var filterBizObj = new SalesProductFilterBusinessObject();
			var nameFilter = (ModuleTextFilter)filterBizObj[SalesProductFilterBusinessObject.FilterDescription.Name];
			AssertNotNull(nameFilter);
			AssertEquals("Name", nameFilter.MultilingualDescription);
			nameFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProduct>.PKOnlyComparer,
				new[] { salesProduct1, salesProduct2, salesProduct3 },
				Factory.Load<OrgSalesProduct>(GetFilterIgnoringSystemDefined(filterBizObj.Filter)));

			nameFilter.Property = "Product 1";
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProduct>.PKOnlyComparer,
				new[] { salesProduct1 },
				Factory.Load<OrgSalesProduct>(GetFilterIgnoringSystemDefined(filterBizObj.Filter)));

			nameFilter.Property = "Product 2";
			nameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProduct>.PKOnlyComparer,
				new[] { salesProduct1, salesProduct3 },
				Factory.Load<OrgSalesProduct>(GetFilterIgnoringSystemDefined(filterBizObj.Filter)));
		}

		#endregion

		#region Implementation

		ZQuery GetFilterIgnoringSystemDefined(ZQuery original)
		{
			var filter = new ZQuery(original);
			filter.AddToFilter(OrgSalesProductSchema.MP_IsSystemDefined, false);
			return filter;
		}

		#endregion

		#region Overrides

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SalesProductFilterBusinessObject();
		}

		#endregion
	}
}
