using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(WorkItemFlattenedCollection))]
	public class WorkItemFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WorkItemFlattenedCollection>
	{
		protected override WorkItemFlattenedCollection GetCollectionToTest()
		{
			return new WorkItemFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WorkItemFlattened();
		}
	}
}
