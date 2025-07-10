using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.NCTS.Business;

public class NctsHeaderMessageSendingObjectParent : EU.NCTS.Business.NctsHeaderMessageSendingObjectParent
{
	public NctsHeaderMessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public new NctsHeaderMessageSendingObjectCollection SendingObjectsCollection => (NctsHeaderMessageSendingObjectCollection)base.SendingObjectsCollection;

	protected override NonPersistentBusinessObjectCollection<EU.NCTS.Business.NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectCollection = new NctsHeaderMessageSendingObjectCollection(Factory) { new NctsHeaderMessageSendingObject(NctsHeader) };
		return sendingObjectCollection;
	}

	protected override bool SendAndSaveMessagesCore()
	{
		var messageBuilder = GetOutboundMessageBuilder();
		var messageInformationUpdater = GetMessageInformationUpdater();

		var messages = SendingObjectsCollection
			.Cast<NctsHeaderMessageSendingObject>()
			.Select(CreateMessageInformationProvider)
			.Select(messageBuilder.Create)
			.WhereNotNull()
			.ToArray();

		messages.ForEach(m => messageInformationUpdater.UpdateInformation(NctsHeader, m));

		if (messages.Length > 0)
		{
			try
			{
				Factory.Save();
				RefreshMessagesCollection();
				return true;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		return false;
	}

	protected virtual IOutboundMessageBuilder GetOutboundMessageBuilder() => new OutboundMessageBuilder(Factory);

	IMessageInformationUpdater GetMessageInformationUpdater() => OutboundMessageInformationUpdaterFactory.GetInformationUpdater(NctsHeader);

	void RefreshMessagesCollection()
	{
		var messageCollection = IsArrival
			? NctsHeader.Messages
			: NctsHeader?.MovementHeader?.Messages;

		if (messageCollection is not null)
		{
			messageCollection.Reload(false);
		}
	}

	IMessageInformationProvider CreateMessageInformationProvider(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject)
		=> IsArrival
			? new NctsArrivalMessageInformationProvider(nctsHeaderMessageSendingObject)
			: throw new NotSupportedException("NCTS Departure is not yet implemented");

	bool IsArrival => NctsHeader?.IsArrivalMovement ?? false;
}
