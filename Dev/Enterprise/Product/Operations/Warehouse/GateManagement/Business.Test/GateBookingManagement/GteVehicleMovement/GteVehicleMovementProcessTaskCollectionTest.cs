using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleMovementProcessTaskCollection))]
	public class GteVehicleMovementProcessTaskCollectionTest : ProcessTaskCollectionTest<GteVehicleMovementProcessTaskCollection>
	{
		protected override GteVehicleMovementProcessTaskCollection GetCollectionToTestCore()
		{
			var transportationUnit = Factory.NewWithValidTestData<GteVehicleMovement>();
			return new GteVehicleMovementProcessTaskCollection(transportationUnit);
		}
	}
}
