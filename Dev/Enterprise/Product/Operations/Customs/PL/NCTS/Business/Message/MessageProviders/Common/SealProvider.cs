using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class SealProvider : ISeal
{
	public SealProvider(int sequenceNumber, ZString identifier)
	{
		SequenceNumber = sequenceNumber.ToString();
		Identifier = identifier;
	}

	public string SequenceNumber { get; }

	public string Identifier { get; }
}
