#nullable enable
using System;
using Urs.Api.Integration.Interfaces;

namespace Enterprise.Rating.Business;

public class UrsInclusiveCharge(IUrsCharge charge, IChargeDefinitionDto definition) : IUrsCharge
{
	public string Code { get; set; } = charge.Code;

	public string Name { get; set; } = charge.Name;

	public string UniversalCode { get; set; } = charge.UniversalCode;

	public IChargeDefinitionDto ChargeDefinition { get; set; } = definition;

	public ITradeServiceDto TradeService { get; set; } = charge.TradeService;

	public string Level => throw new NotSupportedException("Level is not supported for InclusiveChargeDto.");

	public IGeoScopeInfoDto GeoScope => throw new NotSupportedException("GeoScope is not supported for InclusiveChargeDto.");

	public IRateCollectionDataDto RateCollections => throw new NotSupportedException("RateCollections is not supported for InclusiveChargeDto.");

	public decimal WmRatioCubicCentimeter => throw new NotSupportedException("WmRatioCubicCentimeter is not supported for InclusiveChargeDto.");

	public IRemarkDataDto Remarks => throw new NotSupportedException("Remarks is not supported for InclusiveChargeDto.");

	public string ExternalReference => throw new NotSupportedException("ExternalReference is not supported for InclusiveChargeDto.");

	public ICompanyEntityDto Owner => throw new NotSupportedException("Owner is not supported for InclusiveChargeDto.");

	public ICompanyEntityDto Company => throw new NotSupportedException("Company is not supported for InclusiveChargeDto.");
}
