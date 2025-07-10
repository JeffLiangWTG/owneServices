using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	class ForwardingBookingModuleMatcher : ModuleMatcher<IShipmentDataObjectReader, ForwardingShipment, QuotedBooking>
	{
		public ForwardingBookingModuleMatcher(UniversalObjectFactory factory)
			: base(factory, GetForwardingBookingJobShipmentFilter())
		{
		}

		static ZQuery GetForwardingBookingJobShipmentFilter()
		{
			var result = new ZQuery();
			result.AddToFilter(JobShipmentSchema.JS_IsBooking, ZBool.True);
			result.AddToFilter(JobShipmentSchema.JS_IsForwardRegistered, ZBool.False);
			return result;
		}

		protected override QuotedBooking GetOuterMatchedBO(ForwardingShipment shipmentBO)
		{
			var bookingBOs = factory.Load<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_JS, shipmentBO.PK))
				.Select(view => view.QuotedBooking)
				.ToArray();
			if (bookingBOs != null && bookingBOs.Length == 1)
			{
				return bookingBOs[0];
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Found Shipment [{0}], but couldn't load the Quoted Booking using the Shipment PK [{1}].", shipmentBO.HumanReadableName, shipmentBO.PK.ToString()));
			}
		}
	}
}
