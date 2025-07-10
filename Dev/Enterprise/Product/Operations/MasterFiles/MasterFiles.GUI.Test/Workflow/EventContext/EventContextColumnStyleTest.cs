using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EventContextColumnStyleTest : TestCaseWithFactory
	{
		public void TestEditControl()
		{
			using (var columnStyle = new EventContextColumnStyle(new EventContextColumnStyleInfo()))
			{
				AssertEquals(typeof(EventContextFindBox), columnStyle.EditControl.GetType());
			}
		}

		public void TestGetWorkflowDescriptor()
		{
			using (var columnStyle = new EventContextColumnStyle(new EventContextColumnStyleInfo()))
			{
				AssertNull(columnStyle.GetWorkflowDescriptor(this));

				var dummy = Factory.New<DummyWithWorkflow>();
				var trigger = dummy.WorkflowItems.Triggers.AddNew();
				AssertEquals(DummyWorkflowDescriptor.Instance.Code, columnStyle.GetWorkflowDescriptor(trigger).Code);
			}
		}

		public void TestGetWorkflowDescriptor_ForTemplateTrigger()
		{
			using (var columnStyle = new EventContextColumnStyle(new EventContextColumnStyleInfo()))
			{
				AssertNull(columnStyle.GetWorkflowDescriptor(this));

				var template = Factory.New<ProcessTaskTemplate>();
				template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

				var trigger = template.TemplateTriggers.AddNew();
				AssertEquals(DummyWorkflowDescriptor.Instance.Code, columnStyle.GetWorkflowDescriptor(trigger).Code);
			}
		}
	}
}
