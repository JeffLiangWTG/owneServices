using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools.Enums;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.WiseRates;

public class UrsWiseRatesConverter(ILogger logger)
{
	readonly ILogger logger = logger;

	public QueryRequestDto Map(RatesSearchRequest ratesServiceRequest)
	{
		var ursRequest = new QueryRequestDto
		{
			Shipment = null,
			QuerySources = [QuerySource.Database, QuerySource.OceanOnDemand]
		};

		if (ratesServiceRequest.RatesQuery.EffectiveDate != null)
		{
			ursRequest.ViewDate.ViewDate = ratesServiceRequest.RatesQuery.EffectiveDate.Value;
		}

		ursRequest.Route.Origin = MapLocation(ratesServiceRequest.RatesQuery.Origin);
		ursRequest.Route.Destination = MapLocation(ratesServiceRequest.RatesQuery.Destination);
		ursRequest.AllowedNamedAccounts = RatingHelper.GetAllowedNamedAccounts();
		ursRequest.Filters.ModesOfTransport.AddRange(ratesServiceRequest.RatesQuery.TransportMode?.SelectMany(MapTransportMode) ?? Enumerable.Empty<string>());
		ursRequest.Filters.FreightShippingTerms.AddRange(MapContainerMode(ratesServiceRequest.RatesQuery));

		if (ratesServiceRequest.RatesQuery.Carrier != null)
		{
			ursRequest.Filters.CarrierCode.AddRange(ratesServiceRequest.RatesQuery.Carrier.Select(r => r.IATACode).Where(c => !string.IsNullOrEmpty(c)));
			ursRequest.Filters.CarrierCode.AddRange(ratesServiceRequest.RatesQuery.Carrier.Select(r => r.SCACCode).Where(c => !string.IsNullOrEmpty(c)));
			ursRequest.Filters.CarrierCode.AddRange(ratesServiceRequest.RatesQuery.Carrier.Select(r => r.C1Code).Where(c => !string.IsNullOrEmpty(c)));
		}

		if (ratesServiceRequest.RatesQuery.Container != null)
		{
			ursRequest.Filters.ContainerCode.AddRange(MapContainer(ratesServiceRequest.RatesQuery.Container));
		}

		if (ratesServiceRequest.RatesQuery.ServiceLevel != null)
		{
			ursRequest.Filters.ServiceLevel.AddRange(ratesServiceRequest.RatesQuery.ServiceLevel);
		}

		if (ratesServiceRequest.RatesQuery.Contract != null)
		{
			ursRequest.Filters.Reference.AddRange(ratesServiceRequest.RatesQuery.Contract.Select(c => c.ContractNumber));
		}

		if (ratesServiceRequest.RatesQuery.NamedAccount != null)
		{
			ursRequest.Filters.NamedAccount.AddRange(ratesServiceRequest.RatesQuery.NamedAccount.Select(a => a.Name));
		}

		if (ratesServiceRequest.RatesQuery.PaymentTerm != null)
		{
			ursRequest.Filters.PaymentTerms.AddRange(MapPaymentTerms(ratesServiceRequest.RatesQuery.PaymentTerm));
		}

		if (ratesServiceRequest.RatesQuery.CargoGuideFilters?.References != null)
		{
			ursRequest.Filters.ExternalReference.AddRange(ratesServiceRequest.RatesQuery.CargoGuideFilters.References);
		}

		return ursRequest;
	}

