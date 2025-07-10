using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Rating.Business;

public interface ICarrierShipmentAutoRater
{
	CarrierShipmentRateResult AutoRateCarrierShipment(
		CarrierShipmentRateQueryDto rateQueryDto, BusinessObjectFactory factory, IRateChooserServices chooserServices, ILogger logger);
}
