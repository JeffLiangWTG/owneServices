using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax
{
	public class SurTaxRatesFactory
	{
		public SurTaxRatesFactory(ITariffDataProducer tariffProducer)
		{
			Argument.NotNull(tariffProducer, nameof(tariffProducer));

			this.tariffProducer = tariffProducer;
		}
		readonly ITariffDataProducer tariffProducer;

		public IEnumerable<RefCusTariff> GetTariffs(WebSurTaxText surTaxText)
		{
			foreach (var tariff in tariffProducer.GetTariff(surTaxText.Classifications, 10))
			{
				var dutyValue = surTaxText.Table.ToLowerInvariant().Contains("table 1") ?
					"0.25" : "0.1";
				var rate = new RefCusRate
				{
					ZZ2_StartDate = new DateTime(2018, 07, 01),
					ZZ2_ZY1_NKRateCode = Constants.SurTax,
					ZZ2_RateFormula = dutyValue + " * VFD"
				};
				var app = new RefCusApplicability
				{
					ZZT_ZZA_NKTradeGroup = Constants.USCountryCode,
					ZZT_StartDate = rate.ZZ2_StartDate
				};
				rate.RefCusApplicabilities = new[] { app };
				tariff.RefCusRates = new[] { rate };
				yield return tariff;
			}
		}
	}
}
