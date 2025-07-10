using System.Collections.Generic;
using DTO = WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public interface IWiseRatesConverter
	{
		IList<WiseEntry> Convert(WiseRatesConversionContext context, IEnumerable<DTO.Rate> apiRates);
	}
}
