using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business.AutoRating.RatesLoad
{
	public class WiseRatesQueryBuilder : IWiseRatesQueryBuilder
	{
		public static class LogMessages
		{
			#region SuppressResourceStringsCheckRegion
			public static string ServiceProviderError => "None of Job's Service Providers/Creditors can be used for rates search";
			public static string ContainerError => "None of Job's containers can be used for rates search";
			public static string UnknownFreightError => "Job is neither Air Freight nor Sea Freight";
			public static string GeneralErrorTemplate => "Request will not be sent to Rates Service because {0}";
			public static string CarrierIsNotShippingProviderErrorTemplate => "{0} ({1}) Service Provider/Creditor will not be included into the search request to Rates Service as it is not marked as Carrier.";
			public static string CarrierCodeIsNotProvidedErrorTemplate => "{0} carrier ({1}) will not be included into the search request to Rates Service as it has no code mapping, SCAC, IATA or C1Code code specified.";
			public static string ContainerCodeIsNotProvidedErrorTemplate => "{0} container will not be included into the search request to Rates Service as it has no code mapping or ISO type specified";
			#endregion
		}
		public WiseRatesQueryBuilder(ILogger logger)
		{
			Argument.NotNull(logger, nameof(logger));

			this.logger = logger;
		}

		public (RatesQuery query, string error) Build(RatingCriteria criteria, IEnumerable<OrgWithSource> overridenCriteriaCarriers = null, IEnumerable<string> contractNumbersFromFilters = null, IEnumerable<OrgWithSource> carriersFromFilters = null)
		{
			var errors = new List<string>();

			var criteriaCarriers = overridenCriteriaCarriers ?? criteria.GetCostsSearchOrgs().WhereNotNull();
			var carriers = GetCarriers(criteriaCarriers);
			if (criteriaCarriers.Any() && !carriers.ratesQueryCarriers.Any())
			{
				errors.Add(LogMessages.ServiceProviderError);
			}

			if (carriersFromFilters.IsNullOrEmpty() && carriers.ratesQueryCarrierSource.Any())
			{
				//now we have Autorating date filtering for service provider, we should use it to get effective date
				carriersFromFilters = carriers.ratesQueryCarrierSource;
			}

			var freightEffectiveDate = GetEffectiveDate(criteria, contractNumbersFromFilters: contractNumbersFromFilters, carriersFromFilters: carriersFromFilters);
			var effectiveDate = freightEffectiveDate.IsEmpty ? ZDate.Today : freightEffectiveDate;

			var criteriaContainers = criteria.GetContainers();
			var containers = GetContainers(criteriaContainers, criteria.FreightMode);
			if (criteriaContainers.Any() && !containers.Any())
			{
				errors.Add(LogMessages.ContainerError);
			}

			var zoneOwners = criteria.ZoneOwnerOrganizations();
			var origins = GetLocations(criteria.Origin, zoneOwners);
			var destinations = GetLocations(criteria.Destination, zoneOwners);
			var contracts = criteria.CarrierContractNumbers
				.Where(c => !c.IsEmpty)
				.Select(c => new RatesQueryContract { ContractNumber = c });

			if (!criteria.IsAirFreight && !criteria.IsSeaFreight)
			{
				errors.Add(LogMessages.UnknownFreightError);
			}

			var transportMode = criteria.IsAirFreight ? WRConstants.TransportModes.AIR : criteria.IsSeaFreight ? WRConstants.TransportModes.SEA : null;
			var containerMode = criteria.ContainerMode;

			if (containerMode == Core.Constants.ContainerModes.Empty)
			{
				containerMode = RatingConstants.GetContainerModeFromMode(criteria.FreightMode.ToString());
			}

			if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(transportMode, containerMode, out string reason))
			{
				errors.Add(string.Format(LogMessages.GeneralErrorTemplate, reason));
			}

			var ratesQuery = new RatesQuery
			{
				Origin = origins,
				Destination = destinations,
				Carrier = carriers.ratesQueryCarriers,
				EffectiveDate = effectiveDate.ToDateTime(),
				EndDate = effectiveDate.ToDateTime(),   // we cannot omit EndDate because in such a case Rates Service will treat it as open EndDate
				Container = containers,
				Contract = contracts,
				ContainerMode = criteria.IsContainerised ? new[] { WRConstants.ContainerModes.FCL } : new[] { WRConstants.ContainerModes.LCL },
				TransportMode = transportMode != null ? new[] { transportMode } : null,
				JobID = criteria.JobID
			};

			return (ratesQuery, errors.ToStringWithNewLineBetweenStrings());
		}

		public string[] GetLocations(ILocation location, OrgHeader[] zoneOwners)
		{
			if (location == null)
			{
				return Array.Empty<string>();
			}

			var result = new List<string>
			{
				location.Code,
				location.UNLOCO?.Code,
				location.IATACityCode?.Code,
				location.Country?.RN_Code,
			};

			var zones = RatingZoneRetriever.GetApplicableWiseRatesZones(location.Zones, zoneOwners);
			result.AddRange(zones.Select(zone => (string)zone));

			return result.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
		}

		public (RatesQueryCarrier[] ratesQueryCarriers, IEnumerable<OrgWithSource> ratesQueryCarrierSource) GetCarriers(IEnumerable<OrgWithSource> carriers)
		{
			var result = new List<RatesQueryCarrier>();
			var carriersSource = new List<OrgWithSource>();

			foreach (var org in carriers.WhereNotNull())
			{
				if (!org.Org.OH_IsShippingProvider)
				{
					logger.Warning(string.Format(CultureInfo.InvariantCulture, LogMessages.CarrierIsNotShippingProviderErrorTemplate, org.Org.OH_Code, org.SourceText));
					continue;
				}

				var orgHeader = org.Org;
				var refShippingLine = orgHeader.ShippingLine;
				var scacCode = refShippingLine?.RSL_StandardCarrierAlphaCode ?? ZString.Empty;
				var iataCode = orgHeader.MiscServ.Airline?.RM_TwoCharacterCode ?? ZString.Empty;
				var c1Code = refShippingLine?.RSL_CargoWiseOneCode ?? ZString.Empty;

				if (scacCode.IsEmpty && iataCode.IsEmpty && c1Code.IsEmpty)
				{
					logger.Warning(string.Format(CultureInfo.InvariantCulture, LogMessages.CarrierCodeIsNotProvidedErrorTemplate, orgHeader.OH_Code, org.SourceText));
					continue;
				}

				result.Add(new RatesQueryCarrier
				{
					IATACode = iataCode.IsEmpty ? null : iataCode.ToString(),
					SCACCode = scacCode.IsEmpty ? null : scacCode.ToString(),
					C1Code = c1Code.IsEmpty ? null : c1Code.ToString(),
					Source = org.SourceText,
					Name = orgHeader.OH_FullName
				});

				carriersSource.Add(org);
			}

			return (result.ToArray(), carriersSource);
		}

		public static ZDate GetEffectiveDate(RatingCriteria criteria, string chargeGroup = ChargeCodeGroupList.Codes.Freight, IEnumerable<string> contractNumbersFromFilters = null, IEnumerable<OrgWithSource> carriersFromFilters = null)
		{
			var contractConfig = criteria?.GetContractNumberConfiguration(CostSell.Cost);
			var singleContract = (contractConfig?.ShouldUseCarrierContractDateFilter ?? false) && contractNumbersFromFilters?.Count() == 1 ? contractNumbersFromFilters.First() : string.Empty;
			var singleCarrier = carriersFromFilters?.Count() == 1 ? carriersFromFilters.First() : null;
			IRatingContract ratingContract = null;

			//if we have only one contract and one service provider in Rate Selector and the contract belongs to service provider, we should use its Date settings
			if (!string.IsNullOrEmpty(singleContract) && singleCarrier?.Org != null)
			{
				var query = new ZQuery(RatingContractSchema.RCT_ContractNumber, singleContract);
				query.AddToFilter(RatingContractSchema.RCT_ContractType, RatingContractTypes.Provider);
				query.AddToFilter(RatingContractSchema.RCT_OH, singleCarrier.Org.PK);
				query.AddToFilter(RatingContractSchema.RCT_IsActive, true);

				var contract = criteria.Factory.LoadTop1<IRatingContract>(query);

				if (contract != null)
				{
					ratingContract = contract;
				}
			}

			// Include the invalid date when fallback is enabled because in that case we want to get another one from the standard configuration.
			// Different from RateLine filter where we want to remove the line if the date is invalid.
			var (rateDateType, actualDateOfHighestPriority) = criteria?.GetEffectiveDate(null, chargeGroup, shouldAcceptInvalidDateWhenFallbackIsEnabled: true, overridenContractNumber: ratingContract, overriddenCostProvider: singleCarrier?.Org) ?? (null, ZDate.Empty);
			var isFallbackDisabled = rateDateType?.IsFallbackDisabled ?? false;

			// Looks like we repeat it again with standard date type below.
			// The date returned from a higher ranked date type can be invalid or empty. In that case we want to fallback to the standard date type.
			if (!isFallbackDisabled && (!actualDateOfHighestPriority.IsValid || actualDateOfHighestPriority.IsEmpty))
			{
				var standardDateType = JobDateTypeRetriever.GetStandardJobDateTypeByChargeGroup(chargeGroup).DateType;
				actualDateOfHighestPriority = criteria?.JobDatesProvider?.GetJobDateByType(standardDateType).Date ?? ZDate.Empty;
			}

			return actualDateOfHighestPriority;
		}

		public RatesQueryContainer[] GetContainers(IEnumerable<RefContainer> containers, IEnumerable<string> transportModes)
		{
			var result = new List<RatesQueryContainer>();

			var isSea = transportModes.Contains(Core.Constants.TransportModes.Sea);
			foreach (var container in containers)
			{
				var isoType = !container.RC_ISOType.IsEmpty ? (string)container.RC_ISOType : null;
				var iataRateClass = !container.RC_IATARateClass.IsEmpty ? (string)container.RC_IATARateClass : null;
				var isoTypeGroups = GetIsoTypeGroups(container);

				if (string.IsNullOrEmpty(isoType) && isSea)
				{
					logger.Warning(string.Format(CultureInfo.InvariantCulture, LogMessages.ContainerCodeIsNotProvidedErrorTemplate, container.RC_Code));
					continue;
				}

				if (container.RC_ShippingMode == RefContainerLookups.ShippingModes.Air)
				{
					result.Add(new RatesQueryContainer { Code = container.RC_Code, IATAULDRateClass = iataRateClass, ISOTypeGroups = isoTypeGroups });
				}
				else
				{
					result.Add(new RatesQueryContainer { Code = container.RC_Code, ISOType = isoType, IATAULDRateClass = iataRateClass, ISOTypeGroups = isoTypeGroups });
				}
			}

			return result.ToArray();
		}

		public static string[] GetIsoTypeGroups(RefContainer container)
		{
			var isoGroupList = new ContainerISOTypeGroupList();
			var groups = new[] {
					(string)container.RC_FreightRateClass,
					(string)container.RC_HandlingRateClass
				}
				.WhereNotNull().Distinct()
				.Where(c => isoGroupList.IsISOTypeGroup(c))
				.ToArray();

			if (groups.IsNullOrEmpty())
			{
				return null;
			}

			return groups;
		}

		public RatesQueryContainer[] GetContainers(IEnumerable<RefContainer> containers, FreightMode freightMode)
		{
			var isAir = (freightMode & FreightMode.AIR) != 0;
			var isSea = (freightMode & FreightMode.SEA) != 0;

			var modes = new List<string>();
			if (isAir)
			{ modes.Add(Core.Constants.TransportModes.Air); }
			if (isSea)
			{ modes.Add(Core.Constants.TransportModes.Sea); }

			return GetContainers(containers, modes);
		}

		public void AddMeasureChargeableVolume(RatesQuery query, RatingCriteria criteria)
		{
			var chargeableVolume = criteria.RateableMeasures.GetQuantity(Integration.MeasureType.Chargeable);
			if (Core.Constants.Volume.ContainsCode(chargeableVolume.Unit))
			{
				AddMeasure(query, new global::WiseRates.Api.Model.MeasureInfo()
				{
					Type = global::WiseRates.Api.Model.MeasureType.Volume,
					Amount = chargeableVolume.Amount,
					Unit = chargeableVolume.Unit
				});
			}
		}

		List<global::WiseRates.Api.Model.MeasureInfo> measureList;

		void AddMeasure(RatesQuery query, global::WiseRates.Api.Model.MeasureInfo measure)
		{
			if (measureList == null)
			{
				measureList = new List<global::WiseRates.Api.Model.MeasureInfo>();
				query.Measures = measureList;
			}
			measureList.Add(measure);
		}

		readonly ILogger logger;
	}
}
