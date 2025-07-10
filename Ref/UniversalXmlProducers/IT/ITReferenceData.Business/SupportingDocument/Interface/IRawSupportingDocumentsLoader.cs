using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument
{
	public interface IRawSupportingDocumentsLoader
	{
		IEnumerable<IRawSupportingDocument> GetRawSupportingDocuments();
	}
}
