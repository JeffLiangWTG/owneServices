using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public class AESAdditionalProcedureProvider : IAdditionalProcedure
{
	public AESAdditionalProcedureProvider(ZString concessionCode, int sequenceNumber)
	{
		this.concessionCode = concessionCode;
		this.sequenceNumber = sequenceNumber;
	}

	readonly int sequenceNumber;
	readonly ZString concessionCode;

	public int SequenceNumber => sequenceNumber;

	public string AdditionalProcedure => concessionCode;
}
