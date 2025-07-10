using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Constants;
using WiseRatesAPI = WiseRates.Api.Model;

namespace Enterprise.Rating.GUI
{
	public class WiseRatesFilterStripBusinessObject : RateFilterStripBusinessObject
	{
		public WiseRatesFilterStripBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "WiseRatesFilterStripBusinessObject";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddEffectiveOnFilter(filters, isReadOnly: false);
			AddLocationFilter(filters, isReadOnly: false);
			AddContainerTypeFilter(filters);
			AddTransportModeFilter(filters);
			AddContainerModeFilter(filters);
			AddCarrierFilter(filters);
			AddContractNumberFilter(filters);
			AddCGReferenceFilter(filters);

			var scacCodeFilter = new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.SCACCode)
			{
				MultilingualDescription = RateEntryFilterUtility.Constants.Description.SCACCode,
				Category = FilterCategories.Organisations,
				IsOrCategoryReadOnly = true
			};
			filters.AddFilter(scacCodeFilter);

			var iataCodeFilter = filters.AddGuidFilter(RateEntryFilterUtility.Constants.Codes.IATACode, ModuleIDs.RefAirline, value => new ZQuery(), Airlines);
			iataCodeFilter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.IATACode;
			iataCodeFilter.Category = FilterCategories.Organisations;
			iataCodeFilter.IsOrCategoryReadOnly = true;

