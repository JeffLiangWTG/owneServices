using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	sealed class USAirAMSMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			var shipment = consol.Shipments.AddNew();

			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			return shipment;
		}

		protected override string TestingCountry => CountryCodes.UnitedStates;
		protected override string RelatedJobName => "US Air AMS (Import)";
		protected override string ExpectedAppLockKey => "USAirAMSCommand";
		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSAirAMS;
	}
}
