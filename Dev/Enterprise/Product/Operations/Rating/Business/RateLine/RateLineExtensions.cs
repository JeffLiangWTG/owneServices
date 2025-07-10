namespace Enterprise.Rating.Business
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Environment;
	using static System.FormattableString;

	public static class RateLineExtensions
	{
		public static bool Uses(this IRateLine line, CalculatorType calcType)
			=> line.RateCalculatorType == calcType;

		public static T GetCalculator<T>(this IRateLine line) where T : Calculator
		{
			return line.Calculator as T;
		}

		public static bool IsInclusive(this IRateLine line) => line != null && line.RateCalculatorType == CalculatorType.FreightInclusive;

		public static bool IsPercentage(this IRateLine line) => line != null && line.RateCalculatorType == CalculatorType.Percentage;

		public static string DescriptionWithInclusiveCharges(this IRateLine rateLine)
		{
			if (rateLine.IncludedLines != null && rateLine.IncludedLines.Any())
			{
				// Ordering lines by mapped charge code then by carrier charge code if the charge is not mapped
				var includedLines = rateLine.IncludedLines
					.OrderBy(l => l.ChargeCode != null ? 0 : 1)
					.ThenBy(l => l.ChargeCode?.AC_Code)
					.ThenBy(l => l.CarrierChargeCode)
					.Select(GetIncludedChargeDescription)
					.Distinct()
					.ToArray();

				var sb = new ZStringBuilder();
				sb.AppendLine(rateLine.TL_RateDesc);
				sb.AppendLine();
				sb.Append(Res.GetString("b264eb6b-d989-4317-8bd5-3dc03c0c3eec", "Inclusive charges:"));

				foreach (var line in includedLines)
				{
					sb.AppendLine();
					sb.Append(line);
				}

				return sb.ToString();
			}
			else
			{
				return rateLine.TL_RateDesc;
			}
		}

		static string GetIncludedChargeDescription(IRateLine rateLine)
		{
			var desc = rateLine.ChargeCode != null
				? Invariant($"\t{rateLine.ChargeCode.AC_Code} - {rateLine.ChargeCode.AC_DescMultilingual}")
				: Invariant($"\t{rateLine.CarrierChargeCode} - {rateLine.CarrierChargeCodeDescription} (Universal Code: {rateLine.UniversalChargeCodes})");   // Not translatable

			if (!rateLine.ChargeType.IsEmpty)
			{
				desc = Invariant($"{desc} ({rateLine.ChargeType})"); // Nothing to translate
			}

			return desc;
		}

		public static ZBool UsesCostBasedCalculator(this IRateLine line)
			=> line.RateCalculatorType == CalculatorType.CostBased;

		public static ZBool UsesCompanyTariffBasedCalculator(this IRateLine line)
			=> line.RateCalculatorType == CalculatorType.CompanyTariffBased;

		public static ZBool UsesCompanyTariffOrCostBasedCalculator(this IRateLine line)
		{
			return UsesCompanyTariffBasedCalculator(line) || UsesCostBasedCalculator(line);
		}

		public static ZBool UsesCalculatorsSupportedOnFCLRateEntryWithEmptyContainer(this IRateLine line)
		{
			return line.Uses(CalculatorType.Flat) || line.Uses(CalculatorType.Percentage);
		}

		public static ZBool UsesACIZones(this IRateLine line)
		{
			return line.Factory.GetCachedValue(line.GetACICacheKey(), () => line.GetCalculator<CartageZoneDistanceCalculator>()?.UseACIZones ?? false);
		}

		internal static ZString GetACICacheKey(this IRateLine line) => line.PK + line.TL_RateCalculator + ".UsesACIZones"; // Cache Key should not be translated

		public static bool IsCosting(this IRateLine line) => line.ParentRateEntry?.IsCosting() ?? false; // ToDo ParentRateEntry is supposed to never be null - should be no null check here

		public static bool IsWiseCost(this IRateLine line) => line.ParentRateEntry?.IsWiseCost() ?? false; // ToDo ParentRateEntry is supposed to never be null - should be no null check here

		public static bool IsCostRate(this IRateLine line) => line?.ParentRateEntry?.IsCostRate() ?? false;

		public static bool IsIntercompanyTariff(this IRateLine line) => line.ParentRateEntry?.IsIntercompanyTariff() ?? false; // ToDo ParentRateEntry is supposed to never be null - should be no null check here

		public static bool IsFeeChargeSame(this IRateLine line, IRateLine other)
		{
			if (line == null || other == null)
			{
				return false;
			}

			return line.TL_FeeChargeType == other.TL_FeeChargeType && line.TL_FeeChargeLevel == other.TL_FeeChargeLevel;
		}

		public static bool IsApplicableToOrg(this IRateLine rateLine, OrgHeader orgHeader, RatingCriteria criteria)
		{
			var result = false;
			if (orgHeader != null)
			{
				var effectiveDate = criteria.GetEffectiveDateWithFallback(rateLine, isCosting: _Rating.Cost).date;
				if (rateLine.ParentRateEntry.IsCompanyTariff())
				{
					if (criteria != null)
					{
						result = rateLine.IsApplicableToLevel(orgHeader, criteria, effectiveDate) || criteria.GetSubsidiaryDebtors(orgHeader).Any(sub => rateLine.IsApplicableToLevel(sub, criteria, effectiveDate)) || rateLine.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel == criteria.TariffLevel;
					}
				}
				else
				{
					var lineOrgHeader = rateLine.ParentRateEntry.ParentRatingHeader.Header;
					result = lineOrgHeader.PKEquals(orgHeader) || IsGroupRateApplicableSubsidiaryOrg(rateLine.ParentRateEntry, orgHeader, criteria, effectiveDate);
				}
			}

			return result;
		}

		public static bool IsApplicableToLevel(this IRateLine rateLine, OrgHeader orgHeader, RatingCriteria criteria, ZDate effectiveDate)
		{
			string rateMode = RatingHelper.GetRateMode(criteria, rateLine.ParentRateEntry.TI_RateCategory);
			var level = RatingCache.GetCompanyTariffLevel(orgHeader, criteria.Company, rateLine.ParentRateEntry.TI_RateCategory, rateMode, criteria.Direction, effectiveDate).level;

			return rateLine.TL_FeeChargeType.IsEmpty
				? level == rateLine.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel
				: rateLine.ParentRateEntry.IsRateEntryCompanyTariffLevelCorrespondToOrgLevel(level) && rateLine.IsFeeChargeApplicable(orgHeader);
		}

		static bool IsRateEntryCompanyTariffLevelCorrespondToOrgLevel(this IRateEntry rateEntry, int level)
		{
			const int feesAndChargeCompanyTariffLevel = 1;
			var headerLevel = rateEntry.ParentRatingHeader.TH_GlobalRateLevel;

			return level == 0
				? headerLevel == feesAndChargeCompanyTariffLevel
				: headerLevel == level;
		}

		public static bool IsGroupRateApplicableSubsidiaryOrg(IRateEntry rateEntry, OrgHeader orgHeader, RatingCriteria criteria, ZDate effectiveDate = default)
		{
			var rateMode = rateEntry.TI_RateCategory.IsEmpty
				? (ZString)criteria.FreightMode.ToString()
				: RatingHelper.GetRateMode(criteria, rateEntry.TI_RateCategory);

			var companyTariffLevel = RatingCache.GetCompanyTariffLevel(orgHeader, criteria.Company, rateEntry.TI_RateCategory, rateMode, criteria.Direction, effectiveDate);
			if (!companyTariffLevel.applyGroupRate)
			{
				return false;
			}

			var lineOrgHeader = rateEntry.ParentRatingHeader.Header;
			return lineOrgHeader.RelatedManagementSubsidiaries().Any(subsidy => subsidy.PK == orgHeader.PK);
		}

		public static bool IsFeeChargeApplicable(this IRateLine rateLine, OrgHeader org)
		{
			if (org == null)
			{
				return false;
			}

			return org.CompanyData != null && org.CompanyData.RateFeeChargeLevels.Any(x => x.ORF_ServiceType == rateLine.TL_FeeChargeType && x.ORF_Level == rateLine.TL_FeeChargeLevel);
		}

		public static ZBool IsPivotBreakOverrideApplicable(this IRateLine line)
		{
			return (line.ParentRateEntry != null && line.ParentRateEntry.IsCostRate())
				&& (line.Uses(CalculatorType.CartageZoneDistance) || line.Uses(CalculatorType.Combined) || line.Uses(CalculatorType.Cartage))
				&& QuantityUnit.IsWeight(line.TL_WeightVolume)
				&& line.ChargeCode.PK == Env.Registry.FreightChargeCode;
		}

		public static ZBool IsBreaksPerApplicable(this IRateLine line)
			=> (line.ParentRateEntry != null && line.ParentRateEntry.IsForwarding())
			&& QuantityUnit.IsWeight(line.TL_WeightVolume)
			&& !RateLineHelper.RequiresChargeableFromJob(line)
			&& (line.Uses(CalculatorType.CartageZoneDistance) || line.Uses(CalculatorType.Combined) || line.Uses(CalculatorType.Cartage));

		public static RateType RateType(this IRateLine line)
		{
			return line.ParentRateEntry.RateType();
		}

		public static bool UseOnlyActualWeightMeasure(this IRateLine line)
		{
			return line.TL_ActualPercentage == 100;
		}

		public static void SetCalculationOrder(this IRateLine line, ZInt value)
		{
			var dependentCalculator = line.Calculator as IDependentCalculator;
			if (dependentCalculator != null)
			{
				dependentCalculator.SetCalculationOrder(value);
			}
		}

		public static ZInt GetCalculationOrder(this IRateLine line)
		{
			var dependentCalculator = line.Calculator as IDependentCalculator;
			if (dependentCalculator != null)
			{
				return dependentCalculator.GetCalculationOrder();
			}
			return 0;
		}

		public static IRateLineItem FindRateLineItem(this IRateLine line, string tm_type)
		{
			return line.ChildRateLineItems.FirstOrDefault(x => x.TM_Type.Trim().ToUpper() == tm_type.Trim().ToUpper());
		}

		public static OrgSupplierPart ProductNumber(this IRateLine line)
		{
			return line.Factory.Load<OrgSupplierPart>(line.TL_OP_ProductNumber);
		}

		public static FeeChargeType GetFeeChargeType(this IRateLine line)
		{
			return OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value
					.FeeChargeTypes
					.Cast<FeeChargeType>()
					.FirstOrDefault(t => t.Code == line.TL_FeeChargeType);
		}

		public static ZString DisplayInfo(this IRateLine line)
		{
			if (line == null)
			{
				return Res.GetString("040d709b-ff64-466b-aff3-60b9e493bb49", "null");
			}

			if (line.ParentRateEntry.IsSpotEntry)
			{
				return SpotEntryDescription(line);
			}

			var ac_code = line.ChargeCode != null ? line.ChargeCode.AC_Code : ZString.Empty;
			var container = line.ParentRateEntry.Container;
			var containerCode = container != null ? container.RC_Code : ZString.Empty;

			var result = new ZStringBuilder();
			result.Append(ac_code);
			result.Append(line.TL_RateCalculator);
			if (!line.TL_WeightVolume.IsEmpty)
			{
				result.Append(line.TL_WeightVolume);
			}

			result.AppendIfNotEmpty(containerCode);
			result.AppendIfNotEmpty(line.ParentRateEntry.ParentRatingHeader.DisplayInfo());

			return result.ToStringWithDelimiterBetweenAppends("-");
		}

		public static ZString GetSpotRateDescription(this IRateLine line)
		{
			if (!(line.ParentRateEntry?.IsSpotEntry).GetValueOrDefault())
			{
				return ZString.Empty;
			}
			else
			{
				var spotRateSource = line.IsContainerSpotRate()
									? Res.GetString("175e9d3f-71ed-45ad-a17d-abca75f29981", "Container")
									: Res.GetString("8d31a82f-ec4b-41c7-a1e8-3a902d410b88", "Job");

				return string.Format(CultureInfo.CurrentCulture, "{0} {1}", spotRateSource, (line as RateLine)?.SpotRateDescription);
			}
		}

		static ZString SpotEntryDescription(IRateLine line)
		{
			var sb = new ZStringBuilder();
			sb.AppendIfNotEmpty(line?.ChargeCode?.AC_Code);
			sb.Append(GetSpotRateDescription(line));

			return sb.ToStringWithDelimiterBetweenAppends("-");
		}

		public static bool IsContainerSpotRate(this IRateLine line)
		{
			return line.ParentRateEntry != null && line.ParentRateEntry.ContainerPKForSpotEntry != ZGuid.Empty;
		}

		internal static ILocation GetCartageLocationForZones(this IRateLine line)
		{
			ILocation result = null;
			if (line != null)
			{
				var parentRateEntry = line.ParentRateEntry;
				if (parentRateEntry.IsWHS() || parentRateEntry.IsTRW() || parentRateEntry.IsTWU())
				{
					var warehouse = parentRateEntry.Warehouse();
					if (warehouse != null)
					{
						result = line.Factory.Load<OrgAddress>(warehouse.WW_OA_WarehouseAddress);
					}
					else
					{
						result = GlbCompany.CurrentCompany.Country;
					}
				}
				else if (parentRateEntry.IsDestinationEntry())
				{
					result = parentRateEntry.CartageDeliveryAddressOverride ?? parentRateEntry.Destination();
				}
				else if (parentRateEntry.IsOriginEntry())
				{
					result = parentRateEntry.CartagePickupAddressOverride ?? parentRateEntry.Origin();
				}
			}

			return result;
		}

		public static void ConvertChargeCode(this RateLine line, bool isConvertingToGlobal)
		{
			var ledgerType = line.IsCosting() ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
			if (line.ChargeCode != null)
			{
				var chargeCode = GetChargeCode(isConvertingToGlobal, ledgerType, line.ChargeCode);
				if (chargeCode != null && chargeCode.PK != line.TL_AC)
				{
					using (line.GetValidationSuspender())
					{
						var locked = line.LockCalculator;
						line.LockCalculator = true;
						line.TL_AC = chargeCode.PK;
						line.LockCalculator = locked;
					}
				}
			}

			var percentageOfRateLineItems = line.RateLineItems.Cast<RateLineItem>().Where(x => x.RateOperatorIsApplyTo() && x.ChargeCode != null);
			foreach (var rateLineItem in percentageOfRateLineItems)
			{
				var percentOf = GetChargeCode(isConvertingToGlobal, ledgerType, rateLineItem.ChargeCode);
				if (percentOf != null && percentOf.PK != rateLineItem.TM_AC)
				{
					using (rateLineItem.GetValidationSuspender())
					{
						rateLineItem.TM_AC = percentOf.PK;
					}
				}
			}
		}

		static AccChargeCode GetChargeCode(bool isConvertingToGlobal, string ledgerType, AccChargeCode chargeCode)
		{
			if (isConvertingToGlobal)
			{
				return chargeCode.GetGlobalChargeCode(ledgerType, null);
			}

			return chargeCode.GetLocalChargeCode(ledgerType, null);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool RequiresWeightVolume(this IRateLine line)
		{
			if (line == null)
			{
				return false;
			}

			switch (line.RateCalculatorType)
			{
				case CalculatorType.Agency:
				case CalculatorType.DisbursementInterest:
				case CalculatorType.Percentage:
				case CalculatorType.CompanyTariffBased:
				case CalculatorType.CostBased:
				case CalculatorType.EquipmentHire:
				case CalculatorType.ExcludeCompanyTariffs:
				case CalculatorType.Flat:
				case CalculatorType.HousebillReleaseType:
				case CalculatorType.ValueRange:
				case CalculatorType.WarehouseLocationType:
				case CalculatorType.Minimum:
				case CalculatorType.Note:
				case CalculatorType.PackageCount:
				case CalculatorType.FreightInclusive:
				case CalculatorType.ProfitShareRebate:
				case CalculatorType.HighestCharge:
				case CalculatorType.None:
					return false;

				case CalculatorType.HighestRate:
					// Please don't simplify the code, it has been broken down due to a hard-to-reproduce bug and we're seeing if these changes help
					if (line.ChildRateLineItems != null)
					{
						var items = line.ChildRateLineItems.ToArray();
						var totalCount = items.Count(item => item != null && item.TM_Type == Calculator.Items.Operator.UNT);
						return totalCount == 1;
					}
					else
					{
						return false;
					}

				case CalculatorType.PercentageBreaks:
					return !(line.Calculator as PercentageBreaksCalculator).UseBreaksBasedOnValues;

				case CalculatorType.Unit:
				case CalculatorType.Equalization:
				case CalculatorType.FirstPlusAdditional:
				case CalculatorType.FlatPlusPerUnit:
				case CalculatorType.MinimumOrPerUnit:
				case CalculatorType.SplitMonthBilling:
				case CalculatorType.Cartage:
				case CalculatorType.CartageZoneDistance:
				case CalculatorType.Combined:
				case CalculatorType.CombinedWithIncrement:
				case CalculatorType.Time:
				case CalculatorType.WarehousePack:
#if DEBUG
				case CalculatorType.ForTest_CalculationLog:
				case CalculatorType.ForTest_CalculatorPropertyAttribute:
				case CalculatorType.ForTest_CartageCalculatorWithOverride:
				case CalculatorType.ForTest_ConcreteCalculator:
#endif
					return true;

				default:
					if (string.IsNullOrWhiteSpace(line.TL_RateCalculator) || line.Calculator is NullCalculator)
					{
						return false;
					}

					throw new NotSupportedException(ZString.Format("Please decide whether {0} calculator supports Unit on the RateLine or not", line.RateCalculatorType));
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "CodeAnalyse, you are drunk")]
		public static ConversionFactor GetDefaultConversionFactor(this IRateLine rateLine, ZString weightVolumeUnit)
		{
			if (rateLine == null)
			{
				return ConversionFactor.Empty;
			}

			if (Constants.LoadingLength.ContainsCode(weightVolumeUnit))
			{
				return ConversionFactor.Standard.Metric.LoadingMeters;
			}

			var unitsSystem = Constants.Weight.IsImperial(weightVolumeUnit) || Constants.Volume.IsImperial(weightVolumeUnit)
				? UnitsSystem.Imperial
				: UnitsSystem.Metric;

			switch (rateLine.ParentRateEntry.TI_Mode)
			{
				case Core.Constants.RateMode.LSE:
					return unitsSystem == UnitsSystem.Metric ? ConversionFactor.Standard.Metric.Air : ConversionFactor.Standard.Imperial.Air;

				case Core.Constants.RateMode.SEA:
				case Core.Constants.RateMode.LCL:
					return unitsSystem == UnitsSystem.Metric ? ConversionFactor.Standard.Metric.Sea : ConversionFactor.Standard.Imperial.Sea;

				case Core.Constants.RateMode.RAI:
				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FWL:
					return unitsSystem == UnitsSystem.Metric ? ConversionFactor.Standard.Metric.Rail : ConversionFactor.Standard.Imperial.Rail;

				case Core.Constants.RateMode.ROA:
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.FTL:
					return unitsSystem == UnitsSystem.Metric ? ConversionFactor.Standard.Metric.Road : ConversionFactor.Standard.Imperial.Domestic;

				default:
					return ConversionFactor.Empty;
			}
		}

		#region MarkUp

		static MarkUpPercentagesCollection GetMarkUpPercentagesCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("MarkupPercentages", () => new MarkUpPercentagesCollection(Env.Registry.Rating.MarkUpPercentages));
		}

		static MarkUpPercentagesCollection GetMinimumMarkUpPercentagesCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("MinimumMarkupPercentages", () => new MarkUpPercentagesCollection(Env.Registry.Rating.MinimumMarkUpPercentages));
		}

		public static ZDecimal DefaultMarkupPercentage(this IRateLine rateLine)
		{
			return GetMarkupPercentage(rateLine, GetMarkUpPercentagesCollection(rateLine.Factory), 0m);
		}

		public static ZDecimal[] DefaultMarkupMinimumAndPerUnit(this IRateLine rateLine)
		{
			return GetMarkupMinimumAndPerUnit(rateLine, GetMarkUpPercentagesCollection(rateLine.Factory));
		}

		public static ZDecimal MinimumMarkupPercentage(this IRateLine rateLine)
		{
			return GetMarkupPercentage(rateLine, GetMinimumMarkUpPercentagesCollection(rateLine.Factory), decimal.MinValue);
		}

		public static ZDecimal[] MinimumMarkupMinimumAndPerUnit(this IRateLine rateLine)
		{
			return GetMarkupMinimumAndPerUnit(rateLine, GetMinimumMarkUpPercentagesCollection(rateLine.Factory));
		}

		static ZDecimal GetMarkupPercentage(IRateLine rateLine, MarkUpPercentagesCollection collection, ZDecimal defaultValue)
		{
			var markUp = FindMarkUpPercentageElement(rateLine, collection);

			return markUp == null ? defaultValue : markUp.Percentage;
		}

		static ZDecimal[] GetMarkupMinimumAndPerUnit(IRateLine rateLine, MarkUpPercentagesCollection collection)
		{
			var markUp = FindMarkUpPercentageElement(rateLine, collection);
			return markUp == null ? new ZDecimal[2] : new[] { markUp.Minimum, markUp.PerUnit };
		}

		static MarkUpPercentage FindMarkUpPercentageElement(IRateLine rateLine, MarkUpPercentagesCollection collection)
		{
			ILocation locationForMarkup;

			if (rateLine.ParentRateEntry.IsOriginEntry())
			{
				locationForMarkup = rateLine.ParentRateEntry.Origin();
			}
			else if (rateLine.ParentRateEntry.IsDestinationEntry())
			{
				locationForMarkup = rateLine.ParentRateEntry.Destination();
			}
			else if (rateLine.ParentRateEntry.IsImport())
			{
				locationForMarkup = rateLine.ParentRateEntry.Origin();
			}
			else
			{
				locationForMarkup = rateLine.ParentRateEntry.Destination();
			}

			var isImport = rateLine.ParentRateEntry.Destination() != null && rateLine.ParentRateEntry.Destination().Country != null && rateLine.ParentRateEntry.Company() != null && rateLine.ParentRateEntry.Destination().Country.Code == rateLine.ParentRateEntry.Company().GC_RN_NKCountryCode;

			var companyPK = rateLine.ParentRateEntry.Company()?.PK.ToGuid() ?? Guid.Empty;

			ZString mode;
			if (rateLine.TL_AC == Env.Registry.GetFreightChargeCode(companyPK) || rateLine.ParentRateEntry.IsSupplementaryEntry())
			{
				mode = rateLine.ParentRateEntry.TI_RateCategory;
			}
			else if (rateLine.ParentRateEntry.IsAirFreight())
			{
				mode = MarkUpPercentage.AIO;
			}
			else if (rateLine.ParentRateEntry.IsLCLFreight())
			{
				mode = MarkUpPercentage.LCO;
			}
			else if (rateLine.ParentRateEntry.TI_RateCategory == RatingConstants.RateCategory.FCL)
			{
				mode = MarkUpPercentage.FCO;
			}
			else
			{
				mode = ZString.Empty;
			}

			return collection.FindElement(mode, locationForMarkup, isImport);
		}

		#endregion

		#region MayGSTBeApplicable

		public static bool MayGSTBeApplicable(this IRateLine rateLine, ZString incoTerm)
		{
			var entry = rateLine.ParentRateEntry;
			if (entry != null && rateLine.ChargeCode != null)
			{
				var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
				{
					IncoTerm = incoTerm,
					CostOrSell = entry.IsCosting() ? CostSell.Cost : CostSell.Revenue,
					JobType = ConsumerType(rateLine.ChargeCode).Code,
					Direction = entry.JobDirection,
					TransportMode = RatingConstants.GetTransportModeFromMode(entry.TI_Mode),
					Organisation = entry.ParentRatingHeader?.Header,
					Origin = entry.Origin(),
					Destination = entry.Destination(),
					Branch = GlbBranch.CurrentBranch,
				};

				var gstRate = rateLine.ChargeCode.GetGSTRate(parameters, out _);

				return gstRate != null && gstRate.GetRate(ZDate.Today) > 0;
			}

			return false;
		}

		static JobInvoicingConsumerType ConsumerType(AccChargeCode chargeCode)
		{
			switch (chargeCode.AC_ChargeGroup)
			{
				case ChargeCodeGroupList.Codes.Brokerage:
				case ChargeCodeGroupList.Codes.BrokerageOnly:
				case ChargeCodeGroupList.Codes.CustomsDuty:
					return JobInvoicingConsumerTypes.Brokerage;

				case ChargeCodeGroupList.Codes.CFSLoadList:
					return JobInvoicingConsumerTypes.CFSLoadList;

				case ChargeCodeGroupList.Codes.CFSShipment:
					return JobInvoicingConsumerTypes.CFSShipment;

				case ChargeCodeGroupList.Codes.WHSInwards:
					return JobInvoicingConsumerTypes.WarehouseInwards;

				case ChargeCodeGroupList.Codes.WHSOutwards:
					return JobInvoicingConsumerTypes.WarehouseOutwards;

				case ChargeCodeGroupList.Codes.TRWReceive:
					return JobInvoicingConsumerTypes.TransitReceive;

				case ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit:
					return JobInvoicingConsumerTypes.TransitReceiveTransportationUnit;

				case ChargeCodeGroupList.Codes.TRWDispatch:
					return JobInvoicingConsumerTypes.TransitDispatch;

				case ChargeCodeGroupList.Codes.TRWDispatchLoadList:
					return JobInvoicingConsumerTypes.TransitDispatchLoadList;

				case ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit:
					return JobInvoicingConsumerTypes.TransitDispatchTransportationUnit;

				case ChargeCodeGroupList.Codes.WHSStorage:
					return JobInvoicingConsumerTypes.WarehouseStorage;

				default:
					return JobInvoicingConsumerTypes.Shipment;
			}
		}

		#endregion

		public static int DecimalPlaces(this IRateLine rateLine)
		{
			return rateLine.ParentRateEntry?.DecimalPlaces() ?? 4;
		}

		public static RefCountry Country(this IRateLine rateLine)
		{
			return rateLine?.ParentRateEntry?.Country() ?? GlbCompany.CurrentCompany.Country;
		}

		public static RateLinesLookups Lookups(this IRateLine line)
		{
			if (line is RateLine rateLineBizO)
			{
				return rateLineBizO.Lookups;
			}

			return new RateLinesLookups(line, line.Factory);
		}

		public static bool IsBulkRateUpdateActionLine(this IRateLine line) => (line as RateLine)?.IsBulkRateUpdateActionLine ?? false;
		public static ZDecimal CompanyTariffDiscount(this IRateLine line) => (line as RateLine)?.CompanyTariffDiscount ?? ZDecimal.Zero;

		public static bool HasValueForDocumentPrinting(this RateLine line, bool overrideViewAgentRates = false)
		{
			var calculator = line.Calculator;
			var viewAgentRates = overrideViewAgentRates || line.ViewAgentRates;

			var result = line.RateLineItems.Cast<RateLineItem>().Any(HasValueVisibleOnDocument);
			bool HasValueVisibleOnDocument(RateLineItem item)
			{
				var isItemVisibleOnDocument = calculator.ShouldValueBeDiscounted(item);
				if (isItemVisibleOnDocument)
				{
					if (!item.TM_FlatAmount.IsEmpty)
					{
						return true;
					}

					return viewAgentRates ? !item.TM_AgentDeclaredRate.IsEmpty : !item.TM_Value.IsEmpty;
				}

				return false;
			}

			return result;
		}

		public static ZString GetUniversalChargeCodes(this IRateLine rateLine)
		{
			return rateLine.ChargeCode?.UniversalChargeCodeMappings ?? ZString.Empty;
		}

		public static bool IsExpired(this IRateLine line)
		{
			return line.TL_RateEndDate < ZDateTime.Today;
		}

		public static bool IsJobDateOutOfRange(this IRateLine line, ZDate jobDate)
		{
			var startDate = line.TL_RateStartDate;
			if (startDate.IsEmpty)
			{
				startDate = line.ParentRateEntry.TI_RateStartDate;
			}
			var endDate = line.TL_RateEndDate;
			if (endDate.IsEmpty)
			{
				endDate = line.ParentRateEntry.TI_RateEndDate;
			}
			return jobDate < startDate || !endDate.IsEmpty && jobDate > endDate;
		}
	}
}
