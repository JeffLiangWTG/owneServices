using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

public class AESSealProvider : ISeal
{
	public AESSealProvider(int sequenceNumber, string identifier)
	{
		SequenceNumber = sequenceNumber;
		Identifier = Argument.NotNullOrEmpty(identifier, nameof(identifier));
	}

	public int SequenceNumber { get; }

	public string Identifier { get; }
}
