using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDDeliveryHeaderProcessTaskCollection))]
	public class CYDDeliveryHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<CYDDeliveryHeaderProcessTaskCollection>
	{
		protected override CYDDeliveryHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var deliveryHeader = Factory.NewWithValidTestData<CYDDeliveryHeader>();
			return new CYDDeliveryHeaderProcessTaskCollection(deliveryHeader);
		}
	}
}
