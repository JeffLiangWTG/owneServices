using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrder))]
	public class WhsVASOrderTest : WhsBusinessObjectTestCase
	{
		#region Constructor

		public void TestConstructor_SetConcurrencyPolicy()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			AssertEquals("Concurrency Policy should be strict for WVO_CancelledTimeUtc.", ConcurrencyPolicy.Strict, vasOrder.WVO_CancelledTimeUtcInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WVO_GS_NKCancelledBy.", ConcurrencyPolicy.Strict, vasOrder.WVO_GS_NKCancelledByInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WVO_FinalizedTimeUtc.", ConcurrencyPolicy.Strict, vasOrder.WVO_FinalizedTimeUtcInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WVO_GS_NKFinalizedBy.", ConcurrencyPolicy.Strict, vasOrder.WVO_GS_NKFinalizedByInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WVO_WorkCompletedTimeUtc.", ConcurrencyPolicy.Strict, vasOrder.WVO_WorkCompletedTimeUtcInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WVO_GS_NKWorkCompletedBy.", ConcurrencyPolicy.Strict, vasOrder.WVO_GS_NKWorkCompletedByInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WVO_WD_TransferIntoServiceArea.", ConcurrencyPolicy.Strict, vasOrder.WVO_WD_TransferIntoServiceAreaInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be strict for WVO_WD_TransferOutOfServiceArea.", ConcurrencyPolicy.Strict, vasOrder.WVO_WD_TransferOutOfServiceAreaInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency Policy should be Ignore for WVO_CriticalChangesVersionID.", ConcurrencyPolicy.Ignore, vasOrder.WVO_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		#endregion

		#region Related Entities

		#region TestLines

		public void TestLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();

			AssertEquals("Lines collection should have correct type.", typeof(WhsVASOrderLineCollection), vasOrder.Lines.GetType());
			AssertEquals("Lines collection should be registered child editable.", true, vasOrder.IsRegisteredEditableChildObject(vasOrder.Lines));

			AssertEquals("Precondition", false, vasOrder.HasChanges);
			vasOrder.Lines.AddNew();
			AssertEquals("Adding a line should set Has Changes On Order.", true, vasOrder.HasChanges);
		}

		#endregion

		#region TestServiceArea

		public void TestServiceArea()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertEquals("Precondition", data.Whs1.Areas[0].PK, vasOrder.WVO_WA_ServiceArea);
			AssertEquals(data.Whs1.Areas[0], vasOrder.ServiceArea);
		}

		#endregion

		#region TestTransferIntoServiceArea

		public void TestTransferIntoServiceArea()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			AssertNull(vasOrder.TransferIntoServiceArea);

			var transfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferIntoServiceArea = transfer.PK;
			AssertEquals(transfer, vasOrder.TransferIntoServiceArea);
		}

		#endregion

		#region TestTransferOutOfServiceArea

		public void TestTransferOutOfServiceArea()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			AssertNull(vasOrder.TransferOutOfServiceArea);

			var transfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK;
			AssertEquals(transfer, vasOrder.TransferOutOfServiceArea);
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region TestWVO_FinalizedTimeUtc

		public void TestWVO_FinalizedTimeUtc()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			AssertExceptionThrown(typeof(InvalidOperationException), "You cannot finalise the VAS Order without first completing it.", () => vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow);

			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("VAS Order WVO_GS_NKWorkCompletedBy is set.", false, string.IsNullOrEmpty(vasOrder.WVO_GS_NKWorkCompletedBy));

			Factory.Save();
			vasOrder.FinaliseVASOrder(Notify);
			AssertEquals("VAS Order is finalised.", true, vasOrder.IsFinalised);
			AssertEquals("VAS Order WVO_GS_NKFinalizedBy is set.", false, string.IsNullOrEmpty(vasOrder.WVO_GS_NKFinalizedBy));
			AssertExceptionThrown(typeof(InvalidOperationException), "Once a VAS Order is finalised, you cannot unfinalise it.", () => vasOrder.WVO_FinalizedTimeUtc = ZDateTime.Empty);
		}

		#endregion

		#region	TestWVO_FinalizedTimeUtc_FromTransferOut

		[TestDate(2020, 6, 16)]
		public void TestWVO_FinalizedTimeUtc_FromTransferOut()
		{
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			AssertExceptionThrown(typeof(InvalidOperationException), "You cannot finalise the VAS Order without first completing it.", () => vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Has value only if when is finalize.", ZDateTime.Empty, vasOrder.WVO_FinalizedTimeUtc);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(initialTransfer);
			AssertEquals("Has value only if when is finalize.", ZDateTime.Empty, vasOrder.WVO_FinalizedTimeUtc);

			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Has value only if when is finalize.", ZDateTime.Empty, vasOrder.WVO_FinalizedTimeUtc);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals("Precondition: VAS Order is finalised.", true, vasOrder.IsFinalised);
			AssertNotEquals("Has value if when is finalize.", ZDateTime.Empty, vasOrder.WVO_FinalizedTimeUtc);
			AssertNotEquals("Should not be same as transfer in finalise date.", initialTransfer.WD_FinalisedDate, vasOrder.WVO_FinalizedTimeUtc);
			AssertEquals("Should be same as transfer out finalise date.", returnTransfer.WD_FinalisedDate.ToUtcDateTime(), vasOrder.WVO_FinalizedTimeUtc);
		}

		#endregion

		#region TestWVO_FinalizedTimeUtcInfo

		public void TestWVO_FinalizedTimeUtcInfo()
		{
			AssertEquals("Concurrency Policy should be strict for WVO_FinalizedTimeUtc.", ConcurrencyPolicy.Strict, Factory.New<WhsVASOrder>().WVO_FinalizedTimeUtcInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWVO_WorkCompletedTimeUtc

		public void TestWVO_WorkCompletedTimeUtc()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			AssertExceptionThrown(typeof(InvalidOperationException), "You cannot set work as completed without a Into Service Area Transfer.", () => vasOrder.WVO_WorkCompletedTimeUtc = ZDateTime.UtcNow);

			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("VAS Order is complete by set.", false, string.IsNullOrEmpty(vasOrder.WVO_GS_NKWorkCompletedBy));
			AssertExceptionThrown(typeof(InvalidOperationException), "Once a VAS Order is completed, you cannot mark it as incomplete.", () => vasOrder.WVO_WorkCompletedTimeUtc = ZDateTime.Empty);
		}

		#endregion

		#region TestWVO_JobID

		public void TestWVO_JobID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertEquals("Precondition.", ZString.Empty, vasOrder.WVO_JobID);

			Factory.Save();
			AssertEquals("Should have set Job ID.", "WV00000001", vasOrder.WVO_JobID);

			Factory.Save();
			AssertEquals("Should *not* change Job ID.", "WV00000001", vasOrder.WVO_JobID);
			AssertEquals("WV00000001", ((IJobNumber)vasOrder).JobNumber);
		}

		#endregion

		#region TestWVO_JobIDIsClearedOnSaveFailure

		public void TestWVO_JobIDIsClearedOnSaveFailure()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder1 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			vasOrder1.WVO_JobID = "abc";
			vasOrder2.WVO_JobID = "abc";
			AssertEquals("Precondition - VAS Order 1's JobID must not be empty.", false, vasOrder1.WVO_JobID.IsEmpty);
			AssertEquals("Precondition - VAS Order 2's JobID must not be empty.", false, vasOrder2.WVO_JobID.IsEmpty);

			AssertExceptionThrown("Precondition - Should have thrown a save exception as the JobIDs are duplicated.", typeof(ZSaveException), () => Factory.Save());
			AssertEquals(true, vasOrder1.WVO_JobID.IsEmpty);
			AssertEquals(true, vasOrder2.WVO_JobID.IsEmpty);
		}

		#endregion

		#region TestWVO_WD_TransferIntoServiceArea_UpdateVersionIdPolicyWhenSet

		public void TestWVO_WD_TransferIntoServiceArea_UpdateVersionIdPolicyWhenSet()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			AssertEquals("Concurrency Policy should be Ignore when WVO_WD_TransferIntoServiceArea is not set.", ConcurrencyPolicy.Ignore, vasOrder.WVO_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			vasOrder.WVO_WD_TransferIntoServiceArea = Factory.New<WhsTransfer>().PK;
			AssertEquals("Concurrency Policy should be Strict when WVO_WD_TransferIntoServiceArea is set.", ConcurrencyPolicy.Strict, vasOrder.WVO_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
			AssertEquals("Concurrency Policy should be Ignore when WVO_WD_TransferIntoServiceArea is not set.", ConcurrencyPolicy.Ignore, vasOrder.WVO_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWVO_WD_TransferOutOfServiceArea

		public void TestWVO_WD_TransferOutOfServiceArea()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			vasOrder.WVO_WD_TransferIntoServiceArea = Factory.New<WhsTransfer>().PK;

			var transfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK;

			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot overwrite TransferOutOfServiceArea with another one.",
					() => vasOrder.WVO_WD_TransferOutOfServiceArea = ZGuid.NewZGuid());

			vasOrder.WVO_WD_TransferOutOfServiceArea = ZGuid.Empty;
			vasOrder.WVO_WorkCompletedTimeUtc = ZDateTime.UtcNow;
			vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow;
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot create a TransferOutOfServiceArea if the VAS Order is already finalised.",
					() => vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK);
		}

		#endregion

		#region TestTransferIntoServiceArea

		public void TestWVO_WD_TransferIntoServiceArea()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			vasOrder.WVO_WD_TransferIntoServiceArea = Factory.New<WhsTransfer>().PK;

			AssertExceptionThrown<InvalidOperationException>("Cannot overwrite TransferIntoServiceArea with another one.",
					() => vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.NewZGuid());
		}

		#endregion

		// calculated

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertEquals("Warehouse VAS Order", vasOrder.HumanReadableName);

			Factory.Save();
			AssertEquals("Warehouse VAS Order WV00000001", vasOrder.HumanReadableName);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertEquals("Preconditon", "", vasOrder.ProductCode);

			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 2m);
			AssertEquals("P1", vasOrder.ProductCode);

			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 2m);
			AssertEquals("P1", vasOrder.ProductCode);

			var part2Line = Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 3m);
			AssertEquals("Many", vasOrder.ProductCode);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Desc = "P1 Description";
			data.Part2.OP_Desc = "Description P2";
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertEquals("Preconditon", "", vasOrder.ProductDescription);

			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 2m);
			AssertEquals("P1 Description", vasOrder.ProductDescription);

			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 2m);
			AssertEquals("P1 Description", vasOrder.ProductDescription);

			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 3m);
			AssertEquals("Many", vasOrder.ProductDescription);
		}

		#endregion

		#region TestProductQty

		public void TestProductQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertEquals("Preconditon", 0m, vasOrder.ProductQty);

			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 2m);
			AssertEquals(2m, vasOrder.ProductQty);

			var part2Line = Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 3m);
			AssertEquals(5m, vasOrder.ProductQty);

			part2Line.Delete();
			AssertEquals(2m, vasOrder.ProductQty);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder2, data.Part2, 10m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			AssertEquals("NEW ENTRY", vasOrder1.Status);
			AssertEquals("NEW ENTRY", vasOrder2.Status);

			Factory.Save();
			AssertEquals("ENTERED", vasOrder1.Status);
			AssertEquals("ENTERED", vasOrder2.Status);

			var initialTransfer1 = vasOrder1.GetOrCreateInitialTransfer(Notify);
			var initialTransfer2 = vasOrder2.GetOrCreateInitialTransfer(Notify);
			AssertEquals("TRANSFERRING IN", vasOrder1.Status);
			AssertEquals("TRANSFERRING IN", vasOrder2.Status);

			initialTransfer1.FinaliseDocketWithoutUserConfirmation();
			initialTransfer2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("WORKING", vasOrder1.Status);
			AssertEquals("WORKING", vasOrder2.Status);

			Factory.Save();
			vasOrder1.MarkVASOrderAsCompleted(Notify);
			vasOrder2.MarkVASOrderAsCompleted(Notify);
			AssertEquals("WORK COMPLETED", vasOrder1.Status);
			AssertEquals("WORK COMPLETED", vasOrder2.Status);

			Factory.Save();
			vasOrder2.FinaliseVASOrder(Notify);
			AssertEquals("FINALIZED", vasOrder2.Status);

			WhsTransfer transferOutOfServiceArea;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				transferOutOfServiceArea = vasOrder1.GetOrCreateReturnTransfer(Notify);
			}
			AssertEquals("TRANSFERRING OUT", vasOrder1.Status);

			transferOutOfServiceArea.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("FINALIZED", vasOrder1.Status);
		}

		#endregion

		#region TestWarehousePK

		public void TestWarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Factory.New<WhsVASOrder>();
			vasOrder.WVO_OH_Client = Helper.CreateClient().PK;
			AssertEquals("Precondition", ZGuid.Empty, vasOrder.WarehousePK);

			vasOrder.WVO_WA_ServiceArea = data.Whs1.Areas[0].PK;
			AssertEquals(data.Whs1.PK, vasOrder.WarehousePK);

			vasOrder.WVO_WA_ServiceArea = ZGuid.Empty;
			AssertEquals(data.Whs1.PK, vasOrder.WarehousePK);

			vasOrder.WVO_WA_ServiceArea = data.Whs1.Areas[0].PK;
			AssertEquals(data.Whs1.PK, vasOrder.WarehousePK);

			var newWarehouse = Helper.CreateWarehouse("2");
			vasOrder.WarehousePK = newWarehouse.PK;
			AssertEquals(ZGuid.Empty, vasOrder.WVO_WA_ServiceArea);
			AssertEquals(newWarehouse.PK, vasOrder.WarehousePK);

			vasOrder.WVO_WA_ServiceArea = data.Whs1.Areas[0].PK;
			AssertEquals(data.Whs1.PK, vasOrder.WarehousePK);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			AssertEquals(data.Whs1.PK, vasOrderInOtherFactory.WarehousePK);

			vasOrderInOtherFactory.WVO_WA_ServiceArea = ZGuid.Empty;
			AssertEquals(data.Whs1.PK, vasOrderInOtherFactory.WarehousePK);
		}

		#endregion

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertEquals("Preconditon", false, vasOrder.IsFinalised);

			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.NewZGuid();
			vasOrder.WVO_WorkCompletedTimeUtc = ZDateTime.UtcNow;
			vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow;

			AssertEquals("IsFinalised should be true", true, vasOrder.IsFinalised);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			AssertEquals(true, Factory.New<WhsVASOrder>().IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestIsFinalisingOrTransferringOut

		public void TestIsFinalisingOrTransferringOut_Finalising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 5m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var isFinalising = vasOrder.IsFinalisingOrTransferringOut;
			AssertEquals(false, isFinalising);

			ZPropertyValueChangedEventHandler handler = (property, oldValue) => { isFinalising = vasOrder.IsFinalisingOrTransferringOut; };
			vasOrder.PropertyValueChanged += handler;

			Notify.Clear();
			Notify.DefaultResponse = true;
			AssertEquals("Should Finalise VAS Order.", true, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should Finalise VAS Order.", true, vasOrder.IsFinalised);
			AssertEquals("Flag changes to true when Finalising.", true, isFinalising);
			AssertEquals("Flag reset to false after Finalising.", false, vasOrder.IsFinalisingOrTransferringOut);
			vasOrder.PropertyValueChanged -= handler;
		}

		public void TestIsFinalisingOrTransferringOut_TransferringOut()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 5m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var isTransferringOut = vasOrder.IsFinalisingOrTransferringOut;
			AssertEquals(false, isTransferringOut);

			ZPropertyValueChangedEventHandler handler = (property, oldValue) => { isTransferringOut = vasOrder.IsFinalisingOrTransferringOut; };
			vasOrder.PropertyValueChanged += handler;

			Notify.Clear();
			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNotNull("Should be able to create return transfer.", returnTransfer);
			}
			AssertEquals("Flag changes to true when TransferringOut.", true, isTransferringOut);
			AssertEquals("Flag reset to false after TransferringOut.", false, vasOrder.IsFinalisingOrTransferringOut);
			vasOrder.PropertyValueChanged -= handler;
		}

		#endregion

		#endregion

		#region Delete

		public void TestDelete()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			var line = vasOrder.Lines.AddNew();
			vasOrder.Delete();
			AssertEquals("Deleting a VAS Order should delete its associated lines.", true, line.IsDeleted);
			// deletion of WorkflowItems is in the VAS Order Workflow Provider test
		}

		#endregion

		#region Save

		#region TestCustomerReferenceNoIsPopulatedOnSaveIfEmpty

		public void TestCustomerReferenceNoIsPopulatedOnSaveIfEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			AssertEquals("Precondition: Customer Reference is initially empty.", "", vasOrder1.WVO_CustomerReferenceNo);

			Factory.Save();
			AssertEquals("Precondition: Job Number is populated on Save.", false, vasOrder1.WVO_JobID.IsEmpty);
			AssertEquals("An empty Customer Reference should be populated with the Job ID.", vasOrder1.WVO_JobID, vasOrder1.WVO_CustomerReferenceNo);

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder2.WVO_CustomerReferenceNo = "abc";
			Factory.Save();
			AssertEquals("An existing Customer Reference should *not* be overridden with the Job ID.", "abc", vasOrder2.WVO_CustomerReferenceNo);

			var vasOrder3 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder3.WVO_WA_ServiceArea = ZGuid.NewZGuid(); // to violate FK constraint
			try
			{ Factory.Save(); }
			catch (ZSaveException) { }
			AssertEquals("The Customer Reference should be rolled back on Save failure.", "", vasOrder3.WVO_CustomerReferenceNo);
			vasOrder3.Delete(); // cleanup

			var vasOrder4 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder4.WVO_CustomerReferenceNo = "xyz";
			vasOrder4.WVO_WA_ServiceArea = ZGuid.NewZGuid(); // to violate FK constraint
			try
			{ Factory.Save(); }
			catch (ZSaveException) { }
			AssertEquals("An existing Customer Reference should *not* be rolled back on Save failure.", "xyz", vasOrder4.WVO_CustomerReferenceNo);
			vasOrder4.Delete(); // cleanup

			var vasOrder5 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Factory.Save();
			AssertEquals("Precondition: Customer Reference populated on Save.", false, vasOrder5.WVO_CustomerReferenceNo.IsEmpty);
			vasOrder5.WVO_CustomerReferenceNo = "";
			Factory.Save();
			AssertEquals("Customer Reference should *never* be empty.", false, vasOrder5.WVO_CustomerReferenceNo.IsEmpty);
		}

		#endregion

		#region TestServiceCommencedEventIsAddedOnSave

		public void TestServiceCommencedEventIsAddedOnSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			AssertEquals("Precondition:", 0, vasOrder1.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceCommencedCode).Count());

			Factory.Save();
			AssertEquals("Should have added the Service Commenced Event on Save.", 1, vasOrder1.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceCommencedCode).Count());

			vasOrder1.WVO_CustomerReferenceNo = "NEW_CUSTOMER_REF";
			Factory.Save();
			AssertEquals("Should never add the Service Commenced Event again once VAS Order is in DB.", 1,
				vasOrder1.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceCommencedCode).Count());

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			vasOrder2.WVO_JobID = vasOrder1.WVO_JobID; // Cause Save Failure
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("Should not have a Service Commenced Log if the Save failed.", 0, vasOrder2.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceCommencedCode).Count());
		}

		#endregion

		#endregion

		// functions

		#region TestFetchForLoad

		public override void TestFetchForLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var inventoryLocation = data.Whs1.FindLocation("A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, inventoryLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m, inventoryLocation, "");
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 5m);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			factory2.Load<WhsVASOrder>(vasOrder.PK);

			var dbHits = new Dictionary<string, int>();
			dbHits.Add(WhsVASOrderSchema.Constants.TableName, 1);
			AssertDbHits(dbHits, factory2);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", initialTransfer);
			AssertEquals("Precondition", 2, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNotNull("Precondition", returnTransfer);
			}

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(returnTransfer);

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			factory3.Load<WhsVASOrder>(vasOrder.PK);

			AssertDbHits(dbHits, factory3);
		}

		#endregion

		#region TestFinaliseVASOrder

		public void TestFinaliseVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 5m, data.Whs1.FindLocation("A"), "");

			AssertEquals("Should not Finalise VAS Order if there are changes.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not Finalise VAS Order if there are changes.", false, vasOrder.IsFinalised);
			AssertEquals("Should not Finalise VAS Order if there are changes.", "Save all changes before Finalizing this VAS Order.\r\n", Notify.AsString);

			Notify.Clear();
			Factory.Save();
			AssertEquals("Should not Finalise VAS Order if work is not complete.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not Finalise VAS Order if work is not complete.", false, vasOrder.IsFinalised);
			AssertEquals("Should not Finalise VAS Order if work is not complete.", "Cannot Finalize VAS Order until the Work has been Completed.\r\n", Notify.AsString);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 2, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			Notify.Clear();
			AssertEquals("Should not Finalise VAS Order if work is not complete.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not Finalise VAS Order if work is not complete.", false, vasOrder.IsFinalised);
			AssertEquals("Should not Finalise VAS Order if work is not complete.", "Cannot Finalize VAS Order until the Work has been Completed.\r\n", Notify.AsString);

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();
			Notify.Clear();
			using (vasOrder.SuspendSettingHasChanges())
			{
				vasOrder.WVO_CancelledTimeUtc = ZDateTime.UtcNow;
				AssertEquals(false, vasOrder.FinaliseVASOrder(Notify));
				AssertEquals("Cannot Finalize Canceled VAS Orders.\r\n", Notify.AsString);

				vasOrder.WVO_CancelledTimeUtc = ZDateTime.Empty;
			}
			Notify.Clear();

			Notify.DefaultResponse = false;
			AssertEquals("Should not Finalise VAS Order if user said no.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not Finalise VAS Order if user said no.", false, vasOrder.IsFinalised);

			var queryUserArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("User query Caption should be correct.", "Finalize VAS Order", queryUserArgs.Caption);
			AssertEquals("User query Message should be correct.",
@"Finalizing the VAS Order will make the Stock Transferred into the Service Area available for Picking.
You will not be allowed to Create a Transfer out of the Service Area from this VAS Order if you Finalize it.
This process cannot be undone.

Are you sure you want to do this?", queryUserArgs.Message);
			AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNo, queryUserArgs.Context.Buttons);
			AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No }, queryUserArgs.Context.DialogResultsToNotSave);

			Notify.Clear();
			Notify.DefaultResponse = true;
			AssertEquals("Should Finalise VAS Order.", true, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should Finalise VAS Order.", true, vasOrder.IsFinalised);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
			AssertEquals("Should have added a Finalised Event.", 1, vasOrder.Logs.Find(
				l => l.SL_SE_NKEvent == Events.ItemDocumentJobFinalisedCode && l.SL_Reference == "VAS Order").Count());

			var transferLine1 = initialTransfer.Lines[0];
			var transferLine2 = initialTransfer.Lines[1];
			var matchingLine = initialTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OP == data.Part1.PK).MatchingLines.Single();
			AssertEquals("Inventory on the Transfer should be made available.", InventoryStatus.Codes.Available, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on the Transfer should be made available.", InventoryStatus.Codes.Available, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on the Transfer should be made available.", InventoryStatus.Codes.Available, matchingLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory on the Transfer should be made available.", true, transferLine1.Inventory[0].IsAvailable);
			AssertEquals("Inventory on the Transfer should be made available.", true, transferLine2.Inventory[0].IsAvailable);
			AssertEquals("Inventory on the Transfer should be made available.", true, matchingLine.Inventory[0].IsAvailable);

			Factory.Save();
			Notify.Clear();
			AssertEquals("Should not re-finalise the VAS Order if it is already finalised.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not re-finalise the VAS Order if it is already finalised.", "VAS Order already Finalized.\r\n", Notify.AsString);
		}

		#endregion

		#region TestFinaliseVASOrder_WhenOutOfServiceAreaTransferExists

		public void TestFinaliseVASOrder_WhenOutOfServiceAreaTransferExists()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var inventoryLocationPK = receive.Lines.Single().WE_WL;
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
				AssertEquals("Precondition: Return Transfer is transferring correct stock.", 1, returnTransfer.Lines.Count);
			}

			Factory.Save();
			AssertEquals("Should not be able to finalise the VAS Order if the Out of Service Area Transfer exists.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not be able to finalise the VAS Order if the Out of Service Area Transfer exists.", false, vasOrder.IsFinalised);
			AssertEquals("Should not be able to finalise the VAS Order if the Out of Service Area Transfer exists.",
				"There is already a Transfer out of the Service Area for this VAS Order and the Order can no longer be Finalized this way. You must Finalize the Transfer instead.\r\n", Notify.AsString);
		}

		#endregion

		#region TestFinaliseVASOrder_GivesPromptWithDefaultableQueryEventArgs

		public void TestFinaliseVASOrder_GivesPromptWithDefaultableQueryEventArgs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.FindLocation("A"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 5m, data.Whs1.FindLocation("A"), "");

			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Precondition: VAS Order is unfinalized", false, vasOrder.IsFinalised);

			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
				}
			};

			Factory.Save();

			vasOrder.FinaliseVASOrder(Notify);

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Postcondition: correct eventArgs type", true, lastQueryEventArgs is DefaultableQueryUserEventArgs);
			var expectedMessage = "Finalizing the VAS Order will make the Stock Transferred into the Service Area available for Picking.\r\nYou will not be allowed to Create a Transfer out of the Service Area from this VAS Order if you Finalize it.\r\nThis process cannot be undone.\r\n\r\nAre you sure you want to do this?";
			AssertEquals("Postcondition: correct notification message", expectedMessage, lastQueryEventArgs.Message);
			AssertEquals("Postcondition: correct response", true, lastQueryEventArgs.Response);
			AssertEquals("Postcondition: correct notification caption", "Finalize VAS Order", lastQueryEventArgs.Caption);
			AssertEquals("Postcondition: VAS Order is finalized", true, vasOrder.IsFinalised);
			AssertEquals("Postcondition: VAS Order has no errors.", false, vasOrder.HasErrors);
		}

		#endregion

		#region TestFinaliseVASOrder_WhenThereAreIncompleteServices

		public void TestFinaliseVASOrder_WhenThereAreIncompleteServices()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();
			Notify.Clear();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			var service1 = Factory.New<WhsJobService>();
			service1.ES_ParentID = vasOrder.PK;
			service1.ES_ParentTableCode = vasOrder.TablePrefix;

			var service2 = Factory.New<WhsJobService>();
			service2.ES_ParentID = vasOrder.PK;
			service2.ES_ParentTableCode = vasOrder.TablePrefix;

			AssertEquals("Collection was not loaded and/or the relationship filter is incorrect.", 2, vasOrder.Services.Count);

			Factory.Save();
			Notify.Clear();
			Notify.DefaultResponse = true;
			AssertEquals("Should not Finalise VAS Order if there are incomplete services.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not Finalise VAS Order if there are incomplete services.", "Cannot Finalize VAS Order until all Services have been Completed.\r\n", Notify.AsString);

			Notify.Clear();
			service1.ES_Completed = ZDateTime.Today;
			Factory.Save();
			AssertEquals("Should not Finalise VAS Order if there are incomplete services.", false, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not Finalise VAS Order if there are incomplete services.", "Cannot Finalize VAS Order until all Services have been Completed.\r\n", Notify.AsString);

			Notify.Clear();
			Notify.DefaultResponse = true;
			service2.ES_Completed = ZDateTime.Today;
			Factory.Save();

			Notify.Clear();
			Notify.DefaultResponse = true;
			AssertEquals("Should Finalise VAS Order.", true, vasOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should Finalise VAS Order.", true, vasOrder.IsFinalised);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer

		public void TestGetOrCreateInitialTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var part5 = Helper.CreateProduct(data.Org1, "P5");
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Save all changes before creating the Transfer to Service Area.\r\n", Notify.AsString);
			Factory.Save();

			Notify.Clear();
			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Cannot create Transfer as there are no Service Lines.\r\n", Notify.AsString);

			Notify.Clear();
			var varOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			var varOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 7m);
			var varOrderLine3 = Helper.CreateWhsVASOrderLine(vasOrder, part3, 20m);
			var varOrderLine4 = Helper.CreateWhsVASOrderLine(vasOrder, part4, 2m);
			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Save all changes before creating the Transfer to Service Area.\r\n", Notify.AsString);

			// fill locations with stock
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 6m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 21m, data.Whs1.FindLocation("A-2"), "PLT-3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part4, 2m, data.Whs1.FindLocation("A-2"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", part5, 6m, data.Whs1.FindLocation("A-2"), "");

			Factory.Save();
			Notify.Clear();
			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			// there are no P1s in the warehouse and 6 units of P2
			AssertEquals("Cannot create Transfer because of the following Shortfalls:\r\n10x P1\r\n1x P2\r\n", Notify.AsString);

			// receive in 12 units of P1 and 4 more units of P2 to ensure there is no overcommitting of stock
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 1m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 1m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part1, 4m, data.Whs1.FindLocation("A-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", data.Part1, 5m, data.Whs1.FindLocation("A-2"), "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R9", data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R10", data.Part2, 4m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();
			Notify.Clear();

			using (vasOrder.SuspendSettingHasChanges())
			{
				vasOrder.IsCancelled = true;
				AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
				AssertEquals("Cannot create Transfer for Canceled VAS Orders.\r\n", Notify.AsString);

				vasOrder.IsCancelled = false;
			}
			Notify.Clear();

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull(transfer);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
			AssertEquals("VAS Order should become Read-only after Initial Transfer is created.", true, vasOrder.ReadOnly);
			AssertEquals("VAS Order Lines should become Read-only after Initial Transfer is created.", true, varOrderLine1.ReadOnly);
			AssertEquals("VAS Order Lines should become Read-only after Initial Transfer is created.", true, varOrderLine2.ReadOnly);
			AssertEquals("VAS Order Lines should become Read-only after Initial Transfer is created.", true, varOrderLine3.ReadOnly);
			AssertEquals("VAS Order Lines should become Read-only after Initial Transfer is created.", true, varOrderLine4.ReadOnly);
			AssertEquals("No reason for Triggers to become read-only.", false, vasOrder.WorkflowItems.ReadOnly);

			Factory.Save();
			AssertEquals("Transfer Into Service Area should be set on VAS Order.", transfer.PK, vasOrder.WVO_WD_TransferIntoServiceArea);
			AssertEquals("Transfer should have the same client as VAS Order.", data.Org1.PK, transfer.WD_OH_Client);
			AssertEquals("Transfer should have the same warehouse as VAS Order.", data.Whs1.PK, transfer.WD_WW_Whs);
			AssertEquals("Transfer external reference should contain VAS Order Number.", "WV00000001 W00000011", transfer.WD_ExternalReference);
			AssertEquals(6, transfer.Lines.Count);

			// ensure we successfully created transfer with correct total units (10 units of P1 and 7 units of P2)
			var transferLine1 = transfer.Lines.Cast<WhsTransferLine>().Single(t => t.WE_WL_TransferFrom == data.Whs1.FindLocation("A-1").PK && t.WE_OP != data.Part2.PK);
			var transferLine2 = transfer.Lines.Cast<WhsTransferLine>().Single(t => t.WE_WL_TransferFrom == data.Whs1.FindLocation("A-2").PK && t.WE_TransferFromPalletId == "");
			var transferLine3 = transfer.Lines.Cast<WhsTransferLine>().Single(t => t.WE_WL_TransferFrom == data.Whs1.FindLocation("A-2").PK && t.WE_TransferFromPalletId == "PLT-1" && t.WE_OP != part4.PK);
			var transferLine4 = transfer.Lines.Cast<WhsTransferLine>().Single(t => t.WE_OP == data.Part2.PK);
			var transferLine5 = transfer.Lines.Cast<WhsTransferLine>().Single(t => t.WE_OP == part3.PK);
			var transferLine6 = transfer.Lines.Cast<WhsTransferLine>().Single(t => t.WE_OP == part4.PK);
			AssertEquals(data.Part1.PK, transferLine1.WE_OP);
			AssertEquals(data.Part1.PK, transferLine2.WE_OP);
			AssertEquals(data.Part1.PK, transferLine3.WE_OP);
			AssertEquals(data.Whs1.FindLocation("A-1").PK, transferLine4.WE_WL_TransferFrom);
			AssertEquals(data.Whs1.FindLocation("A-2").PK, transferLine5.WE_WL_TransferFrom);
			AssertEquals("PLT-3", transferLine5.WE_TransferFromPalletId);
			AssertEquals(data.Whs1.FindLocation("A-2").PK, transferLine6.WE_WL_TransferFrom);
			AssertEquals("PLT-1", transferLine6.WE_TransferFromPalletId);

			// Pallet Ids
			AssertEquals("Pallet Id should be retained if transferring whole Pallet.", "PLT-1", transferLine3.WE_PalletID);
			AssertEquals("Pallet Id should be retained if transferring whole Pallet.", "PLT-1", transferLine6.WE_PalletID);
			AssertEquals("Pallet Id should be dropped if not transferring whole Pallet.", "", transferLine5.WE_PalletID);

			// Service Location
			AssertEquals("All Transfer Lines should be to the Service Area Location.", serviceArea.PickLocations.Single().PK, transferLine1.WE_WL);
			AssertEquals("All Transfer Lines should be to the Service Area Location.", serviceArea.PickLocations.Single().PK, transferLine2.WE_WL);
			AssertEquals("All Transfer Lines should be to the Service Area Location.", serviceArea.PickLocations.Single().PK, transferLine3.WE_WL);
			AssertEquals("All Transfer Lines should be to the Service Area Location.", serviceArea.PickLocations.Single().PK, transferLine4.WE_WL);
			AssertEquals("All Transfer Lines should be to the Service Area Location.", serviceArea.PickLocations.Single().PK, transferLine5.WE_WL);
			AssertEquals("All Transfer Lines should be to the Service Area Location.", serviceArea.PickLocations.Single().PK, transferLine6.WE_WL);

			// P1 - Should have committed a total of 10 units
			AssertEquals(1m, transferLine1.QtyToMoveIncludingMatchingLines);
			AssertEquals(4m, transferLine2.QtyToMoveIncludingMatchingLines);
			AssertEquals(5m, transferLine3.QtyToMoveIncludingMatchingLines);
			// P2 - Should have committed a total of 7 units
			AssertEquals(7m, transferLine4.QtyToMoveIncludingMatchingLines);
			// P3 - Should have committed a total of 20 units
			AssertEquals(20m, transferLine5.QtyToMoveIncludingMatchingLines);
			// P4 - Should have committed a total of 2 units
			AssertEquals(2m, transferLine6.QtyToMoveIncludingMatchingLines);

			Factory.Save();
			AssertEquals("If transfer has already been created, it should be returned.", transfer, vasOrder.GetOrCreateInitialTransfer(Notify));

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Transfer should have been created in a finalisable state.", true, transfer.IsFinalised);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_MultipleOrderLinesForSameProduct

		public void TestGetOrCreateInitialTransfer_MultipleOrderLinesForSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRow(data.Whs1, "B");
			var emptyArea = Helper.CreateArea(data.Whs1, "Service");
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PickingArea = emptyArea.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("B"), "PLT-1");

			var vasOrder = Helper.CreateWhsVASOrder(emptyArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 6m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 6m);
			Factory.Save();

			AssertNull("Initial Transfer should not be created.", vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Cannot create Transfer because of the following Shortfalls:\r\n2x P1\r\n", Notify.AsString);
		}

		public void TestGetOrCreateInitialTransfer_MultipleOrderLinesForSameProduct_WithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.CreateRow(data.Whs1, "B");
			var emptyArea = Helper.CreateArea(data.Whs1, "Service");
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PickingArea = emptyArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("B"), "PLT-1", finalise: false);
			receive.Lines[0].WE_PartAttrib1 = "RED";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var vasOrder = Helper.CreateWhsVASOrder(emptyArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 6m);
			var orderLineWithAttribs = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 6m);
			orderLineWithAttribs.WVL_PartAttrib1 = "RED";
			Factory.Save();

			AssertNull("Initial Transfer should not be created.", vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Cannot create Transfer because of the following Shortfalls:\r\n6x P1\r\n", Notify.AsString);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_MustSpecifyAttribute

		public void TestGetOrCreateInitialTransfer_MustSpecifyAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.CreateRow(data.Whs1, "B");
			var emptyArea = Helper.CreateArea(data.Whs1, "Service");
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PickingArea = emptyArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("B"), "PLT-1", finalise: false);
			receive.Lines[0].WE_PartAttrib1 = "RED";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var vasOrder = Helper.CreateWhsVASOrder(emptyArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			AssertNull("Initial Transfer should not be created.", vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Cannot create Transfer because of the following Shortfalls:\r\n10x P1\r\n", Notify.AsString);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_GetsStockFromFreeStore

		public void TestGetOrCreateInitialTransfer_GetsStockFromFreeStore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = data.Whs1.Areas[0];
			bondedArea.WA_AreaType = AreaTypes.Codes.Bonded;
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "EntryNumber123-1", false, false);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			AssertNull("Should not create Initial Transfer.", vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Should inform the user of a Shortfall.", "Cannot create Transfer because of the following Shortfalls:\r\n10x P1\r\n", Notify.AsString);
			AssertEquals("Should have cleared TransferIn FK.", true, vasOrder.WVO_WD_TransferIntoServiceArea.IsEmpty);
			AssertEquals("Should *not* have triggered has changes.", false, vasOrder.HasChanges);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_MandatoryCustomAttributes

		public void TestGetOrCreateInitialTransfer_MandatoryCustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var customAttrib1 = data.Org1.CustomLabels.AddNew();
			customAttrib1.OT_FieldName = Core.Constants.CustomLabels.WhsDocket.CustomAttribute1;
			customAttrib1.OT_IsMandatory = true;

			Helper.CreateRow(data.Whs1, "B");
			var emptyArea = Helper.CreateArea(data.Whs1, "EMPTY");
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PickingArea = emptyArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("B"), "", finalise: false);
			receive.WD_CustomAttrib1 = "TEST";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var vasOrder = Helper.CreateWhsVASOrder(emptyArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Initial Transfer should be created.", initialTransfer);
			AssertEquals("Should have no errors.", false, initialTransfer.HasErrors);
			AssertEquals("Should have triggered has changes.", true, vasOrder.HasChanges);
		}

		#endregion

		#region TestTransitInventory_GetValidNotFullServiceAreaLocations

		public void TestTransitInventory_GetValidNotFullServiceAreaLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRow(data.Whs1, "B");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Factory.Save();

			var sourceLocation = data.Whs1.FindLocation("A");
			var destLocation = data.Whs1.FindLocation("SERVICEROW");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, sourceLocation, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, sourceLocation, destLocation);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			sourceLocation.WLV_MaxQuantity = 5m;
			destLocation.WLV_MaxQuantity = 5m;
			Factory.Save();

			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 5m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("The Service Area must have at least one not full location with a Normal Status.\r\n", Notify.AsString);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_ServiceAreaLocationIsChosenWithLocationSort

		public void TestGetOrCreateInitialTransfer_ServiceAreaLocationIsChosenWithLocationSort_ByRowPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 3);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, null, "PLT-123");

			var serviceArea = Helper.CreateArea(data.Whs1, "SERVICE AREA");
			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2);
			row1.WR_PickPathSequence = 2;
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
			row2.WR_PickPathSequence = 1;

			foreach (var location in row1.Locations.Concat(row2.Locations))
			{
				location.WLV_WA_PickingArea = serviceArea.PK;
				location.WLV_WA_PutawayArea = serviceArea.PK;
			}

			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			var transferLine1 = initialTransfer.Lines.Single();
			AssertEquals("Destination Location should be the Next Location in Sort Sequence.", data.Whs1.FindLocation("C"), transferLine1.Location);
		}

		public void TestGetOrCreateInitialTransfer_ServiceAreaLocationIsChosenWithLocationSort_ByPutawayPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 3);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, null, "PLT-123");

			var serviceArea = Helper.CreateArea(data.Whs1, "SERVICE AREA");
			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 1, 1);
			row1.WR_PickPathSequence = 1;
			var locationInRow1 = row1.Locations.Single();
			locationInRow1.WLV_PutawayPathSequence = 2;

			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
			row2.WR_PickPathSequence = 1;
			var locationInRow2 = row2.Locations.Single();
			locationInRow2.WLV_PutawayPathSequence = 1;

			foreach (var location in row1.Locations.Concat(row2.Locations))
			{
				location.WLV_WA_PickingArea = serviceArea.PK;
				location.WLV_WA_PutawayArea = serviceArea.PK;
			}

			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			var transferLine1 = initialTransfer.Lines.Single();
			AssertEquals("Destination Location should be the Next Location in Sort Sequence.", data.Whs1.FindLocation("C"), transferLine1.Location);
		}

		public void TestGetOrCreateInitialTransfer_ServiceAreaLocationIsChosenWithLocationSort_ByRowName()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 3);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, null, "PLT-123");

			var serviceArea = Helper.CreateArea(data.Whs1, "SERVICE AREA");
			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 1, 1);
			var locationInRow1 = row1.Locations.Single();
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
			var locationInRow2 = row2.Locations.Single();

			AssertEquals("Precondition", locationInRow1.RowPathSequence, locationInRow2.RowPathSequence);
			AssertEquals("Precondition", locationInRow1.WLV_PutawayPathSequence, locationInRow2.WLV_PutawayPathSequence);

			foreach (var location in row1.Locations.Concat(row2.Locations))
			{
				location.WLV_WA_PickingArea = serviceArea.PK;
				location.WLV_WA_PutawayArea = serviceArea.PK;
			}

			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			var transferLine1 = initialTransfer.Lines.Single();
			AssertEquals("Destination Location should be the Next Location in Sort Sequence.", data.Whs1.FindLocation("B"), transferLine1.Location);
		}

		public void TestGetOrCreateInitialTransfer_ServiceAreaLocationIsChosenWithLocationSort_ByColumn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 3);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, null, "PLT-123");

			var serviceArea = Helper.CreateArea(data.Whs1, "SERVICE AREA");
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2);

			foreach (var location in row.Locations)
			{
				location.WLV_WA_PickingArea = serviceArea.PK;
				location.WLV_WA_PutawayArea = serviceArea.PK;
			}

			Factory.Save();

			// add capacity limit to ensure we skip the first location.
			var firstLocation = data.Whs1.FindLocation("B-1-1");
			firstLocation.WLV_MaxQuantity = 1m;

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			var transferLine1 = initialTransfer.Lines.Single();
			AssertEquals("Destination Location should be the Next Location in Sort Sequence.", data.Whs1.FindLocation("B-1-2"), transferLine1.Location);
		}

		public void TestGetOrCreateInitialTransfer_ServiceAreaLocationIsChosenWithLocationSort_ByLevel()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 3);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, null, "PLT-123");

			var serviceArea = Helper.CreateArea(data.Whs1, "SERVICE AREA");
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 1, 3);

			foreach (var location in row.Locations)
			{
				location.WLV_WA_PickingArea = serviceArea.PK;
				location.WLV_WA_PutawayArea = serviceArea.PK;
			}

			Factory.Save();

			var location2 = data.Whs1.FindLocation("B-1-2");
			location2.WLV_Tray = 2;

			var location3 = data.Whs1.FindLocation("B-1-3");
			location3.WLV_Tray = 1;

			Factory.Save();

			// add capacity limit to ensure we skip the first location.
			var firstLocation = data.Whs1.FindLocation("B-1-1");
			firstLocation.WLV_MaxQuantity = 1m;

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			var transferLine1 = initialTransfer.Lines.Single();
			AssertEquals("Destination Location should be the Next Location in Sort Sequence.", data.Whs1.FindLocation("B-1-2"), transferLine1.Location);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_ServiceAreaLocation

		public void TestGetOrCreateInitialTransfer_ServiceAreaLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRow(data.Whs1, "B");
			var emptyArea = Helper.CreateArea(data.Whs1, "EMPTY");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("B"), "");

			var vasOrder = Helper.CreateWhsVASOrder(emptyArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Should inform the user there are no valid locations in the Service Area.", "The Service Area must have at least one not full location with a Normal Status.\r\n", Notify.AsString);

			Notify.Clear();
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PickingArea = emptyArea.PK;
			location.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			Factory.Save();
			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Should inform the user there are no valid locations with a Normal Status.", "The Service Area must have at least one not full location with a Normal Status.\r\n", Notify.AsString);

			Notify.Clear();
			location.WLV_LocationStatus = LocationStatus.Codes.Normal;
			Factory.Save();
			AssertNotNull("Initial Transfer should be created.", vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertNull(Notify.LastEvent);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_ValidationCheckMaxWeightVolumeQuantity

		#region TestCheckMaxWeightVolume_NotEnoughSpace_Weight

		public void TestCheckMaxWeightVolume_NotEnoughSpace_Weight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = SetupEnviromentForVasOrderCheckMaxWeightVolume(data,
				maxWeightServiceArea1: 10m,
				maxWeightServiceArea2: 10m,
				receivePart1: 50m,
				vasOrderPart1: 25m);

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);

			// Total inventory Weight (25.00 KG) exceeds the maximum allowed Weight (10.00 KG) for these locations.
			AssertForVasOrderCheckMaxWeightVolume(transfer, data, expectedError: "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n");
		}

		#endregion

		#region TestCheckMaxWeightVolume_NotEnoughSpace_Volume

		public void TestCheckMaxWeightVolume_NotEnoughSpace_Volume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = SetupEnviromentForVasOrderCheckMaxWeightVolume(data,
				maxVolumeServiceArea1: 5m,
				maxVolumeServiceArea2: 5m,
				receivePart1: 50m,
				vasOrderPart1: 25m);

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);

			// Total inventory Volume (250.000 CC) exceeds the maximum allowed Volume (5.000 CC) for this locations.
			AssertForVasOrderCheckMaxWeightVolume(transfer, data, expectedError: "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n");
		}

		#endregion

		#region TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_Quantity

		public void TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_Quantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			location1.WLV_MaxQuantity = 5m;
			var location2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			location2.WLV_MaxQuantity = 5m;

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 25m);
			Factory.Save();
			var expectedError = "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n";
			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", transfer);
			AssertEquals(expectedError, Notify.AsString);

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part1, 10m);
			Factory.Save();
			var transfer2 = vasOrder2.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", transfer2);

			var vasOrder3 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine3 = Helper.CreateWhsVASOrderLine(vasOrder3, data.Part1, 5m);
			Factory.Save();
			var transfer3 = vasOrder3.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer3);
		}

		#endregion

		#region TestTransitInventory_NotEnoughCapacity

		public void TestTransitInventory_SetDestinationLocationWithEnoughCapacity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1, saveFactory_doNotUseForNewTests: false);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Factory.Save();

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destLocation = data.Whs1.FindLocation("SERVICEROW");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 35m, sourceLocation, "");
			Factory.Save();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, sourceLocation, destLocation);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 15m, transferLine.QtyCommittedIncludingMatchingLines);
			destLocation.WLV_MaxQuantity = 20m;
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 20m);
			Factory.Save();

			var expectedError = "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n";
			var newTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", newTransfer);
			AssertEquals(expectedError, Notify.AsString);
		}

		#endregion

		#region TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocation_Weight_SameProduct

		public void TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocation_Weight_SameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 1m, "M3");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			location1.WLV_MaxWeight = 5m;
			location1.WLV_MaxWeightUnit = "KG";
			var location2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			location2.WLV_MaxWeight = 1m;
			location2.WLV_MaxWeightUnit = "KG";
			Factory.Save();

			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 5m);
			Factory.Save();
			var transfer1 = vasOrder1.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer1);
			Factory.Save();

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part1, 5m);
			var expectedError = "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n";
			Factory.Save();
			var transfer2 = vasOrder2.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", transfer2);
			AssertEquals(expectedError, Notify.AsString);

			vasOrder1.Delete();
			transfer1.Delete();
			Factory.Save();
			var vasOrder3 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine3 = Helper.CreateWhsVASOrderLine(vasOrder3, data.Part1, 5m);
			Factory.Save();
			var transfer3 = vasOrder3.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer3);
		}

		public void TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocaiton_Weight_DifferentProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m);
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 1m, "M3");
			Helper.SetProductWeightAndVolume(data.Part2, 2m, "KG", 1m, "M3");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			location1.WLV_MaxWeight = 5m;
			location1.WLV_MaxWeightUnit = "KG";
			var location2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			location2.WLV_MaxWeight = 1m;
			location2.WLV_MaxWeightUnit = "KG";

			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 4m);
			Factory.Save();
			var transfer1 = vasOrder1.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer1);

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part2, 1m);
			var expectedError = "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n";
			Factory.Save();
			var transfer2 = vasOrder2.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", transfer2);
			AssertEquals(expectedError, Notify.AsString);

			vasOrder1.Delete();
			transfer1.Delete();
			Factory.Save();
			var vasOrder3 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine3 = Helper.CreateWhsVASOrderLine(vasOrder3, data.Part1, 5m);
			Factory.Save();
			var transfer3 = vasOrder3.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer3);
		}
		#endregion

		#region TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocation_Volume_SameProduct

		public void TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocation_Volume_SameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 1m, "M3");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();
			var location1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			location1.WLV_MaxCubic = 5m;
			location1.WLV_MaxCubicUnit = "M3";
			var location2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			location2.WLV_MaxCubic = 1m;
			location2.WLV_MaxCubicUnit = "M3";
			Factory.Save();

			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 5m);
			Factory.Save();
			var transfer1 = vasOrder1.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer1);
			Factory.Save();

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part1, 5m);
			var expectedError = "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n";
			Factory.Save();
			var transfer2 = vasOrder2.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", transfer2);
			AssertEquals(expectedError, Notify.AsString);

			vasOrder1.Delete();
			transfer1.Delete();
			Factory.Save();
			var vasOrder3 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine3 = Helper.CreateWhsVASOrderLine(vasOrder3, data.Part1, 5m);
			Factory.Save();
			var transfer3 = vasOrder3.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer3);
		}

		public void TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocaiton_Volume_DifferentProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m);
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 1m, "M3");
			Helper.SetProductWeightAndVolume(data.Part2, 2m, "KG", 2m, "M3");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			location1.WLV_MaxCubic = 5m;
			location1.WLV_MaxCubicUnit = "M3";
			var location2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			location2.WLV_MaxCubic = 1m;
			location2.WLV_MaxCubicUnit = "M3";

			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 4m);
			Factory.Save();
			var transfer1 = vasOrder1.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer1);

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part2, 1m);
			var expectedError = "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n";
			Factory.Save();
			var transfer2 = vasOrder2.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", transfer2);
			AssertEquals(expectedError, Notify.AsString);

			vasOrder1.Delete();
			transfer1.Delete();
			Factory.Save();
			var vasOrder3 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine3 = Helper.CreateWhsVASOrderLine(vasOrder3, data.Part1, 5m);
			Factory.Save();
			var transfer3 = vasOrder3.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer3);
		}

		#endregion

		#region TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocation_Quantity

		public void TestCheckMaxWeightVolumeQuantity_NotEnoughSpace_OtherTransferAlreadyTakingLocation_Quantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			location1.WLV_MaxQuantity = 5m;
			var location2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			location2.WLV_MaxQuantity = 1m;

			var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 5m);
			Factory.Save();
			var transfer1 = vasOrder1.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer1);

			var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder2, data.Part1, 5m);
			var expectedError = "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n";
			Factory.Save();
			var transfer2 = vasOrder2.GetOrCreateInitialTransfer(Notify);
			AssertNull("Should not create Initial Transfer.", transfer2);
			AssertEquals(expectedError, Notify.AsString);

			vasOrder1.Delete();
			transfer1.Delete();
			Factory.Save();
			var vasOrder3 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine3 = Helper.CreateWhsVASOrderLine(vasOrder3, data.Part1, 5m);
			Factory.Save();
			var transfer3 = vasOrder3.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer3);
		}

		#endregion

		#region TestCheckMaxWeightVolumeQuantity_WillOnlyFitAgainstMultipleLocations

		public void TestCheckMaxWeightVolumeQuantity_WillOnlyFitAgainstMultipleLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 4m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4");
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 2m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, part4, 2m, data.Whs1.DefaultLocation, "PLT-1");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 6, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			var location2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			var location3 = data.Whs1.FindLocation("SERVICEROW-1-3");
			var location4 = data.Whs1.FindLocation("SERVICEROW-1-4");
			var location5 = data.Whs1.FindLocation("SERVICEROW-1-5");
			var location6 = data.Whs1.FindLocation("SERVICEROW-1-6");
			location1.WLV_MaxQuantity = 8m;
			location2.WLV_MaxQuantity = 5m;
			location3.WLV_MaxQuantity = 5m;
			location4.WLV_MaxQuantity = 2m;
			location5.WLV_MaxQuantity = 2m;
			location6.WLV_MaxQuantity = 4m;

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 5m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 3m);
			Helper.CreateWhsVASOrderLine(vasOrder, part3, 2m);
			Helper.CreateWhsVASOrderLine(vasOrder, part4, 2m);
			Factory.Save();

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Should create Initial Transfer.", transfer);
			AssertEquals("Initial Transfer should have two Lines.", 4, transfer.Lines.Count);
			AssertEquals("Should be able to split the VAS Order Lines across multiple Locations.", 1, transfer.Lines.Cast<WhsTransferLine>()
				.Count(l => l.WE_WL == location1.PK && l.QtyToMoveIncludingMatchingLines == 8m));
			AssertEquals("Should be able to split the VAS Order Lines across multiple Locations.", 1, transfer.Lines.Cast<WhsTransferLine>()
				.Count(l => (l.WE_WL == location2.PK || l.WE_WL == location3.PK) && l.QtyToMoveIncludingMatchingLines == 5m));

			var locationForPallet = transfer.Lines.Cast<WhsTransferLine>().FirstOrDefault(l => l.WE_PalletID == "PLT-1").WE_WL;
			AssertEquals(true, locationForPallet == location2.PK || locationForPallet == location3.PK || locationForPallet == location6.PK);
			AssertEquals("Should be able to split the VAS Order Lines across multiple Locations.", 2, transfer.Lines.Cast<WhsTransferLine>()
				.Count(l => l.WE_WL == locationForPallet && l.QtyToMoveIncludingMatchingLines == 2m && l.WE_PalletID == "PLT-1"));
		}

		#endregion

		#region TestCheckMaxWeightVolume_NotEnoughSpace_OnlyFitsInSecondLocation

		public void TestCheckMaxWeightVolume_NotEnoughSpace_OnlyFitsInSecondLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = SetupEnviromentForVasOrderCheckMaxWeightVolume(data,
				maxWeightServiceArea1: 10m,
				maxVolumeServiceArea1: 5m,
				receivePart1: 50m,
				vasOrderPart1: 25m
				);

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			// Second location does not have a max weight or volume defined which means it has unlimited capacity.
			AssertForVasOrderCheckMaxWeightVolume(transfer, data, expectedItemsinLocation2: 25m);
		}

		#endregion

		#region TestCheckMaxWeightVolume_NotEnoughSpace_LocationHasLimitationButIsFit

		public void TestCheckMaxWeightVolume_NotEnoughSpace_LocationHasLimitationButIsFit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = SetupEnviromentForVasOrderCheckMaxWeightVolume(data,
				maxWeightServiceArea1: 10m,
				maxVolumeServiceArea1: 5m,
				maxWeightServiceArea2: 10m,
				maxVolumeServiceArea2: 5m,
				maxVolumeUQServiceArea2: "M3", // to check Convert
				receivePart1: 50m,
				receivePart2: 50m,
				vasOrderPart1: 1m,
				vasOrderPart2: 2m
				);

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			// we have stock with 5 KG Weight & 50 CC Volume,
			// location 1 can only fit 5 CC, whereas location 2 can fit 5 M3 which is enough capacity.
			AssertForVasOrderCheckMaxWeightVolume(transfer, data, expectedItemsinLocation2: 3m);
		}

		#endregion

		#region TestCheckMaxWeightVolume_P1AndP2TogetherCannotFitInOneLocation

		public void TestCheckMaxWeightVolume_P1AndP2TogetherCannotFitInOneLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = SetupEnviromentForVasOrderCheckMaxWeightVolume(data,
				maxWeightServiceArea1: 10m,
				maxVolumeServiceArea1: 100m,
				maxWeightServiceArea2: 10m,
				maxVolumeServiceArea2: 100m,
				receivePart1: 4m,
				receivePart2: 4m,
				vasOrderPart1: 4m,
				vasOrderPart2: 4m
				);

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);

			// Total inventory Weight (4 for P1 + 8 for P2 = 12.00 KG) exceeds the maximum allowed Weight (10.00 KG) for these locations,
			// so it will split the two lines across two locations.
			AssertNotNull("Should create Initial Transfer.", transfer);
			AssertEquals(2, transfer.Lines.Count);

			var expectedDestinationLocation1 = data.Whs1.FindLocation("SERVICEROW-1-1");
			AssertEquals("One Transfer Line should go into the first location.", 1,
				transfer.Lines.Cast<WhsTransferLine>().Count(t => t.WE_WL == expectedDestinationLocation1.PK && t.QtyToMoveIncludingMatchingLines == 4m));

			var expectedDestinationLocation2 = data.Whs1.FindLocation("SERVICEROW-1-2");
			AssertEquals("One Transfer Line should go into the second location.", 1,
				transfer.Lines.Cast<WhsTransferLine>().Count(t => t.WE_WL == expectedDestinationLocation2.PK && t.QtyToMoveIncludingMatchingLines == 4m));

			AssertEquals("Expect no error after creating the transfer.", "", Notify.AsString);
		}

		#endregion

		#region TestCheckMaxWeightVolume_NotEnoughSpace_OneLineIsFitButBothAreNot

		public void TestCheckMaxWeightVolume_NotEnoughSpace_OneLineIsFitButBothAreNot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = SetupEnviromentForVasOrderCheckMaxWeightVolume(data,
				maxWeightServiceArea1: 1m,
				maxWeightServiceArea2: 1m,
				receivePart1: 1m,
				receivePart2: 1m,
				vasOrderPart1: 1m,
				vasOrderPart2: 1m
				);

			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);

			// Total inventory Weight (1 for P1 + 2 for P2 = 3.00 KG) exceeds the maximum allowed Weight (1.00 KG) for these locations.
			AssertForVasOrderCheckMaxWeightVolume(transfer, data, expectedError: "Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n");
		}

		#endregion

		#region Setup / Assert

		WhsVASOrder SetupEnviromentForVasOrderCheckMaxWeightVolume(
			TestDataSimpleEnvironment data,
			decimal maxWeightServiceArea1 = 0m, string maxWeightUQServiceArea1 = "KG", decimal maxVolumeServiceArea1 = 0m, string maxVolumeUQServiceArea1 = "CC",
			decimal maxWeightServiceArea2 = 0m, string maxWeightUQServiceArea2 = "KG", decimal maxVolumeServiceArea2 = 0m, string maxVolumeUQServiceArea2 = "CC",
			decimal receivePart1 = 0m,
			decimal receivePart2 = 0m,
			decimal vasOrderPart1 = 0m,
			decimal vasOrderPart2 = 0m)
		{
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 10m, "CC");
			Helper.SetProductWeightAndVolume(data.Part2, 2m, "KG", 20m, "CC");

			if (receivePart1 > 0)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, receivePart1);
			}
			if (receivePart2 > 0)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, receivePart2);
			}

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, 1, 2, 1);
			Factory.Save();

			Helper.SetLocationMaxWeightAndVolume(data.Whs1.FindLocation("SERVICEROW-1-1"), maxWeightServiceArea1, maxWeightUQServiceArea1, maxVolumeServiceArea1, maxVolumeUQServiceArea1);
			Helper.SetLocationMaxWeightAndVolume(data.Whs1.FindLocation("SERVICEROW-1-2"), maxWeightServiceArea2, maxWeightUQServiceArea2, maxVolumeServiceArea2, maxVolumeUQServiceArea2);

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			if (vasOrderPart1 > 0)
			{
				Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, vasOrderPart1);
			}
			if (vasOrderPart2 > 0)
			{
				Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, vasOrderPart2);
			}

			Factory.Save();

			return vasOrder;
		}

		void AssertForVasOrderCheckMaxWeightVolume(WhsTransfer initialTransfer, TestDataSimpleEnvironment data, decimal expectedItemsinLocation1 = 0m, decimal expectedItemsinLocation2 = 0m, string expectedError = "")
		{
			if (!string.IsNullOrEmpty(expectedError))
			{
				AssertNull("Should not create Initial Transfer.", initialTransfer);
				AssertEquals(expectedError, Notify.AsString);
			}
			else
			{
				AssertNotNull("Should create Initial Transfer.", initialTransfer);

				if (expectedItemsinLocation1 > 0)
				{
					var expectedDestinationLocation = data.Whs1.FindLocation("SERVICEROW-1-1");
					AssertEquals("Each transfer line should have the same destination location.",
						initialTransfer.Lines.Count, initialTransfer.Lines.Cast<WhsTransferLine>().Count(t => t.WE_WL == expectedDestinationLocation.PK));

					var transferLinesForLocation1 = initialTransfer.Lines.Cast<WhsTransferLine>().Where(t => t.WE_WL == expectedDestinationLocation.PK);
					AssertEquals("Expected to put item in location 1 in service area", expectedItemsinLocation1, transferLinesForLocation1.Sum(l => l.QtyToMoveIncludingMatchingLines));
				}

				if (expectedItemsinLocation2 > 0)
				{
					var expectedDestinationLocation = data.Whs1.FindLocation("SERVICEROW-1-2");
					AssertEquals("Each transfer line should have the same destination location.",
						initialTransfer.Lines.Count, initialTransfer.Lines.Cast<WhsTransferLine>().Count(t => t.WE_WL == expectedDestinationLocation.PK));

					var transferLinesForLocation2 = initialTransfer.Lines.Cast<WhsTransferLine>().Where(t => t.WE_WL == expectedDestinationLocation.PK);
					AssertEquals("Expected to put item in location 2 in service area", expectedItemsinLocation2, transferLinesForLocation2.Sum(l => l.QtyToMoveIncludingMatchingLines));
				}

				AssertEquals("Expect no error after creating the transfer.", "", Notify.AsString);
			}
		}

		#endregion

		#endregion

		#region TestGetOrCreateInitialTransfer_WithAttributes

		public void TestGetOrCreateInitialTransfer_WithAttributes()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			receiveLine1.WI_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			receiveLine2.WI_SerialNumber = "SN3";
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m, new ZDate(year, 1, 2), new ZDate(year, 1, 1));
			vasOrderLine1.WVL_PartAttrib1 = "PA1";
			vasOrderLine1.WVL_PartAttrib2 = "PA2";
			vasOrderLine1.WVL_PartAttrib3 = "PA3";
			vasOrderLine1.WVL_SerialNumber = "SN1";
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m, new ZDate(year, 1, 1), new ZDate(year, 1, 2));
			vasOrderLine2.WVL_PartAttrib1 = "SA1";
			vasOrderLine2.WVL_PartAttrib2 = "SA2";
			vasOrderLine2.WVL_PartAttrib3 = "SA3";
			vasOrderLine2.WVL_SerialNumber = "SN2";
			Factory.Save();

			AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals(string.Format(@"Cannot create Transfer because of the following Shortfalls:
1x P1 {{ Part Attrib. 1 - SA1, Part Attrib. 2 - SA2, Part Attrib. 3 - SA3, Serial Number - SN2, Expiry Date - {0}, Packing Date - {1} }}
", new ZDate(year, 1, 2).ToShortDateString(), new ZDate(year, 1, 1).ToShortDateString()), Notify.AsString);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, data.Whs1.DefaultLocation, new ZDate(year, 1, 2), new ZDate(year, 1, 1), "SA1", "SA2", "SA3", "");
			receiveLine3.WI_SerialNumber = "SN2";
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			Notify.Clear();
			var transfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull(transfer);
			AssertEquals("Transfer should have no problem being created.", "", Notify.AsString);
			AssertEquals("Transfer should have two Lines.", 2, transfer.Lines.Count);
			AssertEquals("Should have populated attributes correctly.", 1, transfer.Lines.Count(l =>
				l.WE_PartAttrib1 == "PA1" &&
				l.WE_PartAttrib2 == "PA2" &&
				l.WE_PartAttrib3 == "PA3" &&
				l.WE_SerialNumber == "SN1" &&
				l.WE_ExpiryDate == new ZDateTime(year, 1, 1) &&
				l.WE_PackingDate == new ZDateTime(year, 1, 2)));
			AssertEquals("Should have populated attributes correctly.", 1, transfer.Lines.Count(l =>
				l.WE_PartAttrib1 == "SA1" &&
				l.WE_PartAttrib2 == "SA2" &&
				l.WE_PartAttrib3 == "SA3" &&
				l.WE_SerialNumber == "SN2" &&
				l.WE_ExpiryDate == new ZDateTime(year, 1, 2) &&
				l.WE_PackingDate == new ZDateTime(year, 1, 1)));

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Transfer should have been created in a finalisable state.", true, transfer.IsFinalised);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_ValidationErrorOnTransfer

		public void TestGetOrCreateInitialTransfer_ValidationErrorOnTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			using (vasOrder.CreateErrorOnTransferAfterValidationForTest())
			{
				Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Factory.Save();

				AssertNull(vasOrder.GetOrCreateInitialTransfer(Notify));
				var transferQuery = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
				AssertEquals("Transfer should be deleted if Validation fails.", 0, Factory.Load<WhsTransfer>(transferQuery).Length);
				AssertEquals("Into Service Area Transfer could not be created:\r\nError - Warehouse Transfer: Test Error\r\n", Notify.AsString);
			}
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_DBHits

		public void TestGetOrCreateInitialTransfer_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receiveLocation = data.Whs1.DefaultLocation;
			short numLocations = 20;
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, numLocations, rowCode: "S");
			foreach (var location in serviceArea.PickLocations.Cast<WhsLocation>())
			{
				location.WLV_MaxQuantity = 1; // each location can fit 1 unit
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (int i = 0; i < numLocations; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, receiveLocation, ZDate.Empty, ZDate.Empty, "VIN-" + i, "", "", "");
			}
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// now let's fill all service area locations but one
			for (int i = 0; i < numLocations - 1; i++)
			{
				var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 1);
				vasOrder.Lines.Single().WVL_PartAttrib1 = "VIN-" + i;
				Factory.Save();
				var transferIn = vasOrder.GetOrCreateInitialTransfer(Notify);
				AssertNotNull("Should create transfer in for VAS order #" + (i + 1), transferIn);
				if (i % 2 == 0)
				{
					// let's finalise every second trasfer (therefore half will create pending stock, the other half, finalised stock)
					transferIn.FinaliseDocketWithoutUserConfirmation();
				}
			}

			var theLastVASOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 1);
			theLastVASOrder.Lines.Single().WVL_PartAttrib1 = "VIN-" + (numLocations - 1);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			otherFactory.RefreshEnabled = false;
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(theLastVASOrder.PK);
			WhsTransfer transferInForLastVasOrder;

			using (RowFactory.SetCachedTables())
			{
				transferInForLastVasOrder = vasOrderInOtherFactory.GetOrCreateInitialTransfer(Notify);
			}

			AssertNotNull(transferInForLastVasOrder);
			AssertEquals("Should allocate trasfer to the last location as others are not available", "S-20", transferInForLastVasOrder.Lines.Single().LocationString);

			// when transfer in for the last vas order gets created - it should not loop through full locations
			// to evaluate stock / transactions for those locations
			var expectedDBHitsForInitialTransfer = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsVASOrderLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDBHitsForInitialTransfer, otherFactory);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_DBHits_LocationsCheckedOnce

		public void TestGetOrCreateInitialTransfer_DBHits_LocationsCheckedOnce()
		{
			const short numLocations = 20;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = "KG";

			var receiveLocation = data.Whs1.DefaultLocation;
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1, numLocations, rowCode: "S");
			foreach (var location in serviceArea.PickLocations.Cast<WhsLocation>())
			{
				location.WLV_MaxWeight = 1;
				location.WLV_MaxWeightUnit = "KG";
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			for (int i = 0; i < numLocations; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, receiveLocation);
			}
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			for (int i = 0; i < numLocations; i++)
			{
				Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1);
			}
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var vasOrderInOtherFactory = otherFactory.Load<WhsVASOrder>(vasOrder.PK);
			WhsTransfer transferInForLastVasOrder;

			using (RowFactory.SetCachedTables())
			{
				transferInForLastVasOrder = vasOrderInOtherFactory.GetOrCreateInitialTransfer(Notify);
			}

			AssertNull("Transfer will not be created as it will not fit anywhere. But we are interested in amount of DB Hits required to figure it out.", transferInForLastVasOrder);
			AssertEquals("Cannot find location in Service Area SERVICE AREA with enough capacity for the transfer.\r\n", Notify.AsString);

			var expectedDBHitsForInitialTransfer = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessJobTriggerLinkSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 2 },
				{ WhsDocketJobPivotSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsVASOrderLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 }, // Transfer-In Dest location is now sorted by Row/Col/Lvl/Tray Client setup, need access to WhsRow for this.
				// these are important
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expectedDBHitsForInitialTransfer, otherFactory);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_IgnoresLocationsWithWrongStatus

		public void TestGetOrCreateInitialTransfer_IgnoresLocationsWithWrongStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receiveLocation = data.Whs1.DefaultLocation;
			var serviceAreaS = Helper.CreateServiceAreaForVASOrder(data.Whs1, areaName: "Area13", rowCode: "S");
			var location = serviceAreaS.PickLocations[0];

			// another service area to check that its locations will NOT be used when we create VAS Order for service area "serviceAreaS"
			var serviceAreaX = Helper.CreateServiceAreaForVASOrder(data.Whs1, rowCode: "X");
			AssertEquals("Precondition - another service area has normal location.", LocationStatus.Codes.Normal, serviceAreaX.PickLocations[0].WLV_LocationStatus);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2, receiveLocation, "");
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceAreaS, data.Org1, data.Part1, 1);
			foreach (CodeDescriptionPair locationStatusPair in new LocationStatus())
			{
				if (locationStatusPair.Code != LocationStatus.Codes.Normal)
				{
					location.WLV_LocationStatus = locationStatusPair.Code;
					Factory.Save();
					AssertNull("Absence of Normal locations should prevent creation of transfer in.", vasOrder.GetOrCreateInitialTransfer(Notify));
					AssertEquals("Should inform the user there are no valid locations in the Service Area.", "The Service Area must have at least one not full location with a Normal Status.\r\n", Notify.AsString);
					Notify.Clear();
				}
			}

			location.WLV_LocationStatus = LocationStatus.Codes.Normal;
			Factory.Save();
			AssertNotNull("As there is now location with normal status in required service area - trasfer in should be created.", vasOrder.GetOrCreateInitialTransfer(Notify));

			location.WLV_MaxQuantity = 1;
			Factory.Save();
			var secondVasOrder = Helper.CreateWhsVASOrderWithLine(serviceAreaS, data.Org1, data.Part1, 1);
			Factory.Save();
			AssertNull("The only valid location in target service area is full now - no trasfer in should be created.", secondVasOrder.GetOrCreateInitialTransfer(Notify));
			AssertEquals("Should inform the user there are no valid locations in the Service Area.", "The Service Area must have at least one not full location with a Normal Status.\r\n", Notify.AsString);
		}

		#endregion

		#region TestGetOrCreateInitialTransfer_UsesMutexToCheckLocationsCapacity

		public void TestGetOrCreateInitialTransfer_UsesMutexToCheckLocationsCapacity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receiveLocation = data.Whs1.DefaultLocation;
			var serviceArea1 = Helper.CreateServiceAreaForVASOrder(data.Whs1, areaName: "Area13", rowCode: "S");
			var serviceArea2 = Helper.CreateServiceAreaForVASOrder(data.Whs1, areaName: "Area131", rowCode: "S2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10, receiveLocation, "");
			Factory.Save();

			var vasOrder1 = Helper.CreateWhsVASOrderWithLine(serviceArea1, data.Org1, data.Part1, 1);
			Factory.Save();

			using (var mutex = new ZGlobalMutex(MutexIDs.WhsVasOrderCheckingLocationCapacity, serviceArea1.PK.ToString()))
			{
				mutex.Lock();
				AssertNull("Mutex for service area should prevent creation of transfer in.", vasOrder1.GetOrCreateInitialTransfer(Notify));
				AssertEquals("Should inform the user there are other users making transfer to same location.", "Another VAS order is currently creating a transfer in for the same service area. To avoid capacity overflow please try again in short time.\r\n", Notify.AsString);
				mutex.Unlock();
				AssertNotNull("If there is no lock on mutex - transfer should be created.", vasOrder1.GetOrCreateInitialTransfer(Notify));
			}

			var vasOrder2 = Helper.CreateWhsVASOrderWithLine(serviceArea2, data.Org1, data.Part1, 1);
			Factory.Save();

			using (var mutex = new ZGlobalMutex(MutexIDs.WhsVasOrderCheckingLocationCapacity, serviceArea1.PK.ToString()))
			{
				mutex.Lock();
				AssertNotNull("Mutex is locking another service area, so transfer should be created.", vasOrder2.GetOrCreateInitialTransfer(Notify));
				mutex.Unlock();
			}
		}

		#endregion

		#region TestGetOrCreateReturnTransfer

		public void TestGetOrCreateReturnTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.FindLocation("A-1").WLV_LocationStatus = LocationStatus.Codes.Held; // create a held location
			var inventoryLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, inventoryLocation, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m, inventoryLocation, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 6m, inventoryLocation, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 6m, inventoryLocation, "PLT-2");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);

			AssertNull("Should not create return Transfer if there are changes.", vasOrder.GetOrCreateReturnTransfer(Notify));
			AssertEquals("Should not create return Transfer if there are changes.", ZGuid.Empty, vasOrder.WVO_WD_TransferOutOfServiceArea);
			AssertEquals("Should not create return Transfer if there are changes.", "Save all changes before creating the Transfer out of the Service Area.\r\n", Notify.AsString);

			Notify.Clear();
			Factory.Save();
			AssertNull("Should not create return Transfer if Initial Transfer does not exist.", vasOrder.GetOrCreateReturnTransfer(Notify));
			AssertEquals("Should not create return Transfer if Initial Transfer does not exist.", ZGuid.Empty, vasOrder.WVO_WD_TransferOutOfServiceArea);
			AssertEquals("Should not create return Transfer if Initial Transfer does not exist.", "Cannot create Transfer out of Service Area as the VAS Order has not yet been Completed.\r\n", Notify.AsString);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 2, initialTransfer.Lines.Count);

			Notify.Clear();
			Factory.Save();
			AssertNull("Should not create return Transfer if Initial Transfer is not Finalised.", vasOrder.GetOrCreateReturnTransfer(Notify));
			AssertEquals("Should not create return Transfer if Initial Transfer is not Finalised.", ZGuid.Empty, vasOrder.WVO_WD_TransferOutOfServiceArea);
			AssertEquals("Should not create return Transfer if Initial Transfer is not Finalised.", "Cannot create Transfer out of Service Area as the VAS Order has not yet been Completed.\r\n", Notify.AsString);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			Notify.Clear();
			AssertNull("Should not create return Transfer if VAS Order not marked as completed.", vasOrder.GetOrCreateReturnTransfer(Notify));
			AssertEquals("Should not create return Transfer if VAS Order not marked as completed.", ZGuid.Empty, vasOrder.WVO_WD_TransferOutOfServiceArea);
			AssertEquals("Should not create return Transfer if VAS Order not marked as completed.", "Cannot create Transfer out of Service Area as the VAS Order has not yet been Completed.\r\n", Notify.AsString);

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			Notify.Clear();
			using (vasOrder.SuspendSettingHasChanges())
			{
				vasOrder.WVO_CancelledTimeUtc = ZDateTime.UtcNow;
				AssertNull("Should not create return Transfer if vasOrder is cancelled.", vasOrder.GetOrCreateReturnTransfer(Notify));
				AssertEquals("Cannot create Transfer for Canceled VAS Orders.\r\n", Notify.AsString);

				vasOrder.WVO_CancelledTimeUtc = ZDateTime.Empty;
			}
			Notify.Clear();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNotNull("Should be able to create return transfer.", returnTransfer);
				AssertNull("No Error Message should have been made.", Notify.LastEvent);
			}

			Factory.Save();
			AssertEquals("Out of Service Area Transfer should have the same client as VAS Order.", data.Org1.PK, returnTransfer.WD_OH_Client);
			AssertEquals("Out of Service Area Transfer should have the same warehouse as VAS Order.", data.Whs1.PK, returnTransfer.WD_WW_Whs);
			AssertEquals("Out of Service Area Transfer external reference should contain VAS Order Number.", "WV00000001 W00000006", returnTransfer.WD_ExternalReference);
			AssertEquals("On an out of Service Area Transfer, a Transfer line should be created for each Transfer line on the Initial Transfer.", 2, returnTransfer.Lines.Count);

			var initialTransferLine1 = initialTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransferFromPalletId == "PLT-1");
			var initialMatchingTransferLine = (WhsTransferLine)initialTransferLine1.MatchingLines.Single();
			var initialTransferLine2 = initialTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_TransferFromPalletId == "PLT-2");
			var inventoryFromInitialTransfer1 = initialTransferLine1.Inventory[0];
			var inventoryFromInitialTransfer2 = initialTransferLine2.Inventory[0];
			var inventoryFromInitialMatchingTranferLine = initialMatchingTransferLine.Inventory[0];
			var returnTransferLine1 = returnTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.PickLines.Single().WZ_WE_InventoryLine == inventoryFromInitialTransfer1.WI_WE_InDocketLine);
			var returnTransferLine2 = returnTransfer.Lines.Cast<WhsTransferLine>().Single(l => l.PickLines.Single().WZ_WE_InventoryLine == inventoryFromInitialTransfer2.WI_WE_InDocketLine);
			var returnMatchingTransferLine = (WhsTransferLine)returnTransferLine1.MatchingLines.Single();
			var matchingLinePickLine = returnMatchingTransferLine.PickLines.Single();
			AssertEquals("Should commit Inventory created by Initial Transfer.", inventoryFromInitialMatchingTranferLine.WI_WE_InDocketLine, matchingLinePickLine.WZ_WE_InventoryLine);
			AssertEquals("Should only create matching lines if Initial Transfer Line had matching lines.", 0, returnTransferLine2.MatchingLines.Count);
			// inventory line
			AssertEquals("Inventory Line should match appropriate Inventory Docket Line.", inventoryFromInitialTransfer1.WI_WE_InDocketLine, returnTransferLine1.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("Inventory Line should match appropriate Inventory Docket Line.", inventoryFromInitialTransfer2.WI_WE_InDocketLine, returnTransferLine2.PickLines.Single().WZ_WE_InventoryLine);
			AssertEquals("Inventory Line should match appropriate Inventory Docket Line.", inventoryFromInitialMatchingTranferLine.WI_WE_InDocketLine, matchingLinePickLine.WZ_WE_InventoryLine);
			// inventory status
			AssertEquals("Transfer Line Status should be correct.", InventoryStatus.Codes.Available, returnTransferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Transfer Line Status should be correct.", InventoryStatus.Codes.Available, returnTransferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Transfer Line Status should be correct.", InventoryStatus.Codes.Available, returnMatchingTransferLine.WE_CurrentInventoryStatus);
			AssertEquals("Transfer Line Status should be correct.", InventoryStatus.Codes.Available, returnTransferLine1.WE_OriginalInventoryStatus);
			AssertEquals("Transfer Line Status should be correct.", InventoryStatus.Codes.Available, returnTransferLine2.WE_OriginalInventoryStatus);
			AssertEquals("Transfer Line Status should be correct.", InventoryStatus.Codes.Available, returnMatchingTransferLine.WE_OriginalInventoryStatus);
			// product
			AssertEquals("Product should be correct.", data.Part1.PK, returnTransferLine1.WE_OP);
			AssertEquals("Product should be correct.", data.Part1.PK, returnTransferLine2.WE_OP);
			AssertEquals("Product should be correct.", data.Part1.PK, returnMatchingTransferLine.WE_OP);
			// units
			AssertEquals("Should transfer correct Units.", 4m, returnTransferLine1.QtyToMoveIncludingMatchingLines);
			AssertEquals("Should transfer correct Units.", 6m, returnTransferLine2.QtyToMoveIncludingMatchingLines);
			AssertEquals("Should transfer correct Units.", 2m, returnTransferLine1.WE_TransactionQuantity);
			AssertEquals("Should transfer correct Units.", 2m, returnMatchingTransferLine.WE_TransactionQuantity);
			// pallet ID
			AssertEquals("Should transfer whole Pallet if it was a whole Pallet to begin with.", "PLT-1", returnTransferLine1.WE_TransferFromPalletId);
			AssertEquals("Should transfer whole Pallet if it was a whole Pallet to begin with.", "PLT-1", returnMatchingTransferLine.WE_TransferFromPalletId);
			AssertEquals("If Pallet ID was dropped, it should remain dropped.", "", returnTransferLine2.WE_TransferFromPalletId);
			AssertEquals("Should transfer whole Pallet if it was a whole Pallet to begin with.", "PLT-1", returnTransferLine1.WE_PalletID);
			AssertEquals("Should transfer whole Pallet if it was a whole Pallet to begin with.", "PLT-1", returnMatchingTransferLine.WE_PalletID);
			AssertEquals("If Pallet ID was dropped, it should remain dropped.", "", returnTransferLine2.WE_PalletID);
			// location
			AssertEquals("Should be transferring from the Service Area Location.", initialTransferLine1.WE_WL, returnTransferLine1.WE_WL_TransferFrom);
			AssertEquals("Should be transferring from the Service Area Location.", initialTransferLine2.WE_WL, returnTransferLine2.WE_WL_TransferFrom);
			AssertEquals("Should be transferring from the Service Area Location.", initialTransferLine1.WE_WL, returnMatchingTransferLine.WE_WL_TransferFrom);
			AssertEquals("Putaway location is mocked so they all go to the same location. Putaway location logic is now in PutawayEngineManager.", data.Whs1.FindLocation("A-2").PK, returnTransferLine1.WE_WL);
			AssertEquals("Putaway location is mocked so they all go to the same location. Putaway location logic is now in PutawayEngineManager.", data.Whs1.FindLocation("A-2").PK, returnTransferLine2.WE_WL);
			AssertEquals("Putaway location is mocked so they all go to the same location. Putaway location logic is now in PutawayEngineManager.", returnTransferLine1.WE_WL, returnMatchingTransferLine.WE_WL);

			Notify.Clear();
			AssertEquals("If Out of Service Area transfer has already been created, it should be returned.", returnTransfer, vasOrder.GetOrCreateReturnTransfer(Notify));

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Return transfer should be created in a finalisable state.", true, returnTransfer.IsFinalised);
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_WhenThereAreIncompleteServices

		public void TestGetOrCreateReturnTransfer_WhenThereAreIncompleteServices()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A"), "");
			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();
			Notify.Clear();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			var service1 = Factory.New<WhsJobService>();
			service1.ES_ParentID = vasOrder.PK;
			service1.ES_ParentTableCode = vasOrder.TablePrefix;
			var service2 = Factory.New<WhsJobService>();
			service2.ES_ParentID = vasOrder.PK;
			service2.ES_ParentTableCode = vasOrder.TablePrefix;
			AssertEquals("Collection was not loaded and/or the relationship filter is incorrect.", 2, vasOrder.Services.Count);

			Factory.Save();
			Notify.Clear();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNull("Should no be able to create return transfer.", returnTransfer);
				AssertEquals("Should not create Transfer Out if there are incomplete services.", "Cannot create Transfer out of Service Area until all Services have been Completed.\r\n", Notify.AsString);

				Notify.Clear();
				service1.ES_Completed = ZDateTime.Today;
				Factory.Save();
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNull("Should no be able to create return transfer.", returnTransfer);
				AssertEquals("Should not create Transfer Out if there are incomplete services.", "Cannot create Transfer out of Service Area until all Services have been Completed.\r\n", Notify.AsString);

				Notify.Clear();
				service2.ES_Completed = ZDateTime.Today;
				Factory.Save();
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNotNull("Should be able to create return transfer.", returnTransfer);
			}
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_WithPutawayEngine

		public void TestGetOrCreateReturnTransfer_WithPutawayEngine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2m, location2, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 6m, location2, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 6m, location2, "PLT-2");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 2, initialTransfer.Lines.Count);
			Factory.Save();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			WhsTransfer returnTransfer;

			var putawayEngineMock = new Mock<IPutawayEngineManagerForVASTransferLine>(MockBehavior.Strict);
			putawayEngineMock.
				Setup(pe => pe.Putaway(It.Is<IEnumerable<WhsVASOrder>>(o => o.Contains(vasOrder)), It.IsAny<IEnumerable<VASReturnTransferLine>>(), It.IsNotNull<INotifications>(), null))
				.Verifiable();
			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNotNull("Should be able to create return transfer.", returnTransfer);
				AssertNull("No Error Message should have been made.", Notify.LastEvent);
			}

			var linesToPutaway = VASReturnTransferLine.GetLinesToPutawayFromTransfer(returnTransfer);

			putawayEngineMock.
				Verify(pe =>
					pe.Putaway(It.Is<IEnumerable<WhsVASOrder>>(o => o.Contains(vasOrder)), It.Is<IEnumerable<VASReturnTransferLine>>(lines => !linesToPutaway.Except(lines).Any()), It.IsNotNull<INotifications>(), null));
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_WhenVASOrderIsFinalised

		public void TestGetOrCreateReturnTransfer_WhenVASOrderIsFinalised()
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
			vasOrder.FinaliseVASOrder(Notify);
			AssertEquals("Precondition: VAS Order is finalised.", true, vasOrder.IsFinalised);

			Factory.Save();
			AssertNull("Should not create Return Transfer if VAS Order already finalised.", vasOrder.GetOrCreateReturnTransfer(Notify));
			AssertEquals("Should not create Return Transfer if VAS Order already finalised.",
				"Cannot create Transfer out of Service Area as the VAS Order has been Finalized.\r\n", Notify.AsString);
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_WithAttributes

		public void TestGetOrCreateReturnTransfer_WithAttributes()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, new ZDate(year, 1, 1), new ZDate(year, 1, 2), "PA1", "PA2", "PA3", "");
			inv.WI_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m, new ZDate(year, 1, 2), new ZDate(year, 1, 1));
			vasOrderLine.WVL_PartAttrib1 = "PA1";
			vasOrderLine.WVL_PartAttrib2 = "PA2";
			vasOrderLine.WVL_PartAttrib3 = "PA3";
			vasOrderLine.WVL_SerialNumber = "SN1";
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			var putawayEngineMock = new Mock<IPutawayEngineManagerForVASTransferLine>(MockBehavior.Strict);
			putawayEngineMock.Setup(putawayEngine => putawayEngine.Putaway(It.Is<IEnumerable<WhsVASOrder>>(o => o.Contains(vasOrder)), It.IsAny<IEnumerable<VASReturnTransferLine>>(), It.IsNotNull<INotifications>(), null))
				.Callback((Action<IEnumerable<WhsVASOrder>, IEnumerable<VASReturnTransferLine>, INotifications, RefEquipment>)((vasOrderParam, vasReturnTransferLines, iNotifications, refEquipment) =>
				{
					foreach (var vasTransferLine in vasReturnTransferLines)
					{
						vasTransferLine.TransferLine.WE_WL = data.Whs1.DefaultLocation.PK;
					}
				}));

			WhsTransfer returnTransfer;
			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			AssertNotNull("Should create return transfer.", returnTransfer);
			AssertEquals("Transfer should have no problem being created.", "", Notify.AsString);
			AssertEquals("Transfer should have one Line.", 1, returnTransfer.Lines.Count);
			AssertEquals("Should have populated attributes correctly.", 1, returnTransfer.Lines.Count(l =>
				l.WE_PartAttrib1 == "PA1" &&
				l.WE_PartAttrib2 == "PA2" &&
				l.WE_PartAttrib3 == "PA3" &&
				l.WE_SerialNumber == "SN1" &&
				l.WE_ExpiryDate == new ZDateTime(year, 1, 1) &&
				l.WE_PackingDate == new ZDateTime(year, 1, 2)));

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Return transfer should be created in a finalisable state.", true, returnTransfer.IsFinalised);
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_MandatoryCustomAttributes

		public void TestGetOrCreateReturnTransfer_MandatoryCustomAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var customAttrib1 = data.Org1.CustomLabels.AddNew();
			customAttrib1.OT_FieldName = Core.Constants.CustomLabels.WhsDocket.CustomAttribute1;
			customAttrib1.OT_IsMandatory = true;

			Helper.CreateRow(data.Whs1, "B");
			var emptyArea = Helper.CreateArea(data.Whs1, "EMPTY");
			var location = data.Whs1.FindLocation("A");
			location.WLV_WA_PickingArea = emptyArea.PK;
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("B");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, inventoryLocation, "", finalise: false);
			receive.WD_CustomAttrib1 = "TEST";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var vasOrder = Helper.CreateWhsVASOrder(emptyArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var putawayEngineMock = new Mock<IPutawayEngineManagerForVASTransferLine>(MockBehavior.Strict);
			putawayEngineMock.Setup(putawayEngine => putawayEngine.Putaway(It.Is<IEnumerable<WhsVASOrder>>(o => o.Contains(vasOrder)), It.IsAny<IEnumerable<VASReturnTransferLine>>(), It.IsNotNull<INotifications>(), null))
				.Callback((Action<IEnumerable<WhsVASOrder>, IEnumerable<VASReturnTransferLine>, INotifications, RefEquipment>)((vasOrderParam, vasReturnTransferLines, iNotifications, refEquipment) =>
				{
					foreach (var vasTransferLine in vasReturnTransferLines)
					{
						vasTransferLine.TransferLine.WE_WL = inventoryLocation.PK;
					}
				}));

			WhsTransfer returnTransfer;
			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			AssertNotNull("Return Transfer should be created.", returnTransfer);
			AssertEquals("Should have no errors.", false, returnTransfer.HasErrors);
			AssertEquals("Should have triggered has changes.", true, vasOrder.HasChanges);
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_WithPutawayErrors

		public void TestGetOrCreateReturnTransfer_WithPutawayErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();
			data.Whs1.DefaultLocation.PickingArea.WA_AreaType = AreaTypes.Codes.Bonded; // make area bonded to have no locations to putaway to.

			var putawayEngineMock = new Mock<IPutawayEngineManagerForVASTransferLine>(MockBehavior.Strict);
			putawayEngineMock.Setup(putawayEngine => putawayEngine.Putaway(It.Is<IEnumerable<WhsVASOrder>>(o => o.Contains(vasOrder)), It.IsAny<IEnumerable<VASReturnTransferLine>>(), It.IsNotNull<INotifications>(), null))
				.Callback((Action<IEnumerable<WhsVASOrder>, IEnumerable<VASReturnTransferLine>, INotifications, RefEquipment>)((vasOrderParam, vasReturnTransferLines, iNotifications, refEquipment) =>
				{
					iNotifications.Notify(new ErrorNotification(VASOrderErrorTypes.NoLocationsDefined));
				}));

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			using (vasOrder.CreateErrorOnTransferAfterValidationForTest())
			{
				AssertNull("Should not have created the Return Transfer.", vasOrder.GetOrCreateReturnTransfer(Notify));
				AssertEquals("Should not show Putaway Error if there is a problem creating the Transfer.",
					"Out of Service Area Transfer could not be created:\r\nError - Warehouse Transfer: Test Error\r\n", Notify.AsString);
				AssertEquals("Should have cleared TransferOut FK.", true, vasOrder.WVO_WD_TransferOutOfServiceArea.IsEmpty);
				AssertEquals("Should *not* have triggered has changes.", false, vasOrder.HasChanges);
			}

			bool notificationsWereEmptyBeforeSettingTransferOut = false;
			vasOrder.WVO_WD_TransferOutOfServiceAreaInfo.ValueChanged += delegate
			{
				notificationsWereEmptyBeforeSettingTransferOut = Notify.LastEvent == null;
			};

			Notify.Clear();

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				AssertNotNull("Should have successfully created the Return Transfer.", vasOrder.GetOrCreateReturnTransfer(Notify));
				AssertEquals("Should have Putaway Error.", @"Error: There are no locations defined for this warehouse, or there are no locations with a suitable status to transfer to.
There needs to be at least one location in the warehouse that does NOT have a status of Held, Damaged or Void and the Area Types must match.

The Transfer Out will need to have its Locations manually entered.
", Notify.AsString);
				AssertEquals("Should have only added the notification after setting the Out of Service Area Transfer.", true, notificationsWereEmptyBeforeSettingTransferOut);
			}
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_ValidationErrorOnTransfer

		public void TestGetOrCreateReturnTransfer_ValidationErrorOnTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var inventoryLocationPK = receive.Lines.Single().WE_WL;
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition: Successfully created Initial Transfer.", initialTransfer);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();

			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			using (vasOrder.CreateErrorOnTransferAfterValidationForTest())
			{
				AssertNull("Should not create return transfer.", vasOrder.GetOrCreateReturnTransfer(Notify));
				AssertNull("Should not set the Out of Service Area Transfer if Validation fails.", vasOrder.TransferOutOfServiceArea);

				var transferQuery = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
				transferQuery.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, initialTransfer.PK);
				AssertEquals("Transfer should be deleted if Validation fails.", 0, Factory.Load<WhsTransfer>(transferQuery).Length);
				AssertEquals("Out of Service Area Transfer could not be created:\r\nError - Warehouse Transfer: Test Error\r\n", Notify.AsString);
			}
		}

		#endregion

		#region TestGetOrCreateReturnTransfer_CreateIntialTransferInOneFactory_WhileFinaliseItInAnotherFactory_AndNeedToSplitLines

		[GuiTest]
		public void TestGetOrCreateReturnTransfer_CreateIntialTransferInOneFactory_WhileFinaliseItInAnotherFactory_AndNeedToSplitLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location4 = data.Whs1.FindLocation("A-4");
			var location5 = data.Whs1.FindLocation("A-5");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);
			Factory.Save();
			// Visit the Inventory Property before finalising, it is important for this test to simulate the real scenario.
			AssertEquals("Precondition: WhsInventoryView can't be loaded as transfer line is not finalised yet.", 0, initialTransfer.Lines[0].Inventory.Count);

			// In real world, CW1 opens Edit Transfer Screen as a dialog with a new Factory.
			var newFactoryCreatedByZController = new BusinessObjectFactory();
			var transferInAnotherFactory = newFactoryCreatedByZController.Load<WhsTransfer>(initialTransfer.PK);
			transferInAnotherFactory.FinaliseDocketWithoutUserConfirmation();
			newFactoryCreatedByZController.Save();
			AssertIsFinalisedPrecondition(transferInAnotherFactory);
			// Updated by data refresh
			AssertIsFinalisedPrecondition(initialTransfer);
			AssertEquals("Precondition: Not updated due to the query cache of Factory.", 0, initialTransfer.Lines[0].Inventory.Count);

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is marked as completed.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			WhsTransfer returnTransfer;

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>();
			engineMock.Setup(em =>
					em.RunRulesEngine(
						Factory,
						It.IsNotNull<INotifications>(),
						RulesContextType.InventoryPutaway,
						It.IsAny<ProductionRuleSetFilter>(),
						It.IsAny<Func<IEnumerable<IInputFact>>>(),
						It.IsAny<Func<ProductionRulesEngineResult, INotification>>()))
				.Callback<BusinessObjectFactory, INotifications, RulesContextType, ProductionRuleSetFilter,
					Func<IEnumerable<IInputFact>>, Func<ProductionRulesEngineResult, INotification>>(
					(f, n, rct, filters, getFacts, processResults) =>
					{
						var returnTransferQuery = new ZQuery(WhsDocketSchema.WD_ExternalReference, vasOrder.WVO_JobID);
						var transfer = Factory.LoadTop1<WhsTransfer>(returnTransferQuery);
						var line = transfer.Lines.Single();
						var outputFacts = new IFact[]
						{
							new PutawayResultFact(line.PK.ToGuid(), location4.PK.ToGuid(), 5m, ""),
							new PutawayResultFact(line.PK.ToGuid(), location5.PK.ToGuid(), 5m, "")
						};

						processResults(new ProductionRulesEngineResult(outputFacts));
					});

			using (ObjectFactory.Substitute(engineMock.Object))
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
				AssertNotNull("Should be able to create return transfer.", returnTransfer);
				AssertNull("No Error Message should have been made.", Notify.LastEvent);

				AssertEquals("Should split.", 2, returnTransfer.Lines.Count);
				var line1 = returnTransfer.Lines.Single(l => l.WE_WL == location4.PK);
				var line2 = returnTransfer.Lines.Single(l => l.WE_WL == location5.PK);
				AssertEquals("5m each location.", 5m, line1.WE_TransactionQuantity);
				AssertEquals("5m each location.", 5m, line2.WE_TransactionQuantity);
			}

			engineMock.Verify(em => em.RunRulesEngine(Factory,
				It.IsNotNull<INotifications>(),
				RulesContextType.InventoryPutaway,
				It.IsAny<ProductionRuleSetFilter>(),
				It.IsAny<Func<IEnumerable<IInputFact>>>(),
				It.IsAny<Func<ProductionRulesEngineResult,
				INotification>>()));
		}

		#endregion

		#region TestMarkVASOrderAsCompleted

		public void TestMarkVASOrderAsCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			AssertEquals("Should not Mark VAS Order as Complete if there are changes.", false, vasOrder.MarkVASOrderAsCompleted(Notify));
			AssertEquals("Should not Mark VAS Order as Complete if there are changes.", false, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Should not Mark VAS Order as Complete if there are changes.", "Save all changes before Completing this VAS Order.\r\n", Notify.AsString);

			Notify.Clear();
			Factory.Save();
			AssertEquals("Should not Mark VAS Order as Complete if Initial Transfer does not exist.", false, vasOrder.MarkVASOrderAsCompleted(Notify));
			AssertEquals("Should not Mark VAS Order as Complete if Initial Transfer does not exist.", false, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Should not Mark VAS Order as Complete if Initial Transfer does not exist.", "Before completing the VAS Order, a Transfer into the Service Area must first be created.\r\n", Notify.AsString);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			Notify.Clear();
			Factory.Save();
			AssertEquals("Should not Mark VAS Order as Complete if Initial Transfer is not Finalised.", false, vasOrder.MarkVASOrderAsCompleted(Notify));
			AssertEquals("Should not Mark VAS Order as Complete if Initial Transfer is not Finalised.", false, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Should not Mark VAS Order as Complete if Initial Transfer is not Finalised.", "Before completing the VAS Order, the Transfer into the Service Area must first be finalized.\r\n", Notify.AsString);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();

			Notify.Clear();
			using (vasOrder.SuspendSettingHasChanges())
			{
				vasOrder.WVO_CancelledTimeUtc = ZDateTime.UtcNow;
				AssertEquals(false, vasOrder.MarkVASOrderAsCompleted(Notify));
				AssertEquals("Cannot Complete Canceled VAS Orders.\r\n", Notify.AsString);

				vasOrder.WVO_CancelledTimeUtc = ZDateTime.Empty;
			}

			Notify.Clear();
			AssertEquals("Should Mark VAS Order as Complete.", true, vasOrder.MarkVASOrderAsCompleted(Notify));
			AssertEquals("Should Mark VAS Order as Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertNull("No Error Message should have been made.", Notify.LastEvent);
			AssertEquals("Should have added a ServiceCompleted Event.", 1, vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Count());

			Factory.Save();
			AssertEquals("Should not re-Mark VAS Order as Complete if it is already Complete.", false, vasOrder.MarkVASOrderAsCompleted(Notify));
			AssertEquals("Should not re-Mark VAS Order as Complete if it is already Complete.", "VAS Order already Complete.\r\n", Notify.AsString);
		}

		#endregion

		#region TestReturnTransferDoesNotMissOutCopyingNewPossibleTransferPertinentColumns

		public void TestReturnTransferDoesNotMissOutCopyingNewPossibleTransferPertinentColumns()
		{
			var columnsHandledOrExcludedInReturnTransferCreation = new HashSet<string>(new[]
			{
				// these columns are copied / set correctly
				WhsDocketLineSchema.Constants.PK,
				WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate,
				WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus,
				WhsDocketLineSchema.Constants.WE_DocketLineStatus,
				WhsDocketLineSchema.Constants.WE_DocketLineType,
				WhsDocketLineSchema.Constants.WE_ExpiryDate,
				WhsDocketLineSchema.Constants.WE_F3_NKPackType,
				WhsDocketLineSchema.Constants.WE_FinalisedDate,
				WhsDocketLineSchema.Constants.WE_IsOriginalInventory,
				WhsDocketLineSchema.Constants.WE_OP,
				WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus,
				WhsDocketLineSchema.Constants.WE_PackingDate,
				WhsDocketLineSchema.Constants.WE_PalletID,
				WhsDocketLineSchema.Constants.WE_PartAttrib1,
				WhsDocketLineSchema.Constants.WE_PartAttrib2,
				WhsDocketLineSchema.Constants.WE_PartAttrib3,
				WhsDocketLineSchema.Constants.WE_SerialNumber,
				WhsDocketLineSchema.Constants.WE_TransferFromPalletId,
				WhsDocketLineSchema.Constants.WE_TransactionQuantity,
				WhsDocketLineSchema.Constants.WE_WD,
				WhsDocketLineSchema.Constants.WE_WE_MatchingLine,
				WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating,
				WhsDocketLineSchema.Constants.WE_WL,
				WhsDocketLineSchema.Constants.WE_WL_TransferFrom,
				WhsDocketLineSchema.Constants.WE_WHC_NKCurrentInventoryHeldCode,
				WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode,
				// these columns do not need to be copied
				WhsDocketLineSchema.Constants.WE_AllocationKey, // Allocation Key is not used by Transfers
				WhsDocketLineSchema.Constants.WE_BondedEntryKey,
				WhsDocketLineSchema.Constants.WE_ClientOrderedUnits,
				WhsDocketLineSchema.Constants.WE_CurrentHoldReason, // you can't select held stock and you can't hold code change Inventory on the VAS Order Transfer In
				WhsDocketLineSchema.Constants.WE_CustomAttrib1,
				WhsDocketLineSchema.Constants.WE_CustomAttrib2,
				WhsDocketLineSchema.Constants.WE_CustomAttrib3,
				WhsDocketLineSchema.Constants.WE_CustomAttrib4,
				WhsDocketLineSchema.Constants.WE_CustomAttrib5,
				WhsDocketLineSchema.Constants.WE_CustomAttrib6,
				WhsDocketLineSchema.Constants.WE_CustomDate1,
				WhsDocketLineSchema.Constants.WE_CustomDate2,
				WhsDocketLineSchema.Constants.WE_CustomDate3,
				WhsDocketLineSchema.Constants.WE_CustomDate4,
				WhsDocketLineSchema.Constants.WE_CustomDate5,
				WhsDocketLineSchema.Constants.WE_CustomDecimal1,
				WhsDocketLineSchema.Constants.WE_CustomDecimal2,
				WhsDocketLineSchema.Constants.WE_CustomDecimal3,
				WhsDocketLineSchema.Constants.WE_CustomDecimal4,
				WhsDocketLineSchema.Constants.WE_CustomDecimal5,
				WhsDocketLineSchema.Constants.WE_CustomFlag1,
				WhsDocketLineSchema.Constants.WE_CustomFlag2,
				WhsDocketLineSchema.Constants.WE_CustomFlag3,
				WhsDocketLineSchema.Constants.WE_CustomFlag4,
				WhsDocketLineSchema.Constants.WE_CustomFlag5,
				WhsDocketLineSchema.Constants.WE_CustomTextBlob1,
				WhsDocketLineSchema.Constants.WE_ExtendedLinePrice,
				WhsDocketLineSchema.Constants.WE_GS_NKPutawayBy,
				WhsDocketLineSchema.Constants.WE_IsValid,
				WhsDocketLineSchema.Constants.WE_LineComment,
				WhsDocketLineSchema.Constants.WE_LineNo,
				WhsDocketLineSchema.Constants.WE_PackageGroupId,
				WhsDocketLineSchema.Constants.WE_PerPackageQty,
				WhsDocketLineSchema.Constants.WE_PickGroup,
				WhsDocketLineSchema.Constants.WE_PutawayTime,
				WhsDocketLineSchema.Constants.WE_ReasonCode,
				WhsDocketLineSchema.Constants.WE_ReceiveCrossDockOrderNo,
				WhsDocketLineSchema.Constants.WE_RecommendedUnitPrice,
				WhsDocketLineSchema.Constants.WE_RequiredByDate,
				WhsDocketLineSchema.Constants.WE_RX_NKUnitPriceCurrency,
				WhsDocketLineSchema.Constants.WE_SubLineNo,
				WhsDocketLineSchema.Constants.WE_StockOnHand,
				WhsDocketLineSchema.Constants.WE_UnitDiscountAmount,
				WhsDocketLineSchema.Constants.WE_UnitDiscountPercent,
				WhsDocketLineSchema.Constants.WE_UnitPriceAfterDiscount,
				WhsDocketLineSchema.Constants.WE_UnloadedTime,
				WhsDocketLineSchema.Constants.WE_GS_NKUnloadedBy,
				WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine,
				WhsDocketLineSchema.Constants.WE_WHC_NKOrderedHeldCode,
				WhsDocketLineSchema.Constants.WE_SystemCreateTimeUtc,
				WhsDocketLineSchema.Constants.WE_SystemCreateUser,
				WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc,
				WhsDocketLineSchema.Constants.WE_SystemLastEditUser,
				WhsDocketLineSchema.Constants.WE_WB_CustomsData,
				WhsDocketLineSchema.Constants.WE_WPL_PutawayLine,
				WhsDocketLineSchema.Constants.WE_P9_Task,
			});

			var columnsThatNeedToBeHandled = new ZStringBuilder();
			foreach (SchemaColumn column in WhsDocketLineSchema.All)
			{
				if (!columnsHandledOrExcludedInReturnTransferCreation.Contains(column.Name)
					&& (column.GetEquivalentZType() != typeof(ZGuid) || column.IsNullable)) // no need to check for non-nullable Guid columns as they will be FKs and the data will be unsavable if not set
				{
					columnsThatNeedToBeHandled.AppendLine(column.Name);
				}
			}

			if (columnsThatNeedToBeHandled.Length > 0)
			{
				Fail(@"The following Docket Line columns were added and not handled by the Return VAS Order Transfer creation.
Either make sure this column is copied correctly or put just put it in the exclusion list." + "\r\n" + columnsThatNeedToBeHandled.ToString());
			}
			else
			{
				Assert(true); // no errors
			}
		}

		#endregion

		#region TestPropertiesModifiedAfterVasOrderTransferIsCreated

		public void TestPropertiesModifiedAfterVasOrderTransferIsCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateArea(data.Whs1, "A2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			AssertNotNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var vasOrderInNewFactory = newFactory.Load<WhsVASOrder>(vasOrder.PK);

			AssertChangedProperties(newFactory, vasOrderInNewFactory, WhsVASOrderSchema.WVO_OH_Client, Helper.CreateClient("C2").PK);
			AssertChangedProperties(newFactory, vasOrderInNewFactory, WhsVASOrderSchema.WVO_WA_ServiceArea, data.Whs1.Areas[1].PK);
		}

		static void AssertChangedProperties(BusinessObjectFactory newFactory, WhsVASOrder vasOrderInNewFactory, SchemaColumn column, IZType differentValue)
		{
			var originalValue = vasOrderInNewFactory[column];
			var helper = new WhsTestHelperFunctions(newFactory);
			vasOrderInNewFactory[column] = differentValue;
			Assert(vasOrderInNewFactory.HasChanges);
			helper.AssertZCannotSaveExceptionThrown("Cannot save as fields are modified after Into Service Area Transfer is created.", newFactory.Save);
			vasOrderInNewFactory[column] = originalValue;
			vasOrderInNewFactory.HasChanges = false;
			Assert(!vasOrderInNewFactory.HasChanges);
			AssertNoExceptionThrown(() => newFactory.Save());
		}

		#endregion

		#region TestClientRate_UnitCalculator_AutoRatingVASOrder

		public void TestClientRate_UnitCalculator_AutoRatingVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();

			var jobHeader = new JobHeader.Loader(vasOrder).TryLoadOrCreateWithoutMutexForTestOnly();
			Factory.Save();
			jobHeader.InitializeParentFromGenericJobWithoutSettingDefaults();

			var newFactory = new BusinessObjectFactory();
			var loadedJob = newFactory.Load<DummyJobHeader>(jobHeader.PK);
			var logs = new List<string>();
			loadedJob.ProcessLogs = logs;

			loadedJob.InitializeParentFromGenericJobWithSettingDefaults();
			AssertEquals("Job defaults are set", "SetParentCore", string.Join("=>", logs));
			AssertEquals(true, loadedJob.DefaultValuesHasBeenAssigned);
		}

		#endregion

		#region TestIAutoRatingVASOrderConsumerType

		public void TestIAutoRatingVASOrderConsumerType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();

			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);
			AssertEquals(JobInvoicingConsumerTypes.WarehouseVASOrder, autoRating.InvoicingSupporter.ConsumerType);
		}

		#endregion

		#region TestIAutoRatingVASOrder

		public void TestIAutoRatingVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();

			var autoRating = (IAutoRating)new WhsVASOrderRatingAdapter(vasOrder);

			var servicesWrapper = autoRating.JobServices;
			var allServiceTypes = vasOrder.Services.AddNew().Lookups.JobServiceType_List;

			foreach (CodeDescriptionPair serviceType in allServiceTypes)
			{
				AssertEquals(true, servicesWrapper.Contains(ChargeCodeGroupList.Codes.WHSAdHocServiceJob, serviceType.Code));
			}

			AssertNotEquals("List should not be cached.", autoRating.JobServices, servicesWrapper);
			AssertEquals(allServiceTypes.Count, servicesWrapper.Count);
		}

		#endregion

		#region TestIAutoRatingVASOrder_ChargeShouldPresentInInvoice

		[TestDate(2024, 1, 16, 18, 0, 0)]
		public void TestIAutoRatingVASOrder_ChargeShouldPresentInInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			new AccountingPeriodTestHelper().SetupPeriods();
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			var yesterday = ZDateTimeOffset.Today.AddDays(-1);
			helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday, data.Part1, 10m);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();
			vasOrder.FinaliseVASOrder(Notify);
			AssertEquals("Precondition: VAS Order is finalised.", true, vasOrder.IsFinalised);

			// Setting to generate charge when autorate invoice
			var fumigationCharge = helper.CreateChargeCode($"WLAB", "Warehouse Handling Fumigation", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, FreightServiceType.Codes.Fumigation);
			var clientRate = helper.CreateClientRate(client);
			var warehouseRate = helper.CreateRateEntry(clientRate, ZDate.Today.AddYears(-1), ZDate.Empty);
			helper.CreateRateLine(warehouseRate, fumigationCharge, "UNT", 5m);

			// any charges
			var fumigationService = vasOrder.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = yesterday.ToZDateTime();

			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(vasOrder, WhsVASOrderSchema.Constants.Prefix);
			jobHeader.JH_OA_LocalChargesAddr = client.Addresses[0].PK;
			var jobsToRate = new IAutoRating[] { ((IRatingSupporter)vasOrder).AdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue).FirstOrDefault() };
			Helper.AutoRateJob(vasOrder, jobHeader, jobsToRate);
			AssertEquals("Precondition - ensure has charges.", true, jobHeader.Charges.Count > 0);

			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-6), ZDateTime.Now);
			Factory.Save();

			var logger = new Mock<IAutoRatingServiceLogger>();
			var whsInvoiceHelper = new WhsInvoiceHelper();
			var succeed = whsInvoiceHelper.AutoRateJobHeader(logger.Object, invoice);
			AssertEquals("Ensure invoice include vasOrder charges.", true, invoice.JobHeader.Charges.Count > 0);
			whsInvoiceHelper.DisposeLoadedJobHeaders(invoice.Factory);
			Assert(succeed);
		}

		#endregion

		// interfaces

		#region ICancellable Members

		#region TestCanCancel

		public void TestCanCancel()
		{
			var cancellableVasOrder = Factory.New<WhsVASOrder>();
			cancellableVasOrder.WVO_JobID = "WVO1";
			AssertNull(cancellableVasOrder.CanCancel());

			var vasOrderWithUnfinalisedTransferInventoryIn = Factory.New<WhsVASOrder>();
			vasOrderWithUnfinalisedTransferInventoryIn.WVO_WD_TransferIntoServiceArea = Factory.New<WhsTransfer>().PK;
			AssertNull(vasOrderWithUnfinalisedTransferInventoryIn.CanCancel());
		}

		#endregion

		#region TestCanCancel_WithCommencedInitialTransfer

		public void TestCanCancel_WithCommencedInitialTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var vasOrderWithFinalisedTransfer = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrderWithFinalisedTransfer, data.Part1, 5m);
			Factory.Save();

			var vasOrderTransfer = vasOrderWithFinalisedTransfer.GetOrCreateInitialTransfer(Notify);
			vasOrderTransfer.Logs.AddNew(Events.ServiceCommenced);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			vasOrderWithFinalisedTransfer = factory2.Load<WhsVASOrder>(vasOrderWithFinalisedTransfer.PK);

			AssertEquals(string.Format("Transfer / Work has already started. {0} cannot be canceled.", vasOrderWithFinalisedTransfer.HumanReadableName), vasOrderWithFinalisedTransfer.CanCancel());
		}

		#endregion

		#region TestCanCancel_WithFinalisedInitialTransfer

		public void TestCanCancel_WithFinalisedInitialTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var vasOrderWithFinalisedTransfer = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrderWithFinalisedTransfer, data.Part1, 5m);
			Factory.Save();

			var vasOrderTransfer = vasOrderWithFinalisedTransfer.GetOrCreateInitialTransfer(Notify);
			vasOrderTransfer.NotificationManager.Push(Notify);
			vasOrderTransfer.FinaliseDocket();
			AssertEquals(true, vasOrderTransfer.IsFinalised);
			AssertEquals(string.Format("Transfer / Work has already started. {0} cannot be canceled.", vasOrderWithFinalisedTransfer.HumanReadableName), vasOrderWithFinalisedTransfer.CanCancel());
		}

		#endregion

		#region TestCancel

		public void TestCancel()
		{
			var cancellableVasOrder = Factory.New<WhsVASOrder>();
			cancellableVasOrder.IsCancelled = true;
			AssertEquals(true, cancellableVasOrder.WVO_CancelledTimeUtc.IsValid);
			AssertEquals(true, cancellableVasOrder.ReadOnly);
		}

		#endregion

		#region TestCancel_WithInitialTransfer

		public void TestCancel_WithInitialTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var vasOrderWithFinalisedTransfer = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrderWithFinalisedTransfer, data.Part1, 5m);
			Factory.Save();

			var vasOrderTransfer = vasOrderWithFinalisedTransfer.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition", false, vasOrderTransfer.IsDeleted);

			vasOrderWithFinalisedTransfer.IsCancelled = true;
			AssertEquals(true, vasOrderWithFinalisedTransfer.WVO_CancelledTimeUtc.IsValid);
			AssertEquals(true, vasOrderWithFinalisedTransfer.ReadOnly);
			AssertEquals(true, vasOrderTransfer.IsDeleted);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestPreventDelete

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", true, PreventDeleteAttribute.IsTrue(typeof(WhsVASOrder)));
		}

		#endregion

		#region TestCanReactivate

		public void TestCanReactivate()
		{
			AssertNull(Factory.New<WhsVASOrder>().CanReactivate());
		}

		#endregion

		#endregion

		#region ITemplateCopyableMembers

		#region TestITemplateCopyableTemplateCopy

		public void TestITemplateCopyableTemplateCopy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var whsVASOrder = CreateWhsVASOrderWithTwoLines(data, serviceArea);
			Factory.Save();

			var copy = (WhsVASOrder)((ITemplateCopyable)whsVASOrder).TemplateCopy();
			AssertWhsVASOrderCopy(copy, data, serviceArea); // Copy Should create new entry from new WhsVASOrder.
			copy.WVO_JobID = "WV00000002";
			copy.RunPreSaveValidation();
			AssertEquals("Copy should have no errors", true, !copy.HasErrors);
			Factory.Save();
			Notify.Clear();

			var initialTransfer = whsVASOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 3, initialTransfer.Lines.Count);

			var copyOfOrderWithInitialTransfer = (WhsVASOrder)((ITemplateCopyable)whsVASOrder).TemplateCopy();
			AssertWhsVASOrderCopy(copyOfOrderWithInitialTransfer, data, serviceArea); //Copy Should create new entry from dbo.WhsVASOrder with initial transfer but not copy transfer.
			copyOfOrderWithInitialTransfer.WVO_JobID = "WV00000003";
			copyOfOrderWithInitialTransfer.RunPreSaveValidation();
			AssertEquals("Copy should have no errors", true, !copyOfOrderWithInitialTransfer.HasErrors);
			Factory.Save();
			Notify.Clear();

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);

			var copyOfOrderWithFinalisedInitialTransfer = (WhsVASOrder)((ITemplateCopyable)whsVASOrder).TemplateCopy();
			AssertWhsVASOrderCopy(copyOfOrderWithFinalisedInitialTransfer, data, serviceArea); //Copy Should create new entry from dbo.WhsVASOrder with finalised transfer but not copy transfer.
			copyOfOrderWithFinalisedInitialTransfer.WVO_JobID = "WV00000004";
			copyOfOrderWithFinalisedInitialTransfer.RunPreSaveValidation();
			AssertEquals("Copy should have no errors", true, !copyOfOrderWithFinalisedInitialTransfer.HasErrors);
			Factory.Save();
			Notify.Clear();

			Factory.Save();
			Notify.Clear();
			AssertEquals("Should not Finalise VAS Order if work is not complete.", false, whsVASOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should not Finalise VAS Order if work is not complete.", false, whsVASOrder.IsFinalised);
			AssertEquals("Should not Finalise VAS Order if work is not complete.", "Cannot Finalize VAS Order until the Work has been Completed.\r\n", Notify.AsString);

			whsVASOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, whsVASOrder.WVO_WorkCompletedTimeUtc.IsValid);

			Factory.Save();
			Notify.Clear();

			Notify.DefaultResponse = true;
			AssertEquals("Should Finalise VAS Order.", true, whsVASOrder.FinaliseVASOrder(Notify));
			AssertEquals("Should Finalise VAS Order.", true, whsVASOrder.IsFinalised);

			var copyOfFinalised = (WhsVASOrder)((ITemplateCopyable)whsVASOrder).TemplateCopy();
			AssertWhsVASOrderCopy(copyOfFinalised, data, serviceArea); //Copy Should create new entry from finalised WhsVASOrder but not copy transfers or status.
			copyOfFinalised.RunPreSaveValidation();
			AssertEquals("Copy should have no errors", true, !copyOfFinalised.HasErrors);
		}

		#endregion

		#region TestITemplateCopyableTemplateCopy_Excludes_WVO_IsWorkCompleted

		public void TestITemplateCopyableTemplateCopy_Excludes_WVO_IsWorkCompleted()
		{
			TestITemplateCopyableTemplateCopy_ExcludeFields("No constraint violation Constraint_WVO_IsWorkCompleted", true, false, false);
		}

		#endregion

		#region TestITemplateCopyableTemplateCopy_Excludes_WVO_IsFinalised

		public void TestITemplateCopyableTemplateCopy_Excludes_WVO_IsFinalised()
		{
			TestITemplateCopyableTemplateCopy_ExcludeFields("No constraint violation Constraint_WVO_IsFinalised", true, true, false);
		}

		#endregion

		#region TestITemplateCopyableTemplateCopy_Excludes_WVO_IsCancelled

		public void TestITemplateCopyableTemplateCopy_Excludes_WVO_IsCancelled()
		{
			TestITemplateCopyableTemplateCopy_ExcludeFields("No constraint violation", false, false, true);
		}

		#endregion

		#region TestITemplateCopyableTemplateCopy_ExcludeFields

		void TestITemplateCopyableTemplateCopy_ExcludeFields(string message, bool isWorkCompleted, bool isFinalised, bool isCancelled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var transferIntoServiceArea = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var whsVASOrder = CreateWhsVASOrderWithTwoLines(data, serviceArea);
			whsVASOrder.WVO_WD_TransferIntoServiceArea = !isCancelled ? transferIntoServiceArea.PK : ZGuid.Empty;
			whsVASOrder.WVO_WorkCompletedTimeUtc = isWorkCompleted ? ZDateTime.UtcNow : ZDateTime.Empty;
			whsVASOrder.WVO_FinalizedTimeUtc = isFinalised ? ZDateTime.UtcNow : ZDateTime.Empty;
			whsVASOrder.WVO_CancelledTimeUtc = isCancelled ? ZDateTime.UtcNow : ZDateTime.Empty;
			Factory.Save();

			AssertEquals("precondition: Is Work Completed", isWorkCompleted, whsVASOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("precondition: Is Finalised", isFinalised, whsVASOrder.IsFinalised);
			AssertEquals("precondition: Is Cancelled", isCancelled, whsVASOrder.IsCancelled);
			Assert("precondition - no errors", !whsVASOrder.HasErrors);

			var copy = (WhsVASOrder)((ITemplateCopyable)whsVASOrder).TemplateCopy();
			AssertWhsVASOrderCopy(copy, data, serviceArea); // Copy Should create new entry from new WhsVASOrder.
			copy.WVO_JobID = "WV00000002";
			copy.RunPreSaveValidation();

			AssertEquals("Copy should have no errors", false, copy.HasErrors);
			AssertNoExceptionThrown(message, () => Factory.Save());
		}

		#endregion

		#region AssertWhsVASOrderCopy

		void AssertWhsVASOrderCopy(WhsVASOrder copy, TestDataSimpleEnvironment data, WhsArea serviceArea)
		{
			AssertEquals("JobID should be empty for a new WhsVASOrder", true, string.IsNullOrEmpty(copy.WVO_JobID));
			AssertEquals("Customer Reference No should be empty for a new WhsVASOrder", true, string.IsNullOrEmpty(copy.WVO_CustomerReferenceNo));
			AssertEquals("Copy should have same Client Address as original", data.Org1.PK, copy.WVO_OH_Client);
			AssertEquals("Copy should have same Service Area as original", serviceArea.PK, copy.WVO_WA_ServiceArea);
			AssertEquals("Copy should have no TransferIntoServiceArea", ZGuid.Empty, copy.WVO_WD_TransferIntoServiceArea);
			AssertEquals("Copy should have no TransferOutOfServiceArea", ZGuid.Empty, copy.WVO_WD_TransferOutOfServiceArea);
			AssertEquals("Copy should have same Client as original", data.Org1, copy.Client);
			AssertEquals("Copy should have same Line count as original", 2, copy.Lines.Count);
			AssertEquals("Copy should have same Notifications count as original", 0, copy.Notifications.Count());
			AssertEquals("Copy should have same Product code as original", "Many", copy.ProductCode);
			AssertEquals("Copy should have same Product Quantity as original", 15m, copy.ProductQty);
			AssertEquals("Copy should have same Service area as original", data.Whs1.Areas[2].PK, copy.ServiceArea.PK);
			AssertEquals("Copy should not have same Status as original, copy is a new entry", "NEW ENTRY", copy.Status);
			AssertEquals("Copy should have same Warehouse as original", data.Whs1, copy.Warehouse);
			AssertEquals("Copy should have not set IsWorkCompleted", false, copy.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Copy should have not set IsFinalised", false, copy.WVO_FinalizedTimeUtc.IsValid);
			AssertEquals("Copy should have not set IsCancelled", false, copy.WVO_CancelledTimeUtc.IsValid);
		}

		#endregion

		#endregion

		#region IHaveServices

		public void TestIHaveServices()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			var iHaveServices = (IHaveServices)vasOrder;

			AssertEquals("", iHaveServices.ContainerMode);
			AssertEquals("", iHaveServices.TransportMode);
			AssertEquals(0, iHaveServices.DependentServiceParents.Length);
			AssertEquals(vasOrder, iHaveServices.ServiceParent);
			AssertEquals(vasOrder.TablePrefix, iHaveServices.TableCode);
		}

		public void TestServiceBranch()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			var iHaveServices = (IHaveServices)vasOrder;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var warehouse = Helper.CreateWarehouse("Whs");
			AssertEquals(true, warehouse.WW_GB_RelatedCompanyBranch.IsValid);
			var area = Helper.CreateArea(warehouse, "A1");

			vasOrder.WVO_WA_ServiceArea = area.PK;
			AssertEquals("Service branch is warehouse branch", warehouse.WW_GB_RelatedCompanyBranch, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#region TestVASOrderSerivices

		public void TestVASOrderSerivices()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var service = vasOrder.Services.AddNew();
			service.ES_ServiceCode = "FUM";
			Factory.Save();
			AssertEquals(false, service.HasErrors);
			AssertCollectionContains("Collection sould have this serivce.", service, vasOrder.Services);
			AssertEquals(typeof(WhsJobService), vasOrder.Services.TypeOfElements);
			AssertEquals(true, vasOrder.IsRegisteredEditableChildObject(vasOrder.Services));
		}

		#endregion

		#region	TestServices_readonly

		public void TestServices_readonly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			AssertExceptionThrown(typeof(InvalidOperationException), "You cannot finalise the VAS Order without first completing it.", () => vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow);

			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Is Readonly only if when is finalize.", false, vasOrder.Services.ReadOnly);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			AssertEquals("Is Readonly only if when is finalize.", false, vasOrder.Services.ReadOnly);

			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			AssertEquals("Is Readonly only if when is finalize.", false, vasOrder.Services.ReadOnly);

			Factory.Save();
			vasOrder.FinaliseVASOrder(Notify);
			AssertEquals("Precondition: VAS Order is finalised.", true, vasOrder.IsFinalised);
			AssertEquals("Is Readonly when is finalize.", true, vasOrder.Services.ReadOnly);
		}

		#endregion

		#region TestInvoicingSupporter

		public void TestInvoicingSupporter()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var job = (IJobInvoicingPlugIn)vasOrder;
			AssertType(typeof(WhsVASOrderInvoicingSupporter), job.InvoicingSupporter);
			AssertEquals(true, job.AllowInvoiceDeletion);
		}

		#endregion

		#region TestIJobHeaderParent

		public void TestIJobHeaderParent()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var jobheader = new JobHeader.Loader(vasOrder).TryCreate();
			AssertEquals(jobheader, vasOrder.Job);
		}

		#endregion

		#region ILineToPutawayParentMembers

		public void TestILineToPutawayParentMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			AssertEquals(nameof(ILineToPutawayParent.ClientPK), vasOrder.Client.PK, ((ILineToPutawayParent)vasOrder).ClientPK);
			AssertEquals(nameof(ILineToPutawayParent.WarehousePK), vasOrder.WarehousePK, ((ILineToPutawayParent)vasOrder).WarehousePK);
			AssertEquals(nameof(ILineToPutawayParent.Client), vasOrder.Client, ((ILineToPutawayParent)vasOrder).Client);
			AssertEquals(nameof(ILineToPutawayParent.Warehouse), vasOrder.Warehouse, ((ILineToPutawayParent)vasOrder).Warehouse);
		}

		#endregion

		#region IWhsVASOrderMembers

		public void TestIWhsVASOrderMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			AssertEquals(nameof(IWhsVASOrder.PK), vasOrder.PK, ((IWhsVASOrder)vasOrder).PK);
			AssertEquals(nameof(IWhsVASOrder.WVO_CustomerReferenceNo), vasOrder.WVO_CustomerReferenceNo, ((IWhsVASOrder)vasOrder).WVO_CustomerReferenceNo);
			AssertEquals(nameof(IWhsVASOrder.WarehousePK), vasOrder.WarehousePK, ((IWhsVASOrder)vasOrder).WarehousePK);
		}

		#endregion

		#region TestVASOrderNoteTypes

		public void TestVASOrderNoteTypes()
		{
			var noteTypes = Factory.New<WhsVASOrder>().NoteTypes.Cast<PredefinedNoteType>();
			AssertEquals("Expecting AutoRatingAuditLog note types", true, noteTypes.Contains(PredefinedNoteTypes.Instance.AutoRatingAuditLog));
		}

		#endregion

		#region Implementation

		#region CreateWhsVASOrderWithTwoLines

		WhsVASOrder CreateWhsVASOrderWithTwoLines(TestDataSimpleEnvironment data, WhsArea serviceArea)
		{
			var whsVASOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(whsVASOrder, data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(whsVASOrder, data.Part2, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 5m);

			whsVASOrder.WVO_JobID = "WV00000001";
			whsVASOrder.WVO_CustomerReferenceNo = "5";
			whsVASOrder.RunPreSaveValidation();

			Assert(!whsVASOrder.HasErrors);

			return whsVASOrder;
		}

		#endregion

		// hack to get refreshbinding test to pass
		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
			info.BizObj.IgnoreValidationSuspended =
				info.Name == WhsVASOrderSchema.Constants.WVO_FinalizedTimeUtc
				|| info.Name == WhsVASOrderSchema.Constants.WVO_GS_NKFinalizedBy
				|| info.Name == WhsVASOrderSchema.Constants.WVO_WorkCompletedTimeUtc
				|| info.Name == WhsVASOrderSchema.Constants.WVO_GS_NKWorkCompletedBy;
			if (info.BizObj.IgnoreValidationSuspended)
			{
				RunValidationInvoker validation = null;
				validation = () => { info.RefreshBinding(); info.AdditionalValidation -= validation; };
				info.AdditionalValidation += validation;
			}
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result.Add(WhsVASOrderSchema.Constants.WVO_FinalizedTimeUtc, ZDateTime.Empty);
				result.Add(WhsVASOrderSchema.Constants.WVO_GS_NKFinalizedBy, ZString.Empty);
				result.Add(WhsVASOrderSchema.Constants.WVO_WorkCompletedTimeUtc, ZDateTime.Empty);
				result.Add(WhsVASOrderSchema.Constants.WVO_GS_NKWorkCompletedBy, ZString.Empty);
				return result;
			}
		}

		protected override TestNotificationBuffer GetNotify()
		{
			return new TestNotificationBuffer { DefaultResponse = true };
		}

		#endregion

		#region TestICriticalChangesVersionID

		public void TestICriticalChangesVersionID_IsImmutableStatus()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			AssertEquals("Precondition", false, vasOrder.WVO_WD_TransferIntoServiceArea.IsValid);
			AssertEquals(false, ((ICriticalChangesVersionID)vasOrder).IsImmutableStatus);

			var transfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferIntoServiceArea = transfer.PK;

			AssertEquals("Precondition", true, ((ICriticalChangesVersionID)vasOrder).IsImmutableStatus);
			AssertEquals(true, ((ICriticalChangesVersionID)vasOrder).IsImmutableStatus);
		}

		public void TestICriticalChangesVersionID_CriticalChangesVersionID()
		{
			var vasOrder = (ICriticalChangesVersionID)Factory.New<WhsVASOrder>();
			var newGuid = ZGuid.NewZGuid();
			AssertNotEquals("Precondition", newGuid, vasOrder.CriticalChangesVersionID);

			vasOrder.CriticalChangesVersionID = newGuid;
			AssertEquals("Should be able to cast and set and get value", newGuid, vasOrder.CriticalChangesVersionID);
		}

		public void TestUpdateVASOrderVersion_DeleteVASOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 5m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 1m);
			Factory.Save();

			var vasOrderVersion = vasOrder.WVO_CriticalChangesVersionID;
			vasOrder.Lines[1].Delete();
			Factory.Save();

			AssertNotEquals("Should update version after delete a line.", vasOrderVersion, vasOrder.WVO_CriticalChangesVersionID);
		}

		public void TestUpdateVASOrderVersion_ClearVersionAfterTransferIn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			AssertNotEquals("Precondition", ZGuid.Empty, vasOrder.WVO_CriticalChangesVersionID);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);
			Factory.Save();

			AssertEquals("Should clear after transfer in.", ZGuid.Empty, vasOrder.WVO_CriticalChangesVersionID);
		}

		public void TestUpdateVASOrderVersion_UpdateOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("C2");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 1m);
			Factory.Save();

			var changed = 0;
			vasOrder.WVO_CriticalChangesVersionIDInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				changed++;
			};

			var client = vasOrder.WVO_OH_Client;
			var newClient = org2;
			vasOrder.WVO_OH_Client = newClient.PK;
			AssertNotEquals(client, vasOrder.WVO_OH_Client);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 1m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 1m);

			AssertEquals("Precondition.", 0, changed);
			Factory.Save();

			AssertEquals("Should only call once.", 1, changed);
		}

		#endregion
	}

	public class WhsVASOrderNonTransactionedTestCase : TestCase
	{
		#region	TestWVO_CriticalChangesVersionID

		[UseSnapshotProtection]
		public void TestWVO_CriticalChangesVersionID_NonCriticalFields()
		{
			Action<WhsVASOrder, OrgSupplierPart> makeChange = (vasOrder, product) =>
			{
				vasOrder.WVO_SystemLastEditTimeUtc = ZDateTime.Now;
				vasOrder.Factory.Save();
			};

			TestWVO_CriticalChangesVersionIDCore(makeChange, expectedException: false);
		}

		[UseSnapshotProtection]
		public void TestWVO_CriticalChangesVersionID_WVO_OA_ClientAddress()
		{
			var otherClientPK = Helper.CreateClient("C2").PK;
			Factory.Save();
			Action<WhsVASOrder, OrgSupplierPart> makeChange = (vasOrder, product) =>
			{
				vasOrder.WVO_OH_Client = otherClientPK;
				vasOrder.Factory.Save();
			};

			TestWVO_CriticalChangesVersionIDCore(makeChange, expectedException: true);
		}

		[UseSnapshotProtection]
		public void TestWVO_CriticalChangesVersionID_TransferToServiceArea()
		{
			Action<WhsVASOrder, OrgSupplierPart> makeChange = (vasOrder, product) =>
			{
				var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
				initialTransfer.Factory.Save();
			};

			TestWVO_CriticalChangesVersionIDCore(makeChange, expectedException: true);
		}

		[UseSnapshotProtection]
		public void TestWVO_CriticalChangesVersionID_ServiceArea()
		{
			Action<WhsVASOrder, OrgSupplierPart> makeChange = (vasOrder, product) =>
			{
				Helper.CreateArea(vasOrder.Warehouse, "A2");
				vasOrder.WVO_WA_ServiceArea = vasOrder.Warehouse.Areas.First(a => a.WA_Name == "A2").PK;
				vasOrder.Factory.Save();
			};

			TestWVO_CriticalChangesVersionIDCore(makeChange, expectedException: true);
		}

		[UseSnapshotProtection]
		public void TestWVO_CriticalChangesVersionID_AddLine()
		{
			Action<WhsVASOrder, OrgSupplierPart> makeChange = (vasOrder, product) =>
			{
				Helper.CreateWhsVASOrderLine(vasOrder, product, 5m);
				vasOrder.Factory.Save();
			};

			TestWVO_CriticalChangesVersionIDCore(makeChange, expectedException: true);
		}

		[UseSnapshotProtection]
		public void TestWVO_CriticalChangesVersionID_RemoveLine()
		{
			Action<WhsVASOrder, OrgSupplierPart> makeChange = (vasOrder, product) =>
			{
				vasOrder.Lines[0].Delete();
				vasOrder.Factory.Save();
			};

			TestWVO_CriticalChangesVersionIDCore(makeChange, expectedException: true);
		}

		[UseSnapshotProtection]
		public void TestWVO_CriticalChangesVersionID_EditLine()
		{
			Action<WhsVASOrder, OrgSupplierPart> makeChange = (vasOrder, product) =>
			{
				vasOrder.Lines[0].WVL_LineNumber += 1;
				vasOrder.Factory.Save();
			};

			TestWVO_CriticalChangesVersionIDCore(makeChange, expectedException: true);
		}

		void TestWVO_CriticalChangesVersionIDCore(Action<WhsVASOrder, OrgSupplierPart> makeChange, bool expectedException)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);

			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Factory.Save();

			AssertEquals("Precondition", false, vasOrder.WVO_WD_TransferIntoServiceArea.IsValid);

			SomeoneMakeChangeInOtherFactory(makeChange);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			if (expectedException)
			{
				var ex = AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
				AssertContains("Should not other user be able change same vasOrder when it transfer into service area.", "**CONCURRENCY Error Saving Record **", ex.Message);
			}
			else
			{
				Factory.Save();
			}

			void SomeoneMakeChangeInOtherFactory(Action<WhsVASOrder, OrgSupplierPart> makeSomeChanges)
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var factory = new BusinessObjectFactory(connection) { RefreshEnabled = false };
					var vasOrderInOtherFactory = factory.Load<WhsVASOrder>(vasOrder.PK);
					makeSomeChanges(vasOrderInOtherFactory, data.Part1);
				}
			}
		}

		#endregion

		#region TestWVO_WD_TransferIntoServiceArea_WhenTransferToSeriveAreaCannotChangeVersionID

		[UseSnapshotProtection]
		public void TestWVO_WD_TransferIntoServiceArea_WhenTransferToSeriveAreaCannotChangeVersionID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);

			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 5m);
			Factory.Save();

			SomeoneTransferIn();

			vasOrder.WVO_CriticalChangesVersionID = ZGuid.NewZGuid();
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
			AssertContains("Should not other user be able change same vasOrder when it transfer into service area.", "**CONCURRENCY Error Saving Record **", ex.Message);

			void SomeoneTransferIn()
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var factory = new BusinessObjectFactory(connection) { RefreshEnabled = false };
					var vasOrderInOtherFactory = factory.Load<WhsVASOrder>(vasOrder.PK);
					var initialTransfer = vasOrderInOtherFactory.GetOrCreateInitialTransfer(Notify);
					initialTransfer.Factory.Save();
				}
			}
		}

		#endregion

		#region TestWVO_WD_TransferIntoServiceArea_ConcurrencyPolicyStrict

		[UseSnapshotProtection]
		public void TestWVO_WD_TransferIntoServiceArea_ConcurrencyPolicyStrict()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);

			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 5m);
			Factory.Save();

			SomeoneTransferIn();

			Helper.CreateArea(data.Whs1, "A2");
			vasOrder.WVO_WA_ServiceArea = vasOrder.Warehouse.Areas.First(a => a.WA_Name == "A2").PK;
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
			AssertContains("Should not other user be able change same vasOrder when it transfer into service area.", "**CONCURRENCY Error Saving Record **", ex.Message);

			void SomeoneTransferIn()
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var factory = new BusinessObjectFactory(connection) { RefreshEnabled = false };
					var vasOrderInOtherFactory = factory.Load<WhsVASOrder>(vasOrder.PK);
					var initialTransfer = vasOrderInOtherFactory.GetOrCreateInitialTransfer(Notify);
					initialTransfer.Factory.Save();
				}
			}
		}

		#endregion

		#region TestWVO_WD_TransferOutOfServiceArea_ConcurrencyPolicyStrict

		[UseSnapshotProtection]
		public void TestWVO_WD_TransferOutOfServiceArea_ConcurrencyPolicyStrict()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);

			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, data.Org1, data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			Factory.Save();
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();

			SomeoneTransferIn();

			var transfer = Factory.New<WhsTransfer>();
			vasOrder.WVO_WD_TransferOutOfServiceArea = transfer.PK;
			AssertExceptionThrown<ZSaveException>("Factory save fails.", Factory.Save);

			void SomeoneTransferIn()
			{
				using (Db.DisposableActionForDbConnection())
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var factory = new BusinessObjectFactory(connection) { RefreshEnabled = false };
					var vasOrderInOtherFactory = factory.Load<WhsVASOrder>(vasOrder.PK);
					using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
					{
						vasOrderInOtherFactory.GetOrCreateReturnTransfer(Notify);
					}

					initialTransfer.Factory.Save();
					AssertEquals("Precondition", true, vasOrderInOtherFactory.WVO_WD_TransferOutOfServiceArea.IsValid);
				}
			}
		}

		#endregion

		#region Implementation

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory() { RefreshEnabled = false });
		BusinessObjectFactory factory;

		TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		TestNotificationBuffer notify;

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
