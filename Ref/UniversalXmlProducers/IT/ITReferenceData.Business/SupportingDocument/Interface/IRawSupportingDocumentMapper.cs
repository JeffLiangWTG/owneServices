using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public interface IRawSupportingDocumentMapper
	{
		IEnumerable<RefCusCodeList> GetMappings(IRawSupportingDocument rawSupportingDocument);
	}
}
