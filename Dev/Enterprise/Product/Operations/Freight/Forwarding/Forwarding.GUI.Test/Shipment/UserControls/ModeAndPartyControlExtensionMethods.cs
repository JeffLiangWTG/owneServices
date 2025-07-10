using System.Reflection;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	internal static class ModeAndPartyControlExtensionMethods
	{
		public static ModeAndPartyControl GetModeAndPartyControl(this ShipmentBasicRegistrationControl shipmentControl)
		{
			var detailsEntryControlField = shipmentControl.GetType().GetField("ModeAndParty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			return (ModeAndPartyControl)(detailsEntryControlField.GetValue(shipmentControl));
		}
	}
}
