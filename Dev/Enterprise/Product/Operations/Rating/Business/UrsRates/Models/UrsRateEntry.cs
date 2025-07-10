#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business;

[DebuggerDisplay("{TI_Mode}-{ParentRatingHeader.ServiceProvider}")]
public class UrsRateEntry(ITradeServiceDto tradeService, BusinessObjectFactory factory) : WiseEntry(tradeService, factory)
{
	public UrsRatingHeader UrsRatingHeader => (UrsRatingHeader)ParentRatingHeader;

	public string? ProductClassCode => TradeService.Product?.Classification?.Code;

	public string? ProductClassName => TradeService.Product?.Classification?.Name;

	List<RefCommodityCode>? refCommodities;
	public List<RefCommodityCode> RefCommodities => refCommodities ??= UrsRatesParseHelper.GetRefCommodities(CommodityGroup);

	public IEnumerable<string> Route => TradeService.RouteInfo.Waypoints.Select(waypoint => waypoint.Location.Code);

	public UrsBookingInfo? BookingInfo { get; set; }

	CarrierSpecificCommodity? carrierSpecificCommodity;
	public CarrierSpecificCommodity? CarrierSpecificCommodity
	{
		get => carrierSpecificCommodity;
		set
		{
			carrierSpecificCommodity = value;
			Commodities = UrsRatesParseHelper.GetCommodities(value);
			ProductName = UrsRatesParseHelper.GetProduct(value);
		}
	}

	public UrsContainer? UrsContainer { get; set; }
}
