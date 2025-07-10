using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CsvToCodesSetConverter : CsvToItemCodeSetsConverter
	{
		[CodeSetName("COMMODITY_CODE", CodeSetValueType.@string)]
		public string CommodityCode { get; set; }

		[CodeSetName("PRESERVATION_CODE", CodeSetValueType.@string)]
		public string PreservationCode { get; set; }

		[CodeSetName("PRODUCT_TYPE_CODE", CodeSetValueType.@string)]
		public string ProductTypeCode { get; set; }

		[CodeSetName("PACK_TYPE_CODE", CodeSetValueType.@string)]
		public string PackTypeCode { get; set; }

		[CodeSetName("SUPPLEMENTARY_CODE", CodeSetValueType.@string)]
		public string SupplementaryCode { get; set; }
	}
}
