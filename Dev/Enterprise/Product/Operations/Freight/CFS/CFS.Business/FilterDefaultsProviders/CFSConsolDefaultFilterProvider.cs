using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSConsolDefaultFilterProvider : DefaultFilterProvider
	{
		public ZString TransportMode { get; set; }
		public ZString ContainerMode { get; set; }
		public ZString OriginPort { get; set; }
		public ZString DestinationPort { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public ZDateTime ETDFrom { get; set; }
		public ZDateTime ETDTo { get; set; }
		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }
		public ZGuid Client { get; set; }
		public ZString Voyage { get; set; }
		public ZString Vessel { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, "Transport / Container Modes", TransportMode, ContainerMode);
			AddFilterDefaults(collection, "Load / Discharge", LoadPort, DischargePort);
			AddFilterDefaults(collection, "Origin / Destination", OriginPort, DestinationPort);
			AddFilterDefaults(collection, "ETD", ETDFrom, ETDTo);
			AddFilterDefaults(collection, "ETA", ETAFrom, ETATo);

			AddFilterDefaults(collection, "Client", Client);

			AddCustomFilterDefaults(collection, "Voyage / Flight / Vessel", "VoyageFlightNo", Voyage);
			AddCustomFilterDefaults(collection, "Voyage / Flight / Vessel", nameof(Vessel), Vessel);
		}
	}
}
