using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(ProductStyleFilterBusinessObject))]
	class ProductStyleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestProductStyleCode

		public void TestProductStyleCode()
		{
			AssertProductStyleTextFilter(WhsProductStyleSchema.WST_Code, ProductStyleFilterBusinessObject.FilterConstants.ProductStyleCode);
		}

		#endregion

		#region TestProductStyleDescription

		public void TestProductStyleDescription()
		{
			AssertProductStyleTextFilter(WhsProductStyleSchema.WST_Description, ProductStyleFilterBusinessObject.FilterConstants.ProductStyleDescription);
		}

		#endregion

		#region TestProductStyleOwnerFiler

		public void TestProductStyleOwnerFiler()
		{
			var org1 = Helper.CreateClient("O1");
			var org2 = Helper.CreateClient("O2");
			var productStyle1 = Helper.CreateProductStyle("T1", "Temp 1", org1.PK);
			var productStyle2 = Helper.CreateProductStyle("T2", "Temp 2", org2.PK);
			var productStyle3 = Helper.CreateProductStyle("T3", "Temp 3", org1.PK);
			Asserter.AddToScope(productStyle1, productStyle2, productStyle3);
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var ownerFilter = (ModuleGuidFilter)filterBizO[ProductStyleFilterBusinessObject.FilterConstants.ProductStyleOwner];
			ownerFilter.IsActive = true;
			ownerFilter.Property = org1.PK;
			Asserter.AssertMatches("Styles own by Org1 should be returned.", filterBizO.Filter, productStyle1, productStyle3);

			ownerFilter.Property = org2.PK;
			Asserter.AssertMatches("Style owns by Org2 should be returned.", filterBizO.Filter, productStyle2);

			ownerFilter.Property = ZGuid.Empty;
			Asserter.AssertMatches("All styles should be returned.", filterBizO.Filter, productStyle1, productStyle2, productStyle3);
		}

		#endregion

		#region AssertProductStyleTextFilter

		void AssertProductStyleTextFilter(SchemaStringColumn column, string filterName)
		{
			var org1 = Helper.CreateClient("O1");
			var productStyle1 = Helper.CreateProductStyle("T1", "Temp 1", org1.PK);
			var productStyle2 = Helper.CreateProductStyle("T2", "Temp 2", org1.PK);
			var productStyle3 = Helper.CreateProductStyle("T3", "Temp 3", org1.PK);
			productStyle1[column.Name] = "S1";
			productStyle2[column.Name] = "S2";
			productStyle3[column.Name] = "S3";
			Asserter.AddToScope(productStyle1, productStyle2, productStyle3);
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterBizO[filterName];
			textFilter.IsActive = true;
			textFilter.Property = "S1";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("productStyle1", filterBizO.Filter, productStyle1);

			textFilter.Property = "A";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("productStyle1", filterBizO.Filter);

			textFilter.Property = "S";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("productStyle1", filterBizO.Filter, productStyle1, productStyle2, productStyle3);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProductStyleFilterBusinessObject();
		}

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		FilterStripAsserter<WhsProductStyle> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsProductStyle>(Factory, s => $"Product Style {s.WST_Code}"));
		FilterStripAsserter<WhsProductStyle> asserter;

		#endregion
	}
}
