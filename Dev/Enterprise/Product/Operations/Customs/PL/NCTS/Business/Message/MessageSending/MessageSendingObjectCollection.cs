using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class MessageSendingObjectCollection : NctsHeaderMessageSendingObjectCollection
{
	public MessageSendingObjectCollection(NctsHeader nctsHeader) : base(Argument.NotNull(nctsHeader, nameof(nctsHeader)).Factory)
	{
	}

	public new MessageSendingObject this[int index] => (MessageSendingObject)base[index];

	public new MessageSendingObject AddNew() => (MessageSendingObject)base.AddNew();

	protected override bool AllowNewCore => false;
}
