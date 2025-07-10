using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Packing.GUI.PackageDetailUserControl;

namespace Enterprise.Packing.GUI.Testing
{
	public class PackageDetailUserControlTest : PackingTestCaseWithFactory
	{
		#region TestContainerDetailsTab_IsVisibleWhenChangingPackageTypeToContainer

		public void TestContainerDetailsTab_IsVisibleWhenChangingPackageTypeToContainer()
		{
			Data.CreatePackingData();

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;
				var packageJob = Data.PackageJob;
				var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Pallet);

				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertNotContainsTabs("Should *not* show Container Tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);
				AssertContainsTabs("Should show Packing Temperature Tab.", userControl.DetailsTabControl, userControl.PackageTemperaturesTabPage);

				package.KP_F3_NKPackType = Constants.PkgUnit.Container;
				AssertNotContainsTabs("No events are hooked, needs a manual call to bind to refresh tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);
				AssertContainsTabs("No events are hooked, needs a manual call to bind to refresh tabs.", userControl.DetailsTabControl, userControl.PackageTemperaturesTabPage);

				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertContainsTabs("Should show Container Tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);
				AssertNotContainsTabs("Should *not* show Packing Temperature Tab.", userControl.DetailsTabControl, userControl.PackageTemperaturesTabPage);
			}
		}

		#endregion

		#region TestContainerDetailsTab_IsNotVisibleWhenChangingPackageTypeToNonContainer

		public void TestContainerDetailsTab_IsNotVisibleWhenChangingPackageTypeToNonContainer()
		{
			Data.CreatePackingData();

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;
				var packageJob = Data.PackageJob;
				var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);

				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertContainsTabs("Should show Container Tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);
				AssertNotContainsTabs("Should *not* show Packing Temperature Tab.", userControl.DetailsTabControl, userControl.PackageTemperaturesTabPage);

				package.KP_F3_NKPackType = Constants.PkgUnit.Pallet;
				AssertContainsTabs("No events are hooked, needs a manual call to bind to refresh tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);
				AssertNotContainsTabs("No events are hooked, needs a manual call to bind to refresh tabs.", userControl.DetailsTabControl, userControl.PackageTemperaturesTabPage);

				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertNotContainsTabs("Should *not* show Container Tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);
				AssertContainsTabs("Should show Packing Temperature Tab.", userControl.DetailsTabControl, userControl.PackageTemperaturesTabPage);
			}
		}

		#endregion

		#region TestTabIndex_IsSetTo0AfterAddingOrRemovingPackage

		public void TestTabIndex_IsSetTo0AfterAddingOrRemovingPackage()
		{
			Data.CreatePackingData();

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;
				var packageJob = Data.PackageJob;
				var package1 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
				var package2 = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
				form.PackageDetailUserControl.Bind(packageJob, package1);
				var tabControl = form.PackageDetailUserControl.DetailsTabControl;
				AssertEquals("Default Selected Index should be 0.", 0, tabControl.SelectedIndex);
				AssertContainsTabs("Should show Container Tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);

				tabControl.SelectedIndex = 2;
				AssertEquals("Selected Index should be 2.", 2, tabControl.SelectedIndex);
				AssertContainsTabs("Should show Container Tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);

				form.PackageDetailUserControl.Bind(packageJob, null);
				AssertEquals("Selected Index should be 0 after removing Container Tabs.", 0, tabControl.SelectedIndex);
				AssertNotContainsTabs("Should *not* show Container Tabs.", userControl.DetailsTabControl, userControl.ContainerTabPage, userControl.ContainerTemperaturesTabPage);

				tabControl.SelectedIndex = 2;
				AssertEquals("Selected Index should be 2.", 2, tabControl.SelectedIndex);
				form.PackageDetailUserControl.Bind(packageJob, package2);
				AssertEquals("Selected Index should be 0 after adding a Package.", 0, tabControl.SelectedIndex);
			}
		}

		#endregion

		#region TestScanEventsTabVisibilityIsBasedOnIPackingParent

		public void TestScanEventsTabVisibilityIsBasedOnIPackingParent()
		{
			Data.CreatePackingData();

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;

				Data.Dummy.IsScanEventsVisible = false;
				form.PackageDetailUserControl.Bind(Data.PackageJob, package: null);
				AssertNotContainsTabs("Should *not* show Scan Events tab.", userControl.DetailsTabControl, userControl.ScanEventsTabPage);

				Data.Dummy.IsScanEventsVisible = true;
				form.PackageDetailUserControl.Bind(Data.PackageJob, package: null);
				AssertContainsTabs("Should show Scan Events tab.", userControl.DetailsTabControl, userControl.ScanEventsTabPage);
			}
		}

		#endregion

		#region TestScanEventsAddNewEvent

		#region TestAddNewEvent_MenuOptionNotAddedIfNoPackageIsBoundToTheControl

		[RequiresSTA]
		public void TestAddNewEvent_MenuOptionNotAddedIfNoPackageIsBoundToTheControl()
		{
			Data.CreatePackingData();
			Data.Dummy.IsScanEventsVisible = true;

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.PackageDetailUserControl.Bind(Data.PackageJob, package: null);
				var contextMenu = form.PackageDetailUserControl.ScanEventsTabPage.Controls["ScanEventsGrid"].ContextMenu;
				contextMenu.DoPopup();
				var addNewEventMenuItem = GetAddNewEventMenuItem(contextMenu);

				AssertEquals("Add new event menu item should not be added if no package is bound (e.g. when selecting the job on packing grid)", null, addNewEventMenuItem);
			}
		}

		#endregion

		#region TestAddNewEvent_CancelButtonClicked

		[RequiresSTA]
		public void TestAddNewEvent_CancelButtonClicked()
		{
			Data.CreatePackingData();
			Data.Dummy.IsScanEventsVisible = true;

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				var pkgPackage = Helper.CreatePackage(Data.PackageJob, 1, "BOX");
				form.PackageDetailUserControl.Bind(Data.PackageJob, package: pkgPackage);
				var contextMenu = form.PackageDetailUserControl.ScanEventsTabPage.Controls["ScanEventsGrid"].ContextMenu;
				contextMenu.DoPopup();
				var addNewEventMenuItem = GetAddNewEventMenuItem(contextMenu);

				using (ZFormModaliser.SuspendDispose())
				{
					addNewEventMenuItem.PerformClick();
					var addForm = (ZStmALogAddForm)ZFormModaliser.LastFormShownDialogForTest;
					addForm.Show();
					var cancelButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["CancelAddButton"];
					cancelButton.PerformClick();
					AssertEquals("Should not add event (StmALog) to collection if adding was cancelled", 0, pkgPackage.Logs.GetAllLogs().Count);
				}
			}
		}

