namespace Enterprise.Customs.PL.Business;

public class JobDeclarationMessageSendingEntryLineValidation : AutoJobDeclarationMessageSendingEntryLineValidation
{
	public JobDeclarationMessageSendingEntryLineValidation(AutoJobDeclarationMessageSendingEntryLine parent) : base(parent)
	{
	}

	new JobDeclarationMessageSendingEntryLine Parent => (JobDeclarationMessageSendingEntryLine)base.Parent;

	protected override void CheckSend()
	{
		base.CheckSend();
		if (!Parent.Send && !Parent.ParentCollection.AnyToSend())
		{
			Parent.SendInfo.AddError(Res.GetString("PLJobDeclarationMessageSendingEntryLineValidation|CheckSend", "The ZCX05 message must have at least 1 Entry Line selected."));
		}
	}
}
