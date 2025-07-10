using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class ExtractedCodeListProvider
	{
		public ExtractedCodeListProvider(string codeType, string datasource, bool supportsAttributes, DateTime publicationTime, IEnumerable<RefCusCodeList> parsedXml)
		{
			DataSource = datasource;
			CodeType = codeType;
			SupportsAttributes = supportsAttributes;
			PublicationTime = publicationTime;
			ParsedXml = parsedXml;
		}

		public string CodeType { get; }

		public string DataSource { get; }

		public bool SupportsAttributes { get; }

		public DateTime PublicationTime { get; }

		public IEnumerable<RefCusCodeList> ParsedXml { get; }
	}
}
