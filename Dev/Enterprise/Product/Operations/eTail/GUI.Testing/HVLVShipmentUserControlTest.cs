using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.Module;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class HVLVShipmentUserControlTest : TestCaseWithFactory
	{
		public void TestConsignmentUserControl_WhenInShipment_ThenDisplayOrderColumnRemoved()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				var shipmentUserControl = plugin.UserControl as HVLVShipmentUserControl;
				var consignmentGrid = shipmentUserControl.Controls.Find("consignmentsGrid", true).Single() as HVLVConsignmentModuleButtonGrid;
				var displayOrderColumn = consignmentGrid.InnerGrid.Columns["HVC_DisplayOrder"];
				AssertEquals("DisplayOrderColumn is removed", null, displayOrderColumn);
			}
		}

		public void TestConsignmentUserControl_WhenInShipment_WhenShowingForm_DoNotLoadConsignmentsIfCountIsLargerThanThreshold_AndGridIsReadOnly()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			Factory.Save();

			var newFactory = NewFactory();
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			ChildEditableService.SetState(newFactory, ChildEditableServiceStates.Shipment);

			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (var form = new ShipmentForm(shipmentInNewFactory))
			{
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.SelectTabPage();
				Assert(!shipmentInNewFactory.GetHVLVConsignmentHeader().ConsignmentsFilteredView.IsLoaded);
				AssertCollectionNotContains(ConsignmentsLoadedInFactory(newFactory), x => header.Consignments.Contains(x));
			}
		}

		public void TestConsignmentUserControl_WhenInShipment_WhenConsignmentsNotLoaded_LoadConsignmentsWhenCoverPanelLinkClicked()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			Factory.Save();

			var newFactory = NewFactory();
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

			ChildEditableService.SetState(newFactory, ChildEditableServiceStates.Shipment);
			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (var form = new ShipmentForm(shipmentInNewFactory))
			{
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.SelectTabPage();
				var coverPanelControl = form.Controls.Find("consignmentsCoverPanel", searchAllChildren: true).Single();
				AssertCollectionNotContains("Precondition: no consignments loaded", ConsignmentsLoadedInFactory(newFactory), x => header.Consignments.Contains(x));
				Assert("Precondition: CoverPanel is visible", coverPanelControl.Visible);

				var consignmentsCoverPanelLinkLabelControl = form.Controls.Find("consignmentsCoverPanelLinkLabel", searchAllChildren: true)[0] as ZLinkLabel;
				consignmentsCoverPanelLinkLabelControl.OnLinkClicked_Exposed(null);

				AssertCollectionContains("Consignments should load", ConsignmentsLoadedInFactory(newFactory), x => header.Consignments.Contains(x));
				Assert("CoverPanel should be hidden", !coverPanelControl.Visible);

				var consignmentsGrid = ((ZModuleButtonGrid)form.Controls.Find("consignmentsGrid", searchAllChildren: true).SingleOrDefault())?.InnerGrid;
				consignmentsGrid.SelectAllElements();
				AssertEquals("Consignments appear in search grid", 4, consignmentsGrid.GetSelectedElements<HVLVConsignment>().Length);
			}
		}

		public void TestConsignmentAttachAndDetachButtonIsVisible()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				var shipmentUserControl = plugin.UserControl as HVLVShipmentUserControl;
				var consignmentGrid = shipmentUserControl.Controls.Find("consignmentsGrid", true).Single() as HVLVConsignmentModuleButtonGrid;
				AssertEquals(true, consignmentGrid.AttachButtonForTest.Visible);
				AssertEquals(true, consignmentGrid.DetachButtonForTest.Visible);
			}
		}

		IEnumerable<HVLVConsignment> ConsignmentsLoadedInFactory(BusinessObjectFactory factory)
		{
			return ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects.OfType<HVLVConsignment>();
		}

		public void TestConsignmentUserControl_WhenMade_ShouldHaveCorrectNames()
		{
			var header = Factory.NewWithValidTestData<HVLVConsignmentHeader>();

			using (var form = new ShipmentUserControlTestForm(header))
			{
				AssertEquals("Total Items", (form.Controls.Find("lblItemCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Import Cleared", (form.Controls.Find("lblImportClearedCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Import Held", (form.Controls.Find("lblImportHeldCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Import None Reported", (form.Controls.Find("lblImportNoneReportedCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Export Cleared", (form.Controls.Find("lblExportClearedCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Export Held", (form.Controls.Find("lblExportHeldCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Export None Reported", (form.Controls.Find("lblExportNoneReportedCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Surplus", (form.Controls.Find("lblSurplusCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Short Shipped", (form.Controls.Find("lblShortCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Delivered", (form.Controls.Find("lblDeliveredCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Destination Scanned", (form.Controls.Find("lblScannedCount", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Scanned Cleared", (form.Controls.Find("lblScannedCleared", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Scanned Held", (form.Controls.Find("lblScannedHeld", true)[0] as ZLabel).CaptionResourceString.Caption);
				AssertEquals("Scanned None Reported", (form.Controls.Find("lblScannedNoneReported", true)[0] as ZLabel).CaptionResourceString.Caption);
			}
		}

		public void TestConsignmentUserControl_WhenInShipment_ConsignmentsCoverPanelIsHidden_WhenConsignmentsAreLoaded()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			Factory.Save();

			var newFactory = NewFactory();
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

			ChildEditableService.SetState(newFactory, ChildEditableServiceStates.Shipment);

			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			using (var form = new ShipmentForm(shipmentInNewFactory))
			{
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.SelectTabPage();
				var coverPanelControl = form.Controls.Find("consignmentsCoverPanel", searchAllChildren: true).Single();
				AssertCollectionContains("Precondition: consignments are loaded", ConsignmentsLoadedInFactory(newFactory), x => header.Consignments.Contains(x));

				Assert("Consignments cover panel is hidden", !coverPanelControl.Visible);
			}
		}

		public void TestConsignmentUserControl_WhenInShipment_ConsignmentsCoverPanelIsVisible_WhenConsignmentsAreNotLoaded()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			header.Consignments.AddNew();
			Factory.Save();

			var newFactory = NewFactory();
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

			ChildEditableService.SetState(newFactory, ChildEditableServiceStates.Shipment);

			using (HVLVDataRegistry.Instance.ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (var form = new ShipmentForm(shipmentInNewFactory))
			{
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
				plugin.SelectTabPage();
				var coverPanelControl = form.Controls.Find("consignmentsCoverPanel", searchAllChildren: true).Single();

				AssertCollectionNotContains("Precondition: no consignments loaded", ConsignmentsLoadedInFactory(newFactory), x => header.Consignments.Contains(x));
				Assert("When Consignments not loaded, Cover Panel should be visible", coverPanelControl.Visible);

				_ = shipmentInNewFactory.GetHVLVConsignmentHeader().Consignments;
				AssertCollectionContains("Precondition: consignments are loaded", ConsignmentsLoadedInFactory(newFactory), x => header.Consignments.Contains(x));

				Assert("When Consignments are loaded, Cover Panel should be hidden", !coverPanelControl.Visible);
			}
		}
	}
}
