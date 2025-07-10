using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public interface IExtractedUCCCodeList
	{
		string CodeType { get; }
		string FileName { get; }
		DateTime DownloadDate { get; }
		string DataSource { get; }
		List<(string attributeName, string attributeValue)> AttributeValues { get; }
	}
}
