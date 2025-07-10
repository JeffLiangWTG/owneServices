using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;

interface IPuescService : IDisposable
{
	AcceptDocumentResponse AcceptDocument(AcceptDocumentRequest acceptDocumentRequest);
	GetDocumentsResponse GetDocuments(GetDocumentsRequest GetDocumentsRequest);
}
