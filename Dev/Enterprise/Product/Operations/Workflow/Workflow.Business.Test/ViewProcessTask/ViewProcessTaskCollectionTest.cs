using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ViewProcessTaskCollection))]
	class ViewProcessTaskCollectionTest : ActiveBusinessObjectCollectionTestCase<ViewProcessTaskCollection>
	{
		protected override ViewProcessTaskCollection GetCollectionToTest()
		{
			return new ViewProcessTaskCollection(Factory.NewWithValidTestData<DummyWithWorkflow>());
		}
	}
}
