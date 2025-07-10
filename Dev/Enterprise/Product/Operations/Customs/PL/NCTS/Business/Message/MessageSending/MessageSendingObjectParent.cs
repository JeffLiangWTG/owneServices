using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using static CargoWise.EntityFramework.ZNotificationCollector;

namespace Enterprise.Customs.PL.NCTS.Business;

public class MessageSendingObjectParent : NctsHeaderMessageSendingObjectParent
{
	public MessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader)
	{ }

	protected new NctsHeader TopLevelBusinessObject => (NctsHeader)base.TopLevelBusinessObject;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			foreach (var property in base.MessageSendingObjectProperties)
			{
				if (property.PropertyName == AutoNctsHeaderMessageSendingObject.Schema.MRN)
				{
					yield return new MessageSendingObjectProperty(MessageSendingObject.Schema.MovementReferenceNumber, true, 160);
				}
				else
				{
					yield return property;
					if (property.PropertyName == AutoNctsHeaderMessageSendingObject.Schema.MessageType)
					{
						yield return new MessageSendingObjectProperty(MessageSendingObject.Schema.Description, true, 200);
					}
				}
			}
		}
	}

	public new MessageSendingObjectCollection SendingObjectsCollection => (MessageSendingObjectCollection)base.SendingObjectsCollection;

	protected override NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		return new MessageSendingObjectCollection(TopLevelBusinessObject) { new MessageSendingObject(TopLevelBusinessObject) };
	}

	protected override void HookMessageSendingObjectEvents(BaseMessageSendingObject baseSendingObject)
	{
		base.HookMessageSendingObjectEvents(baseSendingObject);
		if (baseSendingObject is MessageSendingObject sendingObject)
		{
			sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
		}
	}

	void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
	{
		ResetValidationMessages();
	}

	protected override IEnumerable<INotification> GetNewMessageErrorCollector() => GetSupplementaryMessageErrorCollector()?.Concat(base.GetNewMessageErrorCollector());

	protected IEnumerable<INotification> GetSupplementaryMessageErrorCollector()
	{
		var notifications = new List<INotification>();
		foreach (var sendingObject in SelectedSendingObjects.OfType<MessageSendingObject>())
		{
			sendingObject.Validation.ValidateAll();
			notifications.AddRange(new CustomsNotificationCollector(sendingObject, true, false, PropertyDescriptionType.HumanReadableName).GetMessageErrors());
		}

		return notifications;
	}

	protected override bool SendAndSaveMessagesCore()
	{
		var result = false;
		var sentCount = 0;

		NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(this.NctsHeader);

		foreach (var messageSendingObject in SelectedSendingObjects.Cast<MessageSendingObject>())
		{
			switch (messageSendingObject.MessageType)
			{
				case ArrivalMessageSendingObjectTypeList.Codes.ARN:
					new IE007MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				case ArrivalMessageSendingObjectTypeList.Codes.RNM:
					new IE141MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				case ArrivalMessageSendingObjectTypeList.Codes.URM:
					new IE044MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				case DepartureMessageSendingObjectTypeList.Codes.AMD:
					SetMovementReferenceNumberIfNeeded(messageSendingObject);
					new IE013MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				case DepartureMessageSendingObjectTypeList.Codes.DEC:
					new IE015MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				case DepartureMessageSendingObjectTypeList.Codes.RRL:
					new IE054MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				case DepartureMessageSendingObjectTypeList.Codes.INV:
					new IE014MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				case DepartureMessageSendingObjectTypeList.Codes.PRN:
					new IE170MessageSender(messageSendingObject).Send();
					sentCount++;
					break;
				default:
					break;
			}
		}

		if (sentCount > 0)
		{
			try
			{
				Factory.Save();
				result = true;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}
		return result;
	}

	void SetMovementReferenceNumberIfNeeded(MessageSendingObject messageSendingObject)
	{
		var nctsHeader = TopLevelBusinessObject;
		if (messageSendingObject.MRN.IsEmpty && !messageSendingObject.MovementReferenceNumber.IsEmpty)
		{
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = messageSendingObject.MovementReferenceNumber;
		}
	}
}
