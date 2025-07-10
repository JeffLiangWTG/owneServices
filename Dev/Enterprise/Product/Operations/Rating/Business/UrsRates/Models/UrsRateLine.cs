#nullable enable
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business;

public class UrsRateLine : WiseLine
{
	public UrsRateLine(BusinessObjectFactory factory, IUrsCharge baseCharge, ChargeType chargeType) : base(factory, baseCharge, chargeType)
	{
		CarrierChargeCode = baseCharge.Code;
		CarrierChargeCodeDescription = baseCharge.Name;
		TradeService = baseCharge.TradeService;
		if (baseCharge is not UrsInclusiveCharge)
		{
			CustomFields = UrsRatesParseHelper.GetCustomFields(baseCharge);
		}
	}

	public ITradeServiceDto TradeService { get; }

	public string? UniversalChargeCode => BaseCharge.ChargeDefinition?.UniversalCode;

	public string? UniversalChargeCodeDescription => BaseCharge.ChargeDefinition?.Description;

	public IEnumerable<string> Route => TradeService.RouteInfo?.Waypoints.Select(waypoint => waypoint.Location.Code) ?? [];

	public string? HandlingOfficeName => GetCustomFieldValue(Rate.CustomFields.CargoSphere.HandlingOffice) as string;
}
