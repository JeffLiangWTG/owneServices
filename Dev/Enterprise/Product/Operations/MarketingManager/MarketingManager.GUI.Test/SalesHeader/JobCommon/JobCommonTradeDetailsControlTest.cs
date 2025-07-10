using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(JobCommonTradeDetailsControl))]
	class JobCommonTradeDetailsControlTest : TradeDetailsControlBaseTest
	{
		public void TestCreateDefaultElementWhenEmpty()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			salesProduct.MP_Code = "XXX";
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = salesProduct.PK;
			var entitySales = EntitySalesWrapper.Get(sales, org);

			using (var form = new ZForm(entitySales))
			using (var control = new JobCommonTradeDetailsControl(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				var innerControl = (IJobCommonTradeDetailsInnerControl)control.Controls[0];
				var groupingCollection = (OrgTradeDetailJobCommonGroupingCollection)((ZUserControl)innerControl).BindingSource.DataSource;
				AssertEquals(1, groupingCollection.Count);
			}

			entitySales.EntityTradeDetailsCollection.RemoveAndDeleteAll();
			entitySales.EntityTradeDetailsCollection.AddNew().PA_TradeMode = "AAA";
			entitySales.EntityTradeDetailsCollection.AddNew().PA_TradeMode = "AAA";
			using (var form = new ZForm(entitySales))
			using (var control = new JobCommonTradeDetailsControl(salesProduct))
			{
				form.Controls.Add(control);
				form.Show();

				var innerControl = (IJobCommonTradeDetailsInnerControl)control.Controls[0];
				var groupingCollection = (OrgTradeDetailJobCommonGroupingCollection)((ZUserControl)innerControl).BindingSource.DataSource;
				AssertEquals(1, groupingCollection.Count);
			}

			entitySales.EntityTradeDetailsCollection.RemoveAndDeleteAll();
			entitySales.EntityDetailGroupings.RemoveAndDeleteAll();
			using (var form = new ZForm(entitySales))
			using (var control = new JobCommonTradeDetailsControl(salesProduct))
			{
				form.Controls.Add(control);
				control.SetReadOnlyIncludingChildren(true);
				form.Show();

				var innerControl = (IJobCommonTradeDetailsInnerControl)control.Controls[0];
				var groupingCollection = (OrgTradeDetailJobCommonGroupingCollection)((ZUserControl)innerControl).BindingSource.DataSource;
				AssertEquals(0, groupingCollection.Count);
				Assert(innerControl.ReadOnly);
			}
		}

		public void TestPersistInnerControlCheckBoxReadOnlyWhenChangingDataItem()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			salesProduct.MP_Code = "XXX";

			var org = Factory.New<OrgHeader>();
			var sales1 = org.SalesCollection.AddNew();
			sales1.OW_MP_Product = salesProduct.PK;
			sales1.TradeDetails.AddNew();
			var sales2 = org.SalesCollection.AddNew();
			sales2.OW_MP_Product = salesProduct.PK;
			sales2.TradeDetails.AddNew();
			var salesHeader = new SalesHeader(org, salesProduct);

			using (var form = new ZForm(salesHeader))
			using (var grid = new ZGrid())
			using (var control = new JobCommonTradeDetailsControl(salesProduct))
			{
				var bindingSource = ((ICompositeControlBindingSourceProvider)form).BindingSource;
				bindingSource.SetBindingMember(grid, "EntitySalesCollectionProductView");
				bindingSource.SetBindingMember(control, "EntitySalesCollectionProductView");

				ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
				zGuidFindBoxColumnStyleInfo1.ColumnName = "OW_OriginID";
				grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);

				form.Controls.Add(grid);
				form.Controls.Add(control);
				form.Show();

				control.SetReadOnlyIncludingChildren(true);
				var innerControl = (ZUserControl)control.Controls[0];
				var checkboxOnInnerControl = new ZCheckBox();
				innerControl.BindingSource.SetBindingMember(checkboxOnInnerControl, "Elements.ProspectDetail.IsPeriodOfActivityOverridden");
				innerControl.Controls.Add(checkboxOnInnerControl);

				grid.ListManager.Position = 0;
				AssertEquals("Precondition", true, innerControl.GetReadOnly());
				AssertEquals("Precondition", true, checkboxOnInnerControl.ReadOnly);

				grid.ListManager.Position = 1;
				AssertEquals(true, innerControl.GetReadOnly());
				AssertEquals(true, checkboxOnInnerControl.ReadOnly);
			}
		}

		public void TestGridHeadersAreVisibleWhenParentIsNull()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();

			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = shipments.PK;
			opp.AssociatedTradeLanesPivots.AddPivotFor(sales);

			var salesWrapper = EntitySalesWrapper.Get(sales, opp);
			salesWrapper.EntityTradeDetailsCollection.AddNew();
			Factory.Save();

			var header = new SalesHeader(org, opp, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLaneWithDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.Focus();

				var tradeDetailsGrid = control.TradeDetailsControl.TradeDetailsGrid;
				var tradeLanesGrid = control.TradeLanesGrid;
				var detailsControl = control.TradeDetailsControl as JobCommonTradeDetailsControl;
				var innerControl = detailsControl.Controls[0];
				var groupingGrid = innerControl.Controls.Find("groupingGrid", true).Single();
				var associatedEntitiesGridControl = detailsControl.Controls.Find("associatedEntitiesGrid", true).Single() as SalesAssociatedEntitiesGridControl;
				var associatedEntitiesGrid = associatedEntitiesGridControl.Controls.Find("grid", true).Single();

				AssertNotNull(groupingGrid);
				AssertNotNull(associatedEntitiesGrid);

				AssertGrid((ZGrid)groupingGrid, 2, 2, false);
				AssertGrid(tradeDetailsGrid, 26, 18, false);
				AssertGrid((ZGrid)associatedEntitiesGrid, 7, 5, true);

				AssertEquals(1, tradeLanesGrid.ListManager.Count);
				AssertEquals(1, tradeDetailsGrid.ListManager.Count);
				AssertEquals(1, salesWrapper.EntityTradeDetailsCollection.Count);

				control.TradeLanesGrid.Select(0);
				AssertEquals(1, tradeLanesGrid.SelectedElements.Length);
				AssertEquals(1, tradeLanesGrid.SelectedRowCount);

				tradeLanesGrid.ContextMenu.ShowPopupMenu();
				tradeLanesGrid.DeleteMenuItem.PerformClick();

				AssertEquals(0, tradeLanesGrid.ListManager.Count);
				AssertEquals(0, salesWrapper.EntityTradeDetailsCollection.Count);

				AssertGrid((ZGrid)groupingGrid, 2, 2, true);
				AssertGrid(tradeDetailsGrid, 26, 18, true);
				AssertGrid((ZGrid)associatedEntitiesGrid, 7, 5, true);
				AssertEquals(0, tradeDetailsGrid.ListManager.Count);
			}

			void AssertGrid(ZGrid grid, int numColumns, int numColumnsVisible, bool isReadOnly)
			{
				AssertEquals(numColumns, grid.Columns.Count);
				AssertEquals(numColumnsVisible, grid.Columns.Count(c => c.IsVisible));
				AssertEquals(isReadOnly, grid.ReadOnly);
			}
		}

		public void TestGridsAreReadonlyWhenParentIsNull()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();

			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = shipments.PK;
			opp.AssociatedTradeLanesPivots.AddPivotFor(sales);

			var salesWrapper = EntitySalesWrapper.Get(sales, opp);
			salesWrapper.EntityTradeDetailsCollection.AddNew();
			Factory.Save();

			var header = new SalesHeader(org, opp, shipments);
			using (var form = new ZForm(header))
			using (var control = new TradeLaneWithDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.Focus();

				var tradeDetailsGrid = control.TradeDetailsControl.TradeDetailsGrid;
				var tradeLanesGrid = control.TradeLanesGrid;
				var detailsControl = control.TradeDetailsControl as JobCommonTradeDetailsControl;
				var innerControl = detailsControl.Controls[0];
				var groupingGrid = innerControl.Controls.Find("groupingGrid", true).Single() as ZGrid;
				var associatedEntitiesGridControl = detailsControl.Controls.Find("associatedEntitiesGrid", true).Single() as SalesAssociatedEntitiesGridControl;
				var associatedEntitiesGrid = associatedEntitiesGridControl.Controls.Find("grid", true).Single() as ZGrid;

				AssertNotNull(groupingGrid);
				AssertNotNull(associatedEntitiesGrid);
				AssertEquals(1, tradeLanesGrid.ListManager.Count);

				control.TradeLanesGrid.Select(0);
				AssertEquals(1, tradeDetailsGrid.ListManager.Count);
				AssertEquals(1, salesWrapper.EntityTradeDetailsCollection.Count);
				AssertReadOnly(false, tradeDetailsGrid, groupingGrid, associatedEntitiesGrid);

				tradeLanesGrid.ContextMenu.ShowPopupMenu();
				tradeLanesGrid.DeleteMenuItem.PerformClick();

				AssertEquals(0, tradeLanesGrid.ListManager.Count);
				AssertEquals(0, salesWrapper.EntityTradeDetailsCollection.Count);
				AssertReadOnly(true, tradeDetailsGrid, groupingGrid, associatedEntitiesGrid);
			}

			var sales2 = org.SalesCollection.AddNew();
			var salesWrapper2 = EntitySalesWrapper.Get(sales2, opp);
			salesWrapper2.EntityTradeDetailsCollection.AddNew();
			Factory.Save();

			var header2 = new SalesHeader(org, opp, shipments);
			using (var form = new ZForm(header2))
			using (var control = new TradeLaneWithDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.Focus();

				var tradeDetailsGrid = control.TradeDetailsControl.TradeDetailsGrid;
				var tradeLanesGrid = control.TradeLanesGrid;
				var detailsControl = control.TradeDetailsControl as JobCommonTradeDetailsControl;
				var innerControl = detailsControl.Controls[0];
				var groupingGrid = innerControl.Controls.Find("groupingGrid", true).Single() as ZGrid;
				var associatedEntitiesGridControl = detailsControl.Controls.Find("associatedEntitiesGrid", true).Single() as SalesAssociatedEntitiesGridControl;
				var associatedEntitiesGrid = associatedEntitiesGridControl.Controls.Find("grid", true).Single() as ZGrid;

				AssertNotNull(groupingGrid);
				AssertNotNull(associatedEntitiesGrid);

				AssertEquals(1, tradeLanesGrid.ListManager.Count);
				AssertEquals(0, tradeDetailsGrid.ListManager.Count);
				AssertEquals(1, salesWrapper2.EntityTradeDetailsCollection.Count);

				Assert(tradeDetailsGrid.ReadOnly);
				Assert(!groupingGrid.ReadOnly);
				Assert(associatedEntitiesGrid.ReadOnly);
			}

			void AssertReadOnly(bool expected, ZGrid detailsGrid, ZGrid groupingGrid, ZGrid entitiesGrid)
			{
				AssertEquals(expected, detailsGrid.ReadOnly);
				AssertEquals(expected, groupingGrid.ReadOnly);
				AssertEquals(true, entitiesGrid.ReadOnly);
			}
		}

		protected override TradeDetailsControl GetNewControlForTest()
		{
			var product = Factory.New<OrgSalesProduct>();
			return new JobCommonTradeDetailsControl(product);
		}
	}
}
