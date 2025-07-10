using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NZ.Business.TradeSingleWindow.TSWStatus;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry
{
	public class TSWIM1MessageBuilder : XmlMessageBuilder
	{
		public TSWIM1MessageBuilder(CusEntryHeader entryHeader, IAdditionalInformation additionalInformation, MessageBuilder.MessageTypes messageType)
			: base(entryHeader)
		{
			this.additionalInformation = additionalInformation;
			this.messageType = messageType;

			msgBuilder = new IM1MessageBuilder(new CusEntryHeaderWrapper(entryHeader, additionalInformation), TransactionType);
		}
		readonly IAdditionalInformation additionalInformation;
		readonly MessageBuilder.MessageTypes messageType;
		readonly IM1MessageBuilder msgBuilder;

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
			message.EM_ApplicationReference = EntryHeader.CH_BGMReference;
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

		protected override void SetParentMessagingStatusAfterMessagePosting()
		{
			base.SetParentMessagingStatusAfterMessagePosting();
			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(EntryHeader.Declaration))
			{
				Declaration.JE_TSWCombinedStatus = GetNewTSWStatus();
			}
		}

		string GetNewTSWStatus()
		{
			var status = TSWEntryStatusList.Codes.STC;
			var currentStatus = Declaration.JE_TSWCombinedStatus;
			if (messageType != MessageBuilder.MessageTypes.Original)
			{
				if (messageType == MessageBuilder.MessageTypes.CancelEntry)
				{
					status = TSWEntryStatusList.Codes.DCP;
				}
				else
				{
					var mpiStatuses = currentStatus.SubstringSafe(1, 2);
					var nzcsStatus = StatusCodes.ResponsePending;
					status = nzcsStatus + mpiStatuses;
				}
			}

			return status;
		}
	}
}
