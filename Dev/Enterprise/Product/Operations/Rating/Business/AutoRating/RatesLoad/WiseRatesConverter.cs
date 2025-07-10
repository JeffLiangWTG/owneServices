using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools;
using static System.FormattableString;
using Constants = Enterprise.Core.Constants;
using DTO = WiseRates.Api.Model;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.Business
{
	#region SuppressResourceStringsCheckRegion

	public class WiseRatesConverter : IWiseRatesConverter
	{
		public WiseRatesConverter(BusinessObjectFactory factory, ILogger logger)
		{
			Factory = factory;
			Logger = logger;
		}

		public IList<WiseEntry> Convert(WiseRatesConversionContext context, IEnumerable<Rate> apiRates)
		{
			Argument.NotNull(context, nameof(context));

			if (apiRates == null || !apiRates.Any())
			{
				return Array.Empty<WiseEntry>();
			}

			var wiseHeaders = GetHeaders(apiRates, context);
			var rates = wiseHeaders.SelectMany(h => h.ChildRateEntries).Cast<WiseEntry>().ToList();

			return rates;
		}

		public IList<WiseEntry> Convert(RatesSearchResponse response, RatingCriteria criteria, ConversionOptions conversionOptions = default, string searchTraceID = default)
			=> Convert(new WiseRatesConversionContext(response, criteria, conversionOptions, searchTraceID), response.Rates);

		#region Header

		IEnumerable<WiseHeader> GetHeaders(IEnumerable<Rate> rates, WiseRatesConversionContext context)
		{
			var transportModeGroups = rates.GroupBy(r => r.TransportMode).ToArray();
			var headers = transportModeGroups.SelectMany(transportModeGroup => transportModeGroup
				.GroupBy(rate => rate.Carrier)
				.Select(ratesByTransportModeAndCarrier => GetHeader(transportModeGroup.Key, ratesByTransportModeAndCarrier, context)));

			return headers;
		}

		WiseHeader GetHeader(string transportMode, IEnumerable<Rate> rates, WiseRatesConversionContext context)
		{
			var header = new WiseHeader(Factory);

			var (convertedCarrierResult, error) = ConvertCarrier(transportMode, rates.First().Carrier, context);
			TryAddError(header.Errors, RatingHeaderSchema.TH_OH, error);

			header.TH_OH = convertedCarrierResult.PK;
			header.WiseCarrier = convertedCarrierResult.WiseCarrier;
			header.Header?.MiscServ?.CarrierServiceLevels?.Reload(true);

			var entries = GetEntries(rates, context, header.Header);
			if (rates.Count() != entries.Count())
			{
				Logger.Information(ZString.Format("Created {0} entries from {1} Rates Service rates for carrier '{2}'", entries.Count(), rates.Count(), header.Header?.OH_Code));
			}

			header.ChildRateEntries = entries;
			return header;
		}

		#endregion

		#region Entries

		IEnumerable<WiseEntry> GetEntries(IEnumerable<Rate> wiseRates, WiseRatesConversionContext context, OrgHeader carrier)
		{
			var entries = new List<WiseEntry>();

			foreach (var rate in wiseRates)
			{
				var ratesByChargeCodeGroup = SplitByChargeGroup(rate, context, Factory, carrier);

				var convertedRates = ratesByChargeCodeGroup.SelectMany(r => GetEntries(r, context, carrier)).ToArray();

				var lines = convertedRates.SelectMany(r => r.ChildRateLines).ToArray();
				foreach (var line in lines)
				{
					line.IncludedLines.UniqueAddRange(lines.GetIncludedLines(line));
				}

				entries.AddRange(convertedRates);
			}

			return entries;
		}

		IEnumerable<WiseEntry> GetEntries(Rate wiseRate, WiseRatesConversionContext context, OrgHeader carrier)
		{
			var result = new List<WiseEntry>();

			var (containers, errorContainer) = ConvertContainer(wiseRate, context);
			var containerPKs = containers.Select(x => x.PK).Where(x => !x.IsEmpty);

			if (containerPKs.IsNullOrEmpty())
			{
				containerPKs = new[] { ZGuid.Empty };
			}

			result.AddRange(containerPKs
				.Select(containerPK => CreateNewEntry(wiseRate, context, carrier, containerPK, errorContainer)));
			return result;
		}

		WiseEntry CreateNewEntry(Rate wiseRate, WiseRatesConversionContext context, OrgHeader carrier, ZGuid containerPK, string errorContainer)
		{
			ZString serviceLevel, rateMode, rateCategory, aircraftType;
			ILocation origin, destination;
			string error, errorRateMode;

			var entry = new WiseEntry(wiseRate, Factory);

			TryAddError(entry.Errors, RateEntrySchema.TI_RC, errorContainer);

			(origin, error) = ConvertLocation(wiseRate.Origin, nameof(wiseRate.Origin));
			TryAddError(entry.Errors, RateEntrySchema.TI_OriginLRC, error);

			(destination, error) = ConvertLocation(wiseRate.Destination, nameof(wiseRate.Destination));
			TryAddError(entry.Errors, RateEntrySchema.TI_DestinationLRC, error);

			(serviceLevel, error) = ConvertServiceLevel(wiseRate.ServiceLevel, wiseRate.ProviderCustomFields, carrier);
			TryAddError(entry.Errors, RateEntrySchema.TI_PL_NKCarrierServiceLevel, error);

			var lines = GetLines(context, wiseRate, entry, carrier);

			(rateCategory, error) = ConvertRateCategory(lines, Logger, wiseRate.ContainerMode, wiseRate.TransportMode);
			(rateMode, errorRateMode) = ConvertRateMode(wiseRate.ContainerMode, wiseRate.TransportMode, rateCategory, Logger);

			if (context.ConversionOptions.AddRateModeAndCategoryValidation)
			{
				TryAddError(entry.Errors, RateEntrySchema.TI_RateCategory, error);
				TryAddError(entry.Errors, RateEntrySchema.TI_Mode, errorRateMode);
			}

			aircraftType = ConvertAircraftType(wiseRate.ProviderCustomFields);

			entry.TI_Mode = rateMode;
			entry.TI_RateCategory = rateCategory;
			entry.TI_OriginLRC = origin?.Code ?? "";
			entry.TI_ViaLRC = wiseRate.Via;
			entry.TI_DestinationLRC = destination?.Code ?? "";
			entry.TI_RateStartDate = wiseRate.StartDate();
			entry.TI_RateEndDate = wiseRate.ExpiryDate();
			entry.TI_RC = containerPK;
			entry.TI_MatchContainerRateClass = false;
			entry.TI_PL_NKCarrierServiceLevel = serviceLevel;
			entry.TI_ContractNumber = wiseRate.ContractNumber;
			entry.TI_PaymentTerm = wiseRate.PaymentTerm;
			entry.TI_AircraftType = aircraftType;
			entry.ReservedForJobIDs = wiseRate.ReservedForJobIDs;
			entry.NamedAccounts = wiseRate.NamedAccounts;
			entry.RateProvider = wiseRate.Provider;
			entry.ChildRateLines = lines;
			entry.ContainerPayloadWeightOverride = wiseRate.Container?.PayloadWeight ?? 0m;
			entry.ContainerPayloadVolumeOverride = wiseRate.Container?.PayloadVolume ?? 0m;
			entry.CommodityGroup = wiseRate.Commodity;
			if (wiseRate.CarrierCommodityInfo != null)
			{
				entry.ProductName = wiseRate.CarrierCommodityInfo.GroupName ?? entry.ProductName;
				entry.Commodities = wiseRate.CarrierCommodityInfo.IncludedCommodities?.Select(str => new ZString(str)).ToArray() ?? entry.Commodities;
			}
			entry.CustomFields = wiseRate.ProviderCustomFields;

			return entry;
		}

		static IEnumerable<Rate> SplitByChargeGroup(Rate wiseRate, WiseRatesConversionContext context, BusinessObjectFactory factory, OrgHeader carrier)
		{
			if (!wiseRate.Charges.Any())
			{
				return new[] { wiseRate };
			}

			var groupsByChargeGroup = wiseRate.Charges
				.GroupBy(x =>
				{
					var (accChargeCode, _) = ConvertChargeCode(x.ChargeCode, factory, x.CarrierChargeCodeInfo?.Code, carrier, wiseRate?.TransportMode);
					return accChargeCode?.AC_ChargeGroup;
				})
				.ToArray();

			if (groupsByChargeGroup.Any(g => !g.Key.HasValue || g.Key.Value.IsEmpty))
			{
				return new[] { wiseRate };
			}

			var rates = groupsByChargeGroup.Select(g =>
			{
				var rateWithChargesFromSameGroup = wiseRate.DeepClone();
				rateWithChargesFromSameGroup.Charges = g.ToArray();

				return rateWithChargesFromSameGroup;
			});

			return rates;
		}

		#endregion

		#region Lines

		IEnumerable<WiseLine> GetLines(WiseRatesConversionContext context, Rate rate, WiseEntry entry, OrgHeader carrier)
		{
			// When converting CS LCL rate, we don't compare units and conversion factors
			// so that the 2 similar charges having the same RateLineID can be grouped together loosely to form an HRC calculator.
			// With the rest of the cases, the above elements should be used to differentiate charges.
			var compareUnitAndConversionFactor = rate.Provider != WRConstants.RateProviders.CargoSphere || rate.ContainerMode != Core.Constants.RateMode.LCL;
			var lines = rate.Charges
				.GroupBy(c => new WiseLineKey(c, compareUnitAndConversionFactor))
				.Select(group => GetLine(context, group, rate, entry, carrier));
			return lines.ToList();
		}

		WiseLine GetLine(WiseRatesConversionContext context, IEnumerable<Charge> charges, Rate rate, WiseEntry entry, OrgHeader carrier)
		{
			AccChargeCode chargeCode;
			ZString currencyCode, calculatorCode;
			IRateLineItem[] lineItems;
			string error;

			// ULD rates have two charges :
			// One charge is per container for pivot costs
			// The other charge is per weight/volume unit for over pivot costs
			// CMB calculator will need weight/volume unit to calculate the ULD costs and "pivot costs" will be the "flat amount" of "over pivot costs".
			// So here we will use the charge related to over pivot costs to get required information like unit and currency....
			var charge = charges.FirstOrDefault(c => !string.IsNullOrEmpty(c.EquipmentUnit) && c.Break.HasValue)
				?? charges.First();

			var line = new WiseLine(Factory, charge);
			line.ParentRateEntry = entry;

			(chargeCode, error) = ConvertChargeCode(charge.ChargeCode, Factory, charge.CarrierChargeCodeInfo?.Code, carrier, rate?.TransportMode);
			TryAddError(line.Errors, RateLinesSchema.TL_AC, error);

			(currencyCode, error) = ConvertCurrency(context, charge);
			TryAddError(line.Errors, RateLinesSchema.TL_RX_NKCurrency, error);
			(calculatorCode, lineItems, error) = ConvertCalculator(charges, line, rate.ContainerMode, carrier, rate?.TransportMode);
			TryAddError(line.Errors, RateLinesSchema.TL_RateCalculator, error);

			var origin = LocationHelper.GetLocationFromString(rate.Origin, Factory);
			var destination = LocationHelper.GetLocationFromString(rate.Destination, Factory);
			var direction = ImportExportHelper.GetJobDirection(origin?.Country?.Code ?? "", destination?.Country?.Code ?? "");
			var conversionFactor = ConvertConversionFactor(charge, direction, rate.TransportMode);

			line.TL_AC = chargeCode?.PK ?? ZGuid.Empty;
			line.TL_RX_NKCurrency = currencyCode;
			line.TL_RateCalculator = calculatorCode;
			line.Comment = charge.Comment;
			line.TL_WeightVolume = ConvertUnit(charge.Unit, calculatorCode, entry.TI_RateCategory);
			line.CarrierChargeCode = charge.CarrierChargeCodeInfo?.Code ?? string.Empty;
			line.CarrierChargeCodeDescription = charge.CarrierChargeCodeInfo?.Description ?? string.Empty;
			line.ChildRateLineItems = lineItems;
			line.ConversionFactor = conversionFactor;
			// Since HRC calculator has unit multipliers on individual charges, it makes no sense to use 1 unit multiplier from 1 charge (rate line item) to apply to the line.
			// Better leave it empty (zero) so the calculation on an item without a multiplier can refer to it as ZDecimal.Zero or converted to ZDecimal.One in divisions.
			if (calculatorCode != HighestRateCalculator.Code)
			{
				line.TL_WeightVolumeMultiple = charge.UnitMultiplier;
			}
			line.TL_ActualPercentage = charge.ActualPercentage ?? 0;
			line.CustomFields = charge.ProviderCustomFields ?? Enumerable.Empty<CustomField>();

			var precision = line.CustomFields.FirstOrDefault(c => c.Code == Rate.CustomFields.Common.Precision);
			line.TL_Rounding = precision != null ? (ZString)RatingRoundingTypes.Custom : ZString.Empty;
			line.TL_RoundingFactor = precision != null ? new ZDecimal(precision.Value) : ZDecimal.Zero;

			return line;
		}

		#endregion

		#region Container

		/// <summary>
		/// This function generally returns only 1 container. But it can return
		/// more than one in very special cases. When it returns more than one
		/// you are guaranteed that the container being returned is in the criteria.
		/// Otherwise if it returns just one, it may not be in the criteria
		/// </summary>
		public (IEnumerable<MasterFiles.Business.RefContainer> containerPK, string error) ConvertContainer(Rate wiseRate, WiseRatesConversionContext context)
		{
			if (wiseRate.Container == null)
			{
				return (Enumerable.Empty<MasterFiles.Business.RefContainer>(), null);
			}

			var cw1ContainersFromRateService = FindContainers(wiseRate);
			var finalContainers = FilterByCriteriaContainers(context.Criteria?.GetContainers(), cw1ContainersFromRateService);

			return finalContainers.Any()
				? (finalContainers, null)
				: (Enumerable.Empty<MasterFiles.Business.RefContainer>(), Res.GetString("dc4dd4f5-c1ca-4c80-bf5e-a9c1000eff41", "No Container Type is assigned with the ISO Type or Container Code: {0}", wiseRate.Container.Code));
		}

		/// <summary>
		/// It is necessary to ensure the converted container from RS is part of
		/// the criteria. Since there could be more than one container in CW1
		/// that matches the code returned from RS we need to make sure what we
		/// return was in the job itself.
		/// </summary>
		/// <returns></returns>
		IEnumerable<MasterFiles.Business.RefContainer> FilterByCriteriaContainers(
			IEnumerable<MasterFiles.Business.RefContainer> criteriaContainers,
			MasterFiles.Business.RefContainer[] cw1ContainersFromRateService)
		{
			return criteriaContainers.IsNullOrEmpty()
				? cw1ContainersFromRateService
				: IntersectByPK(cw1ContainersFromRateService, criteriaContainers);
		}

		/// <summary>
		/// Returns zero or more containers that match the wiseRate.
		/// Might match depending on RC_ISOType, RC_Code, RC_FreightRateClass or RC_HandlingRateClass
		/// according to various conditions.
		///
		/// The containers it returns may not actually be in the job!
		/// </summary>
		MasterFiles.Business.RefContainer[] FindContainers(Rate wiseRate)
		{
			if (wiseRate.TransportMode == WRConstants.TransportModes.SEA)
			{
				var queryForISOType =
					new ZQuery(
						RefContainerSchema.RC_ISOType,
						wiseRate.Container.Code
					);

				var queryForFreightClass =
					!wiseRate.Container.ISOTypeGroup.IsNullOrEmpty()
					? new ZQuery(
							RefContainerSchema.RC_FreightRateClass,
							wiseRate.Container.ISOTypeGroup
					)
					: ZQuery.NoResultQuery;

				var queryForHandlingClass =
					!wiseRate.Container.ISOTypeGroup.IsNullOrEmpty()
					? new ZQuery(
						RefContainerSchema.RC_HandlingRateClass,
						wiseRate.Container.ISOTypeGroup
					)
					: ZQuery.NoResultQuery;

				var query =
					new ZQuery(queryForISOType, JoinCondition.Or, new ZQuery(queryForHandlingClass, JoinCondition.Or, queryForFreightClass));

				return Factory.Load<MasterFiles.Business.RefContainer>(query);
			}
			else if (wiseRate.TransportMode == WRConstants.TransportModes.AIR)
			{
				return Factory.Load<MasterFiles.Business.RefContainer>(
					new ZQuery(
						RefContainerSchema.RC_Code,
						wiseRate.Container.Code
					)
				);
			}
			return Array.Empty<MasterFiles.Business.RefContainer>();
		}

		#endregion

		#region Commodity

		#endregion

		#region Carrier Service Level

		public (ZString code, string error) ConvertServiceLevel(string wiseCode, IEnumerable<CustomField> providerCustomFields, OrgHeader carrier)
		{
			if (carrier == null)
			{
				return (ZString.Empty, null);
			}

			// Try matching by product first, since it takes preference over a match by universal service level...
			var productCode = providerCustomFields?.Where(x => x.Code == Rate.CustomFields.Cargoguide.ProductCode).Select(x => x.Value?.ToString() ?? string.Empty).FirstOrDefault();
			if (!string.IsNullOrEmpty(productCode))
			{
				var matchByProduct = carrier.MiscServ.CarrierServiceLevels
					.Cast<OrgCarrierServiceLevel>()
					.FirstOrDefault(x => x.ProductCodes.Any(c => c.EqualsIgnoringCase(productCode)));

				if (matchByProduct != null)
				{
					return (matchByProduct.PL_Code, null);
				}
			}

			if (string.IsNullOrEmpty(wiseCode))
			{
				return (ZString.Empty, null);
			}

			var carrierServiceLevels = carrier.MiscServ.CarrierServiceLevels
				.Cast<OrgCarrierServiceLevel>()
				.Where(l => l.CarrierServiceCodes.Any(c => c.EqualsIgnoringCase(wiseCode)))
				.ToList();

			if (carrierServiceLevels.Count > 1)
			{
				return (ZString.Empty, Res.GetString("033c3091-829d-4305-8895-c89ef6809dea", "Service Code '{0}' under Carrier '{1}' has been duplicated and must be unique.", wiseCode, carrier.OH_Code));
			}

			if (carrierServiceLevels.Count == 1)
			{
				return (carrierServiceLevels[0].PL_Code, null);
			}

			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().FirstOrDefault(l => l.PL_Code == wiseCode);
			return carrierServiceLevel != null
				? (wiseCode, null)
				: (ZString.Empty, Res.GetString("653444ea-17a0-47a4-b47d-2adab9b3feaf", "No Carrier Service Level under Carrier '{0}' is assigned to '{1}'", carrier.OH_Code, wiseCode));
		}

		#endregion

		#region Rate Category

		public static (ZString code, string error) ConvertRateCategory(IEnumerable<IRateLine> lines, ILogger logger, string wrContainerMode, string wrTransportMode)
		{
			if (!lines.Any())
			{
				var error = Res.GetString("71B8C645-C9BA-426A-AEA4-51C4281B7FEB", "Category cannot be identified as no Charge is found under the Rate.");
				return (ZString.Empty, error);
			}

			if (lines.Any(l => l.ChargeCode == null))
			{
				var error = Res.GetString("52ee6b04-40b9-4f72-b3d3-d513becb1091", "Category cannot be identified as the Charge(s) under the Rate cannot be converted into CW1 Charges. Please check validation errors of the Rate line.");
				return (ZString.Empty, error);
			}

			if (!lines.AllSame(l => l.ChargeCode.AC_ChargeGroup))
			{
				var error = Res.GetString("3caa0f56-ac46-4aea-8859-2acd47808da7", "Category cannot be identified as the Charge(s) under the Rate are placed under different Charge Code Groups.");
				return (ZString.Empty, error);
			}

			var chargeCode = lines.First().ChargeCode;
			if (string.IsNullOrEmpty(chargeCode.AC_ChargeGroup))
			{
				var error = Res.GetString("BB442704-800D-444A-96F6-E4C594C01F88", "Category cannot be identified as the Charge(s) under the Rate has no Charge Code Group selected.");
				return (ZString.Empty, error);
			}

			var isContainerized = RatingConstants.GetContainerModeFromMode(wrContainerMode) == Enterprise.Core.Constants.ContainerModes.FCL;
			var category = RatingConstants.InferForwardingRateCategory(chargeCode.AC_ChargeGroup, wrTransportMode, isContainerized);

			if (string.IsNullOrEmpty(category))
			{
				logger?.Warning(ZString.Format(
					"Unable to infer rate category using the following values: ChargeCodeGroup {0}, WRTransportMode {1}, WRContainerMode {2}",      // Log message
					chargeCode.AC_ChargeGroup, wrTransportMode, wrContainerMode));

				var error = Res.GetString("c1f52e4a-6da6-448f-85ab-82cb5352e184", "Category cannot be identified for the Rate with error report sent to WTG. Please refer to the Autorating Log for details.");
				return (ZString.Empty, error);
			}

			return (category, null);
		}

		#endregion

		#region Rate Mode

		static readonly Dictionary<string, string> rateModeMappings = new Dictionary<string, string>
		{
			{ "AIR FCL", "ULD" },								// Constants
			{ "AIR LCL", "LSE" },								// Constants
			{ "AIR "   , "AIR" },								// Constants
//			{ "SEA FCL", "Special case, handled separately" },
			{ "SEA LCL", "LCL" },								// Constants
			{ "SEA "   , "SEA" },								// Constants
			{ "RAI FCL", "RAI" },								// Constants
			{ "ROA FCL", "ROA" },								// Constants
		};

		public static (ZString code, string error) ConvertRateMode(string wrContainerMode, string wrTransportMode, string cw1Category, ILogger logger)
		{
			wrTransportMode = wrTransportMode.ToUpper(CultureInfo.InvariantCulture);
			wrContainerMode = wrContainerMode.ToUpper(CultureInfo.InvariantCulture);

			if (rateModeMappings.TryGetValue(Invariant($"{wrTransportMode} {wrContainerMode}"), out var mode))     // Just a key
			{
				return (mode, null);
			}

			if (wrTransportMode == "SEA" && wrContainerMode == "FCL")
			{
				if (cw1Category.IsNullOrEmpty())
				{
					var error = Res.GetString("9f70e80b-d18d-42f7-afe7-084ef06fe630", "Transport Mode cannot be identified as Category on the Rate is not identified. Please check validation errors of the Category.");
					return (ZString.Empty, error);
				}

				return (cw1Category == "FCL" ? "SEA" : "FCL", null);
			}

			logger?.Warning(ZString.Format("Unable to infer rate mode for '{0}' transport mode and '{1}' container mode", wrTransportMode, wrContainerMode));  // Log message
			return (ZString.Empty, Res.GetString("b2b38c07-d01d-4f75-a593-304ca1ec5825", "Transport Mode cannot be identified for the Rate with error report sent to WTG. Please refer to the Autorating Log for details."));
		}

		#endregion

		#region Carrier

		public (ConvertedCarrierResult, string error) ConvertCarrier(string transportMode, string wiseCarrierCode, WiseRatesConversionContext context)
		{
			var wiseCarrier = context.Response?.Carriers?.FirstOrDefault(c => c.Code == wiseCarrierCode);

			if (context.Carrier != null)
			{
				return (new ConvertedCarrierResult(context.Carrier.PK, wiseCarrier), null);
			}

			var isSCACOrC1COrIATAMultimapped = false;
			var message = new StringBuilder();

			if (string.IsNullOrWhiteSpace(wiseCarrierCode))
			{
				return (new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier), null);
			}

			if (wiseCarrier == null)
			{
				ErrorReporter.ReportOnce("MissingCarrierInWiseRatesSearchResponse", $"[TraceID: {context.SearchTraceID}] Carrier with code '{wiseCarrierCode}' not found among carriers in the response ({string.Join(", ", context.Response.Carriers.Select(c => c.Code))})");
				return (new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier), Res.GetString("63b3aa1f-5405-4f18-851b-96c695f77e77", "Carrier cannot be identified for the Rate with error report sent to WTG. Please refer to the Autorating Log for details."));
			}

			if (transportMode == WRConstants.TransportModes.AIR && !string.IsNullOrEmpty(wiseCarrier.IATACode))
			{
				var result = CheckAndGetCarrierByCode(GetCarrierByIATACode);
				if (result != null)
				{
					return (result, null);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(wiseCarrier.SCACCode))
				{
					var result = CheckAndGetCarrierByCode(GetCarrierBySCACCode);
					if (result != null)
					{
						return (result, null);
					}
				}

				if (!string.IsNullOrEmpty(wiseCarrier.C1Code))
				{
					var result = CheckAndGetCarrierByCode(GetCarrierByC1Code);
					if (result != null)
					{
						return (result, null);
					}
				}
			}

			return
			(
				new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, isSCACOrC1COrIATAMultimapped, message.ToString().Trim(',', ' ')),
				Res.GetString("85544d07-1b60-40cb-a696-aee83bf724b6", "No single Carrier is assigned with the SCAC, IATA or C1C Code of the Carrier from Rates Service:\r\n{0}", wiseCarrier.ToJSON()));

			ConvertedCarrierResult CheckAndGetCarrierByCode(Func<RefCarrier, WiseRatesConversionContext, ConvertedCarrierResult> getCarrierByCode)
			{
				var convertedCarrierResult = getCarrierByCode(wiseCarrier, context);
				if (!convertedCarrierResult.PK.IsEmpty)
				{
					return convertedCarrierResult;
				}

				isSCACOrC1COrIATAMultimapped |= convertedCarrierResult.IsMultiMapped;
				message.Append(convertedCarrierResult.Message + ", ");

				return null;
			}
		}

		ConvertedCarrierResult GetCarrierBySCACCode(RefCarrier wiseCarrier, WiseRatesConversionContext context)
		{
			var key = GetSCACCacheKey(wiseCarrier.SCACCode, context.ConversionSessionID);
			var convertedCarrierResult = Factory.GetCachedValue(key, () =>
			{
				var subQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK, OrgHeaderSchema.OH_RSL_ShippingLine);
				subQuery.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, wiseCarrier.SCACCode);

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				query.AddSubQuery(subQuery, JoinCondition.And);

				var cw1Carriers = Factory.Load<OrgHeader>(query);

				if (cw1Carriers.Length == 0)
				{
					var message = ZString.Format("No active Carrier is assigned with SCAC Code '{0}'", wiseCarrier.SCACCode);
					Logger.Warning(message);
					return new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, false, message);
				}

				if (cw1Carriers.Length > 1)
				{
					var criteriaCarriers = context.GetCriteriaCarriers();

					var intersect = IntersectByPK(cw1Carriers, criteriaCarriers);
					if (intersect.Count() == 1)
					{
						return new ConvertedCarrierResult(intersect.First().PK, wiseCarrier);
					}

					var message = ZString.Format("More than one of the active Carriers ({0}) are assigned with '{1}'", string.Join(", ", cw1Carriers.Select(c => c.OH_Code).OrderBy(x => x)), wiseCarrier.SCACCode);
					Logger.Warning(message);

					return new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, true, message);
				}

				return new ConvertedCarrierResult(cw1Carriers[0].PK, wiseCarrier);
			});

			return convertedCarrierResult;
		}

		public class ConvertedCarrierResult
		{
			public ConvertedCarrierResult(ZGuid pk, RefCarrier wiseCarrier, bool isMultiMapped = false, string message = default(string))
			{
				PK = pk;
				WiseCarrier = wiseCarrier;
				IsMultiMapped = isMultiMapped;
				Message = message;
			}

			public ZGuid PK { get; }
			public bool IsMultiMapped { get; }
			public string Message { get; }
			public RefCarrier WiseCarrier { get; }
		}

		public static string GetSCACCacheKey(string scacCode, string sessionID) => Invariant($"SCAC_{scacCode}_{sessionID}");

		ConvertedCarrierResult GetCarrierByC1Code(RefCarrier wiseCarrier, WiseRatesConversionContext context)
		{
			var key = GetC1CCacheKey(wiseCarrier.C1Code, context.ConversionSessionID);
			var convertedCarrierResult = Factory.GetCachedValue(key, () =>
			{
				var subQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK, OrgHeaderSchema.OH_RSL_ShippingLine);
				subQuery.AddToFilter(RefShippingLineSchema.RSL_CargoWiseOneCode, wiseCarrier.C1Code);

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				query.AddSubQuery(subQuery, JoinCondition.And);

				var cw1Carriers = Factory.Load<OrgHeader>(query);

				if (cw1Carriers.Length == 0)
				{
					var message = ZString.Format("No active Carrier is assigned with C1 Code '{0}'", wiseCarrier.C1Code);
					Logger.Warning(message);
					return new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, false, message);
				}

				if (cw1Carriers.Length > 1)
				{
					var criteriaCarriers = context.GetCriteriaCarriers();

					var intersect = IntersectByPK(cw1Carriers, criteriaCarriers);
					if (intersect.Count() == 1)
					{
						return new ConvertedCarrierResult(intersect.First().PK, wiseCarrier);
					}

					var message = ZString.Format("More than one of the active Carriers ({0}) are assigned with C1 Code '{1}'", string.Join(", ", cw1Carriers.Select(c => c.OH_Code).OrderBy(x => x)), wiseCarrier.C1Code);
					Logger.Warning(message);

					return new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, true, message);
				}

				return new ConvertedCarrierResult(cw1Carriers[0].PK, wiseCarrier);
			});

			return convertedCarrierResult;
		}

		public static string GetC1CCacheKey(string c1Code, string sessionID) => Invariant($"C1_{c1Code}_{sessionID}");

		ConvertedCarrierResult GetCarrierByIATACode(RefCarrier wiseCarrier, WiseRatesConversionContext context)
		{
			var iataCode = wiseCarrier.IATACode;
			var key = "IATA" + "_" + iataCode + "_" + context.ConversionSessionID;
			var convertedCarrierResult = Factory.GetCachedValue(key, () =>
			{
				var airLineSubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK, OrgMiscServSchema.OM_RM_Airline);
				airLineSubQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty);

				var codeLength = iataCode.Length;
				if (codeLength == 2)
				{
					airLineSubQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, iataCode);
				}
				else if (codeLength == 3)
				{
					airLineSubQuery.AddToFilter(RefAirlineSchema.RM_ThreeLetterCode, iataCode);
				}
				else
				{
					var message = ZString.Format("IATA Code '{0}' with length {1} has not been supported", iataCode, codeLength);
					Logger.Warning(message);
					return new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, false, message);
				}

				var orgMiscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, OrgHeaderSchema.PK);
				orgMiscServSubQuery.AddSubQuery(airLineSubQuery, JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				query.AddSubQuery(orgMiscServSubQuery, JoinCondition.And);

				var cw1Carriers = Factory.Load<OrgHeader>(query);

				if (cw1Carriers.Length == 0)
				{
					var message = ZString.Format("No active Carrier is assigned with IATA Code '{0}'", iataCode);
					Logger.Warning(message);
					return new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, false, message);
				}

				if (cw1Carriers.Length > 1)
				{
					var criteriaCarriers = context.GetCriteriaCarriers();

					var intersect = IntersectByPK(cw1Carriers, criteriaCarriers);
					if (intersect.Count() == 1)
					{
						return new ConvertedCarrierResult(intersect.First().PK, wiseCarrier);
					}

					var message = ZString.Format("More than one of the active Carriers ({0}) are assigned with IATA Code '{1}'", string.Join(", ", cw1Carriers.Select(c => c.OH_Code).OrderBy(x => x)), iataCode);
					Logger.Warning(message);
					return new ConvertedCarrierResult(ZGuid.Empty, wiseCarrier, true, message);
				}

				return new ConvertedCarrierResult(cw1Carriers[0].PK, wiseCarrier);
			});

			return convertedCarrierResult;
		}

		static IEnumerable<T> IntersectByPK<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : BusinessObject
		{
			var collecion1PKs = collection1.Select(c => c.PK).ToArray();
			var collecion2PKs = collection2.Select(c => c.PK).ToArray();

			var intersectPKs = collecion1PKs.Intersect(collecion2PKs).ToArray();
			var intersect = collection1.Where(c => intersectPKs.Contains(c.PK)).ToArray();

			return intersect;
		}

		public (OrgHeader, ConvertedCarrierResult) GetCarrierOrgHeader(WiseRatesConversionContext context, Rate rate)
		{
			var (convertedCarrierResult, _) = ConvertCarrier(rate.TransportMode, rate.Carrier, context);
			var carrierPK = convertedCarrierResult.PK;
			return !carrierPK.IsEmpty
				? (Factory.Load<OrgHeader>(carrierPK), new ConvertedCarrierResult(carrierPK, convertedCarrierResult.WiseCarrier))
				: (null, convertedCarrierResult);
		}

		#endregion

		#region Aircraft Type

		public ZString ConvertAircraftType(IEnumerable<CustomField> providerCustomFields)
		{
			var isCargoOnly = providerCustomFields?
				.Where(x => x.Code == Rate.CustomFields.Cargoguide.CargoAircraftOnly)
				.Select(x => x.Value)
				.OfType<bool>()
				.FirstOrDefault() ?? false;

			return isCargoOnly ? (ZString)Constants.AircraftType.CAO : ZString.Empty;
		}

		#endregion

		#region Locations

		(ILocation location, string error) ConvertLocation(string locationCode, string source)
		{
			if (string.IsNullOrWhiteSpace(locationCode))
			{
				return (null, null);
			}

			var location = LocationHelper.GetLocationFromString(locationCode, Factory);
			if (location != null)
			{
				return (location, null);
			}

			return (null, Res.GetString("FDA0D4F9-DCE9-42B5-9952-9D2BDDAFCC05", "No Location matches {0} '{1}'", source, locationCode));
		}

		#endregion

		#region Charge Code

		public static (AccChargeCode chargeCode, string error) ConvertChargeCode(string wiseChargeCode, BusinessObjectFactory factory, string carrierChargeCode = "", OrgHeader carrier = null, string transportMode = "")
		{
			if (wiseChargeCode.IsNullOrEmpty())
			{
				return (null, Res.GetString("a7b74ea2-736d-408a-b3a8-0fea031c08b8", "The charge code has NO mapping with any Universal Charge Code."));
			}

			var localCarrierCode = GetChargeCodeByUniversalOrCarrierCode(carrierChargeCode, Env.CurrentCompanyPK, factory, AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier, carrier, transportMode);
			if (localCarrierCode != null)
			{
				return (localCarrierCode, null);
			}

			var localUniversalCode = GetChargeCodeByUniversalOrCarrierCode(wiseChargeCode, Env.CurrentCompanyPK, factory, AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Universal);
			if (localUniversalCode != null)
			{
				return (localUniversalCode, null);
			}

			var globalCarrierCode = GetChargeCodeByUniversalOrCarrierCode(carrierChargeCode, null, factory, AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier, carrier, transportMode);
			if (globalCarrierCode != null)
			{
				localCarrierCode = GetChargeCodeByCode(globalCarrierCode.AC_Code, Env.CurrentCompanyPK, factory);
				if (localCarrierCode != null)
				{
					return (localCarrierCode, null);
				}
			}

			var globalUniversalCode = GetChargeCodeByUniversalOrCarrierCode(wiseChargeCode, null, factory, AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Universal);
			if (globalUniversalCode != null)
			{
				localUniversalCode = GetChargeCodeByCode(globalUniversalCode.AC_Code, Env.CurrentCompanyPK, factory);
				if (localUniversalCode != null)
				{
					return (localUniversalCode, null);
				}
			}

			localUniversalCode = GetChargeCodeByCode(wiseChargeCode, Env.CurrentCompanyPK, factory);
			if (localUniversalCode != null)
			{
				return (localUniversalCode, null);
			}

			return (null, Res.GetString("5FE78800-42E8-4A24-80BE-397357A7E45C", "No Charge Code is assigned with or has the same Code as Universal '{0}'.", wiseChargeCode));
		}

		static AccChargeCode GetChargeCodeByUniversalOrCarrierCode(string chargeCode, Guid? companyPK, BusinessObjectFactory factory, string universalOrCarrierType, OrgHeader carrier = null, string transportMode = "")
		{
			if (string.IsNullOrEmpty(chargeCode))
			{
				return null;
			}

			// AccChargeCodeUniversalCodeMappings of type `Carrier` with empty `AUP_OH_Carrier` apply to all carriers.
			// First try to find a CodeMapping with the supplied carrier, otherwise find a CodeMapping that applies to all carriers.
			if (universalOrCarrierType == AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier && carrier != null)
			{
				var carrierSpecificChargeCode = factory.LoadTop1<AccChargeCode>(GenerateAccChargeCodeQuery(chargeCode, companyPK, universalOrCarrierType, carrier, transportMode));
				if (carrierSpecificChargeCode != null)
				{
					return carrierSpecificChargeCode;
				}
			}

			return factory.LoadTop1<AccChargeCode>(GenerateAccChargeCodeQuery(chargeCode, companyPK, universalOrCarrierType, null, transportMode));
		}

		static ZDBOnlyQuery GenerateAccChargeCodeQuery(string code, Guid? companyPK, string universalOrCarrierType, OrgHeader carrier, string transportMode)
		{
			var query = new ZDBOnlyQuery(typeof(AccChargeCode));
			query.AddToFilter(AccChargeCodeSchema.AC_GC, companyPK);
			query.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);

			var mappingSubQuery = new ZDBOnlySubQuery(typeof(AccChargeCodeUniversalCodeMapping), AccChargeCodeUniversalCodeMappingSchema.AUP_AC);
			mappingSubQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Type, universalOrCarrierType);
			mappingSubQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Code, code);
			mappingSubQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_TransportMode, transportMode);
			mappingSubQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_OH_Carrier, carrier?.PK);

			query.AddSubQuery(AccChargeCodeSchema.PK, mappingSubQuery, JoinCondition.And);
			return query;
		}

		static AccChargeCode GetChargeCodeByCode(string chargeCode, Guid? companyPK, BusinessObjectFactory factory)
		{
			if (string.IsNullOrWhiteSpace(chargeCode))
			{
				return null;
			}

			var query = new ZQuery(AccChargeCodeSchema.AC_GC, companyPK);
			query.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);

			return factory.LoadTop1<AccChargeCode>(query);
		}

		#endregion

		#region Currency

		(ZString code, string error) ConvertCurrency(WiseRatesConversionContext context, Charge charge)
		{
			if (charge.ChargeType == DTO.ChargeType.Included ||
				((charge.Restricted ?? false) && charge.Applicability != RateApplicableCode.GetDescription(RateApplicableCode.Tact)))
			{
				// The charge is included in another charge and thus has no its own amount/currency
				return (ZString.Empty, null);
			}

			if (string.IsNullOrEmpty(charge.Currency))
			{
				ErrorReporter.ReportOnce("MissingCurrencyOnWiseRate", $"[TraceID: {context.SearchTraceID}] Charge from the Rates Service has no Currency");
				return (ZString.Empty, Res.GetString("1545ee4e-d773-4adb-ab5e-363b0d5313cd", "Currency cannot be identified for the Rate with error report sent to WTG. Please refer to the Autorating Log for details."));
			}

			var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, charge.Currency);
			if (refCurrency != null)
			{
				return (charge.Currency, null);
			}

			return (ZString.Empty, Res.GetString("E0E77BBB-55B4-4C56-A139-B044016CE6A3", "No Currency is assigned with or has the same Code as '{0}'", charge.Currency));
		}

		#endregion

		#region Calculator

		(ZString code, IRateLineItem[] lineItems, string error) ConvertCalculator(IEnumerable<Charge> charges, WiseLine line, string containerMode, OrgHeader carrier, string transportMode)
		{
			var calcDecider = new CalculatorDecider(charges, line, Logger, containerMode, transportMode, ConvertChargeCodes(charges, carrier, transportMode));

			if (calcDecider.Code == NullCalculator.Code)
			{
				var error = Res.GetString("6b5cebb3-ce24-497f-8c43-423c24a1f163", "Unable to resolve calculator from charges");
				return (ZString.Empty, Array.Empty<WiseLineItem>(), error);
			}

			return (calcDecider.Code, calcDecider.RateLineItems.ToArray(), null);
		}

		Dictionary<string, ZGuid> ConvertChargeCodes(IEnumerable<Charge> charges, OrgHeader carrier, string transportMode)
		{
			var charge = charges.First();

			var listOfChargeCodesToConvert = new List<string>()
			{
				charge.FreightInclusiveCarriageCharge,
				charge.PercentageAppliesTo,
			};

			return listOfChargeCodesToConvert
					.Select(convertingCode => new { convertingCode, Result = ConvertChargeCode(convertingCode, Factory, charge.CarrierChargeCodeInfo?.Code, carrier, transportMode) })
					.Where(x => x.Result.chargeCode != null)
					.ToDictionary(x => x.convertingCode, x => x.Result.chargeCode.PK);
		}

		#endregion

		#region Conversion Factor

		ConversionFactor ConvertConversionFactor(Charge wiseCharge, Directions direction, string transportMode)
		{
			ConversionFactor factor;

			if (wiseCharge.ConversionFactor.HasValue)
			{
				factor = new ConversionFactor(
				   wiseCharge.ConversionFactor.Value,
				   wiseCharge.ConversionFactorUnit,
				   wiseCharge.ConversionFactorDenominatorUnit);
			}
			else
			{
				var isDomestic = direction == Directions.Domestic;
				var factors = ChargeableAmountCalculator.GetDefaultConversionFactors(isDomestic, transportMode, wiseCharge.Unit);
				factor = factors.FirstOrDefault();
			}

			return factor;
		}

		#endregion

		#region Unit

		string ConvertUnit(string unit, string calculator, string rateCategory)
		{
			if (!string.IsNullOrEmpty(unit))
			{
				return unit;
			}

			if (calculator == CombinedCalculator.Code)
			{
				// Rate Service flat charges get converted as CMB calculator with BAS line. But, CMB calculator requires unit on a line
				// for autorating engine which rates service doesn't have since the charge is flat. So, we need to default unit on the line
				// to avoid side-effects with autorating.
				//
				// This solution is a hack though. If there is another Per Unit charge with the same charge code, this charge will conflict with it.
				// To fix it properly we need to improve the CMB calculator so that it doesn't require unit if there is no per unit rate. But it breaks lots
				// of tests. A separate WI must be created to address it.
				return !string.IsNullOrEmpty(rateCategory)
					? (string)RateEntry.GetDefaultUnit(rateCategory)
					: Env.Registry.FreightWeightUnit;
			}

			return string.Empty;
		}

		#endregion

		static void TryAddError(IDictionary<SchemaColumn, string> errors, SchemaColumn column, string error)
		{
			if (!string.IsNullOrEmpty(error))
			{
				errors[column] = error;
			}
		}

		public BusinessObjectFactory Factory { get; }
		ILogger Logger { get; }
	}

	public class WiseRatesConversionContext
	{
		public WiseRatesConversionContext(RatesSearchResponse response, RatingCriteria criteria, ConversionOptions conversionOptions = default, string searchTraceID = "")
		{
			Argument.NotNull(response, nameof(response));

			Response = response;
			Criteria = criteria;
			ConversionSessionID = Guid.NewGuid().ToString();
			ConversionOptions = conversionOptions;
			SearchTraceID = searchTraceID;
		}

		public RatesSearchResponse Response { get; }

		public RatingCriteria Criteria { get; }

		public OrgHeader Carrier { get; set; }

		/// <summary>
		/// This is an ID representing the session of this conversion.
		/// It is used for associating cached-items against this conversion.
		/// </summary>
		public string ConversionSessionID { get; }

		/// <summary>
		/// The TraceID that was used to perform the rates search. If this
		/// conversion instance is being used as a result of a recent search
		/// then this field will be filled out. However, if its from a cached
		/// query that was re-made then it may be blank.
		/// </summary>
		public string SearchTraceID { get; }

		public IEnumerable<OrgHeader> GetCriteriaCarriers()
		{
			if (Criteria == null)
			{
				return Array.Empty<OrgHeader>();
			}

			var criteriaCarriers = Criteria.GetCostsSearchOrgs()
				.WhereNotNull()
				.Select(x => x.Org)
				.Where(x => x.OH_IsShippingProvider)
				.ToArray();

			return criteriaCarriers;
		}

		public ConversionOptions ConversionOptions { get; }
	}

	public struct ConversionOptions
	{
		public bool AddRateModeAndCategoryValidation;
	}

	#endregion
}
