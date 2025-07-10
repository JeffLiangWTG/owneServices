using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementProcessTaskCollection))]
	public class GteGateMovementProcessTaskCollectionTest : ProcessTaskCollectionTest<GteGateMovementProcessTaskCollection>
	{
		protected override GteGateMovementProcessTaskCollection GetCollectionToTestCore()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			return new GteGateMovementProcessTaskCollection(gateMovement);
		}
	}
}
