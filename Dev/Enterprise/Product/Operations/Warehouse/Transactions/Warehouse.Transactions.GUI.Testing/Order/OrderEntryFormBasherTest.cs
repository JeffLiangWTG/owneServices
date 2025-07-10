using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(OrderEntryForm))]
	public class OrderEntryFormBasherTest : ZFormBasherTest
	{
		#region TestCreateShipmentMenuItem

		public void TestCreateShipmentMenuItem()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = Factory.NewWithValidTestData<WhsWarehouse>().PK;
			order.WD_OH_Forwarder = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.Forwarder.OH_Code = "SAM";
			order.ConsigneeAddressPK = Factory.NewWithValidTestData<OrgAddress>().PK;
			order.ConsigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var relatedJobsMenuItem = form.Menu.MenuItems.FindByText("Related Jobs");
				AssertNotNull(relatedJobsMenuItem);
				relatedJobsMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(2, relatedJobsMenuItem.MenuItems.Count);

				var localTransportPluginMenuItem = relatedJobsMenuItem.MenuItems.FindByText("Port Transport");
				AssertNotNull(localTransportPluginMenuItem);

				var shipmentsMenuItem = relatedJobsMenuItem.MenuItems.FindByText("Shipments");
				AssertNotNull(shipmentsMenuItem);
				AssertEquals(true, shipmentsMenuItem.Enabled);
				AssertEquals(1, shipmentsMenuItem.MenuItems.Count);

				var createShipmentMenuItem = shipmentsMenuItem.MenuItems[0];
				createShipmentMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Shipment", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("You must save this Warehouse Order before creating a Shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createShipmentMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertMultilineASCIIEquals("UnitTestUserNotification.Instance.LastMessage.Text", @"
Failed to create Shipment:
No EDI Communications settings were found on the Recipient Organization [SAM]. Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.
			".Trim(), UnitTestUserNotification.Instance.LastMessage.Text);

				order.ConsigneeAddressPK = ZGuid.Empty;
				order.ConsigneeDocAddress.E2_AddressOverride = true;
				order.ConsigneeDocAddress.E2_City = "";

				Factory.Save();
				createShipmentMenuItem.PerformClick();
				AssertEquals(@"Failed to create Shipment:
Cannot create a Shipment as a Consignee Location Port/UNLOCO could not be determined. Check the City, State and Country/Region for a valid combination.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestMustRunPreSaveValidationForOrdersInError

		public void TestMustRunPreSaveValidationForOrdersInError()
		{
			var order = Factory.New<WhsOrder>();
			order.WD_DocketStatus = DocketStatus.Codes.Error;
			AssertEquals(false, order.HasErrors);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
			}

			AssertEquals("Order must have errors after loading Order Entry Form", true, order.HasErrors);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess_WhenBound

		/// <summary>
		/// This test is important because the GUI attempts to pull shortfalls during
		/// the initial calculation (causing poor performance if not properly managed).
		/// The business-layer tests do not prove this problem as no GUI is attached.
		/// </summary>
		public void TestWE_ShortfallQuantityCached_HitsDbToUpdateAllLinesOnFirstAccess_WhenBound()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			// commit some units
			var orderToCommitUnits = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(orderToCommitUnits, data.Part1, 10m);
			Helper.CreatePickNew(orderToCommitUnits);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 100); // only 90 available (10 are committed)
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 120); // only 100 available
			var line3 = Helper.CreateWhsOrderLine(order, part3, 50); // 0 available

			Factory.Save();
			AssertNoWarnings("Precondition", line1.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings("Precondition", line2.WE_ShortfallQuantityCachedInfo);
			AssertNoWarnings("Precondition", line3.WE_ShortfallQuantityCachedInfo);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var dbHits = Factory.GetTableHitCount(WhsInventoryViewSchema.Constants.TableName);
				form.Show();

				AssertEquals(10m, line1.WE_ShortfallQuantityCached);
				AssertEquals(20m, line2.WE_ShortfallQuantityCached);
				AssertEquals(50m, line3.WE_ShortfallQuantityCached);
				AssertHasWarning(line1.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 90 unit(s) currently available");
				AssertHasWarning(line2.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 100 unit(s) currently available");
				AssertHasWarning(line3.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 0 unit(s) currently available");

				// these are the real tests
				AssertEquals("The DB should be hit once to calculate shortfalls for *all* lines, thereafter ShortfallQty for each line should be cached.",
					1, order.CalculateShortfallForAllLinesInOneDbHit_WasCalled_HitCountForTest);

				AssertEquals("During the one DB hit calculation the Grid attempted to pull the shortfalls *per line*. This calculation should have been suspended.",
					dbHits, Factory.GetTableHitCount(WhsInventoryViewSchema.Constants.TableName));
			}
		}

		#endregion

		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new OrderEntryForm(order, new NotificationSubscriberGuiHelper()));
		}

		#endregion

		#region TestPerformance_Pick_DBHits

		public void TestPerformance_Pick_DBHits()
		{
			// create 10 locations
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);

			// create 100 receives with 10 lines in each location 
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			for (var l = 0; l < 10; l++)
			{
				for (var r = 0; r < 10; r++)
				{
					Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + l + r, data.Part1, 2m, locations[l], "");
				}
			}
			Factory.Save();

			// create a single order and pick
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Factory.Save();

			// open release screen and finalise orders, pick and save
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			using (var form = new OrderEntryForm(orderInNewFactory, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				GUITestHelper.FindControl<ZButton>(form.Controls, "PickButton").PerformClick();

				var expectedDbHits = new Dictionary<string, int>()
				{
					{ OrgAddressSchema.Constants.TableName, 3 },
				};
				AssertDbHits(expectedDbHits, newFactory, true);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var result = new OrderEntryForm(Factory.New<WhsOrder>(), new NotificationSubscriberGuiHelper());
			result.ControllerID = ControllerIDs.WhsOrder;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Client = Helper.CreateClient();
			Whs = Helper.CreateWarehouse("TST");
		}

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;
		protected OrgHeader Client;
		protected WhsWarehouse Whs;

		#endregion
	}
}
