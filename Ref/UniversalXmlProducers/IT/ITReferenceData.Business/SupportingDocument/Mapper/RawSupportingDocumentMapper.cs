using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Types = CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument.Constants.RefCusCodeListCodeTypes;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public sealed class RawSupportingDocumentMapper : IRawSupportingDocumentMapper
	{
		public RawSupportingDocumentMapper(IEnumerable<string> validEuNctsSupportingDocumentCodes = null)
		{
			_validEuNctsSupportingDocumentCodes = validEuNctsSupportingDocumentCodes ?? Enumerable.Empty<string>();
		}

		IEnumerable<RefCusCodeList> IRawSupportingDocumentMapper.GetMappings(IRawSupportingDocument rawSupportingDocument)
		{
			Argument.NotNull(rawSupportingDocument, nameof(rawSupportingDocument));

			yield return RefCusCodeListMapper.MapFromRawSupportingDocument(Types.SupportingDocumentImport, rawSupportingDocument);
			yield return RefCusCodeListMapper.MapFromRawSupportingDocument(Types.SupportingDocumentExport, rawSupportingDocument);

			if (rawSupportingDocument.Type != SupportingDocumentType.European
				|| IsValidNctsEuropeanSupportingDocumentCode(rawSupportingDocument))
			{
				yield return RefCusCodeListMapper.MapFromRawSupportingDocument(Types.SupportingDocumentNcts, rawSupportingDocument, alwaysIncludeReferenceNumberAttributeIfPresent: true);
			}

			if (rawSupportingDocument.BelongsToUnitedNationEdifactCategory())
			{
				yield return RefCusCodeListMapper.MapFromRawSupportingDocument(Types.SupportingDocumentTemporaryStorage, rawSupportingDocument, alwaysIncludeReferenceNumberAttributeIfPresent: true);
			}
		}

		bool IsValidNctsEuropeanSupportingDocumentCode(IRawSupportingDocument rawSupportingDocument)
			=> rawSupportingDocument.Type == SupportingDocumentType.European && _validEuNctsSupportingDocumentCodes.Contains(rawSupportingDocument.Code);

		readonly IEnumerable<string> _validEuNctsSupportingDocumentCodes;
	}
}
