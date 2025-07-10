using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(ProjectFlattenedCollection))]
	public class ProjectFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProjectFlattenedCollection>
	{
		protected override ProjectFlattenedCollection GetCollectionToTest()
		{
			return new ProjectFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProjectFlattened();
		}
	}
}
