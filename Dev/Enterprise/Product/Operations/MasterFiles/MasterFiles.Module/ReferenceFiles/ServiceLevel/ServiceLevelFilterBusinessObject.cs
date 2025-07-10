using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ServiceLevelFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class FilterConstants
		{
			public const string ServiceLevelsSupportedByCarrier = "Service Levels supported by Carrier";
			public const string Code = "Code";
			public const string Description = "Description";
			public const string ServiceDeliveryType = "Service Agreement";
			public const string ServiceDeliveryPercentage = "DIFOT Percentage";
			public const string IsGateway = "Is Gateway";
			public const string DefaultTransitTime = "Default Transit Time";
			public const string DeliverOnWeekend = "Deliver On Weekend";
			public const string DefaultArrivalTime = "Default Arrival Time";
			public const string ServiceDeliveryDueTime = "Service Delivery Due Time";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddGuidFilters(filters);
			AddTextFilters(filters);
			AddFlagFilters(filters);
			AddTimeFilters(filters);
			AddCustomFilters(filters);

			return filters;
		}

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(FilterConstants.ServiceLevelsSupportedByCarrier, ModuleIDs.Organisation, OrgHeaderSchema.PK, new ShippingProviderCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|ServiceLevelsSupportedByCarrier", FilterConstants.ServiceLevelsSupportedByCarrier);
			filter.SubGroup = new CarrierSubGroup();
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(FilterConstants.Code, RefServiceLevelSchema.RS_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|Code", FilterConstants.Code);
			filters.AddFiltersForTranslatableText(FilterConstants.Description, RefServiceLevelSchema.RS_Description, typeof(RefServiceLevel), ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|Description", FilterConstants.Description));
			filters.AddTextFilter(FilterConstants.ServiceDeliveryType, RefServiceLevelSchema.RS_ServiceDeliveryType, () => new ServiceLevelDeliveryTypeList()).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|ServiceDeliveryType", FilterConstants.ServiceDeliveryType);
			filters.AddNumberRangeFilter(FilterConstants.ServiceDeliveryPercentage, RefServiceLevelSchema.RS_ServiceDeliveryPercentage).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|ServiceDeliveryPercentage", FilterConstants.ServiceDeliveryPercentage);
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var isGatewayFilter = filters.AddFlagFilter(
				FilterConstants.IsGateway,
				ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|IsGateway|Name", FilterConstants.IsGateway),
				RefServiceLevelSchema.RS_IsGateway,
				ModuleFilterSubGroup.Default);

			isGatewayFilter.MultilingualDescription =
				ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|IsGateway|Desc", FilterConstants.IsGateway);

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				var deliverOnWeekendLabel = Res.GetString("3e06e3f2-5454-4363-aa06-e5d08f44904c", "Deliver On Weekend");

				var deliverOnWeekendFilter = filters.AddFlagsFilter(
					FilterConstants.DeliverOnWeekend,
					new[] { deliverOnWeekendLabel },
					new GetFlagsQuery[] { DeliverOnWeekendQuery });

				deliverOnWeekendFilter.MultilingualDescription =
					ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|DeliverOnWeekend|Desc",
						FilterConstants.DeliverOnWeekend);
			}
		}

		void AddCustomFilters(ModuleFilterCollection filters)
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				var defaultTransitTimeFilter = new TransitTimeModuleFilter(FilterConstants.DefaultTransitTime, RefServiceLevelSchema.RS_DefaultTransitHours);
				defaultTransitTimeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|DefaultTransitTime", FilterConstants.DefaultTransitTime);
				filters.AddCustomFilter(defaultTransitTimeFilter);
			}
		}

		void AddTimeFilters(ModuleFilterCollection filters)
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				var defaultArrivalTimeFilter = new ModuleTimeFilter(FilterConstants.DefaultArrivalTime, RefServiceLevelSchema.RS_DefaultArrivalTime);
				defaultArrivalTimeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|DefaultArrivalTime", FilterConstants.DefaultArrivalTime);
				filters.AddCustomFilter(defaultArrivalTimeFilter);

				var defaultDeliveryDueTimeFilter = new ModuleTimeFilter(FilterConstants.ServiceDeliveryDueTime, RefServiceLevelSchema.RS_DefaultDeliveryDueTime);
				defaultDeliveryDueTimeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ServiceLevelFilter|ServiceDeliveryDueTime", FilterConstants.ServiceDeliveryDueTime);
				filters.AddCustomFilter(defaultDeliveryDueTimeFilter);
			}
		}

		#region GetCarrierQuery

		class CarrierSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orgHeaderFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgMiscServSchema.OM_OH);
				orgHeaderFilter.AddToFilter(filter);

				var orgMiscServFilter = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgCarrierServiceLevelSchema.PL_OM);
				orgMiscServFilter.AddSubQuery(orgHeaderFilter, JoinCondition.And);

				var carrierServiceLevelFilter = new ZDBOnlySubQuery(typeof(OrgCarrierServiceLevel), RefServiceLevelSchema.RS_Code);
				carrierServiceLevelFilter.AddSubQuery(orgMiscServFilter, JoinCondition.And);

				var serviceLevelFilter = new ZDBOnlyQuery(typeof(RefServiceLevel));
				serviceLevelFilter.AddSubQuery(RefServiceLevelSchema.RS_Code, OrgCarrierServiceLevelSchema.PL_Code, carrierServiceLevelFilter, JoinCondition.And);

				// all Orgs support STD.
				serviceLevelFilter.AddToFilter(new ZQuery(RefServiceLevelSchema.RS_Code, "STD"), JoinCondition.Or);

				return serviceLevelFilter;
			}
		}

		#endregion

		#region GetDeliverOnWeekendQuery

		ZQuery DeliverOnWeekendQuery(ZBool deliverOnWeekend)
		{
			var query = new ZQuery();
			if (deliverOnWeekend)
			{
				query.AddToFilter(RefServiceLevelSchema.RS_DeliverOnSaturday, SQLComparisonOperator.Equal, true);
				query.AddToFilter(JoinCondition.Or, RefServiceLevelSchema.RS_DeliverOnSunday, SQLComparisonOperator.Equal, true);
			}
			else
			{
				query.AddToFilter(RefServiceLevelSchema.RS_DeliverOnSaturday, SQLComparisonOperator.Equal, false);
				query.AddToFilter(JoinCondition.And, RefServiceLevelSchema.RS_DeliverOnSunday, SQLComparisonOperator.Equal, false);
			}
			return query;
		}

		#endregion
	}
}
