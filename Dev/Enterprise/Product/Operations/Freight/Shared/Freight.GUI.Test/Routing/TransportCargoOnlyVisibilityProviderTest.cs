using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class TransportCargoOnlyVisibilityProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_TransportIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new TransportCargoOnlyVisibilityProvider(null));
		}

		public void TestVisibleGetter_TransportIsNotAir()
		{
			var visibilityProvider = CreateVisibilityProvider(Core.Constants.TransportModes.Sea, true);

			AssertEquals("Visible should be false as transport mode is not Air.", false, visibilityProvider.Visible);
		}

		public void TestVisibleGetter_TransportIsAir()
		{
			var visibilityProvider = CreateVisibilityProvider(Core.Constants.TransportModes.Air, false);

			AssertEquals("Visible should be true as transport mode is Air.", true, visibilityProvider.Visible);
		}

		public void TestVisibleChangedEvent_TransportTypeIsChanged()
		{
			var eventIsFired = false;
			var visibilityProvider = CreateVisibilityProvider(Core.Constants.TransportModes.Air, true);

			visibilityProvider.VisibleChanged += (sender, eventArgs) => eventIsFired = true;

			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals("Event has been fired.", true, eventIsFired);
			AssertEquals("Visible should be false as isLinked is true and transport mode is not Air.", false, visibilityProvider.Visible);
		}

		#region Implementation

		Transport transport;

		TransportCargoOnlyVisibilityProvider CreateVisibilityProvider(string transportMode, bool isLinked)
		{
			var consol = Factory.New<CommonConsol>();

			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = transportMode;
			transport.JW_IsLinked = isLinked;

			return new TransportCargoOnlyVisibilityProvider(transport);
		}

		#endregion

	}
}
