using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SE.NCTS.Business;

public class NctsHeaderMessageSendingObjectParent : EU.NCTS.Business.NctsHeaderMessageSendingObjectParent
{
	public NctsHeaderMessageSendingObjectParent(EU.NCTS.Business.NctsHeader header) : base(header)
	{
	}

	public new NctsHeaderMessageSendingObjectCollection SendingObjectsCollection => (NctsHeaderMessageSendingObjectCollection)base.SendingObjectsCollection;

	protected override NonPersistentBusinessObjectCollection<EU.NCTS.Business.NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
		=> new NctsHeaderMessageSendingObjectCollection(Factory) { new NctsHeaderMessageSendingObject(NctsHeader) };

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.LRN, true, 160);
			yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MRN, true, 160);
			yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MessageType, true, 100);
			yield return new MessageSendingObjectProperty(nameof(NctsHeaderMessageSendingObject.MessageTypeDescription), true, 200);

			if (NctsHeader.IsPhase5Departure)
			{
				yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MessageStatus, true, 100);
			}
		}
	}
}
