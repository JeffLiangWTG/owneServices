using CargoWise.Types;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCFSDetail))]
	sealed class GateTransportCFSDetailWorkflowProviderTest : WorkflowProviderTest<GateTransportCFSDetail, GateTransportCFSDetailProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GateTransportCFSWorkflowDescriptorCode;
	}
}
