using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData
{
	public interface ISIMATextExtractor
	{
		string[] ExtractClassificationNumbers(string classificationText);
		IEnumerable<string> ExtractDutyTypes(string dutyText);
		string ExtractEffectiveDate(string dutyText);
		string ExtractDeterminationDate(string dutyText);
		Tuple<string, string> ExtractDutyRate(string dutyText);
		string[] ExtractCountryOfOriginOrExport(string[] countryNames);
		string ExtractDumpingCase(string referenceNumberText);
	}
}
