using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	public class XudTriggerActionValidatorTest : TestCaseWithFactory
	{
		public void TestXudWithoutFiringEventIsInvalid()
		{
			var job = Factory.New<IForwardingShipment>();
			job.JS_UniqueConsignRef = "S001";

			var workflowProvider = job as IWorkflowProvider;
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;

			var validator = new XudTriggerActionValidator();
			var logger = new TestLogger();
			var result = validator.IsValid(trigger, action, null, null, trigger.GetWorkflowDescriptor(), job as BusinessObject, logger);

			AssertEquals("Trigger action should be false", false, result);
			AssertContains("Could not load triggering event", logger.GetAllLogsAsString());
		}
	}
}
