using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class IE013IE015MessageSender(MessageSendingObject sendingObject) : MessageSender(sendingObject)
{
	protected override void PreSend()
	{
		base.PreSend();

		if (NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(nctsHeader))
		{
			NctsHeaderDeclarationGoodsItemNumbersHelper.SetDeclarationGoodsItemNumbersToZero(nctsHeader);
		}

		NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(nctsHeader);
	}
}
