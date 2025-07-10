using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Newtonsoft.Json.Linq;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRate = WiseRates.Api.Model.Rate;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// This class a utility class that converts a collection of IRateEntry objects to Rate objects.
	/// </summary>
	public class RateEntryToRateConverter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factory"></param>
		public RateEntryToRateConverter(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		/// <summary>
		/// Convert
		/// </summary>
		/// <param name="inputs"></param>
		/// <param name="rateType"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public IReadOnlyCollection<Rate> Convert(IEnumerable<IRateEntry> inputs, string rateType, ILogger logger)
		{
			var result = new List<Rate>();

			foreach (var item in inputs)
			{
				result.Add(Convert(item, rateType, logger));
			}

			return result.AsReadOnly();
		}

		/// <summary>
		/// This method converts one RateEntry to its equivalent Rate object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="rateType"></param>
		/// <param name="logger"></param>
		/// <param name="ratingAdapter"></param>
		/// <param name="autoRateInfos"></param>
		/// <returns></returns>
		public Rate Convert(IRateEntry entry, string rateType, ILogger logger, RateQueryRatingAdapter ratingAdapter = null, IEnumerable<AutoRateInfo> autoRateInfos = null)
		{
			if (!entry.IsValidRate())
			{
				logger.Log(LogType.Warning, "An entry returned from 'Rate Service' could not be considered as a valid rate. Reason: " + entry.InvalidReason);
			}

			var rate = new Rate()
			{
				TransportMode = GetTransportModeFromMode(entry.TI_Mode),
				ContainerMode = GetContainerModeFromModeAndCategory(entry.TI_RateCategory, entry.TI_Mode),
				Origin = ConvertLocation(entry.Origin()),
				Destination = ConvertLocation(entry.Destination()),
				RateOrigin = ConvertLocation(entry.RateOrigin()),
				RateDestination = ConvertLocation(entry.RateDestination()),
				PlannedLoad = ConvertLocation(entry.PlannedLoad()),
				PlannedDischarge = ConvertLocation(entry.PlannedDischarge()),
				Via = ConvertLocation(entry.Via()),
				Locations = ConvertMatchingLocations(entry),
				ServiceProvider = ConvertServiceProvider(entry),
				ControllingCustomer = entry.ControllingCustomer?.OH_Code,
				Consignee = entry.Consignee?.OH_Code,
				Consignor = entry.Consignor?.OH_Code,
				CarrierContract = rateType == RatingConstants.RatingHeaderTypes.Costing ? entry.TI_ContractNumber : null,
				ClientContract = rateType != RatingConstants.RatingHeaderTypes.Costing ? entry.TI_ContractNumber : null,
				CarrierServiceLevel = ConvertCarrierServiceLevel(entry),
				ServiceLevel = entry.TI_RS_NKServiceLevel_NI,
				GatewayServiceLevel = entry.TI_RS_NKGatewayServiceLevel,
				ShipmentGatewayServiceLevel = entry.TI_RS_NKShipmentGatewayServiceLevel,
				ContainerType = ConvertContainerType(entry),
				GWAgentType = entry.TI_GatewayAgentType,
				Commodity = ConvertCommodity(entry),
				TransitTime = entry.TI_TransitTime,
				Frequency = entry.TI_Frequency,
				FrequencyUnit = entry.TI_FrequencyUnit,
				StartDate = entry.TI_RateStartDate.ToISO8601ShortDateString(),
				ExpiryDate = entry.TI_RateEndDate.ToISO8601ShortDateString(),
				PayTermOverride = entry is WiseEntry ? null : entry.TI_PaymentTerm,
				AircraftType = entry.TI_AircraftType,
				RateProvider = GetRateProvider(entry),
				IsCWGlobal = entry.IsGlobal(),
				CargoSphere = ConvertCargoSphereInformation(entry),
				CargoGuide = ConvertCargoGuideInformation(entry),
				Charges = ConvertCharges(entry, rateType, ratingAdapter, autoRateInfos, logger),
				DestPostCode = entry.TI_CartageDeliveryAddressPostCode,
				OriginPostCode = entry.TI_CartagePickupAddressPostCode,
				IsCntrClassMatch = entry.TI_MatchContainerRateClass,
				IsCrossTrade = entry.IsCrossTrade(),
				ExcludeAutorate = entry.TI_IsExcludedFromAutoRating,
				SHPConsolStatus = entry.TI_ShipmentConsolidationStatus,
				HBLDeliveryMode = entry.TI_HBLDeliveryMode,
				FMCTariffID = entry.TI_FMCTariffID
			};

			if (autoRateInfos != null)
			{
				rate.RateModule = ConvertRateModule(entry);
				rate.RateParty = ConvertRateParty(entry);
			}

			if (entry.IsOriginEntry())
			{
				rate.PortTrpAddr = entry.CartagePickupAddressOverride?.AddressCode;
			}
			else if (entry.IsDestinationEntry())
			{
				rate.PortTrpAddr = entry.CartageDeliveryAddressOverride?.AddressCode;
			}

			if (entry is RateEntry re)
			{
				rate.Carrier = re.TransportProvider?.OH_Code;
				rate.ContainerClass = re.ContainerClass;
				rate.CreationSource = re.TI_CreationSource;
				rate.CarrierCode = re.TransportProviderCarrierCode;
				rate.ContractNumberLinked = re.TI_ContractNumberLinked;
			}

			return rate;
		}

		ChargeInfo[] ConvertCharges(IRateEntry input, string rateType, RateQueryRatingAdapter ratingAdapter, IEnumerable<AutoRateInfo> autoRateInfos, ILogger logger)
		{
			var result = new List<ChargeInfo>();

			if (input.ChildRateLines != null)
			{
				foreach (var item in input.ChildRateLines)
				{
					AutoRateInfo autoRateInfo = null;

					if (autoRateInfos != null)
					{
						autoRateInfo = autoRateInfos.SingleOrDefault(a => a.Line == item);
						if (autoRateInfo == null)
						{
							// When autoRateInfos is provided, we only convert those charges that has calculation result. 
							continue;
						}
					}

					var chargeCode = GetRateLineChargeCode(item);

					var chargeInfo = new ChargeInfo()
					{
						Calculators = ConvertCalculators(item, rateType, logger),
						ChargeCode = new ChargeCodeInfo()
						{
							CWCode = chargeCode?.AC_Code,
							UniversalCodes =
								chargeCode?
								.UniversalChargeCodeMappingsCollection
								.Select(uc => uc.AUP_Code.ToString())
								.ToArray(),
						},
						ChargeGroup = chargeCode?.AC_ChargeGroup,
						ChargeSubGroup = chargeCode?.AC_ChargeSubGroup,
						ChargeDesc = chargeCode?.AC_DescMultilingual,
						ConversionFactor = new ConversionFactor()
						{
							Factor = item.ConversionFactor.Factor.ToString(),
							NumeratorUnit = item.ConversionFactor.NumeratorUnit,
							DenominatorUnit = item.ConversionFactor.DenominatorUnit
						},
						Unit = item.TL_WeightVolume,
						Currency = item.TL_RX_NKCurrency,
						IsOptional = false,
						Rounding = item.TL_Rounding,
						RoundingFactor = item.RoundingFactor,
						ActualPercentage = item.TL_ActualPercentage,
						ActualWgtVolOnly = item.UseOnlyActualWeightMeasure(),
						PublicNote = item.ChargeInformationNoteText,
						ContainerOwnership = item.TL_ContainerOwnership,
						StartDate = item.TL_RateStartDate.ToISO8601ShortDateString(),
						EndDate = item.TL_RateEndDate.ToISO8601ShortDateString(),
						UnitFactor = item.TL_UnitFactor,
						Condition = item.TL_Condition,
						ConditionExp = item.TL_ConditionalExpression,
						IsWhsJobLevelCharge = item.TL_IsWhsJobLevelCharge,
						IsOnPallets = item.TL_IsOnPallets,
						FeeChargeLevel = item.TL_FeeChargeLevel,
						FeeChargeType = item.TL_FeeChargeType,
						CompanyTariffLevel = item.TL_CompanyTariffLevel,
						UnitMultiple = item.UnitMultipleAsString,
						ChargeLocalDesc = item.TL_RateDescLocal,
						OverriddenChargeDesc = item.TL_RateDesc
					};

					if (item is RateLine rateLine)
					{
						chargeInfo.InternalNote = rateLine.ChargeInternalNoteText;
						chargeInfo.IsChargeDescOverride = rateLine.OverrideChargeDescription;
						chargeInfo.ConditionExpDesc = rateLine.TL_ConditionalExpressionDescription;
					}

					if (autoRateInfo != null)
					{
						var calculation = GetCalculation(ratingAdapter, autoRateInfo);

						chargeInfo.Cost = autoRateInfo.IsCost ? calculation : null;
						chargeInfo.Revenue = autoRateInfo.IsCost ? null : calculation;
						chargeInfo.IncotermPayer = autoRateInfo.IsSell ? ConvertIncotermPayer(ratingAdapter, item) : null;
					}

					if (item is WiseLine wiseLine)
					{
						chargeInfo.IsOptional = wiseLine.WiseCharge?.IsOptional ?? false;

						if
							(
								!(chargeInfo.ChargeCode.UniversalCodes?.Any() ?? false)
								&& !string.IsNullOrEmpty(wiseLine.WiseCharge?.ChargeCode)
							)
						{
							chargeInfo.ChargeCode.UniversalCodes = new[] { wiseLine.WiseCharge.ChargeCode };
						}
					}

					result.Add(chargeInfo);
				}
			}

			return result.ToArray();
		}

		CalculationItem GetCalculation(RateQueryRatingAdapter ratingAdapter, AutoRateInfo autoRateInfo)
		{
			var result = new CalculationItem()
			{
				Amount = autoRateInfo.Amount,
				Currency = autoRateInfo.Currency,
				LocalCurrency = autoRateInfo.LocalCurrency,
				Audit = autoRateInfo.SingleLineDescription
			};

			if (autoRateInfo.Currency != autoRateInfo.LocalCurrency)
			{
				var job = ratingAdapter.Job as Job;

				var charge = job.Charges.AddNew();
				job.SetAmountsOnCharge(charge, autoRateInfo, autoRateInfo.IsCost ? CostSell.Cost : CostSell.Revenue, canUpdateCreditor: false);

				result.LocalAmount = autoRateInfo.IsCost ? charge.JR_LocalCostAmt : charge.JR_LocalSellAmt;
				result.ExchangeRate = autoRateInfo.IsCost ? charge.JR_OSCostExRate : charge.JR_OSSellExRate;
			}
			else
			{
				result.LocalAmount = result.Amount;
				result.ExchangeRate = 1;
			}

			return result;
		}

		AccChargeCode GetRateLineChargeCode(IRateLine rateLine)
		{
			if (rateLine.ChargeCode != null)
			{
				return rateLine.ChargeCode;
			}

			if (rateLine is WiseLine wiseLine)
			{
				var (charge, _) = WiseRatesConverter.ConvertChargeCode(wiseLine.WiseCharge.ChargeCode, factory);
				return charge;
			}

			return null;
		}

		CalculatorInfo[] ConvertCalculators(IRateLine input, string rateType, ILogger logger)
		{
			var converter = new CalculatorToCalculatorInfoConverter(factory);
			return converter.Convert(input, rateType, logger);
		}

		string ConvertIncotermPayer(RateQueryRatingAdapter ratingAdapter, IRateLine rateLine)
		{
			var paymentTerm = ratingAdapter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
			var incotermPayer = "";

			if (paymentTerm != null && !string.IsNullOrEmpty(paymentTerm.Value))
			{
				var incotermRegistry = RatingDataRegistry.Instance.IncoTermDefinition.Value
					.Cast<IncoTermChargeCodes>()
					.FirstOrDefault(incotermChargeCode => incotermChargeCode.IncoTerm == paymentTerm.Value);

				if (incotermRegistry != null)
				{
					switch (rateLine.ChargeCode?.AC_ChargeGroup)
					{
						case ChargeCodeGroupList.Codes.Brokerage:
							incotermPayer = incotermRegistry.Brokerage;
							break;
						case ChargeCodeGroupList.Codes.Origin:
							incotermPayer = incotermRegistry.Origin;
							break;
						case ChargeCodeGroupList.Codes.Loading:
							incotermPayer = incotermRegistry.Loading;
							break;
						case ChargeCodeGroupList.Codes.Freight:
							incotermPayer = incotermRegistry.Freight;
							break;
						case ChargeCodeGroupList.Codes.Insurance:
							incotermPayer = incotermRegistry.Insurance;
							break;
						case ChargeCodeGroupList.Codes.Unloading:
							incotermPayer = incotermRegistry.Unloading;
							break;
						case ChargeCodeGroupList.Codes.Destination:
							incotermPayer = incotermRegistry.Destination;
							break;
						case ChargeCodeGroupList.Codes.CustomsDuty:
							incotermPayer = incotermRegistry.CustomsDuty;
							break;
					}
				}
			}

			return incotermPayer;
		}

		string ConvertRateModule(IRateEntry entry)
		{
			var rateModule = "";
			if (entry.IsClientRate())
			{
				rateModule = "CLR";
			}
			else if (entry.IsCosting())
			{
				rateModule = "CST";
			}
			else if (entry.IsCompanyTariff())
			{
				rateModule = "CTR";
			}
			else if (entry.IsIntercompanyTariff())
			{
				rateModule = "ICT";
			}
			return rateModule;
		}

		string ConvertRateParty(IRateEntry entry)
		{
			if (entry.IsCompanyTariff())
			{
				return entry.ParentRatingHeader.Company != null ? entry.ParentRatingHeader.Company.OrgProxy.OH_Code : ZString.Empty;
			}
			else
			{
				return entry.ParentRatingHeader?.Header?.OH_Code;
			}
		}

		Location ConvertLocation(ILocation location, string relatedField = null)
		{
			if (location.IsCountry())
			{
				return new Location()
				{
					Type = Location.Types.Country,
					Value = location.Code,
					RelatedField = relatedField
				};
			}

			if (location.IsCity())
			{
				return new Location()
				{
					Type = Location.Types.City,
					Value = location.Code,
					RelatedField = relatedField
				};
			}

			if (location.IsZone())
			{
				return new Location()
				{
					Type = Location.Types.Zone,
					Value = location.Code,
					RelatedField = relatedField
				};
			}

			if (location.IsUNLOCO())
			{
				return new Location()
				{
					Type = Location.Types.UNLOCO,
					Value = location.Code,
					RelatedField = relatedField
				};
			}

			return null;
		}

		Location[] ConvertMatchingLocations(IRateEntry input)
		{
			return new[]
			{
				ConvertLocation(input.FirstLoad(), RateEntryLookups.LocationSourceOption.FirstLoad.Code),
				ConvertLocation(input.LastDischarge(), RateEntryLookups.LocationSourceOption.LastDischarge.Code),
				ConvertLocation(input.FirstRouteSetLoad(), RateEntryLookups.LocationSourceOption.FirstRouteSetLoad.Code),
				ConvertLocation(input.LastRouteSetDischarge(), RateEntryLookups.LocationSourceOption.LastRouteSetDischarge.Code),
			}.Where(location => location != null).ToArray();
		}

		Organisation ConvertServiceProvider(IRateEntry input)
		{
			if ((input.IsCosting() || input.IsIntercompanyTariff()) && !input.ServiceProviderPK().IsEmpty)
			{
				var serviceProvider = factory.Load<OrgHeader>(input.ServiceProviderPK());

				var result = new Organisation()
				{
					CWCode = serviceProvider.OH_Code,
					IATACode = serviceProvider.MiscServ?.Airline?.RM_TwoCharacterCode,
					SCAC = serviceProvider.ShippingLine?.RSL_StandardCarrierAlphaCode,
					C1CCode = serviceProvider.ShippingLine?.RSL_CargoWiseOneCode
				};

				return result;
			}

			if (input.IsClientRate() && !input.TI_OH_Supplier.IsEmpty)
			{
				var serviceProvider = input.Supplier;

				var result = new Organisation()
				{
					CWCode = serviceProvider.OH_Code,
					IATACode = serviceProvider.MiscServ?.Airline?.RM_TwoCharacterCode,
					SCAC = serviceProvider.ShippingLine?.RSL_StandardCarrierAlphaCode,
					C1CCode = serviceProvider.ShippingLine?.RSL_CargoWiseOneCode
				};

				return result;
			}

			if (input is WiseEntry wiseEntry && wiseEntry.ParentRatingHeader is WiseHeader wiseHeader && wiseHeader.WiseCarrier != null)
			{
				var result = new Organisation()
				{
					CWCode = null,
					IATACode = wiseHeader.WiseCarrier.IATACode,
					SCAC = wiseHeader.WiseCarrier.SCACCode,
					C1CCode = wiseHeader.WiseCarrier.C1Code
				};

				return result;
			}

			return null;
		}

		CarrierServiceLevel ConvertCarrierServiceLevel(IRateEntry input)
		{
			if (!input.TI_PL_NKCarrierServiceLevel.IsEmpty)
			{
				return new CarrierServiceLevel()
				{
					Type = CarrierServiceLevel.Types.CargoWise,
					Value = input.TI_PL_NKCarrierServiceLevel
				};
			}
			else if (input is WiseEntry wiseEntry)
			{
				if (!string.IsNullOrEmpty(wiseEntry.WiseRate?.ServiceLevel))
				{
					return new CarrierServiceLevel()
					{
						Type = CarrierServiceLevel.Types.Universal,
						Value = wiseEntry.WiseRate.ServiceLevel,
					};
				}
			}

			return null;
		}

		ContainerType ConvertContainerType(IRateEntry input)
		{
			if (input.Container != null)
			{
				return new ContainerType()
				{
					Type = ContainerType.Types.CargoWise,
					Value = input.Container.RC_Code
				};
			}

			return null;
		}

		CommodityInfo ConvertCommodity(IRateEntry input)
		{
			if (!input.TI_RH_NKCommodityCode.IsEmpty)
			{
				var ci = new CommodityInfo()
				{
					Type = CommodityInfo.Types.CargoWise,
					Value = input.TI_RH_NKCommodityCode
				};

				if (input is RateEntry re)
				{
					ci.Description = re.CommodityCode?.RH_DescriptionMultilingual;
				}

				return ci;
			}
			else if (input is WiseEntry wiseEntry)
			{
				return new CommodityInfo()
				{
					Type = CommodityInfo.Types.UniversalCommodityGroup,
					Value = wiseEntry.WiseRate.Commodity
				};
			}

			return null;
		}

		CSRateInfo ConvertCargoSphereInformation(IRateEntry input)
		{
			if (input is WiseEntry wiseEntry)
			{
				if (WRConstants.RateProviders.CargoSphere.Equals(wiseEntry.RateProvider))
				{
					var result = new CSRateInfo()
					{
						AddOn = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.CargoSphere.ArbitraryPermission)?.ToString(),
						Commodity = input.CommodityGroup,
						NamedAccounts = input.NamedAccounts?.ToArray(),
						RateType = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.CargoSphere.RateType)?.ToString(),
						RateType2 = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.CargoSphere.RateType2)?.ToString(),
						Routing = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Common.Routing)?.ToString(),
						ServiceString = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.CargoSphere.ServiceString)?.ToString(),
						TradeLane = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.CargoSphere.TradeLane)?.ToString(),
						Vessel = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.CargoSphere.Vessel)?.ToString(),
					};

					return result;
				}
			}

			return null;
		}

		CGRateInfo ConvertCargoGuideInformation(IRateEntry input)
		{
			if (input is WiseEntry wiseEntry)
			{
				if (WRConstants.RateProviders.CargoGuide.Equals(wiseEntry.RateProvider))
				{
					string issueDate = null;
					if (wiseEntry.WiseRate.IssueDate != default)
					{
						issueDate = new ZDateTime(wiseEntry.WiseRate.IssueDate).ToISO8601ShortDateString();
					}

					string[] via = null;
					if (wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.Via) is JArray viaArray)
					{
						via = viaArray.Where(t => t.Type == JTokenType.String).Select(t => t.ToString()).ToArray();
					}

					bool? cargoAircraftOnly = null;
					if (wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.CargoAircraftOnly) is bool customFieldCargoAirCraftOnly)
					{
						cargoAircraftOnly = customFieldCargoAirCraftOnly;
					}

					TemperatureRange temperatureRange = null;
					if (wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.ProductTemperatureRange) is TemperatureRange productTemperatureRange)
					{
						temperatureRange = productTemperatureRange;
					}

					var result = new CGRateInfo()
					{
						GSAName = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.CarrierGSAName)?.ToString(),
						NamedAccounts = input.NamedAccounts?.ToArray(),
						Deck = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.DeckType)?.ToString(),
						IssueDate = issueDate,
						Origin = wiseEntry.TI_OriginLRC,
						OriginName = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.OriginName)?.ToString(),
						OriginCity = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.OriginCity)?.ToString(),
						OriginCityName = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.OriginCityName)?.ToString(),
						POL = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.PortOfLoading)?.ToString(),
						Destination = wiseEntry.TI_DestinationLRC,
						DestinationName = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.DestinationName)?.ToString(),
						DestinationCity = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.DestinationCity)?.ToString(),
						DestinationCityName = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.DestinationCityName)?.ToString(),
						POD = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.PortOfDischarge)?.ToString(),
						PaymentTerm = wiseEntry.TI_PaymentTerm,
						ProductClass = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.ProductClass)?.ToString(),
						ProductCode = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.ProductCode)?.ToString(),
						ProductId = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.ProductId)?.ToString(),
						ProductName = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.ProductName)?.ToString(),
						RateClass = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.RateClass)?.ToString(),
						Ratio = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.Ratio)?.ToString(),
						Reference = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.Reference)?.ToString(),
						Remarks = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.Remarks)?.ToString(),
						Via = via,
						ProductDeck = wiseEntry.GetCustomFieldValue(WiseRate.CustomFields.Cargoguide.ProductDeckType)?.ToString(),
						CargoAircraftOnly = cargoAircraftOnly,
						TemperatureRange = temperatureRange
					};

					return result;
				}
			}

			return null;
		}

		string GetRateProvider(IRateEntry input)
		{
			var result = "CW";
			if (input is WiseEntry)
			{
				result = input.RateProvider;
			}

			return result;
		}

		string GetTransportModeFromMode(string mode)
		{
			if (Core.Constants.RateMode.ALL.Equals(mode, System.StringComparison.InvariantCultureIgnoreCase))
			{
				return Core.Constants.TransportModes.All;
			}
			else
			{
				return RatingConstants.GetTransportModeFromMode(mode);
			}
		}

		string GetContainerModeFromModeAndCategory(string category, string mode)
		{
			switch (mode)
			{
				case "":
					return "";
				case Core.Constants.RateMode.LSE:
					return Core.Constants.ContainerModes.Loose;

				case Core.Constants.RateMode.ULD:
					return Core.Constants.ContainerModes.ULD;

				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FWL:
					return Core.Constants.ContainerModes.LCL;

				case Core.Constants.RateMode.FTL:
					return Core.Constants.ContainerModes.FTL;

				case Core.Constants.RateMode.FCL:
				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.FRA:
					return Core.Constants.ContainerModes.FCL;

				case Core.Constants.RateMode.COU:
					return Core.Constants.ContainerModes.OnBoardCourier;

				case Core.Constants.RateMode.ALL:
				case Core.Constants.RateMode.AIR:
				case Core.Constants.RateMode.SEA:
				case Core.Constants.RateMode.RAI:
				case Core.Constants.RateMode.ROA:
					switch (category)
					{
						case RatingConstants.RateCategory.FCL:
							return Core.Constants.ContainerModes.FCL;
						case RatingConstants.RateCategory.LCL:
							return Core.Constants.ContainerModes.LCL;
						default:
							return "";
					}
				default:
					return Core.Constants.ContainerModes.Other;
			}
		}
	}

	internal static class ILocationExtentions
	{
		public static bool IsCountry(this ILocation location)
		{
			return location?.Code.Length == 2;
		}

		public static bool IsCity(this ILocation location)
		{
			return location?.Code.Length == 3;
		}

		public static bool IsZone(this ILocation location)
		{
			return location?.Code.Length == 4;
		}

		public static bool IsUNLOCO(this ILocation location)
		{
			return location?.Code.Length == 5;
		}
	}
}
