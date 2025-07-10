using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CarrierMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestIsSuitableForForwardAirMessage()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";

			Factory.Save();

			AssertEquals("Current Company is not US/Canada.", false, consol.IsSuitableForForwardAirMessage());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("Current Company is US.", true, consol.IsSuitableForForwardAirMessage());

				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Factory.Save();

				AssertEquals("Transport mode is not Road.", false, consol.IsSuitableForForwardAirMessage());
			}

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "CAALV";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			AssertEquals("Transport mode is not Road.", false, consol.IsSuitableForForwardAirMessage());

			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Current Company is not US/Canada.", false, consol.IsSuitableForForwardAirMessage());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AssertEquals("Current Company is Canada.", true, consol.IsSuitableForForwardAirMessage());
			}
		}
	}
}
