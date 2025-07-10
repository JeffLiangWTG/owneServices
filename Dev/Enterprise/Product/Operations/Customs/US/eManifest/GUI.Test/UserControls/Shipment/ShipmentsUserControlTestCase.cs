using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class ShipmentsUserControlTestCase : TestCaseWithFactory
	{
		public void TestInterestedShipmentPropertyChanges_PropagatedTo_SingleCommodity()
		{
			var trip = Factory.New<Trip>();
			using (var form = new ManifestForm(trip))
			{
				form.Show();
				var tabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(form, "TabControl");
				tabControl.SelectTab("ShipmentsTabPage");
				var shipmentsTabPage = tabControl.SelectedTab;
				var shipmentsGrid = (ZGrid)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentsGrid");
				Application.DoEvents();
				var shipment = trip.Shipments.AddNew();
				shipment.B0_ManifestQty = 2;
				shipment.B0_ManifestUQ = Constants.PkgUnit.Piece;
				shipment.B0_Weight = 2;
				shipment.B0_WeightUQ = Constants.Weight.Kilograms;
				shipment.B0_DescriptionOfCargo = "Test DescriptionOfCargo";
				shipment.B0_RN_NKCountryOfExport = "AU";
				shipmentsGrid.Select(0);
				var commodity = shipment.Commodities[0];
				AssertEquals("Shipment contains single commodity", 1, shipment.Commodities.Count);
				AssertEquals("commodity.BY_PieceCount default to Shipment.B0_ManifestQty", shipment.B0_ManifestQty, commodity.BY_PieceCount);
				AssertEquals("commodity.BY_ManifestUnitCode default to Shipment.B0_ManifestUQ", shipment.B0_ManifestUQ, commodity.BY_ManifestUnitCode);
				AssertEquals("commodity.BY_GrossWeight default to Shipment.B0_Weight", shipment.B0_Weight, commodity.BY_GrossWeight);
				AssertEquals("commodity.BY_GrossWeightUnit default to Shipment.B0_WeightUQ", shipment.B0_WeightUQ, commodity.BY_GrossWeightUnit);
				AssertEquals("commodity.BY_Description default to Shipment.B0_DescriptionOfCargo", shipment.B0_DescriptionOfCargo, commodity.BY_Description);
				AssertEquals("commodity.BY_RN_NKCountryOfOrigin default to Shipment.B0_RN_NKCountryOfExport", shipment.B0_RN_NKCountryOfExport, commodity.BY_RN_NKCountryOfOrigin);
				shipment.B0_ManifestQty *= 2;
				shipment.B0_ManifestUQ = Constants.PkgUnit.BaleUncompressed;
				shipment.B0_Weight *= 2;
				shipment.B0_WeightUQ = Constants.Weight.Grams;
				shipment.B0_DescriptionOfCargo = "Test DescriptionOfCargo 2";
				shipment.B0_RN_NKCountryOfExport = "GB";
				AssertEquals("commodity.BY_PieceCount updated to Shipment.B0_ManifestQty", shipment.B0_ManifestQty, commodity.BY_PieceCount);
				AssertEquals("commodity.BY_ManifestUnitCode updated to Shipment.B0_ManifestUQ", shipment.B0_ManifestUQ, commodity.BY_ManifestUnitCode);
				AssertEquals("commodity.BY_GrossWeight updated to Shipment.B0_Weight", shipment.B0_Weight, commodity.BY_GrossWeight);
				AssertEquals("commodity.BY_GrossWeightUnit updated to Shipment.B0_WeightUQ", shipment.B0_WeightUQ, commodity.BY_GrossWeightUnit);
				AssertEquals("commodity.BY_Description updated to Shipment.B0_DescriptionOfCargo", shipment.B0_DescriptionOfCargo, commodity.BY_Description);
				AssertEquals("commodity.BY_RN_NKCountryOfOrigin default to Shipment.B0_RN_NKCountryOfExport", shipment.B0_RN_NKCountryOfExport, commodity.BY_RN_NKCountryOfOrigin);
			}
		}

		public void TestInterestedShipmentPropertyChanges_NotPropagatedTo_MultipleCommodities()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ManifestQty = 2;
			shipment.B0_ManifestUQ = Constants.PkgUnit.Piece;
			shipment.B0_Weight = 2;
			shipment.B0_WeightUQ = Constants.Weight.Kilograms;
			shipment.B0_RN_NKCountryOfExport = "AU";
			shipment.Commodities.AddNew();
			var commodity1 = shipment.Commodities[0];
			var commodity2 = shipment.Commodities.AddNew();
			Assert("Shipment contains more than 1 commodities", shipment.Commodities.Count > 1);
			AssertEquals("commodity.BY_PieceCount default to Shipment.B0_ManifestQty", shipment.B0_ManifestQty, commodity1.BY_PieceCount);
			AssertEquals("commodity.BY_ManifestUnitCode default to Shipment.B0_ManifestUQ", shipment.B0_ManifestUQ, commodity1.BY_ManifestUnitCode);
			AssertEquals("commodity.BY_GrossWeight default to Shipment.B0_Weight", shipment.B0_Weight, commodity1.BY_GrossWeight);
			AssertEquals("commodity.BY_GrossWeightUnit default to Shipment.B0_WeightUQ", shipment.B0_WeightUQ, commodity1.BY_GrossWeightUnit);
			AssertEquals("commodity.BY_Description default to Shipment.B0_DescriptionOfCargo", shipment.B0_DescriptionOfCargo, commodity1.BY_Description);
			AssertEquals("commodity.BY_RN_NKCountryOfOrigin default to Shipment.B0_RN_NKCountryOfExport", shipment.B0_RN_NKCountryOfExport, commodity1.BY_RN_NKCountryOfOrigin);
			AssertNotEquals("except for the first commodity, rest new commidity.BY_PieceCount not default to Shipment.B0_ManifestQty", shipment.B0_ManifestQty, commodity2.BY_PieceCount);
			AssertEquals("commodity.BY_ManifestUnitCode default to Shipment.B0_ManifestUQ", shipment.B0_ManifestUQ, commodity2.BY_ManifestUnitCode);
			AssertNotEquals("except for the first commodity, rest new commodity.BY_GrossWeight default to Shipment.B0_Weight", shipment.B0_Weight, commodity2.BY_GrossWeight);
			AssertEquals("commodity.BY_GrossWeightUnit default to Shipment.B0_WeightUQ", shipment.B0_WeightUQ, commodity2.BY_GrossWeightUnit);
			AssertEquals("commodity.BY_Description default to Shipment.B0_DescriptionOfCargo", shipment.B0_DescriptionOfCargo, commodity2.BY_Description);
			AssertEquals("commodity.BY_RN_NKCountryOfOrigin default to Shipment.B0_RN_NKCountryOfExport", shipment.B0_RN_NKCountryOfExport, commodity2.BY_RN_NKCountryOfOrigin);
			shipment.B0_ManifestQty *= 2;
			shipment.B0_ManifestUQ = Constants.PkgUnit.BaleUncompressed;
			shipment.B0_Weight *= 2;
			shipment.B0_WeightUQ = Constants.Weight.Grams;
			shipment.B0_DescriptionOfCargo = "Test DescriptionOfCargo 2";
			shipment.B0_RN_NKCountryOfExport = "GB";
			AssertNotEquals("commodity.BY_PieceCount not changed with Shipment.B0_ManifestQty", shipment.B0_ManifestQty, commodity1.BY_PieceCount);
			AssertNotEquals("commodity.BY_ManifestUnitCode not changed with Shipment.B0_ManifestUQ", shipment.B0_ManifestUQ, commodity1.BY_ManifestUnitCode);
			AssertNotEquals("commodity.BY_GrossWeight not changed with Shipment.B0_Weight", shipment.B0_Weight, commodity1.BY_GrossWeight);
			AssertNotEquals("commodity.BY_GrossWeightUnit not changed with Shipment.B0_WeightUQ", shipment.B0_WeightUQ, commodity1.BY_GrossWeightUnit);
			AssertNotEquals("commodity.BY_Description not changed with Shipment.B0_DescriptionOfCargo", shipment.B0_DescriptionOfCargo, commodity1.BY_Description);
			AssertNotEquals("commodity.BY_RN_NKCountryOfOrigin not changed with Shipment.B0_RN_NKCountryOfExport", shipment.B0_RN_NKCountryOfExport, commodity1.BY_RN_NKCountryOfOrigin);
			AssertNotEquals("commodity.BY_PieceCount not changed with Shipment.B0_ManifestQty", shipment.B0_ManifestQty, commodity2.BY_PieceCount);
			AssertNotEquals("commodity.BY_ManifestUnitCode not changed with Shipment.B0_ManifestUQ", shipment.B0_ManifestUQ, commodity2.BY_ManifestUnitCode);
			AssertNotEquals("commodity.BY_GrossWeight not changed with Shipment.B0_Weight", shipment.B0_Weight, commodity2.BY_GrossWeight);
			AssertNotEquals("commodity.BY_GrossWeightUnit not changed with Shipment.B0_WeightUQ", shipment.B0_WeightUQ, commodity2.BY_GrossWeightUnit);
			AssertNotEquals("commodity.BY_Description not changed with Shipment.B0_DescriptionOfCargo", shipment.B0_DescriptionOfCargo, commodity2.BY_Description);
			AssertNotEquals("commodity.BY_RN_NKCountryOfOrigin not changed with Shipment.B0_RN_NKCountryOfExport", shipment.B0_RN_NKCountryOfExport, commodity2.BY_RN_NKCountryOfOrigin);
		}

		public void TestTransferContextMenu()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			using (var form = new ManifestForm(trip))
			{
				form.Show();
				var tabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(form, "TabControl");
				tabControl.SelectTab("ShipmentsTabPage");
				Application.DoEvents();
				var shipmentsTabPage = tabControl.SelectedTab;
				var shipmentsGrid = (ZGrid)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentsGrid");
				var transferMenuItem = shipmentsGrid.ContextMenu.MenuItems[0];
				AssertEquals("Transfer Shipments From Other Trips", transferMenuItem.Text);
				trip.Factory.Save();
				transferMenuItem.PerformClick();
				using (var lastDialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(lastDialog);
					AssertEquals(typeof(MultiSelectModuleForm), lastDialog.GetType());
				}
			}
		}

		public void TestFirstCommodityColumn()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			using (var form = new ManifestForm(trip))
			{
				form.Show();
				var tabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(form, "TabControl");
				tabControl.SelectTab("ShipmentsTabPage");
				Application.DoEvents();
				var shipmentsGrid = tabControl.Controls.Find("ShipmentsGrid", true)[0] as ZGrid;
				AssertEquals("No. of columns", 70, shipmentsGrid.Columns.Count);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityPieceCount]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityManifestUnitCode]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityWeight]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityWeightUnit]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityDescription]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityEquipment]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityMarksAndNumbers]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityMonetaryValue]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityCountryOfOrigin]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityHarmonizedNumbers]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityHazardousGoodsIdentifier]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityHazardousGoodsContact]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityHazardousGoodsContactPhone]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityVehicleIdentificationNumbers]);
				AssertNotNull("Column is available", shipmentsGrid.Columns[Shipment.Schema.FirstCommodityC4Codes]);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityPieceCount).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityManifestUnitCode).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityWeight).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityWeightUnit).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityDescription).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityEquipment).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityMarksAndNumbers).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityMonetaryValue).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityCountryOfOrigin).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityHarmonizedNumbers).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityHazardousGoodsIdentifier).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityHazardousGoodsContact).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityHazardousGoodsContactPhone).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityVehicleIdentificationNumbers).IsVisible);
				Assert(!shipmentsGrid.GetColumnStyle(Shipment.Schema.FirstCommodityC4Codes).IsVisible);
			}
		}

		public void TestTransferContextMenuAfterSave()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			using (var form = new ManifestForm(trip))
			{
				form.Show();
				var tabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(form, "TabControl");
				tabControl.SelectTab("ShipmentsTabPage");
				Application.DoEvents();
				var shipmentsTabPage = tabControl.SelectedTab;
				var shipmentsGrid = (ZGrid)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentsGrid");
				var transferMenuItem = shipmentsGrid.ContextMenu.MenuItems[0];
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				transferMenuItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
