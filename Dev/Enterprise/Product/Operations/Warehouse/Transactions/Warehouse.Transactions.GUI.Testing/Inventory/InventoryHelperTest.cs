using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	internal class InventoryHelperTest : WhsTestCaseWithFactory
	{
		#region TestViewReceipt

		public void TestViewReceipt()
		{
			InventoryHelper.lastShowForm = null;

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InventoryHelper.ViewReceipt(receive.Lines[0]);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			var form = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
			AssertEquals(receive.PK, ((BusinessObject)form.BusinessEntity).PK);
			form.Dispose();
		}

		#endregion

		#region TestViewStatus

		public void TestViewStatusForReceived()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);

				inventory.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(inventory.InDocketLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Received to Dock Door for Receipt {0}", receive.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
				AssertEquals(receive.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForArrived()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
				inventory.InDocketLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(inventory.InDocketLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Arrived for Receipt {0}", receive.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
				AssertEquals(receive.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForPending()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);

				var receive =
					Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 5));
				receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
				inventory.InDocketLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Empty;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(inventory.InDocketLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Pending Receipt for Receipt {0}", receive.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
				AssertEquals(receive.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForAvailable()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				receive.WD_ArrivalDate = ZDateTimeOffset.Today;
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				inventory.RunPreSaveValidation();

				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				receive.WD_FinalisedDate = receive.WD_ArrivalDate;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(inventory.InDocketLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Available for Receipt {0}", receive.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
				AssertEquals(receive.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForAvailableAfterTransfer()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				receive.FinaliseDocket();
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
				transferLine.RunPreSaveValidation();
				Factory.Save();
				transfer.FinaliseDocketWithoutUserConfirmation();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Available for Receipt {0}", receive.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
				AssertEquals(receive.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForHeld()
		{
			using (var control = new InventoryUserControl())
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				Helper.CreateProductUnit(data.Part1, "PLT", 5m);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
				transferLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
				transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
				transferLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
				transferLine.WE_F3_NKPackType = "PLT";
				transferLine.WE_LineNo = 2;
				transferLine.WE_PackageGroupId = "123";
				transferLine.WE_PalletID = "PLT-123";
				transferLine.WE_TransferFromPalletId = "PLT-456";
				transferLine.WE_TransactionQuantity = 5m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Inventory is Held with Hold Code Damaged"));

				transferLine.WE_CurrentHoldReason = "Any Reason";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Inventory is Held with Hold Code Damaged for Reason Any Reason"));
			}
		}

		public void TestViewStatusForPutawayReceive()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(inventory.InDocketLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Putaway for Receive {0}", receive.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
				AssertEquals(receive.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForPutawayTransfer()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				receive.FinaliseDocket();
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
				transferLine.RunPreSaveValidation();

				Factory.Save();
				transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;
				transfer.WD_IsPutawayTransfer = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Putaway for Putaway Transfer {0}", transfer.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as TransferEntryForm;
				AssertEquals(transfer.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForPutingaway()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				receive.FinaliseDocket();
				Factory.Save();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
				transferLine.RunPreSaveValidation();
				transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.PuttingAway;
				transfer.WD_IsPutawayTransfer = true;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Putting Away for Putaway Transfer {0}", transfer.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as TransferEntryForm;
				AssertEquals(transfer.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForInTransit()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				var orignalInventroy = receive.Lines[0];
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
				var pick = Helper.CreatePickNew(order);

				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is In-Transit for Transfer {0}", transferLine.Docket.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as TransferEntryForm;
				AssertEquals(transferLine.WE_WD, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForStaged()
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				var orignalInventroy = receive.Lines[0];
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
				var pick = Helper.CreatePickNew(order);

				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				pickLine.WZ_WE_InventoryLine = transferLine.PK;
				pickLine.WZ_WE_TransactionLine = orderLine.PK;
				orderLine.WE_WD = order.PK;

				transferLine.FinaliseDocketLine();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Staged for Order {0}", order.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as OrderEntryForm;
				AssertEquals(order.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		public void TestViewStatusForReadyToPack_PackingStation()
		{
			TestViewStatusForReadyToPackCore(LocationClasses.Codes.PST);
		}

		public void TestViewStatusForReadyToPack_PackingConsolidationLocation()
		{
			TestViewStatusForReadyToPackCore(LocationClasses.Codes.CON);
		}

		void TestViewStatusForReadyToPackCore(string locationClass)
		{
			using (var control = new InventoryUserControl())
			{
				InventoryHelper.lastShowForm = null;

				var data = new TestDataSimpleEnvironment(Factory);

				var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, locationClass);
				var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
				packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
				var pick = Helper.CreatePickNew(order);

				var pickLine = pick.GetAllPickLines().First();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				pickLine.WZ_WE_InventoryLine = transferLine.PK;
				pickLine.WZ_WE_TransactionLine = orderLine.PK;
				orderLine.WE_WD = order.PK;

				transferLine.FinaliseDocketLine();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				InventoryHelper.ViewStatus(transferLine, control);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(string.Format("Inventory is Ready To Pack for Order {0}", order.WD_DocketID)));

				var form = InventoryHelper.lastShowForm[0] as OrderEntryForm;
				AssertEquals(order.PK, ((BusinessObject)form.BusinessEntity).PK);
				form.Dispose();
			}
		}

		#endregion

		#region TestViewAdjustment

		public void TestViewAdjustment()
		{
			InventoryHelper.lastShowForm = null;
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "1", Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, "A-1-1");
			adjustment.FinaliseDocket();

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InventoryHelper.ViewReceipt(adjustmentLine);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			var adjustForm = InventoryHelper.lastShowForm[0] as AdjustmentEntryForm;
			AssertEquals(adjustment.PK, ((BusinessObject)adjustForm.BusinessEntity).PK);
			adjustForm.Dispose();
		}

		#endregion

		#region TestViewTransfer

		public void TestViewTransfer()
		{
			InventoryHelper.lastShowForm = null;
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory("T1");

			var whs2Data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			whs2Data.Org1.OH_Code = "XX2";
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, whs2Data.Whs1, "1", Notify);
			transfer.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", data.Whs1.PK, "A-2");
			transfer.FinaliseDocket();

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InventoryHelper.ViewReceipt(data.Line111.InDocketLine);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			var receiveForm = InventoryHelper.lastShowForm[0] as ReceiveEntryForm;
			AssertEquals(data.Receive11.PK, ((BusinessObject)receiveForm.BusinessEntity).PK);
			receiveForm.Dispose();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InventoryHelper.ViewReceipt(transferLine);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			var transferForm = InventoryHelper.lastShowForm[0] as TransferEntryForm;
			AssertEquals(transfer.PK, ((BusinessObject)transferForm.BusinessEntity).PK);
			transferForm.Dispose();
		}

		#endregion

		#region TestViewReleaseForPackage

		public void TestViewReleaseForPackage()
		{
			InventoryHelper.lastShowForm = null;
			
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var orderPackage1 = packageJob1.Packages.AddNew("PLT", "o11");
			var orderPackage2 = packageJob1.Packages.AddNew("PLT", "o12");

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines[0];
			pickLine1.WZ_Units = 5m;
			pickLine1.WZ_WE_InventoryLine = receive.Lines[0].PK;
			var pickLine2 = orderLine.PickLines.AddNew();
			pickLine2.WZ_Units = 5m;
			pickLine2.WZ_WE_InventoryLine = receive.Lines[0].PK;

			var divot1 = orderPackage1.PackedItemDivots.AddNew();
			divot1.KI_ParentID = pickLine1.PK;
			divot1.KI_PackedQty = pickLine1.WZ_Units;
			divot1.KI_ParentTableCode = pickLine1.TablePrefix;
			
			var divot2 = orderPackage2.PackedItemDivots.AddNew();
			divot2.KI_ParentID = pickLine2.PK;
			divot2.KI_PackedQty = pickLine2.WZ_Units;
			divot2.KI_ParentTableCode = pickLine2.TablePrefix;
			Factory.Save();

			InventoryHelper.ViewReleaseForPackage(order);

			var form = InventoryHelper.lastShowForm[0] as ReleaseEntryForm;
			AssertEquals(pick.PK, ((BusinessObject)form.BusinessEntity).PK);
			form.Dispose();
		}

		#endregion

		#region TestViewRelease

		public void TestViewRelease()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var inventoryLine = Factory.New<WhsReceiveLine>();
			InventoryHelper.lastShowForm = null;
			InventoryHelper.ViewRelease(inventoryLine);

			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(InventoryHelper.NoReleaseErrorMsg));
			AssertEquals(null, InventoryHelper.lastShowForm);

			inventoryLine.Delete();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 15m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick1.AutoAllocateItems();
			pick2.AutoAllocateItems();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InventoryHelper.ViewRelease(data.Line111.InDocketLine);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("This inventory is committed to 2 different picks. A Release form will be opened for each pick"));
			AssertEquals(2, InventoryHelper.lastShowForm.Length);

			var form = InventoryHelper.lastShowForm[0] as ReleaseEntryForm;
			AssertEquals(pick1.PK, ((BusinessObject)form.BusinessEntity).PK);
			form.Dispose();

			form = InventoryHelper.lastShowForm[1] as ReleaseEntryForm;
			AssertEquals(pick2.PK, ((BusinessObject)form.BusinessEntity).PK);
			form.Dispose();
		}

		public void TestViewRelease_WithWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bomProduct = data.Part1;
			Helper.CreateProductBOM(bomProduct, data.Part2, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			InventoryHelper.lastShowForm = null;
			AssertNoExceptionThrown(() => InventoryHelper.ViewRelease(inventory.InDocketLine));

			var form = InventoryHelper.lastShowForm.Single() as PickEntryForm;
			AssertEquals(pick.PK, ((BusinessObject)form.BusinessEntity).PK);
			form.Dispose();
		}

		#endregion

		#region TestViewCrossDock

		public void TestViewCrossDock()
		{
			InventoryHelper.lastShowForm = null;
			var inventoryLine = Factory.New<WhsReceiveLine>();
			InventoryHelper.ViewCrossDock(inventoryLine);

			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(InventoryHelper.NoCrossDockErrorMsg));
			AssertEquals(null, InventoryHelper.lastShowForm);

			inventoryLine.Delete();
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryWithSaveFactoryForWarehouse();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 15m);

			var pickLine1 = Helper.CreateReservePickLine(orderLine1, data.Line111, 1m);
			var pickLine2 = Helper.CreateReservePickLine(orderLine2, data.Line111, 1m);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InventoryHelper.ViewCrossDock(data.Line111.InDocketLine);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("This inventory is allocated to 2 different orders. An Order Entry form will be opened for each allocation"));
			AssertEquals(2, InventoryHelper.lastShowForm.Length);

			var form1 = InventoryHelper.lastShowForm[0] as OrderEntryForm;
			AssertEquals(order1.PK, ((BusinessObject)form1.BusinessEntity).PK);
			form1.Dispose();

			var form2 = InventoryHelper.lastShowForm[1] as OrderEntryForm;
			AssertEquals(order2.PK, ((BusinessObject)form2.BusinessEntity).PK);
			form2.Dispose();
		}

		#endregion

		#region TestShowWhyInventoryIsCommitted

		public void TestShowWhyInventoryIsCommitted()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1, 2, 3, 4, 5, true);

			InventoryHelper.ShowWhyInventoryIsCommitted(data.Line111.InDocketLine);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("This inventory"));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InventoryHelper.ShowWhyInventoryIsCommitted(data.Receive11.Lines.ToArray());
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("This inventory"));
		}

		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			InventoryHelper.lastShowForm = null;
		}
	}
}
