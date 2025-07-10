using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToTreatmentSetConverterTests : CsvToItemCodeSetsConverterTests<CsvToTreatmentSetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|T1
DESCRIPTION|string|T1 DESC
START_DATE|dateTime|2019-01-01T21:45:35.000
END_DATE|dateTime|2020-12-31T11:35:25.000
UPDATED_DATE|dateTime|2019-11-13T00:00:00.000";

		protected override CsvToTreatmentSetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToTreatmentSetConverter();
			result.Code = "T1";
			result.Description = "T1 DESC";
			result.StartDate = new DateTime(2019, 01, 01, 21, 45, 35);
			result.EndDate = new DateTime(2020, 12, 31, 11, 35, 25);
			result.UpdateDate = new DateTime(2019, 11, 13);
			return result;
		}
	}
}
