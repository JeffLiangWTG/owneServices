using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.Business;

public class CommunicationWrapper : ICommunication
{
	public CommunicationWrapper(string id, string typeCode, int sequenceNumeric)
	{
		identification = id;
		code = typeCode;
		SequenceNumeric = sequenceNumeric;
	}
	readonly string identification;
	readonly string code;

	public string Id => identification;

	public string TypeCode => code;

	public int SequenceNumeric { get; }
}
