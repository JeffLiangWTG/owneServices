using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsTaskFactoryTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTaskFactory(null));
		}

		public void TestCreateTask_NullWorkflowProvider_Throws()
		{
			var generator = new Mock<IProcessHeaderUniqueCompletionStatementGenerator>();
			var taskFactory = new WhsTaskFactory(generator.Object);

			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			AssertExceptionThrown<ArgumentNullException>(() => taskFactory.CreateTask(null, "WUL", "Workflow", "Task", "BRS", 10, "PWH", ZGuid.BrettsGuid, taskType: "CDF"));
		}

		public void TestCreateTask() => TestCreateTask(canLoadCapability: true);
		public void TestCreateTask_CannotLoadCapability() => TestCreateTask(canLoadCapability: false);

		void TestCreateTask(bool canLoadCapability)
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WIN", true);

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = canLoadCapability ? "PWH" : "OTH";
			capability.G4_Description = "Product Warehouse";

			var generator = new Mock<IProcessHeaderUniqueCompletionStatementGenerator>();
			var taskFactory = new WhsTaskFactory(generator.Object);

			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			generator.Setup(g => g.GetUniqueCompletionStatement(receive.Workflows, "Workflow")).Returns("Workflow (2)");

			var task = taskFactory.CreateTask(receive, "WUL", "Workflow", "Task", "BRS", 50, "PWH", releaseGroup.PK, taskType: "CDF");
			CombineAssertions(() =>
			{
				AssertEquals("Formflow type should be set.", "WUL", task.P9_FormFlowType);
				AssertEquals("Type should be set.", "CDF", task.P9_Type);
				AssertEquals("Sequence should be set.", 100, task.P9_Sequence);
				AssertEquals("Description should be set.", "Task", task.P9_Description);
				AssertEquals("Assigned staff member should be set.", "BRS", task.P9_GS_NKAssignedStaffMember);
				AssertEquals("Required capability.", canLoadCapability ? capability.PK : ZGuid.Empty, task.P9_G4_RequiredCapability);

				var workflow = task.ProcessHeader;
				AssertNotNull("Should have created a process header.", workflow);
				AssertEquals("Should have set the workflow name.", "Workflow (2)", workflow.FH_CompletionStatement);
				AssertEquals("Should have set the nudge.", (short)50, workflow.FH_VoteUpDownAmount);
				AssertEquals("Should have left auto assign disabled.", false, workflow.FH_AllowTaskAutoAssignment);
				AssertEquals("Should have set a release group.", releaseGroup.PK, workflow.FH_GG_ReleaseGroup);
				AssertNotNull("Should have a job level workflow.", workflow.ParentHeader);

				AssertContainsExactElementsInAnyOrder("Task should exist on parent job.", [task], receive.WorkflowItems);
				AssertContainsExactElementsInAnyOrder("Workflow should exist on parent job.", [workflow], receive.Workflows.Cast<IProcessHeader>());
			});
		}

		public void TestCreateTask_CannotCreateParentWorkflow()
		{
			var generator = new Mock<IProcessHeaderUniqueCompletionStatementGenerator>();
			var taskFactory = new WhsTaskFactory(generator.Object);

			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			generator.Setup(g => g.GetUniqueCompletionStatement(receive.Workflows, "Workflow")).Returns("Workflow (2)");

			AssertExceptionThrown<InvalidOperationException>(() => taskFactory.CreateTask(receive, "WUL", "Workflow", "Task", "BRS", 50, "PWH", releaseGroup.PK, taskType: "CDF"));
		}

		public void TestCreateTask_MultipleApplications()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WIN", true);

			var generator = new Mock<IProcessHeaderUniqueCompletionStatementGenerator>();
			var taskFactory = new WhsTaskFactory(generator.Object);

			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			generator.Setup(g => g.GetUniqueCompletionStatement(receive.Workflows, "Workflow")).Returns(new Queue<ZString>(["Workflow (2)", "Workflow (3)"]).Dequeue);

			var task1 = taskFactory.CreateTask(receive, "WUL", "Workflow", "Task", "BRS", 50, "PWH", releaseGroup.PK, taskType: "CDF");
			var task2 = taskFactory.CreateTask(receive, "WUL", "Workflow", "Task", "BRS", 50, "PWH", releaseGroup.PK, taskType: "CDF");

			CombineAssertions(() =>
			{
				AssertNotEquals(task1, task2);
				AssertEquals("Sequence should be set.", 100, task1.P9_Sequence);
				AssertEquals("Sequence should be set.", 200, task2.P9_Sequence);

				var workflow1 = task1.ProcessHeader;
				AssertNotNull("Should have created a process header.", workflow1);
				AssertEquals("Should have set the workflow name.", "Workflow (2)", workflow1.FH_CompletionStatement);

				var workflow2 = task2.ProcessHeader;
				AssertNotEquals(workflow1, workflow2);
				AssertNotNull("Should have created a process header.", workflow2);
				AssertEquals("Should have set the workflow name.", "Workflow (3)", workflow2.FH_CompletionStatement);
			});
		}

		public void TestCreateTask_MultipleApplications_NotATask()
		{
			var bmsHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsHelper.EnableBMSInRegistry();
			bmsHelper.CreateSystemAndRelatedWorkflowType(Factory, "WIN", true);

			var generator = new Mock<IProcessHeaderUniqueCompletionStatementGenerator>();
			var taskFactory = new WhsTaskFactory(generator.Object);

			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "PWH";
			capability.G4_Description = "Product Warehouse";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			generator.Setup(g => g.GetUniqueCompletionStatement(receive.Workflows, "Workflow")).Returns(new Queue<ZString>(["Workflow (2)", "Workflow (3)"]).Dequeue);

			var task1 = taskFactory.CreateTask(receive, "WUL", "Workflow", "Task", "BRS", 50, "PWH", releaseGroup.PK, taskType: "TRG");
			var task2 = taskFactory.CreateTask(receive, "WUL", "Workflow", "Task", "BRS", 50, "PWH", releaseGroup.PK, taskType: "CDF");

			CombineAssertions(() =>
			{
				AssertNotEquals(task1, task2);
				AssertEquals("Sequence should be set.", 100, task1.P9_Sequence);
				AssertEquals("Sequence should be set.", 100, task2.P9_Sequence);
			});
		}
	}
}
