using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business
{
	/// This class does contain a related BusinessObject testcase, however, since the test class's bass class is generic,
	/// and not directly ActiveBusinessObjectTestCase<T> (instead it inherits from ActiveBusinessObjectTestCase<T>),
	/// the reflection test does not find it, and so adding this attribute is required.

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class RelatedCommonShipmentsCollection : RelatedShipmentsCollection<CommonShipment>
	{
		public RelatedCommonShipmentsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RelatedCommonShipmentsCollection(CommonShipment referenceShipment, bool isMasterShipmentCollection)
			: base(referenceShipment, isMasterShipmentCollection)
		{
		}
	}
}
