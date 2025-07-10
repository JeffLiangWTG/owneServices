using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskEstimateLogsTest : TestCaseWithFactory
	{
		(ProcessHeader, ProcessTask[]) CreateFourTasksWithEstimates()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var job = newFactory.New<DummyWithWorkflow>();
			var workflow = ProcessJobHeader.GetForParent(job, newFactory).ProcessHeaders.AddNew();

			var task1 = workflow.Parent.WorkflowItems.Tasks.AddNew();
			var task2 = workflow.Parent.WorkflowItems.Tasks.AddNew();
			var task3 = workflow.Parent.WorkflowItems.Tasks.AddNew();
			var task4 = workflow.Parent.WorkflowItems.Tasks.AddNew();

			newFactory.Save();

			workflow = Factory.Load<ProcessHeader>(workflow.PK);
			workflow.FH_CompletionStatement = "workflow";

			task1 = Factory.Load<ProcessTask>(task1.PK);
			task1.P9_FH_ProcessHeader = workflow.PK;
			task1.P9_Type = "IDK";
			task1.P9_Status = "ASN";
			task1.P9_EstDuration = new ZDateTime(2000, 1, 1, 1, 0, 0);
			task1.P9_EstimateVariationFactor = 1.0;
			task1.P9_GS_NKAssignedStaffMember = "ABC";

			task2 = Factory.Load<ProcessTask>(task2.PK);
			task2.P9_FH_ProcessHeader = workflow.PK;
			task2.P9_Type = "IDK";
			task2.P9_Status = "ASN";
			task2.P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			task2.P9_EstimateVariationFactor = 2.0;
			task2.P9_GS_NKAssignedStaffMember = "DEF";

			task3 = Factory.Load<ProcessTask>(task3.PK);
			task3.P9_FH_ProcessHeader = workflow.PK;
			task3.P9_Type = "AFK";
			task3.P9_Status = "ASN";
			task3.P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			task3.P9_EstimateVariationFactor = 3.0;
			task3.P9_GS_NKAssignedStaffMember = "GHI";

			task4 = Factory.Load<ProcessTask>(task4.PK);
			task4.P9_FH_ProcessHeader = workflow.PK;
			task4.P9_Type = "AFK";
			task4.P9_Status = "ASN";
			task4.P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			task4.P9_EstimateVariationFactor = 4.0;
			task4.P9_GS_NKAssignedStaffMember = "JKL";

			WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			return (workflow, new[] { task1, task2, task3, task4 });
		}

		IEnumerable<IProcessEstimateLog> GetLogs(string tableCode, ZGuid parentId)
		{
			return EstimateLogs.Where(l => l.P9E_ParentTableCode == tableCode && l.P9E_ParentId == parentId);
		}

		IProcessEstimateLog[] EstimateLogs
		{
			get
			{
				if (estimateLogs == null)
				{
					estimateLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentTableCode,
						new[] { ProcessTasksSchema.Constants.Prefix, ProcessHeaderSchema.Constants.Prefix }));
				}

				return estimateLogs;
			}
		}
		IProcessEstimateLog[] estimateLogs;

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestProcessTaskOnSavingThatEstimateLogIsCreatedCorrectly()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(1, taskLogs.Count());

			AssertTaskLog(taskLogs.Single(), tasks[0].PK, TestDateAttribute.Date, "ABC", 0, 0, 60, 60, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(1, taskLogs.Count());

			AssertTaskLog(taskLogs.Single(), tasks[1].PK, TestDateAttribute.Date, "DEF", 0, 0, 120, 240, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(1, taskLogs.Count());

			AssertTaskLog(taskLogs.Single(), tasks[2].PK, TestDateAttribute.Date, "GHI", 0, 0, 180, 540, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(1, taskLogs.Count());

			AssertTaskLog(taskLogs.Single(), tasks[3].PK, TestDateAttribute.Date, "JKL", 0, 0, 240, 960, false, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(1, workflowLogs.Count());
			AssertWorkflowLog(workflowLogs.Single(), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 0, 0, 600, 1800, false, false);

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.LoadTop1<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflow.PK));
			Factory.Save();

			estimateLogs = null;

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, false, false);

			workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, false, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestDoNotCreateProcessHeaderLog_WhenPreviousEstimateIsSameAsNewEstimate()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			tasks[0].P9_EstimateVariationFactor = 1.0d;
			tasks[1].P9_EstimateVariationFactor = 1.0d;
			tasks[2].P9_EstimateVariationFactor = 1.0d;
			tasks[3].P9_EstimateVariationFactor = 1.0d;

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 180, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 240, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 300, false, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 840, false, false);

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);

			var date = TestDateAttribute.Date;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2);

			Factory.Save();

			estimateLogs = null;

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(3, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, date, "ABC", 60, 60, 120, 120, false, false);
			AssertTaskLog(orderTasks[2], tasks[0].PK, TestDateAttribute.Date, "ABC", 120, 120, 300, 300, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(3, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, date, "DEF", 120, 240, 180, 180, false, false);
			AssertTaskLog(orderTasks[2], tasks[1].PK, TestDateAttribute.Date, "DEF", 180, 180, 240, 240, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(3, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, date, "GHI", 180, 540, 240, 240, false, false);
			AssertTaskLog(orderTasks[2], tasks[2].PK, TestDateAttribute.Date, "GHI", 240, 240, 180, 180, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(3, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, date, "JKL", 240, 960, 300, 300, false, false);
			AssertTaskLog(orderTasks[2], tasks[3].PK, TestDateAttribute.Date, "JKL", 300, 300, 120, 120, false, false);

			workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals("No need to create another workflow log since the previous aggregate estimate is the same as the current estimate", 2, workflowLogs.Count());
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsFalse_AnyTaskActualDurationIsNotEmptyIsFalse_TaskStatusIsWorkingOrSuspendedOrClosedIsFalse()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, false, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, false, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsFalse_AnyTaskActualDurationIsNotEmptyIsFalse_TaskStatusIsWorkingOrSuspendedorClosedIsTrue()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			tasks[0].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[1].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[3].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);

			tasks[0].P9_Status = "WRK";
			tasks[1].P9_Status = "SUS";
			tasks[2].P9_Status = "SUS";
			tasks[3].P9_Status = "WRK";

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, false, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, false, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsFalse_AnyTaskActualDurationIsNotEmptyIsTrue_TaskStatusIsWorkingOrSuspendedOrClosedIsFalse()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			tasks[0].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[1].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[3].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, false, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, false, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsFalse_AnyTaskActualDurationIsNotEmptyIsTrue_TaskStatusIsWorkingOrSuspendedOrClosedIsTrue()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			tasks[0].P9_ActualDuration = new ZDateTime(2000, 1, 1, 1, 0, 0);
			tasks[1].P9_ActualDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[2].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[3].P9_ActualDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);

			tasks[0].P9_Status = "WRK";
			tasks[1].P9_Status = "SUS";
			tasks[2].P9_Status = "WRK";
			tasks[3].P9_Status = "SUS";

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, false, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, false, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsTrue_AnyTaskActualDurationIsNotEmptyIsFalse_TaskStatusIsWorkingOrSuspendedorClosedIsFalse()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			SetIsWorkProduction();

			tasks[0].P9_ActualDuration = ZDateTime.Empty;
			tasks[1].P9_ActualDuration = ZDateTime.Empty;
			tasks[2].P9_ActualDuration = ZDateTime.Empty;
			tasks[3].P9_ActualDuration = ZDateTime.Empty;

			tasks[0].P9_Status = "ASN";
			tasks[1].P9_Status = "ASN";
			tasks[2].P9_Status = "ASN";
			tasks[3].P9_Status = "ASN";

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, false, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, false, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, false, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsTrue_AnyTaskActualDurationIsNotEmptyIsFalse_TaskStatusIsWorkingOrSuspendedorClosedIsTrue()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			SetIsWorkProduction();

			tasks[0].P9_ActualDuration = ZDateTime.Empty;
			tasks[1].P9_ActualDuration = ZDateTime.Empty;
			tasks[2].P9_ActualDuration = ZDateTime.Empty;
			tasks[3].P9_ActualDuration = ZDateTime.Empty;

			tasks[0].P9_Status = "WRK";
			tasks[1].P9_Status = "SUS";
			tasks[2].P9_Status = "WRK";
			tasks[3].P9_Status = "SUS";

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, true, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, true, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsTrue_AnyTaskActualDurationIsNotEmptyIsTrue_TaskStatusIsWorkingOrSuspendedorClosedIsFalse()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			SetIsWorkProduction();

			tasks[0].P9_ActualDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_ActualDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_ActualDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			tasks[0].P9_Status = "ASN";
			tasks[1].P9_Status = "ASN";
			tasks[2].P9_Status = "ASN";
			tasks[3].P9_Status = "ASN";

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, true, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, true, false);
		}

		[TestDate(2019, 10, 9, 9, 0, 0)]
		public void TestHasWorkStartedIsFalse_WhenIsWorkProductionIsTrue_AnyTaskActualDurationIsNotEmptyIsTrue_TaskStatusIsWorkingOrSuspendedorClosedIsTrue()
		{
			var collection = CreateFourTasksWithEstimates();
			var workflow = collection.Item1;
			var tasks = collection.Item2;
			Factory.Save();

			var previousEstimates = tasks.ToDictionary(t => t.PK, t => new
			{
				PreviousLowEstimatedDurationMinutes = Convert.ToInt32(t.LowEstimatedDurationHours * 60),
				PreviousHighEstimatedDurationMinutes = Convert.ToInt32(t.HighEstimatedDurationHours * 60),
			});

			tasks[0].P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[1].P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[2].P9_EstDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);
			tasks[3].P9_EstDuration = new ZDateTime(2000, 1, 1, 5, 0, 0);

			SetIsWorkProduction();

			tasks[0].P9_ActualDuration = new ZDateTime(2000, 1, 1, 1, 0, 0);
			tasks[1].P9_ActualDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			tasks[2].P9_ActualDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			tasks[3].P9_ActualDuration = new ZDateTime(2000, 1, 1, 4, 0, 0);

			tasks[0].P9_Status = "WRK";
			tasks[1].P9_Status = "SUS";
			tasks[2].P9_Status = "CLS";
			tasks[3].P9_Status = "CLS";

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			Factory.Save();

			var taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[0].PK);
			AssertEquals(2, taskLogs.Count());

			var orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[0].PK, TestDateAttribute.Date, "ABC", 60, 60, 120, 120, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[1].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[1].PK, TestDateAttribute.Date, "DEF", 120, 240, 180, 360, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[2].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[2].PK, TestDateAttribute.Date, "GHI", 180, 540, 240, 720, true, false);

			taskLogs = GetLogs(ProcessTasksSchema.Constants.Prefix, tasks[3].PK);
			AssertEquals(2, taskLogs.Count());

			orderTasks = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray();
			AssertTaskLog(orderTasks[1], tasks[3].PK, TestDateAttribute.Date, "JKL", 240, 960, 300, 1200, true, false);

			var workflowLogs = GetLogs(ProcessHeaderSchema.Constants.Prefix, workflow.PK);
			AssertEquals(2, workflowLogs.Count());

			AssertWorkflowLog(workflowLogs.Single(l => l.P9E_LogDateTime.ToDateTime() == TestDateAttribute.Date), workflow.PK, TestDateAttribute.Date, GlbStaff.CurrentUser.GS_Code, 600, 1800, 840, 2400, true, false);
		}

		public void TestPartialWorkflowTemplateTaskAndWorkflowEstimatesAreLogged()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_Name = "Added by Trigger";
			template.P0_IsPartialTemplate = true;
			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Workflow";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;
			templateTask.P9_Description = "Task 1";
			templateTask.P9_EstDuration = new ZDateTime(2000, 1, 1, 3, 0, 0);
			templateTask.P9_EstimateVariationFactor = 2.0;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var dummy = (DummyWithWorkflow)jobHeader.Parent;
			var dummyWorkflow = jobHeader.ProcessHeaders.AddNew();
			dummyWorkflow.FH_CompletionStatement = "Workflow";

			Factory.Save();

			var taskLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, templateTask.PK));
			AssertEquals(0, taskLogs.Length);

			var templateWorkflowLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, templateWorkflow.PK));
			AssertEquals(0, templateWorkflowLogs.Length);

			var workflowLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, dummyWorkflow.PK));
			AssertEquals(0, workflowLogs.Length);

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = template.PK;

			dummy.Logs.AddNew(Events.CustomsEntryStatus);
			new WorkflowApplyTemplateProcessor(action, dummy).Process(new NotificationsForTest());

			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Tasks.Count);

			var processTask = dummy.WorkflowItems.Tasks.First() as ProcessTask;

			taskLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, processTask.PK));

			var firstLowEstimate = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray()[0].P9E_NewLowEstimateMinutes / 60;
			var firstHighEstimate = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray()[0].P9E_NewHighEstimateMinutes / 60;

			AssertEquals("ProcessEstimateLog was not created correctly.", templateTask.LowEstimatedDurationHours, (ZDecimal)firstLowEstimate);
			AssertEquals("ProcessEstimateLog was not created correctly.", templateTask.HighEstimatedDurationHours, (ZDecimal)firstHighEstimate);

			templateWorkflowLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, templateWorkflow.PK));
			AssertEquals(0, templateWorkflowLogs.Length);

			workflowLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, dummyWorkflow.PK));
			AssertEquals(1, workflowLogs.Length);

			processTask.Delete();

			templateTask.P9_EstDuration = new ZDateTime(2000, 1, 1, 2, 0, 0);
			templateTask.P9_EstimateVariationFactor = 4.0;

			dummy.Logs.AddNew(Events.CustomsEntryStatus);
			new WorkflowApplyTemplateProcessor(action, dummy).Process(new NotificationsForTest());

			Factory.Save();

			processTask = dummy.WorkflowItems.Tasks.First() as ProcessTask;

			taskLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, processTask.PK));

			var secondLowEstimate = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray()[0].P9E_NewLowEstimateMinutes / 60;
			var secondHighEstimate = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray()[0].P9E_NewHighEstimateMinutes / 60;

			AssertEquals("ProcessEstimateLog was not created correctly.", templateTask.LowEstimatedDurationHours, (ZDecimal)secondLowEstimate);
			AssertEquals("ProcessEstimateLog was not created correctly.", templateTask.HighEstimatedDurationHours, (ZDecimal)secondHighEstimate);

			templateWorkflowLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, templateWorkflow.PK));
			AssertEquals(0, templateWorkflowLogs.Length);

			workflowLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, dummyWorkflow.PK));
			AssertEquals(2, workflowLogs.Length);
		}

		public void TestTaskEstimatesRegistryIsOffAndChangesAreNotLogged()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var registryStatus = WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.Value;

			if (!registryStatus)
			{
				var helper = ObjectFactory.Get<IBMTestHelper>();
				helper.EnableBMSInRegistry();
				helper.CreateSystem(Factory, "DUM");

				var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory);
				var dummy = (DummyWithWorkflow)jobHeader.Parent;
				var dummyWorkflow = jobHeader.ProcessHeaders.AddNew();
				dummyWorkflow.FH_CompletionStatement = "Workflow";

				var task1 = dummy.WorkflowItems.AddNew();
				task1.P9_Type = "IDK";
				task1.P9_Status = "ASN";
				task1.P9_EstDuration = new ZDateTime(2000, 1, 1, 1, 0, 0);
				task1.P9_EstimateVariationFactor = 2.0;
				task1.P9_FH_ProcessHeader = dummyWorkflow.PK;

				Factory.Save();

				var taskLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, task1.PK));

				var noLowTaskEstimate = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray().IsNullOrEmpty();
				var noHighTaskEstimate = taskLogs.OrderBy(l => l.P9E_LogDateTime).ToArray().IsNullOrEmpty();

				AssertEquals("ProcessEstimateLog was not created correctly.", true, noLowTaskEstimate);
				AssertEquals("ProcessEstimateLog was not created correctly.", true, noHighTaskEstimate);

				var workflowLogs = Factory.Load<IProcessEstimateLog>(new ZQuery(ProcessEstimateLogSchema.P9E_ParentId, task1.ProcessHeader.PK));

				var noLowWorkflowEstimate = workflowLogs.OrderBy(l => l.P9E_LogDateTime).ToArray().IsNullOrEmpty();
				var noHighWorkflowEstimate = workflowLogs.OrderBy(l => l.P9E_LogDateTime).ToArray().IsNullOrEmpty();

				AssertEquals("ProcessEstimateLog was not created correctly.", true, noLowWorkflowEstimate);
				AssertEquals("ProcessEstimateLog was not created correctly.", true, noHighWorkflowEstimate);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		void AssertTaskLog(IProcessEstimateLog log, ZGuid taskPK, ZDateTimeOffset logDate, ZString staffCode,
			ZInt expectedPreviousLowEstimateMinutes, ZInt expectedPreviousHighEstimateMinutes, int previousNewLowEstimateMinutes, int previousNewHighEstimateMinutes,
			bool hasWorkStarted, bool wasWorkPreviouslyStarted)
		{
			AssertLog(log, taskPK, "P9", logDate, staffCode,
				expectedPreviousLowEstimateMinutes, expectedPreviousHighEstimateMinutes, previousNewLowEstimateMinutes, previousNewHighEstimateMinutes,
				hasWorkStarted, wasWorkPreviouslyStarted);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		void AssertWorkflowLog(IProcessEstimateLog log, ZGuid workflowPK, ZDateTimeOffset logDate, ZString staffCode,
			ZInt? expectedPreviousLowEstimateMinutes, ZInt? expectedPreviousHighEstimateMinutes, int newLowEstimateMinutes, int newHighEstimateMinutes,
			bool hasWorkStarted, bool wasWorkPreviouslyStarted)
		{
			AssertLog(log, workflowPK, "FH", logDate, staffCode,
				expectedPreviousLowEstimateMinutes, expectedPreviousHighEstimateMinutes, newLowEstimateMinutes, newHighEstimateMinutes,
				hasWorkStarted, wasWorkPreviouslyStarted);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		void AssertLog(IProcessEstimateLog log, ZGuid taskOrWorkflowPK, ZString parentTableCode, ZDateTimeOffset logDate, ZString staffCode,
			ZInt? expectedPreviousLowEstimateMinutes, ZInt? expectedPreviousHighEstimateMinutes, int newLowEstimateMinutes, int newHighEstimateMinutes,
			bool hasWorkStarted, bool wasWorkPreviouslyStarted)
		{
			AssertEquals(taskOrWorkflowPK, log.P9E_ParentId);
			AssertEquals(parentTableCode, log.P9E_ParentTableCode);
			AssertEquals(logDate, log.P9E_LogDateTime);
			AssertEquals(staffCode, log.P9E_GS_NKUser);
			AssertEquals(expectedPreviousLowEstimateMinutes, log.P9E_PreviousLowEstimateMinutes);
			AssertEquals(expectedPreviousHighEstimateMinutes, log.P9E_PreviousHighEstimateMinutes);
			AssertEquals(newLowEstimateMinutes, log.P9E_NewLowEstimateMinutes);
			AssertEquals(newHighEstimateMinutes, log.P9E_NewHighEstimateMinutes);
			AssertEquals(hasWorkStarted, log.P9E_HasWorkStarted);
			AssertEquals(wasWorkPreviouslyStarted, log.P9E_WasWorkPreviouslyStarted);
		}

		#region Setup

		WorkflowTaskType IDK_WorkflowTaskType;
		WorkflowTaskType AFK_WorkflowTaskType;

		protected override void SetUp()
		{
			base.SetUp();

			_ = DummyWorkflowDescriptor.Instance;
			WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var workflowTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;

			var categorisedWorkflowTypes = workflowTypes.Cast<CategorisedWorkflowTaskTypes>().First(w => w.Code == "DUM");
			var categorisedWorkflowTaskTypes = categorisedWorkflowTypes.TaskTypes;

			IDK_WorkflowTaskType = categorisedWorkflowTaskTypes.AddNew();
			IDK_WorkflowTaskType.IsWorkProduction = false;
			IDK_WorkflowTaskType.Code = "IDK";

			AFK_WorkflowTaskType = categorisedWorkflowTaskTypes.AddNew();
			AFK_WorkflowTaskType.IsWorkProduction = false;
			AFK_WorkflowTaskType.Code = "AFK";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, workflowTypes);
		}

		void SetIsWorkProduction(bool value = true)
		{
			var workflowTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var categorisedWorkflowTaskTypes = workflowTypes.Cast<CategorisedWorkflowTaskTypes>();
			var categorisedWorkflowType = categorisedWorkflowTaskTypes.First(w => w.Code == "DUM");

			foreach (WorkflowTaskType taskType in categorisedWorkflowType.TaskTypes)
			{
				taskType.IsWorkProduction = value;
			}

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, workflowTypes);
		}

		#endregion
	}
}
