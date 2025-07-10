using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalTaskWriterTest : TestCaseWithFactory
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2018, 8, 3)]
		public void TestPopulateTasks()
		{
			TestDateAttribute.UseUNLOCO = true;
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var job = (BusinessObject)Factory.New<IWorkItem>();
			job.FillWithValidTestData();
			var task = ((IWorkflowProvider)job).WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Be exported";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.P9_EstDuration = new ZDateTime(2018, 1, 1, 0, 1, 0);
			task.P9_Sequence = 2;
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_GG_AssignedGroup = group.PK;
			task.P9_EstimateVariationFactor = 2;
			task.P9_NotesAsString = "That's my note.";
			task.P9_CardNote = "READY TO START";
			task.P9_ScheduledDateUtc = ZDateTime.UtcNow.AddDays(2);
			task.P9_ActualDateUtc = ZDateTime.UtcNow.AddDays(3);

			Factory.Save();

			TestDateAttribute.AddMinutes(10);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var taskSet = new TaskSet(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNull(taskSet.TaskCollection);

			var writer = new UniversalTaskWriter();
			writer.PopulateTasks(((IWorkflowProvider)job).WorkflowItems.Tasks.Cast<ProcessTask>(), taskSet);

			AssertNotNull(taskSet.TaskCollection);
			var activityTask = taskSet.TaskCollection.Single();

			AssertEquals(task.P9_TaskID, activityTask.TaskID);
			AssertEquals(2, activityTask.Sequence);
			AssertEquals("Be exported", activityTask.Description);
			AssertEquals("That's my note.", activityTask.TaskNotes);
			AssertEquals("READY TO START", activityTask.CardNote);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, activityTask.Status?.Code);
			AssertEquals("UDF", activityTask.Type?.Code);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, activityTask.AssignedStaff?.Code);
			AssertEquals(capability.G4_Code, activityTask.AssignedCapability?.Code);
			AssertEquals(group.GG_Code, activityTask.AssignedGroup?.Code);
			AssertEquals(task.P9_EstDuration.ToTimeSpan(), activityTask.EstimatedDuration);
			AssertEquals(2m, activityTask.EstimateVariationFactor);
			AssertEquals(task.P9_ActualDuration.ToTimeSpan(), activityTask.ActualDuration);
			AssertEquals(ZDateTime.UtcNow, activityTask.CompletedTimeUTC.Value.ToZDateTime());
			AssertEquals("UTC date so should have zero offset", TimeSpan.Zero, activityTask.CompletedTimeUTC.Value.Offset);
			AssertEquals(task.P9_ScheduledDateUtc, activityTask.EstimatedStartTimeUTC.Value.ToZDateTime());
			AssertEquals("UTC date so should have zero offset", TimeSpan.Zero, activityTask.EstimatedStartTimeUTC.Value.Offset);
			AssertEquals(task.P9_ActualDateUtc, activityTask.ActualStartTimeUTC.Value.ToZDateTime());
			AssertEquals("UTC date so should have zero offset", TimeSpan.Zero, activityTask.ActualStartTimeUTC.Value.Offset);
		}

		public void TestPopulateTasks_WithEmptyDurations()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();
			job.FillWithValidTestData();
			var task = ((IWorkflowProvider)job).WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Sell fake doors!!!!!";

			Factory.Save();

			var taskSet = new TaskSet(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskWriter();
			writer.PopulateTasks(((IWorkflowProvider)job).WorkflowItems.Tasks.Cast<ProcessTask>(), taskSet);

			var activitTask = taskSet.TaskCollection.Single();
			AssertEquals(TimeSpan.Zero, activitTask.ActualDuration);
			AssertEquals(TimeSpan.Zero, activitTask.EstimatedDuration);
		}
	}
}
