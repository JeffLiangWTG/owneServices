using System;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(RelatedCommonShipmentsCollection))]
	sealed class RelatedCommonShipmentsCollectionTest : RelatedShipmentsCollectionTest<CommonShipment>
	{
		public void TestCollectionType()
		{
			var shipment = GetRelatedShipmentsCollection().AddNew();
			AssertEquals(typeof(CommonShipment), shipment.GetType());
		}

		#region Implementation

		protected override RelatedShipmentsCollection<CommonShipment> GetRelatedShipmentsCollection()
		{
			return new RelatedCommonShipmentsCollection(Factory);
		}

		protected override RelatedShipmentsCollection<CommonShipment> GetRelatedShipmentsCollection(CommonShipment referenceShipment, bool isMasterShipmentCollection)
		{
			return new RelatedCommonShipmentsCollection(referenceShipment, isMasterShipmentCollection);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(RelatedCommonShipmentsCollection);
		}

		#endregion
	}
}
