using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(WhsItemPackageStateFilterBusinessObject.Schema))]

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(TransitWarehousePackageDetachForm))]
	class TransitWarehousePackageDetachFormTest : ZFormBasherTest
	{
		#region TestRemoveAndPutBackAttachedPackage

		public void TestRemoveAndPutBackAttachedPackage() => TestRemoveAndPutBackAttachedPackage(false);

		public void TestRemoveAndPutBackAttached_NonBlindPackage() => TestRemoveAndPutBackAttachedPackage(true);

		void TestRemoveAndPutBackAttachedPackage(bool isNonBlindPackage)
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			if (isNonBlindPackage)
			{
				rcn.WRC_ParentID = ZGuid.NewZGuid();
				rcn.WRC_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var attachedPackage1 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var attachedPackage2 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);

			Factory.Save();

			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			Factory.Save();

			using (var form = new TransitWarehousePackageDetachForm(parent))
			{
				form.Show();
				var detachedPackagesGrid = (ZGrid)form.Controls.Find("DetachPackingGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var removeButton = (form.Controls.Find("AttachAndRemoveButtons", true).Single() as ZToolStrip).Items.Find("RemoveToolStripButton", true).Single() as ZToolStripButton;
				var putBackButton = (form.Controls.Find("AttachAndRemoveButtons", true).Single() as ZToolStrip).Items.Find("AttachToolStripButton", true).Single() as ZToolStripButton;
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), detachedPackagesGrid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1, attachedPackage2 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(attachedPackage1);
				removeButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1 }, detachedPackagesGrid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage2 }, attachedPackagesGrid.List);

				detachedPackagesGrid.SelectSingleElement(attachedPackage1);
				putBackButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), detachedPackagesGrid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1, attachedPackage2 }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(attachedPackage1);
				removeButton.PerformClick();
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage1 }, detachedPackagesGrid.List);
				AssertContainsExactElementsInAnyOrder(new[] { attachedPackage2 }, attachedPackagesGrid.List);

				UnitTestUserNotification.Instance.ClearMessages();
				var createButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				bool isAttachedPackagesCalled = false;
				bool isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				createButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(isRemovePackagesCalled);
				Assert(!attachedPackage1.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				Assert(!attachedPackage2.IsRemovedFromAttachedPacklinesAndHasJobNumber);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"You did not add all packages from the following RCNs:
EXTREF1 - RCN1

Continue anyway?"));
			}
		}

		#endregion

		#region TestCreatePackLineButton_HasRemovedOVPs

		public void TestCreatePackLineButton_HasRemovedOVPsAndPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var standalonePackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKGS", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: parent.JobNumber);
			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", receiveConsignment, receiveTransportationUnit, rcn: receiveConsignment, entryNum: parent.JobNumber);

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", overpackPackage);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageDetachForm(parent))
			{
				form.Show();
				var detachedPackagesGrid = (ZGrid)form.Controls.Find("DetachPackingGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var removeButton = (form.Controls.Find("AttachAndRemoveButtons", true).Single() as ZToolStrip).Items.Find("RemoveToolStripButton", true).Single() as ZToolStripButton;

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState, overpackPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(overpackPackage);
				removeButton.PerformClick();
				attachedPackagesGrid.SelectSingleElement(standalonePackageState);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { standalonePackageState, overpackPackage }, detachedPackagesGrid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				var isAttachedPackagesCalled = false;
				var isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };

				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				createPackLinesButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(isRemovePackagesCalled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You have removed 1 Package(s) and 1 Overpack(s). These Packlines will be removed from the Shipment.\r\nDetaching an overpack with inner packlines will remove the inner packlines from the Shipment."));
			}
		}

		public void TestCreatePackLineButton_HasRemovedOVPs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var overpackPackage = Helper.CreateOverpackPackage("OVP1", receiveConsignment, receiveTransportationUnit, rcn: receiveConsignment, entryNum: parent.JobNumber);

			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", overpackPackage);
			Helper.PackPackageIntoHandlingUnit(overpackPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", overpackPackage);

			Factory.Save();

			var transitWarehouseParent = (ITransitWarehouseParent)parent;
			using (var form = new TransitWarehousePackageDetachForm(parent))
			{
				form.Show();
				var detachedPackagesGrid = (ZGrid)form.Controls.Find("DetachPackingGrid", true).Single();
				var attachedPackagesGrid = (ZGrid)form.Controls.Find("PackingGrid", true).Single();
				var removeButton = (form.Controls.Find("AttachAndRemoveButtons", true).Single() as ZToolStrip).Items.Find("RemoveToolStripButton", true).Single() as ZToolStripButton;

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { overpackPackage }, attachedPackagesGrid.List);

				attachedPackagesGrid.SelectSingleElement(overpackPackage);
				removeButton.PerformClick();

				AssertContainsExactElementsInAnyOrder(new List<WhsItemPackageState> { overpackPackage }, detachedPackagesGrid.List);
				AssertContainsExactElementsInAnyOrder(Array.Empty<WhsItemPackageState>(), attachedPackagesGrid.List);

				var isAttachedPackagesCalled = false;
				var isRemovePackagesCalled = false;
				parent.AttachPackagesCalled += (p) => { isAttachedPackagesCalled = true; };
				parent.RemovePackagesCalled += (p) => { isRemovePackagesCalled = true; };

				var createPackLinesButton = (ZButton)form.Controls.Find("CreatePackLinesButton", true).Single();
				createPackLinesButton.PerformClick();

				Assert(isAttachedPackagesCalled);
				Assert(isRemovePackagesCalled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You have removed 1 Overpack(s). These Packlines will be removed from the Shipment.\r\nDetaching an overpack with inner packlines will remove the inner packlines from the Shipment."));
			}
		}

		#endregion

		#region TestRelatedItems_DoubleClickNotSupported

		public void TestRelatedItems_DoubleClickNotSupported()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Factory.Save();

			using (var form = new ShowEDocsForm(packageState))
			{
				form.Show();

				var userControl = form.Controls.Cast<Control>().OfType<ZTabControl>().Single().Controls.Cast<ZAutoSizedTabPagePlugIn>().Single().Controls.Cast<eDocsUserControl>().Single();
				var relatedParentsGrid = (ZGrid)userControl.Controls.Find("RelatedParentsGrid", true).Single();
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, relatedParentsGrid.GetRowNotificationRectangle(0).X, relatedParentsGrid.GetRowNotificationRectangle(0).Y, 0);
				userControl.DoubleClickOn_RelatedParents(relatedParentsGrid, mouseEvent);

				AssertEquals($"Double click on Related eDocs is not supported in the Transit Packages module. Please contact {Core.Constants.ProductName} support for more information.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = Helper.CreateTRWWarehouse().WarehouseAddress.PK;
			return new TransitWarehousePackageDetachForm(parent);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
