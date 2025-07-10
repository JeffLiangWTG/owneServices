using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class JobRoadSailingFilterBusinessObject : JobSailingFilterBusinessObject
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Descriptions
		{
			public const string TruckRef = "Truck Ref";
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
			var truckRefFilter = filters.AddTextFilter(Descriptions.TruckRef, JobVoyageSchema.JV_VoyageFlight);
			truckRefFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobRoadSailingFilter|TruckRef", "Truck Ref");
			truckRefFilter.SubGroup = JobSailingFilterProcessor;
		}

		#endregion

		#region Implementation

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Road; }
		}

		#endregion
	}
}
