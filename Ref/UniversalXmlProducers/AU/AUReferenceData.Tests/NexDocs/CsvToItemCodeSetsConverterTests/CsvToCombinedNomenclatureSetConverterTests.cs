using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToCombinedNomenclatureSetConverterTests : CsvToItemCodeSetsConverterTests<CsvToCombinedNomenclatureSetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|0406
DESCRIPTION|string|DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN, NOT ELSEWHERE SPECIFIED OR INCLUDED -Cheese and curd
COM_TYPE_CODE|string|D
START_DATE|dateTime|2022-08-02T00:00:00.000
END_DATE|dateTime|0001-01-01T00:00:00.000
UPDATED_DATE|dateTime|2022-08-02T04:17:27.000";

		protected override CsvToCombinedNomenclatureSetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToCombinedNomenclatureSetConverter();
			result.Code = "0406";
			result.Description = "DAIRY PRODUCE; BIRDS' EGGS; NATURAL HONEY; EDIBLE PRODUCTS OF ANIMAL ORIGIN, NOT ELSEWHERE SPECIFIED OR INCLUDED -Cheese and curd";
			result.ComTypeCode = "D";
			result.StartDate = new DateTime(2022, 08, 02, 00, 00, 00);
			result.UpdatedDate = new DateTime(2022, 08, 02, 04, 17, 27);
			return result;
		}
	}
}
