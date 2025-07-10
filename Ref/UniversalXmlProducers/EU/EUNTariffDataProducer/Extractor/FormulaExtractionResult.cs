using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class FormulaExtractionResult : IFormulaExtractionResult
	{
		public string Formula { get; }
		public string RateCode { get; }
		public string RateType { get; }

		public FormulaExtractionResult(string formula, string rateCode)
		{
			Argument.NotNullOrEmpty(formula, nameof(formula));
			Argument.NotNullOrEmpty(rateCode, nameof(rateCode));
			Formula = formula;
			RateCode = rateCode;
			RateType = ApplicationConfig.GetRateTypeFromRateCode(rateCode);
		}
	}
}
