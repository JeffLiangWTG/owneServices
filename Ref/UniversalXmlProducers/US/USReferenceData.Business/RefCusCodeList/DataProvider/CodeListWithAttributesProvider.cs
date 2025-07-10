using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class CodeListWithAttributesProvider : CodeListProvider, ICodeListAttributesProvider, IDateProvider
	{
		public IEnumerable<ICodeListAttribute> Attributes { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
