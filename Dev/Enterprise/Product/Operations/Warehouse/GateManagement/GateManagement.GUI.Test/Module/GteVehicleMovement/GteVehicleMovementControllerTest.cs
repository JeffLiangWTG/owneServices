using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.GUI.Test
{
	[TestedType(typeof(GteVehicleMovementController))]
	public class GteVehicleMovementControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GteVehicleMovement;
		}
	}
}
