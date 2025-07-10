using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVBookingHeaderAdhocEdocsSupportCollection))]
	class HVLVBookingHeaderAdhocEdocsSupportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVBookingHeaderAdhocEdocsSupportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVBookingHeader>();
		}
	}
}
