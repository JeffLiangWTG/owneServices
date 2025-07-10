using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Module.Testing
{
	[TestedType(typeof(PortMessagingController))]
	sealed class PortMessagingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PortMessaging;
		}

		protected override string CountryCode
		{
			get { return Constants.CountryCodes.Germany; }
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			return consol;
		}
	}
}
