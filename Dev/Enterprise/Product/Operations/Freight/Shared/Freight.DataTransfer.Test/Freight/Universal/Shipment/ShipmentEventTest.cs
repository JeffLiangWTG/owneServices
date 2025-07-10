using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	internal class ShipmentEventTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPackLineTriggerCanFindContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.FillWithValidTestData();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA";
			container.JC_ContainerJobID = "D000015";

			var shipment = Factory.New<ForwardingShipment>();

			shipment.Consols.Add(consol);
			shipment.FillWithValidTestData();
			shipment.OuterPackLines.AddNew();

			Factory.SaveForTesting();

			var trigger = shipment.WorkflowItems.AddNew();
			trigger.P9_Description = "test";
			trigger.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<OuterPackLines[1].ConsolOrShipmentContainer.JC_ContainerNum>\" == \"AAAA\"";
			trigger.TriggerConditions.TriggerFiredCountdown = 10;

			Factory.SaveForTesting();

			AssertEquals("Trigger has unexpected countdown value", (short)10, trigger.P9_TriggerFiredCountdown);

			shipment.Logs.AddNew(Events.CustomisableEvent00);

			AssertEquals("Trigger has not been fired", (short)9, trigger.P9_TriggerFiredCountdown);
		}
	}
}
