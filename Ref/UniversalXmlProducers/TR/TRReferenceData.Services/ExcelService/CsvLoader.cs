using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class CsvLoader
	{
		public static IEnumerable<BanderolTariffRate> GetBanderolTariffRates() => CsvRepository.GetAllFrom<BanderolTariffRate>(Path.Combine(ApplicationConfig.ResPath, "ETradeBanderolAmounts.csv"));

		public static IEnumerable<DeclarationTariff> GetDeclarationTariffs() =>
			CsvRepository.GetAllFrom<DeclarationTariff>(Path.Combine(ApplicationConfig.ResPath, "ImportDeclarationBanderolTariffCodes.csv")).Select(r =>
			{
				r.TariffCode = r.TariffCode.Replace(".", "");
				return r;
			});

		public static IEnumerable<TradeGroupCountryCodes> GetTradeGroupCountryCodes() => CsvRepository.GetAllFrom<TradeGroupCountryCodes>(Path.Combine(ApplicationConfig.ResPath, "TradeGroupCountryCodes.csv"));
	}
}
