using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Rating.Business.WiseRates;
using Urs.Api.Integration.DTOs.Request;

namespace Enterprise.Rating.Business;

public class UrsRatesQueryBuilder(ILogger logger) : IUrsRatesQueryBuilder<RatingCriteria>
{
	internal string TransportMode { get; private set; }

	public (QueryRequestDto ursRequest, string error) Build(RatingCriteria criteria)
	{
		var errors = new List<string>();

		if (!criteria.IsAirFreight && !criteria.IsSeaFreight)
		{
			errors.Add(WiseRatesQueryBuilder.LogMessages.UnknownFreightError);
		}

		var request = new QueryRequestDto
		{
			Shipment = null,
			QuerySources = new[] { QuerySource.Database, QuerySource.OceanOnDemand }
		 };

		// Carrier
		var criteriaCarriers = criteria.GetCostsSearchOrgs().WhereNotNull().ToList();
		var ratesQueryCarrierSource = GetCarriers(criteriaCarriers, request);
		if (criteriaCarriers.Count > 0 && ratesQueryCarrierSource.Count == 0)
		{
			errors.Add(WiseRatesQueryBuilder.LogMessages.ServiceProviderError);
		}

		var freightEffectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(criteria, ChargeCodeGroupList.Codes.Freight, null, ratesQueryCarrierSource);
		var effectiveDate = freightEffectiveDate.IsEmpty ? ZDate.Today : freightEffectiveDate;
		if (effectiveDate.IsValid && !effectiveDate.IsEmpty)
		{
			request.ViewDate.ViewDate = effectiveDate.ToDateTime();
		}

		// Route
		request.Route.Origin = criteria.Origin?.UNLOCO?.Code ?? string.Empty;
		request.Route.Destination = criteria.Destination?.UNLOCO?.Code ?? string.Empty;

		// AllowedNamedAccounts
		request.AllowedNamedAccounts = RatingHelper.GetAllowedNamedAccounts();

		var filters = request.Filters;

		// ModesOfTransport
		if (criteria.IsAirFreight)
		{
			TransportMode = Core.Constants.TransportModes.Air;
		}
		else if (criteria.IsSeaFreight)
		{
			TransportMode = Core.Constants.TransportModes.Sea;
		}

		if (!string.IsNullOrEmpty(TransportMode))
		{
			filters.ModesOfTransport.AddRange(UrsWiseRatesConverter.MapTransportMode(TransportMode));
		}

		if (!DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(TransportMode, criteria.ContainerMode, out string reason))
		{
			errors.Add(string.Format(WiseRatesQueryBuilder.LogMessages.GeneralErrorTemplate, reason));
		}

		// ContainerMode
		var containerModes = criteria.IsContainerised ? new[] { Core.Constants.ContainerModes.FCL } : new[] { Core.Constants.ContainerModes.LCL };
		filters.FreightShippingTerms.AddRange(UrsWiseRatesConverter.MapContainerMode(TransportMode, containerModes));

		// ContainerCode
		var criteriaContainers = criteria.GetContainers();
		var containers = GetContainerCodes(criteriaContainers, criteria.IsSeaFreight);
		if (criteriaContainers.Any() && !containers.Any())
		{
			errors.Add(WiseRatesQueryBuilder.LogMessages.ContainerError);
		}
		filters.ContainerCode.AddRange(containers);

		// Contract
		filters.Reference.AddRange(
			criteria.CarrierContractNumbers
				.Where(c => !c.IsEmpty)
				.Select(c => c.ToString())
				.Distinct()
		);

		// For the following fields, we are not using them in the current implementation.
		// We may need to implement them in the future when it is needed by Rate Selector.
		// ServiceLevel
		// NamedAccount
		// PaymentTerms
		// ExternalReference (CargoGuideFilters)

		return (request, errors.ToStringWithNewLineBetweenStrings());
	}

	List<OrgWithSource> GetCarriers(IEnumerable<OrgWithSource> carriers, QueryRequestDto queryRequest)
	{
		var carriersSource = new List<OrgWithSource>();
		foreach (var org in carriers.WhereNotNull())
		{
			if (!org.Org.OH_IsShippingProvider)
			{
				logger.Warning(string.Format(CultureInfo.InvariantCulture, WiseRatesQueryBuilder.LogMessages.CarrierIsNotShippingProviderErrorTemplate, org.Org.OH_Code, org.SourceText));
				continue;
			}

			var orgHeader = org.Org;
			var refShippingLine = orgHeader.ShippingLine;
			var scacCode = refShippingLine?.RSL_StandardCarrierAlphaCode ?? ZString.Empty;
			var iataCode = orgHeader.MiscServ.Airline?.RM_TwoCharacterCode ?? ZString.Empty;
			var c1Code = refShippingLine?.RSL_CargoWiseOneCode ?? ZString.Empty;

			if (scacCode.IsEmpty && iataCode.IsEmpty && c1Code.IsEmpty)
			{
				logger.Warning(string.Format(CultureInfo.InvariantCulture, WiseRatesQueryBuilder.LogMessages.CarrierCodeIsNotProvidedErrorTemplate, orgHeader.OH_Code, org.SourceText));
				continue;
			}
			queryRequest.Filters.CarrierCode.AddRange(
				new[] { scacCode, iataCode, c1Code }
					.Where(code => !code.IsEmpty)
					.Select(code => code.ToString()));

			carriersSource.Add(org);
		}
		return carriersSource;
	}

	List<string> GetContainerCodes(IEnumerable<RefContainer> containers, bool isSeaFreight)
	{
		var result = new List<string>();

		foreach (var container in containers)
		{
			var isoType = !container.RC_ISOType.IsEmpty ? (string)container.RC_ISOType : null;

			if (string.IsNullOrEmpty(isoType) && isSeaFreight)
			{
				logger.Warning(string.Format(CultureInfo.InvariantCulture, WiseRatesQueryBuilder.LogMessages.ContainerCodeIsNotProvidedErrorTemplate, container.RC_Code));
				continue;
			}

			if (container.RC_ShippingMode == RefContainerLookups.ShippingModes.Air || string.IsNullOrEmpty(isoType))
			{
				result.Add(container.RC_Code);
			}
			else
			{
				result.Add(isoType);
			}
		}
		return result;
	}
}

