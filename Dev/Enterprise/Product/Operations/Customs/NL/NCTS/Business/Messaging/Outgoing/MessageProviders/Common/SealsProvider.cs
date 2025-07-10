using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.NCTS.Business;

public class SealsProvider : ISeal
{
	public SealsProvider(string identifier, int sequenceNumber)
	{
		Id = identifier;
		SequenceNumeric = sequenceNumber;
	}

	public int SequenceNumeric { get; }

	public string Id { get; }
}
