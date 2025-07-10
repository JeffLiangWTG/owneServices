using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	public class EDIMessageDeliveryContextSelectorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcessTypes()
		{
			var bizo = Factory.New<EDIMessageDeliveryContextSelector>();
			var descriptors = new WorkflowDescriptorList();
			AssertEquals(descriptors.Count, bizo.Lookups.ProcessTypes.Count);
			AssertEquals(true, bizo.Lookups.ProcessTypes.ContainsCode(descriptors[0]));
		}
	}
}
