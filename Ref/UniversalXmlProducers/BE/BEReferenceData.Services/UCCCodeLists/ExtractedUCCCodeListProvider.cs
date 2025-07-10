using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public class ExtractedUCCCodeListProvider : IExtractedUCCCodeList
	{
		public ExtractedUCCCodeListProvider(string codeType, string fileName, DateTime downloadDate, string dataSource, List<(string, string)> attributeValues)
		{
			CodeType = codeType;
			FileName = fileName;
			DownloadDate = downloadDate;
			DataSource = dataSource;
			AttributeValues = attributeValues;
		}

		public string CodeType { get; }

		public string FileName { get; }

		public DateTime DownloadDate { get; }

		public string DataSource { get; }

		public List<(string attributeName, string attributeValue)> AttributeValues { get; }
	}
}
