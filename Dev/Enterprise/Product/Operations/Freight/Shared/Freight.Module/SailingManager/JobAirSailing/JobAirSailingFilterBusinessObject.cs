using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class JobAirSailingFilterBusinessObject : JobSailingFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddAirTextFilter(filters);
			AddBookingStatusFilter(filters);

			return filters;
		}

		void AddAirTextFilter(ModuleFilterCollection filters)
		{
			var voyageFilter = filters.AddTextFilter(AirFilterTypes.FlightNo, JobVoyageSchema.JV_VoyageFlight);
			voyageFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobAirSailingFilter|FlightNo", "Flight No.");
			voyageFilter.SubGroup = JobSailingFilterProcessor;
			voyageFilter.UseMultiSearch = true;
		}

		void AddBookingStatusFilter(ModuleFilterCollection filters)
		{
			var flightStatusFilter = filters.AddTextFilter(AirFilterTypes.FlightStatus, GetFlightStatusQuery, FlightStatusList);
			flightStatusFilter.Category = FilterCategories.StatusAndFlags;
			flightStatusFilter.ErrorOnCodeNotPresent = true;
			flightStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Freight|JobAirSailingFilter|FlightStatus", "Flight Status");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		static public class AirFilterTypes
		{
			public const string FlightNo = "Flight No.";
			public const string FlightStatus = "Flight Status";
		}

		#region GetFlightStatusQuery

		ZQuery GetFlightStatusQuery(ZString flightStatus)
		{
			return new ZQuery(JobSailingSchema.JX_OnlineScheduleStatus, flightStatus);
		}

		#endregion

		protected override ZString TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		protected internal override string BookingRefLabel
		{
			get { return (NoResString)"Reserved Master"; }   // Already translated using BookingRefResourceId
		}

		protected internal override MultilingualString BookingRefCaption
		{
			get { return ResString.GetMultilingualString("Freight|JobAirSailingFilter|ReservedMaster", "Reserved Master"); }
		}

		CodeDescriptionPairList FlightStatusList => JobConsolTransportLookups.GetFlightStatusList(Factory);
	}
}
