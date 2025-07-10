using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class FlightBookingStatusManager
	{
		public static bool UpdateBookingStatus(BusinessObjectFactory factory, AirBookingRequest bookingRequest, string bookingStatus)
		{
			if (factory == null
				|| bookingRequest?.FlightDetails == null
				|| string.IsNullOrWhiteSpace(bookingStatus))
			{
				return false;
			}

			var transportLegPks = new List<ZGuid>();

			foreach (var flight in bookingRequest.FlightDetails)
			{
				if (flight.Status == null)
				{
					continue;
				}

				flight.Status.Code = bookingStatus;

				if (ZGuid.TryParse(flight.Identifier, out var pk))
				{
					transportLegPks.Add(pk);
				}
			}

			var transports = factory.Load<Freight.Business.Transport>(new ZQuery(JobConsolTransportSchema.PK, transportLegPks));

			foreach (var transport in transports)
			{
				transport.JW_Status = bookingStatus;
			}

			return true;
		}
	}
}
