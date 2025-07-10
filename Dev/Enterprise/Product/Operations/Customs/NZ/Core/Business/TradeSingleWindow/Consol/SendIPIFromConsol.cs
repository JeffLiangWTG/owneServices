using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class SendIPIFromConsol : SendTSW
	{
		public SendIPIFromConsol(MAFMessagingBO mafMessaging, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes type)
			: base(mafMessaging, additionalMessageInformation, type)
		{
			this.mafMessaging = mafMessaging;
			this.additionalMessageInformation = additionalMessageInformation;
		}
		readonly MAFMessagingBO mafMessaging;
		readonly IAdditionalInformation additionalMessageInformation;

		public override ZString GetMessageText() => IPIBuilder.GetXMLMessage();

		public override ZString DeclarantPinEncrypted => IPIBuilder.DeclarantPinEncrypted;

		public override ZBool DeclarantPinRequired => IPIBuilder.DeclarantPinRequired;

		protected override ZString MessageType => MessageTypeList.Codes.IPI;

		protected override ZString MessageSubType
		{
			get
			{
				var result = string.Empty;
				switch (TransactionType)
				{
					case TSWTransactionTypes.Original:
						result = Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original;
						break;
					case TSWTransactionTypes.Cancel:
						result = Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation;
						break;
					case TSWTransactionTypes.Replace:
						result = Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Replacement;
						break;
				}

				return result;
			}
		}

		public override ZString ApplicationReference => mafMessaging.Consol.JK_UniqueConsignRef;

		protected override void SetStatusOnSuccess(StatusTransactionScope scope)
		{
			scope.Add(mafMessaging.ZX_MessagingStatusInfo);
			mafMessaging.ZX_MessagingStatus = MessagingStatusList.Codes.SentPendingAcknowledgement;
		}

		protected override void AddMessageToMessages(TSWMessage message) => mafMessaging.TSWMessages.Add(message);

		protected override void CheckErrorsBeforeGeneratingMessageCore()
		{
			CheckIsAwaitingResponse();
			CheckDataIsValid();
		}

		void CheckIsAwaitingResponse()
		{
			var lastMessage = (TSWMessage)mafMessaging.TSWMessages.GetLastMessage(TSWMessage.ApplicationCodes.NewZealandCustoms);
			var waitingResponse = lastMessage != null && lastMessage.IsTransmitMessage &&
								!(lastMessage.EM_Status == TSWMessage.Status.Cancelled || lastMessage.EM_Status == TSWMessage.Status.Failed || lastMessage.EM_Status == TSWMessage.Status.Error || lastMessage.EM_Status == TSWMessage.Status.Rejected) &&
								mafMessaging.ZX_MessagingStatus != MessagingStatusList.Codes.NotSentToMpi; // Reset to Original
			if (waitingResponse)
			{
				errorList.Add("Cannot send another message until the last message you sent gets a response.");
			}
		}

		bool CheckDataIsValid()
		{
			IMAFValidator validator = new MAFValidator(mafMessaging);
			var message = validator.GetErrorMessage();
			if (!message.IsEmpty)
			{
				errorList.Add("Cannot send message due the following validation errors:\r\n" + message);
			}

			return message.IsEmpty;
		}

		IPIMessageBuilder IPIBuilder => ipiBuilder ?? (ipiBuilder = GetIPIMessageBuilder());
		IPIMessageBuilder ipiBuilder;

		protected IPIMessageBuilder GetIPIMessageBuilder() => new IPIMessageBuilder(mafMessaging, TransactionType, additionalMessageInformation);

		protected override ZBool GetIsMessageInTestMode => NZCustomsDataRegistry.Instance.ConsolIPITestMode.Value;
	}
}
