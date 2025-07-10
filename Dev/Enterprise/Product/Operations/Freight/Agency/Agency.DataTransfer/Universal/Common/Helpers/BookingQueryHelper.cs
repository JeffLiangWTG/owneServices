using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	public static class BookingQueryHelper
	{
		public static ZGuid GetBookingParty(this UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var bookingPartyDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			if (bookingPartyDataObject != null)
			{
				var bookingPartyAddress = new OrganisationDataObjectReader(bookingPartyDataObject, logger, factory).GetMatched();
				if (bookingPartyAddress != null)
				{
					return bookingPartyAddress.OA_OH;
				}
			}

			return ZGuid.Empty;
		}

		public static ZString GetBookingPartyName(this UniversalShipment dataObject)
		{
			var bookingPartyDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			return bookingPartyDataObject?.CompanyName.GetValueOrDefault().ToUpper() ?? ZString.Empty;
		}

		public static bool IsBookingPartyNameMatched(AgencyShipment booking, ZString bookingPartyName)
		{
			if (booking != null && booking.BookingParty != null)
			{
				if (booking.BookingPartyDocumentaryAddress.E2_AddressOverride && booking.BookingPartyDocumentaryAddress.E2_CompanyName == bookingPartyName)
				{
					return true;
				}

				if (booking.BookingParty.OH_FullName == bookingPartyName)
				{
					return true;
				}
			}

			return false;
		}
	}
}
