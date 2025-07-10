using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsReferenceProvider(int sequenceNumber, int lineNumber) : IGoodsReference
{
	public int SequenceNumber { get; } = sequenceNumber;

	public int DeclarationGoodsItemNumber { get; } = lineNumber;
}
