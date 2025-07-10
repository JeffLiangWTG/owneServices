using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToAdditionalTextSetConverterTests : CsvToItemCodeSetsConverterTests<CsvToAdditionalTextSetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|T1
DESCRIPTION|string|T1 DESC
UPDATED_DATE|dateTime|2019-11-13T00:00:00.000";

		protected override CsvToAdditionalTextSetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToAdditionalTextSetConverter();
			result.Code = "T1";
			result.Description = "T1 DESC";
			result.UpdateDate = new DateTime(2019, 11, 13);
			return result;
		}
	}
}
