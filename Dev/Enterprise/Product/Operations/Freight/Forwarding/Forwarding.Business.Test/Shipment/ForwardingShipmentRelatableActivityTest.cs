using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment))]
	sealed class ForwardingShipmentRelatableActivityTest : RelatableActivityTestCase<ForwardingShipment>
	{
		protected override ForwardingShipment GetNewActivity()
		{
			return Factory.NewWithValidTestData<ForwardingShipment>();
		}
	}
}
