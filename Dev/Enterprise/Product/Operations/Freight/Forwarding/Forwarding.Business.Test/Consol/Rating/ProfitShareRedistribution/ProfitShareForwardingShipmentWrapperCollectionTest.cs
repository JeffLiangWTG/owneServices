using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ProfitShareForwardingShipmentWrapperCollection))]
	sealed class ProfitShareForwardingShipmentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProfitShareForwardingShipmentWrapperCollection>
	{
		protected override ProfitShareForwardingShipmentWrapperCollection GetCollectionToTest()
			=> new ProfitShareForwardingShipmentWrapperCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new ProfitShareForwardingShipmentWrapper(Factory.New<ForwardingShipment>());
	}
}
