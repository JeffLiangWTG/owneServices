using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public interface ICodebookBuilder
	{
		IDateTimeProvider DateTimeProvider { get; }

		void BuildXml(DateTime publicationDate, IList<TableElement> data, string outputPath);
	}
}
