using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists
{
	public class MappingConfig
	{
		public string[] DomainNames { get; set; }
		public bool OnlyForImport { get; set; }
		public bool OnlyForExport { get; set; }
		public string ListType { get; set; }
		public bool WithLanguagesList { get; set; }
		public bool WithAttributeList { get; set; }
		public bool MixedCasedCodes { get; set; }
		public int YearsToImport { get; set; }
		public Func<IEnumerable<RefCusCodeList>> AdditionalCodes { get; set; }
	}
}
