using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class DeclarationMessageManager : SingleMessageManager
	{
		public DeclarationMessageManager(MessageSendingObject messageSender)
		{
			this.messageSender = messageSender;
		}

		readonly MessageSendingObject messageSender;

		public override string MessageFriendlyName => messageSender?.FriendlyNameForMessageManager ?? ZString.Empty;

		public override BusinessObject BusinessObject => messageSender;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => true;

		public override bool IsWaitingForResponse => messageSender.Header.IsWaitingForResponse;

		protected override bool ShouldWaitUntilResponded => false;

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as MessageSendingObject);
		}

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as MessageSendingObject);
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as MessageSendingObject);
		}

		EDIMessage[] GenerateMessage(MessageSendingObject sendingObject)
		{
			return sendingObject != null ? new EDIMessage[] { GenerateCustomsMessage(sendingObject) } : Array.Empty<EDIMessage>();
		}

		TWMessage GenerateCustomsMessage(MessageSendingObject sendingObject)
		{
			var entry = sendingObject.Header;
			var declaration = entry.Declaration;
			var message = entry.Factory.New<TWMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TaiwanCustoms;
			message.EM_Status = Status.Queued;
			message.EM_ReceiveTransmit = Direction.Transmit;
			message.EM_LinkUniqueID = entry.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;

			message.EM_MessageType = messageSender.MessageType;
			message.EM_ApplicationReference = declaration.JE_CustomsProfile;
			message.EM_MessageOwner = sendingObject.GetMessageOwner();
			message.EM_MessageText = sendingObject.SerializeToMessageString();
			message.EM_IsTestMessage = TWCustomsDataRegistry.IsTestMode;
			message.EM_SendWithMessageErrors = declaration.HasMessageErrors || sendingObject.HasMessageErrors;

			UpdateStatusAndSubmittedDate(entry, message, sendingObject);

			var declarationSendingObject = sendingObject as AdditionalDocumentMessageSendingObject;
			if (declarationSendingObject != null && declarationSendingObject.SupportingDocuments != null)
			{
				foreach (SupportingDocument item in declarationSendingObject.SupportingDocuments)
				{
					if (!item.EDoc.IsEmpty)
					{
						var eDoc = declarationSendingObject.GetIeDocFromUniqueKey(item.EDoc.ToGuid());
						var attachement = message.MessageAttachments.AddNew();
						attachement.EG_StorageDocsGuid = eDoc.UniqueKey;
						attachement.EG_FileName = eDoc.FileName;
						attachement.EG_EdiMsgDocType = eDoc.DocType;
						attachement.EG_EM = message.PK;
					}
				}
			}
			return message;
		}

		protected void UpdateStatusAndSubmittedDate(CusEntryHeader entry, TWMessage message, MessageSendingObject sendingObject)
		{
			var messageType = message.EM_MessageType;
			var status = new EDITWCStatusCalculator(message).GetMessageAwaitingStatus(sendingObject.Action);
			if (!status.IsEmpty)
			{
				entry.CH_Status = status;
			}

			if (messageType == MessageTypeList.Codes.ECD || messageType == MessageTypeList.Codes.ICD)
			{
				entry.PopulateEntrySubmittedDateIfRequired();
			}
		}

		CusEntryHeader Header => messageSender.Header;

		protected override void OnAmendmentSentCore()
		{
			base.OnAmendmentSentCore();
			Header.Messages.Load();
		}

		protected override void OnOriginalSentCore()
		{
			base.OnOriginalSentCore();
			Header.Messages.Load();
		}

		protected override void OnWithdrawalSentCore()
		{
			base.OnWithdrawalSentCore();
			Header.Messages.Load();
		}
	}
}
