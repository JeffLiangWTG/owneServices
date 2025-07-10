using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickEntryFormTest : WhsGuiTestCaseWithFactory
	{
		#region TestPerformance_AllocatePackageLabels

		public void TestPerformance_AllocatePackageLabels()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 5m);
			Factory.Save();

			// will be same for Release Form, but fix is in business layer, so not point having 2 tests.
			var propertiesHit5 = RunAllocatePackageLabels(5, 5);
			var propertiesHit10 = RunAllocatePackageLabels(10, 10);

			// Should have linear growth, if you 2x Order Number and 2x Package Number then will be slightly under ~4x hits.
			var ratio = propertiesHit10 * 1.0 / propertiesHit5;
			AssertLessThanOrEqualTo("Expected ratio of calling with 4x elements to be roughly 4.", ratio, 4.2);

			int RunAllocatePackageLabels(int numberOfOrdersToCreate, int numberOfPackagesPerOrder)
			{
				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var org1 = factory.Load<OrgHeader>(data.Org1.PK);
				var whs1 = factory.Load<WhsWarehouse>(data.Whs1.PK);
				var part1 = factory.Load<OrgSupplierPart>(data.Part1.PK);

				return GetPersistentPropertiesHitCount(typeof(PkgPackage), () =>
				{
					var helper = new WhsTestHelperFunctions(factory);
					helper.CreateWhsReceiveWithInventory(org1, whs1, "R" + numberOfOrdersToCreate, part1, numberOfOrdersToCreate * numberOfPackagesPerOrder * 5m);
					factory.Save();

					var pick = helper.CreatePickNew();
					pick.WP_PickCasesByLabel = true;
					for (int i = 0; i < numberOfOrdersToCreate; i++)
					{
						var order = helper.CreateWhsOrderWithOrderLine(org1, whs1, "O" + numberOfOrdersToCreate + i, part1, numberOfPackagesPerOrder * 5m);
						pick.Orders.Add(order);
					}
					pick.AutoAllocateItemsWithMock();
					factory.Save();

					pick.OuterPackages.ApplySort(PkgPackage.Schema.KP_Weight, ListSortDirection.Ascending); // to simulate user sorting grid by some column

					using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
					{
						form.Show();
						form.MainTabControlForTest.SelectedTab = form.FindSingle<ZTabPage>("PackingViewTabPage");

						var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");

						menuItem.PerformClick();
						AssertEquals("Ensure function worked.", numberOfOrdersToCreate * numberOfPackagesPerOrder, pick.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count));
					}
				}, factory);
			}
		}

		#endregion

		#region TestPerformance_CancelPackageLabelAllocations

		public void TestPerformance_CancelPackageLabelAllocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 5m);
			Factory.Save();

			// will be same for Release Form, but fix is in business layer, so not point having 2 tests.
			var propertiesHit5 = RunCancelPackageLabels(5, 5);
			var propertiesHit10 = RunCancelPackageLabels(10, 10);

			// Should have linear growth, if you 2x Order Number and 2x Package Number then will be slightly under ~4x hits.
			var ratio = propertiesHit10 * 1.0 / propertiesHit5;
			AssertLessThanOrEqualTo("Expected ratio of calling with 4x elements to be roughly 4.", ratio, 4.21);

			int RunCancelPackageLabels(int numberOfOrdersToCreate, int numberOfPackagesPerOrder)
			{
				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var org1 = factory.Load<OrgHeader>(data.Org1.PK);
				var whs1 = factory.Load<WhsWarehouse>(data.Whs1.PK);
				var part1 = factory.Load<OrgSupplierPart>(data.Part1.PK);

				return GetPersistentPropertiesHitCount(typeof(PkgPackage), () =>
				{
					var helper = new WhsTestHelperFunctions(factory);
					helper.CreateWhsReceiveWithInventory(org1, whs1, "R" + numberOfOrdersToCreate, part1, numberOfOrdersToCreate * numberOfPackagesPerOrder * 5m);
					factory.Save();

					var pick = helper.CreatePickNew();
					pick.WP_PickCasesByLabel = true;
					for (int i = 0; i < numberOfOrdersToCreate; i++)
					{
						var order = helper.CreateWhsOrderWithOrderLine(org1, whs1, "O" + numberOfOrdersToCreate + i, part1, numberOfPackagesPerOrder * 5m);
						pick.Orders.Add(order);
					}
					pick.AutoAllocateItemsWithMock();
					factory.Save();

					pick.AllocatePackageLabels();
					factory.Save();
					AssertEquals("Precondition", numberOfOrdersToCreate * numberOfPackagesPerOrder, pick.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count));

					var number = 0;
					foreach (WhsOrder order in pick.Orders)
					{
						foreach (var package in order.PackageJob.Packages)
						{
							package.KP_Weight = number;
							number++;
						}
					}

					pick.OuterPackages.ApplySort(PkgPackage.Schema.KP_Weight, ListSortDirection.Ascending); // to simulate user sorting grid by some column

					using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
					{
						form.Show();
						form.MainTabControlForTest.SelectedTab = form.FindSingle<ZTabPage>("PackingViewTabPage");

						var menuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");

						menuItem.PerformClick();
						AssertEquals("Ensure function worked.", 0, pick.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count));
					}
				}, factory);
			}
		}

		#endregion

		#region Constructors

		public void TestConstructor()
		{
			using (var form = new PickEntryForm(Pick, new NotificationSubscriberGuiHelper()))
			{
				Assert("Action menuitem not added", form.ActionsMenuItemForTest.MenuItems.FindByText("Sort Items for FIFO based Picking") != null);
				Assert("Action menuitem not added", form.ActionsMenuItemForTest.MenuItems.FindByText("Clear All Stock Allocations") != null);
				Assert("Action menuitem not added", form.ActionsMenuItemForTest.MenuItems.FindByText("Auto Allocate Remaining Items") != null);
				Assert("Action menuitem not added", form.ActionsMenuItemForTest.MenuItems.FindByText("Assign All Lines to User") != null);
				Assert("Action menuitem not added", form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to ready") != null);

				AssertEquals(form, Pick.NotificationManager.Peek);
				AssertNotNull("DocumentUDF not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new PickEntryForm(Pick, null));
		}

		#endregion

		#region Methods

		public void TestShowPickSlipTab()
		{
			using (var form = new PickEntryForm(Pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.PickSlipTabPage != null && form.PickSlipTabPage != form.MainTabPageForTest);
				AssertEquals(form.MainTabPageForTest, form.MainTabControlForTest.SelectedTab);

				form.ShowPickSlipTab();
				AssertEquals(form.PickSlipTabPage, form.MainTabControlForTest.SelectedTab);
			}
		}

		#endregion

		#region Properties

		public void TestProperties()
		{
			using (var form = new PickEntryForm(Pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(Pick, form.Pick);
			}
		}

		public void TestFormCaption()
		{
			using (var form = new PickEntryForm(Pick, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Pick", form.FormCaption.Trim());
			}
		}

		#endregion

		// Printing documents from Customize menu does not trigger the DocumentPrinted event, therefore the functionality will not work without core changes
		// the core team make changes to trigger the event at same time as workflow event, then this test can be uncommented
		//#region TestPrintingDocument_FromCustomeisedDocumentsMenu_ChangesStatusAndSave

		//public void TestPrintingDocument_FromCustomeisedDocumentsMenu_ChangesStatusAndSave()
		//{
		//	var data = new TestDataSimpleEnvironment(Factory);
		//	Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
		//	Factory.Save();

		//	var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
		//	var pick = Helper.CreatePickNew(order);
		//	Factory.Save();
		//	AssertEquals("Precondition", 10m, pick.GetAllPickLines().Sum(pl => pl.WZ_Units));
		//	AssertEquals("Precondition", PickStatus.Codes.Created, pick.WP_PickStatus);

		//	using (var form = new PickEntryForm(Pick))
		//	{
		//		form.Show();
		//		Factory.Save();

		//		using (var documentsMenuItem = form.Menu.MenuItems.FindByText("Documents"))
		//		{
		//			documentsMenuItem.OnPopup(EventArgs.Empty);

		//			var pickingSlipMenuItem = documentsMenuItem.MenuItems.FindByText("Customize");
		//			pickingSlipMenuItem.PerformClick();
		//		}

		//		var customiseDocumentsForm = ZFormModaliser.LastFormShownForTest;
		//		AssertEquals("Precondition", "Customize Document Menus", customiseDocumentsForm.Text);

		//		var documentsGrid = GUITestHelper.FindControl<ZGrid>(customiseDocumentsForm.Controls, "MenusGrid");
		//		documentsGrid.Select(documentsGrid.ListManager.List.IndexOf(documentsGrid.ListManager.List.Cast<StmMenuItem>().Single(m => m.SU_MenuName == "Picking Slip" && m.SU_MenuPath.IsEmpty)));

		//		EventHandler handler = null;
		//		handler = (o, e) =>
		//		{
		//			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		//			var pickingSlipMenuItem = documentsGrid.ContextMenu.MenuItems.FindByText("Run");
		//			pickingSlipMenuItem.PerformClick();

		//			documentsGrid.ContextMenu.Popup -= handler;
		//			documentsGrid.ContextMenu.Dispose();
		//		};

		//		documentsGrid.ContextMenu.Popup += handler;
		//		documentsGrid.ContextMenu.Show(documentsGrid, documentsGrid.Location);
		//	}

		//	var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
		//	var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
		//	AssertEquals("Pick status should have been changed and saved to DB.", PickStatus.Codes.PickSlip, pickInOtherFactory.WP_PickStatus);
		//}

		//#endregion

		#region INotifications Members

		public void TestNotify()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var type = new TestINotificationType("Message", "NotZErrorMessageBox");
				var e = new TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				form.Notify(e);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("TestMessageToDisplay"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestQueryUser()
		{
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			var pick = Factory.New<WhsPick>();
			using (var form = new PickEntryForm(pick, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
			}
		}

		#endregion

		#region Test Assign AllLinesToUser

		#region TestAssignAllLinesToUser_PickStatus

		public void TestAssignAllLinesToUser_PickStatus()
		{
			var pick = Factory.New<WhsPick>();
			pick.WP_PickStatus = PickStatus.Codes.Cancelled;

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var assignAllLinesToUserMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Assign All Lines to User");
				AssertEquals(false, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Cancelled;
				AssertEquals(false, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Finalised;
				AssertEquals(false, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Created;
				AssertEquals(true, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Building;
				AssertEquals(true, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.PickSlip;
				AssertEquals(true, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				AssertEquals(false, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				AssertEquals(true, assignAllLinesToUserMenuItem.Enabled);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
				AssertEquals(false, assignAllLinesToUserMenuItem.Enabled);
			}
		}

		#endregion

		#region TestAssignAllLinesToUserClick

		public void TestAssignAllLinesToUserClick()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);

			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			var activeStaff2 = Helper.CreateGlbStaff("02", "T2", true);
			var activeStaff3 = Helper.CreateGlbStaff("03", "T3", true);
			var inactiveStaff = Helper.CreateGlbStaff("04", "T4", false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Should be 2 items", 2, pick.OrderedInventories.Count);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];
			availableInventory1.PickLineQuantity = 10;
			availableInventory2.PickLineQuantity = 10;
			availableInventory1.Allocate = true;
			availableInventory2.Allocate = true;
			// availableInventory1.PickedDate is empty
			// availableInventory2.PickedDate is empty

			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var assignAllLinesToUserMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Assign All Lines to User");
				AssertEquals("Pre-condition", true, assignAllLinesToUserMenuItem.Enabled);
				assignAllLinesToUserMenuItem.PerformClick();

				var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
				var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(form.PickLinesUserControlForTest.PickLineAttacherForTesting);
				AssertCollectionContains("Collection should contain active staff.", activeStaff1, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff2, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff3, staffCollection);
				AssertCollectionNotContains("Collection should not contain inactive staff.", inactiveStaff, staffCollection);

				// All unassigned pick lines with Allocate checked.

				var embeddedModulePopup = form.PickLinesUserControlForTest.LastShownPickLineAttachPopupForTesting;
				TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(embeddedModulePopup, activeStaff1);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == activeStaff1));

				// line with picked date and a line without pick date allocate checked.

				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Now);
				assignAllLinesToUserMenuItem.PerformClick();
				TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(embeddedModulePopup, activeStaff2);

				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == activeStaff2));

				// All lines with pick date and allocate checked.

				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Now);
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Now);
				assignAllLinesToUserMenuItem.PerformClick();

				AssertEquals(PickLinesUserControl.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == activeStaff2));

				// Allocated and non-allocated pick lines with no pick dates.
				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Empty);
				availableInventory1.Allocate = true;
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Empty);
				availableInventory2.Allocate = false;
				assignAllLinesToUserMenuItem.PerformClick();
				TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(embeddedModulePopup, activeStaff3);

				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff3));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == null));

				// Allocated and non-allocated pick line, but the allocated line has a picked date.
				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Now);
				availableInventory1.Allocate = true;
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Empty);
				availableInventory2.Allocate = false;
				assignAllLinesToUserMenuItem.PerformClick();

				AssertEquals(PickLinesUserControl.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff3));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == null));

				// All Non allocated pick lines

				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Empty);
				availableInventory1.Allocate = false;
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Empty);
				availableInventory2.Allocate = false;
				assignAllLinesToUserMenuItem.PerformClick();

				AssertEquals(PickLinesUserControl.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == null));
			}
		}

		#endregion

		#endregion

		#region Test Unassign Lines

		#region TestUnAssignAllLines_MenuEnable

		public void TestUnAssignAllLines_MenuEnable()
		{
			var pick = Factory.New<WhsPick>();
			pick.WP_PickStatus = PickStatus.Codes.Cancelled;

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var unassignAllLinesMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Un-assign All Lines");
				AssertEquals(false, unassignAllLinesMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Cancelled;
				AssertEquals(false, unassignAllLinesMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Finalised;
				AssertEquals(false, unassignAllLinesMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Created;
				AssertEquals(true, unassignAllLinesMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.Building;
				AssertEquals(true, unassignAllLinesMenuItem.Enabled);

				pick.WP_PickStatus = PickStatus.Codes.PickSlip;
				AssertEquals(true, unassignAllLinesMenuItem.Enabled);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				AssertEquals(false, unassignAllLinesMenuItem.Enabled);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				AssertEquals(true, unassignAllLinesMenuItem.Enabled);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
				AssertEquals(false, unassignAllLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestUnAssignAllLines_Click

		public void TestUnAssignAllLines_Click()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 5);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var notify = new TestNotificationBuffer();
			var user = Helper.CreateGlbStaff("XYZ", "XYZ");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, locations[1]);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 5m);

			var createdPick = Helper.CreatePickByAttachingOrders(order1);

			var orderedInvetoryForCreatedPick1 = createdPick.OrderedInventories[0];
			var orderedInvetoryForCreatedPick2 = createdPick.OrderedInventories[1];

			var allocatedForCreatedPick1 = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick1.AvailableInventories[0], true, ZDateTimeOffset.Empty);
			var allocatedForCreatedPick2 = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick2.AvailableInventories[0], true, ZDateTimeOffset.Empty);

			createdPick.OrderedInventories.Add(orderedInvetoryForCreatedPick1);
			createdPick.OrderedInventories.Add(orderedInvetoryForCreatedPick2);
			Factory.Save();

			createdPick.WP_PickStatus = PickStatus.Codes.Created;

			foreach (var line in createdPick.GetAllPickLines())
			{
				line.WZ_GS_NKAssignedTo = user.GS_Code;
			}

			Factory.Save();

			using (var form = new PickEntryForm(createdPick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var unassignAllLinesMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Un-assign All Lines");
				AssertEquals("Pre-condition", true, unassignAllLinesMenuItem.Enabled);
				unassignAllLinesMenuItem.PerformClick();

				AssertEquals("Pick lines have been un-assigned", true, createdPick.GetAllPickLines().All(pl => pl.AssignedTo == null));
			}
		}

		#endregion

		#endregion

		#region TestSetInitialOrderToSelectInGrid

		public void TestSetInitialOrderToSelectInGrid()
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var pick = Factory.New<WhsPick>();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "O1");
			Helper.CreateWhsOrderLine(order1, product, 10m);

			var order2 = Helper.CreateWhsOrder(client, warehouse, "O2");
			Helper.CreateWhsOrderLine(order1, product, 10m);

			var order3 = Helper.CreateWhsOrder(client, warehouse, "O3");
			Helper.CreateWhsOrderLine(order1, product, 10m);

			pick.Orders.Add(order1);
			pick.Orders.Add(order2);
			pick.Orders.Add(order3);
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var orderListManager = form.GetPickOrdersUserControl().PickOrdersModuleButtonGrid.InnerGrid.ListManager;
				AssertEquals("Precondition: Orders grid should have three orders", 3, orderListManager.Count);

				form.SetInitialOrderToSelectInGrid(order1.PK);
				var displayedOrder = (WhsDocket)orderListManager.Current;
				AssertEquals(order1.PK, displayedOrder.PK);

				form.SetInitialOrderToSelectInGrid(order2.PK);
				displayedOrder = (WhsDocket)orderListManager.Current;
				AssertEquals(order2.PK, displayedOrder.PK);
			}
		}

		#endregion

		#region TestReleasePickAwaitingReplenishment

		#region TestReleasePickAwaitingReplenishment_IsAwaitingReplenishment

		public void TestReleasePickAwaitingReplenishment_IsAwaitingReplenishment()
		{
			var pick = Factory.New<WhsPick>();
			pick.WP_PickStatus = PickStatus.Codes.Created;

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var clearIsAwaitingReplenishment = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Clear Is Awaiting Replenishment");
				AssertEquals(false, clearIsAwaitingReplenishment.Enabled);

				pick.WP_IsAwaitingReplenishment = true;
				AssertEquals(true, clearIsAwaitingReplenishment.Enabled);
			}
		}

		#endregion

		#region TestReleasePickAwaitingReplenishmentClick

		public void TestReleasePickAwaitingReplenishmentClick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", PickStatus.Codes.Created, pick.WP_PickStatus);

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				pick.WP_IsAwaitingReplenishment = true;

				var clearIsAwaitingReplenishment = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Clear Is Awaiting Replenishment");
				clearIsAwaitingReplenishment.PerformClick();
				AssertEquals("Is AWaiting Replenishment flag should be reset to false.", false, pick.WP_IsAwaitingReplenishment);
			}
		}

		public void TestCancelPackageLabelAllocations_NotRunIfInsufficientSecurityRights()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Env.Security.WhsPickingCancelPackageLabelAllocations.IsAllowed = false;

			// set package labels allocated
			pick.WP_IsCartonised = true;

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Precondition: No message yet", null, UnitTestUserNotification.Instance.LastMessage.Text);

				var cancelMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Cancel Package Labels");
				cancelMenuItem.PerformClick();

				AssertEquals("Tell the user why nothing happened", WhsErrorTypes.NoSecurityRights.Message, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Shouldn't have run because insufficient security rights", pick.WP_IsCartonised);
			}
		}

		public void TestCancelPackageLabelAllocations_RunIfSufficientSecurityRights()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Env.Security.WhsPickingCancelPackageLabelAllocations.IsAllowed = true;

			// set package labels allocated
			pick.WP_IsCartonised = true;

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Precondition: No message yet", null, UnitTestUserNotification.Instance.LastMessage.Text);

				var cancelMenuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Cancel Package Labels");
				cancelMenuItem.PerformClick();

				AssertEquals("Still no message", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have run because sufficient security rights", false, pick.WP_IsCartonised);
			}
		}

		#endregion

		#endregion

		#region TestChangeTaskPlanningStatusMenu

		#region TestChangeTaskPlanningStatusMenuVisibility

		public void TestChangeTaskPlanningStatusMenuVisibility_TaskManagementEnabled()
		{
			TestChangeTaskPlanningStatusMenuVisibilityCore(true, true);
		}

		public void TestChangeTaskPlanningStatusMenuVisibility_TaskManagementNotEnabled()
		{
			TestChangeTaskPlanningStatusMenuVisibilityCore(false, false);
		}

		void TestChangeTaskPlanningStatusMenuVisibilityCore(bool isTaskManagementEnabled, bool expectedVisibility)
		{
			var warehouse = Helper.CreateWarehouse("WH1");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var pick = Factory.New<WhsPick>();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "O1");
			Helper.CreateWhsOrderLine(order1, product, 10m);

			pick.Orders.Add(order1);
			Factory.Save();

			pick.WP_WW_Whs = warehouse.PK;
			if (isTaskManagementEnabled)
			{
				warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			}
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(expectedVisibility, form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to ready").Visible);
			}
		}

		#endregion

		#region TestChangeTaskPlanningStatusMenu_Status

		public void TestChangeTaskPlanningStatusMenu_StatusIsEmpty()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(string.Empty, TaskPlanningStatus.Codes.Ready, "Change task planning status to ready", "Change task planning status to not ready");
		}

		public void TestChangeTaskPlanningStatusMenu_StatusIsNotReady()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.Ready, "Change task planning status to ready", "Change task planning status to not ready");
		}

		public void TestChangeTaskPlanningStatusMenu_StatusIsReady()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.NotReady, "Change task planning status to not ready", "Change task planning status to ready");
		}

		public void TestChangeTaskPlanningStatusMenu_StatusIsPlanned()
		{
			TestChangeTaskPlanningStatusMenu_StatusCore(TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.NotReady, "Change task planning status to not ready", "Change task planning status to ready");
		}

		void TestChangeTaskPlanningStatusMenu_StatusCore(string status, string expectedStatus, string expectedMenuTextBefore, string expectedMenuTextAfter)
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			pick.Orders.Add(order1);
			pick.WP_TaskPlanningStatus = status;
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(expectedMenuTextBefore).Visible);

				form.ChangeTaskPlanningStatusMenu_ClickForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(expectedStatus, pick.WP_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText(expectedMenuTextAfter).Visible);
				});
			}
		}

		public void TestChangeTaskPlanningStatusMenu_ChangeToReadyAndThenChangeAgain()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			pick.Orders.Add(order1);
			pick.WP_TaskPlanningStatus = string.Empty;
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to ready").Visible);

				form.ChangeTaskPlanningStatusMenu_ClickForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.Ready, pick.WP_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to not ready").Visible);
				});
				form.FireSaveButton();

				form.ChangeTaskPlanningStatusMenu_ClickForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.NotReady, pick.WP_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to ready").Visible);
				});
			}
		}

		public void TestChangeTaskPlanningStatusMenu_ChangeToNotReadyAndThenChangeAgain()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG", "RG");
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			pick.Orders.Add(order1);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to not ready").Visible);

				form.ChangeTaskPlanningStatusMenu_ClickForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.NotReady, pick.WP_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to ready").Visible);
				});
				form.FireSaveButton();

				form.ChangeTaskPlanningStatusMenu_ClickForTest(null, EventArgs.Empty);
				CombineAssertions(() =>
				{
					AssertEquals(TaskPlanningStatus.Codes.Ready, pick.WP_TaskPlanningStatus);
					AssertEquals(true, form.ActionsMenuItemForTest.MenuItems.FindByText("Change task planning status to not ready").Visible);
				});
			}
		}

		#endregion

		#endregion

		#region Implementation

		WhsPick Pick => pick ?? (pick = Factory.New<WhsPick>());
		WhsPick pick;

		#endregion
	}
}