	public RatesSearchResponse Map(IEnumerable<TradeServiceDto> ursRates, string correlationId)
	{
		var errorReporter = new UniversalToWiseRateErrorReporter(correlationId);
		var converter = new UniversalToWiseRateConverter(logger, errorReporter);
		var rates = converter.Convert(ursRates);

		var carriers = rates.Select(r => (r.TransportMode, r.Carrier)).Distinct().Select(r => new RefCarrier
		{
			Code = r.Carrier,
			IATACode = r.TransportMode == WRConstants.TransportModes.AIR ? r.Carrier : string.Empty,
			SCACCode = r.TransportMode == WRConstants.TransportModes.SEA ? r.Carrier : string.Empty,
		}).ToArray();

		var serviceLevels = rates.Select(r => r.ServiceLevel).Distinct().Select(code => new RefServiceLevel
		{
			Code = code
		}).ToArray();

		var providers = rates.GroupBy(r => r.RatesServiceProvider).Select(g => new ProviderResult
		{
			ProviderCode = g.Key,
			ProviderName = g.Key,
			ConnectionResult = ConnectionResult.Success
		}).ToArray();

		var response = new RatesSearchResponse();
		response.TotalPages = 1;
		response.Rates = rates.ToArray();
		response.Carriers = carriers;
		response.Providers = providers;
		response.ServiceLevels = serviceLevels;

		return response;
	}

	public static IEnumerable<string> MapTransportMode(string mode)
	{
		switch (mode)
		{
			case "SEA":
				return new[] { UrsConstants.ModeOfTransport.Ocean, UrsConstants.ModeOfTransport.ShortSea, UrsConstants.ModeOfTransport.InlandNavigation };
			case "AIR":
				return new[] { UrsConstants.ModeOfTransport.Air };
			case "ROA":
				return new[] { UrsConstants.ModeOfTransport.Road };
			case "RAI":
				return new[] { UrsConstants.ModeOfTransport.Rail };
			default:
				throw new NotSupportedException("URS doesn't support CW1 Transport mode: " + mode);
		}
	}

	string MapLocation(IEnumerable<string> locations)
	{
		// It is a collection because we send not only UNLOCO, but country and zones as well.
		// But, URS as well as Rates Service support only UNLOCO at the moment.
		var location = locations.FirstOrDefault(l => l.Length == 5);
		if (string.IsNullOrEmpty(location))
		{
			// It is expected to be 1 unloco there anyway, the check and the exception is just in case.
			throw new NotSupportedException(FormattableString.Invariant($"None of the locations ({string.Join(", ", locations)}) is supported by URS"));
		}

		return location;
	}

	IEnumerable<string> MapContainer(IEnumerable<RatesQueryContainer> containers)
	{
		var codes = new List<string>();

		foreach (var container in containers)
		{
			if (!string.IsNullOrEmpty(container.ISOType))
			{
				codes.Add(container.ISOType);
			}
			else
			{
				// ULD type supposed to be there
				codes.Add(container.Code);
			}
		}

		return codes;
	}

	public IEnumerable<string> MapContainerMode(RatesQuery query)
	{
		if (query == null)
		{
			return [];
		}

		var transportMode = query.TransportMode?.FirstOrDefault();
		return MapContainerMode(transportMode, query.ContainerMode);
	}

	public static IEnumerable<string> MapContainerMode(string transportMode, IEnumerable<string> containerModes)
	{
		if (containerModes == null || string.IsNullOrEmpty(transportMode))
		{
			return [];
		}

		return containerModes.Select((containerMode) =>
			transportMode == WRConstants.TransportModes.AIR
				? containerMode == WRConstants.ContainerModes.FCL ? Constants.ContainerModes.ULD : Constants.ContainerModes.Loose
				: containerMode == WRConstants.ContainerModes.FCL ? Constants.ContainerModes.FCL : Constants.ContainerModes.LCL);
	}

	public IEnumerable<string> MapPaymentTerms(IEnumerable<string> paymentTerms)
	{
		var ursPaymentTerms = new List<string>();

		foreach (var paymentTerm in paymentTerms)
		{
			switch (paymentTerm)
			{
				case WRConstants.PaymentTerm.Collect:
					ursPaymentTerms.Add(UrsConstants.PaymentTermCode.Collect);
					break;
				case WRConstants.PaymentTerm.Prepaid:
					ursPaymentTerms.Add(UrsConstants.PaymentTermCode.Prepaid);
					break;
			}
		}

		return ursPaymentTerms;
	}
}
