using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business.AutoRating;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using LocationKey = Enterprise.Rating.Business.RatingCriteriaLocationsCache.Keys;

namespace Enterprise.Rating.Business
{
	public abstract class RateEntryFilter
	{
		public static IEnumerable<IRateEntry> Filter(RatingCriteria criteria, bool isCosting, IEnumerable<IRateEntry> entries, BusinessObjectFactory factory, ILogger logger, FreightAutoRater.RatesToFindEnum ratesToFind = FreightAutoRater.RatesToFindEnum.ActiveRates)
		{
			var filteredEntries = new List<IRateEntry>();

			using (var filterLogger = logger.SortAndDistinct())
			{
				var freightEntries = entries.Where(e => e.IsFreightEntry()).ToList();
				var freightMatcher = new FreightRateEntryFilter(criteria, isCosting, factory, filterLogger);
				freightMatcher.RatesToFind = ratesToFind;
				filteredEntries.AddRange(freightMatcher.FindBestMatches(freightEntries));

				var suppEntries = entries.Where(e => !e.IsFreightEntry()).ToList();
				var supplementalMatcher = new SupplementalRateEntryFilter(criteria, isCosting, factory, filterLogger);
				supplementalMatcher.RatesToFind = ratesToFind;
				filteredEntries.AddRange(supplementalMatcher.FindBestMatches(suppEntries));
			}

			return filteredEntries;
		}

		protected RateEntryFilter(RatingCriteria criteria, bool isCosting, BusinessObjectFactory factory, ILogger logger)
		{
			this.criteria = criteria;
			this.factory = factory;
			this.isAutoratingForCost = isCosting;
			this.logger = logger;

			var costOrSell = GetCostOrSell();

			var parameters = new RatingCriteriaLocationsCacheParameters
			{
				Origin = criteria.OriginCode,
				Destination = criteria.DestinationCode,
				Via = criteria.GetViaCode(costOrSell),
				PlannedLoad = criteria.PlannedLoadCode(costOrSell),
				PlannedDischarge = criteria.PlannedDischargeCode(costOrSell),
				RateOrigin = criteria.RateOriginCode,
				RateDestination = criteria.RateDestinationCode,
				FirstLoad = criteria.GetFirstLoadCode(costOrSell),
				LastDischarge = criteria.GetLastDischargeCode(costOrSell),
				FirstRouteSetLoad = criteria.GetFirstRouteSetLoadCode(costOrSell),
				LastRouteSetDischarge = criteria.GetLastRouteSetDischargeCode(costOrSell),
				ZoneOwners = criteria.ZoneOwnerOrganizations(),
				SortedOverridenPlannedLoads = criteria.SortedOverridenPlannedLoad.Select(x => x.Code),
				SortedOverridenPlannedDischarges = criteria.SortedOverridenPlannedDischarge.Select(x => x.Code),
				OriginServices = criteria.OriginServiceLocations,
				DestinationServices = criteria.DestinationServiceLocations
			};

			(ratingCriteriaLocationsCache, _) = RatingCache.GetRatingCriteriaLocations(factory, parameters);
			RatesToFind = FreightAutoRater.RatesToFindEnum.ActiveRates;
			ShouldLogEntryRemovalReason = true;
		}

		protected readonly bool isAutoratingForCost;
		protected RateEntriesRepository rateEntriesRepository;
		protected readonly ILogger logger;
		protected readonly RatingCriteria criteria;
		protected readonly BusinessObjectFactory factory;
		protected FreightAutoRater.RatesToFindEnum RatesToFind { get; private set; }
		protected RatingCriteriaLocationsCache ratingCriteriaLocationsCache;
		protected string ReasonDelimitor = (NoResString)" and "; // Filter log message

		#region Find Matches

		/// <summary>
		/// Finds rate entries that match the current autorating criteria.
		/// These rates could come from any relevant client rate, or company tariff.
		/// </summary>
		/// <returns>List of matching rates</returns>
		protected virtual List<IRateEntry> FindBestMatches(IEnumerable<IRateEntry> unfilteredEntries)
		{
			return Filter(unfilteredEntries).ToList();
		}

		#endregion

		#region Filters

		[Flags]
		protected enum LocationsFilterMode
		{
			EverythingButLocations = 1,
			LocationsOnly = 2,
			Everything = EverythingButLocations | LocationsOnly,
		}

		protected IEnumerable<IRateEntry> Filter(
			IEnumerable<IRateEntry> unfilteredEntries,
			bool excludeFiltersForPossibleMatches = false,
			bool excludeVia = false,
			bool excludeIntercompanyTariff = false,
			LocationsFilterMode locationsFilterMode = LocationsFilterMode.Everything)
		{
			rateEntriesRepository = new RateEntriesRepository(unfilteredEntries, logger);

			if (excludeIntercompanyTariff)
			{
				rateEntriesRepository.FilterEntries(IntercompanyTariffFilter);
			}

			RateLocationFilter.Apply(rateEntriesRepository.FilteredEntries, criteria);

			if ((locationsFilterMode & LocationsFilterMode.EverythingButLocations) == LocationsFilterMode.EverythingButLocations)
			{
				FilterBase(excludeFiltersForPossibleMatches);
				FilterAdditional();
			}

			if ((locationsFilterMode & LocationsFilterMode.LocationsOnly) == LocationsFilterMode.LocationsOnly && !criteria.IsLooseRateSearchForCarrierConnect)
			{
				if (!excludeVia)
				{
					rateEntriesRepository.FilterEntries(TranshipmentFilter);
				}
				rateEntriesRepository.FilterEntries(PlannedLoadFilter);
				rateEntriesRepository.FilterEntries(PlannedDischargeFilter);

				if (_Rating.Sell)
				{
					rateEntriesRepository.FilterEntries(RateOriginFilter);
					rateEntriesRepository.FilterEntries(RateDestinationFilter);
				}

				FilterLocations();
			}

			return rateEntriesRepository.FilteredEntries;
		}

		protected abstract void FilterAdditional();

		protected virtual void FilterLocations()
		{
			rateEntriesRepository.FilterEntries(FirstLoadFilter);
			rateEntriesRepository.FilterEntries(LastDischargeFilter);
			rateEntriesRepository.FilterEntries(FirstRouteSetLoadFilter);
			rateEntriesRepository.FilterEntries(LastRouteSetDischargeFilter);
		}

		protected string IsCrossTrade(ILocation origin, ILocation destination)
		{
			if (origin?.Country == null || destination?.Country == null)
			{
				return Res.GetString("EBC40C1A-6562-47F8-B14C-E4189C15290D", "Invalid location is identified on job. Unable to match location");
			}

			return
				criteria.Company.GC_RN_NKCountryCode == origin.Country.Code ||
				criteria.Company.GC_RN_NKCountryCode == destination.Country.Code
					? CustomizedReason(DiscardReporter.Reason.CrossTrade, null, criteria.Company.GC_Code, origin.Country.Code, destination.Country.Code)
					: string.Empty;
		}

		#region Base Filter