			filters.AddFilter(new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.C1Code)
			{
				MultilingualDescription = RateEntryFilterUtility.Constants.Description.C1Code,
				Category = FilterCategories.Organisations,
				IsOrCategoryReadOnly = true
			});

			return filters;
		}

		protected override IReadOnlyList<FilterCategory> CategorySortOrderCore => new FilterCategory[]
		{
			FilterCategories.Dates,
			FilterCategories.Locations,
			FilterCategories.Organisations,
			FilterCategories.NumbersAndReferences,
		};

		readonly SupportedTransportAndContainerModes supportedModes = new SupportedTransportAndContainerModes();

		#region Carriers

		public override string OrgSourceText => (NoResString)"Rates Service Search filter strip"; // internal use only

		// To Get Carriers With SCAC/C1C or IATA code
		public override ZQuery GetAdditionalCarrierOrgListFilter(Type typeOfBusinessObjectToQuery, SchemaColumn fieldName)
		{
			var subQueryIATA = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			subQueryIATA.AddToFilter(OrgMiscServSchema.OM_RM_Airline, SQLComparisonOperator.NotEqual, null);

			var result = new ZDBOnlyQuery(typeOfBusinessObjectToQuery);
			result.AddToFilter(OrgHeaderSchema.OH_RSL_ShippingLine, SQLComparisonOperator.NotEqual, null);
			result.AddSubQuery(fieldName, subQueryIATA, JoinCondition.Or);
			return result;
		}

		IEnumerable<WiseRatesAPI.RatesQueryCarrier> GetAllCarriers(WiseRatesQueryBuilder queryCreator)
		{
			var carriersList = new List<WiseRatesAPI.RatesQueryCarrier>();

			carriersList.AddRange(queryCreator.GetCarriers(Carriers).ratesQueryCarriers);
			carriersList.AddRange(GetIATACarriers());

			foreach (var filter in this.ActiveModuleFilters
						.OfType<WiseRatesModuleTextFilter>()
						.Where(x => !x.Property.IsEmpty))
			{
				WiseRatesAPI.RatesQueryCarrier query = null;
				switch (filter.OriginalCode.ToString())
				{
					case RateEntryFilterUtility.Constants.Codes.SCACCode:
						query = new WiseRatesAPI.RatesQueryCarrier() { SCACCode = filter.Property };
						break;
					case RateEntryFilterUtility.Constants.Codes.C1Code:
						query = new WiseRatesAPI.RatesQueryCarrier() { C1Code = filter.Property };
						break;
				}
				if (query != null)
				{
					carriersList.Add(query);
				}
			}

			return carriersList;
		}

		IEnumerable<WiseRatesAPI.RatesQueryCarrier> GetIATACarriers()
		{
			var airlinePKList = this.ActiveModuleFilters.OfType<ModuleGuidFilter>()
				.Where(filter => filter.OriginalCode == RateEntryFilterUtility.Constants.Codes.IATACode)
				.Select(filter => filter.Property).ToArray();

			return Factory
				.Load<RefAirline>(new ZQuery(RefAirlineSchema.PK, airlinePKList))
				.Select(airline => new WiseRatesAPI.RatesQueryCarrier() { IATACode = airline.RM_TwoCharacterCode });
		}

		#endregion

		#region Container Type

		protected override void AddContainerTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(RateEntryFilterUtility.Constants.Codes.ContainerType, ModuleIDs.RefContainer, value => new ZQuery(), ContainerTypeList);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ContainerType;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.ModesAndTypes;
			filter.IsOrCategoryReadOnly = true;
		}

		protected override System.Collections.IList ContainerTypeList() => containerTypeList ?? (containerTypeList = new RatingRefContainerCollection(Factory));
		RatingRefContainerCollection containerTypeList;

		public override (IEnumerable<RefContainer> refContainers, string message) GetContainerTypes()
		{
			var containerPKs = GetFilters<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.ContainerType)
				.Where(x => !x.Property.IsEmpty)
				.Select(x => x.Property)
				.Distinct()
				.ToArray();

			var containers = Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.PK, containerPKs));
			return (containers, null);
		}

		#endregion

		#region Build RatesQuery

		public List<WiseRatesAPI.RatesQuery> BuildRatesQueries(ILogger logger)
		{
			var ratesQueries = new List<WiseRatesAPI.RatesQuery>();

			if (!TransportModes.Any() || TransportModes.Contains(Core.Constants.TransportModes.Sea))
			{
				var (ratesQuery, _) = BuildRatesQuery(logger);
				ratesQuery.AcceptedProviders = new[] { WRConstants.RateProviders.CargoSphere };
				ratesQuery.TransportMode = new[] { Core.Constants.TransportModes.Sea };

				var containerModes = ContainerModes.Any()
					? ContainerModes
					: new[] { Core.Constants.ContainerModes.FCL }; //Update this when we add new container modes for Sea

				var validContainerModes = GetValidContainerModes(
					WRConstants.RateProviders.CargoSphere,
					Core.Constants.TransportModes.Sea,
					containerModes,
					logger);

				if (validContainerModes.Any())
				{
					ratesQuery.ContainerMode = validContainerModes;
					ratesQueries.Add(ratesQuery);
				}
			}

			if (!TransportModes.Any() || TransportModes.Contains(Core.Constants.TransportModes.Air))
			{
				var (ratesQuery, _) = BuildRatesQuery(logger);
				ratesQuery.AcceptedProviders = new[] { WRConstants.RateProviders.CargoGuide };
				ratesQuery.TransportMode = new[] { Core.Constants.TransportModes.Air };

				var containerModes = ContainerModes.Any()
					? ContainerModes
					: new[] { Core.Constants.ContainerModes.Loose, Core.Constants.ContainerModes.ULD }; //Update this when we add new container modes for Air

				var validContainerModes = GetValidContainerModes(
					WRConstants.RateProviders.CargoGuide,
					Core.Constants.TransportModes.Air,
					containerModes,
					logger);

				if (validContainerModes.Any())
				{
					ratesQuery.ContainerMode = validContainerModes;
					ratesQueries.Add(ratesQuery);
				}

				ratesQuery.CargoGuideFilters = GetCargoguideFilters();
			}

			return ratesQueries;
		}

		public override (WiseRatesAPI.RatesQuery ratesQuery, bool isValidForRatesService) BuildRatesQuery(ILogger logger)
		{
			var result = new WiseRatesAPI.RatesQuery();
			var queryCreator = new WiseRatesQueryBuilder(logger);

			var contracts = ContractNumbersFromFilters
				.Select(n => new WiseRatesAPI.RatesQueryContract { ContractNumber = n })
				.ToArray();

			result.EffectiveDate = EffectiveDate == ZDateTime.Empty ? null : EffectiveDate.ToDateTime();
			result.Carrier = GetAllCarriers(queryCreator);

			var (containers, _) = GetContainerTypes();
			result.Container = queryCreator.GetContainers(containers, TransportModes);

			result.Origin = Origins;
			result.Destination = Destinations;
			result.Contract = contracts;

			return (result, true);
		}

		List<string> GetValidContainerModes(string rateProvider, string transportMode, IEnumerable<string> containerModes, ILogger logger)
		{
			var validContainerModes = new List<string>();

			foreach (var containerMode in containerModes)
			{
				if (supportedModes.GetValidity(transportMode, containerMode) == SupportedTransportAndContainerModes.Validity.Invalid)
				{
					var warning = Res.GetString("897d9318-51ac-40bd-90d6-ff51b8750771", "{0}-{1} is not a valid combination", transportMode, containerMode);
					logger.Log(LogType.Warning, ZString.Format(LogMessageRequestNotSent, warning));
				}
				else if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(transportMode, containerMode, out string reason))
				{
					logger.Log(LogType.Warning, ZString.Format(LogMessageRequestNotSent, reason));
				}
				else
				{
					//Later when we add ULD/LCL or anything else, we need to change them to only FCL/LCL
					//Rates Service only knows FCL and LCL (IsContainerised  or not)

					validContainerModes.Add(cw1ToWRContainerCodeMapping.ContainsKey(containerMode)
						? cw1ToWRContainerCodeMapping[containerMode]
						: containerMode);
				}
			}

			return validContainerModes;
		}

		readonly Dictionary<string, string> cw1ToWRContainerCodeMapping = new Dictionary<string, string>()
		{
			{ Core.Constants.ContainerModes.LCL, WRConstants.ContainerModes.LCL },
			{ Core.Constants.ContainerModes.Loose, WRConstants.ContainerModes.LCL },
			{ Core.Constants.ContainerModes.FCL, WRConstants.ContainerModes.FCL },
			{ Core.Constants.ContainerModes.ULD, WRConstants.ContainerModes.FCL },
		};

		#endregion

		RefAirlineCollection Airlines => airlines ?? (airlines = new RefAirlineCollection(Factory, new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, SQLComparisonOperator.IsNotBlank, string.Empty)));
		RefAirlineCollection airlines;
		MultilingualString LogMessageRequestNotSent => ResString.GetMultilingualString("cc944c36-d57f-494c-8a05-934aae0f5d7d", "Request will not be sent to Rates Service because {0}");
	}
}
