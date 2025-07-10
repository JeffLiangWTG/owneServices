using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToPrintRegionSetConverterTests : CsvToItemCodeSetsConverterTests<CsvToPrintRegionSetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|T1
DESCRIPTION|string|T1 DESC
COMMODITY_TYPE_CODE|string|
UPDATED_DATE|dateTime|2019-11-13T00:00:00.000";

		protected override CsvToPrintRegionSetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToPrintRegionSetConverter();
			result.Code = "T1";
			result.Description = "T1 DESC";
			result.UpdateDate = new DateTime(2019, 11, 13);
			return result;
		}
	}
}
