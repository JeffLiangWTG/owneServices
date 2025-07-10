using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TransportDocumentProvider : IDocument
{
	public TransportDocumentProvider(int sequenceNumber, AdditionalInfo transportDocument)
	{
		this.transportDocument = Argument.NotNull(transportDocument, nameof(transportDocument));
		SequenceNumber = sequenceNumber.ToString();
	}
	readonly AdditionalInfo transportDocument;

	public string SequenceNumber { get; }

	public string DocumentType => transportDocument.CSI_Code;

	public string ReferenceNumber => transportDocument.CSI_ReferenceNumber;
}
