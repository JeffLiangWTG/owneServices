using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public static class ExchangeRateParser
	{
		public static List<RefExchangeRateZZ> ParseResponse(string responseContent)
		{
			var response = JsonConvert.DeserializeObject<ExchangeRateResponse>(responseContent);
			Helper.Assume(response.IsOK, $"Failed to parse response from the source, Response:{responseContent}");

			return response.CurrencyDetail?.SelectMany(x => x.ConvertToRefExchangeRates()).ToList();
		}

		class ExchangeRateResponse
		{
			public int Status { get; set; }
			public string Type { get; set; }
			public List<ExchangeRateDetail> CurrencyDetail { get; set; }
			public string Error { get; set; }

			public bool IsOK => Status == 1;
		}

		class ExchangeRateDetail
		{
			public string CurrencyCode { get; set; }
			public string CurrencyDesc { get; set; }
			public decimal CbicImport { get; set; }
			public decimal CbicExport { get; set; }
			public string Units { get; set; }

			decimal UnitsAsDecimal => Convert.ToDecimal(Units, CultureInfo.InvariantCulture);

			public List<RefExchangeRateZZ> ConvertToRefExchangeRates()
			{
				return new List<RefExchangeRateZZ>
				{
					ConvertToRefExchangeRate(Constants.ExchangeRate.Types.Customs),
					ConvertToRefExchangeRate(Constants.ExchangeRate.Types.CustomsExport)
				};
			}

			RefExchangeRateZZ ConvertToRefExchangeRate(string rateType)
			{
				var currentCbic = rateType == Constants.ExchangeRate.Types.Customs ? CbicImport : CbicExport;
				return new RefExchangeRateZZ()
				{
					ZZN_ExRateType = rateType,
					ZZN_Rate = Math.Round(decimal.Divide(currentCbic, UnitsAsDecimal), 6),
					ZZN_RX_NKExCurrency = CurrencyCode,
					ZZN_AsPublished = $"{UnitsAsDecimal:#.######} {CurrencyCode} = {currentCbic:#.######} INR"
				};
			}
		}
	}
}
