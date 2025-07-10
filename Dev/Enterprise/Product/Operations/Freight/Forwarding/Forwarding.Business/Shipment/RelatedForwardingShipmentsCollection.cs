using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	/// This class does contain a related BusinessObject testcase, however, since the test class's bass class is generic,
	/// and not directly ActiveBusinessObjectTestCase<T> (instead it inherits from ActiveBusinessObjectTestCase<T>),
	/// the reflection test does not find it, and so adding this attribute is required.

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class RelatedForwardingShipmentsCollection : RelatedShipmentsCollection<ForwardingShipment>
	{
		public RelatedForwardingShipmentsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RelatedForwardingShipmentsCollection(ForwardingShipment referenceShipment, bool isMasterShipmentCollection) : base(referenceShipment, isMasterShipmentCollection)
		{
		}
	}
}
