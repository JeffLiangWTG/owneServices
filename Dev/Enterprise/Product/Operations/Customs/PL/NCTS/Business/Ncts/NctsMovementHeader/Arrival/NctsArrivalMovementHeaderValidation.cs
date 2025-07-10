namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsArrivalMovementHeaderValidation : EU.NCTS.Business.NctsArrivalMovementHeaderValidation
{
	public NctsArrivalMovementHeaderValidation(NctsArrivalMovementHeader parent) : base(parent)
	{
	}

	new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

	protected override void CheckGoodsLocationDescriptionCore()
	{
		base.CheckGoodsLocationDescriptionCore();

		var parent = Parent;
		if (parent.IsPhase5 && parent.GoodsLocationDescription.IsEmpty)
		{
			parent.GoodsLocationDescriptionInfo.AddMessageError(Res.GetString("A345472A-25AF-4CF9-AC08-33056B0B0004", "Location of Goods details not declared. Please declare details for Location of Goods."));
		}
	}
}
