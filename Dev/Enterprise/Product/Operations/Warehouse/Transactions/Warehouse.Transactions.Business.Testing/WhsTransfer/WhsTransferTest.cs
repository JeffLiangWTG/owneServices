using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransfer))]
	class WhsTransferTest : WhsDocketTestCase<WhsTransfer>
	{
		#region Constructor

		public void TestConstructor_SetConcurrencyPolicy()
		{
			var receive = Factory.New<WhsTransfer>();

			AssertEquals("Concurrency Policy should be strict for WD_FinalisedDate.", ConcurrencyPolicy.Strict, receive.WD_FinalisedDateInfo.ConcurrencyPolicy);
		}

		#endregion

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.WhsTransfer);
			}
		}

		#endregion

		#region AcceptInventoryLinesFromSearchGrid

		#region TestAcceptInventoryLinesFromSearchGrid_ForInterWhsDestTransfer

		public void TestAcceptInventoryLinesFromSearchGrid_ForInterWhsDestTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(+2), ZDate.Today.AddDays(-5), "PA11", "PA21", "PA31", "BEK-11");
			inventory1.WI_WL = locations[0].PK;
			inventory1.WI_PalletID = "P_ID_001";
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(1);
			Helper.SetInventoryCustomAttributes(inventory1, "CA11", "CA21", "CA31", "CA41", "CA51", "CA61", 11m, 12m, 13m, 14m, 15m,
				ZDateTime.Now.AddDays(11), ZDateTime.Now.AddDays(12), ZDateTime.Now.AddDays(13), ZDateTime.Now.AddDays(14), ZDateTime.Now.AddDays(15), true, true, true, true, true, "TB1");

			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-6), "PA12", "PA22", "PA32", "BEK-12");
			inventory2.WI_WL = locations[1].PK;
			inventory2.WI_PalletID = "P_ID_002";
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(2);
			Helper.SetInventoryCustomAttributes(inventory2, "CA12", "CA22", "CA32", "CA42", "CA52", "CA62", 21m, 22m, 23m, 24m, 25m,
				ZDateTime.Now.AddDays(21), ZDateTime.Now.AddDays(22), ZDateTime.Now.AddDays(23), ZDateTime.Now.AddDays(24), ZDateTime.Now.AddDays(25), false, false, false, false, false, "TB2");

			Docket.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsDest;
			Docket.WD_WW_Whs = data.Whs1.PK;
			Docket.WD_OH_Client = data.Org1.PK;

			AssertEquals("precondition - collection allows new lines", true, ((IBindingList)Docket.Lines).AllowNew);
			Docket.Lines.AddNew();
			Docket.Lines[0].SetDocketLineFromInventory(inventory1.InDocketLine, ExcludeFromCopy.None);
			Docket.Lines.AddNew();
			Docket.Lines[1].SetDocketLineFromInventory(inventory2.InDocketLine, ExcludeFromCopy.None);
			Docket.Lines.ApplySort(WhsDocketLineSchema.Constants.WE_TransactionQuantity, ListSortDirection.Ascending);

			AssertEquals(2, Docket.Lines.Count);
			AssertDocketLineEqualsInventory(inventory1, Docket.Lines[0]);
			AssertDocketLineEqualsInventory(inventory2, Docket.Lines[1]);
		}

		#endregion

		#region TestAcceptInventoryLinesFromSearchGrid_ForInterWhsSourceTransfer

		public void TestAcceptInventoryLinesFromSearchGrid_ForInterWhsSourceTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(+2), ZDate.Today.AddDays(-5), "PA11", "PA21", "PA31", "BEK-11");
			inventory1.WI_WL = locations[0].PK;
			inventory1.WI_PalletID = "P_ID_001";
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(1);
			Helper.SetInventoryCustomAttributes(inventory1, "CA11", "CA21", "CA31", "CA41", "CA51", "CA61", 11m, 12m, 13m, 14m, 15m,
				ZDateTime.Now.AddDays(11), ZDateTime.Now.AddDays(12), ZDateTime.Now.AddDays(13), ZDateTime.Now.AddDays(14), ZDateTime.Now.AddDays(15), true, true, true, true, true, "TB1");

			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-6), "PA12", "PA22", "PA32", "BEK-12");
			inventory2.WI_WL = locations[1].PK;
			inventory2.WI_PalletID = "P_ID_002";
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(2);
			Helper.SetInventoryCustomAttributes(inventory2, "CA12", "CA22", "CA32", "CA42", "CA52", "CA62", 21m, 22m, 23m, 24m, 25m,
				ZDateTime.Now.AddDays(21), ZDateTime.Now.AddDays(22), ZDateTime.Now.AddDays(23), ZDateTime.Now.AddDays(24), ZDateTime.Now.AddDays(25), false, false, false, false, false, "TB2");

			Docket.WD_WW_Whs = data.Whs1.PK;
			Docket.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsSource;
			Docket.WD_WW_Whs = data.Whs1.PK;
			Docket.WD_OH_Client = data.Org1.PK;

			AssertEquals("precondition - collection allows new lines", true, ((IBindingList)Docket.Lines).AllowNew);
			var transferLineFromInventoryHelper = new TransferLineFromInventoryHelper(Docket.NotificationSubscriber, Docket);
			transferLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(Docket.Lines, new BusinessObject[2] { inventory1, inventory2 });
			Docket.Lines.ApplySort(WhsDocketLineSchema.Constants.WE_TransactionQuantity, ListSortDirection.Ascending);

			AssertEquals(2, Docket.Lines.Count);
			AssertDocketLineEqualsInventory(inventory1, Docket.Lines[0]);
			AssertDocketLineEqualsInventory(inventory2, Docket.Lines[1]);
		}

		#endregion

		#region TestAcceptInventoryLinesFromSearchGrid_DifferentInventoryStatuses

		public void TestAcceptInventoryLinesFromSearchGrid_DifferentInventoryStatuses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var heldCodeAAA = Helper.CreateInventoryHeldCode("AAA", "aaa");
			var heldCodeBBB = Helper.CreateInventoryHeldCode("BBB", "bbb");

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory_Available = Helper.CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 1m, location, "", InventoryStatus.Codes.Putaway); // converted to available during finalise.
			var inventory_Held = Helper.CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 2m, location, "", InventoryStatus.Codes.Held);
			var inventory_Damaged = Helper.CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 3m, location, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			var inventory_HeldCodeChanged = Helper.CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 4m, location, "", InventoryStatus.Codes.Held, "AAA");
			receive_Finalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_Finalised);

			var receive_UnFinalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory_ARV = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 4m, null, "", InventoryStatus.Codes.Arrived);
			var inventory_PUT = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 5m, location, "", InventoryStatus.Codes.Putaway);
			var inventory_HLD = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 6m, location, "", InventoryStatus.Codes.Putaway, InventoryHoldCodes.Codes.Held);
			var inventory_DMG = Helper.CreateWhsReceiveInventoryLine(receive_UnFinalised, data.Part1, 7m, location, "", InventoryStatus.Codes.Putaway, InventoryHoldCodes.Codes.Damaged);

			inventory_HeldCodeChanged.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "BBB";
			Factory.Save(); // to set WI_TotalUnits

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			AssertEquals("precondition - collection does not allow new lines", true, ((IBindingList)transfer.Lines).AllowNew);
			transfer.AcceptInventoryLinesFromSearchGrid(new BusinessObject[] { inventory_Available, inventory_Held, inventory_Damaged, inventory_ARV, inventory_PUT, inventory_HLD, inventory_DMG, inventory_HeldCodeChanged });
			AssertEquals("All acepted inventory lines should be added to transfer.", 8, transfer.Lines.Count);

			// all finalised inventory should be added with qty matching available qty.
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 1m && l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Available);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 1m && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Available);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 2m && l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Held);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 2m && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Held);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 3m && l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Held);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 3m && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Held);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 3m && l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Damaged);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 4m && l.WE_WHC_NKOriginalInventoryHeldCode == "BBB");

			// all not finalised inventory should be added with 0 qty.
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Arrived);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Arrived);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Putaway && l.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Putaway && l.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Putaway && l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Held);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Putaway && l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Damaged);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Putaway && l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Held);
			transfer.Lines.Single(l => l.WE_TransactionQuantity == 0m && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Putaway && l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Damaged);
		}

		#endregion

		#region TestAcceptInventoryLinesFromSearchGrid_ChildTransfer

		public void TestAcceptInventoryLinesFromSearchGrid_ChildTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Precondition - collection allows new lines.", true, ((IBindingList)transfer.Lines).AllowNew);

			transfer.AcceptInventoryLinesFromSearchGrid(new[] { Factory.New<WhsReceiveLine>().Inventory[0] });
			AssertEquals("Master transfer should be able to accept inventory lines.", false, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseCollectionDoesNotAllowNew));

			var childTransfer = transfer.ChildTransfers.ElementAt(0);
			AssertEquals("Precondition - collection does not allow new lines.", false, ((IBindingList)childTransfer.Lines).AllowNew);

			childTransfer.AcceptInventoryLinesFromSearchGrid(new[] { Factory.New<WhsReceiveLine>().Inventory[0] });
			AssertEquals("Child transfer should not be able to accept inventory lines.", true, ((NotificationBuffer)childTransfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseCollectionDoesNotAllowNew));
		}

		#endregion

		#region AssertDocketLineEqualsInventory

		protected override void AssertDocketLineEqualsInventory(WhsInventoryView inventory, WhsDocketLine line)
		{
			base.AssertDocketLineEqualsInventory(inventory, line);

			AssertEquals("WE_AdjustmentArrivalDate", inventory.WI_ArrivalDate, line.WE_AdjustmentArrivalDate);
			AssertEquals("WE_TransactionQuantity", inventory.WI_AvailableToTransferQuantity, line.WE_TransactionQuantity);
			AssertEquals("WE_OriginalInventoryStatus", inventory.WI_InventoryStatus, line.WE_OriginalInventoryStatus);
			AssertEquals("WE_WHC_NKOriginalInventoryHeldCode", inventory.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode, line.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("WE_CurrentInventoryStatus", inventory.WI_InventoryStatus, line.WE_CurrentInventoryStatus);
			AssertEquals("WE_WHC_NKCurrentInventoryHeldCode", inventory.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode, line.WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#endregion

		#region Customs Stuff

		protected override void TestIsCustomsTransactionCore()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(false, transfer.IsCustomsTransaction);
			AssertEquals(false, transfer.IsCustomsDataVisible);

			transfer.WD_WW_Whs = Helper.CreateWarehouse("1").PK;

			Helper.EnableWarehouseForBond(transfer.Warehouse, true);
			AssertEquals(true, transfer.IsCustomsTransaction);
			AssertEquals(true, transfer.IsCustomsDataVisible);
			Helper.EnableWarehouseForBond(transfer.Warehouse, false);

			Helper.EnableWarehouseForExcise(transfer.Warehouse, true);
			AssertEquals(true, transfer.IsCustomsTransaction);
			AssertEquals(true, transfer.IsCustomsDataVisible);
			Helper.EnableWarehouseForExcise(transfer.Warehouse, false);

			AssertEquals(false, transfer.IsCustomsTransaction);
			AssertEquals(false, transfer.IsCustomsDataVisible);
		}

		#endregion

		#region Business Object Overrides

		protected override void TestSetDefaultValuesCore(WhsTransfer docket)
		{
			base.TestSetDefaultValuesCore(docket);
			AssertEquals("WD_DocketType must be 'TFR'", CodeLists.DocketType.Codes.Transfer, docket.WD_DocketType);
			AssertEquals("WD_ExternalReference must be 'TRANSFER'", "TRANSFER", docket.WD_ExternalReference);
			AssertEquals("CreateUniqueReferenceOnSaving must be true", true, docket.IsUniqueExternalReferenceCreatedOnSave);
		}

		protected override void TestIsUniqueExternalReferenceCreatedOnSaveCore()
		{
			AssertEquals(true, Docket.IsUniqueExternalReferenceCreatedOnSave);
		}

		public void TestHumanReadableName()
		{
			Docket.WD_DocketID = "WT00001001";
			AssertEquals("Warehouse Transfer WT00001001", Docket.HumanReadableName);
		}

		#region TestOnSaved

		public void TestOnSaved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var masterTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			masterTransfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var childTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			AssertEquals(0, masterTransfer.RelatedJobs.Count);

			Factory.Save();
			AssertEquals(0, masterTransfer.RelatedJobs.Count);

			childTransfer.WD_WD_ParentDocket = masterTransfer.PK;
			masterTransfer.WD_TotalWeight = 1m; // make changes so OnSaved will be called.
			Factory.Save();
			AssertEquals(0, masterTransfer.RelatedJobs.Count);

			masterTransfer.NewRelatedJobsAdded = true;
			masterTransfer.WD_TotalWeight = 2m; // make changes so OnSaved will be called.
			Factory.Save();
			AssertEquals(1, masterTransfer.RelatedJobs.Count);
		}

		#endregion

		#region TestDelete_WithFinalisedLines

		public void TestDelete_WithFinalisedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Precondition", false, transfer.IsFinalised);
			AssertEquals("Transfers with finalised lines should not be deletable.", false, transfer.CanDelete);
			AssertEquals("There is at least one picked, transferred or finalized line on this transfer, preventing this transfer from being deleted.", transfer.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDelete_WithPickedLines

		public void TestDelete_WithPickedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			AssertEquals("Precondition", true, transfer.CanDelete);
			AssertEquals("Precondition", true, string.IsNullOrEmpty(transfer.ReasonForNotAbleToDelete));

			var expectedErrorMessage = "There is at least one picked, transferred or finalized line on this transfer, preventing this transfer from being deleted.";

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Transfers with picked lines should not be deletable.", false, transfer.CanDelete);
			AssertEquals(expectedErrorMessage, transfer.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDelete_WithCommencedLog

		public void TestDelete_WithCommencedLog()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			AssertEquals("Commenced transfer should be deletable.", true, transfer.CanDelete);
			AssertNull(transfer.ReasonForNotAbleToDelete);

			var commencedLog = transfer.Logs.AddNew(Events.ServiceCommenced);
			AssertEquals("Commenced transfer should not be deletable.", false, transfer.CanDelete);
			AssertEquals("This transfer has been commenced and cannot be deleted.", transfer.ReasonForNotAbleToDelete);

			commencedLog.Cancel();
			AssertEquals(true, transfer.CanDelete);
			AssertNull(transfer.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDelete_WithCommencedLog_Noline

		public void TestDelete_WithCommencedLog_Noline()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			AssertEquals("Commenced transfer should be deletable.", true, transfer.CanDelete);
			AssertNull(transfer.ReasonForNotAbleToDelete);

			var commencedLog = transfer.Logs.AddNew(Events.ServiceCommenced);
			transferLine.Delete();
			AssertEquals("Commenced transfer should be deletable if no line.", true, transfer.CanDelete);
			AssertNull(transfer.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestDelete_VASOrderTransfer_DeleteInitialTransfer

		public void TestDelete_VASOrderTransfer_DeleteInitialTransfer()
		{
			TestDelete_VASOrderTransfer_Core(vo => vo.WVO_WD_TransferIntoServiceArea, vo => vo.GetOrCreateInitialTransfer(Notify));
		}

		public void TestDelete_VASOrderTransfer_DeleteReturnTransfer()
		{
			WhsTransfer initialTransfer = null;

			TestDelete_VASOrderTransfer_Core(vo => vo.WVO_WD_TransferOutOfServiceArea,
				vo =>
				{
					initialTransfer = vo.GetOrCreateInitialTransfer(Notify);
					initialTransfer.NotificationManager.Push(Notify);
					initialTransfer.FinaliseDocket();
					vo.WVO_WorkCompletedTimeUtc = ZDateTime.UtcNow;
					Factory.Save();

					WhsTransfer returnTransfer;
					using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
					{
						returnTransfer = vo.GetOrCreateReturnTransfer(Notify);
					}

					return returnTransfer;
				},
				vo => AssertEquals("Should not unlink initial transfer", initialTransfer.PK, vo.WVO_WD_TransferIntoServiceArea));

			AssertEquals(false, initialTransfer.IsDeleted);
		}

		void TestDelete_VASOrderTransfer_Core(Func<WhsVASOrder, ZGuid> getFK, Func<WhsVASOrder, WhsTransfer> createTransfer, Action<WhsVASOrder> additionalAssertion = null)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var vasOrderTransfer = createTransfer(vasOrder);
			AssertNotNull("Precondition", vasOrderTransfer);
			AssertEquals(true, vasOrderTransfer.CanDelete);
			AssertEquals("Precondition", vasOrderTransfer.PK, getFK(vasOrder));

			AssertNoExceptionThrown(() => vasOrderTransfer.Delete());
			AssertEquals(true, vasOrderTransfer.IsDeleted);
			AssertEquals("Should be unlinked", ZGuid.Empty, getFK(vasOrder));

			AssertNoExceptionThrown(() => Factory.Save());

			if (additionalAssertion != null)
			{
				additionalAssertion(vasOrder);
			}
		}

		#endregion

		#region TestOnWarehouseChanged

		[TestDate(2023, 12, 14, 2, 10, 0)]
		public void TestOnWarehouseChanged_PopulateWD_ArrivalDateDateIfRequired()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;

			AssertEquals("WD_ArrivalDate should be empty with new adjustment.", ZDateTimeOffset.Empty, docket.WD_ArrivalDate);

			docket.WD_WW_Whs = whs.PK;
			AssertEquals(new ZDateTimeOffset(2023, 12, 14, 10, 10, 0, TimeSpan.FromHours(8)), docket.WD_ArrivalDate);
		}

		[TestDate(2023, 12, 14, 2, 10, 0)]
		public void TestOnWarehouseChanged_PopulateWD_ArrivalDateIfRequired_OnlyIfNotInDBAndIsEmpty()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			var arrivalDate = new ZDateTimeOffset(2012, 3, 4, 5, 0, 0, TimeSpan.FromHours(2));
			docket.WD_ArrivalDate = arrivalDate;
			AssertEquals("WD_ArrivalDate is set.", arrivalDate, docket.WD_ArrivalDate);

			docket.WD_WW_Whs = whs.PK;
			AssertEquals("WD_ArrivalDate won't change.", arrivalDate, docket.WD_ArrivalDate);
		}

		#endregion

		#endregion

		#region Related Entities

		#region TestLines

		protected override Type ExpectedLineCollectionType => typeof(WhsTransferLineCollection);

		public void TestLines_IsOriginalInventory()
		{
			var transfer = GetNewBusinessObject();
			var transferLine1 = transfer.Lines.AddNew();
			var transferLine2 = transfer.Lines.AddNew();
			AssertEquals("Both lines should be in the collection", 2, transfer.Lines.Count);

			transferLine2.WE_IsOriginalInventory = false;
			AssertEquals("Only original inventory lines should be in the collection.", 1, transfer.Lines.Count);
			transfer.Lines.Single(l => l.PK == transferLine1.PK);
		}

		#endregion

		public override void TestShouldUpdateWeightAndVolumeOnTheFly()
		{
			Docket.WD_WeightVolSetFromImport = true;
			AssertEquals(false, Docket.ShouldUpdateWeightAndVolumeOnTheFly);
			Docket.WD_WeightVolSetFromImport = false;
			AssertEquals(false, Docket.ShouldUpdateWeightAndVolumeOnTheFly);
		}

		#region TestMasterTransfer

		public void TestMasterTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			AssertNull("Precondition - transfer should have no parent.", transfer1.MasterTransfer);
			AssertNull("Precondition - transfer should have no parent.", transfer2.MasterTransfer);
			AssertNull("Precondition - transfer should have no parent.", transfer3.MasterTransfer);

			transfer2.WD_WD_ParentDocket = transfer1.PK;
			transfer2.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			transfer3.WD_WD_ParentDocket = transfer2.PK;
			transfer3.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			AssertNull(transfer1.MasterTransfer);
			AssertEquals(transfer1, transfer2.MasterTransfer);
			AssertEquals(transfer2, transfer3.MasterTransfer);
		}

		#endregion

		#region TestChildTransfers

		public void TestChildTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			AssertEquals("Precondition - transfer should have no children.", 0, transfer1.ChildTransfers.Count());
			AssertEquals("Precondition - transfer should have no children.", 0, transfer2.ChildTransfers.Count());
			AssertEquals("Precondition - transfer should have no children.", 0, transfer3.ChildTransfers.Count());

			transfer2.WD_WD_ParentDocket = transfer1.PK;
			transfer3.WD_WD_ParentDocket = transfer1.PK;
			AssertContainsExactElementsInAnyOrder(new WhsTransfer[] { transfer2, transfer3 }, transfer1.ChildTransfers);
			AssertEquals(0, transfer2.ChildTransfers.Count());
			AssertEquals(0, transfer3.ChildTransfers.Count());
		}

		#endregion

		#region TestLinesToValidateWhenFinalising

		public void TestLinesToValidateWhenFinalising()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2");
			AssertContainsExactElementsInAnyOrder("The only time transfer should have finalising lines is while finalising, should return all transfer lines otherwise.",
				new[] { transferLine1, transferLine2, transferLine3 }, transfer.LinesToValidateWhenFinalising);

			transfer.ValidateAndFinaliseDocketLines(new[] { transferLine1, transferLine2 }, false);
			AssertContainsExactElementsInAnyOrder("The only time transfer should have finalising lines is while finalising, should return all transfer lines otherwise.",
				new[] { transferLine1, transferLine2, transferLine3 }, transfer.LinesToValidateWhenFinalising);
			AssertContainsExactElementsInAnyOrder("Both lines were previously unfinalised, so should be added to finalising collection.",
				new[] { transferLine1, transferLine2 }, transfer.LastFinalisingLines_ForTest);

			bool transferLineWasFinalised = false;

			transferLine3.WE_FinalisedDateInfo.ValueChanged += delegate
			{
				transferLineWasFinalised = transferLine3.IsFinalised;
				AssertContainsExactElementsInAnyOrder("Only currently finalising lines should be in collection.", new[] { transferLine3 }, transfer.LinesToValidateWhenFinalising);
			};

			transfer.ValidateAndFinaliseDocketLines(new[] { transferLine2, transferLine3 }, false);
			AssertContainsExactElementsInAnyOrder("The only time transfer should have finalising lines is while finalising, should return all transfer lines otherwise.",
				new[] { transferLine1, transferLine2, transferLine3 }, transfer.LinesToValidateWhenFinalising);
			AssertContainsExactElementsInAnyOrder("Only one line is unfinalised, so it should be added to finalising collection.",
				new[] { transferLine3 }, transfer.LastFinalisingLines_ForTest);
			AssertEquals("Transfer line should be finalised.", true, transferLineWasFinalised);
		}

		#endregion

		#endregion

		#region Notes

		#region TestNoteContextsForRelatedNotesSetup

		protected override void TestNoteContextsForRelatedNotesSetup(WhsTransfer docket)
		{
			var whsTransfer = docket;
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("Warehouse");
			CreateTestNoteCollection(client);
			CreateTestNoteCollection(warehouse);

			whsTransfer.WD_OH_Client = client.PK;
			whsTransfer.WD_WW_Whs = warehouse.PK;
		}

		#endregion

		#region TestNoteContextsForRelatedNotesAssertions

		protected override void TestNoteContextsForRelatedNotesAssertions(WhsTransfer whsTransfer)
		{
			Assert("Should always be 'Warehouse' module", (whsTransfer.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
			Assert("Should always be 'Out' direction", (whsTransfer.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.O) != 0);
			Assert("Should always be 'Internal' direction", (whsTransfer.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.I) != 0);
			Assert("Should always be 'Transfer' freight mode", (whsTransfer.GetNoteContextsForRelatedNotes().FreightMode & StmNoteContextFreightMode.T) != 0);

			AssertEquals("Visible Notes Count", 12, whsTransfer.Notes.VisibleNotes.Count);
		}

		#endregion

		#region TestNoteTypes

		protected override bool SupportsUnmatchedOrgNoteType => false;

		protected override bool SupportsUnrecognisedAdditionalReferenceType => false;

		#endregion

		#endregion

		#region Validation

		#region TestRunPreSaveValidation_UnCommitsExcessInventoryAndCommitsRequiredInventory

		public void TestRunPreSaveValidation_UnCommitsExcessInventoryAndCommitsRequiredInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine1.RunPreSaveValidation(); // commit inventory
			transferLine1.WE_TransactionQuantity = 1m;
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			AssertEquals("Precondition", 5m, transferLine1.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", 0m, transferLine2.GetQtyCommittedToThisLine());

			// unregister transfer lines as children to prevent validation being run on them
			transfer.UnRegisterEditableChildObject(transfer.Lines);
			transfer.RunPreSaveValidation();
			AssertEquals("Should have uncommitted excess inventory.", 1m, transferLine1.GetQtyCommittedToThisLine());
			AssertEquals("Should have committed required inventory.", 9m, transferLine2.GetQtyCommittedToThisLine());
		}

		#endregion

		#region TestRunPreSaveValidationCore_SynchronisesOffset

		public override void TestRunPreSaveValidationCore_SynchronisesOffset_UpdatesDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dateTime = new ZDateTime(2022, 06, 12, 12, 30, 00);
			var expectedOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(dateTime);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = expectedOffset;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation();

			transferLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			transfer.RunPreSaveValidation();

			Assert("Docket should not be in error", !transfer.HasErrors);

			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				transferLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		#endregion

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsTransferValidation);
		}

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsTransferLookups);
		}

		#endregion

		#region Properties

		#region TestWD_DocketSubType

		public void TestWD_DocketSubType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");

			// Changing one Inter Whs Transfer Type to another Inter Whs Transfer Type. Should clear locations.
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertTransferLine(transferLine1, ZGuid.Empty, "", data.Whs1.PK, "");
			AssertTransferLine(transferLine2, ZGuid.Empty, "", data.Whs1.PK, "");

			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			AssertTransferLine(transferLine1, data.Whs1.PK, "", ZGuid.Empty, "");
			AssertTransferLine(transferLine2, data.Whs1.PK, "", ZGuid.Empty, "");
			transferLine1.TransferFromLocationString = "A-1";
			transferLine2.TransferFromLocationString = "A-1";

			// Changing from Inter Whs Transfer to Internal Transfer.
			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			transferLine1.LocationString = "A-2";
			transferLine2.LocationString = "A-2";
			AssertTransferLine(transferLine1, data.Whs1.PK, "A-1", data.Whs1.PK, "A-2");
			AssertTransferLine(transferLine2, data.Whs1.PK, "A-1", data.Whs1.PK, "A-2");

			// Changing from Internal Transfer to Inter Whs Transfer should clear dest location well as dest Whs.
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			AssertTransferLine(transferLine1, data.Whs1.PK, "A-1", ZGuid.Empty, "");
			AssertTransferLine(transferLine2, data.Whs1.PK, "A-1", ZGuid.Empty, "");

			// Changing from Internal Transfer to Inter Whs Destination Transfer.
			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			transferLine1.LocationString = "A-2";
			transferLine2.LocationString = "A-2";
			AssertTransferLine(transferLine1, data.Whs1.PK, "A-1", data.Whs1.PK, "A-2");
			AssertTransferLine(transferLine2, data.Whs1.PK, "A-1", data.Whs1.PK, "A-2");

			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertTransferLine(transferLine1, ZGuid.Empty, "", data.Whs1.PK, "A-2");
			AssertTransferLine(transferLine2, ZGuid.Empty, "", data.Whs1.PK, "A-2");
		}

		void AssertTransferLine(WhsTransferLine transferLine, ZGuid expectedTransferFromWhsPK, ZString expectedTransferFromLocationString, ZGuid expectedWhsPK, ZString expectedLocationString)
		{
			AssertEquals(expectedTransferFromWhsPK, transferLine.TransferFromWarehousePK);
			AssertEquals(expectedTransferFromLocationString, transferLine.TransferFromLocationString);
			AssertEquals(expectedWhsPK, transferLine.DestinationWarehousePK);
			AssertEquals(expectedLocationString, transferLine.LocationString);
		}

		#endregion

		#region TestWD_WW_Whs_ModifyLines

		protected override void TestWD_WW_Whs_ModifyLines(WhsTransfer docket, WhsWarehouse whs)
		{
			var whs2 = Helper.CreateWarehouse("WH2");
			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			AssertEquals(whs.PK, line1.TransferFromWarehousePK);
			AssertEquals(whs.PK, line2.TransferFromWarehousePK);

			docket.WD_WW_Whs = whs2.PK;
			AssertEquals(whs2.PK, line1.TransferFromWarehousePK);
			AssertEquals(whs2.PK, line2.TransferFromWarehousePK);
		}

		#endregion

		#region TestSubTypeDesc

		public override void TestSubTypeDesc()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(TransferType.Descriptions.Internal, transfer.SubTypeDesc);

			transfer.WD_IsPutawayTransfer = true;
			AssertEquals("Putaway", transfer.SubTypeDesc);

			transfer.WD_IsPutawayTransfer = false;
			transfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();
			AssertEquals("Outbound Dock Door Transfer", transfer.SubTypeDesc);

			transfer.WD_WP_ParentPickForTransfer = ZGuid.Empty;
			transfer.WD_IsPickFaceReplenishment = true;
			AssertEquals("Auto Created Replenishment Transfer", transfer.SubTypeDesc);

			transfer.WD_IsPickFaceReplenishment = false;
			AssertEquals(TransferType.Descriptions.Internal, transfer.SubTypeDesc);
		}

		#endregion

		#region TestIsAutoCreatedTransfer

		public void TestIsAutoCreatedTransfer()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(false, transfer.IsAutoCreatedTransfer);

			transfer.WD_IsPutawayTransfer = true;
			AssertEquals(true, transfer.IsAutoCreatedTransfer);

			transfer.WD_IsPutawayTransfer = false;
			transfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();
			AssertEquals(true, transfer.IsAutoCreatedTransfer);

			transfer.WD_WP_ParentPickForTransfer = ZGuid.Empty;
			Factory.New<WhsVASOrder>().WVO_WD_TransferIntoServiceArea = transfer.PK;
			AssertEquals(true, transfer.IsAutoCreatedTransfer);
		}

		#endregion

		#region TestIsInterWarehouseTransfer

		public void TestIsInterWarehouseTransfer()
		{
			Docket.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsSource;
			AssertEquals(true, Docket.IsInterWarehouseTransfer);
			Docket.WD_DocketSubType = CodeLists.TransferType.Codes.Internal;
			AssertEquals(false, Docket.IsInterWarehouseTransfer);
			Docket.WD_DocketSubType = CodeLists.TransferType.Codes.InterWhsDest;
			AssertEquals(true, Docket.IsInterWarehouseTransfer);
		}

		#endregion

		#region TestIsMasterTransfer

		public void TestIsMasterTransfer()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(true, transfer.IsMasterTransfer);
			AssertEquals(null, transfer.MasterTransfer);

			var childTransfer = GetNewBusinessObject();
			childTransfer.WD_DocketSubType = "";
			childTransfer.WD_WD_ParentDocket = transfer.PK;
			AssertEquals(false, childTransfer.IsMasterTransfer);
			AssertEquals(transfer, childTransfer.MasterTransfer);

			childTransfer.WD_DocketSubType = TransferType.Codes.Internal;
			AssertEquals(true, childTransfer.IsMasterTransfer);
			AssertEquals(null, childTransfer.MasterTransfer);

			childTransfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertEquals(false, childTransfer.IsMasterTransfer);
			AssertEquals(transfer, childTransfer.MasterTransfer);

			childTransfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			AssertEquals(false, childTransfer.IsMasterTransfer);
			AssertEquals(transfer, childTransfer.MasterTransfer);

			childTransfer.WD_WD_ParentDocket = Factory.New<WhsOrder>().PK;
			AssertEquals(false, childTransfer.IsMasterTransfer);

			WhsDocket m;
			AssertExceptionThrown<ApplicationException>("Can't create a WhsTransfer as its constructor threw an exception", () => { m = childTransfer.MasterTransfer; });
		}

		#endregion

		#region TestIsReturnStockTransfer

		public void TestIsReturnStockTransfer()
		{
			var transfer1 = GetNewBusinessObject();
			var transfer2 = GetNewBusinessObject();
			AssertEquals("Regular transfer should *not* be a return stock transfer.", false, transfer1.IsReturnStockTransfer);
			AssertEquals("Regular transfer should *not* be a return stock transfer.", false, transfer2.IsReturnStockTransfer);

			transfer2.WD_WD_ParentDocket = transfer1.PK;
			AssertEquals("Precondition: Subtype.", TransferType.Codes.Internal, transfer2.WD_DocketSubType);
			AssertEquals("Regular transfer should *not* be a return stock transfer.", true, transfer2.IsReturnStockTransfer);

			transfer2.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertEquals("Inter-Whs Child should *not* be a return stock transfer.", false, transfer2.IsReturnStockTransfer);

			transfer2.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			AssertEquals("Inter-Whs Child should *not* be a return stock transfer.", false, transfer2.IsReturnStockTransfer);

			transfer2.WD_WD_ParentDocket = ZGuid.Empty;
			AssertEquals("Inter-Whs Parent should *not* be a return stock transfer.", false, transfer2.IsReturnStockTransfer);
		}

		#endregion

		#region TestIsTransferringForOrder

		public void TestIsTransferringForOrder()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(false, transfer.IsTransferringForOrder);

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			AssertEquals(true, transfer.IsTransferringForOrder);
		}

		#endregion

		#region TestIsTransferLinkedToPickForReplenishment

		public void TestIsTransferLinkedToPickForReplenishment()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(false, transfer.IsTransferLinkedToPickForReplenishment);

			transfer.WD_WP_PickBeingReplenished = Factory.New<WhsPick>().PK;
			AssertEquals(true, transfer.IsTransferLinkedToPickForReplenishment);
		}

		#endregion

		#region TestPickBeingReplenished

		public void TestPickBeingReplenished()
		{
			var transfer = GetNewBusinessObject();
			var pick = Factory.New<WhsPick>();
			AssertNull(transfer.PickBeingReplenished);

			transfer.WD_WP_PickBeingReplenished = pick.PK;
			AssertEquals(pick.PK, transfer.PickBeingReplenished.PK);
		}

		#endregion

		#region TestWD_WP_PickBeingReplenished

		public void TestWD_WP_PickBeingReplenished_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();

			AssertEquals("Precondition: Docket.WD_WW_Whs is empty", ZGuid.Empty, docket.WD_WW_Whs);
			AssertEquals("Precondition: WD_WP_PickBeingReplenished should be readonly.", true, docket.WD_WP_PickBeingReplenishedInfo.ReadOnly);

			docket.WD_OH_Client = data.Org1.PK;
			AssertEquals("WD_WP_PickBeingReplenished should be readonly. If only client is set.", true, docket.WD_WP_PickBeingReplenishedInfo.ReadOnly);

			docket.WD_WW_Whs = data.Whs1.PK;
			AssertEquals("WD_WP_PickBeingReplenished should not be readonly. If both client and Whs is set.", false, docket.WD_WP_PickBeingReplenishedInfo.ReadOnly);
		}

		public void TestWD_WP_PickBeingReplenished_ClearedOnWhsChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("GSR");
			var pick = Factory.New<WhsPick>();

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_WP_PickBeingReplenished = pick.PK;

			docket.WD_WW_Whs = whs2.PK;
			AssertEquals("Docket.WD_WP_PickBeingReplenished should be empty", ZGuid.Empty, docket.WD_WP_PickBeingReplenished);

			docket.WD_WP_PickBeingReplenished = pick.PK;
			docket.WD_WW_Whs = whs2.PK;
			AssertEquals("Docket.WD_WP_PickBeingReplenished should be the same as warehouse did not change", pick.PK, docket.WD_WP_PickBeingReplenished);

			docket.WD_WW_Whs = ZGuid.Empty;
			AssertEquals("Docket.WD_WP_PickBeingReplenished should be empty", ZGuid.Empty, docket.WD_WP_PickBeingReplenished);
		}

		#endregion

		#region TestIsFinalisingLines

		public void TestIsFinalisingLines()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(false, transfer.IsFinalisingLines);

			using (new SemaphoreManager(transfer.FinaliseDocketLineSemaphore))
			{
				AssertEquals(true, transfer.IsFinalisingLines);
			}
		}

		#endregion

		#region TestIsUserAllowedToFinalise

		public void TestIsUserAllowedToFinalise()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			AssertEquals(false, transfer.IsUserAllowedToFinalise);

			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals(false, transfer.IsUserAllowedToFinalise);
			AssertEquals(true, transfer.ChildTransfers.ElementAt(0).IsUserAllowedToFinalise);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			AssertIsFinalisedPrecondition(transfer.ChildTransfers.ElementAt(0));
			AssertEquals(true, transfer.IsUserAllowedToFinalise);
			AssertEquals(true, transfer.ChildTransfers.ElementAt(0).IsUserAllowedToFinalise);
		}

		public void TestIsUserAllowedToFinalise_OutboundDockDoorTransfer()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(false, transfer.IsUserAllowedToFinalise);

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			AssertEquals(true, transfer.IsUserAllowedToFinalise);
		}

		#endregion

		#region TestIsVASOrderTransfer

		public void TestIsVASOrderTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var vasOrderTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", vasOrderTransfer);
			AssertEquals(true, vasOrderTransfer.IsVASOrderTransfer);

			var nonVasOrderTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			AssertEquals(false, nonVasOrderTransfer.IsVASOrderTransfer);

			var outOfServiceTransfer = GetNewBusinessObject();
			vasOrder.WVO_WD_TransferOutOfServiceArea = outOfServiceTransfer.PK;
			AssertEquals(true, outOfServiceTransfer.IsVASOrderTransfer);
		}

		#endregion

		#region TestHasAtLeastOneUnFinalisedTransferLine

		public void TestHasAtLeastOneUnFinalisedTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-1", "A-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "A-2");
			var transferLine4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 4m, "A-1", "A-2");
			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine1, transferLine2 }, false);
			AssertIsFinalisedPrecondition(transferLine1);
			AssertIsFinalisedPrecondition(transferLine2);
			AssertEquals("Precondition - ensure transfer line is not finalised.", false, transferLine3.IsFinalised);
			AssertEquals("Precondition - ensure transfer line is not finalised.", false, transferLine4.IsFinalised);

			AssertEquals(false, transfer.HasAtLeastOneUnFinalisedTransferLine(new WhsTransferLine[] { transferLine1 }));
			AssertEquals(false, transfer.HasAtLeastOneUnFinalisedTransferLine(new WhsTransferLine[] { transferLine1, transferLine2 }));
			AssertEquals(true, transfer.HasAtLeastOneUnFinalisedTransferLine(new WhsTransferLine[] { transferLine1, transferLine2, transferLine3 }));
			AssertEquals(true, transfer.HasAtLeastOneUnFinalisedTransferLine(new WhsTransferLine[] { transferLine3, transferLine4 }));
		}

		#endregion

		#region TestNewRelatedJobsAdded

		public void TestNewRelatedJobsAdded()
		{
			var transfer = GetNewBusinessObject();
			AssertEquals(false, transfer.NewRelatedJobsAdded);

			transfer.NewRelatedJobsAdded = true;
			AssertEquals(true, transfer.NewRelatedJobsAdded);
		}

		#endregion

		#region TestCanCreateInventory

		public void TestCanCreateInventory()
		{
			AssertEquals("Transfer always modify existing and create new Inventroy.", true, Docket.CanCreateInventory);
		}

		#endregion

		#region TestStandardReadOnly

		protected override void TestStandardReadOnlyCore(Func<WhsTransfer, bool> getReadOnly, string name, WhsTransfer docket, bool readOnlyWhenDocketHasLines)
		{
			var transfer = docket;
			var childTransfer = GetNewBusinessObject();
			childTransfer.WD_WD_ParentDocket = transfer.PK;
			childTransfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertEquals(false, getReadOnly(transfer));
			AssertEquals(true, getReadOnly(childTransfer));

			var transferLine = transfer.Lines.AddNew();
			AssertEquals(readOnlyWhenDocketHasLines, getReadOnly(transfer));

			transferLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			transferLine.WE_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals(true, getReadOnly(transfer));
		}

		#endregion

		#region TestTransferReadOnly_TaskPlanningStatus

		public void TestTransferReadOnly_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Factory.Save();
			AssertEquals("Precondition", false, transfer.ReadOnly);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals("The property should be readonly when ready for planning.", true, transfer.ReadOnly);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals("The property should not be readonly when not ready for planning.", false, transfer.ReadOnly);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals("The property should be readonly when planned.", true, transfer.ReadOnly);

			transfer.WD_TaskPlanningStatus = "";
			AssertEquals("The property should not be readonly when task planning status is empty.", false, transfer.ReadOnly);
		}

		#endregion

		#region TestReadonlyPropertiesForVASOrderTransfer

		public void TestReadonlyPropertiesForVASOrderTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var vasOrderTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", vasOrderTransfer);
			AssertEquals(true, vasOrderTransfer.WD_OH_ClientInfo.ReadOnly);
			AssertEquals(true, vasOrderTransfer.WD_WW_WhsInfo.ReadOnly);
			AssertEquals(true, vasOrderTransfer.WD_DocketSubTypeInfo.ReadOnly);
			AssertEquals(false, vasOrderTransfer.WD_ExternalReferenceInfo.ReadOnly);

			var outOfServiceTransfer = GetNewBusinessObject();
			vasOrder.WVO_WD_TransferOutOfServiceArea = outOfServiceTransfer.PK;
			AssertEquals(true, outOfServiceTransfer.WD_OH_ClientInfo.ReadOnly);
			AssertEquals(true, outOfServiceTransfer.WD_WW_WhsInfo.ReadOnly);
			AssertEquals(true, outOfServiceTransfer.WD_DocketSubTypeInfo.ReadOnly);
			AssertEquals(false, outOfServiceTransfer.WD_ExternalReferenceInfo.ReadOnly);
		}

		#endregion

		#region TestReadOnlyPropertiesForOutboundDockDoorTransfer

		public void TestReadOnlyPropertiesForOutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			AssertEquals(true, transfer.WD_OH_ClientInfo.ReadOnly);
			AssertEquals(true, transfer.WD_WW_WhsInfo.ReadOnly);
			AssertEquals(true, transfer.WD_DocketSubTypeInfo.ReadOnly);
			AssertEquals(false, transfer.WD_ExternalReferenceInfo.ReadOnly);
		}

		#endregion

		#region TestReadOnly_InterWhsChild

		#region TestReadOnly_InterWhsChild_Source

		public void TestReadOnly_InterWhsChild_Source()
		{
			TestReadOnly_InterWhsChild_Core(parentType: TransferType.Codes.InterWhsDest); // Parent = Dest
		}

		#endregion

		#region TestReadOnly_InterWhsChild_Destination

		public void TestReadOnly_InterWhsChild_Destination()
		{
			TestReadOnly_InterWhsChild_Core(parentType: TransferType.Codes.InterWhsSource); // Parent = Source
		}

		#endregion

		#region TestReadOnly_InterWhsChild_Core

		void TestReadOnly_InterWhsChild_Core(string parentType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, parentType == TransferType.Codes.InterWhsDest ? whs2 : data.Whs1);
			AssertEquals("Precondition: Transfer should *not* be ReadOnly.", false, transfer.ReadOnly);
			transfer.DocketSubType = parentType;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", parentType == TransferType.Codes.InterWhsDest ? data.Whs1.PK : whs2.PK, "A");
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Child Transfer created.", 1, transfer.ChildTransfers.Count());

			var childTransfer = transfer.ChildTransfers.ElementAt(0);
			AssertEquals("Transfer should *not* be ReadOnly.", false, transfer.ReadOnly);
			AssertEquals("ChildTransfer should be ReadOnly.", true, childTransfer.ReadOnly);
			AssertEquals("ChildTransfer should *not* be deletable.", false, childTransfer.CanDelete);
		}

		#endregion

		#endregion

		#region TestTransfersDoNotPropagatesFinalisedDateAndStatusToLines

		public void TestTransfersDoNotPropagatesFinalisedDateAndStatusToLines()
		{
			AssertEquals(false, Docket.PropagatesFinalisedDateAndStatusToLines);
		}

		#endregion

		#region TestDocketSubType

		public void TestDocketSubType()
		{
			var transfer = GetNewBusinessObject();
			var transferInterface = (IWhsTransfer)transfer;
			transfer.WD_IsPutawayTransfer = true;
			AssertEquals(NonPersistentTransferType.Codes.Putaway, transfer.DocketSubType);
			AssertEquals(NonPersistentTransferType.Codes.Putaway, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.WD_IsPutawayTransfer = false;
			AssertEquals(transfer.WD_DocketSubType, transfer.DocketSubType);
			AssertEquals(transfer.WD_DocketSubType, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.DocketSubType = NonPersistentTransferType.Codes.Putaway;
			AssertEquals(NonPersistentTransferType.Codes.Putaway, transfer.DocketSubType);
			AssertEquals(NonPersistentTransferType.Codes.Putaway, transferInterface.DocketSubType);
			AssertHasError(transfer.DocketSubTypeInfo, "Enter a valid Docket Sub Type.");

			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			AssertEquals(transfer.WD_DocketSubType, transfer.DocketSubType);
			AssertEquals(transfer.WD_DocketSubType, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();
			AssertEquals(NonPersistentTransferType.Codes.OutboundDockDoor, transfer.DocketSubType);
			AssertEquals(NonPersistentTransferType.Codes.OutboundDockDoor, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.WD_WP_ParentPickForTransfer = ZGuid.Empty;
			AssertEquals(transfer.WD_DocketSubType, transfer.DocketSubType);
			AssertEquals(transfer.WD_DocketSubType, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.DocketSubType = NonPersistentTransferType.Codes.OutboundDockDoor;
			AssertEquals(NonPersistentTransferType.Codes.OutboundDockDoor, transfer.DocketSubType);
			AssertEquals(NonPersistentTransferType.Codes.OutboundDockDoor, transferInterface.DocketSubType);
			AssertHasError(transfer.DocketSubTypeInfo, "Enter a valid Docket Sub Type.");

			transfer.WD_WP_ParentPickForTransfer = ZGuid.Empty;
			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			AssertEquals(transfer.WD_DocketSubType, transfer.DocketSubType);
			AssertEquals(transfer.WD_DocketSubType, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.WD_IsPickFaceReplenishment = true;
			AssertEquals(NonPersistentTransferType.Codes.AutoCreatedReplenishment, transfer.DocketSubType);
			AssertEquals(NonPersistentTransferType.Codes.AutoCreatedReplenishment, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.WD_IsPickFaceReplenishment = false;
			AssertEquals(transfer.WD_DocketSubType, transfer.DocketSubType);
			AssertEquals(transfer.WD_DocketSubType, transferInterface.DocketSubType);
			AssertNoErrors(transfer.DocketSubTypeInfo);

			transfer.DocketSubType = NonPersistentTransferType.Codes.AutoCreatedReplenishment;
			AssertEquals(NonPersistentTransferType.Codes.AutoCreatedReplenishment, transfer.DocketSubType);
			AssertEquals(NonPersistentTransferType.Codes.AutoCreatedReplenishment, transferInterface.DocketSubType);
			AssertHasError(transfer.DocketSubTypeInfo, "Enter a valid Docket Sub Type.");
		}

		#endregion

		#region TestWD_FinalisedDate

		public void TestWD_FinalisedDate_UpdateVersionIdPolicyWhenSet()
		{
			var receive = Factory.New<WhsTransfer>();
			AssertEquals("Concurrency Policy should be Ignore when WD_FinalisedDate is not set.", ConcurrencyPolicy.Ignore, receive.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Concurrency Policy should be Strict when WD_FinalisedDate is set.", ConcurrencyPolicy.Strict, receive.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals("Concurrency Policy should be Ignore when WD_FinalisedDate is not set.", ConcurrencyPolicy.Ignore, receive.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWD_TaskPlanningStatus

		public void TestWD_TaskPlanningStatus_InitialSave_ShouldSetToNotReadyForPlanningAfterWarehouseChanged()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse.PK;
				Factory.Save();
				AssertEquals("Task Planning Status should be Not Ready For Planning after changed to warehouse with valid release group on initial save", TaskPlanningStatus.Codes.NotReady, transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_ShouldSetToNotReadyForPlanningAfterWarehouseChanged()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				Factory.Save();
				AssertNullOrEmptyOrWhitespace("Precondition", transfer.WD_TaskPlanningStatus);

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse.PK;
				Factory.Save();

				AssertEquals("Task Planning Status should be Not Ready For Planning after changed to warehouse with valid release group", TaskPlanningStatus.Codes.NotReady, transfer.WD_TaskPlanningStatus);

				var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 2);
				transfer.WD_WW_Whs = warehouse2.PK;
				Factory.Save();

				AssertNullOrEmpty("Task Planning Status should be empty after changed to warehouse without valid release group", transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_ShouldNotChangeIfFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			AssertNullOrEmpty(transfer.WD_TaskPlanningStatus);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();
			AssertNullOrEmpty("Task Planning Status should not be NRP after changed to warehouse with valid release group", transfer.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_ShouldNotChangeIfCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var warehouse = Helper.CreateWarehouse("WH1", "A", 2, 2);
			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			transfer.WD_WW_Whs = warehouse.PK;

			Factory.Save();
			AssertNullOrEmpty("Task Planning Status should not be changed to NRP after changed to warehouse with valid release group", transfer.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_ShouldNotChangeForAutoCreatedTransfers_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var warehouse = Helper.CreateWarehouse("WH1", "A", 2, 2);
			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			transfer.WD_WW_Whs = warehouse.PK;
			Factory.Save();

			AssertEquals("Task Planning Status should not be changed to NRP after changed warehouse with valid release group for auto created transfers", TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);

			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 2);
			transfer.WD_WW_Whs = warehouse2.PK;
			Factory.Save();

			AssertEquals("Task Planning Status should not be changed to empty after changed to warehouse without valid release group for auto created transfers", TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_ShouldNotChangeForAutoCreatedTransfers_TransferForOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer1.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			transfer1.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
			warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
			transfer1.WD_WW_Whs = warehouse1.PK;
			Factory.Save();

			AssertEquals("Task Planning Status should not be changed to NRP after changed to warehouse with valid release group for auto created transfers", TaskPlanningStatus.Codes.Ready, transfer1.WD_TaskPlanningStatus);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer2.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			transfer2.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;

			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 2);
			transfer2.WD_WW_Whs = warehouse2.PK;
			Factory.Save();

			AssertEquals("Task Planning Status should not be changed to empty after changed to warehouse without valid release group for auto created transfers", TaskPlanningStatus.Codes.Planned, transfer2.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_ShouldBeClearAfterFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			AssertNullOrEmpty(transfer.WD_TaskPlanningStatus);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);

			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);
			Factory.Save();
			AssertNullOrEmpty(transfer.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_ReplenishmentTransferSetToReady_EnableTransition()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				transfer.WD_IsPickFaceReplenishment = true;

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse1.PK;
				Factory.Save();

				AssertEquals("Transfer is Replenishment", true, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Replenishment Transfer Task Planning Status should transit to Ready when registry AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning set to true", TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_ReplenishmentTransferSetToReady_DoesNotSetToReadyAfterUserChangeTheStatus()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				transfer.WD_IsPickFaceReplenishment = true;

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse1.PK;
				Factory.Save();

				AssertEquals("Transfer is Replenishment", true, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Replenishment Transfer Task Planning Status should transit to Ready when registry AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning set to true", TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);

				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				Factory.Save();

				AssertEquals("Transfer is Replenishment", true, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Replenishment Transfer Task Planning Status should not transit to Ready when user manually set to other status", TaskPlanningStatus.Codes.NotReady, transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_ReplenishmentTransferSetToReady_DoesNotSetToReadyWhenStatusIsError()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				transfer.WD_IsPickFaceReplenishment = true;

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse1.PK;
				transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Error;
				Factory.Save();

				AssertEquals("Transfer is Replenishment", true, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Replenishment Transfer Task Planning Status should not transit to Ready when current task planning status is Error", TaskPlanningStatus.Codes.Error, transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_ReplenishmentTransferSetToReady_DisableTransition()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				transfer.WD_IsPickFaceReplenishment = true;

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse1.PK;
				Factory.Save();

				AssertEquals("Transfer is Replenishment", true, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Replenishment Transfer Task Planning Status should not transit to Ready when registry AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning set to false", TaskPlanningStatus.Codes.NotReady, transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_NonReplenishmentTransferSetToReady_EnableTransition()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse1.PK;
				Factory.Save();

				AssertEquals("Transfer is not Replenishment", false, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Non Replenishment Transfer Task Planning Status should transit to Ready when registry AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning set to true", TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_NonReplenishmentTransferSetToReady_DoesNotSetToReadyAfterUserChangeTheStatus()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse1.PK;
				Factory.Save();

				AssertEquals("Transfer is not Replenishment", false, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Non Replenishment Transfer Task Planning Status should transit to Ready when registry AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning set to true", TaskPlanningStatus.Codes.Ready, transfer.WD_TaskPlanningStatus);

				transfer.WD_TaskPlanningStatus = string.Empty;
				Factory.Save();

				AssertEquals("Transfer is not Replenishment", false, transfer.WD_IsPickFaceReplenishment);
				AssertNullOrEmpty("Non Replenishment Transfer Task Planning Status should not transit to Ready when user manually set to other status", transfer.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_NonReplenishmentTransferSetToReady_DisableTransition()
		{
			using (WarehouseDataRegistry.Instance.AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);

				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 2);
				warehouse1.WW_GG_ReleaseGroup = releaseGroup.PK;
				transfer.WD_WW_Whs = warehouse1.PK;
				Factory.Save();

				AssertEquals("Transfer is not Replenishment", false, transfer.WD_IsPickFaceReplenishment);
				AssertEquals("Non Replenishment Transfer Task Planning Status should not transit to Ready when registry AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning set to false", TaskPlanningStatus.Codes.NotReady, transfer.WD_TaskPlanningStatus);
			}
		}

		#endregion

		#region TestTransferTaskPlanningStatusPrompt

		public void TestPutawayTransferShouldNotHaveTaskPlanningStatusPrompt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;

			transfer.WD_TaskPlanningStatus = string.Empty;
			AssertNullOrEmptyOrWhitespace("", transfer.TaskPlanningStatusPrompt);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertNullOrEmptyOrWhitespace("", transfer.TaskPlanningStatusPrompt);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertNullOrEmptyOrWhitespace("", transfer.TaskPlanningStatusPrompt);

			transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertNullOrEmptyOrWhitespace("", transfer.TaskPlanningStatusPrompt);
		}

		protected override bool SupportsPlanningStatusPrompt => true;

		#endregion

		#endregion

		#region Allocate Same Destination Locations

		public void TestAllocateSameDestinationLocations()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();

			line1.TransferFromLocationString = "A-1-1";
			line1.LocationString = "A-4-1";
			line2.TransferFromLocationString = "A-2-1";
			line3.TransferFromLocationString = "A-3-1";

			var selectedLines = new WhsTransferLine[2] { line1, line3 };

			transfer.AllocateSameDestinationLocations(selectedLines);

			AssertEquals("", transfer.Lines[1].LocationString);
			AssertEquals("A-4-1", transfer.Lines[2].LocationString);

			line1.LocationString = "";
			transfer.AllocateSameDestinationLocations(selectedLines);

			AssertEquals("A-4-1", transfer.Lines[2].LocationString);
			AssertEquals("Don't want to overwrite user entered values", "A-4-1", transfer.Lines[2].LocationString);
		}

		public void TestAllocateSameDestinationLocationsDoesNotExecuteIfFinalisedOrCancelled()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.TransferFromLocationString = "A-1-1";
			line1.LocationString = "A-3-1";
			line2.TransferFromLocationString = "A-2-1";

			var selectedLines = new WhsTransferLine[2] { line1, line2 };

			transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			transfer.AllocateSameDestinationLocations(selectedLines);
			AssertEquals("Should not be updated because the Transfer is finalised", "", transfer.Lines[1].LocationString);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			transfer.WD_FinalisedDate = ZDateTimeOffset.Empty;

			((NotificationBuffer)transfer.NotificationManager.Peek).Clear();
			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			transfer.AllocateSameDestinationLocations(selectedLines);
			AssertEquals("Should not be updated because the Transfer is cancelled", "", transfer.Lines[1].LocationString);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestAllocateSameDestinationLocationsDoesNotExecuteIfOutboundDockDoorTransfer()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.TransferFromLocationString = "A-1-1";
			line1.LocationString = "A-3-1";
			line2.TransferFromLocationString = "A-2-1";

			var selectedLines = new[] { line1, line2 };

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			transfer.AllocateSameDestinationLocations(selectedLines);
			AssertEquals("Should not be updated because the Transfer is an Outbound Dock Door Transfer.", "", line2.LocationString);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CannotPerformThisOnOutboundDockDoorTransfer));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAllocateSameDestinationLocationsChecksArguments()
		{
			Docket.AllocateSameDestinationLocations(null);
		}

		#endregion

		#region Clear Destination Locations

		public void TestClearDestinationLocations()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();

			line1.LocationString = "A-1-1";
			line2.LocationString = "A-2-1";
			line3.LocationString = "A-3-1";

			var selectedLines = new WhsTransferLine[2] { line1, line3 };

			transfer.ClearDestinationLocations(selectedLines);

			AssertEquals("", line1.LocationString);
			AssertEquals("A-2-1", line2.LocationString);
			AssertEquals("", line3.LocationString);
		}

		public void TestClearDestinationLocationsDoesNotClearIfFinalisedOrCancelled()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.LocationString = "A-1-1";
			line2.LocationString = "A-2-1";

			var selectedLines = new WhsTransferLine[2] { line1, line2 };

			transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			transfer.ClearDestinationLocations(selectedLines);
			AssertEquals("Should not be cleared because the Transfer is finalised", "A-1-1", transfer.Lines[0].LocationString);
			AssertEquals("Should not be cleared because the Transfer is finalised", "A-2-1", transfer.Lines[1].LocationString);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			transfer.WD_FinalisedDate = ZDateTimeOffset.Empty;

			((NotificationBuffer)transfer.NotificationManager.Peek).Clear();
			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			transfer.ClearDestinationLocations(selectedLines);
			AssertEquals("Should not be cleared because the Transfer is cancelled", "A-1-1", transfer.Lines[0].LocationString);
			AssertEquals("Should not be cleared because the Transfer is cancelled", "A-2-1", transfer.Lines[1].LocationString);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestClearDestinationLocationsDoesNotClearIfOutboundDockDoorTransfer()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.LocationString = "A-1-1";
			line2.LocationString = "A-2-1";

			var selectedLines = new[] { line1, line2 };

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			transfer.ClearDestinationLocations(selectedLines);
			AssertEquals("Should not be cleared because the Transfer is an Outbound Dock Door Transfer.", "A-1-1", line1.LocationString);
			AssertEquals("Should not be cleared because the Transfer is an Outbound Dock Door Transfer.", "A-2-1", line2.LocationString);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CannotPerformThisOnOutboundDockDoorTransfer));
		}

		public void TestClearDestinationLocationsDoesNotClearIfLineFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			var selectedLines = new WhsTransferLine[] { transferLine1, transferLine2 };
			transfer.ClearDestinationLocations(selectedLines);
			AssertEquals("Should not be cleared because the Transfer Line is finalised", "A-2", transferLine1.LocationString);
			AssertEquals("Should be cleared because the Transfer Line is *not* finalised", "", transferLine2.LocationString);

			transferLine2.LocationString = "A-2";
			using (new SemaphoreManager(transferLine2.FinaliseDocketLineSemaphore))
			{
				transferLine2.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			Assert("Precondition", transferLine2.LocationStringInfo.ReadOnly);

			transfer.ClearDestinationLocations(new[] { transferLine2 });
			AssertEquals("Should not be cleared because the destination location is read only.", "A-2", transferLine1.LocationString);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestClearDestinationLocationsChecksArguments()
		{
			Docket.ClearDestinationLocations(null);
		}

		#endregion

		#region Generate Destination PalletIDs

		public void TestGenerateDestinationPalletIDs()
		{
			var transfer = GetNewBusinessObject();
			transfer.WD_DocketID = "W0000666";
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();

			transfer.GenerateDestinationPalletIDs(new [] { line1, line3 });

			AssertEquals("W0000666-0001", line1.WE_PalletID);
			AssertEquals("", line2.WE_PalletID);
			AssertEquals("W0000666-0002", line3.WE_PalletID);

			transfer.GenerateDestinationPalletIDs(new [] { line2 });

			AssertEquals("W0000666-0001", line1.WE_PalletID);
			AssertEquals("W0000666-0003", line2.WE_PalletID);
			AssertEquals("W0000666-0002", line3.WE_PalletID);

			transfer.Lines[2].WE_PalletID = "ILikeThisOne";
			transfer.GenerateDestinationPalletIDs(new [] { line1, line2, line3 });

			AssertEquals("W0000666-0001", line1.WE_PalletID);
			AssertEquals("W0000666-0003", line2.WE_PalletID);
			AssertEquals("ILikeThisOne", line3.WE_PalletID);
		}

		public void TestGenerateDestinationPalletIDs_ShouldNotGenerateIfNoDocketIDSpecified()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			AssertNullOrEmpty("Precondition: ", transfer.WD_DocketID);
			var selectedLines = new WhsTransferLine[2] { line1, line2 };

			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer has no Docket ID", "", transfer.Lines[0].WE_PalletID);
			AssertEquals("Should not be generated because the Transfer has no Docket ID", "", transfer.Lines[1].WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.NoDocketIDSpecified));

			transfer.WD_DocketID = "W0000666";
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("W0000666-0001", line1.WE_PalletID);
			AssertEquals("W0000666-0002", line2.WE_PalletID);
		}

		public void TestGenerateDestinationPalletIDs_NoSelectedLines()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			transfer.WD_DocketID = "W0000666";
			transfer.GenerateDestinationPalletIDs(Array.Empty<WhsTransferLine>());
			AssertEquals("", line1.WE_PalletID);
			AssertEquals("", line2.WE_PalletID);

			transfer.GenerateDestinationPalletIDs(new[] { line2 });
			AssertEquals("", line1.WE_PalletID);
			AssertEquals("W0000666-0001", line2.WE_PalletID);

			transfer.GenerateDestinationPalletIDs(Array.Empty<WhsTransferLine>());
			AssertEquals("", line1.WE_PalletID);
			AssertEquals("W0000666-0001", line2.WE_PalletID);
		}

		public void TestGenerateDestinationPalletIDs_ShouldNotGenerateIfFinalisedOrCancelled()
		{
			var transfer = GetNewBusinessObject();
			transfer.WD_DocketID = "W0000666";
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			var selectedLines = new WhsTransferLine[2] { line1, line2 };

			transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is finalised", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is finalised", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)transfer.NotificationManager.Peek).Clear();
			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is cancelled", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is cancelled", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestGenerateDestinationPalletIDs_ShouldNotWorkForOutboundDockDoorTransfer()
		{
			var transfer = GetNewBusinessObject();
			transfer.WD_DocketID = "W0000666";
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			var selectedLines = new[] { line1, line2 };

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is an Outbound Dock Door Transfer.", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is an Outbound Dock Door Transfer.", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CannotPerformThisOnOutboundDockDoorTransfer));
		}

		public void TestGenerateDestinationPalletIDs_ShouldNotWorkForVASOrderTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", transfer);
			AssertEquals(true, transfer.IsVASOrderTransfer);
			transfer.WD_DocketID = "W0000666";
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			var selectedLines = new[] { line1, line2 };

			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is an Outbound Dock Door Transfer.", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is an Outbound Dock Door Transfer.", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CanOnlyPerformThisOnInternalTransfer));
		}

		public void TestGenerateDestinationPalletIDs_ShouldOnlyWorkForInternalTransfer()
		{
			var transfer = GetNewBusinessObject();
			transfer.WD_DocketID = "W0000666";
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			var selectedLines = new[] { line1, line2 };

			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is Inter-Warehouse (Destination).", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is Inter-Warehouse (Destination).", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CanOnlyPerformThisOnInternalTransfer));

			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is Inter-Warehouse (Source).", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is Inter-Warehouse (Source).", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CanOnlyPerformThisOnInternalTransfer));

			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("W0000666-0001", line1.WE_PalletID);
			AssertEquals("W0000666-0002", line2.WE_PalletID);

			line1.WE_PalletID = "";
			line2.WE_PalletID = "";
			transfer.WD_IsPickFaceReplenishment = true;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is PickFaceReplenishment.", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is PickFaceReplenishment.", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CanOnlyPerformThisOnInternalTransfer));

			transfer.WD_IsPickFaceReplenishment = false;
			transfer.WD_IsPutawayTransfer = true;
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer is a Putaway Transfer.", "", line1.WE_PalletID);
			AssertEquals("Should not be generated because the Transfer is a Putaway Transfer.", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CanOnlyPerformThisOnInternalTransfer));
		}

		public void TestGenerateDestinationPalletIDs_ShouldNotGenerateIfLineFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "11");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_DocketID = "W0000666";
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.WE_TransferFromPalletId = "11";
			transferLine2.WE_TransferFromPalletId = "22";
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			Assert("Precondition", !transferLine2.IsFinalised);

			var selectedLines = new WhsTransferLine[] { transferLine1, transferLine2 };
			transfer.GenerateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be generated because the Transfer Line is finalised.", ZString.Empty, transferLine1.WE_PalletID);
			AssertEquals("Should be generated because the Transfer Line is *not* finalised.", "W0000666-0001", transferLine2.WE_PalletID);

			transferLine2.WE_PalletID = ZString.Empty;
			using (new SemaphoreManager(transferLine2.FinaliseDocketLineSemaphore))
			{
				transferLine2.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			Assert("Precondition", transferLine2.LocationStringInfo.ReadOnly);

			transfer.GenerateDestinationPalletIDs(new[] { transferLine2 });
			AssertEquals("Should not be generated because the Transfer Line is read only.", ZString.Empty, transferLine1.WE_PalletID);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGenerateDestinationPalletIDs_ChecksArguments()
		{
			GetNewBusinessObject().GenerateDestinationPalletIDs(null);
		}

		#endregion

		#region Update Destination PalletIDs

		public void TestUpdateDestinationPalletIDs()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();

			line1.WE_TransferFromPalletId = "P_ID_001";
			line2.WE_TransferFromPalletId = "P_ID_002";
			line3.WE_TransferFromPalletId = "P_ID_003";

			var selectedLines = new WhsTransferLine[2] { line1, line3 };

			transfer.UpdateDestinationPalletIDs(selectedLines);

			AssertEquals("P_ID_001", transfer.Lines[0].WE_PalletID);
			AssertEquals("", transfer.Lines[1].WE_PalletID);
			AssertEquals("P_ID_003", transfer.Lines[2].WE_PalletID);

			line1.WE_TransferFromPalletId = "";
			line3.WE_TransferFromPalletId = "DONT UPDATE";
			transfer.UpdateDestinationPalletIDs(selectedLines);

			AssertEquals("Should not be updated because the value to update is empty", "P_ID_001", transfer.Lines[0].WE_PalletID);
			AssertEquals("", transfer.Lines[1].WE_PalletID);
			AssertEquals("Should not be updated because it was not empty, we don't want to overwrite user entered values", "P_ID_003", transfer.Lines[2].WE_PalletID);
		}

		public void TestUpdateDestinationPalletIDsDoesNotUpdateIfFinalisedOrCancelled()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.WE_TransferFromPalletId = "P_ID_001";
			line2.WE_TransferFromPalletId = "P_ID_002";

			var selectedLines = new WhsTransferLine[2] { line1, line2 };

			transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			transfer.UpdateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be updated because the Transfer is finalised", "", transfer.Lines[0].WE_PalletID);
			AssertEquals("Should not be updated because the Transfer is finalised", "", transfer.Lines[1].WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)transfer.NotificationManager.Peek).Clear();
			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			transfer.UpdateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be updated because the Transfer is cancelled", "", transfer.Lines[0].WE_PalletID);
			AssertEquals("Should not be updated because the Transfer is cancelled", "", transfer.Lines[1].WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestUpdateDestinationPalletIDsDoesNotUpdateIfOutboundDockDoorTransfer()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.WE_TransferFromPalletId = "P_ID_001";
			line2.WE_TransferFromPalletId = "P_ID_002";

			var selectedLines = new[] { line1, line2 };

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			transfer.UpdateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be updated because the Transfer is an Outbound Dock Door Transfer.", "", line1.WE_PalletID);
			AssertEquals("Should not be updated because the Transfer is an Outbound Dock Door Transfer.", "", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CannotPerformThisOnOutboundDockDoorTransfer));
		}

		public void TestUpdateDestinationPalletIDsDoesNotUpdateIfLineFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "11");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.WE_TransferFromPalletId = "11";
			transferLine2.WE_TransferFromPalletId = "22";
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			Assert("Precondition", !transferLine2.IsFinalised);

			var selectedLines = new WhsTransferLine[] { transferLine1, transferLine2 };
			transfer.UpdateDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be updated because the Transfer Line is finalised.", ZString.Empty, transferLine1.WE_PalletID);
			AssertEquals("Should be updated because the Transfer Line is *not* finalised.", "22", transferLine2.WE_PalletID);

			transferLine2.WE_PalletID = ZString.Empty;
			using (new SemaphoreManager(transferLine2.FinaliseDocketLineSemaphore))
			{
				transferLine2.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			Assert("Precondition", transferLine2.LocationStringInfo.ReadOnly);

			transfer.UpdateDestinationPalletIDs(new[] { transferLine2 });
			AssertEquals("Should not be updated because the Transfer Line is read only.", ZString.Empty, transferLine1.WE_PalletID);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestUpdateDestinationPalletIDsChecksArguments()
		{
			GetNewBusinessObject().UpdateDestinationPalletIDs(null);
		}

		#endregion

		#region Clear Destination PalletIDs

		public void TestClearDestinationPalletIDs()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();
			var line3 = transfer.Lines.AddNew();

			line1.WE_PalletID = "P_ID_001";
			line2.WE_PalletID = "P_ID_002";
			line3.WE_PalletID = "P_ID_003";

			var selectedLines = new WhsTransferLine[2] { line1, line3 };

			transfer.ClearDestinationPalletIDs(selectedLines);

			AssertEquals("", line1.WE_PalletID);
			AssertEquals("P_ID_002", line2.WE_PalletID);
			AssertEquals("", line3.WE_PalletID);
		}

		public void TestClearDestinationPalletIDsDoesNotClearIfFinalisedOrCancelled()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.WE_PalletID = "P_ID_001";
			line2.WE_PalletID = "P_ID_002";

			var selectedLines = new WhsTransferLine[2] { line1, line2 };

			transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			transfer.ClearDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be cleared because the Transfer is finalised", "P_ID_001", transfer.Lines[0].WE_PalletID);
			AssertEquals("Should not be cleared because the Transfer is finalised", "P_ID_002", transfer.Lines[1].WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			transfer.WD_FinalisedDate = ZDateTimeOffset.Empty; //cleanup

			((NotificationBuffer)transfer.NotificationManager.Peek).Clear();
			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			transfer.ClearDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be cleared because the Transfer is cancelled", "P_ID_001", transfer.Lines[0].WE_PalletID);
			AssertEquals("Should not be cleared because the Transfer is cancelled", "P_ID_002", transfer.Lines[1].WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestClearDestinationPalletIDsDoesNotClearIfOutboundDockDoorTransfer()
		{
			var transfer = GetNewBusinessObject();
			var line1 = transfer.Lines.AddNew();
			var line2 = transfer.Lines.AddNew();

			line1.WE_PalletID = "P_ID_001";
			line2.WE_PalletID = "P_ID_002";

			var selectedLines = new[] { line1, line2 };

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			transfer.ClearDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be cleared because the Transfer is an Outbound Dock Door Transfer.", "P_ID_001", line1.WE_PalletID);
			AssertEquals("Should not be cleared because the Transfer is an Outbound Dock Door Transfer.", "P_ID_002", line2.WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)transfer.NotificationManager.Peek).ContainsNotificationType(TransferErrorTypes.CannotPerformThisOnOutboundDockDoorTransfer));
		}

		public void TestClearDestinationPalletIDsDoesNotUpdateIfLineFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.WE_PalletID = "11";
			transferLine2.WE_PalletID = "22";
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			Assert("Precondition", !transferLine2.IsFinalised);

			var selectedLines = new WhsTransferLine[] { transferLine1, transferLine2 };
			transfer.ClearDestinationPalletIDs(selectedLines);
			AssertEquals("Should not be updated because the Transfer Line is finalised.", "11", transferLine1.WE_PalletID);
			AssertEquals("Should be updated because the Transfer Line is *not* finalised.", "", transferLine2.WE_PalletID);

			transferLine2.WE_PalletID = "22";
			using (new SemaphoreManager(transferLine2.FinaliseDocketLineSemaphore))
			{
				transferLine2.WE_PutawayTime = ZDateTimeOffset.Now;
			}
			Assert("Precondition", transferLine2.LocationStringInfo.ReadOnly);

			transfer.ClearDestinationPalletIDs(new[] { transferLine2 });
			AssertEquals("Should not be updated because the Transfer Line is finalised.", "22", transferLine2.WE_PalletID);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestClearDestinationPalletIDsChecksArguments()
		{
			GetNewBusinessObject().ClearDestinationPalletIDs(null);
		}

		#endregion

		#region Finalisation

		#region TestFinaliseDocket

		#region SetupForTestFinaliseDocket

		protected override WhsTransfer SetupForTestFinaliseDocket()
		{
			var transfer = base.SetupForTestFinaliseDocket();

			// setup some inventory to transfer
			var part = Helper.CreateProduct(transfer.Client, "P1");
			Helper.CreateStock(transfer.Warehouse, transfer.Client, part, 1000m, "A-1");

			var docketLine = Helper.CreateWhsTransferLine(transfer, part, 10m, "A-1", "A-2");
			docketLine.WE_LineComment = "TEST";
			docketLine.RunPreSaveValidation(); // to commit inventory

			return transfer;
		}

		#endregion

		#region TestFinaliseTransfer_SetsAssociatedPutawayLines_ToNotPuttingAway_DBHits

		public void TestFinaliseTransfer_SetsAssociatedPutawayLines_ToNotPuttingAway_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_3");
			Helper.Factory.Save();

			var putawayTransfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_2", 10m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT_3", 10m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;
			var transferLine22 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT_3", 10m);
			transferLine22.RunPreSaveValidation();
			transferLine22.WE_GS_NKPutawayBy = staff1.GS_Code;

			var putawayTransfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine3 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_1", 10m);
			transferLine3.RunPreSaveValidation();
			Helper.Factory.Save();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT_2", isPuttingAway: true);
			var putawayLine2 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT_3", isPuttingAway: true);

			var putawayJob2 = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			var putawayLine3 = Helper.CreateWhsPutawayLine(putawayJob2, "PLT_1", isPuttingAway: true);
			Helper.Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 8 },
				{ WhsInventoryViewSchema.Constants.TableName, 4 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPutawayLineSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 2 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayTransfer1InOtherFactory = newFactory.Load<WhsTransfer>(putawayTransfer1.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				putawayTransfer1InOtherFactory.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
			}
			AssertIsFinalisedPrecondition(putawayTransfer1InOtherFactory);

			var transferLine1InOtherFactory = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			AssertIsFinalisedPrecondition(transferLine1InOtherFactory);
			AssertEquals("TransferLine1 WE_WPL_PutawayLine set correctly", putawayLine1.PK, transferLine1InOtherFactory.WE_WPL_PutawayLine);

			var transferLine2InOtherFactory = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertIsFinalisedPrecondition(transferLine2InOtherFactory);
			AssertEquals("TransferLine2 WE_WPL_PutawayLine set correctly", putawayLine2.PK, transferLine2InOtherFactory.WE_WPL_PutawayLine);

			var transferLine22InOtherFactory = newFactory.Load<WhsTransferLine>(transferLine22.PK);
			AssertIsFinalisedPrecondition(transferLine22InOtherFactory);
			AssertEquals("TransferLine22 WE_WPL_PutawayLine set correctly", putawayLine2.PK, transferLine22InOtherFactory.WE_WPL_PutawayLine);

			var putawayLine1InOtherFactory = newFactory.Load<WhsPutawayLine>(putawayLine1.PK);
			var putawayLine2InOtherFactory = newFactory.Load<WhsPutawayLine>(putawayLine2.PK);
			AssertEquals("PutawayLine1 not putting away", false, putawayLine1InOtherFactory.WPL_IsPuttingAway);
			AssertEquals("PutawayLine2 not putting away", false, putawayLine2InOtherFactory.WPL_IsPuttingAway);

			var putawayTransfer2InOtherFactory = newFactory.Load<WhsTransfer>(putawayTransfer2.PK);
			var transferLine3InOtherFactory = newFactory.Load<WhsTransferLine>(transferLine3.PK);
			var putawayLine3InOtherFactory = newFactory.Load<WhsPutawayLine>(putawayLine3.PK);
			AssertEquals("PutawayTransfer2 is not finalised", false, putawayTransfer2InOtherFactory.IsFinalised);
			AssertEquals("TransferLine3 is not finalised", false, transferLine3InOtherFactory.IsFinalised);
			AssertEquals("TransferLine3 WE_WPL_PutawayLine still null", ZGuid.Empty, transferLine3InOtherFactory.WE_WPL_PutawayLine);
			AssertEquals("PutawayLine3 not putting away", true, putawayLine3InOtherFactory.WPL_IsPuttingAway);
		}

		#endregion

		#region TestFinaliseDocket_DBHits

		public void TestFinaliseDocket_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			inventory1.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.FindLocation("A-1"), "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 20m, "A-1", "A-2");
			transferLine2.WE_TransferFromPalletId = "PLT-1";
			transferLine2.WE_PalletID = "PLT-1";
			transferLine3.WE_TransferFromPalletId = "PLT-1";
			transferLine3.WE_PalletID = "PLT-1";
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);

			using (RowFactory.SetCachedTables())
			{
				transferInOtherFactory.RunPreSaveValidation();
				transferInOtherFactory.Validation.ValidateAll();
			}

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 }, // Due to inventory hold code list validation in docketlines
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedDBHitsForValidation, otherFactory);

			var factoryForFinalise = new BusinessObjectFactory();
			factoryForFinalise.RefreshEnabled = false;
			var transferInFinaliseFactory = factoryForFinalise.Load<WhsTransfer>(transfer.PK);

			var expectedDBHitsForFinalisation = new Dictionary<string, int>(expectedDBHitsForValidation);
			expectedDBHitsForFinalisation[WhsDocketSchema.Constants.TableName] += 2;
			expectedDBHitsForFinalisation[WhsDocketLineSchema.Constants.TableName] += 2;
			// increased by A.V from 0 to 1
			expectedDBHitsForFinalisation[WhsPickLineSchema.Constants.TableName] += 1;
			expectedDBHitsForFinalisation.Add(WhsInventoryViewSchema.Constants.TableName, 3);
			expectedDBHitsForFinalisation.Add(PkgPackageJobSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketPalletSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTasksSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobServiceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskExtraResourceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskNotificationSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(StmEventSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobDocAddressSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(GlbStaffSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);

			expectedDBHitsForFinalisation.Remove(PkgPackageJobSchema.Constants.TableName);
			expectedDBHitsForFinalisation.Remove(ProcessTaskExtraResourceSchema.Constants.TableName);
			expectedDBHitsForFinalisation.Remove(ProcessTaskNotificationSchema.Constants.TableName);

			using (RowFactory.SetCachedTables())
			{
				transferInFinaliseFactory.FinaliseDocketWithoutUserConfirmation();
			}

			AssertEquals(true, transferInFinaliseFactory.IsFinalised);
			AssertDbHits(expectedDBHitsForFinalisation, factoryForFinalise);
		}

		#endregion

		#region TestLoad_FinalizeTransferLinesWithPalletIDs_ManyLines

		public void TestLoad_FinalizeTransferLinesWithPalletIDs_ManyLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Factory.Save();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1500m, data.Whs1.FindLocation("A-1"), "P123");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 200m, data.Whs1.FindLocation("A-2"), "P456");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 2000m, data.Whs1.FindLocation("A-3"), "P789");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 800m, data.Whs1.FindLocation("A-4"), "P101");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 3700m, data.Whs1.FindLocation("A-10"), "P191");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			Assert("Receive should be finalised.", receive.IsFinalised);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			for (int i = 0; i < 100; i++)
			{
				Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "P123", "A-5", "P123");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, "A-2", "P456", "A-6", "P456");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-3", "P789", "A-7", "P789");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 8m, "A-4", "P101", "A-8", "P101");
				Helper.CreateWhsTransferLine(transfer, data.Part1, 37m, "A-10", "P191", "A-11", "P191");
			}
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferInOtherFactory = newfactory.Load<WhsTransfer>(transfer.PK);

			BusinessObjectFactory.StartLogging();
			transferInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			var loadLog = BusinessObjectFactory.DebugLog;
			BusinessObjectFactory.StopLogging();

			CombineAssertions(() =>
			{
				var logCount = Regex.Matches(loadLog, Regex.Escape("WE_StockOnHand > 0 and WE_PalletID <> '' and WE_PalletID =")).Count;
				AssertEquals("Should load minimum Pallets.", 5, logCount);

				var allLoadedBusinessObjects = ((IBusinessObjectFactoryInternals)newfactory).AllBusinessObjects;
				AssertEquals("Should not load any additional inventories.", 505, allLoadedBusinessObjects.OfType<WhsInventoryView>().Count());
				AssertEquals("Should not load any additional receive lines.", 505, allLoadedBusinessObjects.OfType<WhsDocketLine>().Count());
				AssertEquals("Should not load any additional receives.", 2, allLoadedBusinessObjects.OfType<WhsDocket>().Count());
			});
		}

		#endregion

		#region AssertFinaliseDocket

		protected override void AssertFinaliseDocket(WhsTransfer docket)
		{
			var inventoryA1 = Helper.LoadInventory(docket.Warehouse.FindLocation("A-1"));
			var inventoryA2 = Helper.LoadInventory(docket.Warehouse.FindLocation("A-2"));
			AssertEquals("Source location should lose transferred units.", 990m, inventoryA1.UnitsAvailable);
			AssertEquals("Source location should have no stock committed.", 0m, inventoryA1.UnitsCommitted);
			AssertEquals("Destination location should have transferred units.", 10m, inventoryA2.UnitsAvailable);
			AssertEquals("Destination location should have no units committed.", 0m, inventoryA2.UnitsCommitted);
		}

		#endregion

		#region TestFinaliseDocket_FinalisesChildTransfers

		public void TestFinaliseDocket_FinalisesChildTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 2, 1);
			var whs3 = Helper.CreateWarehouse("WHS3", "C", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", whs2.PK, "B-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", whs3.PK, "C-1");
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);
			AssertIsFinalisedPrecondition(transferLine1.ChildTransferLine);
			AssertEquals("Precondition - Only 1 child transfer should be created.", 1, transfer.ChildTransfers.Count());
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertEquals("Precondition - child transfer should not be finalised.", false, transfer.ChildTransfers.ElementAt(0).IsFinalised);

			transfer.FinaliseDocket();
			// finalise of transfer should finalise it and all its lines that were not finalised previously.
			AssertEquals("Transfer should be finalised.", true, transfer.IsFinalised);
			AssertEquals("Transfer Line should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Transfer Line should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Transfer Line should be finalised.", true, transferLine3.IsFinalised);

			// finalise of inter whs transfer should create and finalise child line for each transfer line.
			AssertEquals("Child Transfer Line should be finalised.", true, transferLine1.ChildTransferLine.IsFinalised);
			AssertEquals("Child Transfer Line should be finalised.", true, transferLine2.ChildTransferLine.IsFinalised);
			AssertEquals("Child Transfer Line should be finalised.", true, transferLine3.ChildTransferLine.IsFinalised);

			// finalise of inter whs transfer should create a child transfer for each dest warehouse.
			AssertEquals("The transfer have 2 dest transfers, so 2 child transfers should be created.", 2, transfer.ChildTransfers.Count());
			var childTransferForWhs2 = transfer.ChildTransfers.Single(t => t.WD_WW_Whs == whs2.PK);
			var childTransferForWhs3 = transfer.ChildTransfers.Single(t => t.WD_WW_Whs == whs3.PK);

			// finalise of inter whs transfer should finalise all its child transfers.
			AssertEquals("Child Transfer should be finalised.", true, childTransferForWhs2.IsFinalised);
			AssertEquals("Child Transfer should be finalised.", true, childTransferForWhs3.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_FinalisesVASOrderIfTransferOut

		public void TestFinaliseDocket_FinalisesVASOrderIfTransferOut()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}
			AssertNotNull("Precondition: Created Return Transfer.", returnTransfer);
			AssertEquals("Precondition: Transfer Out is transferring correct stock.", 1, returnTransfer.Lines.Count);
			AssertEquals("Precondition: VAS Order is not finalised.", false, vasOrder.IsFinalised);

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(returnTransfer);
			AssertEquals("Finalising Return Transfer should finalise the VAS Order.", true, vasOrder.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_WithPalletIDs

		public void TestFinaliseDocket_WithPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.FindLocation("A-1"), "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 20m, "A-1", "A-2");
			transferLine2.WE_TransferFromPalletId = "PLT-1";
			transferLine2.WE_PalletID = "PLT-1";
			transferLine3.WE_TransferFromPalletId = "PLT-1";
			transferLine3.WE_PalletID = "PLT-1";
			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalised.", true, transfer.IsFinalised);
			AssertEquals("TransferLine1 should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("TransferLine2 should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("TransferLine3 should be finalised.", true, transferLine3.IsFinalised);
		}

		#endregion

		#region TestSetDocketLineFromInventory

		public void TestSetDocketLineFromInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			inventory1.OriginalInventoryHeldCode = InventoryStatus.Codes.Held;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals(inventory1.OriginalInventoryStatus, InventoryStatus.Codes.Held);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = transfer.CreateDocketLineFromInventory(inventory1);
			AssertEquals(transferLine1.WE_WHC_NKOriginalInventoryHeldCode, InventoryStatus.Codes.Held);
		}

		#endregion

		#region TestFinaliseDocket_ValidatesLines

		public void TestFinaliseDocket_ValidatesLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			WhsTransferLine transferLine;
			using (transfer.GetValidationSuspender())
			{
				transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "", "");
			}

			var expectedErrorMessage = "Please enter a value.";

			AssertNoError("Precondition", transferLine.TransferFromLocationStringInfo, expectedErrorMessage);
			AssertNoError("Precondition", transferLine.LocationStringInfo, "Please enter a Location.");

			transfer.FinaliseDocket();
			AssertEquals("Precondition - transfer should not be finalised.", false, transfer.IsFinalised);
			AssertHasError(transferLine.TransferFromLocationStringInfo, expectedErrorMessage);
			AssertHasError(transferLine.LocationStringInfo, "Please enter a Location.");
		}

		#endregion

		#region TestFinaliseDocket_FailedTransfer

		public void TestFinaliseDocket_FailedTransfer()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10, 10, 10, 10, 10); // A-1-1, A-1-2, A-2-1, A-2-2, A-3-1.
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "1", Notify);
			var line1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, "A-1-1", "A-2-2");
			var line2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20, "A-1-2", "A-2-2");  // will fail
			var line3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, "A-3-2", "A-2-2");  // will fail
			var line4 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20, "A-2-2", "A-1-2");

			transfer.FinaliseDocket();
			AssertEquals("Precondition", true, transfer.HasErrors && !transfer.IsFinalised);
			AssertEquals("Lines should not disappear", 4, transfer.Lines.Count);
			AssertEquals("Lines should not be deleted", true, !line1.IsDeleted && !line2.IsDeleted && !line3.IsDeleted && !line4.IsDeleted);
			AssertHasErrorContaining(line2.QtyToMoveIncludingMatchingLinesInfo, "only 10 Units are available for transfer out of this location");
			AssertHasErrorContaining(line3.QtyToMoveIncludingMatchingLinesInfo, "no Units are available for transfer out of this location");

			var inventoriesBeforeSaveA11 = Helper.LoadInventory(data.Whs1.FindLocation("A-1-1"));
			var inventoriesBeforeSaveA22 = Helper.LoadInventory(data.Whs1.FindLocation("A-2-2"));
			AssertEquals("Failed Finalise should not change inventories Total Units.", 10m, inventoriesBeforeSaveA11.UnitsTotal);
			AssertEquals("Failed Finalise should not change inventories Total Units.", 10m, inventoriesBeforeSaveA22.UnitsTotal);
			AssertEquals("Failed Finalise should commit stock that can be committed.", 10m, inventoriesBeforeSaveA11.UnitsCommitted);
			AssertEquals("Failed Finalise should commit stock that can be committed.", 10m, inventoriesBeforeSaveA22.UnitsCommitted);
		}

		#endregion

		#region TestFinaliseDocket_ChecksFinalisedStatusBeforeFinalisingDocket

		public void TestFinaliseDocket_ChecksFinalisedStatusBeforeFinalisingDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1-1", "A-1-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A-1-1", "A-1-2");
			HackTransferLineToMakeItUnfinalisable(transferLine1);
			HackTransferLineToMakeItUnfinalisable(transferLine2);

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(false, transfer.IsFinalised);
			AssertHasRowError(transfer, "Line(s) 1, 2 could not be finalized.");
		}

		static void HackTransferLineToMakeItUnfinalisable(WhsTransferLine transferLine)
		{
			CollectionCountChangedEventHandler action = null;
			action = (sender, e) =>
			{
				if (e.ItemAdded)
				{
					transferLine.RegisterEditableChildObject(transferLine.PickLines);
					transferLine.PickLines[0].AddRowError("TEST ERROR");
					transferLine.Inventory.CountChanged -= action;
					transferLine.Inventory.CountChanged += delegate
					{
						transferLine.UnRegisterEditableChildObject(transferLine.PickLines);
					};
				}
			};
			transferLine.Inventory.CountChanged += action;
		}

		#endregion

		#region TestFinaliseDocket_InterWhsTransferProperlySplitsDocketAndLinksLocationsAndSetsUnitsMetSource

		public void TestFinaliseDocket_InterWhsTransferProperlySplitsDocketAndLinksLocationsAndSetsUnitsMetSource()
		{
			var whs1 = Helper.CreateWarehouse("1", "A");
			var whs2 = Helper.CreateWarehouse("2", "B");
			var whs3 = Helper.CreateWarehouse("3", "C");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateStock(whs1, org, "1", part, 100m, "A");

			var transfer = Helper.CreateWhsTransfer(org, whs1, "1", Notify, TransferType.Codes.InterWhsSource);
			var line1 = Helper.CreateWhsTransferLine(transfer, part, 10m, "A", whs2.PK, "B");
			var line2 = Helper.CreateWhsTransferLine(transfer, part, 20m, "A", whs3.PK, "C");

			transfer.FinaliseDocket();
			AssertInterWhsSuccessAndNotification(transfer, Notify, "1");

			var transfers = new WhsTransferCollection(Factory);
			transfers.ApplySort(WhsDocketSchema.WD_ExternalReference.Name, ListSortDirection.Ascending);
			AssertEquals(3, transfers.Count);

			AssertTransfer(transfers[0].Lines[0], CodeLists.TransferType.Codes.InterWhsSource, whs1.PK, whs2.PK, "A", "B", 10m);
			AssertTransfer(transfers[0].Lines[1], CodeLists.TransferType.Codes.InterWhsSource, whs1.PK, whs3.PK, "A", "C", 20m);
			AssertTransfer(transfers[1].Lines[0], CodeLists.TransferType.Codes.InterWhsDest, whs1.PK, whs2.PK, "A", "B", 10m);
			AssertTransfer(transfers[2].Lines[0], CodeLists.TransferType.Codes.InterWhsDest, whs1.PK, whs3.PK, "A", "C", 20m);

			var inventoriesWhs1 = Helper.LoadInventory(whs1.DefaultLocation);
			var inventoriesWhs2 = Helper.LoadInventory(whs2.DefaultLocation);
			var inventoriesWhs3 = Helper.LoadInventory(whs3.DefaultLocation);
			AssertEquals(70m, inventoriesWhs1.UnitsTotal);
			AssertEquals(10m, inventoriesWhs2.UnitsTotal);
			AssertEquals(20m, inventoriesWhs3.UnitsTotal);
			AssertEquals(whs2.PK, inventoriesWhs2.Inventory.First().Location.Row.WR_WW_Whs);
			AssertEquals(whs3.PK, inventoriesWhs3.Inventory.First().Location.Row.WR_WW_Whs);
		}

		#endregion

		#region TestFinaliseDocket_InterWhsTransferProperlySplitsDocketAndLinksLocationsAndSetsUnitsMetDest

		public void TestFinaliseDocket_InterWhsTransferProperlySplitsDocketAndLinksLocationsAndSetsUnitsMetDest()
		{
			var whs1 = Helper.CreateWarehouse("1", "A");
			var whs2 = Helper.CreateWarehouse("2", "B");
			var whs3 = Helper.CreateWarehouse("3", "C");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			Helper.CreateStock(whs2, org, "1", part, 50m, "B");
			Helper.CreateStock(whs3, org, "2", part, 40m, "C");

			var transfer = Helper.CreateWhsTransfer(org, whs1, "1", Notify, TransferType.Codes.InterWhsDest);
			var line1 = Helper.CreateWhsTransferLine(transfer, part, 10m, "B", whs2.PK, "A");
			var line2 = Helper.CreateWhsTransferLine(transfer, part, 20m, "C", whs3.PK, "A");
			transfer.FinaliseDocket();
			AssertInterWhsSuccessAndNotification(transfer, Notify, "1");

			var transfers = new WhsTransferCollection(Factory);
			transfers.ApplySort(WhsDocketSchema.WD_ExternalReference.Name, ListSortDirection.Ascending);
			AssertEquals(3, transfers.Count);

			AssertTransfer(transfers[0].Lines[0], TransferType.Codes.InterWhsDest, whs2.PK, whs1.PK, "B", "A", 10m);
			AssertTransfer(transfers[0].Lines[1], TransferType.Codes.InterWhsDest, whs3.PK, whs1.PK, "C", "A", 20m);
			AssertTransfer(transfers[1].Lines[0], TransferType.Codes.InterWhsSource, whs2.PK, whs1.PK, "B", "A", 10m);
			AssertTransfer(transfers[2].Lines[0], TransferType.Codes.InterWhsSource, whs3.PK, whs1.PK, "C", "A", 20m);

			var inventoriesWhs1 = Helper.LoadInventory(whs1.DefaultLocation);
			var inventoriesWhs2 = Helper.LoadInventory(whs2.DefaultLocation);
			var inventoriesWhs3 = Helper.LoadInventory(whs3.DefaultLocation);
			AssertEquals(30m, inventoriesWhs1.UnitsTotal);
			AssertEquals(40m, inventoriesWhs2.UnitsTotal);
			AssertEquals(20m, inventoriesWhs3.UnitsTotal);

			var inventories = inventoriesWhs1.Inventory.ToArray();
			AssertEquals(whs1.PK, inventories[0].Location.Row.WR_WW_Whs);
			AssertEquals(whs1.PK, inventories[1].Location.Row.WR_WW_Whs);
			AssertEquals(whs2.PK, inventoriesWhs2.Inventory.First().Location.Row.WR_WW_Whs);
			AssertEquals(whs3.PK, inventoriesWhs3.Inventory.First().Location.Row.WR_WW_Whs);
		}

		#endregion

		#region TestFinaliseDocket_InnerTransfer_KeepsInventoryStatus

		public void TestFinaliseDocket_InnerTransfer_KeepsInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, data.Whs1.FindLocation("A-1").PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, data.Whs1.FindLocation("A-1").PK, "", "", InventoryStatus.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, data.Whs1.FindLocation("A-1").PK, "", "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2", InventoryHoldCodes.Codes.Damaged);
			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalised.", true, transfer.IsFinalised);
			Factory.Save(); // need to save to put new transfers on DB

			var availableInventory = Helper.LoadInventory(data.Whs1.FindLocation("A-2"), InventoryStatus.Codes.Available);
			var heldInventory = Helper.LoadInventory(data.Whs1.FindLocation("A-2"), InventoryStatus.Codes.Held).Inventory.Where(i => !i.IsDamaged);
			var damagedInventory = Helper.LoadInventory(data.Whs1.FindLocation("A-2"), InventoryStatus.Codes.Held).Inventory.Where(i => i.IsDamaged);
			AssertEquals(10m, availableInventory.UnitsTotal);
			AssertEquals(10m, heldInventory.Sum(i => i.WI_TotalUnits));
			AssertEquals(10m, damagedInventory.Sum(i => i.WI_TotalUnits));
		}

		#endregion

		#region TestFinaliseDocket_InterWhsTransfer_KeepsInventoryStatus

		public void TestFinaliseDocket_InterWhsTransfer_KeepsInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WHS", "B");
			var locationA1 = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, locationA1.PK, "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, locationA1.PK, "", "", InventoryStatus.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 10m, locationA1.PK, "", "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", whs2.PK, "B", InventoryHoldCodes.Codes.Damaged);
			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalised.", true, transfer.IsFinalised);
			Factory.Save(); // need to save to put new transfers on DB

			var locationB1 = whs2.FindLocation("B");
			var availableInventory = Helper.LoadInventory(locationB1, InventoryStatus.Codes.Available);
			var heldInventory = Helper.LoadInventory(locationB1, InventoryStatus.Codes.Held).Inventory.Where(i => !i.IsDamaged);
			var damagedInventory = Helper.LoadInventory(locationB1, InventoryStatus.Codes.Held).Inventory.Where(i => i.IsDamaged);
			AssertEquals(10m, availableInventory.UnitsTotal);
			AssertEquals(10m, heldInventory.Sum(i => i.WI_TotalUnits));
			AssertEquals(10m, damagedInventory.Sum(i => i.WI_TotalUnits));
		}

		#endregion

		#region TestFinaliseDocket_Bug_DoesNotModifyInventory

		// Note: the bug already fixed in Alpha, test added to make sure it will not re-occur in the future.
		public void TestFinaliseDocket_Bug_DoesNotModifyInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var year = ZDateTime.Today.Year - 1;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			inventory.WI_ArrivalDate = inventory.InDocketLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(year, 3, 23, 5, 15, 0); // having hours/minutes is important to recreate the bug
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 7m, "A-1", "A-2");
			transferLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(year, 3, 23);
			transfer.RunPreSaveValidation(); // to commit inventory
			AssertEquals("Precondition - inventory should be committed.", 7m, transferLine.QtyCommittedIncludingMatchingLines);

			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalised.", true, transfer.IsFinalised);
			AssertEquals("Transfer line should be finalised.", true, transferLine.IsFinalised);
			AssertEquals("Inventory should be taken from source location.", 3m, inventory.WI_TotalUnits);
			AssertEquals("Inventory should be transferred to dest location.", 7m, transferLine.Inventory.Cast<WhsInventoryView>().Single().WI_TotalUnits);
		}

		#endregion

		#region TestFinaliseDocket_GivesPromptWithDefaultableQueryEventArgs

		public void TestFinaliseDocket_GivesPromptWithDefaultableQueryEventArgs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");

			AssertEquals("Precondition: transfer has no errors", false, transfer.HasErrors);
			AssertEquals("Precondition: transfer is unfinalized", false, transfer.IsFinalised);
			AssertEquals("Precondition: transferline is unfinalized.", false, transferLine.IsFinalised);

			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
				}
			};

			transfer.FinaliseDocket();

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			var expectedMessage = "Finalizing this Transfer will update the inventory.\r\nOn finalization, this Transfer will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\nDo you wish to Finalize this Transfer?";
			AssertEquals("Postcondition: correct notification message", expectedMessage, lastQueryEventArgs.Message);
			AssertEquals("Postcondition: correct response", true, lastQueryEventArgs.Response);
			AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNoCancel, lastQueryEventArgs.Context.Buttons);
			AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No, ZDialogResult.Cancel }, lastQueryEventArgs.Context.DialogResultsToNotSave);
			AssertEquals("Postcondition: transfer is finalized.", true, transfer.IsFinalised);
			AssertEquals("Postcondition: transferline is finalized.", true, transferLine.IsFinalised);
			AssertEquals("Postcondition: transfer has no errors.", false, transfer.HasErrors);
		}

		#endregion

		#region TestFinaliseDocket_SynchronisesFinaliseTimeAndPutawayTime

		public void TestFinaliseDocket_SynchronisesFinaliseTimeAndPutawayTime()
			=> TestFinalise_SynchronisesFinaliseTimeAndPutawayTime((transfer, _) => transfer.FinaliseDocket(), withFinalisedTimeProvider: false);

		public void TestFinaliseDocket_SynchronisesFinaliseTimeAndPutawayTime_WithFinalisedTimeProvider()
			=> TestFinalise_SynchronisesFinaliseTimeAndPutawayTime((transfer, _) => transfer.FinaliseDocket(), withFinalisedTimeProvider: true);

		public void TestValidateAndFinaliseDocketLines_SynchronisesFinaliseTimeAndPutawayTime()
			=> TestFinalise_SynchronisesFinaliseTimeAndPutawayTime((transfer, transferLine) => transfer.ValidateAndFinaliseDocketLines(new[] { transferLine }, confirmFinalise: false), withFinalisedTimeProvider: false);

		public void TestValidateAndFinaliseDocketLines_SynchronisesFinaliseTimeAndPutawayTime_WithFinalisedTimeProvider()
			=> TestFinalise_SynchronisesFinaliseTimeAndPutawayTime((transfer, transferLine) => transfer.ValidateAndFinaliseDocketLines(new[] { transferLine }, confirmFinalise: false), withFinalisedTimeProvider: true);

		void TestFinalise_SynchronisesFinaliseTimeAndPutawayTime(Action<WhsTransfer, WhsTransferLine> finaliseAction, bool withFinalisedTimeProvider)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var srcWhs = data.Whs1;
			var dstWhs = Helper.CreateWarehouse("W2", "A", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, srcWhs, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, srcWhs.FindLocation("A-1"), "P123");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, srcWhs.FindLocation("A-1"), "P123");
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, srcWhs, "T1", Notify, TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", dstWhs.PK, "A-1");
			transferLine.WE_TransferFromPalletId = "P123";
			transferLine.WE_PalletID = "P123";
			transfer.RunPreSaveValidation();
			Factory.Save();

			transferLine.WE_DocketLineStatusInfo.ValueChanged += (s, e) => System.Threading.Thread.Sleep(1000);
			transferLine.MatchingLines.Single().WE_DocketLineStatusInfo.ValueChanged += (s, e) => System.Threading.Thread.Sleep(1000);

			var providerTime = ZDateTimeOffset.Today.AddDays(-14);
			var provider = new FinalisedDateFromFinaliseEvent(providerTime);

			using (withFinalisedTimeProvider ? SetTransferProvider(transfer, provider) : null)
			{
				finaliseAction(transfer, transferLine);
				AssertIsFinalisedPrecondition(transferLine);

				using (new SemaphoreManager(transfer.FinaliseDocketSemaphore))
				{
					if (withFinalisedTimeProvider)
					{
						AssertEquals("Provider should be correct.", provider, transfer.GetFinalisedDateProvider());
						AssertEquals("Finalized Times should be correct.", providerTime, transferLine.WE_FinalisedDate);
					}
					else
					{
						AssertNull("Provider should not exist.", transfer.GetFinalisedDateProvider());
					}
				}
			}

			var matchingLine = transferLine.MatchingLines.Single();
			AssertIsFinalisedPrecondition(matchingLine);

			var childTransferLine = transferLine.ChildTransferLine;
			AssertIsFinalisedPrecondition(childTransferLine);

			var childMatchingLine = childTransferLine.MatchingLines.Single();
			AssertIsFinalisedPrecondition(childMatchingLine);

			AssertEquals("Finalized Times should be the same.", matchingLine.WE_FinalisedDate, transferLine.WE_FinalisedDate);
			AssertEquals("Finalized Times should be the same.", matchingLine.WE_FinalisedDate, childTransferLine.WE_FinalisedDate);
			AssertEquals("Finalized Times should be the same.", matchingLine.WE_FinalisedDate, childMatchingLine.WE_FinalisedDate);

			AssertEquals("Putaway Times should be the same.", matchingLine.WE_PutawayTime, transferLine.WE_PutawayTime);
			AssertEquals("Putaway Times should be the same.", matchingLine.WE_PutawayTime, childTransferLine.WE_PutawayTime);
			AssertEquals("Putaway Times should be the same.", matchingLine.WE_PutawayTime, childMatchingLine.WE_PutawayTime);

			IDisposable SetTransferProvider(WhsTransfer transfer, FinalisedDateFromFinaliseEvent provider)
			{
				transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
				return transfer.SetFinalisedDateProvider(provider);
			}
		}

		#endregion

		#endregion

		#region TestFinaliseDocket_DoesNotModifyQtyToMove

		public void TestFinaliseDocket_DoesNotModifyQtyToMoveIncludingMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 24m);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 65m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 500m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 500m, locationA1);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 0m, "A-1", "A-2");
			transferLine.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			transferLine.WE_TransactionQuantity = 864m;
			transfer.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", 864m, transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("Precondition", 864m, transferLine.QtyCommittedIncludingMatchingLines);

			transfer.FinaliseDocket();
			AssertEquals("Transfer should be finalised.", true, transfer.IsFinalised);
			AssertEquals("QtyToMoveIncludingMatchingLines should not change during finalisation.", 864m, transferLine.QtyToMoveIncludingMatchingLines);
			AssertEquals("QtyCommittedIncludingMatchingLines should not change during finalisation.", 864m, transferLine.QtyCommittedIncludingMatchingLines);
		}

		#endregion

		#region TestFinaliseDocketLines

		#region TestFinaliseDocketLines

		public void TestFinaliseDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "A-2");

			var userConfirmationRequired = true;

			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine1, transferLine2 }, !userConfirmationRequired);
			AssertEquals("Transfer Line 1 should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Transfer Line 2 should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Transfer Line 3 should not be finalised.", false, transferLine3.IsFinalised);
			AssertNull("Finalisation process should have no user confirmation.", Notify.LastQueryUserEventArgs);

			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine1, transferLine2 }, !userConfirmationRequired);
			AssertEquals("When trying to finalised already finalised lines should get error message.", true, Notify.ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseAllSelectedLinesAreFinalised));
			AssertNull("Finalisation process should have no user confirmation.", Notify.LastQueryUserEventArgs);
			Notify.Clear(); // remove old notifications.

			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine2, transferLine3 }, userConfirmationRequired);
			AssertEquals("When trying to finalised already finalised lines with not finalised lines no errors should be shown.", false, Notify.ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseAllSelectedLinesAreFinalised));
			AssertEquals("Transfer Line 1 should be finalised.", true, transferLine1.IsFinalised);
			AssertEquals("Transfer Line 2 should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Transfer Line 3 should be finalised.", true, transferLine3.IsFinalised);
			AssertEquals("Finalisation process should ask for user confirmation.", transfer.FinaliseLinesConfirmationMessage, ((QueryUserMsgBoxEventArgs)Notify.LastQueryUserEventArgs).Message);
		}

		#endregion

		#region TestFinaliseDocketLines_UncommitsExcessInventoryAndCommitsRequiredInventory

		public void TestFinaliseDocketLines_UncommitsExcessInventoryAndCommitsRequiredInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			transferLine1.RunPreSaveValidation(); // commit inventory
			transferLine1.WE_TransactionQuantity = 1m;
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 9m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));
			AssertEquals("Precondition", 5m, transferLine1.GetQtyCommittedToThisLine());
			AssertEquals("Precondition", 0m, transferLine2.GetQtyCommittedToThisLine());

			transfer.ValidateAndFinaliseDocketLines(new[] { transferLine1, transferLine2 }, confirmFinalise: false);
			AssertEquals(true, transferLine1.IsFinalised);
			AssertEquals(true, transferLine2.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocketLines_WithPalletIDs

		public void TestFinaliseDocketLines_WithPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, data.Whs1.FindLocation("A-1"), "PLT-1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.FindLocation("A-1"), "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2, 20m, "A-1", "A-2");
			transferLine2.WE_TransferFromPalletId = "PLT-1";
			transferLine2.WE_PalletID = "PLT-1";
			transferLine3.WE_TransferFromPalletId = "PLT-1";
			transferLine3.WE_PalletID = "PLT-1";

			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine1, transferLine2 }, false);
			AssertEquals("System should allow to finalise line without pallet ID.", true, transferLine1.IsFinalised);
			AssertEquals("System should not allow partial pallet ID finalisation.", false, transferLine2.IsFinalised);
			AssertHasError("Destination pallet ID should have error when attempting partial pallet ID transfer.", transferLine2.WE_PalletIDInfo, "This transfer would split the Pallet ID into multiple locations. You must transfer ALL units of this Pallet ID at once.");

			transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine2, transferLine3 }, false);
			AssertEquals("System should allow transfer of a whole pallet ID content as a single move.", true, transferLine2.IsFinalised);
			AssertEquals("System should allow transfer of a whole pallet ID content as a single move.", true, transferLine3.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocketLines_WithException

		public void TestFinaliseDocketLines_WithException()
		{
			var transfer = GetNewBusinessObject();
			var transferLine = Factory.New<WhsTransferLine>();

			AssertExceptionThrown(typeof(ArgumentException), "Transfer can only finalise its own lines.", () => transfer.ValidateAndFinaliseDocketLines(new WhsTransferLine[] { transferLine }, false));
		}

		#endregion

		#region TestFinaliseDocketLines_InvalidData

		public void TestFinaliseDocketLines_InvalidData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(ZGuid.Empty, data.Whs1.PK, "T1", Notify);
			Helper.CreateWhsTransferLine(transfer, ZGuid.Empty, 1m, "", ZGuid.Empty, "", "");
			Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, "", ZGuid.Empty, "", "");
			Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, "A-1", ZGuid.Empty, "A-2", "");
			AssertNoExceptionThrown(() => transfer.FinaliseDocket());
		}

		#endregion

		#endregion

		#region TestOverrideFinaliseDate

		[TestDate(2017, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestOverrideFinaliseDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transferLine.RunPreSaveValidation();
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;

			var provider = new Provider { FinaliseTime = new ZDateTimeOffset(2017, 2, 2) };
			using (transfer.SetFinalisedDateProvider(provider))
			{
				transfer.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(transfer);
				AssertEquals("Finalised Date is Overriden with the Correct value.",
					new ZDateTimeOffset(2017, 2, 2, 0, 0, 0, transfer.WD_FinalisedDate.Offset),
					transfer.WD_FinalisedDate);
			}
		}

		#endregion

		#region TestGetFinalisedDateProvider

		public void TestGetFinalisedDateProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			AssertNull(transfer.GetFinalisedDateProvider());

			var provider = new Provider { FinaliseTime = ZDateTimeOffset.Now };
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			using (transfer.SetFinalisedDateProvider(provider))
			{
				AssertExceptionThrown(typeof(InvalidOperationException), "Attempt to call GetFinalisedDateProvider() without finalising the Transfer.", () => transfer.GetFinalisedDateProvider());

				using (new SemaphoreManager(transfer.FinaliseDocketSemaphore))
				{
					AssertEquals("Provider should be correct.", provider, transfer.GetFinalisedDateProvider());
				}

				using (new SemaphoreManager(transfer.FinaliseDocketLineSemaphore))
				{
					AssertEquals("Provider should be correct.", provider, transfer.GetFinalisedDateProvider());
				}
			}
		}

		#endregion

		#region TestSetFinalisedDateProvider

		public void TestSetFinalisedDateProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			AssertExceptionThrown<ArgumentNullException>(() => transfer.SetFinalisedDateProvider(null));

			var provider = new Provider { FinaliseTime = ZDateTimeOffset.Now };
			AssertExceptionThrown(typeof(InvalidOperationException), "Can only use a IFinalisedDateProvider for Outbound Dock Door Transfers.", () => transfer.SetFinalisedDateProvider(provider));

			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;

			using (transfer.SetFinalisedDateProvider(provider))
			{
				using (new SemaphoreManager(transfer.FinaliseDocketSemaphore))
				{
					AssertEquals("Provider should be correct.", provider, transfer.GetFinalisedDateProvider());
				}
			}

			AssertNull("Once the Disposable is Disposed, the Provider should be cleared out.", transfer.GetFinalisedDateProvider());
		}

		class Provider : IFinalisedDateProvider
		{
			ZDateTimeOffset IFinalisedDateProvider.GetFinalisationTimeOffset() => FinaliseTime;

			public ZDateTimeOffset FinaliseTime { get; set; }
		}

		#endregion

		#endregion

		#region TestRelatedJobs

		protected override List<IRelatedJob> GetValidRelatedJobs(WhsTransfer docket)
		{
			var transfer1 = Factory.NewWithValidTestData<WhsTransfer>();
			transfer1.WD_WD_ParentDocket = docket.PK;
			transfer1.WD_DocketSubType = TransferType.Codes.InterWhsDest;

			var transfer2 = Factory.NewWithValidTestData<WhsTransfer>();
			transfer2.WD_DocketSubType = TransferType.Codes.InterWhsDest;

			docket.FillWithValidTestData();
			docket.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			docket.WD_WD_ParentDocket = transfer2.PK;

			Factory.Save();

			return new List<IRelatedJob>() { transfer1, transfer2 };
		}

		protected override List<IRelatedJob> GetInvalidRelatedJobs(WhsTransfer docket)
		{
			var transfer1 = Factory.NewWithValidTestData<WhsTransfer>();
			var transfer2 = Factory.NewWithValidTestData<WhsTransfer>();
			transfer1.WD_DocketSubType = TransferType.Codes.InterWhsDest;
			transfer2.WD_DocketSubType = TransferType.Codes.InterWhsDest;

			Factory.Save();

			return new List<IRelatedJob>() { transfer1, transfer2 };
		}

		#endregion

		#region TestPropertiesModifiedInVASOrderTransfer

		public void TestPropertiesModifiedInVASOrderTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateArea(data.Whs1, "A2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var vasOrderTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull(vasOrderTransfer);
			Factory.Save();

			var exceptionMessage = "Cannot save as fields have been modified on a VAS Order Transfer.";
			AssertChangedProperties(vasOrderTransfer, exceptionMessage, WhsDocketSchema.WD_OH_Client, Helper.CreateClient("C2").PK);
			AssertChangedProperties(vasOrderTransfer, exceptionMessage, WhsDocketSchema.WD_WW_Whs, Helper.CreateWarehouse("W2").PK);
			Assert(!vasOrderTransfer.HasChanges);
			vasOrderTransfer[WhsDocketSchema.WD_DocketSubType] = (ZString)TransferType.Codes.InterWhsSource;
			Assert(vasOrderTransfer.HasChanges);
			Helper.AssertZCannotSaveExceptionThrown(exceptionMessage, Factory.Save);
		}

		void AssertChangedProperties(WhsTransfer transfer, string expectedExceptionMessage, SchemaColumn column, IZType differentValue)
		{
			var originalValue = transfer[column];
			transfer[column] = differentValue;
			Assert(transfer.HasChanges);
			Helper.AssertZCannotSaveExceptionThrown(expectedExceptionMessage, Factory.Save);
			transfer[column] = originalValue;
			transfer.HasChanges = false;
			Assert(!transfer.HasChanges);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestPropertiesModifiedInOutboundDockDoorTransfer

		public void TestPropertiesModifiedInOutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var pick = Factory.New<WhsPick>();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_ParentPickForTransfer = pick.PK;
			Factory.Save();

			var exceptionMessage = "Cannot save as fields have been modified on an Outbound Dock Door Transfer.";
			AssertChangedProperties(transfer, exceptionMessage, WhsDocketSchema.WD_OH_Client, Helper.CreateClient("C2").PK);
			AssertChangedProperties(transfer, exceptionMessage, WhsDocketSchema.WD_WW_Whs, Helper.CreateWarehouse("W2").PK);
			AssertChangedProperties(transfer, exceptionMessage, WhsDocketSchema.WD_DocketSubType, (ZString)TransferType.Codes.InterWhsSource);

			var otherPick = Factory.New<WhsPick>();
			AssertChangedProperties(transfer, exceptionMessage, WhsDocketSchema.WD_WP_ParentPickForTransfer, otherPick.PK);
		}

		#endregion

		#region TestPreventOverCommittingStock_PutawayTransfers

		[ExpectNoExceptions]
		public void TestPreventOverCommittingStock_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);

			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var line = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "A", 2m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 2m);
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is Committed.", 2m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			var factoryForSecondUser = new BusinessObjectFactory { RefreshEnabled = false };
			var docketLineInSecondUser = factoryForSecondUser.Load<WhsReceiveLine>(line.InDocketLine.PK);
			docketLineInSecondUser.WE_TransactionQuantity = 1m;
			docketLineInSecondUser.WE_StockOnHand = 1m;
			docketLineInSecondUser.WE_ClientOrderedUnits = 1m;
			NUnit.Framework.Assert.That(factoryForSecondUser.Save, CustomConstraints.InnermostExceptionThrown(typeof(ZConcurrencyCheckFailureException), $"TriggerLikelyConcurrencyError: {WhsInventoryView.PreventOverReduceStockViaInventoryLineMessageID}"), "Save should be prevented as we are Over-reducing.");
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		protected override bool RequiresCreditCheck
		{
			get { return false; }
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_TransferForOrder()
		{
			var transfer = GetNewBusinessObject();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);

			transfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);
		}

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_PutawayTransfer()
		{
			var transfer = GetNewBusinessObject();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);

			transfer.WD_IsPutawayTransfer = true;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);
		}

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_VASOrder()
		{
			var transfer = GetNewBusinessObject();
			var vasOrder = Factory.New<WhsVASOrder>();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);

			vasOrder.WVO_WD_TransferIntoServiceArea = transfer.PK;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);

			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);

			vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);
		}

		public void TestConfigOrg_DisabledForAutoCreatedTransfers_PickFaceReplenishment()
		{
			var transfer = GetNewBusinessObject();
			var client = Helper.CreateClient("C1");

			transfer.WD_OH_Client = client.PK;
			AssertNotNull("Precondition.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);

			transfer.WD_IsPickFaceReplenishment = true;
			AssertNull("Should *not* support custom labels for auto created transfers.", ((ICustomLabelsConfigOrgProvider)transfer).ConfigOrg);
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_TransferForOrder()
		{
			var transfer = GetNewBusinessObject();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			transfer.WD_WP_ParentPickForTransfer = ZGuid.NewZGuid();

			((ICustomLabelsConfigOrgProvider)transfer).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_PutawayTransfer()
		{
			var transfer = GetNewBusinessObject();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			transfer.WD_IsPutawayTransfer = true;

			((ICustomLabelsConfigOrgProvider)transfer).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_VASOrder()
		{
			var transfer = GetNewBusinessObject();
			var vasOrder = Factory.New<WhsVASOrder>();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			vasOrder.WVO_WD_TransferIntoServiceArea = transfer.PK;

			((ICustomLabelsConfigOrgProvider)transfer).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);

			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
			vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK;
			((ICustomLabelsConfigOrgProvider)transfer).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
		}

		public void TestConfigOrgChanged_DisabledForAutoCreatedTransfers_PickFaceReplenishment()
		{
			var transfer = GetNewBusinessObject();
			var configOrgChangedCalled = false;
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;
			transfer.WD_IsPickFaceReplenishment = true;

			((ICustomLabelsConfigOrgProvider)transfer).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;
			transfer.WD_OH_Client = client2.PK;
			AssertEquals("Should *not* support custom labels for auto created transfers.", false, configOrgChangedCalled);
		}

		public void TestConfigOrgChanged_DetachAfterChangingToAutoCreatedTransfer()
		{
			var transfer = GetNewBusinessObject();
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");

			transfer.WD_OH_Client = client1.PK;

			var configOrgChangedCalledCount = 0;
			EventHandler configOrgChanged = (s, e) => configOrgChangedCalledCount++;
			((ICustomLabelsConfigOrgProvider)transfer).ConfigOrgChanged += configOrgChanged;

			transfer.WD_OH_Client = client2.PK;
			AssertEquals("ConfigOrgChanged should have been called (i.e. should have attached event handler).", 1, configOrgChangedCalledCount);

			transfer.WD_IsPutawayTransfer = true;
			((ICustomLabelsConfigOrgProvider)transfer).ConfigOrgChanged -= configOrgChanged;
			transfer.WD_OH_Client = client1.PK;
			AssertEquals("ConfigOrgChanged should *not* have been called (i.e. should have detached event handler).", 1, configOrgChangedCalledCount);
		}

		#endregion

		#region IDocManagerSupport Members

		public override void TestDocManagerInfo()
		{
			DocManagerInfo info = Docket.DocManagerInfo;
			AssertEquals(Docket, info.BusinessEntity);
			AssertEquals("WTD", info.DocManagerCode);
		}

		#endregion

		#region IDocumentSupportable Members

		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(WhsTransferDocumentSupporter), Docket.DocumentSupporter.GetType());
		}

		#endregion

		#region Test IMasterAssigner Members

		public void TestIMasterAssignerMembers()
		{
			var transfer = GetNewBusinessObject();
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;
			var line = transfer.Lines.AddNew();

			var assigner = (IMasterStaffAssigner)transfer;

			AssertEquals("Pre-condition", line, assigner.Lines.Single());
			AssertEquals(transfer.Factory, assigner.Factory);
			AssertEquals(true, assigner.IsJobAssignable);

			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(false, assigner.IsJobAssignable);

			transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(false, assigner.IsJobAssignable);
		}

		#region TestIMasterAssignerMembers_CanAssignAnyLines

		public void TestIMasterAssignerMembers_CanAssignAnyLinesPickOnly()
		{
			var user = Helper.CreateGlbStaff("T1", "T1");
			var transfer = GetNewBusinessObject();
			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;
			transfer.Option = AssignLineOptions.PickOnly;

			var transferLine1 = Factory.New<WhsTransferLine>();
			var transferLine2 = Factory.New<WhsTransferLine>();
			transferLine1.WE_WD = transfer.PK;
			transferLine2.WE_WD = transfer.PK;

			var assigner = (IMasterStaffAssigner)transfer;

			// All un-assigned lines
			AssertEquals(true, assigner.CanAssignOrUnAssignAnyLines);

			// Assigned and un-assigned line
			transferLine1.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals(true, assigner.CanAssignOrUnAssignAnyLines);

			// All Un-assigned Lines
			transferLine2.PickLines.AddNew().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals(false, assigner.CanAssignOrUnAssignAnyLines);

			// Cancelled transfer
			transferLine1.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Empty;
			transferLine2.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Empty;
			transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(false, assigner.CanAssignOrUnAssignAnyLines);

			// Finalised transfer
			transfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(false, assigner.CanAssignOrUnAssignAnyLines);
		}

		#endregion

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public void TestIProcessHandlingInfoProvider()
		{
			IProcessHandlingInfoProvider transferHandlingInfoProvider = GetNewBusinessObject();
			AssertEquals(typeof(WhsTransferProcessHandlingInfo), transferHandlingInfoProvider.ProcessHandlingInfo.GetType());
		}

		#endregion

		#region IWhsLogEventParent

		protected override string ExpectedEventReferenceParameterType => Constants.EventReferenceParameterTypes.Transfer;

		#endregion

		#region IPickedStockAdjuster Members

		#region TestAdjustOutInventory_ThrowsErrorWhenQtyToReduceIsGreaterThanInventoryTotalUnits

		public void TestAdjustOutInventory_ThrowsErrorWhenQtyToReduceIsGreaterThanInventoryTotalUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Cannot return stock more than inventory TotalUnits.", () => iTransfer.AdjustOutInventory(inventory, inventory.WI_WL, 11m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenInventoryIsNull

		public void TestAdjustOutInventory_ThrowsErrorWhenInventoryIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			AssertExceptionThrown<ArgumentNullException>(() => iTransfer.AdjustOutInventory(null, ZGuid.NewZGuid(), 1));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenQuantityNoPositive

		public void TestAdjustOutInventory_ThrowsErrorWhenQuantityNoPositive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Quantity must be positive.", () => iTransfer.AdjustOutInventory(inventory, inventory.WI_WL, 0m));
			AssertExceptionThrown(typeof(ArgumentException), "Quantity must be positive.", () => iTransfer.AdjustOutInventory(inventory, inventory.WI_WL, -1m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenInventoryClientIsDifferent

		public void TestAdjustOutInventory_ThrowsErrorWhenInventoryClientIsDifferent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var wrongClient = Helper.CreateClient("CL2");
			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, wrongClient, ReduceStockReason.Returned);
			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Client for inventory is different.", () => iTransfer.AdjustOutInventory(inventory, inventory.WI_WL, 1m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenInventoryWarehouseIsDifferent

		public void TestAdjustOutInventory_ThrowsErrorWhenInventoryWarehouseIsDifferent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var wrongWarehouse = Helper.CreateWarehouse("WH2");
			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(wrongWarehouse, data.Org1, ReduceStockReason.Returned);
			var inventory = receive.Inventory[0];
			AssertExceptionThrown(typeof(ArgumentException), "Warehouse for inventory is different.", () => iTransfer.AdjustOutInventory(inventory, inventory.WI_WL, 1m));
		}

		#endregion

		#region TestAdjustOutInventory_ThrowsErrorWhenOriginalLocationIsNotValid

		public void TestAdjustOutInventory_ThrowsErrorWhenOriginalLocationIsNotValid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			AssertExceptionThrown(typeof(ArgumentException), "Original Inventory must be a valid Guid.", () => iTransfer.AdjustOutInventory(receive.Inventory[0], ZGuid.Empty, 1));
			AssertExceptionThrown(typeof(ArgumentException), "Original Inventory must be a valid Guid.", () => iTransfer.AdjustOutInventory(receive.Inventory[0], ZGuid.Invalid, 1));
		}

		#endregion

		#region TestAdjustOutInventory_ReturnPickedOrderLine_Whole

		public void TestAdjustOutInventory_ReturnPickedOrderLine_Whole()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			data.Part1.OP_StockKeepingUnit = "BOT";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var packingDate = ZDate.Today.AddDays(-3);
			var expiryDate = ZDate.Today.AddDays(-2);
			var sourceLocation = data.Whs1.DefaultLocation;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, sourceLocation, "PLT-1", expiryDate, packingDate, "A1", "B2", "C3", "");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			var inventory = receive.Inventory[0];
			iTransfer.AdjustOutInventory(inventory, inventory.WI_WE_InDocketLine, 10m);

			var transfer = (WhsTransfer)iTransfer;
			AssertEquals(1, transfer.Lines.Count);
			var transferLine = transfer.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should Transfer the same units to return.", 10m, transferLine.WE_TransactionQuantity);
				AssertEquals("Product being returned should be the same.", data.Part1.PK, transferLine.WE_OP);
				AssertEquals("From Location should be the Source Location.", sourceLocation.PK, transferLine.WE_WL_TransferFrom);
				AssertEquals("To Location should be the Source Location.", sourceLocation.PK, transferLine.WE_WL);
				AssertEquals("Should have non-release captured attribute 1.", "A1", transferLine.WE_PartAttrib1);
				AssertEquals("Should have non-release captured attribute 2.", "B2", transferLine.WE_PartAttrib2);
				AssertEquals("Should have non-release captured attribute 3.", "C3", transferLine.WE_PartAttrib3);
				AssertEquals("Should have expiry date.", expiryDate, transferLine.WE_ExpiryDate);
				AssertEquals("Should have packing date.", packingDate, transferLine.WE_PackingDate);
				AssertEquals("Should have pack type.", "BOT", transferLine.WE_F3_NKPackType);
				AssertEquals("Should not have pallet id.", "", transferLine.WE_PalletID);
				AssertEquals("Should have Transfer From Pallet Id.", "PLT-1", transferLine.WE_TransferFromPalletId);

				AssertEquals(1, transferLine.PickLines.Count);
				var pickLine = transferLine.PickLines[0];
				AssertEquals("Should Pick all units.", 10m, pickLine.WZ_Units);
				AssertEquals("Pick should be picked now.", true, pickLine.IsPicked);
				AssertEquals("Pick should be picked by current user.", GlbStaff.CurrentUser.GS_Code, pickLine.WZ_GS_NKAssignedTo);
				AssertEquals("Pick should pick from source inventory.", receiveLine.PK, pickLine.WZ_WE_InventoryLine);
			});

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestAdjustOutInventory_ReturnPickedOrderLine_Some

		public void TestAdjustOutInventory_ReturnPickedOrderLine_Some()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			data.Part1.OP_StockKeepingUnit = "BOT";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var packingDate = ZDate.Today.AddDays(-3);
			var expiryDate = ZDate.Today.AddDays(-2);
			var sourceLocation = data.Whs1.DefaultLocation;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, sourceLocation, "PLT-1", expiryDate, packingDate, "A1", "B2", "C3", "");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			var inventory = receive.Inventory[0];
			iTransfer.AdjustOutInventory(inventory, inventory.WI_WE_InDocketLine, 3m);

			var transfer = (WhsTransfer)iTransfer;
			AssertEquals(1, transfer.Lines.Count);
			var transferLine = transfer.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should Transfer the same units to return.", 3m, transferLine.WE_TransactionQuantity);
				AssertEquals("Product being returned should be the same.", data.Part1.PK, transferLine.WE_OP);
				AssertEquals("From Location should be the Source Location.", sourceLocation.PK, transferLine.WE_WL_TransferFrom);
				AssertEquals("To Location should be the Source Location.", sourceLocation.PK, transferLine.WE_WL);
				AssertEquals("Should have non-release captured attribute 1.", "A1", transferLine.WE_PartAttrib1);
				AssertEquals("Should have non-release captured attribute 2.", "B2", transferLine.WE_PartAttrib2);
				AssertEquals("Should have non-release captured attribute 3.", "C3", transferLine.WE_PartAttrib3);
				AssertEquals("Should have expiry date.", expiryDate, transferLine.WE_ExpiryDate);
				AssertEquals("Should have packing date.", packingDate, transferLine.WE_PackingDate);
				AssertEquals("Should have pack type.", "BOT", transferLine.WE_F3_NKPackType);
				AssertEquals("Should have Transfer From Pallet Id.", "PLT-1", transferLine.WE_TransferFromPalletId);
				AssertEquals("Should not have pallet id.", "", transferLine.WE_PalletID);

				AssertEquals(1, transferLine.PickLines.Count);
				var pickLine = transferLine.PickLines[0];
				AssertEquals("Should Pick 3 units.", 3m, pickLine.WZ_Units);
				AssertEquals("Pick should be picked now.", true, pickLine.IsPicked);
				AssertEquals("Pick should be picked by current user.", GlbStaff.CurrentUser.GS_Code, pickLine.WZ_GS_NKAssignedTo);
				AssertEquals("Pick should pick from source inventory.", receiveLine.PK, pickLine.WZ_WE_InventoryLine);
			});

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestAdjustOutInventory_ReturnPickedOrderLine_Staged

		public void TestAdjustOutInventory_ReturnPickedOrderLine_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var stagedTransferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, stagedTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit, stagedTransferLine.WE_CurrentInventoryStatus);
			stagedTransferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Available (original Inventory's status).", InventoryStatus.Codes.Available, stagedTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged, stagedTransferLine.WE_CurrentInventoryStatus);

			orderLine.PickLines.ForEach(pl => pl.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty); // Avoid overpick trigger
			orderLine.PickLines.DeleteAll(); // Avoid overpick trigger

			var inventory = stagedTransferLine.Inventory[0];
			var iTransfer = new PickedStockAdjustersFactory().GetNewAdjuster(data.Whs1, data.Org1, ReduceStockReason.Returned);
			iTransfer.LinkToDocket(order);
			iTransfer.AdjustOutInventory(inventory, originalInventory.PK, 20m);

			var transfer = (WhsTransfer)iTransfer;
			AssertEquals(1, transfer.Lines.Count);
			var transferLine = transfer.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should Transfer the same units to return.", 20m, transferLine.WE_TransactionQuantity);
				AssertEquals("Product being returned should be the same.", data.Part1.PK, transferLine.WE_OP);
				AssertEquals("From Location should be the Pick Location.", stagedTransferLine.WE_WL, transferLine.WE_WL_TransferFrom);
				AssertEquals("To Location should be the Original Location.", originalInventory.WE_WL, transferLine.WE_WL);
				AssertEquals("Inventory line", stagedTransferLine.PK, transferLine.PickLines[0].WZ_WE_InventoryLine);
				AssertEquals("Original inventory line", originalInventory.PK, transferLine.PickLines[0].WZ_WE_OriginalPickedInventoryLine);

				AssertEquals(1, stagedTransferLine.PickLines.Count);
				var pickLine = stagedTransferLine.PickLines[0];
				AssertEquals("Should Pick 20 units.", 20m, pickLine.WZ_Units);
				AssertEquals("Pick should be picked now.", true, pickLine.IsPicked);
				AssertEquals("Pick should be picked by current user.", GlbStaff.CurrentUser.GS_Code, pickLine.WZ_GS_NKAssignedTo);
				AssertEquals("Pick should pick from source inventory.", originalInventory.PK, pickLine.WZ_WE_InventoryLine);
			});

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestAdjustOutInventory_ReturnPickedOrderLine_Staged_MultipleTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var user = Helper.CreateGlbStaff("GUY", "Some Guy");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var originalInventory = receive.Lines.Single();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLineToMakeInTransit = order.Lines.Single(ol => ol.WE_TransactionQuantity == 20m).PickLines.Single();
			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			for (int i = 0; i < 5; i++)
			{
				var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLineToMakeInTransit, ZDateTimeOffset.Today.AddDays(-6 + i), allowMultipleSteps: true);

				var location = i == 4 ? data.Whs1.DefaultOutboundDockDoorLocation : data.Whs1.FindLocation($"A-{i + 2}");
				inTransitLine.WE_WL = location.PK;

				var newPickLine = inTransitLine.PickLines.Single();
				if (i > 0)
				{
					newPickLine.WZ_GS_NKAssignedTo = user.GS_Code;
				}

				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			pickLineToMakeInTransit.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLineToMakeInTransit.WZ_GS_NKAssignedTo = user.GS_Code;
			Factory.Save();

			var orderLine = (WhsOrderLine)order.Lines.Single();
			var finalInTransitTransferLinePK = orderLine.PickLines.Single().WZ_WE_InventoryLine;
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			var result = ReleaseLineReductionManager.ReduceStock(releaseLine, 20, ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);
			AssertEquals($"Reduce Picked Stock should succeed, error was: {result.ErrorMessage}", true, result.IsSuccess);
			Factory.Save();

			var finalInTransitTransferLine = Factory.Load<WhsTransferLine>(finalInTransitTransferLinePK);

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK))[0];
			AssertEquals(1, transfer.Lines.Count);
			var transferLine = transfer.Lines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should Transfer the same units to return.", 20m, transferLine.WE_TransactionQuantity);
				AssertEquals("Product being returned should be the same.", data.Part1.PK, transferLine.WE_OP);
				AssertEquals("From Location should be the Pick Location.", finalInTransitTransferLine.WE_WL, transferLine.WE_WL_TransferFrom);
				AssertEquals("To Location should be the Original Location.", originalInventory.WE_WL, transferLine.WE_WL);
				AssertEquals("Inventory line", finalInTransitTransferLine.PK, transferLine.PickLines[0].WZ_WE_InventoryLine);
				AssertEquals("Original inventory line", originalInventory.PK, transferLine.PickLines[0].WZ_WE_OriginalPickedInventoryLine);
			});

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestLinkToDocket_ThrowsErrorWhenDocketIdNull

		public void TestLinkToDocket_ThrowsErrorWhenDocketIdNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			AssertExceptionThrown<ArgumentNullException>(() => ((IPickedStockAdjuster)transfer).LinkToDocket(null));
		}

		#endregion

		#region TestMakeLinkToOriginalOrder

		public void TestMakeLinkToOriginalOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			((IPickedStockAdjuster)transfer).LinkToDocket(order);
			AssertEquals(transfer, order.RelatedJobs.SingleOrDefault());
			AssertEquals(true, transfer.IsMasterTransfer);
			AssertEquals(null, transfer.MasterTransfer);
		}

		#endregion

		#region TestPrepareForSaving_ReportsValidationErrors

		public void TestPrepareForSaving_ReportsValidationErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, data.Whs1.DefaultLocation, "");

			var result = ((IPickedStockAdjuster)transfer).PrepareForSaving();
			AssertEquals("IsSuccess should be false", false, result.IsSuccess);
			AssertEquals("ErrorMessage must be populated.", false, string.IsNullOrEmpty(result.ErrorMessage));
		}

		#endregion

		#endregion

		#region ITaskPlanningJob

		protected override ZString HumanReadableNameWithoutID => "Warehouse Transfer";

		protected override bool SupportsPlanningStatus(WhsDocket docket) => docket is WhsTransfer transfer ? (!transfer.WD_IsPutawayTransfer && !transfer.IsTransferringForOrder) : base.SupportsPlanningStatus(docket);

		public void TestITaskPlanningJob_TaskPlanningStatus_WhsTransfer()
		{
			var transfer = Factory.New<WhsTransfer>();
			var job = (ITaskPlanningJob)transfer;
			AssertEquals(string.Empty, job.TaskPlanningStatus);

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			CombineAssertions(() =>
			{
				AssertEquals("RFP", job.TaskPlanningStatus);
				AssertEquals(true, transfer.ReadOnly);
			});

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			CombineAssertions(() =>
			{
				AssertEquals("NRP", job.TaskPlanningStatus);
				AssertEquals(false, transfer.ReadOnly);
			});
		}

		public void TestITaskPlanningJob_ClearFKForProcessTasks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			var transfer = Factory.New<WhsTransfer>();
			transfer.WD_OH_Client = data.Org1.PK;
			transfer.WD_WW_Whs = data.Whs1.PK;

			var transferLine = transfer.Lines.AddNew();
			var processTask = Helper.CreateProcessTaskForTransfer(transfer, staff);
			transferLine.WE_P9_Task = processTask.PK;

			var job = (ITaskPlanningJob)transfer;

			AssertEquals("Precondition", processTask.PK, transferLine.WE_P9_Task);
			job.ClearFKForProcessTasks(new HashSet<ZGuid> { processTask.PK });
			AssertEquals("The FK should be cleared.", ZGuid.Empty, transferLine.WE_P9_Task);
		}

		#endregion

		#region ICriticalChangesVersionID

		protected override void UpdatedDocketVersionAndSubscribeToFactoryCore(BusinessObjectFactory factory, WhsDocket docket)
			=> UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsTransfer>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, docket.PK);

		protected override WhsDocketLine PrepareValidDocketLineForICriticalChangesTestCore(BusinessObjectFactory factory, WhsDocket docket, OrgSupplierPart product, WhsLocation location)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(factory.Load<OrgHeader>(docket.WD_OH_Client), factory.Load<WhsWarehouse>(docket.WD_WW_Whs), location.WLV_Column.ToString(), product, 10m);
			var inventory = receive.Inventory[0];
			var transferLine = Helper.CreateWhsTransferLine((WhsTransfer)docket, product, 10, inventory.Location, location);
			transferLine.RunPreSaveValidation(); // to commit inventory
			return transferLine;
		}

		protected override WhsTransfer SetupForTestFinaliseDocket(ZString whsName)
		{
			var org = Helper.CreateClient("c1");
			var whs = Helper.CreateWarehouse(whsName, "A", 2, 1);
			var transfer = Helper.CreateWhsTransfer(org, whs);
			transfer.NotificationManager.Push(Notify);
			Factory.Save();

			return transfer;
		}

		public void TestUpdateCriticalChangesVersionIDWithCriticalFieldUpdates()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WHS2", "B", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify, TransferType.Codes.InterWhsSource);
			Factory.Save();

			var changed = 0;
			transfer.WD_CriticalChangesVersionIDInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				changed++;
			};

			transfer.WD_WW_Whs = whs2.PK;
			transfer.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			AssertEquals("Should increment calls to update.", 1, changed);
			Assert("Should update to valid Guid.", transfer.WD_CriticalChangesVersionID.IsValid);

			var org2 = Helper.CreateClient();
			transfer.WD_OH_Client = org2.PK;
			transfer.WD_OH_Client = data.Org1.PK;
			Factory.Save();

			AssertEquals("Should increment calls to update.", 2, changed);
			Assert("Should update to valid Guid.", transfer.WD_CriticalChangesVersionID.IsValid);

			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			transferLine1.FinaliseDocketLine();
			transfer.FinaliseDocket();

			var today = ZDateTimeOffset.Today;
			AssertEquals(true, transfer.WD_FinalisedDate.Date == today.Date);

			Factory.Save();
			AssertEquals("Should increment calls to update.", 3, changed);
			Assert("Should update to empty Guid.", transfer.WD_CriticalChangesVersionID.IsEmpty);
		}

		public void TestUpdateCriticalChangesVersionIDWithSaveUpdateDeleteOfTranferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);

			var changed = 0;
			transfer.WD_CriticalChangesVersionIDInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				changed++;
			};
			Factory.Save();

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10, inventory.Location, inventory.Location);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			AssertEquals("Should increment calls to update.", 1, changed);
			Assert("Should update to valid Guid.", transfer.WD_CriticalChangesVersionID.IsValid);

			transferLine.WE_PartAttrib1 = "Stuff";
			Factory.Save();

			AssertEquals("Should increment calls to update.", 2, changed);
			Assert("Should update to valid Guid.", transfer.WD_CriticalChangesVersionID.IsValid);

			transferLine.Delete();
			Factory.Save();

			AssertEquals("Should increment calls to update.", 3, changed);
			Assert("Should update to valid Guid.", transfer.WD_CriticalChangesVersionID.IsValid);
		}

		#endregion

		#region TestDocketUpdatedByDataRefresh

		protected override void TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);

			transferInNewFactory.WD_ExternalReference = "NEWTR1";
			AssertEquals(false, transferInNewFactory.IsFinalised);
			newFactory.Save();

			AssertEquals("Transfer is still finalised after data refresh.", true, transfer.IsFinalised);
			AssertEquals("External reference is updated after data refresh.", "NEWTR1", transfer.WD_ExternalReference);
			Helper.AssertZCannotSaveExceptionThrown("The Transfer has been updated by another job. Please reload the Transfer.", Factory.Save);
		}

		protected override void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);

			var newCriticalChangesVersionID = ZGuid.NewZGuid();
			AssertNotEquals(transfer.WD_CriticalChangesVersionID, newCriticalChangesVersionID);

			transferInNewFactory.WD_CriticalChangesVersionID = newCriticalChangesVersionID;
			newFactory.Save();

			AssertEquals("WD_CriticalChangesVersionID is updated after data refresh.", newCriticalChangesVersionID, transfer.WD_CriticalChangesVersionID);
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Transfer should have the 'need to reload' notification", true, Notify.ContainsNotificationType(WhsErrorTypes.CannotFinaliseWithoutReload));
		}

		#endregion

		#region Implementation

		void AssertTransfer(WhsTransferLine line, ZString subType, ZGuid whs1PK, ZGuid whs2PK, string locn1, string locn2, ZDecimal quantity)
		{
			AssertEquals(subType, line.Docket.WD_DocketSubType);
			AssertEquals(whs1PK, line.TransferFromWarehousePK);
			AssertEquals(whs2PK, line.DestinationWarehousePK);
			AssertEquals(locn1, line.TransferFromLocationString);
			AssertEquals(locn2, line.LocationString);
			AssertEquals(quantity, line.WE_TransactionQuantity);
		}

		void AssertInterWhsSuccessAndNotification(WhsTransfer transfer, NotificationBuffer notify, string reference)
		{
			AssertEquals(true, transfer.IsFinalised);
			AssertEquals(true, notify.AsString.Contains("The following Transfer(s) has been created or updated to reflect the stock movements to the destination warehouse(s):"));
			AssertEquals(true, notify.AsString.Contains("Reference: " + reference + "-1"));
			AssertEquals(true, notify.AsString.Contains("Reference: " + reference + "-2"));
		}

		protected override void AssertLocations(WhsDocketLine line, WhsInventoryView inventory)
		{
			AssertEquals("WE_WL_TransferFrom", inventory.WI_WL, line.WE_WL_TransferFrom);
			AssertEquals("WE_TransferFromPalletId", inventory.WI_PalletID, line.WE_TransferFromPalletId);
			AssertEquals("TransferFromWarehousePK", inventory.WI_WW_Whs, ((WhsTransferLine)line).TransferFromWarehousePK);
		}
		protected override ZString ExpectedDescription
		{
			get { return "Transfer"; }
		}

		protected override ControllerID ExpectedControllerId
		{
			get { return ControllerIDs.WhsTransfer; }
		}

		protected override WhsTransfer GetDocketForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			return Helper.CreateWhsTransfer(data.Org1, data.Whs1);
		}

		#endregion
	}
}
