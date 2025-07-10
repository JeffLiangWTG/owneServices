using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(UniversalDataCarrierMessagingController))]
	class CarrierMessagingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.UniversalDataCarrierMessaging;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			return consol;
		}
	}
}
