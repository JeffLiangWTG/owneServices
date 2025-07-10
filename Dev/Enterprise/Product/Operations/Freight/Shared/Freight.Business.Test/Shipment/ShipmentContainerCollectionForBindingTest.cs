using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Business.CommonShipment;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentContainerCollectionForBinding))]
	sealed class ShipmentContainerCollectionForBindingTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<CommonShipment>().ContainersForBinding;
		}
	}
}
