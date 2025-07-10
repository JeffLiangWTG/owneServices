using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public interface IRate
	{
		string RateCode { get; }

		string RateType { get; }

		string RateFormula { get; }

		DateTime StartDate { get; }

		IApplicability Applicability { get; }

		HashSet<string> MeasurementUnits { get; }
	}
}
