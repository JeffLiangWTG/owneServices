using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(CoLoadTrackingShipmentCollection))]
	sealed class CoLoadTrackingShipmentCollectionTest : CoLoadForwardingShipmentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CoLoadTrackingShipmentCollection((TrackingShipment)Helper.MasterShipment, Factory);
		}

		protected override ColoadShipmentTestHelper GetHelper()
		{
			return new ColoadTrackingShipmentTestHelper(Factory);
		}

		public class ColoadTrackingShipmentTestHelper : ColoadForwardingShipmentTestHelper
		{
			public ColoadTrackingShipmentTestHelper(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override CommonShipment GetNewShipmentCore()
			{
				return factory.New<TrackingShipment>();
			}
		}
	}
}
