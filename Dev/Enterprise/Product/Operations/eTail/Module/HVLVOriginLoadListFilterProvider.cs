using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.eTail.Module
{
	public class HVLVOriginLoadListFilterProvider : DefaultFilterProvider
	{
		public ZString MasterBill { get; set; }
		public ZString Vessel { get; set; }
		public ZString Voyage { get; set; }
		public ZDateTime ETDFrom { get; set; }
		public ZDateTime ETDTo { get; set; }
		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, FilterNames.MasterBill, MasterBill);
			AddCustomFilterDefaults(collection, FilterNames.FlightVoyageVessel, "VoyageFlightNo", Voyage);
			AddCustomFilterDefaults(collection, FilterNames.FlightVoyageVessel, nameof(Vessel), Vessel);
			AddFilterDefaults(collection, FilterNames.ETD, ETDFrom, ETDTo);
			AddFilterDefaults(collection, FilterNames.ETA, ETAFrom, ETATo);
		}

		static class FilterNames
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
			public const string MasterBill = "Master Bill #";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
			public const string FlightVoyageVessel = "Voyage / Vessel";
			public const string ETD = "ETD";
			public const string ETA = "ETA";
		}
	}
}
