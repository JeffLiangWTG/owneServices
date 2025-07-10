using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItem))]
	public class WorkItemWorkflowProviderTest : WorkflowProviderTest<WorkItem, WorkItemProcessTaskCollection>
	{
		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWorkflowFieldChangeTriggersWork()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory, "Hi, I'd like to order one large sofa chair with extra chair");
			var trigger = MasterFilesTestHelper.CreateTrigger(workItem, eventCode: null, triggerFieldName: WorkItemSchema.Constants.WKI_Summary);

			Factory.Save();
			MasterFilesTestHelper.RunFieldChangeTriggerProcessorServiceTask();

			workItem.WKI_Summary = "High chair. Nononono recliner!";
			Factory.Save();

			TestDateAttribute.AddMinutes(1); // Makes the edit log in range of what the service task will consider
			MasterFilesTestHelper.RunFieldChangeTriggerProcessorServiceTask();

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			trigger.Reload();

			AssertEquals(ZDateTime.Now.AddMinutes(-1), trigger.P9_ActualDate);
		}

		protected override ZString ExpectedWorkflowType => JobInvoicingConsumerTypes.WorkItem.Code;
	}
}
