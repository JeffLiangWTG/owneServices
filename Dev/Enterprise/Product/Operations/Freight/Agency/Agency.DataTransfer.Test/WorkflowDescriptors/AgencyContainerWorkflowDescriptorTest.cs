using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(AgencyContainerWorkflowDescriptor))]
	internal class AgencyContainerWorkflowDescriptorTest : AgencyContainerWorkflowDescriptorTestBase<AgencyShipmentContainer, AgencyContainerWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "CNS", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Shipping Job Container/Break-bulk", WorkflowDescriptor.Description);
		}
	}
}
