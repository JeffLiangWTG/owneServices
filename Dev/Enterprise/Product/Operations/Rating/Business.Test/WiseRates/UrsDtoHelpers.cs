#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Schedules;
using Urs.Api.Integration.Interfaces;
using Urs.Api.Integration.Interfaces.Schedules;
using static Enterprise.Rating.Business.UrsConstants;

namespace Enterprise.Rating.Business.Test;

public record CreateBaseChargeOptions(
	string universalChargeCode,
	string shippingPhaseCode = ShippingPhaseCode.PortfLoading,
	string usabilityGroupCode = ChargeUsabilityGroupCode.None,
	string chargeDescription = "",
	string currency = "USD",
	decimal price = 100,
	string applicable = RateApplicableCode.UnitPrice,
	string priceUnit = UnitOfMeasurementCode.Kilogram,
	string? chargeDefinitionCode = null,
	decimal breakQuantity = 1,
	string quantityUnit = "",
	bool useVolumetric = false);

public static class UrsDtoHelpers
{
	public static BaseChargeDto CreateBaseCharge(CreateBaseChargeOptions options) =>
		new()
		{
			Level = "TradeServiceCharge",
			Code = options.universalChargeCode + "Local",
			UniversalCode = options.universalChargeCode,
			Name = options.universalChargeCode + " Name",
			ChargeDefinition = new ChargeDefinitionDto()
			{
				Code = options.chargeDefinitionCode ?? options.universalChargeCode + "Local",
				UniversalCode = options.universalChargeCode,
				Description = options.chargeDescription,
				ShippingPhase = new ShippingPhaseDto()
				{
					Code = options.shippingPhaseCode
				},
				UsabilityGroup = options.usabilityGroupCode
			},
			WmRatioCubicCentimeter = 0m,
			RateCollections = new RateCollectionDataDto()
			{
				Items = new[]
				{
					new RateCollectionDto()
					{
						Currency = options.currency,
						PriceEntries = new []
						{
							new UniversalRateEntryDto
							{
								Price = options.price,
								Applicable = options.applicable,
								BreakType = RateBreakTypeCode.Flat,
								BreakQuantity = options.breakQuantity,
								PricingQuantityUnit = options.priceUnit,
								QuantityUnit = options.quantityUnit,
								UseVolumetric = options.useVolumetric
							}
						}
					}
				}
			}
		};

	static BaseChargeDataDto GetOrCreateBaseChargeData(IBaseChargeDataDto? data) => data as BaseChargeDataDto ?? new BaseChargeDataDto
	{
		Items = new List<BaseChargeDto>(),
	};

	static BaseChargeDataDto AddChargeWithSinglePriceEntry(this BaseChargeDataDto baseChargeData, CreateBaseChargeOptions options)
	{
		baseChargeData.Items = baseChargeData.Items.Append(CreateBaseCharge(options));
		return baseChargeData;
	}

	public static PriceInfoDto AddChargeWithSinglePriceEntry(this PriceInfoDto priceInfo, CreateBaseChargeOptions options)
	{
		priceInfo.Charges = GetOrCreateBaseChargeData(priceInfo.Charges).AddChargeWithSinglePriceEntry(options);
		return priceInfo;
	}

	public static PriceInfoDto AddPenaltyWithSinglePriceEntry(this PriceInfoDto priceInfo, CreateBaseChargeOptions options)
	{
		// Penalty charges exist in both lists
		priceInfo.Penalties = GetOrCreateBaseChargeData(priceInfo.Penalties).AddChargeWithSinglePriceEntry(options);
		priceInfo.Charges = GetOrCreateBaseChargeData(priceInfo.Charges).AddChargeWithSinglePriceEntry(options);
		return priceInfo;
	}

	public static PriceInfoDto AddFreeTimeWithSinglePriceEntry(this PriceInfoDto priceInfo, CreateBaseChargeOptions options)
	{
		// FreeTime charges exist in both lists
		priceInfo.FreeTime = GetOrCreateBaseChargeData(priceInfo.FreeTime).AddChargeWithSinglePriceEntry(options);
		priceInfo.Charges = GetOrCreateBaseChargeData(priceInfo.Charges).AddChargeWithSinglePriceEntry(options);
		return priceInfo;
	}

	public static RateCollectionDto AddPriceEntry(this RateCollectionDto rateCollection, UniversalRateEntryDto entry)
	{
		rateCollection.PriceEntries = rateCollection.PriceEntries.Append(entry);
		return rateCollection;
	}

	#region Route

	public static RouteInfoDto CreateRoute(this TradeServiceDto tradeService)
	{
		var routeInfo = new RouteInfoDto { Waypoints = new List<GeoScopeEntryDto>() };
		tradeService.RouteInfo = routeInfo;
		return routeInfo;
	}

	public static RouteInfoDto AddWaypoint(this RouteInfoDto routeInfo, string location, string waypointType, string locationFunctionCode = LocationFunctionCode.Seaport)
	{
		(routeInfo.Waypoints as List<GeoScopeEntryDto>)!
			.Add(new GeoScopeEntryDto { WaypointType = waypointType, Location = new LocationDto { Code = location, FunctionCode = locationFunctionCode } });
		return routeInfo;
	}

	#endregion

	#region Schedules

	public static ShippingScheduleDto AddSchedule(this TradeServiceDto tradeService, IScheduleTravelInfoDto travelInfo, string priceReference)
	{
		var schedule = new ShippingScheduleDto
		{
			TravelInfo = travelInfo,
			ExternalPriceReference = priceReference,
			Segments = new ScheduleSegmentDataDto
			{
				Items = [],
			},
		};

		if (tradeService.Schedules?.Items is null)
		{
			tradeService.Schedules = new ShippingScheduleDataDto { Items = [schedule] };
		}
		else
		{
			((ShippingScheduleDataDto)tradeService.Schedules).Items = tradeService.Schedules.Items.Append(schedule);
		}

		return schedule;
	}

	public static ScheduleSegmentDto AddSegment(this ShippingScheduleDto shippingSchedule, IScheduleTravelInfoDto travelInfo, IScheduleTransportDto transport, IScheduleLocationDataDto locationData)
	{
		var segment = new ScheduleSegmentDto
		{
			TravelInfo = travelInfo,
			Transport = transport,
			Location = locationData,
			Events = new ScheduleEventDataDto { Items = new List<ScheduleEventDto>() },
		};
		((ScheduleSegmentDataDto)shippingSchedule.Segments).Items = shippingSchedule.Segments.Items.Append(segment);
		return segment;
	}

	public static ScheduleSegmentDto AddEvent(this ScheduleSegmentDto segment, string code, string name, string type, DateTime date, TimeSpan time)
	{
		((List<ScheduleEventDto>)segment.Events.Items).Add(new()
		{
			Code = code,
			Name = name,
			Type = type,
			Date = date,
			Time = time,
		});
		return segment;
	}

	#endregion
}
