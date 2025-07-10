using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CsvToCombinedNomenclatureSetConverter : CsvToItemCodeSetsConverter
	{
		[CodeSetName("CODE", CodeSetValueType.@string)]
		public string Code { get; set; }

		[CodeSetName("DESCRIPTION", CodeSetValueType.@string)]
		public string Description { get; set; }

		[CodeSetName("COM_TYPE_CODE", CodeSetValueType.@string)]
		public string ComTypeCode { get; set; }

		[CodeSetName("START_DATE", CodeSetValueType.dateTime)]
		public DateTime StartDate { get; set; }

		[CodeSetName("END_DATE", CodeSetValueType.dateTime)]
		public DateTime EndDate { get; set; }

		[CodeSetName("UPDATED_DATE", CodeSetValueType.dateTime)]
		public DateTime UpdatedDate { get; set; }
	}
}
