using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskNotificationCollection))]
	sealed class ProcessTaskNotificationCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTaskNotificationCollection>
	{
		public void TestContainsTriggerType()
		{
			OrgOpportunity opp = Factory.NewWithValidTestData<OrgOpportunity>();
			ProcessTask task = opp.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotificationCollection collection = new ProcessTaskNotificationCollection(task);

			ProcessTaskNotification notification1 = collection.AddNew();
			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			ProcessTaskNotification notification2 = collection.AddNew();
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			ProcessTaskNotification notification3 = collection.AddNew();
			notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			AssertEquals(true, collection.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail));
			AssertEquals(true, collection.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendDocument));
			AssertEquals(true, collection.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXML));
			AssertEquals(false, collection.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback));
			AssertEquals(false, collection.ContainsTriggerType("some string"));
		}

		protected override ProcessTaskNotificationCollection GetCollectionToTest()
		{
			var trigger = Factory.New<DummyWithWorkflow>().WorkflowItems.Triggers.AddNew();
			return new ProcessTaskNotificationCollection(trigger);
		}

		public void TestAllowNewIsDrivenByTaskReadOnlyTrue()
		{
			AssertAllowNewIsDrivenByTaskReadOnly(true);
		}

		public void TestAllowNewIsDrivenByTaskReadOnlyFalse()
		{
			AssertAllowNewIsDrivenByTaskReadOnly(false);
		}

		void AssertAllowNewIsDrivenByTaskReadOnly(bool taskReadOnly)
		{
			var task = Factory.New<ProcessTask>();
			task.ReadOnly = taskReadOnly;

			var collection = new ProcessTaskNotificationCollection(task);
			AssertEquals(!taskReadOnly, ((IBindingList)collection).AllowNew);
		}
	}
}
