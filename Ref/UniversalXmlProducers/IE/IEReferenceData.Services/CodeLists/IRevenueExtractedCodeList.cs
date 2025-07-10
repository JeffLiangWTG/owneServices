using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	public interface IRevenueExtractedCodeList
	{
		string VersionNumber { get; }
		DateTime VersionDate { get; }
		UpdateType UpdateType { get; }
		string DataGrouping { get; }
		IEnumerable<IRevenueCodeDescriptionPair> CodeList { get; }
	}
}
