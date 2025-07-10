using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class EstimateSalesAnalysisTradeLaneGridColourSchemeManagerTest : TestCaseWithFactory
	{
		public void TestFilterBusinessObject()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, product);
			var sales1 = salesHeader.FilterableEntitySalesCollection.AddNew();
			var sales2 = salesHeader.FilterableEntitySalesCollection.AddNew();

			using (var form = new ZForm(salesHeader))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo() { ColumnName = OrgSales.Schema.OW_OriginID });
				form.Controls.Add(grid);

				grid.SetDataBinding(salesHeader, "FilterableEntitySalesCollection");
				form.Show();

				var manager = new EstimateSalesAnalysisTradeLaneGridColourSchemeManagerForTest(grid, salesHeader.FilterableEntitySalesCollection);
				AssertContainsExactElementsInAnyOrder(new[] { sales1, sales2 }, manager.FilterBusinessObject_Exposed.AllProspectSales);

				var sales3 = salesHeader.FilterableEntitySalesCollection.AddNew();
				AssertContainsExactElementsInAnyOrder(new[] { sales1, sales2, sales3 }, manager.FilterBusinessObject_Exposed.AllProspectSales);
			}
		}

		class EstimateSalesAnalysisTradeLaneGridColourSchemeManagerForTest : EstimateSalesAnalysisTradeLaneGridColourSchemeManager
		{
			public EstimateSalesAnalysisTradeLaneGridColourSchemeManagerForTest(ZGrid grid, EntitySalesWrapperCollectionFilterableView prospectSalesFilterableView)
				: base(grid, prospectSalesFilterableView)
			{
			}

			public OrgSalesFilterBusinessObjectWithActuals FilterBusinessObject_Exposed
			{
				get { return FilterBusinessObject; }
			}
		}
	}
}
