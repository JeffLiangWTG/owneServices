using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Tools;
using static System.FormattableString;
using RefServiceLevel = WiseRates.Api.Model.RefServiceLevel;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// The RateFilterStripBusinessObject should contain filters that are common
	/// to all users of it. This is not the place to add CargoSphere or Cargoguide specific filters.
	/// </summary>
	public abstract class RateFilterStripBusinessObject :
		FilterStripBusinessObject,
		IRateSelectorFilterValueProvider
	{
		protected RateFilterStripBusinessObject()
		{ }

		protected RateFilterStripBusinessObject(RatingCriteria criteria, ILogger logger)
		{
			OriginalCriteria = criteria;
			Logger = logger;
		}

		#region Rating Criteria

		public RatingCriteria OriginalCriteria { get; }

		public RatingCriteria CreateCriteria()
		{
			var criteria = OriginalCriteria?.CreateCopy();

			if (criteria != null)
			{
				WriteFilterValuesIntoRatingCriteria(criteria);
			}

			return criteria;
		}

		void WriteFilterValuesIntoRatingCriteria(RatingCriteria criteria)
		{
			criteria.IsManualCostSelectMode = true;
			criteria.ValuesCanBeSet = true;

			#region Creditors

			if (!Carriers.Any())
			{
				criteria.Creditors = null;
			}
			else
			{
				var originalCreditor = criteria.Creditors;
				var carriersToSearch = Carriers.ToList();

				// In the manual rate selector, we need to load rates based on both Service Provider on parent Costing and Transport Provider/Carrier on RateEntry.
				// Therefore, we need to load rates based on Carrier even Service Provider is not in the Filter values.
				// The following query helps us to load extra Orgs that they do not exist in Carriers but found in RateEntry's Transport Provider/Carrier
				var headerQuery = new ZDBOnlyQuery(typeof(RatingHeader));
				headerQuery.AddToFilter(RatingHeaderSchema.TH_OH, SQLComparisonOperator.NotEqual, null);
				headerQuery.AddToFilter(RatingHeaderSchema.TH_OH, SQLComparisonOperator.NotEqual, CarrierPKs);
				var transportProviderQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);
				transportProviderQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OH_TransportProvider, CarrierPKs);
				headerQuery.AddSubQuery(RatingHeaderSchema.PK, transportProviderQuery, JoinCondition.And);
				var headers = Factory.Load<RatingHeader>(headerQuery);

				var orgPKs = headers
					.Where(x => x != null && !x.TH_OH.IsEmpty)
					.Select(x => x.TH_OH)
					.Distinct()
					.ToList();

				var orgLoadQuery = new ZQuery(OrgHeaderSchema.PK, orgPKs);
				orgLoadQuery.AllowTableValuedParameters = true;

				var orgs = Factory.Load<OrgHeader>(orgLoadQuery);

				carriersToSearch.AddRange(orgs
					.WhereNotNull()
					.Select(x => OrgWithSource.New(x, new List<string> { OrgSourceText })));

				var chargeCodeGroups = originalCreditor.ChargeCodeGroups;

				if (chargeCodeGroups.IsNullOrEmpty() || chargeCodeGroups.Any(string.IsNullOrWhiteSpace))
				{
					criteria.Creditors = Creditors.New(carriersToSearch);
				}
				else
				{
					criteria.Creditors = new Creditors();

					foreach (var item in chargeCodeGroups)
					{
						criteria.Creditors[item].Add(priority: 1, carriersToSearch.ToArray());
					}
				}
			}

			#endregion

			#region Origin/Destination

			if (!string.IsNullOrWhiteSpace(OriginCode))
			{
				criteria.Origin = LocationHelper.GetCachedLocationFromString(OriginCode, Factory);
			}

			if (!string.IsNullOrWhiteSpace(DestinationCode))
			{
				criteria.Destination = LocationHelper.GetCachedLocationFromString(DestinationCode, Factory);
			}

			#endregion

			#region PaymentTerm

			criteria.PaymentTerm = new PaymentTermInfos();

			if (PaymentTermsFromFilters.Any())
			{
				foreach (var item in PaymentTermsFromFilters)
				{
					criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Cost, item));
					criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Revenue, item));
				}
			}

			#endregion

			#region Carrier's Contract Number

			criteria.CarrierContractNumbers = new List<ZString>();
			if (ContractNumbersFromFilters.Any())
			{
				var contractNumbers = new List<ZString>();
				foreach (var item in ContractNumbersFromFilters)
				{
					contractNumbers.Add(item);
				}

				criteria.CarrierContractNumbers = contractNumbers;
			}

			#endregion

			#region Carrier's Service Level

			criteria.CarrierServiceLevelOverride = new List<ZString>();
			if (CarrierServiceLevelsFromFilters.Any())
			{
				criteria.CarrierServiceLevelOverride.AddRange(CarrierServiceLevelsFromFilters.Select(s => new ZString(s)));
			}

			#endregion

			#region Commodity type

			if (!string.IsNullOrEmpty(CommodityCodeFromFilter))
			{
				criteria.RateableMeasures.UpdateCommodities(CommodityCodeFromFilter);
			}

			#endregion

			#region Container Type
			// TODO : Replace Container Types in Measures. It is not that easy though. Maybe we should do it, in "WI00406486 - [ULD Project] Separate tabs based on Container/Commodity in CG Rate Selector"
			// Probably I should entirely change the way we create a Criteria to filter and calculate CW1 rates.
			#endregion
		}

		#endregion Rating Criteria

		#region Effective On

		protected void AddEffectiveOnFilter(ModuleFilterCollection filters, bool isReadOnly, FilterVisibility visibility = FilterVisibility.Visible, Validation validation = null)
		{
			var filter = filters.AddSingleDateFilter(RateEntryFilterUtility.Constants.Codes.EffectiveOn, value => new ZQuery());
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.EffectiveOn;
			filter.Visibility = visibility;
			filter.Category = FilterCategories.Dates;
			filter.Property1 = ZDateTime.Today;
			filter.IsOrCategoryReadOnly = true;
			filter.Property1Info.ValueChanged += MandatoryFilter_ValueChanged;
			if (validation != null)
			{
				filter.Property1Validation = validation;
			}

			if (isReadOnly)
			{
				filter.Visibility = FilterVisibility.AlwaysVisible;
				filter.IsGroupOrCategoryReadOnly = true;
				filter.IsSingleInstanceOnly = true;
				filter.ReadOnly = true;
				filter.Property1Validation = MandatoryValidation.CheckEntered;
			}
		}

		public ModuleSingleDateFilter EffectiveOnFilter => GetFilter<ModuleSingleDateFilter>(RateEntryFilterUtility.Constants.Codes.EffectiveOn);

		public ZDateTime EffectiveDate => EffectiveOnFilter?.Property1 ?? ZDateTime.Empty;

		#endregion

		#region Locations (Origin / Destination)

		protected void AddLocationFilter(ModuleFilterCollection filters, bool isReadOnly)
		{
			var filter = filters.AddLocationFilter(RateEntryFilterUtility.Constants.Codes.OriginDestination, RateEntrySchema.TI_OriginLRC, LocationList, RateEntrySchema.TI_DestinationLRC, LocationList, true);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.OriginDestination;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.Locations;
			filter.SetItemDescriptions(Res.GetData("a3112d09-f438-4c19-b69d-a8c386c24bb8", "Origin"), Res.GetData("79f5dba9-45c7-4bad-bd34-d2b52899e077", "Destination"));
			filter.IsOrCategoryReadOnly = true;
			filter.IsSingleInstanceOnly = true;
			filter.Property1Info.ValueChanged += MandatoryFilter_ValueChanged;
			filter.Property2Info.ValueChanged += MandatoryFilter_ValueChanged;

			if (isReadOnly)
			{
				filter.Visibility = FilterVisibility.AlwaysVisible;
				filter.IsGroupOrCategoryReadOnly = true;
				filter.Property1Validation = CheckPortLocation;
				filter.Property2Validation = CheckPortLocation;
				filter.ReadOnly = RatingDataRegistry.Instance.MultiModalRatingCost.Value;
			}
		}

		void CheckPortLocation(ZPropertyInfo info)
		{
			if (!info.HasMessageErrors())
			{
				MandatoryValidation.CheckEntered(info);
			}

			if (!info.HasMessageErrors())
			{
				if (LocationHelper.GetLocationType((ZString)info.Value) != LocationHelper.LocationType.Port)
				{
					info.AddError(Res.GetString("76D3C2B9-716D-40FF-A02D-424FDF22088D", "Please enter a valid Port/UNLOCO code"));
				}
			}
		}

		RatingLocationCollection LocationList => locationList ?? (locationList = new RatingLocationCollection(Factory));
		RatingLocationCollection locationList;

		public ModuleLocationFilter LocationFilter => GetFilter<ModuleLocationFilter>(RateEntryFilterUtility.Constants.Codes.OriginDestination);

		IEnumerable<ZString> OriginFilterValues => GetFilters<ModuleLocationFilter>(RateEntryFilterUtility.Constants.Codes.OriginDestination).Select(x => x.Property1);
		public IEnumerable<string> Origins => GetSearchLocations(OriginFilterValues.ToArray());
		public string OriginCode => OriginFilterValues.FirstOrDefault();

		IEnumerable<ZString> DestinationFilterValues => GetFilters<ModuleLocationFilter>(RateEntryFilterUtility.Constants.Codes.OriginDestination).Select(x => x.Property2);
		public IEnumerable<string> Destinations => GetSearchLocations(DestinationFilterValues.ToArray());
		public string DestinationCode => DestinationFilterValues.FirstOrDefault();

		IEnumerable<string> GetSearchLocations(ZString[] locationStrings)
		{
			var locationObjects = locationStrings.Distinct().Select(x => LocationHelper.GetCachedLocationFromString(x, Factory)).ToArray();

			var locations = new HashSet<string>();

			if (locationObjects.Any())
			{
				locations.Add(ZString.Empty);

				foreach (var location in locationObjects.WhereNotNull())
				{
					locations.Add(location.Code);

					if (!location.UNLOCO?.RL_IATARegionCode.IsEmpty ?? false)
					{
						locations.Add(location.UNLOCO.RL_IATARegionCode);
					}

					if (location.Country != null)
					{
						locations.Add(location.Country.RN_Code);
					}

					var internationalZones = RatingZoneRetriever.GetApplicableWiseRatesZones(location.Zones, GlbCompany.CurrentCompany.OrgProxy);
					foreach (var internationalZone in internationalZones)
					{
						locations.Add(internationalZone);
					}
				}
			}

			return locations.ToArray();
		}

		#endregion

		void MandatoryFilter_ValueChanged(object sender, EventArgs e)
		{
			var filterCodes = new List<string>
			{
				RateEntryFilterUtility.Constants.Codes.CarrierContractNumber,
				RateEntryFilterUtility.Constants.Codes.NamedAccount,
			};

			foreach (var filter in ActiveModuleFilters.Where(x => filterCodes.Contains(x.OriginalCode)))
			{
				filter.Validation.ValidateAll();
			}
		}

		#region Transport Mode

		protected void AddTransportModeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(RateEntryFilterUtility.Constants.Codes.TransportMode, value => new ZQuery(), TransportModeList);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.TransportMode;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.ModesAndTypes;
			filter.IsOrCategoryReadOnly = true;
		}

		CodeDescriptionPairList TransportModeList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddPair(Core.Constants.RateMode.AIR, Res.GetString("41d257df-7628-44f4-b30b-b314c1115b35", "Air Freight"));
			result.AddPair(Core.Constants.RateMode.SEA, Res.GetString("4626c31b-e193-4b99-8a94-ed4d2b1c667b", "Sea Freight"));

			return result;
		}

		public IEnumerable<string> TransportModes => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.TransportMode);

		#endregion

		#region Container Mode

		protected void AddContainerModeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(RateEntryFilterUtility.Constants.Codes.ContainerMode, value => new ZQuery(), ContainerModeList);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ContainerMode;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.ModesAndTypes;
			filter.IsOrCategoryReadOnly = true;
			ContainerModeFilter = filter;
		}
		public ModuleTextFilter ContainerModeFilter { get; private set; }

		CodeDescriptionPairList ContainerModeList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddPair(Core.Constants.RateMode.FCL, Res.GetString("593e4d01-33d2-403e-b638-40b254190683", "Full Container Load"));
			result.AddPair(Core.Constants.RateMode.LSE, Res.GetString("37428d15-c483-4fbe-aede-d49457ca49c7", "Loose Container Load"));
			result.AddPair(Core.Constants.RateMode.LCL, Res.GetString("f0a4d53e-a17f-40fb-89bf-6179ec66af6c", "Less Container Load"));
			result.AddPair(Core.Constants.RateMode.ULD, Res.GetString("fae82faa-bc83-4008-8110-4d900b8e8fb6", "Unit Load Device"));

			return result;
		}

		public IEnumerable<string> ContainerModes => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.ContainerMode);

		#endregion

		#region Container Type

		protected virtual void AddContainerTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(RateEntryFilterUtility.Constants.Codes.ContainerType, value => new ZQuery(), ContainerTypeList);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ContainerType;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.ModesAndTypes;
			filter.IsOrCategoryReadOnly = true;
		}

		protected virtual IList ContainerTypeList()
		{
			if (containerTypeList == null)
			{
				containerTypeList = new CodeDescriptionPairList();
				foreach (var container in OriginalCriteria?.GetContainers() ?? Array.Empty<MasterFiles.Business.RefContainer>())
				{
					containerTypeList.AddPair(container.RC_Code, container.RC_DescriptionMultilingual);
				}
			}

			return containerTypeList;
		}

		CodeDescriptionPairList containerTypeList;

		public virtual (IEnumerable<MasterFiles.Business.RefContainer> refContainers, string message) GetContainerTypes()
		{
			var containerCodes = GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.ContainerType);
			if (!containerCodes.Any())
			{
				return (Enumerable.Empty<MasterFiles.Business.RefContainer>(), null);
			}

			var refContainerQuery = new ZQuery(RefContainerSchema.RC_Code, containerCodes);
			var refContainers = Factory.Load<MasterFiles.Business.RefContainer>(refContainerQuery);

			return refContainers.Any()
				? (refContainers, null)
				: (Enumerable.Empty<MasterFiles.Business.RefContainer>(), (NoResString)"None of requested containers can be used for rates search."); // Log message
		}

		#endregion

		#region Commodity

		#region Universal Commodity Group

		protected void AddUniversalCommodityGroupFilter(ModuleFilterCollection filters)
		{
			var universalCommodityGroupFilter = new WiseRatesModuleTextFilter(
				RateEntryFilterUtility.Constants.Codes.UniversalCommodityGroup,
				TryGetUniversalCommodityGroupList)
			{
				MultilingualDescription = RateEntryFilterUtility.Constants.Description.UniversalCommodityGroup,
				Visibility = FilterVisibility.Visible,
				Category = FilterCategories.NumbersAndReferences,
				IsOrCategoryReadOnly = true
			};
			universalCommodityGroupFilter.IsActiveChanged += OnModuleFilterIsActiveChanged;

			filters.AddFilter(universalCommodityGroupFilter);
		}

		IList TryGetUniversalCommodityGroupList() => TryGetRefDataFromRatesService(
			(NoResString)"Universal Commodity Group", // for logging
			(ratesServiceClient, correlationID) => ratesServiceClient.GetCommodityGroups(correlationID));

		public IEnumerable<string> UniversalCommodityGroupsFromFilters => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.UniversalCommodityGroup);

		public IEnumerable<string> UniversalCommodityGroupsFromJob => universalCommodityGroupsFromJob ??
			(
				universalCommodityGroupsFromJob = CommodityCodesFromJobContainers
					.SelectMany(commodityCode => GetCommodityGroupsFromCommodityCode(commodityCode))
					.Distinct()
					.ToList()
			);
		IEnumerable<string> universalCommodityGroupsFromJob;

		public IEnumerable<string> UniversalCommodityGroupsFromJobPlusGeneralAndNotClassified
		{
			get
			{
				var commodityGroups = UniversalCommodityGroupsFromJob.ToList();
				if (commodityGroups.Any())
				{
					commodityGroups.UniqueAddRange(new[] { RefCommodityCode.UniversalGroups.General, RefCommodityCode.UniversalGroups.NotClassified });
				}

				return commodityGroups;
			}
		}

		public IEnumerable<string> GetCommodityGroupsFromCommodityCode(ZString commodityCode)
		{
			var groups = new List<string>();

			var refCommodityCode = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, commodityCode);
			if (refCommodityCode == null)
			{
				return groups;
			}

			if (!string.IsNullOrWhiteSpace(refCommodityCode.RH_UniversalCommodityGroup))
			{
				groups.Add(refCommodityCode.RH_UniversalCommodityGroup);
				return groups;
			}

			if (refCommodityCode.RH_IsHazardous)
			{
				groups.Add(RefCommodityCode.HAZD);
			}
			if (refCommodityCode.RH_IsPerishable)
			{
				groups.Add(RefCommodityCode.PERS);
			}
			if (refCommodityCode.RH_IsTimber)
			{
				groups.Add(RefCommodityCode.TIMB);
			}
			if (refCommodityCode.RH_IsFlammable)
			{
				groups.Add(RefCommodityCode.FLAM);
			}
			if (refCommodityCode.RH_ContainerVentRequired)
			{
				groups.Add(RefCommodityCode.CNVT);
			}

			return groups;
		}

		#endregion Universal Commodity Group

		#region Commodity Code

		protected void AddCommodityCodeFilter(ModuleFilterCollection filters)
		{
			var commodityCodeFilter = filters.AddNkFilter(RateEntryFilterUtility.Constants.Codes.CommodityCode, RateEntrySchema.TI_RH_NKCommodityCode, ModuleIDs.RefCommodityCode, new RefCommodityCodeCollection(Factory));
			commodityCodeFilter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.CommodityCode;
			commodityCodeFilter.Visibility = FilterVisibility.Visible;
			commodityCodeFilter.IsSingleInstanceOnly = true;
			commodityCodeFilter.Category = FilterCategories.NumbersAndReferences;
			commodityCodeFilter.IsOrCategoryReadOnly = true;
			commodityCodeFilter.IsActiveChanged += OnModuleFilterIsActiveChanged;
		}

		public string CommodityCodeFromFilter => GetNkFilterValues(RateEntryFilterUtility.Constants.Codes.CommodityCode).FirstOrDefault();

		public IEnumerable<string> CommodityCodesFromJobContainers
			=> (OriginalCriteria?.JobMeasures?.GetContainerUniqueCommodityCodes() ?? Enumerable.Empty<string>())
				.Where(commodityCode =>
					!string.IsNullOrEmpty(commodityCode) &&
					commodityCode != RefCommodityCode.UniversalGroups.General &&
					commodityCode != RefCommodityCode.UniversalGroups.NotClassified);

		public IEnumerable<string> CommodityCodesFromJobPacklines
			=> (OriginalCriteria?.JobMeasures?.GetPackageUniqueCommodityCodes() ?? Enumerable.Empty<string>())
				.Where(commodityCode =>
					!string.IsNullOrEmpty(commodityCode) &&
					commodityCode != RefCommodityCode.UniversalGroups.General &&
					commodityCode != RefCommodityCode.UniversalGroups.NotClassified);

		#endregion Commodity Code

		#endregion Commodity

		#region Carrier / Service Provider

		protected void AddCarrierFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider, ModuleIDs.Organisation, value => new ZQuery(), CarrierOrgList);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.CarrierTransportProvider;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.Organisations;
			filter.IsOrCategoryReadOnly = true;
		}

		OrgHeaderCollection CarrierOrgList => carrierOrgList ?? (carrierOrgList = new OrgHeaderCollection(
			Factory,
			GetAdditionalCarrierOrgListFilter(typeof(OrgHeader), OrgHeaderSchema.PK)));
		OrgHeaderCollection carrierOrgList;

		public abstract ZQuery GetAdditionalCarrierOrgListFilter(Type typeOfBusinessObjectToQuery, SchemaColumn fieldName);

		protected void AddServiceProviderFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider, ModuleIDs.Organisation, value => new ZQuery(), AllOrgList);
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.ServiceProvider;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.Organisations;
			filter.IsOrCategoryReadOnly = true;
			ServiceProviderFilter = filter;
		}
		public ModuleGuidFilter ServiceProviderFilter { get; private set; }

		OrgHeaderCollection AllOrgList => allOrgList ?? (allOrgList = new OrgHeaderCollection(Factory));
		OrgHeaderCollection allOrgList;

		public IEnumerable<ZGuid> CarrierPKs
		{
			get
			{
				var carrierFilters = GetFilters<ModuleGuidFilter>(RateEntryFilterUtility.Constants.Codes.CarrierTransportProvider).ToList();

				return carrierFilters
					.Where(x => !x.Property.IsEmpty)
					.Select(x => x.Property)
					.Distinct()
					.ToList();
			}
		}

		public IEnumerable<OrgWithSource> Carriers
		{
			get
			{
				var carriers = Factory
					.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, CarrierPKs))
					.OrderBy(x => CarrierPKs.ToList().IndexOf(x.PK))
					.Select(x => OrgWithSource.New(x, new List<string> { OrgSourceText }))
					.ToList();

				return carriers;
			}
		}

		public abstract string OrgSourceText { get; }

		#endregion

		#region Contract Number

		protected virtual WiseRatesModuleTextFilter CreateContractNumberFilter()
		{
			return new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber, RateEntrySchema.TI_ContractNumber);
		}

		protected void AddContractNumberFilter(ModuleFilterCollection filters)
		{
			var filter = CreateContractNumberFilter();

			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.CarrierContractNumber;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.IsOrCategoryReadOnly = true;
			filter.IsActiveChanged += OnModuleFilterIsActiveChanged;

			filters.AddFilter(filter);
		}

		public IEnumerable<string> ContractNumbersFromFilters => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber);

		public IEnumerable<string> ContractNumbersFromJob =>
			(OriginalCriteria?.CarrierContractNumbers ?? Enumerable.Empty<ZString>())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.Select(x => x.ToString());

		#endregion

		#region Named Account

		protected virtual WiseRatesModuleTextFilter CreateNamedAccountFilter()
		{
			return new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.NamedAccount);
		}

		protected void AddNamedAccountFilter(ModuleFilterCollection filters)
		{
			var filter = CreateNamedAccountFilter();
			filter.MultilingualDescription = RateEntryFilterUtility.Constants.Description.NamedAccount;
			filter.Visibility = FilterVisibility.Visible;
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.IsOrCategoryReadOnly = true;

			filters.AddFilter(filter);
		}

		public IEnumerable<string> NamedAccounts => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.NamedAccount);

		#endregion

		#region Carrier Service Level

		protected void AddCarrierServiceLevelFilter(ModuleFilterCollection filters)
		{
			CarrierServiceLevelFilter = new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel, GetUniversalCarrierServiceLevelList)
			{
				MultilingualDescription = RateEntryFilterUtility.Constants.Description.CarrierServiceLevel,
				Visibility = FilterVisibility.Visible,
				Category = FilterCategories.NumbersAndReferences,
				IsOrCategoryReadOnly = true
			};
			CarrierServiceLevelFilter.IsActiveChanged += OnModuleFilterIsActiveChanged;

			filters.AddFilter(CarrierServiceLevelFilter);
		}

		public CodeDescriptionPairList GetUniversalCarrierServiceLevelList()
		{
			var carrierLevels = TryGetRefDataFromRatesService(
				(NoResString)"Universal Service Level", // for logging
				(ratesServiceClient, correlationID) => ratesServiceClient.GetServiceLevels(correlationID),
				addStandardCode: true);

			return carrierLevels;
		}

		public WiseRatesModuleTextFilter CarrierServiceLevelFilter;

		public IEnumerable<string> CarrierServiceLevelsFromFilters => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel);

		public IEnumerable<string> UniversalCarrierServiceLevelsFromJob
		{
			get
			{
				var carrierServiceLevel = OriginalCriteria?.ServiceLevel?.GetServiceLevel(ServiceLevelType.Carrier) ?? ZString.Empty;
				if (!string.IsNullOrWhiteSpace(carrierServiceLevel))
				{
					var universalCarrierServiceLevels = JobServiceProviders
						.SelectMany(orgHeader => orgHeader.MiscServ.CarrierServiceLevels
							.Where(orgCarrierServiceLevel => orgCarrierServiceLevel.PL_Code == carrierServiceLevel && !string.IsNullOrWhiteSpace(orgCarrierServiceLevel.PL_CarrierServiceCode))
							.SelectMany(orgCarrierServiceLevel => orgCarrierServiceLevel.CarrierServiceCodes))
						.Select(zs => zs.ToString())
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.ToList();

					// when PL_Code is exactly the same as Universal code we don't map it
					// so it is potentially a universal code
					universalCarrierServiceLevels.UniqueAdd(carrierServiceLevel.ToString(), false, StringComparer.OrdinalIgnoreCase);

					return universalCarrierServiceLevels;
				}

				return Enumerable.Empty<string>();
			}
		}

		#endregion

		#region Payment Term

		protected void AddPaymentTermFilter(ModuleFilterCollection filters)
		{
			var paymentTermFilter = new WiseRatesModuleTextFilter(
				RateEntryFilterUtility.Constants.Codes.PaymentTerm,
				GetPaymentTermList)
			{
				MultilingualDescription = RateEntryFilterUtility.Constants.Description.PaymentTerm,
				Visibility = FilterVisibility.Visible,
				Category = FilterCategories.NumbersAndReferences,
				IsOrCategoryReadOnly = true,
				IsSingleInstanceOnly = true
			};
			paymentTermFilter.IsActiveChanged += OnModuleFilterIsActiveChanged;

			filters.AddFilter(paymentTermFilter);
		}

		IList GetPaymentTermList() => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentType);

		public IEnumerable<string> PaymentTermsFromFilters => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.PaymentTerm);

		public string PaymentTermsFromJob =>
			(OriginalCriteria?.DefaultFilterValueForPaymentTerm?.PaymentTermInfoCollection ?? Enumerable.Empty<PaymentTermInfo>())
				.Where(p => p.InfoType == PaymentTermType.PrepaidCollect)
				.Select(p => p.Value)
				.FirstOrDefault();

		#endregion

		#region CG Reference filter

		protected void AddCGReferenceFilter(ModuleFilterCollection filters)
		{
			var newFilter = new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.CGReference)
			{
				MultilingualDescription = RateEntryFilterUtility.Constants.Description.CGReference,
				Visibility = FilterVisibility.Visible,
				Category = FilterCategories.NumbersAndReferences,
				IsOrCategoryReadOnly = true
			};

			filters.AddFilter(newFilter);
		}

		public IEnumerable<string> CGReferences => GetTextFilterValues(RateEntryFilterUtility.Constants.Codes.CGReference);

		protected virtual CargoGuideFilters GetCargoguideFilters()
		{
			var cgReferences = CGReferences.ToArray();
			if (cgReferences.Any())
			{
				return new CargoGuideFilters { References = cgReferences };
			}

			return null;
		}

		#endregion CG Reference filter

		#region IRateSelectorFilterValueProvider members

		public abstract (RatesQuery ratesQuery, bool isValidForRatesService) BuildRatesQuery(ILogger logger);

		#endregion

		#region Events propagation

		public event EventHandler<ModuleFilter.IsActiveChangedEventArgs> ModuleFilterIsActive;

		/// <summary>
		/// Capture ModuleFilter's IsActiveChanged event and propagate it
		/// </summary>
		protected void OnModuleFilterIsActiveChanged(object sender, ModuleFilter.IsActiveChangedEventArgs e)
		{
			if (!e.ModuleFilter.IsActive)
			{
				return;
			}

			ModuleFilterIsActive?.Invoke(this, e);
		}

		#endregion

		#region Helpers for Job values

		public IEnumerable<OrgHeader> JobServiceProviders => OriginalCriteria?.PossibleServiceProviders ?? Enumerable.Empty<OrgHeader>();

		#endregion

		#region Rates Service Helper Methods

		UntranslatableCodeDescriptionPairList TryGetRefDataFromRatesService<T>(string refName, Func<IWiseRatesClient, string, T[]> getRefData, bool addStandardCode = false) =>
			Factory.GetCachedValue(
				Invariant($"RateSelectorFilterStripBusinessObject|{refName}"), // log message
				() =>
				{
					var codeDescriptionPairList = new UntranslatableCodeDescriptionPairList(Invariant($"{refName} codes values cannot be translated")); // Untranslatable reason

					var correlationID = WiseRatesClient.GenerateTraceID();
					var clientFactory = ObjectFactory.Get<IWiseRatesClientFactory>();
					var (ratesServiceClient, _) = clientFactory.TryCreate(correlationID, logger: Logger);

					if (ratesServiceClient != null)
					{
						Logger?.Information(Invariant($"Requesting {refName}. RequestID: {correlationID}")); // Log item

						var refData = getRefData(ratesServiceClient, correlationID);
						if (refData != null)
						{
							codeDescriptionPairList.AddRange(refData.Select(CreateItem).WhereNotNull().ToList());

							if (addStandardCode && !codeDescriptionPairList.ContainsCode(OrgCarrierServiceLevel.StandardCode))
							{
								codeDescriptionPairList.AddPair(OrgCarrierServiceLevel.StandardCode, OrgCarrierServiceLevel.StandardDescription);
							}
						}
					}

					return codeDescriptionPairList;
				});

		static CodeDescriptionPair CreateItem<T>(T refData)
		{
			if (refData is RefServiceLevel serviceLevel)
			{
				return new CodeDescriptionPair(serviceLevel.Code, serviceLevel.Description);
			}

			if (refData is RefCommodityGroup commodity)
			{
				return new CodeDescriptionPair(commodity.Code, commodity.Description);
			}

			return null;
		}

		#endregion

		#region Module Filter Helper Methods

		public IEnumerable<TModuleFilter> GetFilters<TModuleFilter>(ZString code) where TModuleFilter : ModuleFilter =>
			ActiveModuleFilters.Union(AlwaysVisibleModuleFilters).OfType<TModuleFilter>().Where(x => x.OriginalCode == code);

		protected TModuleFilter GetFilter<TModuleFilter>(ZString code) where TModuleFilter : ModuleFilter =>
			GetFilters<TModuleFilter>(code).FirstOrDefault();

		protected IEnumerable<string> GetTextFilterValues(string code) =>
			GetFilters<ModuleTextFilter>(code)
				.Select(x => x.Property.ToString())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToArray();

		protected IEnumerable<string> GetNkFilterValues(string code) =>
			GetFilters<ModuleNkFilter>(code)
				.Select(x => x.Property.ToString())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToArray();

		#endregion

		public virtual void SetCargoSphereFilters(IEnumerable<RefCargoSphereContract> contracts, IEnumerable<RefCargoSphereNamedAccount> namedAccounts)
		{
		}

		readonly ILogger Logger;
	}
}
