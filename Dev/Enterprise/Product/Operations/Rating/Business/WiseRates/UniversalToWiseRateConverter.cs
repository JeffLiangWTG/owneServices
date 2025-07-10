using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.Interfaces;
using Urs.Api.Integration.Interfaces.Schedules;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.Business
{
	public class UniversalToWiseRateConverter
	{
		ILogger Logger { get; }
		IUniversalToWiseRateErrorReporter ErrorReporter { get; }

		public const string UrsProviderCode = "URS";

		public UniversalToWiseRateConverter(ILogger logger, IUniversalToWiseRateErrorReporter errorReporter)
		{
			Logger = logger;
			ErrorReporter = errorReporter;
		}

		public IList<Rate> Convert(IEnumerable<TradeServiceDto> tradeLanes)
		{
			Argument.NotNull(tradeLanes, nameof(tradeLanes));

			var result = new List<Rate>();

			foreach (var tradeLane in tradeLanes.WhereNotNull())
			{
				var (rate, error) = Convert(tradeLane);
				if (!string.IsNullOrEmpty(error))
				{
					Logger.Error(error);
					continue;
				}

				result.Add(rate);
			}

			return result;
		}

		(Rate rate, string error) Convert(TradeServiceDto tradelane)
		{
			var rate = new Rate();

			rate.Id = string.IsNullOrEmpty(tradelane.ExternalReference)
				? ZGuid.NewZGuid().ToString()
				: tradelane.ExternalReference;

			rate.Provider = UrsProviderCode;
			rate.RatesServiceProvider = UrsProviderCode;
			rate.ProviderRateId = rate.Id;
			rate.Origin = UrsRatesParseHelper.GetOriginCode(tradelane);
			rate.Via = UrsRatesParseHelper.GetViaCode(tradelane);
			rate.Destination = UrsRatesParseHelper.GetDestinationCode(tradelane);
			rate.TransportMode = UrsRatesParseHelper.GetTransportMode(tradelane);
			rate.Carrier = UrsRatesParseHelper.GetCarrier(tradelane.TransportProvider);
			rate.ServiceLevel = UrsRatesParseHelper.GetUniversalServiceLevel(tradelane);
			rate.Container = GetContainer(tradelane);
			rate.CarrierCommodityInfo = UrsRatesParseHelper.GetCarrierCommodity(tradelane);
			rate.PaymentTerm = UrsRatesParseHelper.GetPaymentTerms(tradelane);
			rate.NamedAccounts = UrsRatesParseHelper.GetNamedAccounts(tradelane);
			var isAirMode = rate.TransportMode == WRConstants.TransportModes.AIR;
			rate.ProviderCustomFields = UrsRatesParseHelper.GetProviderCustomFields(tradelane, isAirMode);
			rate.RawRate = JsonConvert.SerializeObject(tradelane).Compress();
			rate.ContractNumber = UrsRatesParseHelper.GetContractNumber(tradelane, isAirMode);
			rate.Commodity = UrsRatesParseHelper.GetCommodityGroup(tradelane);
			// TODO: support multiple booking infos on URS rate for RSL
			rate.BookingInfo = GetBookingInfo(tradelane).FirstOrDefault();

			// WiseRate ContainerMode is either FCL or LCL for both AIR and SEA (not ULD or LSE or anything else)
			rate.ContainerMode = tradelane.Container != null
				? WRConstants.ContainerModes.FCL
				: WRConstants.ContainerModes.LCL;

			var dates = tradelane.PriceInfo?.BaseRates?.Items?.FirstOrDefault()?.VersionDateInfo;
			if (dates != null)
			{
				rate.StartDate = dates.StartDate;
				rate.ExpiryDate = dates.EndDate;
				rate.IssueDate = dates.IssueDate;
			}

			// Only for CargoSphere. Not yet known what this is in URS
			// rate.ServiceGroupId = ???

			var charges = new List<Charge>();

			var tradeServices = tradelane.TradeServices != null && tradelane.TradeServices.Any()
				? tradelane.TradeServices
				: new[] { tradelane };

			foreach (var tradeService in tradeServices)
			{
				var converted = ConvertCharges(tradeService);
				if (converted.error == null)
				{
					charges.AddRange(converted.charges);
				}
				else
				{
					return (null, converted.error);
				}
			}
			if (isAirMode && charges.Any(c => c.ChargeCode == UrsRatesParseHelper.FRTUniversalChargeCode))
			{
				foreach (var charge in charges)
				{
					charge.IsHigherBreakLowerRate = true;
				}
			}

			rate.Charges = charges;

			return (rate, null);
		}

		#region Container

		RefContainer GetContainer(TradeServiceDto tradelane)
		{
			if (tradelane.Container == null)
			{
				return null;
			}

			var container = string.IsNullOrEmpty(tradelane.Container.IsoCode)
				? new RefContainer { Code = tradelane.Container.Code }
				: new RefContainer { Code = tradelane.Container.IsoCode, ISOType = tradelane.Container.IsoCode };

			if (tradelane.Container.Weight != null)
			{
				if (tradelane.Container.Weight.Pivot.Quantity > 0)
				{
					var (unit, error) = UrsRatesParseHelper.GetUnit(tradelane.Container.Weight.Pivot.Unit);
					if (string.IsNullOrEmpty(error))
					{
						var ursPivotWeight = new Quantity((decimal)tradelane.Container.Weight.Pivot.Quantity, unit);
						container.PivotWeight = ursPivotWeight.AmountFor(QuantityUnit.KG).Amount;
					}
					else
					{
						Logger.Log(LogType.Warning, error);
					}
				}

				if (tradelane.Container.Weight.MaxNet.Quantity > 0)
				{
					var (unit, error) = UrsRatesParseHelper.GetUnit(tradelane.Container.Weight.MaxNet.Unit);
					if (string.IsNullOrEmpty(error))
					{
						var ursPayloadWeight = new Quantity((decimal)tradelane.Container.Weight.MaxNet.Quantity, unit);
						container.PayloadWeight = ursPayloadWeight.AmountFor(QuantityUnit.KG).Amount;
					}
					else
					{
						Logger.Log(LogType.Warning, error);
					}
				}
			}

			if ((tradelane.Container.Measurements?.Inside?.Volume.Quantity ?? 0) > 0)
			{
				var (unit, error) = UrsRatesParseHelper.GetUnit(tradelane.Container.Measurements.Inside.Volume.Unit);
				if (string.IsNullOrEmpty(error))
				{
					var ursPayloadVolume = new Quantity((decimal)tradelane.Container.Measurements.Inside.Volume.Quantity, unit);
					container.PayloadVolume = ursPayloadVolume.AmountFor(QuantityUnit.M3).Amount;
				}
				else
				{
					Logger.Log(LogType.Warning, error);
				}
			}
			return container;
		}

		#endregion

		#region Charges

		(IEnumerable<Charge> charges, string error) ConvertCharges(ITradeServiceDto tradelane)
		{
			if (tradelane.PriceInfo == null)
			{
				return (null, null);
			}

			var charges = new List<Charge>();

			// FRT charge(s)
			var chargeCode = UrsRatesParseHelper.GetFreightChargeCode(tradelane);

			var frtChargeRateCollections = new RateCollectionDataDto();
			frtChargeRateCollections.Items = tradelane.PriceInfo?.BaseRates?.Items;
			frtChargeRateCollections.Inclusive = tradelane.PriceInfo?.BaseRates?.Inclusive;

			var frtCharge = new BaseChargeDto();
			frtCharge.RateCollections = frtChargeRateCollections;
			frtCharge.Code = chargeCode; // Used as local charge code
			frtCharge.UniversalCode = chargeCode;
			frtCharge.Name = "Freight";
			frtCharge.ChargeDefinition = new ChargeDefinitionDto
			{
				Code = chargeCode, // Used as local charge code
				UniversalCode = chargeCode,
				Description = (NoResString)"Freight",
			};

			var ursCharges = new List<IBaseChargeDto>();
			ursCharges.Add(frtCharge);

			// Penalty & BookingTerms charges are not treated as regular charges and are only be used for populating BookingInfo.
			var otherCharges = tradelane.PriceInfo?.Charges?.Items?
				.Where(charge => !UrsRatesParseHelper.IsPenaltyCharge(charge) && !UrsRatesParseHelper.IsBookingTermCharge(charge));
			if (otherCharges != null && otherCharges.Any())
			{
				ursCharges.AddRange(otherCharges);
			}

			foreach (var charge in ursCharges)
			{
				var (wiseCharges, chargeError) = ConvertCharge(tradelane, charge, frtCharge);
				if (!string.IsNullOrEmpty(chargeError))
				{
					if (charge == frtCharge)
					{
						return (null, chargeError);
					}
					else
					{
						LogChargeDiscardReason("", charge.ChargeDefinition?.UniversalCode, chargeError);
						continue;
					}
				}

				charges.AddRange(wiseCharges);
			}

			return (charges, null);
		}

		(IEnumerable<Charge> charges, string error) ConvertCharge(ITradeServiceDto tradeService, IBaseChargeDto ursCharge, IBaseChargeDto freightCharge)
		{
			var charges = new List<Charge>();
			var rateItems = ursCharge.RateCollections?.Items?.FirstOrDefault();
			var rateItem = rateItems?.PriceEntries?.FirstOrDefault();

			if (rateItems == null)
			{
				return (null, FormattableString.Invariant($"The charge \"{ursCharge.UniversalCode} - {ursCharge.Name}\" has no rate information"));
			}

			if (rateItem == null)
			{
				return (null, FormattableString.Invariant($"The charge \"{ursCharge.UniversalCode} - {ursCharge.Name}\" has no rate price information"));
			}

			if (ursCharge.ChargeDefinition == null)
			{
				return (null, FormattableString.Invariant($"The charge \"{ursCharge.Code} - {ursCharge.Name}\" has no universal code"));
			}

			var charge = CreateCharge(tradeService, ursCharge, ursCharge.ChargeDefinition, rateItem);
			var (currency, error) = GetCurrency(tradeService, charge, ursCharge, freightCharge);

			if (!string.IsNullOrEmpty(error))
			{
				return (null, error);
			}
			charge.Currency = currency;

			if (rateItem.Applicable == RateApplicableCode.Vatos)
			{
				charge.FreightInclusiveCarriageCharge = freightCharge.Code;
				charge.ChargeType = ChargeType.SubjectTo;
			}

			var flatOrPerUnitCharges = GetFlatOrPerUnitCharges(tradeService, ursCharge, charge, currency);
			if (flatOrPerUnitCharges.error != null)
			{
				return (null, flatOrPerUnitCharges.error);
			}
			charges.AddRange(flatOrPerUnitCharges.charges);

			if (ursCharge.RateCollections.Inclusive != null)
			{
				foreach (var inclusiveCharge in ursCharge.RateCollections.Inclusive)
				{
					var inclusive = CreateCharge(tradeService, ursCharge, inclusiveCharge, rateItem, isInclusive: true);
					inclusive.FreightInclusiveCarriageCharge = ursCharge.ChargeDefinition.UniversalCode;
					charges.Add(inclusive);
				}
			}

			if (charges.Any())
			{
				return (charges, null);
			}
			else
			{
				return (null, (NoResString)"The rate is not supported");
			}
		}

		(string currency, string error) GetCurrency(ITradeServiceDto tradeService, Charge charge, IBaseChargeDto ursCharge, IBaseChargeDto frtCharge)
		{
			var rateItems = ursCharge.RateCollections.Items.First();
			var rate = rateItems.PriceEntries.First();

			if (!string.IsNullOrEmpty(rateItems.Currency))
			{
				return (rateItems.Currency, null);
			}

			// Percentage charges gets their currency from the freight charge
			if (rate.Applicable == RateApplicableCode.Percentage)
			{
				return (frtCharge.RateCollections.Items.First().Currency, null);
			}

			if (UrsRatesParseHelper.IsRestricted(rate) && rate.Applicable != RateApplicableCode.Tact)
			{
				return (null, null);
			}

			// If it reaches this point, it's a faulty incoming charge that we have been told should have a currency
			var errorMessage = FormattableString.Invariant($"The charge \"{ursCharge.UniversalCode} - {ursCharge.Name}\" has no currency for {nameof(ursCharge.RateCollections)}.{nameof(ursCharge.RateCollections.Items)}[0].{nameof(IRateCollectionDto.Currency)}");
			ErrorReporter.ReportMappingError(errorMessage, nameof(ConvertCharge), (nameof(tradeService), tradeService), (nameof(ursCharge), ursCharge));
			return (null, errorMessage);
		}

		(IEnumerable<Charge> charges, string error) GetFlatOrPerUnitCharges(ITradeServiceDto tradeService, IBaseChargeDto ursCharge, Charge charge, string currency)
		{
			var rateItems = ursCharge.RateCollections.Items.First();
			var rateItem = rateItems.PriceEntries.First();
			var charges = new List<Charge>();

			if (UrsRatesParseHelper.IsFlatPrice(rateItems.PriceEntries))
			{
				charge.FlatRate = rateItem.Price;
				charges.Add(charge);
			}
			else
			{
				var perUnitCharges = ConvertPerUnit(tradeService, ursCharge, currency);
				if (perUnitCharges.error != null)
				{
					return perUnitCharges;
				}
				charges.AddRange(perUnitCharges.charges);
			}

			return (charges, null);
		}

		(IEnumerable<Charge> charges, string error) ConvertPerUnit(ITradeServiceDto tradelane, IBaseChargeDto ursCharge, string currency)
		{
			var (pivotHandled, pivotCharges, pivotError) = HandlePivotWeightFlow(tradelane, ursCharge);
			if (pivotHandled)
			{
				return (pivotCharges, pivotError);
			}

			var charges = new List<Charge>();

			var rateItems = ursCharge.RateCollections.Items.First();
			var min = rateItems.PriceEntries.FirstOrDefault(e => e.BreakType == UrsConstants.RateBreakTypeCode.Min);
			var flt = rateItems.PriceEntries.FirstOrDefault(e => e.BreakType == UrsConstants.RateBreakTypeCode.Base);
			var max = rateItems.PriceEntries.FirstOrDefault(e => e.BreakType == UrsConstants.RateBreakTypeCode.Max);

			foreach (var item in rateItems.PriceEntries.Except(new[] { min, flt, max }))
			{
				if (item.BreakQuantity != Math.Truncate(item.BreakQuantity))
				{
					return (null, $"Charges with decimal breaks ({item.BreakQuantity}) are not supported at the moment");
				}

				var (unit, error) = UrsRatesParseHelper.GetUnit(item.PricingQuantityUnit);
				if (!string.IsNullOrEmpty(error))
				{
					return (charges, error);
				}

				var charge = CreateCharge(tradelane, ursCharge, ursCharge.ChargeDefinition, item);
				charge.MinRate = min?.Price;
				charge.FlatRate = flt?.Price;
				charge.MaxRate = max?.Price;
				charge.Unit = unit;
				charge.ActualPercentage = item.UseVolumetric ? null : 100;
				charge.UnitMultiplier = item.PricingQuantity;
				charge.Currency = currency;

				if (item.UseVolumetric && ursCharge.WmRatioCubicCentimeter > 0)
				{
					charge.ConversionFactor = ursCharge.WmRatioCubicCentimeter;
					charge.ConversionFactorUnit = "CC";
					charge.ConversionFactorDenominatorUnit = "KG";
				}

				if (tradelane.Container != null && ursCharge.ChargeDefinition.UniversalCode == UrsRatesParseHelper.FRTUniversalChargeCode)
				{
					// It is for over-pivot rate. In cases if the rate is per container but there is a price per each kilogram over a specific threshold.
					// So, EquipmentUnit is always container for rate with container.
					charge.EquipmentUnit = WRConstants.Units.CN;
				}

				var customFields = new List<CustomField>();
				customFields.AddRange(charge.ProviderCustomFields);
				customFields.Add(item.MeasurementPrecision, Rate.CustomFields.Common.Precision, (NoResString)"Precision");

				charge.ProviderCustomFields = customFields;

				switch (item.BreakType)
				{
					case UrsConstants.RateBreakTypeCode.Flat:
						break;
					case UrsConstants.RateBreakTypeCode.BreakLess:
						charge.Break = (int)item.BreakQuantity;
						charge.BreakOperator = "<";
						break;
					case UrsConstants.RateBreakTypeCode.BreakPlus:
						charge.Break = (int)item.BreakQuantity;
						charge.BreakOperator = ">=";
						break;
					default:
						return (null, $"Charges with break criteria '{item.BreakType}' are not supported at the moment");
				}

				charge.PerUnitRate = item.Price > 0 ? item.Price : null;

				if (charge.PerUnitRate == null)
				{
					// We still want to show restricted charge on UI (MMS for example) for references.
					// Set PerUnitRate to 0 for the charges to be resolved as a CMB calculator - with 0 price in the breaks.
					// Or, if the charge applicability is `Zero – costs nothing`, not necessarily is restricted,
					// it should be converted to a zero-rate UNT calculator.

					var zeroApplicable = item.Applicable == RateApplicableCode.Zero;
					if (zeroApplicable || charge.Restricted.GetValueOrDefault())
					{
						charge.PerUnitRate = 0;
					}

					if (zeroApplicable)
					{
						charge.FlatRate = null;
					}
				}

				charges.Add(charge);
			}

			FixBreaks(charges);

			return (charges, null);
		}

		(bool canHandle, IEnumerable<Charge> charges, string error) HandlePivotWeightFlow(ITradeServiceDto tradeService, IBaseChargeDto ursCharge)
		{
			var entries = ursCharge.RateCollections.Items.First().PriceEntries;
			if (entries.Count() != 2)
			{
				return (false, Array.Empty<Charge>(), null);
			}

			var baseEntry = entries.FirstOrDefault(e =>
				e.BreakType == UrsConstants.RateBreakTypeCode.Base &&
				e.QuantityUnit == UrsConstants.UnitOfMeasurementCode.Container);

			var pivotEntry = entries.FirstOrDefault(e =>
				e.BreakType == UrsConstants.RateBreakTypeCode.Pivot);

			if (baseEntry == null || pivotEntry == null)
			{
				return (false, Array.Empty<Charge>(), null);
			}

			var (unit, unitError) = UrsRatesParseHelper.GetUnit(pivotEntry.PricingQuantityUnit);
			if (!string.IsNullOrEmpty(unitError))
			{
				return (true, Array.Empty<Charge>(), unitError);
			}

			var rateItems = ursCharge.RateCollections.Items.First();
			var rateItem = rateItems.PriceEntries.First();

			var charge1 = CreateCharge(tradeService, ursCharge, ursCharge.ChargeDefinition, rateItem);
			charge1.PerUnitRate = baseEntry.Price;
			charge1.Currency = ursCharge.RateCollections.Items.First().Currency;
			charge1.Unit = RatingConstants.Units.CN;
			charge1.EquipmentUnit = RatingConstants.Units.CN;
			charge1.ActualPercentage = baseEntry.UseVolumetric ? null : 100;

			var charge2 = CreateCharge(tradeService, ursCharge, ursCharge.ChargeDefinition, rateItem);
			charge2.Break = (int)pivotEntry.BreakQuantity;
			charge2.BreakOperator = ">=";
			charge2.BreakUnit = unit;
			charge2.PerUnitRate = pivotEntry.Price;
			charge2.Currency = ursCharge.RateCollections.Items.First().Currency;
			charge2.Unit = unit;
			charge2.EquipmentUnit = RatingConstants.Units.CN;
			charge2.ActualPercentage = pivotEntry.UseVolumetric ? null : 100;

			return (true, new[] { charge1, charge2 }, null);
		}

		Charge CreateCharge(ITradeServiceDto tradeService, IBaseChargeDto ursCharge, IChargeDefinitionDto chargeDefinition, IUniversalRateEntryDto rateItem, bool isInclusive = false)
		{
			var origin = UrsRatesParseHelper.GetOriginCode(tradeService);
			var destination = UrsRatesParseHelper.GetDestinationCode(tradeService);

			var customFields = new List<CustomField>();
			customFields.Add($"{origin} -> {destination}", Rate.CustomFields.Common.Routing, (NoResString)"Routing");

			var transportMode = UrsRatesParseHelper.GetTransportMode(tradeService);
			var charge = new Charge
			{
				Restricted = UrsRatesParseHelper.IsRestricted(rateItem),
				Applicability = UrsRatesParseHelper.GetChargeApplicability(rateItem.Applicable),
				ChargeCode = chargeDefinition.UniversalCode,
				ChargeCodeInfo = new RefChargeCode
				{
					Code = chargeDefinition.UniversalCode,
					Description = chargeDefinition.Description
				},
				CarrierChargeCodeInfo = new CarrierSpecificChargeCode
				{
					Code = chargeDefinition.Code,
					Description = chargeDefinition.Description
				},
				CustomCategory = GetChargeCategory(tradeService),
				ProviderCustomFields = customFields,
				ChargeType = transportMode == WRConstants.TransportModes.SEA
					? GetChargeTypeForSea(chargeDefinition, tradeService, ursCharge.RateCollections, isInclusive)
					: GetChargeTypeForAir(chargeDefinition, tradeService, rateItem.Applicable, isInclusive),
			};
			charge.IsOptional = charge.ChargeType == ChargeType.Optional || charge.ChargeType == ChargeType.Additional || chargeDefinition.RequiresQuantifiedInput;
			if (charge.ChargeType == ChargeType.Bol)
			{
				charge.CustomCategory = WRConstants.ChargeCustomCategory.BOL;
			}

			if (rateItem.Applicable == RateApplicableCode.Percentage)
			{
				charge.Percentage = rateItem.Price;
				charge.PercentageAppliesTo = UrsRatesParseHelper.FRTUniversalChargeCode;
			}

			if (rateItem.Applicable == RateApplicableCode.Zero)
			{
				charge.FlatRate = 0; // Setting a flat rate, so that correct calculator can be resolved.
			}

			return charge;
		}

		ChargeType GetChargeTypeForAir(IChargeDefinitionDto chargeDefinition, ITradeServiceDto tradeService, string rateApplicable, bool isInclusive)
		{
			if (isInclusive)
			{
				return ChargeType.Included;
			}

			if (rateApplicable == RateApplicableCode.NotApplicable)
			{
				return ChargeType.NotApplicable;
			}

			if (chargeDefinition.UniversalCode == UrsRatesParseHelper.GetFreightChargeCode(tradeService))
			{
				return ChargeType.None;
			}

			return chargeDefinition.RequiresQuantifiedInput ? ChargeType.Optional : ChargeType.SubjectTo;
		}

		ChargeType GetChargeTypeForSea(IChargeDefinitionDto chargeDefinition, ITradeServiceDto tradeService, IRateCollectionDataDto rateCollections, bool isInclusive)
		{
			if (isInclusive)
			{
				return ChargeType.Included;
			}
			if (chargeDefinition.UniversalCode == UrsRatesParseHelper.GetFreightChargeCode(tradeService))
			{
				return ChargeType.Freight;
			}
			if (rateCollections?.Items != null && rateCollections.Items.Any())
			{
				foreach (var item in rateCollections.Items)
				{
					// Check Bol first: if any PriceEntries.quantityUnit = Bol, then it's a BOL. 
					if (item.PriceEntries != null && item.PriceEntries.Any(x => x.PricingQuantityUnit == "Bol"))
					{
						return ChargeType.Bol;
					}
				}
			}

			if (chargeDefinition.RequiresQuantifiedInput)
			{
				return ChargeType.Additional;
			}

			if (rateCollections?.Items != null && rateCollections.Items.Any())
			{
				foreach (var item in rateCollections.Items)
				{
					// If PriceEntries.quantityUnit != Bol(not just Cnt), then it's a Freight
					if (item.PriceEntries != null && item.PriceEntries.Any(x => x.PricingQuantityUnit != "Bol"))
					{
						return ChargeType.Freight;
					}
				}
			}

			return ChargeType.None;
		}

		/// <summary>
		/// Same logic as WiseRates CargoguideRatesConverter.FixBreaks. Assumes it is still needed.
		/// </summary>
		void FixBreaks(List<Charge> charges)
		{
			var breakLessCharges = charges
				.Where(x => x.BreakOperator == "<")
				.OrderBy(x => x.Break)
				.ToList();

			var biggestBreakLessValue = breakLessCharges.Max(x => x.Break) ?? 0;
			var smallestBreakMoreValue = charges
				.Where(x => x.BreakOperator == ">=")
				.Min(x => x.Break) ?? int.MaxValue;
			var hasAGap = biggestBreakLessValue < smallestBreakMoreValue;

			var prevBreakValue = 0;
			foreach (var breakLessCharge in breakLessCharges)
			{
				breakLessCharge.BreakOperator = ">=";

				if (prevBreakValue > smallestBreakMoreValue)
				{
					charges.Remove(breakLessCharge);
				}
				else if (hasAGap && breakLessCharge.Break == biggestBreakLessValue)
				{
					var callForPriceBreak = breakLessCharge.Clone() as Charge;
					callForPriceBreak.PerUnitRate = -1;
					charges.Add(callForPriceBreak);
				}

				var currentBreakValue = breakLessCharge.Break ?? 0;
				breakLessCharge.Break = prevBreakValue;
				prevBreakValue = currentBreakValue;
			}
		}

		static string GetChargeCategory(ITradeServiceDto tradeServiceDto)
		{
			switch (tradeServiceDto.ServiceClass)
			{
				case UrsConstants.ServiceClassCode.PreCarriage:
					return WRConstants.ChargeCustomCategory.Inland;
				case UrsConstants.ServiceClassCode.OnCarriage:
					return WRConstants.ChargeCustomCategory.Outland;
				default:
					return tradeServiceDto.ModesOfTransport.Items.Any(i => i.Code == UrsConstants.ModeOfTransport.Ocean)
						? WRConstants.ChargeCustomCategory.Ocean
						: string.Empty;
			}
		}

		void LogChargeDiscardReason(string id, string universalCode, string reason)
		{
			var msg = $"Charge {universalCode} from Rate '{id}' was ignored because {reason}";
			Logger?.Log(LogType.Warning, msg);
		}

		#endregion

		#region BookingInfo

		public static List<UrsBookingInfo> GetBookingInfo(ITradeServiceDto tradeService)
		{
			var schedules = GetSchedules(tradeService);
			var bookingTerms = GetBookingTerms(tradeService);
			var penalties = GetPenalties(tradeService);

			if (schedules.Count == 0)
			{
				return bookingTerms is not null || !penalties.IsNullOrEmpty()
					? [new UrsBookingInfo { BookingTerms = bookingTerms, Penalties = penalties }]
					: [];
			}

			return schedules.ConvertAll(schedule => new UrsBookingInfo
			{
				BookingTerms = bookingTerms,
				Penalties = penalties,
				Schedule = schedule,
			});
		}

		public static BookingTerms GetBookingTerms(ITradeServiceDto tradeService)
		{
			var termCharges = tradeService.PriceInfo?.Penalties?.Items;
			if (termCharges.IsNullOrEmpty())
			{
				return null;
			}

			return new BookingTerms
			{
				Items = termCharges
					.Select(charge =>
					{
						var collection = charge.RateCollections.Items.First();
						return new BookingTermItem
						{
							Currency = collection.Currency,
							Fee = collection.PriceEntries.First().Price,
							Name = charge.ChargeDefinition.Description,
						};
					})
					.ToArray()
			};
		}

		public static Penalty[] GetPenalties(ITradeServiceDto tradeService)
		{
			var penaltyCharges = tradeService.PriceInfo?.FreeTime?.Items;
			if (penaltyCharges.IsNullOrEmpty())
			{
				return [];
			}

			return penaltyCharges
				.Select(charge =>
				{
					var collection = charge.RateCollections.Items.First();
					// BookingInfo only displays the first non-free penalty tier.
					var priceEntries = collection.PriceEntries.Where(e => e.Price != 0).OrderBy(e => e.BreakQuantity).ToList();
					if (priceEntries.IsNullOrEmpty())
					{
						return null;
					}

					return new Penalty
					{
						Currency = collection.Currency,
						Direction = UrsRatesParseHelper.MapShippingPhase(charge.ChargeDefinition.ShippingPhase.Code),
						StartDay = (int)priceEntries[0].BreakQuantity,
						EndDay = priceEntries.Count > 1 ? (int)priceEntries[1].BreakQuantity - 1 : null,
						Name = charge.ChargeDefinition.Description,
						PerUnitRate = priceEntries[0].Price,
						Type = UrsRatesParseHelper.MapPenaltyType(charge.ChargeDefinition.Code),
					};
				})
				.WhereNotNull()
				.ToArray();
		}

		#region Schedule

		static (string origin, string destination) GetLocationOriginDestination(IScheduleLocationDataDto data)
		{
			var items = data.Items.ToList();
			return (items[0].Location, items[1].Location);
		}

		static DateTime GetDateTime(IScheduleArriveDepartDto dto) => dto.Date + dto.Time;
		static DateTime GetDateTime(IScheduleEventDto dto) => dto.Date + dto.Time;

		static DateTime? GetFirstEventDate(IScheduleSegmentDto dto, string code)
		{
			var matchingEvents = dto.Events.Items.Where(d => d.Code == code).OrderBy(d => d.Date)?.Select(d => d.Date);
			return matchingEvents.IsNullOrEmpty() ? null : matchingEvents.First();
		}

		static string GetTransportValue(IScheduleSegmentDto transport, string key) => transport.Transport.Description.GetValueOrDefault(key) as string;

		public static List<UrsSchedule> GetSchedules(ITradeServiceDto tradeService)
		{
			var schedules = tradeService.Schedules?.Items;
			if (schedules.IsNullOrEmpty())
			{
				return [];
			}

			return tradeService.Schedules.Items.Select(schedule =>
			{
				var firstSegment = schedule.Segments.Items.First();
				return new UrsSchedule
				{
					ExternalPriceReference = schedule.ExternalPriceReference,
					ScheduleId = Guid.NewGuid().ToString(),
					VesselName = GetTransportValue(firstSegment, ScheduleTransportValue.VesselName),
					VoyageNumber = GetTransportValue(firstSegment, ScheduleTransportValue.VoyageNumber),
					ArrivalDate = GetDateTime(schedule.TravelInfo.Arrival),
					DepartureDate = GetDateTime(schedule.TravelInfo.Departure),
					ScheduleDetails = schedule.Segments.Items
						.Select(item =>
						{
							var (origin, destination) = GetLocationOriginDestination(item.Location);
							return new ScheduleDetail
							{
								Origin = origin,
								Destination = destination,
								ArrivalDate = GetDateTime(item.TravelInfo.Arrival),
								DepartureDate = GetDateTime(item.TravelInfo.Departure),
								TransitTime = item.TravelInfo.TransitDuration,
								VesselName = GetTransportValue(item, ScheduleTransportValue.VesselName),
								VoyageNumber = GetTransportValue(item, ScheduleTransportValue.VoyageNumber),
								IMONumber = GetTransportValue(item, ScheduleTransportValue.ImoNumber),
								FlagCode = GetTransportValue(item, ScheduleTransportValue.FlagCode),
								ServiceCode = GetTransportValue(item, ScheduleTransportValue.ServiceCode),
								ServiceName = GetTransportValue(item, ScheduleTransportValue.ServiceName),
								TradeLane = GetTransportValue(item, ScheduleTransportValue.TradeLane),
								VGMCutOff = GetFirstEventDate(item, Constants.RouteOriginDetails.VGMCutOffKey),
								CTOCutOff = GetFirstEventDate(item, Constants.RouteOriginDetails.CTOCutOffKey),
								DocsDue = GetFirstEventDate(item, Constants.RouteOriginDetails.DocsDueKey),
								DateInfos = item.Events.Items
									.Select(e => new ScheduleDateInfo
									{
										Code = e.Code,
										Date = GetDateTime(e),
										Name = e.Name,
										Type = e.Type
									})
									.ToArray(),
							};
						})
						.ToArray()
				};
			}).ToList();
		}

		#endregion

		#endregion
	}
}
