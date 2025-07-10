using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToEstablishmentIndicatorSetConverterTests : CsvToItemCodeSetsConverterTests<CsvToEstablishmentIndicatorSetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|AQ
DESCRIPTION|string|Aquaculture Farm
START_DATE|dateTime|2023-10-01T00:00:00.000
END_DATE|dateTime|0001-01-01T00:00:00.000
UPDATED_DATE|dateTime|2024-04-29T21:07:32.000";

		protected override CsvToEstablishmentIndicatorSetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToEstablishmentIndicatorSetConverter
			{
				Code = "AQ",
				Description = "Aquaculture Farm",
				StartDate = new DateTime(2023, 10, 01),
				EndDate = default,
				UpdateDate = new DateTime(2024, 4, 29, 21, 7, 32)
			};
			return result;
		}
	}
}
