using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public sealed class ScrappedRate : IRate
	{
		public string RateCode { get; set; }

		public string RateType { get; set; }

		public string RateFormula { get; set; }

		public DateTime StartDate { get; set; }

		public IApplicability Applicability { get; set; }

		public HashSet<string> MeasurementUnits { get; set; }
	}
}
