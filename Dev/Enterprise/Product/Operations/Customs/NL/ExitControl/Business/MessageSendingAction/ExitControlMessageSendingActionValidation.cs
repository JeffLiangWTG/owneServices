using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.ExitControl.Business;

public class ExitControlMessageSendingActionValidation : ZValidation
{
	public ExitControlMessageSendingActionValidation(ExitControlMessageSendingObject parent) : base(parent)
	{
		Parent = parent;
	}

	public ExitControlMessageSendingObject Parent;

	public override Type AutoValidationType => typeof(ExitControlMessageSendingActionValidation);

	public override void ValidateAll()
	{
		ValidateEntryType();
	}

	public void ValidateEntryType()
	{
		ValidateCalculatedProperty(Parent.EntryTypeInfo);
	}

	protected void CheckEntryType()
	{
		ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.EntryTypeInfo);
	}
}
