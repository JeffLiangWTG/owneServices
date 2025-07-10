using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.NCTS.Business;

public class AdditionalInformationProvider : IAdditionalInformation
{
	public AdditionalInformationProvider(int sequenceNumber, AdditionalInfo additionalInformation)
	{
		this.additionalInformation = Argument.NotNull(additionalInformation, nameof(additionalInformation));
		SequenceNumber = sequenceNumber.ToString();
	}
	readonly AdditionalInfo additionalInformation;

	public string SequenceNumber { get; }

	public string Code => additionalInformation.CSI_Code;

	public string Text => additionalInformation.CSI_Description;
}
