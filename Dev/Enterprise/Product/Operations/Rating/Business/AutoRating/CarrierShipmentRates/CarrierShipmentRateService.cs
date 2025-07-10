using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business;

/// <summary>
/// Service to auto-rate Carrier Shipments from the OCS module
/// </summary>
public sealed class CarrierShipmentRateService : ICarrierShipmentRateService
{
	readonly BusinessObjectFactory factory;
	readonly RateChooserServices chooserServices;
	readonly IRateQueryDtoConverter rateQueryDtoConverter;
	readonly ICarrierShipmentAutoRater carrierShipmentAutoRater;
	readonly ILogger logger;

	string DefaultCurrencyCode => GlbCompany.CurrentCompany?.GC_RX_NKLocalCurrency ?? string.Empty;

	public CarrierShipmentRateService() : this(new SimpleLogger())
	{
	}

	public CarrierShipmentRateService(ILogger logger = null)
	{
		factory = new BusinessObjectFactory();
		chooserServices = new RateChooserServices(factory, ZDateTime.Today, DefaultCurrencyCode);
		this.rateQueryDtoConverter = ObjectFactory.Get<IRateQueryDtoConverter>();
		this.carrierShipmentAutoRater = ObjectFactory.Get<ICarrierShipmentAutoRater>();
		this.logger = logger;
	}

	public CarrierShipmentRateResult GetCarrierShipmentRates(
		CalculateRatesQueryParameters parameters)
	{
		var queryParameterErrors = ValidateQueryParameters(parameters).ToArray();
		if (queryParameterErrors.Any())
		{
			return new([], [], queryParameterErrors);
		}

		var (rateQueryDto, conversionErrors) = rateQueryDtoConverter.Convert(parameters, factory);
		if (conversionErrors.Any())
		{
			return new([], [], conversionErrors.ToArray());
		}

		var dtoErrors = ValidateQueryDto(rateQueryDto).ToArray();
		if (dtoErrors.Any())
		{
			return new([], [], dtoErrors);
		}

		var autoRateResults = carrierShipmentAutoRater.AutoRateCarrierShipment(rateQueryDto, factory, chooserServices, logger);
		if (autoRateResults.Rates.Count == 0)
		{
			autoRateResults = autoRateResults with { Logs = [.. autoRateResults.Logs, Res.GetString("b4b893c8-06f3-4d84-973b-6f305b143427", "No rates were found for the given query.")] };
		}

		return autoRateResults;
	}

	IEnumerable<string> ValidateQueryParameters(CalculateRatesQueryParameters parameters)
	{
		if (!parameters.GetRatesForCosts && !parameters.GetRatesForSales)
		{
			yield return Res.GetString("6b89fb6b-04a8-4b11-a730-fac6c17e97c3", "You have to select at least one type of rates.");
		}

		if (parameters.ShipmentHeaderId == Guid.Empty)
		{
			yield return Res.GetString("5a5098e9-88ab-4b48-8ea2-375df19e7d48", "The Shipment ID must be provided.");
		}

		if (parameters.RouteLegIds == null || parameters.RouteLegIds.Count == 0 || parameters.RouteLegIds.All(x => x == Guid.Empty))
		{
			yield return Res.GetString("3b2a12a0-93f7-4b7a-b27e-6301f9c3c4a2", "At least 1 Route Leg ID must be provided.");
		}
	}

	IEnumerable<string> ValidateQueryDto(CarrierShipmentRateQueryDto rateQueryDto)
	{
		if (rateQueryDto.RouteLegs == null || !rateQueryDto.RouteLegs.Any())
		{
			yield return Res.GetString("c161c908-377e-43e9-ab2a-12d6027841c2", "At least 1 valid Route Leg must be provided.");
		}

		if (rateQueryDto.Cargo == null || !rateQueryDto.Cargo.Any())
		{
			yield return Res.GetString("488cde20-f842-43b1-b918-bfe769797f1e", "At least 1 Cargo piece must be provided.");
		}
	}
}
