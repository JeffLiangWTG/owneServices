using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Types = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListCodeTypes;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class NctsRawSupportingDocumentMapper : IRawSupportingDocumentMapper
	{
		public NctsRawSupportingDocumentMapper(IEnumerable<string> excludedCodeCollection)
		{
			_excludedCodeCollection = excludedCodeCollection ?? new List<string>();
		}

		IEnumerable<RefCusCodeList> IRawSupportingDocumentMapper.GetMappings(IRawSupportingDocument rawSupportingDocument)
		{
			if (_excludedCodeCollection.Contains(rawSupportingDocument.Code))
			{
				return Enumerable.Empty<RefCusCodeList>();
			}

			return new[]
			{
				RefCusCodeListMapper.MapFromRawSupportingDocument(Types.SupportingDocumentNcts, rawSupportingDocument, mapAttributes: false)
			};
		}

		readonly IEnumerable<string> _excludedCodeCollection;
	}
}
