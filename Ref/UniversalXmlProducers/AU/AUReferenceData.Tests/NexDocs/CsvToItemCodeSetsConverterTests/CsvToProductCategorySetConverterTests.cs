using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToProductCategorySetConverterTests : CsvToItemCodeSetsConverterTests<CsvToProductCategorySetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|T1
DESCRIPTION|string|T1 DESC
CAT_CODE|string|CC1
CAT_DESCRIPTION|string|CC1 DESC
PRODUCT_TYPE|string|PT1
START_DATE|dateTime|2019-01-01T21:45:35.000
END_DATE|dateTime|2020-12-31T11:35:25.000";

		protected override CsvToProductCategorySetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToProductCategorySetConverter();
			result.Code = "T1";
			result.Description = "T1 DESC";
			result.CatCode = "CC1";
			result.CatDescription = "CC1 DESC";
			result.ProductType = "PT1";
			result.StartDate = new DateTime(2019, 01, 01, 21, 45, 35);
			result.EndDate = new DateTime(2020, 12, 31, 11, 35, 25);
			return result;
		}
	}
}
