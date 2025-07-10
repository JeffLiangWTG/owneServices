using System;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment))]
	sealed class RelatedForwardingShipmentsCollectionTest : RelatedShipmentsCollectionTest<ForwardingShipment>
	{
		public void TestShipmentType()
		{
			var shipment = GetRelatedShipmentsCollection().AddNew();
			AssertEquals("Shipment type should be forwarding shipment", typeof(ForwardingShipment), shipment.GetType());
		}

		#region Implimentation

		protected override RelatedShipmentsCollection<ForwardingShipment> GetRelatedShipmentsCollection()
		{
			return new RelatedForwardingShipmentsCollection(Factory);
		}

		protected override RelatedShipmentsCollection<ForwardingShipment> GetRelatedShipmentsCollection(ForwardingShipment referenceShipment, bool isMasterShipmentCollection)
		{
			return new RelatedForwardingShipmentsCollection(referenceShipment, isMasterShipmentCollection);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(RelatedForwardingShipmentsCollection);
		}

		#endregion
	}
}
