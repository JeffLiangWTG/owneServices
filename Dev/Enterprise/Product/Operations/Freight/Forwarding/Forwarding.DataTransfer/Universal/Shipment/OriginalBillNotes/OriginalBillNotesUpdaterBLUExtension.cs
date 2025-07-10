using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal static class OriginalBillNotesUpdaterBLUExtension
	{
		public static void OnEventsRelatedToOriginalBillNotes(this ForwardingShipment shipment, UniversalEvent eventAdded)
		{
			new OriginalBillNotesUpdaterForEventReceiver(shipment, eventAdded).PopulateOriginalBillNotes();
		}

		public static bool HasAddressDetails(this OrganizationAddress organizationAddress)
		{
			if (organizationAddress == null)
			{
				return false;
			}

			return new[]
			{
				organizationAddress.Address1.GetValueOrDefault(),
				organizationAddress.Address2.GetValueOrDefault(),
				organizationAddress.City.GetValueOrDefault(),
				organizationAddress.Postcode.GetValueOrDefault(),
				organizationAddress.State?.Code.GetValueOrDefault() ?? ZString.Empty,
				organizationAddress.Country?.Name.GetValueOrDefault() ?? ZString.Empty
			}.Any(value => !value.IsEmpty);
		}
	}
}
