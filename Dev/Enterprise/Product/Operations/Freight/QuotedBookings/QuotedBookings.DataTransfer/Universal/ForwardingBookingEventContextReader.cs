using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	class ForwardingBookingEventContextReader
	{
		internal ForwardingBookingEventContextReader(QuotedBooking quotedBooking, IUniversalFreightHelper helper)
		{
			this.quotedBooking = Argument.NotNull(quotedBooking, "QuotedBooking quotedBooking");
			this.helper = Argument.NotNull(helper, "helper");
		}

		readonly QuotedBooking quotedBooking;
		readonly IUniversalFreightHelper helper;

		internal void AddQuotedBookingContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			var booking = quotedBooking.Booking;
			if (booking != null)
			{
				var shipmentEventContextReader = new CommonShipmentEventContextReader(booking, helper);
				shipmentEventContextReader.AddShipmentContextValues(contextValues);

				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CFSReference, booking.JS_CFSReference);
			}
		}
	}
}
