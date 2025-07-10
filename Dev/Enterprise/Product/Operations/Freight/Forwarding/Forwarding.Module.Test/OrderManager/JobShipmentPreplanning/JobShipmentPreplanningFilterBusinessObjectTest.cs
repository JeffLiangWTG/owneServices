using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(JobShipmentPreplanningFilterBusinessObject))]
	public class JobShipmentPreplanningFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobShipmentPreplanningFilterBusinessObject();
		}

		public void TestWorkflowFiltersPresent()
		{
			JobShipmentPreplanningFilterBusinessObject milestoneFilter = new JobShipmentPreplanningFilterBusinessObject();
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", milestoneFilter["Milestone Date"]);
		}
	}
}
