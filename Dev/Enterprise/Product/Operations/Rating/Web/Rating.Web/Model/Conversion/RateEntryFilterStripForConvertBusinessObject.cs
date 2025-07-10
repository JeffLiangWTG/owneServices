using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// We have used this class as a utility to help with converting RateQuery to a ZQuery so we can fetch CW1 rates from DB.
	/// </summary>
	public class RateEntryFilterStripForConvertBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		internal const string PlannedLoadDischarge = "Planned Load/Discharge";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		internal const string RateOriginDestination = "Rate Origin/Destination";
		internal const string ServiceLevelFilterAsText = RateEntryFilterUtility.Constants.Codes.ServiceLevel + "AsText";
		internal const string GatewayServiceLevelFilterAsText = RateEntryFilterUtility.Constants.Codes.GatewayServiceLevel + "AsText";
		internal const string ShipmentGatewayServiceLevelFilterAsText = RateEntryFilterUtility.Constants.Codes.ShipmentGatewayServiceLevel + "AsText";

		/// <summary>
		/// This constructor is required By ZArchitecture (Enterprise.ZArchitecture.Business.FilterStripBusinessObject.CloneInternal)
		/// </summary>		
		public RateEntryFilterStripForConvertBusinessObject()
		{
			SourceEndpoint = SourceEndpoint.Costing;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		public RateEntryFilterStripForConvertBusinessObject(SourceEndpoint sourceEndpoint)
		{
			SourceEndpoint = sourceEndpoint;
		}

		readonly SourceEndpoint SourceEndpoint;

		ModuleFilterCollection filters;

		/// <summary>
		/// GetModuleFiltersCore
		/// </summary>
		/// <returns></returns>
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			filters = new ModuleFilterCollection();

			var includeAll = SourceEndpoint != SourceEndpoint.IntercompanyTariffs;

			foreach (var filter in new RateEntryFilterProvider(Factory).GetRateEntryFilters(SourceEndpoint.GetRateType(), filterCategory: string.Empty, includeAll))
			{
				filters.AddFilter(filter);
			}

			if (SourceEndpoint != SourceEndpoint.Costing)
			{
				AddPlannedLoadDischargeFilter(filters);
				AddRateOriginDestinationFilter(filters);
			}

			AddServiceLevelFilterAsText(filters);
			AddGatewayServiceLevelFilterAsText(filters);
			AddShipmentGatewayServiceLevelFilterAsText(filters);

			return filters;
		}

		void AddPlannedLoadDischargeFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddLocationFilter(PlannedLoadDischarge, RateEntrySchema.TI_PlannedLoadLRC, LocationList, RateEntrySchema.TI_PlannedDischargeLRC, LocationList, true);
			filter.Category = FilterCategories.Locations;
		}

		void AddRateOriginDestinationFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddLocationFilter(RateOriginDestination, RateEntrySchema.TI_RateOrigin, LocationList, RateEntrySchema.TI_RateDestination, LocationList, true);
			filter.Category = FilterCategories.Locations;
		}

		// The default ServiceLevel/GatewayServiceLevel filter in RateEntryFilterUtility is of Category 'FilterCategories.ModesAndTypes'.
		// In this case, when you try to set the filter SQLComparisonOperator to 'SQLComparisonOperator.IsBlank' it won't create the necessary zQuery.
		// It seem that the filter category should be 'FilterCategories.TextSearch'.
		// Take a look at Convert method in RateQueryToZQueryConverter for the usage.
		void AddServiceLevelFilterAsText(ModuleFilterCollection filterCollection)
		{
			var filter =
				filterCollection
				.AddTextFilter
					(
						ServiceLevelFilterAsText,
						RateEntrySchema.TI_RS_NKServiceLevel_NI
					);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ServiceLevel;
			filter.Category = FilterCategories.TextSearch;
		}

		void AddGatewayServiceLevelFilterAsText(ModuleFilterCollection filterCollection)
		{
			var filter =
				filterCollection
				.AddTextFilter
					(
						GatewayServiceLevelFilterAsText,
						RateEntrySchema.TI_RS_NKGatewayServiceLevel
					);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.GatewayServiceLevel;
			filter.Category = FilterCategories.TextSearch;
		}

		void AddShipmentGatewayServiceLevelFilterAsText(ModuleFilterCollection filterCollection)
		{
			var filter =
				filterCollection
					.AddTextFilter
					(
						ShipmentGatewayServiceLevelFilterAsText,
						RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel
					);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ShipmentGatewayServiceLevel;
			filter.Category = FilterCategories.TextSearch;
		}

		RatingLocationCollection LocationList
		{
			get { return locationList ?? (locationList = new RatingLocationCollection(Factory)); }
		}

		RatingLocationCollection locationList;
	}
}
