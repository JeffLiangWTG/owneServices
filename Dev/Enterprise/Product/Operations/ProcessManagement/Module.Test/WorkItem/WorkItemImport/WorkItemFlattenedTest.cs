using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(WorkItemFlattened))]
	class WorkItemFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkItemFlattened();
		}

		#endregion
	}
}
