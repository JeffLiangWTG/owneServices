using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class GoodsReferenceProvider : IGoodsReference
{
	public GoodsReferenceProvider(int sequenceNumber, ZInt declarationGoodsItemNumber)
	{
		SequenceNumber = sequenceNumber.ToString();
		DeclarationGoodsItemNumber = declarationGoodsItemNumber.ToString();
	}

	public string SequenceNumber { get; }

	public string DeclarationGoodsItemNumber { get; }
}
