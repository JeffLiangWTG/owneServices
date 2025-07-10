using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public interface IScrappedRecordParser
	{
		ParsingResult GetRefCusVatApplicabilities(IScrappedRecord scrappedRecord, RefCusTariff refCusTariff);
		ParsingResult GetRefCusRates(IScrappedRecord scrappedRecord, string nationalSection, RefCusTariff refCusTariff, IRateGenerationStrategy rateGenerationStrategy);
		ParsingResult GetRefCusConditions(IScrappedRecord scrappedRecord, RefCusTariff refCusTariff, List<CertificateData> certificateData);
	}
}
