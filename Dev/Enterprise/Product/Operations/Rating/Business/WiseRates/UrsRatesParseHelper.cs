using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.Business
{
	public static class UrsRatesParseHelper
	{
		public const string FRTUniversalChargeCode = "FRT";

		#region Transport Mode

		public static string GetTransportMode(ITradeServiceDto tradelane)
		{
			var allModes = tradelane?.ModesOfTransport?.Items;
			if (allModes == null)
			{
				return string.Empty;
			}

			var mainMode = allModes
				.Select(x => ConvertUrsTransportMode(x.Code))
				.DefaultIfEmpty()
				.Max();

			return mainMode switch
			{
				RankedTransportMode.Air => WRConstants.TransportModes.AIR,
				RankedTransportMode.Sea => WRConstants.TransportModes.SEA,
				RankedTransportMode.Road => WRConstants.TransportModes.ROA,
				RankedTransportMode.Rail => WRConstants.TransportModes.RAI,
				_ => string.Empty
			};
		}

		static RankedTransportMode ConvertUrsTransportMode(string ursMode)
		{
			switch (ursMode)
			{
				case UrsConstants.ModeOfTransport.Ocean:
				case UrsConstants.ModeOfTransport.ShortSea:
				case UrsConstants.ModeOfTransport.InlandNavigation:
					return RankedTransportMode.Sea;

				case UrsConstants.ModeOfTransport.Air:
					return RankedTransportMode.Air;

				case UrsConstants.ModeOfTransport.Road:
					return RankedTransportMode.Road;

				case UrsConstants.ModeOfTransport.Rail:
					return RankedTransportMode.Rail;

				default:
					return RankedTransportMode.None;
			}
		}

		/// <summary>
		/// Highest number determines the overall mode, when the rate has multiple modes.
		/// </summary>
		enum RankedTransportMode
		{
			None = 0,
			Rail = 1,
			Road = 2,
			Sea = 3,
			Air = 4,
		}

		#endregion

		#region Routing

		public static string GetOriginCode(ITradeServiceDto tradelane)
			=> GetFirstWaypointCodeByType(tradelane, UrsConstants.RouteWaypointCode.Origin);

		public static string GetDestinationCode(ITradeServiceDto tradelane)
			=> GetFirstWaypointCodeByType(tradelane, UrsConstants.RouteWaypointCode.Destination);

		public static string GetViaCode(ITradeServiceDto tradelane)
			=> GetFirstWaypointCodeByType(tradelane, UrsConstants.RouteWaypointCode.Via);

		public static string GetFirstWaypointCodeByType(ITradeServiceDto tradelane, string waypointType)
			=> GetFirstWaypointByType(tradelane, waypointType)?.Location?.Code;

		public static IGeoScopeEntryDto GetFirstWaypointByType(ITradeServiceDto tradelane, string waypointType)
			=> tradelane.RouteInfo?.Waypoints?.FirstOrDefault(x => x.WaypointType == waypointType);

		#endregion

		#region Service Level

		public static (ZString code, string error) ConvertServiceLevel(ITradeServiceDto tradeService, OrgHeader carrier)
		{
			if (carrier == null)
			{
				return (ZString.Empty, null);
			}
			// Try matching tradeService.Product.Code first, since it takes preference over a match by universal service level...
			var productCode = tradeService.Product?.Code;
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

			var serviceLevel = GetUniversalServiceLevel(tradeService);
			var carrierServiceLevels = carrier.MiscServ.CarrierServiceLevels
				.Cast<OrgCarrierServiceLevel>()
				.Where(l => l.CarrierServiceCodes.Any(c => c.EqualsIgnoringCase(serviceLevel)))
				.ToList();
			if (carrierServiceLevels.Count > 1)
			{
				return (ZString.Empty, Res.GetString("564d1181-f760-492b-be17-8589010ac65a", "Service Code '{0}' under Carrier '{1}' has been duplicated and must be unique.", serviceLevel, carrier.OH_Code));
			}

			if (carrierServiceLevels.Count == 1)
			{
				return (carrierServiceLevels[0].PL_Code, null);
			}

			var carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().FirstOrDefault(l => l.PL_Code == serviceLevel);
			return carrierServiceLevel != null
				? (serviceLevel, null)
				: (ZString.Empty, Res.GetString("e63cfada-a6f3-4fdb-aa40-2a359c721802", "No Carrier Service Level under Carrier '{0}' is assigned to '{1}'", carrier.OH_Code, serviceLevel));
		}

		public static string GetUniversalServiceLevel(ITradeServiceDto tradelane)
		{
			var transportMode = GetTransportMode(tradelane);
			if (transportMode == WRConstants.TransportModes.SEA)
			{
				//for ocean rates, we will get voyageInfo.oceanRoutingTerm
				var oceanRoutingTerm = tradelane.VoyageInfo?.OceanRoutingTerm;
				if (!string.IsNullOrEmpty(oceanRoutingTerm))
				{
					return oceanRoutingTerm.ToUpperInvariant();
				}
				return string.Empty;
			}

			var ursCode = tradelane.Product?.ServiceLevel?.Code;
			if (string.IsNullOrEmpty(ursCode) || ursCode == UrsConstants.None)
			{
				return string.Empty;
			}
			else
			{
				return ursCode.ToUpperInvariant();
			}
		}

		#endregion

		#region Freight Charge Code

		public static string GetFreightChargeCode(ITradeServiceDto tradeServiceDto) =>
			tradeServiceDto.ServiceClass switch
			{
				ServiceClassCode.PreCarriage => "OFRT",
				ServiceClassCode.OnCarriage => "DFRT",
				_ => FRTUniversalChargeCode,
			};

		#endregion

		#region Unit

		public static readonly Dictionary<string, string> unitsMapping = new Dictionary<string, string>
		{
			// Length
			{ UrsConstants.UnitOfMeasurementCode.Centimeter, WRConstants.Units.Length.Centimetres },
			{ UrsConstants.UnitOfMeasurementCode.Foot, WRConstants.Units.Length.Feet },
			{ UrsConstants.UnitOfMeasurementCode.Inch, WRConstants.Units.Length.Inches },
			{ UrsConstants.UnitOfMeasurementCode.Km, WRConstants.Units.Length.Kilometres },
			{ UrsConstants.UnitOfMeasurementCode.Meter, WRConstants.Units.Length.Metres },
			{ UrsConstants.UnitOfMeasurementCode.Mile, WRConstants.Units.Length.Miles },
			{ UrsConstants.UnitOfMeasurementCode.Millimeter, WRConstants.Units.Length.Millimetres },

			// Temperature
			{ UrsConstants.UnitOfMeasurementCode.Celsius, null },
			{ UrsConstants.UnitOfMeasurementCode.Fahrenheit, null },

			// Time
			{ UrsConstants.UnitOfMeasurementCode.Day, WRConstants.Units.Time.Days },
			{ UrsConstants.UnitOfMeasurementCode.Fortnight, null },
			{ UrsConstants.UnitOfMeasurementCode.Hour, WRConstants.Units.Time.Hours },
			{ UrsConstants.UnitOfMeasurementCode.Month, null },
			{ UrsConstants.UnitOfMeasurementCode.Week, WRConstants.Units.Time.Weeks },
			{ UrsConstants.UnitOfMeasurementCode.Year, null },
			{ UrsConstants.UnitOfMeasurementCode.WorkingDay, null },
			{ UrsConstants.UnitOfMeasurementCode.DoIPlusWorkingDay, null },

			// Weight
			{ UrsConstants.UnitOfMeasurementCode.Gram, WRConstants.Units.Weight.Grams },
			{ UrsConstants.UnitOfMeasurementCode.Kilogram, WRConstants.Units.Weight.Kilograms },
			{ UrsConstants.UnitOfMeasurementCode.KilogramVerbose, WRConstants.Units.Weight.Kilograms },
			{ UrsConstants.UnitOfMeasurementCode.Ounce, WRConstants.Units.Weight.Ounces },
			{ UrsConstants.UnitOfMeasurementCode.Pound, WRConstants.Units.Weight.Pounds },
			{ UrsConstants.UnitOfMeasurementCode.MetricTon, WRConstants.Units.Weight.Tonnes },
			{ UrsConstants.UnitOfMeasurementCode.LongTonUS, WRConstants.Units.Weight.LongTons },

			// Volume
			{ UrsConstants.UnitOfMeasurementCode.CubicCentimeter, WRConstants.Units.Volume.CubicCentimeters },
			{ UrsConstants.UnitOfMeasurementCode.CubicFoot, WRConstants.Units.Volume.CubicFeet },
			{ UrsConstants.UnitOfMeasurementCode.CubicInch, WRConstants.Units.Volume.CubicInches },
			{ UrsConstants.UnitOfMeasurementCode.CubicMeter, WRConstants.Units.Volume.CubicMetres },
			{ UrsConstants.UnitOfMeasurementCode.CubicMeterVerbose, WRConstants.Units.Volume.CubicMetres },
			{ UrsConstants.UnitOfMeasurementCode.CubicMillimeter, null },

			// Other
			{ UrsConstants.UnitOfMeasurementCode.BillOfLading, string.Empty },
			{ UrsConstants.UnitOfMeasurementCode.Container, WRConstants.Units.CN },
			{ UrsConstants.UnitOfMeasurementCode.ShipmentValue, null },
			{ UrsConstants.UnitOfMeasurementCode.Case, null },
			{ UrsConstants.UnitOfMeasurementCode.Declaration, null },
			{ UrsConstants.UnitOfMeasurementCode.Document, null },
			{ UrsConstants.UnitOfMeasurementCode.Hawb, WRConstants.Units.HouseOfBill },
			{ UrsConstants.UnitOfMeasurementCode.Mawb, string.Empty },
			{ UrsConstants.UnitOfMeasurementCode.Object, null },
			{ UrsConstants.UnitOfMeasurementCode.Package, WRConstants.Units.PkgUnit.Package },
			{ UrsConstants.UnitOfMeasurementCode.Percentage, string.Empty },
			{ UrsConstants.UnitOfMeasurementCode.Shipment, WRConstants.Units.HouseOfBill },
			{ UrsConstants.UnitOfMeasurementCode.Ston, null },
			{ UrsConstants.UnitOfMeasurementCode.Teu, QuantityUnit.TU },
			{ UrsConstants.UnitOfMeasurementCode.DangerousPackage, null },
			{ UrsConstants.UnitOfMeasurementCode.HsCode, null },
			{ UrsConstants.UnitOfMeasurementCode.UNnumber, null },
			{ UrsConstants.UnitOfMeasurementCode.None, string.Empty },
		};

		public static (string unit, string error) GetUnit(string rateUnit)
		{
			if (unitsMapping.TryGetValue(rateUnit, out var unit) && unit != null)
			{
				return (unit, null);
			}

			return (null, $"Rate Unit '{rateUnit}' is not supported");
		}

		#endregion

		#region Carrier

		public static string GetCarrier(ITransportProviderDto transportProvider)
		{
			if (!string.IsNullOrEmpty(transportProvider.Carrier?.Code))
			{
				return transportProvider.Carrier?.Code;
			}

			return transportProvider.Code;
		}

		#endregion

		#region Named Accounts

		/// <summary>
		///		The array .namedAccounts.groups may contain several groups, if the various segments contain a different Named Account group each.
		///		For instance, let’s consider a composed Trade Service with 2 segments:
		///			-	Segment 1 for NAC group A (contains named account Nike US only)
		///			-	Segment 2 for NAC group B (contains named accounts Nike US and Nike EU)
		///
		///		In this example, the result.namedAccounts.group property of the composed Trade Service contains:
		///		"namedAccount": {
		///			"groups": [
		///			{
		///				"code": "A",
		///				"description": "Group A",
		///				"items": [
		///					{ "code": "Nike US", "name": "Nike US" }
		///				]
		///			},
		///			{
		///				"code": "B",
		///				"description": "Group B",
		///				"items": [
		///					{ "code": "Nike US", "name": "Nike US" },
		///					{ "code": "Nike EU", "name": "Nike EU" }
		///				]
		///			}]
		///		}
		///
		///		In this case, the rate represented by the composed Trade Service would be invalid for Nike EU, since the first
		///		segment is applicable to Nike US only. Therefore, the mapping for display in CW1 Rate Selector (and to populate
		///		Named Accounts drop-down list) should only consider the intersection of all named account groups (separated by commas),
		///		ie. Nike US only here.
		/// </summary>
		public static string[] GetNamedAccounts(ITradeServiceDto tradelane)
		{
			var groups = tradelane.NamedAccount?.Groups;
			if (groups == null)
			{
				return Array.Empty<string>();
			}

			var allNamedAccounts = groups
				.SelectMany(g => g.Items)
				.Select(g => g.Name)
				.GroupBy(g => g);

			var namedAccountsInAllGroups =
				allNamedAccounts
					.Where(g => g.Count() == tradelane.NamedAccount.Groups.Count())
					.Select(g => g.Key)
					.ToArray();

			return namedAccountsInAllGroups;
		}

		#endregion

		#region Payment Terms

		public static string GetPaymentTerms(ITradeServiceDto tradelane)
		{
			foreach (var baseRate in tradelane.PriceInfo.BaseRates.Items)
			{
				if (baseRate.PaymentTerm == UrsConstants.PaymentTermCode.Prepaid)
				{
					return WRConstants.PaymentTerm.Prepaid;
				}
				else if (baseRate.PaymentTerm == UrsConstants.PaymentTermCode.Collect)
				{
					return WRConstants.PaymentTerm.Collect;
				}
			}

			return string.Empty;
		}

		#endregion

		#region Flat price check

		public static bool IsFlatPrice(IEnumerable<IUniversalRateEntryDto> priceItems)
		{
			// Make sure there is only one price item, and it is categorized as a flat price type.
			return priceItems.Take(2).Count() == 1 && IsFlatPrice(priceItems.Single());
		}

		public static bool IsFlatPrice(IUniversalRateEntryDto priceItem) =>
			priceItem switch
			{
				{ BreakType: not RateBreakTypeCode.Flat } => false,
				{ PricingQuantityUnit: UnitOfMeasurementCode.Mawb } => true,
				{ PricingQuantityUnit: UnitOfMeasurementCode.Shipment } => true,
				{ PricingQuantityUnit: UnitOfMeasurementCode.BillOfLading } => true,
				{ PricingQuantityUnit: UnitOfMeasurementCode.None } => true,
				_ => false
			};

		#endregion

		#region Charge Applicability

		public static string GetChargeApplicability(string ursApplicability)
			=> RateApplicableCode.GetDescription(ursApplicability);

		public static bool IsRestricted(IUniversalRateEntryDto rate)
		{
			switch (rate.Applicable)
			{
				case RateApplicableCode.OnRequest:
				case RateApplicableCode.Iata:
				case RateApplicableCode.Vatos:
				case RateApplicableCode.Tact:
				case RateApplicableCode.NotApplicable:
				case RateApplicableCode.AllIn:
					return true;
				default:
					return false;
			}
		}

		public static bool ShouldShowApplicabilityOnCarrierConnect(IUniversalRateEntryDto rate)
		{
			switch (rate.Applicable)
			{
				case RateApplicableCode.OnRequest:
				case RateApplicableCode.Vatos:
				case RateApplicableCode.NotApplicable:
					return true;
				default:
					return false;
			}
		}

		#endregion

		#region Contract Number

		public static string GetContractNumber(ITradeServiceDto tradelane, bool isAirMode)
		{
			if (!isAirMode)
			{
				return tradelane.Contract?.Header?.Name ?? string.Empty;
			}

			if (tradelane.ServiceClass == ServiceClassCode.Contract)
			{
				return tradelane.ExternalReference ?? string.Empty;
			}

			return string.Empty;
		}

		#endregion

		#region Conversion Factor

		public static ConversionFactor ConvertConversionFactor(IBaseChargeDto ursCharge, Directions direction, string transportMode, string unit)
		{
			ConversionFactor factor;
			var rateItem = ursCharge.RateCollections?.Items?.FirstOrDefault();

			// iterate on rateItem.PriceEntries, and find if the item of UseVolumetric is true. if there is one item which UseVolumetric is true, return a value which is true
			if (rateItem.PriceEntries.Any(x => x.UseVolumetric) && ursCharge.WmRatioCubicCentimeter > 0)
			{
				factor = new ConversionFactor(ursCharge.WmRatioCubicCentimeter, "CC", "KG");// This is refer to the method ConvertPerUnit in UniversalToWiseRateConverter, Don't know why it is hardcoded :(
			}
			else
			{
				var isDomestic = direction == Directions.Domestic;
				var factors = ChargeableAmountCalculator.GetDefaultConversionFactors(isDomestic, transportMode, unit);
				factor = factors.FirstOrDefault();
			}

			return factor;
		}

		#endregion

		#region Custom Fields

		public static IEnumerable<CustomField> GetCustomFields(IBaseChargeDto charge)
		{
			if (!string.IsNullOrEmpty(charge.Company?.Name))
			{
				return new CustomField[] {
					new () {
						Code = Rate.CustomFields.CargoSphere.HandlingOffice,
						Value = charge.Company?.Name,
						Description = (NoResString)"Handling Office"
					}
				};
			}
			return [];
		}

		public static IEnumerable<CustomField> GetProviderCustomFields(ITradeServiceDto tradelane, bool isAirMode)
		{
			var customFields = new List<CustomField>();

			var routingPorts = GetRoutingPorts(tradelane.RouteInfo);
			var routingRoute = routingPorts?.Any() ?? false ? string.Join(" -> ", routingPorts) : null;
			customFields.Add(routingRoute, Rate.CustomFields.Common.Routing, (NoResString)"Routing");

			customFields.AddRange(
				isAirMode
					? GetCargoguideCustomFields(tradelane)
					: GetCargoSphereCustomFields(tradelane));

			return customFields;
		}

		public static IEnumerable<CustomField> GetCargoguideCustomFields(ITradeServiceDto tradelane) =>
			new List<CustomField>
			{
				{ tradelane.Product?.Aircraft?.FlightDeck, Rate.CustomFields.Cargoguide.DeckType, (NoResString)"Deck Type" },
				{ tradelane.Product?.Aircraft?.Cao, Rate.CustomFields.Cargoguide.CargoAircraftOnly, (NoResString)"Cargo Aircraft Only" },
				{ tradelane.Remark?.Descriptions?.FirstOrDefault(), Rate.CustomFields.Cargoguide.Remarks, (NoResString)"Additional important information about the trade lane" },
				{ tradelane.ExternalReference, Rate.CustomFields.Cargoguide.Reference, (NoResString)"Reference" },
				{ GetRateClass(tradelane.ServiceClass), Rate.CustomFields.Cargoguide.RateClass, (NoResString)"Rate/Service class" },
				{ tradelane.Product?.Code, Rate.CustomFields.Cargoguide.ProductCode, (NoResString)"Product Code" },
				{ tradelane.Product?.Name, Rate.CustomFields.Cargoguide.ProductName, (NoResString)"Product Name" },
				{ tradelane.Product?.Classification?.Name, Rate.CustomFields.Cargoguide.ProductClass, (NoResString)"Product Class" },
				{ tradelane.Calculation?.Measurements?.WmRatio, Rate.CustomFields.Cargoguide.Ratio, (NoResString)"WM Ratio" }
			};

		/// <summary>
		/// Get CS custom fields. See CargoSphereRatesConverter.GetCustomFields in rates services
		///
		/// Not converted:
		/// - OriginRouting - is it the via port? what's the format?
		/// - DestinationUNLOCORouting - mapping unknown
		/// - InlandRouting - Populated as Routing on a charge level instead
		/// - OutlandRouting - Populated as Routing on a charge level instead
		///
		/// - ArbitraryPermission
		/// taggedValues with the key "ArbitraryIndicator"
		///
		/// - Tradelane
		/// voyageInfo.tradeScope
		///
		/// - OverallRouting - mapping unknown
		///
		/// - ContainerOrUnit - mapping unknown
		/// Is text like "Cntr. 20 ft", "Cubic Meter", "Cubic Meter/1,000 KGS"
		///
		/// - ContainerQuality - mapped from Container properties ShipperOwned, Type, Overweight, CargoFit, and CargoOversize
		/// Only add it to customFields when a code is found.
		/// </summary>
		public static IEnumerable<CustomField> GetCargoSphereCustomFields(ITradeServiceDto tradelane)
		{
			var customFields = new List<CustomField>();

			customFields.Add(tradelane.VoyageInfo?.VesselName, Rate.CustomFields.CargoSphere.Vessel, (NoResString)"Vessel");

			// VoyageInfo.ServiceName corresponds to CS property: serviceInfo.serviceString
			customFields.Add(tradelane.VoyageInfo?.ServiceName, Rate.CustomFields.CargoSphere.ServiceString, (NoResString)"Service String");

			// Container Quality corresponds to CS property: ServiceInfo.ContainerQuality
			var container = tradelane.Container;
			if (container != null)
			{
				var containerQualityCode = GetContainerQualityCode(container);
				if (!string.IsNullOrEmpty(containerQualityCode))
				{
					customFields.Add(containerQualityCode, Rate.CustomFields.CargoSphere.ContainerQuality, (NoResString)"Container Quality");
				}
			}

			var originWaypoint = UrsRatesParseHelper.GetFirstWaypointByType(tradelane, UrsConstants.RouteWaypointCode.Origin);
			var originUNLOCO = originWaypoint?.Location?.Code;

			var destinationWaypoint = UrsRatesParseHelper.GetFirstWaypointByType(tradelane, UrsConstants.RouteWaypointCode.Destination);
			var destinationUNLOCO = destinationWaypoint?.Location?.Code;

			if (!string.IsNullOrEmpty(originUNLOCO) && !string.IsNullOrEmpty(destinationUNLOCO))
			{
				customFields.Add($"{originUNLOCO} > {destinationUNLOCO}", Rate.CustomFields.CargoSphere.OceanRouting, (NoResString)"Ocean Routing");
			}

			if (tradelane.TaggedValues != null)
			{
				foreach (var val in tradelane.TaggedValues.Items)
				{
					switch (val.Key)
					{
						case UrsConstants.TaggedValueKey.RateType1:
							customFields.Add(val.Value, Rate.CustomFields.CargoSphere.RateType, (NoResString)"Rate Type 1");
							break;

						case UrsConstants.TaggedValueKey.RateType2:
							customFields.Add(val.Value, Rate.CustomFields.CargoSphere.RateType2, (NoResString)"Rate Type 2");
							break;

						// ArbitraryPermission also known as Add-on in Rate Selector
						case UrsConstants.TaggedValueKey.ArbitraryIndicator:
							customFields.Add(val.Value, Rate.CustomFields.CargoSphere.ArbitraryPermission, (NoResString)"Arbitrary Permission");
							break;
					}
				}
			}

			//tradeLane
			customFields.Add(tradelane.VoyageInfo?.TradeScope, Rate.CustomFields.CargoSphere.TradeLane, (NoResString)"Trade Lane");

			return customFields;
		}

		#region SuppressResourceStringsCheckRegion - Justification: only codes, no translatable strings

		static string GetContainerQualityCode(IContainerDto container)
		{
			var containerType = container.Type?.Trim().ToUpper();
			if (container.IsShipperOwned)
			{
				// ShipperOwned should only be with SOR or SOC
				return containerType == "NOR" ? "SOR" : "SOC";
			}

			var cargoFit = container.CargoFit?.Trim().ToLower();
			if (cargoFit?.Length == 0)
			{
				// Empty CargoFit equivalent to "none"
				cargoFit = "none";
			}

			var cargoOversize = (container.CargoOversize ?? Enumerable.Empty<string>())
				.Select(x => x.Trim().ToLower())
				.ToHashSet();
			if (container.IsOverweight ?? false)
			{
				if (cargoFit == "none" && (cargoOversize.Count == 0 || cargoOversize.Contains("none")))
				{
					return "OVW";
				}

				if (cargoFit == "outofgauge" && cargoOversize.SequenceEqual(new[] { "height" }))
				{
					return "OHW";
				}

				// all other CargoFit and CargoOversize values are invalid when Overweight, no fallback
				return null;
			}

			switch (containerType)
			{
				case "NOR":
				case "GOH":
				case "GHS":
				case "GHD":
					return containerType;
				case "RF":
					return "REF";
				case "FOOD":
					return "FOD";
				case "FLEX":
					return "FLX";
			}

			switch (cargoFit)
			{
				case "ingauge":
					return "ING";

				case "outofgauge":
				{
					if (cargoOversize.SequenceEqualIgnoringOrder(new[] { "height", "widthoneside" }))
					{
						return "OOS";
					}
					if (cargoOversize.SequenceEqualIgnoringOrder(new[] { "none" }))
					{
						return "OOG";
					}
					if (cargoOversize.SequenceEqualIgnoringOrder(new[] { "height", "width" }))
					{
						return "HWD";
					}
					if (cargoOversize.SequenceEqualIgnoringOrder(new[] { "height" }))
					{
						return "OVH";
					}
					if (cargoOversize.SequenceEqualIgnoringOrder(new[] { "width" }))
					{
						return "OWD";
					}

					break;
				}
			}

			return null;
		}

		#endregion

		static IEnumerable<string> GetRoutingPorts(IRouteInfoDto routeInfo)
		{
			return routeInfo?.Waypoints?
				.Select(x => (Order: GetWaypointOrder(x.WaypointType), Waypoint: x))
				.Where(y => y.Order >= 0)
				.OrderBy(y => y.Order)
				.Select(y => y.Waypoint?.Location?.Code)
				.Where(code => !string.IsNullOrEmpty(code))
				.ToList();
		}

		/// <summary>
		/// Order for waypoints, Origin first
		/// </summary>
		static int GetWaypointOrder(string waypointType)
		{
			switch (waypointType)
			{
				case UrsConstants.RouteWaypointCode.Origin:
					return 0;
				case UrsConstants.RouteWaypointCode.Via:
					return 1;
				case UrsConstants.RouteWaypointCode.Via2:
					return 2;
				case UrsConstants.RouteWaypointCode.Destination:
					return 4;
				default:
					return -1;
			}
		}

		static string GetRateClass(string serviceClass)
		{
			switch (serviceClass)
			{
				case UrsConstants.ServiceClassCode.Market:
					return (NoResString)"Market";
				case UrsConstants.ServiceClassCode.Promotional:
					return (NoResString)"Promotional";
				case UrsConstants.ServiceClassCode.Adhoc:
					return "Ad-hoc";
				case UrsConstants.ServiceClassCode.Contract:
					return (NoResString)"Contract";
				case UrsConstants.ServiceClassCode.Gateway:
					return (NoResString)"Gateway";
				case UrsConstants.ServiceClassCode.Custom:
					return (NoResString)"Custom";
				case UrsConstants.ServiceClassCode.NamedAccount:
					return (NoResString)"Named Account";
				case UrsConstants.ServiceClassCode.TACT:
					return "TACT";
				case UrsConstants.ServiceClassCode.PreCarriage:
					return (NoResString)"Pre Carriage";
				case UrsConstants.ServiceClassCode.OnCarriage:
					return (NoResString)"On Carriage";
				case UrsConstants.ServiceClassCode.UserRate:
					return (NoResString)"User Rate";
				case UrsConstants.ServiceClassCode.PriceList:
					return (NoResString)"Price List";
				case UrsConstants.ServiceClassCode.Selling:
					return (NoResString)"Selling";
				case UrsConstants.ServiceClassCode.SourceReferenceRate:
					return (NoResString)"Source Reference Rate";
				case UrsConstants.ServiceClassCode.SharedContractRate:
					return (NoResString)"Shared Contract Rate";
				default:
					return serviceClass;
			}
		}

		#endregion

		#region Carrier Commodity

		public static CarrierSpecificCommodity GetCarrierCommodity(ITradeServiceDto tradelane)
		{
			var product = tradelane.Product;
			if (product == null)
			{
				return null;
			}

			var commodity = tradelane.Commodity;
			return new CarrierSpecificCommodity
			{
				Code = product.Code,
				GroupName = GetTransportMode(tradelane) == WRConstants.TransportModes.SEA ? commodity?.Groups?.FirstOrDefault()?.Description : product.Name,
				GroupType = GetCommodityCategory(tradelane),
				IncludedCommodities = commodity?.Groups?.SelectMany(g => g.Items ?? []).WhereNotNull().Select(x => x.Name).ToList() ?? [],
				ExcludedCommodities = commodity?.Groups?.SelectMany(g => g.ExcludedItems ?? []).WhereNotNull().Select(x => x.Name).ToList() ?? [],
			};
		}

		static string GetCommodityCategory(ITradeServiceDto tradeService) =>
			tradeService.Product.Classification.Code switch
			{
				ProductClassCode.Dgr => CommodityCategory.Hazardous,
				ProductClassCode.Com or ProductClassCode.Gen => CommodityCategory.NonHazardous,
				_ => string.Empty
			};

		public static string GetCommodityGroup(ITradeServiceDto tradelane)
		{
			var transportMode = GetTransportMode(tradelane);

			if (transportMode == WRConstants.TransportModes.SEA)
			{
				return string.Empty;
			}

			return tradelane?.Product?.UniversalCode ?? string.Empty;
		}

		public static ZString[] GetCommodities(CarrierSpecificCommodity carrierSpecificCommodity)
		{
			return carrierSpecificCommodity?.IncludedCommodities?.Select(x => (ZString)x).ToArray();
		}

		public static string GetProduct(CarrierSpecificCommodity carrierSpecificCommodity)
		{
			return carrierSpecificCommodity?.GroupName;
		}

		public static List<RefCommodityCode> GetRefCommodities(string universalCommodityGroup)
		{
			if (string.IsNullOrEmpty(universalCommodityGroup))
			{
				return [];
			}

			var query = new ZDBOnlyQuery(typeof(RefCommodityCode));
			query.AddToFilter(RefCommodityCodeSchema.RH_UniversalCommodityGroup, universalCommodityGroup);
			query.AddToFilter(RefCommodityCodeSchema.RH_IsActive, ZBool.True);
			return [.. new BusinessObjectFactory().Load<RefCommodityCode>(query)];
		}

		#endregion

		#region Shipping Phase

		public static string MapShippingPhase(string code) =>  code switch
		{
			ShippingPhaseCode.PortfLoading => Core.Constants.FreightShipmentDirection.Code.Export,
			ShippingPhaseCode.PortOfDischarge => Core.Constants.FreightShipmentDirection.Code.Import,
			_ => string.Empty,
		};

		#endregion

		#region Booking Info

		public static bool IsBookingTermCharge(IBaseChargeDto charge) => charge.ChargeDefinition?.UsabilityGroup == UrsConstants.ChargeUsabilityGroupCode.Pen;

		public static bool IsPenaltyCharge(IBaseChargeDto charge) => charge.ChargeDefinition?.UsabilityGroup == UrsConstants.ChargeUsabilityGroupCode.Storage;

		public static string MapPenaltyType(string type) =>
			type switch
			{
				UrsPenaltyType.Storage or UrsPenaltyType.Demurrage => Core.Constants.ContainerDetentionPenaltyType.STO,
				UrsPenaltyType.Detention => Core.Constants.ContainerDetentionPenaltyType.DET,
				_ => string.Empty,
			};

		#endregion
	}
}
