using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToDeclarationStatmentSetConverterTests : CsvToItemCodeSetsConverterTests<CsvToDeclarationStatmentSetConverter>
	{
		protected override string ExpectedResult => @"CODE|string|T1
DESCRIPTION|string|T1 DESC
TYPE|string|TP1
UPDATED_DATE|dateTime|2019-11-13T00:00:00.000";

		protected override CsvToDeclarationStatmentSetConverter GetFullyPopulatedConverter()
		{
			var result = new CsvToDeclarationStatmentSetConverter();
			result.Code = "T1";
			result.Description = "T1 DESC";
			result.Type = "TP1";
			result.UpdateDate = new DateTime(2019, 11, 13);
			return result;
		}
	}
}
