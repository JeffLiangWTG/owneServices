using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskExcludingCompletionStatementCollectionView))]
	class ProcessTaskExcludingCompletionStatementCollectionViewTest : BusinessObjectCollectionViewTestCase<ProcessTaskExcludingCompletionStatementCollectionView>
	{
		protected override ProcessTaskExcludingCompletionStatementCollectionView GetCollectionToTest()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			return (ProcessTaskExcludingCompletionStatementCollectionView)template.TasksExcludingCompletionStatements;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ProcessTask>();
		}
	}
}
