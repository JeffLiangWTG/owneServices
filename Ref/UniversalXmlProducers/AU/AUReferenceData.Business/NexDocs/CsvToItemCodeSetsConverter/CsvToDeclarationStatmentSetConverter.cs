using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CsvToDeclarationStatmentSetConverter : CsvToItemCodeSetsConverter
	{
		[CodeSetName("CODE", CodeSetValueType.@string)]
		public string Code { get; set; }

		[CodeSetName("DESCRIPTION", CodeSetValueType.@string)]
		public string Description { get; set; }

		[CodeSetName("TYPE", CodeSetValueType.@string)]
		public string Type { get; set; }

		[CodeSetName("UPDATED_DATE", CodeSetValueType.dateTime)]
		public DateTime UpdateDate { get; set; }
	}
}