		void FilterBase(bool excludeFiltersForPossibleMatches)
		{
			rateEntriesRepository.FilterEntries(ShipmentConsolidationStatusFilter);
			rateEntriesRepository.FilterEntries(RatesWithScheduleInformationFilter);
			rateEntriesRepository.FilterEntries(RatesExpirationFilter.Filter);
			FilterContracts();
			FilterHBLDeliveryModes();

			//criteria.FreightMode could be FCL|BCN / LCL|BCN
			if (ShouldFilterContainer)
			{
				if (criteria.IsContainerised ||
					criteria.IsFullLoad ||
					criteria.ConsumerType == JobInvoicingConsumerTypes.WarehouseInwards ||
					criteria.ConsumerType == JobInvoicingConsumerTypes.CYDReceiveAdvice ||
					criteria.ConsumerType == JobInvoicingConsumerTypes.CYDReleaseAdvice ||
					criteria.ConsumerType == JobInvoicingConsumerTypes.CYDTransportationUnit)
				{
					rateEntriesRepository.FilterEntries(ContainerFilter);
					rateEntriesRepository.FilterEntries(ContainerQualityFilter);
					rateEntriesRepository.FilterEntries(YardUnitTypeFilter);
					rateEntriesRepository.FilterEntries(YardUnitLoadFilter);
				}
				else if (criteria.ConsumerType == JobInvoicingConsumerTypes.MNRWorkOrderHeader && !isAutoratingForCost)
				{
					rateEntriesRepository.FilterEntries(RefRepairCodeFilter);
					rateEntriesRepository.FilterEntries(RefUnitSectionFilter);
					rateEntriesRepository.FilterEntries(RefComponentCodeFilter);
					rateEntriesRepository.FilterEntries(RefContainerMaterialFilter);
				}
				// For NonContainerised(LCL), TI_RC should be empty
				else
				{
					Func<IRateEntry, string> containerFilter =
						entry => entry.TI_RC.IsEmpty
							? string.Empty
							: CustomizedReason(DiscardReporter.Reason.NotApplicableToJob, RateEntrySchema.TI_RC, entry.Container.RC_Code, criteria.FreightMode.ToString());
					rateEntriesRepository.FilterEntries(containerFilter);
				}
			}

			if (!excludeFiltersForPossibleMatches && !criteria.IsLooseRateSearchForCarrierConnect)
			{
				// Some of the below may also be implemented in RateEntryQueries.
				// Sometimes, the rates we want need to have these applied in the
				// application instead of directly to the database, so these and
				// the queries in that RateEntryQueries needs to be synchronised.
				rateEntriesRepository.FilterEntries(TransportProviderFilter);
				rateEntriesRepository.FilterEntries(ClientServiceLevelFilter);
				rateEntriesRepository.FilterEntries(CarrierServiceLevelFilter);
			}

			if (!excludeFiltersForPossibleMatches)
			{
				rateEntriesRepository.FilterEntries(CommodityFilter);
			}

			if (!criteria.IsLooseRateSearchForCarrierConnect)
			{
				rateEntriesRepository.FilterEntries(ConsignorConsigneeControllingCustomerFilter);
			}

			rateEntriesRepository.FilterEntries(WarehouseFilter);
			rateEntriesRepository.FilterEntries(GatewayAgentTypeFilter);
			rateEntriesRepository.FilterEntries(ShipmentGatewayServiceLevelFilter);
			rateEntriesRepository.FilterEntries(GatewayServiceLevelFilter);

			// === Slow Filters ===
			//
			// Add slow filters here so they get added to the end of the pipeline 
			// and execute last if other filters pass
			rateEntriesRepository.FilterEntries(SupplierFilter);
		}

		#endregion

		bool ShouldFilterContainer => !IsBCN && !IsSCN;

		bool IsBCN => RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.Value && criteria.IsBCNFreight;

		bool IsSCN => criteria.IsSCNFreight;

		#region Shipment Consolidation Status Filter

		string ShipmentConsolidationStatusFilter(IRateEntry entry)
		{
			if (entry.TI_ShipmentConsolidationStatus.IsEmpty || entry.TI_ShipmentConsolidationStatus == criteria.ShipmentConsolidationStatus)
			{
				return string.Empty;
			}

			return JobReason(RateEntrySchema.TI_ShipmentConsolidationStatus, criteria.ShipmentConsolidationStatus);
		}

		#endregion

		#region Rates With Schedule Information Filter

		string RatesWithScheduleInformationFilter(IRateEntry entry)
		{
			if (entry is WiseEntry wiseEntry)
			{
				if (wiseEntry.WiseRate?.BookingInfo?.Schedule?.ScheduleDetails?.Any() == true)
				{
					return ShouldLogEntryRemovalReason ? Res.GetString("ac2f237f-4e44-4234-b4f8-f56fd3de85a2", "Rate ID '{0}' discarded because it has schedule information and is considered as an spot rate", wiseEntry.WiseRate?.ProviderRateId) : string.Empty;
				}
			}

			return string.Empty;
		}

		#endregion

		#region Date Filter

		RateExpiryFilter RatesExpirationFilter
			=> ratesExpirationFilter ?? (ratesExpirationFilter = new RateExpiryFilter(this));
		RateExpiryFilter ratesExpirationFilter;

		/// <summary>
		/// Rate Expiry Filter - all invariant quantities are calculated once rather than for every rate.
		/// </summary>
		class RateExpiryFilter
		{
			readonly RateEntryFilter entryFilter;
			readonly FreightAutoRater.RatesToFindEnum ratesToFind;
			readonly ZDate today;
			readonly ZDate endDateLimit;

			public RateExpiryFilter(RateEntryFilter entryFilter)
			{
				this.entryFilter = entryFilter;
				ratesToFind = entryFilter.RatesToFind;
				if (ratesToFind == FreightAutoRater.RatesToFindEnum.JustExpiredRates)
				{
					today = ZDate.Today;
					endDateLimit = today.AddDays(-AutoRater.ExpiredRateNotificationDays);
				}
				else if (ratesToFind == FreightAutoRater.RatesToFindEnum.RatesGoingToExpire)
				{
					today = ZDate.Today;
					endDateLimit = today.AddDays(AutoRater.ExpiringRateNotificationDays);
				}
			}

			public string Filter(IRateEntry entry)
			{
				if (ratesToFind == FreightAutoRater.RatesToFindEnum.JustExpiredRates)
				{
					return entry.TI_RateEndDate < today && entry.TI_RateEndDate >= endDateLimit
						? string.Empty
						: Reason;
				}
				else if (ratesToFind == FreightAutoRater.RatesToFindEnum.RatesGoingToExpire)
				{
					return entry.TI_RateEndDate <= endDateLimit && entry.TI_RateEndDate >= today
						? string.Empty
						: Reason;
				}
				else
				{
					return string.Empty;
				}
			}

