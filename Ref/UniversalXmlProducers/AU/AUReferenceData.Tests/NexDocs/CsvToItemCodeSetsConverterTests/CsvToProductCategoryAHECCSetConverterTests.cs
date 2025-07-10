using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToProductCategoryAHECCSetConverterTests : CsvToItemCodeSetsConverterTests<CsvToProductCategoryAHECCSetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|DC0699
AHECC_CODE|string|04029900
START_DATE|dateTime|1990-01-01T00:00:00.000
END_DATE|dateTime|0001-01-01T00:00:00.000
UPDATED_DATE|dateTime|2020-10-28T15:40:47.000";

		protected override CsvToProductCategoryAHECCSetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToProductCategoryAHECCSetConverter();
			result.Code = "DC0699";
			result.AHECCCode = "04029900";
			result.StartDate = new DateTime(1990, 01, 01, 00, 00, 00);
			result.UpdatedDate = new DateTime(2020, 10, 28, 15, 40, 47);
			return result;
		}
	}
}
