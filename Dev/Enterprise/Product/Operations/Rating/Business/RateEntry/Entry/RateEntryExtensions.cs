using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Warehouse.Integration;
using WiseRates.Api.Model;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public static class RateEntryExtensions
	{
		public static bool IsDuplicateForPricingPageGrouping(this RateEntry entry1, RateEntry entry2, bool ignoreCategoryAndMode, bool checkMatchContainerRateClass = false)
		{
			var columnToExclude = ignoreCategoryAndMode
				? new[] { nameof(RateEntry.TI_RC), nameof(RateEntry.ContainerClass), nameof(RateEntry.TI_RateCategory), nameof(RateEntry.TI_Mode) }
				: new[] { nameof(RateEntry.TI_RC), nameof(RateEntry.ContainerClass) };

			var result = entry2.IsDuplicateExcludingColumns(entry1, columnToExclude);
			result = result && entry1.TI_RateStartDate == entry2.TI_RateStartDate;
			result = result && entry1.TI_RateEndDate == entry2.TI_RateEndDate;

			if (checkMatchContainerRateClass)
			{
				result = result && entry1.TI_MatchContainerRateClass == entry2.TI_MatchContainerRateClass;
			}

			return result;
		}

		public static bool IsSameContainerOrSameClass(this IRateEntry entry, ZGuid containerPK, bool checkContainerClass)
		{
			var result = entry.TI_RC == containerPK;
			if (!result && entry.Container != null)
			{
				result = checkContainerClass && entry.IsFreightEntry() && entry.Container.ContainersInSameFreightRateClass.FindByPK(containerPK) != null;
				result = result || checkContainerClass && !entry.IsFreightEntry() && entry.Container.ContainersInSameHandlingRateClass.FindByPK(containerPK) != null;
			}

			return result;
		}

		public static bool IsFreightEntry(this IRateEntry entry)
		{
			return RatingConstants.RateCategory.IsFreight(entry.TI_RateCategory);
		}

		public static bool IsSupplementaryEntry(this IRateEntry entry)
		{
			return !entry.IsFreightEntry();
		}

		public static IEnumerable<int> GetCompanyTariffLevels(this IRateEntry entry, OrgRateTariffLevel.Directions direction = OrgRateTariffLevel.Directions.ALL | OrgRateTariffLevel.Directions.EXP | OrgRateTariffLevel.Directions.IMP)
		{
			if (entry.ParentRatingHeader != null)
			{
				var result = entry.ParentRatingHeader.IsTariff()
					? new[] { (int)entry.ParentRatingHeader.TH_GlobalRateLevel }
					: CompanyTariff.GetLevelsForRequestedDirections(entry.ParentRatingHeader.Header, entry.ParentRatingHeader.Company, entry.TI_RateCategory, entry.TI_Mode, direction, entry.TI_RateStartDate, entry.TI_RateEndDate);

				if (result.Any())
				{
					return result;
				}
			}

			return [Env.Registry.GlobalTariffDefault];
		}

		public static bool IsExpired(this IRateEntry entry)
		{
			return entry.TI_RateEndDate < ZDateTime.Today;
		}

		public static bool IsGlobal(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsGlobal();
		}

		public static bool IsCompanyTariff(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsTariff();
		}

		public static bool IsCostRate(this IRateEntry entry)
		{
			var ratingHeader = entry.ParentRatingHeader;
			if (ratingHeader != null)
			{
				return ratingHeader.IsCosting() || ratingHeader.IsWiseCostRate() || (_Rating.Cost && ratingHeader.IsIntercompanyTariff());
			}

			return false;
		}

		public static bool IsIntercompanyTariff(this IRateEntry entry) =>
			entry.ParentRatingHeader.IsIntercompanyTariff();

		public static bool IsWiseCost(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsWiseCostRate();
		}

		public static bool IsCosting(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsCosting();
		}

		public static bool HasCarrierContractNumberLookup(this IRateEntry entry)
		{
			return entry.IsCosting() && RatingConstants.RateCategory.SupportsContractNumberLookup(entry.TI_RateCategory);
		}

		public static bool HasClientContractNumberLookup(this IRateEntry entry)
		{
			return entry.IsClientRate() && RatingConstants.RateCategory.SupportsContractNumberLookup(entry.TI_RateCategory);
		}

		public static bool IsStandardCostRate(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsStandardCostRate();
		}

		public static bool IsClientRate(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsClientRate();
		}

		public static bool IsLCL(this IRateEntry rateEntry)
		{
			return rateEntry.IsLCLFreight() || rateEntry.IsRoadLCL() || (rateEntry.IsSupplementaryEntry() && (rateEntry.TI_Mode == Core.Constants.RateMode.LCL || rateEntry.TI_Mode == Core.Constants.RateMode.LRO || rateEntry.TI_Mode == Core.Constants.RateMode.FTL || rateEntry.TI_Mode == Core.Constants.RateMode.LRA || rateEntry.TI_Mode == Core.Constants.RateMode.FWL));
		}

		public static bool IsRoadLCL(this IRateEntry rateEntry)
		{
			return (rateEntry.FreightMode() & MasterFiles.Business.FreightMode.NonContainerised) != 0 && !rateEntry.IsAir();
		}

		public static bool IsGlobalClientRate(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsGlobalClientRate();
		}

		public static bool IsClientRateHavingSubsidiaryRelations(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsClientRateHavingSubsidiaryRelations();
		}

		public static bool IsQuote(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.IsQuote();
		}

		public static bool IsOneOffQuote(this IRateEntry entry)
		{
			return entry.ParentRatingHeader.TH_OneTimeQuote;
		}

		public static bool IsOriginEntry(this IRateEntry entry)
		{
			return RatingConstants.RateCategory.IsOrigin(entry.TI_RateCategory);
		}

		public static bool IsDestinationEntry(this IRateEntry entry)
		{
			return RatingConstants.RateCategory.IsDestination(entry.TI_RateCategory);
		}

		public static bool IsFCLEntryWithEmptyContainer(this IRateEntry entry)
			=> entry.TI_RateCategory.ToString().In(RatingConstants.RateCategory.FCL, RatingConstants.RateCategory.CFC)
				&& entry.TI_RC.IsEmpty;

		public static GlbCompany Company(this IRateEntry entry)
		{
			return entry?.ParentRatingHeader == null ? GlbCompany.CurrentCompany : entry.ParentRatingHeader.Company;
		}

		public static RefCountry Country(this IRateEntry entry)
		{
			return Company(entry)?.Country ?? GlbCompany.CurrentCompany.Country;
		}

		public static RateType RateType(this IRateEntry entry)
		{
			return RatingConstants.RateCategory.GetRateType(entry.TI_RateCategory);
		}

		public static ILocation Origin(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_OriginLRC, entry.Factory);
		}

		public static ILocation Destination(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_DestinationLRC, entry.Factory);
		}

		public static ILocation Via(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_ViaLRC, entry.Factory);
		}

		public static ILocation PlannedLoad(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_PlannedLoadLRC, entry.Factory);
		}

		public static ILocation PlannedDischarge(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_PlannedDischargeLRC, entry.Factory);
		}

		public static ILocation FirstLoad(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_FirstLoadLRC, entry.Factory);
		}

		public static ILocation LastDischarge(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_LastDischargeLRC, entry.Factory);
		}

		public static ILocation FirstRouteSetLoad(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_FirstRouteSetLoadPortLRC, entry.Factory);
		}

		public static ILocation LastRouteSetDischarge(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_LastRouteSetDischargePortLRC, entry.Factory);
		}

		public static ILocation RateOrigin(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_RateOrigin, entry.Factory);
		}

		public static ILocation RateDestination(this IRateEntry entry)
		{
			return LocationHelper.GetCachedLocationFromString(entry.TI_RateDestination, entry.Factory);
		}

		public static bool IsBCN(this IRateEntry rateEntry) =>
			rateEntry.TI_Mode == Core.Constants.RateMode.BCN &&
			rateEntry.TI_RateCategory.ToString().In(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.DST, RatingConstants.RateCategory.AIR);

		public static bool IsSCN(this IRateEntry rateEntry) =>
			rateEntry.TI_Mode == Core.Constants.RateMode.SCN &&
			rateEntry.TI_RateCategory.ToString().In(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.DST, RatingConstants.RateCategory.AIR);

		public static bool IsULD(this IRateEntry rateEntry)
		{
			return rateEntry.TI_Mode == Core.Constants.RateMode.ULD
				|| rateEntry.TI_RateCategory == RatingConstants.RateCategory.CST && rateEntry.TI_Mode == Core.Constants.RateMode.AIR;
		}

		public static bool IsFCL(this IRateEntry entry)
		{
			return
				entry.TI_RateCategory == RatingConstants.RateCategory.FCL ||
				entry.TI_RateCategory == RatingConstants.RateCategory.CFC ||
				entry.TI_RateCategory == RatingConstants.RateCategory.SCO ||
				entry.IsRoadFCL() ||
				entry.IsShippingImportDetention() ||
				entry.IsShippingExportDetention() ||
				(
					entry.IsSupplementaryEntry() &&
					!(entry.TI_RateCategory == RatingConstants.RateCategory.CST) &&
					(
						entry.TI_Mode == Core.Constants.RateMode.FCL ||
						entry.TI_Mode == Core.Constants.RateMode.GRP ||
						entry.TI_Mode == Core.Constants.RateMode.FRO ||
						entry.TI_Mode == Core.Constants.RateMode.FRA
						)
					) ||
				(
					entry.TI_RateCategory == RatingConstants.RateCategory.CST &&
					(
						entry.TI_Mode == Core.Constants.RateMode.SEA ||
						entry.TI_Mode == Core.Constants.RateMode.ROA ||
						entry.TI_Mode == Core.Constants.RateMode.RAI
						)
					);
		}

		public static bool IsContainerTypeAllowed(this IRateEntry entry) =>
			entry.IsFCL() ||
			entry.IsULD() ||
			entry.IsWHS() ||
			entry.IsTRW() ||
			entry.IsTWU() ||
			entry.IsBCN() ||
			entry.IsSCN() ||
			((entry.IsContainerYard() || entry.IsContainerYardTPU()) && !entry.TI_YardUnitType.IsEmpty) ||
			entry.TI_Mode == Core.Constants.RateMode.FTL;

		public static FreightMode FreightMode(this IRateEntry entry)
		{
			var result = MasterFiles.Business.FreightMode.UKN;

			if (entry.TI_RateCategory.ToString().In(RatingConstants.RateCategory.FCL, RatingConstants.RateCategory.SCO, RatingConstants.RateCategory.CFC))
			{
				switch (entry.TI_Mode)
				{
					case Core.Constants.RateMode.SEA:
						result = MasterFiles.Business.FreightMode.FCL;
						break;

					case Core.Constants.RateMode.ROA:
						result = MasterFiles.Business.FreightMode.FRO;
						break;

					case Core.Constants.RateMode.RAI:
						result = MasterFiles.Business.FreightMode.FRA;
						break;

					case Core.Constants.RateMode.BCN:
						result = MasterFiles.Business.FreightMode.BCN;
						break;
				}
			}
			else if (!Enum.TryParse(entry.TI_Mode, out result))
			{
				result = MasterFiles.Business.FreightMode.UKN;
			}

			return result;
		}

		#region IsAir / IsSea / IsRoad / IsRail / IsMail

		public static bool IsAir(this IRateEntry entry)
		{
			return (entry.FreightMode() & MasterFiles.Business.FreightMode.AIR) != 0
					|| entry.TI_Mode == Core.Constants.RateMode.AIR
					|| entry.TI_Mode == Core.Constants.RateMode.ULD
					|| entry.TI_Mode == Core.Constants.RateMode.LSE;
		}

		public static bool IsSea(this IRateEntry entry)
		{
			return entry.TI_Mode == Core.Constants.RateMode.SEA ||
					entry.TI_Mode == Core.Constants.RateMode.LCL ||
					entry.TI_Mode == Core.Constants.RateMode.FCL ||
					entry.TI_Mode == Core.Constants.RateMode.GRP;
		}

		public static bool IsRoad(this IRateEntry entry)
		{
			return entry.TI_Mode == Core.Constants.RateMode.ROA ||
					entry.TI_Mode == Core.Constants.RateMode.FRO ||
					entry.TI_Mode == Core.Constants.RateMode.LRO ||
					entry.TI_Mode == Core.Constants.RateMode.FTL;
		}

		public static bool IsRail(this IRateEntry entry)
		{
			return entry.TI_Mode == Core.Constants.RateMode.RAI ||
					entry.TI_Mode == Core.Constants.RateMode.FRA ||
					entry.TI_Mode == Core.Constants.RateMode.LRA ||
					entry.TI_Mode == Core.Constants.RateMode.FWL;
		}

		public static bool IsCourier(this IRateEntry entry)
		{
			return entry.TI_Mode == RateMode.COU ||
					entry.TI_Mode == RateMode.OBC ||
					entry.TI_Mode == RateMode.UNA;
		}

		public static bool IsMail(this IRateEntry entry)
		{
			return entry.TI_Mode == Core.Constants.RateMode.MAI;
		}

		#endregion

		public static bool IsShipping(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.Shipping;
		}

		public static bool IsForwarding(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.Forwarding;
		}

		public static bool IsCustoms(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.Customs;
		}

		public static bool IsWHS(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.Warehouse;
		}

		public static bool IsTRW(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.TransitWarehouse;
		}

		public static bool IsTWU(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.TransitWarehouseTransportationUnit;
		}

		public static bool IsContainerYard(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.ContainerYard;
		}

		public static bool IsContainerYardTPU(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.ContainerYardTransportationUnit;
		}

		public static bool IsRoadFCL(this IRateEntry entry)
		{
			return (entry.FreightMode() & MasterFiles.Business.FreightMode.Containerised) != 0 && !entry.IsAir();
		}

		public static bool IsShippingExportDetention(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.ShippingExportDetention;
		}

		public static bool IsShippingImportDetention(this IRateEntry entry)
		{
			return entry.RateType() == Integration.RateType.ShippingImportDetention;
		}

		internal static IWhsWarehouse Warehouse(this IRateEntry entry)
		{
			return entry.Factory.Load<IWhsWarehouse>(entry.TI_WW_Warehouse);
		}

		internal static IWhsWarehouse Yard(this IRateEntry entry)
		{
			return entry.Factory.Load<IWhsWarehouse>(entry.TI_CYC_WW_Facility);
		}

		public static ZString KeyForQuotationPricingPage(this IRateEntry entry, bool exactMatch = false)
		{
			var header = entry.ParentRatingHeader as RatingHeader;
			if (header != null)
			{
				return string.Join(entry.PK.ToString(),
					header.TH_PrintRateLevelOriginCharges,
					header.TH_PrintInheritedOriginCharges,
					header.TH_PrintRateLevelDestinationCharges,
					header.TH_PrintInheritedDestinationCharges,
					exactMatch);
			}

			return entry.PK.ToString();
		}

		public static Directions JobDirection(this IRateEntry entry)
		{
			var originCountryCode = entry.Origin()?.Country?.Code ?? ZString.Empty;
			var destinationCountryCode = entry.Destination()?.Country?.Code ?? ZString.Empty;
			return entry.TI_IsCrossTrade ? Directions.CrossTrade : ImportExportHelper.GetJobDirection(originCountryCode, destinationCountryCode);
		}

		public static bool IsAirFreight(this IRateEntry entry) => entry.TI_RateCategory.ToString().In(RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.CAI);

		public static bool IsLCLFreight(this IRateEntry entry)
			=> entry.TI_RateCategory.ToString().In(RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.CLC);

		public static bool IsULDFreight(this IRateEntry entry)
		{
			return entry.IsAirFreight() && entry.TI_Mode == Core.Constants.RateMode.ULD;
		}

		public static bool IsLooseFreight(this IRateEntry entry)
		{
			return entry.IsAirFreight() && entry.TI_Mode == Core.Constants.RateMode.LSE;
		}

		public static bool IsSeaFreight(this IRateEntry entry)
		{
			return entry.IsFreightEntry() && entry.IsSea();
		}

		public static bool IsRoadFreight(this IRateEntry entry)
		{
			return entry.IsFreightEntry() && entry.IsRoad();
		}

		public static bool IsRailFreight(this IRateEntry entry)
		{
			return entry.IsFreightEntry() && entry.IsRail();
		}

		public static bool IsCFS(this IRateEntry entry)
		{
			return (entry.RateType() & Integration.RateType.CFS) != 0;
		}

		public static bool IsDomesticTransport(this IRateEntry entry)
		{
			return (entry.RateType() & Integration.RateType.TransportBookings) != 0;
		}

		public static bool IsPortTransport(this IRateEntry entry)
		{
			return (entry.RateType() & Integration.RateType.LocalTransport) != 0;
		}

		public static bool IsYardUnitTypeEmpty(this IRateEntry entry)
		{
			return (entry.IsContainerYard() || entry.IsContainerYardTPU()) && entry.TI_YardUnitType.IsEmpty;
		}

		public static int DecimalPlaces(this IRateEntry entry)
		{
			return !entry.IsCostRate() ? DataRegistryRating.Instance.SellRatesDecimals.GetDecimals(entry.TI_RateCategory) : 4;
		}

		public static ZString OriginCountryCode(this IRateEntry entry)
		{
			return entry.Origin()?.Country?.Code ?? ZString.Empty;
		}

		public static OrgHeader CarrierServiceLevelParent(this IRateEntry entry)
		{
			return entry.TransportProvider ?? (entry.IsCostRate()
											? (entry.ParentRatingHeader.Header ?? entry.Supplier)
											: entry.Supplier);
		}

		public static ZGuid ServiceProviderPK(this IRateEntry entry)
		{
			return entry.ParentRatingHeader?.TH_OH ?? ZGuid.Empty;
		}

		public static RateEntryLookups Lookups(this IRateEntry entry)
		{
			if (entry is RateEntry rateEntryBizO)
			{
				return rateEntryBizO.Lookups();
			}

			return new RateEntryLookups(entry, entry.Factory);
		}

		public static string ProductName(this IRateEntry entry)
		{
			if (entry is WiseEntry wiseEntry)
			{
				return wiseEntry.ProductName;
			}

			return string.Empty;
		}

		public static string Commodities(this IRateEntry entry)
		{
			if (entry is WiseEntry wiseEntry && wiseEntry.Commodities != null)
			{
				return String.Join(", ", wiseEntry.Commodities);
			}

			return string.Empty;
		}

		public static string FreightRatePerChargeableUnit(this IRateEntry entry)
		{
			var result = string.Empty;
			if (entry.ChildRateLines != null && entry.ChildRateLines.Any())
			{
				var sb = new ZStringBuilder();
				var perUnitFreightRatesByCurrency = entry.ChildRateLines
															.Where(x => x.IsCalculatorInitialized && x.Calculator != null && x.TL_AC == Env.Registry.FreightChargeCode)
															.SelectMany(x => x.Calculator.PricePerSingleChargeable)
															.Where(pb => pb.RateInfo.Type == RateInfo.RateInfoType.UNT)
															.ExcludingZeroValues()
															.GroupBy(x => x.Currency)
															.ToArray();

				if (perUnitFreightRatesByCurrency.Any())
				{
					foreach (var currencyGroup in perUnitFreightRatesByCurrency)
					{
						var basesByUnit = currencyGroup.GroupBy(x => x.RateInfo.Unit);
						foreach (var unitGroup in basesByUnit)
						{
							sb.AppendFormat("{0} {1}/{2}", currencyGroup.Key, unitGroup.Sum(x => x.RateInfo.PerUnitRate)?.ToString("F4", CultureInfo.CurrentCulture), unitGroup.Key);
						}
					}
				}

				result = sb.ToStringWithDelimiterBetweenAppends(" + ");
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "CA should be smarter")]
		public static string AllInCost(this IRateEntry entry)
		{
			const string defaultCurrencyCodeToDisplayAllInCost = CurrencyCodes.UnitedStates;
			var isWiseEntryView = entry is WiseEntryView;
			var result = string.Empty;
			if (entry.ChildRateLines != null && entry.ChildRateLines.Any())
			{
				var allCostsPerChargeable = new List<(decimal? amount, ZString currency)>();

				foreach (var line in entry.ChildRateLines.Where(x => x.IsCalculatorInitialized && x.Calculator != null && x.Calculator.PricePerSingleChargeable.Any()))
				{
					decimal originalRateToRateFactor = 0;
					ZString currencyCode = ZString.Empty;
					if (isWiseEntryView && line is WiseLine wiseLine)
					{
						currencyCode = Convert.ToString(wiseLine.CustomFields?.FirstOrDefault(x => x.Code == Rate.CustomFields.CargoSphere.CurrencyCode)?.Value);
						if (string.Equals(currencyCode, defaultCurrencyCodeToDisplayAllInCost, StringComparison.OrdinalIgnoreCase)
							&& decimal.TryParse(Convert.ToString(wiseLine.CustomFields.First(x => x.Code == Rate.CustomFields.CargoSphere.OriginalRateToRateFactor).Value), out decimal factor))
						{
							originalRateToRateFactor = factor;
						}
					}

					if (line.Calculator.PricePerSingleChargeable.Any(x => x.RateInfo.Type == RateInfo.RateInfoType.MIN && x.RateInfo.MinRate > decimal.MinValue))
					{
						var minimumBasesPerCurrency = line.Calculator.PricePerSingleChargeable.Where(x => x.RateInfo.Type == RateInfo.RateInfoType.MIN).GroupBy(x => x.Currency);
						foreach (var minimums in minimumBasesPerCurrency)
						{
							var amount = minimums.OrderByDescending(x => x.RateInfo.MinRate).Select(x => x.RateInfo.MinRate).First();

							if (originalRateToRateFactor > 0)
							{
								allCostsPerChargeable.Add((amount * originalRateToRateFactor, currencyCode));
							}
							else
							{
								allCostsPerChargeable.Add((amount, minimums.Key));
							}
						}
					}
					else
					{
						var flatAndPerUnitBases = line.Calculator.PricePerSingleChargeable
														.Where(x => x.RateInfo.Type == RateInfo.RateInfoType.FLT || x.RateInfo.Type == RateInfo.RateInfoType.UNT)
														.ExcludingZeroValues()
														.Select(x => originalRateToRateFactor > 0
															? ((x.RateInfo.Type == RateInfo.RateInfoType.FLT ? x.RateInfo.FlatRate : x.RateInfo.PerUnitRate) * originalRateToRateFactor, currencyCode)
															: (x.RateInfo.Type == RateInfo.RateInfoType.FLT ? x.RateInfo.FlatRate : x.RateInfo.PerUnitRate, x.Currency));
						allCostsPerChargeable.AddRange(flatAndPerUnitBases);
					}
				}

				if (allCostsPerChargeable.Any())
				{
					var sb = new ZStringBuilder();

					var costsByCurrency = allCostsPerChargeable.GroupBy(x => x.currency).ToArray();
					foreach (var currency in costsByCurrency)
					{
						sb.AppendFormat("{0} {1}", currency.Key, currency.Sum(x => x.amount)?.ToString("F4", CultureInfo.CurrentCulture));
					}

					result = sb.ToStringWithDelimiterBetweenAppends(" + ");
				}
			}

			return result;
		}

		public static ZString DisplayInfo(this IRateEntry entry)
		{
			if (entry == null)
			{
				return Res.GetString("040d709b-ff64-466b-aff3-60b9e493bb49", "null");
			}

			return entry.ParentRatingHeader.DisplayInfo();
		}

		/// <summary>
		/// Determine which rate types can be filtered by Carrier Contract Number
		/// </summary>
		public static IEnumerable<IRateEntry> FilterByContractNumber(this IEnumerable<IRateEntry> rateEntries, bool isCosting) =>
			isCosting
				? rateEntries.Where(CanFilterByCarrierContractNumber)
				: rateEntries.Where(CanFilterByClientContractNumber);

		/// <summary>
		/// Determine which rate types can be used to update job Contract Number.
		/// Similar to FilterByContractNumber now because when a rate type uses contract number to filter rates, it should be able to update job contract number.
		/// Rating adapters can decide how they update the numbers later too.
		/// </summary>
		public static IEnumerable<AutoRateInfo> FilterByContractNumber(this IEnumerable<AutoRateInfo> rateInfos, bool isCosting) =>
			isCosting
			 ? rateInfos.Where(rateInfo => rateInfo.Entry.CanFilterByCarrierContractNumber())
			 : rateInfos.Where(rateInfo => rateInfo.Entry.CanFilterByClientContractNumber());

		static bool CanFilterByCarrierContractNumber(this IRateEntry entry)
		{
			// Tact entries (only in Costing and Standard Costing) are not filtered by contract number
			if (entry == null || entry.TI_IsTact)
			{
				return false;
			}
			var parentHeader = entry.ParentRatingHeader;
			return parentHeader != null && ApplicableRateTypesForCarrierContractNumberFiltering.Contains(parentHeader.TH_RateType);
		}

		static bool CanFilterByClientContractNumber(this IRateEntry entry)
		{
			if (entry == null)
			{
				return false;
			}
			var parentHeader = entry.ParentRatingHeader;
			return parentHeader != null && ApplicableRateTypesForClientContractNumberFiltering.Contains(parentHeader.TH_RateType);
		}

		// WTG does not recommend "CW1021 The static field may not be thread safe" so let's calculate it every time.
		static HashSet<string> ApplicableRateTypesForCarrierContractNumberFiltering => new HashSet<string>
		{
			RatingConstants.RatingHeaderTypes.Costing,
			RatingConstants.RatingHeaderTypes.WiseCost
		};

		// WTG does not recommend "CW1021 The static field may not be thread safe" so let's calculate it every time.
		static HashSet<string> ApplicableRateTypesForClientContractNumberFiltering => new HashSet<string>
		{
			RatingConstants.RatingHeaderTypes.ClientRate,
			RatingConstants.RatingHeaderTypes.Tariff,
			RatingConstants.RatingHeaderTypes.Quote,
		};
	}
}
