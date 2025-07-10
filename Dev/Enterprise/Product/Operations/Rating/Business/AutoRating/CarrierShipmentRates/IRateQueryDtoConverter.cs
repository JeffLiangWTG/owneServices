using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business;

public interface IRateQueryDtoConverter
{
	(CarrierShipmentRateQueryDto rateQueryDto, ICollection<string> errors) Convert(CalculateRatesQueryParameters parameters, IFactory factory);
}
