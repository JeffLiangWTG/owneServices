using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleEntryCollection))]
	public class GteVehicleEntryCollectionTest : ActiveBusinessObjectCollectionTestCase<GteVehicleEntryCollection>
	{
		protected override GteVehicleEntryCollection GetCollectionToTest()
		{
			return new GteVehicleEntryCollection(Factory.New<GteVehicleMovement>());
		}
	}
}