			string Reason
				=> reasonLazy ?? (reasonLazy = CreateReason());
			string reasonLazy;
			string CreateReason()
			{
				if (ratesToFind == FreightAutoRater.RatesToFindEnum.JustExpiredRates)
				{
					return entryFilter.CustomizedReason(DiscardReporter.Reason.DateBetween, RateEntrySchema.TI_RateEndDate, endDateLimit.ToShortDateString(), today.ToShortDateString());
				}
				else
				{
					return entryFilter.CustomizedReason(DiscardReporter.Reason.DateBetween, RateEntrySchema.TI_RateEndDate, today.ToShortDateString(), endDateLimit.ToShortDateString());
				}
			}
		}

		#endregion

		#region Container Filter

		string ContainerFilter(IRateEntry entry)
		{
			if (!entry.TI_RC.IsEmpty)
			{
				var allContainers = new List<RefContainer>();
				if (criteria.JobMeasures.HasContainerMeasure)
				{
					var containerTypes = criteria.JobMeasures.GetContainerTypePKs();

					foreach (var containerPk in containerTypes)
					{
						var containers = GetContainersInSameClass(containerPk);

						if (RatingHelper.ContainerMatch(containerPk, containers, entry))
						{
							return string.Empty;
						}

						if (containers != null)
						{
							allContainers.AddRange(containers);
						}
					}

					return JobReason(RateEntrySchema.TI_RC, allContainers.Select(x => x.RC_Code).ToArray());
				}
			}

			return string.Empty;
		}

		protected abstract RefContainerCollection GetContainersInSameClass(ZGuid containerPK);

		#endregion

		#region Container Quality Filter

		string ContainerQualityFilter(IRateEntry entry)
		{
			if (entry is WiseEntry wiseEntry && !string.IsNullOrEmpty(wiseEntry.ContainerQuality))
			{
				var qualities = criteria.RateableMeasures.GetDistinctContainerQualities();

				if (!qualities.Contains(wiseEntry.ContainerQuality))
				{
					return JobReason(nameof(WiseEntry.ContainerQuality), qualities.Select(x => new ZString(x)).ToArray());
				}
			}

			return string.Empty;
		}

		#endregion

		#region Yard Unit Filters

		string YardUnitTypeFilter(IRateEntry entry)
		{
			if (!string.IsNullOrEmpty(entry.TI_YardUnitType))
			{
				var rateableYardUnitTypes = criteria.RateableMeasures
					.GetPartList(MeasureType.Unit)
					?.Select(p => p is RateableContainer container ? new ZString(container.YardUnitType) : ZString.Empty)
					?.Distinct();

				if (rateableYardUnitTypes != null && !rateableYardUnitTypes.Contains(entry.TI_YardUnitType))
				{
					return JobReason(RateEntrySchema.TI_YardUnitType, rateableYardUnitTypes.ToArray());
				}
			}

			return string.Empty;
		}

		string YardUnitLoadFilter(IRateEntry entry)
		{
			if (!string.IsNullOrEmpty(entry.TI_YardUnitLoad))
			{
				var rateableYardUnitLoads = criteria.RateableMeasures
					.GetPartList(MeasureType.Unit)
					?.Select(p => p is RateableContainer container ? new ZString(container.YardUnitLoad) : ZString.Empty)
					?.Distinct();

				if (rateableYardUnitLoads != null && !rateableYardUnitLoads.Contains(entry.TI_YardUnitLoad))
				{
					return JobReason(RateEntrySchema.TI_YardUnitLoad, rateableYardUnitLoads.ToArray());
				}
			}

			return string.Empty;
		}

		#endregion

		#region Transport Provider Filter

		string TransportProviderFilter(IRateEntry entry)
		{
			if (!entry.TI_OH_TransportProvider.IsEmpty && !criteria.IsManualCostSelectSearchingCW1Rates)
			{
				var orgPKs = criteria.AllDistinctCarriersAndConsortiumOrgProxies;
				if (orgPKs.Count > 0)
				{
					return orgPKs.Contains(entry.TI_OH_TransportProvider)
						? string.Empty
						: ShouldLogEntryRemovalReason
							? JobReason(RateEntrySchema.TI_OH_TransportProvider, orgPKs.Select(x => criteria.GetOrgHeader(x)?.OH_Code ?? ZString.Empty).ToArray())
							: DummyError;
				}

				return JobReason(RateEntrySchema.TI_OH_TransportProvider, string.Empty);
			}

			return string.Empty;
		}

		#endregion

		#region Supplier Filter

		// Note: This filter is expensive. It loads all the ratelines
		// then gets the organisations from the creditors and the contractors
		// and then checks if the service provider on the rateentry matches
		// the determined list, otherwise, t checks if each organisation
		// consortium or related forward party - which is deep.
		string SupplierFilter(IRateEntry entry)
		{
			if (!entry.TI_OH_Supplier.IsEmpty)
			{
				var relevantChargeCodes = entry.ChildRateLines.Select(x => x.ChargeCode).Where(chargeCode => chargeCode != null).Distinct();
				var serviceProviderPKs = relevantChargeCodes.Select(criteria.GetTransportProvidersAndContractorPKs).SelectMany(i => i).Where(x => x.IsValid).Distinct();

				return serviceProviderPKs.Any(matchSupplier)
					? string.Empty
					: ShouldLogEntryRemovalReason
						? JobReason(RateEntrySchema.TI_OH_Supplier, serviceProviderPKs.Select(x => criteria.GetOrgHeader(x)?.OH_Code ?? ZString.Empty).ToArray())
						: DummyError;
			}

			return string.Empty;

			bool matchSupplier(ZGuid serviceProviderPK)
			{
				if (serviceProviderPK == entry.TI_OH_Supplier)
				{
					return true;
				}

				if (criteria.GetConsortiumOrgProxyForOrg(serviceProviderPK).Any(x => x == entry.TI_OH_Supplier))
				{
					return true;
				}

				return criteria.GetRelatedForwarderGroupParty(serviceProviderPK) == entry.TI_OH_Supplier;
			}
		}

		#endregion

		#region Service Level Filters

		/// <summary>
		///  This filter should remain in sync with the one in
		///  <see cref="RateEntryQueries"/>.
		/// </summary>
		string ClientServiceLevelFilter(IRateEntry entry)
		{
			var serviceLevel = criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Client);
			return entry.TI_RS_NKServiceLevel_NI.IsEmpty || entry.TI_RS_NKServiceLevel_NI == serviceLevel
				? string.Empty
				: JobReason(RateEntrySchema.TI_RS_NKServiceLevel_NI, entry.TI_RS_NKServiceLevel_NI, serviceLevel);
		}

		/// <summary>
		///  This filter should remain in sync with the one in
		///  <see cref="RateEntryQueries"/>.
		/// </summary>
		string CarrierServiceLevelFilter(IRateEntry entry)
		{
			var rateServiceLevel = entry.TI_PL_NKCarrierServiceLevel;
			if (rateServiceLevel.IsEmpty)
			{
				return string.Empty;
			}

			var criteriaServiceLevels = criteria.CarrierServiceLevelOverride ?? new List<ZString> { criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier) };
			if (!criteriaServiceLevels.Any() || criteriaServiceLevels.Contains(rateServiceLevel))
			{
				return string.Empty;
			}

