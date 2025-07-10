using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeclarationDefaultFilterProvider : DefaultFilterProvider
	{
		public ZString TransportMode { get; set; }
		public ZString ContainerMode { get; set; }
		public ZString OriginPort { get; set; }
		public ZString DestinationPort { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Constants
		{
			public const string TransportMode = "Transport Mode";
			public const string ContainerMode = "Container Mode (Customs)";
			public const string LoadDischarge = "Consol Load/Discharge";
			public const string OriginDestination = "Shipment Origin/Destination";
		}

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, Constants.TransportMode, TransportMode);
			AddFilterDefaults(collection, Constants.ContainerMode, ContainerMode);
			AddFilterDefaults(collection, Constants.LoadDischarge, LoadPort, DischargePort);
			AddFilterDefaults(collection, Constants.OriginDestination, OriginPort, DestinationPort);
		}
	}
}