		#endregion

		#region TestAddNewEvent_AddButtonClicked

		public void TestAddNewEvent_AddButtonClicked()
		{
			Data.CreatePackingData();
			Data.Dummy.IsScanEventsVisible = true;

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				var pkgPackage = Helper.CreatePackage(Data.PackageJob, 1, "BOX");
				form.PackageDetailUserControl.Bind(Data.PackageJob, package: pkgPackage);
				var contextMenu = form.PackageDetailUserControl.ScanEventsTabPage.Controls["ScanEventsGrid"].ContextMenu;
				contextMenu.DoPopup();
				var addNewEventMenuItem = GetAddNewEventMenuItem(contextMenu);

				using (ZFormModaliser.SuspendDispose())
				{
					addNewEventMenuItem.PerformClick();
					var addForm = (ZStmALogAddForm)ZFormModaliser.LastFormShownDialogForTest;
					addForm.Show();

					var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
					addFormBusinessEntity.SL_SE_NKEvent = Events.ArrivalCode;
					addFormBusinessEntity.SL_Reference = "TEST";
					var addButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];

					AssertEquals("Precondition", 0, pkgPackage.Logs.GetAllLogs().Count);
					addButton.PerformClick();
					var allLogs = pkgPackage.Logs.GetAllLogs();
					AssertEquals("Should have added log", 1, allLogs.Count);
					AssertEquals(Events.ArrivalCode, allLogs[0].SL_SE_NKEvent);
					AssertEquals("Reference should be 'TEST'", "TEST", allLogs[0].SL_Reference);
				}
			}
		}

		#endregion

		#endregion

		#region TestPackageSealsTab_IsVisibleWhenChangingEnablePackageSealNumbersRegistryToTrue

		public void TestPackageSealsTab_IsVisibleWhenChangingEnablePackageSealNumbersRegistryToTrue()
		{
			Data.CreatePackingData();
			WarehouseDataRegistry.Instance.EnablePackageSealNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();
				var userControl = form.PackageDetailUserControl;
				form.PackageDetailUserControl.Bind(Data.PackageJob, package: null);

				AssertContainsTabs("Should show Package Seals Tab.", userControl.DetailsTabControl, userControl.PackageSealsTabPage);
			}
		}

		#endregion

		#region TestPackageSealsTab_IsNotVisibleByDefault

		public void TestPackageSealsTab_IsNotVisibleByDefault()
		{
			Data.CreatePackingData();

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();
				var userControl = form.PackageDetailUserControl;
				form.PackageDetailUserControl.Bind(Data.PackageJob, package: null);

				AssertNotContainsTabs("Should *not* show Package Seals Tab.", userControl.DetailsTabControl, userControl.PackageSealsTabPage);
			}
		}

		#endregion

		#region TestPackageOrderReferenceTab_IsVisibleBasedOnPackageIsTransitOrOther

		public void TestPackageOrderReferencesTab_IsVisibleBasedOnPackageIsTransitOrOther()
		{
			Data.CreatePackingData();
			var whsHelper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)whsHelper.CreateTRWWarehouse();
			var row = whsHelper.CreateRowAndGenerateLocations(warehouse, "DOCK", 1, 1, 1);
			warehouse.Rows.Add(row);
			var location = row.Locations[0] as IWhsLocation;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = whsHelper.CreateWhsReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, dateTimeOffset);

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();
				var userControl = form.PackageDetailUserControl;
				var packageJob = Data.PackageJob;
				var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertNotContainsTabs("Should *not* show Package Order References Tab", userControl.DetailsTabControl, userControl.PackageOrderReferenceTabPage);

				var packageState = whsHelper.CreateWhsItemPackageState("FIN", location, rtu, package);
				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertContainsTabs("Should show Package Order References Tab", userControl.DetailsTabControl, userControl.PackageOrderReferenceTabPage);
			}
		}

		#endregion

		#region TestLabelPrinterDropEditVisibility

		public void TestLabelPrinterDropEdit()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: Package can print carrier label.", true, packagePLT.CanPrintCarrierLabel);

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;
				form.PackageDetailUserControl.Bind(packageJob, packagePLT);
				AssertEquals("Label Printer lookup control is visible.", true, userControl.PackageTabPage.Controls.Find("LabelPrinterDropEdit", true).Single().Visible);
			}
		}

		public void TestLabelPrinterDropEditVisibility_NotShownIfJobTypeIsNotConfigured()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.None;

			var packageJob = Data.PackageJob;
			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: Package cannot print carrier label.", false, package.CanPrintCarrierLabel);

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;
				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertEquals("Label Printer lookup control is not visible.", false, userControl.PackageTabPage.Controls.Find("LabelPrinterDropEdit", true).Single().Visible);
			}
		}

		public void TestLabelPrinterDropEditVisibility_NotShownIfPackageHasNoId()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: Package cannot print carrier label.", false, packagePLT.CanPrintCarrierLabel);

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;
				form.PackageDetailUserControl.Bind(packageJob, packagePLT);
				AssertEquals("Label Printer lookup control is not visible.", false, userControl.PackageTabPage.Controls.Find("LabelPrinterDropEdit", true).Single().Visible);
			}
		}

		public void TestLabelPrinterDropEditVisibility_NotShownIfParentHasNoCarrierBookingAgent()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.WarehouseOrder;

			var packageJob = Data.PackageJob;
			var packagePLT = packageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Pre-condition: Package cannot print carrier label.", false, packagePLT.CanPrintCarrierLabel);

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var userControl = form.PackageDetailUserControl;
				form.PackageDetailUserControl.Bind(packageJob, packagePLT);
				AssertEquals("Label Printer lookup control is not visible.", false, userControl.PackageTabPage.Controls.Find("LabelPrinterDropEdit", true).Single().Visible);
			}
		}

		#endregion

		#region TestAdditionalReferenceTabVisibilityIsBasedOnPackageIsTransitOrOther

		public void TestAdditionalReferenceTabVisibilityIsBasedOnPackageIsTransitOrOther()
		{
			Data.CreatePackingData();
			var whsHelper = CargoWise.Application.ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)whsHelper.CreateTRWWarehouse();
			var row = whsHelper.CreateRowAndGenerateLocations(warehouse, "DOCK", 1, 1, 1);
			warehouse.Rows.Add(row);
			var location = row.Locations[0] as IWhsLocation;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			var rtu = whsHelper.CreateWhsReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, dateTimeOffset);

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();
				var userControl = form.PackageDetailUserControl;
				var packageJob = Data.PackageJob;
				var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Package);
				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertNotContainsTabs("Should not show Additional Reference tab.", userControl.DetailsTabControl, userControl.AdditionalReferenceTabPage);

				var packageState = whsHelper.CreateWhsItemPackageState("FIN", location, rtu, package);
				form.PackageDetailUserControl.Bind(packageJob, package);
				AssertContainsTabs("Should show Additional Reference Tab.", userControl.DetailsTabControl, userControl.AdditionalReferenceTabPage);
			}
		}

		#endregion

		public void TestSubstancePKInsteadOfDIDGInDangerousGoodsGrid()
		{
			Data.CreatePackingData();

			using (var form = new PackageDetailUserControlFormForTest(Data.Dummy))
			{
				form.Show();

				var gridInfo = form.PackageDetailUserControl.FindSingle<ZGrid>("DangerousGoodsGrid");
				var substancePKColumnInfo = gridInfo.ColumnStyles.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "SubstancePK");

				var dIDGColumnInfo = gridInfo.ColumnStyles.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "DI_DG");

				AssertNotNull(substancePKColumnInfo);
				AssertNull(dIDGColumnInfo);
			}
		}

		[RequiresSTA]
		public void TestSequenceTextBoxCaption()
		{
			using (var control = new PackageDetailUserControl())
			{
				var sequenceTextBoxCaptionResourceString = control.FindSingleOrDefault<ZTextBox>(c => c.Name == "SequenceTextBox").CaptionResourceString;
				CombineAssertions(() =>
				{
					AssertEquals("Package Sequence", sequenceTextBoxCaptionResourceString.FullDescription);
					AssertEquals("Package Seq.", sequenceTextBoxCaptionResourceString.ShortCaption);
				});
			}
		}

		#region Implementation

		MenuItem GetAddNewEventMenuItem(ContextMenu menu)
		{
			foreach (MenuItem menuItem in menu.MenuItems)
			{
				if (menuItem.Text == "Add New Event")
				{
					return menuItem;
				}
			}
			return null;
		}

		void AssertContainsTabs(ZString failMessage, ZTabControl tabControl, params ZTabPage[] expectedTabPages)
		{
			foreach (var expectedTabPage in expectedTabPages)
			{
				AssertEquals(failMessage, true, tabControl.TabPages.Contains(expectedTabPage));
			}
		}

		void AssertNotContainsTabs(ZString failMessage, ZTabControl tabControl, params ZTabPage[] unexpectedTabPages)
		{
			foreach (var unexpectedTabPage in unexpectedTabPages)
			{
				AssertEquals(failMessage, false, tabControl.TabPages.Contains(unexpectedTabPage));
			}
		}

		#region class PackageDetailUserControlFormForTest

		class PackageDetailUserControlFormForTest : ZForm
		{
			public PackageDetailUserControlFormForTest(DummyWithPacking dummy)
				: base(dummy)
			{
				PackageDetailUserControl = new PackageDetailUserControl();
				Controls.Add(PackageDetailUserControl);
			}

			public readonly PackageDetailUserControl PackageDetailUserControl;
		}

		#endregion

		#endregion
	}

	[TestedType(typeof(NonActivePkgPackageCollection))]
	class NonActivePkgPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonActivePkgPackageCollection(Factory);
		}
	}
}
