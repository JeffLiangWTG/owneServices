using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Workflow.Test
{
	[TestedType(typeof(StmJobQueueCollection))]
	sealed class StmJobQueueCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StmJobQueueCollection>
	{
		protected override StmJobQueueCollection GetCollectionToTest()
		{
			return new StmJobQueueCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StmJobQueueViewModel(null);
		}
	}
}
