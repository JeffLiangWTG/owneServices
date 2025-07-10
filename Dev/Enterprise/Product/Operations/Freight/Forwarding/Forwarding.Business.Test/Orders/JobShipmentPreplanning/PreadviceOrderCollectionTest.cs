using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanningLookups;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(PreadviceOrderCollection))]
	sealed class PreadviceOrderCollectionTest : ActiveBusinessObjectCollectionTestCase<PreadviceOrderCollection>
	{
		protected override PreadviceOrderCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			return new PreadviceOrderCollection(Factory, parent);
		}
	}
}
