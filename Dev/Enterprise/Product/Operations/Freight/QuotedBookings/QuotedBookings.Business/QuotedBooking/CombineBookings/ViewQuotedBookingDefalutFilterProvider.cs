using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ViewQuotedBookingDefalutFilterProvider : DefaultFilterProvider
	{
		public ZString Vessel { get; set; }
		public ZString Voyage { get; set; }
		public ZGuid Client { get; set; }
		public ZGuid BookingParty { get; set; }
		public ZString OriginPort { get; set; }
		public ZString DestinationPort { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public ZString TransportMode { get; set; }
		public ZString ContainerMode { get; set; }
		public ZGuid Carrier { get; set; }
		public ZGuid Consignor { get; set; }
		public ZGuid Consignee { get; set; }

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddCustomFilterDefaults(collection, FilterNames.VoyageVessel, "VoyageFlightNo", Voyage);
			AddCustomFilterDefaults(collection, FilterNames.VoyageVessel, nameof(Vessel), Vessel);
			AddFilterDefaults(collection, FilterNames.Client, Client);
			AddFilterDefaults(collection, FilterNames.BookingParty, BookingParty);
			AddFilterDefaults(collection, FilterNames.LoadDischarge, LoadPort, DischargePort);
			AddFilterDefaults(collection, FilterNames.OriginDestination, OriginPort, DestinationPort);
			AddFilterDefaults(collection, FilterNames.TransportMode, TransportMode);
			AddFilterDefaults(collection, FilterNames.ContainerMode, ContainerMode);
			AddFilterDefaults(collection, FilterNames.Carrier, Carrier);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public static class FilterNames
		{
			public const string VoyageVessel = "Flight/Voyage # and Vessel";
			public const string Client = "Client";
			public const string BookingParty = "Booking Party";
			public const string TransportMode = "Transport Mode";
			public const string ContainerMode = "Container Mode";
			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
			public const string Carrier = "Carrier";
		}
	}
}
