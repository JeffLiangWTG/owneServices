using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public interface IApplicability
	{
		DateTime StartDate { get; }

		string AdditionalCode { get; }

		string TradeGroup { get; }
	}
}
