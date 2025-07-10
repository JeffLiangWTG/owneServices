using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	public class RevenueExtractedCodeListProvider : IRevenueExtractedCodeList, ICodeListAttribute
	{
		public RevenueExtractedCodeListProvider(string versionNumber, DateTime verionDate, UpdateType updateType, string dataGrouping, IEnumerable<IRevenueCodeDescriptionPair> codeList, bool isCodeListAttributeNeeded = false)
		{
			VersionNumber = versionNumber;
			VersionDate = verionDate;
			UpdateType = updateType;
			DataGrouping = dataGrouping;
			CodeList = codeList;
			IsCodeListAttributeNeeded = isCodeListAttributeNeeded;
		}
		public string VersionNumber { get; private set; }

		public DateTime VersionDate { get; private set; }

		public UpdateType UpdateType { get; private set; }

		public string DataGrouping { get; private set; }

		public IEnumerable<IRevenueCodeDescriptionPair> CodeList { get; private set; }

		public bool IsCodeListAttributeNeeded { get; private set; }
	}
}
