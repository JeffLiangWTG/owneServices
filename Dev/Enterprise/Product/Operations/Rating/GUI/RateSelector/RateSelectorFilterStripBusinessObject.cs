using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.RateSelector
{
	public class RateSelectorFilterStripBusinessObject : RateFilterStripBusinessObject
	{
		public RateSelectorFilterStripBusinessObject()
		{
		}

		public RateSelectorFilterStripBusinessObject(RatingCriteria criteria, ILogger logger)
			: base(criteria, logger)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "RateSelectorFilterStripBusinessObject";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddEffectiveOnFilter(filters, isReadOnly: false, FilterVisibility.AlwaysVisible, validation: MandatoryValidation.CheckEntered);
			AddLocationFilter(filters, isReadOnly: true);
			AddServiceProviderFilter(filters);
			ServiceProviderFilter.IsActiveChanged += OnModuleFilterIsActiveChanged;
			AddCarrierServiceLevelFilter(filters);

			// extra filters
			AddPaymentTermFilter(filters);
			AddContractNumberFilter(filters);
			AddUniversalCommodityGroupFilter(filters);
			AddCGReferenceFilter(filters);

			AddContainerModeDependentFilters(filters);

			return filters;
		}

		#region Filters

		void AddContainerModeDependentFilters(ModuleFilterCollection filters)
		{
			switch (OriginalCriteria.ContainerMode)
			{
				case Core.Constants.ContainerModes.ULD:
					AddContainerTypeFilter(filters);
					break;

				case Core.Constants.ContainerModes.Loose:
					AddCommodityCodeFilter(filters);
					break;
			}
		}

		#endregion

		#region Query Building

		public override (RatesQuery ratesQuery, bool isValidForRatesService) BuildRatesQuery(ILogger logger)
		{
			RunPreSaveValidation();
			if (HasErrors)
			{
				return (null, false);
			}

			var queryBuilder = new WiseRatesQueryBuilder(logger);
			var (ratesQuery, errors) = queryBuilder.Build(OriginalCriteria, Carriers);
			if (!string.IsNullOrWhiteSpace(errors))
			{
				logger.Log(LogType.Warning, errors);
				return (null, false);
			}

			ratesQuery.TransportMode = new[] { "AIR" };
			ratesQuery.EffectiveDate = EffectiveDate.ToDateTime();
			ratesQuery.EndDate = EffectiveDate.ToDateTime();

			var zoneOwners = OriginalCriteria.ZoneOwnerOrganizations();
			if (!string.IsNullOrWhiteSpace(OriginCode))
			{
				var origin = LocationHelper.GetCachedLocationFromString(OriginCode, Factory);
				ratesQuery.Origin = queryBuilder.GetLocations(origin, zoneOwners);
			}

			if (!string.IsNullOrWhiteSpace(DestinationCode))
			{
				var destination = LocationHelper.GetCachedLocationFromString(DestinationCode, Factory);
				ratesQuery.Destination = queryBuilder.GetLocations(destination, zoneOwners);
			}

			ratesQuery.ServiceLevel = CarrierServiceLevelsFromFilters;
			ratesQuery.PaymentTerm = PaymentTermsFromFilters;
			ratesQuery.Contract = ContractNumbersFromFilters.Select(x => new RatesQueryContract { ContractNumber = x });
			ratesQuery.Commodities = UniversalCommodityGroupsFromFilters;
			ratesQuery.CargoGuideFilters = GetCargoguideFilters();

			var (containers, containerError) = GetContainerTypes();
			if (!string.IsNullOrWhiteSpace(containerError))
			{
				logger.Log(LogType.Error, containerError);
				return (null, false);
			}

			if (!containers.IsNullOrEmpty())
			{
				ratesQuery.Container = queryBuilder.GetContainers(containers, OriginalCriteria.FreightMode);
				if (!ratesQuery.Container.Any())
				{
					logger.Log(LogType.Error, "None of Job's containers can be used for rates search"); // Log message
					return (null, false);
				}
			}

			return (ratesQuery, true);
		}

		protected override CargoGuideFilters GetCargoguideFilters()
		{
			var cgFilters = base.GetCargoguideFilters() ?? new CargoGuideFilters();
			cgFilters.PreferHigherBreakLowerRate = true;

			return cgFilters;
		}

		#endregion

		public override ZQuery GetAdditionalCarrierOrgListFilter(Type typeOfBusinessObjectToQuery, SchemaColumn fieldName)
		{
			var query = new ZDBOnlyQuery(typeOfBusinessObjectToQuery);

			// valid Service Providers to search in CW1
			var cw1SubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			cw1SubQuery.AddToFilter(ServiceProviderCollection.GetServiceProviderFilter());
			query.AddSubQuery(fieldName, cw1SubQuery, JoinCondition.Or);

			// valid Organizations for Cargoguide
			var cgSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			cgSubQuery.AddToFilter(OrgMiscServSchema.OM_RM_Airline, SQLComparisonOperator.NotEqual, null);
			query.AddSubQuery(fieldName, cgSubQuery, JoinCondition.Or);

			return query;
		}

		public override string OrgSourceText => (NoResString)"Rate Selector Search Criteria"; // Not translatable
	}
}
