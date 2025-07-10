using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(QuotationWorkflowDescriptor))]
	public class QuotationWorkflowDescriptorTest : RatingHeaderWorkflowDescriptorTest<Quote, QuotationWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.QuotationWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Quotation", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public void TestGetWorkflowTriggerAction()
		{
			var quote = Factory.New<Quote>();
			ProcessTask processTask = quote.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification notification = processTask.ProcessTaskNotifications.AddNew();
			QuotationWorkflowDescriptor descriptor = new QuotationWorkflowDescriptor();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertEquals(typeof(XmlMessageDeliver), descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)).GetType());

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			WorkflowTriggerNotification workflowTriggerNotification = (WorkflowTriggerNotification)descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull(workflowTriggerNotification.ExtraDataSubstitution);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertNotNull(descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)));
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeRate;
	}
}
