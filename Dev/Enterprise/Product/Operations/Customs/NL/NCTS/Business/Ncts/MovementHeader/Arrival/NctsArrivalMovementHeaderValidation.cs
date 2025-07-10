namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsArrivalMovementHeaderValidation : EU.NCTS.Business.NctsArrivalMovementHeaderValidation
{
	public NctsArrivalMovementHeaderValidation(EU.NCTS.Business.NctsArrivalMovementHeader parent) : base(parent)
	{
	}

	new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateUnloadingRemarksFreeText();
	}

	public void ValidateUnloadingRemarksFreeText()
	{
		ValidateCalculatedProperty(Parent.UnloadingRemarksFreeTextInfo);
	}

	protected void CheckUnloadingRemarksFreeText()
	{
		var parent = Parent;
		if (parent.UnloadingRemarksFreeText.ContainsAnyChar("<>;"))
		{
			parent.UnloadingRemarksFreeTextInfo.AddError(Res.GetString("873B1745-237E-4A98-A9F1-CD7164377798", "Characters '<', '>' and ';' are not allowed."));
		}
	}
}
