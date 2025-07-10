using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Types = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListCodeTypes;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class AdditionalReferenceRawSupportingDocumentMapper : IRawSupportingDocumentMapper
	{
		public AdditionalReferenceRawSupportingDocumentMapper(IEnumerable<string> validEuAdditionalReferenceCodes)
		{
			ArgumentNullException.ThrowIfNull(validEuAdditionalReferenceCodes);
			_validEuAdditionalReferenceCodes = validEuAdditionalReferenceCodes;
		}

		IEnumerable<RefCusCodeList> IRawSupportingDocumentMapper.GetMappings(IRawSupportingDocument rawSupportingDocument)
		{
			ArgumentNullException.ThrowIfNull(rawSupportingDocument);

			if (!_validEuAdditionalReferenceCodes.Contains(rawSupportingDocument.Code))
			{
				return Enumerable.Empty<RefCusCodeList>();
			}

			return new[]
			{
				RefCusCodeListMapper.MapFromRawSupportingDocument(Types.SupportingDocumentAdditionalReference, rawSupportingDocument, mapAttributes: true, alwaysIncludeReferenceNumberAttributeIfPresent: true)
			};

		}

		readonly IEnumerable<string> _validEuAdditionalReferenceCodes;
	}
}
