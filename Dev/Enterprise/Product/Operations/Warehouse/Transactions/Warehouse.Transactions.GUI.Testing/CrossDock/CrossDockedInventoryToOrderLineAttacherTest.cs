using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class CrossDockedInventoryToOrderLineAttacherTest : ZRecordAttacherTest
	{
		#region TestAttach

		public void TestAttach()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.CancelReactivateDocket(); // Cancel Order

			using (var form = new ZForm())
			{
				form.Show();
				var attacher = new CrossDockedInventoryToOrderLineAttacher(orderLine.ReservedPickLines, orderLine.InventoryFilter, ModuleIDs.WhsInventory, orderLine);
				attacher.Show(form);
				AssertInventoryCouldBeAttached(data.Line111, attacher, false);

				order.CancelReactivateDocket(); // Reactivate Order
				AssertInventoryCouldBeAttached(data.Line111, attacher, true);

				var pick = helper.CreatePickNew(order);
				pick.FinaliseAllOrders();
				AssertEquals(true, order.IsFinalised);
				AssertInventoryCouldBeAttached(data.Line111, attacher, false);

				pick.FinalisePick();
				AssertEquals(true, pick.IsFinalised);
				AssertInventoryCouldBeAttached(data.Line111, attacher, false);
			}
		}

		static void AssertInventoryCouldBeAttached(WhsInventoryView inventory, CrossDockedInventoryToOrderLineAttacher attacher, bool canAttach)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var popup = attacher.LastShownAttachPopupForTesting)
			{
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { inventory });

				if (canAttach)
				{
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
				else
				{
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
					AssertContains(attacher.UnableToAttachText, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region TestAttach_ChecksPackageGroupID

		public void TestAttach_ChecksPackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation.PK, "123-1", "ABC", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "123-1", "DummyOutward-1", "");

			using (var form = new ZForm())
			{
				form.Show();
				var attacher = new CrossDockedInventoryToOrderLineAttacher(orderLine.ReservedPickLines, orderLine.InventoryFilter, ModuleIDs.WhsInventory, orderLine);
				attacher.Show(form);
				AssertInventoryCouldBeAttached(inventory, attacher, false);

				orderLine.WE_PackageGroupId = "ABC";
				AssertInventoryCouldBeAttached(inventory, attacher, true);
			}
		}

		#endregion

		#region TestUnableToAttachText

		public void TestUnableToAttachText()
		{
			AssertEquals(@"because of one of the following reasons:

1. The order is finalized or canceled.
2. The receipt line is already attached.
3. The receipt line quantity is already allocated.
4. The receipt line is damaged.
5. There is a mismatch on warehouse, client or product.
6. There is a mismatch on a part attribute, expiry date, packing date or ordered pallet id.
7. The inventory status is In-Transit.", new CrossDockedInventoryToOrderLineAttacher(null, null, ModuleIDs.WhsInventory, null).UnableToAttachText);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
