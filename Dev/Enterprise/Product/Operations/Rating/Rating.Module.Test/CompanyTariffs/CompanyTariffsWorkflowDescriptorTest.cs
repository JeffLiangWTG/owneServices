using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CompanyTariffsWorkflowDescriptor))]
	public class CompanyTariffsWorkflowDescriptorTest : RatingHeaderWorkflowDescriptorTest<CompanyTariff, CompanyTariffsWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CompanyTariffsWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Company Tariff", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public void TestGetWorkflowTriggerAction()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			var processTask = companyTariff.WorkflowItems.Triggers.AddNew();
			processTask.P9_ParentID = companyTariff.PK;

			var notification = processTask.ProcessTaskNotifications.AddNew();
			var descriptor = new CompanyTariffsWorkflowDescriptor();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertEquals(typeof(XmlMessageDeliver), descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)).GetType());

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			var workflowTriggerNotification = (WorkflowTriggerNotification)descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull(workflowTriggerNotification.ExtraDataSubstitution);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertNotNull(descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)));
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeRate;
	}
}
