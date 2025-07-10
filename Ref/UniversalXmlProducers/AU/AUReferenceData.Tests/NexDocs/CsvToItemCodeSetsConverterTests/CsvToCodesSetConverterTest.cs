using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	class CsvToCodesSetConverterTest : CsvToItemCodeSetsConverterTests<CsvToCodesSetConverter>
	{
		protected override string ExpectedResult => @"COMMODITY_CODE|string|D
PRESERVATION_CODE|string|C
PRODUCT_TYPE_CODE|string|AMF
PACK_TYPE_CODE|string|BB
SUPPLEMENTARY_CODE|string|DM";

		protected override CsvToCodesSetConverter GetFullyPopulatedConverter() => new CsvToCodesSetConverter
		{
			CommodityCode = "D",
			PreservationCode = "C",
			ProductTypeCode = "AMF",
			PackTypeCode = "BB",
			SupplementaryCode = "DM"
		};
	}
}
