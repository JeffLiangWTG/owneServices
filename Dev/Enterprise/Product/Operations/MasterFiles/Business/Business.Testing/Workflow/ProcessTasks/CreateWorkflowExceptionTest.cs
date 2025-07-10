using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class CreateWorkflowExceptionTest : TestCaseWithFactory
	{
		[TestDate(2018, 7, 3)]
		public void TestThatExceptionsDoNotUseEventsThatDoNotMatchMilestone_Baseline()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Summer Time!";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone.TriggerConditions.TriggerConditionValue = "And the living is easy";
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(-2);

			Factory.Save();
			var exception = (ProcessTask)dummy.WorkflowItems.Exceptions.SingleOrDefault();
			AssertEquals("The sun is high", ZDateTime.Now, exception.ExceptionProperties.ActualDate);
		}

		[TestDate(2018, 7, 3)]
		public void TestThatExceptionsDoNotUseEventsThatDoNotMatchMilestone()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "And the cotton is warm";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone.TriggerConditions.TriggerConditionValue = "Your daddies rich";
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(-2);
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00, "And your mamas good looking", ZDateTimeOffset.Now.AddDays(24));

			Factory.Save();
			var exception = (ProcessTask)dummy.WorkflowItems.Exceptions.SingleOrDefault();
			AssertEquals("So c'mon little baby don't you cry", ZDateTime.Now, exception.ExceptionProperties.ActualDate);
		}
	}
}
