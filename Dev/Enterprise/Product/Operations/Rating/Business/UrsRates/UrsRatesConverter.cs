#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.Interfaces;
using WiseRates.Constants;
using static System.FormattableString;
using static Enterprise.Rating.Business.UrsConstants;
using Constants = Enterprise.Core.Constants;
using Model = WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public class UrsRatesConverter(BusinessObjectFactory factory, ILogger logger, RatingCriteria criteria, string? requestID = default)
	{
		readonly BusinessObjectFactory factory = factory;
		readonly ILogger logger = logger;
		internal readonly string SessionID = Guid.NewGuid().ToString();
		readonly string? RequestID = requestID;

		readonly RatingCriteria ratingCriteria = criteria;

		public IEnumerable<IRateEntry> ConvertTradeServiceDtoToWiseEntries(IEnumerable<TradeServiceDto> tradeServiceLanes)
		{
			var transportModeGroups = tradeServiceLanes
				.GroupBy(UrsRatesParseHelper.GetTransportMode)
				.ToArray();
			var wiseHeaders = transportModeGroups.SelectMany(transportModeGroup => transportModeGroup
				.GroupBy(rate => UrsRatesParseHelper.GetCarrier(rate.TransportProvider))
				.Select(ratesByTransportModeAndCarrier => GetHeader(transportModeGroup.Key, ratesByTransportModeAndCarrier.Key, ratesByTransportModeAndCarrier)));

			return wiseHeaders.SelectMany(h => h.ChildRateEntries).Cast<UrsRateEntry>().ToList();
		}

		#region Header

		internal UrsRatingHeader GetHeader(string transportMode, string carrierCode, IEnumerable<TradeServiceDto> tradeServiceLanesWithSameTransportModeAndCarrier)
		{
			var carrier = ConvertCarrier(transportMode, carrierCode);
			var header = new UrsRatingHeader(factory, carrier);
			if (carrier.OrgHeader is null)
			{
				TryAddError(
					header.Errors,
					RatingHeaderSchema.TH_OH,
					Res.GetString(
						"188d7f9d-6c92-46d4-8173-fb968d4da1ec",
						"No single Carrier is assigned with the SCAC, IATA Code of the Carrier from Rates Service:\r\n{0}", carrierCode
					)
				);
			}

			header.ChildRateEntries = GetEntries(tradeServiceLanesWithSameTransportModeAndCarrier, header.Header, transportMode);

			return header;
		}

		#endregion

		#region Entry

		// tradeServiceLanes has same Transport Mode and Carrier
		List<UrsRateEntry> GetEntries(IEnumerable<TradeServiceDto> tradeServiceLanesWithSameTransportModeAndCarrier, OrgHeader carrier, string transportMode) =>
			tradeServiceLanesWithSameTransportModeAndCarrier
				.SelectMany(tradeService => CreateRates(
					tradeService,
					factory,
					carrier,
					transportMode,
					tradeService.Container != null ? WRConstants.ContainerModes.FCL : WRConstants.ContainerModes.LCL))
				.ToList();

		static List<UrsInclusiveCharge> CreateInclusiveCharges(IUrsCharge baseChargeDto) =>
			baseChargeDto
				.RateCollections?
				.Inclusive?
				.Select(inclusiveCharge => new UrsInclusiveCharge(baseChargeDto, inclusiveCharge))
				.ToList() ?? [];

		static UrsCharge CreateFreightCharge(ITradeServiceDto tradeService)
		{
			var chargeCode = UrsRatesParseHelper.GetFreightChargeCode(tradeService);
			return new UrsCharge(
				new BaseChargeDto
				{
					RateCollections = new RateCollectionDataDto
					{
						Items = tradeService.PriceInfo?.BaseRates?.Items,
						Inclusive = tradeService.PriceInfo?.BaseRates?.Inclusive
					},
					Code = chargeCode,
					UniversalCode = chargeCode,
					Name = (NoResString)"Freight",
					ChargeDefinition = new ChargeDefinitionDto
					{
						Code = chargeCode,
						UniversalCode = chargeCode,
						Description = (NoResString)"Freight"
					},
				},
				tradeService,
				true
			);
		}

		List<UrsRateEntry> CreateRates(ITradeServiceDto tradeService, BusinessObjectFactory factory, OrgHeader? carrier, string transportMode, string containerMode)
		{
			if (tradeService.PriceInfo == null)
			{
				return [];
			}

			// Main FRT charge defines rate details on RateEntry.
			var frtCharge = CreateFreightCharge(tradeService);
			var isComposedTradeService = !tradeService.TradeServices.IsNullOrEmpty();

			var tradeServices = isComposedTradeService ? tradeService.TradeServices : new[] { tradeService };
			var ursCharges = tradeServices
				.SelectMany<ITradeServiceDto, IUrsCharge>(service =>
				{
					var frtCharge = CreateFreightCharge(service);
					var inclusiveCharges = CreateInclusiveCharges(frtCharge);
					var otherCharges = service.PriceInfo?.Charges?.Items?
						.Where(charge => !UrsRatesParseHelper.IsPenaltyCharge(charge) && !UrsRatesParseHelper.IsBookingTermCharge(charge))
						.Select(charge => new UrsCharge(charge, service)).ToList() ?? [];
					var otherInclusiveCharges = otherCharges.SelectMany(CreateInclusiveCharges);
					return [frtCharge, .. inclusiveCharges, .. otherCharges, .. otherInclusiveCharges];
				}).ToList();

			if (isComposedTradeService)
			{
				ursCharges.AddRange(
					tradeService.PriceInfo?.Charges?.Items?
						.Where(charge => !UrsRatesParseHelper.IsPenaltyCharge(charge) && !UrsRatesParseHelper.IsBookingTermCharge(charge))
						.Select(charge => new UrsCharge(charge, tradeService)) ?? []
				);
			}

			// group chargeGroup by container PK
			ZGuid[] filteredContainerPKs = [ZGuid.Empty];
			UrsContainer? ursContainer = null;

			if (tradeService.Container != null)
			{
				var container = FindContainers(tradeService, transportMode);
				if (container.IsMapped)
				{
					var criteriaContainers = ratingCriteria.GetContainerPKs();
					filteredContainerPKs = criteriaContainers.IsNullOrEmpty()
						? container.ContainerPKs
						: container.ContainerPKs.Intersect(criteriaContainers).ToArray();
				}
				ursContainer = container;
			}

			if (filteredContainerPKs.IsNullOrEmpty())
			{
				return [];
			}

			var bookingInfos = UniversalToWiseRateConverter.GetBookingInfo(tradeService);
			if (bookingInfos.IsNullOrEmpty())
			{
				bookingInfos = [null];
			}

			var convertedEntries = ursCharges
				.GroupBy(charge => WiseRatesConverter.ConvertChargeCode(charge.ChargeDefinition?.UniversalCode, factory, charge.Code, carrier, transportMode).chargeCode?.AC_ChargeGroup)
				.SelectMany(chargeGroup => filteredContainerPKs.SelectMany(containerPK => bookingInfos.Select(bookingInfo =>
					CreateNewEntry(tradeService, ursContainer, [.. chargeGroup], transportMode, containerMode, bookingInfo, frtCharge, carrier, containerPK))));

			var entries = new List<UrsRateEntry>();
			foreach (var (entry, error) in convertedEntries)
			{
				if (entry != null)
				{
					entries.Add(entry);
				}
				else if (error != null)
				{
					logger?.Warning(error);
					return [];
				}
			}
			return entries;
		}

		static Model.ChargeType GetChargeTypeForAir(IChargeDefinitionDto chargeDefinition, bool isFreight, bool isInclusive)
		{
			if (isFreight)
			{
				return Model.ChargeType.None;
			}

			if (isInclusive)
			{
				return Model.ChargeType.Included;
			}

			return chargeDefinition.RequiresQuantifiedInput ? Model.ChargeType.Optional : Model.ChargeType.SubjectTo;
		}

		static Model.ChargeType GetChargeTypeForSea(IChargeDefinitionDto chargeDefinition, IRateCollectionDataDto? rateCollections, bool isFreight, bool isInclusive)
		{
			if (isFreight)
			{
				return Model.ChargeType.Freight;
			}

			if (isInclusive)
			{
				return Model.ChargeType.Included;
			}

			if (rateCollections?.Items == null || !rateCollections.Items.Any())
			{
				return Model.ChargeType.None;
			}

			foreach (var item in rateCollections.Items)
			{
				var priceEntries = item.PriceEntries?.ToArray();
				if (priceEntries == null)
				{
					continue;
				}

				// Note: priceEntries mostly have 1 item so multiple iterations below shouldn't affect performance.
				if (priceEntries.Any(x => x.Applicable == RateApplicableCode.Vatos))
				{
					return Model.ChargeType.SubjectTo;
				}

				if (priceEntries.Any(x => x.PricingQuantityUnit == UnitOfMeasurementCode.BillOfLading))
				{
					return Model.ChargeType.Bol;
				}
			}

			if (chargeDefinition.RequiresQuantifiedInput)
			{
				return Model.ChargeType.Additional;
			}
			foreach (var item in rateCollections.Items)
			{
				var priceEntries = item.PriceEntries?.ToArray();

				if (priceEntries != null && priceEntries.Any(x => x.PricingQuantityUnit == UnitOfMeasurementCode.Container))
				{
					return Model.ChargeType.Freight;
				}
			}

			return Model.ChargeType.None;
		}

		static (Model.ChargeType chargeType, bool isOptional) GetChargeType(string transportMode, IChargeDefinitionDto chargeDefinition, Func<IRateCollectionDataDto?> funcGetRateCollection, IBaseChargeDto frtCharge, bool isInclusive)
		{
			var isFreight = chargeDefinition.UniversalCode == frtCharge.Code;
			var chargeType = transportMode == WRConstants.TransportModes.AIR
				? GetChargeTypeForAir(chargeDefinition, isFreight, isInclusive)
				: GetChargeTypeForSea(chargeDefinition, funcGetRateCollection(), isFreight, isInclusive);
			var isOptional = chargeType == Model.ChargeType.Optional || chargeType == Model.ChargeType.Additional || chargeDefinition.RequiresQuantifiedInput;
			return (chargeType, isOptional);
		}

		(UrsRateEntry? entry, string? error) CreateNewEntry(ITradeServiceDto tradeService, UrsContainer? ursContainer, List<IUrsCharge> ursChargesWithSameChargeGroupAndContainerType, string transportMode, string containerMode, UrsBookingInfo? bookingInfo, IBaseChargeDto frtCharge, OrgHeader? carrier, ZGuid containerPK)
		{
			ZString rateCategory, rateMode, carrierServiceLevel;
			ILocation? origin, destination, via;
			string? error;
			List<UrsRateLine>? lines;

			var entry = new UrsRateEntry(tradeService, factory);

			(origin, error) = ConvertLocation(UrsRatesParseHelper.GetOriginCode(tradeService), nameof(origin));
			TryAddError(entry.Errors, RateEntrySchema.TI_OriginLRC, error);

			(destination, error) = ConvertLocation(UrsRatesParseHelper.GetDestinationCode(tradeService), nameof(destination));
			TryAddError(entry.Errors, RateEntrySchema.TI_DestinationLRC, error);

			(via, error) = ConvertLocation(UrsRatesParseHelper.GetViaCode(tradeService), nameof(via));
			TryAddError(entry.Errors, RateEntrySchema.TI_ViaLRC, error);

			(carrierServiceLevel, error) = UrsRatesParseHelper.ConvertServiceLevel(tradeService, carrier);
			TryAddError(entry.Errors, RateEntrySchema.TI_PL_NKCarrierServiceLevel, error);

			entry.TI_RC = containerPK;
			entry.UrsContainer = ursContainer;

			entry.ContainerPayloadWeightOverride = ursContainer?.PayloadWeight ?? 0m;
			entry.ContainerPayloadVolumeOverride = ursContainer?.PayloadVolume ?? 0m;
			entry.ContainerPivotWeight = ursContainer?.PivotWeight ?? 0m;

			(lines, error) = GetLines(tradeService, ursChargesWithSameChargeGroupAndContainerType, entry, transportMode, frtCharge, carrier);
			if (error != null)
			{
				return (null, error);
			}
			entry.ChildRateLines = lines;

			(rateCategory, error) = WiseRatesConverter.ConvertRateCategory(lines, logger, containerMode, transportMode);
			TryAddError(entry.Errors, RateEntrySchema.TI_RateCategory, error);

			(rateMode, error) = WiseRatesConverter.ConvertRateMode(containerMode, transportMode, rateCategory, logger);
			TryAddError(entry.Errors, RateEntrySchema.TI_Mode, error);

			entry.TI_Mode = rateMode;
			entry.TI_RateCategory = rateCategory;
			entry.TI_OriginLRC = origin?.Code ?? "";
			entry.TI_DestinationLRC = destination?.Code ?? "";
			entry.TI_PL_NKCarrierServiceLevel = carrierServiceLevel;
			entry.TI_ViaLRC = via?.Code ?? "";
			if (tradeService.Product?.Aircraft?.Cao ?? false)
			{
				entry.TI_AircraftType = (ZString)Constants.AircraftType.CAO;
			}
			var dates = tradeService.PriceInfo?.BaseRates?.Items?.FirstOrDefault()?.VersionDateInfo;
			entry.TI_RateStartDate = dates?.StartDate.ToZDate() ?? ZDate.Empty;
			entry.TI_RateEndDate = dates?.EndDate.ToZDate() ?? ZDate.Empty;
			entry.TI_MatchContainerRateClass = false;
			entry.TI_PaymentTerm = UrsRatesParseHelper.GetPaymentTerms(tradeService);
			entry.NamedAccounts = UrsRatesParseHelper.GetNamedAccounts(tradeService);
			entry.RateProvider = "URS";

			var isAirMode = transportMode == WRConstants.TransportModes.AIR;
			entry.CustomFields = UrsRatesParseHelper.GetProviderCustomFields(tradeService, isAirMode);
			entry.TI_ContractNumber = UrsRatesParseHelper.GetContractNumber(tradeService, isAirMode);

			entry.CommodityGroup = UrsRatesParseHelper.GetCommodityGroup(tradeService);
			entry.CarrierSpecificCommodity = UrsRatesParseHelper.GetCarrierCommodity(tradeService);

			entry.BookingInfo = bookingInfo;

			return (entry, null);
		}

	#endregion

		(List<UrsRateLine>? lines, string? error) GetLines(ITradeServiceDto tradeService, List<IUrsCharge> ursChargesWithSameChargeGroup, UrsRateEntry wiseEntry, string transportMode, IBaseChargeDto frtCharge, OrgHeader? carrier)
		{
			var lines = new List<UrsRateLine>();

			foreach (var charge in ursChargesWithSameChargeGroup)
			{
				if (charge.ChargeDefinition == null)
				{
					var message = Res.GetString("aab8f999-cb73-46de-aa75-54d95fe5642e", "The charge '{0} - {1}' has no universal code", charge.Code, charge.Name);
					LogChargeDiscardReason(charge.Code, message);
					continue;
				}

				if (charge is UrsInclusiveCharge inclusiveCharge)
				{
					lines.Add(GetInclusiveLine(inclusiveCharge, wiseEntry, transportMode, frtCharge, carrier));
					continue;
				}

				var (line, error) = GetLine(tradeService, charge, wiseEntry, transportMode, frtCharge, carrier);

				if (line != null)
				{
					lines.Add(line);
				}
				else if (error != null && charge is not UrsCharge { IsGeneratedFreightCharge: true })
				{
					LogChargeDiscardReason(charge.Code, error);
				}
				else
				{
					return (null, error);
				}
			}

			return (lines, null);
		}

		/// <summary>
		///		Assign the line an FRT calculator code. Apply the charge code from the universal code.
		///		Refer to base charge's charge code as the main charge code in PreCarriageOnCarriageChargeType rate line item.
		/// </summary>
		void SetFreightInclusiveCalculator(UrsRateLine line, string universalCode, Model.ChargeType chargeType, bool isRestricted, IBaseChargeDto baseChargeDto, OrgHeader? carrier, string transportMode)
		{
			var (accChargeCode, error) = WiseRatesConverter.ConvertChargeCode(universalCode, factory, baseChargeDto.Code, carrier, transportMode);
			TryAddError(line.Errors, RateLinesSchema.TL_AC, error);
			line.TL_AC = accChargeCode?.PK ?? ZGuid.Empty;

			line.TL_RateCalculator = FreightInclusiveCalculator.Code;

			line.ChildRateLineItems = new List<WiseLineItem>
			{
				new(line, FreightInclusiveCalculator.Items.FreightCalcType, CalculatorDeciderHelper.GetFreightCalcTypeCode(chargeType), ZDecimal.Zero, string.Empty, ZDecimal.Zero, 0, isRestricted),
				new(line, FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType, string.Empty, ZDecimal.Zero, baseChargeDto.UniversalCode, ZDecimal.Zero, 0, isRestricted),
			};
		}

		/// <summary>
		///		Convert DTO inclusive charge to a rate line with FreightInclusiveCalculator referencing the base charge.
		/// </summary>
		UrsRateLine GetInclusiveLine(UrsInclusiveCharge baseChargeDto, UrsRateEntry wiseEntry, string transportMode, IBaseChargeDto frtCharge, OrgHeader? carrier)
		{
			var chargeDefinition = baseChargeDto.ChargeDefinition;

			var (chargeType, isOptional) = GetChargeType(transportMode, chargeDefinition, () => null, frtCharge, isInclusive: true);
			var line = new UrsRateLine(factory, baseChargeDto, chargeType) { ParentRateEntry = wiseEntry };
			line.IsOptional = isOptional;
			SetFreightInclusiveCalculator(line, chargeDefinition.UniversalCode, chargeType, isRestricted: false, baseChargeDto, carrier, transportMode);

			return line;
		}

		(UrsRateLine? line, string? error) GetLine(ITradeServiceDto tradeService, IUrsCharge charge, UrsRateEntry wiseEntry, string transportMode, IBaseChargeDto frtCharge, OrgHeader? carrier)
		{
			var rateCollection = charge.RateCollections?.Items?.FirstOrDefault();
			var rateItem = rateCollection?.PriceEntries?.FirstOrDefault();
			if (rateCollection == null)
			{
				return (null, Res.GetString("85ca33cd-a5b3-4c1f-beda-951a7c5b8d9d", "The charge '{0} - {1}' has no rate information", charge.UniversalCode, charge.Name));
			}

			if (rateItem == null)
			{
				return (null, Res.GetString("884cc75a-9395-491f-abed-cea952634cfd", "The charge \"{0} - {1}\" has no rate price information", charge.UniversalCode, charge.Name));
			}

			var chargeDefinition = charge.ChargeDefinition;
			var (chargeType, isOptional) = GetChargeType(transportMode, chargeDefinition, () => charge.RateCollections, frtCharge, isInclusive: false);

			var line = new UrsRateLine(factory, charge, chargeType) { ParentRateEntry = wiseEntry };
			line.IsOptional = isOptional;

			// A VATOS charge should be inclusive and subject to the main FREIGHT charge.
			// It is different from the explicit inclusive charges which are in baseChargeDto.RateCollections.Inclusive collection.
			// Why doesn't URS put VATOS in the inclusive collection? I don't know.
			// The below conditions are only with the VATOS charge at the moment.
			// Use them here instead of finding VATOS applicable code in the rate collection (again).
			// Cref: GetChargeType > GetChargeTypeForSea
			if (transportMode == WRConstants.TransportModes.SEA && chargeType == Model.ChargeType.SubjectTo)
			{
				SetFreightInclusiveCalculator(line, chargeDefinition.UniversalCode, chargeType, isRestricted: true, frtCharge, carrier, transportMode);
				return (line, null);
			}

			ZString currencyCode;

			var (accChargeCode, error) = WiseRatesConverter.ConvertChargeCode(chargeDefinition.UniversalCode, factory, charge.Code, carrier, transportMode);
			TryAddError(line.Errors, RateLinesSchema.TL_AC, error);
			line.TL_AC = accChargeCode?.PK ?? ZGuid.Empty;

			(currencyCode, error) = ConvertCurrency(charge, frtCharge);
			if (error != null)
			{
				TryAddError(line.Errors, RateLinesSchema.TL_RX_NKCurrency, error);
				return (null, error);
			}
			line.TL_RX_NKCurrency = currencyCode;

			if (!UrsRatesParseHelper.IsFlatPrice(rateCollection.PriceEntries))
			{
				foreach (var entry in rateCollection.PriceEntries)
				{
					if (!UrsCalculatorDecider.IsValidBreakType(entry.BreakType))
					{
						return (null, $"Charges with break criteria '{entry.BreakType}' are not supported at the moment");
					}
				}
			}

			var calc = UrsCalculatorDecider.GetCalculator(rateCollection.PriceEntries, line, logger, transportMode);
			if (calc.Code == NullCalculator.Code)
			{
				return (null, Res.GetString("ef530a70-566d-4f2a-92fe-68ea7307d4fd", "Could not create calculator from:{0}{1}", System.Environment.NewLine, charge.ToYAML()));
			}
			line.TL_RateCalculator = calc.Code;
			line.ChildRateLineItems = calc.RateLineItems.ToArray();
			TryAddError(line.Errors, RateLinesSchema.TL_RateCalculator, error);

			line.TL_ActualPercentage = calc.ActualPercentage;
			line.TL_Rounding = calc.RoundingFactor != 0 ? (ZString)RatingRoundingTypes.Custom : ZString.Empty;
			line.TL_RoundingFactor = calc.RoundingFactor != 0 ? calc.RoundingFactor : 0;
			(line.TL_WeightVolume, _) = UrsRatesParseHelper.GetUnit(calc.Unit ?? string.Empty);
			line.TL_WeightVolumeMultiple = calc.UnitMultiplier;

			var origin = LocationHelper.GetLocationFromString(UrsRatesParseHelper.GetOriginCode(tradeService), factory);
			var destination = LocationHelper.GetLocationFromString(UrsRatesParseHelper.GetDestinationCode(tradeService), factory);
			var direction = ImportExportHelper.GetJobDirection(origin?.Country?.Code ?? "", destination?.Country?.Code ?? "");
			var conversionFactor = UrsRatesParseHelper.ConvertConversionFactor(charge, direction, transportMode, line.TL_WeightVolume);
			line.ConversionFactor = conversionFactor;

			return (line, null);
		}

		void LogChargeDiscardReason(string chargeCode, string reason) => logger?.Warning($"Charge {chargeCode} was ignored because {reason}");

		#region Currency

		string GetOrCreateChargeID(IBaseChargeDto ursCharge)
		{
			return
				ursCharge.ChargeDefinition.Id == Guid.Empty
				? (NoResString)"No Charge ID (likely freight charge)"
				: ursCharge.ChargeDefinition.Id.ToString();
		}

		(ZString code, string? error) ConvertCurrency(IBaseChargeDto ursCharge, IBaseChargeDto frtCharge)
		{
			var rateItems = ursCharge.RateCollections.Items.First();
			var rate = rateItems.PriceEntries.First();

			if (!string.IsNullOrEmpty(rateItems.Currency))
			{
				var refCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, rateItems.Currency);
				if (refCurrency != null)
				{
					return (rateItems.Currency, null);
				}

				return (ZString.Empty, Res.GetString("295ba23e-e167-41c8-9a7f-3d756b73c61e", "No Currency is assigned with or has the same Code as '{0}'", rateItems.Currency));
			}

			// Percentage charges gets their currency from the freight charge
			// Freight charge is evaluated first so this currency should be valid
			if (rate.Applicable == RateApplicableCode.Percentage)
			{
				return (frtCharge.RateCollections.Items.First().Currency, null);
			}

			if (UrsRatesParseHelper.IsRestricted(rate) && rate.Applicable != RateApplicableCode.Tact)
			{
				return (null, null);
			}

			// If it reaches this point, it's a faulty incoming charge that we have been told should have a currency
			var errorMessage = Invariant($"The charge \"{ursCharge.Code} - {ursCharge.Name} - {GetOrCreateChargeID(ursCharge)}\" has no currency for {nameof(ursCharge.RateCollections)}.{nameof(ursCharge.RateCollections.Items)}[0].{nameof(IRateCollectionDto.Currency)}");
			ErrorReporter.ReportOnce("MissingCurrencyOnWiseRate", Invariant($"[TraceID: {RequestID}] {errorMessage}"));
			return (ZString.Empty, errorMessage);
		}

		#endregion

		#region Location

		(ILocation? location, string? error) ConvertLocation(string locationCode, string source)
		{
			if (string.IsNullOrWhiteSpace(locationCode))
			{
				return (null, null);
			}

			var location = LocationHelper.GetLocationFromString(locationCode, factory);
			if (location != null)
			{
				return (location, null);
			}

			return (null, Res.GetString("70af8801-6ecb-4396-8f12-b1665a46f2c1", "No Location matches {0} '{1}'", source, locationCode));
		}

		#endregion

		#region Container Type

		/// <summary>
		/// Returns a UrsContainer containing 0 or more container PKs mapped to the given trade service container.
		/// Might match depending on RC_ISOType or RC_Code depending on transport mode
		/// </summary>
		UrsContainer FindContainers(ITradeServiceDto tradeService, string transportMode)
		{
			var code = string.IsNullOrEmpty(tradeService.Container.IsoCode) ? tradeService.Container.Code : tradeService.Container.IsoCode;
			var column = transportMode == WRConstants.TransportModes.SEA
				? RefContainerSchema.RC_ISOType
				: RefContainerSchema.RC_Code;

			var query = new ZDBOnlyQuery(typeof(RefContainer));
			query.AddToFilter(column, code);
			var finalContainers = factory.Load<RefContainer>(query);

			return new UrsContainer
			{
				Code = code,
				ContainerPKs = [.. finalContainers.Select(c => c.PK)],
				PivotWeight = GetUnit(tradeService.Container.Weight?.Pivot, QuantityUnit.KG),
				PayloadWeight = GetUnit(tradeService.Container.Weight?.MaxNet, QuantityUnit.KG),
				PayloadVolume = GetUnit(tradeService.Container.Measurements?.Inside?.Volume, QuantityUnit.M3)
			};
		}

		decimal? GetUnit(IUnitDto? measurementDto, string quantityUnit)
		{
			if (measurementDto != null && measurementDto.Quantity > 0)
			{
				var (unit, error) = UrsRatesParseHelper.GetUnit(measurementDto.Unit);
				if (string.IsNullOrEmpty(error))
				{
					var payload = new Quantity((decimal)measurementDto.Quantity, unit);
					return payload.AmountFor(quantityUnit).Amount;
				}
				else
				{
					logger.Log(LogType.Warning, error);
				}
			}

			return null;
		}

		#endregion

		#region Carrier

		public UrsCarrier ConvertCarrier(string transportMode, string ursCarrierCode) =>
			transportMode == WRConstants.TransportModes.AIR
				? GetCarrierByIATACode(ursCarrierCode)
				: GetCarrierBySCACCode(ursCarrierCode);

		UrsCarrier GetCarrierByIATACode(string airlineCarrierIATACode) =>
			factory.GetCachedValue(
				string.Join("_", "IATA", airlineCarrierIATACode, SessionID),
				() =>
				{
					var airLineSubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK, OrgMiscServSchema.OM_RM_Airline);
					airLineSubQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty);

					var codeLength = airlineCarrierIATACode.Length;
					if (codeLength == 2)
					{
						airLineSubQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, airlineCarrierIATACode);
					}
					else if (codeLength == 3)
					{
						airLineSubQuery.AddToFilter(RefAirlineSchema.RM_ThreeLetterCode, airlineCarrierIATACode);
					}
					else
					{
						var message = ZString.Format(Res.GetString("b6749f6f-18a7-4fb1-b0ed-f1ec863335fa", "IATA Code '{0}' with length {1} has not been supported"), airlineCarrierIATACode, codeLength);
						logger.Warning(message);
						return UrsCarrier.FromIATA(airlineCarrierIATACode);
					}

					var orgMiscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, OrgHeaderSchema.PK);
					orgMiscServSubQuery.AddSubQuery(airLineSubQuery, JoinCondition.And);

					var query = new ZDBOnlyQuery(typeof(OrgHeader));
					query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
					query.AddSubQuery(orgMiscServSubQuery, JoinCondition.And);

					var cw1Carriers = factory.Load<OrgHeader>(query);

					if (cw1Carriers.Length == 0)
					{
						var message = ZString.Format(Res.GetString("c3aac9af-40be-4405-b218-be71167a13bb", "No active Carrier is assigned with IATA Code '{0}'"), airlineCarrierIATACode);
						logger.Warning(message);
						return UrsCarrier.FromIATA(airlineCarrierIATACode);
					}

					if (cw1Carriers.Length > 1)
					{
						var message = ZString.Format(Res.GetString("039cc040-a1d3-4f14-a6e0-671665afab36", "More than one of the active Carriers ({0}) are assigned with IATA Code '{1}'"), string.Join(", ", cw1Carriers.Select(c => c.OH_Code).OrderBy(x => x)), airlineCarrierIATACode);
						logger.Warning(message);
						return UrsCarrier.FromIATA(
							airlineCarrierIATACode,
							cw1Carriers.Select(carrier => carrier.OH_Code.ToString()).Distinct().ToArray(),
							cw1Carriers.GroupBy(c => c.MiscServ.OM_RM_Airline).Any(g => g.IsCountMoreThan(1))
						);
					}

					return UrsCarrier.FromOrg(cw1Carriers[0]);
				});

		UrsCarrier GetCarrierBySCACCode(string seaCarrierSCACCode) =>
			factory.GetCachedValue(
				string.Join("_", "SCAC", seaCarrierSCACCode, SessionID),
				() =>
				{
					var subQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK, OrgHeaderSchema.OH_RSL_ShippingLine);
					subQuery.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, seaCarrierSCACCode);

					var query = new ZDBOnlyQuery(typeof(OrgHeader));
					query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
					query.AddSubQuery(subQuery, JoinCondition.And);

					var cw1Carriers = factory.Load<OrgHeader>(query);

					if (cw1Carriers.Length == 0)
					{
						var message = ZString.Format(Res.GetString("19423f12-00cf-4413-9a15-e8858170648e", "No active Carrier is assigned with SCAC Code '{0}'"), seaCarrierSCACCode);
						logger.Warning(message);
						return UrsCarrier.FromSCAC(seaCarrierSCACCode);
					}

					return UrsCarrier.FromOrg(cw1Carriers[0]);
				});

		#endregion

		#region Helper

		static void TryAddError(IDictionary<SchemaColumn, string> errors, SchemaColumn column, string? error)
		{
			if (!string.IsNullOrWhiteSpace(error))
			{
				errors[column] = error!;
			}
		}

		#endregion
	}
}
