using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskCompletionStatementCollectionView))]
	class ProcessTaskCompletionStatementCollectionViewTest : BusinessObjectCollectionViewTestCase<ProcessTaskCompletionStatementCollectionView>
	{
		protected override ProcessTaskCompletionStatementCollectionView GetCollectionToTest()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			return (ProcessTaskCompletionStatementCollectionView)template.CompletionStatementTasks;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var task = template.CompletionStatementTasks.AddNew();
			task.P9_Type = "COM";

			return task;
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFilesTestHelper.MakeCompletionStatementTaskType("ORG", "COM");
		}
	}
}
