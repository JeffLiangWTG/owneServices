using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class JobRailSailingFilterBusinessObject : JobSailingFilterBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Descriptions
		{
			public const string JourneyNo = "Journey No.";
			public const string JourneyName = "Journey Name";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddRoadFilter(filters);
			return filters;
		}

		#region Add*Filters

		void AddRoadFilter(ModuleFilterCollection filters)
		{
			var journeyNameFilter = filters.AddTextFilter(Descriptions.JourneyName, JobVoyageSchema.JV_RV_NKVessel);
			journeyNameFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobRailSailingFilter|JourneyName", "Journey Name");
			journeyNameFilter.SubGroup = JobSailingFilterProcessor;
			var journeyNumberFilter = filters.AddTextFilter(Descriptions.JourneyNo, JobVoyageSchema.JV_VoyageFlight);
			journeyNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobRailSailingFilter|JourneyNo", "Journey No.");
			journeyNumberFilter.SubGroup = JobSailingFilterProcessor;
		}

		#endregion

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Rail; }
		}
	}
}
