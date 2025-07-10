using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLine))]
	sealed class OrderLineWorkflowProviderTest : WorkflowProviderTest<OrderLine, OrderLineProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.OrderLineWorkflowDescriptorCode;
	}
}
