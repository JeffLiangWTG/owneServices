using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveHeaderProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestMoveHeaderEventsCascadeToHeaderAndHeaderParent()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var trigger = shipment.WorkflowItems.AddNew();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			trigger.P9_RespondToCascadedEvents = true;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_EmailAddr = "abc@cargowise.com";
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			var trigger1 = header.WorkflowItems.AddNew();
			trigger1.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger1.TriggerConditions.TriggerEventCode = Events.MessageStatusChangeCode;
			trigger1.P9_RespondToCascadedEvents = true;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action1.PQ_EmailAddr = "abc@cargowise.com";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.FillWithValidTestData();
			Factory.Save();
			moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			Assert(!trigger.P9_ActualDate.IsEmpty);
			Assert(!trigger1.P9_ActualDate.IsEmpty);
		}
	}
}
