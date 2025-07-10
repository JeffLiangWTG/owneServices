using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.NL.NCTS.Business;

public class MessageSendingActionParent : NctsHeaderMessageSendingObjectParent
{
	public MessageSendingActionParent(EU.NCTS.Business.NctsHeader nctsHeader) : base(nctsHeader)
	{
		this.nctsHeader = (NctsHeader)Argument.NotNull(nctsHeader, nameof(nctsHeader));
	}
	readonly NctsHeader nctsHeader;

	public override BusinessObject TopLevelBusinessObject => nctsHeader;

	protected override NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore() => new MessageSendingActionCollection(nctsHeader);

	public new MessageSendingActionCollection SendingObjectsCollection => (MessageSendingActionCollection)base.SendingObjectsCollection;

	public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => new List<MessageSendingObjectProperty>
	{
		new MessageSendingObjectProperty(EU.NCTS.Business.AutoNctsHeaderMessageSendingObject.Schema.LRN, true, 160),
		new MessageSendingObjectProperty(EU.NCTS.Business.AutoNctsHeaderMessageSendingObject.Schema.MRN, true, 160),
		new MessageSendingObjectProperty(MessageSendingAction.Schema.MessageType, true, 100),
	};

	protected override void HookMessageSendingObjectEvents(BaseMessageSendingObject bo)
	{
		base.HookMessageSendingObjectEvents(bo);
		if (bo is MessageSendingAction action)
		{
			action.MessageTypeInfo.ValueChanged += (sender, e) => ResetValidationMessages();
		}
	}

	public bool ShowValidationErrors => SendingObjectsCollection.Cast<MessageSendingAction>().Any(x => x.ShowValidationErrors);

	protected override ZString GetBizObjValidationMessageErrors() => ShowValidationErrors ? base.GetBizObjValidationMessageErrors() : ZString.Empty;

	protected override bool SendAndSaveMessagesCore()
	{
		var result = false;
		nctsHeader.AssignDeclarationGoodsItemNumbers(true);
		var messageSendingActions = SendingObjectsCollection.Cast<MessageSendingAction>().Where(x => x.ShouldSend);
		var senders = new List<IMessageSender>();
		var destinationOfficeForDepartureBeforeSend = string.Empty;
		var destinationOfficeForDepartureAfterSend = string.Empty;
		foreach (var messageSendingAction in messageSendingActions)
		{
			switch (messageSendingAction.MessageType)
			{
				case NctsMessageTypeListNL.Codes.ArrivalNotification:
					senders.Add(new CC007CSender(messageSendingAction));
					break;
				case NctsMessageTypeListNL.Codes.Amendment:
					senders.Add(new CC013CSender(messageSendingAction));
					break;
				case NctsMessageTypeListNL.Codes.Declaration:
					senders.Add(new CC015CSender(messageSendingAction));
					break;
				case NctsMessageTypeListNL.Codes.InvalidationCancellation:
					senders.Add(new CC014CSender(messageSendingAction));
					break;
				case NctsMessageTypeListNL.Codes.UnloadingRemarks:
					senders.Add(new CC044CSender(messageSendingAction));
					break;
				case NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement:
					destinationOfficeForDepartureBeforeSend = nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCodeForDeparture;
					destinationOfficeForDepartureAfterSend = messageSendingAction.ActualOfficeOfDestination;
					senders.Add(new CC141CSender(messageSendingAction));
					break;
				case NctsMessageTypeListNL.Codes.PresentationNotification:
					senders.Add(new CC170CSender(messageSendingAction));
					break;
			}
		}

		senders.ForEach(x => x.Send());

		if (senders.Count > 0)
		{
			try
			{
				if (!string.IsNullOrEmpty(destinationOfficeForDepartureAfterSend))
				{
					nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCodeForDeparture = destinationOfficeForDepartureAfterSend;
				}
				nctsHeader.Factory.Save();
				result = true;
			}
			catch (ZSaveException ex)
			{
				if (!string.IsNullOrEmpty(destinationOfficeForDepartureAfterSend))
				{
					nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCodeForDeparture = destinationOfficeForDepartureBeforeSend;
				}
				ZExceptionReporting.HandleSaveException(ex);
			}
		}
		return result;
	}
}
