using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public class MeasureData
	{
		public string Description { get; }
		public string[] TradeGroup { get; }
		public string Formula { get; }

		public MeasureData(string description, string[] tradeGroup, string formula)
		{
			Argument.NotNullOrEmpty(description, nameof(description));
			Argument.NotNull(tradeGroup, nameof(tradeGroup));

			Description = description.Trim();
			TradeGroup = tradeGroup;

			Formula = !string.IsNullOrEmpty(formula) ? formula.Trim() : string.Empty;
		}
	}
}
