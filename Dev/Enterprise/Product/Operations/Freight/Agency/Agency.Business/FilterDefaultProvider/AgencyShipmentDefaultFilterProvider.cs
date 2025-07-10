using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentDefaultFilterProvider : DefaultFilterProvider
	{
		public ZString Vessel { get; set; }
		public ZString Voyage { get; set; }
		public ZString ContainerNumber { get; set; }

		public ZString ContainerMode { get; set; }
		public ZString OriginPort { get; set; }
		public ZString DestinationPort { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public ZDateTime ETDFrom { get; set; }
		public ZDateTime ETDTo { get; set; }
		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }

		public ZGuid Consignor { get; set; }
		public ZGuid Consignee { get; set; }
		public ZGuid BookingParty { get; set; }

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddCustomFilterDefaults(collection, (NoResString)"Voyage / Vessel", "VoyageFlightNo", Voyage); // Filter related.
			AddCustomFilterDefaults(collection, (NoResString)"Voyage / Vessel", nameof(Vessel), Vessel); // Filter related.
			AddFilterDefaults(collection, "Container #", ContainerNumber); // filter related
			AddFilterDefaults(collection, "Cargo Type", ContainerMode); // filter related
			AddFilterDefaults(collection, "Load / Discharge", LoadPort, DischargePort); // filter related
			AddFilterDefaults(collection, "Origin / Destination", OriginPort, DestinationPort); // filter related
			AddFilterDefaults(collection, "ETD", ETDFrom, ETDTo);
			AddFilterDefaults(collection, "ETA", ETAFrom, ETATo);
			AddFilterDefaults(collection, "Booking Party", BookingParty); // filter related
			AddFilterDefaults(collection, "Consignor / Consignee", Consignor, Consignee); // filter related
		}
	}
}



