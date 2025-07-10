using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessTaskIterationLinkCollection))]
	class ProcessTaskIterationLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTaskIterationLinkCollection>
	{
		public void TestLinksForQCBTask()
		{
			WorkflowTestCase.EnableBufferManagement();
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");

			var job = (IWorkflowProvider)Factory.New<IWorkItem>();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = "QCB";
			var task2 = job.WorkflowItems.Tasks.AddNew();
			task2.P9_Type = "QCB";

			var iterationTask = job.WorkflowItems.Tasks.AddNew();

			var collection1 = new ProcessTaskIterationLinkCollection(task1);
			var collection2 = new ProcessTaskIterationLinkCollection(task2);

			var link1_1 = collection1.AddNew();
			var link1_2 = collection1.AddNew();

			var link2_1 = collection2.AddNew();
			var link2_2 = collection2.AddNew();

			link1_1.P9I_P9_IterationTask = link1_2.P9I_P9_IterationTask = link2_1.P9I_P9_IterationTask = link2_2.P9I_P9_IterationTask = iterationTask.PK;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);

			collection1 = new ProcessTaskIterationLinkCollection(loadedTask1);
			collection2 = new ProcessTaskIterationLinkCollection(loadedTask2);

			AssertEquals(2, collection1.Count);
			AssertEquals(2, collection2.Count);
		}

		[ExpectNoExceptions]
		public void TestAddNew_WhenTaskIsNotQCB_ShouldThrowException()
		{
			var task = Factory.New<ProcessTask>();
			NUnit.Framework.Assert.That(delegate
			{
				new ProcessTaskIterationLinkCollection(task).AddNew();
			}, CustomConstraints.InnermostExceptionThrown(typeof(ArgumentException)));
		}
	}
}
