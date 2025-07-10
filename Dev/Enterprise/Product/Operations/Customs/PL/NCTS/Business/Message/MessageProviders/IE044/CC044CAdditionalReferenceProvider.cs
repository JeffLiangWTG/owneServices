using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC044CAdditionalReferenceProvider : IDocument
{
	public CC044CAdditionalReferenceProvider(int sequenceNumber, AdditionalInfo additionalReference)
	{
		this.additionalReference = Argument.NotNull(additionalReference, nameof(additionalReference));
		SequenceNumber = sequenceNumber.ToString();
	}
	readonly AdditionalInfo additionalReference;

	public string SequenceNumber { get; }

	public string DocumentType => additionalReference.CSI_Code;

	public string ReferenceNumber => additionalReference.CSI_ReferenceNumber;
}
