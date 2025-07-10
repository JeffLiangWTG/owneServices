using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleMovementCollection))]
	public class GteVehicleMovementCollectionTest : ActiveBusinessObjectCollectionTestCase<GteVehicleMovementCollection>
	{
		#region Implementation

		protected override GteVehicleMovementCollection GetCollectionToTest()
		{
			return new GteVehicleMovementCollection(Factory);
		}

		#endregion
	}
}
