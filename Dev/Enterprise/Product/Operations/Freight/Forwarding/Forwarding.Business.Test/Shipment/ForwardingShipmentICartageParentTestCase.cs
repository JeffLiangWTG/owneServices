using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment))]
	sealed class ForwardingShipmentICartageParentTestCase : ICartageParentTestCase
	{
		protected override ICartageParent GetNewParent()
		{
			return Factory.New<ForwardingShipment>();
		}
	}
}
