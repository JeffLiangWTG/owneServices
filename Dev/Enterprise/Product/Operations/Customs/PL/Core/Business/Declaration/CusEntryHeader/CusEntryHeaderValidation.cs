using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryHeaderValidation : EU.Business.Declaration.CusEntryHeaderValidation
{
	public CusEntryHeaderValidation(EU.Business.Declaration.CusEntryHeader parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		CheckAllMaximumCollectionsAmounts();
	}

	protected void CheckAllMaximumCollectionsAmounts()
	{
		if (Parent.AllEntryLines.Count > MaximumBusinessObjectsAmounts.MaximumEntryLines)
		{
			Parent.AddRowMessageError(Res.GetString("PLCusEntryHeaderValidation|CheckAllMaximumCollectionsAmounts|EntryLines", "Entry Lines count exceeds maximum of 999 - create a new Entry Instruction"));
		}
	}
}
