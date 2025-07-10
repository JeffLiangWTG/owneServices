using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketLineProcessHandlingInfoTest : WhsTestCaseWithFactory
	{
		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var handlingInfo = new WhsDocketLineProcessHandlingInfo(receive.Lines[0]);
			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ArrivalCode;
			}

			AssertEquals(Enumerable.Empty<CascadingLink>(), handlingInfo.GetCascadingTargets(eventLog));
		}

		#endregion

		#region TestPropagationTargets

		public void TestPropagationTargets()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);

			var receiveWorkflowItemNonPropagated = receive.WorkflowItems.AddNew();
			receiveWorkflowItemNonPropagated.P9_Type = Constants.Workflow.WorkflowTriggerType;
			receiveWorkflowItemNonPropagated.TriggerConditions.TriggerEventCode = Events.ServiceCommencedCode;

			var receiveWorkflowItemPropagated = receive.WorkflowItems.AddNew();
			receiveWorkflowItemPropagated.P9_Type = Constants.Workflow.WorkflowTriggerType;
			receiveWorkflowItemPropagated.TriggerConditions.TriggerEventCode = Events.ChangeOfIdentifierCode;

			transferLine.Logs.AddNew(Events.ServiceCommenced);
			AssertEquals("Should not propagate Event if it is not Hold Code Change event.", true, receiveWorkflowItemNonPropagated.P9_ActualDate.IsEmpty);

			var logAdded = transferLine.Logs.AddNew(Events.ChangeOfIdentifier,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode));
			AssertEquals("Inventory should propagate hold code change event to the original docket.", logAdded.SL_EventTime, receiveWorkflowItemPropagated.P9_ActualDate.ToZDateTime());
		}

		#endregion

		#region TestPropagationTargets_WithInvalidParent

		public void TestPropagationTargets_WithInvalidParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);
			var receiveLine = receive.Lines[0];
			Factory.Save();

			var log = receiveLine.Logs.AddNew(Events.ChangeOfIdentifier,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode));
			receiveLine.WE_WE_OriginalDocketLineForRating = ZGuid.Empty;
			receiveLine.WE_WD = ZGuid.Empty;

			// no docket
			AssertEquals("No Original Docket.", 0, new WhsDocketLineProcessHandlingInfo(receiveLine).GetPropagationTargets(log).Count());

			receiveLine.WE_WE_OriginalDocketLineForRating = (ZGuid)receiveLine.WE_WE_OriginalDocketLineForRatingInfo.OriginalValue;
			receiveLine.WE_WD = (ZGuid)receiveLine.WE_WDInfo.OriginalValue;

			// deleted inventory
			receiveLine.Delete();
			AssertEquals("Inventory is deleted.", 0, new WhsDocketLineProcessHandlingInfo(receiveLine).GetPropagationTargets(log).Count());
		}

		#endregion
	}
}
