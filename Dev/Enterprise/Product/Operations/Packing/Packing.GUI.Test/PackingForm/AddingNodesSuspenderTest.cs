using CargoWise.EntityFramework;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Packing.GUI.Testing
{
	class AddingNodesSuspenderTest : PackingTestCaseWithFactory
	{
		public void TestAddingNodesSuspender()
		{
			AssertEquals(false, AddingNodesSuspender.IsAddingNodesSuspended(Factory));

			using (AddingNodesSuspender.SuspendAddingNodes(Factory))
			{
				AssertEquals(true, AddingNodesSuspender.IsAddingNodesSuspended(Factory));
				AssertEquals(false, AddingNodesSuspender.IsAddingNodesSuspended(new BusinessObjectFactory()));
			}

			AssertEquals(false, AddingNodesSuspender.IsAddingNodesSuspended(Factory));
		}
	}
}
