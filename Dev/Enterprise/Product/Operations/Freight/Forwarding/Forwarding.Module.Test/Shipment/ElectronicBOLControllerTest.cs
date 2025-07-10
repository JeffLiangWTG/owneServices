using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ElectronicBOLController))]
	public class ElectronicBOLControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ElectronicBOL;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
			return shipment;
		}
	}
}
