#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.CarrierConnect
{
	class RateCurator(BusinessObjectFactory factory, LoggerDecorator logger)
	{
		readonly RateChooserServices chooserServices = new(factory, ZDateTime.Today, DefaultCurrencyCode);

		public const string UrsRate = "URS";

		public const string CargoWiseRate = "CW1";

		/// <summary>
		/// Whether to permit rate results that do not contain a freight rate (e.g. only origin charges).
		/// </summary>
		const bool RequireFreightRateInResult = true;

		/// <summary>
		/// The currency to be used as the "local" currency in results.
		/// </summary>
		static string DefaultCurrencyCode => GlbCompany.CurrentCompany?.GC_RX_NKLocalCurrency ?? string.Empty;

		public RateSearchResponseDto CurateAndCalculateRates(RatingCriteria ratingCriteria, ICollection<IRateEntry> rateEntries)
		{
			var groupedRates = CurateRateEntries(ratingCriteria, rateEntries);
			return CalculateResults(ratingCriteria, groupedRates);
		}

		/// <summary>
		/// Curating rates is where raw rates (RateEntry) are curated into a set of results made up of multiple rates (RateGroup).
		/// </summary>
		List<RateGroup> CurateRateEntries(RatingCriteria criteria, ICollection<IRateEntry> costRateEntries)
		{
			logger.Information((NoResString)"***** BEGIN RATE CURATION *****");
			logger.Information(string.Format((NoResString)"Curating {0} rate entries", costRateEntries.Count));

			// Create initial rate groups
			var transportMode = criteria.FreightMode.ToTransportMode();
			var allGroups = new HashSet<RateGroup>(costRateEntries.Select(r => new RateGroup(transportMode, r)));

			// Potential optimisation for URS, don't create groups that are both compatible and less specific then a previously created group

			logger.Information((NoResString)"Initial RateGroups before curation:");
			LogRateGroups(allGroups);

			foreach (var rate in costRateEntries)
			{
				var newGroups = new HashSet<RateGroup>();
				foreach (var group in allGroups)
				{
					if (!group.IsRateInGroup(rate) && group.IsCompatibleWith(rate, criteria.JobID.IsEmpty))
					{
						// If adding the rate entry would not change the groups properties
						if (group.IsMoreSpecificThan(rate))
						{
							group.RateEntries.Add(rate);
						}
						else
						{
							// Otherwise, we potentially have a new 'job configuration'
							newGroups.Add(new RateGroup(transportMode, rate, group));
						}
					}
				}

				allGroups.UnionWith(newGroups);
			}

			var distinctGroups = allGroups
				.Where(group => group.ServiceProvider is not null)
				.Where(group => !RequireFreightRateInResult || group.RateEntries.Any(entry => entry.IsFreightEntry()))
				// Remove groups that contain the exact same rate entries, this is specifically for some special URS cases.
				.GroupBy(group => [.. group.RateEntries.Select(rate => rate.PK)], HashSet<ZGuid>.CreateSetComparer())
				.Select(group => group.First())
				.ToList();

			logger.Information((NoResString)"Final RateGroups after curation:");
			LogRateGroups(distinctGroups);

			logger.Information((NoResString)"***** END RATE CURATION *****");

			return distinctGroups;
		}

		RateSearchResponseDto CalculateResults(RatingCriteria ratingCriteria, List<RateGroup> groupedRates)
		{
			var freightAutoRater = new FreightAutoRater(new RatingContext(logger));
			var results = new List<RateResultDto>();
			var jobDatesProvider = GetCriteriaDatesProvider(ratingCriteria);
			var calculators = new Dictionary<Guid, CalculatorDto>();

			UpdateCriteriaCreditors(ratingCriteria, groupedRates);

			foreach (var rateGroup in groupedRates)
			{
				ratingCriteria.ValuesCanBeSet = true;
				ratingCriteria.CarrierServiceLevelOverride = string.IsNullOrWhiteSpace(rateGroup.CarrierServiceLevel)
					? []
					: [rateGroup.CarrierServiceLevel];
				ratingCriteria.CarrierContractNumbers = string.IsNullOrWhiteSpace(rateGroup.ContractNumber)
					? []
					: [rateGroup.ContractNumber];

				if (jobDatesProvider != null)
				{
					jobDatesProvider.TransitTimeOverride = rateGroup.TransitTime;
				}

				var filterOptions = new NotApplicableRateLineRemover.FilterOptions { DisableSpotFilter = true, DisableInvalidRatesFilter = rateGroup.RateSource == UrsRate };
				var calculatedCharges = freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, ratingCriteria, rateGroup.RateEntries, filterOptions);

				var unmappedCharges = rateGroup.RateEntries
					.SelectMany(entry => entry.ChildRateLines)
					.Where(line => line is UrsRateLine ursLine && ursLine.ChargeCode is null)
					.Cast<UrsRateLine>()
					.Select(ConvertLineToDto);

				if (calculatedCharges.Count == 0 && !unmappedCharges.Any())
				{
					continue;
				}

				results.Add(new RateResultDto(
					ratingCriteria,
					rateGroup,
					calculatedCharges,
					calculatedCharges.Select(charge => ConvertChargeToDto(charge, ratingCriteria)).Concat(unmappedCharges).ToList()));
				calculatedCharges
					.Where(charge => charge.Line.TL_RateCalculator == CombinedCalculator.Code && !calculators.ContainsKey(charge.Line.PK.ToGuid()))
					.ForEach(charge => calculators.Add(charge.Line.PK.ToGuid(), new(charge.Line)));
			}

			return new RateSearchResponseDto()
			{
				Calculators = calculators,
				Rates = results.ToArray(),
			};
		}

		RateChargeDto ConvertChargeToDto(AutoRateInfo autoRateInfo, RatingCriteria criteria)
		{
			var localMoney = chooserServices.ConvertToDefaultCurrency(autoRateInfo.Amount, autoRateInfo.Currency);
			var rateEntry = autoRateInfo.Line.ParentRateEntry;

			return new RateChargeDto
			{
				ChargeID = Guid.NewGuid(),
				ContainerType = autoRateInfo.Attributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.ContainerCode),
				ContainerQuality = autoRateInfo.Entry is WiseEntry e && !string.IsNullOrWhiteSpace(e.ContainerQuality)
					? e.ContainerQuality
					: null,
				Commodity = new(rateEntry),
				SourcePK = autoRateInfo.Line.PK.ToGuid(),
				ChargeCode = new(autoRateInfo.Line),
				ChargeUnit = autoRateInfo.ChargeUnit,
				CalculationDescription = autoRateInfo.CalculationDescription,
				HandlingOfficeName = autoRateInfo.Line switch { UrsRateLine ursLine => ursLine.HandlingOfficeName, _ => null },
				LocalAmount = localMoney.IsValid ? localMoney.Amount : 0,
				LocalCurrency = autoRateInfo.LocalCurrency,
				RateAmount = autoRateInfo.Amount,
				RateCurrency = autoRateInfo.Currency,
				Description = autoRateInfo.Description,
				ChargeType = autoRateInfo.Line.ChargeType,
				IsInclusiveCalculator = autoRateInfo.IsInclusiveCalculator,
				IsOptional = autoRateInfo.Line switch { UrsRateLine ursLine => ursLine.IsOptional, _ => false },
				Route = autoRateInfo.Line switch
				{
					UrsRateLine ursLine => ursLine.Route.ToArray(),
					IRateLine line => new[] { criteria.OriginCode, rateEntry.TI_ViaLRC, criteria.DestinationCode }
						.Where(l => !string.IsNullOrEmpty(l))
						.Select(l => l.ToString())
						.ToArray()
				},
				AutoRateInfo = autoRateInfo
			};
		}

		RateChargeDto ConvertLineToDto(UrsRateLine line) => new()
		{
			ChargeCode = new (line),
			ChargeType = line.ChargeType,
			Commodity = new (line.ParentRateEntry),
		};

		static void UpdateCriteriaCreditors(RatingCriteria ratingCriteria, IEnumerable<RateGroup> rates)
		{
			var creditors = rates
				.SelectMany(result => result.RateEntries)
				.SelectMany(result => result.ChildRateLines)
				.Select(line => (line.ChargeCode?.AC_ChargeGroup ?? string.Empty, line.ParentRateEntry.ParentRatingHeader.Header));

			var source = new[] { "CarrierConnect" }.ToList();

			foreach (var (chargeGroup, creditor) in creditors)
			{
				var org = OrgWithSource.New(creditor, source);
				ratingCriteria.Creditors[chargeGroup].Add(1, org);
			}
		}

		static RateSelectorDatesProvider? GetCriteriaDatesProvider(RatingCriteria criteria)
			=> (criteria.JobDatesProvider as RateSelectorDatesProvider);

		#region Logging Helpers

		void LogRateGroups(ICollection<RateGroup> rateGroups)
		{
			logger.Information((NoResString)$"{rateGroups.Count} unique rate group(s)");
			foreach (var grouping in rateGroups)
			{
				logger.Information((NoResString)$"Group: {grouping.GetKey()}");
				logger.Information((NoResString)$"{grouping.RateEntries.Count} rate(s).");
			}
		}

		#endregion
	}
}
