using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(TransportWorkflowDescriptor))]
	public class TransportProcessTaskProviderTest : TransportProcessTaskProviderTestBase<CommonCartage, TransportWorkflowDescriptor>
	{
		public void TestTransportJobTypeList()
		{
			Assert(((TransportWorkflowDescriptor)WorkflowDescriptor).TransportJobTypeList.Count > 0);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Cartage, WorkflowDescriptor.DocumentBusinessContext[0]);
			AssertEquals(BusinessContext.ContainerLeg, WorkflowDescriptor.DocumentBusinessContext[1]);
		}
	}
}
