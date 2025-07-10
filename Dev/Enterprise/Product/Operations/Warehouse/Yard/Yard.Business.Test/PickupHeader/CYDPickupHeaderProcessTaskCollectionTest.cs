using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickupHeaderProcessTaskCollection))]
	public class CYDPickupHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<CYDPickupHeaderProcessTaskCollection>
	{
		protected override CYDPickupHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var pickupHeader = Factory.NewWithValidTestData<CYDPickupHeader>();
			return new CYDPickupHeaderProcessTaskCollection(pickupHeader);
		}
	}
}
