using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CsvToProductCategorySetConverter : CsvToItemCodeSetsConverter
	{
		[CodeSetName("CODE", CodeSetValueType.@string)]
		public string Code { get; set; }

		[CodeSetName("DESCRIPTION", CodeSetValueType.@string)]
		public string Description { get; set; }

		[CodeSetName("CAT_CODE", CodeSetValueType.@string)]
		public string CatCode { get; set; }

		[CodeSetName("CAT_DESCRIPTION", CodeSetValueType.@string)]
		public string CatDescription { get; set; }

		[CodeSetName("PRODUCT_TYPE", CodeSetValueType.@string)]
		public string ProductType { get; set; }

		[CodeSetName("START_DATE", CodeSetValueType.dateTime)]
		public DateTime StartDate { get; set; }

		[CodeSetName("END_DATE", CodeSetValueType.dateTime)]
		public DateTime EndDate { get; set; }
	}
}
