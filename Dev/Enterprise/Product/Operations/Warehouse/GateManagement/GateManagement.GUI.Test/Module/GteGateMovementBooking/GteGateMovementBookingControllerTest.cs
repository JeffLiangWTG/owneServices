using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.GUI.Test
{
	[TestedType(typeof(GteGateMovementBookingController))]
	public class GteGateMovementBookingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GteGateMovementBooking;
		}
	}
}
