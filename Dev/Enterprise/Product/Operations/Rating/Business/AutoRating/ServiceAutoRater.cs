using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Constants;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// All logic for rating services goes here.
	///
	/// Currently not for warehouse rating, since that uses AmountByLineTable filters to match lines to parts of a job.
	/// See FreightAutoRater.GetMeasureDimensionsForLine.
	/// </summary>
	public class ServiceAutoRater
	{
		internal ServiceAutoRater(RatingCriteria criteria, BusinessObjectFactory factoryForLoadingLocationsEtc, bool? isRatingCost)
		{
			Criteria = criteria;
			Factory = factoryForLoadingLocationsEtc;
			IsRatingCost = isRatingCost;
			if (isRatingCost.HasValue)
			{
				JobServices = new JobServicesCollection();
				JobServices.AddRange(Criteria.JobServices.Where(x => x.IsForRateSearch(isRatingCost.Value)));
			}
			else
			{
				JobServices = Criteria.JobServices;
			}

			var usesAmountByLineTableFilters = AutoRatingCalculatorParametersWithoutFilter.IsUsingFilter(criteria);
			IsEnabled = !usesAmountByLineTableFilters && JobServices.Count > 0;
		}

		internal readonly bool? IsRatingCost;
		internal RatingCriteria Criteria { get; }
		internal BusinessObjectFactory Factory { get; }
		internal JobServicesCollection JobServices { get; }
		internal bool IsEnabled { get; }
		internal IEnumerable<FastLine> AllLines => lineToServices.Keys;
		readonly Dictionary<FastLine, List<JobServiceInfo>> lineToServices = new Dictionary<FastLine, List<JobServiceInfo>>();
		readonly Dictionary<JobServiceInfo, List<FastLine>> serviceToLines = new Dictionary<JobServiceInfo, List<FastLine>>();

		internal bool CanAutoRate(FastLine line)
		{
			if (!IsEnabled)
			{
				return false;
			}

			if (line.ChargeCode.AC_ChargeSubGroup.IsEmpty)
			{
				return false;
			}

			// Only Service Occurrences and service time are actually calculated by service autorater.
			var unit = line.Line.TL_WeightVolume;
			return unit == QuantityUnit.SV || QuantityUnit.IsTime(unit);
		}

		/// <summary>
		/// Returns null containers if the services are not container services
		/// </summary>
		internal (IEnumerable<IRateableContainer> containers, string reference) GetContainersForContainerServices(IRateLine rateLine)
		{
			if (Criteria.JobMeasures.HasContainerMeasure)
			{
				if (!IsEnabled)
				{
					return (Enumerable.Empty<IRateableContainer>(), null);
				}

				var fastLine = Criteria.Cache.GetOrCreateFastLine(rateLine);
				if (lineToServices.TryGetValue(fastLine, out var services))
				{
					if (services.Any())
					{
						if (services.Any(x => x.Container.IsEmpty))
						{
							return (Enumerable.Empty<IRateableContainer>(), null);
						}

						var serviceContainers = services.Where(x => !x.Container.IsEmpty)
							.Select(x => x.Container)
							.ToList();

						var serviceReferences = services
							.Select(s => !s.ServiceReference.IsEmpty
								? s.ServiceReference
								: s.ContainerNumber)
							.Distinct();

						var reference = string.Join(", ", serviceReferences);

						var allContainers = Criteria.JobMeasures.GetAllContainers();
						var containers = allContainers
							.Where(x => x.ContainerPK.HasValue)
							.Where(x => serviceContainers.Contains(x.ContainerPK.Value))
							.ToList();

						if (containers.Count == 0)
						{
							containers = allContainers.ToList();
						}

						return (containers, reference);
					}
				}
				else if (!JobServices.FindServices(fastLine.ChargeCode).Any())
				{
					return (Enumerable.Empty<IRateableContainer>(), null);
				}
			}

			return (Enumerable.Empty<IRateableContainer>(), null);
		}

		internal void RemoveRatesOverridenByServiceSpotRates(RateLinesRepository linesRepository)
		{
			if (!IsEnabled)
			{
				return;
			}

			// If an entry is a spot entry and there are no other non-spot services with that same service code
			// then we can eliminate all other rate lines with that service code since they have nothing to match.
			// Except if the line is an IsIntercompanyTariff since they can rate as both a spot rate and an intercompany rate.
			var rateLines = linesRepository.GetLines();
			var spotRateCodesToCountOfServices = rateLines.Where(x => x.ParentRateEntry.JobServiceForSpotEntry != null)
				.GroupBy(x => (x.ParentRateEntry.JobServiceForSpotEntry.ChargeCodeGroup, x.ParentRateEntry.JobServiceForSpotEntry.ServiceCode));

			foreach (var codeAndLines in spotRateCodesToCountOfServices)
			{
				var serviceCode = codeAndLines.Key.ServiceCode;
				var chargeCodeGroup = codeAndLines.Key.ChargeCodeGroup;
				var isPenaltyServiceCharge = IsPenaltyServiceCharge(chargeCodeGroup, serviceCode);

				var totalServicesCount = JobServices
					.Count(x => x.IsEnabled
						&& x.ChargeCodeGroup == chargeCodeGroup
						&& x.ServiceCode == serviceCode);

				if (totalServicesCount == codeAndLines.Count())
				{
					foreach (var line in rateLines.Where(x => !x.Line.IsIntercompanyTariff()
						&& x.ParentRateEntry.JobServiceForSpotEntry == null
						&&
						(
							(
								!isPenaltyServiceCharge
								&& x.ChargeCode.AC_ChargeGroup == chargeCodeGroup
								&& x.ChargeCode.AC_ChargeSubGroup == serviceCode
							)
							||
							(
								isPenaltyServiceCharge
								&& codeAndLines.Any(y => y.Line.TL_AC == x.Line.TL_AC)
							)
						)))
					{
						linesRepository.Remove(line, Res.GetString("f5ae8a74-2e92-4c7a-aab2-4d5aaf611fc7", "replaced by Job Service ({0} / {1})", line.ChargeCode.AC_ChargeGroup, line.ChargeCode.AC_ChargeSubGroup));
					}
				}
			}
		}

		bool IsPenaltyServiceCharge(string chargeCodeGroup, string serviceCode)
		{
			return
				(
					chargeCodeGroup == ChargeCodeGroupList.Codes.Origin || chargeCodeGroup == ChargeCodeGroupList.Codes.Destination
				) &&
				(
					serviceCode == ChargeCodeSubGroupList.CarrierStorage
					|| serviceCode == ChargeCodeSubGroupList.CartageDemurrageTotal
					|| serviceCode == ChargeCodeSubGroupList.ContainerDetention
					|| serviceCode == ChargeCodeSubGroupList.Labor
					|| serviceCode == ChargeCodeSubGroupList.Storage
					|| serviceCode == ChargeCodeSubGroupList.MergedDemurrageDetention
				);
		}

		/// <summary>
		/// Origin/destination non-service rates may have been loaded at service locations.
		/// Remove them if they don't match criteria location.
		/// </summary>
		/// <param name="linesRepository"></param>
		internal void RemoveOtherOriginAndDestinationChargesWithInvalidLocation(RateLinesRepository linesRepository)
		{
			var chargeCodes = linesRepository.GetChargeCodes();
			var criteriaOrigins = new[] { Criteria.Origin, Criteria.RateOrigin }.WhereNotNull();
			var criteriaDestinations = new[] { Criteria.Destination, Criteria.RateDestination }.WhereNotNull();
			foreach (var chargeCode in chargeCodes)
			{
				if (chargeCode.AC_ChargeSubGroup.IsEmpty)
				{
					foreach (var line in linesRepository.GetLines(chargeCode))
					{
						var entry = line.ParentRateEntry;

						// Interchangeable locations provided by CargoSphere should not be removed.
						if (entry.RateProvider == WRConstants.RateProviders.CargoSphere)
						{
							continue;
						}

						ILocation rateLocation = null;
						IEnumerable<ILocation> criteriaLocations = null;
						bool isDestination = false;
						if (criteriaDestinations.Any() && entry.IsDestinationEntry())
						{
							rateLocation = entry.Destination();
							criteriaLocations = criteriaDestinations;
							isDestination = true;
						}
						else if (criteriaOrigins.Any() && entry.IsOriginEntry())
						{
							rateLocation = entry.Origin();
							criteriaLocations = criteriaOrigins;
						}

						if (rateLocation != null && criteriaLocations != null)
						{
							if (criteriaLocations.Any(x => rateLocation.CompletelyCovers(x)))
							{
								continue;
							}

							var column = isDestination ? RateEntrySchema.TI_DestinationLRC : RateEntrySchema.TI_OriginLRC;
							linesRepository.Remove(line, DiscardReporter.DiscardReason(DiscardReporter.Reason.VsJob, column, criteriaLocations.Select(x => x.Code).ToArray()));
						}
					}
				}
			}
		}

#if DEBUG
		public void RemoveOtherOriginAndDestinationChargesWithInvalidLocation_ForTestOnly(RateLinesRepository linesRepository)
		{
			RemoveOtherOriginAndDestinationChargesWithInvalidLocation(linesRepository);
		}
#endif

		internal void RemoveUnmatchedOrInvalidLines(
			RateLinesRepository linesRepository,
			AccChargeCode chargeCode)
		{
			// Nothing to do with non-service charges.
			if (chargeCode.AC_ChargeSubGroup.IsEmpty)
			{
				return;
			}

			var allServices = JobServices;
			if (allServices.Count == 0)
			{
				// When there is no service at all, we should filter lines with service charge codes unless adapters tell us to keep them.
				if (Criteria.ShouldRemoveChargeWhenMissingServiceOrChargeableUnit(chargeCode))
				{
					linesRepository.Remove(chargeCode, Res.GetString("3841a4ed-8608-4fb4-b93d-a1750364bae9", "{0} Service was not present", chargeCode.AC_ChargeSubGroup));
				}
				return;
			}

			if (!allServices.IsEnabledOrServiceInactive(chargeCode))
			{
				linesRepository.Remove(chargeCode, Res.GetString("881e80fd-dcdd-4566-bb5a-a4c63092b196", "{0} Service was either not present or completion date was not applicable for {1} charge code group.", chargeCode.AC_ChargeSubGroup, chargeCode.AC_ChargeGroup));
				return;
			}

			var services = allServices.FindServices(chargeCode).Where(x => x.IsEnabled).ToList();
			if (services.Count == 0)
			{
				return;
			}

			var prepaidCollect = PaymentTermInfos.GetPrepaidCollect(chargeCode.AC_ChargeGroup);
			if (string.IsNullOrEmpty(prepaidCollect))
			{
				return;
			}

			var isDestination = prepaidCollect == Constants.PaymentType.Collect;
			var criteriaLocation = isDestination ? Criteria.Destination : Criteria.Origin;
			if (RemoveLinesWhenCriteriaLocationIsBlank(linesRepository, chargeCode, criteriaLocation))
			{
				// Lines are already filtered. No need further check.
				return;
			}

			RemoveLinesThatDontMatchServiceLocation(linesRepository, chargeCode, services, isDestination, criteriaLocation);

			// ALL services must have the same country code with the job.
			var incorrectLocationMessage = "";
			var hasIncorrectLocationMessage = services.Any(service => !string.IsNullOrWhiteSpace(incorrectLocationMessage = ServiceCountryIncorrect(service, isDestination, criteriaLocation)));
			if (hasIncorrectLocationMessage)
			{
				linesRepository.Remove(chargeCode, incorrectLocationMessage);
			}
		}

		/// <summary>
		/// Verify that job's location presents. If not, nothing for services to match locations with, remove the lines.
		/// Even if service.Location is blank we don't want to match it.
		/// </summary>
		/// <returns>Whether the removal occurs</returns>
		static bool RemoveLinesWhenCriteriaLocationIsBlank(RateLinesRepository linesRepository, AccChargeCode chargeCode, ILocation criteriaLocation)
		{
			if (criteriaLocation != null)
			{
				return false;
			}

			var reason = Res.GetString("12f6e097-5139-487e-88b3-561b00636157", "job origin and/or destination are blank.");
			foreach (var line in linesRepository.GetLines(chargeCode))
			{
				linesRepository.Remove(line, reason);
			}

			return true;
		}

		/// <summary>
		/// Match service location (code length > 2) vs job's location.
		/// RemoveLinesWhenCriteriaLocationIsBlank should be called and checked prior to this.
		/// </summary>
		void RemoveLinesThatDontMatchServiceLocation(
			RateLinesRepository linesRepository,
			AccChargeCode chargeCode,
			IEnumerable<JobServiceInfo> services,
			bool isDestination,
			ILocation criteriaLocation)
		{
			// Only for locations that are not country codes.
			// Country matching is done in ServiceCountryIncorrect
			var servicesWithLocations = services.Where(x => x.LocationCode.Length > 2);
			if (!servicesWithLocations.Any())
			{
				return;
			}

			var serviceLocations = servicesWithLocations
				.Select(x => x.LocationCode)
				.Distinct()
				.Select(x => LocationHelper.GetCachedLocationFromString(x, Factory))
				.WhereNotNull()
				.ToList();
			if (serviceLocations.Count == 0)
			{
				return;
			}

			foreach (var line in linesRepository.GetLines(chargeCode))
			{
				var entry = line.ParentRateEntry;
				var rateLocation = isDestination ? entry.Destination() : entry.Origin();
				if (rateLocation != null && !rateLocation.CompletelyCovers(criteriaLocation))
				{
					var rateCoversAnyServiceLocation = serviceLocations.Any(x => rateLocation.CompletelyCovers(x));
					if (!rateCoversAnyServiceLocation)
					{
						var locationCodes = string.Join(", ", serviceLocations.Select(x => x.Code));
						var reason = isDestination
							? Res.GetString("DD7F0AD3-6BE3-43E0-A988-2D121B04128E", "service location {0} and job destination {1} did not match rate destination {2}", locationCodes, criteriaLocation.Code, rateLocation.Code)
							: Res.GetString("3D7FF6EA-B005-47B7-A0C5-CFDDA13CBF34", "service location {0} and job origin {1} did not match rate origin {2}", locationCodes, criteriaLocation.Code, rateLocation.Code);
						linesRepository.Remove(line, reason);
					}
				}
			}
		}

		/// <summary>
		/// Match service country vs job's country.
		/// Return a non-empty string for a filtering reason. Otherwise, an empty string means the charge code is good for the service.
		/// RemoveLinesWhenCriteriaLocationIsBlank should be called and checked prior to this.
		/// </summary>
		static string ServiceCountryIncorrect(JobServiceInfo service, bool isDestination, ILocation criteriaLocation)
		{
			// Exception: A service at a port must have been matched earlier then it is good.
			// Only check further other types of location vs the job's country.
			if (LocationHelper.GetLocationType(service.LocationCode) == LocationHelper.LocationType.Port)
			{
				return string.Empty;
			}

			var locationCountryCode = criteriaLocation.Country.Code;
			if (service.LocationCountryCode.IsEmpty || service.LocationCountryCode == locationCountryCode)
			{
				return string.Empty;
			}

			return isDestination
				? Res.GetString("E7FF403E-8AA2-4502-BE41-B385221DDB00", "Service country/region {0} did not match Job Destination {1}.", service.LocationCountryCode, locationCountryCode)
				: Res.GetString("CDDD4B1B-E706-4AA6-94A5-A2761D57F946", "Service country/region {0} did not match Job Origin {1}.", service.LocationCountryCode, locationCountryCode);
		}

		/// <summary>
		/// Return true if the two lines should use ServiceAutoRater.RemoveOverriden instead of OriginDestinationComparer
		/// </summary>
		internal static bool UseServiceAutoRaterToCompareOriginDestination(RatingCriteria criteria, FastLine line1, FastLine line2)
		{
			if (criteria == null)
			{
				return false;
			}

			var charge1 = line1.ChargeCode;
			var charge2 = line2.ChargeCode;
			if (charge1 == null || charge2 == null || charge1.AC_ChargeSubGroup.IsEmpty || charge1.AC_ChargeSubGroup != charge2.AC_ChargeSubGroup)
			{
				return false;
			}

			var contractors1 = new HashSet<ZGuid>();
			var secondary1 = new HashSet<ZGuid>();
			var contractors2 = new HashSet<ZGuid>();
			var secondary2 = new HashSet<ZGuid>();
			AnalyseContractors(criteria, charge1, charge2, contractors1, secondary1, contractors2, secondary2);

			bool hasOneContractor = contractors1.Count == 1
				&& contractors2.Count == 1
				&& contractors1.First() == contractors2.First()
				&& secondary1.Count == secondary2.Count
				&& (secondary1.Count == 0 || secondary1.First() == secondary2.First());
			return !hasOneContractor;
		}

		/// <summary>
		/// Prefer a rate line with a provider that matches all serviceInfo Contractors
		/// </summary>
		internal static int? CompareServiceContractors(RatingCriteria criteria, FastLine line1, FastLine line2)
		{
			var charge1 = line1.ChargeCode;
			var charge2 = line2.ChargeCode;
			if (charge1 == null || charge2 == null || charge1.AC_ChargeSubGroup.IsEmpty || charge1.AC_ChargeSubGroup != charge2.AC_ChargeSubGroup)
			{
				return null;
			}

			var provider1 = line1.ParentRateEntry.ParentRatingHeader.Header;
			var provider2 = line2.ParentRateEntry.ParentRatingHeader.Header;
			if (provider1 == null && provider2 == null)
			{
				return null;
			}

			var contractors1 = new HashSet<ZGuid>();
			var secondary1 = new HashSet<ZGuid>();
			var contractors2 = new HashSet<ZGuid>();
			var secondary2 = new HashSet<ZGuid>();
			AnalyseContractors(criteria, charge1, charge2, contractors1, secondary1, contractors2, secondary2);

			// can only compare if all services have the same contractor
			if (contractors1.Count == 1 && contractors2.Count == 1)
			{
				var isContractor1 = provider1 != null && provider1.PK == contractors1.First();
				var isContractor2 = provider2 != null && provider2.PK == contractors2.First();
				if (isContractor1 || isContractor2)
				{
					return isContractor1.CompareTo(isContractor2);
				}

				// neither line provider is the contractor
				// can also compare if all services have the same secondary
				if (secondary1.Count >= 1 && secondary1.Count == secondary2.Count)
				{
					var isSecondary1 = provider1 != null && secondary1.Contains(provider1.PK);
					var isSecondary2 = provider2 != null && secondary2.Contains(provider2.PK);
					if (isSecondary1 || isSecondary2)
					{
						return isSecondary1.CompareTo(isSecondary2);
					}
				}
			}
			else if (contractors1.Count > 1 || contractors2.Count > 1)
			{
				// multiple contractors - resolve after MatchingServices
				return 0;
			}

			return null;
		}

		static void AnalyseContractors(RatingCriteria criteria, AccChargeCode charge1, AccChargeCode charge2, HashSet<ZGuid> contractors1, HashSet<ZGuid> secondary1, HashSet<ZGuid> contractors2, HashSet<ZGuid> secondary2)
		{
			foreach (var service in criteria.JobServices.Where(x => x.IsEnabled))
			{
				if (service.Contractor != null)
				{
					if (service.IsServiceFor(charge1.AC_ChargeGroup, charge1.AC_ChargeSubGroup))
					{
						contractors1.Add(service.Contractor.PK);
						if (service.FallbackContractors != null)
						{
							secondary1.UnionWith(service.FallbackContractors.Select(x => x.PK));
						}
					}
					if (service.IsServiceFor(charge2.AC_ChargeGroup, charge2.AC_ChargeSubGroup))
					{
						contractors2.Add(service.Contractor.PK);
						if (service.FallbackContractors != null)
						{
							secondary2.UnionWith(service.FallbackContractors.Select(x => x.PK));
						}
					}
				}
			}
		}

		internal void RemoveOverridden(
			RateLinesRepository linesRepository,
			AccChargeCode chargeCode,
			bool isCosting)
		{
			if (IsEnabled && !chargeCode.AC_ChargeSubGroup.IsEmpty)
			{
				MatchServicesToLines(linesRepository, chargeCode, isCosting);

				RemoveByCostsProviderComparer(linesRepository, chargeCode, isCosting);

				RemoveServiceLinesAtDefaultLocationWhenOverriddenByLineAtServiceLocation(linesRepository, chargeCode);

				RemoveServiceLinesAtLessSpecificLocation(linesRepository, chargeCode);
			}
		}

		void RemoveByCostsProviderComparer(RateLinesRepository linesRepository, AccChargeCode chargeCode, bool isCosting)
		{
			if (!isCosting)
			{
				return;
			}

			var applicableServices = JobServices.FindServices(chargeCode)
				.Where(x => x.IsEnabled
					&& x.Contractor != null);

			if (!applicableServices.Any())
			{
				return;
			}

			foreach (var service in applicableServices)
			{
				if (serviceToLines.TryGetValue(service, out var lines))
				{
					if (lines.Count > 1)
					{
						RemoveServiceLinesByCostsProviderComparer(linesRepository, service, lines);
					}
				}
			}
		}

		void RemoveServiceLinesByCostsProviderComparer(RateLinesRepository linesRepository, JobServiceInfo service, List<FastLine> lines)
		{
			var contractor = service.Contractor;
			var fallbacks = service.FallbackContractors;
			bool hasFallbacks = fallbacks != null && fallbacks.Length > 0;
			int bestFallbackIndex = fallbacks?.Length ?? 0;
			ZGuid bestProviderPk = ZGuid.Empty;
			FastLine firstMatchingLine = null;
			foreach (var line in lines)
			{
				var entry = line.ParentRateEntry;
				if (entry.ParentRatingHeader != null)
				{
					var providerPk = entry.ParentRatingHeader.TH_OH;
					if (providerPk == contractor.PK)
					{
						bestProviderPk = providerPk;
						firstMatchingLine = line;
						break;
					}
					else if (hasFallbacks && bestFallbackIndex > 0)
					{
						for (int i = 0; i < bestFallbackIndex; ++i)
						{
							if (providerPk == fallbacks[i].PK)
							{
								bestFallbackIndex = i;
								bestProviderPk = providerPk;
								firstMatchingLine = line;
							}
						}
					}
				}
			}

			if (!bestProviderPk.IsEmpty)
			{
				var linesToRemove = new List<FastLine>();

				foreach (var line in lines)
				{
					var entry = line.ParentRateEntry;
					if (entry.ParentRatingHeader != null)
					{
						var providerPk = entry.ParentRatingHeader.TH_OH;
						if (providerPk != bestProviderPk)
						{
							linesToRemove.Add(line);
						}
					}
				}

				foreach (var line in linesToRemove)
				{
					if (RemoveMatch(service, line, lines))
					{
						var reason = GetReason(firstMatchingLine, (NoResString)"service contractor"); // log message, subject to change, more for support people as of now
						linesRepository.Remove(line, reason);
					}
				}
			}
		}

		/// <summary>
		/// Removes rates that match the default service location if there is a rate that matches the overriden location.
		/// Services can have two locations, one explictly given on the service, and a default from the job for that type of service.
		/// For example, container storage penalty location defaults to the Discharge Port, but it can be be given another location.
		/// The preferred rate is the one that matches the overridden location.
		/// Only applies to services that have a Location that is a port code.
		/// </summary>
		void RemoveServiceLinesAtDefaultLocationWhenOverriddenByLineAtServiceLocation(
			RateLinesRepository linesRepository,
			AccChargeCode chargeCode)
		{
			const int PortCodeLength = 5;
			var applicableServices = JobServices.FindServices(chargeCode)
				.Where(x => x.IsEnabled
					&& x.LocationCode.Length == PortCodeLength);
			if (!applicableServices.Any())
			{
				return;
			}

			var prepaidCollect = PaymentTermInfos.GetPrepaidCollect(chargeCode.AC_ChargeGroup);
			if (string.IsNullOrEmpty(prepaidCollect))
			{
				return;
			}

			foreach (var service in applicableServices)
			{
				var locationCode = service.LocationCode;
				if (serviceToLines.TryGetValue(service, out var lines))
				{
					if (lines.Count > 1)
					{
						var serviceLocation = LocationHelper.GetCachedLocationFromString(locationCode, Factory);
						FastLine firstLineCoveringServiceLocation = null;
						var linesNotCoveringServiceLocation = new List<FastLine>();
						bool isDestination = prepaidCollect == Constants.PaymentType.Collect;
						foreach (var line in lines)
						{
							var entry = line.ParentRateEntry;
							ILocation rateLocation;
							if (isDestination)
							{
								rateLocation = entry.Destination();
							}
							else
							{
								rateLocation = entry.Origin();
							}

							if (rateLocation == null || rateLocation.CompletelyCovers(serviceLocation))
							{
								if (firstLineCoveringServiceLocation == null)
								{
									firstLineCoveringServiceLocation = line;
								}
							}
							else
							{
								linesNotCoveringServiceLocation.Add(line);
							}
						}

						if (firstLineCoveringServiceLocation != null && linesNotCoveringServiceLocation.Count > 0)
						{
							foreach (var line in linesNotCoveringServiceLocation)
							{
								if (RemoveMatch(service, line, lines))
								{
									var reason = GetReason(firstLineCoveringServiceLocation, (NoResString)"service location"); // log message, subject to change, more for support people as of now
									linesRepository.Remove(line, reason);
								}
							}
						}
					}
				}
			}
		}

		void RemoveServiceLinesAtLessSpecificLocation(
			RateLinesRepository linesRepository,
			AccChargeCode chargeCode)
		{
			// If two lines match the same service and one is a more specific location
			// then remove the match for the less specific rate .
			// Note, this comes after comparing cost providers
			// so we already know lines from lower priority cost providers are gone.
			// Note 2, also comes after removing lines that don't match the service location if there is one that does.
			var applicableServices = JobServices.FindServices(chargeCode)
				.Where(x => x.IsEnabled);
			if (!applicableServices.Any())
			{
				return;
			}

			var prepaidCollect = PaymentTermInfos.GetPrepaidCollect(chargeCode.AC_ChargeGroup);
			if (string.IsNullOrEmpty(prepaidCollect))
			{
				return;
			}

			foreach (var service in applicableServices)
			{
				if (serviceToLines.TryGetValue(service, out var lines))
				{
					var linesToCompare = lines.Where(l => chargeCode.PK.Equals(l.ChargeCode?.PK)).ToList();
					if (linesToCompare.Count > 1)
					{
						CompareAndRemove(service, linesToCompare, linesRepository, OriginDestinationCompare);
					}
				}
			}
		}

		(int compareResult, string comparerName) OriginDestinationCompare(FastLine line1, FastLine line2)
		{
			var result = OriginDestinationComparer.IsOriginDestinationOverridden(line2.Line, line1.Line, Criteria)
				- OriginDestinationComparer.IsOriginDestinationOverridden(line1.Line, line2.Line, Criteria);
			if (result == 0)
			{
				return (0, null);
			}
			else
			{
				return (result, (NoResString)"Origin Destination"); // log message, subject to change, more for support people as of now)
			}
		}

		delegate (int compareResult, string comparerName) ServiceLineComparer(FastLine line1, FastLine line2);

		void CompareAndRemove(JobServiceInfo service, List<FastLine> rateLines, RateLinesRepository rateLinesRepository, ServiceLineComparer comparer)
		{
			for (var i = rateLines.Count - 2; i >= 0; i--)
			{
				var line1 = rateLines[i];
				for (var j = rateLines.Count - 1; j > i; j--)
				{
					var line2 = rateLines[j];
					(var compareResult, var comparerName) = comparer(line1, line2);
					if (compareResult != 0)
					{
						var lineToRemove = compareResult > 0 ? line2 : line1;
						var lineToKeep = compareResult > 0 ? line1 : line2;

						if (RemoveMatch(service, lineToRemove, rateLines))
						{
							var reason = GetReason(lineToKeep, comparerName);
							rateLinesRepository.Remove(lineToRemove, reason);
						}

						if (compareResult < 0)
						{
							break;
						}
					}
				}
			}
		}

		string GetReason(FastLine overriddenBy, string comparerName)
		{
			var reason = (NoResString)" by " + comparerName + (NoResString)" comparer"; // log message, subject to change, more for support people as of now

			return ZString.Format((NoResString)"overridden by {0}{1}", overriddenBy.DisplayInfo(), reason); // log message, subject to change, more for support people as of now
		}

		bool isServiceMatchingDone;

		/// <summary>
		/// Match services to given lines.
		/// Called once for the main rating calculation.
		/// Can then be called repeatedly for lines with a CompanyTariffOrCostBasedCalculator, passing the base lines (see LoadCostOrCompanyTariffRateLines).
		/// </summary>
		void MatchServicesToLines(RateLinesRepository rateLinesRepository, AccChargeCode chargeCode, bool isCosting)
		{
			isServiceMatchingDone = true;
			var lines = rateLinesRepository.GetLines(chargeCode);
			// Ignore lines already matched
			lines.RemoveAll(x => lineToServices.ContainsKey(x));

			if (lines.Count == 0)
			{
				return;
			}

			var services = JobServices.FindServices(chargeCode)
				.Where(x => x.IsEnabled)
				.ToList();

			if (services.Count == 0)
			{
				return;
			}

			var prepaidCollect = PaymentTermInfos.GetPrepaidCollect(chargeCode.AC_ChargeGroup);
			bool? isCollect = prepaidCollect.In(Constants.PaymentType.Collect, Constants.PaymentType.Prepaid)
				? prepaidCollect == Constants.PaymentType.Collect
				: null;
			bool isDestination = prepaidCollect == Constants.PaymentType.Collect;
			bool isOrigin = prepaidCollect == Constants.PaymentType.Prepaid;

			var criteriaLocation = isDestination ? Criteria.Destination : isOrigin ? Criteria.Origin : null;
			var jobCreditors = Criteria.Creditors[chargeCode.AC_ChargeGroup];

			// Do spot rate lines first since they remove the service from further matching
			var nonspotServices = MatchSpotRates(rateLinesRepository, chargeCode, lines, services);

			foreach (var line in lines.Where(x => !IsSpotRateForCurrentCostOrSell(x.ParentRateEntry.JobServiceForSpotEntry)))
			{
				// Intercompany tariffs can also match services with spot rates
				var servicesToMatch = line.IsIntercompanyTariff()
					? services
					: nonspotServices;

				MatchLine(rateLinesRepository, chargeCode, servicesToMatch, isCollect, criteriaLocation, jobCreditors, line, isCosting);
			}
		}

		/// <summary>
		/// Returns list of services that were not matched
		/// </summary>
		List<JobServiceInfo> MatchSpotRates(RateLinesRepository rateLinesRepository, AccChargeCode chargeCode, List<FastLine> lines, List<JobServiceInfo> services)
		{
			var unmatchedServices = new List<JobServiceInfo>(services);

			foreach (var line in lines)
			{
				var spotService = line.ParentRateEntry.JobServiceForSpotEntry;
				if (IsSpotRateForCurrentCostOrSell(spotService))
				{
					var matchedServices = new List<JobServiceInfo>();

					int serviceIndex = -1;
					for (int i = 0; i < unmatchedServices.Count; ++i)
					{
						if (IsServiceEqual(spotService, unmatchedServices[i]))
						{
							serviceIndex = i;
							break;
						}
					}
					if (serviceIndex >= 0)
					{
						var service = unmatchedServices[serviceIndex];
						matchedServices.Add(service);
						unmatchedServices.RemoveAt(serviceIndex);
					}

					SetMatchedServicesForLine(rateLinesRepository, chargeCode, line, matchedServices);
				}
			}
			return unmatchedServices;
		}

		bool IsSpotRateForCurrentCostOrSell(JobServiceInfo spotService)
		{
			return spotService != null &&
				(!IsRatingCost.HasValue || IsRatingCost.Value == spotService.IsCostForSpotRate);
		}

		void MatchLine(RateLinesRepository rateLinesRepository,
			AccChargeCode chargeCode,
			List<JobServiceInfo> services,
			bool? isCollect,
			ILocation criteriaLocation,
			OrgPrioritizedList jobCreditors,
			FastLine line,
			bool isCosting)
		{
			bool isDestination = isCollect.HasValue && isCollect.Value;
			bool isOrigin = isCollect.HasValue && !isCollect.Value;
			var matchedServices = new List<JobServiceInfo>();
			var entry = line.ParentRateEntry;
			var rateLocation = isDestination ? entry.Destination() : isOrigin ? entry.Origin() : null;

			foreach (var service in services)
			{
				if (HasMatchingOrEmptyContainerType(service, line)
					&& HasMatchingOrEmptyLocation(service, rateLocation, criteriaLocation)
					&& (!isCosting || HasMatchingOrEmptyContractorOrCreditor(service, line, jobCreditors))
					&& (HasMatchingUnitFactor(service, line)))
				{
					matchedServices.Add(service);
				}
			}

			SetMatchedServicesForLine(rateLinesRepository, chargeCode, line, matchedServices);
		}

		/*
		When Unit Factor is empty, only service on lead shipment should be matched.
		When Unit Factor is BCN, then service on current shipment should be matched.
		While autorating for a Buyers consol, the job service info will contain both lead shipment services and current shipment services.Then we match related shipment services based on Unit Factor.
		*/
		bool HasMatchingUnitFactor(JobServiceInfo service, FastLine line)
		{
			var isForMaster = line.Line.TL_UnitFactor != UnitFactorList.Codes.BCN;
			var result = service.IsMasterShipment == isForMaster || !IsBCNRelatedInvoicingStyle;
			return result;
		}

		bool IsBCNRelatedInvoicingStyle
		{
			get
			{
				if (isBCNRelatedInvoicingStyle == null)
				{
					isBCNRelatedInvoicingStyle = Criteria.Job?.LocalCharges?.CompanyData?.EffectiveBuyersConsolInvoicingStyle.ToString().In(Constants.ConsolInvoicingStyles.Apportion, Constants.ConsolInvoicingStyles.ApportionInvoiceMaster)
						?? false;
				}
				return isBCNRelatedInvoicingStyle.Value;
			}
		}

		bool? isBCNRelatedInvoicingStyle;

		/// <summary>
		/// Return true if the line is no longer matched to any service
		/// </summary>
		bool RemoveMatch(JobServiceInfo service, FastLine line, List<FastLine> currentServiceLines)
		{
			currentServiceLines.Remove(line);
			if (lineToServices.TryGetValue(line, out var services))
			{
				services.Remove(service);
				if (services.Count == 0)
				{
					lineToServices.Remove(line);
					return true;
				}
			}

			return false;
		}

		void SetMatchedServicesForLine(RateLinesRepository rateLinesRepository, AccChargeCode chargeCode, FastLine line, List<JobServiceInfo> matchedServices)
		{
			if (matchedServices.Any())
			{
				if (lineToServices.ContainsKey(line))
				{
					var matchedServicesAsString = string.Join(",", matchedServices.Select(x => x.ServiceCode));
					var rateLineItems = line.Line?.ChildRateLineItems != null
						? string.Join(",", line.Line.ChildRateLineItems.Select(x => $"{x.TM_Type}-{x.TM_RelevantValue}"))
						: string.Empty;
					ErrorReporter.ReportOnce
					(
						"ServiceAutoRater|SetMatchedServicesForLine|ArgumentException|KeyExists|Issue01277096",
						$"Processing chargeCode ({chargeCode.AC_Code}) with matchedServices ({matchedServicesAsString}) and rateLine ({line.DisplayInfo()} > {rateLineItems})"
					);
				}

				lineToServices.Add(line, matchedServices);
				foreach (var service in matchedServices)
				{
					if (!serviceToLines.TryGetValue(service, out var serviceLines))
					{
						serviceLines = new List<FastLine>();
						serviceToLines.Add(service, serviceLines);
					}
					serviceLines.Add(line);
				}
			}
			else
			{
				var reason = Res.GetString("65AFF36A-59AB-4017-AB11-091ECD4454AB", "{0} {1} Service did not match", chargeCode.AC_ChargeSubGroup, chargeCode.AC_ChargeGroup);
				rateLinesRepository.Remove(line, reason);
			}
		}

		internal IEnumerable<JobServiceInfo> GetAllServicesForLine(FastLine line)
		{
			if (!IsEnabled || !isServiceMatchingDone)
			{
				return GetDefaultServicesForLine(line);
			}

			if (lineToServices.TryGetValue(line, out var services))
			{
				return services;
			}
			else
			{
				return Enumerable.Empty<JobServiceInfo>();
			}
		}

		IEnumerable<JobServiceInfo> GetDefaultServicesForLine(FastLine line)
		{
			var services = JobServices.FindServices(line.ChargeCode).ToList();
			if (!services.Any())
			{
				return services;
			}

			var rateEntry = line.ParentRateEntry;
			if (rateEntry != null)
			{
				if (!rateEntry.ContainerPKForSpotEntry.IsEmpty)
				{
					var containerServiceInfos = services.Where(s => s.Container == rateEntry.ContainerPKForSpotEntry).ToList();
					if (containerServiceInfos.Any())
					{
						services = containerServiceInfos;
					}
				}
				else if (!rateEntry.TI_RC.IsEmpty)
				{
					var serviceInfosSameContainerType = services.Where(s => rateEntry.IsSameContainerOrSameClass(s.ContainerType, true)).ToList();
					if (serviceInfosSameContainerType.Any())
					{
						services = serviceInfosSameContainerType;
					}
				}
			}

			return services;
		}

		internal IEnumerable<JobServiceInfo> GetServicesBeingCalculated(IRateLine line)
			=> GetServicesBeingCalculated(Criteria.Cache.GetOrCreateFastLine(line));

		internal IEnumerable<FastLine> GetLinesForService(JobServiceInfo service)
		{
			if (serviceToLines.TryGetValue(service, out var lines))
			{
				return lines;
			}
			else
			{
				return Enumerable.Empty<FastLine>();
			}
		}

		internal void SetBestLinesForService(JobServiceInfo service, IEnumerable<FastLine> bestMatches)
		{
			if (serviceToLines.TryGetValue(service, out var oldList))
			{
				foreach (var line in oldList)
				{
					if (!bestMatches.Contains(line))
					{
						if (lineToServices.TryGetValue(line, out var services))
						{
							services.Remove(service);
							if (services.Count == 0)
							{
								lineToServices.Remove(line);
							}
						}
					}
				}

				foreach (var line in bestMatches)
				{
					if (!oldList.Contains(line))
					{
						// line wasn't previously linked to the service
						if (!lineToServices.TryGetValue(line, out var services))
						{
							services = new List<JobServiceInfo>();
							lineToServices.Add(line, services);
						}
						services.Add(service);
					}
				}
			}
			serviceToLines[service] = bestMatches.ToList();
		}

		/// <summary>
		/// Criteria.JobServices may return a new collection of services each time, so we can't compare by reference
		/// </summary>
		static bool IsServiceEqual(JobServiceInfo a, JobServiceInfo b)
		{
			return a.Contractor == b.Contractor
				&& a.IsContractorCreditor == b.IsContractorCreditor
				&& a.IsCostForSpotRate == b.IsCostForSpotRate
				&& a.IsCostOrSellForRateSearch == b.IsCostOrSellForRateSearch
				&& a.IsEnabled == b.IsEnabled
				&& a.IsHiddenService == b.IsHiddenService
				&& a.LocationCode == b.LocationCode
				&& a.Rate == b.Rate
				&& a.ServiceId == b.ServiceId
				&& a.ServiceCode == b.ServiceCode
				&& a.ServiceCount == b.ServiceCount
				&& a.ServiceDescription == b.ServiceDescription
				&& a.ServiceDuration == b.ServiceDuration
				&& a.ServiceReference == b.ServiceReference
				&& a.TotalCost == b.TotalCost
				&& a.Unit == b.Unit
				&& ((a.FallbackContractors == null && b.FallbackContractors == null) ||
					(a.FallbackContractors != null && b.FallbackContractors != null && a.FallbackContractors.SequenceEqual(b.FallbackContractors)));
		}

		static bool MustMatchContractors(JobServiceInfo service) => service.IsHiddenService;

		bool HasMatchingOrEmptyContractorOrCreditor(JobServiceInfo service, FastLine line, OrgPrioritizedList jobCreditors)
		{
			if (service.Contractor == null)
			{
				return !MustMatchContractors(service);
			}

			var entry = line.ParentRateEntry;
			var header = entry.ParentRatingHeader;
			if (IsMatchingProviderPk(service, header.TH_OH) ||
				IsMatchingProviderPk(service, entry.TI_OH_TransportProvider))
			{
				return true;
			}

			if (!MustMatchContractors(service) && jobCreditors.Any(x => x.Org != null &&
				(
					IsMatchingProviderPk(service, x.Org.PK)
					|| x.Org.PK == header.TH_OH
					|| header.TH_OH.IsEmpty
				)))
			{
				return true;
			}

			return false;
		}

		static bool IsMatchingProviderPk(JobServiceInfo service, ZGuid providerPk)
		{
			return !providerPk.IsEmpty &&
				(
					(service.Contractor != null && service.Contractor.PK == providerPk)
					||
					(service.FallbackContractors != null && service.FallbackContractors.Any(x => x.PK == providerPk))
				);
		}

		bool HasMatchingOrEmptyLocation(JobServiceInfo service, ILocation rateEntryLocation, ILocation criteriaLocation)
		{
			if (rateEntryLocation == null || service.LocationCode.Length <= 2)
			{
				return true;
			}

			var serviceLocation = LocationHelper.GetCachedLocationFromString(service.LocationCode, Factory);
			return serviceLocation == null
				|| rateEntryLocation.CompletelyCovers(serviceLocation)
				|| (criteriaLocation != null && rateEntryLocation.CompletelyCovers(criteriaLocation));
		}

		static bool HasMatchingOrEmptyContainerType(JobServiceInfo service, FastLine line)
		{
			var rateEntry = line.ParentRateEntry;
			if (rateEntry == null)
			{
				return true;
			}
			else if (!rateEntry.ContainerPKForSpotEntry.IsEmpty)
			{
				return service.Container == rateEntry.ContainerPKForSpotEntry;
			}
			else if (service.ContainerType.IsEmpty)
			{
				return true;
			}
			else
			{
				return rateEntry.TI_RC.IsEmpty || rateEntry.IsSameContainerOrSameClass(service.ContainerType, true);
			}
		}

		/// <summary>
		/// For some services, when rating costs, the ProviderPK is always the contractor
		/// even if the rate was found on another organisation.
		/// If there are multiple contractors, prefer the one that matches the rate provider.
		/// </summary>
		internal ZGuid GetProviderPkForRatingCost(IRateLine line)
		{
			var chargeCode = line.ChargeCode;
			if (chargeCode != null && !chargeCode.AC_ChargeSubGroup.IsEmpty)
			{
				var entry = line.ParentRateEntry;
				var fastLine = Criteria.Cache.GetOrCreateFastLine(line);
				var services = GetServicesBeingCalculated(fastLine);

				var serviceWithCreditor = services
					.Where(x => x.IsEnabled
						&& x.IsContractorCreditor
						&& x.Contractor != null)
					.OrderBy(x => x.Contractor.PK == entry.ParentRatingHeader.TH_OH ? 0 : 1)
					.FirstOrDefault();

				if (serviceWithCreditor != null)
				{
					return serviceWithCreditor.Contractor.PK;
				}
			}

			return ZGuid.Empty;
		}

		/// <summary>
		/// Set the services currently being calculated.
		/// Can be a subset of all services that match the line
		/// when the services have different contractors and need to be calculated separately.
		/// </summary>
		internal void SetServicesToCalculate(FastLine line, IEnumerable<JobServiceInfo> services)
		{
			ServicesBeingCalculated = services;
			LineBeingCalculated = line;
		}
		internal IEnumerable<JobServiceInfo> ServicesBeingCalculated { get; private set; }
		internal FastLine LineBeingCalculated { get; private set; }

		internal IEnumerable<JobServiceInfo> GetServicesBeingCalculated(FastLine line)
		{
			return LineBeingCalculated == line
				? ServicesBeingCalculated
				: GetAllServicesForLine(line);
		}
	}
}