			return JobReason(RateEntrySchema.TI_PL_NKCarrierServiceLevel, criteriaServiceLevels.ToArray());
		}

		string GatewayServiceLevelFilter(IRateEntry entry)
		{
			if (entry.IsIntercompanyTariff())
			{
				return criteria.GatewayServiceLevelFilteredReason(
					gatewayServiceLevel: entry.TI_RS_NKGatewayServiceLevel,
					gatewayAgentPk: entry.ParentRatingHeader?.TH_OH ?? ZGuid.Empty);
			}

			return string.Empty;
		}

		string ShipmentGatewayServiceLevelFilter(IRateEntry entry)
		{
			if (entry.IsIntercompanyTariff())
			{
				return
					entry.TI_RS_NKShipmentGatewayServiceLevel.IsEmpty ||
					entry.TI_RS_NKShipmentGatewayServiceLevel == GetStandardServiceLevel(criteria.ShipmentGatewayServiceLevel)
						? ZString.Empty
						: JobReason(RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel, criteria.ShipmentGatewayServiceLevel);
			}

			return string.Empty;

			ZString GetStandardServiceLevel(ZString serviceLevel)
				=> serviceLevel.IsEmpty ? new ZString("STD") : serviceLevel;
		}

		#endregion

		#region Commodity Code Filter

		string CommodityFilter(IRateEntry entry)
		{
			var jobReason = CommodityCodeFilter(entry.TI_RH_NKCommodityCode);

			return string.IsNullOrWhiteSpace(jobReason)
			  ? CommodityGroupFilter(entry.CommodityGroup)
			  : jobReason;
		}

		string CommodityCodeFilter(ZString rateEntryCommodityCode)
		{
			if (!rateEntryCommodityCode.IsEmpty)
			{
				var commodities = GetJobCommodityCodes();
				var commodityCodes = new List<ZString>();

				if (!commodities.Any() || commodities.Any(x => string.IsNullOrEmpty(x) || x == "GEN"))
				{
					if (rateEntryCommodityCode == (ZString)"GEN")
					{
						return string.Empty;
					}
				}

				foreach (var commodity in commodities)
				{
					ZString commodityCode = commodity;
					if (!commodityCode.IsEmpty && commodityCode != "GEN" && rateEntryCommodityCode == commodityCode)
					{
						return string.Empty;
					}
					commodityCodes.Add(commodityCode);
				}

				return JobReason(RateEntrySchema.TI_RH_NKCommodityCode, commodityCodes.ToArray());
			}

			return string.Empty;
		}

		IEnumerable<string> GetJobCommodityCodes()
		{
			var commodities = criteria
				.OverriddenCommodity
				?.Select(c => (string)c.RH_Code)
				.Where(c => !string.IsNullOrEmpty(c));
			if (isAutoratingForCost || commodities.IsNullOrEmpty())
			{
				// the OverriddenCommodity only apply for revenue autorating
				commodities = criteria.JobMeasures.GetCommodities();
			}

			return commodities;
		}

		string RefRepairCodeFilter(IRateEntry entry)
		{
			if (!entry.TI_RRC_RepairCode.IsEmpty)
			{
				var rateablePart = criteria.RateableMeasures.GetDistinctRefContainerInfo().SingleOrDefault(p => p is RefContainerInfoParts);
				var repairCodePK = rateablePart is RefContainerInfoParts containerInfo ? containerInfo.RefContainerRepair : Guid.Empty;
				if (repairCodePK != entry.TI_RRC_RepairCode)
				{
					var repairCode = factory.Load<RefRepairCode>(entry.TI_RRC_RepairCode);
					return JobReason(RateEntrySchema.TI_RRC_RepairCode, repairCode.RRC_Code);
				}
			}

			return string.Empty;
		}

		string RefUnitSectionFilter(IRateEntry entry)
		{
			if (!entry.TI_ContainerUnitSection.IsEmpty)
			{
				var rateablePart = criteria.RateableMeasures.GetDistinctRefContainerInfo().SingleOrDefault(p => p is RefContainerInfoParts);
				var unitSection = rateablePart is RefContainerInfoParts containerInfo ? containerInfo.RefUnitSection : String.Empty;
				if (unitSection != entry.TI_ContainerUnitSection)
				{
					return JobReason(RateEntrySchema.TI_ContainerUnitSection, unitSection);
				}
			}

			return string.Empty;
		}

		string RefComponentCodeFilter(IRateEntry entry)
		{
			if (!entry.TI_RCC_ComponentCode.IsEmpty)
			{
				var rateablePart = criteria.RateableMeasures.GetDistinctRefContainerInfo().SingleOrDefault(p => p is RefContainerInfoParts);
				var componentPK = rateablePart is RefContainerInfoParts containerInfo ? containerInfo.RefContainerComponent : Guid.Empty;
				if (componentPK != entry.TI_RCC_ComponentCode)
				{
					var componentCode = factory.Load<RefMRComponentCode>(entry.TI_RCC_ComponentCode);
					return JobReason(RateEntrySchema.TI_RCC_ComponentCode, componentCode.RCC_Code);
				}
			}

			return string.Empty;
		}

		string RefContainerMaterialFilter(IRateEntry entry)
		{
			if (!entry.TI_RMC_Material.IsEmpty)
			{
				var rateablePart = criteria.RateableMeasures.GetDistinctRefContainerInfo().SingleOrDefault(p => p is RefContainerInfoParts);
				var materialCodePK = rateablePart is RefContainerInfoParts containerInfo ? containerInfo.RefContainerMaterial : Guid.Empty;
				if (materialCodePK != entry.TI_RMC_Material)
				{
					var materialCode = factory.Load<RefMaterial>(entry.TI_RMC_Material);
					return JobReason(RateEntrySchema.TI_RMC_Material, materialCode.RMC_Code);
				}
			}

			return string.Empty;
		}

		string CommodityGroupFilter(ZString rateEntryCommodityGroup)
		{
			if (!rateEntryCommodityGroup.IsEmpty)
			{
				var criteriaCommodityCodes = GetJobCommodityCodes();
				var criteriaCommodities = criteria.Factory.Load<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, criteriaCommodityCodes));

				if (criteriaCommodities.Any(criteriaCommodity => criteriaCommodity.HasGroup(rateEntryCommodityGroup)))
				{
					return string.Empty;
				}

				return JobReason
				(
					RateEntrySchema.TI_RH_NKCommodityCode,
					criteriaCommodities.Select(criteriaCommodity => criteriaCommodity.RH_Code).ToArray()
				);
			}

			return string.Empty;
		}

		#endregion

		#region Location Filters

		string TranshipmentFilter(IRateEntry entry) =>
			LocationFilter(
				locationColumn: RateEntrySchema.TI_ViaLRC,
				rateLocationCode: entry.TI_ViaLRC,
				hasCriteriaLocationValue: criteria.GetVia(GetCostOrSell()) != null,
				criteriaLocationKey: LocationKey.Via);

		string FirstLoadFilter(IRateEntry entry) =>
			LocationFilter(
				locationColumn: RateEntrySchema.TI_FirstLoadLRC,
				rateLocationCode: entry.TI_FirstLoadLRC,
				hasCriteriaLocationValue: criteria.GetFirstLoad(GetCostOrSell()) != null,
				criteriaLocationKey: LocationKey.FirstLoad);

		string LastDischargeFilter(IRateEntry entry) =>
			LocationFilter(
				locationColumn: RateEntrySchema.TI_LastDischargeLRC,
				rateLocationCode: entry.TI_LastDischargeLRC,
				hasCriteriaLocationValue: criteria.GetLastDischarge(GetCostOrSell()) != null,
				criteriaLocationKey: LocationKey.LastDischarge);

		string FirstRouteSetLoadFilter(IRateEntry entry) =>
			LocationFilter(
				locationColumn: RateEntrySchema.TI_FirstRouteSetLoadPortLRC,
				rateLocationCode: entry.TI_FirstRouteSetLoadPortLRC,
				hasCriteriaLocationValue: criteria.GetFirstRouteSetLoad(GetCostOrSell()) != null,
				criteriaLocationKey: LocationKey.FirstRouteSetLoad);

		string LastRouteSetDischargeFilter(IRateEntry entry) =>
			LocationFilter(
				locationColumn: RateEntrySchema.TI_LastRouteSetDischargePortLRC,
				rateLocationCode: entry.TI_LastRouteSetDischargePortLRC,
				hasCriteriaLocationValue: criteria.GetLastRouteSetDischarge(GetCostOrSell()) != null,
				criteriaLocationKey: LocationKey.LastRouteSetDischarge);

		protected CostSell GetCostOrSell()
		{
			return isAutoratingForCost ? CostSell.Cost : CostSell.Revenue;
		}

		string LocationFilter(SchemaStringColumn locationColumn, ZString rateLocationCode, bool hasCriteriaLocationValue, string criteriaLocationKey)
		{
			if (!rateLocationCode.IsEmpty && hasCriteriaLocationValue)
			{
				var unmatchedLocations = new List<ZString>();

				foreach (var location in GetCriteriaLocations(criteriaLocationKey))
				{
					if (string.Equals(rateLocationCode, location, StringComparison.OrdinalIgnoreCase))
					{
						return string.Empty;
					}

					unmatchedLocations.Add(location);
				}

				return JobReason(locationColumn, unmatchedLocations.ToArray());
			}

			return rateLocationCode.IsEmpty ? string.Empty : JobReason(locationColumn, rateLocationCode);
		}

		IEnumerable<ZString> GetCriteriaLocations(string locationKey)
		{
			var location = ratingCriteriaLocationsCache.GetLocation(locationKey);

			yield return location.UNLOCO;
			yield return location.Country;

			foreach (var zone in location.Zones)
			{
				yield return zone;
			}
		}

		#endregion

		#region FMCTariffID

		protected string FMCTariffIDFilter(IRateEntry entry)
		{
			if (criteria.FMCTariffIDMatchEnabled && !isAutoratingForCost && RatingHeader.IsFMCTariffAllowed(entry.ParentRatingHeader.GetType()))
			{
				var isRateMatch =
					(entry.TI_FMCTariffID.IsEmpty || entry.TI_FMCTariffID == criteria.FMCTariffID);

				return
					isRateMatch
					? string.Empty
					: JobReason(RateEntrySchema.TI_FMCTariffID, criteria.FMCTariffID);
			}

			return string.Empty;
		}

		#endregion

		#region IsNonOperatingReefer

		protected string IsNonOperatingReeferFilter(IRateEntry entry)
		{
			if (!entry.TI_IsNonOperatedReefer.IsEmpty)
			{
				var reeferValues = criteria.JobMeasures.Measures.GetDistinctContainerIsNonOperatingReefers();

				return
					reeferValues.IsNullOrEmpty() || reeferValues.Contains(new ZBool(entry.TI_IsNonOperatedReefer))
					? string.Empty
					: JobReason(RateEntrySchema.TI_IsNonOperatedReefer, entry.TI_IsNonOperatedReefer);
			}

			return string.Empty;
		}

		#endregion

		#region Contract Number filter

		/// <summary>
		/// Filter entries from job's contract numbers.
		/// This is not implemented as other filters because one entry will need to compare with others and causes exponent complexity.
		/// It uses contract number configurations from specific rating adapters to fit filter requirements from each adapter.
		/// </summary>
		void FilterContracts()
		{
			var contractNumbers = isAutoratingForCost ? criteria.CarrierContractNumbers.ToArray() : criteria.ClientContractNumbers.ToArray();
			if (contractNumbers.Length == 0)
			{
				return;
			}

			var criteriaContractNumbers = new HashSet<string>(
				contractNumbers.Select(x => x.ToString()),
				StringComparer.OrdinalIgnoreCase);

			HashSet<ZGuid> entriesToRemove;
			var configuration = criteria.GetContractNumberConfiguration(GetCostOrSell());
			if (configuration != null && configuration.ShouldApplySpecificAdapterContractNumberFilter)
			{
				entriesToRemove = ApplySpecificAdapterContractNumberFilter(configuration, criteriaContractNumbers);
			}
			else
			{
				entriesToRemove = rateEntriesRepository.FilteredEntries
					.FilterByContractNumber(isAutoratingForCost)
					.Where(x => !x.TI_ContractNumber.IsEmpty && !criteriaContractNumbers.Contains(x.TI_ContractNumber))
					.Select(x => x.PK)
					.ToHashSet();
			}

			rateEntriesRepository.FilterEntries(entriesToRemove, JobReason(RateEntrySchema.TI_ContractNumber, contractNumbers));
		}

		void FilterHBLDeliveryModes()
		{
			if (!isAutoratingForCost)
			{
				var hblDeliveryMode = criteria.HBLDeliveryMode;
				var deliveryModesWithFallback = new[] { hblDeliveryMode };

				if (!string.IsNullOrWhiteSpace(hblDeliveryMode))
				{
					var hblDeliveryPriorityConfig = RatingDataRegistry
						.Instance
						.HBLDeliveryPriority
						.Value.Cast<HBLDeliveryPriorityConfig>()
						.FirstOrDefault(x => x.ContainerMode == criteria.ContainerMode && x.HBLDeliveryMode == hblDeliveryMode);

					var fallbacksInRegistry = hblDeliveryPriorityConfig?
						.Settings?
						.Cast<HBLDeliveryPrioritySetting>()?
						.Select(i => i.HBLDeliveryModePriority)?
						.Where(i => !i.IsEmpty)?
						.ToArray();

					if (fallbacksInRegistry?.Any() ?? false)
					{
						deliveryModesWithFallback = fallbacksInRegistry;
					}
				}

				var entriesToRemove = rateEntriesRepository.FilteredEntries
					.Where(x => !x.TI_HBLDeliveryMode.IsEmpty && !deliveryModesWithFallback.Contains(x.TI_HBLDeliveryMode))
					.Select(x => x.PK).ToHashSet();

				rateEntriesRepository.FilterEntries(entriesToRemove, JobReason(RateEntrySchema.TI_HBLDeliveryMode, deliveryModesWithFallback));
			}
		}

		HashSet<ZGuid> ApplySpecificAdapterContractNumberFilter(IContractNumberConfiguration configuration, HashSet<string> criteriaContractNumbers)
		{
			var entriesToRemove = new HashSet<ZGuid>();

			switch (isAutoratingForCost)
			{
				// Revenue + don't apply client contract number filter
				case false when configuration.ShouldIgnoreJobClientContractNumbers:
					return entriesToRemove;

				// Costing + don't apply carrier contract number filter. Or Costing + manual select CW1 rate from RSL
				case true when criteria.IsManualCostSelectSearchingCW1Rates ||
					!criteria.IsManualCostSelectMode &&
					configuration.ShouldIgnoreJobCarrierContractNumbers:
					return entriesToRemove;
			}

			var groupedEntries = rateEntriesRepository.FilteredEntries
				.FilterByContractNumber(isAutoratingForCost)
				.GroupBy(x => !x.TI_ContractNumber.IsEmpty)
				.ToArray();
			var numberMatchedCache = new HashSet<(ZString, ZGuid?)>();

			var entriesHavingNumbers = groupedEntries.FirstOrDefault(x => x.Key);
			if (entriesHavingNumbers != null)
			{
				foreach (var entry in entriesHavingNumbers)
				{
					if (criteriaContractNumbers.Contains(entry.TI_ContractNumber))
					{
						numberMatchedCache.Add((entry.TI_RateCategory, entry.ParentRatingHeader?.TH_OH));
					}
					else
					{
						entriesToRemove.Add(entry.PK);
					}
				}
			}

			var entriesNoNumbers = groupedEntries.FirstOrDefault(x => !x.Key);
			if (entriesNoNumbers == null)
			{
				return entriesToRemove;
			}

			var jobHasBlankNumber = criteriaContractNumbers.Contains(ZString.Empty);
			if (!configuration.ShouldMatchJobBlankContractNumber && (!isAutoratingForCost || jobHasBlankNumber))
			{
				// Do not filter blank number rates when strict matching for bank contract number doesn't apply:
				// - Revenue: accepts all empty number rates.
				// - Costing: job has empty number.
				return entriesToRemove;
			}

			// Filter blank number Revenue rates with: Strict match and job has no blank contract number.
			if (configuration.ShouldMatchJobBlankContractNumber && !isAutoratingForCost && !jobHasBlankNumber)
			{
				entriesNoNumbers.ForEach(x => entriesToRemove.Add(x.PK));
				return entriesToRemove;
			}

			// Now,
			//	For revenue:
			//		- Strict match, Job has blank number.
			//
			//	For cost:
			//		- Loose match, Job doesn't have blank number.
			//		- Strict match, Job has or doesn't have blank number.

			// Remove blank-number Freight Cost rates when job doesn't have blank number.
			if (!jobHasBlankNumber && criteriaContractNumbers.Count == 1)
			{
				entriesToRemove.UnionWith(entriesNoNumbers.Where(e => e.IsFreightEntry()).Select(e => e.PK));
			}

			var skippedCategories = new HashSet<ZString>
			{
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.DST,
			};

			// If matched non-blank number rates are found, empty ones can be filtered.
			// Except for ORG/DST rates. ContractNumberComparer will sort out same charge codes.
			foreach (var entry in entriesNoNumbers)
			{
				if (!skippedCategories.Contains(entry.TI_RateCategory) && numberMatchedCache.Contains((entry.TI_RateCategory, entry.ParentRatingHeader?.TH_OH)))
				{
					entriesToRemove.Add(entry.PK);
				}
			}

			return entriesToRemove;
		}

		#endregion

		#region Warehouse Filter

		string WarehouseFilter(IRateEntry entry)
		{
			if (entry.TI_WW_Warehouse.IsEmpty)
			{
				return string.Empty;
			}

			return criteria.WarehousePK.IsEmpty || entry.TI_WW_Warehouse == criteria.WarehousePK
				? string.Empty
				: JobReason(WhsWarehouseSchema.WW_WarehouseCode, entry.Warehouse().WW_WarehouseCode, factory.Load<IWhsWarehouse>(criteria.WarehousePK).WW_WarehouseCode);
		}

		#endregion

		#region Consignor/Consignee Filter

		string ConsignorConsigneeControllingCustomerFilter(IRateEntry entry)
		{
			var results = new[] { ControllingCustomerFilter(entry), ConsignorFilter(entry), ConsigneeFilter(entry) };

			return results.All(x => string.IsNullOrEmpty(x))
				? string.Empty
				: string.Join(ReasonDelimitor, results.Where(x => !string.IsNullOrEmpty(x)));
		}

		string ConsignorFilter(IRateEntry entry)
		{
			return BuildConsignorConsigneeAddressPostcodeFilter(
				entry.TI_OH_Consignor,
				entry,
				entry.TI_OA_CartagePickupAddressOverride,
				entry.TI_CartagePickupAddressPostCode,
				criteria.Consignor ?? criteria.WarehouseFallbackConsignorForFilterOnly,
				criteria.PickupAddress,
				criteria.ConsignorDocumentaryAddress,
				isConsignor: true);
		}

		string ConsigneeFilter(IRateEntry entry)
		{
			return BuildConsignorConsigneeAddressPostcodeFilter(
				entry.TI_OH_Consignee,
				entry,
				entry.TI_OA_CartageDeliveryAddressOverride,
				entry.TI_CartageDeliveryAddressPostCode,
				criteria.Consignee,
				criteria.DeliveryAddress,
				criteria.ConsigneeDocumentaryAddress,
				isConsignor: false);
		}

		string ControllingCustomerFilter(IRateEntry entry)
		{
			var controllingCustomer = criteria.DebtorOrgs[RatingDebtorOrgTypes.CCUS];

			if (entry.IsContainerYardTPU() || entry.TI_OH_ControllingCustomer.IsEmpty || entry.TI_OH_ControllingCustomer == controllingCustomer?.PK)
			{
				return string.Empty;
			}

			if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value && entry.IsIntercompanyTariff())
			{
				return criteria.SortedControllingCustomerPKs.Contains(entry.TI_OH_ControllingCustomer)
					? string.Empty
					: JobReason(RateEntrySchema.TI_OH_ControllingCustomer, entry.ControllingCustomer.OH_Code, controllingCustomer?.OH_Code ?? ZString.Empty);
			}

			return JobReason(RateEntrySchema.TI_OH_ControllingCustomer, entry.ControllingCustomer.OH_Code, controllingCustomer?.OH_Code ?? ZString.Empty);
		}

		string BuildConsignorConsigneeAddressPostcodeFilter(
			ZGuid entryOrgPk,
			IRateEntry entry,
			ZGuid entryAddressPk,
			ZString entryPostcode,
			OrgHeader jobConsignorOrConsignee,
			IDocAddress jobPickupOrDeliveryAddress,
			IDocAddress jobDocumentaryAddress,
			bool isConsignor)
		{
			var lazyJobAddress = new Lazy<OrgAddress>(() =>
				jobPickupOrDeliveryAddress != null
					&& jobPickupOrDeliveryAddress.E2_OA_Address.IsValid
						? factory.Load<OrgAddress>(jobPickupOrDeliveryAddress.E2_OA_Address)
						: null
			);

			var byOrgResult = AddressFilterByOrg(entryOrgPk, jobConsignorOrConsignee, isConsignor, lazyJobAddress);
			if (!string.IsNullOrEmpty(byOrgResult))
			{
				return byOrgResult;
			}

			var byAddressResult = AddressFilterByAddress(entry, entryAddressPk, jobPickupOrDeliveryAddress, jobDocumentaryAddress, isConsignor, lazyJobAddress);
			if (!string.IsNullOrEmpty(byAddressResult))
			{
				return byAddressResult;
			}

			return AddressFilterByPostcode(entryPostcode, jobPickupOrDeliveryAddress, jobDocumentaryAddress, isConsignor);
		}

		string AddressFilterByPostcode(ZString entryPostcode, IDocAddress jobPickupOrDeliveryAddress, IDocAddress jobDocumentaryAddress, bool isConsignor)
		{
			ZString jobPostcode = ZString.Empty;
			var isPostcodeMatched = entryPostcode.IsEmpty;
			if (!isPostcodeMatched && jobPickupOrDeliveryAddress != null)
			{
				jobPostcode = jobPickupOrDeliveryAddress.E2_Postcode;
				if (jobPostcode.IsEmpty)
				{
					jobPostcode = jobDocumentaryAddress?.E2_Postcode ?? ZString.Empty;
				}

				isPostcodeMatched = entryPostcode == jobPostcode
								 || entryPostcode.Length >= 3 && jobPostcode.StartsWith(entryPostcode);
			}

			if (!isPostcodeMatched)
			{
				var postCodeSchema = isConsignor
					? RateEntrySchema.TI_CartagePickupAddressPostCode
					: RateEntrySchema.TI_CartageDeliveryAddressPostCode;

				return JobReason(postCodeSchema, entryPostcode, jobPostcode);
			}

			return string.Empty;
		}

		string AddressFilterByAddress(IRateEntry entry, ZGuid entryAddressPk, IDocAddress jobPickupOrDeliveryAddress, IDocAddress jobDocumentaryAddress, bool isConsignor, Lazy<OrgAddress> lazyJobAddress)
		{
			var isAddressMatched =
				entryAddressPk.IsEmpty
				|| (jobPickupOrDeliveryAddress != null
					&& ((!jobPickupOrDeliveryAddress.E2_AddressOverride && entryAddressPk == jobPickupOrDeliveryAddress.E2_OA_Address) // Pickup address when not overridden
					|| (jobPickupOrDeliveryAddress.E2_AddressOverride && entryAddressPk == jobDocumentaryAddress?.E2_OA_Address))); // fallback to Documentary address
			if (!isAddressMatched)
			{
				var addressColumnName = isConsignor
					? RateEntrySchema.TI_OA_CartagePickupAddressOverride
					: RateEntrySchema.TI_OA_CartageDeliveryAddressOverride;
				var entryAddressCode = isConsignor
					? entry.CartagePickupAddressOverride.OA_Code
					: entry.CartageDeliveryAddressOverride.OA_Code;

				return JobReason(addressColumnName, entryAddressCode, lazyJobAddress.Value?.OA_Code ?? ZString.Empty);
			}
			return string.Empty;
		}

		string AddressFilterByOrg(ZGuid entryOrgPk, OrgHeader jobConsignorOrConsignee, bool isConsignor, Lazy<OrgAddress> lazyJobAddress)
		{
			var isOrgMatched =
				entryOrgPk.IsEmpty
				|| (jobConsignorOrConsignee != null && entryOrgPk == jobConsignorOrConsignee.PK)
				|| IsActiveForOrg(lazyJobAddress.Value, entryOrgPk);
			if (!isOrgMatched)
			{
				return JobReason(isConsignor ? RateEntrySchema.TI_OH_Consignor : RateEntrySchema.TI_OH_Consignee, jobConsignorOrConsignee?.OH_Code ?? string.Empty);
			}

			return string.Empty;
		}

		static bool IsActiveForOrg(OrgAddress address, ZGuid orgPk)
			=> address != null && address.OA_IsActive && address.OA_OH == orgPk;

		#endregion

		#region Rate Category Filter

		protected string RateCategoryFilter(RateCategoryGroup rateCategoryGroup, IRateEntry entry, RatingCriteria criteria)
		{
			var rateCategories = criteria.GetRateCategories(rateCategoryGroup);
			return rateCategories.Contains(entry.TI_RateCategory.ToString())
				? string.Empty
				: JobReason(RateEntrySchema.TI_RateCategory, rateCategories.Select(x => (ZString)x).ToArray());
		}

		#endregion

		#region Aircraft Type Filter

		protected string AircraftTypeFilter(IRateEntry entry)
		{
			return entry.TI_AircraftType.IsEmpty || entry.TI_AircraftType == criteria.AircraftType
				? string.Empty
				: JobReason(RateEntrySchema.TI_AircraftType, criteria.AircraftType);
		}

		#endregion

		#region Origin Destination Location

		protected IEnumerable<ZString> GetCriteriaOrigin(bool isFreightFallBack = false)
		{
			var originLocation = ratingCriteriaLocationsCache.GetLocation(LocationKey.Origin);
			var rateOriginLocation = ratingCriteriaLocationsCache.GetLocation(LocationKey.RateOrigin);

			if (isFreightFallBack)
			{
				yield return originLocation.UNLOCO;
				yield return rateOriginLocation.UNLOCO;
			}

			yield return originLocation.IATACityCode;
			yield return originLocation.Country;
			yield return rateOriginLocation.IATACityCode;
			yield return rateOriginLocation.Country;

			foreach (var zone in originLocation.Zones)
			{
				yield return zone;
			}

			foreach (var zone in rateOriginLocation.Zones)
			{
				yield return zone;
			}
		}

		protected IEnumerable<ZString> GetCriteriaDestinations(bool isFreightFallBack = false)
		{
			var destinationLocation = ratingCriteriaLocationsCache.GetLocation(LocationKey.Destination);
			var rateDestinationLocation = ratingCriteriaLocationsCache.GetLocation(LocationKey.RateDestination);

			if (isFreightFallBack)
			{
				yield return destinationLocation.UNLOCO;
				yield return rateDestinationLocation.UNLOCO;
			}

			yield return destinationLocation.IATACityCode;
			yield return destinationLocation.Country;

			yield return rateDestinationLocation.IATACityCode;
			yield return rateDestinationLocation.Country;

			foreach (var zone in destinationLocation.Zones)
			{
				yield return zone;
			}

			foreach (var zone in rateDestinationLocation.Zones)
			{
				yield return zone;
			}
		}

		protected IEnumerable<ZString> GetLocationCodes(IEnumerable<RatingCriteriaLocationsCache.LocationInfo> infos)
		{
			foreach (var info in infos)
			{
				yield return info.IATACityCode;
				yield return info.Country;
			}

			foreach (var info in infos)
			{
				foreach (var zone in info.Zones)
				{
					yield return zone;
				}
			}
		}

		#endregion

		#region Planned Load Planned Discharge

		IEnumerable<ZString> GetAllLocationsApplicableToPlannedLoad()
		{
			var plannedLoads = new List<ZString>();
			plannedLoads.AddRange(GetCriteriaPlannedLoads());

			if (IsGatewayConfigured)
			{
				plannedLoads.AddRange(GetCriteriaOverridenPlannedLoads());
			}

			return plannedLoads;
		}

		IEnumerable<ZString> GetCriteriaPlannedLoads()
		{
			var location = ratingCriteriaLocationsCache.GetLocation(LocationKey.PlannedLoad);

			yield return location.UNLOCO;
			yield return location.IATACityCode;
			yield return location.Country;

			foreach (var zone in location.Zones)
			{
				yield return zone;
			}
		}

		IEnumerable<ZString> GetCriteriaOverridenPlannedLoads()
		{
			foreach (var location in ratingCriteriaLocationsCache[LocationKey.SortedOverridenPlannedLoads])
			{
				yield return location.UNLOCO;
				yield return location.IATACityCode;
				yield return location.Country;

				foreach (var zone in location.Zones)
				{
					yield return zone;
				}
			}
		}

		IEnumerable<ZString> GetAllLocationsApplicableToPlannedDischarge()
		{
			var plannedDischarge = new List<ZString>();
			plannedDischarge.AddRange(GetCriteriaPlannedDischarges());

			if (IsGatewayConfigured)
			{
				plannedDischarge.AddRange(GetCriteriaOverridenPlannedDischarges());
			}

			return plannedDischarge;
		}

		IEnumerable<ZString> GetCriteriaPlannedDischarges()
		{
			var plannedDischargeLocation = ratingCriteriaLocationsCache.GetLocation(LocationKey.PlannedDischarge);

			yield return plannedDischargeLocation.UNLOCO;
			yield return plannedDischargeLocation.IATACityCode;
			yield return plannedDischargeLocation.Country;

			foreach (var zone in plannedDischargeLocation.Zones)
			{
				yield return zone;
			}
		}

		IEnumerable<ZString> GetCriteriaOverridenPlannedDischarges()
		{
			foreach (var location in ratingCriteriaLocationsCache[LocationKey.SortedOverridenPlannedDischarges])
			{
				yield return location.UNLOCO;
				yield return location.IATACityCode;
				yield return location.Country;

				foreach (var zone in location.Zones)
				{
					yield return zone;
				}
			}
		}

		/// <summary>
		/// Some gateway scenarios don't specify a GatewayBillingSupporter (and therefore IsGateway is false),
		/// so we also check if any gateway agents are configured for this job.
		/// </summary>
		bool IsGatewayConfigured => criteria.GatewayConfiguration.IsGateway || criteria.GatewayAgentPKsForIntercompanyTariff.Count > 0;

		string PlannedLoadFilter(IRateEntry entry)
		{
			if (entry.TI_PlannedLoadLRC.IsEmpty)
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetAllLocationsApplicableToPlannedLoad())
			{
				if (string.Equals(entry.TI_PlannedLoadLRC, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			return JobReason(RateEntrySchema.TI_PlannedLoadLRC, unmatchedLocations.ToArray());
		}

		string PlannedDischargeFilter(IRateEntry entry)
		{
			if (entry.TI_PlannedDischargeLRC.IsEmpty)
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetAllLocationsApplicableToPlannedDischarge())
			{
				if (string.Equals(entry.TI_PlannedDischargeLRC, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			return JobReason(RateEntrySchema.TI_PlannedDischargeLRC, unmatchedLocations.ToArray());
		}

		IEnumerable<ZString> GetCriteriaRateOrigins()
		{
			var rateOriginLocation = ratingCriteriaLocationsCache.GetLocation(LocationKey.RateOrigin);

			yield return rateOriginLocation.UNLOCO;
			yield return rateOriginLocation.IATACityCode;
			yield return rateOriginLocation.Country;

			foreach (var zone in rateOriginLocation.Zones)
			{
				yield return zone;
			}
		}

		IEnumerable<ZString> GetCriteriaRateDestinations()
		{
			var rateDestinationLocation = ratingCriteriaLocationsCache.GetLocation(LocationKey.RateDestination);

			yield return rateDestinationLocation.UNLOCO;
			yield return rateDestinationLocation.IATACityCode;
			yield return rateDestinationLocation.Country;

			foreach (var zone in rateDestinationLocation.Zones)
			{
				yield return zone;
			}
		}

		string RateOriginFilter(IRateEntry entry)
		{
			if (entry.TI_RateOrigin.IsEmpty)
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetCriteriaRateOrigins())
			{
				if (string.Equals(entry.TI_RateOrigin, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			return JobReason(RateEntrySchema.TI_RateOrigin, unmatchedLocations.ToArray());
		}

		string RateDestinationFilter(IRateEntry entry)
		{
			if (entry.TI_RateDestination.IsEmpty)
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetCriteriaRateDestinations())
			{
				if (string.Equals(entry.TI_RateDestination, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			return JobReason(RateEntrySchema.TI_RateDestination, unmatchedLocations.ToArray());
		}

		#endregion

		#region Intercompany Tariff Filter

		string IntercompanyTariffFilter(IRateEntry entry) =>
			entry.IsIntercompanyTariff()
				? Res.GetString("5BD4D3F8-D516-494F-8A85-6B565B5F30C7", "It is not applicable for Intercompany Tariffs.")
				: string.Empty;

		#endregion

		#region Gateway Agent Type

		string GatewayAgentTypeFilter(IRateEntry entry)
		{
			if (entry.IsIntercompanyTariff())
			{
				return criteria.GatewayAgentTypeFilteredReason(entry.TI_GatewayAgentType, entry.ParentRatingHeader.TH_OH, _Rating.BillingType, _Rating.CostOrSell);
			}

			return string.Empty;
		}

		#endregion

		#region Filter Logging

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "logging constant")]
		const string DummyError = "Error";

		public bool ShouldLogEntryRemovalReason { get; set; } = true;

		protected string JobReason(string columnName, params ZString[] jobValues)
		{
			return ShouldLogEntryRemovalReason ? DiscardReporter.DiscardReason(DiscardReporter.Reason.VsJob, columnName, jobValues) : DummyError;
		}

		protected string JobReason(SchemaColumn column, params ZString[] jobValues)
		{
			return ShouldLogEntryRemovalReason ? DiscardReporter.DiscardReason(DiscardReporter.Reason.VsJob, column, jobValues) : DummyError;
		}

		protected string CustomizedReason(DiscardReporter.Reason reasonType, SchemaColumn column, params ZString[] jobValues)
		{
			return ShouldLogEntryRemovalReason ? DiscardReporter.DiscardReason(reasonType, column, jobValues) : DummyError;
		}

		#endregion

		#endregion
	}
}
