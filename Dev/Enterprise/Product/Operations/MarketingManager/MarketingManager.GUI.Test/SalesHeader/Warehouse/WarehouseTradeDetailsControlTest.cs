using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(WarehouseTradeDetailsControl))]
	class WarehouseTradeDetailsControlTest : TradeDetailsControlBaseTest
	{
		#region Service Change

		public void TestGridControlShouldChangeAccordingToCurrentService()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var receiptSale = EntitySalesWrapper.Get(Factory.New<OrgSales>(), org);
			receiptSale.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			var orderSale = EntitySalesWrapper.Get(Factory.New<OrgSales>(), org);
			orderSale.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Orders;
			var storageSale = EntitySalesWrapper.Get(Factory.New<OrgSales>(), org);
			storageSale.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var layout = product.FormLayout;

			var columnDef1 = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
			columnDef1.XC_Name = "CustomStr";
			columnDef1.XC_Type = AddOnColumnDataType.Codes.String;

			var columnDef2 = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
			columnDef2.XC_Name = "CustomInt";
			columnDef2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var gridColumn1 = layout.TradeDetailGridColumnDefinitions.AddNew();
			gridColumn1.GenCustomColumnDefinitionFk = columnDef1.PK;

			var gridColumn2 = layout.TradeDetailGridColumnDefinitions.AddNew();
			gridColumn2.GenCustomColumnDefinitionFk = columnDef2.PK;

			Factory.Save();

			using (var form = new ZForm(receiptSale))
			using (var control = new WarehouseTradeDetailsControlForTest(product))
			{
				form.Controls.Add(control);
				form.Show();
				AssertType(typeof(WarehouseReceiptsTradeDetailsGridControl), control.CurrentlyVisibleGridControl_Exposed);

				var resultColumns = control.TradeDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("Custom string column should be added", resultColumns.Any(c => c.Caption == "CustomStr"));
				Assert("Custom int column should be added", resultColumns.Any(c => c.Caption == "CustomInt"));

				control.SetDataBinding(orderSale, null);
				AssertType(typeof(WarehouseOrdersTradeDetailsGridControl), control.CurrentlyVisibleGridControl_Exposed);

				resultColumns = control.TradeDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("Custom string column should be added", resultColumns.Any(c => c.Caption == "CustomStr"));
				Assert("Custom int column should be added", resultColumns.Any(c => c.Caption == "CustomInt"));

				orderSale.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
				AssertType(typeof(WarehouseStorageTradeDetailsGridControl), control.CurrentlyVisibleGridControl_Exposed);

				resultColumns = control.TradeDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("Custom string column should be added", resultColumns.Any(c => c.Caption == "CustomStr"));
				Assert("Custom int column should be added", resultColumns.Any(c => c.Caption == "CustomInt"));

				var columnDef3 = layout.TradeDetailCustomColumnDefinitionCollection.AddNew();
				columnDef3.XC_Name = "DuplicateCustomStr";
				columnDef3.XC_Type = AddOnColumnDataType.Codes.String;
				var gridColumn3 = layout.TradeDetailGridColumnDefinitions.AddNew();
				gridColumn3.GenCustomColumnDefinitionFk = columnDef3.PK;

				Factory.Save();

				control.SetDataBinding(storageSale, null);
				resultColumns = control.TradeDetailsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals("Duplicate Custom string column should not be added", false, resultColumns.Any(c => c.Caption == "DuplicateCustomStr"));
			}
		}

		#endregion

		#region Overrides

		protected override TradeDetailsControl GetNewControlForTest()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			return new WarehouseTradeDetailsControl(product);
		}

		#endregion

		#region Non Grouping

		public void TestWarehouseTradeDetailsGrid_ShouldPopulateNewDetailWithOpportunityStatus()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.New<OrgHeader>();
			var opp1 = org.SalesOpportunities.AddNew();
			opp1.P8_Status = "WON";
			AssertEquals("Precondition", OpportunityTradeStatus.Codes.Successful, OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetTradeStatusFromCode(opp1.P8_Status));
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = warehouseProduct.PK;
			var entitySales = EntitySalesWrapper.Get(sales, opp1);

			using (var form = new TradeDetailsControlFormForTest(entitySales))
			using (var control = new WarehouseTradeDetailsControlForTest(warehouseProduct))
			{
				form.Controls.Add(control);
				control.SetDataBinding(entitySales, ZString.Empty);
				control.OnCurrentDataItemChangedExposed();
				form.Show();

				AssertEquals("One new detail should be added", 1, entitySales.EntityTradeDetailsCollection.Count);
				AssertEquals("This detail should adopt the status of the opportunity", OpportunityTradeStatus.Codes.Successful, entitySales.EntityTradeDetailsCollection[0].PA_Status);
			}
		}

		#endregion

		public void TestTopSplitContainer()
		{
			var warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);

			using (var form = new ZForm())
			using (var control = new WarehouseTradeDetailsControl(warehouseProduct))
			{
				form.Controls.Add(control);
				form.Show();

				Assert(control.topSplitContainer.IsSplitterFixed);
				AssertEquals(FixedPanel.Panel2, control.topSplitContainer.FixedPanel);
			}
		}

		class WarehouseTradeDetailsControlForTest : WarehouseTradeDetailsControl
		{
			public WarehouseTradeDetailsControlForTest(OrgSalesProduct salesProduct)
				: base(salesProduct)
			{
			}

			public void OnCurrentDataItemChangedExposed()
			{
				OnCurrentDataItemChanged(null);
			}

			public WarehouseTradeDetailsGridControl CurrentlyVisibleGridControl_Exposed
			{
				get { return base.CurrentlyVisibleGridControl; }
			}
		}
	}
}
