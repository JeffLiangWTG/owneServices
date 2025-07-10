using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLine))]
	sealed class OrderLineCustomFieldProviderTest : TestICustomFieldProvider
	{
	}
}
