using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	internal static class QuotedBookingDataObjectWriterHelper
	{
		public static void SetLocalClientAddress(this Shipment shipmentData, IDataWritingManager writeManager, QuotedBooking quotedBooking)
		{
			var jobHeader = new JobHeader.Loader(quotedBooking.Quote).Load();

			if (jobHeader != null)
			{
				shipmentData.AddOrgAddress(writeManager, jobHeader.LocalChargesAddr, AddressTypes.SendersLocalClient);
			}
		}

		public static void SetCarrier(this Shipment shipmentData, IDataWritingManager writeManager, QuotedBooking quotedBooking)
		{
			shipmentData.AddOrgAddress(writeManager, quotedBooking.Carrier, DocAddressType.ShippingLineAddress);
		}

		public static void SetCreditor(this Shipment shipmentData, IDataWritingManager writeManager, QuotedBooking quotedBooking) =>
			shipmentData.AddOrgAddress(writeManager,
				quotedBooking.Booking?.Creditor ?? quotedBooking.Quote?.CurrentOneOffQuote?.Creditor,
				DocAddressType.Creditor);
	}
}
