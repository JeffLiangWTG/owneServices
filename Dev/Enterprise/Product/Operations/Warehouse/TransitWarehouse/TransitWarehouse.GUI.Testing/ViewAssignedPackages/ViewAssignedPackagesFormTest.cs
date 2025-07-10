using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(ViewAssignedPackagesForm))]
	class TransitWarehouseShowAssignedPackagesFormTest : ZFormBasherTest
	{
		public void TestCFSsAreShown()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse1 = Helper.CreateTRWWarehouse("T1");
			var transitWarehouse2 = Helper.CreateTRWWarehouse("T2");
			var nonTransitAddress1 = Helper.CreateClient("O1").MainAddress;
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, transitWarehouse1.WarehouseAddress.PK);
			parent.CFSAddressPksInOrder.Add(2, nonTransitAddress1.PK);
			parent.CFSAddressPksInOrder.Add(3, transitWarehouse2.WarehouseAddress.PK);
			parent.TransitWarehouseAddressPK = transitWarehouse1.WarehouseAddress.PK;
			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("CFSToolStripButtons", true).Single();
				AssertEquals(5, toolStrip.Items.Count);

				var buttons = toolStrip.Items.OfType<CFSDetailsToolStripButton>();
				AssertEquals(3, buttons.Count());
				var transitWarehouse1Button = buttons.Single(b => b.Details.WarehouseName == "T1");
				var nonTransitAddress1Button = buttons.Single(b => b.Details.WarehouseName == "O1");
				var transitWarehouse2Button = buttons.Single(b => b.Details.WarehouseName == "T2");
				AssertTileProperties(transitWarehouse1Button, true, "T1", "0", "0 KG", "0 M3");
				AssertTileProperties(nonTransitAddress1Button, false, "O1", "-", "-", "-");
				AssertTileProperties(transitWarehouse2Button, true, "T2", "0", "0 KG", "0 M3");
				AssertEquals(transitWarehouse1.WarehouseAddress.PK, planner.Warehouse.WarehouseAddress.PK);

				transitWarehouse2Button.PerformClick();
				AssertTileProperties(transitWarehouse1Button, true, "T1", "0", "0 KG", "0 M3");
				AssertTileProperties(nonTransitAddress1Button, false, "O1", "-", "-", "-");
				AssertTileProperties(transitWarehouse2Button, true, "T2", "0", "0 KG", "0 M3");
				AssertEquals(transitWarehouse2.WarehouseAddress.PK, planner.Warehouse.WarehouseAddress.PK);

				nonTransitAddress1Button.PerformClick();
				AssertTileProperties(transitWarehouse1Button, true, "T1", "0", "0 KG", "0 M3");
				AssertTileProperties(nonTransitAddress1Button, false, "O1", "-", "-", "-");
				AssertTileProperties(transitWarehouse2Button, true, "T2", "0", "0 KG", "0 M3");
				AssertEquals(transitWarehouse2.WarehouseAddress.PK, planner.Warehouse.WarehouseAddress.PK);
			}
		}

		void AssertTileProperties(CFSDetailsToolStripButton toolStripButton, bool isEnabled,
			string expectedWarehouseName, string expectedPackageCount, string expectedWeight, string expectedVolume)
		{
			var tile = (CFSTile)toolStripButton.Control;
			var warehouseNameLabel = tile.FindAll<ZLabel>(l => l.Name == "WarehouseName").Single();
			var packageCountLabel = tile.FindAll<ZLabel>(l => l.Name == "NumberOfPackages").Single();
			var weightLabel = tile.FindAll<ZLabel>(l => l.Name == "Weight").Single();
			var volumeLabel = tile.FindAll<ZLabel>(l => l.Name == "Volume").Single();
			AssertEquals(isEnabled, tile.Enabled);
			AssertEquals(expectedWarehouseName, warehouseNameLabel.Text);
			AssertEquals(expectedPackageCount, packageCountLabel.Text);
			AssertEquals(expectedWeight, weightLabel.Text);
			AssertEquals(expectedVolume, volumeLabel.Text);
		}

		public void TestCFSsAreShown_NoCFSAddresses()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			AssertEquals(0, ((ITransitWarehouseParent)parent).GetOrderedCFSOrgAddressPKs().Count);

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("CFSToolStripButtons", true).Single();
				AssertEquals(0, toolStrip.Items.Count);
			}
		}

		public void TestCFSsAreShown_CFSAddressesWithInValidOrgAddress()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, ZGuid.NewZGuid());
			parent.CFSAddressPksInOrder.Add(2, ZGuid.NewZGuid());
			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("CFSToolStripButtons", true).Single();
				AssertEquals(0, toolStrip.Items.Count);
			}
		}

		public void TestCFSsAreShown_CFSAddressesWithValidOrgAddressButInvalidTransitWarehouses()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, Factory.NewWithValidTestData<OrgAddress>().PK);
			parent.CFSAddressPksInOrder.Add(2, Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK);
			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("CFSToolStripButtons", true).Single();
				var buttons = toolStrip.Items.OfType<CFSDetailsToolStripButton>();
				AssertEquals(2, buttons.Count());
			}
		}

		public void TestCFSsAreShown_FirstCFSTileIsNotTransitWarehouse()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse1 = Helper.CreateTRWWarehouse("T1");
			var nonTransitAddress1 = Helper.CreateClient("O1").MainAddress;
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, nonTransitAddress1.PK);
			parent.CFSAddressPksInOrder.Add(2, transitWarehouse1.WarehouseAddress.PK);
			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("CFSToolStripButtons", true).Single();
				var buttons = toolStrip.Items.OfType<CFSDetailsToolStripButton>();
				AssertEquals(2, buttons.Count());
				var transitWarehouse1Button = buttons.Single(b => b.Details.WarehouseName == "T1");
				var nonTransitAddress1Button = buttons.Single(b => b.Details.WarehouseName == "O1");
				AssertTileProperties(transitWarehouse1Button, true, "T1", "0", "0 KG", "0 M3");
				AssertTileProperties(nonTransitAddress1Button, false, "O1", "-", "-", "-");
				AssertEquals(transitWarehouse1.WarehouseAddress.PK, planner.Warehouse.WarehouseAddress.PK);

				nonTransitAddress1Button.PerformClick();
				AssertTileProperties(transitWarehouse1Button, true, "T1", "0", "0 KG", "0 M3");
				AssertTileProperties(nonTransitAddress1Button, false, "O1", "-", "-", "-");
				AssertEquals(transitWarehouse1.WarehouseAddress.PK, planner.Warehouse.WarehouseAddress.PK);
			}
		}

		public void TestCFSsAreShown_OnlyNonTransitAddresses()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var nonTransitAddress1 = Helper.CreateClient("O1").MainAddress;
			var nonTransitAddress2 = Helper.CreateClient("O2").MainAddress;
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, nonTransitAddress1.PK);
			parent.CFSAddressPksInOrder.Add(2, nonTransitAddress2.PK);
			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("CFSToolStripButtons", true).Single();
				var buttons = toolStrip.Items.OfType<CFSDetailsToolStripButton>();
				AssertEquals(2, buttons.Count());
				var nonTransitWarehouseButton1 = buttons.Single(b => b.Details.WarehouseName == "O1");
				var nonTransitWarehouseButton2 = buttons.Single(b => b.Details.WarehouseName == "O2");
				AssertTileProperties(nonTransitWarehouseButton1, false, "O1", "-", "-", "-");
				AssertTileProperties(nonTransitWarehouseButton2, false, "O2", "-", "-", "-");
				AssertNull(planner.Warehouse);

				nonTransitWarehouseButton2.PerformClick();
				AssertTileProperties(nonTransitWarehouseButton1, false, "O1", "-", "-", "-");
				AssertTileProperties(nonTransitWarehouseButton2, false, "O2", "-", "-", "-");
				AssertNull(planner.Warehouse);
			}
		}

		public void TestPackageDetailsAreCollapsed_WhenNoPackages()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("T1");
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, transitWarehouse.WarehouseAddress.PK);
			parent.TransitWarehouseAddressPK = transitWarehouse.WarehouseAddress.PK;
			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var packLineFilterStripControl = (ZFilterStripCommonControl)viewAssignedPackagesForm.Controls.Find("LinesGrid", true).Single();
				AssertEquals(0, packLineFilterStripControl.GridCollection.Count);

				var packageDetailsControl = (TabControl)viewAssignedPackagesForm.Controls.Find("DetailsTabControl", true).Single();
				var splitContainer1 = (KSplitContainer)viewAssignedPackagesForm.Controls.Find("splitContainer1", true).Single();

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("ShowPackageDetailToolStrip", true).Single();
				var arrowBtn = toolStrip.Items.Find("PackageDetailArrowBtn", true).Single();

				AssertEquals(false, packageDetailsControl.Visible);
				AssertEquals(true, splitContainer1.Panel2Collapsed);
				AssertEquals("5", arrowBtn.Text);

				arrowBtn.PerformClick();
				AssertEquals("6", arrowBtn.Text);
				AssertEquals(true, packageDetailsControl.Visible);
				AssertEquals(false, splitContainer1.Panel2Collapsed);
			}
		}

		public void TestPackageDetailsAreShown_WhenThereArePackages()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("T1");
			var row = Helper.CreateRowAndGenerateLocations(transitWarehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, transitWarehouse.WarehouseAddress.PK);
			parent.TransitWarehouseAddressPK = transitWarehouse.WarehouseAddress.PK;

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, location.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", transitWarehouse.PK, bookedByParty: transitWarehouse.WarehouseAddress.Header);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(receiveConsignment);
			var package = Helper.CreatePackage(packageJob, "Pkg1", 1, "PKG");
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Arrived, receiveConsignment: receiveConsignment);
			packageState.WPS_WRH_TransitReceiveHeader = receiveTransportationUnit.PK;
			packageState.WPS_WL_LastLocation = location.PK;
			var jobShipmentReference = Helper.CreateAdditionalReference(packageState, parent.JobNumber, "BPR");

			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();
				var planner = (ViewPackagesManager)viewAssignedPackagesForm.DataSource;

				var packLineFilterStripControl = (ZFilterStripCommonControl)viewAssignedPackagesForm.Controls.Find("LinesGrid", true).Single();
				AssertEquals(1, packLineFilterStripControl.GridCollection.Count);

				var packageDetailsControl = (TabControl)viewAssignedPackagesForm.Controls.Find("DetailsTabControl", true).Single();
				var splitContainer1 = (KSplitContainer)viewAssignedPackagesForm.Controls.Find("splitContainer1", true).Single();

				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("ShowPackageDetailToolStrip", true).Single();
				var arrowBtn = toolStrip.Items.Find("PackageDetailArrowBtn", true).Single();

				AssertEquals(false, splitContainer1.Panel2Collapsed);
				AssertEquals(true, packageDetailsControl.Visible);
				AssertEquals("6", arrowBtn.Text);

				arrowBtn.PerformClick();
				AssertEquals(true, splitContainer1.Panel2Collapsed);
				AssertEquals(false, packageDetailsControl.Visible);
				AssertEquals("5", arrowBtn.Text);
			}
		}

		public void TestPackageDetailControlIs_ReadOnly()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("T1");
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, transitWarehouse.WarehouseAddress.PK);
			parent.TransitWarehouseAddressPK = transitWarehouse.WarehouseAddress.PK;
			Factory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();

				var packageDetailsControl = (TabControl)viewAssignedPackagesForm.Controls.Find("DetailsTabControl", true).Single();
				var toolStrip = (ZToolStrip)viewAssignedPackagesForm.Controls.Find("ShowPackageDetailToolStrip", true).Single();
				var arrowBtn = toolStrip.Items.Find("PackageDetailArrowBtn", true).Single();

				arrowBtn.PerformClick();
				AssertEquals(true, packageDetailsControl.Visible);

				var packQtyCalcEdit = (ZCalcEdit)packageDetailsControl.Controls.Find("PackQtyCalcEdit", true).Single();
				var packageIDTextBox = (ZTextBox)packageDetailsControl.Controls.Find("PackageIDTextBox", true).Single();
				var transportRefTextBox = (ZTextBox)packageDetailsControl.Controls.Find("TransportRefTextBox", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals(true, packQtyCalcEdit.ReadOnly);
					AssertEquals(true, packageIDTextBox.ReadOnly);
					AssertEquals(true, transportRefTextBox.ReadOnly);
				});
			}
		}

		public void TestSearchPackageInNewFactory()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse("T1");
			parent.CFSAddressPksInOrder = new SortedList<int, ZGuid>();
			parent.CFSAddressPksInOrder.Add(1, warehouse.WarehouseAddress.PK);
			parent.TransitWarehouseAddressPK = warehouse.WarehouseAddress.PK;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var attachedPackage1 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var helperInNewFactory = new WhsTransitTestHelper(newFactory);
			var rcnInNewFactory = newFactory.Load<WhsItemReceiveConsignment>(rcn.PK);
			var attachedPackage2 = helperInNewFactory.CreatePackageState(rcnInNewFactory, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			newFactory.Save();

			using (var viewAssignedPackagesForm = new ViewAssignedPackagesForm(parent))
			{
				viewAssignedPackagesForm.Show();

				var packLineFilterStripControl = (ZFilterStripCommonControl)viewAssignedPackagesForm.Controls.Find("LinesGrid", true).Single();
				AssertEquals(2, packLineFilterStripControl.GridCollection.Count);

				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1.PK, attachedPackage2.PK }, packLineFilterStripControl.Grid.List.Cast<WhsItemPackageState>().Select(p => p.PK));
			}
		}

		#region TestWarehouseStatusColor

		public void TestWarehouseStatusColor_Booked()
		{
			var shipmentParent = Factory.NewWithValidTestData<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, JobShipmentSchema.Constants.Prefix);
			var bookedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				using (var tile = new CFSTile(cfsInfo))
				{
					AssertEquals(TransitWarehouseJobsStatus.Descriptions.Booked, cfsInfo.WarehouseStatus);
					AssertEquals(TransitWarehouseStatusColors.Blue, tile.Find(c => c.Name == "Status").Single().BackColor);
				}
			});
		}

		public void TestWarehouseStatusColor_Receiving()
		{
			var shipmentParent = Factory.NewWithValidTestData<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, JobShipmentSchema.Constants.Prefix);
			var bookedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				using (var tile = new CFSTile(cfsInfo))
				{
					AssertEquals(TransitWarehouseJobsStatus.Descriptions.Receiving, cfsInfo.WarehouseStatus);
					AssertEquals(TransitWarehouseStatusColors.Red, tile.Find(c => c.Name == "Status").Single().BackColor);
				}
			});
		}

		public void TestWarehouseStatusColor_Received()
		{
			var shipmentParent = Factory.NewWithValidTestData<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, JobShipmentSchema.Constants.Prefix);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, volume: 1m, volumeUQ: "M3", weight: 2, weightUQ: "KG");
			var putaway = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Putaway, rtu, Helper.CreateDispatchConsignment("DSPC1", transitWarehouse.PK));
			putaway.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;

			var committed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGCTT", TransitWarehouseStatuses.Codes.Committed, rtu, volume: 2m, volumeUQ: "M3", weight: 3, weightUQ: "KG");
			committed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			committed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var picked = Helper.CreatePackageState(rcn, 1, "PKG", "PKGPIC", TransitWarehouseStatuses.Codes.Picked, rtu, dispatchLoadList: dispatchLoadList, volume: 3m, volumeUQ: "M3", weight: 4, weightUQ: "KG");
			picked.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			picked.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			var staged = Helper.CreatePackageState(rcn, 1, "PKG", "PKGSTA", TransitWarehouseStatuses.Codes.Staged, rtu, dispatchLoadList: dispatchLoadList, volume: 4m, volumeUQ: "M3", weight: 5, weightUQ: "KG");
			staged.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			staged.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;

			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				using (var tile = new CFSTile(cfsInfo))
				{
					AssertEquals(TransitWarehouseJobsStatus.Descriptions.Received, cfsInfo.WarehouseStatus);
					AssertEquals(TransitWarehouseStatusColors.Green, tile.Find(c => c.Name == "Status").Single().BackColor);
				}
			});
		}

		public void TestWarehouseStatusColor_Dispatching()
		{
			var shipmentParent = Factory.NewWithValidTestData<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, JobShipmentSchema.Constants.Prefix);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var arrivedPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment,
				dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 2m, volumeUQ: "M3", weight: 5, weightUQ: "KG");
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu,
				dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu, volume: 1m, volumeUQ: "M3", weight: 1, weightUQ: "KG");
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				using (var tile = new CFSTile(cfsInfo))
				{
					AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatching, cfsInfo.WarehouseStatus);
					AssertEquals(TransitWarehouseStatusColors.Red, tile.Find(c => c.Name == "Status").Single().BackColor);
				}
			});
		}

		public void TestWarehouseStatusColor_Departed()
		{
			var shipmentParent = Factory.NewWithValidTestData<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TR1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", transitWarehouse.PK, transitWarehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", transitWarehouse.PK, "EXTREF1", shipmentParent.PK, JobShipmentSchema.Constants.Prefix);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", transitWarehouse.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", transitWarehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", transitWarehouse.PK);
			var departed = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departed.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			departed.WPS_IsSecure = true;
			departed.WPS_SecurityStatus = "SEC";

			var freightLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "PKGFLO", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			freightLoaded.WPS_WL_LastLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_WL_ReceiveLocation = transitWarehouse.DefaultLocation.PK;
			freightLoaded.WPS_IsSecure = true;
			freightLoaded.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var viewPackagesManager = new ViewPackagesManager(Factory, shipmentParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo = new ViewPackagesCFSInfo(Factory, transitWarehouse.WarehouseAddress.PK, controller);
			CombineAssertions(() =>
			{
				using (var tile = new CFSTile(cfsInfo))
				{
					AssertEquals(TransitWarehouseJobsStatus.Descriptions.Dispatched, cfsInfo.WarehouseStatus);
					AssertEquals(TransitWarehouseStatusColors.Blue, tile.Find(c => c.Name == "Status").Single().BackColor);
				}
			});
		}

		#endregion

		protected override Form GetFormToBashCore()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = Helper.CreateTRWWarehouse().WarehouseAddress.PK;
			return new ViewAssignedPackagesForm(parent);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);
	}
}
