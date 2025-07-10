using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ProfitShareForwardingShipmentWrapper))]
	sealed class ProfitShareForwardingShipmentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ProfitShareForwardingShipmentWrapper(Factory.New<ForwardingShipment>());
	}
}
