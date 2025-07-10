using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public sealed class ScrappedApplicability : IApplicability
	{
		public DateTime StartDate { get; set; }

		public string AdditionalCode { get; set; }

		public string TradeGroup { get; set; }
	}
}
