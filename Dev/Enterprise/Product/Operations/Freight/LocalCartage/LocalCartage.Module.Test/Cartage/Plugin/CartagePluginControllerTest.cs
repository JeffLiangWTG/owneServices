using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartagePluginController))]
	public class CartagePluginControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CartagePlugin;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CommonShipment shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1000";
			Factory.Save();
			return shipment;
		}
	}
}
