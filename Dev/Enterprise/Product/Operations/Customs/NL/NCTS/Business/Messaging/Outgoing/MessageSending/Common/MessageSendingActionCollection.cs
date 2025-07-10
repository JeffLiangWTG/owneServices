using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class MessageSendingActionCollection : NctsHeaderMessageSendingObjectCollection
{
	public MessageSendingActionCollection(NctsHeader nctsHeader)
		: base(Argument.NotNull(nctsHeader, nameof(nctsHeader)).Factory)
	{
		AddMessageSendingActions(nctsHeader);
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("Users cannot add a new member");

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;

	public new MessageSendingAction this[int index] => (MessageSendingAction)base[index];

	public new MessageSendingAction AddNew() => (MessageSendingAction)base.AddNew();

	void AddMessageSendingActions(NctsHeader header)
	{
		if (header.IsDepartureMovement)
		{
			Add(new MessageSendingAction(header.MovementHeader));
		}

		if (header.IsArrivalMovement)
		{
			Add(new MessageSendingAction(header.ArrivalMovementHeader));
		}
	}
}
