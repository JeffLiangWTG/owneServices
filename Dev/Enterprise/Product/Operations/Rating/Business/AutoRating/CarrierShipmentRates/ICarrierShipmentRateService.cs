namespace Enterprise.Rating.Business;

public interface ICarrierShipmentRateService
{
	CarrierShipmentRateResult GetCarrierShipmentRates(CalculateRatesQueryParameters queryParameters);
}
