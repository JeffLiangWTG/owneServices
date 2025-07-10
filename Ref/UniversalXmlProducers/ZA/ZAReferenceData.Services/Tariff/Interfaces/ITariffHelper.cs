using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces
{
	public interface ITariffHelper
	{
		(short UniqueId, bool Exists) GetUniqueId(string tariffCode, string checkDigit, string relationshipTariffCode);

		List<RuleMapping> GetRules();

		List<TariffData> GetTariffs(string tariffCode);

		List<Rate> GetRates(string tariffCode, DateTime startDate, DateTime endDate);
	}
}
