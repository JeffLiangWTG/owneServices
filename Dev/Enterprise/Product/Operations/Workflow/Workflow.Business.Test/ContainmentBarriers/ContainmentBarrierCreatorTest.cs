using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	public class ContainmentBarrierCreatorTest : WorkflowTestCase
	{
		ProcessTask task1, task2, task3;
		IProcessHeader workflow;
		ZGuid reasonPK;
		ContainmentBarrierCreator creator;
		GlbStaff theAbc;

		public void TestHandleConcurrencyErrors()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var tasks = factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, task1.P9_ParentID));
			tasks.ForEach(t => t.P9_Description = t.P9_Description + "Nnng");

			var factories = new[] { factory, new BusinessObjectFactory() };
			var fIndex = 0;
			var barrierCreator = new ContainmentBarrierCreator(null, () => factories[fIndex++]);
			task1.P9_Description = "GAAAA";
			Factory.Save();
			AssertNoExceptionThrown(() => barrierCreator.CreateQualityIteration(task3.PK, task1.PK, reasonPK));
		}

		public void TestFindBestIterateFromTask_WhenTaskNotMarkedAsContainmentBarrier()
		{
			task3.P9_Type = "MAL";
			Factory.Save();

			var iterateFromTaskPK = creator.FindBestIterateFromTask(task3.PK, ContainmentBarrierIterateFromTaskSelectionMode.None, new string[] { task1.P9_Type, task2.P9_Type });

			AssertEquals("If the task provided to the interface isn't a containment barrier, don't explode, just return an empty GUID so consumers can take alternative actions (like not creating a QI)", ZGuid.Empty, iterateFromTaskPK);
		}

		public void TestCreateQualityIteration_WithDefaultValues()
		{
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			Assert(task3.P9_Status == "CLS");
			AssertEquals(1, workflow.Links.Count());

			var link = workflow.Links.First();
			var iteration = link.HeaderFrom;

			Assert(iteration.FH_CompletionStatement.Contains("Quality Iteration"));

			var query = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, SQLComparisonOperator.Equal, iteration.PK);
			var newTasks = Factory.Load<ProcessTask>(query);

			AssertEquals(3, newTasks.Length);
			Assert(newTasks.All(x => x.P9_Status == "ASN"));
		}

		[TestDate(2015, 8, 25, 10, 30, 0)]
		public void TestCreateQualityIteration_ShouldSetSpecifiedQcbTaskStatusAndNotes()
		{
			creator.QcbTaskNewStatus = ProcessTaskStatusCodeList.Codes.Cancelled;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);
			AssertEquals("A new status for the qcbTask was specified so the task's status should have been updated.", "CAN", task3.P9_Status);

			var expected = GlbStaff.CurrentUser.GS_Code + " 25-Aug-15 10:30: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: CRE - Wasn't creepy enough";
			AssertRtfText("The qcbTask's notes should not contain an extra line about the containment barrier being cancelled.", expected, task3.P9_Notes);
		}

		[TestDate(2015, 8, 25, 10, 30, 0)]
		public void TestCreateQualityIteration_ShouldActAsSpecifiedUser()
		{
			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var expected = "ABC 25-Aug-15 10:30: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: CRE - Wasn't creepy enough";
			AssertRtfText("The qcbTask's notes should contain a timestamp that refers to the specified user.", expected, task3.P9_Notes);
		}

		public void TestCreateQualityIteration_ShouldExcludeSpecifiedTaskTypes()
		{
			creator.TaskTypesToNotRepeat = new[] { "CDF" };
			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link = workflow.Links.First();
			var iteration = link.HeaderFrom;

			var query = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, SQLComparisonOperator.Equal, iteration.PK);
			var newTasks = Factory.Load<ProcessTask>(query);

			AssertEquals(2, newTasks.Length);
			Assert("There should not be a CDF task in the QI because it was excluded, and yet...", newTasks.All(x => x.P9_Type != "CDF"));
		}

		[GuiTest]
		public void TestCreateQualityIterationFromContainmentBarrier_TasksToIncludeShouldHaveGreaterSequenceNumbersThanTheIterateFromTask()
		{
			var goldy = Factory.NewWithValidTestData<GlbStaff>();
			goldy.GS_FullName = "Mr Goldenfold";
			var summer = Factory.NewWithValidTestData<GlbStaff>();
			summer.GS_FullName = "Summer Smith";
			var mrNeedful = Factory.NewWithValidTestData<GlbStaff>();
			mrNeedful.GS_Code = "DEV";
			mrNeedful.GS_LoginName = "Mr Needful";

			var doubleWorkflowJobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var myLustWorkflow = BMTestHelper.CreateWorkflow(doubleWorkflowJobHeader, "A price for everything");
			var myGreedWorkflow = BMTestHelper.CreateWorkflow(doubleWorkflowJobHeader, "This should counteract");
			myLustWorkflow.GetOrCreateDependencyLink(myGreedWorkflow);

			var lustTask1 = (ProcessTask)BMTestHelper.CreateTask(myLustWorkflow, goldy.GS_Code, 10, sequence: 1, description: "This aftershave made me irresistible", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDU");
			var lustTask2 = (ProcessTask)BMTestHelper.CreateTask(myLustWorkflow, goldy.GS_Code, 10, sequence: 2, description: "My lust! My greed!", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDF");
			var lustTask3 = (ProcessTask)BMTestHelper.CreateTask(myLustWorkflow, summer.GS_Code, 10, sequence: 3, description: "X gon give it to ya", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			var greedTask1 = (ProcessTask)BMTestHelper.CreateTask(myGreedWorkflow, mrNeedful.GS_Code, 10, sequence: 10, description: "I haven't learned a thing!", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			AssertEquals(4, doubleWorkflowJobHeader.Tasks.Count());

			Factory.Save();
			//Create quality iteration from base workflow
			creator.QcbCreatingUserLoginName = mrNeedful.GS_LoginName;
			creator.CreateQualityIteration(lustTask3.PK, lustTask1.PK, reasonPK);

			var link1 = myLustWorkflow.Links.Single(x => x.HeaderFrom != myLustWorkflow);
			var iteration1 = link1.HeaderFrom;

			var query = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, SQLComparisonOperator.Equal, iteration1.PK);
			var newTasks = Factory.Load<ProcessTask>(query);

			AssertEquals("A price for everything (Quality Iteration 1)", iteration1.FH_CompletionStatement);
			AssertEquals("Should have added 3 tasks from quality iteration", 3, newTasks.Length);

			ProcessTask codingTask = null;
			ProcessTask qcbTask = null;

			foreach (var task in newTasks)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				if (task.P9_Type == "CDF")
				{
					codingTask = task;
				}
				else if (task.P9_Type == "QCB")
				{
					qcbTask = task;
				}
			}

			AssertNotNull(codingTask);

			//Create quality iteration from first quality iteration
			creator.QcbCreatingUserLoginName = mrNeedful.GS_LoginName;
			creator.CreateQualityIteration(qcbTask.PK, codingTask.PK, reasonPK);

			var link2 = iteration1.Links.Single(x => x.HeaderFrom != iteration1);
			var iteration2 = link2.HeaderFrom;

			query = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, SQLComparisonOperator.Equal, iteration2.PK);
			var newTasks2 = Factory.Load<ProcessTask>(query);

			AssertEquals("A price for everything (Quality Iteration 2)", iteration2.FH_CompletionStatement);
			AssertEquals("Should have added 2 tasks from quality iteration", 2, newTasks2.Length);

			foreach (var task in newTasks2)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				if (task.P9_Type == "CDF")
				{
					codingTask = task;
				}
			}

			//Now create quality iteration from dependent base workflow
			creator.QcbCreatingUserLoginName = mrNeedful.GS_LoginName;
			creator.CreateQualityIteration(greedTask1.PK, codingTask.PK, reasonPK);

			var link3 = myGreedWorkflow.Links.Single(x => x.HeaderFrom != myLustWorkflow);
			var iteration3 = link3.HeaderFrom;

			query = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, SQLComparisonOperator.Equal, iteration3.PK);
			var newTasks3 = Factory.Load<ProcessTask>(query);

			AssertEquals("Should have ONLY added 3 tasks from quality iteration", 3, newTasks3.Length);
			AssertEquals("This should counteract (Quality Iteration 1)", iteration3.FH_CompletionStatement);
		}

		public void TestCreateQualityIteration_ShouldSuspendQcbTaskCopyWhenItsTypeIsSpecified()
		{
			creator.TaskTypesToSuspendOnRepeatIfQcbTask = new[] { "QCB" };
			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link = workflow.Links.First();
			var iteration = link.HeaderFrom;

			var query = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, SQLComparisonOperator.Equal, iteration.PK);
			var newTasks = Factory.Load<ProcessTask>(query);

			AssertEquals(3, newTasks.Length);

			var qcbCopy = newTasks.SingleOrDefault(x => x.P9_Type == "QCB");
			AssertNotNull("A copy of the QcbTask should appear in the QI, and yet...", qcbCopy);
			AssertEquals("The copy of the QcbTask should be suspended because its type was specified to do so, and yet...", ProcessTaskStatusCodeList.Codes.Suspended, qcbCopy.P9_Status);
		}

		public void TestCreateQualityIteration_WithCustomTasks()
		{
			IEnumerable<QualityIterationTaskDescriptor> CustomQualityIterationTasks()
			{
				yield return new QualityIterationTaskDescriptor();
				yield return new QualityIterationTaskDescriptor
				{
					Type = "COD",
				};
				yield return new QualityIterationTaskDescriptor
				{
					Type = "CBC",
					AssignedStaffCode = "ABC",
					EstDuration = TimeSpan.FromMinutes(60),
				};
				yield return new QualityIterationTaskDescriptor
				{
					Type = "CBF",
					Description = "Usability review",
					AssignedStaffCode = "DEF",
					EstDuration = TimeSpan.FromMinutes(120),
				};
			}

			creator.TaskTypesToSuspendOnRepeatIfQcbTask = new[] { "QCB" };
			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task3.PK, reasonPK, customQualityIterationTasks: CustomQualityIterationTasks());

			var link = workflow.Links.First();
			var iteration = link.HeaderFrom;

			var query = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, SQLComparisonOperator.Equal, iteration.PK);
			var newTasks = Factory.Load<ProcessTask>(query);

			AssertEquals(5, newTasks.Length);
			CombineAssertions(delegate
			{
				AssertEquals(4, newTasks[0].P9_Sequence);
				AssertEquals(5, newTasks[1].P9_Sequence);
				AssertEquals(6, newTasks[2].P9_Sequence);
				AssertEquals(7, newTasks[3].P9_Sequence);
				AssertEquals(8, newTasks[4].P9_Sequence);

				AssertEquals("UDF", newTasks[0].P9_Type);
				AssertEquals("COD", newTasks[1].P9_Type);
				AssertEquals("CBC", newTasks[2].P9_Type);
				AssertEquals("CBF", newTasks[3].P9_Type);
				AssertEquals("QCB", newTasks[4].P9_Type);

				AssertEquals("ASN", newTasks[0].P9_Status);
				AssertEquals("OPN", newTasks[1].P9_Status);
				AssertEquals("ASN", newTasks[2].P9_Status);
				AssertEquals("ASN", newTasks[3].P9_Status);
				AssertEquals("SUS", newTasks[4].P9_Status);

				AssertEquals("Undefined - You can modify this in the System Reg", newTasks[0].P9_Description);
				AssertEquals(string.Empty, newTasks[1].P9_Description);
				AssertEquals(string.Empty, newTasks[2].P9_Description);
				AssertEquals("Usability review", newTasks[3].P9_Description);
				AssertEquals(task3.P9_Description, newTasks[4].P9_Description);

				AssertEquals(string.Empty, newTasks[0].P9_GS_NKAssignedStaffMember);
				AssertEquals(string.Empty, newTasks[1].P9_GS_NKAssignedStaffMember);
				AssertEquals("ABC", newTasks[2].P9_GS_NKAssignedStaffMember);
				AssertEquals("DEF", newTasks[3].P9_GS_NKAssignedStaffMember);
				AssertEquals(task3.P9_GS_NKAssignedStaffMember, newTasks[4].P9_GS_NKAssignedStaffMember);

				AssertEquals(ZDateTime.Empty, newTasks[0].P9_EstDuration);
				AssertEquals(ZDateTime.Empty, newTasks[1].P9_EstDuration);
				AssertEquals((ZDateTime)TimeSpan.FromMinutes(60), newTasks[2].P9_EstDuration);
				AssertEquals((ZDateTime)TimeSpan.FromMinutes(120), newTasks[3].P9_EstDuration);
				AssertEquals(task3.P9_EstDuration, newTasks[4].P9_EstDuration);
			});
		}

		public void TestCreateSecondQualityIteration_ShouldNotCreateDuplicateCompletionStatement()
		{
			creator.TaskTypesToSuspendOnRepeatIfQcbTask = new[] { "QCB" };
			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link1 = workflow.Links.First();
			var iteration1 = link1.HeaderFrom;

			AssertEquals("Perform Gaffes (Quality Iteration 1)", iteration1.FH_CompletionStatement);

			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link2 = workflow.Links.Single(x => x.HeaderFrom != iteration1);
			var iteration2 = link2.HeaderFrom;

			AssertEquals("Perform Gaffes (Quality Iteration 2)", iteration2.FH_CompletionStatement);

			iteration1.FH_CompletionStatement = "Perform Gaffes (Quality Iteration)";
			iteration1.Factory.Save();
			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link3 = workflow.Links.Single(x => x.HeaderFrom != iteration1 && x.HeaderFrom != iteration2);
			var iteration3 = link3.HeaderFrom;

			AssertEquals("Perform Gaffes (Quality Iteration 1)", iteration3.FH_CompletionStatement);
		}

		public void TestCustomQualityIterationName()
		{
			creator.IterationType = "Qualeration";
			creator.QcbCreatingUserLoginName = theAbc.GS_LoginName;
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link1 = workflow.Links.First();
			var iteration1 = link1.HeaderFrom;

			AssertEquals("Perform Gaffes (Qualeration 1)", iteration1.FH_CompletionStatement);
		}

		public void TestResourceUnderReviewPk_ShouldBePassedToViewModel()
		{
			task1.P9_ActualDuration = new TimeSpan(0, 1, 0, 0);
			task2.P9_ActualDuration = new TimeSpan(0, 0, 30, 0);
			task3.P9_ActualDuration = new TimeSpan(0, 0, 30, 0);

			creator.ResourceUnderReviewNk = "JNO";
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link = Factory.LoadTop1<ProcessTaskIterationLink>(new ZQuery());
			AssertEquals("JNO", link.P9I_GS_NKResourceUnderReview);
		}

		public void TestResourceUnderReviewPk_ShouldBePassedToViewModel_NotSet()
		{
			task1.P9_ActualDuration = new TimeSpan(0, 1, 0, 0);
			task2.P9_ActualDuration = new TimeSpan(0, 0, 30, 0);
			task3.P9_ActualDuration = new TimeSpan(0, 0, 30, 0);

			AssertNull(creator.ResourceUnderReviewNk);
			creator.CreateQualityIteration(task3.PK, task1.PK, reasonPK);

			var link = Factory.LoadTop1<ProcessTaskIterationLink>(new ZQuery());
			AssertEquals("TAB", link.P9I_GS_NKResourceUnderReview);
		}

		#region Setup 

		protected override void SetUp()
		{
			base.SetUp();

			EnableBufferManagement();
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetUpWorkItem();
			creator = new ContainmentBarrierCreator();
		}

		void SetUpWorkItem()
		{
			var tones = Factory.NewWithValidTestData<GlbStaff>();
			tones.GS_FullName = "Tony Abbott";
			tones.GS_Code = "TAB";
			var journalist = Factory.NewWithValidTestData<GlbStaff>();
			journalist.GS_FullName = "Journalist";
			journalist.GS_Code = "JNO";
			theAbc = Factory.NewWithValidTestData<GlbStaff>();
			theAbc.GS_Code = "ABC";
			theAbc.GS_LoginName = "The ABC";

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			workflow = BMTestHelper.CreateWorkflow(jobHeader, "Perform Gaffes");

			task1 = (ProcessTask)BMTestHelper.CreateTask(workflow, tones.GS_Code, 10, sequence: 1, description: "Wink creepily", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDU");
			task2 = (ProcessTask)BMTestHelper.CreateTask(workflow, tones.GS_Code, 10, sequence: 2, description: "Eat onion like an apple", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CDF");
			task3 = (ProcessTask)BMTestHelper.CreateTask(workflow, journalist.GS_Code, 10, sequence: 3, description: "Receive the answer I deserve", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");

			const string jobType = "WKI";
			const string reasonCode = "CRE";
			SetAsQCBTaskType("QCB", jobType);
			AddIterationReasonToRegistry(jobType, reasonCode, "Wasn't creepy enough");
			reasonPK = WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReason(jobType, reasonCode).PK;

			AssertEquals(3, jobHeader.Tasks.Count());

			Factory.Save();
		}

		#endregion
	}
}
