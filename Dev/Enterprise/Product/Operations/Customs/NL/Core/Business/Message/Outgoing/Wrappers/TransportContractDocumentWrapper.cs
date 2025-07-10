using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class TransportContractDocumentWrapper : ITransportContractDocument
{
	public TransportContractDocumentWrapper(AdditionalInfo additionalInfo, int sequenceNumeric)
	{
		this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly AdditionalInfo additionalInfo;

	public string Id => additionalInfo.CSI_ReferenceNumber;

	public string TypeCode => additionalInfo.CSI_Code;

	public int SequenceNumeric { get; }
}
