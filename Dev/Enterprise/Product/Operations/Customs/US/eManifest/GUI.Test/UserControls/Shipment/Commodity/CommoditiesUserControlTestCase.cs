using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class CommoditiesUserControlTestCase : TestCaseWithFactory
	{
		public void TestCommodityFieldsVisibility()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			shipment1.Commodities.AddNew();
			shipment1.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			shipment2.Commodities.AddNew();
			var shipment3 = trip.Shipments.AddNew();
			shipment3.Commodities.AddNew();
			shipment3.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			var shipment4 = trip.Shipments.AddNew();
			shipment4.Commodities.AddNew();
			shipment4.B0_ShipmentType = ShipmentTypes.Codes.LowValue;
			var shipment5 = trip.Shipments.AddNew();
			shipment5.Commodities.AddNew();
			shipment5.B0_ShipmentType = ShipmentTypes.Codes.LowValue;
			shipment5.Commodities.DeleteAll();
			using (var form = new ManifestForm(trip))
			{
				form.Show();
				var tabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(form, "TabControl");
				tabControl.SelectTab("ShipmentsTabPage");
				Application.DoEvents();
				var shipmentsTabPage = tabControl.SelectedTab;
				var shipmentsGrid = (ZGrid)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentsGrid");
				var shipmentTabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentTabControl");
				var commoditiesTabPage = (ZTabPage)ManifestUserControlTestCase.FindControl(shipmentTabControl, "CommoditiesTabPage");
				shipmentTabControl.SelectTab(commoditiesTabPage);
				Application.DoEvents();
				AssertColumnsAndFieldsVisibility(commoditiesTabPage, false, false, false); //PAPS
				shipmentsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				AssertColumnsAndFieldsVisibility(commoditiesTabPage, true, false, false); //BRASS
				shipmentsGrid.CurrentCell = new DataGridCell(2, 0);
				Application.DoEvents();
				AssertColumnsAndFieldsVisibility(commoditiesTabPage, false, true, false); //Inbond
				shipmentsGrid.CurrentCell = new DataGridCell(3, 0);
				Application.DoEvents();
				AssertColumnsAndFieldsVisibility(commoditiesTabPage, false, true, true); //LowValue
				shipmentsGrid.CurrentCell = new DataGridCell(4, 0);
				Application.DoEvents();
				AssertColumnsVisibility(commoditiesTabPage, false, true, true); //Columns not changed for LowValue without commodity
				AssertFieldsVisibility(commoditiesTabPage, false, false); //LowValue without commodity
				shipment5.Commodities.AddNew();
				Application.DoEvents();
				AssertColumnsAndFieldsVisibility(commoditiesTabPage, false, true, true); //LowValue commodity added
				shipment5.B0_ShipmentType = ShipmentTypes.Codes.GoodsAstray;
				Application.DoEvents();
				AssertColumnsAndFieldsVisibility(commoditiesTabPage, false, true, false); //Shipment type changed to GoodsAstray
				shipment5.B0_ShipmentType = ShipmentTypes.Codes.FreeOfDuty;
				Application.DoEvents();
				AssertColumnsAndFieldsVisibility(commoditiesTabPage, false, false, false); //Shipment type changed to FreeOfDuty
			}
		}

		static void AssertColumnsAndFieldsVisibility(ZTabPage commoditiesTabPage, bool c4Codes, bool customsValue, bool countryOfOrigin)
		{
			AssertColumnsVisibility(commoditiesTabPage, c4Codes, customsValue, countryOfOrigin);
			AssertFieldsVisibility(commoditiesTabPage, customsValue, countryOfOrigin);
		}

		static void AssertColumnsVisibility(ZTabPage commoditiesTabPage, bool c4Codes, bool customsValue, bool countryOfOrigin)
		{
			var commoditiesGrid = (ZGrid)ManifestUserControlTestCase.FindControl(commoditiesTabPage, "CommoditiesGrid");
			AssertEquals("Is column BY_C4Codes visible", c4Codes, commoditiesGrid.Columns[Commodity.Schema.BY_C4Codes].IsVisible);
			AssertEquals("Is column BY_RN_NKCountryOfOrigin visible", countryOfOrigin, commoditiesGrid.Columns[Commodity.Schema.BY_RN_NKCountryOfOrigin].IsVisible);
		}

		static void AssertFieldsVisibility(ZTabPage commoditiesTabPage, bool customsValue, bool countryOfOrigin)
		{
			var countryOfOriginCodeDropEdit = ManifestUserControlTestCase.FindControl(commoditiesTabPage, "CountryOfOriginCodeFindBox");
			AssertEquals("Is CountryOfOriginCodeFindBox visible", countryOfOrigin, countryOfOriginCodeDropEdit.Visible);
		}
	}
}
