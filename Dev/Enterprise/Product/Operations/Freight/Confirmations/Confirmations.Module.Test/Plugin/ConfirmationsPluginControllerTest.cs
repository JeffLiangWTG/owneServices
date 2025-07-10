using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(ConfirmationsPluginController))]
	public class ConfirmationsPluginControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ConfirmationsPlugin;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S1000";
			Factory.Save();
			return shipment;
		}
	}
}
