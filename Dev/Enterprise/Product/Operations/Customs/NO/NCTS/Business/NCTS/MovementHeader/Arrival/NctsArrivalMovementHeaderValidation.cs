using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.NCTS.Business;

public class NctsArrivalMovementHeaderValidation : EU.NCTS.Business.NctsArrivalMovementHeaderValidation
{
	public NctsArrivalMovementHeaderValidation(NctsArrivalMovementHeader parent)
	   : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateGoodsRegistrationNumber();
	}

	internal void ValidateGoodsRegistrationNumber()
	{
		ValidateCalculatedProperty(Header.GoodsRegistrationNumberInfo);
	}

	protected virtual void CheckGoodsRegistrationNumber()
	{
		MandatoryValidation.MessageErrorIfNotEntered(Header.GoodsRegistrationNumberInfo);
	}

	protected override void CheckGoodsLocationDescriptionCore()
	{
	}

	NctsArrivalMovementHeader Header => Parent as NctsArrivalMovementHeader;
}
