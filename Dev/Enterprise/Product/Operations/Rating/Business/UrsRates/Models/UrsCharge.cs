using Urs.Api.Integration.Interfaces;

namespace Enterprise.Rating.Business;

public class UrsCharge : IUrsCharge
{
	public string Level { get; init; }

	public string Code { get; init; }

	public string Name { get; init; }

	public IChargeDefinitionDto ChargeDefinition { get; init; }

	public IGeoScopeInfoDto GeoScope { get; init; }

	public IRateCollectionDataDto RateCollections { get; init; }

	public decimal WmRatioCubicCentimeter { get; init; }

	public IRemarkDataDto Remarks { get; init; }

	public string ExternalReference { get; init; }

	public string UniversalCode { get; init; }

	public ICompanyEntityDto Owner { get; init; }

	public ICompanyEntityDto Company { get; init; }

	public ITradeServiceDto TradeService { get; init; }

	public bool IsGeneratedFreightCharge { get; }

	public UrsCharge(IBaseChargeDto charge, ITradeServiceDto tradeService, bool isGeneratedFreightCharge = false)
	{
		Level = charge.Level;
		Code = charge.Code;
		Name = charge.Name;
		ChargeDefinition = charge.ChargeDefinition;
		GeoScope = charge.GeoScope;
		RateCollections = charge.RateCollections;
		WmRatioCubicCentimeter = charge.WmRatioCubicCentimeter;
		Remarks = charge.Remarks;
		ExternalReference = charge.ExternalReference;
		UniversalCode = charge.UniversalCode;
		Owner = charge.Owner;
		Company = charge.Company;
		TradeService = tradeService;
		IsGeneratedFreightCharge = isGeneratedFreightCharge;
	}

	public UrsCharge() { }
}
