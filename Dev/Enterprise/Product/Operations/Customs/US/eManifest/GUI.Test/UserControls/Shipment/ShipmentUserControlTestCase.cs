using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class ShipmentUserControlTestCase : TestCaseWithFactory
	{
		public void TestInBondDataBindingAndVisibility()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment2.InBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			var shipment3 = trip.Shipments.AddNew();
			shipment3.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment3.InBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateTransportation;
			using (var form = new ManifestForm(trip))
			{
				form.Show();
				var tabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(form, "TabControl");
				tabControl.SelectTab("ShipmentsTabPage");
				Application.DoEvents();
				var shipmentsTabPage = tabControl.SelectedTab;
				var shipmentsGrid = (ZGrid)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentsGrid");
				var shipmentTabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentTabControl");
				var inBondTabPage = (ZTabPage)ManifestUserControlTestCase.FindControl(shipmentTabControl, "InBondTabPage");
				AssertEquals("InBondTabPage should NOT be visible for PAPS", false, inBondTabPage.TabVisible);
				shipmentsGrid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();
				AssertEquals("InBondTabPage should be visible for InBond", true, inBondTabPage.TabVisible);
				shipmentTabControl.SelectTab(inBondTabPage);
				Application.DoEvents();
				AssertInBondDetails(inBondTabPage, InbondTypes.Codes.ImmediateExportation, false, true);
				shipmentsGrid.CurrentCell = new DataGridCell(2, 0);
				Application.DoEvents();
				AssertInBondDetails(inBondTabPage, InbondTypes.Codes.ImmediateTransportation, false, false);
				shipmentsGrid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();
				AssertEquals("InBondTabPage should NOT be visible when back to PAPS shipment", false, inBondTabPage.TabVisible);
				shipment1.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
				Application.DoEvents();
				AssertEquals("InBondTabPage should be visible as shipment type changed to InBond", true, inBondTabPage.TabVisible);
				shipmentTabControl.SelectTab(inBondTabPage);
				shipment1.InBond.BM_InBondEntryType = "XX";
				Application.DoEvents();
				AssertInBondDetails(inBondTabPage, "XX", true, false);
				shipment1.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
				Application.DoEvents();
				shipment1.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
				shipmentTabControl.SelectTab(inBondTabPage);
				Application.DoEvents();
				AssertInBondDetails(inBondTabPage, "XX", true, false);
				shipment1.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
				Application.DoEvents();
				Factory.Save();
				Application.DoEvents();
				shipment1.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
				shipmentTabControl.SelectTab(inBondTabPage);
				Application.DoEvents();
				AssertInBondDetails(inBondTabPage, string.Empty, false, false);
				shipment1.InBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
				Application.DoEvents();
				AssertInBondDetails(inBondTabPage, InbondTypes.Codes.ImmediateExportation, false, true);
			}
		}

		static void AssertInBondDetails(ZTabPage inBondTabPage, string type, bool typeHasNotifications, bool exportsVisible)
		{
			var inBondEntryTypeDropEdit = (ZDropEdit)ManifestUserControlTestCase.FindControl(inBondTabPage, "InBondEntryTypeDropEdit");
			var exportInBondDetailsGroupBox = ManifestUserControlTestCase.FindControl(inBondTabPage, "ExportInBondDetailsGroupBox");
			AssertEquals("InBondEntryTypeDropEdit.Text", type, inBondEntryTypeDropEdit.Text);
			AssertEquals("InBondEntryTypeDropEdit.HasNotifications", typeHasNotifications, inBondEntryTypeDropEdit.Extensions.Get<NotificationExtension>().Notifications.Any());
			AssertEquals("ExportInBondDetailsGroupBox.Visible", exportsVisible, exportInBondDetailsGroupBox.Visible);
		}
	}
}
