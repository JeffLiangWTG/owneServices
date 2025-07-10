using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

internal class TransportDocumentProvider (int index, string type, string referenceNumber) : ITransportDocument
{
	public int SequenceNumber => index;

	public string Type => type;

	public string ReferenceNumber => referenceNumber;
}
