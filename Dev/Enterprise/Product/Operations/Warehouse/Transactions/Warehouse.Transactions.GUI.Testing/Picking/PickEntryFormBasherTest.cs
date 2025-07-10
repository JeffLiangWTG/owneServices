using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(PickEntryForm))]
	class PickEntryFormBasherTest : ZFormBasherTest
	{
		#region TestAllocatePackageLabels

		public void TestCartonisationMenuItems_WorkOrderPick()
		{
			var pick = Factory.New<WhsPick>();
			var order = Factory.New<WhsWorkOrder>();
			pick.Orders.Add(order);

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				var cancelPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");
				var printAllPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Print All Package Labels");

				AssertNull("Work Orders cannot be cartonized / do not have PackageJobs.", allocatePackageLabelsMenuItem);
				AssertNull("Work Orders cannot be cartonized / do not have PackageJobs.", cancelPackageLabelsMenuItem);
				AssertNull("Work Orders cannot be cartonized / do not have PackageJobs.", printAllPackageLabelsMenuItem);
			}
		}

		public void TestAllocatePackageLabelsAndCancelPackageLabels_NotCWSupport()
		{
			var pick = Factory.New<WhsPick>();

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				var cancelPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");
				AssertNotNull("Should NOT be hiden to non-CWSupport.", allocatePackageLabelsMenuItem);
				AssertNotNull("Should NOT be hiden to non-CWSupport.", cancelPackageLabelsMenuItem);
			}
		}

		public void TestAllocatePackageLabelsAndCancelPackageLabels_ReadOnly()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				var cancelPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");
				AssertNotNull(allocatePackageLabelsMenuItem);
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				pick.WP_PickPalletsByLabel = true;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, true, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				var pkg1 = order.PackageJob.Packages.AddNew();
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				order.PackageJob.Packages.RemoveFromRelationship(pkg1);
				pick.WP_PickPalletsByLabel = false;
				pick.WP_PickCasesByLabel = true;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, true, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				var pkg2 = order.PackageJob.Packages.AddNew();
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				order.PackageJob.Packages.RemoveFromRelationship(pkg2);
				pick.WP_PickPalletsByLabel = true;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, true, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				var pkg3 = order.PackageJob.Packages.AddNew();
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				order.PackageJob.Packages.RemoveFromRelationship(pkg3);
				pick.WP_CartoniseSplitCases = true;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, true, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				var pkg4 = order.PackageJob.Packages.AddNew();
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				order.PackageJob.Packages.RemoveFromRelationship(pkg4);
				pick.WP_PickPalletsByLabel = false;
				pick.WP_PickCasesByLabel = false;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, true, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				var pkg5 = order.PackageJob.Packages.AddNew();
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				order.PackageJob.Packages.RemoveFromRelationship(pkg5);
				pick.WP_CartoniseSplitCases = false;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				pick.WP_IsCartonised = true;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(false, false, true, true, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				pick.GetAllPickLines().First().WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(false, false, true, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);
			}
		}

		public void TestCancelPackageLabels_ReadOnly_InTransit()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				var cancelPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");
				AssertNotNull(allocatePackageLabelsMenuItem);
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				pick.WP_IsCartonised = true;
				var pickLine = pick.GetAllPickLines().First();
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(false, false, true, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);
			}
		}

		public void TestAllocatePackageLabelsAndCancelPackageLabels_FinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			pick.FinaliseAllOrders();
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				var cancelPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, true, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);
			}

			pick.WP_IsCartonised = true;
			pick.FinalisePick();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				var cancelPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(false, false, true, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);
			}
		}

		public void TestAllocatePackageLabelsAndCancelPackageLabels_CancelledPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			pick.CancelPick();
			AssertEquals("Precondition.", true, pick.IsCancelled);

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				var cancelPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Cancel Package Labels");
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(true, false, false, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);

				pick.WP_IsCartonised = true;
				AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(false, false, true, false, form, allocatePackageLabelsMenuItem, cancelPackageLabelsMenuItem);
			}
		}

		static void AssertVisibilityAndReadOnlyOfPackageLabelsMenuItems(bool allocateVisible, bool allocateEnabled, bool cancelVisible, bool cancelEnabled,
			PickEntryForm form, MenuItem allocatePackageLabelsMenuItem, MenuItem cancelPackageLabelsMenuItem)
		{
			form.ActionsMenuItemForTest.ShowPopupMenu(); // To force checking whether it should be enabled
			AssertEquals(allocateVisible, allocatePackageLabelsMenuItem.Visible);
			AssertEquals(allocateEnabled, allocatePackageLabelsMenuItem.Enabled);
			AssertEquals(cancelVisible, cancelPackageLabelsMenuItem.Visible);
			AssertEquals(cancelEnabled, cancelPackageLabelsMenuItem.Enabled);
		}

		public void TestAllocatePackageLabels_CallsAllocatePackageLabels()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				pick.SetCartonizationProgressForm((status) => { return pick.MockProgressForm.Show(status); });
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				AllocatePackageLabelsTestHelper.TestAllocatePackageLabels(allocatePackageLabelsMenuItem.PerformClick, pick);
			}
		}

		public void TestAllocatePackageLabels_ShowsProgressForm()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var assertionRan = false;
				var mock = new Mock<IAllocatePackageLabelsStrategy>();
				mock.Setup(m => m.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), It.IsAny<bool>())).Returns(true);
				mock.Setup(m => m.PickCasesByLabel(It.IsAny<WhsPick>())).Returns(true);
				mock.Setup(m => m.PickPalletsByLabel(It.IsAny<WhsPick>())).Returns(true);
				mock.Setup(m => m.CartoniseSplitCases(It.IsAny<WhsPick>(), It.IsAny<bool>())).Returns((WhsPick whsPick, bool saveFactory) =>
				{
					var lastForm = ZFormModaliser.LastFormShownForTest;
					AssertNotNull("Should have shown progress form.", lastForm);
					AssertType<ProgressForm>("Should have shown progress form.", lastForm);
					AssertEquals("Should have shown progress form.", true, lastForm.Enabled);

					var progressBarForm = (ProgressForm)lastForm;
					AssertEquals("Should have no progress bar.", false, progressBarForm.ShowProgressBar);
					AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);

					assertionRan = true;
					return CartonisationResult.Cartonised;
				});

				pick.SetAllocatePackageLabelsStrategyForTest(mock.Object);
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				allocatePackageLabelsMenuItem.PerformClick();
				AssertEquals("Should have run assertion.", true, assertionRan);
				mock.VerifyAll();
			}
		}

		#region TestAllocatePackageLabels_DbHits

		public void TestAllocatePackageLabels_DbHits_CartoniseSplitCases()
		{
			var dbHits = new Dictionary<string, int>();
			dbHits.Add(OrgCusCodeSchema.Constants.TableName, 2);
			dbHits.Add(ProcessCompanyLinkRuleSchema.Constants.TableName, 1);
			dbHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 2);
			// increased by AUS from 1 to 2
			dbHits.Add(OrgMiscServSchema.Constants.TableName, 1);
			dbHits.Add(OrgPartRelationSchema.Constants.TableName, 2);
			//decreased by AUS from 3 to 2
			dbHits.Add(RefPackTypeSchema.Constants.TableName, 1);
			dbHits.Add(OrgPartUnitSchema.Constants.TableName, 2);
			dbHits.Add(WhsCartonGroupSchema.Constants.TableName, 1);
			dbHits.Add(WhsCartonGroupSizeLinkSchema.Constants.TableName, 1);
			dbHits.Add(WhsCartonSizeSchema.Constants.TableName, 1);
			// decreased by AUS from 2 to 0
			dbHits.Add(WhsPickLineSchema.Constants.TableName, 0);
			dbHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			dbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			dbHits.Add(PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1);
			dbHits.Add(PkgPackageItemDivotSchema.Constants.TableName, 2);
			dbHits.Add(StmEventSchema.Constants.TableName, 2);  // increased by LBM from 1 to 2

			TestAllocatePackageLabels_DbHits(dbHits, UOMPackTypesList.Codes.SplitCase);
		}

		public void TestAllocatePackageLabels_DbHits_PickByPallet()
		{
			TestAllocatePackageLabels_DbHits_PickByLabel(UOMPackTypesList.Codes.Pallet);
		}

		public void TestAllocatePackageLabels_DbHits_PickByCases()
		{
			TestAllocatePackageLabels_DbHits_PickByLabel(UOMPackTypesList.Codes.Case);
		}

		void TestAllocatePackageLabels_DbHits_PickByLabel(string uomType)
		{
			var dbHits = new Dictionary<string, int>();
			dbHits.Add(OrgCusCodeSchema.Constants.TableName, 2);
			dbHits.Add(OrgPartRelationSchema.Constants.TableName, 2);
			dbHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 2);
			dbHits.Add(ProcessCompanyLinkRuleSchema.Constants.TableName, 1);
			// decreased by AUS from 2 to 1
			dbHits.Add(RefPackTypeSchema.Constants.TableName, 1);
			dbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			// decreased by AUS from 2 to 0
			dbHits.Add(WhsPickLineSchema.Constants.TableName, 0);
			dbHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			dbHits.Add(OrgPartUnitSchema.Constants.TableName, 2);
			dbHits.Add(PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1);
			dbHits.Add(PkgPackageItemDivotSchema.Constants.TableName, 2);
			// increased by LBM from 1 to 2
			dbHits.Add(StmEventSchema.Constants.TableName, 2);

			TestAllocatePackageLabels_DbHits(dbHits, uomType);
		}

		void TestAllocatePackageLabels_DbHits(Dictionary<string, int> dbHits, string uomType)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 3);
			var area2 = Helper.CreateArea(data.Whs1, "B");
			var area3 = Helper.CreateArea(data.Whs1, "C");
			var area4 = Helper.CreateArea(data.Whs1, "D");
			var area5 = Helper.CreateArea(data.Whs1, "E");
			var area6 = Helper.CreateArea(data.Whs1, "F");
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-1-3");
			var location4 = data.Whs1.FindLocation("A-2-1");
			var location5 = data.Whs1.FindLocation("A-2-2");
			var location6 = data.Whs1.FindLocation("A-2-3");
			location2.WLV_WA_PickingArea = area2.PK;
			location3.WLV_WA_PickingArea = area3.PK;
			location4.WLV_WA_PickingArea = area4.PK;
			location5.WLV_WA_PickingArea = area5.PK;
			location6.WLV_WA_PickingArea = area6.PK;

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, uomType);
			var isSplitCase = uomType == UOMPackTypesList.Codes.SplitCase;

			var orders = new List<WhsOrder>();
			var receives = new List<WhsReceive>();

			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);

				for (var j = 0; j < 10; j++)
				{
					var uniqueID = i.ToString() + j;
					var part = Helper.CreateProduct(data.Org1, uniqueID);
					part.OP_Cubic = 0m; // Make it easy to cartonise
					part.OP_Weight = 0m; // Make it easy to cartonise

					if (isSplitCase)
					{
						part.OP_StockKeepingUnit = refType1.F3_Code;

						Helper.CreateWhsReceiveInventoryLine(receive, part, 1m);
						Helper.CreateWhsOrderLine(order, part, 1m);
					}
					else
					{
						Helper.CreateProductUnit(part, part.OP_StockKeepingUnit, refType1.F3_Code, 2m);

						Helper.CreateWhsReceiveInventoryLine(receive, part, 2m);
						Helper.CreateWhsOrderLine(order, part, 2m);
					}
				}

				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				orders.Add(order);
				receives.Add(receive);
			}

			AssertEquals("Precondition: Multiple areas.", true, receives.SelectMany(r => r.Lines).Select(rl => rl.LocationArea).Distinct().IsCountMoreThan(1));
			Factory.Save();

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10, 10, 10, 10, 20, 1, new ZByte(99), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK; // lowest priority, has to exhaust all fallbacks
			Factory.Save();

			var pick = Helper.CreatePickNew(orders.ToArray());
			pick.WP_CartoniseSplitCases = isSplitCase;
			pick.WP_PickPalletsByLabel = uomType == UOMPackTypesList.Codes.Pallet;
			pick.WP_PickCasesByLabel = uomType == UOMPackTypesList.Codes.Case;
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);
			using (var form = new PickEntryForm(pickInFactory2, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				System.Threading.Thread.Sleep(3000);
				Application.DoEvents();

				factory2.ResetDatabaseLoadCount();
				form.ActionsMenuItemForTest.ShowPopupMenu(); // To force checking whether it should be enabled
				using (AssertDbHitsWithUsefulQueryInformation(dbHits, factory2))
				{
					var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
					allocatePackageLabelsMenuItem.PerformClick();

					AssertEquals("Precondition: Should have cartonised orders.", 100, pickInFactory2.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count));
					AssertEquals("Precondition: Should have generated Package IDs for orders.", 100, pickInFactory2.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count(p => !p.KP_PackageID.IsEmpty)));

					// Hits on ProcessTasks is somehow non deterministic if debugging or first time you have opened the test runner
					// Usually 5, but in those cases it is 2 - but I cant debug to find out why. Without fetch hints this count was considerably higher, so I still want to assert it.
					var hitCountOnProcessTasks = factory2.GetTableHitCount(ProcessTasksSchema.Constants.TableName);
					AssertEquals($"ProccessTasks hits: {hitCountOnProcessTasks}", true, hitCountOnProcessTasks <= 5);
					dbHits.Add(ProcessTasksSchema.Constants.TableName, hitCountOnProcessTasks);
				}
			}
		}

		public void TestAllocatePackageLabels_DbHits_CartoniseSplitCases_AllCartonGroupsFallbacks()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);

			var cartonGroup = Helper.CreateWhsCartonGroup("1", "Carton");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10, 10, 10, 10, 20, 1, new ZByte(99), Constants.Length.Metres, Constants.Weight.Kilograms);
			cartonGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = cartonGroup.PK;

			var orders = new List<WhsOrder>();
			// 3,6,9 CartonGroup in TransportCo.MiscServ
			// 4,8   CartonGroup in Consignee.MiscServ
			// 5,10  CartonGroup in Client.MiscServ
			// 1,2,7 CartonGroup in Warehouse.Header.MiscServ
			for (var i = 1; i <= 10; i++)
			{
				var client = i % 5 == 0 ? helper.CreateClient("C" + i) : data.Org1;
				var order = Helper.CreateWhsOrder(client, data.Whs1, "O" + i);

				if (i % 3 == 0)
				{
					var carrier = Helper.CreateClient("T" + i);
					order.TransportCoPK = carrier.PK;
					order.GetTransportCo().MiscServ.OM_WCG_CartonGroup = cartonGroup.PK;
				}
				else if (i % 4 == 0)
				{
					var consignee = Helper.CreateClient("E" + i);
					order.ConsigneePK = consignee.PK;
					order.Consignee.MiscServ.OM_WCG_CartonGroup = cartonGroup.PK;
				}
				else if (i % 5 == 0)
				{
					order.Client.MiscServ.OM_WCG_CartonGroup = cartonGroup.PK;
				}

				var receive = Helper.CreateWhsReceive(client, data.Whs1, "R" + i);

				for (var j = 0; j < 10; j++)
				{
					var uniqueID = i.ToString() + j;
					var part = Helper.CreateProduct(client, uniqueID);
					part.OP_StockKeepingUnit = refType1.F3_Code;
					part.OP_Cubic = 0m; // Make it easy to cartonise
					part.OP_Weight = 0m; // Make it easy to cartonise

					Helper.CreateWhsReceiveInventoryLine(receive, part, 1m);
					Helper.CreateWhsOrderLine(order, part, 1m);
				}

				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				orders.Add(order);
			}
			Factory.Save();

			var pick = Helper.CreatePickNew(orders.ToArray());
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);
			using (var form = new PickEntryForm(pickInFactory2, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				System.Threading.Thread.Sleep(3000);
				Application.DoEvents();

				factory2.ResetDatabaseLoadCount();
				form.ActionsMenuItemForTest.ShowPopupMenu(); // To force checking whether it should be enabled
				var allocatePackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Allocate Package Labels");
				allocatePackageLabelsMenuItem.PerformClick();

				AssertEquals("Precondition: Should have cartonised orders.", 100, pickInFactory2.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count));
				AssertEquals("Precondition: Should have generated Package IDs for orders.", 100, pickInFactory2.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count(p => !p.KP_PackageID.IsEmpty)));

				// Hits on ProcessTasks is somehow non deterministic if debugging or first time you have opened the test runner
				// Usually 4, but in those cases it is 2 - but I cant debug to find out why. Without fetch hints this count was considerably higher, so I still want to assert it.
				var hitCountOnProcessTasks = factory2.GetTableHitCount(ProcessTasksSchema.Constants.TableName);
				AssertEquals($"ProccessTasks hits: {hitCountOnProcessTasks}", true, hitCountOnProcessTasks <= 4);

				var dbHits = new Dictionary<string, int>();
				dbHits.Add(OrgCusCodeSchema.Constants.TableName, 2);
				// 4 Clients in Orders
				dbHits.Add(ProcessCompanyLinkRuleSchema.Constants.TableName, 1);
				dbHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 2);
				// increased by AUS from 1 to 2
				dbHits.Add(OrgMiscServSchema.Constants.TableName, 1);
				dbHits.Add(OrgPartRelationSchema.Constants.TableName, 2);
				// decreased by AUS from 3 to 2
				dbHits.Add(RefPackTypeSchema.Constants.TableName, 1);
				dbHits.Add(OrgPartUnitSchema.Constants.TableName, 2);
				dbHits.Add(WhsCartonGroupSchema.Constants.TableName, 1);
				dbHits.Add(WhsCartonGroupSizeLinkSchema.Constants.TableName, 1);
				dbHits.Add(WhsCartonSizeSchema.Constants.TableName, 1);
				dbHits.Add(WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1);
				// decreased by AUS from 2 to 0
				dbHits.Add(WhsPickLineSchema.Constants.TableName, 0);
				dbHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
				dbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
				dbHits.Add(PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1);
				dbHits.Add(PkgPackageItemDivotSchema.Constants.TableName, 2);

				dbHits.Add(ProcessTasksSchema.Constants.TableName, hitCountOnProcessTasks);
				dbHits.Add(StmEventSchema.Constants.TableName, 2);  // increased by LBM from 1 to 2

				AssertDbHits(dbHits, factory2);
			}
		}

		#endregion

		#endregion

		#region TestPrintAllLabels

		public void TestPrintAllLabels()
		{
			TestPrintAllLabelsCore("Print All Package Labels");
		}
		public void TestPrintAllLabelsWithoutSeparatorLabels()
		{
			TestPrintAllLabelsCore("Print All Package Labels (without Separator Labels)");
		}

		void TestPrintAllLabelsCore(string menuItemPrintLabel)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, "PKG1", 1, "BOX");
			packingHelper.CreatePackageDivot(package1, pick.GetAllPickLines().Single());
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var printAllPackageLabelsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText(menuItemPrintLabel);
				AssertNotNull("Precondition", printAllPackageLabelsMenuItem);

				printAllPackageLabelsMenuItem.PerformClick();
				AssertEquals("DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().Name);
			}
		}

		public void TestPrintAllLabelsCaption()
		{
			TestPrintAllLabelsCaptionCore("Print All Package Labels", "Reprint All Package Labels");
		}

		public void TestPrintAllLabelsCaptionWithoutSeparatorLabels()
		{
			TestPrintAllLabelsCaptionCore("Print All Package Labels (without Separator Labels)", "Reprint All Package Labels (without Separator Labels)");
		}

		void TestPrintAllLabelsCaptionCore(string captionForPrint, string captionForReprint)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(order.PackageJob, "PKG1", 1, "BOX");
			packingHelper.CreatePackageDivot(package, pick.GetAllPickLines().Single());
			Factory.Save();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				var printAllPackageLabelsMenuItemWithoutSeparatorLabels = form.ActionsMenuItemForTest.MenuItems.FindByText(captionForPrint);
				AssertNotNull(printAllPackageLabelsMenuItemWithoutSeparatorLabels);

				package.IsLabelPrinted = true;
				Factory.Save();

				form.ActionsMenuItemForTest.ShowPopupMenu();
				var reprintAllPackageLabelsMenuItemWithoutSeparatorLabels = form.ActionsMenuItemForTest.MenuItems.FindByText(captionForReprint);
				AssertNotNull(reprintAllPackageLabelsMenuItemWithoutSeparatorLabels);

				package.IsLabelPrinted = false;
				Factory.Save();

				form.ActionsMenuItemForTest.ShowPopupMenu();
				printAllPackageLabelsMenuItemWithoutSeparatorLabels = form.ActionsMenuItemForTest.MenuItems.FindByText(captionForPrint);
				AssertNotNull(printAllPackageLabelsMenuItemWithoutSeparatorLabels);
			}
		}

		#endregion

		#region TestPackageViewTabVisible

		public void TestPackageViewTabVisible()
		{
			var workOrderPick = Factory.New<WhsPick>();
			var workOrder = Factory.New<WhsWorkOrder>();
			workOrderPick.Orders.Add(workOrder);
			using (var form = new PickEntryForm(workOrderPick, new NotificationSubscriberGuiHelper()))
			{
				AssertNull(GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage"));
			}

			var pick = Factory.New<WhsPick>();
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				AssertNotNull(GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage"));
				AssertEquals(true, GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage").TabVisible);
			}

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				AssertNotNull(GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage"));
			}
		}

		#endregion

		#region TestPackageViewTabDbHits

		public void TestPackageViewTabDbHits()
		{
			var additionalHits = new Dictionary<string, int>() { { WhsInventoryViewSchema.Constants.TableName, 1 } };
			TestPackageViewTabDbHits_Core("Not Printed", additionalHits);
		}

		public void TestPackageViewTabDbHits_Picked()
		{
			var additionalHits = new Dictionary<string, int>()
			{
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};
			TestPackageViewTabDbHits_Core("Picked", additionalHits, isPicked: true);
		}

		public void TestPackageViewTabDbHits_Loaded()
		{
			var additinalHits = new Dictionary<string, int>() { { WhsLoadSchema.Constants.TableName, 1 } };
			TestPackageViewTabDbHits_Core("Loaded", additinalHits);
		}

		public void TestPackageViewTabDbHits_Departed()
		{
			var additinalHits = new Dictionary<string, int>() { { WhsLoadSchema.Constants.TableName, 1 } };
			TestPackageViewTabDbHits_Core("Departed", additinalHits);
		}

		public void TestPackageViewTabDbHits_ReleaseCapturedAttributes()
		{
			var additionalHits = new Dictionary<string, int>() {
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 }
			};
			TestPackageViewTabDbHits_Core("Not Printed", additionalHits, isRCA: true);
		}

		public void TestPackageViewTabDbHits_ReleaseCapturedAttributes_Picked()
		{
			var additionalHits = new Dictionary<string, int>()
			{
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};

			TestPackageViewTabDbHits_Core("Picked", additionalHits, isRCA: true, isPicked: true);
		}

		void TestPackageViewTabDbHits_Core(
			string packageLabelStatus,
			Dictionary<string, int> extraHitsToDefault,
			bool isRCA = false,
			bool isPicked = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);

			var part = Helper.CreateProduct(data.Org1, "1");
			part.OP_StockKeepingUnit = refType1.F3_Code;
			part.OP_Cubic = 0m; // Make it easy to cartonise
			part.OP_Weight = 0m; // Make it easy to cartonise

			if (isRCA)
			{
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(data.Org1, part, AttributeNumber.One, use: true);
				Helper.SetProductAttributeUse(data.Org1, part, AttributeNumber.Two, use: true);
			}

			for (var i = 0; i < 10; i++)
			{
				var loc = data.Whs1.FindLocation("A-" + (i + 1));
				var area = Helper.CreateArea(data.Whs1, "Area - " + (i + 1));
				loc.WLV_WA_PickingArea = area.PK;
				if (isRCA)
				{
					var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + (i + 1));
					Helper.CreateWhsReceiveInventoryLine(receive, part, 100m, loc, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
					receive.AllocateLocationsWithMock();
					receive.FinaliseDocketWithoutUserConfirmation();
					WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				}
				else
				{
					Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + (i + 1), part, 100m, loc, "");
				}
			}

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, part, 1000m);
			Factory.Save();

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10, 10, 10, 10, 20, 10, new ZByte(99), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK; // lowest priority, has to exhaust all fallbacks

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			Factory.Save();

			if (isPicked)
			{
				var pickLines = pick.GetAllPickLines();
				pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
				Factory.Save();
			}

			var loadPkgPackagePivots = Enumerable.Empty<WhsLoadPkgPackagePivot>();
			if ("Loaded" == packageLabelStatus || "Departed" == packageLabelStatus)
			{
				loadPkgPackagePivots = LoadPackage(pick.Orders.Cast<WhsOrder>().SelectMany(o => o.PackageJob.Packages), data.Whs1.DefaultOutboundDockDoorLocation).ToArray();
			}

			if ("Departed" == packageLabelStatus)
			{
				DepartPackage(loadPkgPackagePivots);
			}
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);

			var packages = ((WhsOrder)pickInFactory2.Orders[0]).PackageJob.Packages;
			AssertEquals("Precondition", 100, packages.Count);

			using (AssertDBHitsPackageView(extraHitsToDefault, factory2))
			using (RowFactory.SetCachedTables())
			using (var form = new PickEntryForm(pickInFactory2, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var tabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.Controls, "MainTabControl");
				var packingTab = GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage");
				var packagesGrid = GUITestHelper.FindControl<ZGrid>(form.Controls, "PackagesGrid");
				tabControl.SelectedTab = packingTab;

				packagesGrid.SelectSingleElement(packages[0]);
				Application.DoEvents();

				// Scroll across the grid
				int horizontalScrollPosition = 0;
				packagesGrid.HorizontalScrollToOffset(0);
				while (horizontalScrollPosition < packagesGrid.HorizontalScrollBarMaximum)
				{
					int offset = Math.Min(packagesGrid.HorizontalScrollBarMaximum - horizontalScrollPosition, packagesGrid.ClientRectangle.Width);
					horizontalScrollPosition += offset;
					packagesGrid.HorizontalScrollToOffset(horizontalScrollPosition);
					Application.DoEvents();
				}

				for (int index = 0; index < packagesGrid.VisibleRowCount - 1; index++)
				{
					var package = packages[index];
					packagesGrid.SelectSingleElement(package); // selecting the row will change the child Grid, to test its DB Hits.
					Application.DoEvents();
					AssertEquals(packageLabelStatus, new PackagePackingHelper(package).GetPackageLabelStatus());
				}
			}
		}

		static IDisposable AssertDBHitsPackageView(Dictionary<string, int> extraHitsToDefault, BusinessObjectFactory factory)
		{
			var dbHits = new Dictionary<string, int>()
			{
				// Core stuff, hits varies slightly. Out of the scope of what i'm trying to test here.
				{ StmNoteSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },

				// Dbhits in scope
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ GenAddOnColumnSchema.Constants.TableName, 2 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 }, // decreased by LBM from 3 to 2
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 2 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
#if WINZOR
				{ UNDGDataItemSchema.Constants.TableName, 2 },
#endif
				// Dock Door location + Pick Area
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsAreaSchema.Constants.TableName, 1 },
			};

			foreach (var extraHit in extraHitsToDefault)
			{
				if (dbHits.ContainsKey(extraHit.Key))
				{
					dbHits[extraHit.Key] += extraHit.Value;
				}
				else
				{
					dbHits.Add(extraHit.Key, extraHit.Value);
				}
			}

			return AssertDbHitsWithUsefulQueryInformation(dbHits, factory);
		}

		void DepartPackage(IEnumerable<WhsLoadPkgPackagePivot> loadPkgPackagePivots)
		{
			foreach (var loadPkgPackagePivot in loadPkgPackagePivots)
			{
				Helper.DepartPackageNow(loadPkgPackagePivot);
			}
		}

		IEnumerable<WhsLoadPkgPackagePivot> LoadPackage(IEnumerable<PkgPackage> packages, WhsLocation dockDoorLocation)
		{
			var transportCompany = Helper.CreateClient("TC1");
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			foreach (var package in packages)
			{
				var load = Helper.CreateWhsLoad(transportCompany, dockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
				load.WLO_PL_NKCarrierServiceLevel = carrierServicelevel.PL_Code;
				var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
				loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
				loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";
				yield return loadPkgPackagePivot;
			}
		}

		#endregion

		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => (ZForm)GetFormToBash());
		}

		#endregion

		#region TestPackagesGridHasAdditionalColumn

		public void TestPackagesGridHasAdditionalColumn()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertNotNull(GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage"));
				AssertNotNull(GUITestHelper.FindControl<ZGrid>(form.Controls, "PackagesGrid"));

				var packagesGrid = GUITestHelper.FindControl<ZGrid>(form.Controls, "PackagesGrid");
				AssertEquals(true, packagesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Package Status"));
				AssertEquals(true, packagesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Pick Area"));
				AssertEquals(true, packagesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Picker"));
			}

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertNotNull(GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage"));
				AssertNotNull(GUITestHelper.FindControl<ZGrid>(form.Controls, "PackagesGrid"));
			}
		}

		#endregion

		#region TestPackedGridHasAdditionalColumns

		public void TestPackedGridHasAdditionalColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var refType1 = PackingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);

			var part = Helper.CreateProduct(data.Org1, "1");
			part.OP_StockKeepingUnit = refType1.F3_Code;
			part.OP_Cubic = 0m; // Make it easy to cartonise
			part.OP_Weight = 0m; // Make it easy to cartonise

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part, 1000m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, part, 1000m);
			Factory.Save();

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10, 10, 10, 10, 20, 10, new ZByte(99), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK; // lowest priority, has to exhaust all fallbacks

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			pick.AllocatePackageLabels();

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var tabControl = GUITestHelper.FindControl<ZTabControl>(form.Controls, "MainTabControl");
				AssertNotNull("Precondition. MainTabControl has to exist", tabControl);

				var tabPage = GUITestHelper.FindControl<ZTabPage>(form.Controls, "PackingViewTabPage");
				AssertNotNull("Precondition. PackingViewTabPage has to exist", tabPage);

				var packedGrid = GUITestHelper.FindControl<ZGrid>(form.Controls, "PackedGrid");
				AssertNotNull("Precondition. PackedGrid has to exist", packedGrid);

				var packagesGrid = GUITestHelper.FindControl<ZGrid>(form.Controls, "PackagesGrid");
				AssertNotNull("Precondition. PackagesGrid has to exist", packagesGrid);

				tabControl.SelectTab(tabPage);

				AssertEquals("Should have added AdditionalProperties.", true, packedGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Part Attrib. 1"));
				AssertEquals("Should have added AdditionalProperties.", true, packedGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Part Attrib. 2"));
				AssertEquals("Should have added AdditionalProperties.", true, packedGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Part Attrib. 3"));
				AssertEquals("Should have added AdditionalProperties.", true, packedGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Expiry"));
				AssertEquals("Should have added AdditionalProperties.", true, packedGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.Caption == "Packing"));
			}
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		protected override Form GetFormToBashCore()
		{
			return new PickEntryForm(Factory.New<WhsPick>(), new NotificationSubscriberGuiHelper());
		}

		#endregion
	}
}
