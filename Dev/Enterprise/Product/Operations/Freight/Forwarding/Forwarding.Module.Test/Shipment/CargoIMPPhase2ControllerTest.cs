using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(CargoIMPPhase2Controller))]
	public class CargoIMPPhase2ControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CargoIMPPhase2;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ForwardingShipment obj = Factory.New<ForwardingShipment>();
			Factory.Save();
			return obj;
		}
	}
}
