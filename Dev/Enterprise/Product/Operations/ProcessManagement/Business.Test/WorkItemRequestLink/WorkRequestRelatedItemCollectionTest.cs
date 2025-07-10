using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestRelatedItemCollection))]
	class WorkRequestRelatedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var request = Factory.NewWithValidTestData<WorkRequest>();
			return new WorkRequestRelatedItemCollection(request);
		}
	}
}
