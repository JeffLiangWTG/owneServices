using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CsvToPrintRegionSetConverter : CsvToItemCodeSetsConverter
	{
		[CodeSetName("CODE", CodeSetValueType.@string)]
		public string Code { get; set; }

		[CodeSetName("DESCRIPTION", CodeSetValueType.@string)]
		public string Description { get; set; }

		[CodeSetName("COMMODITY_TYPE_CODE", CodeSetValueType.@string)]
		public string CommodiyTypeCode { get; set; }

		[CodeSetName("UPDATED_DATE", CodeSetValueType.dateTime)]
		public DateTime UpdateDate { get; set; }
	}
}
