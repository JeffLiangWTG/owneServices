using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(ETailShipmentController))]
	public class ETailShipmentControllerTestCase : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ETailShipment;
		}
	}
}
