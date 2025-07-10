using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CsvToProductTypeSetConverter : CsvToItemCodeSetsConverter
	{
		[CodeSetName("CODE", CodeSetValueType.@string)]
		public string Code { get; set; }

		[CodeSetName("TYPE", CodeSetValueType.@string)]
		public string Type { get; set; }

		[CodeSetName("SPECIES_GROUP", CodeSetValueType.@string)]
		public string SpeciesGroup { get; set; }

		[CodeSetName("DESCRIPTION", CodeSetValueType.@string)]
		public string Description { get; set; }

		[CodeSetName("EPN", CodeSetValueType.@string)]
		public string EPN { get; set; }

		[CodeSetName("SCIENTIFIC_NAME", CodeSetValueType.@string)]
		public string ScientificName { get; set; }

		[CodeSetName("HALAL_REQ", CodeSetValueType.@string)]
		public string HalalReq { get; set; }

		[CodeSetName("START_DATE", CodeSetValueType.dateTime)]
		public DateTime StartDate { get; set; }

		[CodeSetName("END_DATE", CodeSetValueType.dateTime)]
		public DateTime EndDate { get; set; }

		[CodeSetName("UPDATED_DATE", CodeSetValueType.dateTime)]
		public DateTime UpdateDate { get; set; }
	}
}
