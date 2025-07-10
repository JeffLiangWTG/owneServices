using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Messaging.Business;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry
{
	public class TSWEX1MessageBuilder : XmlMessageBuilder
	{
		public TSWEX1MessageBuilder(CusEntryHeader entryHeader, IAdditionalInformation additionalInformation, MessageBuilder.MessageTypes messageType)
			: base(entryHeader)
		{
			this.additionalInformation = additionalInformation;
			this.messageType = messageType;

			msgBuilder = new EX1MessageBuilder(new CusEntryHeaderWrapper(entryHeader, additionalInformation), TransactionType);
		}
		readonly IAdditionalInformation additionalInformation;
		readonly MessageBuilder.MessageTypes messageType;
		readonly EX1MessageBuilder msgBuilder;

		protected new CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)base.EntryHeader; }
		}

		TSWTransactionTypes TransactionType
		{
			get
			{
				if (messageType == MessageBuilder.MessageTypes.CompletionEntry)
				{
					return TSWTransactionTypes.Completion;
				}
				else if (messageType == MessageBuilder.MessageTypes.CancelEntry)
				{
					return TSWTransactionTypes.Cancel;
				}
				else if (messageType == MessageBuilder.MessageTypes.Replacement)
				{
					SetPDOFlagIfCurrentSendIsAmendment();
					return TSWTransactionTypes.Replace;
				}
				else
				{
					return TSWTransactionTypes.Original;
				}
			}
		}

		protected override EDIMessage GetNewMessage()
		{
			var message = EntryHeader.Messages.AddNew(typeof(TSWMessage));
			message.EM_MessageType = Declaration.MappedTSWMessageSubType;
			message.EM_ApplicationReference = Declaration.JE_DeclarationReference;
			message.EM_IsTestMessage = Declaration.IsInTestMode;
			message.EM_LinkedObject = EntryHeader;

			if (additionalInformation != null && additionalInformation.SupportingDocuments != null)
			{
				foreach (ITSWAttachment cusAttachment in additionalInformation.SupportingDocuments)
				{
					EDIMessageAttach ediMessageAttach = message.MessageAttachments.AddNew();
					ediMessageAttach.EG_FileName = TSWMessageFormatter.FormatAcceptableFileNameForNZC(cusAttachment.FileName);
					ediMessageAttach.EG_StorageDocsGuid = cusAttachment.UniqueIdentifier;
					ediMessageAttach.EG_EdiMsgDocType = cusAttachment.DocType;
				}
			}

			return (TSWMessage)message;
		}

		public override string GetMessageText()
		{
			return msgBuilder.GetXMLMessage();
		}

		public override ZString DeclarantPinEncrypted
		{
			get { return msgBuilder.DeclarantPinEncrypted; }
		}

		public override ZBool DeclarantPinRequired
		{
			get { return msgBuilder.DeclarantPinRequired; }
		}

		protected override void SetMessageSubType()
		{
			switch (messageType)
			{
				case MessageBuilder.MessageTypes.CancelEntry:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Cancellation;
					break;
				case MessageBuilder.MessageTypes.Replacement:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Replacement;
					break;
				case MessageBuilder.MessageTypes.Original:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Original;
					break;
				case MessageBuilder.MessageTypes.CompletionEntry:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Completion;
					break;
			}
		}
	}
}
