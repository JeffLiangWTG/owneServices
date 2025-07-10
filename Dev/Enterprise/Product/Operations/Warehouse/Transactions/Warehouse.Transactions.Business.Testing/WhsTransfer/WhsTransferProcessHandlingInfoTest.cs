using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsTransferProcessHandlingInfoTest : WhsTestCaseWithFactory
	{
		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Factory.Save();

			var handlingInfo = new WhsTransferProcessHandlingInfo(transfer);
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ServiceCancelledCode;
			}

			AssertEquals(Enumerable.Empty<CascadingLink>(), handlingInfo.GetCascadingTargets(eventLog));
		}

		#endregion

		#region TestPropagationTargets_TransferIn

		public void TestPropagationTargets_TransferIn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);

			Factory.Save();
			var vasOrderWorkflowItemNonPropagated = vasOrder.WorkflowItems.AddNew();
			vasOrderWorkflowItemNonPropagated.P9_Type = Constants.Workflow.WorkflowTriggerType;
			vasOrderWorkflowItemNonPropagated.TriggerConditions.TriggerEventCode = Events.ServiceCompletedCode;

			var vasOrderWorkflowItemPropagated = vasOrder.WorkflowItems.AddNew();
			vasOrderWorkflowItemPropagated.P9_Type = Constants.Workflow.WorkflowTriggerType;
			vasOrderWorkflowItemPropagated.TriggerConditions.TriggerEventCode = Events.ItemDocumentJobFinalisedCode;

			initialTransfer.WD_DocketStatus = DocketStatus.Codes.Finalised;
			initialTransfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			initialTransfer.Logs.AddNew(Events.ServiceCompleted);
			AssertEquals("Should not propagate Event if it is not Finalise event.", true, vasOrderWorkflowItemNonPropagated.P9_ActualDate.IsEmpty);
			AssertEquals("Should not propagate Event if it is not Finalise event.", 0, vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Count());

			var logAdded = initialTransfer.Logs.AddNew(Events.ItemDocumentJobFinalised);
			AssertEquals("Transfer In should propagate Finalise event to the VAS Order.", false, vasOrderWorkflowItemPropagated.P9_ActualDate.IsEmpty);
			AssertEquals("Transfer In should propagate Finalise event to the VAS Order.", logAdded.SL_EventTime, vasOrderWorkflowItemPropagated.P9_ActualDate.ToZDateTime());
			AssertEquals("Event Reference needs to specify Transfer In was Finalised. The Whole Reference must match so that Trigger Conditions will work.", "Propagated: All Transfer In",
				vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ItemDocumentJobFinalisedCode).Single().SL_Reference);
		}

		#endregion

		#region TestPropagationTargets_TransferOut

		public void TestPropagationTargets_TransferOut()
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
			AssertEquals("Precondition: Return Transfer is transferring correct stock.", 1, returnTransfer.Lines.Count);

			Factory.Save();
			var vasOrderWorkflowItemNonPropagated = vasOrder.WorkflowItems.AddNew();
			vasOrderWorkflowItemNonPropagated.P9_Type = Constants.Workflow.WorkflowTriggerType;
			vasOrderWorkflowItemNonPropagated.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;

			var vasOrderWorkflowItemPropagated = vasOrder.WorkflowItems.AddNew();
			vasOrderWorkflowItemPropagated.P9_Type = Constants.Workflow.WorkflowTriggerType;
			vasOrderWorkflowItemPropagated.TriggerConditions.TriggerEventCode = Events.ItemDocumentJobFinalisedCode;

			returnTransfer.WD_DocketStatus = DocketStatus.Codes.Finalised;
			returnTransfer.WD_FinalisedDate = ZDateTimeOffset.Now;
			returnTransfer.Logs.AddNew(Events.ServiceInvoicePosted);
			AssertEquals("Should not propagate Event if it is not Finalise event.", true, vasOrderWorkflowItemNonPropagated.P9_ActualDate.IsEmpty);
			AssertEquals("Should not propagate Event if it is not Finalise event.", 0, vasOrder.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceInvoicePostedCode).Count());

			var logAdded = returnTransfer.Logs.AddNew(Events.ItemDocumentJobFinalised);
			AssertEquals("Transfer Out should propagate Finalise event to the VAS Order.", false, vasOrderWorkflowItemPropagated.P9_ActualDate.IsEmpty);
			AssertEquals("Transfer Out should propagate Finalise event to the VAS Order.", logAdded.SL_EventTime, vasOrderWorkflowItemPropagated.P9_ActualDate.ToZDateTime());
			AssertEquals("Event Reference needs to specify Transfer Out was Finalised. The Whole Reference must match so that Trigger Conditions will work.", "Propagated: All Transfer Out",
				vasOrder.Logs.Find(l => !l.IsInDatabase && l.SL_SE_NKEvent == Events.ItemDocumentJobFinalisedCode).Single().SL_Reference);
		}

		#endregion

		#region TestPropagationTargets_WithInvalidParent

		public void TestPropagationTargets_WithInvalidParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertEquals("Precondition: Initial Transfer is transferring correct stock.", 1, initialTransfer.Lines.Count);
			Factory.Save();

			var log = initialTransfer.Logs.AddNew(Events.ItemDocumentJobFinalised);
			AssertEquals("Transfer is not finalised.", 0, new WhsTransferProcessHandlingInfo(initialTransfer).GetPropagationTargets(log).Count());

			initialTransfer.WD_DocketStatus = DocketStatus.Codes.Finalised;
			vasOrder.WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
			AssertEquals("No VAS Order attached.", 0, new WhsTransferProcessHandlingInfo(initialTransfer).GetPropagationTargets(log).Count());

			vasOrder.WVO_WD_TransferIntoServiceArea = initialTransfer.PK;
			initialTransfer.Delete();
			AssertEquals("Transfer is deleted.", 0, new WhsTransferProcessHandlingInfo(initialTransfer).GetPropagationTargets(log).Count());
		}

		#endregion
	}
}
